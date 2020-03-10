using System.Numerics;
using SpiceSharp.Algebra;

namespace SpiceSharp.Simulations
{
    public abstract partial class FrequencySimulation
    {
        /// <summary>
        /// A simulation state using complex numbers.
        /// </summary>
        /// <seealso cref="IComplexSimulationState" />
        protected class ComplexSimulationState : IComplexSimulationState
        {
            /// <summary>
            /// Gets the solution.
            /// </summary>
            public IVector<Complex> Solution { get; protected set; }

            /// <summary>
            /// Gets or sets the current laplace variable.
            /// </summary>
            public Complex Laplace { get; set; } = new Complex();

            /// <summary>
            /// Gets the solver.
            /// </summary>
            /// <value>
            /// The solver.
            /// </value>
            public ISparseSolver<Complex> Solver { get; }

            /// <summary>
            /// Gets the variable to index map.
            /// </summary>
            /// <value>
            /// The map.
            /// </value>
            public IVariableMap Map { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="ComplexSimulationState" /> class.
            /// </summary>
            /// <param name="solver">The solver.</param>
            /// <param name="map">The map.</param>
            public ComplexSimulationState(ISparseSolver<Complex> solver, IVariableMap map)
            {
                Solver = solver.ThrowIfNull(nameof(solver));
                Map = map.ThrowIfNull(nameof(map));
            }

            /// <summary>
            /// Set up the simulation state for the simulation.
            /// </summary>
            /// <param name="simulation">The simulation.</param>
            public void Setup(ISimulation simulation)
            {
                Solution = new DenseVector<Complex>(Solver.Size);
            }
        }
    }
}
