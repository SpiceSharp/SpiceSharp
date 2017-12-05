using System;
using SpiceSharp.Behaviors;
using SpiceSharp.Diagnostics;
using SpiceSharp.Components.Transistors;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// Temperature behaviour for a <see cref="MOS2Model"/>
    /// </summary>
    public class MOS2ModelTemperatureBehavior : CircuitObjectBehaviorTemperature
    {
        /// <summary>
        /// Execute behaviour
        /// </summary>
        /// <param name="ckt"></param>
        public override void Temperature(Circuit ckt)
        {
            var model = ComponentTyped<MOS2Model>();
            double kt1, arg1, fermis, wkfng, fermig, wkfngs, vfb = 0.0;

            /* now model parameter preprocessing */
            if (!model.MOS2tnom.Given)
                model.MOS2tnom.Value = ckt.State.NominalTemperature;
            model.fact1 = model.MOS2tnom / Circuit.CONSTRefTemp;
            model.vtnom = model.MOS2tnom * Circuit.CONSTKoverQ;
            kt1 = Circuit.CONSTBoltz * model.MOS2tnom;
            model.egfet1 = 1.16 - (7.02e-4 * model.MOS2tnom * model.MOS2tnom) / (model.MOS2tnom + 1108);
            arg1 = -model.egfet1 / (kt1 + kt1) + 1.1150877 / (Circuit.CONSTBoltz * (Circuit.CONSTRefTemp + Circuit.CONSTRefTemp));
            model.pbfact1 = -2 * model.vtnom * (1.5 * Math.Log(model.fact1) + Circuit.CHARGE * arg1);

            if (!model.MOS2oxideThickness.Given)
            {
                model.MOS2oxideThickness.Value = 1e-7;
            }
            model.MOS2oxideCapFactor = 3.9 * 8.854214871e-12 / model.MOS2oxideThickness;

            if (!model.MOS2surfaceMobility.Given)
                model.MOS2surfaceMobility.Value = 600;
            if (!model.MOS2transconductance.Given)
            {
                model.MOS2transconductance.Value = model.MOS2surfaceMobility * 1e-4 * model.MOS2oxideCapFactor;
            }
            if (model.MOS2substrateDoping.Given)
            {
                if (model.MOS2substrateDoping * 1e6 > 1.45e16)
                {
                    if (!model.MOS2phi.Given)
                    {
                        model.MOS2phi.Value = 2 * model.vtnom * Math.Log(model.MOS2substrateDoping * 1e6 / 1.45e16);
                        model.MOS2phi.Value = Math.Max(.1, model.MOS2phi);
                    }
                    fermis = model.MOS2type * .5 * model.MOS2phi;
                    wkfng = 3.2;
                    if (!model.MOS2gateType.Given)
                        model.MOS2gateType.Value = 1;
                    if (model.MOS2gateType != 0)
                    {
                        fermig = model.MOS2type * model.MOS2gateType * .5 * model.egfet1;
                        wkfng = 3.25 + .5 * model.egfet1 - fermig;
                    }
                    wkfngs = wkfng - (3.25 + .5 * model.egfet1 + fermis);
                    if (!model.MOS2gamma.Given)
                    {
                        model.MOS2gamma.Value = Math.Sqrt(2 * 11.70 * 8.854214871e-12 * Circuit.CHARGE * model.MOS2substrateDoping * 1e6) / model.MOS2oxideCapFactor;
                    }
                    if (!model.MOS2vt0.Given)
                    {
                        if (!model.MOS2surfaceStateDensity.Given)
                            model.MOS2surfaceStateDensity.Value = 0;
                        vfb = wkfngs - model.MOS2surfaceStateDensity * 1e4 * Circuit.CHARGE / model.MOS2oxideCapFactor;
                        model.MOS2vt0.Value = vfb + model.MOS2type * (model.MOS2gamma * Math.Sqrt(model.MOS2phi) + model.MOS2phi);
                    }
                    else
                    {
                        vfb = model.MOS2vt0 - model.MOS2type * (model.MOS2gamma * Math.Sqrt(model.MOS2phi) + model.MOS2phi);
                    }
                    model.MOS2xd = Math.Sqrt((Transistor.EPSSIL + Transistor.EPSSIL) / (Circuit.CHARGE * model.MOS2substrateDoping * 1e6));
                }
                else
                {
                    model.MOS2substrateDoping.Value = 0;
                    throw new CircuitException($"{model.Name}: Nsub < Ni");
                }
            }
            if (!model.MOS2bulkCapFactor.Given)
            {
                model.MOS2bulkCapFactor.Value = Math.Sqrt(Transistor.EPSSIL * Circuit.CHARGE * model.MOS2substrateDoping * 1e6 /* cm**3/m**3 */  / (2 *
                    model.MOS2bulkJctPotential));
            }
        }
    }
}
