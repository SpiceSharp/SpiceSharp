using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// Data for setting up a <see cref="Behavior"/>
    /// </summary>
    public class SetupDataProvider
    {
        /// <summary>
        /// Available behaviors and parameters
        /// </summary>
        private readonly Dictionary<string, ParameterSetDictionary> _parameterSets = new Dictionary<string, ParameterSetDictionary>();
        private readonly Dictionary<string, EntityBehaviorDictionary> _entityBehaviors = new Dictionary<string, EntityBehaviorDictionary>();

        /// <summary>
        /// Available number of parameter collections
        /// </summary>
        public int ParametersCount => _parameterSets.Count;

        /// <summary>
        /// Available number of entity behaviors
        /// </summary>
        public int BehaviorsCount => _entityBehaviors.Count;

        /// <summary>
        /// Add a collection of parameters
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="pc">Parameter collection</param>
        public void Add(string name, ParameterSetDictionary pc) => _parameterSets.Add(name, pc);

        /// <summary>
        /// Add entity behaviors
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="behaviors">Entity behaviors</param>
        public void Add(string name, EntityBehaviorDictionary behaviors) => _entityBehaviors.Add(name, behaviors);
        
        /// <summary>
        /// Gets a parameter set for a certain name
        /// </summary>
        /// <typeparam name="T">The type of Parameters</typeparam>
        /// <param name="name">Name of the parameter set</param>
        /// <returns></returns>
        public T GetParameterSet<T>(string name = "entity") where T : ParameterSet => _parameterSets[name].Get<T>();

        /// <summary>
        /// Try getting a parameter set
        /// </summary>
        /// <typeparam name="T">The type of Parameter set</typeparam>
        /// <param name="name">Name of parameter set</param>
        /// <param name="value">The returned value</param>
        /// <returns></returns>
        public bool TryGetParameterSet<T>(string name, out T value) where T : ParameterSet
        {
            if (_parameterSets.TryGetValue(name, out var r))
                return r.TryGet(out value);
            value = default(T);
            return false;
        }

        /// <summary>
        /// Gets the behaviors for a certain name
        /// </summary>
        /// <typeparam name="T">The type of Behavior</typeparam>
        /// <param name="name">Name of the behavior collection</param>
        /// <returns></returns>
        public T GetBehavior<T>(string name = "entity") where T : Behavior => _entityBehaviors[name].Get<T>();

        /// <summary>
        /// Try getting a behavior
        /// </summary>
        /// <typeparam name="T">The type of Behavior</typeparam>
        /// <param name="name">Name of the behavior collection</param>
        /// <param name="value">The returned value</param>
        /// <returns></returns>
        public bool TryGetBehavior<T>(string name, out T value) where T : Behavior
        {
            if (_entityBehaviors.TryGetValue(name, out var r))
                return r.TryGet(out value);
            value = default(T);
            return false;
        }
    }
}
