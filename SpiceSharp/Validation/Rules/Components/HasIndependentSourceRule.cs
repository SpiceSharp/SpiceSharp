using System;
using SpiceSharp.Components;
using SpiceSharp.Diagnostics;
using SpiceSharp.Diagnostics.Validation;

namespace SpiceSharp.Validation.Rules
{
    /// <summary>
    /// An <see cref="IRule"/> that checks for the existence of at least one independent source.
    /// </summary>
    /// <seealso cref="IComponentRule" />
    public class HasIndependentSourceRule : IComponentRule
    {
        /// <summary>
        /// Gets a value indicating whether the rule has found an independent source.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the rule found an independent source; otherwise, <c>false</c>.
        /// </value>
        public bool HasSource { get; private set; }

        /// <summary>
        /// Occurs when the rule has been violated.
        /// </summary>
        public event EventHandler<RuleViolationEventArgs> Violated;

        /// <summary>
        /// Resets the rule.
        /// </summary>
        /// <param name="parameters">The configuration parameters.</param>
        public void Reset(IParameterSetDictionary parameters)
        {
            HasSource = false;
        }

        /// <summary>
        /// Checks the specified component against the rule.
        /// </summary>
        /// <param name="component">The component.</param>
        public void ApplyComponent(IComponent component)
        {
            if (component is VoltageSource || component is CurrentSource)
                HasSource = true;
        }

        /// <summary>
        /// Finish the check by validating the results.
        /// </summary>
        /// <exception cref="NoIndependentSourceException">Thrown if no independent source has been found.</exception>
        public void Validate()
        {
            if (HasSource)
                return;

            var args = new RuleViolationEventArgs();
            Violated?.Invoke(this, args);
            if (!args.Ignore)
                throw new NoIndependentSourceException();
        }
    }
}
