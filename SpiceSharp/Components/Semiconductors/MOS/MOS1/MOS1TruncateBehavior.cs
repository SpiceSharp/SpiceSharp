using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// Truncate behavior for a <see cref="MOS1"/>
    /// </summary>
    public class MOS1TruncateBehavior : CircuitObjectBehaviorTruncate
    {
        /// <summary>
        /// Truncate the timestep
        /// </summary>
        /// <param name="ckt">Circuit</param>
        /// <param name="timestep">Timestep</param>
        public override void Truncate(Circuit ckt, ref double timestep)
        {
            var mos1 = ComponentTyped<MOS1>();
            var method = ckt.Method;
            method.Terr(mos1.MOS1states + MOS1.MOS1qgs, ckt, ref timestep);
            method.Terr(mos1.MOS1states + MOS1.MOS1qgd, ckt, ref timestep);
            method.Terr(mos1.MOS1states + MOS1.MOS1qgb, ckt, ref timestep);
        }
    }
}
