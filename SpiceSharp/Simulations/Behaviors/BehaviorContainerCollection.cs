using SpiceSharp.Simulations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// A pool of all behaviors. This class will keep track which behavior belongs to which entity. Only behaviors can be requested from the collection.
    /// </summary>
    public class BehaviorContainerCollection
    {
        private readonly ReaderWriterLockSlim Lock = new ReaderWriterLockSlim();

        /// <summary>
        /// Behaviors indexed by the entity that created them.
        /// </summary>
        private readonly Dictionary<string, IBehaviorContainer> _entityBehaviors;

        /// <summary>
        /// Lists of behaviors indexed by type of behavior.
        /// </summary>
        private readonly Dictionary<Type, List<IBehavior>> _behaviorLists = new Dictionary<Type, List<IBehavior>>();

        /// <summary>
        /// Gets the number of behaviors in the collection.
        /// </summary>
        public int Count
        {
            get
            {
                Lock.EnterReadLock();
                try
                {
                    var count = 0;
                    foreach (var pair in _behaviorLists)
                        count += pair.Value.Count;
                    return count;
                }
                finally
                {
                    Lock.ExitReadLock();
                }
            }
        }

        /// <summary>
        /// Gets the number of entities in the collection.
        /// </summary>
        /// <value>
        /// The entity count.
        /// </value>
        public int EntityCount => _entityBehaviors.Count;

        /// <summary>
        /// Enumerates all names in the pool.
        /// </summary>
        public IEnumerable<string> Names
        {
            get
            {
                Lock.EnterReadLock();
                try
                {
                    foreach (var key in _entityBehaviors.Keys)
                        yield return key;
                }
                finally
                {
                    Lock.ExitReadLock();
                }
            }
        }

        /// <summary>
        /// Gets the types.
        /// </summary>
        /// <value>
        /// The types.
        /// </value>
        public IEnumerable<Type> Types => _behaviorLists.Keys;

        /// <summary>
        /// Gets the associated <see cref="Behavior"/> of an entity.
        /// </summary>
        /// <param name="name">The entity identifier.</param>
        /// <returns>The behavior associated to the specified entity identifier.</returns>
        public virtual IBehaviorContainer this[string name]
        {
            get
            {
                Lock.EnterReadLock();
                try
                {
                    if (_entityBehaviors.TryGetValue(name, out var result))
                        return result;
                    return null;
                }
                finally
                {
                    Lock.ExitReadLock();
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BehaviorContainerCollection"/> class.
        /// </summary>
        /// <param name="types">The types for which a list will be kept which can be retrieved later.</param>
        public BehaviorContainerCollection(IEnumerable<Type> types)
        {
            types.ThrowIfNull(nameof(types));
            _entityBehaviors = new Dictionary<string, IBehaviorContainer>();
            foreach (var type in types)
                _behaviorLists.Add(type, new List<IBehavior>());
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="BehaviorContainerCollection"/> class.
        /// </summary>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}" /> implementation to use when comparing entity names, or <c>null</c> to use the default <see cref="EqualityComparer{T}"/>.</param>
        public BehaviorContainerCollection(IEqualityComparer<string> comparer)
        {
            _entityBehaviors = new Dictionary<string, IBehaviorContainer>(comparer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BehaviorContainerCollection"/> class.
        /// </summary>
        /// <param name="comparer">The comparer.</param>
        /// <param name="types">The types.</param>
        public BehaviorContainerCollection(IEqualityComparer<string> comparer, IEnumerable<Type> types)
        {
            types.ThrowIfNull(nameof(types));
            _entityBehaviors = new Dictionary<string, IBehaviorContainer>(comparer);
            foreach (var type in types)
                _behaviorLists.Add(type, new List<IBehavior>());
        }

        /// <summary>
        /// Adds the entity behaviors.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="behaviors">The behaviors.</param>
        /// <exception cref="CircuitException">There are already behaviors for '{0}'".FormatString(id)</exception>
        public void Add(string id, IBehaviorContainer behaviors)
        {
            id.ThrowIfNull(nameof(id));
            behaviors.ThrowIfNull(nameof(behaviors));
            Lock.EnterReadLock();
            try
            {
                // First see if we already have a behavior
                if (_entityBehaviors.ContainsKey(id))
                    throw new CircuitException("There are already behaviors for '{0}'".FormatString(id));
            }
            finally
            {
                Lock.ExitReadLock();
            }

            Lock.EnterWriteLock();
            try
            {
                _entityBehaviors.Add(id, behaviors);
                foreach (var type in _behaviorLists.Keys)
                {
                    if (behaviors.TryGetValue(type, out IBehavior behavior))
                        _behaviorLists[type].Add(behavior);
                }
            }
            finally
            {
                Lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Gets the entity identifier comparer.
        /// </summary>
        /// <value>
        /// The comparer.
        /// </value>
        public IEqualityComparer<string> Comparer => _entityBehaviors.Comparer;

        /// <summary>
        /// Gets a list of behaviors of a specific type.
        /// </summary>
        /// <typeparam name="T">The base behavior type.</typeparam>
        /// <returns>
        /// A <see cref="BehaviorList{T}" /> with all behaviors of the specified type.
        /// </returns>
        public virtual BehaviorList<T> GetBehaviorList<T>() where T : IBehavior
        {
            Lock.EnterReadLock();
            try
            {
                if (_behaviorLists.TryGetValue(typeof(T), out var list))
                    return new BehaviorList<T>(list.Cast<T>());
                return new BehaviorList<T>(new T[0]);
            }
            finally
            {
                Lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Tries to the get the entity behaviors by a specified identifier.
        /// </summary>
        /// <param name="name">The identifier.</param>
        /// <param name="ebd">The dictionary of entity behaviors.</param>
        /// <returns></returns>
        public virtual bool TryGetBehaviors(string name, out IBehaviorContainer ebd)
        {
            Lock.EnterReadLock();
            try
            {
                return _entityBehaviors.TryGetValue(name, out ebd);
            }
            finally
            {
                Lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Checks if behaviors exist for a specified entity identifier.
        /// </summary>
        /// <param name="name">The entity identifier.</param>
        /// <returns>
        ///   <c>true</c> if behaviors exist; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool ContainsKey(string name)
        {
            Lock.EnterReadLock();
            try
            {
                return _entityBehaviors.ContainsKey(name);
            }
            finally
            {
                Lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Clears all behaviors in the pool.
        /// </summary>
        public virtual void Clear()
        {
            Lock.EnterWriteLock();
            try
            {
                _behaviorLists.Clear();
                _entityBehaviors.Clear();
            }
            finally
            {
                Lock.ExitWriteLock();
            }
        }
    }
}
