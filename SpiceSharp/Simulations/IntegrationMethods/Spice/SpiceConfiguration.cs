using SpiceSharp.Attributes;

namespace SpiceSharp.Simulations.IntegrationMethods.Spice
{
    /// <summary>
    /// A configuration for Spice-based integration methods.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    public class SpiceConfiguration : ParameterSet
    {
        /// <summary>
        /// Gets or sets the truncation tolerance correction factor.
        /// </summary>
        /// <value>
        /// The truncation tolerance.
        /// </value>
        [ParameterName("trtol"), ParameterInfo("The truncation tolerance correction factor")]
        public double TrTol { get; set; } = 7.0;

        /// <summary>
        /// Gets or sets the local truncation error relative tolerance.
        /// </summary>
        /// <value>
        /// The local truncation error relative tolerance.
        /// </value>
        [ParameterName("ltereltol"), ParameterInfo("The allowed relative tolerance for timestep truncation")]
        public double LteRelTol { get; set; } = 1e-3;

        /// <summary>
        /// Gets or sets the local truncation truncation error absolute tolerance.
        /// </summary>
        /// <value>
        /// The local truncation error absolute tolerance.
        /// </value>
        [ParameterName("lteabstol"), ParameterInfo("The allowed absolute tolerance for timestep truncation")]
        public double LteAbsTol { get; set; } = 1e-6;

        /// <summary>
        /// Gets or sets the absolute tolerance for charges.
        /// </summary>
        /// <value>
        /// The allowed tolernace on charges.
        /// </value>
        [ParameterName("chgtol"), ParameterInfo("The allowed absolute tolerance on charge")]
        public double ChgTol { get; set; } = 1e-14;

        /// <summary>
        /// Gets or sets the maximum timestep expansion factor.
        /// </summary>
        /// <value>
        /// The expansion factor.
        /// </value>
        /// <remarks>
        /// This is the maximum factor a timestep can be made from one point to the next. For example,
        /// if the previous delta was 1us, the next delta is maximum Expansion * 1us.
        /// </remarks>
        [ParameterName("expansion"), ParameterInfo("The maximum expansion of a timestep")]
        public double Expansion { get; set; } = 2.0;
    }
}
