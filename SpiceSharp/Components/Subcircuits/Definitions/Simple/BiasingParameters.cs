using SpiceSharp.Attributes;

namespace SpiceSharp.Components.SubcircuitBehaviors.Simple
{
    /// <summary>
    /// Parameters for biasing behaviors of a subcircuit.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    public class BiasingParameters : ParameterSet
    {
        /// <summary>
        /// Gets or sets a value indicating whether a local solver should be used or not.
        /// </summary>
        /// <value>
        ///   <c>true</c> if a local solver should be used; otherwise, <c>false</c>.
        /// </value>
        [ParameterName("localsolver"), ParameterInfo("Flag indicating whether a local solver should be used.")]
        public bool LocalSolver { get; set; }
    }
}
