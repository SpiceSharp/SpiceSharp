namespace SpiceSharp.Simulations
{
    /// <summary>
    /// The simulation state.
    /// </summary>
    public interface ISimulationState
    {
        /// <summary>
        /// Set up the simulation state for the simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        void Setup(ISimulation simulation);

        /// <summary>
        /// Destroys the simulation state.
        /// </summary>
        void Unsetup();
    }
}
