using System;
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
        private class EntityPriorityComparer : IComparer<Entity>
        {
            public int Compare(Entity x, Entity y)
            {
                if (x == null)
                    throw new ArgumentNullException(nameof(x));
                if (y == null)
                    throw new ArgumentNullException(nameof(y));
                if (x == y)
                    return 0;

                // Put the highest priority first!
                if (x.Priority < y.Priority)
                    return 1;
                if (x.Priority > y.Priority)
                    return -1;

                // Then check the hash
                var hx = x.GetHashCode();
                var hy = y.GetHashCode();
                if (hx < hy)
                    return -1;
                if (hx > hy)
                    return 1;

                // If STILL equal, then we will just use the name of the entity
                return StringComparer.Ordinal.Compare(x.Name, y.Name);
            }
        }

        /// <summary>
        /// Private variables
        /// </summary>
        private readonly Dictionary<string, Entity> _entities;
        private readonly SortedSet<Entity> _ordered;
        private readonly ReaderWriterLockSlim _lock;

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
            _ordered = new SortedSet<Entity>(new EntityPriorityComparer());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityCollection"/> class.
        /// </summary>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}" /> implementation to use when comparing entity names, or <c>null</c> to use the default <see cref="EqualityComparer{T}"/>.</param>
        public EntityCollection(IEqualityComparer<string> comparer)
        {
            _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
            _entities = new Dictionary<string, Entity>(comparer);
            _ordered = new SortedSet<Entity>(new EntityPriorityComparer());
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
                try
                {
                    _lock.EnterReadLock();
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
                try
                {
                    _lock.EnterReadLock();
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
            try
            {
                _lock.EnterWriteLock();
                _entities.Clear();
                _ordered.Clear();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Add one or more entities.
        /// </summary>
        /// <param name="cs">The entities that need to be added.</param>
        public void Add(params Entity[] cs)
        {
            try
            {
                _lock.EnterWriteLock();

                if (cs == null)
                    return;
                foreach (var c in cs)
                {
                    if (c == null)
                        throw new ArgumentNullException();
                    if (_entities.ContainsKey(c.Name))
                        throw new CircuitException(
                            "An entity by the name of '{0}' already exists".FormatString(c.Name));
                    _entities.Add(c.Name, c);
                    _ordered.Add(c);
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Remove specific entities from the collection.
        /// </summary>
        /// <param name="names">strings of the entities that need to be deleted.</param>
        public void Remove(params string[] names)
        {
            try
            {
                _lock.EnterWriteLock();

                if (names == null)
                    return;
                foreach (var name in names)
                {
                    if (name == null)
                        throw new ArgumentNullException();
                    if (_entities.TryGetValue(name, out var entity))
                    {
                        _entities.Remove(name);
                        _ordered.Remove(entity);
                    }
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// This method checks if a component exists with a specified string.
        /// </summary>
        /// <param name="name">The string.</param>
        /// <returns>True if the collection contains an entity with a certain string.</returns>
        public bool Contains(string name)
        {
            try
            {
                _lock.EnterReadLock();
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
            try
            {
                _lock.EnterReadLock();
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
            try
            {
                _lock.EnterReadLock();
                var result = new List<Entity>();
                foreach (var c in _ordered)
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
            try
            {
                _lock.EnterReadLock();

                foreach (var t in _ordered)
                    yield return t;
            }
            finally
            {
                _lock.ExitReadLock();
            }
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
