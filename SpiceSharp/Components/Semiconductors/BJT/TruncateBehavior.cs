using SpiceSharp.Circuits;

namespace SpiceSharp.Behaviors.BJT
{
    /// <summary>
    /// Truncate behavior for a <see cref="BJT"/>
    /// </summary>
    public class TruncateBehavior : CircuitObjectBehaviorTruncate
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
        public override void Setup(CircuitObject component, Circuit ckt)
        {
            // Get behaviors
            load = GetBehavior<LoadBehavior>(component);
        }

        /// <summary>
        /// Truncate the timestep
        /// </summary>
        /// <param name="ckt">Circuit</param>
        /// <param name="timestep">Timestep</param>
        public override void Truncate(Circuit ckt, ref double timestep)
        {
            var method = ckt.Method;
            method.Terr(load.BJTstate + LoadBehavior.BJTqbe, ckt, ref timestep);
            method.Terr(load.BJTstate + LoadBehavior.BJTqbc, ckt, ref timestep);
            method.Terr(load.BJTstate + LoadBehavior.BJTqcs, ckt, ref timestep);
        }
    }
}
