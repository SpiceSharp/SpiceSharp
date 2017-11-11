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
            var cstate = ckt.State;
            var resistor = ComponentTyped<Resistor>();

            resistor.RESposPosPtr.Add(resistor.RESconduct);
            resistor.RESnegNegPtr.Add(resistor.RESconduct);
            resistor.RESposNegPtr.Sub(resistor.RESconduct);
            resistor.RESnegPosPtr.Sub(resistor.RESconduct);
        }
    }
}
