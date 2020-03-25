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
        /// Gets the units of the variable.
        /// </summary>
        /// <value>
        /// The units of the variable.
        /// </value>
        Units Units { get; }
    }
}
