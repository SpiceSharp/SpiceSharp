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
        public override void Execute(Circuit ckt)
        {
            var ccvs = ComponentTyped<CurrentControlledVoltagesource>();
            var cstate = ckt.State.Complex;
            // cstate.Matrix[ccvs.CCVSposNode, ccvs.CCVSbranch] += 1.0;
            // cstate.Matrix[ccvs.CCVSbranch, ccvs.CCVSposNode] += 1.0;
            // cstate.Matrix[ccvs.CCVSnegNode, ccvs.CCVSbranch] -= 1.0;
            // cstate.Matrix[ccvs.CCVSbranch, ccvs.CCVSnegNode] -= 1.0;
            // cstate.Matrix[ccvs.CCVSbranch, ccvs.CCVScontBranch] -= ccvs.CCVScoeff.Value;
        }
    }
}
