using SpiceSharp.Parameters;
using SpiceSharp.Behaviors;
using SpiceSharp.Circuits;

namespace SpiceSharp.Components.CAP
{
    /// <summary>
    /// Temperature behavior for a <see cref="CapacitorModel"/>
    /// </summary>
    public class ModelTemperatureBehavior : CircuitObjectBehaviorTemperature
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("cj"), SpiceInfo("Bottom capacitance per area")]
        public Parameter CAPcj { get; } = new Parameter();
        [SpiceName("cjsw"), SpiceInfo("Sidewall capacitance per meter")]
        public Parameter CAPcjsw { get; } = new Parameter();
        [SpiceName("defw"), SpiceInfo("Default width")]
        public Parameter CAPdefWidth { get; } = new Parameter(10.0e-6);
        [SpiceName("narrow"), SpiceInfo("Width correction factor")]
        public Parameter CAPnarrow { get; } = new Parameter();

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        /// <returns></returns>
        public override void Setup(CircuitObject component, Circuit ckt)
        {
            DataOnly = true;
        }

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
