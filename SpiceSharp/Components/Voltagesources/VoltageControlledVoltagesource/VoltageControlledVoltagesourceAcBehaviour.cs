using SpiceSharp.Behaviours;

namespace SpiceSharp.Components.ComponentBehaviours
{
    /// <summary>
    /// AC behaviour for a <see cref="VoltageControlledVoltagesource"/>
    /// </summary>
    public class VoltageControlledVoltagesourceAcBehaviour : CircuitObjectBehaviourAcLoad
    {
        /// <summary>
        /// Execute behaviour
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Execute(Circuit ckt)
        {
            var vcvs = ComponentTyped<VoltageControlledVoltagesource>();
            var cstate = ckt.State.Complex;
            cstate.Matrix[vcvs.VCVSposNode, vcvs.VCVSbranch] += 1.0;
            cstate.Matrix[vcvs.VCVSbranch, vcvs.VCVSposNode] += 1.0;
            cstate.Matrix[vcvs.VCVSnegNode, vcvs.VCVSbranch] -= 1.0;
            cstate.Matrix[vcvs.VCVSbranch, vcvs.VCVSnegNode] -= 1.0;
            cstate.Matrix[vcvs.VCVSbranch, vcvs.VCVScontPosNode] -= vcvs.VCVScoeff.Value;
            cstate.Matrix[vcvs.VCVSbranch, vcvs.VCVScontNegNode] += vcvs.VCVScoeff.Value;
        }
    }
}
