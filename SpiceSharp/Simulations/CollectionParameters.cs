using SpiceSharp.Validation;
using System;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// This class can configure how collections are created in simulations.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    public class CollectionParameters : ParameterSet
    {
        /// <summary>
        /// Gets or sets a factory for the variable set.
        /// </summary>
        /// <value>
        /// The variables.
        /// </value>
        public Func<IVariableSet> Variables { get; set; }

        /// <summary>
        /// Gets or sets a factory for a rule provider that can be used to validate the simulation.
        /// </summary>
        /// <value>
        /// The rule provider.
        /// </value>
        public Func<IRuleProvider> RuleProvider { get; set; }
    }
}
