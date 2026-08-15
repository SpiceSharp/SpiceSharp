using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.NoiseSources;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Components.Resistors;

/// <summary>
/// Transient noise behavior for a <see cref="Resistor"/>.
/// </summary>
/// <seealso cref="Biasing"/>
/// <seealso cref="ITimeNoiseBehavior"/>
[BehaviorFor(typeof(Resistor)), AddBehaviorIfNo(typeof(ITimeNoiseBehavior))]
[GeneratedParameters]
public partial class TimeNoise : Biasing,
    ITimeNoiseBehavior,
    ITimeBehavior
{
    private readonly TimeNoiseThermal _thermal;

    /// <inheritdoc/>
    public double NoiseDensity => _thermal.NoiseDensity;

    /// <inheritdoc/>
    /// <remarks>
    /// Implemented explicitly, because <see cref="Biasing.Current"/> is the deterministic current
    /// through the resistor and this one is the noise current that is injected next to it.
    /// </remarks>
    double ITimeNoiseSource.Current => _thermal.Current;

    /// <summary>
    /// Gets the thermal noise source of the resistor.
    /// </summary>
    /// <value>
    /// The thermal noise source.
    /// </value>
    [ParameterName("thermal"), ParameterInfo("The thermal noise source")]
    public ITimeNoiseSource Thermal => _thermal;

    /// <summary>
    /// Initializes a new instance of the <see cref="TimeNoise"/> class.
    /// </summary>
    /// <param name="context">The binding context.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
    public TimeNoise(IComponentBindingContext context) : base(context)
    {
        var biasing = context.GetState<IBiasingSimulationState>();
        var noise = context.GetState<ITimeNoiseSimulationState>();

        // The name is unique within the circuit, unlike the local name that the frequency-domain
        // noise sources use, because the random stream of the source is seeded from it.
        _thermal = new TimeNoiseThermal(Name.Combine("r"), noise, biasing,
            biasing.GetSharedVariable(context.Nodes[0]),
            biasing.GetSharedVariable(context.Nodes[1]));
    }

    /// <inheritdoc/>
    void ITimeBehavior.InitializeStates()
    {
        // The thermal noise of a resistor does not depend on the operating point, so the density is
        // computed once for the whole run instead of once per timepoint.
        _thermal.Compute(Conductance, Parameters.Temperature);
    }

    /// <inheritdoc/>
    void ITimeNoiseBehavior.Probe() => _thermal.Probe();

    /// <inheritdoc/>
    void ITimeNoiseBehavior.Inject() => _thermal.Inject();
}
