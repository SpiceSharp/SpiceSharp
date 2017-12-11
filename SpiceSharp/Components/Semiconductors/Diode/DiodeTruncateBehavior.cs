using SpiceSharp.Behaviors;
using SpiceSharp.Circuits;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// Truncate behavior for a <see cref="Diode"/>
    /// </summary>
    public class DiodeTruncateBehavior : CircuitObjectBehaviorTruncate
    {
        /// <summary>
        /// Private variables
        /// </summary>
        private int DIOstate;

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        /// <returns></returns>
        public override bool Setup(CircuitObject component, Circuit ckt)
        {
            var diode = component as Diode;

            // Get behaviors
            var load = diode.GetBehavior(typeof(CircuitObjectBehaviorLoad)) as DiodeLoadBehavior;

            DIOstate = load.DIOstate;
            return true;
        }

        /// <summary>
        /// Truncate the timestep
        /// </summary>
        /// <param name="ckt">Circuit</param>
        /// <param name="timestep">Timestep</param>
        public override void Truncate(Circuit ckt, ref double timestep)
        {
            ckt.Method.Terr(DIOstate + DiodeLoadBehavior.DIOcapCharge, ckt, ref timestep);
        }
    }
}
