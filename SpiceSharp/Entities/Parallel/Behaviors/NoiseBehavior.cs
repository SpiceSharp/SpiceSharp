using SpiceSharp.Behaviors;

namespace SpiceSharp.Entities.ParallelLoaderBehaviors
{
    /// <summary>
    /// A noise behavior for a <see cref="ParallelEntity"/>.
    /// </summary>
    /// <seealso cref="ParallelBehavior{T}" />
    /// <seealso cref="INoiseBehavior" />
    public class NoiseBehavior : ParallelBehavior<INoiseBehavior>, INoiseBehavior
    {
        private NoiseParameters _np;

        /// <summary>
        /// Initializes a new instance of the <see cref="NoiseBehavior"/> class.
        /// </summary>
        /// <param name="name">The identifier of the behavior.</param>
        /// <remarks>
        /// The identifier of the behavior should be the same as that of the entity creating it.
        /// </remarks>
        public NoiseBehavior(string name)
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
            context.Behaviors.Parameters.TryGetValue(out _np);
        }

        /// <summary>
        /// Calculate the noise contributions.
        /// </summary>
        public void Noise()
        {
            if (_np != null && _np.ParallelNoise)
                For(behavior => behavior.Noise);
            else
            {
                for (var t = 0; t < Behaviors.Length; t++)
                {
                    var b = Behaviors[t];
                    for (var i = 0; i < b.Count; i++)
                        b[i].Noise();
                }
            }
        }
    }
}
