using SpiceSharp.Entities;
using System;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// A builder for behavior containers.
    /// </summary>
    /// <typeparam name="TContext">The type of binding context.</typeparam>
    public interface IBehaviorContainerBuilder<TContext>
        where TContext : IBindingContext
    {
        /// <summary>
        /// Adds a behavior if the specified behavior does not yet exist in the container.
        /// </summary>
        /// <typeparam name="TBehavior">The target type of the behavior.</typeparam>
        /// <param name="factory">The factory.</param>
        /// <returns>
        /// The original container builder. This can be used for chaining.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="factory"/> is <c>null</c>.</exception>
        IBehaviorContainerBuilder<TContext> AddIfNo<TBehavior>(Func<TContext, IBehavior> factory)
            where TBehavior : IBehavior;
    }
}
