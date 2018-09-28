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
        /// <value>
        /// The current time.
        /// </value>
        double Time { get; }

        /// <summary>
        /// Gets the breakpoint system.
        /// </summary>
        /// <value>
        /// The breakpoints.
        /// </value>
        Breakpoints Breakpoints { get; }

        /// <summary>
        /// Gets a value indicating whether this point is the first after a breakpoint.
        /// </summary>
        /// <value>
        ///   <c>true</c> if we just hit a breakpoint; otherwise, <c>false</c>.
        /// </value>
        bool Break { get; }
    }
}
