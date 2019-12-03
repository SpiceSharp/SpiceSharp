using SpiceSharp.Components;
using SpiceSharp.Diagnostics;
using SpiceSharp.Diagnostics.Validation;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Validation.Rules
{
    /// <summary>
    /// An <see cref="IRule"/> that checks for the existence of a ground node of at least one component.
    /// </summary>
    /// <seealso cref="IRule" />
    public class HasGroundRule : IComponentValidationRule
    {
        private IVariableSet _variables;
        private bool _hasGround = false;

        /// <summary>
        /// Occurs when the rule has been violated.
        /// </summary>
        public event EventHandler<RuleViolationEventArgs> Violated;

        /// <summary>
        /// Sets up the validation rule.
        /// </summary>
        /// <exception cref="ValidationException">Thrown when no variable set has been specified.</exception>
        public void Setup(IParameterSetDictionary parameters)
        {
            var config = parameters.GetValue<VariableParameters>();
            _variables = config.Variables ?? throw new ValidationException(Properties.Resources.Validation_NoVariableSet);
            _hasGround = false;
        }

        /// <summary>
        /// Checks the specified component against the rule.
        /// </summary>
        /// <param name="component">The component.</param>
        public void Check(IComponent component)
        {
            component.ThrowIfNull(nameof(component));
            if (_hasGround)
                return;
            for (var i = 0; i < component.PinCount; i++)
            {
                if (_variables.TryGetNode(component.GetNode(i), out var result) && result == _variables.Ground)
                {
                    _hasGround = true;
                    return;
                }
            }
        }

        /// <summary>
        /// Finish the check by validating the results.
        /// </summary>
        /// <exception cref="NoGroundException">Thrown when no ground node has been found.</exception>
        public void Validate()
        {
            if (_hasGround)
                return;
            var args = new RuleViolationEventArgs();
            Violated?.Invoke(this, args);
            if (!args.Ignore)
                throw new NoGroundException();
        }
    }
}
