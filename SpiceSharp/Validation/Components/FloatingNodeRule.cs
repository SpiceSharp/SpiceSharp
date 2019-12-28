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
        private readonly Dictionary<Variable, HashSet<Variable>> _groups = new Dictionary<Variable, HashSet<Variable>>();

        /// <summary>
        /// Gets a value indicating whether this <see cref="IRule" /> is being violated.
        /// </summary>
        /// <value>
        ///   <c>true</c> if violated; otherwise, <c>false</c>.
        /// </value>
        public bool IsViolated
        {
            get
            {
                HashSet<Variable> bulk = null;
                foreach (var group in _groups.Values)
                {
                    if (bulk == null)
                        bulk = group;
                    else if (bulk != group)
                        return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Gets or sets the violations.
        /// </summary>
        /// <value>
        /// The violations.
        /// </value>
        public IEnumerable<IRuleViolation> Violations
        {
            get
            {
                HashSet<Variable> bulk = null;
                int bulkCount = 0;
                foreach (var group in _groups.Values.Distinct())
                {
                    // The new group is essentially the bulk of the circuit
                    if (group.Count > bulkCount)
                    {
                        if (bulk != null)
                            yield return new FloatingNodeRuleViolation(this, bulk);
                        bulk = group;
                        bulkCount = group.Count;
                    }
                    else if (group != bulk)
                        yield return new FloatingNodeRuleViolation(this, group);
                }
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
            if (variables.Length == 1)
                _groups.Add(variables[0], new HashSet<Variable>() { variables[0] });

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
                // Join the groups
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
