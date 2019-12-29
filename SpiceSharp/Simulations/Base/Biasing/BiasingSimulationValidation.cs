using SpiceSharp.Validation;
using System.Collections.Generic;

namespace SpiceSharp.Simulations.Biasing
{
    /// <summary>
    /// Necessary validation for biasing simulations.
    /// </summary>
    /// <seealso cref="BaseRuleProvider" />
    public class BiasingSimulationValidation : BaseRuleProvider,
        IParameterized<ComponentValidationParameters>
    {
        private readonly FloatingNodeRule _floatingNode = new FloatingNodeRule();
        private readonly VoltageLoopRule _voltageLoop = new VoltageLoopRule();
        private readonly VariablePresenceRule _groundPresence;

        /// <summary>
        /// Gets the parameter set.
        /// </summary>
        /// <value>
        /// The parameter set.
        /// </value>
        public ComponentValidationParameters Parameters { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BiasingSimulationValidation"/> class.
        /// </summary>
        /// <param name="parent">The parent variable set.</param>
        public BiasingSimulationValidation(IVariableSet parent)
        {
            _groundPresence = new VariablePresenceRule(parent.Ground);
            Parameters = new ComponentValidationParameters(parent);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public override IEnumerator<IRule> GetEnumerator()
        {
            yield return _floatingNode;
            yield return _voltageLoop;
            yield return _groundPresence;
        }
    }
}
