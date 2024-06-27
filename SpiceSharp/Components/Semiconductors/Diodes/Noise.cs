using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.NoiseSources;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Components.Diodes
{
    /// <summary>
    /// Noise behavior for <see cref="Diode"/>.
    /// </summary>
    /// <seealso cref="Frequency"/>
    /// <seealso cref="INoiseBehavior"/>
    [BehaviorFor(typeof(Diode)), AddBehaviorIfNo(typeof(INoiseBehavior))]
    [GeneratedParameters]
    public partial class Noise : Frequency,
        INoiseBehavior
    {
        private readonly INoiseSimulationState _state;
        private readonly NoiseThermal _rs;
        private readonly NoiseShot _id;
        private readonly NoiseGain _flicker;

        /// <inheritdoc/>
        public double OutputNoiseDensity => _rs.OutputNoiseDensity + _id.OutputNoiseDensity + _flicker.OutputNoiseDensity;

        /// <inheritdoc/>
        public double TotalOutputNoise => _rs.TotalOutputNoise + _id.TotalOutputNoise + _flicker.TotalOutputNoise;

        /// <inheritdoc/>
        public double TotalInputNoise => _rs.TotalInputNoise + _id.TotalInputNoise + _flicker.TotalInputNoise;

        /// <summary>
        /// Gets the thermal noise source of the series resistance.
        /// </summary>
        /// <value>
        /// The thermal noise source.
        /// </value>
        [ParameterName("rs"), ParameterInfo("The thermal noise of the resistance")]
        public INoiseSource ThermalResistance => _rs;

        /// <summary>
        /// Gets the shot noise source of the diode current.
        /// </summary>
        /// <value>
        /// The shot noise source.
        /// </value>
        [ParameterName("id"), ParameterInfo("The shot noise of the diode current")]
        public INoiseSource ShotCurrent => _id;

        /// <summary>
        /// Gets the flicker noise source.
        /// </summary>
        /// <value>
        /// The flicker noise source.
        /// </value>
        [ParameterName("flicker"), ParameterInfo("The flicker noise")]
        public INoiseSource Flicker => _flicker;

        /// <summary>
        /// Initializes a new instance of the <see cref="Noise"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
        public Noise(IComponentBindingContext context)
            : base(context)
        {
            _state = context.GetState<INoiseSimulationState>();
            _rs = new NoiseThermal("rs", ComplexVariables.Positive, ComplexVariables.PosPrime);
            _id = new NoiseShot("id", ComplexVariables.PosPrime, ComplexVariables.Negative);
            _flicker = new NoiseGain("flicker", ComplexVariables.PosPrime, ComplexVariables.Negative);
        }

        /// <inheritdoc/>
        void INoiseSource.Initialize()
        {
            _rs.Initialize();
            _id.Initialize();
            _flicker.Initialize();
        }

        /// <inheritdoc />
        void INoiseBehavior.Load() { }

        /// <inheritdoc/>
        void INoiseBehavior.Compute()
        {
            double m = Parameters.ParallelMultiplier;
            double n = Parameters.SeriesMultiplier;

            _rs.Compute(ModelTemperature.Conductance * m / n * Parameters.Area, Parameters.Temperature);
            _id.Compute(LocalCurrent * m / n);
            _flicker.Compute(ModelParameters.FlickerNoiseCoefficient * m / n * Math.Exp(ModelParameters.FlickerNoiseExponent
                * Math.Log(Math.Max(Math.Abs(LocalCurrent), 1e-38))) / _state.Point.Value.Frequency);
        }
    }
}
