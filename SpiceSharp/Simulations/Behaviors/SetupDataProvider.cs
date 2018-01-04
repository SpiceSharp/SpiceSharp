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
        List<ParametersCollection> parameters = new List<ParametersCollection>();
        List<EntityBehaviors> behaviors = new List<EntityBehaviors>();

        /// <summary>
        /// Available data count
        /// </summary>
        public int Count { get; protected set; }

        /// <summary>
        /// Add a collection of parameters and behaviors to the data provider
        /// </summary>
        /// <param name="pc">Parameters collection</param>
        /// <param name="eb">Entity behaviors</param>
        public void Add(ParametersCollection pc, EntityBehaviors eb)
        {
            parameters.Add(pc);
            behaviors.Add(eb);
            Count++;
        }
        
        /// <summary>
        /// Get parameters of a specific type
        /// </summary>
        /// <typeparam name="T">The type of Parameters</typeparam>
        /// <param name="index">The index in the provider (first one by default)</param>
        /// <returns></returns>
        public T GetParameters<T>(int index = 0) where T : Parameters => parameters[index].Get<T>();

        /// <summary>
        /// Get behaviors of a specific type
        /// </summary>
        /// <typeparam name="T">The type of Behavior</typeparam>
        /// <param name="index">The index in the provider (first on by default)</param>
        /// <returns></returns>
        public T GetBehavior<T>(int index = 0) where T : Behavior => behaviors[index].Get<T>();
    }
}
