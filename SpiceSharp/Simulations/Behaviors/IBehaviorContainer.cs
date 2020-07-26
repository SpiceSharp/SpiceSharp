using SpiceSharp.General;
using SpiceSharp.ParameterSets;
using SpiceSharp.Simulations;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// A container for behaviors
    /// </summary>
    /// <seealso cref="IParameterSetCollection"/>
    public interface IBehaviorContainer :
        IParameterSetCollection
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name of the behavior container.
        /// </value>
        /// <remarks>
        /// This is typically the name of the entity that creates the behaviors in this container.
        /// </remarks>
        string Name { get; }

        /// <summary>
        /// Gets the behaviors in the container.
        /// </summary>
        IEnumerable<IBehavior> Values { get; }

        /// <summary>
        /// Adds a behavior to the container.
        /// </summary>
        /// <param name="behavior">The behavior.</param>
        void Add(IBehavior behavior);

        /// <summary>
        /// Gets a behavior from the container of the specified type.
        /// </summary>
        /// <typeparam name="B">The behavior type.</typeparam>
        /// <returns>The behavior.</returns>
        /// <exception cref="TypeNotFoundException">Thrown if a behavior of type <typeparamref name="B"/> could not be found.</exception>
        /// <exception cref="AmbiguousTypeException">Thrown if there are multiple values for type <typeparamref name="B"/>.</exception>
        B GetValue<B>() where B : IBehavior;

        /// <summary>
        /// Tries to get behavior from the dictionary.
        /// </summary>
        /// <typeparam name="B">The behavior type.</typeparam>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if a behavior of the specified key was found; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="AmbiguousTypeException">If the value could not be uniquely determined.</exception>
        bool TryGetValue<B>(out B value) where B : IBehavior;

        /// <summary>
        /// Determines whether the container contains one or more behaviors of the specified type.
        /// </summary>
        /// <typeparam name="B">The behavior type.</typeparam>
        /// <returns>
        ///     <c>true</c> if the container contains one or more values of the specified type.
        /// </returns>
        bool Contains<B>() where B : IBehavior;

        /// <summary>
        /// Determines whether the container contains the specified behavior.
        /// </summary>
        /// <param name="value">The behavior.</param>
        /// <returns>
        ///     <c>true</c> if the behavior is part of the container; otherwise, <c>false</c>.
        /// </returns>
        bool Contains(IBehavior value);

        /// <summary>
        /// Adds a behavior if the specified behavior does not yet exist in the container.
        /// </summary>
        /// <typeparam name="Target">The target behavior type.</typeparam>
        /// <param name="simulation">The simulation.</param>
        /// <param name="factory">The factory.</param>
        /// <returns>The container itself for chaining calls.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="simulation"/> or <paramref name="factory"/> is <c>null</c>.</exception>
        IBehaviorContainer AddIfNo<Target>(ISimulation simulation, Func<IBehavior> factory) where Target : IBehavior;
    }
}
