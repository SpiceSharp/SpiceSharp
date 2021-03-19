using SpiceSharp.Entities;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SpiceSharp
{
    /// <summary>
    /// Represents an electronic circuit.
    /// </summary>
    /// <seealso cref="IEntityCollection" />
    public class Circuit : IEntityCollection
    {
        private readonly IEntityCollection _entities;

        /// <inheritdoc/>
        public IEqualityComparer<string> Comparer => _entities.Comparer;

        /// <summary>
        /// Gets the number of elements contained in the <see cref="ICollection{T}" />.
        /// </summary>
        public int Count => _entities.Count;

        /// <summary>
        /// Gets a value indicating whether the <see cref="ICollection{T}" /> is read-only.
        /// </summary>
        public bool IsReadOnly => _entities.IsReadOnly;

        /// <inheritdoc/>
        public IEntity this[string name] => _entities[name];

        /// <summary>
        /// Initializes a new instance of the <see cref="Circuit"/> class.
        /// </summary>
        public Circuit()
        {
            _entities = new EntityCollection();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Circuit"/> class.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="entities"/> is <c>null</c>.</exception>
        public Circuit(IEntityCollection entities)
        {
            _entities = entities.ThrowIfNull(nameof(entities));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Circuit"/> class.
        /// </summary>
        /// <param name="entities">The entities describing the circuit.</param>
        public Circuit(IEnumerable<IEntity> entities)
            : this()
        {
            if (entities == null)
                return;
            foreach (var entity in entities)
                Add(entity);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Circuit"/> class.
        /// </summary>
        /// <param name="entities">The entities describing the circuit.</param>
        public Circuit(params IEntity[] entities)
            : this()
        {
            if (entities == null)
                return;
            foreach (var entity in entities)
                Add(entity);
        }

        /// <summary>
        /// Merge a circuit with this one. Entities are merged by reference!
        /// </summary>
        /// <param name="ckt">The circuit to merge with.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="ckt"/> is <c>null</c>.</exception>
        public void Merge(Circuit ckt)
        {
            ckt.ThrowIfNull(nameof(ckt));
            foreach (var entity in ckt)
                Add(entity);
        }

        /// <inheritdoc/>
        public bool Remove(string name) => _entities.Remove(name);

        /// <inheritdoc/>
        public bool Contains(string name) => _entities.Contains(name);

        /// <inheritdoc/>
        public bool TryGetEntity(string name, out IEntity entity) => _entities.TryGetEntity(name, out entity);

        /// <inheritdoc/>
        public IEnumerable<E> ByType<E>() where E : IEntity => _entities.ByType<E>();

        /// <summary>
        /// Adds an item to the <see cref="ICollection{T}" />.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="ICollection{T}" />.</param>
        public void Add(IEntity item) => _entities.Add(item);

        /// <summary>
        /// Removes all items from the <see cref="ICollection{T}" />.
        /// </summary>
        public void Clear() => _entities.Clear();

        /// <summary>
        /// Determines whether this instance contains the object.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="ICollection{T}" />.</param>
        /// <returns>
        /// true if <paramref name="item" /> is found in the <see cref="ICollection{T}" />; otherwise, false.
        /// </returns>
        public bool Contains(IEntity item) => _entities.Contains(item);

        /// <summary>
        /// Copies the elements of the <see cref="ICollection{T}" /> to an <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="ICollection{T}" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
        public void CopyTo(IEntity[] array, int arrayIndex) => _entities.CopyTo(array, arrayIndex);

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="ICollection{T}" />.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="ICollection{T}" />.</param>
        /// <returns>
        /// true if <paramref name="item" /> was successfully removed from the <see cref="ICollection{T}" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="ICollection{T}" />.
        /// </returns>
        public bool Remove(IEntity item) => _entities.Remove(item);

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<IEntity> GetEnumerator() => _entities.GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_entities).GetEnumerator();

        /// <inheritdoc/>
        public IEntityCollection Clone()
            => new Circuit(_entities.Clone());
    }
}
