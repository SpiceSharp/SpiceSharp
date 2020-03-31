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
            IVariableSet<IVariable<Complex>> IVariableFactory<IVariable<Complex>>.Variables => _parent.Variables;
            IVariableSet IVariableFactory.Variables => _parent.Variables;

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
            public IVariable<Complex> GetSharedVariable(string name) => _parent.GetSharedVariable(name);

            IVariable IVariableFactory.GetSharedVariable(string name) => GetSharedVariable(name);

            /// <summary>
            /// Creates a local variable that should not be shared by the state with anyone else.
            /// </summary>
            /// <param name="name">The name.</param>
            /// <param name="unit">The unit of the variable.</param>
            /// <returns>
            /// The local variable.
            /// </returns>
            public IVariable<Complex> CreatePrivateVariable(string name, IUnit unit) => _parent.CreatePrivateVariable(name, unit);

            IVariable IVariableFactory.CreatePrivateVariable(string name, IUnit unit) => CreatePrivateVariable(name, unit);
        }
    }
}
