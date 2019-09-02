using System.Collections.Generic;
using System.Threading;

namespace SpiceSharp
{
    /// <summary>
    /// Collection for parameter sets. This class will keep track which parameter sets belong to which entity.
    /// Only a <see cref="ParameterSet" /> can be requested from the collection.
    /// </summary>
    public class ParameterPool
    {
        private readonly ReaderWriterLockSlim Lock = new ReaderWriterLockSlim();

        /// <summary>
        /// The entity parameters
        /// </summary>
        private readonly Dictionary<string, ParameterSetDictionary> _entityParameters;

        /// <summary>
        /// Gets the associated <see cref="ParameterSetDictionary"/> of an entity.
        /// </summary>
        /// <param name="name">The entity identifier.</param>
        /// <returns>The parameter set associated to the specified entity identifier.</returns>
        public virtual ParameterSetDictionary this[string name]
        {
            get
            {
                Lock.EnterReadLock();
                try
                {
                    if (_entityParameters.TryGetValue(name, out var result))
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
        /// Gets the entity identifier comparer.
        /// </summary>
        /// <value>
        /// The comparer.
        /// </value>
        public IEqualityComparer<string> Comparer => _entityParameters.Comparer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterPool"/> class.
        /// </summary>
        public ParameterPool()
        {
            _entityParameters = new Dictionary<string, ParameterSetDictionary>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterPool"/> class.
        /// </summary>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}" /> implementation to use when comparing entity names, or <c>null</c> to use the default <see cref="EqualityComparer{T}"/>.</param>
        public ParameterPool(IEqualityComparer<string> comparer)
        {
            _entityParameters = new Dictionary<string, ParameterSetDictionary>(comparer);
        }

        /// <summary>
        /// Adds the specified parameter set to the pool.
        /// </summary>
        /// <param name="creator">The entity identifier to which the parameter set belongs.</param>
        /// <param name="parameters">The parameter set.</param>
        public virtual void Add(string creator, ParameterSet parameters)
        {
            Lock.EnterWriteLock();
            try
            {
                if (!_entityParameters.TryGetValue(creator, out var ep))
                {
                    ep = new ParameterSetDictionary();
                    _entityParameters.Add(creator, ep);
                }
                ep.Add(parameters);
            }
            finally
            {
                Lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Checks if a parameter set exists for a specified entity identifier.
        /// </summary>
        /// <param name="name">The entity identifier.</param>
        /// <returns>
        ///   <c>true</c> if a parameter set exists; otherwise <c>false</c>.
        /// </returns>
        public virtual bool Contains(string name)
        {
            Lock.EnterReadLock();
            try
            {
                return _entityParameters.ContainsKey(name);
            }
            finally
            {
                Lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Clears all parameter sets in the pool.
        /// </summary>
        public virtual void Clear()
        {
            Lock.EnterWriteLock();
            try
            {
                _entityParameters.Clear();
            }
            finally
            {
                Lock.ExitWriteLock();
            }
        }
    }
}
