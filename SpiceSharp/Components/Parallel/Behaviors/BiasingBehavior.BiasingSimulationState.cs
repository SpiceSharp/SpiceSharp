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
        protected class BiasingSimulationState : IBiasingSimulationState
        {
            private readonly IBiasingSimulationState _parent;
            private readonly ParallelSolver<double> _solver;

            IVector<double> ISolverSimulationState<double>.Solution => _parent.Solution;
            IVector<double> IBiasingSimulationState.OldSolution => _parent.OldSolution;
            ISparseSolver<double> ISolverSimulationState<double>.Solver => _solver;
            IVariableMap ISolverSimulationState<double>.Map => _parent.Map;

            /// <summary>
            /// Initializes a new instance of the <see cref="BiasingSimulationState"/> class.
            /// </summary>
            /// <param name="parent">The parent biasing simulation state.</param>
            public BiasingSimulationState(IBiasingSimulationState parent)
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
            /// Applies the changes to the common solver.
            /// </summary>
            public void Apply()
            {
                _solver.Apply();
            }
        }
    }
}
