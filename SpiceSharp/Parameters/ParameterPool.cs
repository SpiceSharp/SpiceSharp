using System.Collections.Generic;

namespace SpiceSharp.Parameters
{
    /// <summary>
    /// Collection for parameters
    /// This class will keep track which parameter sets belong to which entity. Only ParameterSets can be requested from the collection.
    /// </summary>
    public class ParameterPool
    {
        private readonly Dictionary<Identifier, ParameterSetDictionary> _entityParameters = new Dictionary<Identifier, ParameterSetDictionary>();

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
        public ParameterSetDictionary GetEntityParameters(Identifier name)
        {
            if (_entityParameters.TryGetValue(name, out var result))
                return result;
            return null;
        }
    }
}
