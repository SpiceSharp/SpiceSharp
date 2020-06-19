using System.Collections.Generic;

namespace SpiceSharp.Validation
{
    /// <summary>
    /// A rule that can be validated.
    /// </summary>
    public interface IRule
    {
        /// <summary>
        /// Gets the number of violations of this rule.
        /// </summary>
        /// <value>
        /// The violation count.
        /// </value>
        int ViolationCount { get; }

        /// <summary>
        /// Gets the rule violations.
        /// </summary>
        /// <value>
        /// The rule violations.
        /// </value>
        IEnumerable<IRuleViolation> Violations { get; }

        /// <summary>
        /// Resets the rule.
        /// </summary>
        void Reset();
    }
}
