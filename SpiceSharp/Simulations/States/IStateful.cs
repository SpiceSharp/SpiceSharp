using System;
using System.Collections.Generic;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// An interface describing a class that can contain states.
    /// </summary>
    public interface IStateful
    {
        /// <summary>
        /// Gets the state of the specified type.
        /// </summary>
        /// <typeparam name="S">The simulation state type.</typeparam>
        /// <returns>The type, or <c>null</c> if the state isn't used.</returns>
        S GetState<S>() where S : ISimulationState;

        /// <summary>
        /// Checks if the class uses the specified state.
        /// </summary>
        /// <typeparam name="S">The simulation state type.</typeparam>
        /// <returns>
        /// <c>true</c> if the class uses the state; otherwise <c>false</c>.
        /// </returns>
        bool UsesState<S>() where S : ISimulationState;

        /// <summary>
        /// Gets all the states that the class uses.
        /// </summary>
        /// <value>
        /// The states.
        /// </value>
        IEnumerable<Type> States { get; }
    }

    /// <summary>
    /// Contract for a class that uses an <see cref="ISimulationState"/>.
    /// </summary>
    /// <typeparam name="S">The type of simulation state.</typeparam>
    public interface IStateful<out S> : IStateful where S : ISimulationState
    {
        /// <summary>
        /// Gets the state.
        /// </summary>
        /// <value>
        /// The state.
        /// </value>
        S State { get; }
    }
}
