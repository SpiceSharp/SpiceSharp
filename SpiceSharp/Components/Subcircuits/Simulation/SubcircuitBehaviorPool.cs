using System.Collections.Generic;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.SubcircuitBehaviors
{
    /// <summary>
    /// A behavior pool for a <see cref="SubcircuitSimulation"/>.
    /// </summary>
    /// <remarks>
    /// The pool first tries to find a behavior in the subcircuit, but if it can't find it, it
    /// will forward the behavior from the parent simulation.
    /// </remarks>
    /// <seealso cref="SpiceSharp.Behaviors.BehaviorContainerCollection" />
    public class SubcircuitBehaviorContainerCollection : BehaviorContainerCollection
    {
        private BehaviorContainerCollection _parent;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubcircuitBehaviorContainerCollection"/> class.
        /// </summary>
        /// <param name="comparer">The comparer.</param>
        /// <param name="parentPool">The parent behavior pool.</param>
        public SubcircuitBehaviorContainerCollection(IEqualityComparer<string> comparer, BehaviorContainerCollection parentPool) 
            : base(comparer, parentPool.Types)
        {
            _parent = parentPool.ThrowIfNull(nameof(parentPool));
        }

        /// <summary>
        /// Gets the <see cref="BehaviorContainer"/> with the specified name.
        /// </summary>
        /// <value>
        /// The <see cref="BehaviorContainer"/>.
        /// </value>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public override IBehaviorContainer this[string name]
        {
            get
            {
                if (base.ContainsKey(name))
                    return base[name];

                // We expect it to exist, but it doesn't, so let's ask the parent simulation
                // if it knows more
                return _parent[name];
            }
        }
    }
}
