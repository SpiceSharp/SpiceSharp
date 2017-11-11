using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// General behaviour for <see cref="CurrentControlledVoltagesource"/>
    /// </summary>
    public class CurrentControlledVoltagesourceLoadBehavior : CircuitObjectBehaviorLoad
    {
        /// <summary>
        /// Execute behaviour
        /// </summary>
        /// <param name="ckt"></param>
        public override void Execute(Circuit ckt)
        {
            var ccvs = ComponentTyped<CurrentControlledVoltagesource>();
            var rstate = ckt.State;

            ccvs.CCVSposIbrptr.Add(1.0);
            ccvs.CCVSibrPosptr.Add(1.0);
            ccvs.CCVSnegIbrptr.Sub(1.0);
            ccvs.CCVSibrNegptr.Sub(1.0);
            ccvs.CCVSibrContBrptr.Sub(ccvs.CCVScoeff);
        }
    }
}
