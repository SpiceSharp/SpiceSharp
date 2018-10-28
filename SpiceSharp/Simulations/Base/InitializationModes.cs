using System;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Possible modes for initialization of behaviors.
    /// </summary>
    [Flags]
    public enum InitializationModes
    {
        /// <summary>
        /// The default mode.
        /// </summary>
        None,

        /// <summary>
        /// Indicates that nodes may still be everywhere, and a first solution should be calculated.
        /// </summary>
        Float,

        /// <summary>
        /// Indicates that PN junctions or other difficult-to-converge dependencies should be initialized to a starting voltage.
        /// </summary>
        /// <remarks>
        /// PN junction often don't behave well in iterative methods due to their exponential dependency. A good initial value can be critical.
        /// </remarks>
        Junction,

        /// <summary>
        /// Indicates that an initial iteration has been done and that we need to fix the solution to check for convergence.
        /// </summary>
        Fix
    }
}
