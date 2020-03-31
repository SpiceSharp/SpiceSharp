using SpiceSharp.Simulations;
using System.Collections.Generic;
using System.Numerics;

namespace SpiceSharp.Components.SubcircuitBehaviors.Simple
{
    public partial class FrequencyBehavior
    {
        /// <summary>
        /// An <see cref="IComplexSimulationState"/> that can be used with a local solver and solution.
        /// </summary>
        /// <seealso cref="LocalSolverState{T, S}" />
        /// <seealso cref="IComplexSimulationState" />
        protected class FlatSimulationState : FlatSolverState<Complex, IComplexSimulationState>, IComplexSimulationState
        {
            /// <summary>
            /// Gets or sets the current laplace variable.
            /// </summary>
            public Complex Laplace => Parent.Laplace;

            /// <summary>
            /// Initializes a new instance of the <see cref="FlatSimulationState"/> class.
            /// </summary>
            /// <param name="name">The name.</param>
            /// <param name="nodes">The nodes.</param>
            /// <param name="parent">The parent.</param>
            public FlatSimulationState(string name, IEnumerable<Bridge<string>> nodes, IComplexSimulationState parent)
                : base(name, nodes, parent)
            {
            }
        }
    }
}
