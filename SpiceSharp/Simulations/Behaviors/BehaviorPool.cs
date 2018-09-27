using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
        private readonly Dictionary<Identifier, EntityBehaviorDictionary> _entityBehaviors = new Dictionary<Identifier, EntityBehaviorDictionary>();

        /// <summary>
        /// Lists of behaviors.
        /// </summary>
        private readonly Dictionary<Type, List<Behavior>> _behaviors = new Dictionary<Type, List<Behavior>>();

        /// <summary>
        /// Gets the associated <see cref="Behavior"/> of an entity.
        /// </summary>
        /// <value>
        /// The <see cref="EntityBehaviorDictionary"/>.
        /// </value>
        /// <param name="name">The entity identifier.</param>
        /// <returns>The behavior associated to the specified entity identifier.</returns>
        public EntityBehaviorDictionary this[Identifier name]
        {
            get
            {
                if (_entityBehaviors.TryGetValue(name, out var result))
                    return result;
                return null;
            }
        }

        /// <summary>
        /// Adds the specified behavior to the pool.
        /// </summary>
        /// <param name="creator">The entity identifier to which the .</param>
        /// <param name="behavior">The behavior.</param>
        /// <exception cref="SpiceSharp.CircuitException">Invalid behavior</exception>
        public void Add(Identifier creator, Behavior behavior)
        {
            if (!_entityBehaviors.TryGetValue(creator, out var eb))
            {
                eb = new EntityBehaviorDictionary(creator);
                _entityBehaviors.Add(creator, eb);
            }
            eb.Register(behavior);

            // Store in the behavior list
            var basetype = behavior.GetType().GetTypeInfo().BaseType ?? throw new CircuitException("Invalid behavior");
            if (!_behaviors.TryGetValue(basetype, out var list))
            {
                list = new List<Behavior>();
                _behaviors.Add(basetype, list);
            }
            list.Add(behavior);
        }

        /// <summary>
        /// Gets a list of behaviors of a specific type.
        /// </summary>
        /// <typeparam name="T">The base behavior type.</typeparam>
        /// <returns>
        /// A <see cref="BehaviorList{T}" /> with all behaviors of the specified type.
        /// </returns>
        public BehaviorList<T> GetBehaviorList<T>() where T : Behavior
        {
            if (_behaviors.TryGetValue(typeof(T), out var list))
                return new BehaviorList<T>(list.Cast<T>());

            return new BehaviorList<T>(new T[0]);
        }

        /// <summary>
        /// Gets the entity behaviors for a specific identifier.
        /// </summary>
        /// <param name="name">The identifier of the entity.</param>
        /// <returns>The behaviors associated to the specified entity identifier.</returns>
        [Obsolete]
        public EntityBehaviorDictionary GetEntityBehaviors(Identifier name)
        {
            if (_entityBehaviors.TryGetValue(name, out var result))
                return result;
            return null;
        }

        /// <summary>
        /// Checks if behaviors exist for a specified entity identifier.
        /// </summary>
        /// <param name="name">The entity identifier.</param>
        /// <returns>
        ///   <c>true</c> if behaviors exist; otherwise, <c>false</c>.
        /// </returns>
        public bool ContainsKey(Identifier name) => _entityBehaviors.ContainsKey(name);

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
