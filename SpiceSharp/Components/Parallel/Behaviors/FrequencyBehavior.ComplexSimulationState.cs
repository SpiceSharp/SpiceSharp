using SpiceSharp.Algebra;
using SpiceSharp.Simulations;
using System.Numerics;

namespace SpiceSharp.Components.ParallelBehaviors
{
    public partial class FrequencyBehavior
    {
        /// <summary>
        /// An <see cref="IComplexSimulationState"/> that will insert a custom solver that allows concurrent write access.
        /// </summary>
        /// <seealso cref="IComplexSimulationState" />
        protected class ComplexSimulationState : IComplexSimulationState
        {
            private readonly IComplexSimulationState _parent;
            private readonly ParallelSolver<Complex> _solver;

            Complex IComplexSimulationState.Laplace => _parent.Laplace;
            ISparseSolver<Complex> ISolverSimulationState<Complex>.Solver => _solver;
            IVector<Complex> ISolverSimulationState<Complex>.Solution => _parent.Solution;
            IVariableMap ISolverSimulationState<Complex>.Map => _parent.Map;

            /// <summary>
            /// Initializes a new instance of the <see cref="ComplexSimulationState"/> class.
            /// </summary>
            /// <param name="parent">The parent.</param>
            public ComplexSimulationState(IComplexSimulationState parent)
            {
                _parent = parent.ThrowIfNull(nameof(parent));
                _solver = new ParallelSolver<Complex>(_parent.Solver);
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
