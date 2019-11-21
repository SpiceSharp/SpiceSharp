using SpiceSharp.Simulations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// A pool of all behaviors. This class will keep track which behavior belongs to which entity. Only behaviors can be requested from the collection.
    /// </summary>
    public class BehaviorContainerCollection : IBehaviorContainerCollection
    {
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        /// <summary>
        /// Behaviors indexed by the entity that created them.
        /// </summary>
        private readonly Dictionary<string, IBehaviorContainer> _entityBehaviors;

        /// <summary>
        /// Lists of behaviors indexed by type of behavior.
        /// </summary>
        private readonly Dictionary<Type, List<IBehavior>> _behaviorLists = new Dictionary<Type, List<IBehavior>>();

        /// <summary>
        /// Occurs when a behavior has not been found.
        /// </summary>
        public event EventHandler<BehaviorsNotFoundEventArgs> BehaviorsNotFound;

        /// <summary>
        /// Gets the number of entities in the collection.
        /// </summary>
        /// <value>
        /// The entity count.
        /// </value>
        public int Count => _entityBehaviors.Count;

        /// <summary>
        /// Enumerates all names in the pool.
        /// </summary>
        public IEnumerable<string> Keys
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    foreach (var key in _entityBehaviors.Keys)
                        yield return key;
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }

        /// <summary>
        /// Gets the types.
        /// </summary>
        /// <value>
        /// The types.
        /// </value>
        public IEnumerable<Type> BehaviorTypes => _behaviorLists.Keys;

        /// <summary>
        /// Gets the associated <see cref="Behavior"/> of an entity.
        /// </summary>
        /// <param name="name">The entity identifier.</param>
        /// <returns>The behavior associated to the specified entity identifier.</returns>
        public virtual IBehaviorContainer this[string name]
        {
            get
            {
                _lock.EnterUpgradeableReadLock();
                try
                {
                    if (_entityBehaviors.TryGetValue(name, out var result))
                        return result;
                    var args = new BehaviorsNotFoundEventArgs(name);
                    OnBehaviorsNotFound(args);
                    if (args.Behaviors != null)
                        return args.Behaviors;
                    throw new CircuitException("Cannot find behaviors for '{0}'".FormatString(name));
                }
                finally
                {
                    _lock.ExitUpgradeableReadLock();
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BehaviorContainerCollection" /> class.
        /// </summary>
        /// <param name="simulation">The simulation for which behaviors need to be created.</param>
        public BehaviorContainerCollection(ISimulation simulation)
            : this(EqualityComparer<string>.Default, simulation)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BehaviorContainerCollection"/> class.
        /// </summary>
        /// <param name="comparer">The comparer for behaviors.</param>
        /// <param name="simulation">The simulation for which behaviors need to be created.</param>
        public BehaviorContainerCollection(IEqualityComparer<string> comparer, ISimulation simulation)
        {
            _entityBehaviors = new Dictionary<string, IBehaviorContainer>(comparer);
            var ifs = simulation.GetType().GetTypeInfo().GetInterfaces();
            foreach (var type in ifs)
            {
                var info = type.GetTypeInfo();
                if (info.IsGenericType && (info.GetGenericTypeDefinition() == typeof(IBehavioral<>)))
                    _behaviorLists.Add(info.GetGenericArguments()[0], new List<IBehavior>());
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BehaviorContainerCollection"/> class.
        /// </summary>
        /// <param name="source">The collection to serve as a template.</param>
        public BehaviorContainerCollection(IBehaviorContainerCollection source)
        {
            _entityBehaviors = new Dictionary<string, IBehaviorContainer>(source.Comparer);
            foreach (var type in _behaviorLists.Keys)
                _behaviorLists.Add(type, new List<IBehavior>());
        }

        /// <summary>
        /// Adds the entity behaviors.
        /// </summary>
        /// <param name="behaviors">The behaviors.</param>
        /// <exception cref="CircuitException">There are already behaviors for '{0}'".FormatString(id)</exception>
        public void Add(IBehaviorContainer behaviors)
        {
            behaviors.ThrowIfNull(nameof(behaviors));
            _lock.EnterReadLock();
            try
            {
                // First see if we already have a behavior
                if (_entityBehaviors.ContainsKey(behaviors.Name))
                    throw new CircuitException("There are already behaviors for '{0}'".FormatString(behaviors.Name));
            }
            finally
            {
                _lock.ExitReadLock();
            }

            _lock.EnterWriteLock();
            try
            {
                _entityBehaviors.Add(behaviors.Name, behaviors);
                foreach (var type in _behaviorLists.Keys)
                {
                    if (behaviors.TryGetValue(type, out IBehavior behavior))
                        _behaviorLists[type].Add(behavior);
                }
            }
            finally
            {
                _lock.ExitWriteLock();
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
            _lock.EnterReadLock();
            try
            {
                if (_behaviorLists.TryGetValue(typeof(T), out var list))
                    return new BehaviorList<T>(list.Cast<T>());
                return new BehaviorList<T>(new T[0]);
            }
            finally
            {
                _lock.ExitReadLock();
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
            _lock.EnterReadLock();
            try
            {
                return _entityBehaviors.TryGetValue(name, out ebd);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Checks if behaviors exist for a specified entity identifier.
        /// </summary>
        /// <param name="name">The entity identifier.</param>
        /// <returns>
        ///   <c>true</c> if behaviors exist; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool Contains(string name)
        {
            _lock.EnterReadLock();
            try
            {
                return _entityBehaviors.ContainsKey(name);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Checks if the collection tracks an <see cref="IBehavior"/>.
        /// </summary>
        /// <typeparam name="B">The behavior.</typeparam>
        /// <returns>\
        /// <c>true</c> if the collection tracks the behavior type; otherwise <c>false</c>.
        /// </returns>
        public bool Tracks<B>() where B : IBehavior
            => _behaviorLists.ContainsKey(typeof(B));

        /// <summary>
        /// Clears all behaviors in the pool.
        /// </summary>
        public virtual void Clear()
        {
            _lock.EnterWriteLock();
            try
            {
                _behaviorLists.Clear();
                _entityBehaviors.Clear();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Raises the <see cref="BehaviorsNotFound" /> event.
        /// </summary>
        /// <param name="args">The <see cref="BehaviorsNotFoundEventArgs"/> instance containing the event data.</param>
        protected virtual void OnBehaviorsNotFound(BehaviorsNotFoundEventArgs args) => BehaviorsNotFound?.Invoke(this, args);
    }
}
