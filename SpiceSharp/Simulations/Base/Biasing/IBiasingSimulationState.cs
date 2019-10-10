using SpiceSharp.Algebra;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Simulation state for a <see cref="BiasingSimulation"/>.
    /// </summary>
    /// <seealso cref="ISimulationState" />
    public interface  IBiasingSimulationState : ISimulationState
    {
        /// <summary>
        /// Gets or sets the initialization flag.
        /// </summary>
        InitializationModes Init { get; }

        /// <summary>
        /// Gets or sets the flag for ignoring time-related effects. If true, each device should assume the circuit is not moving in time.
        /// </summary>
        bool UseDc { get; }

        /// <summary>
        /// Gets or sets the flag for using initial conditions. If true, the operating point will not be calculated, and initial conditions will be used instead.
        /// </summary>
        bool UseIc { get; }

        /// <summary>
        /// The current source factor.
        /// This parameter is changed when doing source stepping for aiding convergence.
        /// </summary>
        /// <remarks>
        /// In source stepping, all sources are considered to be at 0 which has typically only one single solution (all nodes and
        /// currents are 0V and 0A). By increasing the source factor in small steps, it is possible to progressively reach a solution
        /// without having non-convergence.
        /// </remarks>
        double SourceFactor { get; set; }

        /// <summary>
        /// Gets or sets the a conductance that is shunted with PN junctions to aid convergence.
        /// </summary>
        double Gmin { get; set; }

        /// <summary>
        /// Is the current iteration convergent?
        /// This parameter is used to communicate convergence.
        /// </summary>
        bool IsConvergent { get; set; }

        /// <summary>
        /// The current temperature for this circuit in Kelvin.
        /// </summary>
        double Temperature { get; set; }

        /// <summary>
        /// The nominal temperature for the circuit in Kelvin.
        /// Used by models as the default temperature where the parameters were measured.
        /// </summary>
        double NominalTemperature { get; }

        /// <summary>
        /// Gets the solution vector.
        /// </summary>
        IVector<double> Solution { get; }

        /// <summary>
        /// Gets the previous solution vector.
        /// </summary>
        /// <remarks>
        /// This vector is needed for determining convergence.
        /// </remarks>
        IVector<double> OldSolution { get; }

        /// <summary>
        /// Gets the sparse solver.
        /// </summary>
        /// <value>
        /// The solver.
        /// </value>
        ISparseSolver<double> Solver { get; }
    }
}
