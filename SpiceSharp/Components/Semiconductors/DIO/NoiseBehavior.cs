﻿using System;
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

        private INoiseSimulationState _state;

        /// <summary>
        /// Initializes a new instance of the <see cref="NoiseBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public NoiseBehavior(string name, ComponentBindingContext context) : base(name, context) 
        {
            _mnp = context.ModelBehaviors.Parameters.GetValue<ModelNoiseParameters>();
            _state = context.GetState<INoiseSimulationState>();
            DiodeNoise.Bind(context, context.Nodes[0], PosPrime, context.Nodes[1]);
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
