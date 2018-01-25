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
        public NoiseGain(string name, int a, int b) : base(name, a, b) { }

        /// <summary>
        /// Set the values for the noise source
        /// </summary>
        /// <param name="values">Values</param>
        public override void Set(params double[] values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));
            Gain = values[0];
        }

        /// <summary>
        /// Calculate noise coefficient
        /// </summary>
        /// <param name="sim">Noise simulation</param>
        /// <returns></returns>
        protected override double CalculateNoise(Noise sim)
        {
            if (sim == null)
                throw new ArgumentNullException(nameof(sim));
            var sol = sim.State.Solution;
            var isol = sim.State.iSolution;
            var rval = sol[NOISEnodes[0]] - sol[NOISEnodes[1]];
            var ival = isol[NOISEnodes[0]] - isol[NOISEnodes[1]];
            double gain = rval * rval + ival * ival;
            return gain * Gain;
        }
    }
}
