using SpiceSharp.Behaviors;
using System;
using System.Collections.Generic;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// An interface that describes a class that can use behaviors.
    /// </summary>
    public interface IBehavioral
    {
        /// <summary>
        /// Gets all behavior types that are used by the class.
        /// </summary>
        /// <value>
        /// The behaviors.
        /// </value>
        IEnumerable<Type> Behaviors { get; }

        /// <summary>
        /// Gets the entity behaviors.
        /// </summary>
        /// <value>
        /// The entity behaviors.
        /// </value>
        IBehaviorContainerCollection EntityBehaviors { get; }

        /// <summary>
        /// Checks if the class uses the specified behaviors.
        /// </summary>
        /// <typeparam name="B">The behavior type.</typeparam>
        /// <returns>
        ///   <c>true</c> if the class uses the behavior; otherwise <c>false</c>.
        /// </returns>
        bool UsesBehaviors<B>() where B : IBehavior;

        /// <summary>
        /// Checks if the class uses the specified behavior.
        /// </summary>
        /// <param name="types">The behavior (interface) type.</param>
        /// <returns>
        ///     <c>true</c> if the class uses the behavior; otherwise <c>false</c>.
        /// </returns>
        bool UsesBehavior(Type types);
    }

    /// <summary>
    /// Specifies that the class uses behaviors of a certain type.
    /// </summary>
    /// <typeparam name="B">The behavior type that is used by the class.</typeparam>
    public interface IBehavioral<B> : IBehavioral
        where B : IBehavior
    {
    }
}
