using System;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.NoiseSources;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.MosfetBehaviors.Level2
{
    /// <summary>
    /// Noise behavior for <see cref="Mosfet2"/>
    /// </summary>
    public class NoiseBehavior : Behaviors.NoiseBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        private BaseParameters _bp;
        private ModelBaseParameters _mbp;
        private ModelNoiseParameters _mnp;
        private LoadBehavior _load;
        private TemperatureBehavior _temp;
        private ModelTemperatureBehavior _modeltemp;

        /// <summary>
        /// Noise generators by their index
        /// </summary>
        private const int RdNoise = 0;
        private const int RsNoise = 1;
        private const int IdNoise = 2;
        private const int FlickerNoise = 3;

        /// <summary>
        /// Nodes
        /// </summary>
        private int _drainNode, _gateNode, _sourceNode, _bulkNode, _sourceNodePrime, _drainNodePrime;

        /// <summary>
        /// Noise generators
        /// </summary>
        public ComponentNoise Mosfet2Noise { get; } = new ComponentNoise(
            new NoiseThermal("rd", 0, 4),
            new NoiseThermal("rs", 2, 5),
            new NoiseThermal("id", 4, 5),
            new NoiseGain("1overf", 4, 5)
            );

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public NoiseBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Setup behavior
        /// </summary>
        /// <param name="provider">Data provider</param>
        public override void Setup(SetupDataProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            // Get parameters
            _bp = provider.GetParameterSet<BaseParameters>("entity");
            _mbp = provider.GetParameterSet<ModelBaseParameters>("model");
            _mnp = provider.GetParameterSet<ModelNoiseParameters>("model");

            // Get behaviors
            _temp = provider.GetBehavior<TemperatureBehavior>("entity");
            _load = provider.GetBehavior<LoadBehavior>("entity");
            _modeltemp = provider.GetBehavior<ModelTemperatureBehavior>("model");
        }

        /// <summary>
        /// Connect
        /// </summary>
        /// <param name="pins">Pins</param>
        public void Connect(params int[] pins)
        {
            if (pins == null)
                throw new ArgumentNullException(nameof(pins));
            if (pins.Length != 4)
                throw new CircuitException("Pin count mismatch: 4 pins expected, {0} given".FormatString(pins.Length));
            _drainNode = pins[0];
            _gateNode = pins[1];
            _sourceNode = pins[2];
            _bulkNode = pins[3];
        }

        /// <summary>
        /// Connect noise sources
        /// </summary>
        public override void ConnectNoise()
        {
            // Get extra equations
            _drainNodePrime = _load.DrainNodePrime;
            _sourceNodePrime = _load.SourceNodePrime;

            // Connect noise source
            Mosfet2Noise.Setup(_drainNode, _gateNode, _sourceNode, _bulkNode, _drainNodePrime, _sourceNodePrime);
        }

        /// <summary>
        /// Noise calculations
        /// </summary>
        /// <param name="simulation">Noise simulation</param>
        public override void Noise(Noise simulation)
        {
            if (simulation == null)
                throw new ArgumentNullException(nameof(simulation));

            var noise = simulation.NoiseState;

            // Set noise parameters
            Mosfet2Noise.Generators[RdNoise].SetCoefficients(_temp.DrainConductance);
            Mosfet2Noise.Generators[RsNoise].SetCoefficients(_temp.SourceConductance);
            Mosfet2Noise.Generators[IdNoise].SetCoefficients(2.0 / 3.0 * Math.Abs(_load.Transconductance));
            Mosfet2Noise.Generators[FlickerNoise].SetCoefficients(_mnp.FlickerNoiseCoefficient * Math.Exp(_mnp.FlickerNoiseExponent * Math.Log(Math.Max(Math.Abs(_load.DrainCurrent), 1e-38))) 
                / (_bp.Width * (_bp.Length - 2 * _mbp.LateralDiffusion) * _mbp.OxideCapFactor * _mbp.OxideCapFactor) / noise.Frequency);

            // Evaluate noise sources
            Mosfet2Noise.Evaluate(simulation);
        }
    }
}
