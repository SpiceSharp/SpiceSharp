using SpiceSharp.Simulations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharp.Validation
{
    public partial class FloatingNodeRule
    {
        /// <summary>
        /// A group of variables connected by an unconditional conductive path.
        /// </summary>
        /// <seealso cref="IEnumerable{Variable}" />
        protected class Group : IEnumerable<Variable>
        {
            private readonly HashSet<Variable> _variables = new HashSet<Variable>();
            private readonly Dictionary<Group, ConductionTypes> _paths = new Dictionary<Group, ConductionTypes>();

            /// <summary>
            /// Gets the number of variables in the group.
            /// </summary>
            /// <value>
            /// The count.
            /// </value>
            public int Count => _variables.Count;

            /// <summary>
            /// Initializes a new instance of the <see cref="Group"/> class.
            /// </summary>
            /// <param name="variables">The variables.</param>
            public Group(params Variable[] variables)
            {
                foreach (var variable in variables)
                    _variables.Add(variable);
            }

            /// <summary>
            /// Determines whether the variable is part of the group.
            /// </summary>
            /// <param name="variable">The variable.</param>
            /// <returns>
            ///   <c>true</c> if the group contains the variable; otherwise, <c>false</c>.
            /// </returns>
            public bool Contains(Variable variable) => _variables.Contains(variable);

            /// <summary>
            /// Adds the variable.
            /// </summary>
            /// <param name="variable">The variable.</param>
            protected void AddVariable(Variable variable)
            {
                _variables.Add(variable);
            }

            /// <summary>
            /// Adds the specified variable.
            /// </summary>
            /// <param name="variable">The variable.</param>
            public void Add(Variable variable) => _variables.Add(variable);

            /// <summary>
            /// Adds a path to another group.
            /// </summary>
            /// <param name="other">The other group.</param>
            /// <param name="type">The type.</param>
            /// <returns>
            /// <c>true</c> if the path caused the other group to be usurped; otherwise <c>false</c>.
            /// </returns>
            protected bool AddPath(Group other, ConductionTypes type)
            {
                if (type != ConductionTypes.All)
                {
                    if (_paths.TryGetValue(other, out var existing) || other._paths.TryGetValue(this, out existing))
                        type |= existing;
                }

                if (type == ConductionTypes.All)
                {
                    // We need to join these two groups
                    foreach (var variable in other)
                        _variables.Add(variable);
                    _paths.Remove(other); // Obsolete path
                    return true;
                }
                else
                {
                    _paths[other] = type;
                    other._paths[this] = type;
                }
                return false;
            }

            /// <summary>
            /// Returns an enumerator that iterates through the collection.
            /// </summary>
            /// <returns>
            /// An enumerator that can be used to iterate through the collection.
            /// </returns>
            public IEnumerator<Variable> GetEnumerator() => _variables.GetEnumerator();

            /// <summary>
            /// Returns an enumerator that iterates through a collection.
            /// </summary>
            /// <returns>
            /// An <see cref="IEnumerator" /> object that can be used to iterate through the collection.
            /// </returns>
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            /// <summary>
            /// Connects two groups together.
            /// </summary>
            /// <param name="group1">The first group.</param>
            /// <param name="group2">The second group.</param>
            /// <param name="type">The path type.</param>
            /// <param name="removeGroup">Called when a group should be removed.</param>
            public static void Connect(Group group1, Group group2, ConductionTypes type, RemoveGroup removeGroup)
            {
                // Check if there is already a path from one group to the other
                if (type != ConductionTypes.All)
                {
                    // Paths are reciprocal, so we can get away with checking one group
                    if (group1._paths.TryGetValue(group2, out var existing))
                        type |= existing;
                }

                // Can we join the two groups?
                if (type == ConductionTypes.All)
                {
                    // Take the smallest group to join with the larger group
                    Group newGroup;
                    if (group1.Count < group2.Count)
                    {
                        newGroup = group2;
                        removeGroup(group1, group2);
                        foreach (var variable in group1)
                            group2._variables.Add(variable);
                    }
                    else
                    {
                        newGroup = group1;
                        removeGroup(group2, group1);
                        foreach (var variable in group2)
                            group1._variables.Add(variable);
                    }

                    // Resolve paths
                    if (group1._paths.Count < group2._paths.Count)
                    {
                        // We're calling recursively here, so we need to convert to an array
                        foreach (var path in group1._paths.ToArray())
                        {
                            if (group2._paths.TryGetValue(path.Key, out var common))
                                Connect(path.Key, newGroup, path.Value | common, removeGroup);
                        }
                    }
                    else
                    {
                        // We're calling recursively here, so we need to convert to an array
                        foreach (var path in group2._paths.ToArray())
                        {
                            if (group1._paths.TryGetValue(path.Key, out var common))
                                Connect(path.Key, newGroup, path.Value | common, removeGroup);
                        }
                    }
                }
                else if (type != ConductionTypes.None)
                {
                    // Create a path between these groups
                    group1._paths[group2] = type;
                    group2._paths[group1] = type;
                }
            }

            /// <summary>
            /// A method for removing a group after joining them.
            /// </summary>
            /// <param name="oldGroup">The old group.</param>
            /// <param name="newGroup">The new group.</param>
            public delegate void RemoveGroup(Group oldGroup, Group newGroup);
        }
    }
}
