using SpiceSharp.Circuits;
using SpiceSharp.Simulations;

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
        public override void Setup(Entity component, Circuit ckt)
        {
            load = GetBehavior<LoadBehavior>(component);
        }

        /// <summary>
        /// Truncate the timestep
        /// </summary>
        /// <param name="sim">Simulation</param>
        /// <param name="timestep">Timestep</param>
        public override void Truncate(TimeSimulation sim, ref double timestep)
        {
            var method = sim.Circuit.Method;
            method.Terr(load.MOS2states + LoadBehavior.MOS2qgs, sim, ref timestep);
            method.Terr(load.MOS2states + LoadBehavior.MOS2qgd, sim, ref timestep);
            method.Terr(load.MOS2states + LoadBehavior.MOS2qgb, sim, ref timestep);
        }
    }
}
