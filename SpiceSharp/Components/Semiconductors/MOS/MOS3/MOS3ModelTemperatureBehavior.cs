using System;
using SpiceSharp.Diagnostics;
using SpiceSharp.Components.Transistors;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// Temperature behaviour for a <see cref="MOS3Model"/>
    /// </summary>
    public class MOS3ModelTemperatureBehavior : CircuitObjectBehaviorTemperature
    {
        /// <summary>
        /// Execute the behaviour
        /// </summary>
        /// <param name="ckt"></param>
        public override void Temperature(Circuit ckt)
        {
            var model = ComponentTyped<MOS3Model>();
            double kt1, arg1, fermis, wkfng, fermig, wkfngs, vfb;

            if (!model.MOS3tnom.Given)
            {
                model.MOS3tnom.Value = ckt.State.NominalTemperature;
            }
            model.fact1 = model.MOS3tnom / Circuit.CONSTRefTemp;
            model.vtnom = model.MOS3tnom * Circuit.CONSTKoverQ;
            kt1 = Circuit.CONSTBoltz * model.MOS3tnom;
            model.egfet1 = 1.16 - (7.02e-4 * model.MOS3tnom * model.MOS3tnom) / (model.MOS3tnom + 1108);
            arg1 = -model.egfet1 / (kt1 + kt1) + 1.1150877 / (Circuit.CONSTBoltz * (Circuit.CONSTRefTemp + Circuit.CONSTRefTemp));
            model.pbfact1 = -2 * model.vtnom * (1.5 * Math.Log(model.fact1) + Circuit.CHARGE * arg1);

            model.MOS3oxideCapFactor = 3.9 * 8.854214871e-12 / model.MOS3oxideThickness;
            if (!model.MOS3surfaceMobility.Given)
                model.MOS3surfaceMobility.Value = 600;
            if (!model.MOS3transconductance.Given)
            {
                model.MOS3transconductance.Value = model.MOS3surfaceMobility * model.MOS3oxideCapFactor * 1e-4;
            }
            if (model.MOS3substrateDoping.Given)
            {
                if (model.MOS3substrateDoping * 1e6 /* (cm *  * 3 / m *  * 3) */ > 1.45e16)
                {
                    if (!model.MOS3phi.Given)
                    {
                        model.MOS3phi.Value = 2 * model.vtnom * Math.Log(model.MOS3substrateDoping * 1e6 /* (cm *  * 3 / m *  * 3) */  / 1.45e16);
                        model.MOS3phi.Value = Math.Max(.1, model.MOS3phi);
                    }
                    fermis = model.MOS3type * .5 * model.MOS3phi;
                    wkfng = 3.2;
                    if (!model.MOS3gateType.Given)
                        model.MOS3gateType.Value = 1;
                    if (model.MOS3gateType != 0)
                    {
                        fermig = model.MOS3type * model.MOS3gateType * .5 * model.egfet1;
                        wkfng = 3.25 + .5 * model.egfet1 - fermig;
                    }
                    wkfngs = wkfng - (3.25 + .5 * model.egfet1 + fermis);
                    if (!model.MOS3gamma.Given)
                    {
                        model.MOS3gamma.Value = Math.Sqrt(2 * Transistor.EPSSIL * Circuit.CHARGE * model.MOS3substrateDoping * 1e6 /* (cm *  * 3 / m *  * 3) */) /
                            model.MOS3oxideCapFactor;
                    }
                    if (!model.MOS3vt0.Given)
                    {
                        if (!model.MOS3surfaceStateDensity.Given)
                            model.MOS3surfaceStateDensity.Value = 0;
                        vfb = wkfngs - model.MOS3surfaceStateDensity * 1e4 * Circuit.CHARGE / model.MOS3oxideCapFactor;
                        model.MOS3vt0.Value = vfb + model.MOS3type * (model.MOS3gamma * Math.Sqrt(model.MOS3phi) + model.MOS3phi);
                    }
                    else
                    {
                        vfb = model.MOS3vt0 - model.MOS3type * (model.MOS3gamma * Math.Sqrt(model.MOS3phi) + model.MOS3phi);
                    }
                    model.MOS3alpha = (Transistor.EPSSIL + Transistor.EPSSIL) / (Circuit.CHARGE * model.MOS3substrateDoping * 1e6 /* (cm *  * 3 / m *  * 3) */);
                    model.MOS3coeffDepLayWidth = Math.Sqrt(model.MOS3alpha);
                }
                else
                {
                    model.MOS3substrateDoping.Value = 0;
                    throw new CircuitException($"{model.Name}: Nsub < Ni");
                }
            }
            /* now model parameter preprocessing */
            model.MOS3narrowFactor = model.MOS3delta * 0.5 * Circuit.CONSTPI * Transistor.EPSSIL / model.MOS3oxideCapFactor;
        }
    }
}
