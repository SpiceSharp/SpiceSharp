using SpiceSharp.Algebra;
using SpiceSharp.Simulations;
using SpiceSharp.Simulations.Variables;
using System.Collections.Generic;
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

            /// <summary>
            /// Maps a shared node in the simulation.
            /// </summary>
            /// <param name="name">The name of the shared node.</param>
            /// <returns>
            /// The shared node variable.
            /// </returns>
            public IVariable<Complex> MapNode(string name) => _parent.MapNode(name);

            /// <summary>
            /// Maps a number of nodes.
            /// </summary>
            /// <param name="names">The nodes.</param>
            /// <returns>
            /// The shared node variables.
            /// </returns>
            public IEnumerable<IVariable<Complex>> MapNodes(IEnumerable<string> names) => _parent.MapNodes(names);

            /// <summary>
            /// Creates a local variable that should not be shared by the state with anyone else.
            /// </summary>
            /// <param name="name">The name.</param>
            /// <param name="unit">The unit of the variable.</param>
            /// <returns>
            /// The local variable.
            /// </returns>
            public IVariable<Complex> Create(string name, IUnit unit) => _parent.Create(name, unit);

            /// <summary>
            /// Determines whether the specified variable is a node without mapping it.
            /// </summary>
            /// <param name="name">The name of the node.</param>
            /// <returns>
            /// <c>true</c> if the specified variable has node; otherwise, <c>false</c>.
            /// </returns>
            public bool HasNode(string name) => _parent.HasNode(name);
        }
    }
}
