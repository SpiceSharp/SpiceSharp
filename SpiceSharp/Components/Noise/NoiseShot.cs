using System;

namespace SpiceSharp.Components.Noise
{
    /// <summary>
    /// Shotnoise generator
    /// </summary>
    public class NoiseShot : NoiseGenerator
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name of the noise source</param>
        public NoiseShot(string name) : base(name) { }

        /// <summary>
        /// Calculate the noise contribution
        /// </summary>
        /// <param name="ckt">Circuit</param>
        /// <param name="param">The DC current in a semiconductor</param>
        /// <returns></returns>
        protected override double CalculateNoise(Circuit ckt, double param)
        {
            return 2.0 * Circuit.CHARGE * Math.Abs(param);
        }
    }
}
