using SpiceSharp.Components;
using SpiceSharp.Circuits;

namespace SpiceSharp.Behaviors.MOS1
{
    /// <summary>
    /// Truncate behavior for a <see cref="MOS1"/>
    /// </summary>
    public class TruncateBehavior : Behaviors.TruncateBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        private LoadBehavior load;

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        /// <returns></returns>
        public override void Setup(CircuitObject component, Circuit ckt)
        {
            // Get behaviors
            load = GetBehavior<LoadBehavior>(component);
        }

        /// <summary>
        /// Truncate the timestep
        /// </summary>
        /// <param name="ckt">Circuit</param>
        /// <param name="timestep">Timestep</param>
        public override void Truncate(Circuit ckt, ref double timestep)
        {
            var method = ckt.Method;
            method.Terr(load.MOS1states + LoadBehavior.MOS1qgs, ckt, ref timestep);
            method.Terr(load.MOS1states + LoadBehavior.MOS1qgd, ckt, ref timestep);
            method.Terr(load.MOS1states + LoadBehavior.MOS1qgb, ckt, ref timestep);
        }
    }
}
