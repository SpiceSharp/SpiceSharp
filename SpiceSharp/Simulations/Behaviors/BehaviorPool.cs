using System;
using System.Collections.Generic;
using System.Linq;

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
        /// Lists of behaviors indexed by type of behavior.
        /// </summary>
        private readonly Dictionary<Type, List<IBehavior>> _behaviorLists = new Dictionary<Type, List<IBehavior>>();

        /// <summary>
        /// Gets the number of behaviors in the pool.
        /// </summary>
        public int Count
        {
            get
            {
                var count = 0;
                foreach (var pair in _behaviorLists)
                    count += pair.Value.Count;
                return count;
            }
        }

        /// <summary>
        /// Gets the behavior keys.
        /// </summary>
        public IEnumerable<string> Keys => _entityBehaviors.Keys;

        /// <summary>
        /// Gets the associated <see cref="Behavior"/> of an entity.
        /// </summary>
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
        /// <param name="types">The types for which a list will be kept which can be retrieved later.</param>
        public BehaviorPool(IEnumerable<Type> types)
        {
            types.ThrowIfNull(nameof(types));

            _entityBehaviors = new Dictionary<string, EntityBehaviorDictionary>();
            foreach (var type in types)
                _behaviorLists.Add(type, new List<IBehavior>());
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
        /// Initializes a new instance of the <see cref="BehaviorPool"/> class.
        /// </summary>
        /// <param name="comparer">The comparer.</param>
        /// <param name="types">The types.</param>
        public BehaviorPool(IEqualityComparer<string> comparer, Type[] types)
        {
            types.ThrowIfNull(nameof(types));

            _entityBehaviors = new Dictionary<string, EntityBehaviorDictionary>(comparer);
            foreach (var type in types)
                _behaviorLists.Add(type, new List<IBehavior>());
        }

        /// <summary>
        /// Adds the specified behavior.
        /// </summary>
        /// <param name="behavior">The behavior.</param>
        public void Add(IBehavior behavior)
        {
            behavior.ThrowIfNull(nameof(behavior));

            // Try finding the entity behavior dictionary
            if (!_entityBehaviors.TryGetValue(behavior.Name, out var ebd))
            {
                ebd = new EntityBehaviorDictionary(behavior.Name);
                _entityBehaviors.Add(behavior.Name, ebd);
            }
            ebd.Add(behavior.GetType(), behavior);

            // Track lists
            foreach (var pair in _behaviorLists)
            {
                if (ebd.TryGetValue(pair.Key, out var b) && b == behavior)
                    pair.Value.Add(b);
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
            if (_behaviorLists.TryGetValue(typeof(T), out var list))
                return new BehaviorList<T>(list.Cast<T>());
            return new BehaviorList<T>(new T[0]);
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
            _behaviorLists.Clear();
            _entityBehaviors.Clear();
        }
    }
}
