using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Entities
{
    /// <summary>
    /// Describes a strategy for resolving behaviors.
    /// </summary>
    public interface IBehaviorResolver
    {
        /// <summary>
        /// Registers a behavior factory for the specified target behavior after any
        /// previously defined factories.
        /// </summary>
        /// <param name="behavior">The behavior.</param>
        /// <param name="behaviorImplementation">The behavior implementation.</param>
        /// <returns>
        /// The <see cref="IBehaviorResolver" /> for chaining.
        /// </returns>
        IBehaviorResolver RegisterAfter(Type behavior, Type behaviorImplementation);

        /// <summary>
        /// Registers a behavior factory for the specified target behavior before any
        /// previously defined factories.
        /// </summary>
        /// <param name="behavior">The behavior.</param>
        /// <param name="behaviorImplementation">The behavior implementation.</param>
        /// <returns>
        /// The <see cref="IBehaviorResolver" /> for chaining.
        /// </returns>
        IBehaviorResolver RegisterBefore(Type behavior, Type behaviorImplementation);

        /// <summary>
        /// Creates behaviors in the <paramref name="container"/> for the specified <paramref name="simulation"/> and <paramref name="entity"/>.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="container">The behavior container.</param>
        void Resolve(ISimulation simulation, IEntity entity, IBehaviorContainer container);

        /// <summary>
        /// Clears any strategies.
        /// </summary>
        void Clear();
    }

    /// <summary>
    /// Describes a strategy for resolving behaviors that all accept the same type of context.
    /// </summary>
    /// <typeparam name="TContext">The type of binding context.</typeparam>
    public interface IBehaviorResolver<TContext> : IBehaviorResolver
        where TContext : IBindingContext
    {
        /// <summary>
        /// Registers a behavior factory for the specified target behavior after any
        /// previously defined factories.
        /// </summary>
        /// <typeparam name="TBehavior">The target behavior.</typeparam>
        /// <param name="factory">The factory.</param>
        /// <returns>The <see cref="IBehaviorResolver{TContext}"/> for chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="factory"/> is <c>null</c>.</exception>
        public IBehaviorResolver<TContext> RegisterAfter<TBehavior>(Func<TContext, IBehavior> factory)
            where TBehavior : IBehavior;

        /// <summary>
        /// Registers a behavior factory for the specified target behavior before any
        /// previously defined factories.
        /// </summary>
        /// <typeparam name="TBehavior">The target behavior.</typeparam>
        /// <param name="factory">The factory.</param>
        /// <returns>The <see cref="IBehaviorResolver{TContext}"/> for chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="factory"/> is <c>null</c>.</exception>
        public IBehaviorResolver<TContext> RegisterBefore<TBehavior>(Func<TContext, IBehavior> factory)
            where TBehavior : IBehavior;

        /// <summary>
        /// Registers a behavior factory for the specified target behavior after any
        /// previously defined factories.
        /// </summary>
        /// <typeparam name="TBehavior">The target behavior.</typeparam>
        /// <typeparam name="TBehaviorImpl">The behavior implementation.</typeparam>
        /// <returns>The <see cref="IBehaviorResolver{TContext}"/> for chaining.</returns>
        public IBehaviorResolver<TContext> RegisterAfter<TBehavior, TBehaviorImpl>()
            where TBehavior : IBehavior
            where TBehaviorImpl : TBehavior, IBehavior;

        /// <summary>
        /// Registers a behavior factory for the specified target behavior before any
        /// previously defined factories.
        /// </summary>
        /// <typeparam name="TBehavior">The target behavior.</typeparam>
        /// <typeparam name="TBehaviorImpl">The behavior implementation.</typeparam>
        /// <returns>The <see cref="IBehaviorResolver{TContext}"/> for chaining.</returns>
        public IBehaviorResolver<TContext> RegisterBefore<TBehavior, TBehaviorImpl>()
            where TBehavior : IBehavior
            where TBehaviorImpl : TBehavior, IBehavior;

        /// <summary>
        /// Resolves behaviors in the <paramref name="container"/> for the specified simulation and context.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="container">The container.</param>
        /// <param name="context">The context.</param>
        void Resolve(ISimulation simulation, IBehaviorContainer container, TContext context);
    }
}
