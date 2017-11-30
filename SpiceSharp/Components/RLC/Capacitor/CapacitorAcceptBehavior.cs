using SpiceSharp.Behaviors;
using SpiceSharp.Circuits;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// Accept behavior for capacitances
    /// </summary>
    public class CapacitorAcceptBehavior : CircuitObjectBehaviorAccept
    {
        /// <summary>
        /// Accept the current timepoint
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Accept(Circuit ckt)
        {
            var cap = ComponentTyped<Capacitor>();

            // Copy DC states when accepting the first timepoint
            if (ckt.State.Init == CircuitState.InitFlags.InitTransient)
                ckt.State.CopyDC(cap.CAPstate + Capacitor.CAPqcap);
        }
    }
}
