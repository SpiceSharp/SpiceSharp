using System;
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
        /// Constructor
        /// </summary>
        /// <param name="name">Name of the noise source</param>
        public NoiseGain(string name, int node1, int node2) : base(name, node1, node2) { }

        /// <summary>
        /// Set the values for the noise source
        /// </summary>
        /// <param name="coefficients">Values</param>
        public override void SetCoefficients(params double[] coefficients)
        {
            if (coefficients == null)
                throw new ArgumentNullException(nameof(coefficients));
            Gain = coefficients[0];
        }

        /// <summary>
        /// Calculate noise coefficient
        /// </summary>
        /// <param name="simulation">Noise simulation</param>
        /// <returns></returns>
        protected override double CalculateNoise(Noise simulation)
        {
            if (simulation == null)
                throw new ArgumentNullException(nameof(simulation));
            var sol = simulation.State.Solution;
            var isol = simulation.State.iSolution;
            var rval = sol[Nodes[0]] - sol[Nodes[1]];
            var ival = isol[Nodes[0]] - isol[Nodes[1]];
            double gain = rval * rval + ival * ival;
            return gain * Gain;
        }
    }
}
