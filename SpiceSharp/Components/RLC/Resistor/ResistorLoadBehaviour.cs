using SpiceSharp.Behaviours;

namespace SpiceSharp.Components.ComponentBehaviours
{
    /// <summary>
    /// General behaviour for <see cref="Resistor"/>
    /// </summary>
    public class ResistorLoadBehaviour : CircuitObjectBehaviourLoad
    {
        /// <summary>
        /// Perform calculations
        /// </summary>
        /// <param name="ckt">Circuit</param>
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
