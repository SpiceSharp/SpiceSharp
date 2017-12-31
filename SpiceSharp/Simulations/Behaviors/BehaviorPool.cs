using System;
using System.Collections.Generic;
using SpiceSharp.Circuits;

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
        Dictionary<Identifier, EntityBehaviors> entitybehaviors = new Dictionary<Identifier, EntityBehaviors>();

        /// <summary>
        /// Lists of behaviors
        /// </summary>
        Dictionary<Type, List<Behavior>> behaviors = new Dictionary<Type, List<Behavior>>();

        /// <summary>
        /// The source entity of the last registered behavior
        /// </summary>
        EntityBehaviors active;

        /// <summary>
        /// Set the current entity of which behaviors can be requested
        /// </summary>
        /// <param name="id">Name of the active entity</param>
        public void SetCurrentEntity(Identifier id)
        {
            if (!entitybehaviors.TryGetValue(id, out active))
                active = null;
        }

        /// <summary>
        /// Add a behavior to the collection
        /// </summary>
        /// <param name="creator">Name of the entity creating the behavior</param>
        /// <param name="behavior">Created behavior</param>
        public void Add(Identifier creator, Behavior behavior)
        {
            EntityBehaviors eb;
            if (!entitybehaviors.TryGetValue(creator, out eb))
            {
                eb = new EntityBehaviors(creator);
                entitybehaviors.Add(creator, eb);
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
        /// Get a behavior
        /// </summary>
        /// <typeparam name="T">Behavior base type</typeparam>
        /// <param name="source">Name of the entity that created the behavior</param>
        /// <returns></returns>
        public T GetBehavior<T>(Identifier source) where T : Behavior
        {
            if (entitybehaviors.TryGetValue(source, out EntityBehaviors eb))
                return eb.Get<T>();
            return null;
        }

        /// <summary>
        /// Get a behavior of the entity that last registered a behavior
        /// </summary>
        /// <typeparam name="T">Behavior type</typeparam>
        /// <returns></returns>
        public T GetBehavior<T>() where T : Behavior => active?.Get<T>();

        /// <summary>
        /// Get a list of behaviors
        /// </summary>
        /// <typeparam name="T">Behavior base type</typeparam>
        /// <returns></returns>
        public T[] GetBehaviors<T>() where T : Behavior
        {
            if (behaviors.TryGetValue(typeof(T), out List<Behavior> list))
                return (T[])list.ToArray();
            return new T[] { };
        }

        /// <summary>
        /// Get a list of behaviors
        /// </summary>
        /// <typeparam name="T">Behavior type</typeparam>
        /// <returns></returns>
        public List<T> GetBehaviorList<T>() where T : Behavior
        {
            if (behaviors.TryGetValue(typeof(T), out List<Behavior> list))
                return list.ConvertAll((Behavior b) => (T)b);
            return new List<T>();
        }
    }
}
