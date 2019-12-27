using SpiceSharp.Algebra;
using SpiceSharp.Simulations;
using System.Collections.Generic;

namespace SpiceSharp.Components.SubcircuitBehaviors.Simple
{
    public partial class BiasingBehavior
    {
        /// <summary>
        /// An <see cref="IBiasingSimulationState" /> that can be used with a local solver and solution.
        /// </summary>
        /// <seealso cref="LocalSolverState{T}" />
        /// <seealso cref="IBiasingSimulationState" />
        protected class SimulationState : LocalSolverState<double>, IBiasingSimulationState
        {
            /// <summary>
            /// Gets the previous solution vector.
            /// </summary>
            /// <remarks>
            /// This vector is needed for determining convergence.
            /// </remarks>
            public IVector<double> OldSolution { get; private set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="SimulationState"/> class.
            /// </summary>
            /// <param name="parent">The parent simulation state.</param>
            /// <param name="solver">The solver.</param>
            public SimulationState(IBiasingSimulationState parent, ISparseSolver<double> solver)
                : base(parent, solver)
            {
            }

            /// <summary>
            /// Initializes the simulation state.
            /// </summary>
            public override void Initialize(IEnumerable<Variable> shared)
            {
                base.Initialize(shared);
                OldSolution = new DenseVector<double>(Solver.Size);
            }

            /// <summary>
            /// Updates the state with the new solution.
            /// </summary>
            public override void Update()
            {
                if (Updated)
                    return;

                // We need to keep track of the old solution
                var tmp = OldSolution;
                OldSolution = Solution;
                Solution = tmp;

                // Update the current solution
                base.Update();
            }
        }
    }
}
