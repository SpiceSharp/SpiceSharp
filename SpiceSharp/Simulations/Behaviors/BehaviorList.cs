using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// Class representing an ordered list of behaviors.
    /// </summary>
    /// <typeparam name="T">The base behavior type.</typeparam>
    /// <seealso cref="IEnumerable{T}"/>
    public class BehaviorList<T> : IEnumerable<T> where T : IBehavior
    {
        private readonly T[] _behaviors;

        /// <summary>
        /// Gets the behavior at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The behavior at the specified index.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the index exceeds the bounds of the list.</exception>
        public T this[int index] => _behaviors[index];

        /// <summary>
        /// Gets the number of behaviors in the list.
        /// </summary>
        /// <value>
        /// The number of behaviors in the list.
        /// </value>
        public int Count { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BehaviorList{T}"/> class.
        /// </summary>
        /// <param name="behaviors">An enumeration of all behaviors that need to be added.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="behaviors"/> is <c>null</c>.</exception>
        public BehaviorList(IEnumerable<T> behaviors)
        {
            behaviors.ThrowIfNull(nameof(behaviors));

            // Turn into an array
            _behaviors = behaviors.ToArray();
            Count = _behaviors.Length;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)_behaviors).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _behaviors.GetEnumerator();
    }
}
