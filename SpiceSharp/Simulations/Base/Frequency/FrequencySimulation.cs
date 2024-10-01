using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.ParameterSets;
using SpiceSharp.Simulations.Frequency;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A template for frequency-dependent analysis.
    /// </summary>
    /// <seealso cref="BiasingSimulation" />
    /// <seealso cref="IFrequencySimulation"/>
    /// <seealso cref="IBehavioral{B}"/>
    /// <seealso cref="IFrequencyUpdateBehavior"/>
    /// <seealso cref="IParameterized{P}"/>
    /// <seealso cref="Simulations.FrequencyParameters"/>
    public abstract partial class FrequencySimulation : BiasingSimulation,
        IFrequencySimulation,
        IBehavioral<IFrequencyUpdateBehavior>,
        IParameterized<FrequencyParameters>
    {
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

        /// <inheritdoc/>
        IComplexSimulationState IStateful<IComplexSimulationState>.State => _state;

        /// <inheritdoc/>
        FrequencyParameters IParameterized<FrequencyParameters>.Parameters => FrequencyParameters;

        /// <inheritdoc/>
        IVariableDictionary<IVariable<Complex>> ISimulation<IVariable<Complex>>.Solved => _state;

        /// <summary>
        /// Initializes a new instance of the <see cref="FrequencySimulation"/> class.
        /// </summary>
        /// <param name="name">The name of the simulation.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
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
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        protected FrequencySimulation(string name, IEnumerable<double> frequencySweep)
            : this(name)
        {
            FrequencyParameters.Frequencies = frequencySweep;
        }

        /// <inheritdoc />
        protected override void CreateStates()
        {
            base.CreateStates();
            _state = new ComplexSimulationState(FrequencyParameters.CreateSolver(), BiasingParameters.NodeComparer);
            _loadStateEventArgs = new LoadStateEventArgs(_state);
        }

        /// <inheritdoc />
        protected override void CreateBehaviors(IEntityCollection entities)
        {
            base.CreateBehaviors(entities);
            _frequencyBehaviors = EntityBehaviors.GetBehaviorList<IFrequencyBehavior>();
            _frequencyUpdateBehaviors = EntityBehaviors.GetBehaviorList<IFrequencyUpdateBehavior>();
            _state.Setup();
        }

        /// <inheritdoc/>
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
        /// <returns>The magnitude of the complex number.</returns>
        private double ComplexMagnitude(Complex value) => Math.Abs(value.Real) + Math.Abs(value.Imaginary);

        /// <inheritdoc/>
        protected override IEnumerable<int> Execute(int mask)
        {
            foreach (int exportType in base.Execute(mask))
                yield return exportType;

            // Initialize the state
            _shouldReorderAc = true;
        }

        /// <summary>
        /// Iterate small-signal matrix and vector.
        /// </summary>
        /// <exception cref="SpiceSharpException">Thrown if a behavior cannot load the complex matrix and/or right hand side vector.</exception>
        /// <exception cref="SingularException">Thrown if the equation matrix is singular.</exception>
        protected void AcIterate()
        {
            var solver = _state.Solver;

        retry:

            // Load AC
            FrequencyLoad();

            if (_shouldReorderAc)
            {
                Statistics.ComplexReorderTime.Start();
                try
                {
                    int eliminated = solver.OrderAndFactor();
                    if (eliminated < solver.Size)
                        throw new SingularException(eliminated + 1);
                    _shouldReorderAc = false;
                }
                finally
                {
                    Statistics.ComplexReorderTime.Stop();
                }
            }
            else
            {
                Statistics.ComplexDecompositionTime.Start();
                try
                {
                    bool factored = solver.Factor();
                    if (!factored)
                    {
                        _shouldReorderAc = true;
                        goto retry;
                    }
                }
                finally
                {
                    Statistics.ComplexDecompositionTime.Stop();
                }
            }

            // Solve
            Statistics.ComplexSolveTime.Start();
            try
            {
                solver.ForwardSubstitute(_state.Solution);
                solver.BackwardSubstitute(_state.Solution);
            }
            finally
            {
                Statistics.ComplexSolveTime.Stop();
            }

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
        /// Initializes the small-signal parameters.
        /// </summary>
        protected void InitializeAcParameters()
        {
            // Some behaviors want to have the most up to date information from their biasing behavior
            base.Statistics.LoadTime.Start();
            try
            {
                Load();
            }
            finally
            {
                base.Statistics.LoadTime.Stop();
            }

            // Initialize the parameters
            foreach (var behavior in _frequencyBehaviors)
                behavior.InitializeParameters();
        }

        /// <summary>
        /// Loads the Y-matrix and right hand side vector.
        /// </summary>
        /// <exception cref="SpiceSharpException">Thrown if a behavior cannot load the complex matrix and/or right hand side vector.</exception>
        protected void FrequencyLoad()
        {
            OnBeforeFrequencyLoad(_loadStateEventArgs);

            Statistics.ComplexLoadTime.Start();
            try
            {
                _state.Solver.Reset();
                LoadFrequencyBehaviors();
            }
            finally
            {
                Statistics.ComplexLoadTime.Reset();
            }

            OnAfterFrequencyLoad(_loadStateEventArgs);
        }

        /// <summary>
        /// Loads the Y-matrix and right hand side vector.
        /// </summary>
        /// <exception cref="SpiceSharpException">Thrown if a behavior cannot load the complex matrix and/or right hand side vector.</exception>
        protected virtual void LoadFrequencyBehaviors()
        {
            foreach (var behavior in _frequencyBehaviors)
                behavior.Load();
        }
    }
}
