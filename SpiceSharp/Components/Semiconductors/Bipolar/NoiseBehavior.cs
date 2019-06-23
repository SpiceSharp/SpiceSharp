using System;
using SpiceSharp.Behaviors;
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
        /// <value>
        /// The noise parameters.
        /// </value>
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
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public NoiseBehavior(string name) : base(name) { }

        /// <summary>
        /// Setup behavior
        /// </summary>
        /// <param name="simulation">Simulation</param>
        /// <param name="provider">Data provider</param>
        /// <exception cref="ArgumentNullException">provider</exception>
        public override void Setup(Simulation simulation, SetupDataProvider provider)
        {
            base.Setup(simulation, provider);
            provider.ThrowIfNull(nameof(provider));

            // Get parameters
            NoiseParameters = provider.GetParameterSet<ModelNoiseParameters>("model");
        }

        /// <summary>
        /// Connect noise sources
        /// </summary>
        public void ConnectNoise()
        {
            // Connect noise
            BipolarJunctionTransistorNoise.Setup(CollectorNode, BaseNode, EmitterNode, SubstrateNode,
                CollectorPrimeNode, BasePrimeNode, EmitterPrimeNode);
        }

        /// <summary>
        /// Noise calculations
        /// </summary>
        /// <param name="simulation">Noise simulation</param>
        public void Noise(Noise simulation)
        {
            simulation.ThrowIfNull(nameof(simulation));

            var noise = simulation.NoiseState;
            var generators = BipolarJunctionTransistorNoise.Generators;

            // Set noise parameters
            generators[RcNoise].SetCoefficients(ModelTemperature.CollectorConduct * BaseParameters.Area);
            generators[RbNoise].SetCoefficients(ConductanceX);
            generators[ReNoise].SetCoefficients(ModelTemperature.EmitterConduct * BaseParameters.Area);
            generators[IcNoise].SetCoefficients(CollectorCurrent);
            generators[IbNoise].SetCoefficients(BaseCurrent);
            generators[FlickerNoise].SetCoefficients(NoiseParameters.FlickerNoiseCoefficient * Math.Exp(NoiseParameters.FlickerNoiseExponent 
                * Math.Log(Math.Max(Math.Abs(BaseCurrent), 1e-38))) / noise.Frequency);

            // Evaluate all noise sources
            BipolarJunctionTransistorNoise.Evaluate(simulation);
        }
    }
}
