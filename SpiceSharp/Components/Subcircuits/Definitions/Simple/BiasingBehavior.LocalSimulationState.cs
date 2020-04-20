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
        /// <seealso cref="LocalSolverState{T, S}" />
        /// <seealso cref="IBiasingSimulationState" />
        protected class LocalSimulationState : LocalSolverState<double, IBiasingSimulationState>, IBiasingSimulationState
        {
            /// <summary>
            /// Gets the previous solution vector.
            /// </summary>
            /// <remarks>
            /// This vector is needed for determining convergence.
            /// </remarks>
            public IVector<double> OldSolution { get; private set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="LocalSimulationState"/> class.
            /// </summary>
            /// <param name="name">The name of the subcircuit instance.</param>
            /// <param name="parent">The parent simulation state.</param>
            /// <param name="solver">The solver.</param>
            public LocalSimulationState(string name, IBiasingSimulationState parent, ISparsePivotingSolver<double> solver)
                : base(name, parent, solver)
            {
            }

            /// <summary>
            /// Initializes the specified shared.
            /// </summary>
            /// <param name="nodes">The nodes.</param>
            public override void Initialize(IReadOnlyList<Bridge<string>> nodes)
            {
                base.Initialize(nodes);
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
                OldSolution = LocalSolution;
                LocalSolution = tmp;

                // Update the current solution
                base.Update();
            }
        }
    }
}
