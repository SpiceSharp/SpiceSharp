using SpiceSharp.Simulations;

namespace SpiceSharp.Validation
{
    /// <summary>
    /// An <see cref="IRuleViolation"/> for a <see cref="VariablePresenceRule"/>.
    /// </summary>
    /// <seealso cref="IRuleViolation" />
    public class VariablePresenceRuleViolation : IRuleViolation
    {
        /// <summary>
        /// Gets the variable that needed to be found.
        /// </summary>
        /// <value>
        /// The searched variable.
        /// </value>
        public IVariable Variable { get; }

        /// <summary>
        /// Gets the rule that was violated.
        /// </summary>
        /// <value>
        /// The violated rule.
        /// </value>
        public IRule Rule { get; }

        /// <summary>
        /// Gets the subject that caused the rule violation (if any).
        /// </summary>
        /// <value>
        /// The subject that caused the violation.
        /// </value>
        public IRuleSubject Subject => null;

        /// <summary>
        /// Initializes a new instance of the <see cref="VariablePresenceRuleViolation"/> class.
        /// </summary>
        /// <param name="rule">The rule.</param>
        /// <param name="variable">The variable.</param>
        public VariablePresenceRuleViolation(IRule rule, IVariable variable)
        {
            Rule = rule.ThrowIfNull(nameof(rule));
            Variable = variable.ThrowIfNull(nameof(variable));
        }
    }
}
