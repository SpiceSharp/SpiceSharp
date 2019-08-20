using System.Collections.Generic;
using SpiceSharp.Behaviors;
using SpiceSharp.Circuits;
using SpiceSharp.IntegrationMethods;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A base class for time-domain analysis.
    /// </summary>
    /// <seealso cref="SpiceSharp.Simulations.BaseSimulation" />
    public abstract class TimeSimulation : BaseSimulation
    {
        /// <summary>
        /// Gets the active integration method.
        /// </summary>
        public IntegrationMethod Method { get; protected set; }

        /// <summary>
        /// Time-domain behaviors.
        /// </summary>
        private BehaviorList<ITimeBehavior> _transientBehaviors;
        private BehaviorList<IAcceptBehavior> _acceptBehaviors;
        private readonly List<ConvergenceAid> _initialConditions = new List<ConvergenceAid>();
        private bool _shouldReorder = true, _useIc;

        /// <summary>
        /// Time simulation statistics.
        /// </summary>
        protected TimeSimulationStatistics TimeSimulationStatistics { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeSimulation"/> class.
        /// </summary>
        /// <param name="name">The identifier of the simulation.</param>
        protected TimeSimulation(string name) : base(name)
        {
            Configurations.Add(new TimeConfiguration());
            TimeSimulationStatistics = new TimeSimulationStatistics();
            Statistics.Add(typeof(TimeSimulationStatistics), TimeSimulationStatistics);

            // Add the behavior in the order they are (usually) called
            BehaviorTypes.AddRange(new []
            {
                typeof(ITimeBehavior),
                typeof(IAcceptBehavior)
            });
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeSimulation"/> class.
        /// </summary>
        /// <param name="name">The identifier of the simulation.</param>
        /// <param name="step">The step size.</param>
        /// <param name="final">The final time.</param>
        protected TimeSimulation(string name, double step, double final)
            : base(name)
        {
            Configurations.Add(new TimeConfiguration(step, final));
            TimeSimulationStatistics = new TimeSimulationStatistics();
            Statistics.Add(typeof(TimeSimulationStatistics), TimeSimulationStatistics);

            // Add the behavior in the order they are (usually) called
            BehaviorTypes.AddRange(new []
            {
                typeof(ITimeBehavior),
                typeof(IAcceptBehavior)
            });
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeSimulation"/> class.
        /// </summary>
        /// <param name="name">The identifier of the simulation.</param>
        /// <param name="step">The step size.</param>
        /// <param name="final">The final time.</param>
        /// <param name="maxStep">The maximum step.</param>
        protected TimeSimulation(string name, double step, double final, double maxStep)
            : base(name)
        {
            Configurations.Add(new TimeConfiguration(step, final, maxStep));
            TimeSimulationStatistics = new TimeSimulationStatistics();
            Statistics.Add(typeof(TimeSimulationStatistics), TimeSimulationStatistics);

            // Add the behavior in the order they are (usually) called
            BehaviorTypes.AddRange(new []
            {
                typeof(ITimeBehavior),
                typeof(IAcceptBehavior)
            });
        }

        /// <summary>
        /// Set up the simulation.
        /// </summary>
        /// <param name="entities">The circuit that will be used.</param>
        protected override void Setup(EntityCollection entities)
        {
            entities.ThrowIfNull(nameof(entities));

            // Get behaviors and configurations
            var config = Configurations.Get<TimeConfiguration>().ThrowIfNull("time configuration");
            _useIc = config.UseIc;
            Method = config.Method.ThrowIfNull("method");

            // Setup
            base.Setup(entities);

            // Cache local variables
            _transientBehaviors = EntityBehaviors.GetBehaviorList<ITimeBehavior>();
            _acceptBehaviors = EntityBehaviors.GetBehaviorList<IAcceptBehavior>();

            Method.Setup(this);

            // Set up initial conditions
            foreach (var ic in config.InitialConditions)
                _initialConditions.Add(new ConvergenceAid(ic.Key, ic.Value));
        }

        /// <summary>
        /// Executes the simulation.
        /// </summary>
        protected override void Execute()
        {
            base.Execute();

            // Apply initial conditions if they are not set for the devices (UseIc).
            if (_initialConditions.Count > 0 && !RealState.UseIc)
            {
                // Initialize initial conditions
                foreach (var ic in _initialConditions)
                    ic.Initialize(this);
                AfterLoad += LoadInitialConditions;
            }

            // Calculate the operating point of the circuit
            var state = RealState;
            state.UseIc = _useIc;
            state.UseDc = true;
            Op(DcMaxIterations);
            TimeSimulationStatistics.TimePoints++;

            // Stop calculating the operating point
            state.UseIc = false;
            state.UseDc = false;
            InitializeStates();
            AfterLoad -= LoadInitialConditions;
        }

        /// <summary>
        /// Destroys the simulation.
        /// </summary>
        protected override void Unsetup()
        {
            // Remove references
            for (var i = 0; i < _transientBehaviors.Count; i++)
                _transientBehaviors[i].Unbind();
            _transientBehaviors = null;
            for (var i = 0; i < _acceptBehaviors.Count; i++)
                _acceptBehaviors[i].Unbind();
            _acceptBehaviors = null;

            // Destroy the integration method
            Method.Unsetup(this);
            Method = null;

            // Destroy the initial conditions
            AfterLoad -= LoadInitialConditions;
            foreach (var ic in _initialConditions)
                ic.Unsetup();
            _initialConditions.Clear();

            base.Unsetup();
        }

        /// <summary>
        /// Iterates to a solution for time simulations.
        /// </summary>
        /// <param name="maxIterations">The maximum number of iterations.</param>
        /// <returns>
        ///   <c>true</c> if the iterations converged to a solution; otherwise, <c>false</c>.
        /// </returns>
        protected bool TimeIterate(int maxIterations)
        {
            var state = RealState;
            var solver = state.Solver;
            // var pass = false;
            var iterno = 0;
            var initTransient = Method.BaseTime.Equals(0.0);

            // Ignore operating condition point, just use the solution as-is
            if (state.UseIc)
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
                    Load();
                    iterno++;
                }
                catch (CircuitException)
                {
                    iterno++;
                    BaseSimulationStatistics.Iterations = iterno;
                    throw;
                }

                // Preordering is already done in the operating point calculation

                if (state.Init == InitializationModes.Junction || initTransient)
                    _shouldReorder = true;

                // Reorder
                if (_shouldReorder)
                {
                    BaseSimulationStatistics.ReorderTime.Start();
                    solver.OrderAndFactor();
                    BaseSimulationStatistics.ReorderTime.Stop();
                    _shouldReorder = false;
                }
                else
                {
                    // Decompose
                    BaseSimulationStatistics.DecompositionTime.Start();
                    var success = solver.Factor();
                    BaseSimulationStatistics.DecompositionTime.Stop();

                    if (!success)
                    {
                        _shouldReorder = true;
                        continue;
                    }
                }

                // The current solution becomes the old solution
                state.StoreSolution();

                // Solve the equation
                BaseSimulationStatistics.SolveTime.Start();
                solver.Solve(state.Solution);
                BaseSimulationStatistics.SolveTime.Stop();

                // Reset ground nodes
                state.Solution[0] = 0.0;
                state.OldSolution[0] = 0.0;

                // Exceeded maximum number of iterations
                if (iterno > maxIterations)
                {
                    BaseSimulationStatistics.Iterations += iterno;
                    return false;
                }

                if (state.IsConvergent && iterno != 1)
                    state.IsConvergent = IsConvergent();
                else
                    state.IsConvergent = false;

                if (initTransient)
                {
                    initTransient = false;
                    if (iterno <= 1)
                        _shouldReorder = true;
                    state.Init = InitializationModes.Float;
                }
                else
                {
                    switch (state.Init)
                    {
                        case InitializationModes.Float:
                            if (state.IsConvergent)
                            {
                                BaseSimulationStatistics.Iterations += iterno;
                                return true;
                            }

                            break;

                        case InitializationModes.Junction:
                            state.Init = InitializationModes.Fix;
                            _shouldReorder = true;
                            break;

                        case InitializationModes.Fix:
                            if (state.IsConvergent)
                                state.Init = InitializationModes.Float;
                            // pass = true;
                            break;

                        case InitializationModes.None:
                            state.Init = InitializationModes.Float;
                            break;

                        default:
                            BaseSimulationStatistics.Iterations += iterno;
                            throw new CircuitException("Could not find flag");
                    }
                }
            }
        }

        /// <summary>
        /// Initializes all transient behaviors to assume that the current solution is the DC solution.
        /// </summary>
        protected virtual void InitializeStates()
        {
            for (var i = 0; i < _transientBehaviors.Count; i++)
                _transientBehaviors[i].InitializeStates();
            Method.Initialize(this);
        }

        /// <summary>
        /// Applies initial conditions.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Arguments</param>
        protected void LoadInitialConditions(object sender, LoadStateEventArgs e)
        {
            foreach (var ic in _initialConditions)
                ic.Aid();
        }

        /// <summary>
        /// Load all behaviors for time simulation.
        /// </summary>
        protected override void LoadBehaviors()
        {
            base.LoadBehaviors();

            // Not calculating DC behavior
            if (!RealState.UseDc)
            {
                for (var i = 0; i < _transientBehaviors.Count; i++)
                    _transientBehaviors[i].Load();
            }
        }

        /// <summary>
        /// Accepts the current simulation state as a valid timepoint.
        /// </summary>
        protected void Accept()
        {
            for (var i = 0; i < _acceptBehaviors.Count; i++)
                _acceptBehaviors[i].Accept();
            Method.Accept(this);
            TimeSimulationStatistics.Accepted++;
        }

        /// <summary>
        /// Probe for a new time point.
        /// </summary>
        /// <param name="delta">The timestep.</param>
        protected void Probe(double delta)
        {
            Method.Probe(this, delta);
            for (var i = 0; i < _acceptBehaviors.Count; i++)
                _acceptBehaviors[i].Probe();
        }
    }
}
