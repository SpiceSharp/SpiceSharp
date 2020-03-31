using SpiceSharp.Algebra;
using SpiceSharp.Simulations;
using SpiceSharp.Simulations.Variables;
using System.Collections.Generic;

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
            IVariableSet<IVariable<double>> IVariableFactory<IVariable<double>>.Variables => _parent.Variables;
            IVariableSet IVariableFactory.Variables => _parent.Variables;

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

            /// <summary>
            /// Maps a shared node in the simulation.
            /// </summary>
            /// <param name="name">The name of the shared node.</param>
            /// <returns>
            /// The shared node variable.
            /// </returns>
            public IVariable<double> GetSharedVariable(string name) => _parent.GetSharedVariable(name);

            IVariable IVariableFactory.GetSharedVariable(string name) => GetSharedVariable(name);

            /// <summary>
            /// Creates a local variable that should not be shared by the state with anyone else.
            /// </summary>
            /// <param name="name">The name.</param>
            /// <param name="unit">The unit of the variable.</param>
            /// <returns>
            /// The local variable.
            /// </returns>
            public IVariable<double> CreatePrivateVariable(string name, IUnit unit) => _parent.CreatePrivateVariable(name, unit);

            IVariable IVariableFactory.CreatePrivateVariable(string name, IUnit unit) => CreatePrivateVariable(name, unit);
        }
    }
}
