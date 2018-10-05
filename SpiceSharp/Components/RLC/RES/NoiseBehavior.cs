using System;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.NoiseSources;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.ResistorBehaviors
{
    /// <summary>
    /// Noise behavior for <see cref="Resistor"/>
    /// </summary>
    public class NoiseBehavior : BaseNoiseBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        private TemperatureBehavior _temp;

        /// <summary>
        /// Nodes
        /// </summary>
        private int _posNode, _negNode;

        /// <summary>
        /// Gets resistor noise sources
        /// </summary>
        public ComponentNoise ResistorNoise { get; } = new ComponentNoise(new NoiseThermal("thermal", 0, 1));

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public NoiseBehavior(string name) : base(name) { }

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="simulation">Simulation</param>
        /// <param name="provider">Data provider</param>
        public override void Setup(Simulation simulation, SetupDataProvider provider)
        {
			if (provider == null)
				throw new ArgumentNullException(nameof(provider));

            // Get behaviors
            _temp = provider.GetBehavior<TemperatureBehavior>();
        }
        
        /// <summary>
        /// Connect the noise
        /// </summary>
        public void Connect(params int[] pins)
        {
            if (pins == null)
                throw new ArgumentNullException(nameof(pins));
            if (pins.Length != 2)
                throw new CircuitException("Pin count mismatch: 2 pins expected, {0} given".FormatString(pins.Length));
            _posNode = pins[0];
            _negNode = pins[1];
        }

        /// <summary>
        /// Connect the noise
        /// </summary>
        public override void ConnectNoise()
        {
            ResistorNoise.Setup(_posNode, _negNode);
        }

        /// <summary>
        /// Noise calculations
        /// </summary>
        /// <param name="simulation">Noise simulation</param>
        public override void Noise(Noise simulation)
        {
            ResistorNoise.Generators[0].SetCoefficients(_temp.Conductance);
            ResistorNoise.Evaluate(simulation);
        }
    }
}
