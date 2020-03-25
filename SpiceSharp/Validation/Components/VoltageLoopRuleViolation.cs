using SpiceSharp.Simulations;

namespace SpiceSharp.Validation
{
    /// <summary>
    /// A rule violation of a <see cref="VoltageLoopRule"/>.
    /// </summary>
    /// <seealso cref="IRuleViolation" />
    public class VoltageLoopRuleViolation : IRuleViolation
    {
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
        public IRuleSubject Subject { get; }

        /// <summary>
        /// Gets the first node that is being fixed.
        /// </summary>
        /// <value>
        /// The first node.
        /// </value>
        public IVariable First { get; }

        /// <summary>
        /// Gets the second node that is being fixed.
        /// </summary>
        /// <value>
        /// The second node.
        /// </value>
        public IVariable Second { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VoltageLoopRuleViolation"/> class.
        /// </summary>
        /// <param name="rule">The rule.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="first">The first node.</param>
        /// <param name="second">The second node.</param>
        public VoltageLoopRuleViolation(VoltageLoopRule rule, IRuleSubject subject, IVariable first, IVariable second)
        {
            Rule = rule.ThrowIfNull(nameof(rule));
            Subject = subject.ThrowIfNull(nameof(subject));
            First = first.ThrowIfNull(nameof(first));
            Second = second.ThrowIfNull(nameof(second));
        }
    }
}
