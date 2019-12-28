using SpiceSharp.Simulations;
using System.Collections.Generic;

namespace SpiceSharp.Validation
{
    /// <summary>
    /// An <see cref="IRuleViolation"/> for a <see cref="FloatingNodeRule"/>.
    /// </summary>
    /// <seealso cref="IRuleViolation" />
    public class FloatingNodeRuleViolation : IRuleViolation
    {
        private readonly HashSet<Variable> _group;

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
        /// Gets the variables.
        /// </summary>
        /// <value>
        /// The variables.
        /// </value>
        public IEnumerable<Variable> Variables
        {
            get
            {
                foreach (var v in _group)
                    yield return v;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FloatingNodeRuleViolation"/> class.
        /// </summary>
        /// <param name="rule">The rule.</param>
        /// <param name="group">The group.</param>
        public FloatingNodeRuleViolation(IRule rule, HashSet<Variable> group)
        {
            Rule = rule.ThrowIfNull(nameof(rule));
            _group = group.ThrowIfNull(nameof(group));
        }

        /// <summary>
        /// Determines whether the specified variable has variable.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <returns>
        ///   <c>true</c> if the specified variable has variable; otherwise, <c>false</c>.
        /// </returns>
        public bool HasVariable(Variable variable) => _group.Contains(variable);
    }
}
