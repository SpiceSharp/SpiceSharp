using System;
using System.Collections.Generic;
using System.Numerics;
using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.Simulations.Frequency;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A template for frequency-dependent analysis.
    /// </summary>
    /// <seealso cref="BiasingSimulation" />
    public abstract partial class FrequencySimulation : BiasingSimulation,
        IFrequencySimulation,
        IBehavioral<IFrequencyUpdateBehavior>,
        IParameterized<FrequencyParameters>
    {
        /// <summary>
        /// Private variables
        /// </summary>
        private BehaviorList<IFrequencyBehavior> _frequencyBehaviors;
        private BehaviorList<IFrequencyUpdateBehavior> _frequencyUpdateBehaviors;
        private LoadStateEventArgs _loadStateEventArgs;
        private bool _shouldReorderAc;
        private ComplexSimulationState _state;

        /// <summary>
        /// Gets the frequency parameters.
        /// </summary>
        /// <value>
        /// The frequency parameters.
        /// </value>
        public FrequencyParameters FrequencyParameters { get; } = new FrequencyParameters();

        /// <summary>
        /// Gets the statistics.
        /// </summary>
        /// <value>
        /// The statistics.
        /// </value>
        public new ComplexSimulationStatistics Statistics { get; }

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

        IComplexSimulationState IStateful<IComplexSimulationState>.State => _state;
        FrequencyParameters IParameterized<FrequencyParameters>.Parameters => FrequencyParameters;
        IVariableDictionary<IVariable<Complex>> ISimulation<IVariable<Complex>>.Solved => _state;

        /// <summary>
        /// Initializes a new instance of the <see cref="FrequencySimulation"/> class.
        /// </summary>
        /// <param name="name">The name of the simulation.</param>
        protected FrequencySimulation(string name) 
            : base(name)
        {
            ModifiedNodalAnalysisHelper<Complex>.Magnitude = ComplexMagnitude;
            Statistics = new ComplexSimulationStatistics();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FrequencySimulation"/> class.
        /// </summary>
        /// <param name="name">The name of the simulation.</param>
        /// <param name="frequencySweep">The frequency points.</param>
        protected FrequencySimulation(string name, IEnumerable<double> frequencySweep) 
            : this(name)
        {
            FrequencyParameters.Frequencies = frequencySweep;
        }

        /// <summary>
        /// Set up the simulation.
        /// </summary>
        /// <param name="entities">The circuit that will be used.</param>
        protected override void Setup(IEntityCollection entities)
        {
            entities.ThrowIfNull(nameof(entities));

            // Setup the rest of the behaviors
            base.Setup(entities);

            // Cache local variables
            _frequencyBehaviors = EntityBehaviors.GetBehaviorList<IFrequencyBehavior>();
            _frequencyUpdateBehaviors = EntityBehaviors.GetBehaviorList<IFrequencyUpdateBehavior>();
            _loadStateEventArgs = new LoadStateEventArgs(_state);

            _state.Setup();
        }

        /// <summary>
        /// Validates the circuit.
        /// </summary>
        /// <param name="entities">The entities to be validated.</param>
        /// <exception cref="SimulationValidationFailed">Thrown if the simulation failed its validation.</exception>
        protected override void Validate(IEntityCollection entities)
        {
            if (FrequencyParameters.Validate)
                Validate(new Rules(GetState<IBiasingSimulationState>(), FrequencyParameters.Frequencies), entities);
            else
                base.Validate(entities);
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
            _state = new ComplexSimulationState(
                FrequencyParameters.Solver ?? LUHelper.CreateSparseComplexSolver(),
                BiasingParameters.NodeComparer
                );
            /* var strategy = ComplexState.Solver.Strategy;
            strategy.RelativePivotThreshold = config.RelativePivotThreshold;
            strategy.AbsolutePivotThreshold = config.AbsolutePivotThreshold; */

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

            base.Unsetup();
        }

        /// <summary>
        /// Acs the iterate.
        /// </summary>
        protected void AcIterate()
        {
            var solver = _state.Solver;

            retry:

            // Load AC
            FrequencyLoad();

            if (_shouldReorderAc)
            {
                Statistics.ComplexReorderTime.Start();
                if (solver.OrderAndFactor() < solver.Size)
                    throw new SingularException();
                Statistics.ComplexReorderTime.Stop();
                _shouldReorderAc = false;
            }
            else
            {
                Statistics.ComplexDecompositionTime.Start();
                var factored = solver.Factor();
                Statistics.ComplexDecompositionTime.Stop();

                if (!factored)
                {
                    _shouldReorderAc = true;
                    goto retry;
                }
            }

            // Solve
            Statistics.ComplexSolveTime.Start();
            solver.Solve(_state.Solution);
            Statistics.ComplexSolveTime.Stop();

            // Update with the found solution
            foreach (var behavior in _frequencyUpdateBehaviors)
                behavior.Update();

            // Reset values
            _state.Solution[0] = 0.0;
            Statistics.ComplexPoints++;
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
            // Support legacy models
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
            Statistics.ComplexLoadTime.Start();
            _state.Solver.Reset();
            LoadFrequencyBehaviors();
            Statistics.ComplexLoadTime.Reset();
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
