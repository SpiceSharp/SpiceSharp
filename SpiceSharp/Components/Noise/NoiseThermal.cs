using SpiceSharp.Simulations;

namespace SpiceSharp.Components.NoiseSources
{
    /// <summary>
    /// Thermal noise generator
    /// </summary>
    public class NoiseThermal : NoiseGenerator
    {
        /// <summary>
        /// The second node of the noise source.
        /// </summary>
        public int Node2 { get; }

        /// <summary>
        /// Gets or sets the gain of the thermal noise
        /// The noise is 4 * k * T * G
        /// </summary>
        public double Conductance { get; set; }

        /// <summary>
        /// Creates a new instance of the <see cref="NoiseThermal"/> class.
        /// </summary>
        /// <param name="name">Name of the noise source</param>
        /// <param name="node1">Node 1</param>
        /// <param name="node2">Node 2</param>
        public NoiseThermal(string name, int node1, int node2) : base(name, node1, node2)
        {
            Node2 = node2;
        }

        /// <summary>
        /// Set the parameters for the thermal noise
        /// </summary>
        /// <param name="coefficients">Values</param>
        public override void SetCoefficients(params double[] coefficients)
        {
            coefficients.ThrowIfNot(nameof(coefficients), 1);
            Conductance = coefficients[0];
        }

        /// <summary>
        /// Calculate the noise quantity
        /// </summary>
        /// <param name="simulation">Noise simulation</param>
        /// <returns></returns>
        protected override double CalculateNoise(Noise simulation)
        {
            simulation.ThrowIfNull(nameof(simulation));

            var state = simulation.ComplexState;
            var val = state.Solution[Nodes[0]] - state.Solution[Nodes[1]];
            var gain = val.Real * val.Real + val.Imaginary * val.Imaginary;
            return 4.0 * Constants.Boltzmann * simulation.RealState.Temperature * Conductance * gain;
        }
    }
}
