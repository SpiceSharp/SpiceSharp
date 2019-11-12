using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using System.Threading.Tasks;

namespace SpiceSharp.Components.SubcircuitBehaviors
{
    /// <summary>
    /// A temperature behavior for a <see cref="Subcircuit"/>.
    /// </summary>
    /// <seealso cref="Behavior" />
    /// <seealso cref="ITemperatureBehavior" />
    public class TemperatureBehavior : SubcircuitBehavior<ITemperatureBehavior>, ITemperatureBehavior
    {
        private TemperatureParameters _tp;

        /// <summary>
        /// Initializes a new instance of the <see cref="TemperatureBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public TemperatureBehavior(string name, SubcircuitBindingContext context) : base(name, context)
        {
            context.ThrowIfNull(nameof(context));
            context.Behaviors.Parameters.TryGetValue(out _tp);
        }

        /// <summary>
        /// Perform temperature-dependent calculations.
        /// </summary>
        public void Temperature()
        {
            if (_tp != null && _tp.ParallelTemperature && Behaviors.Length > 1)
            {
                var tasks = new Task[Behaviors.Length];
                for (var t = 0; t < Behaviors.Length; t++)
                {
                    var task = t;
                    tasks[t] = Task.Run(() =>
                    {
                        foreach (var behavior in Behaviors[task])
                            behavior.Temperature();
                    });
                }
                Task.WaitAll(tasks);
            }
            else
            {
                foreach (var bs in Behaviors)
                {
                    foreach (var behavior in bs)
                        behavior.Temperature();
                }
            }
        }
    }
}
