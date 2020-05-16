using System;
using System.Collections.Generic;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// An interface that describes a collection of <see cref="IBehaviorContainer"/> instances.
    /// </summary>
    public interface IBehaviorContainerCollection : IEnumerable<IBehaviorContainer>
    {
        /// <summary>
        /// Occurs when a behavior has not been found.
        /// </summary>
        event EventHandler<BehaviorsNotFoundEventArgs> BehaviorsNotFound;

        /// <summary>
        /// Gets the number of behavior containers in the collection.
        /// </summary>
        /// <value>
        /// The number of behavior containers.
        /// </value>
        int Count { get; }

        /// <summary>
        /// Gets the behavior container names.
        /// </summary>
        /// <value>
        /// An <see cref="IEnumerable{T}"/> object that contains the name of each <see cref="BehaviorContainer"/>.
        /// </value>
        IEnumerable<string> Keys { get; }

        /// <summary>
        /// Gets the comparer used to identify elements.
        /// </summary>
        /// <value>
        /// The comparer.
        /// </value>
        IEqualityComparer<string> Comparer { get; }

        /// <summary>
        /// Gets the <see cref="IBehaviorContainer"/> with the specified name.
        /// </summary>
        /// <value>
        /// The <see cref="IBehaviorContainer"/>.
        /// </value>
        /// <param name="name">The name.</param>
        /// <returns>The container associated to the specified name.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        /// <exception cref="BehaviorsNotFoundException">Thrown if the behavior container of the specified name could not be found.</exception>
        IBehaviorContainer this[string name] { get; }

        /// <summary>
        /// Adds the <see cref="IBehaviorContainer"/> with the specified name.
        /// </summary>
        /// <param name="container">The behavior container.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="container"/> if <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown if a behavior container already exists with the same name.</exception>
        void Add(IBehaviorContainer container);

        /// <summary>
        /// Gets a list of behaviors of a specific behavior type.
        /// </summary>
        /// <typeparam name="B">The <see cref="IBehavior"/> type.</typeparam>
        /// <returns>
        /// A <see cref="BehaviorList{T}"/> with all behaviors of the specified type.
        /// </returns>
        BehaviorList<B> GetBehaviorList<B>() where B : IBehavior;

        /// <summary>
        /// Tries to get the behavior container associated with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="container">The container.</param>
        /// <returns>
        /// <c>true</c> if the behavior was found; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        bool TryGetBehaviors(string name, out IBehaviorContainer container);

        /// <summary>
        /// Determines whether this instance contains a container by the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>
        ///   <c>true</c> if the collection contains the container; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        bool Contains(string name);

        /// <summary>
        /// Clears all containers in the collection.
        /// </summary>
        void Clear();
    }
}
