using System;

namespace SpiceSharp.Validation
{
    /// <summary>
    /// An interface that describes a rule for validation.
    /// </summary>
    public interface IRule
    {
        /// <summary>
        /// Occurs when the rule has been violated.
        /// </summary>
        event EventHandler<RuleViolationEventArgs> Violated;

        /// <summary>
        /// Sets up the validation rule.
        /// </summary>
        /// <param name="parameters">The configuration parameters.</param>
        void Setup(IParameterSetDictionary parameters);

        /// <summary>
        /// Finish the check by validating the results.
        /// </summary>
        void Validate();
    }
}
