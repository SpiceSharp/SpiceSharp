using System;

namespace SpiceSharp.Entities
{
    /// <summary>
    /// Event arguments for passing an entity.
    /// </summary>
    /// <seealso cref="EventArgs" />
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
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="entity"/> if <c>null</c>.</exception>
        public EntityEventArgs(IEntity entity)
        {
            Entity = entity.ThrowIfNull(nameof(entity));
        }
    }
}
