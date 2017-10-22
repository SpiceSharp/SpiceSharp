using SpiceSharp.Components;

namespace SpiceSharp.Behaviours.AcLoad
{
    public class VoltageSourceLoadAcBehaviour : CircuitObjectBehaviorAcLoad
    {
        public override void Execute(Circuit ckt)
        {
            var voltagesource = ComponentTyped<Voltagesource>();

            var cstate = ckt.State.Complex;
            cstate.Matrix[voltagesource.VSRCposNode, voltagesource.VSRCbranch] += 1.0;
            cstate.Matrix[voltagesource.VSRCnegNode, voltagesource.VSRCbranch] -= 1.0;
            cstate.Matrix[voltagesource.VSRCbranch, voltagesource.VSRCnegNode] -= 1.0;
            cstate.Matrix[voltagesource.VSRCbranch, voltagesource.VSRCposNode] += 1.0;
            cstate.Rhs[voltagesource.VSRCbranch] += voltagesource.VSRCac;
        }
    }
}
