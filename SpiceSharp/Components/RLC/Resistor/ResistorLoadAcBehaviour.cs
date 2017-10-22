using SpiceSharp.Components;

namespace SpiceSharp.Behaviours.AcLoad
{
    public class ResistorLoadAcBehaviour : CircuitObjectBehaviorAcLoad
    {
        public override void Execute(Circuit ckt)
        {
            var cstate = ckt.State.Complex;
            var resistor = ComponentTyped<Resistor>();

            cstate.Matrix[resistor.RESposNode, resistor.RESposNode] += resistor.RESconduct;
            cstate.Matrix[resistor.RESposNode, resistor.RESnegNode] -= resistor.RESconduct;
            cstate.Matrix[resistor.RESnegNode, resistor.RESposNode] -= resistor.RESconduct;
            cstate.Matrix[resistor.RESnegNode, resistor.RESnegNode] += resistor.RESconduct;
        }
    }
}
