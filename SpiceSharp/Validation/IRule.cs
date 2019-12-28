using System.Collections.Generic;

namespace SpiceSharp.Validation
{
    /// <summary>
    /// A rule that can be validated.
    /// </summary>
    public interface IRule
    {
        /// <summary>
        /// Gets a value indicating whether this <see cref="IRule"/> is being violated.
        /// </summary>
        /// <value>
        ///   <c>true</c> if violated; otherwise, <c>false</c>.
        /// </value>
        bool IsViolated { get; }

        /// <summary>
        /// Gets the rule violations.
        /// </summary>
        /// <value>
        /// The rule violations.
        /// </value>
        IEnumerable<IRuleViolation> Violations { get; }
    }
}
