using System;

namespace SpiceSharp.IntegrationMethods
{
    /// <summary>
    /// Default parameters for an integration configuration
    /// </summary>
    public class IntegrationParameters : ParameterSet
    {
        /// <summary>
        /// Enumeration of default truncation methods
        /// </summary>
        [Flags]
        public enum TruncationMethods
        {
            PerNode = 0x01,
            PerDevice = 0x02
        }

        /// <summary>
        /// Gets or sets the transient tolerance
        /// Used for timeStep truncation
        /// </summary>
        public double TruncationTol { get; set; } = 7.0;

        /// <summary>
        /// Gets or sets the local truncation error relative tolerance
        /// Used for calculating a timeStep based on the estimated error
        /// </summary>
        public double LteRelTol { get; set; } = 1e-3;

        /// <summary>
        /// Gets or sets the local truncation error absolute tolerance
        /// Used for calculating a timeStep based on the estimated error
        /// </summary>
        public double LteAbsTol { get; set; } = 1e-6;

        /// <summary>
        /// The truncation method used
        /// </summary>
        public TruncationMethods TruncationMethod { get; set; } = TruncationMethods.PerDevice;
    }
}
