using SpiceSharp.Behaviors;
using System;
using System.Collections.Concurrent;

namespace SpiceSharp.Entities
{
    /// <summary>
    /// Factory for behaviors
    /// </summary>
    public class BehaviorFactoryDictionary : ConcurrentDictionary<Type, BehaviorFactoryMethod>
    {
        /// <summary>
        /// Add a factory method for a specific behavior type.
        /// </summary>
        /// <param name="type">The behavior type.</param>
        /// <param name="method">The factory method.</param>
        public void Add(Type type, BehaviorFactoryMethod method)
        {
            if (!TryAdd(type, method))
                throw new CircuitException("Invalid initialization");
        }
    }

    /// <summary>
    /// Create a behavior for an entity
    /// </summary>
    /// <param name="entity">The entity creating the behavior.</param>
    /// <returns></returns>
    public delegate IBehavior BehaviorFactoryMethod(Entity entity);
}
