using SpiceSharp.Behaviors;

namespace SpiceSharp.Entities.ParallelLoaderBehaviors
{
    /// <summary>
    /// Accept behavior for a <see cref="ParallelLoader"/>
    /// </summary>
    /// <seealso cref="ParallelBehavior{T}" />
    /// <seealso cref="IAcceptBehavior" />
    public class AcceptBehavior : ParallelBehavior<IAcceptBehavior>, IAcceptBehavior
    {
        private AcceptParameters _ap;

        /// <summary>
        /// Initializes a new instance of the <see cref="AcceptBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public AcceptBehavior(string name)
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
            context.Behaviors.Parameters.TryGetValue(out _ap);
        }

        /// <summary>
        /// Accepts the current timepoint.
        /// </summary>
        public void Accept()
        {
            if (_ap != null && _ap.ParallelAccept)
                For(behavior => behavior.Accept);
            else
            {
                for (var t = 0; t < Behaviors.Length; t++)
                {
                    var b = Behaviors[t];
                    for (var i = 0; i < b.Count; i++)
                        b[i].Accept();
                }
            }
        }

        /// <summary>
        /// Called when a new timepoint is being tested.
        /// </summary>
        public void Probe()
        {
            if (_ap != null && _ap.ParallelProbe)
                For(behavior => behavior.Probe);
            else
            {
                for (var t = 0; t < Behaviors.Length; t++)
                {
                    var b = Behaviors[t];
                    for (var i = 0; i < b.Count; i++)
                        b[i].Probe();
                }
            }
        }
    }
}
