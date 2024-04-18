using System.Collections.Generic;

namespace SpiceSharp.Validation
{
    /// <summary>
    /// A generic rule provider that can have any custom set of rules.
    /// </summary>
    /// <seealso cref="IRules" />
    public class GenericRules : BaseRules
    {
        private readonly HashSet<IRule> _rules = [];

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericRules"/> class.
        /// </summary>
        public GenericRules()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericRules"/> class.
        /// </summary>
        /// <param name="rules">The rules.</param>
        public GenericRules(params IRule[] rules)
        {
            foreach (var rule in rules)
                _rules.Add(rule);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericRules"/> class.
        /// </summary>
        /// <param name="rules">The rules.</param>
        public GenericRules(IEnumerable<IRule> rules)
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
