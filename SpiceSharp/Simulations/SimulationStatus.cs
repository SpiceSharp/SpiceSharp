namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Possible statuses for a simulation.
    /// </summary>
    public enum SimulationStatus
    {
        /// <summary>
        /// Indicates that the simulation has not started.
        /// </summary>
        None,

        /// <summary>
        /// Indicates that the simulation is now in its setup phase.
        /// </summary>
        Setup,

        /// <summary>
        /// Indicates that the simulation is validating the input.
        /// </summary>
        Validation,

        /// <summary>
        /// Indicates that the simulation is running.
        /// </summary>
        Running,

        /// <summary>
        /// Indicates that the simulation is cleaning up all its resources.
        /// </summary>
        Unsetup
    }
}
