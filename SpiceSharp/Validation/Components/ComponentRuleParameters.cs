using SpiceSharp.Components;
using SpiceSharp.Simulations;

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
        public IVariableSet Variables { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentRuleParameters"/> class.
        /// </summary>
        /// <param name="variables">The variables.</param>
        public ComponentRuleParameters(IVariableSet variables)
        {
            Variables = variables.ThrowIfNull(nameof(variables));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentRuleParameters"/> class.
        /// </summary>
        public ComponentRuleParameters()
        {
            Variables = new VariableSet();
        }
    }
}
