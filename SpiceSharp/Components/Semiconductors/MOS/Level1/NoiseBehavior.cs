using System;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.NoiseSources;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.MosfetBehaviors.Level1
{
    /// <summary>
    /// Noise behavior for a <see cref="Mosfet1"/>
    /// </summary>
    public class NoiseBehavior : BaseNoiseBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        private BaseParameters _bp;
        private ModelBaseParameters _mbp;
        private TemperatureBehavior _temp;
        private LoadBehavior _load;
        private ModelTemperatureBehavior _modeltemp;
        private ModelNoiseParameters _mnp;

        /// <summary>
        /// Nodes
        /// </summary>
        private int _drainNode, _gateNode, _sourceNode, _bulkNode, _sourceNodePrime, _drainNodePrime;

        /// <summary>
        /// Noise generators
        /// </summary>
        public ComponentNoise Mosfet1Noise { get; } = new ComponentNoise(
            new NoiseThermal("rd", 0, 4),
            new NoiseThermal("rs", 2, 5),
            new NoiseThermal("id", 4, 5),
            new NoiseGain("1overf", 4, 5)
            );

        private const int RdNoise = 0;
        private const int RsNoise = 1;
        private const int IdNoise = 2;
        private const int FlickerNoise = 3;

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
            _bp = provider.GetParameterSet<BaseParameters>();
            _mbp = provider.GetParameterSet<ModelBaseParameters>("model");
            _mnp = provider.GetParameterSet<ModelNoiseParameters>("model");

            // Get behaviors
            _temp = provider.GetBehavior<TemperatureBehavior>();
            _load = provider.GetBehavior<LoadBehavior>();
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
        /// Connect the noise sources
        /// </summary>
        public override void ConnectNoise()
        {
            // Get extra equations
            _drainNodePrime = _load.DrainNodePrime;
            _sourceNodePrime = _load.SourceNodePrime;

            // Connect noise
            Mosfet1Noise.Setup(_drainNode, _gateNode, _sourceNode, _bulkNode,
                _drainNodePrime, _sourceNodePrime);
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

            double coxSquared;
            if (_modeltemp.OxideCapFactor > 0.0)
                coxSquared = _modeltemp.OxideCapFactor;
            else
                coxSquared = 3.9 * 8.854214871e-12 / 1e-7;
            coxSquared *= coxSquared;

            // Set noise parameters
            Mosfet1Noise.Generators[RdNoise].SetCoefficients(_temp.DrainConductance);
            Mosfet1Noise.Generators[RsNoise].SetCoefficients(_temp.SourceConductance);
            Mosfet1Noise.Generators[IdNoise].SetCoefficients(2.0 / 3.0 * Math.Abs(_load.Transconductance));
            Mosfet1Noise.Generators[FlickerNoise].SetCoefficients(_mnp.FlickerNoiseCoefficient * Math.Exp(_mnp.FlickerNoiseExponent * Math.Log(Math.Max(Math.Abs(_load.DrainCurrent), 1e-38))) 
                / (_bp.Width * (_bp.Length - 2 * _mbp.LateralDiffusion) * coxSquared) / noise.Frequency);

            // Evaluate noise sources
            Mosfet1Noise.Evaluate(simulation);
        }
    }
}
