using System.Collections;
using System.Collections.Generic;
using System;

namespace SpiceSharp.Components.NoiseSources
{
    /// <summary>
    /// Collection of nodes (used for noise generators)
    /// </summary>
    public class NodeCollection : IReadOnlyCollection<int>
    {
        /// <summary>
        /// Nodes in the collection
        /// </summary>
        private readonly List<int> _nodes = new List<int>();

        /// <summary>
        /// Gets number of nodes
        /// </summary>
        public int Count => _nodes.Count;

        /// <summary>
        /// Gets node
        /// </summary>
        /// <param name="index">Index.</param>
        /// <returns>The node index.</returns>
        /// <exception cref="IndexOutOfRangeException">Thrown if <paramref name="index"/> is out of range.</exception>
        public int this[int index] => _nodes[index];

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeCollection"/> class.
        /// </summary>
        /// <param name="nodes">The nodes.</param>
        public NodeCollection(IEnumerable<int> nodes)
        {
            nodes.ThrowIfNull(nameof(nodes));

            foreach (var node in nodes)
                _nodes.Add(node);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<int> GetEnumerator() => _nodes.GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator() => _nodes.GetEnumerator();
    }
}
