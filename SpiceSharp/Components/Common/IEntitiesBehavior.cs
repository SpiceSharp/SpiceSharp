using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.Common
{
    /// <summary>
    /// A description of a behavior that allows access to any local behaviors.
    /// </summary>
    /// <seealso cref="IBehavior"/>
    public interface IEntitiesBehavior : IBehavior
    {
        /// <summary>
        /// Gets a simulation state from a potentially local simulation.
        /// </summary>
        /// <typeparam name="S">The simulation state type.</typeparam>
        /// <returns>The simulation state.</returns>
        public S GetState<S>() where S : ISimulationState;

        /// <summary>
        /// Tries to get a simulation state from a potentially local simulation.
        /// </summary>
        /// <typeparam name="S">The simulation state type.</typeparam>
        /// <param name="state">The simulation state.</param>
        /// <returns>Returns <c>true</c> if the state was found; otherwise, <c>false</c>.</returns>
        public bool TryGetState<S>(out S state) where S : ISimulationState;

        /// <summary>
        /// Gets the local simulation behaviors.
        /// </summary>
        /// <value>
        /// The local behaviors.
        /// </value>
        public IBehaviorContainerCollection LocalBehaviors { get; }
    }
}
