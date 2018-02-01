using System;
using System.Collections.ObjectModel;
using SpiceSharp.Behaviors;
using SpiceSharp.IntegrationMethods;
using SpiceSharp.Diagnostics;
using SpiceSharp.Sparse;

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
        /// Gets all states in the simulation
        /// </summary>
        public StatePool States { get; protected set; }

        /// <summary>
        /// Time-domain behaviors
        /// </summary>
        protected Collection<TransientBehavior> TransientBehaviors { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        protected TimeSimulation(Identifier name) : base(name)
        {
        }

        /// <summary>
        /// Setup the simulation
        /// </summary>
        protected override void Setup()
        {
            // Get base behaviors
            base.Setup();

            // Get behaviors and configurations
            var config = Parameters.Get<TimeConfiguration>();
            TimeConfiguration = config;
            TransientBehaviors = SetupBehaviors<TransientBehavior>();

            // Also configure the method
            Method = TimeConfiguration.Method ?? throw new CircuitException("{0}: No integration method specified".FormatString(Name));
            Method.Breaks.Clear();
            Method.Breaks.SetBreakpoint(config.InitTime);
            Method.Breaks.SetBreakpoint(config.FinalTime);
            Method.Breaks.MinBreak = config.MaxStep * 5e-5;

            // Setup the state pool and register states
            States = new StatePool(Method);
            foreach (var behavior in TransientBehaviors)
            {
                behavior.GetMatrixPointers(RealState.Matrix);
                behavior.CreateStates(States);
            }
            States.BuildStates();
        }

        /// <summary>
        /// Execute time simulation
        /// </summary>
        protected override void Execute()
        {
            // Base
            base.Execute();

            // Get the method
            Method = States.Method;
        }

        /// <summary>
        /// Unsetup the simulation
        /// </summary>
        protected override void Unsetup()
        {
            // Remove references
            foreach (var behavior in TransientBehaviors)
                behavior.Unsetup();
            TransientBehaviors.Clear();
            TransientBehaviors = null;
            Method = null;

            base.Unsetup();
        }

        /// <summary>
        /// Iterate for time-domain analysis
        /// </summary>
        /// <param name="maxIterations">Maximum iterations</param>
        protected bool TranIterate(int maxIterations)
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
                state.IsCon = true;

                try
                {
                    // Load the Y-matrix and Rhs-vector for DC and transients
                    Load();
                    foreach (var behavior in TransientBehaviors)
                        behavior.Transient(this);
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

                if (state.IsCon && iterno != 1)
                    state.IsCon = IsConvergent();
                else
                    state.IsCon = false;

                switch (state.Init)
                {
                    case RealState.InitializationStates.InitFloat:
                        if (state.UseDC && state.HadNodeSet)
                        {
                            if (pass)
                                state.IsCon = false;
                            pass = false;
                        }
                        if (state.IsCon)
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
                        if (state.IsCon)
                            state.Init = RealState.InitializationStates.InitFloat;
                        pass = true;
                        break;

                    case RealState.InitializationStates.InitTransient:
                        if (iterno <= 1)
                            state.Sparse = RealState.SparseStates.ShouldReorder;
                        state.Init = RealState.InitializationStates.InitFloat;
                        break;

                    case RealState.InitializationStates.Init:
                        state.Init = RealState.InitializationStates.InitFloat;
                        break;

                    default:
                        Statistics.Iterations += iterno;
                        throw new CircuitException("Could not find flag");
                }
            }
        }

        /// <summary>
        /// Create an export method for this type of simulation
        /// The simulation will determine which export method is returned if multiple behaviors implement an export method by the same name.
        /// </summary>
        /// <param name="name">The identifier of the entity</param>
        /// <param name="propertyName">The parameter name</param>
        /// <returns></returns>
        public override Func<RealState, double> CreateExport(Identifier name, string propertyName)
        {
            var eb = Pool.GetEntityBehaviors(name) ?? throw new CircuitException("{0}: Could not find behaviors of {1}".FormatString(Name, name));

            // For transient analysis, the most logical would be to ask the Transient behavior (if it exists)
            var export = eb.Get<TransientBehavior>()?.CreateExport(propertyName);

            // If the transient behavior does not implement the export method, resort to the Load behavior
            if (export == null)
                export = eb.Get<LoadBehavior>()?.CreateExport(propertyName);
            return export;
        }
    }
}
