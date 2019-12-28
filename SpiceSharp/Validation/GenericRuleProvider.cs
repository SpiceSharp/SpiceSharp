using System.Collections;
using System.Collections.Generic;

namespace SpiceSharp.Validation
{
    /// <summary>
    /// A generic rule provider that can have any custom set of rules.
    /// </summary>
    /// <seealso cref="IRuleProvider" />
    public class GenericRuleProvider : IRuleProvider
    {
        private readonly HashSet<IRule> _rules = new HashSet<IRule>();

        /// <summary>
        /// Gets the number of rules that are violated.
        /// </summary>
        /// <value>
        /// The number of violated rules.
        /// </value>
        public int ViolationCount
        {
            get
            {
                var count = 0;
                foreach (var rule in _rules)
                {
                    if (rule.IsViolated)
                        count++;
                }
                return count;
            }
        }

        /// <summary>
        /// Gets the violated rules.
        /// </summary>
        /// <value>
        /// The violated rules.
        /// </value>
        public IEnumerable<IRuleViolation> Violations
        {
            get
            {
                foreach (var rule in _rules)
                {
                    foreach (var violation in rule.Violations)
                        yield return violation;
                }
            }
        }

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
        /// Gets all rules of the specified type.
        /// </summary>
        /// <typeparam name="R">The rule type.</typeparam>
        /// <returns>
        /// The rules of the specified type.
        /// </returns>
        public IEnumerable<R> GetRules<R>() where R : IRule
        {
            foreach (var rule in _rules)
            {
                if (rule is R r)
                    yield return r;
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<IRule> GetEnumerator()
        {
            foreach (var rule in _rules)
                yield return rule;
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
