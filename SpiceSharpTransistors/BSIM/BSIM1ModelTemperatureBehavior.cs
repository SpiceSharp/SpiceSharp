using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// Temperature behaviour of a <see cref="BSIM1Model"/>
    /// </summary>
    public class BSIM1ModelTemperatureBehavior : CircuitObjectBehaviorTemperature
    {
        /// <summary>
        /// Execute the behaviour
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Temperature(Circuit ckt)
        {
            var model = ComponentTyped<BSIM1Model>();

            /* Default value Processing for B1 MOSFET Models */
            /* Some Limiting for Model Parameters */
            if (model.B1bulkJctPotential < 0.1)
            {
                model.B1bulkJctPotential.Value = 0.1;
            }
            if (model.B1sidewallJctPotential < 0.1)
            {
                model.B1sidewallJctPotential.Value = 0.1;
            }

            model.Cox = 3.453e-13 / (model.B1oxideThickness * 1.0e-4); /* in F / cm *  * 2 */
            model.B1Cox = model.Cox; /* unit:  F / cm *  * 2 */
        }
    }
}
