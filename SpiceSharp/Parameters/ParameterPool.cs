using System;
using System.Collections.Generic;

namespace SpiceSharp
{
    /// <summary>
    /// Collection for parameters
    /// This class will keep track which parameter sets belong to which entity. Only ParameterSets can be requested from the collection.
    /// </summary>
    public class ParameterPool
    {
        /// <summary>
        /// Parameters indexed by the entity identifier
        /// </summary>
        private readonly Dictionary<Identifier, ParameterSetDictionary> _entityParameters = new Dictionary<Identifier, ParameterSetDictionary>();

        /// <summary>
        /// Gets the entity parameter set for a specific identifier
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
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
        /// Add a parameter set to the collection
        /// </summary>
        /// <param name="creator">Creator</param>
        /// <param name="parameters">Parameters</param>
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
        /// Gets the entity parameter set for a specific identifier
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns></returns>
        [Obsolete]
        public ParameterSetDictionary GetEntityParameters(Identifier name)
        {
            if (_entityParameters.TryGetValue(name, out var result))
                return result;
            return null;
        }

        /// <summary>
        /// Check if base parameters are available vor a specific identifier
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns></returns>
        public bool Contains(Identifier name) => _entityParameters.ContainsKey(name);

        /// <summary>
        /// Clear all parameter sets in the pool
        /// </summary>
        public void Clear() => _entityParameters.Clear();
    }
}
