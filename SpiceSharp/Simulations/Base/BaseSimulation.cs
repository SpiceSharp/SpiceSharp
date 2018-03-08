using System;
using System.Collections.ObjectModel;
using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Circuits;
using SpiceSharp.Diagnostics;

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
        protected Collection<BaseLoadBehavior> LoadBehaviors { get; private set; }
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
            TemperatureBehaviors = SetupBehaviors<TemperatureBehavior>();
            LoadBehaviors = SetupBehaviors<BaseLoadBehavior>();
            InitialConditionBehaviors = SetupBehaviors<InitialConditionBehavior>();

            // Setup the load behaviors
            RealState = States.Get<RealState>();
            foreach (var behavior in LoadBehaviors)
                behavior.GetEquationPointers(Circuit.Nodes, RealState.Solver);
            RealState.Initialize(Circuit.Nodes);

            // Allow nodesets to help convergence
            OnLoad += LoadNodeSets;
        }

        /// <summary>
        /// Execute the simulation
        /// </summary>
        protected override void Execute()
        {
            // Do temperature-dependent calculations
            foreach (var behavior in TemperatureBehaviors)
                behavior.Temperature(this);

            // Do initial conditions
            InitialConditions();
        }

        /// <summary>
        /// Unsetup the simulation
        /// </summary>
        protected override void Unsetup()
        {
            // Remove nodeset
            OnLoad -= LoadNodeSets;

            // Unsetup all behaviors
            foreach (var behavior in InitialConditionBehaviors)
                behavior.Unsetup();
            foreach (var behavior in LoadBehaviors)
                behavior.Unsetup();
            foreach (var behavior in TemperatureBehaviors)
                behavior.Unsetup();

            // Clear the state
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
            var solver = state.Solver;
            bool pass = false;
            int iterno = 0;

            // Ignore operating condition point, just use the solution as-is
            if (state.UseIc && state.Domain == RealState.DomainType.Time)
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
                    solver.PreorderModifiedNodalAnalysis(Math.Abs);
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
                    solver.ApplyDiagonalGmin(state.DiagonalGmin);
                    solver.OrderAndFactor();
                    Statistics.ReorderTime.Stop();
                    state.Sparse &= ~RealState.SparseStates.ShouldReorder;
                }
                else
                {
                    // Decompose
                    Statistics.DecompositionTime.Start();
                    solver.ApplyDiagonalGmin(state.DiagonalGmin);
                    bool success = solver.Factor();
                    Statistics.DecompositionTime.Stop();

                    if (!success)
                    {
                        state.Sparse |= RealState.SparseStates.ShouldReorder;
                        continue;
                    }
                }

                // The current solution becomes the old solution
                state.StoreSolution();

                // Solve the equation
                Statistics.SolveTime.Start();
                solver.Solve(state.Solution);
                Statistics.SolveTime.Stop();

                // Reset ground nodes
                solver.GetRhsElement(0).Value = 0.0;
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
                        if (state.UseDc && state.HadNodeSet)
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

            // Start the stopwatch
            Statistics.LoadTime.Start();

            // Clear rhs and matrix
            state.Solver.Clear();

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
            var solver = state.Solver;

            // Clear the current solution
            var element = solver.FirstInReorderedRhs();
            while (element != null)
            {
                element.Value = 0.0;
                element = element.Next;
            }

            // Go over all nodes
            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                if (nodes.NodeSets.ContainsKey(node.Name))
                {
                    node.Diagonal = solver.GetMatrixElement(node.Index, node.Index);

                    // Avoid creating a sparse element if it is not needed
                    if (!nodes.NodeSets[node.Name].Equals(0.0))
                        solver.GetRhsElement(node.Index).Value = nodes.NodeSets[node.Name];
                    state.HadNodeSet = true;
                }
                if (nodes.InitialConditions.ContainsKey(node.Name))
                {
                    node.Diagonal = solver.GetMatrixElement(node.Index, node.Index);

                    // Avoid creating a sparse element if it is not needed
                    if (!nodes.InitialConditions[node.Name].Equals(0.0))
                        solver.GetRhsElement(node.Index).Value = nodes.InitialConditions[node.Name];
                }
            }

            // Use initial conditions
            if (state.UseIc)
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
        protected void LoadNodeSets(object sender, LoadStateEventArgs e)
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
                        if (ZeroNoncurrentRow(state.Solver, nodes, node.Index))
                        {
                            if (!ns.Equals(0.0))
                                state.Solver.GetRhsElement(node.Index).Value = 1.0e10 * ns;
                            node.Diagonal.Value = 1.0e10;
                        }
                        else
                        {
                            if (!ns.Equals(0.0))
                                state.Solver.GetRhsElement(node.Index).Value = ns;
                            node.Diagonal.Value = 1.0;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Reset the row to 0.0 and return true if the row is a current equation
        /// </summary>
        /// <param name="solver">Solver</param>
        /// <param name="nodes">List of nodes</param>
        /// <param name="rowIndex">Row number</param>
        /// <returns></returns>
        protected static bool ZeroNoncurrentRow(SparseLinearSystem<double> solver, Nodes nodes, int rowIndex)
        {
            if (solver == null)
                throw new ArgumentNullException(nameof(solver));
            if (nodes == null)
                throw new ArgumentNullException(nameof(nodes));

            bool currents = false;
            for (int n = 0; n < nodes.Count; n++)
            {
                var node = nodes[n];
                MatrixElement<double> x = solver.FindMatrixElement(rowIndex, node.Index);
                if (x != null && !x.Value.Equals(0.0))
                {
                    if (node.UnknownType == Node.NodeType.Current)
                        currents = true;
                    else
                        x.Value = 0.0;
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
