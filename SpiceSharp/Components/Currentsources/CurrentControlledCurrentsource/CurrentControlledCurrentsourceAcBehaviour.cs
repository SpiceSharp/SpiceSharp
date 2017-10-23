using SpiceSharp.Behaviours;

namespace SpiceSharp.Components.ComponentBehaviours
{
    /// <summary>
    /// AC behaviour for <see cref="CurrentControlledCurrentsource"/>
    /// </summary>
    public class CurrentControlledCurrentsourceAcBehaviour : CircuitObjectBehaviourAcLoad
    {
        /// <summary>
        /// Execute behaviour
        /// </summary>
        /// <param name="ckt"></param>
        public override void Execute(Circuit ckt)
        {
            var src = ComponentTyped<CurrentControlledCurrentsource>();
            var cstate = ckt.State.Complex;
            cstate.Matrix[src.CCCSposNode, src.CCCScontBranch] += src.CCCScoeff.Value;
            cstate.Matrix[src.CCCSnegNode, src.CCCScontBranch] -= src.CCCScoeff.Value;
        }
    }
}
