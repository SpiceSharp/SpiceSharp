using SpiceSharp.Algebra;
using SpiceSharp.Simulations;
using System.Collections.Generic;

namespace SpiceSharp.Components.SubcircuitBehaviors.Simple
{
    public partial class BiasingBehavior
    {
        /// <summary>
        /// An <see cref="IBiasingSimulationState"/> that just maps nodes but uses the same solver.
        /// </summary>
        /// <seealso cref="SubcircuitSolverState{T, S}" />
        /// <seealso cref="IBiasingSimulationState" />
        protected class FlatSimulationState : FlatSolverState<double, IBiasingSimulationState>, IBiasingSimulationState
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
            /// Initializes a new instance of the <see cref="FlatSimulationState"/> class.
            /// </summary>
            /// <param name="name">The name.</param>
            /// <param name="parent">The parent.</param>
            /// <param name="nodes">The nodes.</param>
            public FlatSimulationState(string name, IBiasingSimulationState parent, IEnumerable<Bridge<string>> nodes)
                : base(name, parent, nodes)
            {
            }
        }
    }
}
