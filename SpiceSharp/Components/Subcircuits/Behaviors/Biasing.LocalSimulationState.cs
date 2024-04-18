using SpiceSharp.Algebra;
using SpiceSharp.Simulations;
using System;
using System.Collections.Generic;

namespace SpiceSharp.Components.Subcircuits
{
    public partial class Biasing
    {
        /// <summary>
        /// An <see cref="IBiasingSimulationState" /> that can be used with a local solver and solution.
        /// </summary>
        /// <seealso cref="LocalSolverState{T, S}" />
        /// <seealso cref="IBiasingSimulationState" />
        protected class LocalSimulationState : LocalSolverState<double, IBiasingSimulationState>,
            IBiasingSimulationState
        {
            /// <inheritdoc/>
            public IVector<double> OldSolution { get; private set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="LocalSimulationState"/> class.
            /// </summary>
            /// <param name="name">The name of the subcircuit instance.</param>
            /// <param name="parent">The parent simulation state.</param>
            /// <param name="solver">The solver.</param>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/>, <paramref name="parent"/> or <paramref name="solver"/> is <c>null</c>.</exception>
            public LocalSimulationState(string name, IBiasingSimulationState parent, ISparsePivotingSolver<double> solver)
                : base(name, parent, solver)
            {
            }

            /// <inheritdoc/>
            public override void Initialize(IReadOnlyList<Bridge<string>> nodes)
            {
                base.Initialize(nodes);
                OldSolution = new DenseVector<double>(Solver.Size);
            }

            /// <inheritdoc/>
            public override void Update()
            {
                if (Updated)
                    return;

                // We need to keep track of the old solution
                (LocalSolution, OldSolution) = (OldSolution, LocalSolution);

                // Update the current solution
                base.Update();
            }
        }
    }
}
