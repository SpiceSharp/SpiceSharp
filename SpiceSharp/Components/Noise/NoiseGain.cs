using SpiceSharp.Components.CommonBehaviors;
using SpiceSharp.Simulations;
using System.Numerics;

namespace SpiceSharp.Components.NoiseSources
{
    /// <summary>
    /// Noise generator with fixed gain.
    /// </summary>
    public class NoiseGain : NoiseSource
    {
        private readonly OnePort<Complex> _variables;

        /// <summary>
        /// Initializes a new instance of the <see cref="NoiseGain" /> class.
        /// </summary>
        /// <param name="name">Name of the noise source.</param>
        /// <param name="pos">The positive node.</param>
        /// <param name="neg">The negative node.</param>
        public NoiseGain(string name, IVariable<Complex> pos, IVariable<Complex> neg)
            : base(name)
        {
            _variables = new OnePort<Complex>(pos, neg);
        }

        /// <summary>
        /// Computes the noise density specified gain.
        /// </summary>
        /// <param name="gain">The gain.</param>
        public void Compute(double gain)
        {
            var val = _variables.Positive.Value - _variables.Negative.Value;
            double vgain = val.Real * val.Real + val.Imaginary * val.Imaginary;
            OutputNoiseDensity = vgain * gain;
        }
    }
}
