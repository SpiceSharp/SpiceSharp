using SpiceSharp.Algebra;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Components.ParallelComponents
{
    public partial class Biasing
    {
        /// <summary>
        /// An <see cref="IBiasingSimulationState"/> that will insert a custom solver that allows concurrent write access.
        /// </summary>
        /// <seealso cref="ParallelSolverState{T, S}"/>
        /// <seealso cref="IBiasingSimulationState" />
        protected class BiasingSimulationState : ParallelSolverState<double, IBiasingSimulationState>,
            IBiasingSimulationState
        {
            /// <inheritdoc/>
            public IVector<double> OldSolution => Parent.OldSolution;

            /// <summary>
            /// Initializes a new instance of the <see cref="BiasingSimulationState"/> class.
            /// </summary>
            /// <param name="parent">The parent biasing simulation state.</param>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="parent"/> is <c>null</c>.</exception>
            public BiasingSimulationState(IBiasingSimulationState parent)
                : base(parent)
            {
            }
        }
    }
}
