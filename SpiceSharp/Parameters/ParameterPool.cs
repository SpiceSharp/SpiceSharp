using System;
using System.Collections.Generic;

namespace SpiceSharp
{
    /// <summary>
    /// Collection for parameter sets. This class will keep track which parameter sets belong to which entity.
    /// Only a <see cref="ParameterSet" /> can be requested from the collection.
    /// </summary>
    public class ParameterPool
    {
        /// <summary>
        /// The entity parameters
        /// </summary>
        private readonly Dictionary<Identifier, ParameterSetDictionary> _entityParameters;

        /// <summary>
        /// Gets the associated <see cref="ParameterSetDictionary"/> of an entity.
        /// </summary>
        /// <value>
        /// The <see cref="ParameterSetDictionary"/>.
        /// </value>
        /// <param name="name">The entity identifier.</param>
        /// <returns>The parameter set associated to the specified entity identifier.</returns>
        public ParameterSetDictionary this[Identifier name]
        {
            get
            {
                if (_entityParameters.TryGetValue(name, out var result))
                    return result;
                return null;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterPool"/> class.
        /// </summary>
        public ParameterPool()
        {
            _entityParameters = new Dictionary<Identifier, ParameterSetDictionary>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterPool"/> class.
        /// </summary>
        /// <param name="comparer">The comparer for identifiers.</param>
        public ParameterPool(IEqualityComparer<Identifier> comparer)
        {
            _entityParameters = new Dictionary<Identifier, ParameterSetDictionary>(comparer);
        }

        /// <summary>
        /// Adds the specified parameter set to the pool.
        /// </summary>
        /// <param name="creator">The entity identifier to which the parameter set belongs.</param>
        /// <param name="parameters">The parameter set.</param>
        public void Add(Identifier creator, ParameterSet parameters)
        {
            if (!_entityParameters.TryGetValue(creator, out var ep))
            {
                ep = new ParameterSetDictionary();
                _entityParameters.Add(creator, ep);
            }
            ep.Add(parameters);
        }

        /// <summary>
        /// Gets the entity parameter set for a specific identifier.
        /// Obsolete, use the indexer instead.
        /// </summary>
        /// <param name="name">The identifier of the entity.</param>
        /// <returns>The parameter set associated to the specified entity identifier.</returns>
        [Obsolete]
        public ParameterSetDictionary GetEntityParameters(Identifier name)
        {
            if (_entityParameters.TryGetValue(name, out var result))
                return result;
            return null;
        }

        /// <summary>
        /// Checks if a parameter set exists for a specified entity identifier.
        /// </summary>
        /// <param name="name">The entity identifier.</param>
        /// <returns>
        ///   <c>true</c> if a parameter set exists; otherwise <c>false</c>.
        /// </returns>
        public bool Contains(Identifier name) => _entityParameters.ContainsKey(name);

        /// <summary>
        /// Clears all parameter sets in the pool.
        /// </summary>
        public void Clear() => _entityParameters.Clear();
    }
}
