using System;
using SpiceSharp.Behaviors;
using SpiceSharp.IntegrationMethods;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A base class for time-domain analysis
    /// </summary>
    public abstract class TimeSimulation : BaseSimulation
    {
        /// <summary>
        /// Gets the currently active configuration
        /// </summary>
        public TimeConfiguration TimeConfiguration { get; protected set; }

        /// <summary>
        /// Gets the integration method
        /// </summary>
        public IntegrationMethod Method { get; protected set; }

        /// <summary>
        /// Gets the state pool
        /// </summary>
        public StatePool StatePool { get; private set; }

        /// <summary>
        /// Time-domain behaviors
        /// </summary>
        protected BehaviorList<BaseTransientBehavior> TransientBehaviors { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        protected TimeSimulation(Identifier name) : base(name)
        {
            ParameterSets.Add(new TimeConfiguration());
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="step">Timestep</param>
        /// <param name="final">Final time</param>
        protected TimeSimulation(Identifier name, double step, double final)
            : base(name)
        {
            ParameterSets.Add(new TimeConfiguration(step, final));
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="step">Timstep</param>
        /// <param name="stop">Stop</param>
        /// <param name="maxStep">Maximum timestep</param>
        protected TimeSimulation(Identifier name, double step, double stop, double maxStep)
            : base(name)
        {
            ParameterSets.Add(new TimeConfiguration(step, stop, maxStep));
        }

        /// <summary>
        /// Setup the simulation
        /// </summary>
        /// <param name="circuit">Circuit</param>
        protected override void Setup(Circuit circuit)
        {
            if (circuit == null)
                throw new ArgumentNullException(nameof(circuit));

            // Get base behaviors
            base.Setup(circuit);

            // Get behaviors and configurations
            var config = ParameterSets.Get<TimeConfiguration>() ?? throw new CircuitException("{0}: No time configuration".FormatString(Name));
            TimeConfiguration = config;
            Method = config.Method ?? throw new CircuitException("{0}: No integration method specified".FormatString(Name));
            TransientBehaviors = SetupBehaviors<BaseTransientBehavior>(circuit.Objects);

            // Setup the state pool and register states
            StatePool = new StatePool(Method);
            for (var i = 0; i < TransientBehaviors.Count; i++)
            {
                TransientBehaviors[i].GetEquationPointers(RealState.Solver);
                TransientBehaviors[i].CreateStates(StatePool);
            }
            StatePool.BuildStates();
        }

        /// <summary>
        /// Execute time simulation
        /// </summary>
        protected override void Execute()
        {
            // Base
            base.Execute();

            // Initialize the method
            Method.Initialize(TransientBehaviors);
            Method.Breaks.Clear();
            Method.Breaks.SetBreakpoint(TimeConfiguration.InitTime);
            Method.Breaks.SetBreakpoint(TimeConfiguration.FinalTime);
            Method.Breaks.MinBreak = TimeConfiguration.MaxStep * 5e-5;
        }

        /// <summary>
        /// Unsetup the simulation
        /// </summary>
        protected override void Unsetup()
        {
            // Remove references
            for (var i = 0; i < TransientBehaviors.Count; i++)
                TransientBehaviors[i].Unsetup();
            TransientBehaviors = null;
            Method = null;

            base.Unsetup();
        }

        /// <summary>
        /// Iterate for time-domain analysis
        /// </summary>
        /// <param name="maxIterations">Maximum iterations</param>
        protected bool TimeIterate(int maxIterations)
        {
            var state = RealState;
            var solver = state.Solver;
            var pass = false;
            var iterno = 0;

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
                    // Load the Y-matrix and Rhs-vector for DC and transients
                    TimeLoad();
                    iterno++;
                }
                catch (CircuitException)
                {
                    iterno++;
                    Statistics.Iterations = iterno;
                    throw;
                }

                // Preorder matrix
                if ((state.Sparse & RealState.SparseStates.DidPreorder) == 0) // !state.Sparse.HasFlag(RealState.SparseStates.DidPreorder)
                {
                    solver.PreorderModifiedNodalAnalysis(Math.Abs);
                    state.Sparse |= RealState.SparseStates.DidPreorder;
                }
                if (state.Init == RealState.InitializationStates.InitJunction || state.Init == RealState.InitializationStates.InitTransient)
                {
                    state.Sparse |= RealState.SparseStates.ShouldReorder;
                }

                // Reorder
                if ((state.Sparse & RealState.SparseStates.ShouldReorder) != 0) // state.Sparse.HasFlag(RealState.SparseStates.ShouldReorder))
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
                    var success = solver.Factor();
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
        protected void TimeLoad()
        {
            var state = RealState;

            // Start the stopwatch
            Statistics.LoadTime.Start();

            // Clear rhs and matrix
            state.Solver.Clear();

            // Load all devices
            for (var i = 0; i < LoadBehaviors.Count; i++)
                LoadBehaviors[i].Load(this);
            for (var i = 0; i < TransientBehaviors.Count; i++)
                TransientBehaviors[i].Transient(this);

            // Keep statistics
            Statistics.LoadTime.Stop();
        }
    }
}
