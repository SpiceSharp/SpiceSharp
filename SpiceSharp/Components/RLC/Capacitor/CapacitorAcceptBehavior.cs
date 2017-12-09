using SpiceSharp.Behaviors;
using SpiceSharp.Circuits;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// Accept behavior for capacitances
    /// </summary>
    public class CapacitorAcceptBehavior : CircuitObjectBehaviorAccept
    {
        CapacitorLoadBehavior load = null;

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        public override void Setup(CircuitObject component, Circuit ckt)
        {
            base.Setup(component, ckt);

            load = component.GetBehavior(typeof(CircuitObjectBehaviorLoad)) as CapacitorLoadBehavior;
        }

        /// <summary>
        /// Accept the current timepoint
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Accept(Circuit ckt)
        {
            // Copy DC states when accepting the first timepoint
            if (ckt.State.Init == CircuitState.InitFlags.InitTransient)
                ckt.State.CopyDC(load.CAPstate + CapacitorLoadBehavior.CAPqcap);
        }
    }
}
