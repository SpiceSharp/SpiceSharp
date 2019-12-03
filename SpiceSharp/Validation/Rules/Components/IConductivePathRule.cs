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
        /// Applies a conductive path between nodes.
        /// </summary>
        /// <param name="component">The component that applies the conductive paths.</param>
        /// <param name="nodes">The nodes that are connected together via a conductive path.</param>
        void AddConductivePath(IComponent component, params string[] nodes);

        /// <summary>
        /// Specifies a component that does not have a conductive path between any nodes.
        /// </summary>
        /// <param name="component">The component.</param>
        void NoConductivePath(IComponent component);
    }
}
