using System;
using SpiceSharp.Simulations;

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
        /// <param name="sim">Noise simulation</param>
        /// <returns></returns>
        protected override double CalculateNoise(Noise sim)
        {
            var state = sim.State;
            var sol = state.Solution;
            var isol = state.iSolution;
            var rval = sol[NOISEnodes[0]] - sol[NOISEnodes[1]];
            var ival = isol[NOISEnodes[0]] - isol[NOISEnodes[1]];
            double gain = rval * rval + ival * ival;
            return 2.0 * Circuit.Charge * Math.Abs(Current) * gain;
        }
    }
}
