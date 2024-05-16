using SpiceSharp.Simulations;
using SpiceSharp.Validation.Components;
using System;
using System.Collections.Generic;

namespace SpiceSharp.Validation
{
    /// <summary>
    /// An <see cref="IConductiveRule"/> that checks for the presence of a floating node.
    /// </summary>
    /// <seealso cref="IConductiveRule" />
    public partial class FloatingNodeRule : IConductiveRule
    {
        private readonly Group _representative;
        private readonly Dictionary<IVariable, Group> _dcGroups = [], _acGroups = [];
        private int _dcGroupCount, _acGroupCount;

        /// <summary>
        /// Gets or sets the fixed-potential node.
        /// </summary>
        /// <value>
        /// The fixed-potential node.
        /// </value>
        public IVariable FixedVariable { get; }

        /// <summary>
        /// Gets the number of violations of this rule.
        /// </summary>
        /// <value>
        /// The violation count.
        /// </value>
        public int ViolationCount => Math.Max(_dcGroupCount, _acGroupCount) - 1;

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
                foreach (var pair in _dcGroups)
                {
                    ConductionTypes type = ConductionTypes.None;
                    if (pair.Value == _representative)
                        type |= ConductionTypes.Dc;
                    if (_acGroups[pair.Key] == _representative)
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
        public FloatingNodeRule(IVariable fixedVariable)
        {
            FixedVariable = fixedVariable.ThrowIfNull(nameof(fixedVariable));
            _representative = new Group(fixedVariable);
            _dcGroups.Add(fixedVariable, _representative); _dcGroupCount = 1;
            _acGroups.Add(fixedVariable, _representative); _acGroupCount = 1;
        }

        /// <summary>
        /// Resets the rule.
        /// </summary>
        public void Reset()
        {
            _acGroups.Clear();
            _acGroups.Add(FixedVariable, _representative);
            _acGroupCount = 1;

            _dcGroups.Clear();
            _dcGroups.Add(FixedVariable, _representative);
            _dcGroupCount = 1;
        }

        /// <summary>
        /// Determines whether this rule encountered the specified variable.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <returns>
        ///   <c>true</c> if the specified variable was encountered; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(Variable variable) => _dcGroups.ContainsKey(variable);

        /// <summary>
        /// Applies the specified variables as being connected by a conductive path.
        /// </summary>
        /// <param name="subject">The rule subject.</param>
        /// <param name="variables">The variables.</param>
        public void AddPath(IRuleSubject subject, params IVariable[] variables)
        {
            AddPath(subject, ConductionTypes.All, variables);
        }

        /// <summary>
        /// Specifies variables as being connected by a conductive path of the specified type.
        /// </summary>
        /// <param name="subject">The subject that applies the conductive paths.</param>
        /// <param name="type">The type of path between these variables.</param>
        /// <param name="variables">The variables that are connected.</param>
        public void AddPath(IRuleSubject subject, ConductionTypes type, params IVariable[] variables)
        {
            if (variables == null || variables.Length == 0)
                return;
            if (variables.Length == 1 || type == ConductionTypes.None)
            {
                foreach (var variable in variables)
                {
                    Add(variable, _dcGroups, ref _dcGroupCount);
                    Add(variable, _acGroups, ref _acGroupCount);
                }
            }
            else
            {
                for (int i = 0; i < variables.Length; i++)
                {
                    for (int j = i + 1; j < variables.Length; j++)
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
        private void AddPath(IVariable a, IVariable b, ConductionTypes type)
        {
            if (type == ConductionTypes.None)
                throw new SpiceSharpException("Invalid path");
            if ((type & ConductionTypes.Dc) != 0)
                Connect(a, b, _dcGroups, ref _dcGroupCount);
            else
            {
                Add(a, _dcGroups, ref _dcGroupCount);
                Add(b, _dcGroups, ref _dcGroupCount);
            }
            if ((type & ConductionTypes.Ac) != 0)
                Connect(a, b, _acGroups, ref _acGroupCount);
            else
            {
                Add(a, _acGroups, ref _acGroupCount);
                Add(b, _acGroups, ref _acGroupCount);
            }
        }

        /// <summary>
        /// Connects the specified variables for a group.
        /// </summary>
        /// <param name="a">The first variable.</param>
        /// <param name="b">The second variable.</param>
        /// <param name="groups">The group.</param>
        /// <param name="counter">The counter to keep track of the number of distinct groups.</param>
        private void Connect(IVariable a, IVariable b, Dictionary<IVariable, Group> groups, ref int counter)
        {
            // Add to DC group
            bool hasA = groups.TryGetValue(a, out var groupA);
            bool hasB = groups.TryGetValue(b, out var groupB);
            if (hasA && hasB)
            {
                // Join the groups
                if (groupA != groupB)
                {
                    if (groupA == _representative)
                    {
                        foreach (var variable in groupB)
                            groups[variable] = _representative;
                    }
                    else if (groupB == _representative)
                    {
                        foreach (var variable in groupA)
                            groups[variable] = _representative;
                    }
                    else
                    {
                        if (groupA.Count < groupB.Count)
                        {
                            foreach (var variable in groupA)
                                groups[variable] = groupB;
                            groupB.Join(groupA);
                        }
                        else
                        {
                            foreach (var variable in groupB)
                                groups[variable] = groupA;
                            groupA.Join(groupB);
                        }
                    }
                    counter--;
                }
            }
            else if (hasA)
            {
                groups.Add(b, groupA);
                if (groupA != _representative)
                    groupA.Add(b);
            }
            else if (hasB)
            {
                groups.Add(a, groupB);
                if (groupB != _representative)
                    groupB.Add(a);
            }
            else
            {
                var group = new Group(a, b);
                groups[a] = group;
                groups[b] = group;
                counter++;
            }
        }

        /// <summary>
        /// Adds the specified variable as a new group.
        /// </summary>
        /// <param name="a">The variable.</param>
        /// <param name="groups">The group.</param>
        /// <param name="counter">The counter to keep track of the number of distinct groups.</param>
        private static void Add(IVariable a, Dictionary<IVariable, Group> groups, ref int counter)
        {
            if (groups.ContainsKey(a))
                return;
            groups.Add(a, new Group(a));
            counter++;
        }
    }
}
