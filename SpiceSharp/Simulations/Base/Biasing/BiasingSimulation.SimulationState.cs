using SpiceSharp.Algebra;

namespace SpiceSharp.Simulations
{
    public abstract partial class BiasingSimulation
    {
        /// <summary>
        /// A simulation state for simulations using real numbers.
        /// </summary>
        /// <seealso cref="IBiasingSimulationState" />
        private class SimulationState : IBiasingSimulationState
        {
            /// <summary>
            /// The current temperature for this circuit in Kelvin.
            /// </summary>
            public double Temperature { get; set; } = 300.15;

            /// <summary>
            /// The nominal temperature for the circuit in Kelvin.
            /// Used by models as the default temperature where the parameters were measured.
            /// </summary>
            public double NominalTemperature { get; set; } = 300.15;

            /// <summary>
            /// Gets the solution vector.
            /// </summary>
            public IVector<double> Solution { get; protected set; }

            /// <summary>
            /// Gets the previous solution vector.
            /// </summary>
            /// <remarks>
            /// This vector is needed for determining convergence.
            /// </remarks>
            public IVector<double> OldSolution { get; protected set; }

            /// <summary>
            /// Gets the map.
            /// </summary>
            /// <value>
            /// The map.
            /// </value>
            public IVariableMap Map { get; }

            /// <summary>
            /// Gets the sparse solver.
            /// </summary>
            /// <value>
            /// The solver.
            /// </value>
            public ISparseSolver<double> Solver { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="SimulationState" /> class.
            /// </summary>
            /// <param name="solver">The solver.</param>
            /// <param name="map">The variable map.</param>
            public SimulationState(ISparseSolver<double> solver, IVariableMap map)
            {
                Solver = solver.ThrowIfNull(nameof(solver));
                Map = map.ThrowIfNull(nameof(map));
            }

            /// <summary>
            /// Sets up the simulation state.
            /// </summary>
            public void Setup()
            {
                // Initialize all matrices
                Solution = new DenseVector<double>(Solver.Size);
                OldSolution = new DenseVector<double>(Solver.Size);
                Solver.Reset();
            }

            /// <summary>
            /// Stores the solution.
            /// </summary>
            public void StoreSolution()
            {
                var tmp = OldSolution;
                OldSolution = Solution;
                Solution = tmp;
            }
        }
    }
}
