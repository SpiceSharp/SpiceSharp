using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.ParameterSets;
using SpiceSharp.Simulations.IntegrationMethods;
using SpiceSharp.Simulations.Time;
using SpiceSharp.Validation;
using System;
using System.Collections.Generic;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A base class for time-domain analysis.
    /// </summary>
    /// <seealso cref="BiasingSimulation" />
    public partial class Transient : BiasingSimulation,
        ITimeSimulation,
        IBehavioral<IAcceptBehavior>,
        IBehavioral<ITruncatingBehavior>,
        IParameterized<TimeParameters>
    {
        /// <summary>
        /// Time-domain behaviors.
        /// </summary>
        private BehaviorList<ITimeBehavior> _transientBehaviors;
        private BehaviorList<IAcceptBehavior> _acceptBehaviors;
        private BehaviorList<ITruncatingBehavior> _truncatingBehaviors;
        private readonly List<ConvergenceAid> _initialConditions = [];
        private bool _shouldReorder = true;
        private IIntegrationMethod _method;
        private readonly SimulationState _time;

        /// <summary>
        /// The constant returned when exporting the operating point.
        /// </summary>
        public const int ExportOperatingPoint = 0x01;

        /// <summary>
        /// The constant returned when exporting a transient point.
        /// </summary>
        public const int ExportTransient = 0x02;

        /// <summary>
        /// Gets the time parameters.
        /// </summary>
        /// <value>
        /// The time parameters.
        /// </value>
        public TimeParameters TimeParameters { get; }

        /// <summary>
        /// Gets the statistics.
        /// </summary>
        /// <value>
        /// The statistics.
        /// </value>
        public new TimeSimulationStatistics Statistics { get; }

        /// <inheritdoc/>
        TimeParameters IParameterized<TimeParameters>.Parameters => TimeParameters;

        /// <inheritdoc/>
        ITimeSimulationState IStateful<ITimeSimulationState>.State => _time;

        /// <inheritdoc/>
        IIntegrationMethod IStateful<IIntegrationMethod>.State => _method;

        /// <summary>
        /// Gets the current time point.
        /// </summary>
        public double Time => _method.Time;

        /// <summary>
        /// Initializes a new instance of the <see cref="Transient"/> class.
        /// </summary>
        /// <param name="name">The name of the simulation.</param>
        public Transient(string name)
            : base(name)
        {
            TimeParameters = new Trapezoidal();
            Statistics = new TimeSimulationStatistics();
            _time = new SimulationState();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Transient"/> class.
        /// </summary>
        /// <param name="name">The name of the simulation.</param>
        /// <param name="parameters">The time parameters.</param>
        public Transient(string name, TimeParameters parameters)
            : base(name)
        {
            TimeParameters = parameters.ThrowIfNull(nameof(parameters));
            Statistics = new TimeSimulationStatistics();
            _time = new SimulationState();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Transient"/> class.
        /// </summary>
        /// <param name="name">The name of the simulation.</param>
        /// <param name="step">The step size.</param>
        /// <param name="final">The final time.</param>
        public Transient(string name, double step, double final)
            : base(name)
        {
            TimeParameters = new Trapezoidal { InitialStep = step, StopTime = final };
            Statistics = new TimeSimulationStatistics();
            _time = new SimulationState();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Transient"/> class.
        /// </summary>
        /// <param name="name">The name of the simulation.</param>
        /// <param name="step">The step size.</param>
        /// <param name="final">The final time.</param>
        /// <param name="maxStep">The maximum step.</param>
        public Transient(string name, double step, double final, double maxStep)
            : base(name)
        {
            TimeParameters = new Trapezoidal { InitialStep = step, StopTime = final, MaxStep = maxStep };
            Statistics = new TimeSimulationStatistics();
            _time = new SimulationState();
        }

        /// <inheritdoc />
        protected override void CreateStates()
        {
            base.CreateStates();
            _method = TimeParameters.Create(GetState<IBiasingSimulationState>());
        }

        /// <inheritdoc/>
        protected override void CreateBehaviors(IEntityCollection entities)
        {
            base.CreateBehaviors(entities);
            _transientBehaviors = EntityBehaviors.GetBehaviorList<ITimeBehavior>();
            _acceptBehaviors = EntityBehaviors.GetBehaviorList<IAcceptBehavior>();
            _truncatingBehaviors = EntityBehaviors.GetBehaviorList<ITruncatingBehavior>();
            _method.Initialize();

            // Set up initial conditions
            var state = GetState<IBiasingSimulationState>();
            _initialConditions.Clear();
            foreach (var ic in TimeParameters.InitialConditions)
            {
                if (state.ContainsKey(ic.Key))
                    _initialConditions.Add(new ConvergenceAid(state.GetSharedVariable(ic.Key), GetState<IBiasingSimulationState>(), ic.Value));
                else
                    SpiceSharpWarning.Warning(this, Properties.Resources.Simulations_ConvergenceAidVariableNotFound.FormatString(ic.Key));
            }
        }

        /// <inheritdoc/>
        protected override void Validate(IEntityCollection entities)
        {
            if (TimeParameters.Validate)
            {
                var state = GetState<IBiasingSimulationState>();
                var rules = new Biasing.Rules(state, BiasingParameters.NodeComparer);

                // We want to add our own connections for initial conditions
                foreach (var ic in _initialConditions)
                {
                    foreach (var rule in rules.GetRules<IConductiveRule>())
                        rule.AddPath(null, ConductionTypes.Dc, ic.Variable, state.Map[0]);
                }
                Validate(rules, entities);
            }
        }

        /// <inheritdoc/>
        protected override IEnumerable<int> Execute(int mask = Exports)
        {
            foreach (int exportType in base.Execute(mask))
                yield return exportType;

            // Calculate the operating point of the circuit
            _time.UseIc = TimeParameters.UseIc;
            _time.UseDc = true;
            if (_time.UseIc)
            {
                // Copy initial conditions in the current solution such that device may be able to copy them
                // in InitializeStates()
                var state = GetState<IBiasingSimulationState>();
                foreach (var ic in _initialConditions)
                {
                    int index = state.Map[ic.Variable];
                    state.Solution[index] = ic.Value;
                    state.OldSolution[index] = ic.Value; // Just to make sure...
                }
            }
            else
            {
                if (_initialConditions.Count > 0)
                {
                    AfterLoad += LoadInitialConditions;
                    Op(BiasingParameters.DcMaxIterations);
                    AfterLoad -= LoadInitialConditions;
                }
                else
                    Op(BiasingParameters.DcMaxIterations);
            }
            Statistics.TimePoints++;

            // Stop calculating the operating point and allow the devices to 
            // retrieve their initial conditions if necessary
            InitializeStates();
            _time.UseIc = false;
            _time.UseDc = false;

            // Export the operating point
            if ((mask & ExportOperatingPoint) != 0)
                yield return ExportOperatingPoint;

            // Start our statistics
            var stats = ((BiasingSimulation)this).Statistics;
            int startIters = stats.Iterations;
            var startselapsed = stats.SolveTime.Elapsed;
            Statistics.TransientTime.Start();
            try
            {
                while (true)
                {
                    // Accept the last evaluated time point
                    Accept();

                    // Export the current timepoint
                    if (_method.Time >= TimeParameters.StartTime && (mask & ExportTransient) != 0)
                        yield return ExportTransient;

                    // Detect the end of the simulation
                    if (_method.Time >= TimeParameters.StopTime)
                    {
                        // Keep our statistics
                        Statistics.TransientTime.Stop();
                        Statistics.TransientIterations += stats.Iterations - startIters;
                        Statistics.TransientSolveTime += stats.SolveTime.Elapsed - startselapsed;

                        // Finished!
                        yield break;
                    }

                    // Continue integration
                    foreach (var behavior in _truncatingBehaviors)
                        _method.Truncate(behavior.Prepare());
                    _method.Prepare();

                    // Find a valid time point
                    while (true)
                    {
                        // Probe the next time point
                        Probe();

                        // Try to solve the new point
                        bool converged = TimeIterate(TimeParameters.TransientMaxIterations);
                        Statistics.TimePoints++;

                        // Did we fail to converge to a solution?
                        if (!converged)
                        {
                            _method.Reject();
                            Statistics.Rejected++;
                        }
                        else
                        {
                            // If our integration method approves of our solution, continue to the next timepoint
                            double max = double.PositiveInfinity;
                            foreach (var behavior in _truncatingBehaviors)
                                max = Math.Min(behavior.Evaluate(), max);
                            if (_method.Evaluate(max))
                                break;
                            Statistics.Rejected++;
                        }
                    }
                }
            }
            finally
            {
                Statistics.TransientTime.Stop();
                Statistics.TransientIterations += stats.Iterations - startIters;
                Statistics.TransientSolveTime += stats.SolveTime.Elapsed - startselapsed;
            }
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
            var state = GetState<IBiasingSimulationState>();
            var solver = state.Solver;
            int iterno = 0;
            bool initTransient = _method.BaseTime.Equals(0.0);

            // Ignore operating condition point, just use the solution as-is
            if (_time.UseIc)
            {
                StoreSolution();

                // Voltages are set using IC statement on the nodes
                // Internal initial conditions are calculated by the components
                Load();
                return true;
            }

            try
            {
                // Perform iteration
                while (true)
                {
                    // Reset convergence flag
                    Iteration.IsConvergent = true;

                    // Load the Y-matrix and right hand side vector
                    Load();
                    iterno++;

                    // Preordering is already done in the operating point calculation
                    if (Iteration.Mode == IterationModes.Junction || initTransient)
                        _shouldReorder = true;

                    if (_shouldReorder)
                    {
                        // Reorder and factor the solver
                        base.Statistics.ReorderTime.Start();
                        try
                        {
                            int eliminated = solver.OrderAndFactor();
                            if (eliminated < solver.Size)
                                throw new SingularException(eliminated + 1);
                            _shouldReorder = false;
                        }
                        finally
                        {
                            base.Statistics.ReorderTime.Stop();
                        }
                    }
                    else
                    {
                        // Factor the solver (without losing time reordering)
                        base.Statistics.FactoringTime.Start();
                        try
                        {
                            if (!solver.Factor())
                            {
                                _shouldReorder = true;
                                continue;
                            }
                        }
                        finally
                        {
                            base.Statistics.FactoringTime.Stop();
                        }
                    }

                    // The current solution becomes the old solution
                    StoreSolution();

                    // Solve the equation
                    base.Statistics.SolveTime.Start();
                    try
                    {
                        solver.ForwardSubstitute(state.Solution);
                        solver.BackwardSubstitute(state.Solution);
                    }
                    finally
                    {
                        base.Statistics.SolveTime.Stop();
                    }

                    // Reset ground nodes
                    state.Solution[0] = 0.0;
                    state.OldSolution[0] = 0.0;

                    // Exceeded maximum number of iterations
                    if (iterno > maxIterations)
                        return false;

                    if (Iteration.IsConvergent && iterno != 1)
                        Iteration.IsConvergent = IsConvergent();
                    else
                        Iteration.IsConvergent = false;

                    if (initTransient)
                    {
                        initTransient = false;
                        if (iterno <= 1)
                            _shouldReorder = true;
                        Iteration.Mode = IterationModes.Float;
                    }
                    else
                    {
                        switch (Iteration.Mode)
                        {
                            case IterationModes.Float:
                                if (Iteration.IsConvergent)
                                    return true;
                                break;

                            case IterationModes.Junction:
                                Iteration.Mode = IterationModes.Fix;
                                _shouldReorder = true;
                                break;

                            case IterationModes.Fix:
                                if (Iteration.IsConvergent)
                                    Iteration.Mode = IterationModes.Float;
                                break;

                            case IterationModes.None:
                                Iteration.Mode = IterationModes.Float;
                                break;

                            default:
                                throw new SpiceSharpException(Properties.Resources.Simulations_InvalidInitializationMode);
                        }
                    }
                }
            }
            finally
            {
                Statistics.TransientIterations += iterno;
            }
        }

        /// <summary>
        /// Initializes all transient behaviors to assume that the current solution is the DC solution.
        /// </summary>
        protected virtual void InitializeStates()
        {
            foreach (var behavior in _transientBehaviors)
                behavior.InitializeStates();
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
        /// Accepts the current simulation state as a valid timepoint.
        /// </summary>
        protected void Accept()
        {
            foreach (var behavior in _acceptBehaviors)
                behavior.Accept();
            _method.Accept();
            Statistics.Accepted++;
        }

        /// <summary>
        /// Probe for a new time point.
        /// </summary>
        protected void Probe()
        {
            _method.Probe();
            foreach (var behavior in _acceptBehaviors)
                behavior.Probe();
        }
    }
}
