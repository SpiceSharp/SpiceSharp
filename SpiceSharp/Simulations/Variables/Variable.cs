using SpiceSharp.Algebra;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A class that describes an unknown variable in a system of equations.
    /// </summary>
    public class Variable
    {
        /// <summary>
        /// Gets the identifier of the variable.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the index of the node.
        /// </summary>
        /// <remarks>
        /// This index is typically used as the row index for the KCL law of this node voltage.
        /// </remarks>
        public int Index { get; }

        /// <summary>
        /// Gets or sets the diagonal matrix element associated with the node.
        /// </summary>
        /// <remarks>
        /// This variable is used by simulations to aid convergence in specific situations.
        /// </remarks>
        public MatrixElement<double> Diagonal { get; set; }

        /// <summary>
        /// Gets the node type.
        /// </summary>
        public VariableType UnknownType { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Variable"/> class.
        /// </summary>
        /// <param name="name">The identifier of the variable.</param>
        /// <param name="index">The index of the unknown variable.</param>
        public Variable(string name, int index)
        {
            Name = name;
            UnknownType = VariableType.Voltage;
            Index = index;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Variable"/> class.
        /// </summary>
        /// <param name="name">The identifier of the variable.</param>
        /// <param name="type">The type of variable.</param>
        /// <param name="index">The index of the unknown variable.</param>
        public Variable(string name, VariableType type, int index)
        {
            Name = name;
            UnknownType = type;
            Index = index;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return "Node {0} ({1})".FormatString(Name, UnknownType);
        }

        /// <summary>
        /// Clones this variable.
        /// </summary>
        /// <returns>A clone of this variable.</returns>
        public Variable Clone() => new Variable(Name, UnknownType, Index);
    }
}
