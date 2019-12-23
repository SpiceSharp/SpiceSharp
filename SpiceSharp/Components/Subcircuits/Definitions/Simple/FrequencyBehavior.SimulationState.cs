using SpiceSharp.Algebra;
using SpiceSharp.Simulations;
using System.Numerics;

namespace SpiceSharp.Components.SubcircuitBehaviors.Simple
{
    public partial class FrequencyBehavior
    {
        /// <summary>
        /// An <see cref="IComplexSimulationState"/> that can be used with a local solver and solution.
        /// </summary>
        /// <seealso cref="LocalSolverState{T}" />
        /// <seealso cref="IComplexSimulationState" />
        protected class SimulationState : LocalSolverState<Complex>, IComplexSimulationState
        {
            private readonly IComplexSimulationState _parent;

            /// <summary>
            /// Gets or sets a value indicating whether the solution converges.
            /// </summary>
            public bool IsConvergent { get; set; }

            /// <summary>
            /// Gets or sets the current laplace variable.
            /// </summary>
            public Complex Laplace => _parent.Laplace;

            /// <summary>
            /// Initializes a new instance of the <see cref="SimulationState"/> class.
            /// </summary>
            /// <param name="parent">The parent simulation state.</param>
            /// <param name="solver">The solver.</param>
            public SimulationState(IComplexSimulationState parent, ISparseSolver<Complex> solver)
                : base(parent, solver)
            {
                _parent = parent.ThrowIfNull(nameof(parent));
            }


            /// <summary>
            /// Applies the local solver to the parent solver.
            /// </summary>
            /// <returns>
            /// <c>true</c> if the application was successful; otherwise, <c>false</c>.
            /// </returns>
            /// <exception cref="NoEquivalentSubcircuitException">Thrown if no equivalent contributions could be calculated.</exception>
            public override bool Apply()
            {
                if (base.Apply())
                {
                    if (!IsConvergent)
                        _parent.IsConvergent = false;
                    return true;
                }
                return false;
            }
        }
    }
}
