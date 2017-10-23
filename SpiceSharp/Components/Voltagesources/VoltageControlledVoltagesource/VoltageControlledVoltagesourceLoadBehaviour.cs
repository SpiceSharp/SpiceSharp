using SpiceSharp.Behaviours;

namespace SpiceSharp.Components.ComponentBehaviours
{
    /// <summary>
    /// General behaviour for a <see cref="VoltageControlledVoltagesource"/>
    /// </summary>
    public class VoltageControlledVoltagesourceLoadBehaviour : CircuitObjectBehaviourLoad
    {
        /// <summary>
        /// Execute behaviour
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Execute(Circuit ckt)
        {
            var vcvs = ComponentTyped<VoltageControlledVoltagesource>();
            var rstate = ckt.State.Real;
            rstate.Matrix[vcvs.VCVSposNode, vcvs.VCVSbranch] += 1.0;
            rstate.Matrix[vcvs.VCVSbranch, vcvs.VCVSposNode] += 1.0;
            rstate.Matrix[vcvs.VCVSnegNode, vcvs.VCVSbranch] -= 1.0;
            rstate.Matrix[vcvs.VCVSbranch, vcvs.VCVSnegNode] -= 1.0;
            rstate.Matrix[vcvs.VCVSbranch, vcvs.VCVScontPosNode] -= vcvs.VCVScoeff;
            rstate.Matrix[vcvs.VCVSbranch, vcvs.VCVScontNegNode] += vcvs.VCVScoeff;
        }
    }
}
