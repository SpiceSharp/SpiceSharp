using System;
using System.Numerics;
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
        public NoiseThermal(string name, int node1, int node2) : base(name, node1, node2) { }

        /// <summary>
        /// Set the parameters for the thermal noise
        /// </summary>
        /// <param name="coefficients">Values</param>
        public override void SetCoefficients(params double[] coefficients)
        {
            if (coefficients == null)
                throw new ArgumentNullException(nameof(coefficients));
            Conductance = coefficients[0];
        }

        /// <summary>
        /// Calculate the noise quantity
        /// </summary>
        /// <param name="simulation">Noise simulation</param>
        /// <returns></returns>
        protected override double CalculateNoise(Noise simulation)
        {
            if (simulation == null)
                throw new ArgumentNullException(nameof(simulation));

            var state = simulation.ComplexState;
            Complex val = state.Solution[Nodes[0]] - state.Solution[Nodes[1]];
            double gain = val.Real * val.Real + val.Imaginary * val.Imaginary;
            return 4.0 * Circuit.Boltzmann * simulation.State.Temperature * Conductance * gain;
        }
    }
}
