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
    public class HasGroundRule : IComponentRule
    {
        private VariableParameters _vp;

        /// <summary>
        /// Gets a value indicating whether the rule has found a ground node.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has found a ground node; otherwise, <c>false</c>.
        /// </value>
        public bool HasGround { get; private set; }

        /// <summary>
        /// Occurs when the rule has been violated.
        /// </summary>
        public event EventHandler<RuleViolationEventArgs> Violated;

        /// <summary>
        /// Resets the rule.
        /// </summary>
        /// <param name="parameters">The configuration parameters.</param>
        /// <exception cref="ValidationException">Thrown when no variable set has been specified.</exception>
        public void Reset(IParameterSetDictionary parameters)
        {
            _vp = parameters.GetValue<VariableParameters>();
            if (_vp == null || _vp.Variables == null)
                throw new ValidationException(Properties.Resources.Validation_NoVariableSet);
            HasGround = false;
        }

        /// <summary>
        /// Checks the specified component against the rule.
        /// </summary>
        /// <param name="component">The component.</param>
        public void ApplyComponent(IComponent component)
        {
            component.ThrowIfNull(nameof(component));
            if (_vp == null || _vp.Variables == null)
                throw new ValidationException(Properties.Resources.Validation_NoVariableSet);
            var variables = _vp.Variables;
            if (HasGround)
                return;
            for (var i = 0; i < component.PinCount; i++)
            {
                if (variables.TryGetNode(component.GetNode(i), out var result) && result.Equals(variables.Ground))
                {
                    HasGround = true;
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
            if (HasGround)
                return;
            var args = new RuleViolationEventArgs();
            Violated?.Invoke(this, args);
            if (!args.Ignore)
                throw new NoGroundException();
        }
    }
}
