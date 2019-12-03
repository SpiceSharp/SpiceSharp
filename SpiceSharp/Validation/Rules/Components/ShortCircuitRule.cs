using SpiceSharp.Components;
using SpiceSharp.Diagnostics;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Validation.Rules
{
    /// <summary>
    /// An <see cref="IRule"/> that checks for a short-circuited <see cref="IComponent"/>.
    /// </summary>
    /// <seealso cref="IRule" />
    public class ShortCircuitRule : IRule
    {
        private IVariableSet _variables;

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
            var config = parameters.GetValue<VariableParameters>();
            _variables = config.Variables ?? throw new ValidationException(Properties.Resources.Validation_NoVariableSet);
        }

        /// <summary>
        /// Checks the specified component.
        /// </summary>
        /// <param name="component">The component.</param>
        /// <exception cref="ValidationException">Thrown when all pins of the component have been shorted.</exception>
        public void Check(IComponent component)
        {
            component.ThrowIfNull(nameof(component));
            if (component.PinCount <= 1)
                return;
            var reference = component.GetNode(0);
            for (var i = 1; i < component.PinCount; i++)
            {
                if (!_variables.Comparer.Equals(reference, component.GetNode(i)))
                    return;
            }

            Problem = component;
            var args = new RuleViolationEventArgs();
            Violated?.Invoke(this, args);
            if (args.Ignore)
                throw new ValidationException(Properties.Resources.Validation_ShortCircuitComponent);
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
