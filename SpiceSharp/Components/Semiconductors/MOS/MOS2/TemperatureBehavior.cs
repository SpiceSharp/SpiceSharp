using System;
using SpiceSharp.Diagnostics;
using SpiceSharp.Parameters;
using SpiceSharp.Circuits;

namespace SpiceSharp.Behaviors.MOS2
{
    /// <summary>
    /// Temperature behavior for a <see cref="Components.MOS2"/>
    /// </summary>
    public class TemperatureBehavior : Behaviors.TemperatureBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        private ModelTemperatureBehavior modeltemp;

        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("temp"), SpiceInfo("Instance operating temperature")]
        public double MOS2_TEMP
        {
            get => MOS2temp - Circuit.CONSTCtoK;
            set => MOS2temp.Set(value + Circuit.CONSTCtoK);
        }
        public Parameter MOS2temp { get; } = new Parameter();
        [SpiceName("w"), SpiceInfo("Width")]
        public Parameter MOS2w { get; } = new Parameter(1e-4);
        [SpiceName("l"), SpiceInfo("Length")]
        public Parameter MOS2l { get; } = new Parameter(1e-4);
        [SpiceName("as"), SpiceInfo("Source area")]
        public Parameter MOS2sourceArea { get; } = new Parameter();
        [SpiceName("ad"), SpiceInfo("Drain area")]
        public Parameter MOS2drainArea { get; } = new Parameter();
        [SpiceName("ps"), SpiceInfo("Source perimeter")]
        public Parameter MOS2sourcePerimiter { get; } = new Parameter();
        [SpiceName("pd"), SpiceInfo("Drain perimeter")]
        public Parameter MOS2drainPerimiter { get; } = new Parameter();
        [SpiceName("nrs"), SpiceInfo("Source squares")]
        public Parameter MOS2sourceSquares { get; } = new Parameter(1);
        [SpiceName("nrd"), SpiceInfo("Drain squares")]
        public Parameter MOS2drainSquares { get; } = new Parameter(1);
        [SpiceName("sourceconductance"), SpiceInfo("Source conductance")]
        public double MOS2sourceConductance { get; internal set; }
        [SpiceName("drainconductance"), SpiceInfo("Drain conductance")]
        public double MOS2drainConductance { get; internal set; }
        [SpiceName("sourcevcrit"), SpiceInfo("Critical source voltage")]
        public double MOS2sourceVcrit { get; internal set; }
        [SpiceName("drainvcrit"), SpiceInfo("Critical drain voltage")]
        public double MOS2drainVcrit { get; internal set; }
        [SpiceName("cbd0"), SpiceInfo("Zero-Bias B-D junction capacitance")]
        public double MOS2Cbd { get; internal set; }
        [SpiceName("cbdsw0"), SpiceInfo(" ")]
        public double MOS2Cbdsw { get; internal set; }
        [SpiceName("cbs0"), SpiceInfo("Zero-Bias B-S junction capacitance")]
        public double MOS2Cbs { get; internal set; }
        [SpiceName("cbssw0"), SpiceInfo(" ")]
        public double MOS2Cbssw { get; internal set; }
        [SpiceName("rs"), SpiceInfo("Source resistance")]
        public double GetSOURCERESIST(Circuit ckt)
        {
            if (MOS2sourceConductance != 0.0)
                return 1.0 / MOS2sourceConductance;
            else
                return 0.0;
        }
        [SpiceName("rd"), SpiceInfo("Drain resistance")]
        public double GetDRAINRESIST(Circuit ckt)
        {
            if (MOS2drainConductance != 0.0)
                return 1.0 / MOS2drainConductance;
            else
                return 0.0;
        }

        /// <summary>
        /// Extra variables
        /// </summary>
        public double MOS2tTransconductance { get; internal set; }
        public double MOS2tSurfMob { get; internal set; }
        public double MOS2tPhi { get; internal set; }
        public double MOS2tVbi { get; internal set; }
        public double MOS2tVto { get; internal set; }
        public double MOS2tSatCur { get; internal set; }
        public double MOS2tSatCurDens { get; internal set; }
        public double MOS2tCbd { get; internal set; }
        public double MOS2tCbs { get; internal set; }
        public double MOS2tCj { get; internal set; }
        public double MOS2tCjsw { get; internal set; }
        public double MOS2tBulkPot { get; internal set; }
        public double MOS2tDepCap { get; internal set; }
        public double MOS2f2d { get; internal set; }
        public double MOS2f3d { get; internal set; }
        public double MOS2f4d { get; internal set; }
        public double MOS2f2s { get; internal set; }
        public double MOS2f3s { get; internal set; }
        public double MOS2f4s { get; internal set; }
        public double MOS2cgs { get; internal set; }
        public double MOS2cgd { get; internal set; }
        public double MOS2cgb { get; internal set; }

        private Identifier name;

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        /// <returns></returns>
        public override void Setup(Entity component, Circuit ckt)
        {
            modeltemp = GetBehavior<ModelTemperatureBehavior>(component);
            name = component.Name;
        }

        /// <summary>
        /// Execute the behavior
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Temperature(Circuit ckt)
        {
            double vt, ratio, fact2, kt, egfet, arg, pbfact, ratio4, phio, pbo, gmaold, capfact, gmanew, czbd, czbdsw, sarg, sargsw, czbs,
                czbssw;

            /* perform the parameter defaulting */
            if (!MOS2temp.Given)
            {
                MOS2temp.Value = ckt.State.Temperature;
            }

            vt = MOS2temp * Circuit.CONSTKoverQ;
            ratio = MOS2temp / modeltemp.MOS2tnom;
            fact2 = MOS2temp / Circuit.CONSTRefTemp;
            kt = MOS2temp * Circuit.CONSTBoltz;
            egfet = 1.16 - (7.02e-4 * MOS2temp * MOS2temp) / (MOS2temp + 1108);
            arg = -egfet / (kt + kt) + 1.1150877 / (Circuit.CONSTBoltz * (Circuit.CONSTRefTemp + Circuit.CONSTRefTemp));
            pbfact = -2 * vt * (1.5 * Math.Log(fact2) + Circuit.CHARGE * arg);

            if (modeltemp.MOS2drainResistance.Given)
            {
                if (modeltemp.MOS2drainResistance != 0)
                {
                    MOS2drainConductance = 1 / modeltemp.MOS2drainResistance;
                }
                else
                {
                    MOS2drainConductance = 0;
                }
            }
            else if (modeltemp.MOS2sheetResistance.Given)
            {
                if (modeltemp.MOS2sheetResistance != 0)
                {
                    MOS2drainConductance = 1 / (modeltemp.MOS2sheetResistance * MOS2drainSquares);
                }
                else
                {
                    MOS2drainConductance = 0;
                }
            }
            else
            {
                MOS2drainConductance = 0;
            }
            if (modeltemp.MOS2sourceResistance.Given)
            {
                if (modeltemp.MOS2sourceResistance != 0)
                {
                    MOS2sourceConductance = 1 / modeltemp.MOS2sourceResistance;
                }
                else
                {
                    MOS2sourceConductance = 0;
                }
            }
            else if (modeltemp.MOS2sheetResistance.Given)
            {
                if (modeltemp.MOS2sheetResistance != 0)
                {
                    MOS2sourceConductance = 1 / (modeltemp.MOS2sheetResistance * MOS2sourceSquares);
                }
                else
                {
                    MOS2sourceConductance = 0;
                }
            }
            else
            {
                MOS2sourceConductance = 0;
            }
            if (MOS2l - 2 * modeltemp.MOS2latDiff <= 0)
                CircuitWarning.Warning(this, $"{name}: effective channel length less than zero");

            ratio4 = ratio * Math.Sqrt(ratio);
            MOS2tTransconductance = modeltemp.MOS2transconductance / ratio4;
            MOS2tSurfMob = modeltemp.MOS2surfaceMobility / ratio4;
            phio = (modeltemp.MOS2phi - modeltemp.pbfact1) / modeltemp.fact1;
            MOS2tPhi = fact2 * phio + pbfact;
            MOS2tVbi = modeltemp.MOS2vt0 - modeltemp.MOS2type * (modeltemp.MOS2gamma * Math.Sqrt(modeltemp.MOS2phi)) + .5 * (modeltemp.egfet1 - egfet) +
                modeltemp.MOS2type * .5 * (MOS2tPhi - modeltemp.MOS2phi);
            MOS2tVto = MOS2tVbi + modeltemp.MOS2type * modeltemp.MOS2gamma * Math.Sqrt(MOS2tPhi);
            MOS2tSatCur = modeltemp.MOS2jctSatCur * Math.Exp(-egfet / vt + modeltemp.egfet1 / modeltemp.vtnom);
            MOS2tSatCurDens = modeltemp.MOS2jctSatCurDensity * Math.Exp(-egfet / vt + modeltemp.egfet1 / modeltemp.vtnom);
            pbo = (modeltemp.MOS2bulkJctPotential - modeltemp.pbfact1) / modeltemp.fact1;
            gmaold = (modeltemp.MOS2bulkJctPotential - pbo) / pbo;
            capfact = 1 / (1 + modeltemp.MOS2bulkJctBotGradingCoeff * (4e-4 * (modeltemp.MOS2tnom - Circuit.CONSTRefTemp) - gmaold));
            MOS2tCbd = modeltemp.MOS2capBD * capfact;
            MOS2tCbs = modeltemp.MOS2capBS * capfact;
            MOS2tCj = modeltemp.MOS2bulkCapFactor * capfact;
            capfact = 1 / (1 + modeltemp.MOS2bulkJctSideGradingCoeff * (4e-4 * (modeltemp.MOS2tnom - Circuit.CONSTRefTemp) - gmaold));
            MOS2tCjsw = modeltemp.MOS2sideWallCapFactor * capfact;
            MOS2tBulkPot = fact2 * pbo + pbfact;
            gmanew = (MOS2tBulkPot - pbo) / pbo;
            capfact = (1 + modeltemp.MOS2bulkJctBotGradingCoeff * (4e-4 * (MOS2temp - Circuit.CONSTRefTemp) - gmanew));
            MOS2tCbd *= capfact;
            MOS2tCbs *= capfact;
            MOS2tCj *= capfact;
            capfact = (1 + modeltemp.MOS2bulkJctSideGradingCoeff * (4e-4 * (MOS2temp - Circuit.CONSTRefTemp) - gmanew));
            MOS2tCjsw *= capfact;
            MOS2tDepCap = modeltemp.MOS2fwdCapDepCoeff * MOS2tBulkPot;

            if ((MOS2tSatCurDens == 0) || (MOS2drainArea.Value == 0) || (MOS2sourceArea.Value == 0))
            {
                MOS2sourceVcrit = MOS2drainVcrit = vt * Math.Log(vt / (Circuit.CONSTroot2 * MOS2tSatCur));
            }
            else
            {
                MOS2drainVcrit = vt * Math.Log(vt / (Circuit.CONSTroot2 * MOS2tSatCurDens * MOS2drainArea));
                MOS2sourceVcrit = vt * Math.Log(vt / (Circuit.CONSTroot2 * MOS2tSatCurDens * MOS2sourceArea));
            }
            if (modeltemp.MOS2capBD.Given)
            {
                czbd = MOS2tCbd;
            }
            else
            {
                if (modeltemp.MOS2bulkCapFactor.Given)
                {
                    czbd = MOS2tCj * MOS2drainArea;
                }
                else
                {
                    czbd = 0;
                }
            }
            if (modeltemp.MOS2sideWallCapFactor.Given)
            {
                czbdsw = MOS2tCjsw * MOS2drainPerimiter;
            }
            else
            {
                czbdsw = 0;
            }
            arg = 1 - modeltemp.MOS2fwdCapDepCoeff;
            sarg = Math.Exp((-modeltemp.MOS2bulkJctBotGradingCoeff) * Math.Log(arg));
            sargsw = Math.Exp((-modeltemp.MOS2bulkJctSideGradingCoeff) * Math.Log(arg));
            MOS2Cbd = czbd;
            MOS2Cbdsw = czbdsw;
            MOS2f2d = czbd * (1 - modeltemp.MOS2fwdCapDepCoeff * (1 + modeltemp.MOS2bulkJctBotGradingCoeff)) * sarg / arg + czbdsw * (1 -
                modeltemp.MOS2fwdCapDepCoeff * (1 + modeltemp.MOS2bulkJctSideGradingCoeff)) * sargsw / arg;
            MOS2f3d = czbd * modeltemp.MOS2bulkJctBotGradingCoeff * sarg / arg / MOS2tBulkPot + czbdsw * modeltemp.MOS2bulkJctSideGradingCoeff *
                sargsw / arg / MOS2tBulkPot;
            MOS2f4d = czbd * MOS2tBulkPot * (1 - arg * sarg) / (1 - modeltemp.MOS2bulkJctBotGradingCoeff) + czbdsw * MOS2tBulkPot * (1 - arg *
                sargsw) / (1 - modeltemp.MOS2bulkJctSideGradingCoeff) - MOS2f3d / 2 * (MOS2tDepCap * MOS2tDepCap) - MOS2tDepCap * MOS2f2d;
            if (modeltemp.MOS2capBS.Given)
            {
                czbs = MOS2tCbs;
            }
            else
            {
                if (modeltemp.MOS2bulkCapFactor.Given)
                {
                    czbs = MOS2tCj * MOS2sourceArea;
                }
                else
                {
                    czbs = 0;
                }
            }
            if (modeltemp.MOS2sideWallCapFactor.Given)
            {
                czbssw = MOS2tCjsw * MOS2sourcePerimiter;
            }
            else
            {
                czbssw = 0;
            }
            arg = 1 - modeltemp.MOS2fwdCapDepCoeff;
            sarg = Math.Exp((-modeltemp.MOS2bulkJctBotGradingCoeff) * Math.Log(arg));
            sargsw = Math.Exp((-modeltemp.MOS2bulkJctSideGradingCoeff) * Math.Log(arg));
            MOS2Cbs = czbs;
            MOS2Cbssw = czbssw;
            MOS2f2s = czbs * (1 - modeltemp.MOS2fwdCapDepCoeff * (1 + modeltemp.MOS2bulkJctBotGradingCoeff)) * sarg / arg + czbssw * (1 -
                modeltemp.MOS2fwdCapDepCoeff * (1 + modeltemp.MOS2bulkJctSideGradingCoeff)) * sargsw / arg;
            MOS2f3s = czbs * modeltemp.MOS2bulkJctBotGradingCoeff * sarg / arg / MOS2tBulkPot + czbssw * modeltemp.MOS2bulkJctSideGradingCoeff *
                sargsw / arg / MOS2tBulkPot;
            MOS2f4s = czbs * MOS2tBulkPot * (1 - arg * sarg) / (1 - modeltemp.MOS2bulkJctBotGradingCoeff) + czbssw * MOS2tBulkPot * (1 - arg *
                sargsw) / (1 - modeltemp.MOS2bulkJctSideGradingCoeff) - MOS2f3s / 2 * (MOS2tDepCap * MOS2tDepCap) - MOS2tDepCap * MOS2f2s;
        }
    }
}
