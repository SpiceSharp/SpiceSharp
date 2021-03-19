using System;
using System.Collections.Generic;

namespace SpiceSharp.Entities
{
    /// <summary>
    /// Template for a collection of <see cref="Entity" />.
    /// </summary>
    /// <seealso cref="IEnumerable{T}" />
    /// <seealso cref="ICollection{T}" />
    public interface IEntityCollection : IEnumerable<IEntity>, ICollection<IEntity>, ICloneable<IEntityCollection>
    {
        /// <summary>
        /// Gets the <see cref="IEntity"/> with the specified name.
        /// </summary>
        /// <value>
        /// The <see cref="IEntity"/>.
        /// </value>
        /// <param name="name">The name of the entity.</param>
        /// <returns>The entity.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        /// <exception cref="KeyNotFoundException">Thrown if no entity with the name <paramref name="name"/> could be found.</exception>
        IEntity this[string name] { get; }

        /// <summary>
        /// Gets the comparer used to compare <see cref="Entity"/> names.
        /// </summary>
        /// <value>
        /// The comparer.
        /// </value>
        IEqualityComparer<string> Comparer { get; }

        /// <summary>
        /// Removes the <see cref="IEntity" /> with specified name.
        /// </summary>
        /// <param name="name">The name of the entity.</param>
        /// <returns>
        ///   <c>true</c> is the entity was removed succesfully; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        bool Remove(string name);

        /// <summary>
        /// Determines whether this instance contains an <see cref="IEntity"/> with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>
        ///   <c>true</c> if the collection contains the entity; otherwise, <c>false</c>.
        /// </returns>
        bool Contains(string name);

        /// <summary>
        /// Tries to find an <see cref="Entity"/> in the collection.
        /// </summary>
        /// <param name="name">The name of the entity.</param>
        /// <param name="entity">The entity.</param>
        /// <returns>
        /// <c>True</c> if the entity is found; otherwise <c>false</c>.
        /// </returns>
        bool TryGetEntity(string name, out IEntity entity);

        /// <summary>
        /// Gets all entities that are of a specified type.
        /// </summary>
        /// <typeparam name="E">The type of entity.</typeparam>
        /// <returns>The entities.</returns>
        IEnumerable<E> ByType<E>() where E : IEntity;
    }
}
