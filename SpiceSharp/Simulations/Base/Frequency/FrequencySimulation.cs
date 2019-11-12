using System;
using System.Numerics;
using SpiceSharp.Behaviors;
using SpiceSharp.Entities;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A template for frequency-dependent analysis.
    /// </summary>
    /// <seealso cref="BiasingSimulation" />
    public abstract partial class FrequencySimulation : BiasingSimulation,
        IBehavioral<IFrequencyBehavior>, IBehavioral<IFrequencyUpdateBehavior>
    {
        /// <summary>
        /// Private variables
        /// </summary>
        private BehaviorList<IFrequencyBehavior> _frequencyBehaviors;
        private BehaviorList<IFrequencyUpdateBehavior> _frequencyUpdateBehaviors;
        private LoadStateEventArgs _loadStateEventArgs;
        private bool _shouldReorderAc;

        /// <summary>
        /// Gets the (cached) simulation statistics.
        /// </summary>
        protected ComplexSimulationStatistics FrequencySimulationStatistics { get; }

        /// <summary>
        /// Occurs before loading the matrix and right-hand side vector.
        /// </summary>
        /// <remarks>
        /// For better performance you can also create an entity with a high priority that
        /// generates a frequency behavior.
        /// </remarks>
        public event EventHandler<LoadStateEventArgs> BeforeFrequencyLoad;

        /// <summary>
        /// Occurs after loading the matrix and right-hand side vector.
        /// </summary>
        /// <remarks>
        /// For better performance you can also create an entity with a low priority that
        /// generates a frequency behavior.
        /// </remarks>
        public event EventHandler<LoadStateEventArgs> AfterFrequencyLoad;

        /// <summary>
        /// Gets the complex simulation state.
        /// </summary>
        /// <value>
        /// The complex simulation state.
        /// </value>
        protected ComplexSimulationState ComplexState { get; }

        /// <summary>
        /// Gets the frequency sweep.
        /// </summary>
        protected Sweep<double> FrequencySweep { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FrequencySimulation"/> class.
        /// </summary>
        /// <param name="name">The identifier of the simulation.</param>
        protected FrequencySimulation(string name) 
            : base(name)
        {
            Configurations.Add(new FrequencyConfiguration());
            FrequencySimulationStatistics = new ComplexSimulationStatistics();
            Statistics.Add(FrequencySimulationStatistics);

            // Add behavior types in the order they are (usually) called
            Types.Add(typeof(IFrequencyBehavior));
            Types.Add(typeof(IFrequencyUpdateBehavior));

            ComplexState = new ComplexSimulationState();
            States.Add<IComplexSimulationState>(ComplexState);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FrequencySimulation"/> class.
        /// </summary>
        /// <param name="name">The identifier of the simulation.</param>
        /// <param name="frequencySweep">The frequency sweep.</param>
        protected FrequencySimulation(string name, Sweep<double> frequencySweep) 
            : base(name)
        {
            Configurations.Add(new FrequencyConfiguration(frequencySweep));
            FrequencySimulationStatistics = new ComplexSimulationStatistics();
            Statistics.Add(FrequencySimulationStatistics);

            // Add behavior types in the order they are (usually) called
            Types.Add(typeof(IFrequencyBehavior));
            Types.Add(typeof(IFrequencyUpdateBehavior));

            ComplexState = new ComplexSimulationState();
            States.Add<IComplexSimulationState>(ComplexState);
        }

        /// <summary>
        /// Set up the simulation.
        /// </summary>
        /// <param name="entities">The circuit that will be used.</param>
        protected override void Setup(IEntityCollection entities)
        {
            entities.ThrowIfNull(nameof(entities));

            // Get behaviors, configurations and states
            var config = Configurations.GetValue<FrequencyConfiguration>();
            FrequencySweep = config.FrequencySweep.ThrowIfNull("frequency sweep");

            ModifiedNodalAnalysisHelper<Complex>.Magnitude = ComplexMagnitude;

            // Create the state for complex numbers
            /* var strategy = ComplexState.Solver.Strategy;
            strategy.RelativePivotThreshold = config.RelativePivotThreshold;
            strategy.AbsolutePivotThreshold = config.AbsolutePivotThreshold; */
            
            // Setup the rest of the behaviors
            base.Setup(entities);

            // Cache local variables
            _frequencyBehaviors = EntityBehaviors.GetBehaviorList<IFrequencyBehavior>();
            _frequencyUpdateBehaviors = EntityBehaviors.GetBehaviorList<IFrequencyUpdateBehavior>();
            _loadStateEventArgs = new LoadStateEventArgs(ComplexState);

            ComplexState.Setup(this);
        }

        /// <summary>
        /// Default complex magnitude.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        private double ComplexMagnitude(Complex value) => Math.Abs(value.Real) + Math.Abs(value.Imaginary);

        /// <summary>
        /// Creates all behaviors for the simulation.
        /// </summary>
        /// <param name="entities">The entities.</param>
        protected override void CreateBehaviors(IEntityCollection entities)
        {
            ComplexState.Initialize(this);
            base.CreateBehaviors(entities);
        }

        /// <summary>
        /// Executes the simulation.
        /// </summary>
        protected override void Execute()
        {
            base.Execute();

            // Initialize the state
            _shouldReorderAc = true;
        }

        /// <summary>
        /// Destroys the simulation.
        /// </summary>
        protected override void Unsetup()
        {
            // Remove references
            _frequencyBehaviors = null;
            _frequencyUpdateBehaviors = null;

            // Remove the state
            ComplexState.Unsetup();

            // Configuration
            FrequencySweep = null;

            base.Unsetup();
        }

        /// <summary>
        /// Acs the iterate.
        /// </summary>
        protected void AcIterate()
        {
            var cstate = ComplexState;
            var solver = cstate.Solver;

            retry:
            cstate.IsConvergent = true;

            // Load AC
            FrequencyLoad();

            if (_shouldReorderAc)
            {
                FrequencySimulationStatistics.ComplexReorderTime.Start();
                solver.OrderAndFactor();
                FrequencySimulationStatistics.ComplexReorderTime.Stop();
                _shouldReorderAc = false;
            }
            else
            {
                FrequencySimulationStatistics.ComplexDecompositionTime.Start();
                var factored = solver.Factor();
                FrequencySimulationStatistics.ComplexDecompositionTime.Stop();

                if (!factored)
                {
                    _shouldReorderAc = true;
                    goto retry;
                }
            }

            // Solve
            FrequencySimulationStatistics.ComplexSolveTime.Start();
            solver.Solve(cstate.Solution);
            FrequencySimulationStatistics.ComplexSolveTime.Stop();

            // Update with the found solution
            foreach (var behavior in _frequencyUpdateBehaviors)
                behavior.Update();

            // Reset values
            cstate.Solution[0] = 0.0;
            FrequencySimulationStatistics.ComplexPoints++;
        }

        /// <summary>
        /// Raises the <see cref="BeforeFrequencyLoad" /> event.
        /// </summary>
        /// <param name="args">The <see cref="LoadStateEventArgs"/> instance containing the event data.</param>
        protected virtual void OnBeforeFrequencyLoad(LoadStateEventArgs args) => BeforeFrequencyLoad?.Invoke(this, args);

        /// <summary>
        /// Raises the <see cref="AfterFrequencyLoad" /> event.
        /// </summary>
        /// <param name="args">The <see cref="LoadStateEventArgs"/> instance containing the event data.</param>
        protected virtual void OnAfterFrequencyLoad(LoadStateEventArgs args) => AfterFrequencyLoad?.Invoke(this, args);

        /// <summary>
        /// Initializes the ac parameters.
        /// </summary>
        protected void InitializeAcParameters()
        {
            BiasingState.UseDc = false;
            Load();
            foreach (var behavior in _frequencyBehaviors)
                behavior.InitializeParameters();
        }

        /// <summary>
        /// Loads the Y-matrix and right-hand side vector.
        /// </summary>
        protected void FrequencyLoad()
        {
            OnBeforeFrequencyLoad(_loadStateEventArgs);
            FrequencySimulationStatistics.ComplexLoadTime.Start();
            ComplexState.Solver.Reset();
            LoadFrequencyBehaviors();
            FrequencySimulationStatistics.ComplexLoadTime.Reset();
            OnAfterFrequencyLoad(_loadStateEventArgs);
        }

        /// <summary>
        /// Loads the Y-matrix and right-hand side vector.
        /// </summary>
        protected virtual void LoadFrequencyBehaviors()
        {
            foreach (var behavior in _frequencyBehaviors)
                behavior.Load();
        }
    }
}
