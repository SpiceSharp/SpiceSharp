using System.Collections.Generic;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// Class that can be used for binding a <see cref="Behavior" /> to a simulation.
    /// </summary>
    public class BindingContext
    {
        /// <summary>
        /// Available behaviors and parameters
        /// </summary>
        private readonly Dictionary<string, ParameterSetDictionary> _parameterSets = new Dictionary<string, ParameterSetDictionary>();
        private readonly Dictionary<string, EntityBehaviorDictionary> _entityBehaviors = new Dictionary<string, EntityBehaviorDictionary>();

        /// <summary>
        /// Gets the parameter set dictionary count.
        /// </summary>
        public int ParametersCount => _parameterSets.Count;

        /// <summary>
        /// Gets the behavior dictionary count.
        /// </summary>
        public int BehaviorsCount => _entityBehaviors.Count;

        /// <summary>
        /// Adds a parameter set dictionary.
        /// </summary>
        /// <param name="name">The parameter set identifier.</param>
        /// <param name="pc">The parameter set dictionary.</param>
        /// <remarks>
        /// The <paramref name="name"/> parameter can be used by entities to give more information. For example, the entity parameter sets
        /// its own parameter sets and behaviors using the name "entity". In the same way, models can be added
        /// under "model", or other entities can be invoked where needed.
        /// </remarks>
        public void Add(string name, ParameterSetDictionary pc) => _parameterSets.Add(name, pc);

        /// <summary>
        /// Adds an entity behavior dictionary.
        /// </summary>
        /// <param name="name">The behavior dictionary identifier.</param>
        /// <param name="behaviors">The behavior dictionary.</param>
        /// <remarks>
        /// The <paramref name="name"/> parameter can be used by entities to give more information. For example, the entity parameter sets
        /// its own parameter sets and behaviors using the name "entity". In the same way, models can be added
        /// under "model", or other entities can be invoked where needed.
        /// </remarks>
        public void Add(string name, EntityBehaviorDictionary behaviors) => _entityBehaviors.Add(name, behaviors);

        /// <summary>
        /// Gets a parameter set for a specified identifier.
        /// </summary>
        /// <typeparam name="T">The base type of the parameter set.</typeparam>
        /// <param name="name">The identifier of the parameter set.</param>
        /// <returns>
        /// The requested object.
        /// </returns>
        public T GetParameterSet<T>(string name = "entity") where T : ParameterSet => _parameterSets[name].Get<T>();

        /// <summary>
        /// Tries getting a parameter set for a specified identifier.
        /// </summary>
        /// <typeparam name="T">The base type of the parameter set.</typeparam>
        /// <param name="name">The identifier of the parameter set.</param>
        /// <param name="value">The requested object.</param>
        /// <returns>
        ///   <c>true</c> if the object was found; otherwise, <c>false</c>.
        /// </returns>
        public bool TryGetParameterSet<T>(string name, out T value) where T : ParameterSet
        {
            if (_parameterSets.TryGetValue(name, out var r))
                return r.TryGet(out value);
            value = default;
            return false;
        }

        /// <summary>
        /// Gets the behavior for a specified identifier.
        /// </summary>
        /// <typeparam name="T">The base behavior type.</typeparam>
        /// <param name="name">The identifier of the behavior.</param>
        /// <returns>
        /// The requested object.
        /// </returns>
        public T GetBehavior<T>(string name = "entity") where T : IBehavior
        {
            if (_entityBehaviors.TryGetValue(name, out var ebd))
                return ebd.Get<T>();
            throw new CircuitException(
                "Cannot find behavior for {0} of type '{1}'".FormatString(name, typeof(T).FullName));
        }

        /// <summary>
        /// Tries getting the behavior for a specified identifier.
        /// </summary>
        /// <typeparam name="T">The base behavior type.</typeparam>
        /// <param name="name">The identifier of the behavior.</param>
        /// <param name="value">The requested object.</param>
        /// <returns>
        ///   <c>true</c> if the object was found; otherwise, <c>false</c>.
        /// </returns>
        public bool TryGetBehavior<T>(string name, out T value) where T : IBehavior
        {
            if (_entityBehaviors.TryGetValue(name, out var r))
                return r.TryGet(out value);
            value = default;
            return false;
        }
    }
}
