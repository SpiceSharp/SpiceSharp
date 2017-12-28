using System.Collections.Generic;
using SpiceSharp.Behaviors;
using SpiceSharp.Attributes;
using SpiceSharp.IntegrationMethods;
using SpiceSharp.Diagnostics;
using SpiceSharp.Circuits;
using SpiceSharp.Sparse;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A base class for time-domain analysis
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class TimeSimulation : BaseSimulation
    {
        /// <summary>
        /// Gets or sets the initial timepoint that should be exported
        /// </summary>
        [SpiceName("init"), SpiceName("start"), SpiceInfo("The starting timepoint")]
        public double InitTime { get; set; } = 0.0;

        /// <summary>
        /// Gets or sets the final simulation timepoint
        /// </summary>
        [SpiceName("final"), SpiceName("stop"), SpiceInfo("The final timepoint")]
        public double FinalTime { get; set; } = double.NaN;

        /// <summary>
        /// Gets or sets the step
        /// </summary>
        [SpiceName("step"), SpiceInfo("The timestep")]
        public double Step { get; set; } = double.NaN;

        /// <summary>
        /// Gets or sets the maximum timestep
        /// </summary>
        [SpiceName("maxstep"), SpiceInfo("The maximum allowed timestep")]
        public double MaxStep
        {
            get
            {
                if (double.IsNaN(maxstep))
                    return (FinalTime - InitTime) / 50.0;
                return maxstep;
            }
            set { maxstep = value; }
        }
        private double maxstep = double.NaN;

        /// <summary>
        /// Get the minimum timestep allowed
        /// </summary>
        [SpiceName("deltamin"), SpiceInfo("The minimum delta for breakpoints")]
        public double DeltaMin { get { return 1e-13 * MaxStep; } }

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
        protected List<TransientBehavior> tranbehaviors = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public TimeSimulation(Identifier name) : base(name)
        {
        }

        /// <summary>
        /// Setup the simulation
        /// </summary>
        protected override void Setup()
        {
            // Get base behaviors
            base.Setup();

            // Also configure the method
            Method = CurrentConfig.Method ?? throw new CircuitException($"{Name}: No integration method specified");
            Method.Breaks.Clear();
            Method.Breaks.SetBreakpoint(InitTime);
            Method.Breaks.SetBreakpoint(FinalTime);
            Method.Breaks.MinBreak = MaxStep * 5e-5;
            
            // Get behaviors
            tranbehaviors = SetupBehaviors<TransientBehavior>();

            // Setup the state pool and register states
            States = new StatePool(Method);
            foreach (var behavior in tranbehaviors)
                behavior.CreateStates(States);
            States.BuildStates();
        }

        /// <summary>
        /// Unsetup the simulation
        /// </summary>
        protected override void Unsetup()
        {
            // Remove references
            foreach (var behavior in tranbehaviors)
                behavior.Unsetup();
            tranbehaviors.Clear();
            tranbehaviors = null;
            Method = null;

            base.Unsetup();
        }

        /// <summary>
        /// Iterate for time-domain analysis
        /// </summary>
        /// <param name="ckt">Circuit</param>
        protected bool TranIterate(Circuit ckt, int maxiter)
        {
            var state = ckt.State;
            var matrix = state.Matrix;
            bool pass = false;
            int iterno = 0;

            // Make sure we're using real numbers!
            matrix.Complex = false;

            // Initialize the state of the circuit
            if (!state.Initialized)
                state.Initialize(ckt);

            // Ignore operating condition point, just use the solution as-is
            if (ckt.State.UseIC && ckt.State.Domain == State.DomainTypes.Time)
            {
                state.StoreSolution();

                // Voltages are set using IC statement on the nodes
                // Internal initial conditions are calculated by the components
                Load(ckt);

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
                    Load(ckt);
                    foreach (var behavior in tranbehaviors)
                        behavior.Transient(this);
                    iterno++;
                }
                catch (CircuitException)
                {
                    iterno++;
                    ckt.Statistics.NumIter = iterno;
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
                    ckt.Statistics.ReorderTime.Start();
                    matrix.Reorder(state.PivotRelTol, state.PivotAbsTol, state.DiagGmin);
                    ckt.Statistics.ReorderTime.Stop();
                    state.Sparse &= ~State.SparseFlags.NISHOULDREORDER;
                }
                else
                {
                    // Decompose
                    ckt.Statistics.DecompositionTime.Start();
                    matrix.Factor(state.DiagGmin);
                    ckt.Statistics.DecompositionTime.Stop();
                }

                // Solve the equation
                ckt.Statistics.SolveTime.Start();
                matrix.Solve(state.Rhs);
                ckt.Statistics.SolveTime.Stop();

                // The result is now stored in the RHS vector, let's move it to the current solution vector
                state.StoreSolution();

                // Reset ground nodes
                ckt.State.Rhs[0] = 0.0;
                ckt.State.Solution[0] = 0.0;
                ckt.State.OldSolution[0] = 0.0;

                // Exceeded maximum number of iterations
                if (iterno > maxiter)
                {
                    ckt.Statistics.NumIter += iterno;
                    return false;
                }

                if (state.IsCon && iterno != 1)
                    state.IsCon = IsConvergent(ckt);
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
                            ckt.Statistics.NumIter += iterno;
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
                        ckt.Statistics.NumIter += iterno;
                        throw new CircuitException("Could not find flag");
                }
            }
        }
    }
}
