using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.NoiseSources;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Components.Mosfets.Level2
{
    /// <summary>
    /// Noise behavior for a <see cref="Mosfet2"/>.
    /// </summary>
    /// <seealso cref="Frequency"/>
    /// <seealso cref="INoiseBehavior"/>
    [BehaviorFor(typeof(Mosfet2)), AddBehaviorIfNo(typeof(INoiseBehavior)), BehaviorRequires(typeof(IMosfetBiasingBehavior))]
    [GeneratedParameters]
    public partial class Noise : Frequency,
        INoiseBehavior
    {
        private readonly INoiseSimulationState _state;
        private readonly NoiseThermal _rd, _rs, _id;
        private readonly NoiseGain _flicker;
        private readonly ModelProperties _properties;

        /// <inheritdoc/>
        [ParameterName("noise"), ParameterInfo("The total output noise density")]
        public double OutputNoiseDensity => _rd.OutputNoiseDensity + _rs.OutputNoiseDensity + _id.OutputNoiseDensity + _flicker.OutputNoiseDensity;

        /// <inheritdoc/>
        [ParameterName("onoise"), ParameterInfo("The total integrated output noise")]
        public double TotalOutputNoise => _rd.TotalOutputNoise + _rs.TotalOutputNoise + _id.TotalOutputNoise + _flicker.TotalOutputNoise;

        /// <inheritdoc/>
        [ParameterName("inoise"), ParameterInfo("The total integrated input noise")]
        public double TotalInputNoise => _rd.TotalInputNoise + _rs.TotalInputNoise + _id.TotalInputNoise + _flicker.TotalInputNoise;

        /// <include file='../common/docs.xml' path='docs/members/ThermalDrain/*'/>
        [ParameterName("rd"), ParameterInfo("The thermal noise of the drain resistor")]
        public INoiseSource ThermalDrain => _rd;

        /// <include file='../common/docs.xml' path='docs/members/ThermalSource/*'/>
        [ParameterName("rs"), ParameterInfo("The thermal noise of the source resistor")]
        public INoiseSource ThermalSource => _rs;

        /// <include file='../common/docs.xml' path='docs/members/ShotNoise/*'/>
        [ParameterName("id"), ParameterInfo("The shot noise of the drain current")]
        public INoiseSource ShotDrainCurrent => _id;

        /// <include file='../common/docs.xml' path='docs/members/FlickerNoise/*'/>
        [ParameterName("flicker"), ParameterInfo("The flicker noise")]
        public INoiseSource Flicker => _flicker;

        /// <summary>
        /// Initializes a new instance of the <see cref="Noise"/> class.
        /// </summary>
        /// <param name="context">The binding context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
        public Noise(IComponentBindingContext context)
            : base(context)
        {
            _state = context.GetState<INoiseSimulationState>();
            _properties = context.ModelBehaviors.GetValue<ModelTemperature>().Properties;
            var d = Variables.Drain;
            var s = Variables.Source;
            var dp = Variables.DrainPrime;
            var sp = Variables.SourcePrime;

            _rd = new NoiseThermal("rd", d, dp);
            _rs = new NoiseThermal("rs", s, sp);
            _id = new NoiseThermal("id", dp, sp);
            _flicker = new NoiseGain("flicker", dp, sp);
        }

        /// <inheritdoc/>
        void INoiseSource.Initialize()
        {
            _rs.Initialize();
            _rd.Initialize();
            _id.Initialize();
            _flicker.Initialize();
        }

        /// <inheritdoc />
        void INoiseBehavior.Load() { }

        /// <inheritdoc/>
        void INoiseBehavior.Compute()
        {
            _rd.Compute(Behavior.Properties.DrainConductance, Behavior.Parameters.Temperature);
            _rs.Compute(Behavior.Properties.SourceConductance, Behavior.Parameters.Temperature);
            _id.Compute(2.0 / 3.0 * Math.Abs(Behavior.Gm), Behavior.Parameters.Temperature);
            _flicker.Compute(ModelParameters.FlickerNoiseCoefficient *
                 Math.Exp(ModelParameters.FlickerNoiseExponent *
                 Math.Log(Math.Max(Math.Abs(Behavior.Id), 1e-38))) /
                 (_state.Point.Value.Frequency * Behavior.Parameters.Width *
                 Behavior.Parameters.ParallelMultiplier *
                 (Behavior.Parameters.Length - 2 * ModelParameters.LateralDiffusion) *
                 _properties.OxideCapFactor * _properties.OxideCapFactor));
        }
    }
}
