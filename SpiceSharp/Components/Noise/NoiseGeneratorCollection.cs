using System;
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
        private List<NoiseGenerator> _generators = new List<NoiseGenerator>();

        /// <summary>
        /// Gets a noise generator
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Noise source</returns>
        public NoiseGenerator this[int index]
        {
            get
            {
                return _generators[index];
            }
        }

        /// <summary>
        /// Gets the number of noise generators
        /// </summary>
        public int Count { get => _generators.Count; }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="generators">Generators</param>
        public NoiseGeneratorCollection(IEnumerable<NoiseGenerator> generators)
        {
            if (generators == null)
                throw new ArgumentNullException(nameof(generators));

            foreach (var generator in generators)
                this._generators.Add(generator);
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
