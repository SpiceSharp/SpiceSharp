using System.Collections.Generic;

namespace SpiceSharp.Circuits
{
    /// <summary>
    /// A circuit for which the iteration of entities can be ordered.
    /// </summary>
    /// <seealso cref="SpiceSharp.Circuit" />
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
        public IComparer<Entity> EntityComparer { get; set; }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public override IEnumerator<Entity> GetEnumerator()
        {
            var list = new List<Entity>(Count);
            foreach (var e in this)
                list.Add(e);

            if (EntityComparer != null)
                list.Sort(EntityComparer);

            foreach (var e in list)
                yield return e;
        }
    }
}
