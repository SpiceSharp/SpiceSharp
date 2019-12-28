namespace SpiceSharp.Validation
{
    /// <summary>
    /// Describes a rule violation.
    /// </summary>
    public interface IRuleViolation
    {
        /// <summary>
        /// Gets the rule that was violated.
        /// </summary>
        /// <value>
        /// The violated rule.
        /// </value>
        IRule Rule { get; }

        /// <summary>
        /// Gets the subject that caused the rule violation (if any).
        /// </summary>
        /// <value>
        /// The subject that caused the violation.
        /// </value>
        IRuleSubject Subject { get; }
    }
}
