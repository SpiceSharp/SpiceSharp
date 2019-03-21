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
    public class EntityCollection : IEnumerable<Entity>
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
        /// Gets the comparer for entity identifiers.
        /// </summary>
        /// <value>
        /// The comparer.
        /// </value>
        public IEqualityComparer<string> Comparer => _entities.Comparer;

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
        /// Search for an entity by its string.
        /// </summary>
        /// <value>
        /// The <see cref="Entity"/>.
        /// </value>
        /// <param name="name">The string.</param>
        /// <returns>
        /// The entity with the specified string.
        /// </returns>
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
        /// Add one or more entities.
        /// </summary>
        /// <param name="entities">The entities that need to be added.</param>
        public void Add(params Entity[] entities)
        {
            if (entities == null)
                return;
            foreach (var entity in entities)
            {
                if (entity == null)
                    throw new ArgumentNullException();
                OnEntityAdded(new EntityEventArgs(entity));

                _lock.EnterWriteLock();
                try
                {
                    _entities.Add(entity.Name, entity);
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
        }

        /// <summary>
        /// Raises the <seealso cref="EntityAdded"/> event.
        /// </summary>
        protected virtual void OnEntityAdded(EntityEventArgs args) => EntityAdded?.Invoke(this, args);

        /// <summary>
        /// Removes the specified entities from the collection.
        /// </summary>
        /// <param name="names">strings of the entities that need to be deleted.</param>
        public void Remove(params string[] names)
        {
            if (names == null)
                return;
            foreach (var name in names)
            {
                if (name == null)
                    throw new ArgumentNullException();
                if (_entities.TryGetValue(name, out var entity))
                {
                    _lock.EnterWriteLock();
                    try
                    {
                        _entities.Remove(name);
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
        public IEnumerator<Entity> GetEnumerator()
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
    }
}
