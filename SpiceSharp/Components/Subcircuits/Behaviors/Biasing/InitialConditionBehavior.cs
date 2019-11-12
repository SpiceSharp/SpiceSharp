using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using System.Threading.Tasks;

namespace SpiceSharp.Components.SubcircuitBehaviors
{
    /// <summary>
    /// Initial condition behavior for a <see cref="Subcircuit"/>.
    /// </summary>
    /// <seealso cref="SubcircuitBehavior{T}" />
    /// <seealso cref="IInitialConditionBehavior" />
    public class InitialConditionBehavior : SubcircuitBehavior<IInitialConditionBehavior>, IInitialConditionBehavior
    {
        private InitialConditionParameters _icp;

        /// <summary>
        /// Initializes a new instance of the <see cref="InitialConditionBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public InitialConditionBehavior(string name, SubcircuitBindingContext context) : base(name, context)
        {
            context.ThrowIfNull(nameof(context));
            context.Behaviors.Parameters.TryGetValue(out _icp);
        }

        /// <summary>
        /// Sets the initial conditions for the behavior.
        /// </summary>
        public void SetInitialCondition()
        {
            if (_icp != null && _icp.ParallelInitialCondition && Behaviors.Length > 1)
            {
                var tasks = new Task[Behaviors.Length];
                for (var t = 0; t < Behaviors.Length; t++)
                {
                    var task = t;
                    tasks[t] = Task.Run(() =>
                    {
                        foreach (var behavior in Behaviors[task])
                            behavior.SetInitialCondition();
                    });
                }
                Task.WaitAll(tasks);
            }
            else
            {
                foreach (var bs in Behaviors)
                {
                    foreach (var behavior in bs)
                        behavior.SetInitialCondition();
                }
            }
        }
    }
}
