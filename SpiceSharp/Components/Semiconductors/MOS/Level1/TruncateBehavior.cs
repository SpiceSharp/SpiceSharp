using SpiceSharp.Simulations;
using SpiceSharp.Circuits;

namespace SpiceSharp.Behaviors.MOS1
{
    /// <summary>
    /// Truncate behavior for a <see cref="Components.MOS1"/>
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
            // Get behaviors
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
            method.Terr(load.MOS1states + LoadBehavior.MOS1qgs, sim, ref timestep);
            method.Terr(load.MOS1states + LoadBehavior.MOS1qgd, sim, ref timestep);
            method.Terr(load.MOS1states + LoadBehavior.MOS1qgb, sim, ref timestep);
        }
    }
}
