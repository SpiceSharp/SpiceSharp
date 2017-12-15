using SpiceSharp.Behaviors;
using SpiceSharp.Circuits;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// Truncate behavior for inductors
    /// </summary>
    public class InductorTruncateBehavior : CircuitObjectBehaviorTruncate
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        private InductorLoadBehavior load;

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        /// <returns></returns>
        public override void Setup(CircuitObject component, Circuit ckt)
        {
            // Get behaviors
            load = GetBehavior<InductorLoadBehavior>(component);
        }

        /// <summary>
        /// Truncate the timestep
        /// </summary>
        /// <param name="ckt">Circuit</param>
        /// <param name="timestep">Timestep</param>
        public override void Truncate(Circuit ckt, ref double timestep)
        {
            ckt.Method.Terr(load.INDstate + InductorLoadBehavior.INDflux, ckt, ref timestep);
        }
    }
}
