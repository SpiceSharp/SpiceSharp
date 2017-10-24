using System;
using SpiceSharp.Diagnostics;
using SpiceSharp.Behaviours;

namespace SpiceSharp.Components.ComponentBehaviours
{
    /// <summary>
    /// Temperature behaviour for a <see cref="DiodeModel"/>
    /// </summary>
    public class DiodeModelTemperatureBehaviour : CircuitObjectBehaviourTemperature
    {
        /// <summary>
        /// Execute behaviour
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Execute(Circuit ckt)
        {
            var model = ComponentTyped<DiodeModel>();
            if (!model.DIOnomTemp.Given)
            {
                model.DIOnomTemp.Value = ckt.State.NominalTemperature;
            }
            model.vtnom = Circuit.CONSTKoverQ * model.DIOnomTemp;
            /* limit grading coeff to max of .9 */
            if (model.DIOgradingCoeff > .9)
                CircuitWarning.Warning(this, $"{model.Name}: grading coefficient too large, limited to 0.9");

            /* limit activation energy to min of .1 */
            if (model.DIOactivationEnergy < .1)
                CircuitWarning.Warning(this, $"{model.Name}: activation energy too small, limited to 0.1");

            /* limit depletion cap coeff to max of .95 */
            if (model.DIOdepletionCapCoeff > .95)
                CircuitWarning.Warning(this, $"{model.Name}: coefficient Fc too large, limited to 0.95");
            if (!model.DIOresist.Given || model.DIOresist.Value == 0)
                model.DIOconductance = 0;
            else
                model.DIOconductance = 1 / model.DIOresist;
            model.xfc = Math.Log(1 - model.DIOdepletionCapCoeff);

            model.DIOf2 = Math.Exp((1 + model.DIOgradingCoeff) * model.xfc);
            model.DIOf3 = 1 - model.DIOdepletionCapCoeff * (1 + model.DIOgradingCoeff);
        }
    }
}
