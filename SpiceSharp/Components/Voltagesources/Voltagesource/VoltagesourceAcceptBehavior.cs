using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// Accept behavior for voltage sources
    /// </summary>
    public class VoltagesourceAcceptBehavior : CircuitObjectBehaviorAccept
    {
        /// <summary>
        /// Accept the current timepoint
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Accept(Circuit ckt)
        {
            var src = ComponentTyped<Voltagesource>();
            src.VSRCwaveform?.Accept(ckt);
        }
    }
}
