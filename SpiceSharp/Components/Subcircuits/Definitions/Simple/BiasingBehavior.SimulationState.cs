using SpiceSharp.Algebra;
using SpiceSharp.Simulations;
using System.Collections.Generic;

namespace SpiceSharp.Components.SubcircuitBehaviors.Simple
{
    public partial class BiasingBehavior
    {
        /// <summary>
        /// An <see cref="IBiasingSimulationState" /> that can be used with a local solver and solution.
        /// </summary>
        /// <seealso cref="LocalSolverState{T}" />
        /// <seealso cref="IBiasingSimulationState" />
        protected class SimulationState : LocalSolverState<double>, IBiasingSimulationState
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
            public double SourceFactor => _parent.SourceFactor;

            /// <summary>
            /// Gets or sets the a conductance that is shunted with PN junctions to aid convergence.
            /// </summary>
            public double Gmin => _parent.Gmin;

            /// <summary>
            /// Is the current iteration convergent?
            /// This parameter is used to communicate convergence.
            /// </summary>
            public bool IsConvergent { get; set; }

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
            /// Gets the previous solution vector.
            /// </summary>
            /// <remarks>
            /// This vector is needed for determining convergence.
            /// </remarks>
            public IVector<double> OldSolution { get; private set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="SimulationState"/> class.
            /// </summary>
            /// <param name="parent">The parent simulation state.</param>
            /// <param name="solver">The solver.</param>
            public SimulationState(IBiasingSimulationState parent, ISparseSolver<double> solver)
                : base(parent, solver)
            {
                _parent = parent.ThrowIfNull(nameof(parent));
            }

            /// <summary>
            /// Initializes the simulation state.
            /// </summary>
            public override void Initialize(IEnumerable<Variable> shared)
            {
                base.Initialize(shared);
                OldSolution = new DenseVector<double>(Solver.Size);
            }

            /// <summary>
            /// Applies the local solver to the parent solver.
            /// </summary>
            /// <returns>
            /// <c>true</c> if the application was successful; otherwise, <c>false</c>.
            /// </returns>
            /// <exception cref="NoEquivalentSubcircuitException">Thrown if no equivalent contributions could be calculated.</exception>
            public override bool Apply()
            {
                Temperature = _parent.Temperature;
                if (base.Apply())
                {
                    if (!IsConvergent)
                        _parent.IsConvergent = false;
                    return true;
                }
                return false;
            }

            /// <summary>
            /// Updates the state with the new solution.
            /// </summary>
            public override void Update()
            {
                if (Updated)
                    return;

                // We need to keep track of the old solution
                var tmp = OldSolution;
                OldSolution = Solution;
                Solution = tmp;

                // Update the current solution
                base.Update();
            }
        }
    }
}
