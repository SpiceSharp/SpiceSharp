using SpiceSharp.Behaviors;
using SpiceSharp.Circuits;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// Truncate behavior for capacitors
    /// </summary>
    public class CapacitorTruncateBehavior : CircuitObjectBehaviorTruncate
    {
        /// <summary>
        /// Private variables
        /// </summary>
        private CapacitorLoadBehavior load;

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        public override void Setup(CircuitObject component, Circuit ckt)
        {
            load = component.GetBehavior(typeof(CircuitObjectBehaviorLoad)) as CapacitorLoadBehavior;
        }

        /// <summary>
        /// Truncate the timestep
        /// </summary>
        /// <param name="ckt">Circuit</param>
        /// <param name="timestep">Timestep</param>
        public override void Truncate(Circuit ckt, ref double timestep)
        {
            ckt.Method.Terr(load.CAPstate + CapacitorLoadBehavior.CAPqcap, ckt, ref timestep);
        }
    }
}
