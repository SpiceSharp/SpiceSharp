using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// Ordered list of behaviors
    /// </summary>
    public class BehaviorList<T> where T : Behavior
    {
        /// <summary>
        /// Behaviors
        /// </summary>
        private readonly T[] _behaviors;

        /// <summary>
        /// Gets a behavior at a specific index
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns></returns>
        public T this[int index] => _behaviors[index];

        /// <summary>
        /// Gets the number of behaviors in the list
        /// </summary>
        public int Count { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="behaviors"></param>
        public BehaviorList(IEnumerable<T> behaviors)
        {
            if (behaviors == null)
                throw new ArgumentNullException(nameof(behaviors));

            // Turn into an array
            _behaviors = behaviors.ToArray();
            Count = _behaviors.Length;
        }
    }
}
