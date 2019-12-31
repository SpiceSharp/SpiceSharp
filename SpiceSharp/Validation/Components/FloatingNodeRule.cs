using SpiceSharp.Simulations;
using System;
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
        private readonly Dictionary<Variable, int> _dcGroup = new Dictionary<Variable, int>(), _acGroup = new Dictionary<Variable, int>();
        private int _dcGroups = 1, _acGroups = 1;

        /// <summary>
        /// Gets or sets the fixed-potential node.
        /// </summary>
        /// <value>
        /// The fixed-potential node.
        /// </value>
        public Variable FixedVariable { get; }

        /// <summary>
        /// Gets the number of violations of this rule.
        /// </summary>
        /// <value>
        /// The violation count.
        /// </value>
        public int ViolationCount => Math.Max(_dcGroups, _acGroups) - 1;

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
                foreach (var pair in _dcGroup)
                {
                    ConductionTypes type = ConductionTypes.None;
                    if (pair.Value == 0)
                        type |= ConductionTypes.Dc;
                    if (_acGroup[pair.Key] == 0)
                        type |= ConductionTypes.Ac;
                    if (type != ConductionTypes.All)
                        yield return new FloatingNodeRuleViolation(this, pair.Key, FixedVariable, type);
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FloatingNodeRule"/> class.
        /// </summary>
        /// <param name="fixedVariable">The fixed-potential variable.</param>
        public FloatingNodeRule(Variable fixedVariable)
        {
            FixedVariable = fixedVariable.ThrowIfNull(nameof(fixedVariable));
            _dcGroup.Add(fixedVariable, 0);
            _acGroup.Add(fixedVariable, 0);
        }

        /// <summary>
        /// Determines whether this rule encountered the specified variable.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <returns>
        ///   <c>true</c> if the specified variable was encountered; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(Variable variable) => _dcGroup.ContainsKey(variable);

        /// <summary>
        /// Applies the specified variables as being connected by a conductive path.
        /// </summary>
        /// <param name="subject">The rule subject.</param>
        /// <param name="variables">The variables.</param>
        public void AddPath(IRuleSubject subject, params Variable[] variables)
        {
            AddPath(subject, ConductionTypes.All, variables);
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
            if (variables.Length == 1 || type == ConductionTypes.None)
            {
                foreach (var variable in variables)
                {
                    Add(variable, _dcGroup, ref _dcGroups);
                    Add(variable, _acGroup, ref _acGroups);
                }
            }
            else
            {
                for (var i = 0; i < variables.Length; i++)
                {
                    for (var j = i + 1; j < variables.Length; j++)
                        AddPath(variables[i], variables[j], type);
                }
            }
        }

        /// <summary>
        /// Adds a path between two variables.
        /// </summary>
        /// <param name="a">The first variable.</param>
        /// <param name="b">The second variable.</param>
        /// <param name="type">The path type.</param>
        private void AddPath(Variable a, Variable b, ConductionTypes type)
        {
            if (type == ConductionTypes.None)
                throw new SpiceSharpException("Invalid path");
            if ((type & ConductionTypes.Dc) != 0)
                Connect(a, b, _dcGroup, ref _dcGroups);
            if ((type & ConductionTypes.Ac) != 0)
                Connect(a, b, _acGroup, ref _acGroups);
        }

        /// <summary>
        /// Connects the specified variables for a group.
        /// </summary>
        /// <param name="a">The first variable.</param>
        /// <param name="b">The second variable.</param>
        /// <param name="group">The group.</param>
        /// <param name="counter">The counter to keep track of the number of distinct groups.</param>
        private void Connect(Variable a, Variable b, Dictionary<Variable, int> group, ref int counter)
        {
            // Add to DC group
            var hasA = group.TryGetValue(a, out var groupA);
            var hasB = group.TryGetValue(b, out var groupB);
            if (hasA && hasB)
            {
                // Join the groups
                if (groupA != groupB)
                {
                    if (groupA < groupB)
                    {
                        foreach (var pair in group.Where(p => p.Value == groupB).ToArray())
                            group[pair.Key] = groupA;
                    }
                    else
                    {
                        foreach (var pair in _dcGroup.Where(p => p.Value == groupA).ToArray())
                            group[pair.Key] = groupB;
                    }
                    counter--;
                }
            }
            else if (hasA)
                group.Add(b, groupA);
            else if (hasB)
                group.Add(a, groupB);
            else
            {
                var index = group.Count;
                group.Add(a, index);
                group.Add(b, index);
                counter++;
            }
        }

        /// <summary>
        /// Adds the specified variable as a new group.
        /// </summary>
        /// <param name="a">The variable.</param>
        /// <param name="group">The group.</param>
        /// <param name="counter">The counter to keep track of the number of distinct groups.</param>
        private void Add(Variable a, Dictionary<Variable, int> group, ref int counter)
        {
            if (group.ContainsKey(a))
                return;
            group.Add(a, group.Count);
            counter++;
        }
    }
}
