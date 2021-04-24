using SpiceSharp.Behaviors;
using SpiceSharp.ParameterSets;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Entities
{
    /// <summary>
    /// A template for a context that can be used to bind entity behaviors to a simulation.
    /// </summary>
    public interface IBindingContext
    {
        /// <summary>
        /// Gets the previously created behaviors.
        /// </summary>
        /// <value>
        /// The previously created behaviors.
        /// </value>
        IBehaviorContainer Behaviors { get; }

        /// <summary>
        /// Gets a simulation state.
        /// </summary>
        /// <typeparam name="S">The type of simulation state.</typeparam>
        /// <returns>
        /// The simulation state.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown if the state is not defined on the simulation.</exception>
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
        public P GetSimulationParameterSet<P>() where P : IParameterSet, ICloneable<P>;

        /// <summary>
        /// Tries to get a simulation parameter set.
        /// </summary>
        /// <typeparam name="P">The parameter set type.</typeparam>
        /// <param name="parameters">The parameter set.</param>
        /// <returns>
        /// <c>true</c> if the parameter set was found; otherwise <c>false</c>.
        /// </returns>
        public bool TryGetSimulationParameterSet<P>(out P parameters) where P : IParameterSet, ICloneable<P>;

        /// <summary>
        /// Gets the parameter set of the specified type.
        /// </summary>
        /// <typeparam name="P">The parameter set type.</typeparam>
        /// <returns>
        /// The parameter set.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown if the parameter set could not be found.</exception>
        public P GetParameterSet<P>() where P : IParameterSet, ICloneable<P>;

        /// <summary>
        /// Tries to get the parameter set of the specified type.
        /// </summary>
        /// <typeparam name="P">The parameter set type.</typeparam>
        /// <param name="value">The parameter set.</param>
        /// <returns>
        /// <c>true</c> if the parameter set was found; otherwise, <c>false</c>.
        /// </returns>
        public bool TryGetParameterSet<P>(out P value) where P : IParameterSet, ICloneable<P>;
    }
}
