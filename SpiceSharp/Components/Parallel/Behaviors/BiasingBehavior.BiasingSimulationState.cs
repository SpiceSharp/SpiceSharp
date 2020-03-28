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
            public IVariable<double> MapNode(string name) => _parent.MapNode(name);

            /// <summary>
            /// Maps a number of nodes.
            /// </summary>
            /// <param name="names">The nodes.</param>
            /// <returns>
            /// The shared node variables.
            /// </returns>
            public IEnumerable<IVariable<double>> MapNodes(IEnumerable<string> names) => _parent.MapNodes(names);

            /// <summary>
            /// Creates a local variable that should not be shared by the state with anyone else.
            /// </summary>
            /// <param name="name">The name.</param>
            /// <param name="unit">The unit of the variable.</param>
            /// <returns>
            /// The local variable.
            /// </returns>
            public IVariable<double> Create(string name, IUnit unit) => _parent.Create(name, unit);

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
