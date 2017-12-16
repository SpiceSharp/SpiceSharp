using SpiceSharp.Circuits;
using SpiceSharp.Simulations;

namespace SpiceSharp.Behaviors.MOS3
{
    /// <summary>
    /// Truncate behavior for a <see cref="Components.MOS3"/>
    /// </summary>
    public class TruncateBehavior : Behaviors.TruncateBehavior
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
            method.Terr(load.MOS3states + LoadBehavior.MOS3qgs, sim, ref timestep);
            method.Terr(load.MOS3states + LoadBehavior.MOS3qgd, sim, ref timestep);
            method.Terr(load.MOS3states + LoadBehavior.MOS3qgb, sim, ref timestep);
        }
    }
}
