using System;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// Component behaviors
    /// </summary>
    public class EntityBehaviors : TypeDictionary<Behavior>
    {
        /// <summary>
        /// Source entity
        /// </summary>
        public Identifier Source { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="source">The source of the behaviors</param>
        public EntityBehaviors(Identifier source)
            : base(typeof(Func<Behavior>))
        {
            Source = source;
        }

        /// <summary>
        /// Register a behavior
        /// </summary>
        /// <param name="behavior">Behavior</param>
        public void Register(Behavior behavior)
        {
            if (behavior == null)
                throw new ArgumentNullException(nameof(behavior));

            // Get types
            Type mytype = behavior.GetType();
            Type basetype = mytype.BaseType;

            // Register types
            Dictionary[mytype] = behavior;
            Dictionary[basetype] = behavior;
        }

        /// <summary>
        /// Get a behavior
        /// </summary>
        /// <typeparam name="T">Behavior type</typeparam>
        /// <returns></returns>
        public T Get<T>() where T : Behavior
        {
            if (Dictionary.TryGetValue(typeof(T), out Behavior behavior))
                return (T)behavior;
            return null;
        }
    }
}
