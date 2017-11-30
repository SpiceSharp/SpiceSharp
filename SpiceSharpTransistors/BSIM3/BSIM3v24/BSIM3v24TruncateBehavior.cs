using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// Truncate behavior for a <see cref="BSIM3v24"/>
    /// </summary>
    public class BSIM3v24TruncateBehavior : CircuitObjectBehaviorTruncate
    {
        /// <summary>
        /// Truncate
        /// </summary>
        /// <param name="ckt">Circuit</param>
        /// <param name="timeStep">Timestep</param>
        public override void Truncate(Circuit ckt, ref double timeStep)
        {
            var bsim3 = ComponentTyped<BSIM3v24>();
            var method = ckt.Method;
            method.Terr(bsim3.BSIM3states + BSIM3v24.BSIM3qb, ckt, ref timeStep);
            method.Terr(bsim3.BSIM3states + BSIM3v24.BSIM3qg, ckt, ref timeStep);
            method.Terr(bsim3.BSIM3states + BSIM3v24.BSIM3qd, ckt, ref timeStep);
        }
    }
}
