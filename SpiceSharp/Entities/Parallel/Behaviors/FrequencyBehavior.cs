using SpiceSharp.Behaviors;

namespace SpiceSharp.Entities.ParallelLoaderBehaviors
{
    /// <summary>
    /// Frequency behavior for a <see cref="ParallelEntity"/>.
    /// </summary>
    /// <seealso cref="ParallelBehavior{T}" />
    /// <seealso cref="IFrequencyBehavior" />
    public class FrequencyBehavior : ParallelBehavior<IFrequencyBehavior>, IFrequencyBehavior
    {
        private FrequencyParameters _fp;

        /// <summary>
        /// Initializes a new instance of the <see cref="FrequencyBehavior"/> class.
        /// </summary>
        /// <param name="name">The identifier of the behavior.</param>
        /// <remarks>
        /// The identifier of the behavior should be the same as that of the entity creating it.
        /// </remarks>
        public FrequencyBehavior(string name)
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
            context.Behaviors.Parameters.TryGetValue(out _fp);
        }

        /// <summary>
        /// Initializes the parameters.
        /// </summary>
        public void InitializeParameters()
        {
            if (_fp != null && _fp.ParallelInitialize)
                For(behavior => behavior.InitializeParameters);
            else
            {
                for (var t = 0; t < Behaviors.Length; t++)
                {
                    var b = Behaviors[t];
                    for (var i = 0; i < b.Count; i++)
                        b[i].InitializeParameters();
                }
            }
        }

        /// <summary>
        /// Load the Y-matrix and right-hand side vector for frequency domain analysis.
        /// </summary>
        public void Load()
        {
            if (_fp != null && _fp.ParallelLoad)
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
