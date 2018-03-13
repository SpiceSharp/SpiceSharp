using System;
using SpiceSharp.Algebra;

namespace SpiceSharp.Circuits
{
    /// <summary>
    /// Describes a node in an electronic circuit.
    /// </summary>
    public class Node : ICloneable
    {
        /// <summary>
        /// Enumeration of unknown types
        /// </summary>
        public enum NodeType
        {
            /// <summary>
            /// The unknown associated with this node does not fall into a category
            /// </summary>
            None = 0x00,

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
        public Identifier Name { get; }

        /// <summary>
        /// Gets the index of the node
        /// This is also the row index in the state matrix
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// Gets or sets the diagonal matrix element associated with the node
        /// </summary>
        public MatrixElement<double> Diagonal { get; set; }

        /// <summary>
        /// Gets the node type
        /// </summary>
        public NodeType UnknownType { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="index">Row index</param>
        public Node(Identifier name, int index)
        {
            Name = name;
            UnknownType = NodeType.Voltage;
            Index = index;
        }

        /// <summary>
        /// Constructor
        /// Used by <see cref="NodeMap"/>
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="type">Unknown type</param>
        /// <param name="index">Row index</param>
        public Node(Identifier name, NodeType type, int index)
        {
            Name = name;
            UnknownType = type;
            Index = index;
        }

        /// <summary>
        /// Nice format for nodes
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "Node {0}".FormatString(Name);
        }

        /// <summary>
        /// Clone the node
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            var node = new Node(Name, UnknownType, Index);
            return node;
        }
    }
}
