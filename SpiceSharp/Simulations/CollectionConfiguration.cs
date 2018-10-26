using System.Collections.Generic;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// This class can configure how collections are created in simulations.
    /// </summary>
    /// <seealso cref="SpiceSharp.ParameterSet" />
    public class CollectionConfiguration : ParameterSet
    {
        /// <summary>
        /// Gets or sets the comparer used for comparing two variable/node identifiers.
        /// </summary>
        /// <value>
        /// The comparer for <see cref="Variable"/> identifiers.
        /// </value>
        public IEqualityComparer<string> VariableComparer { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating whether the simulation should clone all parameters.
        /// </summary>
        /// <remarks>
        /// This is mainly useful when using the same circuit for multiple simulations and
        /// running them in multiple threads.
        /// </remarks>
        /// <value>
        ///   <c>true</c> if parameters need to be cloned; otherwise, <c>false</c>.
        /// </value>
        public bool CloneParameters { get; set; }
    }
}
