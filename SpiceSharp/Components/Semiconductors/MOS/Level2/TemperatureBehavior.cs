using System;
using SpiceSharp.Diagnostics;
using SpiceSharp.Circuits;
using SpiceSharp.Attributes;
using SpiceSharp.Components.Mosfet.Level2;
using SpiceSharp.Simulations;

namespace SpiceSharp.Behaviors.Mosfet.Level2
{
    /// <summary>
    /// Temperature behavior for a <see cref="Components.MOS2"/>
    /// </summary>
    public class TemperatureBehavior : Behaviors.TemperatureBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        BaseParameters bp;
        ModelBaseParameters mbp;
        ModelTemperatureBehavior modeltemp;

        /// <summary>
        /// Extra variables
        /// </summary>
        [PropertyNameAttribute("sourceconductance"), PropertyInfoAttribute("Source conductance")]
        public double MOS2sourceConductance { get; internal set; }
        [PropertyNameAttribute("drainconductance"), PropertyInfoAttribute("Drain conductance")]
        public double MOS2drainConductance { get; internal set; }
        [PropertyNameAttribute("sourcevcrit"), PropertyInfoAttribute("Critical source voltage")]
        public double MOS2sourceVcrit { get; internal set; }
        [PropertyNameAttribute("drainvcrit"), PropertyInfoAttribute("Critical drain voltage")]
        public double MOS2drainVcrit { get; internal set; }
        [PropertyNameAttribute("cbd0"), PropertyInfoAttribute("Zero-Bias B-D junction capacitance")]
        public double MOS2Cbd { get; internal set; }
        [PropertyNameAttribute("cbdsw0"), PropertyInfoAttribute(" ")]
        public double MOS2Cbdsw { get; internal set; }
        [PropertyNameAttribute("cbs0"), PropertyInfoAttribute("Zero-Bias B-S junction capacitance")]
        public double MOS2Cbs { get; internal set; }
        [PropertyNameAttribute("cbssw0"), PropertyInfoAttribute(" ")]
        public double MOS2Cbssw { get; internal set; }
        [PropertyNameAttribute("rs"), PropertyInfoAttribute("Source resistance")]
        public double GetSOURCERESIST()
        {
            if (MOS2sourceConductance > 0.0)
                return 1.0 / MOS2sourceConductance;
            return 0.0;
        }
        [PropertyNameAttribute("rd"), PropertyInfoAttribute("Drain resistance")]
        public double GetDRAINRESIST()
        {
            if (MOS2drainConductance > 0.0)
                return 1.0 / MOS2drainConductance;
            return 0.0;
        }
        public double MOS2tTransconductance { get; protected set; }
        public double MOS2tSurfMob { get; protected set; }
        public double MOS2tPhi { get; protected set; }
        public double MOS2tVbi { get; protected set; }
        public double MOS2tVto { get; protected set; }
        public double MOS2tSatCur { get; protected set; }
        public double MOS2tSatCurDens { get; protected set; }
        public double MOS2tCbd { get; protected set; }
        public double MOS2tCbs { get; protected set; }
        public double MOS2tCj { get; protected set; }
        public double MOS2tCjsw { get; protected set; }
        public double MOS2tBulkPot { get; protected set; }
        public double MOS2tDepCap { get; protected set; }
        public double MOS2f2d { get; protected set; }
        public double MOS2f3d { get; protected set; }
        public double MOS2f4d { get; protected set; }
        public double MOS2f2s { get; protected set; }
        public double MOS2f3s { get; protected set; }
        public double MOS2f4s { get; protected set; }
        public double MOS2cgs { get; protected set; }
        public double MOS2cgd { get; protected set; }
        public double MOS2cgb { get; protected set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public TemperatureBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Setup behavior
        /// </summary>
        /// <param name="provider">Data provider</param>
        public override void Setup(SetupDataProvider provider)
        {
            // Get parameters
            bp = provider.GetParameters<BaseParameters>();
            mbp = provider.GetParameters<ModelBaseParameters>(1);

            // Get behaviors
            modeltemp = provider.GetBehavior<ModelTemperatureBehavior>(1);
        }

        /// <summary>
        /// Do temperature-dependent calculations
        /// </summary>
        /// <param name="sim">Base simulation</param>
        public override void Temperature(BaseSimulation sim)
        {
            double vt, ratio, fact2, kt, egfet, arg, pbfact, ratio4, phio, pbo, gmaold, capfact, gmanew, czbd, czbdsw, sarg, sargsw, czbs,
                czbssw;

            /* perform the parameter defaulting */
            if (!bp.MOS2temp.Given)
            {
                bp.MOS2temp.Value = sim.State.Temperature;
            }

            vt = bp.MOS2temp * Circuit.CONSTKoverQ;
            ratio = bp.MOS2temp / mbp.MOS2tnom;
            fact2 = bp.MOS2temp / Circuit.CONSTRefTemp;
            kt = bp.MOS2temp * Circuit.CONSTBoltz;
            egfet = 1.16 - (7.02e-4 * bp.MOS2temp * bp.MOS2temp) / (bp.MOS2temp + 1108);
            arg = -egfet / (kt + kt) + 1.1150877 / (Circuit.CONSTBoltz * (Circuit.CONSTRefTemp + Circuit.CONSTRefTemp));
            pbfact = -2 * vt * (1.5 * Math.Log(fact2) + Circuit.CHARGE * arg);

            if (mbp.MOS2drainResistance.Given)
            {
                if (mbp.MOS2drainResistance != 0)
                {
                    MOS2drainConductance = 1 / mbp.MOS2drainResistance;
                }
                else
                {
                    MOS2drainConductance = 0;
                }
            }
            else if (mbp.MOS2sheetResistance.Given)
            {
                if (mbp.MOS2sheetResistance != 0)
                {
                    MOS2drainConductance = 1 / (mbp.MOS2sheetResistance * bp.MOS2drainSquares);
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
            if (mbp.MOS2sourceResistance.Given)
            {
                if (mbp.MOS2sourceResistance != 0)
                {
                    MOS2sourceConductance = 1 / mbp.MOS2sourceResistance;
                }
                else
                {
                    MOS2sourceConductance = 0;
                }
            }
            else if (mbp.MOS2sheetResistance.Given)
            {
                if (mbp.MOS2sheetResistance != 0)
                {
                    MOS2sourceConductance = 1 / (mbp.MOS2sheetResistance * bp.MOS2sourceSquares);
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
            if (bp.MOS2l - 2 * mbp.MOS2latDiff <= 0)
                CircuitWarning.Warning(this, $"{Name}: effective channel length less than zero");

            ratio4 = ratio * Math.Sqrt(ratio);
            MOS2tTransconductance = mbp.MOS2transconductance / ratio4;
            MOS2tSurfMob = mbp.MOS2surfaceMobility / ratio4;
            phio = (mbp.MOS2phi - modeltemp.pbfact1) / modeltemp.fact1;
            MOS2tPhi = fact2 * phio + pbfact;
            MOS2tVbi = mbp.MOS2vt0 - mbp.MOS2type * (mbp.MOS2gamma * Math.Sqrt(mbp.MOS2phi)) + .5 * (modeltemp.egfet1 - egfet) +
                mbp.MOS2type * .5 * (MOS2tPhi - mbp.MOS2phi);
            MOS2tVto = MOS2tVbi + mbp.MOS2type * mbp.MOS2gamma * Math.Sqrt(MOS2tPhi);
            MOS2tSatCur = mbp.MOS2jctSatCur * Math.Exp(-egfet / vt + modeltemp.egfet1 / modeltemp.vtnom);
            MOS2tSatCurDens = mbp.MOS2jctSatCurDensity * Math.Exp(-egfet / vt + modeltemp.egfet1 / modeltemp.vtnom);
            pbo = (mbp.MOS2bulkJctPotential - modeltemp.pbfact1) / modeltemp.fact1;
            gmaold = (mbp.MOS2bulkJctPotential - pbo) / pbo;
            capfact = 1 / (1 + mbp.MOS2bulkJctBotGradingCoeff * (4e-4 * (mbp.MOS2tnom - Circuit.CONSTRefTemp) - gmaold));
            MOS2tCbd = mbp.MOS2capBD * capfact;
            MOS2tCbs = mbp.MOS2capBS * capfact;
            MOS2tCj = mbp.MOS2bulkCapFactor * capfact;
            capfact = 1 / (1 + mbp.MOS2bulkJctSideGradingCoeff * (4e-4 * (mbp.MOS2tnom - Circuit.CONSTRefTemp) - gmaold));
            MOS2tCjsw = mbp.MOS2sideWallCapFactor * capfact;
            MOS2tBulkPot = fact2 * pbo + pbfact;
            gmanew = (MOS2tBulkPot - pbo) / pbo;
            capfact = (1 + mbp.MOS2bulkJctBotGradingCoeff * (4e-4 * (bp.MOS2temp - Circuit.CONSTRefTemp) - gmanew));
            MOS2tCbd *= capfact;
            MOS2tCbs *= capfact;
            MOS2tCj *= capfact;
            capfact = (1 + mbp.MOS2bulkJctSideGradingCoeff * (4e-4 * (bp.MOS2temp - Circuit.CONSTRefTemp) - gmanew));
            MOS2tCjsw *= capfact;
            MOS2tDepCap = mbp.MOS2fwdCapDepCoeff * MOS2tBulkPot;

            if ((MOS2tSatCurDens == 0) || (bp.MOS2drainArea.Value == 0) || (bp.MOS2sourceArea.Value == 0))
            {
                MOS2sourceVcrit = MOS2drainVcrit = vt * Math.Log(vt / (Circuit.CONSTroot2 * MOS2tSatCur));
            }
            else
            {
                MOS2drainVcrit = vt * Math.Log(vt / (Circuit.CONSTroot2 * MOS2tSatCurDens * bp.MOS2drainArea));
                MOS2sourceVcrit = vt * Math.Log(vt / (Circuit.CONSTroot2 * MOS2tSatCurDens * bp.MOS2sourceArea));
            }
            if (mbp.MOS2capBD.Given)
            {
                czbd = MOS2tCbd;
            }
            else
            {
                if (mbp.MOS2bulkCapFactor.Given)
                {
                    czbd = MOS2tCj * bp.MOS2drainArea;
                }
                else
                {
                    czbd = 0;
                }
            }
            if (mbp.MOS2sideWallCapFactor.Given)
            {
                czbdsw = MOS2tCjsw * bp.MOS2drainPerimiter;
            }
            else
            {
                czbdsw = 0;
            }
            arg = 1 - mbp.MOS2fwdCapDepCoeff;
            sarg = Math.Exp((-mbp.MOS2bulkJctBotGradingCoeff) * Math.Log(arg));
            sargsw = Math.Exp((-mbp.MOS2bulkJctSideGradingCoeff) * Math.Log(arg));
            MOS2Cbd = czbd;
            MOS2Cbdsw = czbdsw;
            MOS2f2d = czbd * (1 - mbp.MOS2fwdCapDepCoeff * (1 + mbp.MOS2bulkJctBotGradingCoeff)) * sarg / arg + czbdsw * (1 -
                mbp.MOS2fwdCapDepCoeff * (1 + mbp.MOS2bulkJctSideGradingCoeff)) * sargsw / arg;
            MOS2f3d = czbd * mbp.MOS2bulkJctBotGradingCoeff * sarg / arg / MOS2tBulkPot + czbdsw * mbp.MOS2bulkJctSideGradingCoeff *
                sargsw / arg / MOS2tBulkPot;
            MOS2f4d = czbd * MOS2tBulkPot * (1 - arg * sarg) / (1 - mbp.MOS2bulkJctBotGradingCoeff) + czbdsw * MOS2tBulkPot * (1 - arg *
                sargsw) / (1 - mbp.MOS2bulkJctSideGradingCoeff) - MOS2f3d / 2 * (MOS2tDepCap * MOS2tDepCap) - MOS2tDepCap * MOS2f2d;
            if (mbp.MOS2capBS.Given)
            {
                czbs = MOS2tCbs;
            }
            else
            {
                if (mbp.MOS2bulkCapFactor.Given)
                {
                    czbs = MOS2tCj * bp.MOS2sourceArea;
                }
                else
                {
                    czbs = 0;
                }
            }
            if (mbp.MOS2sideWallCapFactor.Given)
            {
                czbssw = MOS2tCjsw * bp.MOS2sourcePerimiter;
            }
            else
            {
                czbssw = 0;
            }
            arg = 1 - mbp.MOS2fwdCapDepCoeff;
            sarg = Math.Exp((-mbp.MOS2bulkJctBotGradingCoeff) * Math.Log(arg));
            sargsw = Math.Exp((-mbp.MOS2bulkJctSideGradingCoeff) * Math.Log(arg));
            MOS2Cbs = czbs;
            MOS2Cbssw = czbssw;
            MOS2f2s = czbs * (1 - mbp.MOS2fwdCapDepCoeff * (1 + mbp.MOS2bulkJctBotGradingCoeff)) * sarg / arg + czbssw * (1 -
                mbp.MOS2fwdCapDepCoeff * (1 + mbp.MOS2bulkJctSideGradingCoeff)) * sargsw / arg;
            MOS2f3s = czbs * mbp.MOS2bulkJctBotGradingCoeff * sarg / arg / MOS2tBulkPot + czbssw * mbp.MOS2bulkJctSideGradingCoeff *
                sargsw / arg / MOS2tBulkPot;
            MOS2f4s = czbs * MOS2tBulkPot * (1 - arg * sarg) / (1 - mbp.MOS2bulkJctBotGradingCoeff) + czbssw * MOS2tBulkPot * (1 - arg *
                sargsw) / (1 - mbp.MOS2bulkJctSideGradingCoeff) - MOS2f3s / 2 * (MOS2tDepCap * MOS2tDepCap) - MOS2tDepCap * MOS2f2s;
        }
    }
}
