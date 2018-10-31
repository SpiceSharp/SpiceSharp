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
        /// Listen to a type.
        /// </summary>
        /// <param name="type">The type.</param>
        public void ListenTo(Type type)
        {
            if (!_behaviors.ContainsKey(type))
                _behaviors.Add(type, new List<IBehavior>());
        }

        /// <summary>
        /// Adds the specified behavior to the pool.
        /// </summary>
        /// <param name="creator">The entity identifier to which the .</param>
        /// <param name="behavior">The behavior.</param>
        /// <exception cref="SpiceSharp.CircuitException">Invalid behavior</exception>
        public void Add(string creator, IBehavior behavior)
        {
            if (!_entityBehaviors.TryGetValue(creator, out var eb))
            {
                eb = new EntityBehaviorDictionary(creator);
                _entityBehaviors.Add(creator, eb);
            }
            eb.Register(behavior);

            // Add when listened to
            var currentType = behavior.GetType();
            while (currentType != null)
            {
                if (_behaviors.TryGetValue(currentType, out var l))
                    l.Add(behavior);
                currentType = currentType.GetTypeInfo().BaseType;
            }

            foreach (var i in behavior.GetType().GetTypeInfo().GetInterfaces())
            {
                if (_behaviors.TryGetValue(i, out var l))
                    l.Add(behavior);
            }
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
