using System;

namespace SpiceSharp.Components.Noise
{
    /// <summary>
    /// Thermal noise generator
    /// </summary>
    public class NoiseThermal : NoiseGenerator
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name of the noise source</param>
        public NoiseThermal(string name) : base(name) { }

        /// <summary>
        /// Calculate the noise quantity
        /// </summary>
        /// <param name="ckt">Circuit</param>
        /// <param name="param">The conductance of a resistor</param>
        /// <returns></returns>
        protected override double CalculateNoise(Circuit ckt, double param)
        {
            return 4.0 * Circuit.CONSTBoltz * ckt.State.Temperature * param;
        }
    }
}
