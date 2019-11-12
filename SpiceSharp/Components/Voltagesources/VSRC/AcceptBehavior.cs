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
        /// Initializes a new instance of the <see cref="AcceptBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public AcceptBehavior(string name, ComponentBindingContext context) : base(name, context)
        {
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
