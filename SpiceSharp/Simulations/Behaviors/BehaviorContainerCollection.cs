using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// A pool of all behaviors. This class will keep track which behavior belongs to which entity. Only behaviors can be requested from the collection.
    /// </summary>
    /// <seealso cref="IBehaviorContainerCollection"/>
    public class BehaviorContainerCollection : IBehaviorContainerCollection
    {
        private readonly ReaderWriterLockSlim _lock = new(LockRecursionPolicy.SupportsRecursion);
        private readonly Dictionary<string, IBehaviorContainer> _dictionary;
        private readonly List<IBehaviorContainer> _values;

        /// <inheritdoc/>
        public event EventHandler<BehaviorsNotFoundEventArgs> BehaviorsNotFound;

        /// <inheritdoc/>
        public int Count => _dictionary.Count;

        /// <inheritdoc/>
        public IEnumerable<string> Keys
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    foreach (string key in _dictionary.Keys)
                        yield return key;
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }

        /// <inheritdoc/>
        public virtual IBehaviorContainer this[string name]
        {
            get
            {
                name.ThrowIfNull(nameof(name));
                _lock.EnterUpgradeableReadLock();
                try
                {
                    if (_dictionary.TryGetValue(name, out var result))
                        return result;
                    var args = new BehaviorsNotFoundEventArgs(name);
                    OnBehaviorsNotFound(args);
                    if (args.Behaviors != null)
                        return args.Behaviors;

                    // The behaviors could not be found...
                    throw new BehaviorsNotFoundException(name, Properties.Resources.Behaviors_NoBehaviorFor.FormatString(name));
                }
                finally
                {
                    _lock.ExitUpgradeableReadLock();
                }
            }
        }

        /// <inheritdoc/>
        public IEqualityComparer<string> Comparer => _dictionary.Comparer;

        /// <summary>
        /// Initializes a new instance of the <see cref="BehaviorContainerCollection" /> class.
        /// </summary>
        public BehaviorContainerCollection()
            : this(Constants.DefaultComparer)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BehaviorContainerCollection"/> class.
        /// </summary>
        /// <param name="comparer">The comparer for behaviors.</param>
        public BehaviorContainerCollection(IEqualityComparer<string> comparer)
        {
            _dictionary = new Dictionary<string, IBehaviorContainer>(comparer ?? Constants.DefaultComparer);
            _values = [];
        }

        /// <inheritdoc/>
        public void Add(IBehaviorContainer behaviors)
        {
            behaviors.ThrowIfNull(nameof(behaviors));
            _lock.EnterUpgradeableReadLock();
            try
            {
                if (_dictionary.ContainsKey(behaviors.Name))
                    throw new ArgumentException(Properties.Resources.Behaviors_BehaviorsAlreadyExist.FormatString(behaviors.Name));
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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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
