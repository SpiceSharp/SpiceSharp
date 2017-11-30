using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// Truncate behavior for a <see cref="BJT"/>
    /// </summary>
    public class BJTTruncateBehavior : CircuitObjectBehaviorTruncate
    {
        /// <summary>
        /// Truncate the timestep
        /// </summary>
        /// <param name="ckt">Circuit</param>
        /// <param name="timestep">Timestep</param>
        public override void Truncate(Circuit ckt, ref double timestep)
        {
            var bjt = ComponentTyped<BJT>();
            var method = ckt.Method;
            method.Terr(bjt.BJTstate + BJT.BJTqbe, ckt, ref timestep);
            method.Terr(bjt.BJTstate + BJT.BJTqbc, ckt, ref timestep);
            method.Terr(bjt.BJTstate + BJT.BJTqcs, ckt, ref timestep);
        }
    }
}
