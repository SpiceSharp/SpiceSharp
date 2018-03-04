using System;
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
        private List<int> _nodes = new List<int>();

        /// <summary>
        /// Gets number of nodes
        /// </summary>
        public int Count => _nodes.Count;

        /// <summary>
        /// Gets node
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns></returns>
        public int this[int index]
        {
            get => _nodes[index];
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="nodes">Nodes</param>
        public NodeCollection(IEnumerable<int> nodes)
        {
            if (nodes == null)
                throw new ArgumentNullException(nameof(nodes));

            foreach (var node in nodes)
                this._nodes.Add(node);
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
