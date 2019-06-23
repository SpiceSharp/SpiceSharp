using System;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.MosfetBehaviors.Common;
using SpiceSharp.Components.NoiseSources;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.MosfetBehaviors.Level1
{
    /// <summary>
    /// Noise behavior for a <see cref="Mosfet1"/>
    /// </summary>
    public class NoiseBehavior : FrequencyBehavior, INoiseBehavior
    {
        /// <summary>
        /// Gets the noise parameters.
        /// </summary>
        /// <value>
        /// The noise parameters.
        /// </value>
        protected ModelNoiseParameters NoiseParameters { get; private set; }

        /// <summary>
        /// Noise generators by their index
        /// </summary>
        protected const int RdNoise = 0;
        protected const int RsNoise = 1;
        protected const int IdNoise = 2;
        protected const int FlickerNoise = 3;

        /// <summary>
        /// Noise generators
        /// </summary>
        public ComponentNoise MosfetNoise { get; } = new ComponentNoise(
            new NoiseThermal("rd", 0, 4),
            new NoiseThermal("rs", 2, 5),
            new NoiseThermal("id", 4, 5),
            new NoiseGain("1overf", 4, 5)
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
        public override void Setup(Simulation simulation, SetupDataProvider provider)
        {
            base.Setup(simulation, provider);
            provider.ThrowIfNull(nameof(provider));

            // Get parameters
            NoiseParameters = provider.GetParameterSet<ModelNoiseParameters>("model");
        }

        /// <summary>
        /// Connect noise
        /// </summary>
        public void ConnectNoise()
        {
            // Connect noise sources
            MosfetNoise.Setup(
                DrainNode,
                GateNode,
                SourceNode, 
                BulkNode, 
                DrainNodePrime,
                SourceNodePrime);
        }

        /// <summary>
        /// Calculate the noise contributions.
        /// </summary>
        /// <param name="simulation">The noise simulation.</param>
        /// <exception cref="ArgumentNullException">simulation</exception>
        public void Noise(Noise simulation)
        {
            simulation.ThrowIfNull(nameof(simulation));

            var noise = simulation.NoiseState;

            double coxSquared;
            if (ModelParameters.OxideCapFactor > 0.0)
                coxSquared = ModelParameters.OxideCapFactor;
            else
                coxSquared = 3.9 * 8.854214871e-12 / 1e-7;
            coxSquared *= coxSquared;

            // Set noise parameters
            MosfetNoise.Generators[RdNoise].SetCoefficients(DrainConductance);
            MosfetNoise.Generators[RsNoise].SetCoefficients(SourceConductance);
            MosfetNoise.Generators[IdNoise].SetCoefficients(2.0 / 3.0 * Math.Abs(Transconductance));
            MosfetNoise.Generators[FlickerNoise].SetCoefficients(
                NoiseParameters.FlickerNoiseCoefficient *
                Math.Exp(NoiseParameters.FlickerNoiseExponent * Math.Log(Math.Max(Math.Abs(DrainCurrent), 1e-38))) /
                (BaseParameters.Width * (BaseParameters.Length - 2 * ModelParameters.LateralDiffusion) *
                 coxSquared) / noise.Frequency);

            // Evaluate noise sources
            MosfetNoise.Evaluate(simulation);
        }
    }
}
