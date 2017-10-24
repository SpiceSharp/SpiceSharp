using System;
using SpiceSharp.Diagnostics;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// Temperature behaviour for a <see cref="MOS1Model"/>
    /// </summary>
    public class MOS1ModelTemperatureBehavior : CircuitObjectBehaviorTemperature
    {
        /// <summary>
        /// Execute behaviour
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Execute(Circuit ckt)
        {
            var model = ComponentTyped<MOS1Model>();
            double kt1, arg1, fermis, wkfng, fermig, wkfngs, vfb = 0.0;

            /* perform model defaulting */
            if (!model.MOS1tnom.Given)
                model.MOS1tnom.Value = ckt.State.NominalTemperature;

            model.fact1 = model.MOS1tnom / Circuit.CONSTRefTemp;
            model.vtnom = model.MOS1tnom * Circuit.CONSTKoverQ;
            kt1 = Circuit.CONSTBoltz * model.MOS1tnom;
            model.egfet1 = 1.16 - (7.02e-4 * model.MOS1tnom * model.MOS1tnom) / (model.MOS1tnom + 1108);
            arg1 = -model.egfet1 / (kt1 + kt1) + 1.1150877 / (Circuit.CONSTBoltz * (Circuit.CONSTRefTemp + Circuit.CONSTRefTemp));
            model.pbfact1 = -2 * model.vtnom * (1.5 * Math.Log(model.fact1) + Circuit.CHARGE * arg1);

            /* now model parameter preprocessing */

            if (!model.MOS1oxideThickness.Given || model.MOS1oxideThickness.Value == 0)
            {
                model.MOS1oxideCapFactor = 0;
            }
            else
            {
                model.MOS1oxideCapFactor = 3.9 * 8.854214871e-12 / model.MOS1oxideThickness;
                if (!model.MOS1transconductance.Given)
                {
                    if (!model.MOS1surfaceMobility.Given)
                    {
                        model.MOS1surfaceMobility.Value = 600;
                    }
                    model.MOS1transconductance.Value = model.MOS1surfaceMobility * model.MOS1oxideCapFactor * 1e-4;
                }
                if (model.MOS1substrateDoping.Given)
                {
                    if (model.MOS1substrateDoping * 1e6 > 1.45e16)
                    {
                        if (!model.MOS1phi.Given)
                        {
                            model.MOS1phi.Value = 2 * model.vtnom * Math.Log(model.MOS1substrateDoping * 1e6 / 1.45e16);
                            model.MOS1phi.Value = Math.Max(.1, model.MOS1phi);
                        }
                        fermis = model.MOS1type * .5 * model.MOS1phi;
                        wkfng = 3.2;
                        if (!model.MOS1gateType.Given)
                            model.MOS1gateType.Value = 1;
                        if (model.MOS1gateType != 0)
                        {
                            fermig = model.MOS1type * model.MOS1gateType * .5 * model.egfet1;
                            wkfng = 3.25 + .5 * model.egfet1 - fermig;
                        }
                        wkfngs = wkfng - (3.25 + .5 * model.egfet1 + fermis);
                        if (!model.MOS1gamma.Given)
                        {
                            model.MOS1gamma.Value = Math.Sqrt(2 * 11.70 * 8.854214871e-12 * Circuit.CHARGE * model.MOS1substrateDoping * 1e6) / model.MOS1oxideCapFactor;
                        }
                        if (!model.MOS1vt0.Given)
                        {
                            if (!model.MOS1surfaceStateDensity.Given)
                                model.MOS1surfaceStateDensity.Value = 0;
                            vfb = wkfngs - model.MOS1surfaceStateDensity * 1e4 * Circuit.CHARGE / model.MOS1oxideCapFactor;
                            model.MOS1vt0.Value = vfb + model.MOS1type * (model.MOS1gamma * Math.Sqrt(model.MOS1phi) + model.MOS1phi);
                        }
                    }
                    else
                    {
                        model.MOS1substrateDoping.Value = 0;
                        throw new CircuitException($"{model.Name}: Nsub < Ni");
                    }
                }
            }
        }
    }
}
