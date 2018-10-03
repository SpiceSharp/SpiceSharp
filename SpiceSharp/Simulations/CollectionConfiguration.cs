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
        public IEqualityComparer<Identifier> VariableComparer { get; set; }

        /// <summary>
        /// Gets or sets the  comparer used for comparing two entity identifiers.
        /// </summary>
        /// <value>
        /// The comparer for entity identifiers.
        /// </value>
        /// <remarks>Typically, you'll want this comparer to be identical to the one used for circuits.</remarks>
        public IEqualityComparer<Identifier> EntityComparer { get; set; }
    }
}
