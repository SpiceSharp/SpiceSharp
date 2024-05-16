using System.Collections;
using System.Collections.Generic;

namespace SpiceSharpGenerator
{
    /// <summary>
    /// Describes a collection of <see cref="GeneratedProperty"/>.
    /// </summary>
    public class GeneratedPropertyCollection : IEnumerable<GeneratedProperty>
    {
        private readonly List<GeneratedProperty> _properties = new(4);

        /// <summary>
        /// Adds the generated property.
        /// </summary>
        /// <param name="property">The property.</param>
        public void Add(GeneratedProperty property)
        {
            _properties.Add(property);
        }

        /// <summary>
        /// Clears the generated properties.
        /// </summary>
        public void Clear() => _properties.Clear();

        /// <inheritdoc/>
        public IEnumerator<GeneratedProperty> GetEnumerator() => _properties.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
