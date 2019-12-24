using SpiceSharp.Algebra;
using SpiceSharp.Simulations;
using System.Collections.Generic;

namespace SpiceSharp.Components.ParallelBehaviors
{
    public partial class BiasingBehavior
    {
        /// <summary>
        /// An <see cref="IBiasingSimulationState"/> that will insert a custom solver that allows concurrent write access.
        /// </summary>
        /// <seealso cref="IBiasingSimulationState" />
        protected class SimulationState : IBiasingSimulationState
        {
            private readonly IBiasingSimulationState _parent;

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
            public IVector<double> OldSolution => _parent.OldSolution;

            /// <summary>
            /// Gets the solver used to solve the system of equations.
            /// </summary>
            /// <value>
            /// The solver.
            /// </value>
            public ISparseSolver<double> Solver => _solver;
            private readonly ParallelSolver<double> _solver;

            /// <summary>
            /// Gets the solution.
            /// </summary>
            /// <value>
            /// The solution.
            /// </value>
            public IVector<double> Solution => _parent.Solution;

            /// <summary>
            /// Gets the map that maps <see cref="Variable" /> to indices for the solver.
            /// </summary>
            /// <value>
            /// The map.
            /// </value>
            public IVariableMap Map => _parent.Map;

            /// <summary>
            /// Initializes a new instance of the <see cref="SimulationState"/> class.
            /// </summary>
            /// <param name="parent">The parent biasing simulation state.</param>
            public SimulationState(IBiasingSimulationState parent)
            {
                _parent = parent.ThrowIfNull(nameof(parent));
                _solver = new ParallelSolver<double>(parent.Solver);
            }

            /// <summary>
            /// Resets the elements.
            /// </summary>
            public void Reset()
            {
                _solver.Reset();
            }

            /// <summary>
            /// Applies the changes to the state.
            /// </summary>
            public void Apply()
            {
                _solver.Apply();
            }
        }
    }
}
