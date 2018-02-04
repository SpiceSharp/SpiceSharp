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
        Dictionary<string, ParameterSetDictionary> parameterSets = new Dictionary<string, ParameterSetDictionary>();
        Dictionary<string, EntityBehaviorDictionary> entityBehaviors = new Dictionary<string, EntityBehaviorDictionary>();

        /// <summary>
        /// Available number of parameter collections
        /// </summary>
        public int ParametersCount { get => parameterSets.Count; }

        /// <summary>
        /// Available number of entity behaviors
        /// </summary>
        public int BehaviorsCount { get => entityBehaviors.Count; }

        /// <summary>
        /// Add a collection of parameters
        /// </summary>
        /// <param name="pc">Parameter collection</param>
        public void Add(string name, ParameterSetDictionary pc) => parameterSets.Add(name, pc);

        /// <summary>
        /// Add entity behaviors
        /// </summary>
        /// <param name="behaviors">Entity behaviors</param>
        public void Add(string name, EntityBehaviorDictionary behaviors) => entityBehaviors.Add(name, behaviors);
        
        /// <summary>
        /// Gets a parameter set for a certain name
        /// </summary>
        /// <typeparam name="T">The type of Parameters</typeparam>
        /// <param name="name">Name of the parameter set</param>
        /// <returns></returns>
        public T GetParameterSet<T>(string name) where T : ParameterSet => parameterSets[name].Get<T>();

        /// <summary>
        /// Gets the behaviors for a certain name
        /// </summary>
        /// <typeparam name="T">The type of Behavior</typeparam>
        /// <param name="name">Name of the behavior collection</param>
        /// <returns></returns>
        public T GetBehavior<T>(string name) where T : Behavior => entityBehaviors[name].Get<T>();
    }
}
