using SpiceSharp.Simulations;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharp.Validation
{
    /// <summary>
    /// An <see cref="IConductiveRule"/> that checks for the presence of a floating node.
    /// </summary>
    /// <seealso cref="IConductiveRule" />
    public class FloatingNodeRule : IConductiveRule
    {
        int _cgroup = 0;
        private readonly Dictionary<Variable, int> _groups = new Dictionary<Variable, int>();

        /// <summary>
        /// Gets the number of violations of this rule.
        /// </summary>
        /// <value>
        /// The violation count.
        /// </value>
        public int ViolationCount => _groups.Values.Distinct().Count() - 1;

        /// <summary>
        /// Gets the violations.
        /// </summary>
        /// <value>
        /// The violations.
        /// </value>
        public IEnumerable<IRuleViolation> Violations
        {
            get
            {
                int bulk = -1;
                int bulkCount = 0;
                foreach (var group in _groups.Values.Distinct())
                {
                    var count = _groups.Count(p => p.Value == group);
                    if (count > bulkCount)
                    {
                        if (bulk >= 0)
                            yield return new FloatingNodeRuleViolation(this, _groups.Where(p => p.Value == bulk).Select(p => p.Key));
                        bulk = group;
                        bulkCount = count;
                    }
                    else if (group != bulk)
                        yield return new FloatingNodeRuleViolation(this, _groups.Where(p => p.Value == group).Select(p => p.Key));
                }
            }
        }

        /// <summary>
        /// Gets the violations.
        /// </summary>
        /// <param name="bulkNode">A node that is part of the bulk circuit.</param>
        /// <returns>
        /// Rule violations for any group not part of the bulk group.
        /// </returns>
        public IEnumerable<IRuleViolation> GetViolations(Variable bulkNode)
        {
            _groups.TryGetValue(bulkNode, out var bulk);
            foreach (var group in _groups.Values.Distinct())
            {
                if (group != bulk)
                    yield return new FloatingNodeRuleViolation(this, _groups.Where(p => p.Value == group).Select(p => p.Key));
            }
        }

        /// <summary>
        /// Determines whether this rule encountered the specified variable.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <returns>
        ///   <c>true</c> if the specified variable was encountered; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(Variable variable) => _groups.ContainsKey(variable);

        /// <summary>
        /// Applies the specified variables as being connected by a conductive path.
        /// </summary>
        /// <param name="subject">The rule subject.</param>
        /// <param name="variables">The variables.</param>
        public void Apply(IRuleSubject subject, params Variable[] variables)
        {
            if (variables == null || variables.Length == 0)
                return;
            if (variables.Length == 1 && !_groups.ContainsKey(variables[0]))
                _groups[variables[0]] = _cgroup++;

            for (var i = 0; i < variables.Length; i++)
            {
                for (var j = i + 1; j < variables.Length; j++)
                    Apply(variables[i], variables[j]);
            }
        }

        /// <summary>
        /// Applies a conductive path between the two variables.
        /// </summary>
        /// <param name="first">The first.</param>
        /// <param name="second">The second.</param>
        private void Apply(Variable first, Variable second)
        {
            var hasA = _groups.TryGetValue(first, out var groupA);
            var hasB = _groups.TryGetValue(second, out var groupB);
            if (hasA && hasB)
            {
                if (groupA != groupB)
                {
                    // Join the groups
                    foreach (var v in _groups.Where(p => p.Value == groupB).Select(p => p.Key).ToArray())
                        _groups[v] = groupA;
                }
            }
            else if (hasA)
                _groups[second] = groupA;
            else if (hasB)
                _groups[first] = groupB;
            else
            {
                _groups.Add(first, _cgroup);
                _groups.Add(second, _cgroup);
                _cgroup++;
            }
        }
    }
}
