using System;
using SpiceSharp.ParameterSets;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.NoiseSources;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.Mosfets.Level1
{
    /// <summary>
    /// Noise behavior for a <see cref="Mosfet1"/>.
    /// </summary>
    /// <seealso cref="Frequency"/>
    /// <seealso cref="INoiseBehavior"/>
    public class Noise : Frequency, 
        INoiseBehavior
    {
        private readonly INoiseSimulationState _state;
        private readonly NoiseThermal _rd, _rs;
        private readonly NoiseShot _id;
        private readonly NoiseGain _flicker;

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
        /// <param name="name">The name of the behavior.</param>
        /// <param name="context">The binding context.</param>
        public Noise(string name, ComponentBindingContext context) 
            : base(name, context)
        {
            _state = context.GetState<INoiseSimulationState>();
            var d = Variables.Drain;
            var s = Variables.Source;
            var dp = Variables.DrainPrime;
            var sp = Variables.SourcePrime;

            _rd = new NoiseThermal("rd", d, dp);
            _rs = new NoiseThermal("rs", s, sp);
            _id = new NoiseShot("id", dp, sp);
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

        /// <inheritdoc/>
        void INoiseBehavior.Compute()
        {
            double coxSquared;
            if (ModelTemperature.Properties.OxideCapFactor == 0.0)
                coxSquared = 3.9 * 8.854214871e-12 / 1e-7;
            else
                coxSquared = ModelTemperature.Properties.OxideCapFactor;
            coxSquared *= coxSquared;

            _rd.Compute(Properties.DrainConductance, Parameters.Temperature);
            _rs.Compute(Properties.SourceConductance, Parameters.Temperature);
            _id.Compute(2.0 / 3.0 * Math.Abs(Gm));
            _flicker.Compute(ModelParameters.FlickerNoiseCoefficient *
                 Math.Exp(ModelParameters.FlickerNoiseExponent *
                 Math.Log(Math.Max(Math.Abs(Id), 1e-38))) /
                 (_state.Point.Value.Frequency * Parameters.Width * Parameters.ParallelMultiplier *
                 (Parameters.Length - 2 * ModelParameters.LateralDiffusion) * coxSquared));
        }
    }
}
