using SpiceSharp.Behaviours;

namespace SpiceSharp.Components.ComponentBehaviours
{
    /// <summary>
    /// AC behaviour for a <see cref="VoltageControlledCurrentsource"/>
    /// </summary>
    public class VoltageControlledCurrentsourceAcBehaviour : CircuitObjectBehaviourAcLoad
    {
        /// <summary>
        /// Execute the behaviour
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Execute(Circuit ckt)
        {
            var src = ComponentTyped<VoltageControlledCurrentsource>();
            var cstate = ckt.State.Complex;
            cstate.Matrix[src.VCCSposNode, src.VCCScontPosNode] += src.VCCScoeff.Value;
            cstate.Matrix[src.VCCSposNode, src.VCCScontNegNode] -= src.VCCScoeff.Value;
            cstate.Matrix[src.VCCSnegNode, src.VCCScontPosNode] -= src.VCCScoeff.Value;
            cstate.Matrix[src.VCCSnegNode, src.VCCScontNegNode] += src.VCCScoeff.Value;
        }
    }
}
