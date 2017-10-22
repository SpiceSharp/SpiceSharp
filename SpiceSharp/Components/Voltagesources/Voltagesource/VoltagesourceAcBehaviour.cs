using SpiceSharp.Behaviours;

namespace SpiceSharp.Components.ComponentBehaviours
{
    /// <summary>
    /// AC behaviour for <see cref="Voltagesource"/>
    /// </summary>
    public class VoltageSourceLoadAcBehaviour : CircuitObjectBehaviourAcLoad
    {
        /// <summary>
        /// Execute AC behaviour
        /// </summary>
        /// <param name="ckt">Circuit</param>
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
