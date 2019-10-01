using SpiceSharp.Behaviors;
using System.Threading.Tasks;

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
            // Parallel.For(0, Behaviors.Count, (i) => Behaviors[i].Load());
            For(behavior => behavior.Load);
        }
    }
}
