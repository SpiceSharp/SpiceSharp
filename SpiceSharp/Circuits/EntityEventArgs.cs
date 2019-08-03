using System;

namespace SpiceSharp.Circuits
{
    /// <summary>
    /// Event arguments for passing an entity.
    /// </summary>
    /// <seealso cref="System.EventArgs" />
    public class EntityEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the entity.
        /// </summary>
        public Entity Entity { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityEventArgs"/> class.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public EntityEventArgs(Entity entity)
        {
            Entity = entity.ThrowIfNull(nameof(entity));
        }
    }
}
