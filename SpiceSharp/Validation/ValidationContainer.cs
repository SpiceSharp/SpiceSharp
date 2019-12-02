using System.Collections.Generic;

namespace SpiceSharp.Validation
{
    /// <summary>
    /// A container for validation rules.
    /// </summary>
    /// <seealso cref="IValidationContainer" />
    public class ValidationContainer : IValidationContainer
    {
        private ITypeDictionary<IValidationRule> _rules = new TypeDictionary<IValidationRule>();

        /// <summary>
        /// Gets the rules for validation.
        /// </summary>
        /// <value>
        /// The rules.
        /// </value>
        public IEnumerable<IValidationRule> Rules => _rules.Values;

        /// <summary>
        /// Clones the instance.
        /// </summary>
        /// <returns>
        /// The cloned instance.
        /// </returns>
        public ICloneable Clone()
        {
            var clone = new ValidationContainer();
            foreach (var rule in _rules.Values)
                clone._rules.Add((IValidationRule)rule.Clone());
            return clone;
        }

        /// <summary>
        /// Copies the contents of one interface to this one.
        /// </summary>
        /// <param name="source">The source parameter.</param>
        public void CopyFrom(ICloneable source)
        {
            _rules.Clear();
            var src = (ValidationContainer)source;
            foreach (var rule in src._rules.Values)
                _rules.Add(rule);
        }

        /// <summary>
        /// Tries to get a validation rule from the container.
        /// </summary>
        /// <typeparam name="T">The base rule type.</typeparam>
        /// <param name="rule">The rule.</param>
        /// <returns>
        ///   <c>true</c> if the rule has been found; otherwise <c>false</c>.
        /// </returns>
        public bool TryGetRule<T>(out T rule) where T : IValidationRule
            => _rules.TryGetValue(out rule);
    }
}
