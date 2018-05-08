using System;
using SpiceSharp.Algebra;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Describes a node in an electronic circuit.
    /// </summary>
    public class Variable
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
        public VariableType UnknownType { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="index">Row index</param>
        public Variable(Identifier name, int index)
        {
            Name = name;
            UnknownType = VariableType.Voltage;
            Index = index;
        }

        /// <summary>
        /// Constructor
        /// Used by <see cref="VariableSet"/>
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="type">Unknown type</param>
        /// <param name="index">Row index</param>
        public Variable(Identifier name, VariableType type, int index)
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
        public Variable Clone() => new Variable(Name.Clone(), UnknownType, Index);
    }
}
