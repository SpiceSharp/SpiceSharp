using System;
using System.Collections;
using System.Collections.Generic;

namespace SpiceSharp.Entities
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="SpiceSharp.Entities.IEntityCollection" />
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

        /// <summary>
        /// Gets the <see cref="IEntity"/> with the specified name.
        /// </summary>
        /// <value>
        /// The <see cref="IEntity"/>.
        /// </value>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public IEntity this[string name] => _entities[name];

        /// <summary>
        /// Gets the comparer used to compare <see cref="Entity" /> names.
        /// </summary>
        /// <value>
        /// The comparer.
        /// </value>
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
            _entities = new Dictionary<string, IEntity>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityCollection"/> class.
        /// </summary>
        /// <param name="comparer">The comparer.</param>
        public EntityCollection(IEqualityComparer<string> comparer)
        {
            _entities = new Dictionary<string, IEntity>(comparer);
        }

        /// <summary>
        /// Removes all items from the <see cref="ICollection{T}" />.
        /// </summary>
        public void Clear() => _entities.Clear();

        /// <summary>
        /// Adds an item to the <see cref="ICollection{T}" />.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="ICollection{T}" />.</param>
        public void Add(IEntity item)
        {
            item.ThrowIfNull(nameof(item));
            _entities.Add(item.Name, item);
        }

        /// <summary>
        /// Adds the specified entities to the collection.
        /// </summary>
        /// <param name="entities">The entities.</param>
        public void Add(params IEntity[] entities)
        {
            if (entities == null)
                return;
            foreach (var entity in entities)
                Add(entity);
        }

        /// <summary>
        /// Removes the <see cref="Entity" /> with specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
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
        public bool Remove(IEntity item)
        {
            item.ThrowIfNull(nameof(item));
            if (!_entities.TryGetValue(item.Name, out var result) || result != item)
                return false;
            _entities.Remove(item.Name);
            OnEntityRemoved(new EntityEventArgs(item));
            return true;
        }

        /// <summary>
        /// Determines whether this instance contains the object.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>
        ///   <c>true</c> if the collection contains the entity; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(string name) => _entities.ContainsKey(name);

        /// <summary>
        /// Determines whether this instance contains the object.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>
        ///   <c>true</c> if [contains] [the specified entity]; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(IEntity entity)
        {
            if (_entities.TryGetValue(entity.Name, out var result))
                return result == entity;
            return false;
        }

        /// <summary>
        /// Tries to find an <see cref="Entity" /> in the collection.
        /// </summary>
        /// <param name="name">The name of the entity.</param>
        /// <param name="entity">The entity.</param>
        /// <returns>
        ///   <c>True</c> if the entity is found; otherwise <c>false</c>.
        /// </returns>
        public bool TryGetEntity(string name, out IEntity entity) => _entities.TryGetValue(name, out entity);

        /// <summary>
        /// Gets all entities that are of a specified type.
        /// </summary>
        /// <typeparam name="E">The type of entity.</typeparam>
        /// <returns>
        /// The entities.
        /// </returns>
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
        /// Clones the instance.
        /// </summary>
        /// <returns>
        /// The cloned instance.
        /// </returns>
        ICloneable ICloneable.Clone() => Clone();

        /// <summary>
        /// Copies the contents of one interface to this one.
        /// </summary>
        /// <param name="source">The source parameter.</param>
        void ICloneable.CopyFrom(ICloneable source) => CopyFrom(source);

        /// <summary>
        /// Clones the instance.
        /// </summary>
        /// <returns>
        /// The cloned instance.
        /// </returns>
        protected virtual ICloneable Clone()
        {
            var clone = new EntityCollection(Comparer);
            foreach (var entity in this)
                clone.Add((IEntity)entity.Clone());
            return clone;
        }

        /// <summary>
        /// Copies the contents of one interface to this one.
        /// </summary>
        /// <param name="source">The source parameter.</param>
        protected virtual void CopyFrom(ICloneable source)
        {
            var src = (EntityCollection)source;
            _entities.Clear();
            foreach (var entity in src._entities.Values)
                Add(entity);
        }

        /// <summary>
        /// Copies the elements of the <see cref="ICollection{T}" /> to an <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="ICollection{T}" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
        /// <exception cref="ArgumentOutOfRangeException">arrayIndex</exception>
        /// <exception cref="ArgumentException">Not enough elements in the array</exception>
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
    }
}
