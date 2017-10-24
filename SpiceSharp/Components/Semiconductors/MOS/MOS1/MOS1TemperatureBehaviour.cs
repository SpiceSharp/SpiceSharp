using System;
using SpiceSharp.Diagnostics;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// Temperature behaviour for a <see cref="MOS1"/>
    /// </summary>
    public class MOS1TemperatureBehavior : CircuitObjectBehaviorTemperature
    {
        /// <summary>
        /// Execute behaviour
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Execute(Circuit ckt)
        {
            var mos1 = ComponentTyped<MOS1>();
            var model = mos1.Model as MOS1Model;
            double vt, ratio, fact2, kt, egfet, arg, pbfact, ratio4, phio, pbo, gmaold, capfact, gmanew, czbd, czbdsw, sarg, sargsw, czbs,
                czbssw;

            /* perform the parameter defaulting */
            if (!mos1.MOS1temp.Given)
            {
                mos1.MOS1temp.Value = ckt.State.Temperature;
            }
            vt = mos1.MOS1temp * Circuit.CONSTKoverQ;
            ratio = mos1.MOS1temp / model.MOS1tnom;
            fact2 = mos1.MOS1temp / Circuit.CONSTRefTemp;
            kt = mos1.MOS1temp * Circuit.CONSTBoltz;
            egfet = 1.16 - (7.02e-4 * mos1.MOS1temp * mos1.MOS1temp) / (mos1.MOS1temp + 1108);
            arg = -egfet / (kt + kt) + 1.1150877 / (Circuit.CONSTBoltz * (Circuit.CONSTRefTemp + Circuit.CONSTRefTemp));
            pbfact = -2 * vt * (1.5 * Math.Log(fact2) + Circuit.CHARGE * arg);

            if (mos1.MOS1l - 2 * model.MOS1latDiff <= 0)
                CircuitWarning.Warning(this, $"{mos1.Name}: effective channel length less than zero");
            ratio4 = ratio * Math.Sqrt(ratio);
            mos1.MOS1tTransconductance = model.MOS1transconductance / ratio4;
            mos1.MOS1tSurfMob = model.MOS1surfaceMobility / ratio4;
            phio = (model.MOS1phi - model.pbfact1) / model.fact1;
            mos1.MOS1tPhi = fact2 * phio + pbfact;
            mos1.MOS1tVbi = model.MOS1vt0 - model.MOS1type * (model.MOS1gamma * Math.Sqrt(model.MOS1phi)) + .5 * (model.egfet1 - egfet) +
                model.MOS1type * .5 * (mos1.MOS1tPhi - model.MOS1phi);
            mos1.MOS1tVto = mos1.MOS1tVbi + model.MOS1type * model.MOS1gamma * Math.Sqrt(mos1.MOS1tPhi);
            mos1.MOS1tSatCur = model.MOS1jctSatCur * Math.Exp(-egfet / vt + model.egfet1 / model.vtnom);
            mos1.MOS1tSatCurDens = model.MOS1jctSatCurDensity * Math.Exp(-egfet / vt + model.egfet1 / model.vtnom);
            pbo = (model.MOS1bulkJctPotential - model.pbfact1) / model.fact1;
            gmaold = (model.MOS1bulkJctPotential - pbo) / pbo;
            capfact = 1 / (1 + model.MOS1bulkJctBotGradingCoeff * (4e-4 * (model.MOS1tnom - Circuit.CONSTRefTemp) - gmaold));
            mos1.MOS1tCbd = model.MOS1capBD * capfact;
            mos1.MOS1tCbs = model.MOS1capBS * capfact;
            mos1.MOS1tCj = model.MOS1bulkCapFactor * capfact;
            capfact = 1 / (1 + model.MOS1bulkJctSideGradingCoeff * (4e-4 * (model.MOS1tnom - Circuit.CONSTRefTemp) - gmaold));
            mos1.MOS1tCjsw = model.MOS1sideWallCapFactor * capfact;
            mos1.MOS1tBulkPot = fact2 * pbo + pbfact;
            gmanew = (mos1.MOS1tBulkPot - pbo) / pbo;
            capfact = (1 + model.MOS1bulkJctBotGradingCoeff * (4e-4 * (mos1.MOS1temp - Circuit.CONSTRefTemp) - gmanew));
            mos1.MOS1tCbd *= capfact;
            mos1.MOS1tCbs *= capfact;
            mos1.MOS1tCj *= capfact;
            capfact = (1 + model.MOS1bulkJctSideGradingCoeff * (4e-4 * (mos1.MOS1temp - Circuit.CONSTRefTemp) - gmanew));
            mos1.MOS1tCjsw *= capfact;
            mos1.MOS1tDepCap = model.MOS1fwdCapDepCoeff * mos1.MOS1tBulkPot;
            if ((mos1.MOS1tSatCurDens == 0) || (mos1.MOS1drainArea.Value == 0) || (mos1.MOS1sourceArea.Value == 0))
            {
                mos1.MOS1sourceVcrit = mos1.MOS1drainVcrit = vt * Math.Log(vt / (Circuit.CONSTroot2 * mos1.MOS1tSatCur));
            }
            else
            {
                mos1.MOS1drainVcrit = vt * Math.Log(vt / (Circuit.CONSTroot2 * mos1.MOS1tSatCurDens * mos1.MOS1drainArea));
                mos1.MOS1sourceVcrit = vt * Math.Log(vt / (Circuit.CONSTroot2 * mos1.MOS1tSatCurDens * mos1.MOS1sourceArea));
            }

            if (model.MOS1capBD.Given)
            {
                czbd = mos1.MOS1tCbd;
            }
            else
            {
                if (model.MOS1bulkCapFactor.Given)
                {
                    czbd = mos1.MOS1tCj * mos1.MOS1drainArea;
                }
                else
                {
                    czbd = 0;
                }
            }
            if (model.MOS1sideWallCapFactor.Given)
            {
                czbdsw = mos1.MOS1tCjsw * mos1.MOS1drainPerimiter;
            }
            else
            {
                czbdsw = 0;
            }
            arg = 1 - model.MOS1fwdCapDepCoeff;
            sarg = Math.Exp((-model.MOS1bulkJctBotGradingCoeff) * Math.Log(arg));
            sargsw = Math.Exp((-model.MOS1bulkJctSideGradingCoeff) * Math.Log(arg));
            mos1.MOS1Cbd = czbd;
            mos1.MOS1Cbdsw = czbdsw;
            mos1.MOS1f2d = czbd * (1 - model.MOS1fwdCapDepCoeff * (1 + model.MOS1bulkJctBotGradingCoeff)) * sarg / arg + czbdsw * (1 -
                model.MOS1fwdCapDepCoeff * (1 + model.MOS1bulkJctSideGradingCoeff)) * sargsw / arg;
            mos1.MOS1f3d = czbd * model.MOS1bulkJctBotGradingCoeff * sarg / arg / mos1.MOS1tBulkPot + czbdsw * model.MOS1bulkJctSideGradingCoeff *
                sargsw / arg / mos1.MOS1tBulkPot;
            mos1.MOS1f4d = czbd * mos1.MOS1tBulkPot * (1 - arg * sarg) / (1 - model.MOS1bulkJctBotGradingCoeff) + czbdsw * mos1.MOS1tBulkPot * (1 - arg *
                sargsw) / (1 - model.MOS1bulkJctSideGradingCoeff) - mos1.MOS1f3d / 2 * (mos1.MOS1tDepCap * mos1.MOS1tDepCap) - mos1.MOS1tDepCap * mos1.MOS1f2d;
            if (model.MOS1capBS.Given)
            {
                czbs = mos1.MOS1tCbs;
            }
            else
            {
                if (model.MOS1bulkCapFactor.Given)
                {
                    czbs = mos1.MOS1tCj * mos1.MOS1sourceArea;
                }
                else
                {
                    czbs = 0;
                }
            }
            if (model.MOS1sideWallCapFactor.Given)
            {
                czbssw = mos1.MOS1tCjsw * mos1.MOS1sourcePerimiter;
            }
            else
            {
                czbssw = 0;
            }
            arg = 1 - model.MOS1fwdCapDepCoeff;
            sarg = Math.Exp((-model.MOS1bulkJctBotGradingCoeff) * Math.Log(arg));
            sargsw = Math.Exp((-model.MOS1bulkJctSideGradingCoeff) * Math.Log(arg));
            mos1.MOS1Cbs = czbs;
            mos1.MOS1Cbssw = czbssw;
            mos1.MOS1f2s = czbs * (1 - model.MOS1fwdCapDepCoeff * (1 + model.MOS1bulkJctBotGradingCoeff)) * sarg / arg + czbssw * (1 -
                model.MOS1fwdCapDepCoeff * (1 + model.MOS1bulkJctSideGradingCoeff)) * sargsw / arg;
            mos1.MOS1f3s = czbs * model.MOS1bulkJctBotGradingCoeff * sarg / arg / mos1.MOS1tBulkPot + czbssw * model.MOS1bulkJctSideGradingCoeff *
                sargsw / arg / mos1.MOS1tBulkPot;
            mos1.MOS1f4s = czbs * mos1.MOS1tBulkPot * (1 - arg * sarg) / (1 - model.MOS1bulkJctBotGradingCoeff) + czbssw * mos1.MOS1tBulkPot * (1 - arg *
                sargsw) / (1 - model.MOS1bulkJctSideGradingCoeff) - mos1.MOS1f3s / 2 * (mos1.MOS1tDepCap * mos1.MOS1tDepCap) - mos1.MOS1tDepCap * mos1.MOS1f2s;

            if (model.MOS1drainResistance.Given)
            {
                if (model.MOS1drainResistance != 0)
                {
                    mos1.MOS1drainConductance = 1 / model.MOS1drainResistance;
                }
                else
                {
                    mos1.MOS1drainConductance = 0;
                }
            }
            else if (model.MOS1sheetResistance.Given)
            {
                if (model.MOS1sheetResistance != 0)
                {
                    mos1.MOS1drainConductance = 1 / (model.MOS1sheetResistance * mos1.MOS1drainSquares);
                }
                else
                {
                    mos1.MOS1drainConductance = 0;
                }
            }
            else
            {
                mos1.MOS1drainConductance = 0;
            }
            if (model.MOS1sourceResistance.Given)
            {
                if (model.MOS1sourceResistance != 0)
                {
                    mos1.MOS1sourceConductance = 1 / model.MOS1sourceResistance;
                }
                else
                {
                    mos1.MOS1sourceConductance = 0;
                }
            }
            else if (model.MOS1sheetResistance.Given)
            {
                if (model.MOS1sheetResistance != 0)
                {
                    mos1.MOS1sourceConductance = 1 / (model.MOS1sheetResistance * mos1.MOS1sourceSquares);
                }
                else
                {
                    mos1.MOS1sourceConductance = 0;
                }
            }
            else
            {
                mos1.MOS1sourceConductance = 0;
            }
        }
    }
}
