using SpiceSharp.Simulations.Variables;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Describes an unknown variable that will be solved by a simulation.
    /// </summary>
    public interface IVariable
    {
        /// <summary>
        /// Gets the name of the variable.
        /// </summary>
        /// <value>
        /// The name of the variable.
        /// </value>
        string Name { get; }

        /// <summary>
        /// Gets the SI units of the quantity.
        /// </summary>
        /// <value>
        /// The SI units.
        /// </value>
        IUnit Unit { get; }
    }
}
