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
        /// Private variables
        /// </summary>
        private int BJTstate;

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        /// <returns></returns>
        public override bool Setup(CircuitObject component, Circuit ckt)
        {
            var bjt = component as BJT;
            var load = bjt.GetBehavior(typeof(CircuitObjectBehaviorLoad)) as BJTLoadBehavior;
            BJTstate = load.BJTstate;
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
            method.Terr(BJTstate + BJTLoadBehavior.BJTqbe, ckt, ref timestep);
            method.Terr(BJTstate + BJTLoadBehavior.BJTqbc, ckt, ref timestep);
            method.Terr(BJTstate + BJTLoadBehavior.BJTqcs, ckt, ref timestep);
        }
    }
}
