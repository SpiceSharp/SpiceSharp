using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using System.Threading.Tasks;

namespace SpiceSharp.Components.SubcircuitBehaviors
{
    /// <summary>
    /// Accept behavior for a <see cref="Subcircuit"/>.
    /// </summary>
    /// <seealso cref="SubcircuitBehavior{T}" />
    /// <seealso cref="IAcceptBehavior" />
    public class AcceptBehavior : SubcircuitBehavior<IAcceptBehavior>, IAcceptBehavior
    {
        private AcceptParameters _ap;

        /// <summary>
        /// Initializes a new instance of the <see cref="AcceptBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public AcceptBehavior(string name, SubcircuitBindingContext context) : base(name, context)
        {
            context.ThrowIfNull(nameof(context));
            context.Behaviors.Parameters.TryGetValue(out _ap);
        }

        /// <summary>
        /// Accepts the current timepoint.
        /// </summary>
        public void Accept()
        {
            if (_ap != null && _ap.ParallelAccept && Behaviors.Length > 1)
            {
                var tasks = new Task[Behaviors.Length];
                for (var t = 0; t < Behaviors.Length; t++)
                {
                    var task = t;
                    tasks[t] = Task.Run(() =>
                    {
                        foreach (var behavior in Behaviors[task])
                            behavior.Accept();
                    });
                }
                Task.WaitAll(tasks);
            }
            else
            {
                foreach (var bs in Behaviors)
                {
                    foreach (var behavior in bs)
                        behavior.Accept();
                }
            }
        }

        /// <summary>
        /// Called when a new timepoint is being tested.
        /// </summary>
        public void Probe()
        {
            if (_ap != null && _ap.ParallelProbe && Behaviors.Length > 1)
            {
                var tasks = new Task[Behaviors.Length];
                for (var t = 0; t < Behaviors.Length; t++)
                {
                    var task = t;
                    tasks[t] = Task.Run(() =>
                    {
                        foreach (var behavior in Behaviors[task])
                            behavior.Probe();
                    });
                }
                Task.WaitAll(tasks);
            }
            else
            {
                foreach (var bs in Behaviors)
                {
                    foreach (var behavior in bs)
                        behavior.Probe();
                }
            }
        }
    }
}
