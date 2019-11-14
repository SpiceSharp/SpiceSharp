using System;
using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.Components.NoiseSources;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.BipolarBehaviors
{
    /// <summary>
    /// Noise behavior for <see cref="BipolarJunctionTransistor"/>
    /// </summary>
    public class NoiseBehavior : BiasingBehavior, INoiseBehavior
    {
        /// <summary>
        /// Gets the noise parameters.
        /// </summary>
        protected ModelNoiseParameters NoiseParameters { get; private set; }

        /// <summary>
        /// Noise sources by their index
        /// </summary>
        private const int RcNoise = 0;
        private const int RbNoise = 1;
        private const int ReNoise = 2;
        private const int IcNoise = 3;
        private const int IbNoise = 4;
        private const int FlickerNoise = 5;

        /// <summary>
        /// Noise generators
        /// </summary>
        public ComponentNoise BipolarJunctionTransistorNoise { get; } = new ComponentNoise(
            new NoiseThermal("rc", 0, 4),
            new NoiseThermal("rb", 1, 5),
            new NoiseThermal("re", 2, 6),
            new NoiseShot("ic", 4, 6),
            new NoiseShot("ib", 5, 6),
            new NoiseGain("1overf", 5, 6)
            );

        private INoiseSimulationState _state;

        /// <summary>
        /// Initializes a new instance of the <see cref="NoiseBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public NoiseBehavior(string name, ComponentBindingContext context) : base(name, context) 
        {
            NoiseParameters = context.ModelBehaviors.Parameters.GetValue<ModelNoiseParameters>();
            _state = context.GetState<INoiseSimulationState>();
            BipolarJunctionTransistorNoise.Bind(context, context.Nodes[0], context.Nodes[1], context.Nodes[2], context.Nodes[3],
                CollectorPrime, BasePrime, EmitterPrime);
        }
        
        /// <summary>
        /// Noise calculations
        /// </summary>
        void INoiseBehavior.Noise()
        {
            var generators = BipolarJunctionTransistorNoise.Generators;

            // Set noise parameters
            generators[RcNoise].SetCoefficients(ModelTemperature.CollectorConduct * BaseParameters.Area);
            generators[RbNoise].SetCoefficients(ConductanceX);
            generators[ReNoise].SetCoefficients(ModelTemperature.EmitterConduct * BaseParameters.Area);
            generators[IcNoise].SetCoefficients(CollectorCurrent);
            generators[IbNoise].SetCoefficients(BaseCurrent);
            generators[FlickerNoise].SetCoefficients(NoiseParameters.FlickerNoiseCoefficient * Math.Exp(NoiseParameters.FlickerNoiseExponent 
                * Math.Log(Math.Max(Math.Abs(BaseCurrent), 1e-38))) / _state.Frequency);

            // Evaluate all noise sources
            BipolarJunctionTransistorNoise.Evaluate();
        }
    }
}
