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

            /// <summary>
            /// Gets or sets the current laplace variable.
            /// </summary>
            public Complex Laplace => _parent.Laplace;

            /// <summary>
            /// Gets the solver used to solve the system of equations.
            /// </summary>
            /// <value>
            /// The solver.
            /// </value>
            public ISparseSolver<Complex> Solver => _solver;
            private readonly ParallelSolver<Complex> _solver;

            /// <summary>
            /// Gets the solution.
            /// </summary>
            /// <value>
            /// The solution.
            /// </value>
            public IVector<Complex> Solution => _parent.Solution;

            /// <summary>
            /// Gets the map that maps <see cref="Variable" /> to indices for the solver.
            /// </summary>
            /// <value>
            /// The map.
            /// </value>
            public IVariableMap Map => _parent.Map;

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
