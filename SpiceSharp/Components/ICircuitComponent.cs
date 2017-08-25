namespace SpiceSharp.Components
{
    /// <summary>
    /// Interface for circuit components
    /// </summary>
    public interface ICircuitComponent : ICircuitObject
    {
        /// <summary>
        /// Get the model of the component
        /// </summary>
        ICircuitObject Model { get; }

        /// <summary>
        /// Connect the component
        /// </summary>
        /// <param name="nodes">Nodes</param>
        void Connect(params string[] nodes);

        /// <summary>
        /// Get a connected node name of the node
        /// </summary>
        /// <param name="i">The index</param>
        /// <returns></returns>
        string GetNode(int i);
    }
}
