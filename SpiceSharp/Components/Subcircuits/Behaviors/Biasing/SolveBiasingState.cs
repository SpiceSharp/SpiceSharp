using System;
using SpiceSharp.Algebra;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.SubcircuitBehaviors
{
    /// <summary>
    /// An <see cref="IBiasingSimulationState"/> for <see cref="BiasingBehavior"/> that can solve in parallel.
    /// </summary>
    /// <seealso cref="ISubcircuitBiasingSimulationState" />
    public class SolveBiasingState : ParallelSolveSolverState<double>, ISubcircuitBiasingSimulationState
    {
        private double _relTol, _absTol;
        private IBiasingSimulationState _parent;
        private bool _isConvergent;

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
            get => _parent.IsConvergent && _isConvergent;
            set => _isConvergent = value;
        }

        /// <summary>
        /// Gets the previous solution vector.
        /// </summary>
        /// <remarks>
        /// This vector is needed for determining convergence.
        /// </remarks>
        public IVector<double> OldSolution { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SolveBiasingState"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="bp">The parameters for the state.</param>
        public SolveBiasingState(IBiasingSimulationState parent, BiasingParameters bp)
            : base(parent, LUHelper.CreateSparseRealSolver())
        {
            _parent = parent;
            _relTol = bp.RelativeTolerance;
            _absTol = bp.AbsoluteTolerance;
        }

        /// <summary>
        /// Set up the simulation state for the simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public override void Setup(ISimulation simulation)
        {
            base.Setup(simulation);
            OldSolution = new DenseVector<double>(Solver.Size);
            Temperature = _parent.Temperature;
        }

        /// <summary>
        /// Resets the biasing state for loading the local behaviors.
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            _isConvergent = true;
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

            // Check convergence for each node
            foreach (var v in Map)
            {
                var node = v.Key;
                var n = Solution[v.Value];
                var o = OldSolution[v.Value];

                if (double.IsNaN(n))
                    throw new CircuitException("Non-convergence, node {0} is not a number".FormatString(node.Name));

                if (node.UnknownType == VariableType.Voltage)
                {
                    var tol = _relTol * Math.Max(Math.Abs(n), Math.Abs(o)) + _absTol;
                    if (Math.Abs(n - o) > tol)
                        return false;
                }
                else
                {
                    var tol = _relTol * Math.Max(Math.Abs(n), Math.Abs(o)) + _absTol;
                    if (Math.Abs(n - o) > tol)
                        return false;
                }
            }

            // Check for convergence on the variables, similar to BiasingSimulation.
            return true;
        }
    }
}
