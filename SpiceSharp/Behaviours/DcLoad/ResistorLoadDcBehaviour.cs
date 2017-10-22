using SpiceSharp.Components;

namespace SpiceSharp.Behaviours.DcLoad
{
    public class ResistorLoadDcBehaviour : CircuitObjectBehaviorDcLoad
    {
        public override void Execute(Circuit ckt)
        {
            var rstate = ckt.State.Real;
            var resistor = ComponentTyped<Resistor>();

            rstate.Matrix[resistor.RESposNode, resistor.RESposNode] += resistor.RESconduct;
            rstate.Matrix[resistor.RESnegNode, resistor.RESnegNode] += resistor.RESconduct;
            rstate.Matrix[resistor.RESposNode, resistor.RESnegNode] -= resistor.RESconduct;
            rstate.Matrix[resistor.RESnegNode, resistor.RESposNode] -= resistor.RESconduct;
        }
    }
}
