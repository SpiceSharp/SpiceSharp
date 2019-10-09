using SpiceSharp.Behaviors;

namespace SpiceSharp.Entities.ParallelLoaderBehaviors
{
    /// <summary>
    /// Initial condition behavior for a <see cref="ParallelEntity"/>.
    /// </summary>
    /// <seealso cref="ParallelBehavior{IInitialConditionBehavior}" />
    /// <seealso cref="IInitialConditionBehavior" />
    public class InitialConditionBehavior : ParallelBehavior<IInitialConditionBehavior>, IInitialConditionBehavior
    {
        private InitialConditionParameters _ip;

        /// <summary>
        /// Initializes a new instance of the <see cref="InitialConditionBehavior"/> class.
        /// </summary>
        /// <param name="name">The identifier of the behavior.</param>
        /// <remarks>
        /// The identifier of the behavior should be the same as that of the entity creating it.
        /// </remarks>
        public InitialConditionBehavior(string name)
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
            context.Behaviors.Parameters.TryGetValue(out _ip);
        }

        /// <summary>
        /// Sets the initial conditions for the behavior.
        /// </summary>
        public void SetInitialCondition()
        {
            if (_ip != null && _ip.ParallelInitialCondition)
                For(behavior => behavior.SetInitialCondition);
            else
            {
                for (var t = 0; t < Behaviors.Length; t++)
                {
                    var b = Behaviors[t];
                    for (var i = 0; i < b.Count; i++)
                        b[i].SetInitialCondition();
                }
            }
        }
    }
}
