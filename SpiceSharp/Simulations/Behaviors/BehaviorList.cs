using System.Collections.Generic;
using System.Linq;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// Class representing an ordered list of behaviors.
    /// </summary>
    /// <typeparam name="T">The base behavior type.</typeparam>
    public class BehaviorList<T> where T : IBehavior
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
    }
}
