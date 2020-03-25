using SpiceSharp.Simulations;
using System.Collections.Generic;

namespace SpiceSharp.Validation
{
    /// <summary>
    /// An <see cref="IConductiveRule"/> that will check for the presence of a specific variable.
    /// </summary>
    /// <seealso cref="IConductiveRule" />
    public class VariablePresenceRule : IConductiveRule
    {
        private readonly IVariable _search;

        /// <summary>
        /// Gets the number of violations of this rule.
        /// </summary>
        /// <value>
        /// The violation count.
        /// </value>
        public int ViolationCount { get; private set; }

        /// <summary>
        /// Gets the rule violations.
        /// </summary>
        /// <value>
        /// The rule violations.
        /// </value>
        public IEnumerable<IRuleViolation> Violations
        {
            get
            {
                if (ViolationCount > 0)
                    yield return new VariablePresenceRuleViolation(this, _search);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VariablePresenceRule"/> class.
        /// </summary>
        /// <param name="search">The varibale that needs to be present.</param>
        public VariablePresenceRule(IVariable search)
        {
            _search = search.ThrowIfNull(nameof(search));
            ViolationCount = 1;
        }

        /// <summary>
        /// Resets the rule.
        /// </summary>
        public void Reset()
        {
            ViolationCount = 1;
        }

        /// <summary>
        /// Applies the specified variables as being connected by a conductive path.
        /// </summary>
        /// <param name="subject">The rule subject.</param>
        /// <param name="variables">The variables.</param>
        public void AddPath(IRuleSubject subject, params IVariable[] variables)
        {
            foreach (var v in variables)
            {
                if (v.Equals(_search))
                    ViolationCount = 0;
            }
        }

        /// <summary>
        /// Specifies variables as being connected by a conductive path of the specified type.
        /// </summary>
        /// <param name="subject">The subject that applies the conductive paths.</param>
        /// <param name="type">The type of path between these variables.</param>
        /// <param name="variables">The variables that are connected.</param>
        public void AddPath(IRuleSubject subject, ConductionTypes type, params IVariable[] variables)
        {
            foreach (var v in variables)
            {
                if (v.Equals(_search))
                    ViolationCount = 0;
            }
        }
    }
}
