using System;
using SpiceSharp.Simulations;
using SpiceSharp.Components.NoiseSources;

namespace SpiceSharp.Components
{
    /// <summary>
    /// Noise variables
    /// </summary>
    public class ComponentNoise
    {
        /// <summary>
        /// Get the total output-referred noise density
        /// </summary>
        public double Noise { get; private set; }

        /// <summary>
        /// Get the log of the total output-referred noise density
        /// </summary>
        public double LnNoise { get; private set; }

        /// <summary>
        /// Gets the total integrated output-referred noise
        /// </summary>
        public double OutNoiz { get; private set; }

        /// <summary>
        /// Gets the total integrated input-referred noise
        /// </summary>
        public double InNoiz { get; private set; }

        /// <summary>
        /// Get all generators
        /// </summary>
        public NoiseGenerator[] Generators { get; }

        /// <summary>
        /// Constructor
        /// One generator is always added at the end for the total noise density
        /// </summary>
        /// <param name="generators">Names of the generators</param>
        public ComponentNoise(params NoiseGenerator[] generators)
        {
            Generators = new NoiseGenerator[generators.Length];
            for (int i = 0; i < generators.Length; i++)
                Generators[i] = generators[i];
        }

        /// <summary>
        /// Setup the component noise
        /// </summary>
        /// <param name="pins">The pin indices</param>
        public void Setup(params int[] pins)
        {
            for (int i = 0; i < Generators.Length; i++)
                Generators[i].Setup(pins);
        }

        /// <summary>
        /// Evaluate all noise source contributions
        /// </summary>
        /// <param name="sim">Noise simulation</param>
        public void Evaluate(Noise sim)
        {
            var noise = sim.State.Noise;

            // Calculate the output noise density
            Noise = 0.0;
            InNoiz = 0.0;
            OutNoiz = 0.0;
            for (int i = 0; i < Generators.Length; i++)
            {
                Generators[i].Evaluate(sim);
                Noise += Generators[i].Noise;
                InNoiz += Generators[i].InNoiz;
                OutNoiz += Generators[i].OutNoiz;
            }

            // Log of the output noise density
            LnNoise = Math.Log(Math.Max(Noise, 1e-38));

            // Output noise density
            noise.outNdens += Noise;

            // Integrated input referred noise
            noise.inNoise += InNoiz;

            // Integrated output referred noise
            noise.outNoiz += OutNoiz;
        }
    }
}
