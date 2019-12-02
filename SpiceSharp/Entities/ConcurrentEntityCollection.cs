using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace SpiceSharp.Entities
{
    /// <summary>
    /// A collection of entities that can be accessed by multiple threads concurrently.
    /// </summary>
    public class ConcurrentEntityCollection : IEntityCollection
    {
        /// <summary>
        /// Private variables
        /// </summary>
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

        /// <summary>
        /// Search for an entity by its string.
        /// </summary>
        /// <param name="name">The string.</param>
        /// <returns>The entity with the specified string.</returns>
        [SuppressMessage("Microsoft.Design", "CA1043:UseIntegralOrStringArgumentForIndexers")]
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

        /// <summary>
        /// Gets the comparer for entity names.
        /// </summary>
        public IEqualityComparer<string> Comparer => _entities.Comparer;

        /// <summary>
        /// Gets a value indicating whether the <see cref="ICollection{T}" /> is read-only.
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// The number of entities.
        /// </summary>
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
        /// Enumerates the names of all entities in the collection.
        /// </summary>
        public IEnumerable<string> Keys
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return _entities.Keys.ToArray();
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
            _entities = new Dictionary<string, IEntity>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentEntityCollection"/> class.
        /// </summary>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}" /> implementation to use when comparing entity names, or <c>null</c> to use the default <see cref="EqualityComparer{T}"/>.</param>
        public ConcurrentEntityCollection(IEqualityComparer<string> comparer)
        {
            _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
            _entities = new Dictionary<string, IEntity>(comparer);
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
        /// Add an entity.
        /// </summary>
        /// <param name="item">The item to be added.</param>
        public void Add(IEntity item)
        {
            item.ThrowIfNull(nameof(item));

            _lock.EnterWriteLock();
            try
            {
                _entities.Add(item.Name, item);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
            OnEntityAdded(new EntityEventArgs(item));
        }

        /// <summary>
        /// Add one or more entities.
        /// </summary>
        /// <param name="entities">The entities that need to be added.</param>
        public void Add(params IEntity[] entities)
        {
            if (entities == null)
                return;
            foreach (var entity in entities)
                Add(entity);
        }

        /// <summary>
        /// Removes the specified entity from the collection.
        /// </summary>
        /// <param name="name">The name of the entity to be deleted.</param>
        /// <returns></returns>
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
        /// true if <paramref name="item" /> was successfully removed from the <see cref="ICollection{T}" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="ICollection{T}" />.
        /// </returns>
        public bool Remove(IEntity item)
        {
            item.ThrowIfNull(nameof(item));
            _lock.EnterUpgradeableReadLock();
            try
            {
                if (!_entities.TryGetValue(item.Name, out var result) || result != item)
                    return false;

                _lock.EnterWriteLock();
                try
                {
                    _entities.Remove(item.Name);
                    OnEntityRemoved(new EntityEventArgs(item));
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
        /// This method checks if a component exists with a specified string.
        /// </summary>
        /// <param name="name">The string.</param>
        /// <returns>True if the collection contains an entity with a certain string.</returns>
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
        /// Find out if an entity is contained in this collection.
        /// </summary>
        /// <param name="item">The entity.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Try to find an entity in the collection.
        /// </summary>
        /// <param name="name">The name to be searched for.</param>
        /// <param name="entity">The found entity.</param>
        /// <returns>True if the entity was found.</returns>
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

        /// <summary>
        /// Gets all entities that are of a specified type.
        /// </summary>
        /// <typeparam name="E">The type of entity.</typeparam>
        /// <returns>
        /// The entities.
        /// </returns>
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
                result = _entities.Values.ToArray();
            }
            finally
            {
                _lock.ExitReadLock();
            }

            // Enumerate
            foreach (var entity in result)
                yield return entity;
        }

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
            _lock.EnterReadLock();
            try
            {
                var clone = new ConcurrentEntityCollection(Comparer);
                foreach (var entity in this)
                    clone.Add((IEntity)entity.Clone());
                return clone;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Copies the contents of one interface to this one.
        /// </summary>
        /// <param name="source">The source parameter.</param>
        protected virtual void CopyFrom(ICloneable source)
        {
            var src = (ConcurrentEntityCollection)source;
            _lock.EnterWriteLock();
            try
            {
                _entities.Clear();
                foreach (var entity in src._entities.Values)
                    _entities.Add(entity.Name, entity);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Copy the elements to an array.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="arrayIndex">The starting index.</param>
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

    }
}
