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
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public NoiseGenerator(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Evaluate
        /// </summary>
        public virtual void Evaluate(Circuit ckt, int node1, int node2, double param)
        {
            var sol = ckt.State.Complex.Solution;
            var noise = ckt.State.Noise;

            Complex val = sol[node1] - sol[node2];
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
                noise.outNoiz += tempOnoise;
                noise.inNoise += tempInoise;
            }

            noise.outNdens += Noise;
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
