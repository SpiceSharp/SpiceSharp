using System;
using SpiceSharp.Components;
using SpiceSharp.Diagnostics;

namespace SpiceSharp.Validation.Rules
{
    /// <summary>
    /// An <see cref="IRule"/> that checks for the existence of at least one independent source.
    /// </summary>
    /// <seealso cref="IComponentValidationRule" />
    public class HasIndependentSourceRule : IComponentValidationRule
    {
        private bool _hasSource;

        /// <summary>
        /// Occurs when the rule has been violated.
        /// </summary>
        public event EventHandler<RuleViolationEventArgs> Violated;

        /// <summary>
        /// Sets up the validation rule.
        /// </summary>
        /// <param name="parameters">The configuration parameters.</param>
        public void Setup(IParameterSetDictionary parameters)
        {
            _hasSource = false;
        }

        /// <summary>
        /// Checks the specified component against the rule.
        /// </summary>
        /// <param name="component">The component.</param>
        public void Check(IComponent component)
        {
            if (component is VoltageSource || component is CurrentSource)
                _hasSource = true;
        }

        /// <summary>
        /// Finish the check by validating the results.
        /// </summary>
        /// <exception cref="ValidationException">Thrown if no independent source has been found.</exception>
        public void Validate()
        {
            if (_hasSource)
                return;

            var args = new RuleViolationEventArgs();
            Violated?.Invoke(this, args);
            if (!args.Ignore)
                throw new ValidationException(Properties.Resources.Validation_NoIndependentSource);
        }
    }
}
