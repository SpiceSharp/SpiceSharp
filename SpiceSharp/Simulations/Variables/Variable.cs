using SpiceSharp.Simulations.Variables;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A class that describes an unknown variable in a system of equations.
    /// </summary>
    public class Variable : IVariable
    {
        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc/>
        public IUnit Unit { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Variable"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="unit">The unit.</param>
        public Variable(string name, IUnit unit)
        {
            Name = name;
            Unit = unit;
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return "Node {0} ({1})".FormatString(Name, Unit);
        }
    }
}
