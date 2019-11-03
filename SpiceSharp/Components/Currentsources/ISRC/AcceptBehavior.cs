using SpiceSharp.Behaviors;
using SpiceSharp.Entities;

namespace SpiceSharp.Components.CurrentSourceBehaviors
{
    /// <summary>
    /// Accept behavior for a <see cref="CurrentSource"/>
    /// </summary>
    public class AcceptBehavior : Behavior, IAcceptBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        private CommonBehaviors.IndependentSourceParameters _bp;

        /// <summary>
        /// Initializes a new instance of the <see cref="AcceptBehavior"/> class.
        /// </summary>
        /// <param name="name">Name</param>
        public AcceptBehavior(string name) : base(name) { }

        /// <summary>
        /// Bind the behavior to a simulation.
        /// </summary>
        /// <param name="context">The binding context.</param>
        public override void Bind(BindingContext context)
        {
            base.Bind(context);
            _bp = context.Behaviors.Parameters.GetValue<CommonBehaviors.IndependentSourceParameters>();
            _bp.Waveform?.Bind(context);
        }

        /// <summary>
        /// Called when a new timepoint is being tested.
        /// </summary>
        void IAcceptBehavior.Probe()
        {
            _bp.Waveform?.Probe();
        }

        /// <summary>
        /// Accept the current timepoint
        /// </summary>
        void IAcceptBehavior.Accept()
        {
            _bp.Waveform?.Accept();
        }
    }
}
