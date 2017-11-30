using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// Truncate behavior for capacitors
    /// </summary>
    public class CapacitorTruncateBehavior : CircuitObjectBehaviorTruncate
    {
        /// <summary>
        /// Truncate the timestep
        /// </summary>
        /// <param name="ckt">Circuit</param>
        /// <param name="timestep">Timestep</param>
        public override void Truncate(Circuit ckt, ref double timestep)
        {
            var cap = ComponentTyped<Capacitor>();
            ckt.Method.Terr(cap.CAPstate + Capacitor.CAPqcap, ckt, ref timestep);
        }
    }
}
