using System.Collections.Generic;

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
        /// Gets the behaviors for a certain name
        /// </summary>
        /// <typeparam name="T">The type of Behavior</typeparam>
        /// <param name="name">Name of the behavior collection</param>
        /// <returns></returns>
        public T GetBehavior<T>(string name = "entity") where T : Behavior => _entityBehaviors[name].Get<T>();
    }
}
