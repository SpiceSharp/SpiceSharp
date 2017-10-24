using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// Behavior for a <see cref="CurrentControlledCurrentsource"/>
    /// </summary>
    public class CurrentControlledCurrentsourceLoadBehavior : CircuitObjectBehaviorLoad
    {
        /// <summary>
        /// Execute behaviour
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Execute(Circuit ckt)
        {
            var src = ComponentTyped<CurrentControlledCurrentsource>();
            var rstate = ckt.State.Real;
            rstate.Matrix[src.CCCSposNode, src.CCCScontBranch] += src.CCCScoeff.Value;
            rstate.Matrix[src.CCCSnegNode, src.CCCScontBranch] -= src.CCCScoeff.Value;
        }
    }
}
