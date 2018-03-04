using System;
using SpiceSharp.Components.NoiseSources;
using SpiceSharp.Simulations;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.BipolarBehaviors
{
    /// <summary>
    /// Noise behavior for <see cref="BipolarJunctionTransistor"/>
    /// </summary>
    public class NoiseBehavior : Behaviors.NoiseBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        private BaseParameters _bp;
        private LoadBehavior _load;
        private ModelNoiseParameters _mnp;
        private ModelTemperatureBehavior _modeltemp;

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
        /// Nodes
        /// </summary>
        private int _collectorNode, _baseNode, _emitterNode, _substrateNode, _colPrimeNode, _basePrimeNode, _emitPrimeNode;

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
            _mnp = provider.GetParameterSet<ModelNoiseParameters>("model");

            // Get behaviors
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
                throw new Diagnostics.CircuitException("Pin count mismatch: 4 pins expected, {0} given".FormatString(pins.Length));
            _collectorNode = pins[0];
            _baseNode = pins[1];
            _emitterNode = pins[2];
            _substrateNode = pins[3];
        }

        /// <summary>
        /// Connect noise sources
        /// </summary>
        public override void ConnectNoise()
        {
            // Get extra nodes
            _colPrimeNode = _load.CollectorPrimeNode;
            _basePrimeNode = _load.BasePrimeNode;
            _emitPrimeNode = _load.EmitterPrimeNode;

            // Connect noise
            BipolarJunctionTransistorNoise.Setup(_collectorNode, _baseNode, _emitterNode, _substrateNode,
                _colPrimeNode, _basePrimeNode, _emitPrimeNode);
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
            var generators = BipolarJunctionTransistorNoise.Generators;

            // Set noise parameters
            generators[RcNoise].SetCoefficients(_modeltemp.CollectorConduct * _bp.Area);
            generators[RbNoise].SetCoefficients(_load.ConductanceX);
            generators[ReNoise].SetCoefficients(_modeltemp.EmitterConduct * _bp.Area);
            generators[IcNoise].SetCoefficients(_load.CollectorCurrent);
            generators[IbNoise].SetCoefficients(_load.BaseCurrent);
            generators[FlickerNoise].SetCoefficients(_mnp.FlickerNoiseCoefficient * Math.Exp(_mnp.FlickerNoiseExponent 
                * Math.Log(Math.Max(Math.Abs(_load.BaseCurrent), 1e-38))) / noise.Frequency);

            // Evaluate all noise sources
            BipolarJunctionTransistorNoise.Evaluate(simulation);
        }
    }
}
