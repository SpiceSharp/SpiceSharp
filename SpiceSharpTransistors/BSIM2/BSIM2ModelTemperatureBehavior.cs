using System;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// Temperature behaviour for a <see cref="BSIM2Model"/>
    /// </summary>
    public class BSIM2ModelTemperatureBehavior : CircuitObjectBehaviorTemperature
    {
        /// <summary>
        /// Execute the behaviour
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Temperature(Circuit ckt)
        {
            var model = ComponentTyped<BSIM2Model>();

            // Some Limiting for Model Parameters
            if (model.B2bulkJctPotential < 0.1)
            {
                model.B2bulkJctPotential.Value = 0.1;
            }
            if (model.B2sidewallJctPotential < 0.1)
            {
                model.B2sidewallJctPotential.Value = 0.1;
            }

            model.B2Cox = 3.453e-13 / (model.B2tox * 1.0e-4); /* in F / cm *  * 2 */
            model.B2vdd2 = 2.0 * model.B2vdd;
            model.B2vgg2 = 2.0 * model.B2vgg;
            model.B2vbb2 = 2.0 * model.B2vbb;
            model.B2Vtm = 8.625e-5 * (model.B2temp + 273.0);
        }
    }
}
