using SpiceSharp.Behaviors;

namespace SpiceSharp.Entities.ParallelLoaderBehaviors
{
    /// <summary>
    /// Temperature behavior for a <see cref="ParallelEntity"/>.
    /// </summary>
    /// <seealso cref="ParallelBehavior{T}" />
    /// <seealso cref="ITemperatureBehavior" />
    public class TemperatureBehavior : ParallelBehavior<ITemperatureBehavior>, ITemperatureBehavior
    {
        private TemperatureParameters _tp;

        /// <summary>
        /// Initializes a new instance of the <see cref="TemperatureBehavior"/> class.
        /// </summary>
        /// <param name="name">The identifier of the behavior.</param>
        /// <remarks>
        /// The identifier of the behavior should be the same as that of the entity creating it.
        /// </remarks>
        public TemperatureBehavior(string name)
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
        /// Perform temperature-dependent calculations.
        /// </summary>
        public void Temperature()
        {
            if (_tp != null && _tp.ParallelTemperature)
                For(behavior => behavior.Temperature);
            else
            {
                for (var t = 0; t < Behaviors.Length; t++)
                {
                    var b = Behaviors[t];
                    for (var i = 0; i < b.Count; i++)
                        b[i].Temperature();
                }
            }
        }
    }
}
