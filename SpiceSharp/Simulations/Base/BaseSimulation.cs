using System;
using System.Collections.Generic;
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
        protected List<LoadBehavior> loadbehaviors = null;
        protected List<TemperatureBehavior> tempbehaviors = null;
        protected List<IcBehavior> icbehaviors = null;

        /// <summary>
        /// Get the currently active configuration for the base simulation
        /// </summary>
        public BaseConfiguration BaseConfiguration { get; protected set; }

        /// <summary>
        /// Gets the current state of the circuit
        /// </summary>
        public State State { get; } = new State();

        /// <summary>
        /// Gets statistics
        /// </summary>
        public Statistics Statistics { get; } = new Statistics();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name"></param>
        public BaseSimulation(Identifier name)
            : base(name)
        {
            Parameters.Add(new BaseConfiguration());
        }

        /// <summary>
        /// Setup the simulation
        /// </summary>
        protected override void Setup()
        {
            // No use simulating an empty circuit
            if (Circuit.Objects.Count == 0)
                throw new CircuitException($"{Name}: No circuit objects for simulation");

            // Setup all objects
            Circuit.Objects.BuildOrderedComponentList();
            foreach (var o in Circuit.Objects)
            {
                o.Setup(Circuit);
            }
            if (Circuit.Nodes.Count < 1)
                throw new CircuitException($"{Name}: No circuit nodes for simulation");

            // Setup behaviors and configuration
            BaseConfiguration = Parameters.Get<BaseConfiguration>();
            tempbehaviors = SetupBehaviors<TemperatureBehavior>();
            loadbehaviors = SetupBehaviors<LoadBehavior>();
            icbehaviors = SetupBehaviors<IcBehavior>();

            // Setup the load behaviors
            var matrix = State.Matrix;
            foreach (var behavior in loadbehaviors)
                behavior.GetMatrixPointers(Circuit.Nodes, matrix);
        }

        /// <summary>
        /// Execute the simulation
        /// </summary>
        protected override void Execute()
        {
            // Do temperature-dependent calculations
            foreach (var behavior in tempbehaviors)
                behavior.Temperature(this);

            // Initialize the solution
            State.Initialize(Circuit);

            // Do initial conditions
            Ic();
        }

        /// <summary>
        /// Unsetup the simulation
        /// </summary>
        protected override void Unsetup()
        {
            // Unsetup all behaviors
            foreach (var behavior in icbehaviors)
                behavior.Unsetup();
            foreach (var behavior in tempbehaviors)
                behavior.Unsetup();
            foreach (var behavior in loadbehaviors)
                behavior.Unsetup();

            // Remove behavior and configuration references
            loadbehaviors.Clear();
            loadbehaviors = null;
            icbehaviors.Clear();
            icbehaviors = null;
            BaseConfiguration = null;

            // Unsetup all objects
            foreach (var o in Circuit.Objects)
                o.Unsetup(Circuit);

            // Clear the state
            State.Clear();

            // Clear the circuit
            Circuit.Clear();
        }

        /// <summary>
        /// Calculate the operating point of the circuit
        /// </summary>
        /// <param name="maxiter">Maximum iterations</param>
        protected void Op(int maxiter)
        {
            var state = State;
            var config = BaseConfiguration;
            state.Init = State.InitFlags.InitJct;
            state.Matrix.Complex = false;

            // First, let's try finding an operating point by using normal iterations
            if (!config.NoOpIter)
            {
                if (Iterate(maxiter))
                    return;
            }

            // No convergence, try Gmin stepping
            if (config.NumGminSteps > 1)
            {
                state.Init = State.InitFlags.InitJct;
                CircuitWarning.Warning(this, "Starting Gmin stepping");
                state.DiagGmin = config.Gmin;
                for (int i = 0; i < config.NumGminSteps; i++)
                    state.DiagGmin *= 10.0;
                for (int i = 0; i <= config.NumGminSteps; i++)
                {
                    state.IsCon = false;
                    if (!Iterate(maxiter))
                    {
                        state.DiagGmin = 0.0;
                        CircuitWarning.Warning(this, "Gmin step failed");
                        break;
                    }
                    state.DiagGmin /= 10.0;
                    state.Init = State.InitFlags.InitFloat;
                }
                state.DiagGmin = 0.0;
                if (Iterate(maxiter))
                    return;
            }

            // Nope, still not converging, let's try source stepping
            if (config.NumSrcSteps > 1)
            {
                state.Init = State.InitFlags.InitJct;
                CircuitWarning.Warning(this, "Starting source stepping");
                for (int i = 0; i <= config.NumSrcSteps; i++)
                {
                    state.SrcFact = i / (double)config.NumSrcSteps;
                    if (!Iterate(maxiter))
                    {
                        state.SrcFact = 1.0;
                        // ckt.CurrentAnalysis = AnalysisType.DoingTran;
                        CircuitWarning.Warning(this, "Source stepping failed");
                        return;
                    }
                }
                state.SrcFact = 1.0;
                return;
            }

            // Failed
            throw new CircuitException("Could not determine operating point");
        }

        /// <summary>
        /// Solve iteratively for simulations
        /// </summary>
        /// <param name="maxiter">Maximum number of iterations</param>
        /// <returns></returns>
        protected bool Iterate(int maxiter)
        {
            var state = State;
            var matrix = state.Matrix;
            bool pass = false;
            int iterno = 0;

            // Make sure we're using real numbers!
            matrix.Complex = false;

            // Initialize the state of the circuit
            if (!state.Initialized)
                state.Initialize(Circuit);

            // Ignore operating condition point, just use the solution as-is
            if (state.UseIC && state.Domain == State.DomainTypes.Time)
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
                state.IsCon = true;

                try
                {
                    Load();
                    iterno++;
                }
                catch (CircuitException)
                {
                    iterno++;
                    Statistics.NumIter = iterno;
                    throw;
                }

                // Preorder matrix
                if (!state.Sparse.HasFlag(State.SparseFlags.NIDIDPREORDER))
                {
                    matrix.PreOrder();
                    state.Sparse |= State.SparseFlags.NIDIDPREORDER;
                }
                if (state.Init == State.InitFlags.InitJct || state.Init == State.InitFlags.InitTransient)
                {
                    state.Sparse |= State.SparseFlags.NISHOULDREORDER;
                }

                // Reorder
                if (state.Sparse.HasFlag(State.SparseFlags.NISHOULDREORDER))
                {
                    Statistics.ReorderTime.Start();
                    matrix.Reorder(state.PivotRelTol, state.PivotAbsTol, state.DiagGmin);
                    Statistics.ReorderTime.Stop();
                    state.Sparse &= ~State.SparseFlags.NISHOULDREORDER;
                }
                else
                {
                    // Decompose
                    Statistics.DecompositionTime.Start();
                    matrix.Factor(state.DiagGmin);
                    Statistics.DecompositionTime.Stop();
                }

                // Solve the equation
                Statistics.SolveTime.Start();
                matrix.Solve(state.Rhs);
                Statistics.SolveTime.Stop();

                // The result is now stored in the RHS vector, let's move it to the current solution vector
                state.StoreSolution();

                // Reset ground nodes
                state.Rhs[0] = 0.0;
                state.Solution[0] = 0.0;
                state.OldSolution[0] = 0.0;

                // Exceeded maximum number of iterations
                if (iterno > maxiter)
                {
                    Statistics.NumIter += iterno;
                    return false;
                }

                if (state.IsCon && iterno != 1)
                    state.IsCon = IsConvergent();
                else
                    state.IsCon = false;

                switch (state.Init)
                {
                    case State.InitFlags.InitFloat:
                        if (state.UseDC && state.HadNodeset)
                        {
                            if (pass)
                                state.IsCon = false;
                            pass = false;
                        }
                        if (state.IsCon)
                        {
                            Statistics.NumIter += iterno;
                            return true;
                        }
                        break;

                    case State.InitFlags.InitJct:
                        state.Init = State.InitFlags.InitFix;
                        state.Sparse |= State.SparseFlags.NISHOULDREORDER;
                        break;

                    case State.InitFlags.InitFix:
                        if (state.IsCon)
                            state.Init = State.InitFlags.InitFloat;
                        pass = true;
                        break;

                    case State.InitFlags.InitTransient:
                        if (iterno <= 1)
                            state.Sparse = State.SparseFlags.NISHOULDREORDER;
                        state.Init = State.InitFlags.InitFloat;
                        break;

                    case State.InitFlags.Init:
                        state.Init = State.InitFlags.InitFloat;
                        break;

                    default:
                        Statistics.NumIter += iterno;
                        throw new CircuitException("Could not find flag");
                }
            }
        }

        /// <summary>
        /// Load the circuit with the load behaviors
        /// </summary>
        protected void Load()
        {
            var state = State;
            var nodes = Circuit.Nodes;

            // Start the stopwatch
            Statistics.LoadTime.Start();

            // Clear rhs and matrix
            state.Clear();

            // Load all devices
            foreach (var behavior in loadbehaviors)
                behavior.Load(this);

            // Check modes
            if (state.UseDC)
            {
                // Consider doing nodeset & ic assignments
                if ((state.Init & (State.InitFlags.InitJct | State.InitFlags.InitFix)) != 0)
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
                                node.Diagonal.Value.Real = 1e10;
                            }
                            else
                            {
                                state.Rhs[node.Index] = ns;
                                node.Diagonal.Value.Real = 1.0;
                            }
                        }
                    }
                }

                if (state.Domain == State.DomainTypes.Time && !state.UseIC)
                {
                    for (int i = 0; i < nodes.Count; i++)
                    {
                        var node = nodes[i];
                        if (nodes.InitialConditions.ContainsKey(node.Name))
                        {
                            double ic = nodes.InitialConditions[node.Name];
                            if (ZeroNoncurRow(state.Matrix, nodes, node.Index))
                            {
                                state.Rhs[node.Index] = 1.0e10 * ic;
                                node.Diagonal.Value.Real = 1e10;
                            }
                            else
                            {
                                state.Rhs[node.Index] = ic;
                                node.Diagonal.Value.Real = 1.0;
                            }
                        }
                    }
                }
            }

            // Keep statistics
            Statistics.LoadTime.Stop();
        }

        /// <summary>
        /// Set the initial conditions
        /// </summary>
        protected void Ic()
        {
            var ckt = Circuit;
            var state = State;
            var nodes = ckt.Nodes;

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
                    state.HadNodeset = true;
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
                foreach (var behavior in icbehaviors)
                    behavior.SetIc(ckt);
            }
        }

        /// <summary>
        /// Reset the row to 0.0 and return true if the row is a current equation
        /// </summary>
        /// <param name="matrix">The matrix</param>
        /// <param name="nodes">The list of nodes</param>
        /// <param name="rownum">The row number</param>
        /// <returns></returns>
        bool ZeroNoncurRow(Matrix matrix, Nodes nodes, int rownum)
        {
            bool currents = false;
            for (int n = 0; n < nodes.Count; n++)
            {
                var node = nodes[n];
                MatrixElement x = matrix.FindElement(rownum, node.Index);
                if (x != null && x.Value.Real != 0.0)
                {
                    if (node.UnknownType == Node.NodeType.Current)
                        currents = true;
                    else
                        x.Value.Real = 0.0;
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
            var ckt = Circuit;
            var rstate = State;
            var config = BaseConfiguration;

            // Check convergence for each node
            for (int i = 0; i < ckt.Nodes.Count; i++)
            {
                var node = ckt.Nodes[i];
                double n = rstate.Solution[node.Index];
                double o = rstate.OldSolution[node.Index];

                if (double.IsNaN(n))
                    throw new CircuitException($"Non-convergence, node {node} is not a number.");

                if (node.UnknownType == Node.NodeType.Voltage)
                {
                    double tol = config.RelTol * Math.Max(Math.Abs(n), Math.Abs(o)) + config.VoltTol;
                    if (Math.Abs(n - o) > tol)
                    {
                        ProblemNode = node;
                        return false;
                    }
                }
                else
                {
                    double tol = config.RelTol * Math.Max(Math.Abs(n), Math.Abs(o)) + config.AbsTol;
                    if (Math.Abs(n - o) > tol)
                    {
                        ProblemNode = node;
                        return false;
                    }
                }
            }

            // Device-level convergence tests
            foreach (var behavior in loadbehaviors)
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

        /// <summary>
        /// Create an export method for this type of simulation
        /// The simulation will determine which export method is returned if multiple behaviors implement a export method by the same name
        /// </summary>
        /// <param name="name">The identifier of the entity</param>
        /// <param name="property">The parameter name</param>
        /// <returns></returns>
        public override Func<State, double> CreateExport(Identifier name, string property)
        {
            var eb = pool.GetEntityBehaviors(name) ?? throw new CircuitException($"{Name}: Could not find behaviors of {name}");
            return eb.Get<LoadBehavior>()?.CreateExport(property);
        }

        /// <summary>
        /// Create an export method for this type of simulation 
        /// </summary>
        /// <param name="pos">Positive voltage</param>
        /// <param name="neg">Negative voltage</param>
        /// <returns></returns>
        public virtual Func<State, double> CreateVoltageExport(Identifier pos, Identifier neg = null)
        {
            int node = Circuit.Nodes[pos].Index;
            if (neg == null)   
                return (State state) => state.Solution[node];
            int refnode = Circuit.Nodes[neg].Index;
            return (State state) => state.Solution[node] - state.Solution[refnode];
        }
    }
}
