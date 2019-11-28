using System;

namespace SpiceSharp.Components.NoiseSources
{
    /// <summary>
    /// Shotnoise generator
    /// </summary>
    public class NoiseShot : NoiseGenerator
    {
        /// <summary>
        /// Gets or sets the gain of the shot noise
        /// The noise will be 2 * q * Gain
        /// </summary>
        public double Current { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NoiseShot"/> class.
        /// </summary>
        /// <param name="name">Name of the noise source</param>
        /// <param name="node1">Node 1</param>
        /// <param name="node2">Node 2</param>
        public NoiseShot(string name, int node1, int node2) : base(name, node1, node2) { }

        /// <summary>
        /// Set the parameters of the shot noise
        /// </summary>
        /// <param name="coefficients">Values</param>
        public override void SetCoefficients(params double[] coefficients)
        {
            if (coefficients == null || coefficients.Length != 1)
                throw new BadParameterException(nameof(coefficients));
            Current = coefficients[0];
        }

        /// <summary>
        /// Calculates the noise contributions.
        /// </summary>
        /// <returns></returns>
        protected override double CalculateNoise()
        {
            var val = ComplexState.Solution[Nodes[0]] - ComplexState.Solution[Nodes[1]];
            var gain = val.Real * val.Real + val.Imaginary * val.Imaginary;
            return 2.0 * Constants.Charge * Math.Abs(Current) * gain;
        }
    }
}
