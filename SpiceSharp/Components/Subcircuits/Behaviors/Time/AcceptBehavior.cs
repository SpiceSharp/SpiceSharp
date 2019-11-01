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
        /// <param name="name">The identifier of the behavior.</param>
        /// <remarks>
        /// The identifier of the behavior should be the same as that of the entity creating it.
        /// </remarks>
        public AcceptBehavior(string name) : base(name)
        {
        }

        /// <summary>
        /// Bind the behavior to a simulation.
        /// </summary>
        /// <param name="context">The binding context.</param>
        public override void Bind(BindingContext context)
        {
            base.Bind(context);
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
                        var bs = Behaviors[task];
                        for (var i = 0; i < bs.Count; i++)
                            bs[i].Accept();
                    });
                }
                Task.WaitAll(tasks);
            }
            else
            {
                foreach (var bs in Behaviors)
                {
                    for (var i = 0; i < bs.Count; i++)
                        bs[i].Accept();
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
                        var bs = Behaviors[task];
                        for (var i = 0; i < bs.Count; i++)
                            bs[i].Probe();
                    });
                }
                Task.WaitAll(tasks);
            }
            else
            {
                foreach (var bs in Behaviors)
                {
                    for (var i = 0; i < bs.Count; i++)
                        bs[i].Probe();
                }
            }
        }
    }
}
