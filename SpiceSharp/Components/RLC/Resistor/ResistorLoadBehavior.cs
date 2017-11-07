using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// General behaviour for <see cref="Resistor"/>
    /// </summary>
    public class ResistorLoadBehavior : CircuitObjectBehaviorLoad
    {
        /// <summary>
        /// Perform calculations
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Execute(Circuit ckt)
        {
            var rstate = ckt.State;
            var resistor = ComponentTyped<Resistor>();

            resistor.RESposPosPtr.Add(resistor.RESconduct);
            resistor.RESnegNegPtr.Add(resistor.RESconduct);
            resistor.RESposNegPtr.Sub(resistor.RESconduct);
            resistor.RESnegPosPtr.Sub(resistor.RESconduct);
        }
    }
}
