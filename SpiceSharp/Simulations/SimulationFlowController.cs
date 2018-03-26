namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Abstract class for controlling the flow of simulations
    /// </summary>
    public abstract class SimulationFlowController
    {
        /// <summary>
        /// Initialize simulation flow controller
        /// </summary>
        /// <param name="simulation">Simulation</param>
        public abstract void Initialize(Simulation simulation);

        /// <summary>
        /// Decide if the simulation should continue with another simulation or not
        /// </summary>
        /// <param name="simulation">Simulation</param>
        /// <returns>True if another simulation needs to be executed</returns>
        public abstract bool ContinueExecution(Simulation simulation);

        /// <summary>
        /// Finalize simulation flow
        /// </summary>
        /// <param name="simulation">Simulation</param>
        public abstract void Finalize(Simulation simulation);
    }
}
