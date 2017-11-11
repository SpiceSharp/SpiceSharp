using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// General behaviour for a <see cref="VoltageControlledVoltagesource"/>
    /// </summary>
    public class VoltageControlledVoltagesourceLoadBehavior : CircuitObjectBehaviorLoad
    {
        /// <summary>
        /// Execute behaviour
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Execute(Circuit ckt)
        {
            var vcvs = ComponentTyped<VoltageControlledVoltagesource>();
            var rstate = ckt.State;

            vcvs.VCVSposIbrptr.Add(1.0);
            vcvs.VCVSibrPosptr.Add(1.0);
            vcvs.VCVSnegIbrptr.Sub(1.0);
            vcvs.VCVSibrNegptr.Sub(1.0);
            vcvs.VCVSibrContPosptr.Sub(vcvs.VCVScoeff);
            vcvs.VCVSibrContNegptr.Add(vcvs.VCVScoeff);
        }
    }
}
