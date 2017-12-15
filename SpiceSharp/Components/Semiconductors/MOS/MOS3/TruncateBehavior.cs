using SpiceSharp.Components;
using SpiceSharp.Circuits;

namespace SpiceSharp.Behaviors.MOS3
{
    /// <summary>
    /// Truncate behavior for a <see cref="MOS3"/>
    /// </summary>
    public class TruncateBehavior : CircuitObjectBehaviorTruncate
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        private LoadBehavior load;

        /// <summary>
        /// Setup behavior
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        /// <returns></returns>
        public override void Setup(CircuitObject component, Circuit ckt)
        {
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
            method.Terr(load.MOS3states + LoadBehavior.MOS3qgs, ckt, ref timestep);
            method.Terr(load.MOS3states + LoadBehavior.MOS3qgd, ckt, ref timestep);
            method.Terr(load.MOS3states + LoadBehavior.MOS3qgb, ckt, ref timestep);
        }
    }
}
