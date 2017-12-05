using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// Truncate behavior for a <see cref="Diode"/>
    /// </summary>
    public class DiodeTruncateBehavior : CircuitObjectBehaviorTruncate
    {
        /// <summary>
        /// Truncate the timestep
        /// </summary>
        /// <param name="ckt">Circuit</param>
        /// <param name="timestep">Timestep</param>
        public override void Truncate(Circuit ckt, ref double timestep)
        {
            var diode = ComponentTyped<Diode>();
            ckt.Method.Terr(diode.DIOstate + Diode.DIOcapCharge, ckt, ref timestep);
        }
    }
}
