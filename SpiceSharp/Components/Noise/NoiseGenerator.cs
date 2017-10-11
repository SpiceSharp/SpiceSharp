using System;
using System.Numerics;
using SpiceSharp.Circuits;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A class that represents a noise generator
    /// </summary>
    public abstract class NoiseGenerator
    {
        /// <summary>
        /// Gets the name of the noise generator
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Get the calculated noise density
        /// </summary>
        public double Noise { get; private set; }

        /// <summary>
        /// Get the log of the calculated noise density
        /// </summary>
        public double LnNoise { get; private set; }

        /// <summary>
        /// Integrated output noise
        /// </summary>
        public double OutNoiz { get; private set; }

        /// <summary>
        /// Integrated input noise
        /// </summary>
        public double InNoiz { get; private set; }

        /// <summary>
        /// Gets node A of the noise source
        /// </summary>
        public int NOISEaNode { get; private set; }

        /// <summary>
        /// Gets node B of the noise source
        /// </summary>
        public int NOISEbNode { get; private set; }

        /// <summary>
        /// Private variables
        /// </summary>
        private int pinA, pinB;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name of the noise source</param>
        /// <param name="a">Pin A</param>
        /// <param name="b">Pin B</param>
        public NoiseGenerator(string name, int a, int b)
        {
            Name = name;
            pinA = a;
            pinB = b;
        }

        /// <summary>
        /// Connect the noise generator in the circuit
        /// </summary>
        /// <param name="node1">Node 1</param>
        /// <param name="node2">Node 2</param>
        public virtual void Setup(Circuit ckt, params int[] pins)
        {
            NOISEaNode = pins[pinA];
            NOISEbNode = pins[pinB];
        }

        /// <summary>
        /// Evaluate
        /// </summary>
        public virtual void Evaluate(Circuit ckt, double param)
        {
            var sol = ckt.State.Complex.Solution;
            var noise = ckt.State.Noise;

            Complex val = sol[NOISEaNode] - sol[NOISEbNode];
            double gain = val.Real * val.Real + val.Imaginary * val.Imaginary;

            // Calculate the noise
            Noise = gain * CalculateNoise(ckt, param);
            double lnNdens = Math.Log(Math.Max(Noise, 1e-38));

            // Initialize the integrated noise if we just started
            if (noise.DelFreq == 0.0)
            {
                LnNoise = lnNdens;
                OutNoiz = 0.0;
                InNoiz = 0.0;
            }
            else
            {
                // Integrate the output noise
                double tempOnoise = noise.Integrate(Noise, lnNdens, LnNoise);
                double tempInoise = noise.Integrate(Noise * noise.GainSqInv, lnNdens + noise.LnGainInv, LnNoise + noise.LnGainInv);
                LnNoise = lnNdens;

                // Add integrated quantity
                OutNoiz += tempOnoise;
                InNoiz += tempInoise;
            }
        }

        /// <summary>
        /// Calculate the noise source quantity
        /// </summary>
        /// <param name="ckt">Circuit</param>
        /// <param name="param">Parameter</param>
        /// <returns></returns>
        protected abstract double CalculateNoise(Circuit ckt, double param);
    }
}
