using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// Truncate behavior for a <see cref="MOS2"/>
    /// </summary>
    public class MOS2TruncateBehavior : CircuitObjectBehaviorTruncate
    {
        /// <summary>
        /// Truncate the timestep
        /// </summary>
        /// <param name="ckt">Circuit</param>
        /// <param name="timestep">Timestep</param>
        public override void Truncate(Circuit ckt, ref double timestep)
        {
            var mos2 = ComponentTyped<MOS2>();
            var method = ckt.Method;
            method.Terr(mos2.MOS2states + MOS2.MOS2qgs, ckt, ref timestep);
            method.Terr(mos2.MOS2states + MOS2.MOS2qgd, ckt, ref timestep);
            method.Terr(mos2.MOS2states + MOS2.MOS2qgb, ckt, ref timestep);
        }
    }
}
