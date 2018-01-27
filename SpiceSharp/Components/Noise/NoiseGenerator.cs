using System;
using SpiceSharp.Simulations;
using SpiceSharp.Diagnostics;

namespace SpiceSharp.Components.NoiseSources
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
        public double LogNoise { get; private set; }

        /// <summary>
        /// Integrated output noise
        /// </summary>
        public double TotalOutputNoise { get; private set; }

        /// <summary>
        /// Integrated input noise
        /// </summary>
        public double TotalInputNoise { get; private set; }

        /// <summary>
        /// Gets the nodes this noise generator is connected to
        /// </summary>
        public NodeCollection Nodes { get; private set; }

        /// <summary>
        /// Private variables
        /// </summary>
        int[] pins;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name of the noise source</param>
        /// <param name="pins">Pins</param>
        protected NoiseGenerator(string name, params int[] pins)
        {
            Name = name;
            this.pins = pins;
            Nodes = null;
        }

        /// <summary>
        /// Connect the noise generator in the circuit
        /// </summary>
        /// <param name="nodes">Nodes</param>
        public virtual void Setup(params int[] nodes)
        {
            if (nodes == null)
                throw new ArgumentNullException(nameof(nodes));

            // Get the nodes
            int[] mapped = new int[nodes.Length];
            for (int i = 0; i < pins.Length; i++)
            {
                if (pins[i] >= nodes.Length)
                    throw new CircuitException("Not enough pins to find node {0}".FormatString(pins[i]));
                mapped[i] = nodes[pins[i]];
            }
            Nodes = new NodeCollection(mapped);
        }

        public virtual void Unsetup()
        {
            Nodes = null;
        }

        /// <summary>
        /// Set the values for evaluating the noise generator
        /// </summary>
        /// <param name="coefficients">Coefficients</param>
        public abstract void SetCoefficients(params double[] coefficients);

        /// <summary>
        /// Evaluate
        /// </summary>
        public virtual void Evaluate(Noise simulation)
        {
            if (simulation == null)
                throw new ArgumentNullException(nameof(simulation));
            var noise = simulation.NoiseState;

            // Calculate the noise
            Noise = CalculateNoise(simulation);
            double lnNdens = Math.Log(Math.Max(Noise, 1e-38));

            // Initialize the integrated noise if we just started
            if (noise.DelFreq == 0.0)
            {
                LogNoise = lnNdens;
                TotalOutputNoise = 0.0;
                TotalInputNoise = 0.0;
            }
            else
            {
                // Integrate the output noise
                double tempOnoise = noise.Integrate(Noise, lnNdens, LogNoise);
                double tempInoise = noise.Integrate(Noise * noise.GainSqInv, lnNdens + noise.LnGainInv, LogNoise + noise.LnGainInv);
                LogNoise = lnNdens;

                // Add integrated quantity
                TotalOutputNoise += tempOnoise;
                TotalInputNoise += tempInoise;
            }
        }

        /// <summary>
        /// Calculate noise coefficient
        /// </summary>
        /// <param name="simulation">Noise simulation</param>
        /// <returns></returns>
        protected abstract double CalculateNoise(Noise simulation);
    }
}
