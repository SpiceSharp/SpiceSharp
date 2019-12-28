using System;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.NoiseSources;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.DiodeBehaviors
{
    /// <summary>
    /// Noise behavior for <see cref="Diode"/>
    /// </summary>
    public class NoiseBehavior : FrequencyBehavior, INoiseBehavior
    {
        private readonly INoiseSimulationState _state;
        private readonly ModelNoiseParameters _mnp;

        /// <summary>
        /// Noise sources by their index
        /// </summary>
        private const int _rsNoise = 0;
        private const int _idNoise = 1;
        private const int _flickerNoise = 2;

        /// <summary>
        /// Noise generators
        /// </summary>
        public ComponentNoise DiodeNoise { get; } = new ComponentNoise(
            new NoiseThermal("rs", 0, 1),
            new NoiseShot("id", 1, 2),
            new NoiseGain("1overf", 1, 2));

        /// <summary>
        /// Initializes a new instance of the <see cref="NoiseBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public NoiseBehavior(string name, ComponentBindingContext context) : base(name, context) 
        {
            _mnp = context.ModelBehaviors.GetParameterSet<ModelNoiseParameters>();
            _state = context.GetState<INoiseSimulationState>();
            DiodeNoise.Bind(context, context.Nodes[0], PosPrime, context.Nodes[1]);
        }

        /// <summary>
        /// Noise calculations
        /// </summary>
        void INoiseBehavior.Noise()
        {
            var noise = _state;
            var m = Parameters.ParallelMultiplier;
            var n = Parameters.SeriesMultiplier;

            // Set noise parameters
            DiodeNoise.Generators[_rsNoise].SetCoefficients(ModelTemperature.Conductance * m / n * Parameters.Area);
            DiodeNoise.Generators[_idNoise].SetCoefficients(LocalCurrent * m / n);
            DiodeNoise.Generators[_flickerNoise].SetCoefficients(_mnp.FlickerNoiseCoefficient * m / n * Math.Exp(_mnp.FlickerNoiseExponent 
                * Math.Log(Math.Max(Math.Abs(LocalCurrent), 1e-38))) / noise.Frequency);

            // Evaluate noise
            DiodeNoise.Evaluate();
        }
    }
}
