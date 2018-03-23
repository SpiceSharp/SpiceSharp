namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Enumeration of unknown types
    /// </summary>
    public enum UnknownType
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
}
