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
        /// <exception cref="TypeNotFoundException">Thrown if the simulation state is not defined on this instance.</exception>
        S GetState<S>() where S : ISimulationState;

        /// <summary>
        /// Tries to get a state of the specified type.
        /// </summary>
        /// <typeparam name="S">The simulation state type.</typeparam>
        /// <param name="state">The state.</param>
        /// <returns>
        ///   <c>true</c> if the state exists; otherwise <c>false</c>.
        /// </returns>
        bool TryGetState<S>(out S state) where S : ISimulationState;

        /// <summary>
        /// Checks if the class uses the specified state.
        /// </summary>
        /// <typeparam name="S">The simulation state type.</typeparam>
        /// <returns>
        ///   <c>true</c> if the class uses the state; otherwise <c>false</c>.
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
