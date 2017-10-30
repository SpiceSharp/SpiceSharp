using System;
using System.Numerics;

namespace SpiceSharp.Components.Noise
{
    /// <summary>
    /// Noise generator for the BSIM4v80
    /// </summary>
    public class NoiseThermal2 : NoiseGenerator
    {
        /// <summary>
        /// Gain^2 A
        /// </summary>
        public double Param1 { get; set; }

        /// <summary>
        /// Gain^2 B
        /// </summary>
        public double Param2 { get; set; }

        /// <summary>
        /// Phase of signal 2 relative to signal 1
        /// </summary>
        public double Phi21 { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Noise source name</param>
        /// <param name="a">Pin A</param>
        /// <param name="b">Pin B</param>
        /// <param name="c">Pin C</param>
        /// <param name="d">Pin D</param>
        public NoiseThermal2(string name, int a, int b, int c, int d)
            : base(name, a, b, c, d)
        {
        }

        /// <summary>
        /// Set the noise parameters
        /// </summary>
        /// <param name="values">Values</param>
        public override void Set(params double[] values)
        {
            Param1 = values[0];
            Param2 = values[1];
            Phi21 = values[2];
        }

        /// <summary>
        /// Calculate the noise
        /// </summary>
        /// <param name="ckt">Circuit</param>
        /// <returns></returns>
        protected override double CalculateNoise(Circuit ckt)
        {
            var sol = ckt.State.Solution;
            var val1 = sol[NOISEnodes[0]] - sol[NOISEnodes[1]];
            var val2 = sol[NOISEnodes[2]] - sol[NOISEnodes[3]];

            double T0 = Math.Sqrt(Param1);
            Complex T1 = Math.Sqrt(Param2) * Complex.Exp(new Complex(0.0, Phi21));
            var Out = T0 * val1 + T1 * val2;

            double param_gain = Out.Real * Out.Real + Out.Imaginary * Out.Imaginary;
            return 4.0 * Circuit.CONSTBoltz * ckt.State.Temperature * param_gain;
        }
    }
}
