using SpiceSharp.Algebra;
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
        protected class LocalSimulationState : LocalSolverState<Complex, IComplexSimulationState>, IComplexSimulationState
        {
            /// <summary>
            /// Gets or sets a value indicating whether the solution converges.
            /// </summary>
            public bool IsConvergent { get; set; }

            /// <summary>
            /// Gets or sets the current laplace variable.
            /// </summary>
            public Complex Laplace => Parent.Laplace;

            /// <summary>
            /// Initializes a new instance of the <see cref="LocalSimulationState"/> class.
            /// </summary>
            /// <param name="name">The name.</param>
            /// <param name="nodes">The nodes.</param>
            /// <param name="parent">The parent.</param>
            /// <param name="solver">The solver.</param>
            public LocalSimulationState(string name, IEnumerable<Bridge<string>> nodes, IComplexSimulationState parent, ISparseSolver<Complex> solver)
                : base(name, parent, nodes, solver)
            {
            }

            /// <summary>
            /// Applies the local solver to the parent solver.
            /// </summary>
            /// <returns>
            /// <c>true</c> if the application was successful; otherwise, <c>false</c>.
            /// </returns>
            /// <exception cref="NoEquivalentSubcircuitException">Thrown if no equivalent contributions could be calculated.</exception>
            public override bool Apply() => base.Apply();
        }
    }
}
