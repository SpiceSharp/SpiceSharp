using SpiceSharp.Attributes;

namespace SpiceSharp.Simulations.IntegrationMethods.Spice
{
    /// <summary>
    /// Configuration for Spice-based integration methods
    /// </summary>
    public class SpiceConfiguration : ParameterSet
    {
        /// <summary>
        /// Gets or sets the truncation tolerance correction factor
        /// </summary>
        [ParameterName("trtol"), ParameterInfo("The truncation tolerance correction factor")]
        public double TrTol { get; set; } = 7.0;

        /// <summary>
        /// Gets or sets the truncation relative tolerance
        /// </summary>
        [ParameterName("ltereltol"), ParameterInfo("The allowed relative tolerance for timestep truncation")]
        public double LteRelTol { get; set; } = 1e-3;

        /// <summary>
        /// Gets or sets the truncation absolute tolerance
        /// </summary>
        [ParameterName("lteabstol"), ParameterInfo("The allowed absolute tolerance for timestep truncation")]
        public double LteAbsTol { get; set; } = 1e-6;

        /// <summary>
        /// Gets or sets the tolerance for charge
        /// </summary>
        [ParameterName("chgtol"), ParameterInfo("The allowed tolerance on charge")]
        public double ChgTol { get; set; } = 1e-14;

        /// <summary>
        /// Gets or sets the maximum timestep expansion factor
        /// </summary>
        /// <remarks>
        /// This is the maximum factor a timestep can be made from one point to the next. For example,
        /// if the previous delta was 1us, the next delta is maximum Expansion * 1us.
        /// </remarks>
        [ParameterName("expansion"), ParameterInfo("The maximum expansion of a timestep")]
        public double Expansion { get; set; } = 2.0;
    }
}
