using SpiceSharp.Circuits;
using SpiceSharp.Simulations;

namespace SpiceSharp.Behaviors.BJT
{
    /// <summary>
    /// Truncate behavior for a <see cref="BJT"/>
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
        /// <param name="sim">Simulations</param>
        /// <param name="timestep">Timestep</param>
        public override void Truncate(TimeSimulation sim, ref double timestep)
        {
            var method = sim.Circuit.Method;
            method.Terr(load.BJTstate + LoadBehavior.BJTqbe, sim, ref timestep);
            method.Terr(load.BJTstate + LoadBehavior.BJTqbc, sim, ref timestep);
            method.Terr(load.BJTstate + LoadBehavior.BJTqcs, sim, ref timestep);
        }
    }
}
