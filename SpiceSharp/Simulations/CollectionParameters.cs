using SpiceSharp.Validation;
using System;
using System.Collections.Generic;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// This class can configure how collections are created in simulations.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    public class CollectionParameters : ParameterSet
    {
        /// <summary>
        /// Gets or sets a factory for a rule provider that can be used to validate the simulation.
        /// </summary>
        /// <value>
        /// The rule provider.
        /// </value>
        public Func<IRules> RuleProvider { get; set; }
    }
}
