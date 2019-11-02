using SpiceSharp.Algebra;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.SubcircuitBehaviors
{
    /// <summary>
    /// An <see cref="IBiasingSimulationState"/> for a <see cref="BiasingBehavior"/> that can load in parallel.
    /// </summary>
    /// <seealso cref="ParallelLoadSolverState{T}" />
    /// <seealso cref="ISubcircuitBiasingSimulationState" />
    public class LoadBiasingState : ParallelLoadSolverState<double>, ISubcircuitBiasingSimulationState
    {
        /// <summary>
        /// The parent simulation state.
        /// </summary>
        private IBiasingSimulationState _parent;

        /// <summary>
        /// Gets or sets the initialization flag.
        /// </summary>
        public InitializationModes Init => _parent.Init;

        /// <summary>
        /// The current temperature for this circuit in Kelvin.
        /// </summary>
        public double Temperature { get; set; }

        /// <summary>
        /// The nominal temperature for the circuit in Kelvin.
        /// Used by models as the default temperature where the parameters were measured.
        /// </summary>
        public double NominalTemperature => _parent.NominalTemperature;

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
        public double SourceFactor => _parent.SourceFactor;

        /// <summary>
        /// Gets or sets the a conductance that is shunted with PN junctions to aid convergence.
        /// </summary>
        public double Gmin => _parent.Gmin;

        /// <summary>
        /// Is the current iteration convergent?
        /// This parameter is used to communicate convergence.
        /// </summary>
        public bool IsConvergent
        {
            get => _parent.IsConvergent;
            set => _parent.IsConvergent = value;
        }

        /// <summary>
        /// Gets the previous solution vector.
        /// </summary>
        /// <remarks>
        /// This vector is needed for determining convergence.
        /// </remarks>
        public IVector<double> OldSolution => _parent.OldSolution;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadBiasingState"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        public LoadBiasingState(IBiasingSimulationState parent)
            : base(parent, LUHelper.CreateSparseRealSolver())
        {
            _parent = parent.ThrowIfNull(nameof(parent));
        }

        /// <summary>
        /// Determines whether the state solution is convergent.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is convergent; otherwise, <c>false</c>.
        /// </returns>
        public bool CheckConvergence()
        {
            Update();
            return true;
        }
    }
}
