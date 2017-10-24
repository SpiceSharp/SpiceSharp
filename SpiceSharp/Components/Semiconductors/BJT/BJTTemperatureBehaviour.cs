using System;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// Temperature behaviour for a <see cref="BJT"/>
    /// </summary>
    public class BJTTemperatureBehavior : CircuitObjectBehaviorTemperature
    {
        /// <summary>
        /// Execute behaviour
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Execute(Circuit ckt)
        {
            var bjt = ComponentTyped<BJT>();
            BJTModel model = bjt.Model as BJTModel;

            double vt, fact2, egfet, arg, pbfact, ratlog, ratio1, factlog, factor, bfactor, pbo, gmaold, gmanew;

            if (!bjt.BJTtemp.Given)
                bjt.BJTtemp.Value = ckt.State.Temperature;
            vt = bjt.BJTtemp * Circuit.CONSTKoverQ;
            fact2 = bjt.BJTtemp / Circuit.CONSTRefTemp;
            egfet = 1.16 - (7.02e-4 * bjt.BJTtemp * bjt.BJTtemp) / (bjt.BJTtemp + 1108);
            arg = -egfet / (2 * Circuit.CONSTBoltz * bjt.BJTtemp) + 1.1150877 / (Circuit.CONSTBoltz * (Circuit.CONSTRefTemp +
                 Circuit.CONSTRefTemp));
            pbfact = -2 * vt * (1.5 * Math.Log(fact2) + Circuit.CHARGE * arg);

            ratlog = Math.Log(bjt.BJTtemp / model.BJTtnom);
            ratio1 = bjt.BJTtemp / model.BJTtnom - 1;
            factlog = ratio1 * model.BJTenergyGap / vt + model.BJTtempExpIS * ratlog;
            factor = Math.Exp(factlog);
            bjt.BJTtSatCur = model.BJTsatCur * factor;
            bfactor = Math.Exp(ratlog * model.BJTbetaExp);
            bjt.BJTtBetaF = model.BJTbetaF * bfactor;
            bjt.BJTtBetaR = model.BJTbetaR * bfactor;
            bjt.BJTtBEleakCur = model.BJTleakBEcurrent * Math.Exp(factlog / model.BJTleakBEemissionCoeff) / bfactor;
            bjt.BJTtBCleakCur = model.BJTleakBCcurrent * Math.Exp(factlog / model.BJTleakBCemissionCoeff) / bfactor;

            pbo = (model.BJTpotentialBE - pbfact) / model.fact1;
            gmaold = (model.BJTpotentialBE - pbo) / pbo;
            bjt.BJTtBEcap = model.BJTdepletionCapBE / (1 + model.BJTjunctionExpBE * (4e-4 * (model.BJTtnom - Circuit.CONSTRefTemp) - gmaold));
            bjt.BJTtBEpot = fact2 * pbo + pbfact;
            gmanew = (bjt.BJTtBEpot - pbo) / pbo;
            bjt.BJTtBEcap *= 1 + model.BJTjunctionExpBE * (4e-4 * (bjt.BJTtemp - Circuit.CONSTRefTemp) - gmanew);

            pbo = (model.BJTpotentialBC - pbfact) / model.fact1;
            gmaold = (model.BJTpotentialBC - pbo) / pbo;
            bjt.BJTtBCcap = model.BJTdepletionCapBC / (1 + model.BJTjunctionExpBC * (4e-4 * (model.BJTtnom - Circuit.CONSTRefTemp) - gmaold));
            bjt.BJTtBCpot = fact2 * pbo + pbfact;
            gmanew = (bjt.BJTtBCpot - pbo) / pbo;
            bjt.BJTtBCcap *= 1 + model.BJTjunctionExpBC * (4e-4 * (bjt.BJTtemp - Circuit.CONSTRefTemp) - gmanew);

            bjt.BJTtDepCap = model.BJTdepletionCapCoeff * bjt.BJTtBEpot;
            bjt.BJTtf1 = bjt.BJTtBEpot * (1 - Math.Exp((1 - model.BJTjunctionExpBE) * model.xfc)) / (1 - model.BJTjunctionExpBE);
            bjt.BJTtf4 = model.BJTdepletionCapCoeff * bjt.BJTtBCpot;
            bjt.BJTtf5 = bjt.BJTtBCpot * (1 - Math.Exp((1 - model.BJTjunctionExpBC) * model.xfc)) / (1 - model.BJTjunctionExpBC);
            bjt.BJTtVcrit = vt * Math.Log(vt / (Circuit.CONSTroot2 * model.BJTsatCur));
        }
    }
}
