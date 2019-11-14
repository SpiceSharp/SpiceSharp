namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Contract for a class that uses an <see cref="ISimulationState"/>.
    /// </summary>
    /// <typeparam name="S">The type of simulation state.</typeparam>
    public interface IStateful<S> : ISimulation where S : ISimulationState
    {
        /// <summary>
        /// Gets the state.
        /// </summary>
        /// <value>
        /// The state.
        /// </value>
        S State { get; }

        /// <summary>
        /// Gets the state.
        /// </summary>
        /// <param name="state">The state.</param>
        void GetState(out S state);
    }
}
