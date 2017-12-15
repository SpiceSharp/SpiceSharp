using SpiceSharp.Circuits;

namespace SpiceSharp.Behaviors.MOS2
{
    /// <summary>
    /// Truncate behavior for a <see cref="Components.MOS2"/>
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
            method.Terr(load.MOS2states + LoadBehavior.MOS2qgs, ckt, ref timestep);
            method.Terr(load.MOS2states + LoadBehavior.MOS2qgd, ckt, ref timestep);
            method.Terr(load.MOS2states + LoadBehavior.MOS2qgb, ckt, ref timestep);
        }
    }
}
