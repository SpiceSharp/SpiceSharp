using System.Numerics;

namespace SpiceSharp.Components.Noise
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
        public NoiseGain(string name, int a, int b) : base(name, a, b) { }

        /// <summary>
        /// Set the values for the noise source
        /// </summary>
        /// <param name="values">Values</param>
        public override void Set(params double[] values)
        {
            Gain = values[0];
        }

        /// <summary>
        /// Calculate the noise contribution
        /// </summary>
        /// <param name="ckt">Circuit</param>
        /// <param name="param">Parameter (unused)</param>
        /// <returns></returns>
        protected override double CalculateNoise(Circuit ckt)
        {
            var sol = ckt.State.Complex.Solution;
            Complex val = sol[NOISEnodes[0]] - sol[NOISEnodes[1]];
            double gain = val.Real * val.Real + val.Imaginary * val.Imaginary;
            return gain * Gain;
        }
    }
}
