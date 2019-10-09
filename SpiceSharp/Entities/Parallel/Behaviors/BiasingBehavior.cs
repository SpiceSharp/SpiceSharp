using SpiceSharp.Behaviors;

namespace SpiceSharp.Entities.ParallelLoaderBehaviors
{
    /// <summary>
    /// Biasing behavior for a <see cref="ParallelEntity"/>.
    /// </summary>
    /// <seealso cref="ParallelBehavior{T}" />
    /// <seealso cref="IBiasingBehavior" />
    public class BiasingBehavior : ParallelBehavior<IBiasingBehavior>, IBiasingBehavior
    {
        private BiasingParameters _bp;

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
            base.Bind(context);
            context.Behaviors.Parameters.TryGetValue(out _bp);
        }

        /// <summary>
        /// Tests convergence at the device-level.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if the device determines the solution converges; otherwise, <c>false</c>.
        /// </returns>
        public bool IsConvergent()
        {
            if (_bp != null && _bp.ParallelConvergences)
                return ForAnd(behavior => behavior.IsConvergent);
            else
            {
                var result = true;
                for (var t = 0; t < Behaviors.Length; t++)
                {
                    var b = Behaviors[t];
                    for (var i = 0; i < b.Count; i++)
                        result &= b[i].IsConvergent();
                }
                return result;
            }
        }

        /// <summary>
        /// Loads the Y-matrix and Rhs-vector.
        /// </summary>
        public void Load()
        {
            if (_bp != null && _bp.ParallelLoad)
                For(behavior => behavior.Load);
            else
            {
                for (var t = 0; t < Behaviors.Length; t++)
                {
                    var b = Behaviors[t];
                    for (var i = 0; i < b.Count; i++)
                        b[i].Load();
                }
            }
        }
    }
}
