using System.Collections.Generic;

namespace SpiceSharp.Validation
{
    /// <summary>
    /// A generic rule provider that can have any custom set of rules.
    /// </summary>
    /// <seealso cref="IRuleProvider" />
    public class GenericRuleProvider : BaseRuleProvider
    {
        private readonly HashSet<IRule> _rules = new HashSet<IRule>();

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericRuleProvider"/> class.
        /// </summary>
        public GenericRuleProvider()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericRuleProvider"/> class.
        /// </summary>
        /// <param name="rules">The rules.</param>
        public GenericRuleProvider(params IRule[] rules)
        {
            foreach (var rule in rules)
                _rules.Add(rule);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericRuleProvider"/> class.
        /// </summary>
        /// <param name="rules">The rules.</param>
        public GenericRuleProvider(IEnumerable<IRule> rules)
        {
            foreach (var rule in rules)
                _rules.Add(rule);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public override IEnumerator<IRule> GetEnumerator()
        {
            foreach (var rule in _rules)
                yield return rule;
        }
    }
}
