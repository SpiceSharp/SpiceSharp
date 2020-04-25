namespace SpiceSharp.Simulations
{
    /// <summary>
    /// An <see cref="ISimulationState"/> that tracks the current iteration mode.
    /// This state is used to help iterating to a solution using some tricks specific to a <see cref="BiasingSimulation"/>.
    /// </summary>
    /// <seealso cref="ISimulationState" />
    public interface IIterationSimulationState : ISimulationState
    {
        /// <summary>
        /// Gets the iteration mode.
        /// </summary>
        /// <value>
        /// The mode.
        /// </value>
        public IterationModes Mode { get; }

        /// <summary>
        /// The current source factor.
        /// This parameter is changed when doing source stepping for aiding convergence.
        /// </summary>
        /// <remarks>
        /// In source stepping, all sources are considered to be at 0 which has typically only one single solution (all nodes and
        /// currents are 0V and 0A). By increasing the source factor in small steps, it is possible to progressively reach a solution
        /// without having non-convergence.
        /// </remarks>
        double SourceFactor { get; }

        /// <summary>
        /// Gets or sets the a conductance that is shunted with PN junctions to aid convergence.
        /// </summary>
        /// <value>
        /// The minimum conductance for PN junctions.
        /// </value>
        double Gmin { get; }

        /// <summary>
        /// Is the current iteration convergent?
        /// This parameter is used to communicate convergence.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this the iteration is convergent; otherwise, <c>false</c>.
        /// </value>
        bool IsConvergent { get; set; }
    }

    /// <summary>
    /// Possible modes for initialization of behaviors.
    /// </summary>
    public enum IterationModes
    {
        /// <summary>
        /// The default mode.
        /// </summary>
        None,

        /// <summary>
        /// Indicates that nodes may still be everywhere, and a first solution should be calculated.
        /// </summary>
        Float,

        /// <summary>
        /// Indicates that PN junctions or other difficult-to-converge dependencies should be initialized to a starting voltage.
        /// </summary>
        /// <remarks>
        /// PN junction often don't behave well in iterative methods due to their exponential dependency. A good initial value can be critical.
        /// </remarks>
        Junction,

        /// <summary>
        /// Indicates that an initial iteration has been done and that we need to fix the solution to check for convergence.
        /// </summary>
        Fix
    }
}
