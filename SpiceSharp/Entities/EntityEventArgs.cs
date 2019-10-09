using System;

namespace SpiceSharp.Entities
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
        public IEntity Entity { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityEventArgs"/> class.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public EntityEventArgs(IEntity entity)
        {
            Entity = entity.ThrowIfNull(nameof(entity));
        }
    }
}
