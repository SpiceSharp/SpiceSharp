using System;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A template for frequency-dependent analysis.
    /// </summary>
    /// <seealso cref="SpiceSharp.Simulations.BaseSimulation" />
    public abstract class FrequencySimulation : BaseSimulation
    {
        /// <summary>
        /// Gets the currently active frequency configuration.
        /// </summary>
        /// <value>
        /// The frequency configuration.
        /// </value>
        public FrequencyConfiguration FrequencyConfiguration { get; protected set; }

        /// <summary>
        /// Private variables
        /// </summary>
        private BehaviorList<BaseFrequencyBehavior> _frequencyBehaviors;
        private LoadStateEventArgs _loadStateEventArgs;
        private bool _shouldReorderAc;

        /// <summary>
        /// Occurs before loading the matrix and right-hand side vector.
        /// </summary>
        public event EventHandler<LoadStateEventArgs> BeforeFrequencyLoad;

        /// <summary>
        /// Occurs after loading the matrix and right-hand side vector.
        /// </summary>
        public event EventHandler<LoadStateEventArgs> AfterFrequencyLoad;

        /// <summary>
        /// Gets the complex simulation state.
        /// </summary>
        /// <value>
        /// The state of the complex.
        /// </value>
        public ComplexSimulationState ComplexState { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FrequencySimulation"/> class.
        /// </summary>
        /// <param name="name">The identifier of the simulation.</param>
        protected FrequencySimulation(Identifier name) : base(name)
        {
            Configurations.Add(new FrequencyConfiguration());
        }

        /// <summary>
        /// Gets the frequency sweep.
        /// </summary>
        /// <value>
        /// The frequency sweep.
        /// </value>
        protected Sweep<double> FrequencySweep { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FrequencySimulation"/> class.
        /// </summary>
        /// <param name="name">The identifier of the simulation.</param>
        /// <param name="frequencySweep">The frequency sweep.</param>
        protected FrequencySimulation(Identifier name, Sweep<double> frequencySweep) : base(name)
        {
            Configurations.Add(new FrequencyConfiguration(frequencySweep));
        }

        /// <summary>
        /// Set up the simulation.
        /// </summary>
        /// <param name="circuit">The circuit that will be used.</param>
        /// <exception cref="ArgumentNullException">circuit</exception>
        /// <exception cref="SpiceSharp.CircuitException">
        /// No frequency configuration found
        /// or
        /// No frequency sweep found
        /// </exception>
        protected override void Setup(Circuit circuit)
        {
            if (circuit == null)
                throw new ArgumentNullException(nameof(circuit));
            base.Setup(circuit);

            // Get behaviors, configurations and states
            FrequencyConfiguration = Configurations.Get<FrequencyConfiguration>() ??
                                     throw new CircuitException("No frequency configuration found");
            FrequencySweep = FrequencyConfiguration.FrequencySweep ??
                             throw new CircuitException("No frequency sweep found");

            // Create the state for complex numbers
            ComplexState = new ComplexSimulationState();
            _loadStateEventArgs = new LoadStateEventArgs(ComplexState);
            var strategy = ComplexState.Solver.Strategy;
            strategy.RelativePivotThreshold = FrequencyConfiguration.RelativePivotThreshold;
            strategy.AbsolutePivotThreshold = FrequencyConfiguration.AbsolutePivotThreshold;

            // Setup behaviors
            _frequencyBehaviors = SetupBehaviors<BaseFrequencyBehavior>(circuit.Entities);
            var solver = ComplexState.Solver;
            for (var i = 0; i < _frequencyBehaviors.Count; i++)
                _frequencyBehaviors[i].GetEquationPointers(solver);
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
                _frequencyBehaviors[i].Unsetup(this);
            _frequencyBehaviors = null;

            // Remove the state
            ComplexState.Unsetup();
            ComplexState = null;

            // Configuration
            FrequencyConfiguration = null;
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
                solver.OrderAndFactor();
                _shouldReorderAc = false;
            }
            else
            {
                if (!solver.Factor())
                {
                    _shouldReorderAc = true;
                    goto retry;
                }
            }

            // Solve
            solver.Solve(cstate.Solution);

            // Reset values
            cstate.Solution[0] = 0.0;
        }

        /// <summary>
        /// Raises the <see cref="E:BeforeFrequencyLoad" /> event.
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
                _frequencyBehaviors[i].Load(this);
                _frequencyBehaviors[i].InitializeParameters(this);
            }
        }

        /// <summary>
        /// Loads the Y-matrix and right-hand side vector.
        /// </summary>
        protected void FrequencyLoad()
        {
            var cstate = ComplexState;

            OnBeforeFrequencyLoad(_loadStateEventArgs);

            cstate.Solver.Clear();
            LoadFrequencyBehaviors();

            OnAfterFrequencyLoad(_loadStateEventArgs);
        }

        /// <summary>
        /// Loads the Y-matrix and right-hand side vector.
        /// </summary>
        protected virtual void LoadFrequencyBehaviors()
        {
            for (var i = 0; i < _frequencyBehaviors.Count; i++)
                _frequencyBehaviors[i].Load(this);
        }
    }
}
