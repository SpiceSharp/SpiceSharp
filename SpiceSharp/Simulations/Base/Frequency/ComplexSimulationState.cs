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
            /// Gets or sets a value indicating whether the solution converges.
            /// </summary>
            public bool IsConvergent { get; set; }

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
            public ISolver<Complex> Solver { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="ComplexSimulationState"/> class.
            /// </summary>
            public ComplexSimulationState()
            {
                Solver = LUHelper.CreateSparseComplexSolver();
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="ComplexSimulationState"/> class.
            /// </summary>
            /// <param name="solver">The solver.</param>
            public ComplexSimulationState(ISolver<Complex> solver)
            {
                Solver = solver.ThrowIfNull(nameof(solver));
            }

            /// <summary>
            /// Set up the simulation state for the simulation.
            /// </summary>
            /// <param name="simulation">The simulation.</param>
            public void Setup(ISimulation simulation)
            {
                Solution = new DenseVector<Complex>(Solver.Size);
            }

            /// <summary>
            /// Unsetup the state.
            /// </summary>
            public void Unsetup()
            {
                Solution = null;

                // TODO: Clear all for the matrix
                Solver.Reset();
            }
        }
    }
}
