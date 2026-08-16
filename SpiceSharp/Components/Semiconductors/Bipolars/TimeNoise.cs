using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.NoiseSources;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Components.Bipolars;

/// <summary>
/// Transient noise behavior for a <see cref="BipolarJunctionTransistor"/>.
/// </summary>
/// <seealso cref="Time"/>
/// <seealso cref="ITimeNoiseBehavior"/>
[BehaviorFor(typeof(BipolarJunctionTransistor)), AddBehaviorIfNo(typeof(ITimeNoiseBehavior))]
[GeneratedParameters]
public partial class TimeNoise : Time,
    ITimeNoiseBehavior
{
    private readonly TimeNoiseThermal _rc, _rb, _re;
    private readonly TimeNoiseShot _ic, _ib;
    private readonly TimeNoiseFlicker _flicker;

    /// <inheritdoc/>
    public double NoiseDensity => _rc.NoiseDensity + _rb.NoiseDensity + _re.NoiseDensity +
        _ic.NoiseDensity + _ib.NoiseDensity + _flicker.NoiseDensity;

    /// <inheritdoc/>
    /// <remarks>
    /// The sources sit across different node pairs, so the sum is an export convenience rather than
    /// a current that is injected anywhere.
    /// </remarks>
    public double Current => _rc.Current + _rb.Current + _re.Current + _ic.Current + _ib.Current +
        _flicker.Current;

    /// <summary>
    /// Gets the thermal noise source of the resistor at the collector.
    /// </summary>
    /// <value>
    /// The thermal noise source.
    /// </value>
    [ParameterName("rc"), ParameterInfo("The thermal noise at the collector")]
    public ITimeNoiseSource ThermalCollectorResistor => _rc;

    /// <summary>
    /// Gets the thermal noise source of the resistor at the base.
    /// </summary>
    /// <value>
    /// The thermal noise source.
    /// </value>
    [ParameterName("rb"), ParameterInfo("The thermal noise at the base")]
    public ITimeNoiseSource ThermalBaseResistor => _rb;

    /// <summary>
    /// Gets the thermal noise source of the resistor at the emitter.
    /// </summary>
    /// <value>
    /// The thermal noise source.
    /// </value>
    [ParameterName("re"), ParameterInfo("The thermal noise at the emitter")]
    public ITimeNoiseSource ThermalEmitterResistor => _re;

    /// <summary>
    /// Gets the shot noise source of the collector-emitter current.
    /// </summary>
    /// <value>
    /// The shot noise source.
    /// </value>
    [ParameterName("ic"), ParameterInfo("The shot noise of the collector-emitter current")]
    public ITimeNoiseSource ShotCollectorCurrent => _ic;

    /// <summary>
    /// Gets the shot noise source of the base-emitter current.
    /// </summary>
    /// <value>
    /// The shot noise source.
    /// </value>
    [ParameterName("ib"), ParameterInfo("The shot noise of the base-emitter current")]
    public ITimeNoiseSource ShotBaseCurrent => _ib;

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
        var noise = context.GetState<ITimeNoiseSimulationState>();
        var c = BiasingState.GetSharedVariable(context.Nodes[0]);
        var b = BiasingState.GetSharedVariable(context.Nodes[1]);
        var e = BiasingState.GetSharedVariable(context.Nodes[2]);

        // The names are unique within the circuit, unlike the local names that the frequency-domain
        // noise sources use, because the random stream of a source is seeded from it.
        _rc = new TimeNoiseThermal(Name.Combine("rc"), noise, BiasingState, c, CollectorPrime);
        _rb = new TimeNoiseThermal(Name.Combine("rb"), noise, BiasingState, b, BasePrime);
        _re = new TimeNoiseThermal(Name.Combine("re"), noise, BiasingState, e, EmitterPrime);
        _ic = new TimeNoiseShot(Name.Combine("ic"), noise, BiasingState, CollectorPrime, EmitterPrime);
        _ib = new TimeNoiseShot(Name.Combine("ib"), noise, BiasingState, BasePrime, EmitterPrime);
        _flicker = new TimeNoiseFlicker(Name.Combine("flicker"), noise, BiasingState, BasePrime, EmitterPrime);
    }

    /// <inheritdoc/>
    void ITimeNoiseBehavior.Probe()
    {
        // The base resistance is modulated by the operating point, and both shot noise densities
        // are, so all six densities are refreshed at every probed timepoint.
        _rc.Compute(ModelTemperature.CollectorConduct * Parameters.Area, Parameters.Temperature);
        _rb.Compute(ConductanceX, Parameters.Temperature);
        _re.Compute(ModelTemperature.EmitterConduct * Parameters.Area, Parameters.Temperature);
        _ic.Compute(CollectorCurrent);
        _ib.Compute(BaseCurrent);
        _flicker.Compute(ModelParameters.FlickerNoiseCoefficient,
            ModelParameters.FlickerNoiseExponent, BaseCurrent);

        _rc.Probe();
        _rb.Probe();
        _re.Probe();
        _ic.Probe();
        _ib.Probe();
        _flicker.Probe();
    }

    /// <inheritdoc/>
    void ITimeNoiseBehavior.Inject()
    {
        _rc.Inject();
        _rb.Inject();
        _re.Inject();
        _ic.Inject();
        _ib.Inject();
        _flicker.Inject();
    }
}
