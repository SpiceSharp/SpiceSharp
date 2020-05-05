using SpiceSharp.ParameterSets;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// A container for behaviors
    /// </summary>
    /// <seealso cref="ITypeDictionary{T}" />
    /// <seealso cref="IParameterSetCollection"/>
    public interface IBehaviorContainer : 
        ITypeDictionary<IBehavior>,
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
        /// Adds a behavior if the specified behavior does not yet exist in the container.
        /// </summary>
        /// <typeparam name="B">The behavior interface type.</typeparam>
        /// <param name="simulation">The simulation.</param>
        /// <param name="factory">The factory.</param>
        /// <returns>The container itself for chaining calls.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="simulation"/> or <paramref name="factory"/> is <c>null</c>.</exception>
        IBehaviorContainer AddIfNo<B>(ISimulation simulation, Func<B> factory) where B : IBehavior;
    }
}
