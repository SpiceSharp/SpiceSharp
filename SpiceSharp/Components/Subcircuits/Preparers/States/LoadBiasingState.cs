using System.Collections.Generic;
using SpiceSharp.Algebra;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.SubcircuitBehaviors
{
    /// <summary>
    /// Biasing state for <see cref="SubcircuitSimulation"/>.
    /// </summary>
    /// <seealso cref="IBiasingSimulationState" />
    public class LoadBiasingState : SubcircuitBiasingState, IBiasingSimulationState
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
            get => _parent.IsConvergent && _isConvergent;
            set => _isConvergent = value; 
        }
        private bool _isConvergent;

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
        public ISparseSolver<double> Solver => _solver;
        private SolverElementProvider<double> _solver;

        /// <summary>
        /// Gets the variable to index map.
        /// </summary>
        /// <value>
        /// The map.
        /// </value>
        public IVariableMap Map => _parent.Map;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadBiasingState"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        public LoadBiasingState(IBiasingSimulationState parent)
        {
            _parent = parent.ThrowIfNull(nameof(parent));
            _solver = new SolverElementProvider<double>(parent.Solver);
        }

        /// <summary>
        /// Notifies the state that these variables can be shared with other states.
        /// </summary>
        /// <param name="common">The common.</param>
        public override void ShareVariables(HashSet<Variable> common)
        {
        }

        /// <summary>
        /// Set up the simulation state for the simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public void Setup(ISimulation simulation)
        {
            Temperature = _parent.Temperature;
        }

        /// <summary>
        /// Destroys the simulation state.
        /// </summary>
        public void Unsetup()
        {
        }

        /// <summary>
        /// Resets the biasing state for loading the local behaviors.
        /// </summary>
        public override void Reset()
        {
            Solver.Reset();
            _isConvergent = true;
        }

        /// <summary>
        /// Apply changes locally.
        /// </summary>
        public override void ApplyAsynchroneously()
        {
        }

        /// <summary>
        /// Apply changes to the parent biasing state.
        /// </summary>
        public override void ApplySynchroneously()
        {
            _solver.ApplyElements();
            _parent.IsConvergent &= _isConvergent;
        }

        /// <summary>
        /// Determines whether the state solution is convergent.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is convergent; otherwise, <c>false</c>.
        /// </returns>
        public override bool CheckConvergence()
        {
            // Nothing to do here
            return true;
        }
    }
}
