using SpiceSharp.Simulations;

namespace SpiceSharp.Components.NoiseSources
{
    /// <summary>
    /// Noise generator with fixed gain
    /// </summary>
    public class NoiseGain : NoiseGenerator
    {
        /// <summary>
        /// Gets or sets the gain for the noise generator
        /// </summary>
        public double Gain { get; set; }

        /// <summary>
        /// Creates a new instance of the <see cref="NoiseGain"/> class.
        /// </summary>
        /// <param name="name">Name of the noise source</param>
        /// <param name="node1">Node 1</param>
        /// <param name="node2">Node 2</param>
        public NoiseGain(string name, int node1, int node2) : base(name, node1, node2) { }

        /// <summary>
        /// Set the values for the noise source
        /// </summary>
        /// <param name="coefficients">Values</param>
        public override void SetCoefficients(params double[] coefficients)
        {
            coefficients.ThrowIfNot(nameof(coefficients), 1);
            Gain = coefficients[0];
        }

        /// <summary>
        /// Calculate noise coefficient
        /// </summary>
        /// <param name="simulation">Noise simulation</param>
        /// <returns></returns>
        protected override double CalculateNoise(Noise simulation)
        {
            simulation.ThrowIfNull(nameof(simulation));

            var state = simulation.ComplexState;
            var val = state.Solution[Nodes[0]] - state.Solution[Nodes[1]];
            var gain = val.Real * val.Real + val.Imaginary * val.Imaginary;
            return gain * Gain;
        }
    }
}
