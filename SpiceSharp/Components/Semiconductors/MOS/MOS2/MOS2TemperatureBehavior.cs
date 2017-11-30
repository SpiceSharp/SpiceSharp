using System;
using SpiceSharp.Diagnostics;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// Temperature behaviour for a <see cref="MOS2"/>
    /// </summary>
    public class MOS2TemperatureBehavior : CircuitObjectBehaviorTemperature
    {
        /// <summary>
        /// Execute the behaviour
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Temperature(Circuit ckt)
        {
            var mos2 = ComponentTyped<MOS2>();
            var model = mos2.Model as MOS2Model;
            double vt, ratio, fact2, kt, egfet, arg, pbfact, ratio4, phio, pbo, gmaold, capfact, gmanew, czbd, czbdsw, sarg, sargsw, czbs,
                czbssw;

            /* perform the parameter defaulting */
            if (!mos2.MOS2temp.Given)
            {
                mos2.MOS2temp.Value = ckt.State.Temperature;
            }
            mos2.MOS2mode = 1;
            mos2.MOS2von = 0;

            vt = mos2.MOS2temp * Circuit.CONSTKoverQ;
            ratio = mos2.MOS2temp / model.MOS2tnom;
            fact2 = mos2.MOS2temp / Circuit.CONSTRefTemp;
            kt = mos2.MOS2temp * Circuit.CONSTBoltz;
            egfet = 1.16 - (7.02e-4 * mos2.MOS2temp * mos2.MOS2temp) / (mos2.MOS2temp + 1108);
            arg = -egfet / (kt + kt) + 1.1150877 / (Circuit.CONSTBoltz * (Circuit.CONSTRefTemp + Circuit.CONSTRefTemp));
            pbfact = -2 * vt * (1.5 * Math.Log(fact2) + Circuit.CHARGE * arg);

            if (model.MOS2drainResistance.Given)
            {
                if (model.MOS2drainResistance != 0)
                {
                    mos2.MOS2drainConductance = 1 / model.MOS2drainResistance;
                }
                else
                {
                    mos2.MOS2drainConductance = 0;
                }
            }
            else if (model.MOS2sheetResistance.Given)
            {
                if (model.MOS2sheetResistance != 0)
                {
                    mos2.MOS2drainConductance = 1 / (model.MOS2sheetResistance * mos2.MOS2drainSquares);
                }
                else
                {
                    mos2.MOS2drainConductance = 0;
                }
            }
            else
            {
                mos2.MOS2drainConductance = 0;
            }
            if (model.MOS2sourceResistance.Given)
            {
                if (model.MOS2sourceResistance != 0)
                {
                    mos2.MOS2sourceConductance = 1 / model.MOS2sourceResistance;
                }
                else
                {
                    mos2.MOS2sourceConductance = 0;
                }
            }
            else if (model.MOS2sheetResistance.Given)
            {
                if (model.MOS2sheetResistance != 0)
                {
                    mos2.MOS2sourceConductance = 1 / (model.MOS2sheetResistance * mos2.MOS2sourceSquares);
                }
                else
                {
                    mos2.MOS2sourceConductance = 0;
                }
            }
            else
            {
                mos2.MOS2sourceConductance = 0;
            }
            if (mos2.MOS2l - 2 * model.MOS2latDiff <= 0)
                CircuitWarning.Warning(this, $"{mos2.Name}: effective channel length less than zero");

            ratio4 = ratio * Math.Sqrt(ratio);
            mos2.MOS2tTransconductance = model.MOS2transconductance / ratio4;
            mos2.MOS2tSurfMob = model.MOS2surfaceMobility / ratio4;
            phio = (model.MOS2phi - model.pbfact1) / model.fact1;
            mos2.MOS2tPhi = fact2 * phio + pbfact;
            mos2.MOS2tVbi = model.MOS2vt0 - model.MOS2type * (model.MOS2gamma * Math.Sqrt(model.MOS2phi)) + .5 * (model.egfet1 - egfet) +
                model.MOS2type * .5 * (mos2.MOS2tPhi - model.MOS2phi);
            mos2.MOS2tVto = mos2.MOS2tVbi + model.MOS2type * model.MOS2gamma * Math.Sqrt(mos2.MOS2tPhi);
            mos2.MOS2tSatCur = model.MOS2jctSatCur * Math.Exp(-egfet / vt + model.egfet1 / model.vtnom);
            mos2.MOS2tSatCurDens = model.MOS2jctSatCurDensity * Math.Exp(-egfet / vt + model.egfet1 / model.vtnom);
            pbo = (model.MOS2bulkJctPotential - model.pbfact1) / model.fact1;
            gmaold = (model.MOS2bulkJctPotential - pbo) / pbo;
            capfact = 1 / (1 + model.MOS2bulkJctBotGradingCoeff * (4e-4 * (model.MOS2tnom - Circuit.CONSTRefTemp) - gmaold));
            mos2.MOS2tCbd = model.MOS2capBD * capfact;
            mos2.MOS2tCbs = model.MOS2capBS * capfact;
            mos2.MOS2tCj = model.MOS2bulkCapFactor * capfact;
            capfact = 1 / (1 + model.MOS2bulkJctSideGradingCoeff * (4e-4 * (model.MOS2tnom - Circuit.CONSTRefTemp) - gmaold));
            mos2.MOS2tCjsw = model.MOS2sideWallCapFactor * capfact;
            mos2.MOS2tBulkPot = fact2 * pbo + pbfact;
            gmanew = (mos2.MOS2tBulkPot - pbo) / pbo;
            capfact = (1 + model.MOS2bulkJctBotGradingCoeff * (4e-4 * (mos2.MOS2temp - Circuit.CONSTRefTemp) - gmanew));
            mos2.MOS2tCbd *= capfact;
            mos2.MOS2tCbs *= capfact;
            mos2.MOS2tCj *= capfact;
            capfact = (1 + model.MOS2bulkJctSideGradingCoeff * (4e-4 * (mos2.MOS2temp - Circuit.CONSTRefTemp) - gmanew));
            mos2.MOS2tCjsw *= capfact;
            mos2.MOS2tDepCap = model.MOS2fwdCapDepCoeff * mos2.MOS2tBulkPot;

            if ((mos2.MOS2tSatCurDens == 0) || (mos2.MOS2drainArea.Value == 0) || (mos2.MOS2sourceArea.Value == 0))
            {
                mos2.MOS2sourceVcrit = mos2.MOS2drainVcrit = vt * Math.Log(vt / (Circuit.CONSTroot2 * mos2.MOS2tSatCur));
            }
            else
            {
                mos2.MOS2drainVcrit = vt * Math.Log(vt / (Circuit.CONSTroot2 * mos2.MOS2tSatCurDens * mos2.MOS2drainArea));
                mos2.MOS2sourceVcrit = vt * Math.Log(vt / (Circuit.CONSTroot2 * mos2.MOS2tSatCurDens * mos2.MOS2sourceArea));
            }
            if (model.MOS2capBD.Given)
            {
                czbd = mos2.MOS2tCbd;
            }
            else
            {
                if (model.MOS2bulkCapFactor.Given)
                {
                    czbd = mos2.MOS2tCj * mos2.MOS2drainArea;
                }
                else
                {
                    czbd = 0;
                }
            }
            if (model.MOS2sideWallCapFactor.Given)
            {
                czbdsw = mos2.MOS2tCjsw * mos2.MOS2drainPerimiter;
            }
            else
            {
                czbdsw = 0;
            }
            arg = 1 - model.MOS2fwdCapDepCoeff;
            sarg = Math.Exp((-model.MOS2bulkJctBotGradingCoeff) * Math.Log(arg));
            sargsw = Math.Exp((-model.MOS2bulkJctSideGradingCoeff) * Math.Log(arg));
            mos2.MOS2Cbd = czbd;
            mos2.MOS2Cbdsw = czbdsw;
            mos2.MOS2f2d = czbd * (1 - model.MOS2fwdCapDepCoeff * (1 + model.MOS2bulkJctBotGradingCoeff)) * sarg / arg + czbdsw * (1 -
                model.MOS2fwdCapDepCoeff * (1 + model.MOS2bulkJctSideGradingCoeff)) * sargsw / arg;
            mos2.MOS2f3d = czbd * model.MOS2bulkJctBotGradingCoeff * sarg / arg / mos2.MOS2tBulkPot + czbdsw * model.MOS2bulkJctSideGradingCoeff *
                sargsw / arg / mos2.MOS2tBulkPot;
            mos2.MOS2f4d = czbd * mos2.MOS2tBulkPot * (1 - arg * sarg) / (1 - model.MOS2bulkJctBotGradingCoeff) + czbdsw * mos2.MOS2tBulkPot * (1 - arg *
                sargsw) / (1 - model.MOS2bulkJctSideGradingCoeff) - mos2.MOS2f3d / 2 * (mos2.MOS2tDepCap * mos2.MOS2tDepCap) - mos2.MOS2tDepCap * mos2.MOS2f2d;
            if (model.MOS2capBS.Given)
            {
                czbs = mos2.MOS2tCbs;
            }
            else
            {
                if (model.MOS2bulkCapFactor.Given)
                {
                    czbs = mos2.MOS2tCj * mos2.MOS2sourceArea;
                }
                else
                {
                    czbs = 0;
                }
            }
            if (model.MOS2sideWallCapFactor.Given)
            {
                czbssw = mos2.MOS2tCjsw * mos2.MOS2sourcePerimiter;
            }
            else
            {
                czbssw = 0;
            }
            arg = 1 - model.MOS2fwdCapDepCoeff;
            sarg = Math.Exp((-model.MOS2bulkJctBotGradingCoeff) * Math.Log(arg));
            sargsw = Math.Exp((-model.MOS2bulkJctSideGradingCoeff) * Math.Log(arg));
            mos2.MOS2Cbs = czbs;
            mos2.MOS2Cbssw = czbssw;
            mos2.MOS2f2s = czbs * (1 - model.MOS2fwdCapDepCoeff * (1 + model.MOS2bulkJctBotGradingCoeff)) * sarg / arg + czbssw * (1 -
                model.MOS2fwdCapDepCoeff * (1 + model.MOS2bulkJctSideGradingCoeff)) * sargsw / arg;
            mos2.MOS2f3s = czbs * model.MOS2bulkJctBotGradingCoeff * sarg / arg / mos2.MOS2tBulkPot + czbssw * model.MOS2bulkJctSideGradingCoeff *
                sargsw / arg / mos2.MOS2tBulkPot;
            mos2.MOS2f4s = czbs * mos2.MOS2tBulkPot * (1 - arg * sarg) / (1 - model.MOS2bulkJctBotGradingCoeff) + czbssw * mos2.MOS2tBulkPot * (1 - arg *
                sargsw) / (1 - model.MOS2bulkJctSideGradingCoeff) - mos2.MOS2f3s / 2 * (mos2.MOS2tDepCap * mos2.MOS2tDepCap) - mos2.MOS2tDepCap * mos2.MOS2f2s;
        }
    }
}
