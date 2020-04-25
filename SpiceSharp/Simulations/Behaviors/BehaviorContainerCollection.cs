using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// A pool of all behaviors. This class will keep track which behavior belongs to which entity. Only behaviors can be requested from the collection.
    /// </summary>
    public class BehaviorContainerCollection : IBehaviorContainerCollection
    {
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        /// <summary>
        /// Behaviors indexed by the entity that created them.
        /// </summary>
        private readonly Dictionary<string, IBehaviorContainer> _dictionary;
        private readonly List<IBehaviorContainer> _values;

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
        public int Count => _dictionary.Count;

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
                    foreach (var key in _dictionary.Keys)
                        yield return key;
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }

        /// <summary>
        /// Gets the associated <see cref="Behavior"/> of an entity.
        /// </summary>
        /// <param name="name">The entity name.</param>
        /// <returns>The behavior associated to the specified entity name.</returns>
        public virtual IBehaviorContainer this[string name]
        {
            get
            {
                _lock.EnterUpgradeableReadLock();
                try
                {
                    if (_dictionary.TryGetValue(name, out var result))
                        return result;
                    var args = new BehaviorsNotFoundEventArgs(name);
                    OnBehaviorsNotFound(args);
                    if (args.Behaviors != null)
                        return args.Behaviors;
                    throw new SpiceSharpException(Properties.Resources.Behaviors_NoBehaviorFor.FormatString(name));
                }
                finally
                {
                    _lock.ExitUpgradeableReadLock();
                }
            }
        }

        /// <summary>
        /// Gets the entity name comparer.
        /// </summary>
        /// <value>
        /// The comparer.
        /// </value>
        public IEqualityComparer<string> Comparer => _dictionary.Comparer;

        /// <summary>
        /// Initializes a new instance of the <see cref="BehaviorContainerCollection" /> class.
        /// </summary>
        public BehaviorContainerCollection()
            : this(EqualityComparer<string>.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BehaviorContainerCollection"/> class.
        /// </summary>
        /// <param name="comparer">The comparer for behaviors.</param>
        public BehaviorContainerCollection(IEqualityComparer<string> comparer)
        {
            _dictionary = new Dictionary<string, IBehaviorContainer>(comparer);
            _values = new List<IBehaviorContainer>();
        }

        /// <summary>
        /// Adds the entity behaviors.
        /// </summary>
        /// <param name="behaviors">The behaviors.</param>
        /// <exception cref="SpiceSharpException">There are already behaviors for '{0}'".FormatString(id)</exception>
        public void Add(IBehaviorContainer behaviors)
        {
            behaviors.ThrowIfNull(nameof(behaviors));
            _lock.EnterUpgradeableReadLock();
            try
            {
                if (_dictionary.ContainsKey(behaviors.Name))
                    throw new SpiceSharpException(Properties.Resources.Behaviors_BehaviorsAlreadyExist.FormatString(behaviors.Name));
                _lock.EnterWriteLock();
                try
                {
                    _dictionary.Add(behaviors.Name, behaviors);
                    _values.Add(behaviors);
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
                var list = new List<T>(_values.Count);
                foreach (var elt in _values)
                {
                    if (elt.TryGetValue(out T value))
                        list.Add(value);
                }
                return new BehaviorList<T>(list.ToArray());
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Tries to the get the entity behaviors by a specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="ebd">The dictionary of entity behaviors.</param>
        /// <returns></returns>
        public virtual bool TryGetBehaviors(string name, out IBehaviorContainer ebd)
        {
            _lock.EnterUpgradeableReadLock();
            try
            {
                if (_dictionary.TryGetValue(name, out ebd))
                    return true;

                // Try asking our event
                var args = new BehaviorsNotFoundEventArgs(name);
                OnBehaviorsNotFound(args);
                if (args.Behaviors != null)
                {
                    ebd = args.Behaviors;
                    return true;
                }

                // Nothing found
                ebd = null;
                return false;
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
        }

        /// <summary>
        /// Checks if behaviors exist for a specified entity name.
        /// </summary>
        /// <param name="name">The entity name.</param>
        /// <returns>
        ///   <c>true</c> if behaviors exist; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool Contains(string name)
        {
            _lock.EnterReadLock();
            try
            {
                return _dictionary.ContainsKey(name);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Clears all behaviors in the pool.
        /// </summary>
        public virtual void Clear()
        {
            _lock.EnterWriteLock();
            try
            {
                _dictionary.Clear();
                _values.Clear();
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

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<IBehaviorContainer> GetEnumerator() => _values.GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
