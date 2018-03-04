using System;
using SpiceSharp.Components.NoiseSources;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.DiodeBehaviors
{
    /// <summary>
    /// Noise behavior for <see cref="Diode"/>
    /// </summary>
    public class NoiseBehavior : Behaviors.NoiseBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        LoadBehavior _load;
        BaseParameters _bp;
        ModelNoiseParameters _mnp;
        ModelTemperatureBehavior _modeltemp;

        /// <summary>
        /// Nodes
        /// </summary>
        int _posNode, _negNode, _posPrimeNode;

        /// <summary>
        /// Noise sources by their index
        /// </summary>
        const int RsNoise = 0;
        const int IdNoise = 1;
        const int FlickerNoise = 2;

        /// <summary>
        /// Noise generators
        /// </summary>
        public ComponentNoise DiodeNoise { get; } = new ComponentNoise(
            new NoiseThermal("rs", 0, 1),
            new NoiseShot("id", 1, 2),
            new NoiseGain("1overf", 1, 2));

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
        /// Connect the noise source
        /// </summary>
        public override void ConnectNoise()
        {
            // Get extra equations
            _posPrimeNode = _load.PosPrimeNode;

            // Connect noise sources
            DiodeNoise.Setup(_posNode, _posPrimeNode, _negNode);
        }
        
        /// <summary>
        /// Connect the behavior
        /// </summary>
        /// <param name="pins">Pins</param>
        public void Connect(params int[] pins)
        {
            if (pins == null)
                throw new ArgumentNullException(nameof(pins));
            if (pins.Length != 2)
                throw new Diagnostics.CircuitException("Pin count mismatch: 2 pins expected, {0} given".FormatString(pins.Length));
            _posNode = pins[0];
            _negNode = pins[1];
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
            DiodeNoise.Generators[RsNoise].SetCoefficients(_modeltemp.Conductance * _bp.Area);
            DiodeNoise.Generators[IdNoise].SetCoefficients(_load.Current);
            DiodeNoise.Generators[FlickerNoise].SetCoefficients(_mnp.FlickerNoiseCoefficient * Math.Exp(_mnp.FlickerNoiseExponent 
                * Math.Log(Math.Max(Math.Abs(_load.Current), 1e-38))) / noise.Frequency);

            // Evaluate noise
            DiodeNoise.Evaluate(simulation);
        }
    }
}
