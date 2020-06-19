namespace SpiceSharp.Simulations
{
    /// <summary>
    /// An <see cref="ISimulationState"/> for a <see cref="Transient"/> analysis.
    /// </summary>
    /// <seealso cref="ISimulationState" />
    public interface ITimeSimulationState : ISimulationState
    {
        /// <summary>
        /// Gets or sets the flag for ignoring time-related effects. 
        /// If true, each device should assume the circuit is not moving in time.
        /// </summary>
        /// <value>
        /// <c>true</c> if the simulation only wants the DC solution; otherwise <c>false</c>.
        /// </value>
        bool UseDc { get; }

        /// <summary>
        /// Gets or sets the flag for using initial conditions.
        /// If true, the operating point will not be calculated, and initial conditions will be used instead.
        /// </summary>
        /// <value>
        /// <c>true</c> if entities should use their own initial conditions; otherwise <c>false</c>.
        /// </value>
        bool UseIc { get; }
    }
}
