using System.Collections;
using System.Collections.Generic;

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
        /// <param name="index">Index</param>
        /// <returns></returns>
        public int this[int index] => _nodes[index];

        /// <summary>
        /// Creates a new instance of the <see cref="NodeCollection"/> class.
        /// </summary>
        /// <param name="nodes">Nodes</param>
        public NodeCollection(IEnumerable<int> nodes)
        {
            nodes.ThrowIfNull(nameof(nodes));

            foreach (var node in nodes)
                _nodes.Add(node);
        }

        /// <summary>
        /// Gets enumerator
        /// </summary>
        /// <returns></returns>
        public IEnumerator<int> GetEnumerator() => _nodes.GetEnumerator();

        /// <summary>
        /// Gets enumerator
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator() => _nodes.GetEnumerator();
    }
}
