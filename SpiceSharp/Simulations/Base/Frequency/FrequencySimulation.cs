using System;
using SpiceSharp.Behaviors;
using SpiceSharp.Circuits;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A template for frequency-dependent analysis.
    /// </summary>
    /// <seealso cref="SpiceSharp.Simulations.BaseSimulation" />
    public abstract class FrequencySimulation : BaseSimulation
    {
        /// <summary>
        /// Private variables
        /// </summary>
        private BehaviorList<IFrequencyBehavior> _frequencyBehaviors;
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
        public ComplexSimulationState ComplexState { get; protected set; }

        /// <summary>
        /// Gets the frequency sweep.
        /// </summary>
        protected Sweep<double> FrequencySweep { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FrequencySimulation"/> class.
        /// </summary>
        /// <param name="name">The identifier of the simulation.</param>
        protected FrequencySimulation(string name) : base(name)
        {
            Configurations.Add(new FrequencyConfiguration());
            FrequencySimulationStatistics = new ComplexSimulationStatistics();
            Statistics.Add(typeof(ComplexSimulationStatistics), FrequencySimulationStatistics);

            // Add behavior types in the order they are (usually) called
            BehaviorTypes.Add(typeof(IFrequencyBehavior));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FrequencySimulation"/> class.
        /// </summary>
        /// <param name="name">The identifier of the simulation.</param>
        /// <param name="frequencySweep">The frequency sweep.</param>
        protected FrequencySimulation(string name, Sweep<double> frequencySweep) : base(name)
        {
            Configurations.Add(new FrequencyConfiguration(frequencySweep));
            FrequencySimulationStatistics = new ComplexSimulationStatistics();
            Statistics.Add(typeof(ComplexSimulationStatistics), FrequencySimulationStatistics);

            // Add behavior types in the order they are (usually) called
            BehaviorTypes.Add(typeof(IFrequencyBehavior));
        }

        /// <summary>
        /// Set up the simulation.
        /// </summary>
        /// <param name="entities">The circuit that will be used.</param>
        protected override void Setup(EntityCollection entities)
        {
            entities.ThrowIfNull(nameof(entities));

            // Get behaviors, configurations and states
            var config = Configurations.Get<FrequencyConfiguration>();
            FrequencySweep = config.FrequencySweep.ThrowIfNull("frequency sweep");

            // Create the state for complex numbers
            ComplexState = new ComplexSimulationState();
            var strategy = ComplexState.Solver.Strategy;
            strategy.RelativePivotThreshold = config.RelativePivotThreshold;
            strategy.AbsolutePivotThreshold = config.AbsolutePivotThreshold;

            // Setup the rest of the behaviors
            base.Setup(entities);

            // Cache local variables
            _frequencyBehaviors = EntityBehaviors.GetBehaviorList<IFrequencyBehavior>();
            _loadStateEventArgs = new LoadStateEventArgs(ComplexState);

            ComplexState.Setup(Variables);
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
            for (var i = 0; i < _frequencyBehaviors.Count; i++)
                _frequencyBehaviors[i].Unbind();
            _frequencyBehaviors = null;

            // Remove the state
            ComplexState.Unsetup();
            ComplexState = null;

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
        /// Raises the <see cref="E:AfterFrequencyLoad" /> event.
        /// </summary>
        /// <param name="args">The <see cref="LoadStateEventArgs"/> instance containing the event data.</param>
        protected virtual void OnAfterFrequencyLoad(LoadStateEventArgs args) => AfterFrequencyLoad?.Invoke(this, args);

        /// <summary>
        /// Initializes the ac parameters.
        /// </summary>
        protected void InitializeAcParameters()
        {
            RealState.UseDc = false;
            Load();
            for (var i = 0; i < _frequencyBehaviors.Count; i++)
            {
                // _frequencyBehaviors[i].Load(this);
                _frequencyBehaviors[i].InitializeParameters();
            }
        }

        /// <summary>
        /// Loads the Y-matrix and right-hand side vector.
        /// </summary>
        protected void FrequencyLoad()
        {
            var cstate = ComplexState;

            OnBeforeFrequencyLoad(_loadStateEventArgs);
            FrequencySimulationStatistics.ComplexLoadTime.Start();
            cstate.Solver.Clear();
            LoadFrequencyBehaviors();
            FrequencySimulationStatistics.ComplexLoadTime.Reset();
            OnAfterFrequencyLoad(_loadStateEventArgs);
        }

        /// <summary>
        /// Loads the Y-matrix and right-hand side vector.
        /// </summary>
        protected virtual void LoadFrequencyBehaviors()
        {
            for (var i = 0; i < _frequencyBehaviors.Count; i++)
                _frequencyBehaviors[i].Load();
        }
    }
}
