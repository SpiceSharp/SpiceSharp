using SpiceSharp.Algebra;
using SpiceSharp.Circuits.Entities.Local;
using SpiceSharp.Simulations;

namespace SpiceSharp.Entities.ParallelLoaderBehaviors
{
    /// <summary>
    /// Biasing state for <see cref="ParallelEntity"/>.
    /// </summary>
    /// <seealso cref="IBiasingSimulationState" />
    public class BiasingSimulationState : IBiasingSimulationState
    {
        private IBiasingSimulationState _parent;

        /// <summary>
        /// Gets or sets the initialization flag.
        /// </summary>
        public InitializationModes Init => _parent.Init;

        /// <summary>
        /// Gets or sets the flag for ignoring time-related effects. If true, each device should assume the circuit is not moving in time.
        /// </summary>
        public bool UseDc => _parent.UseDc;

        /// <summary>
        /// Gets or sets the flag for using initial conditions. If true, the operating point will not be calculated, and initial conditions will be used instead.
        /// </summary>
        public bool UseIc => _parent.UseIc;

        /// <summary>
        /// The current source factor.
        /// This parameter is changed when doing source stepping for aiding convergence.
        /// </summary>
        /// <remarks>
        /// In source stepping, all sources are considered to be at 0 which has typically only one single solution (all nodes and
        /// currents are 0V and 0A). By increasing the source factor in small steps, it is possible to progressively reach a solution
        /// without having non-convergence.
        /// </remarks>
        public double SourceFactor { get => _parent.SourceFactor; set => _parent.SourceFactor = value; }

        /// <summary>
        /// Gets or sets the a conductance that is shunted with PN junctions to aid convergence.
        /// </summary>
        public double Gmin { get => _parent.Gmin; set => _parent.Gmin = value; }

        /// <summary>
        /// Is the current iteration convergent?
        /// This parameter is used to communicate convergence.
        /// </summary>
        public bool IsConvergent { get => _parent.IsConvergent; set => _parent.IsConvergent = value; }

        /// <summary>
        /// The current temperature for this circuit in Kelvin.
        /// </summary>
        public double Temperature { get => _parent.Temperature; set => _parent.Temperature = value; }

        /// <summary>
        /// The nominal temperature for the circuit in Kelvin.
        /// Used by models as the default temperature where the parameters were measured.
        /// </summary>
        public double NominalTemperature => _parent.NominalTemperature;

        /// <summary>
        /// Gets the solution vector.
        /// </summary>
        public IVector<double> Solution => _parent.Solution;

        /// <summary>
        /// Gets the previous solution vector.
        /// </summary>
        /// <remarks>
        /// This vector is needed for determining convergence.
        /// </remarks>
        public IVector<double> OldSolution => _parent.OldSolution;

        /// <summary>
        /// Gets the sparse solver.
        /// </summary>
        /// <value>
        /// The solver.
        /// </value>
        public ISolver<double> Solver { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BiasingSimulationState"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        public BiasingSimulationState(IBiasingSimulationState parent)
        {
            _parent = parent.ThrowIfNull(nameof(parent));
            Solver = new LocalSolver<double>(_parent.Solver);
        }

        /// <summary>
        /// Set up the simulation state for the simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public void Setup(ISimulation simulation)
        {
        }

        /// <summary>
        /// Destroys the simulation state.
        /// </summary>
        public void Unsetup()
        {
        }
    }
}
