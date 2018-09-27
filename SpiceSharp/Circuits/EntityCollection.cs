using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using SpiceSharp.Components;

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
        private readonly Dictionary<Identifier, Entity> _objects = new Dictionary<Identifier, Entity>();
        private readonly List<Entity> _ordered = new List<Entity>();
        private readonly ReaderWriterLockSlim _lock;

        /// <summary>
        /// Gets whether or not the list is already ordered.
        /// </summary>
        private bool _isOrdered;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityCollection"/> class.
        /// </summary>
        public EntityCollection()
        {
            _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
            _isOrdered = false;
        }

        /// <summary>
        /// Search for an entity by its identifier.
        /// </summary>
        /// <value>
        /// The <see cref="Entity"/>.
        /// </value>
        /// <param name="id">The identifier.</param>
        /// <returns>
        /// The entity with the specified identifier.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1043:UseIntegralOrStringArgumentForIndexers")]
        public Entity this[Identifier id]
        {
            get
            {
                try
                {
                    _lock.EnterReadLock();
                    return _objects[id];
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
                    return _objects.Count;
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
                _objects.Clear();
                _ordered.Clear();
                _isOrdered = false;
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
                        throw new CircuitException("No entity specified");
                    if (_objects.ContainsKey(c.Name))
                        throw new CircuitException("A component with the id {0} already exists".FormatString(c.Name));
                    _objects.Add(c.Name, c);
                    _isOrdered = false;
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
        /// <param name="ids">Identifiers of the entities that need to be deleted.</param>
        public void Remove(params Identifier[] ids)
        {
            try
            {
                _lock.EnterWriteLock();

                if (ids == null)
                    return;
                foreach (var id in ids)
                {
                    if (id == null)
                        throw new CircuitException("No identifier specified");
                    _objects.Remove(id);

                    // Note: Removing objects does not interfere with the order!
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// This method checks if a component exists with a specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>True if the collection contains an entity with a certain identifier.</returns>
        public bool Contains(Identifier id)
        {
            try
            {
                _lock.EnterReadLock();
                return _objects.ContainsKey(id);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Try to find an entity in the collection.
        /// </summary>
        /// <param name="id">The identifier to be searched for.</param>
        /// <param name="entity">The found entity.</param>
        /// <returns>True if the entity was found.</returns>
        public bool TryGetEntity(Identifier id, out Entity entity)
        {
            try
            {
                _lock.EnterReadLock();
                return _objects.TryGetValue(id, out entity);
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
                foreach (var c in _objects.Values)
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
        /// This method is called when building an ordered list of circuit objects. Circuit objects will be called by descending priority.
        /// </summary>
        /// <remarks>
        /// Ordering is necessary for entities that depend on one another. For example, a <see cref="MutualInductance"/> needs access
        /// to other <see cref="Inductor"/> in order to apply mutual coupling. So inductors need to come before mutual inductances.
        /// </remarks>
        public void BuildOrderedComponentList()
        {
            try
            {
                _lock.EnterWriteLock();

                if (_isOrdered)
                    return;

                // Initialize
                _ordered.Clear();
                var added = new HashSet<Entity>();

                // Build our list
                foreach (var c in _objects.Values)
                {
                    // Add the object to the ordered list
                    _ordered.Add(c);
                    added.Add(c);

                    // Automatically add models to the ordered list
                    if (c is Component component)
                    {
                        var model = component.Model;
                        if (model != null && !added.Contains(model))
                        {
                            added.Add(model);
                            _ordered.Add(model);
                        }
                    }
                }

                // Sort the list based on priority
                _ordered.Sort((a, b) => b.Priority.CompareTo(a.Priority));
                _isOrdered = true;
            }
            finally
            {
                _lock.ExitWriteLock();
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
