using System;

namespace SpiceSharp.Components.Noise
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
        /// Constructor
        /// </summary>
        /// <param name="name">Name of the noise source</param>
        public NoiseShot(string name, int a, int b) : base(name, a, b) { }

        /// <summary>
        /// Set the parameters of the shot noise
        /// </summary>
        /// <param name="values">Values</param>
        public override void Set(params double[] values)
        {
            Current = values[0];
        }

        /// <summary>
        /// Calculate the noise contribution
        /// </summary>
        /// <param name="ckt">Circuit</param>
        /// <param name="param">The DC current in a semiconductor</param>
        /// <returns></returns>
        protected override double CalculateNoise(Circuit ckt)
        {
            var sol = ckt.State.Complex.Solution;
            var val = sol[NOISEnodes[0]] - sol[NOISEnodes[1]];
            double gain = val.Real * val.Real + val.Imaginary * val.Imaginary;
            return 2.0 * Circuit.CHARGE * Math.Abs(Current) * gain;
        }
    }
}
