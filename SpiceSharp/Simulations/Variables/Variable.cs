using System;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A class that describes an unknown variable in a system of equations.
    /// </summary>
    public class Variable
    {
        /// <summary>
        /// Gets the name of the variable.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the node type.
        /// </summary>
        public VariableType UnknownType { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Variable"/> class.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        public Variable(string name)
        {
            Name = name;
            UnknownType = VariableType.Voltage;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Variable"/> class.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <param name="type">The type of variable.</param>
        public Variable(string name, VariableType type)
        {
            Name = name;
            UnknownType = type;
        }

        /// <summary>
        /// Returns a <see cref="String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return "Node {0} ({1})".FormatString(Name, UnknownType);
        }

        /// <summary>
        /// Clones this variable.
        /// </summary>
        /// <returns>A clone of this variable.</returns>
        public Variable Clone() => new Variable(Name, UnknownType);
    }
}
