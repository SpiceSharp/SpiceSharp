using SpiceSharp.Behaviors;
using SpiceSharp.Circuits;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// Truncate behavior for a <see cref="MOS1"/>
    /// </summary>
    public class MOS1TruncateBehavior : CircuitObjectBehaviorTruncate
    {
        private int MOS1states;

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        /// <returns></returns>
        public override bool Setup(CircuitObject component, Circuit ckt)
        {
            var mos1 = component as MOS1;

            var load = mos1.GetBehavior(typeof(CircuitObjectBehaviorLoad)) as MOS1LoadBehavior;
            MOS1states = load.MOS1states;
            return true;
        }

        /// <summary>
        /// Truncate the timestep
        /// </summary>
        /// <param name="ckt">Circuit</param>
        /// <param name="timestep">Timestep</param>
        public override void Truncate(Circuit ckt, ref double timestep)
        {
            var method = ckt.Method;
            method.Terr(MOS1states + MOS1LoadBehavior.MOS1qgs, ckt, ref timestep);
            method.Terr(MOS1states + MOS1LoadBehavior.MOS1qgd, ckt, ref timestep);
            method.Terr(MOS1states + MOS1LoadBehavior.MOS1qgb, ckt, ref timestep);
        }
    }
}
