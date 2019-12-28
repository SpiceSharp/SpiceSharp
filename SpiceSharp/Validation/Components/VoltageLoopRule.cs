using SpiceSharp.Simulations;
using System.Collections.Generic;

namespace SpiceSharp.Validation
{
    /// <summary>
    /// An implementation for a <see cref="IAppliedVoltageRule"/>. This class
    /// will check for voltage loops.
    /// </summary>
    /// <seealso cref="IAppliedVoltageRule" />
    public class VoltageLoopRule : IAppliedVoltageRule
    {
        private readonly Dictionary<Variable, HashSet<Variable>> _groups = new Dictionary<Variable, HashSet<Variable>>();
        private readonly List<VoltageLoopRuleViolation> _violations = new List<VoltageLoopRuleViolation>();

        /// <summary>
        /// Gets a value indicating whether this <see cref="IRule" /> is being violated.
        /// </summary>
        /// <value>
        ///   <c>true</c> if violated; otherwise, <c>false</c>.
        /// </value>
        public bool IsViolated => _violations.Count > 0;

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
                    // They're different fixed groups, so let's join them together
                    if (groupA.Count < groupB.Count)
                    {
                        foreach (var v in groupA)
                            _groups[v] = groupB;
                        groupB.UnionWith(groupA);
                    }
                    else
                    {
                        foreach (var v in groupB)
                            _groups[v] = groupA;
                        groupA.UnionWith(groupB);
                    }
                }
            }
            else if (hasA)
            {
                groupA.Add(second);
                _groups.Add(second, groupA);
            }
            else if (hasB)
            {
                groupB.Add(first);
                _groups.Add(first, groupB);
            }
            else
            {
                var group = new HashSet<Variable>() { first, second };
                _groups.Add(first, group);
                _groups.Add(second, group);
            }
        }
    }
}
