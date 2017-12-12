using SpiceSharp.Behaviors;
using SpiceSharp.Circuits;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// Truncate behavior for a <see cref="MOS3"/>
    /// </summary>
    public class MOS3TruncateBehavior : CircuitObjectBehaviorTruncate
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        private MOS3LoadBehavior load;

        /// <summary>
        /// Setup behavior
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        /// <returns></returns>
        public override bool Setup(CircuitObject component, Circuit ckt)
        {
            load = GetBehavior<MOS3LoadBehavior>(component);
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
            method.Terr(load.MOS3states + MOS3LoadBehavior.MOS3qgs, ckt, ref timestep);
            method.Terr(load.MOS3states + MOS3LoadBehavior.MOS3qgd, ckt, ref timestep);
            method.Terr(load.MOS3states + MOS3LoadBehavior.MOS3qgb, ckt, ref timestep);
        }
    }
}
