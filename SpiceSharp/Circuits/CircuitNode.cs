namespace SpiceSharp.Circuits
{
    /// <summary>
    /// Describes a node in an electronic circuit.
    /// </summary>
    public class CircuitNode
    {
        /// <summary>
        /// Enumeration of unknown types
        /// </summary>
        public enum NodeType
        {
            /// <summary>
            /// The unknown associated with this node is a voltage
            /// </summary>
            Voltage = 0x03,

            /// <summary>
            /// The unknown associated with this node is a current
            /// </summary>
            Current = 0x04
        }

        /// <summary>
        /// Gets or sets the name of the node
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the index of the node
        /// This is also the row index in the state matrix
        /// </summary>
        public int Index { get; private set; }

        /// <summary>
        /// Gets the node type
        /// </summary>
        public NodeType Type { get; private set; }

        /// <summary>
        /// Constructor
        /// Used by <see cref="Nodes"/>
        /// </summary>
        /// <param name="type">The type of node</param>
        /// <param name="index">The row index</param>
        public CircuitNode(NodeType type, int index = 0)
        {
            Type = type;
            Index = index;
        }

        /// <summary>
        /// Nice format for nodes
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (Name != null)
                return "Node (" + Name + ")";
            else
                return "Created node (" + Type.ToString() + ")";
        }
    }
}
