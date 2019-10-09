using SpiceSharp.Behaviors;

namespace SpiceSharp.Entities.ParallelLoaderBehaviors
{
    /// <summary>
    /// A time behavior for a <see cref="ParallelLoader"/>
    /// </summary>
    /// <seealso cref="ParallelBehavior{T}" />
    /// <seealso cref="ITimeBehavior" />
    public class TimeBehavior : ParallelBehavior<ITimeBehavior>, ITimeBehavior
    {
        private TimeParameters _tp;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeBehavior"/> class.
        /// </summary>
        /// <param name="name">The identifier of the behavior.</param>
        /// <remarks>
        /// The identifier of the behavior should be the same as that of the entity creating it.
        /// </remarks>
        public TimeBehavior(string name)
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
            context.Behaviors.Parameters.TryGetValue(out _tp);
        }

        /// <summary>
        /// Initialize the state values from the current DC solution.
        /// </summary>
        public void InitializeStates()
        {
            if (_tp != null && _tp.ParallelInitialize)
                For(behavior => behavior.InitializeStates);
            else
            {
                for (var t = 0; t < Behaviors.Length; t++)
                {
                    var b = Behaviors[t];
                    for (var i = 0; i < b.Count; i++)
                        b[i].InitializeStates();
                }
            }
        }

        /// <summary>
        /// Load the Y-matrix and Rhs-vector.
        /// </summary>
        public void Load()
        {
            if (_tp != null && _tp.ParallelLoad)
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
