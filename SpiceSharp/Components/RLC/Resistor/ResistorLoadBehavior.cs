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

            resistor.RESposPosPtr.Value.Real += resistor.RESconduct;
            resistor.RESnegNegPtr.Value.Real += resistor.RESconduct;
            resistor.RESposNegPtr.Value.Real -= resistor.RESconduct;
            resistor.RESnegPosPtr.Value.Real -= resistor.RESconduct;
        }
    }
}
