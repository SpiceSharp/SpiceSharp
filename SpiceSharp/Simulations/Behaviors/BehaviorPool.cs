using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharp.Circuits;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// A pool of all behaviors. This class will keep track which behavior belongs to which entity. Only behaviors can be requested from the collection.
    /// </summary>
    public class BehaviorPool
    {
        /// <summary>
        /// Behaviors indexed by the entity that created them.
        /// </summary>
        private readonly Dictionary<string, EntityBehaviorDictionary> _entityBehaviors;

        /// <summary>
        /// Lists of behaviors.
        /// </summary>
        private readonly Dictionary<Type, List<IBehavior>> _behaviors = new Dictionary<Type, List<IBehavior>>();

        /// <summary>
        /// Gets the associated <see cref="Behavior"/> of an entity.
        /// </summary>
        /// <value>
        /// The <see cref="EntityBehaviorDictionary"/>.
        /// </value>
        /// <param name="name">The entity identifier.</param>
        /// <returns>The behavior associated to the specified entity identifier.</returns>
        public EntityBehaviorDictionary this[string name]
        {
            get
            {
                if (_entityBehaviors.TryGetValue(name, out var result))
                    return result;
                return null;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BehaviorPool"/> class.
        /// </summary>
        public BehaviorPool()
        {
            _entityBehaviors = new Dictionary<string, EntityBehaviorDictionary>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BehaviorPool"/> class.
        /// </summary>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}" /> implementation to use when comparing entity names, or <c>null</c> to use the default <see cref="EqualityComparer{T}"/>.</param>
        public BehaviorPool(IEqualityComparer<string> comparer)
        {
            _entityBehaviors = new Dictionary<string, EntityBehaviorDictionary>(comparer);
        }

        /// <summary>
        /// Adds the specified behavior to the pool.
        /// </summary>
        /// <param name="type">The type of the requested behavior.</param>
        /// <param name="creator">The entity identifier to which the .</param>
        /// <param name="behavior">The behavior to be added.</param>
        /// <exception cref="SpiceSharp.CircuitException">Invalid behavior</exception>
        public void Add(Type type, string creator, IBehavior behavior)
        {
            // Create the list entry if necessary
            if (!_behaviors.TryGetValue(type, out var behaviorList))
            {
                behaviorList = new List<IBehavior>();
                _behaviors.Add(type, behaviorList);
            }

            // Find the entity behaviors
            if (!_entityBehaviors.TryGetValue(behavior.Name, out var ebd))
            {
                ebd = new EntityBehaviorDictionary(behavior.Name);
                _entityBehaviors.Add(behavior.Name, ebd);
            }

            // Add the behavior
            ebd.Add(behavior.GetType(), behavior);
            behaviorList.Add(behavior);
        }

        /// <summary>
        /// Gets a list of behaviors of a specific type.
        /// </summary>
        /// <typeparam name="T">The base behavior type.</typeparam>
        /// <returns>
        /// A <see cref="BehaviorList{T}" /> with all behaviors of the specified type.
        /// </returns>
        public BehaviorList<T> GetBehaviorList<T>() where T : IBehavior
        {
            if (_behaviors.TryGetValue(typeof(T), out var list))
                return new BehaviorList<T>(list.Cast<T>());
            return new BehaviorList<T>(new T[0]);
        }

        /// <summary>
        /// Gets the entity behaviors for a specific identifier. Obsolete, use the indexer instead.
        /// </summary>
        /// <param name="name">The identifier of the entity.</param>
        /// <returns>The behaviors associated to the specified entity identifier.</returns>
        [Obsolete]
        public EntityBehaviorDictionary GetEntityBehaviors(string name)
        {
            if (_entityBehaviors.TryGetValue(name, out var result))
                return result;
            return null;
        }

        /// <summary>
        /// Tries to the get the entity behaviors by a specified identifier.
        /// </summary>
        /// <param name="name">The identifier.</param>
        /// <param name="ebd">The dictionary of entity behaviors.</param>
        /// <returns></returns>
        public bool TryGetBehaviors(string name, out EntityBehaviorDictionary ebd) =>
            _entityBehaviors.TryGetValue(name, out ebd);

        /// <summary>
        /// Checks if behaviors exist for a specified entity identifier.
        /// </summary>
        /// <param name="name">The entity identifier.</param>
        /// <returns>
        ///   <c>true</c> if behaviors exist; otherwise, <c>false</c>.
        /// </returns>
        public bool ContainsKey(string name) => _entityBehaviors.ContainsKey(name);

        /// <summary>
        /// Clears all behaviors in the pool.
        /// </summary>
        public void Clear()
        {
            _behaviors.Clear();
            _entityBehaviors.Clear();
        }
    }
}
