using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace SpiceSharp.Circuits
{
    /// <summary>
    /// A class that manages a collection of entities.
    /// </summary>
    public class EntityCollection : IEnumerable<Entity>, ICollection<Entity>, ICollection, IReadOnlyCollection<Entity>
    {
        /// <summary>
        /// Private variables
        /// </summary>
        private readonly Dictionary<string, Entity> _entities;
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
        public Entity this[string name]
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
        /// Gets the comparer for entity identifiers.
        /// </summary>
        public IEqualityComparer<string> Comparer => _entities.Comparer;

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
        public IEnumerable<string> Keys => _entities.Keys;

        /// <summary>
        /// Gets a value indicating whether access to the <see cref="ICollection{T}" /> is synchronized (thread safe).
        /// </summary>
        public bool IsSynchronized => true;

        /// <summary>
        /// Gets an object that can be used to synchronize access to the <see cref="ICollection{T}"/>.
        /// </summary>
        public object SyncRoot => this;

        /// <summary>
        /// Gets a value indicating whether the <see cref="ICollection{T}"/> is read-only.
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityCollection"/> class.
        /// </summary>
        public EntityCollection()
        {
            _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
            _entities = new Dictionary<string, Entity>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityCollection"/> class.
        /// </summary>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}" /> implementation to use when comparing entity names, or <c>null</c> to use the default <see cref="EqualityComparer{T}"/>.</param>
        public EntityCollection(IEqualityComparer<string> comparer)
        {
            _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
            _entities = new Dictionary<string, Entity>(comparer);
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
        public void Add(Entity item)
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
        public void Add(params Entity[] entities)
        {
            if (entities == null)
                return;
            foreach (var entity in entities)
                Add(entity);
        }

        /// <summary>
        /// Raises the <seealso cref="EntityAdded"/> event.
        /// </summary>
        protected virtual void OnEntityAdded(EntityEventArgs args) => EntityAdded?.Invoke(this, args);

        /// <summary>
        /// Removes the specified entity from the collection.
        /// </summary>
        /// <param name="item">The item to be deleted.</param>
        /// <returns></returns>
        public bool Remove(Entity item)
        {
            item.ThrowIfNull(nameof(item));

            _lock.EnterUpgradeableReadLock();
            try
            {
                if (!_entities.ContainsValue(item))
                    return false;
                _lock.EnterWriteLock();
                try
                {
                    _entities.Remove(item.Name);
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
        /// Removes the specified entities from the collection.
        /// </summary>
        /// <param name="names">strings of the entities that need to be deleted.</param>
        public void Remove(params string[] names)
        {
            if (names == null)
                return;
            foreach (var name in names)
                Remove(name);
        }

        /// <summary>
        /// Removes the specified entities from the collection.
        /// </summary>
        /// <param name="entities">The entities.</param>
        public void Remove(params Entity[] entities)
        {
            if (entities == null)
                return;
            foreach (var entity in entities)
            {
                if (Contains(entity.Name))
                {
                    _lock.EnterWriteLock();
                    try
                    {
                        _entities.Remove(entity.Name);
                    }
                    finally
                    {
                        _lock.ExitWriteLock();
                    }
                    OnEntityRemoved(new EntityEventArgs(entity));
                }
            }
        }

        /// <summary>
        /// Raises the <seealso cref="EntityRemoved"/> event.
        /// </summary>
        protected virtual void OnEntityRemoved(EntityEventArgs args) => EntityRemoved?.Invoke(this, args);

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
        /// Try to find an entity in the collection.
        /// </summary>
        /// <param name="name">The name to be searched for.</param>
        /// <param name="entity">The found entity.</param>
        /// <returns>True if the entity was found.</returns>
        public bool TryGetEntity(string name, out Entity entity)
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
        /// Gets all entities of a specific type.
        /// </summary>
        /// <param name="type">The type of entities to be listed.</param>
        /// <returns>An array with entities of the specified type.</returns>
        public Entity[] ByType(Type type)
        {
            var result = new List<Entity>();
            _lock.EnterReadLock();
            try
            {   
                foreach (var c in _entities.Values)
                {
                    if (c.GetType() == type)
                        result.Add(c);
                }
                return result.ToArray();
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
        public virtual IEnumerator<Entity> GetEnumerator()
        {
            Entity[] result;
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
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Copy the elements to an array.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="index">The starting index.</param>
        void ICollection.CopyTo(Array array, int index)
        {
            array.ThrowIfNull(nameof(array));
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (array.Length < index + Count)
                throw new ArgumentException("Not enough elements in the array");

            foreach (var item in _entities.Values)
                array.SetValue(item, index++);
        }

        /// <summary>
        /// Find out if an entity is contained in this collection.
        /// </summary>
        /// <param name="item">The entity.</param>
        /// <returns></returns>
        public bool Contains(Entity item)
        {
            _lock.EnterReadLock();
            try
            {
                return _entities.ContainsValue(item);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Copy the elements to an array.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="arrayIndex">The starting index.</param>
        public void CopyTo(Entity[] array, int arrayIndex)
        {
            array.ThrowIfNull(nameof(array));
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            if (array.Length < arrayIndex + Count)
                throw new ArgumentException("Not enough elements in the array");

            foreach (var item in _entities.Values)
                array[arrayIndex++] = item;
        }
    }
}
