using SpiceSharp.Behaviours;

namespace SpiceSharp.Components.ComponentBehaviours
{
    /// <summary>
    /// Behaviour of a resistor for AC analysis
    /// </summary>
    public class ResistorAcBehaviour : CircuitObjectBehaviourAcLoad
    {
        /// <summary>
        /// Perform AC calculations
        /// </summary>
        /// <param name="ckt"></param>
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
