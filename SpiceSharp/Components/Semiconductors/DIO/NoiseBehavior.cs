using System;
using SpiceSharp.Behaviors;
using SpiceSharp.Circuits;
using SpiceSharp.Components.NoiseSources;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.DiodeBehaviors
{
    /// <summary>
    /// Noise behavior for <see cref="Diode"/>
    /// </summary>
    public class NoiseBehavior : FrequencyBehavior, INoiseBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        private ModelNoiseParameters _mnp;

        /// <summary>
        /// Noise sources by their index
        /// </summary>
        private const int RsNoise = 0;
        private const int IdNoise = 1;
        private const int FlickerNoise = 2;

        /// <summary>
        /// Noise generators
        /// </summary>
        public ComponentNoise DiodeNoise { get; } = new ComponentNoise(
            new NoiseThermal("rs", 0, 1),
            new NoiseShot("id", 1, 2),
            new NoiseGain("1overf", 1, 2));

        
        private NoiseSimulationState _state;

        /// <summary>
        /// Creates a new instance of the <see cref="NoiseBehavior"/> class.
        /// </summary>
        /// <param name="name">Name</param>
        public NoiseBehavior(string name) : base(name) { }

        /// <summary>
        /// Bind the behavior to a simulation.
        /// </summary>
        /// <param name="context">The binding context.</param>
        public override void Bind(BindingContext context)
        {
            base.Bind(context);
            _mnp = ModelTemperature.Parameters.Get<ModelNoiseParameters>();
            _state = context.States.Get<NoiseSimulationState>();
            DiodeNoise.Bind(context, PosNode, PosPrimeNode, NegNode);
        }

        /// <summary>
        /// Noise calculations
        /// </summary>
        void INoiseBehavior.Noise()
        {
            var noise = _state;

            // Set noise parameters
            DiodeNoise.Generators[RsNoise].SetCoefficients(ModelTemperature.Conductance * BaseParameters.Area);
            DiodeNoise.Generators[IdNoise].SetCoefficients(Current);
            DiodeNoise.Generators[FlickerNoise].SetCoefficients(_mnp.FlickerNoiseCoefficient * Math.Exp(_mnp.FlickerNoiseExponent 
                * Math.Log(Math.Max(Math.Abs(Current), 1e-38))) / noise.Frequency);

            // Evaluate noise
            DiodeNoise.Evaluate();
        }
    }
}
