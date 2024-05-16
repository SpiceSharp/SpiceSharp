using SpiceSharp.ParameterSets;
using SpiceSharp.Validation;
using System;
using System.Collections.Generic;

namespace SpiceSharp.Simulations.Biasing
{
    /// <summary>
    /// Necessary rules for biasing simulations.
    /// </summary>
    /// <seealso cref="BaseRules" />
    /// <seealso cref="IParameterized{P}"/>
    /// <seealso cref="ComponentRuleParameters"/>
    public class Rules : BaseRules,
        IParameterized<ComponentRuleParameters>
    {
        private readonly FloatingNodeRule _floatingNode;
        private readonly VoltageLoopRule _voltageLoop = new();
        private readonly VariablePresenceRule _groundPresence;

        /// <inheritdoc/>
        public ComponentRuleParameters Parameters { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Rules"/> class.
        /// </summary>
        /// <param name="factory">The variable factory.</param>
        /// <param name="comparer">The comparer for variable names.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="factory"/> is <c>null</c>.</exception>
        public Rules(IVariableFactory<IVariable> factory, IEqualityComparer<string> comparer)
        {
            var ground = factory.ThrowIfNull(nameof(factory)).GetSharedVariable(Constants.Ground);
            _floatingNode = new FloatingNodeRule(ground);
            _groundPresence = new VariablePresenceRule(ground);
            Parameters = new ComponentRuleParameters(factory, comparer ?? Constants.DefaultComparer);
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
