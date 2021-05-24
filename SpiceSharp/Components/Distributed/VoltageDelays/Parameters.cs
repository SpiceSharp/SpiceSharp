using SpiceSharp.ParameterSets;
using SpiceSharp.Attributes;

namespace SpiceSharp.Components.VoltageDelays
{
    /// <summary>
    /// Base parameters for a <see cref="VoltageDelay" />.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    [GeneratedParameters]
    public partial class Parameters : ParameterSet<Parameters>
    {
        /// <summary>
        /// Gets or sets the delay in seconds.
        /// </summary>
        /// <value>
        /// The delay.
        /// </value>
        [ParameterName("delay"), ParameterName("td"), ParameterInfo("The delay.", Units = "s")]
        [GreaterThanOrEquals(0), Finite]
        private double _delay;

        /// <summary>
        /// Gets or sets the relative tolerance to determine when a breakpoint (heavy nonlinear behavior occurs) needs to be added.
        /// </summary>
        /// <value>
        /// The relative tolerance.
        /// </value>
        [ParameterName("reltol"), ParameterInfo("The relative tolerance used to decide on adding a breakpoint.")]
        [GreaterThan(0), Finite]
        private double _relativeTolerance = 1.0;

        /// <summary>
        /// Gets or sets the absolute tolerance to determine when a breakpoint (heavy nonlinear behavior occurs) needs to be added.
        /// </summary>
        /// <value>
        /// The absolute tolerance.
        /// </value>
        [ParameterName("abstol"), ParameterInfo("The absolute tolerance used to decide on adding a breakpoint.")]
        [GreaterThan(0), Finite]
        private double _absoluteTolerance = 1.0;
    }
}
