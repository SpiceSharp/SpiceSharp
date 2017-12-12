using SpiceSharp.Behaviors;
using SpiceSharp.Circuits;
using SpiceSharp.Parameters;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// Temperature behavior for a <see cref="ResistorModel"/>
    /// </summary>
    public class ResistorModelTemperatureBehavior : CircuitObjectBehaviorTemperature
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("tnom"), SpiceInfo("Parameter measurement temperature", Interesting = false)]
        public double RES_TNOM
        {
            get => REStnom - Circuit.CONSTCtoK;
            set => REStnom.Set(value + Circuit.CONSTCtoK);
        }
        public Parameter REStnom { get; } = new Parameter(300.15);
        [SpiceName("tc1"), SpiceInfo("First order temperature coefficient")]
        public Parameter REStempCoeff1 { get; } = new Parameter();
        [SpiceName("tc2"), SpiceInfo("Second order temperature oefficient")]
        public Parameter REStempCoeff2 { get; } = new Parameter();
        [SpiceName("rsh"), SpiceInfo("Sheet resistance")]
        public Parameter RESsheetRes { get; } = new Parameter();
        [SpiceName("defw"), SpiceInfo("Default device width")]
        public Parameter RESdefWidth { get; } = new Parameter(10.0e-6);
        [SpiceName("narrow"), SpiceInfo("Narrowing of resistor")]
        public Parameter RESnarrow { get; } = new Parameter();

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        /// <returns></returns>
        public override bool Setup(CircuitObject component, Circuit ckt) => false;

        /// <summary>
        /// Temperature behavior
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Temperature(Circuit ckt)
        {
            // Do nothing
        }
    }
}
