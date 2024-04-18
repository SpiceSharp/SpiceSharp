using SpiceSharp.Components.CommonBehaviors;
using SpiceSharp.Simulations;
using System;
using System.Numerics;

namespace SpiceSharp.Components.NoiseSources
{
    /// <summary>
    /// A noise source that can be described using shot noise models.
    /// </summary>
    /// <seealso cref="NoiseSource"/>
    public class NoiseShot : NoiseSource
    {
        private readonly OnePort<Complex> _variables;

        /// <summary>
        /// Initializes a new instance of the <see cref="NoiseShot" /> class.
        /// </summary>
        /// <param name="name">Name of the noise source.</param>
        /// <param name="pos">The positive node.</param>
        /// <param name="neg">The negative node.</param>
        public NoiseShot(string name, IVariable<Complex> pos, IVariable<Complex> neg)
            : base(name)
        {
            _variables = new OnePort<Complex>(pos, neg);
        }

        /// <summary>
        /// Computes the noise density of shot noise.
        /// This is equal to 2 * q * I
        /// </summary>
        public void Compute(double current)
        {
            var val = _variables.Positive.Value - _variables.Negative.Value;
            double gain = val.Real * val.Real + val.Imaginary * val.Imaginary;
            OutputNoiseDensity = 2.0 * Constants.Charge * Math.Abs(current) * gain;
        }
    }
}
