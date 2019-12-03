using SpiceSharp.Components;

namespace SpiceSharp.Validation
{
    /// <summary>
    /// An <see cref="IRule"/> that checks validity using conductive paths.
    /// </summary>
    /// <seealso cref="IRule" />
    public interface IConductivePathRule : IRule
    {
        /// <summary>
        /// Applies a conductive path between nodes of a component. If no nodes are specified,
        /// then none of the component pins create a conductive path to another node.
        /// </summary>
        /// <param name="component">The component that applies the conductive paths.</param>
        /// <param name="nodes">The nodes that are connected together via a conductive path.</param>
        void AddConductivePath(IComponent component, params string[] nodes);
    }
}
