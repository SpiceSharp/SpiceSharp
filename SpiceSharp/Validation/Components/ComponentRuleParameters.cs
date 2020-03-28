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
        public IVariableFactory<IVariable> Factory { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentRuleParameters"/> class.
        /// </summary>
        /// <param name="factory">The variable factory.</param>
        public ComponentRuleParameters(IVariableFactory<IVariable> factory)
        {
            Factory = factory.ThrowIfNull(nameof(factory));
        }
    }
}
