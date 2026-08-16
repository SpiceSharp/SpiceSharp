using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.NoiseSources;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Components.Diodes;

/// <summary>
/// Transient noise behavior for a <see cref="Diode"/>.
/// </summary>
/// <seealso cref="Time"/>
/// <seealso cref="ITimeNoiseBehavior"/>
[BehaviorFor(typeof(Diode)), AddBehaviorIfNo(typeof(ITimeNoiseBehavior))]
[GeneratedParameters]
public partial class TimeNoise : Time,
    ITimeNoiseBehavior
{
    private readonly TimeNoiseThermal _rs;
    private readonly TimeNoiseShot _id;

    /// <inheritdoc/>
    public double NoiseDensity => _rs.NoiseDensity + _id.NoiseDensity;

    /// <inheritdoc/>
    /// <remarks>
    /// Implemented explicitly, because <see cref="Biasing.Current"/> is the deterministic current
    /// through the diode. The two sources sit across different node pairs, so the sum is an export
    /// convenience rather than a current that is injected anywhere.
    /// </remarks>
    double ITimeNoiseSource.Current => _rs.Current + _id.Current;

    /// <summary>
    /// Gets the thermal noise source of the series resistance.
    /// </summary>
    /// <value>
    /// The thermal noise source.
    /// </value>
    [ParameterName("rs"), ParameterInfo("The thermal noise of the resistance")]
    public ITimeNoiseSource ThermalResistance => _rs;

    /// <summary>
    /// Gets the shot noise source of the diode current.
    /// </summary>
    /// <value>
    /// The shot noise source.
    /// </value>
    [ParameterName("id"), ParameterInfo("The shot noise of the diode current")]
    public ITimeNoiseSource ShotCurrent => _id;

    /// <summary>
    /// Initializes a new instance of the <see cref="TimeNoise"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
    public TimeNoise(IComponentBindingContext context)
        : base(context)
    {
        var biasing = context.GetState<IBiasingSimulationState>();
        var noise = context.GetState<ITimeNoiseSimulationState>();

        // The names are unique within the circuit, unlike the local names that the frequency-domain
        // noise sources use, because the random stream of a source is seeded from it.
        _rs = new TimeNoiseThermal(Name.Combine("rs"), noise, biasing, Variables.Positive, Variables.PosPrime);
        _id = new TimeNoiseShot(Name.Combine("id"), noise, biasing, Variables.PosPrime, Variables.Negative);
    }

    /// <inheritdoc/>
    void ITimeNoiseBehavior.Probe()
    {
        double m = Parameters.ParallelMultiplier;
        double n = Parameters.SeriesMultiplier;

        // The series resistance does not depend on the operating point, but the shot noise does, so
        // the device pays for the refresh of both either way.
        _rs.Compute(ModelTemperature.Conductance * m / n * Parameters.Area, Parameters.Temperature);

        // Shot noise comes from the conduction current of the junction. The capacitor current that
        // Time.Load() folded into the local current is a displacement current and carries none.
        _id.Compute((LocalCurrent - CapCurrent) * m / n);

        _rs.Probe();
        _id.Probe();
    }

    /// <inheritdoc/>
    void ITimeNoiseBehavior.Inject()
    {
        _rs.Inject();
        _id.Inject();
    }
}
