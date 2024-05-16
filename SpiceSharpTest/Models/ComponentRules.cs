using SpiceSharp;
using SpiceSharp.ParameterSets;
using SpiceSharp.Validation;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharpTest.Models
{
    public class ComponentRules : ParameterSetCollection, IRules, IParameterized<ComponentRuleParameters>
    {
        private readonly HashSet<IRule> _rules = [];

        /// <summary>
        /// Gets the parameter set.
        /// </summary>
        /// <value>
        /// The parameter set.
        /// </value>
        public ComponentRuleParameters Parameters { get; }

        /// <summary>
        /// Gets the number of rule violations.
        /// </summary>
        /// <value>
        /// The number of rule violations.
        /// </value>
        public int ViolationCount => _rules.Sum(x => x.ViolationCount);

        /// <summary>
        /// Gets the violated rules.
        /// </summary>
        /// <value>
        /// The violated rules.
        /// </value>
        public IEnumerable<IRuleViolation> Violations => _rules.SelectMany(x => x.Violations);

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentRules"/> class.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <param name="rules">The rules.</param>
        public ComponentRules(ComponentRuleParameters parameters, params IRule[] rules)
        {
            Parameters = parameters.ThrowIfNull(nameof(parameters));
            foreach (var rule in rules)
                _rules.Add(rule);
        }

        /// <summary>
        /// Resets all the rules.
        /// </summary>
        public void Reset()
        {
            foreach (var rule in _rules)
                rule.Reset();
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
        public IEnumerator<IRule> GetEnumerator() => _rules.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
