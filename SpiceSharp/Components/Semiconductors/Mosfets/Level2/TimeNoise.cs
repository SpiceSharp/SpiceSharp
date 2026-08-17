using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.NoiseSources;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Components.Mosfets.Level2;

/// <summary>
/// Transient noise behavior for a <see cref="Mosfet2"/>.
/// </summary>
/// <seealso cref="Biasing"/>
/// <seealso cref="ITimeNoiseBehavior"/>
[BehaviorFor(typeof(Mosfet2)), AddBehaviorIfNo(typeof(ITimeNoiseBehavior))]
[GeneratedParameters]
public partial class TimeNoise : Biasing,
    ITimeNoiseBehavior
{
    private readonly TimeNoiseThermal _rd, _rs, _id;
    private readonly TimeNoiseFlicker _flicker;
    private readonly ModelProperties _properties;

    /// <inheritdoc/>
    public double NoiseDensity => _rd.NoiseDensity + _rs.NoiseDensity + _id.NoiseDensity +
        _flicker.NoiseDensity;

    /// <inheritdoc/>
    /// <remarks>
    /// The sources sit across different node pairs, so the sum is an export convenience rather than
    /// a current that is injected anywhere.
    /// </remarks>
    public double Current => _rd.Current + _rs.Current + _id.Current + _flicker.Current;

    /// <include file='../common/docs.xml' path='docs/members/ThermalDrain/*'/>
    [ParameterName("rd"), ParameterInfo("The thermal noise of the drain resistor")]
    public ITimeNoiseSource ThermalDrain => _rd;

    /// <include file='../common/docs.xml' path='docs/members/ThermalSource/*'/>
    [ParameterName("rs"), ParameterInfo("The thermal noise of the source resistor")]
    public ITimeNoiseSource ThermalSource => _rs;

    /// <summary>
    /// Gets the thermal noise source of the channel.
    /// </summary>
    /// <value>
    /// The channel noise source.
    /// </value>
    /// <remarks>
    /// The channel noise of a mosfet is the thermal noise of two thirds of its transconductance.
    /// The frequency-domain behavior exports it under the same name, where it is also modelled as a
    /// thermal rather than a shot noise source.
    /// </remarks>
    [ParameterName("id"), ParameterInfo("The channel noise of the drain current")]
    public ITimeNoiseSource ChannelDrainCurrent => _id;

    /// <include file='../common/docs.xml' path='docs/members/FlickerNoise/*'/>
    [ParameterName("flicker"), ParameterInfo("The flicker noise")]
    public ITimeNoiseSource Flicker => _flicker;

    /// <summary>
    /// Initializes a new instance of the <see cref="TimeNoise"/> class.
    /// </summary>
    /// <param name="context">The binding context.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
    public TimeNoise(IComponentBindingContext context)
        : base(context)
    {
        var biasing = context.GetState<IBiasingSimulationState>();
        var noise = context.GetState<ITimeNoiseSimulationState>();
        _properties = context.ModelBehaviors.GetValue<ModelTemperature>().Properties;

        // The names are unique within the circuit, unlike the local names that the frequency-domain
        // noise sources use, because the random stream of a source is seeded from it.
        _rd = new TimeNoiseThermal(Name.Combine("rd"), noise, biasing, Variables.Drain, Variables.DrainPrime);
        _rs = new TimeNoiseThermal(Name.Combine("rs"), noise, biasing, Variables.Source, Variables.SourcePrime);
        _id = new TimeNoiseThermal(Name.Combine("id"), noise, biasing, Variables.DrainPrime, Variables.SourcePrime);
        _flicker = new TimeNoiseFlicker(Name.Combine("flicker"), noise, biasing, Variables.DrainPrime, Variables.SourcePrime);
    }

    /// <inheritdoc/>
    void ITimeNoiseBehavior.ProbeNoise()
    {
        // The two parasitic resistors do not depend on the operating point, but the channel noise
        // does, so the device pays for the refresh of all four either way.
        _rd.Compute(Properties.DrainConductance, Parameters.Temperature);
        _rs.Compute(Properties.SourceConductance, Parameters.Temperature);
        _id.Compute(2.0 / 3.0 * Math.Abs(Gm), Parameters.Temperature);

        // The flicker noise of a mosfet is referred to the gate oxide, so the coefficient carries
        // the channel area and the oxide capacitance.
        _flicker.Compute(
            ModelParameters.FlickerNoiseCoefficient /
            (Parameters.Width * Parameters.ParallelMultiplier *
            (Parameters.Length - (2 * ModelParameters.LateralDiffusion)) *
            _properties.OxideCapFactor * _properties.OxideCapFactor),
            ModelParameters.FlickerNoiseExponent, Id);

        _rd.ProbeNoise();
        _rs.ProbeNoise();
        _id.ProbeNoise();
        _flicker.ProbeNoise();
    }

    /// <inheritdoc/>
    public override void Load()
    {
        base.Load();
        _rd.InjectNoise();
        _rs.InjectNoise();
        _id.InjectNoise();
        _flicker.InjectNoise();
    }
}
