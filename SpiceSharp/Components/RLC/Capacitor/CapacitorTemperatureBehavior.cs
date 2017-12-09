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
        private CapacitorModel model;

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        public override void Setup(CircuitObject component, Circuit ckt)
        {
            base.Setup(component, ckt);
            
            // Get the model
            model = (component as Capacitor)?.Model as CapacitorModel;
        }

        /// <summary>
        /// Execute the behaviour
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Temperature(Circuit ckt)
        {
            // Default Value Processing for Capacitor Instance
            if (model != null)
            {
                if (!CAPwidth.Given)
                    CAPwidth.Value = model.CAPdefWidth;
                CAPcapac = model.CAPcj *
                                (CAPwidth - model.CAPnarrow) *
                                (CAPlength - model.CAPnarrow) +
                            model.CAPcjsw * 2 * (
                                (CAPlength - model.CAPnarrow) +
                                (CAPwidth - model.CAPnarrow));
            }
        }
    }
}
