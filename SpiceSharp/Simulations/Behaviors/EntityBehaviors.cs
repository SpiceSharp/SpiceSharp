using System;
using System.Collections.Generic;
using SpiceSharp.Circuits;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// Component behaviors
    /// </summary>
    public class EntityBehaviors
    {
        /// <summary>
        /// Source entity
        /// </summary>
        public Identifier Source { get; }

        /// <summary>
        /// Behavior lists (in order of registration)
        /// </summary>
        Dictionary<Type, Behavior> behaviors = new Dictionary<Type, Behavior>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="source">The source of the behaviors</param>
        public EntityBehaviors(Identifier source)
        {
            Source = source;
        }

        /// <summary>
        /// Register a behavior
        /// </summary>
        /// <param name="behavior">Behavior</param>
        public void Register(Behavior behavior)
        {
            // Get types
            Type mytype = behavior.GetType();
            Type basetype = mytype.BaseType;

            // Register types
            behaviors[mytype] = behavior;
            behaviors[basetype] = behavior;
        }

        /// <summary>
        /// Get a behavior
        /// </summary>
        /// <typeparam name="T">Behavior type</typeparam>
        /// <returns></returns>
        public T Get<T>() where T : Behavior
        {
            if (behaviors.TryGetValue(typeof(T), out Behavior behavior))
                return (T)behavior;
            return null;
        }
    }
}
