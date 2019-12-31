using SpiceSharp.Validation;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SpiceSharp.Components.SubcircuitBehaviors
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="SpiceSharp.Validation.IRules" />
    public class SubcircuitRules : IRules
    {
        private readonly IRules _parent;
        private readonly ComponentRuleParameters _validationParameters, _parentValidationParameters;

        /// <summary>
        /// Gets the number of rule violations.
        /// </summary>
        /// <value>
        /// The number of rule violations.
        /// </value>
        public int ViolationCount => _parent.ViolationCount;

        /// <summary>
        /// Gets the violated rules.
        /// </summary>
        /// <value>
        /// The violated rules.
        /// </value>
        public IEnumerable<IRuleViolation> Violations => _parent.Violations;

        /// <summary>
        /// Gets all parameter sets.
        /// </summary>
        /// <value>
        /// The parameter sets.
        /// </value>
        public IEnumerable<IParameterSet> ParameterSets
        {
            get
            {
                foreach (var ps in _parent.ParameterSets)
                {
                    if (ps == _parentValidationParameters)
                        yield return _validationParameters;
                    else
                        yield return ps;
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubcircuitRules"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="parent">The parent.</param>
        public SubcircuitRules(string name, IRules parent)
        {
            _parent = parent.ThrowIfNull(nameof(parent));
            _parentValidationParameters = _parent.GetParameterSet<ComponentRuleParameters>();
            _validationParameters = new ComponentRuleParameters(new SubcircuitVariableSet(name, _parentValidationParameters.Variables));
        }

        /// <summary>
        /// Resets all the rules.
        /// </summary>
        public void Reset() => _parent.Reset();

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<IRule> GetEnumerator() => _parent.GetEnumerator();

        /// <summary>
        /// Gets the parameter set of the specified type.
        /// </summary>
        /// <typeparam name="P">The parameter set type.</typeparam>
        /// <returns>
        /// The parameter set.
        /// </returns>
        /// <exception cref="NotImplementedException"></exception>
        public P GetParameterSet<P>() where P : IParameterSet
        {
            if (_validationParameters is P p)
                return p;
            return _parent.GetParameterSet<P>();
        }

        /// <summary>
        /// Gets all rules of the specified type.
        /// </summary>
        /// <typeparam name="R">The rule type.</typeparam>
        /// <returns>
        /// The rules of the specified type.
        /// </returns>
        public IEnumerable<R> GetRules<R>() where R : IRule => _parent.GetRules<R>();

        /// <summary>
        /// Tries to get the parameter set of the specified type.
        /// </summary>
        /// <typeparam name="P">The parameter set type.</typeparam>
        /// <param name="value">The parameter set.</param>
        /// <returns>
        ///   <c>true</c> if the parameter set was found; otherwise, <c>false</c>.
        /// </returns>
        public bool TryGetParameterSet<P>(out P value) where P : IParameterSet
        {
            if (_validationParameters is P p)
            {
                value = p;
                return true;
            }
            return _parent.TryGetParameterSet<P>(out value);
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
