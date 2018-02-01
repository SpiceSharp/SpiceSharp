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
        Dictionary<Identifier, EntityBehaviors> entityBehaviors = new Dictionary<Identifier, EntityBehaviors>();

        /// <summary>
        /// Lists of behaviors
        /// </summary>
        Dictionary<Type, List<Behavior>> behaviors = new Dictionary<Type, List<Behavior>>();

        /// <summary>
        /// Add a behavior to the collection
        /// </summary>
        /// <param name="creator">Name of the entity creating the behavior</param>
        /// <param name="behavior">Created behavior</param>
        public void Add(Identifier creator, Behavior behavior)
        {
            EntityBehaviors eb;
            if (!entityBehaviors.TryGetValue(creator, out eb))
            {
                eb = new EntityBehaviors(creator);
                entityBehaviors.Add(creator, eb);
            }
            eb.Register(behavior);

            // Store in the behavior list
            Type basetype = behavior.GetType().BaseType;
            List<Behavior> list;
            if (!behaviors.TryGetValue(basetype, out list))
            {
                list = new List<Behavior>();
                behaviors.Add(basetype, list);
            }
            list.Add(behavior);
        }

        /// <summary>
        /// Get a list of behaviors
        /// </summary>
        /// <typeparam name="T">Behavior type</typeparam>
        /// <returns></returns>
        public Collection<T> GetBehaviorList<T>() where T : Behavior
        {
            if (behaviors.TryGetValue(typeof(T), out List<Behavior> list))
                return new Collection<T>(list.ConvertAll((Behavior b) => (T)b));
            return new Collection<T>();
        }

        /// <summary>
        /// Get the entity behaviors for a specific name
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns></returns>
        public EntityBehaviors GetEntityBehaviors(Identifier name)
        {
            if (entityBehaviors.TryGetValue(name, out EntityBehaviors result))
                return result;
            return null;
        }
    }
}
