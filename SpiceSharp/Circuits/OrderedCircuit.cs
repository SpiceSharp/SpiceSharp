using System.Collections.Generic;
using SpiceSharp.Entities;

namespace SpiceSharp
{
    /// <summary>
    /// A circuit for which the iteration of entities can be ordered.
    /// </summary>
    /// <seealso cref="Circuit" />
    /// <remarks>
    /// Entity behaviors will be created and initially executed in the same order.
    /// This type of circuit can be used to speed up allocation, or to optimize
    /// numerical accuracy. 
    /// </remarks>
    public class OrderedCircuit : Circuit
    {
        /// <summary>
        /// Gets or sets the entity comparer for ordering entities before iteration.
        /// </summary>
        public IComparer<IEntity> EntityComparer { get; set; }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public override IEnumerator<IEntity> GetEnumerator()
        {
            var list = new List<IEntity>(Count);
            foreach (var e in this)
                list.Add(e);

            if (EntityComparer != null)
                list.Sort(EntityComparer);

            foreach (var e in list)
                yield return e;
        }
    }
}
