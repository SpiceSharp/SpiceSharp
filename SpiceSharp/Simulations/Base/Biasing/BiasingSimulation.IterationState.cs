namespace SpiceSharp.Simulations.Biasing
{
    /// <summary>
    /// An <see cref="IIterationSimulationState"/> for a <see cref="BiasingSimulation"/>.
    /// </summary>
    /// <seealso cref="IIterationSimulationState" />
    public class IterationState : IIterationSimulationState
    {
        /// <summary>
        /// Gets or sets the iteration mode.
        /// </summary>
        /// <value>
        /// The mode.
        /// </value>
        public IterationModes Mode { get; set; }

        /// <summary>
        /// The current source factor.
        /// This parameter is changed when doing source stepping for aiding convergence.
        /// </summary>
        /// <remarks>
        /// In source stepping, all sources are considered to be at 0 which has typically only one single solution (all nodes and
        /// currents are 0V and 0A). By increasing the source factor in small steps, it is possible to progressively reach a solution
        /// without having non-convergence.
        /// </remarks>
        public double SourceFactor { get; set; } = 1.0;

        /// <summary>
        /// Gets or sets the a conductance that is shunted with PN junctions to aid convergence.
        /// </summary>
        public double Gmin { get; set; } = 1e-12;

        /// <summary>
        /// Is the current iteration convergent?
        /// This parameter is used to communicate convergence.
        /// </summary>
        public bool IsConvergent { get; set; } = true;
    }
}
