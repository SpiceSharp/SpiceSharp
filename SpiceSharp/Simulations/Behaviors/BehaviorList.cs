using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// Class representing an ordered list of behaviors.
    /// </summary>
    /// <typeparam name="T">The base behavior type.</typeparam>
    public class BehaviorList<T> : IEnumerable<T> where T : IBehavior
    {
        /// <summary>
        /// Behaviors
        /// </summary>
        private readonly T[] _behaviors;

        /// <summary>
        /// Gets the behavior at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public T this[int index] => _behaviors[index];

        /// <summary>
        /// Gets the number of behaviors in the list.
        /// </summary>
        public int Count { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BehaviorList{T}"/> class.
        /// </summary>
        /// <param name="behaviors">An enumeration of all behaviors that need to be added.</param>
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

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator() => _behaviors.GetEnumerator();
    }
}
