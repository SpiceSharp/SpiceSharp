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
        List<int> nodes = new List<int>();

        /// <summary>
        /// Gets number of nodes
        /// </summary>
        public int Count => nodes.Count;

        /// <summary>
        /// Gets node
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns></returns>
        public int this[int index]
        {
            get => nodes[index];
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
                this.nodes.Add(node);
        }

        /// <summary>
        /// Gets enumerator
        /// </summary>
        /// <returns></returns>
        public IEnumerator<int> GetEnumerator() => nodes.GetEnumerator();

        /// <summary>
        /// Gets enumerator
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator() => nodes.GetEnumerator();
    }
}
