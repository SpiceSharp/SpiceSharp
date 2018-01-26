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
        public double LogNoise { get; private set; }

        /// <summary>
        /// Gets the total integrated output-referred noise
        /// </summary>
        public double TotalOutNoise { get; private set; }

        /// <summary>
        /// Gets the total integrated input-referred noise
        /// </summary>
        public double TotalInNoise { get; private set; }

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
            if (generators == null)
                throw new ArgumentNullException(nameof(generators));
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
        /// <param name="simulation">Noise simulation</param>
        public void Evaluate(Noise simulation)
        {
            if (simulation == null)
                throw new ArgumentNullException(nameof(simulation));
            var noise = simulation.NoiseState;

            // Calculate the output noise density
            Noise = 0.0;
            TotalInNoise = 0.0;
            TotalOutNoise = 0.0;
            for (int i = 0; i < Generators.Length; i++)
            {
                Generators[i].Evaluate(simulation);
                Noise += Generators[i].Noise;
                TotalInNoise += Generators[i].InNoiz;
                TotalOutNoise += Generators[i].OutNoiz;
            }

            // Log of the output noise density
            LogNoise = Math.Log(Math.Max(Noise, 1e-38));

            // Output noise density
            noise.outNdens += Noise;

            // Integrated input referred noise
            noise.inNoise += TotalInNoise;

            // Integrated output referred noise
            noise.outNoiz += TotalOutNoise;
        }
    }
}
