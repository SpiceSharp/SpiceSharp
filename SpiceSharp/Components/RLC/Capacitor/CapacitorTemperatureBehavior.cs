using SpiceSharp.Behaviors;
using SpiceSharp.Circuits;
using SpiceSharp.Parameters;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// Temperature behaviour for a <see cref="Capacitor"/>
    /// </summary>
    public class CapacitorTemperatureBehavior : CircuitObjectBehaviorTemperature
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("w"), SpiceInfo("Device width", Interesting = false)]
        public Parameter CAPwidth { get; } = new Parameter();
        [SpiceName("l"), SpiceInfo("Device length", Interesting = false)]
        public Parameter CAPlength { get; } = new Parameter();

        /// <summary>
        /// The calculated capacitance
        /// </summary>
        public double CAPcapac { get; private set; }

        /// <summary>
        /// Private variables
        /// </summary>
        private CapacitorModelTemperatureBehavior temp;

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        public override bool Setup(CircuitObject component, Circuit ckt)
        {
            // Get the model
            var model = (component as Capacitor)?.Model as CapacitorModel;
            temp = model?.GetBehavior(typeof(CircuitObjectBehaviorTemperature)) as CapacitorModelTemperatureBehavior;
            return true;
        }

        /// <summary>
        /// Execute the behaviour
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Temperature(Circuit ckt)
        {
            // Default Value Processing for Capacitor Instance
            if (temp != null)
            {
                if (!CAPwidth.Given)
                    CAPwidth.Value = temp.CAPdefWidth;
                CAPcapac = temp.CAPcj *
                                (CAPwidth - temp.CAPnarrow) *
                                (CAPlength - temp.CAPnarrow) +
                            temp.CAPcjsw * 2 * (
                                (CAPlength - temp.CAPnarrow) +
                                (CAPwidth - temp.CAPnarrow));
            }
        }
    }
}
