using System.Collections.Generic;

namespace SpiceSharp.Validation
{
    /// <summary>
    /// Describes a rule provider.
    /// </summary>
    /// <seealso cref="IParameterized" />
    /// <seealso cref="IEnumerable{IRule}" />
    public interface IRuleProvider : IParameterized, IEnumerable<IRule>
    {
        /// <summary>
        /// Gets the number of rule violations.
        /// </summary>
        /// <value>
        /// The number of rule violations.
        /// </value>
        int ViolationCount { get; }

        /// <summary>
        /// Gets the violated rules.
        /// </summary>
        /// <value>
        /// The violated rules.
        /// </value>
        IEnumerable<IRuleViolation> Violations { get; }

        /// <summary>
        /// Gets all rules of the specified type.
        /// </summary>
        /// <typeparam name="R">The rule type.</typeparam>
        /// <returns>
        /// The rules of the specified type.
        /// </returns>
        IEnumerable<R> GetRules<R>() where R : IRule;
    }
}
