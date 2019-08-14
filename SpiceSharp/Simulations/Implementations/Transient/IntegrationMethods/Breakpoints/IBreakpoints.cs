namespace SpiceSharp.IntegrationMethods
{
    /// <summary>
    /// Interface that integration methods can implement to indicate they support breakpoints
    /// </summary>
    /// <remarks>
    /// An integration method that implements this interface is responsible for hitting the
    /// breakpoints and clearing them when they have been hit.
    /// </remarks>
    public interface IBreakpoints
    {
        /// <summary>
        /// Gets the current time point.
        /// </summary>
        double Time { get; }

        /// <summary>
        /// Gets the breakpoint system.
        /// </summary>
        Breakpoints Breakpoints { get; }

        /// <summary>
        /// Gets a value indicating whether this point is the first after a breakpoint.
        /// </summary>
        bool Break { get; }
    }
}
