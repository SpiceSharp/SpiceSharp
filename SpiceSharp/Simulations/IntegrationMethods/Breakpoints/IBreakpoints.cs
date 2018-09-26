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
        /// Gets the current time point
        /// </summary>
        double Time { get; }

        /// <summary>
        /// Gets the breakpoint class
        /// </summary>
        Breakpoints Breakpoints { get; }

        /// <summary>
        /// Gets whether or not we just hit a breakpoint
        /// </summary>
        /// <remarks>
        /// If true, then we are now calculating the first point after a breakpoint
        /// </remarks>
        bool Break { get; }
    }
}
