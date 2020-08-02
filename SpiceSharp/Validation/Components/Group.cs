using SpiceSharp.Simulations;
using System.Collections;
using System.Collections.Generic;

namespace SpiceSharp.Validation.Components
{
    /// <summary>
    /// A group of variables.
    /// </summary>
    /// <seealso cref="IEnumerable{Variable}" />
    public class Group : IEnumerable<IVariable>
    {
        private class Node
        {
            public IVariable Variable { get; }
            public Node Next { get; set; }
            public Node(IVariable variable) { Variable = variable.ThrowIfNull(nameof(variable)); }
        }
        private readonly Node _first;
        private Node _last;

        /// <summary>
        /// Gets the number of nodes.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        public int Count { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Group"/> class.
        /// </summary>
        /// <param name="first">The first variable.</param>
        /// <param name="variables">Any other variables in the group.</param>
        public Group(IVariable first, params IVariable[] variables)
        {
            _first = new Node(first);
            _last = _first;
            Count = 1;
            foreach (var variable in variables)
            {
                _last.Next = new Node(variable);
                _last = _last.Next;
                Count++;
            }
        }

        /// <summary>
        /// Adds the specified variable.
        /// </summary>
        /// <param name="variable">The variable.</param>
        public void Add(IVariable variable)
        {
            variable.ThrowIfNull(nameof(variable));
            _last.Next = new Node(variable);
            _last = _last.Next;
            Count++;
        }

        /// <summary>
        /// Joins the specified group.
        /// </summary>
        /// <param name="group">The group.</param>
        public void Join(Group group)
        {
            // Don't need to join elements if the group is a representative
            _last.Next = group._first;
            Count += group.Count;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<IVariable> GetEnumerator()
        {
            var elt = _first;
            while (elt != null)
            {
                yield return elt.Variable;
                elt = elt.Next;
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
