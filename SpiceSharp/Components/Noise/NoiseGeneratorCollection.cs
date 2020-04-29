using System.Collections;
using System.Collections.Generic;
using System;

namespace SpiceSharp.Components.NoiseSources
{
    /// <summary>
    /// A collection of noise generators.
    /// </summary>
    /// <seealso cref="IReadOnlyCollection{T}"/>
    /// <seealso cref="NoiseGenerator"/>
    public class NoiseGeneratorCollection : IReadOnlyCollection<NoiseGenerator>
    {
        private readonly List<NoiseGenerator> _generators = new List<NoiseGenerator>();

        /// <summary>
        /// Gets a noise generator at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The noise generator.</returns>
        /// <exception cref="IndexOutOfRangeException">Thrown if <paramref name="index"/> is out of the range 0-<see cref="Count"/>.</exception>
        public NoiseGenerator this[int index] => _generators[index];

        /// <summary>
        /// Gets the number of noise generators.
        /// </summary>
        /// <value>
        /// The number of noise generators.
        /// </value>
        public int Count => _generators.Count;

        /// <summary>
        /// Initializes a new instance of the <see cref="NoiseGeneratorCollection"/> class.
        /// </summary>
        /// <param name="generators">The noise generators.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="generators"/> or any of the generators is <c>null</c>.</exception>
        public NoiseGeneratorCollection(IEnumerable<NoiseGenerator> generators)
        {
            generators.ThrowIfNull(nameof(generators));
            foreach (var generator in generators)
                _generators.Add(generator.ThrowIfNull(nameof(generators)));
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<NoiseGenerator> GetEnumerator() => _generators.GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator() => _generators.GetEnumerator();
    }
}
