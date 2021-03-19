using System;
using System.Collections;
using System.Collections.Generic;

namespace SpiceSharp.Entities
{
    /// <summary>
    /// A default implementation for <see cref="IEntityCollection"/>.
    /// </summary>
    /// <seealso cref="IEntityCollection" />
    public class EntityCollection : IEntityCollection
    {
        private readonly Dictionary<string, IEntity> _entities;

        /// <summary>
        /// Occurs when an entity has been added.
        /// </summary>
        public event EventHandler<EntityEventArgs> EntityAdded;

        /// <summary>
        /// Occurs when an entity has been removed.
        /// </summary>
        public event EventHandler<EntityEventArgs> EntityRemoved;

        /// <inheritdoc/>
        public IEntity this[string name] => _entities[name];

        /// <inheritdoc/>
        public IEqualityComparer<string> Comparer => _entities.Comparer;

        /// <summary>
        /// Gets a value indicating whether the <see cref="ICollection{T}" /> is read-only.
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// Gets the number of elements contained in the <see cref="ICollection{T}" />.
        /// </summary>
        public int Count => _entities.Count;

        /// <summary>
        /// Gets the keys.
        /// </summary>
        /// <value>
        /// The keys.
        /// </value>
        public IEnumerable<string> Keys => _entities.Keys;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityCollection"/> class.
        /// </summary>
        public EntityCollection()
        {
            _entities = new Dictionary<string, IEntity>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityCollection"/> class.
        /// </summary>
        /// <param name="comparer">The comparer.</param>
        public EntityCollection(IEqualityComparer<string> comparer)
        {
            _entities = new Dictionary<string, IEntity>(comparer ?? Constants.DefaultComparer);
        }

        /// <summary>
        /// Removes all items from the <see cref="ICollection{T}" />.
        /// </summary>
        public void Clear() => _entities.Clear();

        /// <summary>
        /// Adds an item to the <see cref="ICollection{T}" />.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="ICollection{T}" />.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="item"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown if another entity with the same name already exists.</exception>
        public void Add(IEntity item)
        {
            item.ThrowIfNull(nameof(item));
            try
            {
                _entities.Add(item.Name, item);
            }
            catch (ArgumentException)
            {
                throw new ArgumentException(Properties.Resources.EntityCollection_KeyExists.FormatString(item.Name));
            }
            OnEntityAdded(new EntityEventArgs(item));
        }

        /// <inheritdoc/>
        public bool Remove(string name)
        {
            name.ThrowIfNull(nameof(name));
            if (!_entities.TryGetValue(name, out var entity))
                return false;
            _entities.Remove(name);
            OnEntityRemoved(new EntityEventArgs(entity));
            return true;
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="ICollection{T}" />.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="ICollection{T}" />.</param>
        /// <returns>
        /// true if <paramref name="item" /> was successfully removed from the <see cref="ICollection{T}" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="ICollection{T}" />.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="item"/> is <c>null</c>.</exception>
        public bool Remove(IEntity item)
        {
            item.ThrowIfNull(nameof(item));
            if (!_entities.TryGetValue(item.Name, out var result) || result != item)
                return false;
            _entities.Remove(item.Name);
            OnEntityRemoved(new EntityEventArgs(item));
            return true;
        }

        /// <inheritdoc/>
        public bool Contains(string name) => _entities.ContainsKey(name);

        /// <summary>
        /// Determines whether this instance contains the object.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>
        ///   <c>true</c> if the collection contains the entity; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="entity"/> is <c>null</c>.</exception>
        public bool Contains(IEntity entity)
        {
            entity.ThrowIfNull(nameof(entity));
            if (_entities.TryGetValue(entity.Name, out var result))
                return result == entity;
            return false;
        }

        /// <inheritdoc/>
        public bool TryGetEntity(string name, out IEntity entity) => _entities.TryGetValue(name, out entity);

        /// <inheritdoc/>
        public IEnumerable<E> ByType<E>() where E : IEntity
        {
            foreach (var entity in _entities.Values)
            {
                if (entity is E e)
                    yield return e;
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public virtual IEnumerator<IEntity> GetEnumerator() => _entities.Values.GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Copies the elements of the collection to an array, starting at a particular array index.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="arrayIndex">The array index.</param>
        void ICollection<IEntity>.CopyTo(IEntity[] array, int arrayIndex)
        {
            array.ThrowIfNull(nameof(array));
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            if (array.Length < arrayIndex + Count)
                throw new ArgumentException(Properties.Resources.NotEnoughElements);
            foreach (var item in _entities.Values)
                array[arrayIndex++] = item;
        }

        /// <summary>
        /// Raises the <see cref="EntityAdded" /> event.
        /// </summary>
        /// <param name="args">The <see cref="EntityEventArgs"/> instance containing the event data.</param>
        protected virtual void OnEntityAdded(EntityEventArgs args) => EntityAdded?.Invoke(this, args);

        /// <summary>
        /// Raises the <see cref="EntityRemoved" /> event.
        /// </summary>
        /// <param name="args">The <see cref="EntityEventArgs"/> instance containing the event data.</param>
        protected virtual void OnEntityRemoved(EntityEventArgs args) => EntityRemoved?.Invoke(this, args);

        /// <inheritdoc/>
        public IEntityCollection Clone()
        {
            var clone = new EntityCollection(_entities.Comparer);
            foreach (var entity in _entities.Values)
                clone.Add(entity);
            return clone;
        }
    }
}
