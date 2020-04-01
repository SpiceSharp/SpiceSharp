using SpiceSharp.Simulations;
using System.Numerics;

namespace SpiceSharp.Components.ParallelBehaviors
{
    public partial class FrequencyBehavior
    {
        /// <summary>
        /// An <see cref="IComplexSimulationState"/> that will insert a custom solver that allows concurrent write access.
        /// </summary>
        /// <seealso cref="IComplexSimulationState" />
        protected class ComplexSimulationState : ParallelSolverState<Complex, IComplexSimulationState>, IComplexSimulationState
        {
            Complex IComplexSimulationState.Laplace => Parent.Laplace;

            /// <summary>
            /// Initializes a new instance of the <see cref="ComplexSimulationState"/> class.
            /// </summary>
            /// <param name="parent">The parent.</param>
            public ComplexSimulationState(IComplexSimulationState parent)
                : base(parent)
            {
            }
        }
    }
}
