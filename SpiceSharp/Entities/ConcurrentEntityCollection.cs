using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace SpiceSharp.Entities
{
    /// <summary>
    /// A collection of entities that can be accessed by multiple threads concurrently.
    /// </summary>
    public class ConcurrentEntityCollection : IEntityCollection
    {
        private readonly Dictionary<string, IEntity> _entities;
        private readonly ReaderWriterLockSlim _lock;

        /// <summary>
        /// Occurs when an entity is about to be added to the collection.
        /// </summary>
        public event EventHandler<EntityEventArgs> EntityAdded;

        /// <summary>
        /// Occurs when an entity has been removed from the collection.
        /// </summary>
        public event EventHandler<EntityEventArgs> EntityRemoved;

        /// <inheritdoc/>
        public IEntity this[string name]
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return _entities[name];
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }

        /// <inheritdoc/>
        public IEqualityComparer<string> Comparer => _entities.Comparer;

        /// <summary>
        /// Gets a value indicating whether the <see cref="ICollection{T}" /> is read-only.
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// Gets the number of entities in the collection
        /// </summary>
        /// <value>
        /// The number of entities.
        /// </value>
        public int Count
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return _entities.Count;
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentEntityCollection"/> class.
        /// </summary>
        public ConcurrentEntityCollection()
        {
            _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
            _entities = [];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentEntityCollection"/> class.
        /// </summary>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}" /> implementation to use when comparing entity names, or <c>null</c> to use the default <see cref="EqualityComparer{T}"/>.</param>
        public ConcurrentEntityCollection(IEqualityComparer<string> comparer)
        {
            _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
            _entities = new Dictionary<string, IEntity>(comparer ?? Constants.DefaultComparer);
        }

        /// <summary>
        /// Clear all entities in the collection.
        /// </summary>
        public void Clear()
        {
            _lock.EnterWriteLock();
            try
            {
                _entities.Clear();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Adds an item to the <see cref="ICollection{T}" />.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="ICollection{T}" />.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="item"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown if another entity with the same name already exists.</exception>
        public void Add(IEntity item)
        {
            item.ThrowIfNull(nameof(item));

            _lock.EnterWriteLock();
            try
            {
                _entities.Add(item.Name, item);
            }
            catch (ArgumentException)
            {
                throw new ArgumentException(Properties.Resources.EntityCollection_KeyExists.FormatString(item.Name));
            }
            finally
            {
                _lock.ExitWriteLock();
            }
            OnEntityAdded(new EntityEventArgs(item));
        }

        /// <inheritdoc/>
        public bool Remove(string name)
        {
            name.ThrowIfNull(nameof(name));
            _lock.EnterUpgradeableReadLock();
            try
            {
                if (!_entities.TryGetValue(name, out var entity))
                    return false;

                _lock.EnterWriteLock();
                try
                {
                    _entities.Remove(name);
                    OnEntityRemoved(new EntityEventArgs(entity));
                    return true;
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="ICollection{T}" />.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="ICollection{T}" />.</param>
        /// <returns>
        ///   <c>true</c> if <paramref name="item" /> was successfully removed from the <see cref="ICollection{T}" />; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="item"/> is <c>null</c>.</exception>
        public bool Remove(IEntity item)
        {
            item.ThrowIfNull(nameof(item));
            _lock.EnterUpgradeableReadLock();
            bool success = false;
            try
            {
                if (!_entities.TryGetValue(item.Name, out var result) || result != item)
                    return false;

                _lock.EnterWriteLock();
                try
                {
                    success = _entities.Remove(item.Name);
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
            if (success)
            {
                OnEntityRemoved(new EntityEventArgs(item));
                return true;
            }
            return false;
        }

        /// <inheritdoc/>
        public bool Contains(string name)
        {
            _lock.EnterReadLock();
            try
            {
                return _entities.ContainsKey(name);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Determines whether this instance contains the object.
        /// </summary>
        /// <param name="item">The entity.</param>
        /// <returns>
        ///   <c>true</c> if the collection contains the entity; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="item"/> is <c>null</c>.</exception>
        public bool Contains(IEntity item)
        {
            _lock.EnterReadLock();
            try
            {
                if (_entities.TryGetValue(item.Name, out var result))
                    return result == item;
                return false;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <inheritdoc/>
        public bool TryGetEntity(string name, out IEntity entity)
        {
            _lock.EnterReadLock();
            try
            {
                return _entities.TryGetValue(name, out entity);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <inheritdoc/>
        public IEnumerable<E> ByType<E>() where E : IEntity
        {
            _lock.EnterReadLock();
            try
            {
                foreach (var entity in _entities.Values)
                {
                    if (entity is E e)
                        yield return e;
                }
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public virtual IEnumerator<IEntity> GetEnumerator()
        {
            IEntity[] result;
            _lock.EnterReadLock();
            try
            {
                result = [.. _entities.Values];
            }
            finally
            {
                _lock.ExitReadLock();
            }

            // Enumerate
            foreach (var entity in result)
                yield return entity;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

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
        /// Raises the <seealso cref="EntityAdded"/> event.
        /// </summary>
        protected virtual void OnEntityAdded(EntityEventArgs args) => EntityAdded?.Invoke(this, args);

        /// <summary>
        /// Raises the <seealso cref="EntityRemoved"/> event.
        /// </summary>
        protected virtual void OnEntityRemoved(EntityEventArgs args) => EntityRemoved?.Invoke(this, args);

        /// <inheritdoc/>
        public IEntityCollection Clone()
        {
            var clone = new ConcurrentEntityCollection(_entities.Comparer);
            foreach (var pair in _entities.Values)
                clone.Add(pair.Clone());
            return clone;
        }
    }
}
