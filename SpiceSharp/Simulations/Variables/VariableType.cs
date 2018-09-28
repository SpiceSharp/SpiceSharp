namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Enumeration of variable types.
    /// </summary>
    /// <remarks>
    /// Variables are the unknowns in the set of equations that need to be solved simultaneously.
    /// </remarks>
    public enum VariableType
    {
        /// <summary>
        /// The unknown associated with this node does not fall into a category.
        /// </summary>
        None = 0x00,

        /// <summary>
        /// The unknown associated with this node is a voltage.
        /// </summary>
        Voltage = 0x03,

        /// <summary>
        /// The unknown associated with this node is a current.
        /// </summary>
        Current = 0x04
    }
}
