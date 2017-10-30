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
            // rstate.Matrix[ccvs.CCVSposNode, ccvs.CCVSbranch] += 1.0;
            // rstate.Matrix[ccvs.CCVSbranch, ccvs.CCVSposNode] += 1.0;
            // rstate.Matrix[ccvs.CCVSnegNode, ccvs.CCVSbranch] -= 1.0;
            // rstate.Matrix[ccvs.CCVSbranch, ccvs.CCVSnegNode] -= 1.0;
            // rstate.Matrix[ccvs.CCVSbranch, ccvs.CCVScontBranch] -= ccvs.CCVScoeff;
        }
    }
}
