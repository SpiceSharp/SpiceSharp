using SpiceSharp.Simulations;
using SpiceSharp.Circuits;

namespace SpiceSharp.Behaviors.CAP
{
    /// <summary>
    /// Truncate behavior for a <see cref="Components.Capacitor"/>
    /// </summary>
    public class TruncateBehavior : Behaviors.TruncateBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        private TransientBehavior tran;

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        public override void Setup(Entity component, Circuit ckt)
        {
            // Get behaviors
            tran = GetBehavior<TransientBehavior>(component);
        }

        /// <summary>
        /// Truncate the timestep
        /// </summary>
        /// <param name="sim">Simulation</param>
        /// <param name="timestep">Timestep</param>
        public override void Truncate(TimeSimulation sim, ref double timestep)
        {
            sim.Circuit.Method.Terr(tran.CAPstate + TransientBehavior.CAPqcap, sim, ref timestep);
        }
    }
}
