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
        /// <param name="name">The identifier of the behavior.</param>
        /// <remarks>
        /// The identifier of the behavior should be the same as that of the entity creating it.
        /// </remarks>
        public InitialConditionBehavior(string name) : base(name)
        {
        }

        public override void Bind(BindingContext context)
        {
            base.Bind(context);
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
                        var bs = Behaviors[task];
                        for (var i = 0; i < bs.Count; i++)
                            bs[i].SetInitialCondition();
                    });
                }
                Task.WaitAll(tasks);
            }
            else
            {
                foreach (var bs in Behaviors)
                {
                    for (var i = 0; i < bs.Count; i++)
                        bs[i].SetInitialCondition();
                }
            }
        }
    }
}
