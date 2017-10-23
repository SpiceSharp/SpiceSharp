using SpiceSharp.Behaviours;

namespace SpiceSharp.Components.ComponentBehaviours
{
    /// <summary>
    /// Behaviour for a <see cref="CurrentControlledCurrentsource"/>
    /// </summary>
    public class CurrentControlledCurrentsourceLoadBehaviour : CircuitObjectBehaviourLoad
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
