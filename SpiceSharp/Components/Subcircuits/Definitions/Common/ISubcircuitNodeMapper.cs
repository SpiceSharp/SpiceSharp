namespace SpiceSharp.Components.SubcircuitBehaviors
{
    /// <summary>
    /// Template for a node mapper for a subcircuit.
    /// </summary>
    public interface ISubcircuitNodeMapper
    {
        /// <summary>
        /// Internals to external.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        string Map(string name);

        /// <summary>
        /// Adds the specified internal node to an external one.
        /// </summary>
        /// <param name="internalNode">The internal node.</param>
        /// <param name="externalNode">The external node.</param>
        void Add(string internalNode, string externalNode);
    }
}
