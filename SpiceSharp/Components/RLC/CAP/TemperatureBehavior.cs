using SpiceSharp.Behaviors;
using SpiceSharp.Circuits;
using SpiceSharp.Parameters;

namespace SpiceSharp.Components.CAP
{
    /// <summary>
    /// Temperature behaviour for a <see cref="Capacitor"/>
    /// </summary>
    public class TemperatureBehavior : CircuitObjectBehaviorTemperature
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        private ModelTemperatureBehavior modeltemp;
        
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
        /// Setup the behavior
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        public override void Setup(CircuitObject component, Circuit ckt)
        {
            var cap = component as Capacitor;
            if (cap.Model == null)
            {
                modeltemp = null;
                return;
            }

            // Get behaviors
            modeltemp = GetBehavior<ModelTemperatureBehavior>(cap.Model);
        }

        /// <summary>
        /// Execute the behaviour
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Temperature(Circuit ckt)
        {
            // Default Value Processing for Capacitor Instance
            if (modeltemp != null)
            {
                if (!CAPwidth.Given)
                    CAPwidth.Value = modeltemp.CAPdefWidth;
                CAPcapac = modeltemp.CAPcj *
                                (CAPwidth - modeltemp.CAPnarrow) *
                                (CAPlength - modeltemp.CAPnarrow) +
                            modeltemp.CAPcjsw * 2 * (
                                (CAPlength - modeltemp.CAPnarrow) +
                                (CAPwidth - modeltemp.CAPnarrow));
            }
        }
    }
}
