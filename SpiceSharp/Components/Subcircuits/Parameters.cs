using SpiceSharp.ParameterSets;

namespace SpiceSharp.Components.Subcircuits
{
    /// <summary>
    /// Parameters for a <see cref="Subcircuit"/>.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    public class Parameters : ParameterSet
    {
        /// <summary>
        /// Gets or sets the subcircuit definition.
        /// </summary>
        /// <value>
        /// The definition.
        /// </value>
        [ParameterName("definition"), ParameterInfo("The subcircuit definition.")]
        public ISubcircuitDefinition Definition { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a local solver can be used.
        /// </summary>
        /// <value>
        ///   <c>true</c> if a local solver should be used; otherwise, <c>false</c>.
        /// </value>
        [ParameterName("localsolver"), ParameterInfo("Flag indicating whether or not a local solver should be used.")]
        public bool LocalSolver { get; set; }
    }
}
