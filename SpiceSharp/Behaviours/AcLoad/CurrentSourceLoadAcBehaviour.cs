using SpiceSharp.Components;

namespace SpiceSharp.Behaviours.AcLoad
{
    public class CurrentSourceLoadAcBehaviour : CircuitObjectBehaviorAcLoad
    {
        public override void Execute(Circuit ckt)
        {
            var source = ComponentTyped<Currentsource>();

            var cstate = ckt.State.Complex;
            cstate.Rhs[source.ISRCposNode] += source.ISRCac;
            cstate.Rhs[source.ISRCnegNode] -= source.ISRCac;
        }
    }
}
