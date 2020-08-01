using SpiceSharp.Simulations;
using System;
using System.Numerics;

namespace SpiceSharp.Components.ParallelComponents
{
    public partial class Frequency
    {
        /// <summary>
        /// An <see cref="IComplexSimulationState"/> that will insert a custom solver that allows concurrent write access.
        /// </summary>
        /// <seealso cref="IComplexSimulationState" />
        protected class ComplexSimulationState : ParallelSolverState<Complex, IComplexSimulationState>,
            IComplexSimulationState
        {
            /// <inheritdoc/>
            Complex IComplexSimulationState.Laplace => Parent.Laplace;

            /// <summary>
            /// Initializes a new instance of the <see cref="ComplexSimulationState"/> class.
            /// </summary>
            /// <param name="parent">The parent.</param>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="parent"/> is <c>null</c>.</exception>
            public ComplexSimulationState(IComplexSimulationState parent)
                : base(parent)
            {
            }
        }
    }
}
