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
        List<ParameterSetCollection> parameters = new List<ParameterSetCollection>();
        List<EntityBehaviors> behaviors = new List<EntityBehaviors>();

        /// <summary>
        /// Available number of parameter collections
        /// </summary>
        public int ParametersCount { get => parameters.Count; }

        /// <summary>
        /// Available number of entity behaviors
        /// </summary>
        public int BehaviorsCount { get => behaviors.Count; }

        /// <summary>
        /// Add a collection of parameters
        /// </summary>
        /// <param name="pc">Parameter collection</param>
        public void Add(ParameterSetCollection pc) => parameters.Add(pc);

        /// <summary>
        /// Add entity behaviors
        /// </summary>
        /// <param name="behaviors">Entity behaviors</param>
        public void Add(EntityBehaviors behaviors) => this.behaviors.Add(behaviors);
        
        /// <summary>
        /// Get parameters of a specific type
        /// </summary>
        /// <typeparam name="T">The type of Parameters</typeparam>
        /// <param name="index">The index in the provider (first one by default)</param>
        /// <returns></returns>
        public T GetParameterSet<T>(int index) where T : ParameterSet => parameters[index].Get<T>();

        /// <summary>
        /// Get behaviors of a specific type
        /// </summary>
        /// <typeparam name="T">The type of Behavior</typeparam>
        /// <param name="index">The index in the provider (first on by default)</param>
        /// <returns></returns>
        public T GetBehavior<T>(int index) where T : Behavior => behaviors[index].Get<T>();
    }
}
