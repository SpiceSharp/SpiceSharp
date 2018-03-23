using System;
using SpiceSharp.Algebra;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Describes a node in an electronic circuit.
    /// </summary>
    public class Unknown : ICloneable
    {
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
        public UnknownType UnknownType { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="index">Row index</param>
        public Unknown(Identifier name, int index)
        {
            Name = name;
            UnknownType = UnknownType.Voltage;
            Index = index;
        }

        /// <summary>
        /// Constructor
        /// Used by <see cref="UnknownCollection"/>
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="type">Unknown type</param>
        /// <param name="index">Row index</param>
        public Unknown(Identifier name, UnknownType type, int index)
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
            return "Node {0} ({1})".FormatString(Name, UnknownType);
        }

        /// <summary>
        /// Clone the node
        /// </summary>
        /// <returns></returns>
        public Unknown Clone() => new Unknown(Name.Clone(), UnknownType, Index);

        /// <summary>
        /// Clone the node
        /// </summary>
        /// <returns></returns>
        object ICloneable.Clone() => Clone();
    }
}
