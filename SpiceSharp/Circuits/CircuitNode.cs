using SpiceSharp.Sparse;

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
        public CircuitIdentifier Name { get; }

        /// <summary>
        /// Gets the index of the node
        /// This is also the row index in the state matrix
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// Gets the diagonal matrix element associated with the node
        /// </summary>
        public MatrixElement Diagonal { get; set; }

        /// <summary>
        /// Gets the node type
        /// </summary>
        public NodeType Type { get; }

        /// <summary>
        /// Constructor
        /// Used by <see cref="CircuitNodes"/>
        /// </summary>
        /// <param name="type">The type of node</param>
        /// <param name="index">The row index</param>
        public CircuitNode(CircuitIdentifier name, NodeType type, int index = 0)
        {
            Name = name;
            Type = type;
            Index = index;
        }

        /// <summary>
        /// Nice format for nodes
        /// </summary>
        /// <returns></returns>
        public override string ToString() => "Node " + Name.ToString();
    }
}
