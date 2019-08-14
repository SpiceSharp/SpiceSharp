using System.Collections;
using System.Collections.Generic;

namespace SpiceSharp.Components.NoiseSources
{
    /// <summary>
    /// Collection of noise generators
    /// </summary>
    public class NoiseGeneratorCollection : IReadOnlyCollection<NoiseGenerator>
    {
        /// <summary>
        /// Generators
        /// </summary>
        private readonly List<NoiseGenerator> _generators = new List<NoiseGenerator>();

        /// <summary>
        /// Gets a noise generator
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Noise source</returns>
        public NoiseGenerator this[int index] => _generators[index];

        /// <summary>
        /// Gets the number of noise generators
        /// </summary>
        public int Count => _generators.Count;

        /// <summary>
        /// Creates a new instance of the <see cref="NoiseGeneratorCollection"/> class.
        /// </summary>
        /// <param name="generators">Generators</param>
        public NoiseGeneratorCollection(IEnumerable<NoiseGenerator> generators)
        {
            generators.ThrowIfNull(nameof(generators));

            foreach (var generator in generators)
                _generators.Add(generator);
        }

        /// <summary>
        /// Gets enumerator
        /// </summary>
        /// <returns></returns>
        public IEnumerator<NoiseGenerator> GetEnumerator() => _generators.GetEnumerator();

        /// <summary>
        /// Gets enumerator
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator() => _generators.GetEnumerator();
    }
}
