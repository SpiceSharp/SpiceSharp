using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.ParameterSets;
using SpiceSharp.Simulations.Time;
using System.Collections.Generic;

namespace SpiceSharp.Simulations;

/// <summary>
/// A class that implements a transient noise analysis: a regular time-domain analysis in which every
/// device also injects a stochastic current derived from its own noise density at its running
/// operating point.
/// </summary>
/// <seealso cref="Transient" />
public partial class NoiseTransient : Transient,
    IBehavioral<ITimeNoiseBehavior>,
    IStateful<ITimeNoiseSimulationState>,
    IParameterized<NoiseTransientParameters>
{
    private TimeNoiseSimulationState _state;
    private IIntegrationMethod _method;
    private BehaviorList<ITimeNoiseBehavior> _noiseBehaviors;

    /// <summary>
    /// Gets the transient noise parameters.
    /// </summary>
    /// <value>
    /// The transient noise parameters.
    /// </value>
    public NoiseTransientParameters NoiseParameters { get; } = new NoiseTransientParameters();

    /// <inheritdoc/>
    NoiseTransientParameters IParameterized<NoiseTransientParameters>.Parameters => NoiseParameters;

    /// <inheritdoc/>
    ITimeNoiseSimulationState IStateful<ITimeNoiseSimulationState>.State => _state;

    /// <summary>
    /// Initializes a new instance of the <see cref="NoiseTransient"/> class.
    /// </summary>
    /// <param name="name">The name of the simulation.</param>
    /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
    public NoiseTransient(string name)
        : base(name)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NoiseTransient"/> class.
    /// </summary>
    /// <param name="name">The name of the simulation.</param>
    /// <param name="parameters">The time parameters.</param>
    /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="name"/> or
    /// <paramref name="parameters"/> is <c>null</c>.</exception>
    public NoiseTransient(string name, TimeParameters parameters)
        : base(name, parameters)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NoiseTransient"/> class.
    /// </summary>
    /// <param name="name">The name of the simulation.</param>
    /// <param name="step">The step size.</param>
    /// <param name="final">The final time.</param>
    /// <param name="maximumNoiseFrequency">The maximum noise frequency.</param>
    /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
    /// <exception cref="System.ArgumentOutOfRangeException">Thrown if <paramref name="maximumNoiseFrequency"/>
    /// is not positive.</exception>
    public NoiseTransient(string name, double step, double final, double maximumNoiseFrequency)
        : base(name, step, final)
    {
        NoiseParameters.MaximumNoiseFrequency = maximumNoiseFrequency;
    }

    /// <inheritdoc />
    /// <exception cref="SpiceSharpException">Thrown if no maximum noise frequency was specified.</exception>
    protected override void CreateStates()
    {
        base.CreateStates();

        // Without a band limit every source would be silent rather than white, so refuse to run
        // instead of quietly producing a noiseless transient analysis.
        if (!(NoiseParameters.MaximumNoiseFrequency > 0.0))
            throw new SpiceSharpException(Properties.Resources.Simulations_NoiseTransient_NoMaximumFrequency.FormatString(Name));

        _method = GetState<IIntegrationMethod>();

        // The run length is what sets the lowest frequency of the flicker noise ladder: a run cannot
        // show any power below its own reciprocal.
        _state = new TimeNoiseSimulationState(Name, _method, NoiseParameters, TimeParameters.StopTime);
    }

    /// <inheritdoc />
    protected override void CreateBehaviors(IEntityCollection entities)
    {
        base.CreateBehaviors(entities);
        _noiseBehaviors = EntityBehaviors.GetBehaviorList<ITimeNoiseBehavior>();
    }

    /// <inheritdoc/>
    protected override IEnumerable<int> Execute(int mask = Exports)
    {
        AfterLoad += InjectNoise;
        try
        {
            foreach (int exportType in base.Execute(mask))
                yield return exportType;
        }
        finally
        {
            AfterLoad -= InjectNoise;
        }
    }

    /// <inheritdoc/>
    protected override void InitializeStates()
    {
        // The devices compute their noise densities from the operating point first, so that the
        // stationary realization that is drawn here already has the right amplitude.
        base.InitializeStates();
        _state.Initialize();
    }

    /// <inheritdoc/>
    protected override void Probe()
    {
        base.Probe();

        // Set noise shaping coefficients, then pass this to all noise sources
        _state.SetCurrentPoint(_method.GetPreviousTimestep(0));
        foreach (var behavior in _noiseBehaviors)
            behavior.Probe();
    }

    /// <summary>
    /// Stamps the frozen noise realization of every device into the right-hand side vector.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The arguments.</param>
    private void InjectNoise(object sender, LoadStateEventArgs e)
    {
        foreach (var behavior in _noiseBehaviors)
            behavior.Inject();
    }
}
