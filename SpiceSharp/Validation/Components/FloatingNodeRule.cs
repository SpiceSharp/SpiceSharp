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
        /// Gets or sets the fixed-potential node.
        /// </summary>
        /// <value>
        /// The fixed-potential node.
        /// </value>
        public Variable FixedVariable { get; }

        /// <summary>
        /// Gets the groups.
        /// </summary>
        /// <value>
        /// The groups.
        /// </value>
        protected virtual IEnumerable<int> Groups => _groups.Values.Distinct();

        /// <summary>
        /// Gets the number of violations of this rule.
        /// </summary>
        /// <value>
        /// The violation count.
        /// </value>
        public virtual int ViolationCount => Groups.Count() - 1;

        /// <summary>
        /// Gets the violations.
        /// </summary>
        /// <value>
        /// The violations.
        /// </value>
        public virtual IEnumerable<IRuleViolation> Violations
        {
            get
            {
                int bulk;
                if (FixedVariable != null)
                {
                    if (!_groups.TryGetValue(FixedVariable, out bulk))
                        bulk = -2;
                }
                else
                    bulk = -1;
                foreach (var group in Groups)
                {
                    if (bulk == -1)
                        bulk = group;
                    else if (group != bulk)
                        yield return new FloatingNodeRuleViolation(this, GetGroupVariables(group));
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FloatingNodeRule"/> class.
        /// </summary>
        public FloatingNodeRule()
        {
            FixedVariable = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FloatingNodeRule"/> class.
        /// </summary>
        /// <param name="fixedVariable">The fixed-potential variable.</param>
        public FloatingNodeRule(Variable fixedVariable)
        {
            FixedVariable = fixedVariable;
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
        /// Gets the group.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <returns>
        /// The group number assigned to the variable.
        /// </returns>
        protected int GetGroup(Variable variable)
        {
            if (!_groups.TryGetValue(variable, out int result))
            {
                result = _cgroup++;
                _groups.Add(variable, result);
            }
            return result;
        }

        /// <summary>
        /// Tries the get group.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <param name="group">The group.</param>
        /// <returns>
        /// <c>true</c> if the group exists; otherwise <c>false</c>.
        /// </returns>
        protected bool TryGetGroup(Variable variable, out int group) => _groups.TryGetValue(variable, out group);

        /// <summary>
        /// Gets the variables in the specified group.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <returns>
        /// The variables in the specified group.
        /// </returns>
        protected virtual IEnumerable<Variable> GetGroupVariables(int group) => _groups.Where(p => p.Value == group).Select(p => p.Key);

        /// <summary>
        /// Connects the specified groups together. The combined group will get the lowest group index.
        /// </summary>
        /// <param name="groupA">The first group.</param>
        /// <param name="groupB">The second group.</param>
        protected virtual void Connect(int groupA, int groupB)
        {
            if (groupA == groupB)
                return;
            if (groupA < groupB)
            {
                foreach (var variable in _groups.Where(p => p.Value == groupB).Select(p => p.Key).ToArray())
                    _groups[variable] = groupA;
            }
            else
            {
                foreach (var variable in _groups.Where(p => p.Value == groupA).Select(p => p.Key).ToArray())
                    _groups[variable] = groupB;
            }
        }

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
            first.ThrowIfNull(nameof(first));
            second.ThrowIfNull(nameof(second));
            var hasA = _groups.TryGetValue(first, out var groupA);
            var hasB = _groups.TryGetValue(second, out var groupB);
            if (hasA && hasB)
                Connect(groupA, groupB);
            else if (hasA)
                _groups.Add(second, groupA);
            else if (hasB)
                _groups.Add(first, groupB);
            else
            {
                // Create a new group
                _groups.Add(first, _cgroup);
                _groups.Add(second, _cgroup);
                _cgroup++;
            }
        }
    }
}
