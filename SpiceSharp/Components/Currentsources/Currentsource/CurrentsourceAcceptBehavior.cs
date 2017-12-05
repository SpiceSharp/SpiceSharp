using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// Accept behavior for current sources
    /// </summary>
    public class CurrentsourceAcceptBehavior : CircuitObjectBehaviorAccept
    {
        /// <summary>
        /// Accept the current timepoint
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Accept(Circuit ckt)
        {
            var src = ComponentTyped<Currentsource>();
            src.ISRCwaveform?.Accept(ckt);
        }
    }
}
