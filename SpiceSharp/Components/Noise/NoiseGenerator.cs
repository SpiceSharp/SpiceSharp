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
        /// Gets the nodes this noise generator is connected to
        /// </summary>
        public int[] NOISEnodes { get; private set; }

        /// <summary>
        /// Private variables
        /// </summary>
        private int[] pins;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name of the noise source</param>
        /// <param name="a">Pin A</param>
        /// <param name="b">Pin B</param>
        public NoiseGenerator(string name, params int[] pins)
        {
            Name = name;
            this.pins = pins;
        }

        /// <summary>
        /// Connect the noise generator in the circuit
        /// </summary>
        /// <param name="nodes">Nodes</param>
        public virtual void Setup(params int[] nodes)
        {
            NOISEnodes = new int[pins.Length];
            for (int i = 0; i < pins.Length; i++)
                NOISEnodes[i] = nodes[pins[i]];
        }

        /// <summary>
        /// Set the values for evaluating the noise generator
        /// </summary>
        /// <param name="values"></param>
        public abstract void Set(params double[] values);

        /// <summary>
        /// Evaluate
        /// </summary>
        public virtual void Evaluate(Circuit ckt)
        {
            var noise = ckt.State.Noise;

            // Calculate the noise
            Noise = CalculateNoise(ckt);
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
        protected abstract double CalculateNoise(Circuit ckt);
    }
}
