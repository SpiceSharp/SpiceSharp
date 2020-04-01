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
        protected class BiasingSimulationState : ParallelSolverState<double, IBiasingSimulationState>, IBiasingSimulationState
        {
            /// <summary>
            /// Gets the solution vector of the last computed iteration.
            /// </summary>
            /// <value>
            /// The solution to the last iteration.
            /// </value>
            /// <remarks>
            /// This vector is needed for determining convergence.
            /// </remarks>
            public IVector<double> OldSolution => Parent.OldSolution;

            /// <summary>
            /// Initializes a new instance of the <see cref="BiasingSimulationState"/> class.
            /// </summary>
            /// <param name="parent">The parent biasing simulation state.</param>
            public BiasingSimulationState(IBiasingSimulationState parent)
                : base(parent)
            {
            }
        }
    }
}
