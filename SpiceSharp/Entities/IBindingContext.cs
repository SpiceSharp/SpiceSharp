﻿using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Entities
{
    /// <summary>
    /// A template for a context that can be used to bind entity behaviors to a simulation.
    /// </summary>
    public interface IBindingContext
    {
        /// <summary>
        /// Gets a simulation state.
        /// </summary>
        /// <typeparam name="S">The type of simulation state.</typeparam>
        /// <returns>
        /// The simulation state.
        /// </returns>
        S GetState<S>() where S : ISimulationState;

        /// <summary>
        /// Tries to get a simulation state.
        /// </summary>
        /// <typeparam name="S">The type of simulation state.</typeparam>
        /// <param name="state">The simulation state.</param>
        /// <returns>
        /// <c>true</c> if the state was found; otherwise <c>false</c>.
        /// </returns>
        bool TryGetState<S>(out S state) where S : ISimulationState;

        /// <summary>
        /// Gets a simulation parameter set.
        /// </summary>
        /// <typeparam name="P">The type of parameter set.</typeparam>
        /// <returns>The parameter set.</returns>
        /// <exception cref="ArgumentException">Thrown if the parameter set was not found.</exception>
        public P GetSimulationParameterSet<P>() where P : IParameterSet;

        /// <summary>
        /// Tries to get a simulation parameter set.
        /// </summary>
        /// <typeparam name="P">The parameter set type.</typeparam>
        /// <param name="parameters">The parameter set.</param>
        /// <returns>
        /// <c>true</c> if the parameter set was found; otherwise <c>false</c>.
        /// </returns>
        public bool TryGetSimulationParameterSet<P>(out P parameters) where P : IParameterSet;

        /// <summary>
        /// Gets a parameter set.
        /// </summary>
        /// <typeparam name="P">The parameter set type.</typeparam>
        /// <returns>The parameter set.</returns>
        public P GetParameterSet<P>() where P : IParameterSet;

        /// <summary>
        /// Tries to get a parameter set.
        /// </summary>
        /// <typeparam name="P">The parameter set type.</typeparam>
        /// <param name="parameters">The parameters.</param>
        /// <returns>
        /// <c>true</c> if the parameter set was found; otherwise <c>false</c>;
        /// </returns>
        public bool TryGetParameterSet<P>(out P parameters) where P : IParameterSet;
    }
}
