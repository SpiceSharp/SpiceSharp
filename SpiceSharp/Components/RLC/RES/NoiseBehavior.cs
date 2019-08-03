using SpiceSharp.Behaviors;
using SpiceSharp.Components.NoiseSources;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.ResistorBehaviors
{
    /// <summary>
    /// Noise behavior for <see cref="Resistor"/>
    /// </summary>
    public class NoiseBehavior : FrequencyBehavior, INoiseBehavior
    {
        /// <summary>
        /// Gets resistor noise sources
        /// </summary>
        public ComponentNoise ResistorNoise { get; } = new ComponentNoise(new NoiseThermal("thermal", 0, 1));

        /// <summary>
        /// Create a new instance of the <see cref="NoiseBehavior"/> class.
        /// </summary>
        /// <param name="name">Name</param>
        public NoiseBehavior(string name) : base(name) { }

        /// <summary>
        /// Connect the noise
        /// </summary>
        public void ConnectNoise()
        {
            ResistorNoise.Setup(PosNode, NegNode);
        }

        /// <summary>
        /// Noise calculations
        /// </summary>
        /// <param name="simulation">Noise simulation</param>
        public void Noise(Noise simulation)
        {
            ResistorNoise.Generators[0].SetCoefficients(Conductance);
            ResistorNoise.Evaluate(simulation);
        }
    }
}
