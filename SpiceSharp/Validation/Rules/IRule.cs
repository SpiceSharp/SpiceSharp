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
        /// Resets the rule.
        /// </summary>
        /// <param name="parameters">The configuration parameters.</param>
        void Reset(IParameterSetDictionary parameters);

        /// <summary>
        /// Validate the rule and see if something went wrong.
        /// </summary>
        void Validate();
    }
}
