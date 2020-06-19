using SpiceSharp.Components;
using SpiceSharp.ParameterSets;
using SpiceSharp.Simulations;
using System.Collections.Generic;

namespace SpiceSharp.Validation
{
    /// <summary>
    /// Validation parameters for <see cref="IComponent"/>. This means interconnected entities.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    public class ComponentRuleParameters : ParameterSet
    {
        /// <summary>
        /// Gets the variables.
        /// </summary>
        /// <value>
        /// The variables.
        /// </value>
        public IVariableFactory<IVariable> Factory { get; }

        /// <summary>
        /// Gets the comparer used to compare variable names.
        /// </summary>
        /// <value>
        /// The comparer.
        /// </value>
        public IEqualityComparer<string> Comparer { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentRuleParameters"/> class.
        /// </summary>
        /// <param name="factory">The variable factory.</param>
        /// <param name="comparer">The comparer for comparing variable names.</param>
        public ComponentRuleParameters(IVariableFactory<IVariable> factory, IEqualityComparer<string> comparer)
        {
            Factory = factory.ThrowIfNull(nameof(factory));
            Comparer = comparer.ThrowIfNull(nameof(comparer));
        }
    }
}
