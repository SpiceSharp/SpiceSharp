using SpiceSharp.Behaviors;
using SpiceSharp.Circuits;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// Truncate behavior for a <see cref="MOS2"/>
    /// </summary>
    public class MOS2TruncateBehavior : CircuitObjectBehaviorTruncate
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        private MOS2LoadBehavior load;

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        /// <returns></returns>
        public override void Setup(CircuitObject component, Circuit ckt)
        {
            load = GetBehavior<MOS2LoadBehavior>(component);
        }

        /// <summary>
        /// Truncate the timestep
        /// </summary>
        /// <param name="ckt">Circuit</param>
        /// <param name="timestep">Timestep</param>
        public override void Truncate(Circuit ckt, ref double timestep)
        {
            var method = ckt.Method;
            method.Terr(load.MOS2states + MOS2LoadBehavior.MOS2qgs, ckt, ref timestep);
            method.Terr(load.MOS2states + MOS2LoadBehavior.MOS2qgd, ckt, ref timestep);
            method.Terr(load.MOS2states + MOS2LoadBehavior.MOS2qgb, ckt, ref timestep);
        }
    }
}
