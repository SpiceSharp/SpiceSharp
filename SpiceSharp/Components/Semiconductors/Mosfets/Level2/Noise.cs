using System;
using SpiceSharp.ParameterSets;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.NoiseSources;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.Mosfets.Level2
{
    /// <summary>
    /// Noise behavior for a <see cref="Mosfet2"/>.
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
        public double OutputNoiseDensity => _rd.OutputNoiseDensity + _rs.OutputNoiseDensity + _id.OutputNoiseDensity + _flicker.OutputNoiseDensity;

        /// <inheritdoc/>
        public double TotalOutputNoise => _rd.TotalOutputNoise + _rs.TotalOutputNoise + _id.TotalOutputNoise + _flicker.TotalOutputNoise;

        /// <inheritdoc/>
        public double TotalInputNoise => _rd.TotalInputNoise + _rs.TotalInputNoise + _id.TotalInputNoise + _flicker.TotalInputNoise;

        /// <summary>
        /// Gets the thermal noise of the drain series resistance.
        /// </summary>
        /// <value>
        /// The thermal noise source.
        /// </value>
        [ParameterName("rd"), ParameterInfo("The thermal noise of the drain resistor")]
        public INoiseSource ThermalDrain => _rd;

        /// <summary>
        /// Gets the thermal noise of the source series resistance.
        /// </summary>
        /// <value>
        /// The thermal noise source.
        /// </value>
        [ParameterName("rs"), ParameterInfo("The thermal noise of the source resistor")]
        public INoiseSource ThermalSource => _rs;

        /// <summary>
        /// Gets the shot noise of the drain current.
        /// </summary>
        /// <value>
        /// The shot noise source.
        /// </value>
        [ParameterName("id"), ParameterInfo("The shot noise of the drain current")]
        public INoiseSource ShotDrainCurrent => _id;

        /// <summary>
        /// Gets the flicker noise.
        /// </summary>
        /// <value>
        /// The flicker noise source.
        /// </value>
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
            var complex = context.GetState<IComplexSimulationState>();

            var d = complex.GetSharedVariable(context.Nodes[0]);
            // var g = complex.GetSharedVariable(context.Nodes[1]);
            var s = complex.GetSharedVariable(context.Nodes[2]);
            // var b = complex.GetSharedVariable(context.Nodes[3]);
            var dp = DrainPrime;
            var sp = SourcePrime;

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
            if (ModelParameters.OxideCapFactor > 0.0)
                coxSquared = ModelParameters.OxideCapFactor;
            else
                coxSquared = 3.9 * 8.854214871e-12 / 1e-7;
            coxSquared *= coxSquared;

            _rd.Compute(DrainConductance, Parameters.Temperature);
            _rs.Compute(SourceConductance, Parameters.Temperature);
            _id.Compute(2.0 / 3.0 * Math.Abs(Transconductance));
            _flicker.Compute(ModelParameters.FlickerNoiseCoefficient *
                Math.Exp(ModelParameters.FlickerNoiseExponent * Math.Log(Math.Max(Math.Abs(DrainCurrent), 1e-38))) /
                (Parameters.Width * (Parameters.Length - 2 * ModelParameters.LateralDiffusion) *
                 coxSquared) / _state.Point.Value.Frequency);
        }
    }
}
