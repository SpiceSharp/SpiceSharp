using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SpiceSharp.Circuits;
using SpiceSharp.Diagnostics;
using SpiceSharp.Sparse;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Base class with common methods
    /// </summary>
    public abstract class BaseSimulation : Simulation
    {
        /// <summary>
        /// Necessary behaviors and configurations
        /// </summary>
        protected Collection<LoadBehavior> LoadBehaviors { get; private set; }
        protected Collection<TemperatureBehavior> TemperatureBehaviors { get; private set; }
        protected Collection<InitialConditionBehavior> InitialConditionBehaviors { get; private set; }

        /// <summary>
        /// Gets the currently active configuration
        /// </summary>
        public BaseConfiguration BaseConfiguration { get; protected set; }

        /// <summary>
        /// Gets the currently active state
        /// </summary>
        public RealState RealState { get; protected set; }

        /// <summary>
        /// Gets statistics
        /// </summary>
        public Statistics Statistics { get; } = new Statistics();

        /// <summary>
        /// The node that gives problems
        /// </summary>
        public Node ProblemNode { get; protected set; }

        /// <summary>
        /// Event called when the state is loaded
        /// </summary>
        public event EventHandler<LoadStateEventArgs> OnLoad;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name"></param>
        protected BaseSimulation(Identifier name)
            : base(name)
        {
            ParameterSets.Add(new BaseConfiguration());
            States.Add(new RealState());
        }

        /// <summary>
        /// Setup the simulation
        /// </summary>
        protected override void Setup()
        {
            // No use simulating an empty circuit
            if (Circuit.Objects.Count == 0)
                throw new CircuitException("{0}: No circuit objects for simulation".FormatString(Name));

            // Setup all objects
            Circuit.Objects.BuildOrderedComponentList();
            foreach (var o in Circuit.Objects)
            {
                o.Setup(Circuit);
            }
            if (Circuit.Nodes.Count < 1)
                throw new CircuitException("{0}: No circuit nodes for simulation".FormatString(Name));

            // Setup behaviors, configurations and states
            BaseConfiguration = ParameterSets.Get<BaseConfiguration>();
            RealState = States.Get<RealState>();
            TemperatureBehaviors = SetupBehaviors<TemperatureBehavior>();
            LoadBehaviors = SetupBehaviors<LoadBehavior>();
            InitialConditionBehaviors = SetupBehaviors<InitialConditionBehavior>();

            // Setup the load behaviors
            var matrix = RealState.Matrix;
            foreach (var behavior in LoadBehaviors)
                behavior.GetMatrixPointers(Circuit.Nodes, matrix);

            // Allow nodesets to help convergence
            OnLoad += LoadNodesets;
        }

        /// <summary>
        /// Execute the simulation
        /// </summary>
        protected override void Execute()
        {
            // Do temperature-dependent calculations
            foreach (var behavior in TemperatureBehaviors)
                behavior.Temperature(this);

            // Initialize the solution
            RealState.Initialize(Circuit);

            // Do initial conditions
            InitialConditions();
        }

        /// <summary>
        /// Unsetup the simulation
        /// </summary>
        protected override void Unsetup()
        {
            // Remove nodeset
            OnLoad -= LoadNodesets;

            // Unsetup all behaviors
            foreach (var behavior in InitialConditionBehaviors)
                behavior.Unsetup();
            foreach (var behavior in LoadBehaviors)
                behavior.Unsetup();
            foreach (var behavior in TemperatureBehaviors)
                behavior.Unsetup();

            // Clear the state
            RealState.Clear();
            RealState.Destroy();
            RealState = null;

            // Remove behavior and configuration references
            LoadBehaviors.Clear();
            LoadBehaviors = null;
            InitialConditionBehaviors.Clear();
            InitialConditionBehaviors = null;
            BaseConfiguration = null;

            // Unsetup all objects
            foreach (var o in Circuit.Objects)
                o.Unsetup(Circuit);
        }

        /// <summary>
        /// Calculate the operating point of the circuit
        /// </summary>
        /// <param name="maxIterations">Maximum iterations</param>
        protected void Op(int maxIterations)
        {
            var state = RealState;
            var config = BaseConfiguration;
            state.Init = RealState.InitializationStates.InitJunction;
            state.Matrix.Complex = false;

            // First, let's try finding an operating point by using normal iterations
            if (!config.NoOperatingPointIterations)
            {
                if (Iterate(maxIterations))
                {
                    return;
                }
            }

            // No convergence, try Gmin stepping
            if (config.GminSteps > 1)
            {
                state.Init = RealState.InitializationStates.InitJunction;
                CircuitWarning.Warning(this, "Starting Gmin stepping");
                state.DiagonalGmin = config.Gmin;
                for (int i = 0; i < config.GminSteps; i++)
                    state.DiagonalGmin *= 10.0;
                for (int i = 0; i <= config.GminSteps; i++)
                {
                    state.IsConvergent = false;
                    if (!Iterate(maxIterations))
                    {
                        state.DiagonalGmin = 0.0;
                        CircuitWarning.Warning(this, "Gmin step failed");
                        break;
                    }
                    state.DiagonalGmin /= 10.0;
                    state.Init = RealState.InitializationStates.InitFloat;
                }
                state.DiagonalGmin = 0.0;
                if (Iterate(maxIterations))
                {
                    return;
                }
            }

            // Nope, still not converging, let's try source stepping
            if (config.SourceSteps > 1)
            {
                state.Init = RealState.InitializationStates.InitJunction;
                CircuitWarning.Warning(this, "Starting source stepping");
                for (int i = 0; i <= config.SourceSteps; i++)
                {
                    state.SourceFactor = i / (double)config.SourceSteps;
                    if (!Iterate(maxIterations))
                    {
                        state.SourceFactor = 1.0;
                        // circuit.CurrentAnalysis = AnalysisType.DoingTran;
                        CircuitWarning.Warning(this, "Source stepping failed");
                        return;
                    }
                }
                state.SourceFactor = 1.0;
                return;
            }

            // Failed
            throw new CircuitException("Could not determine operating point");
        }

        /// <summary>
        /// Solve iteratively for simulations
        /// </summary>
        /// <param name="maxIterations">Maximum number of iterations</param>
        /// <returns></returns>
        protected bool Iterate(int maxIterations)
        {
            var state = RealState;
            var matrix = state.Matrix;
            bool pass = false;
            int iterno = 0;

            // Make sure we're using real numbers!
            matrix.Complex = false;

            // Initialize the state of the circuit
            if (!state.Initialized)
                state.Initialize(Circuit);

            // Ignore operating condition point, just use the solution as-is
            if (state.UseIC && state.Domain == RealState.DomainType.Time)
            {
                state.StoreSolution();

                // Voltages are set using IC statement on the nodes
                // Internal initial conditions are calculated by the components
                Load();
                return true;
            }

            // Perform iteration
            while (true)
            {
                // Reset convergence flag
                state.IsConvergent = true;

                try
                {
                    Load();
                    iterno++;
                }
                catch (CircuitException)
                {
                    iterno++;
                    Statistics.Iterations = iterno;
                    throw;
                }

                // Preorder matrix
                if (!state.Sparse.HasFlag(RealState.SparseStates.DidPreorder))
                {
                    matrix.Preorder();
                    state.Sparse |= RealState.SparseStates.DidPreorder;
                }
                if (state.Init == RealState.InitializationStates.InitJunction || state.Init == RealState.InitializationStates.InitTransient)
                {
                    state.Sparse |= RealState.SparseStates.ShouldReorder;
                }

                // Reorder
                if (state.Sparse.HasFlag(RealState.SparseStates.ShouldReorder))
                {
                    Statistics.ReorderTime.Start();
                    matrix.Reorder(state.PivotRelativeTolerance, state.PivotAbsoluteTolerance, state.DiagonalGmin);
                    Statistics.ReorderTime.Stop();
                    state.Sparse &= ~RealState.SparseStates.ShouldReorder;
                }
                else
                {
                    // Decompose
                    Statistics.DecompositionTime.Start();
                    matrix.Factor(state.DiagonalGmin);
                    Statistics.DecompositionTime.Stop();
                }

                // Solve the equation
                Statistics.SolveTime.Start();
                matrix.Solve(state.Rhs, state.Rhs);
                Statistics.SolveTime.Stop();

                // The result is now stored in the RHS vector, let's move it to the current solution vector
                state.StoreSolution();

                // Reset ground nodes
                state.Rhs[0] = 0.0;
                state.Solution[0] = 0.0;
                state.OldSolution[0] = 0.0;

                // Exceeded maximum number of iterations
                if (iterno > maxIterations)
                {
                    Statistics.Iterations += iterno;
                    return false;
                }

                if (state.IsConvergent && iterno != 1)
                    state.IsConvergent = IsConvergent();
                else
                    state.IsConvergent = false;

                switch (state.Init)
                {
                    case RealState.InitializationStates.InitFloat:
                        if (state.UseDC && state.HadNodeSet)
                        {
                            if (pass)
                                state.IsConvergent = false;
                            pass = false;
                        }
                        if (state.IsConvergent)
                        {
                            Statistics.Iterations += iterno;
                            return true;
                        }
                        break;

                    case RealState.InitializationStates.InitJunction:
                        state.Init = RealState.InitializationStates.InitFix;
                        state.Sparse |= RealState.SparseStates.ShouldReorder;
                        break;

                    case RealState.InitializationStates.InitFix:
                        if (state.IsConvergent)
                            state.Init = RealState.InitializationStates.InitFloat;
                        pass = true;
                        break;

                    case RealState.InitializationStates.InitTransient:
                        if (iterno <= 1)
                            state.Sparse = RealState.SparseStates.ShouldReorder;
                        state.Init = RealState.InitializationStates.InitFloat;
                        break;

                    case RealState.InitializationStates.None:
                        state.Init = RealState.InitializationStates.InitFloat;
                        break;

                    default:
                        Statistics.Iterations += iterno;
                        throw new CircuitException("Could not find flag");
                }
            }
        }

        /// <summary>
        /// Load the circuit with the load behaviors
        /// </summary>
        protected void Load()
        {
            var state = RealState;
            var nodes = Circuit.Nodes;

            // Start the stopwatch
            Statistics.LoadTime.Start();

            // Clear rhs and matrix
            state.Clear();

            // Load all devices
            foreach (var behavior in LoadBehaviors)
                behavior.Load(this);

            // Call events
            var args = new LoadStateEventArgs(RealState);
            OnLoad?.Invoke(this, args);

            // Keep statistics
            Statistics.LoadTime.Stop();
        }

        /// <summary>
        /// Set the initial conditions
        /// </summary>
        protected void InitialConditions()
        {
            var circuit = Circuit;
            var state = RealState;
            var nodes = circuit.Nodes;

            // Clear the current solution
            for (int i = 0; i < state.Solution.Length; i++)
            {
                state.Rhs[i] = 0.0;
            }

            // Go over all nodes
            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                if (nodes.NodeSets.ContainsKey(node.Name))
                {
                    node.Diagonal = state.Matrix.GetElement(node.Index, node.Index);
                    state.HadNodeSet = true;
                    state.Rhs[node.Index] = nodes.NodeSets[node.Name];
                }
                if (nodes.InitialConditions.ContainsKey(node.Name))
                {
                    node.Diagonal = state.Matrix.GetElement(node.Index, node.Index);
                    state.Rhs[node.Index] = nodes.InitialConditions[node.Name];
                }
            }

            // Use initial conditions
            if (state.UseIC)
            {
                foreach (var behavior in InitialConditionBehaviors)
                    behavior.SetInitialCondition(circuit);
            }
        }

        /// <summary>
        /// Apply nodesets
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Arguments</param>
        protected void LoadNodesets(object sender, LoadStateEventArgs e)
        {
            var state = RealState;
            var nodes = Circuit.Nodes;

            // Consider doing nodeset & ic assignments
            if ((state.Init & (RealState.InitializationStates.InitJunction | RealState.InitializationStates.InitFix)) != 0)
            {
                // Do nodesets
                for (int i = 0; i < nodes.Count; i++)
                {
                    var node = nodes[i];
                    if (nodes.NodeSets.ContainsKey(node.Name))
                    {
                        double ns = nodes.NodeSets[node.Name];
                        if (ZeroNoncurRow(state.Matrix, nodes, node.Index))
                        {
                            state.Rhs[node.Index] = 1.0e10 * ns;
                            node.Diagonal.Value = 1.0e10;
                        }
                        else
                        {
                            state.Rhs[node.Index] = ns;
                            node.Diagonal.Value = 1.0;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Reset the row to 0.0 and return true if the row is a current equation
        /// </summary>
        /// <param name="matrix">The matrix</param>
        /// <param name="nodes">The list of nodes</param>
        /// <param name="rownum">The row number</param>
        /// <returns></returns>
        protected static bool ZeroNoncurRow(Matrix<double> matrix, Nodes nodes, int rownum)
        {
            bool currents = false;
            for (int n = 0; n < nodes.Count; n++)
            {
                var node = nodes[n];
                MatrixElement<double> x = matrix.FindElement(rownum, node.Index);
                if (x != null && x.Element.Value != 0.0)
                {
                    if (node.UnknownType == Node.NodeType.Current)
                        currents = true;
                    else
                        x.Element.Value = 0.0;
                }
            }
            return currents;
        }

        /// <summary>
        /// Check if we are converging during iterations
        /// </summary>
        /// <returns></returns>
        protected bool IsConvergent()
        {
            var circuit = Circuit;
            var rstate = RealState;
            var config = BaseConfiguration;

            // Check convergence for each node
            for (int i = 0; i < circuit.Nodes.Count; i++)
            {
                var node = circuit.Nodes[i];
                double n = rstate.Solution[node.Index];
                double o = rstate.OldSolution[node.Index];

                if (double.IsNaN(n))
                    throw new CircuitException("Non-convergence, node {0} is not a number.".FormatString(node));

                if (node.UnknownType == Node.NodeType.Voltage)
                {
                    double tol = config.RelativeTolerance * Math.Max(Math.Abs(n), Math.Abs(o)) + config.VoltageTolerance;
                    if (Math.Abs(n - o) > tol)
                    {
                        ProblemNode = node;
                        return false;
                    }
                }
                else
                {
                    double tol = config.RelativeTolerance * Math.Max(Math.Abs(n), Math.Abs(o)) + config.AbsoluteTolerance;
                    if (Math.Abs(n - o) > tol)
                    {
                        ProblemNode = node;
                        return false;
                    }
                }
            }

            // Device-level convergence tests
            foreach (var behavior in LoadBehaviors)
            {
                if (!behavior.IsConvergent(this))
                {
                    // I believe this should be false, but Spice 3f5 doesn't...

                    /*
                     * Each device that checks convergence returns (OK) = 0 regardless
                     * of convergence (eg. Dev2/mos2conv.c). Not being convergent is communicated
                     * through the CKTnoncon variable (state.IsCon for Spice#).
                     * 
                     * The convergence methods are called in CKT/cktop.c at line 121. If an error
                     * occurs, it is returned. If non-convergence is detected through CKTnoncon,
                     * (OK) is returned anyway, so it doesn't make a difference. Remember that 
                     * our devices aren't returning anything else but (OK) anyway.
                     * 
                     * CKTconvTest in turn is called in NI/niconv.c at line 65. The result is
                     * therefore always (OK) too, and so the returned value by NIconvTest()
                     * is always (OK) if each device has tested convergence. Note that 1 is
                     * returned in the case of non-convergence for nodes!
                     * 
                     * Finally, in NI/niiter.c at line 184, when convergence is tested, the result
                     * is used to overwrite CKTnoncon, so there is no way we can still find out if
                     * any device detected non-convergence.
                     */
                    return true;
                }
            }

            // Convergence succeeded
            return true;
        }
    }
}
