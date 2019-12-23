using SpiceSharp.Attributes;

namespace SpiceSharp.Components.SubcircuitBehaviors.Simple
{
    /// <summary>
    /// Parameters for frequency behaviors of a subcircuit.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    public class FrequencyParameters : ParameterSet
    {
        /// <summary>
        /// Gets or sets a value indicating whether a local solver should be used or not.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [local solver]; otherwise, <c>false</c>.
        /// </value>
        [ParameterName("frequency.localsolver"), ParameterInfo("Flag indicating whether a local solver should be used.")]
        public bool LocalSolver { get; set; }
    }
}
