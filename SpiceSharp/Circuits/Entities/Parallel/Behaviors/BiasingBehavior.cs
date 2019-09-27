using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using System;
using System.Diagnostics;

namespace SpiceSharp.Circuits.ParallelBehaviors
{
    /// <summary>
    /// Biasing behavior for a <see cref="ParallelLoader"/>.
    /// </summary>
    /// <seealso cref="ParallelBehavior{T}" />
    /// <seealso cref="IBiasingBehavior" />
    public class BiasingBehavior : ParallelBehavior<IBiasingBehavior>, IBiasingBehavior
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BiasingBehavior"/> class.
        /// </summary>
        /// <param name="name">The identifier of the behavior.</param>
        /// <remarks>
        /// The identifier of the behavior should be the same as that of the entity creating it.
        /// </remarks>
        public BiasingBehavior(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Bind the behavior to a simulation.
        /// </summary>
        /// <param name="context">The binding context.</param>
        public override void Bind(BindingContext context)
        {
            // We want to replace the solver of the state to avoid write conflicts
            var state = context.States.GetValue<BiasingSimulationState>();
            var oldSolver = state.Solver;
            base.Bind(context);
        }

        /// <summary>
        /// Tests convergence at the device-level.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if the device determines the solution converges; otherwise, <c>false</c>.
        /// </returns>
        public bool IsConvergent()
        {
            return ForAnd(behavior => behavior.IsConvergent);
        }

        /// <summary>
        /// Loads the Y-matrix and Rhs-vector.
        /// </summary>
        public void Load()
        {
            For(behavior => behavior.Load);
        }
    }
}
