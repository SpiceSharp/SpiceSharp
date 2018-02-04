using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// Truncate behavior for a <see cref="MOS2"/>
    /// </summary>
    public class BSIM2TruncateBehavior : TruncateBehavior
    {
        /// <summary>
        /// Truncate
        /// </summary>
        /// <param name="ckt">Circuit</param>
        /// <param name="timeStep">Timestep</param>
        public override void Truncate(Circuit ckt, ref double timeStep)
        {
            var bsim2 = ComponentTyped<BSIM2>();
            var method = ckt.Method;
            method.Terr(bsim2.B2states + BSIM2.B2qb, ckt, ref timeStep);
            method.Terr(bsim2.B2states + BSIM2.B2qg, ckt, ref timeStep);
            method.Terr(bsim2.B2states + BSIM2.B2qd, ckt, ref timeStep);
        }
    }
}
