using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// A container for behaviors
    /// </summary>
    /// <seealso cref="ITypeDictionary{T}" />
    public interface IBehaviorContainer : 
        ITypeDictionary<IBehavior>, IExportPropertySet
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        string Name { get; }

        /// <summary>
        /// Gets the parameters.
        /// </summary>
        /// <value>
        /// The parameters.
        /// </value>
        IParameterSetDictionary Parameters { get; }

        /// <summary>
        /// Adds a behavior if the specified behavior does not yet exist in the container.
        /// </summary>
        /// <typeparam name="B">The behavior interface type.</typeparam>
        /// <param name="simulation">The simulation.</param>
        /// <param name="factory">The factory.</param>
        /// <returns>The container itself for chaining calls.</returns>
        IBehaviorContainer AddIfNo<B>(ISimulation simulation, Func<B> factory) where B : IBehavior;
    }
}
