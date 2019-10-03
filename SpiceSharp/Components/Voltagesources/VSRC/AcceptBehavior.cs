using SpiceSharp.Behaviors;
using SpiceSharp.Entities;

namespace SpiceSharp.Components.VoltageSourceBehaviors
{
    /// <summary>
    /// Accept behavior for a <see cref="VoltageSource"/>
    /// </summary>
    public class AcceptBehavior : BiasingBehavior, IAcceptBehavior
    {
        /// <summary>
        /// Creates a new instance of the <see cref="AcceptBehavior"/> class.
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
            BaseParameters.Waveform?.Bind(context);
        }

        /// <summary>
        /// Called when a new timepoint is being tested.
        /// </summary>
        void IAcceptBehavior.Probe()
        {
            BaseParameters.Waveform?.Probe();
        }

        /// <summary>
        /// Accept the current timepoint
        /// </summary>
        void IAcceptBehavior.Accept()
        {
            BaseParameters.Waveform?.Accept();
        }
    }
}
