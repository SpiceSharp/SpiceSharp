using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// Collection for behaviors
    /// This class will keep track which behavior belongs to which entity. Only behaviors can be requested from the collection
    /// </summary>
    public class BehaviorPool
    {
        /// <summary>
        /// Behaviors indexed by the entity that created them
        /// </summary>
        private Dictionary<Identifier, EntityBehaviorDictionary> _entityBehaviors = new Dictionary<Identifier, EntityBehaviorDictionary>();

        /// <summary>
        /// Lists of behaviors
        /// </summary>
        private Dictionary<Type, List<Behavior>> _behaviors = new Dictionary<Type, List<Behavior>>();

        /// <summary>
        /// Add a behavior to the collection
        /// </summary>
        /// <param name="creator">Name of the entity creating the behavior</param>
        /// <param name="behavior">Created behavior</param>
        public void Add(Identifier creator, Behavior behavior)
        {
            EntityBehaviorDictionary eb;
            if (!_entityBehaviors.TryGetValue(creator, out eb))
            {
                eb = new EntityBehaviorDictionary(creator);
                _entityBehaviors.Add(creator, eb);
            }
            eb.Register(behavior);

            // Store in the behavior list
            Type basetype = behavior.GetType().BaseType;
            List<Behavior> list;
            if (!_behaviors.TryGetValue(basetype, out list))
            {
                list = new List<Behavior>();
                _behaviors.Add(basetype, list);
            }
            list.Add(behavior);
        }

        /// <summary>
        /// Gets a list of behaviors
        /// </summary>
        /// <typeparam name="T">Behavior type</typeparam>
        /// <returns></returns>
        public Collection<T> GetBehaviorList<T>() where T : Behavior
        {
            if (_behaviors.TryGetValue(typeof(T), out List<Behavior> list))
                return new Collection<T>(list.ConvertAll((Behavior b) => (T)b));
            return new Collection<T>();
        }

        /// <summary>
        /// Gets the entity behaviors for a specific name
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns></returns>
        public EntityBehaviorDictionary GetEntityBehaviors(Identifier name)
        {
            if (_entityBehaviors.TryGetValue(name, out EntityBehaviorDictionary result))
                return result;
            return null;
        }
    }
}
