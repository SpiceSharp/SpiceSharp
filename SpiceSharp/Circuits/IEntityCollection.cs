using System;
using System.Collections.Generic;

namespace SpiceSharp.Circuits
{
    /// <summary>
    /// Template for a collection of <see cref="Entity"/>.
    /// </summary>
    /// <seealso cref="System.Collections.Generic.IEnumerable{T}" />
    /// <seealso cref="System.Collections.Generic.ICollection{T}" />
    public interface IEntityCollection : IEnumerable<IEntity>, ICollection<IEntity>
    {
        /// <summary>
        /// Gets the <see cref="Entity"/> with the specified name.
        /// </summary>
        /// <value>
        /// The <see cref="Entity"/>.
        /// </value>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        IEntity this[string name] { get; }

        /// <summary>
        /// Gets the comparer used to compare <see cref="Entity"/> identifiers.
        /// </summary>
        /// <value>
        /// The comparer.
        /// </value>
        IEqualityComparer<string> Comparer { get; }

        /// <summary>
        /// Removes the <see cref="Entity"/> with specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        bool Remove(string name);

        /// <summary>
        /// Determines whether this instance contains an <see cref="Entity"/> with the specified name.
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
        /// Enumerates each <see cref="Entity"/> of the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        IEnumerable<IEntity> ByType(Type type);
    }
}
