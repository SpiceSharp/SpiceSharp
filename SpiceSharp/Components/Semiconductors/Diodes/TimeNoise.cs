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
    private readonly TimeNoiseFlicker _flicker;

    /// <inheritdoc/>
    public double NoiseDensity => _rs.NoiseDensity + _id.NoiseDensity + _flicker.NoiseDensity;

    /// <inheritdoc/>
    /// <remarks>
    /// Implemented explicitly, because <see cref="Biasing.Current"/> is the deterministic current
    /// through the diode. The sources sit across different node pairs, so the sum is an export
    /// convenience rather than a current that is injected anywhere.
    /// </remarks>
    double ITimeNoiseSource.Current => _rs.Current + _id.Current + _flicker.Current;

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
    /// Gets the flicker noise source.
    /// </summary>
    /// <value>
    /// The flicker noise source.
    /// </value>
    [ParameterName("flicker"), ParameterInfo("The flicker noise")]
    public ITimeNoiseSource Flicker => _flicker;

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
        _flicker = new TimeNoiseFlicker(Name.Combine("flicker"), noise, biasing, Variables.PosPrime, Variables.Negative);
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
        // Time.Load() folded into the local current is a displacement current and carries none, and
        // neither does it carry flicker noise.
        double conduction = LocalCurrent - CapCurrent;
        _id.Compute(conduction * m / n);

        // The multipliers land on the coefficient rather than on the current, mirroring the
        // frequency-domain behavior so that the two analyses agree on the same netlist.
        _flicker.Compute(ModelParameters.FlickerNoiseCoefficient * m / n,
            ModelParameters.FlickerNoiseExponent, conduction);

        _rs.Probe();
        _id.Probe();
        _flicker.Probe();
    }

    /// <inheritdoc/>
    void ITimeNoiseBehavior.Inject()
    {
        _rs.Inject();
        _id.Inject();
        _flicker.Inject();
    }
}
