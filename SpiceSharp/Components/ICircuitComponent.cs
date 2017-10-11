using SpiceSharp.Circuits;

namespace SpiceSharp.Components
{
    /// <summary>
    /// Represents a circuit object that can be connected in the circuit.
    /// </summary>
    public interface ICircuitComponent : ICircuitObject
    {
        /// <summary>
        /// Get the model of the component
        /// </summary>
        ICircuitObject Model { get; }

        /// <summary>
        /// Get the node count of the component
        /// </summary>
        int PinCount { get; }

        /// <summary>
        /// Connect the component
        /// </summary>
        /// <param name="nodes">Nodes</param>
        void Connect(params CircuitIdentifier[] nodes);

        /// <summary>
        /// Get a connected node name of the node
        /// </summary>
        /// <param name="i">The index</param>
        /// <returns></returns>
        CircuitIdentifier GetNode(int i);

        /// <summary>
        /// Get the index of a node after mapping
        /// This will only be valid if the component is set up
        /// </summary>
        /// <param name="i">The index</param>
        /// <returns></returns>
        int GetNodeIndex(int i);
    }
}
