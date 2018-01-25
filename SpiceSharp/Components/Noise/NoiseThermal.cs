using System;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.NoiseSources
{
    /// <summary>
    /// Thermal noise generator
    /// </summary>
    public class NoiseThermal : NoiseGenerator
    {
        /// <summary>
        /// Gets or sets the gain of the thermal noise
        /// The noise is 4 * k * T * G
        /// </summary>
        public double Conductance { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name of the noise source</param>
        public NoiseThermal(string name, int a, int b) : base(name, a, b) { }

        /// <summary>
        /// Set the parameters for the thermal noise
        /// </summary>
        /// <param name="values">Values</param>
        public override void Set(params double[] values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));
            Conductance = values[0];
        }

        /// <summary>
        /// Calculate the noise quantity
        /// </summary>
        /// <param name="sim">Noise simulation</param>
        /// <returns></returns>
        protected override double CalculateNoise(Noise sim)
        {
            if (sim == null)
                throw new ArgumentNullException(nameof(sim));

            var state = sim.State;
            var rsol = state.Solution;
            var isol = state.iSolution;
            var rval = rsol[NOISEnodes[0]] - rsol[NOISEnodes[1]];
            var ival = isol[NOISEnodes[0]] - isol[NOISEnodes[1]];
            double gain = rval * rval + ival * ival;
            return 4.0 * Circuit.Boltzmann * sim.State.Temperature * Conductance * gain;
        }
    }
}
