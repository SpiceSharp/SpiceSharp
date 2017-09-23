namespace SpiceSharp.IntegrationMethods
{
    public class IntegrationConfiguration
    {
        /// <summary>
        /// Enumeration of possible truncation methods
        /// </summary>
        public enum TruncationMethods
        {
            PerNode,
            PerDevice
        }

        /// <summary>
        /// Gets or sets the transient tolerance
        /// Used for timestep truncation
        /// </summary>
        public double TrTol { get; set; } = 7.0;

        /// <summary>
        /// Gets or sets the local truncation error relative tolerance
        /// Used for calculating a timestep based on the estimated error
        /// </summary>
        public double LteRelTol { get; set; } = 1e-3;

        /// <summary>
        /// Gets or sets the local truncation error absolute tolerance
        /// Used for calculating a timestep based on the estimated error
        /// </summary>
        public double LteAbsTol { get; set; } = 1e-6;

        /// <summary>
        /// The truncation method used
        /// </summary>
        public TruncationMethods TruncationMethod { get; set; } = TruncationMethods.PerDevice;
    }
}
