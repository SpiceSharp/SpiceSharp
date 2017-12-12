using SpiceSharp.Behaviors;
using SpiceSharp.Circuits;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// Truncate behavior for a <see cref="BJT"/>
    /// </summary>
    public class BJTTruncateBehavior : CircuitObjectBehaviorTruncate
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        private BJTLoadBehavior load;

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        /// <returns></returns>
        public override bool Setup(CircuitObject component, Circuit ckt)
        {
            load = GetBehavior<BJTLoadBehavior>(component);
            return true;
        }

        /// <summary>
        /// Truncate the timestep
        /// </summary>
        /// <param name="ckt">Circuit</param>
        /// <param name="timestep">Timestep</param>
        public override void Truncate(Circuit ckt, ref double timestep)
        {
            var method = ckt.Method;
            method.Terr(load.BJTstate + BJTLoadBehavior.BJTqbe, ckt, ref timestep);
            method.Terr(load.BJTstate + BJTLoadBehavior.BJTqbc, ckt, ref timestep);
            method.Terr(load.BJTstate + BJTLoadBehavior.BJTqcs, ckt, ref timestep);
        }
    }
}
