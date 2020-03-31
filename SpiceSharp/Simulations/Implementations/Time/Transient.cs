﻿using System.Collections.Generic;
using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.Simulations.IntegrationMethods;
using SpiceSharp.Simulations.Time;
using SpiceSharp.Validation;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A base class for time-domain analysis.
    /// </summary>
    /// <seealso cref="BiasingSimulation" />
    public partial class Transient : BiasingSimulation,
        ITimeSimulation,
        IBehavioral<IAcceptBehavior>,
        IParameterized<TimeParameters>
    {
        /// <summary>
        /// Time-domain behaviors.
        /// </summary>
        private BehaviorList<ITimeBehavior> _transientBehaviors;
        private BehaviorList<IAcceptBehavior> _acceptBehaviors;
        private readonly List<ConvergenceAid> _initialConditions = new List<ConvergenceAid>();
        private bool _shouldReorder = true;
        private IIntegrationMethod _method;
        private SimulationState _time;

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

        TimeParameters IParameterized<TimeParameters>.Parameters => TimeParameters;
        ITimeSimulationState IStateful<ITimeSimulationState>.State => _time;
        IIntegrationMethod IStateful<IIntegrationMethod>.State => _method;

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

        /// <summary>
        /// Set up the simulation.
        /// </summary>
        /// <param name="entities">The circuit that will be used.</param>
        protected override void Setup(IEntityCollection entities)
        {
            entities.ThrowIfNull(nameof(entities));

            // Get behaviors and configurations
            _method = TimeParameters.Create(this);

            // Setup
            base.Setup(entities);

            // Cache local variables
            _transientBehaviors = EntityBehaviors.GetBehaviorList<ITimeBehavior>();
            _acceptBehaviors = EntityBehaviors.GetBehaviorList<IAcceptBehavior>();

            // Set up initial conditions
            var state = GetState<IBiasingSimulationState>();
            foreach (var ic in TimeParameters.InitialConditions)
            {
                if (state.Variables.ContainsKey(ic.Key))
                    _initialConditions.Add(new ConvergenceAid(state.GetSharedVariable(ic.Key), GetState<IBiasingSimulationState>(), ic.Value));
                else
                    SpiceSharpWarning.Warning(this, Properties.Resources.Simulations_ConvergenceAidVariableNotFound.FormatString(ic.Key));
            }

            // Initialize the integration method (all components have been able to allocate integration states).
            _method.Initialize();
        }

        /// <summary>
        /// Validates the circuit.
        /// </summary>
        /// <param name="entities">The entities to be validated.</param>
        protected override void Validate(IEntityCollection entities)
        {
            if (TimeParameters.Validate)
            {
                var state = GetState<IBiasingSimulationState>();
                var rules = new Biasing.Rules(state);

                // We want to add our own connections for initial conditions
                foreach (var ic in _initialConditions)
                {
                    foreach (var rule in rules.GetRules<IConductiveRule>())
                        rule.AddPath(null, ConductionTypes.Dc, ic.Variable, state.Map[0]);
                }
                Validate(rules, entities);
            }
        }

        /// <summary>
        /// Executes the simulation.
        /// </summary>
        protected override void Execute()
        {

            base.Execute();

            // Apply initial conditions if they are not set for the devices (UseIc).
            if (_initialConditions.Count > 0 && !_time.UseIc)
                AfterLoad += LoadInitialConditions;

            // Calculate the operating point of the circuit
            _time.UseIc = TimeParameters.UseIc;
            _time.UseDc = true;
            Op(BiasingParameters.DcMaxIterations);
            Statistics.TimePoints++;

            // Stop calculating the operating point
            _time.UseIc = false;
            _time.UseDc = false;
            InitializeStates();
            AfterLoad -= LoadInitialConditions;

            var exportargs = new ExportDataEventArgs(this);
            
            // Start our statistics
            Statistics.TransientTime.Start();
            var stats = ((BiasingSimulation)this).Statistics;
            var startIters = stats.Iterations;
            var startselapsed = stats.SolveTime.Elapsed;

            try
            {
                while (true)
                {
                    // Accept the last evaluated time point
                    Accept();

                    // Export the current timepoint
                    if (_method.Time >= TimeParameters.StartTime)
                        OnExport(exportargs);

                    // Detect the end of the simulation
                    if (_method.Time >= TimeParameters.StopTime)
                    {
                        // Keep our statistics
                        Statistics.TransientTime.Stop();
                        Statistics.TransientIterations += stats.Iterations - startIters;
                        Statistics.TransientSolveTime += stats.SolveTime.Elapsed - startselapsed;

                        // Finished!
                        return;
                    }

                    // Continue integration
                    _method.Prepare();

                    // Find a valid time point
                    while (true)
                    {
                        // Probe the next time point
                        Probe();

                        // Try to solve the new point
                        var converged = TimeIterate(TimeParameters.TransientMaxIterations);
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
                            if (_method.Evaluate())
                                break;
                            Statistics.Rejected++;
                        }
                    }
                }
            }
            catch (SpiceSharpException ex)
            {
                // Keep our statistics
                Statistics.TransientTime.Stop();
                Statistics.TransientIterations += stats.Iterations - startIters;
                Statistics.TransientSolveTime += stats.SolveTime.Elapsed - startselapsed;
                throw new SpiceSharpException(Properties.Resources.Simulations_Time_Terminated.FormatString(Name), ex);
            }
        }

        /// <summary>
        /// Destroys the simulation.
        /// </summary>
        protected override void Unsetup()
        {
            // Remove references
            _transientBehaviors = null;
            _acceptBehaviors = null;

            // Destroy the initial conditions
            AfterLoad -= LoadInitialConditions;
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
            var state = GetState<IBiasingSimulationState>();
            var solver = state.Solver;
            var iterno = 0;
            var initTransient = _method.BaseTime.Equals(0.0);

            // Ignore operating condition point, just use the solution as-is
            if (_time.UseIc)
            {
                StoreSolution();

                // Voltages are set using IC statement on the nodes
                // Internal initial conditions are calculated by the components
                Load();
                return true;
            }

            // Perform iteration
            while (true)
            {
                // Reset convergence flag
                Iteration.IsConvergent = true;

                try
                {
                    // Load the Y-matrix and Rhs-vector for DC and transients
                    Load();
                    iterno++;
                }
                catch (SpiceSharpException)
                {
                    iterno++;
                    base.Statistics.Iterations = iterno;
                    throw;
                }

                // Preordering is already done in the operating point calculation
                if (Iteration.Mode == IterationModes.Junction || initTransient)
                    _shouldReorder = true;

                // Reorder
                if (_shouldReorder)
                {
                    base.Statistics.ReorderTime.Start();
                    if (solver.OrderAndFactor() < solver.Size)
                        throw new SingularException();
                    base.Statistics.ReorderTime.Stop();
                    _shouldReorder = false;
                }
                else
                {
                    // Decompose
                    base.Statistics.DecompositionTime.Start();
                    var success = solver.Factor();
                    base.Statistics.DecompositionTime.Stop();

                    if (!success)
                    {
                        _shouldReorder = true;
                        continue;
                    }
                }

                // The current solution becomes the old solution
                StoreSolution();

                // Solve the equation
                base.Statistics.SolveTime.Start();
                solver.Solve(state.Solution);
                base.Statistics.SolveTime.Stop();

                // Reset ground nodes
                state.Solution[0] = 0.0;
                state.OldSolution[0] = 0.0;

                // Exceeded maximum number of iterations
                if (iterno > maxIterations)
                {
                    base.Statistics.Iterations += iterno;
                    return false;
                }

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
                            {
                                base.Statistics.Iterations += iterno;
                                return true;
                            }

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
                            base.Statistics.Iterations += iterno;
                            throw new SpiceSharpException(Properties.Resources.Simulations_InvalidInitializationMode);
                    }
                }
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
