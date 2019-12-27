using SpiceSharp.Attributes;

namespace SpiceSharp.Components.SubcircuitBehaviors.Simple
{
    /// <summary>
    /// Parameters for biasing behaviors of a subcircuit.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    public class BaseParameters : ParameterSet
    {
        /// <summary>
        /// Gets or sets a value indicating whether a local solver should be used or not.
        /// </summary>
        /// <value>
        ///   <c>true</c> if a local solver should be used; otherwise, <c>false</c>.
        /// </value>
        [ParameterName("biasing.localsolver"), ParameterInfo("Flag indicating whether a local solver should be used.")]
        public bool LocalBiasingSolver { get; set; }
    }
}
