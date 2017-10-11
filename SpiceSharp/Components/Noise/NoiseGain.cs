namespace SpiceSharp.Components.Noise
{
    /// <summary>
    /// Noise generator with fixed gain
    /// </summary>
    public class NoiseGain : NoiseGenerator
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name of the noise source</param>
        public NoiseGain(string name, int a, int b) : base(name, a, b) { }

        /// <summary>
        /// Calculate the noise contribution
        /// </summary>
        /// <param name="ckt">Circuit</param>
        /// <param name="param">Parameter (unused)</param>
        /// <returns></returns>
        protected override double CalculateNoise(Circuit ckt, double param) => param;
    }
}
