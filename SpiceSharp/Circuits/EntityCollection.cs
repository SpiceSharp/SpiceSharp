using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using SpiceSharp.Components;

namespace SpiceSharp.Circuits
{
    /// <summary>
    /// Contains and manages a collection circuit objects.
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
        /// Gets whether or not the list is already ordered
        /// </summary>
        private bool _isOrdered;

        /// <summary>
        /// Constructor
        /// </summary>
        public EntityCollection()
        {
            _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
            _isOrdered = false;
        }

        /// <summary>
        /// Search for an object by id
        /// </summary>
        /// <param name="id">ID</param>
        /// <returns></returns>
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
        /// The amount of circuit objects
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
        /// Clear all circuit objects
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
        /// Add one or more circuit objects
        /// </summary>
        /// <param name="cs">The objects that need to be added</param>
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
        /// Remove specific circuit objects from the collection
        /// </summary>
        /// <param name="ids">Names of the objects that need to be deleted</param>
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
        /// Check if a component exists
        /// Multiple names can be specified, in which case the first names will refer to subcircuits
        /// </summary>
        /// <param name="id">A list of names. If there are multiple names, the first names will refer to a subcircuit</param>
        /// <returns></returns>
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
        /// Gets a circuit object
        /// </summary>
        /// <param name="id">Identifier</param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public bool TryGetEntity(Identifier id, out Entity obj)
        {
            try
            {
                _lock.EnterReadLock();
                return _objects.TryGetValue(id, out obj);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Gets all objects of a specific type
        /// </summary>
        /// <param name="type">The type of objects you wish to find</param>
        /// <returns></returns>
        public Entity[] ByType(Type type)
        {
            try
            {
                _lock.EnterReadLock();
                List<Entity> result = new List<Entity>();
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
        /// This method is called when building an ordered list of circuit objects
        /// Circuit objects will be called by descending priority
        /// </summary>
        public void BuildOrderedComponentList()
        {
            try
            {
                _lock.EnterWriteLock();

                if (_isOrdered)
                    return;

                // Initialize
                _ordered.Clear();
                HashSet<Entity> added = new HashSet<Entity>();

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
        /// Gets enumerator
        /// </summary>
        /// <returns></returns>
        public IEnumerator<Entity> GetEnumerator()
        {
            try
            {
                _lock.EnterReadLock();

                for (int i = 0; i < _ordered.Count; i++)
                    yield return _ordered[i];
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Gets enumerator
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
