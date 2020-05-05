using SpiceSharp.ParameterSets;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharp.Validation
{
    /// <summary>
    /// A base rule provider implementation.
    /// </summary>
    /// <seealso cref="IRules" />
    public abstract class BaseRules : ParameterSetCollection, IRules
    {
        /// <summary>
        /// Gets the number of rules that are violated.
        /// </summary>
        /// <value>
        /// The number of violated rules.
        /// </value>
        public int ViolationCount => this.Sum(r => r.ViolationCount);

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
                foreach (var rule in this)
                {
                    foreach (var violation in rule.Violations)
                        yield return violation;
                }
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public abstract IEnumerator<IRule> GetEnumerator();

        /// <summary>
        /// Resets all the rules.
        /// </summary>
        public virtual void Reset()
        {
            foreach (var rule in this)
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
            foreach (var rule in this)
            {
                if (rule is R r)
                    yield return r;
            }
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
