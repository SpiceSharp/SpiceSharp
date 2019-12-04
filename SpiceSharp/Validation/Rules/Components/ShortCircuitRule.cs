using SpiceSharp.Components;
using SpiceSharp.Diagnostics;
using SpiceSharp.Diagnostics.Validation;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Validation.Rules
{
    /// <summary>
    /// An <see cref="IRule"/> that checks for a short-circuited <see cref="IComponent"/>.
    /// </summary>
    /// <seealso cref="IRule" />
    public class ShortCircuitRule : IComponentValidationRule
    {
        private VariableParameters _vp;

        /// <summary>
        /// Occurs when the rule has been violated.
        /// </summary>
        public event EventHandler<RuleViolationEventArgs> Violated;

        /// <summary>
        /// Gets the problematic components.
        /// </summary>
        /// <value>
        /// The problems.
        /// </value>
        public IComponent Problem { get; private set; }

        /// <summary>
        /// Sets up the validation rule.
        /// </summary>
        /// <exception cref="ValidationException">Thrown when no variable set has been specified.</exception>
        public void Setup(IParameterSetDictionary parameters)
        {
            Problem = null;
            _vp = parameters.GetValue<VariableParameters>();
            if (_vp == null || _vp.Variables == null)
                throw new ValidationException(Properties.Resources.Validation_NoVariableSet);
        }

        /// <summary>
        /// Checks the specified component against the rule.
        /// </summary>
        /// <param name="component">The component.</param>
        /// <exception cref="ValidationException">Thrown when no variable set is specified.</exception>
        /// <exception cref="ShortCircuitComponentException">Thrown when all pins of the component have been shorted.</exception>
        public void Check(IComponent component)
        {
            component.ThrowIfNull(nameof(component));
            if (_vp == null || _vp.Variables == null)
                throw new ValidationException(Properties.Resources.Validation_NoVariableSet);
            var variables = _vp.Variables;
            if (component.PinCount <= 1)
                return;

            var reference = variables.MapNode(component.GetNode(0), VariableType.Voltage);
            for (var i = 1; i < component.PinCount; i++)
            {
                var other = variables.MapNode(component.GetNode(i), VariableType.Voltage);
                if (!reference.Equals(other))
                    return;
            }

            // All the nodes are the same, so the rule is violated.
            Problem = component;
            var args = new RuleViolationEventArgs();
            Violated?.Invoke(this, args);
            if (!args.Ignore)
                throw new ShortCircuitComponentException(component);
        }

        /// <summary>
        /// Finish the check by validating the results.
        /// </summary>
        public void Validate()
        {
            // Validation is done as soon as it has been encountered.
        }
    }
}
