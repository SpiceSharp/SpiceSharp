namespace SpiceSharp.Simulations.IntegrationMethods
{
    /// <summary>
    /// Interface of an integration method that supports breakpoints. The
    /// integration method makes sure the breakpoint timepoints are hit.
    /// </summary>
    /// <seealso cref="IIntegrationMethod"/>
    public interface IBreakpointMethod : IIntegrationMethod
    {
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
