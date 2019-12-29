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
        private readonly Variable _search;

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
        public VariablePresenceRule(Variable search)
        {
            _search = search.ThrowIfNull(nameof(search));
            ViolationCount = 1;
        }

        /// <summary>
        /// Applies the specified variables as being connected by a conductive path.
        /// </summary>
        /// <param name="subject">The rule subject.</param>
        /// <param name="variables">The variables.</param>
        public void Apply(IRuleSubject subject, params Variable[] variables)
        {
            foreach (var v in variables)
            {
                if (v.Equals(_search))
                    ViolationCount = 0;
            }
        }
    }
}
