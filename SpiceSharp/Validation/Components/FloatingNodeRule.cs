using SpiceSharp.Simulations;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharp.Validation
{
    /// <summary>
    /// An <see cref="IConductiveRule"/> that checks for the presence of a floating node.
    /// </summary>
    /// <seealso cref="IConductiveRule" />
    public partial class FloatingNodeRule : IConductiveRule
    {
        int _cgroup = 0;
        private readonly Dictionary<Variable, int> _groups = new Dictionary<Variable, int>();
        private readonly Dictionary<Path, ConductionTypes> _paths = new Dictionary<Path, ConductionTypes>();

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
                foreach (var group in Groups)
                {
                    if (group > 0)
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
            _groups.Add(FixedVariable, _cgroup++);
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
        /// Gets the variables in the specified group.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <returns>
        /// The variables in the specified group.
        /// </returns>
        protected virtual IEnumerable<Variable> GetGroupVariables(int group) => _groups.Where(p => p.Value == group).Select(p => p.Key);

        /// <summary>
        /// Connects two existing groups together. The combined group will get the lowest group index.
        /// </summary>
        /// <param name="groupA">The first group.</param>
        /// <param name="groupB">The second group.</param>
        /// <param name="type">The conduction path type.</param>
        protected virtual void Connect(int groupA, int groupB, ConductionTypes type)
        {
            if (groupA == groupB)
                return;

            // Modify the type with what's already been specified.
            var key = new Path(groupA, groupB);
            if (_paths.TryGetValue(key, out var existing))
                type |= existing;

            // If the two groups became unconditionally conducting, then just merge the groups
            if (type == ConductionTypes.All)
            {
                // The path becomes obsolete
                if (_paths.ContainsKey(key))
                    _paths.Remove(key);

                // Try to make it a little bit easier
                int oldGroup, newGroup;
                if (groupA < groupB)
                {
                    oldGroup = groupB;
                    newGroup = groupA;
                }
                else
                {
                    oldGroup = groupA;
                    newGroup = groupB;
                }

                // Map all variables from the old group to the new one
                foreach (var variable in _groups.Where(p => p.Value == oldGroup).Select(p => p.Key).ToArray())
                    _groups[variable] = newGroup;

                // Combine any unresolved paths that will now short together
                foreach (var path in _paths.ToArray())
                {
                    // Combine the path with an existing one
                    if (path.Key.Group1 == oldGroup)
                    {
                        _paths.Remove(path.Key);
                        var nkey = new Path(path.Key.Group2, newGroup);
                        if (_paths.TryGetValue(nkey, out var ct))
                        {
                            ct |= path.Value;
                            Connect(path.Key.Group2, newGroup, ct);
                        }
                        else
                            _paths.Add(nkey, path.Value);
                    }
                    else if (path.Key.Group2 == oldGroup)
                    {
                        _paths.Remove(path.Key);
                        var nkey = new Path(path.Key.Group1, newGroup);
                        if (_paths.TryGetValue(nkey, out var ct))
                        {
                            ct |= path.Value;
                            Connect(path.Key.Group1, newGroup, ct);
                        }
                        else
                            _paths.Add(nkey, path.Value);
                    }
                }
            }
            else
            {
                // We can't merge the groups, we just modify the bridge between these two groups
                _paths[key] = type;
            }
        }

        /// <summary>
        /// Applies the specified variables as being connected by a conductive path.
        /// </summary>
        /// <param name="subject">The rule subject.</param>
        /// <param name="variables">The variables.</param>
        public void AddPath(IRuleSubject subject, params Variable[] variables)
        {
            if (variables == null || variables.Length == 0)
                return;
            if (variables.Length == 1 && !_groups.ContainsKey(variables[0]))
                _groups[variables[0]] = _cgroup++;

            for (var i = 0; i < variables.Length; i++)
            {
                for (var j = i + 1; j < variables.Length; j++)
                    Apply(variables[i], variables[j], ConductionTypes.All);
            }
        }

        /// <summary>
        /// Specifies variables as being connected by a conductive path of the specified type.
        /// </summary>
        /// <param name="subject">The subject that applies the conductive paths.</param>
        /// <param name="type">The type of path between these variables.</param>
        /// <param name="variables">The variables that are connected.</param>
        public void AddPath(IRuleSubject subject, ConductionTypes type, params Variable[] variables)
        {
            if (variables == null || variables.Length == 0)
                return;
            if (variables.Length == 1 && !_groups.ContainsKey(variables[0]))
                _groups[variables[0]] = _cgroup++;

            for (var i = 0; i < variables.Length; i++)
            {
                for (var j = i + 1; j < variables.Length; j++)
                    Apply(variables[i], variables[j], type);
            }
        }

        /// <summary>
        /// Applies a conductive path between the two variables.
        /// </summary>
        /// <param name="first">The first.</param>
        /// <param name="second">The second.</param>
        /// <param name="type">The conduction type.</param>
        private void Apply(Variable first, Variable second, ConductionTypes type)
        {
            first.ThrowIfNull(nameof(first));
            second.ThrowIfNull(nameof(second));
            var hasA = _groups.TryGetValue(first, out var groupA);
            var hasB = _groups.TryGetValue(second, out var groupB);
            if (hasA && hasB)
                Connect(groupA, groupB, type);
            else if (hasA)
            {
                if (type == ConductionTypes.All)
                    _groups.Add(second, groupA);
                else
                {
                    if (type != ConductionTypes.None)
                        _paths.Add(new Path(groupA, _cgroup), type);
                    _groups.Add(second, _cgroup++);
                }
            }
            else if (hasB)
            {
                if (type == ConductionTypes.All)
                    _groups.Add(first, groupB);
                else
                {
                    if (type != ConductionTypes.None)
                        _paths.Add(new Path(groupB, _cgroup), type);
                    _groups.Add(first, _cgroup++);
                }
            }
            else
            {
                if (type == ConductionTypes.All)
                {
                    _groups.Add(first, _cgroup);
                    _groups.Add(second, _cgroup++);
                }
                else
                {
                    if (type != ConductionTypes.None)
                        _paths.Add(new Path(_cgroup, _cgroup + 1), type);
                    _groups.Add(first, _cgroup++);
                    _groups.Add(second, _cgroup++);
                }
            }
        }
    }
}
