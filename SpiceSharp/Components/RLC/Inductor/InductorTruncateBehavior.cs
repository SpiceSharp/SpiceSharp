using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// Truncate behavior for inductors
    /// </summary>
    public class InductorTruncateBehavior : CircuitObjectBehaviorTruncate
    {
        /// <summary>
        /// Truncate the timestep
        /// </summary>
        /// <param name="ckt">Circuit</param>
        /// <param name="timestep">Timestep</param>
        public override void Truncate(Circuit ckt, ref double timestep)
        {
            var ind = ComponentTyped<Inductor>();
            ckt.Method.Terr(ind.INDstate + Inductor.INDflux, ckt, ref timestep);
        }
    }
}
