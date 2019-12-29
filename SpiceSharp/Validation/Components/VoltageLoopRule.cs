using SpiceSharp.Simulations;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharp.Validation
{
    /// <summary>
    /// An implementation for a <see cref="IAppliedVoltageRule"/>. This class
    /// will check for voltage loops.
    /// </summary>
    /// <seealso cref="IAppliedVoltageRule" />
    public class VoltageLoopRule : IAppliedVoltageRule
    {
        private int _cgroup = 0;
        private readonly Dictionary<Variable, int> _groups = new Dictionary<Variable, int>();
        private readonly List<VoltageLoopRuleViolation> _violations = new List<VoltageLoopRuleViolation>();

        /// <summary>
        /// Gets the number of violations of this rule.
        /// </summary>
        /// <value>
        /// The violation count.
        /// </value>
        public int ViolationCount => _violations.Count;

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
                foreach (var violation in _violations)
                    yield return violation;
            }
        }

        /// <summary>
        /// Applies a fixed-voltage relation between two variables.
        /// </summary>
        /// <param name="subject">The subject that applies to the rule.</param>
        /// <param name="first">The first variable.</param>
        /// <param name="second">The second variable.</param>
        public void Apply(IRuleSubject subject, Variable first, Variable second)
        {
            // If both variables are part of the same fixed-voltage group, then this rule is violated
            bool hasA = _groups.TryGetValue(first, out var groupA);
            bool hasB = _groups.TryGetValue(second, out var groupB);
            if (hasA && hasB)
            {
                if (groupA == groupB)
                    _violations.Add(new VoltageLoopRuleViolation(this, subject, first, second));
                else
                {
                    foreach (var v in _groups.Where(p => p.Value == groupB).Select(p => p.Key).ToArray())
                        _groups[v] = groupA;
                }
            }
            else if (hasA)
                _groups.Add(second, groupA);
            else if (hasB)
                _groups.Add(first, groupB);
            else
            {
                _groups.Add(first, _cgroup);
                _groups.Add(second, _cgroup);
                _cgroup++;
            }
        }
    }
}
