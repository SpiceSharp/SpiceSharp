using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// AC behaviour for <see cref="Resistor"/>
    /// </summary>
    public class ResistorAcBehavior : CircuitObjectBehaviorAcLoad
    {
        /// <summary>
        /// Perform AC calculations
        /// </summary>
        /// <param name="ckt"></param>
        public override void Execute(Circuit ckt)
        {
            var cstate = ckt.State.Complex;
            var resistor = ComponentTyped<Resistor>();

            // cstate.Matrix[resistor.RESposNode, resistor.RESposNode] += resistor.RESconduct;
            // cstate.Matrix[resistor.RESposNode, resistor.RESnegNode] -= resistor.RESconduct;
            // cstate.Matrix[resistor.RESnegNode, resistor.RESposNode] -= resistor.RESconduct;
            // cstate.Matrix[resistor.RESnegNode, resistor.RESnegNode] += resistor.RESconduct;
        }
    }
}
