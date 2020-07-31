using SpiceSharp.Algebra;
using SpiceSharp.Simulations;
using System;
using System.Collections.Generic;

namespace SpiceSharp.Components.Subcircuits
{
    public partial class Biasing
    {
        /// <summary>
        /// An <see cref="IBiasingSimulationState"/> that just maps nodes but uses the same solver.
        /// </summary>
        /// <seealso cref="SubcircuitSolverState{T, S}" />
        /// <seealso cref="IBiasingSimulationState" />
        protected class FlatSimulationState : FlatSolverState<double, IBiasingSimulationState>,
            IBiasingSimulationState
        {
            /// <inheritdoc/>
            public IVector<double> OldSolution => Parent.OldSolution;

            /// <summary>
            /// Initializes a new instance of the <see cref="FlatSimulationState"/> class.
            /// </summary>
            /// <param name="name">The name.</param>
            /// <param name="parent">The parent.</param>
            /// <param name="nodes">The nodes.</param>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/>, <paramref name="parent"/> or <paramref name="nodes"/> is <c>null</c>.</exception>
            public FlatSimulationState(string name, IBiasingSimulationState parent, IEnumerable<Bridge<string>> nodes)
                : base(name, parent, nodes)
            {
            }
        }
    }
}
