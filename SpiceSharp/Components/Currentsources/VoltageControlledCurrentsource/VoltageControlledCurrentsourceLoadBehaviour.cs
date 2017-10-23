using SpiceSharp.Behaviours;

namespace SpiceSharp.Components.ComponentBehaviours
{
    /// <summary>
    /// General behaviour for a <see cref="VoltageControlledCurrentsource"/>
    /// </summary>
    public class VoltageControlledCurrentsourceLoadBehaviour : CircuitObjectBehaviourLoad
    {
        /// <summary>
        /// Execute behaviour
        /// </summary>
        /// <param name="ckt"></param>
        public override void Execute(Circuit ckt)
        {
            var src = ComponentTyped<VoltageControlledCurrentsource>();
            var rstate = ckt.State.Real;
            rstate.Matrix[src.VCCSposNode, src.VCCScontPosNode] += src.VCCScoeff;
            rstate.Matrix[src.VCCSposNode, src.VCCScontNegNode] -= src.VCCScoeff;
            rstate.Matrix[src.VCCSnegNode, src.VCCScontPosNode] -= src.VCCScoeff;
            rstate.Matrix[src.VCCSnegNode, src.VCCScontNegNode] += src.VCCScoeff;
        }
    }
}
