using SpiceSharp.Simulations;

namespace SpiceSharp.Validation
{
    /// <summary>
    /// An <see cref="IRuleViolation"/> for a <see cref="FloatingNodeRule"/>.
    /// </summary>
    /// <seealso cref="IRuleViolation" />
    public class FloatingNodeRuleViolation : IRuleViolation
    {
        /// <summary>
        /// Gets the floating node variable.
        /// </summary>
        /// <value>
        /// The floating node variable.
        /// </value>
        public IVariable FloatingVariable { get; }

        /// <summary>
        /// Gets the fixed node variable.
        /// </summary>
        /// <value>
        /// The fixed node variable.
        /// </value>
        public IVariable FixedVariable { get; }

        /// <summary>
        /// Gets the type of connection to the fixed variable.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public ConductionTypes Type { get; }

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
        /// Initializes a new instance of the <see cref="FloatingNodeRuleViolation" /> class.
        /// </summary>
        /// <param name="rule">The rule.</param>
        /// <param name="floatingVariable">The floating node variable.</param>
        /// <param name="fixedVariable">The fixed node variable.</param>
        /// <param name="type">The path type.</param>
        public FloatingNodeRuleViolation(IRule rule, IVariable floatingVariable, IVariable fixedVariable, ConductionTypes type)
        {
            Rule = rule.ThrowIfNull(nameof(rule));
            if (type == ConductionTypes.All)
                throw new SpiceSharpException("This is not a floating node!");
            FixedVariable = fixedVariable;
            FloatingVariable = floatingVariable;
            Type = type;
        }
    }
}
