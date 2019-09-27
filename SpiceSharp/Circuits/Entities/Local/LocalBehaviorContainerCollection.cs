using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Entities.Local
{
    /// <summary>
    /// A <see cref="BehaviorContainerCollection"/> designed to intercept locally created behaviors.
    /// </summary>
    /// <remarks>
    /// When the collection is asked for behaviors that it can't find, it will return
    /// behaviors from the parent collection instead of throwing an exception.
    /// </remarks>
    /// <seealso cref="BehaviorContainerCollection" />
    public class LocalBehaviorContainerCollection : BehaviorContainerCollection
    {
        private BehaviorContainerCollection _parent;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalBehaviorContainerCollection"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        public LocalBehaviorContainerCollection(BehaviorContainerCollection parent)
            : base(parent.Comparer, parent.Types)
        {
            _parent = parent.ThrowIfNull(nameof(parent));
        }

        /// <summary>
        /// Gets the <see cref="IBehaviorContainer"/> with the specified name.
        /// </summary>
        /// <value>
        /// The <see cref="IBehaviorContainer"/>.
        /// </value>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public override IBehaviorContainer this[string name]
        {
            get
            {
                if (base.ContainsKey(name))
                    return base[name];

                // We expect it to exist locally, but if it doesn't let's ask the parent
                return _parent[name];
            }
        }
    }
}
