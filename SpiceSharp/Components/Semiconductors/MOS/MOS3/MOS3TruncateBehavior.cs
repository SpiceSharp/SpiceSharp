using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// Truncate behavior for a <see cref="MOS3"/>
    /// </summary>
    public class MOS3TruncateBehavior : CircuitObjectBehaviorTruncate
    {
        /// <summary>
        /// Truncate the timestep
        /// </summary>
        /// <param name="ckt">Circuit</param>
        /// <param name="timestep">Timestep</param>
        public override void Truncate(Circuit ckt, ref double timestep)
        {
            var mos3 = ComponentTyped<MOS3>();
            var method = ckt.Method;
            method.Terr(mos3.MOS3states + MOS3.MOS3qgs, ckt, ref timestep);
            method.Terr(mos3.MOS3states + MOS3.MOS3qgd, ckt, ref timestep);
            method.Terr(mos3.MOS3states + MOS3.MOS3qgb, ckt, ref timestep);
        }
    }
}
