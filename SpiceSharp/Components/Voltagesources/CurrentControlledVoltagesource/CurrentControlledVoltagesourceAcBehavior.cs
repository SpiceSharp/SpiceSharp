using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// AC behaviour for <see cref="CurrentControlledVoltagesource"/>
    /// </summary>
    public class CurrentControlledVoltagesourceAcBehavior : CircuitObjectBehaviorAcLoad
    {
        /// <summary>
        /// Execute behaviour
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Load(Circuit ckt)
        {
            var ccvs = ComponentTyped<CurrentControlledVoltagesource>();
            var cstate = ckt.State;

            ccvs.CCVSposIbrptr.Add(1.0);
            ccvs.CCVSibrPosptr.Add(1.0);
            ccvs.CCVSnegIbrptr.Sub(1.0);
            ccvs.CCVSibrNegptr.Sub(1.0);
            ccvs.CCVSibrContBrptr.Sub(ccvs.CCVScoeff);
        }
    }
}
