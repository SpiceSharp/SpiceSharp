using System;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.NoiseSources;
using SpiceSharp.Simulations;
using SpiceSharp.Simulations.Behaviors;

namespace SpiceSharp.Components.MosfetBehaviors.Common
{
    /// <summary>
    /// Common noise behavior for MOS transistors.
    /// </summary>
    public class NoiseBehavior : ExportingBehavior, INoiseBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        private BaseParameters _bp;
        private ModelBaseParameters _mbp;
        private ModelNoiseParameters _mnp;
        private BiasingBehavior _load;
        private TemperatureBehavior _temp;

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
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            // Get parameters
            _bp = provider.GetParameterSet<BaseParameters>();
            _mbp = provider.GetParameterSet<ModelBaseParameters>("model");
            _mnp = provider.GetParameterSet<ModelNoiseParameters>("model");

            // Get behaviors
            _temp = provider.GetBehavior<TemperatureBehavior>();
            _load = provider.GetBehavior<BiasingBehavior>();
        }

        /// <summary>
        /// Connect noise
        /// </summary>
        public void ConnectNoise()
        {
            // Connect noise sources
            MosfetNoise.Setup(
                _load.DrainNode,
                _load.GateNode,
                _load.SourceNode, 
                _load.BulkNode, 
                _load.DrainNodePrime,
                _load.SourceNodePrime);
        }

        /// <summary>
        /// Noise calculations
        /// </summary>
        /// <param name="simulation">Noise simulation</param>
        public void Noise(Noise simulation)
        {
            if (simulation == null)
                throw new ArgumentNullException(nameof(simulation));

            var noise = simulation.NoiseState;

            // Set noise parameters
            MosfetNoise.Generators[RdNoise].SetCoefficients(_temp.DrainConductance);
            MosfetNoise.Generators[RsNoise].SetCoefficients(_temp.SourceConductance);
            MosfetNoise.Generators[IdNoise].SetCoefficients(2.0 / 3.0 * Math.Abs(_load.Transconductance));
            MosfetNoise.Generators[FlickerNoise].SetCoefficients(_mnp.FlickerNoiseCoefficient * Math.Exp(_mnp.FlickerNoiseExponent 
                * Math.Log(Math.Max(Math.Abs(_load.DrainCurrent), 1e-38))) / (_bp.Width * (_bp.Length - 2 * _mbp.LateralDiffusion) 
                * OxideCapSquared) / noise.Frequency);

            // Evaluate noise sources
            MosfetNoise.Evaluate(simulation);
        }

        /// <summary>
        /// Gets the oxide capacitance factor squared.
        /// </summary>
        /// <value>
        /// The oxide capacitance factor squared.
        /// </value>
        protected virtual double OxideCapSquared => _mbp.OxideCapFactor * _mbp.OxideCapFactor;
    }
}
