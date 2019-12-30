using SpiceSharp.Simulations;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharp.Validation
{
    public partial class FloatingNodeRule
    {
        /// <summary>
        /// The graph of variables
        /// </summary>
        protected class Graph
        {
            private readonly Dictionary<Variable, Group> _groups = new Dictionary<Variable, Group>();

            /// <summary>
            /// Gets the number of groups in the graph.
            /// </summary>
            /// <value>
            /// The group count.
            /// </value>
            public int Count { get; private set; }

            /// <summary>
            /// Gets the groups.
            /// </summary>
            /// <value>
            /// The groups.
            /// </value>
            public IEnumerable<Group> Groups
            {
                get
                {
                    foreach (var group in _groups.Values.Distinct())
                        yield return group;
                }
            }

            /// <summary>
            /// Adds the specified variable.
            /// </summary>
            /// <param name="variable">The variable.</param>
            public void Add(Variable variable)
            {
                if (!_groups.ContainsKey(variable))
                {
                    _groups.Add(variable, new Group(variable));
                    Count++;
                }
            }

            /// <summary>
            /// Determines whether this graph contains the specified variable.
            /// </summary>
            /// <param name="variable">The variable.</param>
            /// <returns>
            ///   <c>true</c> if the graph contains the node; otherwise, <c>false</c>.
            /// </returns>
            public bool Contains(Variable variable) => _groups.ContainsKey(variable);

            /// <summary>
            /// Connects the specified nodes through a path.
            /// </summary>
            /// <param name="node1">The first node.</param>
            /// <param name="node2">The second node.</param>
            /// <param name="type">The conductive path type.</param>
            public void Connect(Variable node1, Variable node2, ConductionTypes type)
            {
                var has1 = _groups.TryGetValue(node1, out var group1);
                var has2 = _groups.TryGetValue(node2, out var group2);

                if (has1 && has2)
                {
                    // Are the groups already the same?
                    if (group1 == group2)
                        return;
                    Group.Connect(group1, group2, type, RemoveGroup);
                }
                else if (type == ConductionTypes.All)
                {
                    // Common: unconditional connection
                    if (has1)
                    {
                        group1.Add(node2);
                        _groups.Add(node2, group1);
                    }
                    else if (has2)
                    {
                        group2.Add(node1);
                        _groups.Add(node1, group2);
                    }
                    else
                    {
                        group1 = new Group(node1, node2);
                        _groups.Add(node1, group1);
                        _groups.Add(node2, group1);
                        Count++;
                    }
                }
                else
                {
                    if (has1)
                    {
                        group2 = new Group(node2);
                        _groups.Add(node2, group2);
                        Count++;
                    }
                    else if (has2)
                    {
                        group1 = new Group(node1);
                        _groups.Add(node1, group1);
                        Count++;
                    }
                    else
                    {
                        group1 = new Group(node1);
                        _groups.Add(node1, group1);
                        group2 = new Group(node2);
                        _groups.Add(node2, group2);
                        Count += 2;
                    }
                    Group.Connect(group1, group2, type, RemoveGroup);
                }
            }

            /// <summary>
            /// Removes the group.
            /// </summary>
            /// <param name="oldGroup">The old group.</param>
            /// <param name="newGroup">The new group.</param>
            private void RemoveGroup(Group oldGroup, Group newGroup)
            {
                foreach (Variable variable in oldGroup)
                {
                    newGroup.Add(variable);
                    _groups[variable] = newGroup;
                }

                // We just removed all references to a group
                Count--;
            }
        }
    }
}
