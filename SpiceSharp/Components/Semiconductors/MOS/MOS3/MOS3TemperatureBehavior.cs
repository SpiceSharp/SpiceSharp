using System;
using SpiceSharp.Diagnostics;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// Temperature behaviour for a <see cref="mos3.MOS3"/>
    /// </summary>
    public class MOS3TemperatureBehavior : CircuitObjectBehaviorTemperature
    {
        /// <summary>
        /// Execute behaviour
        /// </summary>
        /// <param name="ckt"></param>
        public override void Temperature(Circuit ckt)
        {
            var mos3 = ComponentTyped<MOS3>();
            var model = mos3.Model as MOS3Model;
            double vt, ratio, fact2, kt, egfet, arg, pbfact, ratio4, phio, pbo, gmaold, capfact, gmanew, czbd, czbdsw, sarg, sargsw, czbs,
                czbssw;

            /* perform the parameter defaulting */

            if (!mos3.MOS3temp.Given)
            {
                mos3.MOS3temp.Value = ckt.State.Temperature;
            }
            vt = mos3.MOS3temp * Circuit.CONSTKoverQ;
            ratio = mos3.MOS3temp / model.MOS3tnom;
            fact2 = mos3.MOS3temp / Circuit.CONSTRefTemp;
            kt = mos3.MOS3temp * Circuit.CONSTBoltz;
            egfet = 1.16 - (7.02e-4 * mos3.MOS3temp * mos3.MOS3temp) / (mos3.MOS3temp + 1108);
            arg = -egfet / (kt + kt) + 1.1150877 / (Circuit.CONSTBoltz * (Circuit.CONSTRefTemp + Circuit.CONSTRefTemp));
            pbfact = -2 * vt * (1.5 * Math.Log(fact2) + Circuit.CHARGE * arg);

            if (model.MOS3drainResistance.Given)
            {
                if (model.MOS3drainResistance != 0)
                {
                    mos3.MOS3drainConductance = 1 / model.MOS3drainResistance;
                }
                else
                {
                    mos3.MOS3drainConductance = 0;
                }
            }
            else if (model.MOS3sheetResistance.Given)
            {
                if (model.MOS3sheetResistance != 0)
                {
                    mos3.MOS3drainConductance = 1 / (model.MOS3sheetResistance * mos3.MOS3drainSquares);
                }
                else
                {
                    mos3.MOS3drainConductance = 0;
                }
            }
            else
            {
                mos3.MOS3drainConductance = 0;
            }
            if (model.MOS3sourceResistance.Given)
            {
                if (model.MOS3sourceResistance != 0)
                {
                    mos3.MOS3sourceConductance = 1 / model.MOS3sourceResistance;
                }
                else
                {
                    mos3.MOS3sourceConductance = 0;
                }
            }
            else if (model.MOS3sheetResistance.Given)
            {
                if (model.MOS3sheetResistance != 0)
                {
                    mos3.MOS3sourceConductance = 1 / (model.MOS3sheetResistance * mos3.MOS3sourceSquares);
                }
                else
                {
                    mos3.MOS3sourceConductance = 0;
                }
            }
            else
            {
                mos3.MOS3sourceConductance = 0;
            }

            if (mos3.MOS3l - 2 * model.MOS3latDiff <= 0)
                throw new CircuitException($"{mos3.Name}: effective channel length less than zero");
            ratio4 = ratio * Math.Sqrt(ratio);
            mos3.MOS3tTransconductance = model.MOS3transconductance / ratio4;
            mos3.MOS3tSurfMob = model.MOS3surfaceMobility / ratio4;
            phio = (model.MOS3phi - model.pbfact1) / model.fact1;
            mos3.MOS3tPhi = fact2 * phio + pbfact;
            mos3.MOS3tVbi = model.MOS3vt0 - model.MOS3type * (model.MOS3gamma * Math.Sqrt(model.MOS3phi)) + .5 * (model.egfet1 - egfet) +
                model.MOS3type * .5 * (mos3.MOS3tPhi - model.MOS3phi);
            mos3.MOS3tVto = mos3.MOS3tVbi + model.MOS3type * model.MOS3gamma * Math.Sqrt(mos3.MOS3tPhi);
            mos3.MOS3tSatCur = model.MOS3jctSatCur * Math.Exp(-egfet / vt + model.egfet1 / model.vtnom);
            mos3.MOS3tSatCurDens = model.MOS3jctSatCurDensity * Math.Exp(-egfet / vt + model.egfet1 / model.vtnom);
            pbo = (model.MOS3bulkJctPotential - model.pbfact1) / model.fact1;
            gmaold = (model.MOS3bulkJctPotential - pbo) / pbo;
            capfact = 1 / (1 + model.MOS3bulkJctBotGradingCoeff * (4e-4 * (model.MOS3tnom - Circuit.CONSTRefTemp) - gmaold));
            mos3.MOS3tCbd = model.MOS3capBD * capfact;
            mos3.MOS3tCbs = model.MOS3capBS * capfact;
            mos3.MOS3tCj = model.MOS3bulkCapFactor * capfact;
            capfact = 1 / (1 + model.MOS3bulkJctSideGradingCoeff * (4e-4 * (model.MOS3tnom - Circuit.CONSTRefTemp) - gmaold));
            mos3.MOS3tCjsw = model.MOS3sideWallCapFactor * capfact;
            mos3.MOS3tBulkPot = fact2 * pbo + pbfact;
            gmanew = (mos3.MOS3tBulkPot - pbo) / pbo;
            capfact = (1 + model.MOS3bulkJctBotGradingCoeff * (4e-4 * (mos3.MOS3temp - Circuit.CONSTRefTemp) - gmanew));
            mos3.MOS3tCbd *= capfact;
            mos3.MOS3tCbs *= capfact;
            mos3.MOS3tCj *= capfact;
            capfact = (1 + model.MOS3bulkJctSideGradingCoeff * (4e-4 * (mos3.MOS3temp - Circuit.CONSTRefTemp) - gmanew));
            mos3.MOS3tCjsw *= capfact;
            mos3.MOS3tDepCap = model.MOS3fwdCapDepCoeff * mos3.MOS3tBulkPot;

            if ((model.MOS3jctSatCurDensity.Value == 0) || (mos3.MOS3drainArea.Value == 0) || (mos3.MOS3sourceArea.Value == 0))
            {
                mos3.MOS3sourceVcrit = mos3.MOS3drainVcrit = vt * Math.Log(vt / (Circuit.CONSTroot2 * model.MOS3jctSatCur));
            }
            else
            {
                mos3.MOS3drainVcrit = vt * Math.Log(vt / (Circuit.CONSTroot2 * model.MOS3jctSatCurDensity * mos3.MOS3drainArea));
                mos3.MOS3sourceVcrit = vt * Math.Log(vt / (Circuit.CONSTroot2 * model.MOS3jctSatCurDensity * mos3.MOS3sourceArea));
            }
            if (model.MOS3capBD.Given)
            {
                czbd = mos3.MOS3tCbd;
            }
            else
            {
                if (model.MOS3bulkCapFactor.Given)
                {
                    czbd = mos3.MOS3tCj * mos3.MOS3drainArea;
                }
                else
                {
                    czbd = 0;
                }
            }
            if (model.MOS3sideWallCapFactor.Given)
            {
                czbdsw = mos3.MOS3tCjsw * mos3.MOS3drainPerimiter;
            }
            else
            {
                czbdsw = 0;
            }
            arg = 1 - model.MOS3fwdCapDepCoeff;
            sarg = Math.Exp((-model.MOS3bulkJctBotGradingCoeff) * Math.Log(arg));
            sargsw = Math.Exp((-model.MOS3bulkJctSideGradingCoeff) * Math.Log(arg));
            mos3.MOS3Cbd = czbd;
            mos3.MOS3Cbdsw = czbdsw;
            mos3.MOS3f2d = czbd * (1 - model.MOS3fwdCapDepCoeff * (1 + model.MOS3bulkJctBotGradingCoeff)) * sarg / arg + czbdsw * (1 -
                model.MOS3fwdCapDepCoeff * (1 + model.MOS3bulkJctSideGradingCoeff)) * sargsw / arg;
            mos3.MOS3f3d = czbd * model.MOS3bulkJctBotGradingCoeff * sarg / arg / model.MOS3bulkJctPotential + czbdsw *
                model.MOS3bulkJctSideGradingCoeff * sargsw / arg / model.MOS3bulkJctPotential;
            mos3.MOS3f4d = czbd * model.MOS3bulkJctPotential * (1 - arg * sarg) / (1 - model.MOS3bulkJctBotGradingCoeff) + czbdsw *
                model.MOS3bulkJctPotential * (1 - arg * sargsw) / (1 - model.MOS3bulkJctSideGradingCoeff) - mos3.MOS3f3d / 2 * (mos3.MOS3tDepCap *
                mos3.MOS3tDepCap) - mos3.MOS3tDepCap * mos3.MOS3f2d;
            if (model.MOS3capBS.Given)
            {
                czbs = mos3.MOS3tCbs;
            }
            else
            {
                if (model.MOS3bulkCapFactor.Given)
                {
                    czbs = mos3.MOS3tCj * mos3.MOS3sourceArea;
                }
                else
                {
                    czbs = 0;
                }
            }
            if (model.MOS3sideWallCapFactor.Given)
            {
                czbssw = mos3.MOS3tCjsw * mos3.MOS3sourcePerimiter;
            }
            else
            {
                czbssw = 0;
            }
            arg = 1 - model.MOS3fwdCapDepCoeff;
            sarg = Math.Exp((-model.MOS3bulkJctBotGradingCoeff) * Math.Log(arg));
            sargsw = Math.Exp((-model.MOS3bulkJctSideGradingCoeff) * Math.Log(arg));
            mos3.MOS3Cbs = czbs;
            mos3.MOS3Cbssw = czbssw;
            mos3.MOS3f2s = czbs * (1 - model.MOS3fwdCapDepCoeff * (1 + model.MOS3bulkJctBotGradingCoeff)) * sarg / arg + czbssw * (1 -
                model.MOS3fwdCapDepCoeff * (1 + model.MOS3bulkJctSideGradingCoeff)) * sargsw / arg;
            mos3.MOS3f3s = czbs * model.MOS3bulkJctBotGradingCoeff * sarg / arg / model.MOS3bulkJctPotential + czbssw *
                model.MOS3bulkJctSideGradingCoeff * sargsw / arg / model.MOS3bulkJctPotential;
            mos3.MOS3f4s = czbs * model.MOS3bulkJctPotential * (1 - arg * sarg) / (1 - model.MOS3bulkJctBotGradingCoeff) + czbssw *
                model.MOS3bulkJctPotential * (1 - arg * sargsw) / (1 - model.MOS3bulkJctSideGradingCoeff) - mos3.MOS3f3s / 2 * (mos3.MOS3tBulkPot *
                mos3.MOS3tBulkPot) - mos3.MOS3tBulkPot * mos3.MOS3f2s;
        }
    }
}
