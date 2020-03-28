using System.Collections.Generic;

namespace SpiceSharp.Components.SubcircuitBehaviors
{
    /// <summary>
    /// The default implementation of a subcircuit node mapper.
    /// </summary>
    /// <seealso cref="ISubcircuitNodeMapper" />
    public class SubcircuitNodeMapper : ISubcircuitNodeMapper
    {
        private readonly string _subckt;
        private readonly Dictionary<string, string> _map;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubcircuitNodeMapper"/> class.
        /// </summary>
        /// <param name="subcircuitName">Name of the subcircuit.</param>
        /// <param name="nodeComparer">The comparer used for comparing node names.</param>
        public SubcircuitNodeMapper(string subcircuitName, IEqualityComparer<string> nodeComparer)
        {
            _subckt = subcircuitName.ThrowIfNull(nameof(subcircuitName));
            _map = new Dictionary<string, string>(nodeComparer);
        }

        /// <summary>
        /// Internals to external.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The mapped node name.</returns>
        public string Map(string name)
        {
            if (_map.TryGetValue(name, out var result))
                return result;
            return _subckt.Combine(name);
        }

        /// <summary>
        /// Adds the specified internal node to an external one.
        /// </summary>
        /// <param name="internalNode">The internal node.</param>
        /// <param name="externalNode">The external node.</param>
        public void Add(string internalNode, string externalNode)
        {
            _map.Add(internalNode, externalNode);
        }
    }
}
