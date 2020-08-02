using SpiceSharp.Simulations;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace SpiceSharp.Components.Subcircuits
{
    public partial class Frequency
    {
        /// <summary>
        /// An <see cref="IComplexSimulationState"/> that can be used with a local solver and solution.
        /// </summary>
        /// <seealso cref="LocalSolverState{T, S}" />
        /// <seealso cref="IComplexSimulationState" />
        protected class FlatSimulationState : FlatSolverState<Complex, IComplexSimulationState>,
            IComplexSimulationState
        {
            /// <inheritdoc/>
            public Complex Laplace => Parent.Laplace;

            /// <summary>
            /// Initializes a new instance of the <see cref="FlatSimulationState"/> class.
            /// </summary>
            /// <param name="name">The name.</param>
            /// <param name="parent">The parent.</param>
            /// <param name="nodes">The nodes.</param>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/>, <paramref name="parent"/> or <paramref name="nodes"/> is <c>null</c>.</exception>
            public FlatSimulationState(string name, IComplexSimulationState parent, IEnumerable<Bridge<string>> nodes)
                : base(name, parent, nodes)
            {
            }
        }
    }
}
