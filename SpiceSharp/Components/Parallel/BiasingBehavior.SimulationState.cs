using SpiceSharp.Algebra;
using SpiceSharp.Simulations;

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
