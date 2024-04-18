using SpiceSharp.Components.CommonBehaviors;
using SpiceSharp.Simulations;
using System.Numerics;

namespace SpiceSharp.Components.NoiseSources
{
    /// <summary>
    /// A noise source that can be described by Johnson noise (thermal noise) models.
    /// </summary>
    /// <seealso cref="NoiseSource"/>
    public class NoiseThermal : NoiseSource
    {
        private readonly OnePort<Complex> _variables;

        /// <summary>
        /// Initializes a new instance of the <see cref="NoiseThermal" /> class.
        /// </summary>
        /// <param name="name">Name of the noise source.</param>
        /// <param name="pos">The positive node.</param>
        /// <param name="neg">The negative node.</param>
        public NoiseThermal(string name, IVariable<Complex> pos, IVariable<Complex> neg)
            : base(name)
        {
            _variables = new OnePort<Complex>(pos, neg);
        }

        /// <summary>
        /// Computes the Johnson or thermal noise output density.
        /// This is 4 * k * T * G.
        /// </summary>
        /// <param name="conductance">The conductance.</param>
        /// <param name="temperature">The temperature.</param>
        public void Compute(double conductance, double temperature)
        {
            var val = _variables.Positive.Value - _variables.Negative.Value;
            double gain = val.Real * val.Real + val.Imaginary * val.Imaginary;
            OutputNoiseDensity = 4.0 * Constants.Boltzmann * temperature * conductance * gain;
        }
    }
}
