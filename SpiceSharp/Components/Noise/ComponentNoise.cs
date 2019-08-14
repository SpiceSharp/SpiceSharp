using System;
using System.Collections.Generic;
using SpiceSharp.Components.NoiseSources;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components
{
    /// <summary>
    /// Noise variables
    /// </summary>
    public class ComponentNoise
    {
        /// <summary>
        /// Gets the total output-referred noise density
        /// </summary>
        public double Noise { get; private set; }

        /// <summary>
        /// Gets the log of the total output-referred noise density
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
        /// Gets all generators
        /// </summary>
        public NoiseGeneratorCollection Generators { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="ComponentNoise"/> class.
        /// </summary>
        /// <param name="generators">Names of the generators</param>
        public ComponentNoise(params NoiseGenerator[] generators)
        {
            generators.ThrowIfNull(nameof(generators));
            Generators = new NoiseGeneratorCollection(generators);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ComponentNoise"/> class.
        /// </summary>
        /// <param name="generators"></param>
        public ComponentNoise(IEnumerable<NoiseGenerator> generators)
        {
            generators.ThrowIfNull(nameof(generators));
            Generators = new NoiseGeneratorCollection(generators);
        }

        /// <summary>
        /// Setup the component noise
        /// </summary>
        /// <param name="pins">The pin indices</param>
        public void Setup(params int[] pins)
        {
            foreach (var generator in Generators)
                generator.Setup(pins);
        }

        /// <summary>
        /// Evaluate all noise source contributions
        /// </summary>
        /// <param name="simulation">Noise simulation</param>
        public void Evaluate(Noise simulation)
        {
            simulation.ThrowIfNull(nameof(simulation));
            var noise = simulation.NoiseState;

            // Calculate the output noise density
            Noise = 0.0;
            TotalInNoise = 0.0;
            TotalOutNoise = 0.0;
            foreach (var generator in Generators)
            {
                generator.Evaluate(simulation);
                Noise += generator.Noise;
                TotalInNoise += generator.TotalInputNoise;
                TotalOutNoise += generator.TotalOutputNoise;
            }

            // Log of the output noise density
            LogNoise = Math.Log(Math.Max(Noise, 1e-38));

            // Output noise density
            noise.OutputNoiseDensity += Noise;

            // Integrated input referred noise
            noise.InputNoise += TotalInNoise;

            // Integrated output referred noise
            noise.OutputNoise += TotalOutNoise;
        }
    }
}
