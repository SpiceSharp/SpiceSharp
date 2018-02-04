using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// Truncate behavior for a <see cref="BSIM1"/>
    /// </summary>
    public class BSIM1TruncateBehavior : TruncateBehavior
    {
        /// <summary>
        /// Truncate
        /// </summary>
        /// <param name="ckt">Circuit</param>
        /// <param name="timeStep">Timestep</param>
        public override void Truncate(Circuit ckt, ref double timeStep)
        {
            var bsim1 = ComponentTyped<BSIM1>();
            var method = ckt.Method;
            method.Terr(bsim1.B1states + BSIM1.B1qb, ckt, ref timeStep);
            method.Terr(bsim1.B1states + BSIM1.B1qg, ckt, ref timeStep);
            method.Terr(bsim1.B1states + BSIM1.B1qd, ckt, ref timeStep);
        }
    }
}
