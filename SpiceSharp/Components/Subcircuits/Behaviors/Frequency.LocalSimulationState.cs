using SpiceSharp.Algebra;
using SpiceSharp.Simulations;
using System;
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
        protected class LocalSimulationState : LocalSolverState<Complex, IComplexSimulationState>,
            IComplexSimulationState
        {
            /// <summary>
            /// Gets or sets a value indicating whether the solution converges.
            /// </summary>
            /// <value>
            /// If <c>true</c>, the solution for this subcircuit converges.
            /// </value>
            public bool IsConvergent { get; set; }

            /// <inheritdoc/>
            public Complex Laplace => Parent.Laplace;

            /// <summary>
            /// Initializes a new instance of the <see cref="LocalSimulationState"/> class.
            /// </summary>
            /// <param name="name">The name.</param>
            /// <param name="parent">The parent.</param>
            /// <param name="solver">The solver.</param>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/>, <paramref name="parent"/> or <paramref name="solver"/> is <c>null</c>.</exception>
            public LocalSimulationState(string name, IComplexSimulationState parent, ISparsePivotingSolver<Complex> solver)
                : base(name, parent, solver)
            {
            }
        }
    }
}
