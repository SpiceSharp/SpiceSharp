using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// Temperature behaviour for a <see cref="Capacitor"/>
    /// </summary>
    public class CapacitorTemperatureBehavior : CircuitObjectBehaviorTemperature
    {
        /// <summary>
        /// Execute the behaviour
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Execute(Circuit ckt)
        {
            // Default Value Processing for Capacitor Instance
            var cap = ComponentTyped<Capacitor>();
            var model = cap.Model as CapacitorModel;

            if (model != null)
            {
                if (!cap.CAPwidth.Given)
                    cap.CAPwidth.Value = model.CAPdefWidth;
                if (!cap.CAPcapac.Given)
                {
                    cap.CAPcapac.Value =
                            model.CAPcj *
                                (cap.CAPwidth - model.CAPnarrow) *
                                (cap.CAPlength - model.CAPnarrow) +
                            model.CAPcjsw * 2 * (
                                (cap.CAPlength - model.CAPnarrow) +
                                (cap.CAPwidth - model.CAPnarrow));
                }
            }
        }
    }
}
