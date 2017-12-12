using SpiceSharp.Behaviors;
using SpiceSharp.Circuits;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// Truncate behavior for a <see cref="MOS1"/>
    /// </summary>
    public class MOS1TruncateBehavior : CircuitObjectBehaviorTruncate
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        private MOS1LoadBehavior load;

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        /// <returns></returns>
        public override bool Setup(CircuitObject component, Circuit ckt)
        {
            load = GetBehavior<MOS1LoadBehavior>(component);
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
            method.Terr(load.MOS1states + MOS1LoadBehavior.MOS1qgs, ckt, ref timestep);
            method.Terr(load.MOS1states + MOS1LoadBehavior.MOS1qgd, ckt, ref timestep);
            method.Terr(load.MOS1states + MOS1LoadBehavior.MOS1qgb, ckt, ref timestep);
        }
    }
}
