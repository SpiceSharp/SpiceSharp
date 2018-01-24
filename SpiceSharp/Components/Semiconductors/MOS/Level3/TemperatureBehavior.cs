using System;
using SpiceSharp.Diagnostics;
using SpiceSharp.Attributes;
using SpiceSharp.Circuits;
using SpiceSharp.Components.Mosfet.Level3;
using SpiceSharp.Simulations;

namespace SpiceSharp.Behaviors.Mosfet.Level3
{
    /// <summary>
    /// Temperature behavior for a <see cref="Components.MOS3"/>
    /// </summary>
    public class TemperatureBehavior : Behaviors.TemperatureBehavior
    {
        /// <summary>
        /// Necessary behaviors and parameters
        /// </summary>
        BaseParameters bp;
        ModelBaseParameters mbp;
        ModelTemperatureBehavior modeltemp;

        /// <summary>
        /// Shared parameters
        /// </summary>
        [SpiceName("sourceconductance"), SpiceInfo("Source conductance")]
        public double MOS3sourceConductance { get; internal set; }
        [SpiceName("drainconductance"), SpiceInfo("Drain conductance")]
        public double MOS3drainConductance { get; internal set; }
        [SpiceName("sourcevcrit"), SpiceInfo("Critical source voltage")]
        public double MOS3sourceVcrit { get; internal set; }
        [SpiceName("drainvcrit"), SpiceInfo("Critical drain voltage")]
        public double MOS3drainVcrit { get; internal set; }
        [SpiceName("cbd0"), SpiceInfo("Zero-Bias B-D junction capacitance")]
        public double MOS3Cbd { get; internal set; }
        [SpiceName("cbdsw0"), SpiceInfo("Zero-Bias B-D sidewall capacitance")]
        public double MOS3Cbdsw { get; internal set; }
        [SpiceName("cbs0"), SpiceInfo("Zero-Bias B-S junction capacitance")]
        public double MOS3Cbs { get; internal set; }
        [SpiceName("cbssw0"), SpiceInfo("Zero-Bias B-S sidewall capacitance")]
        public double MOS3Cbssw { get; internal set; }

        /// <summary>
        /// Methods
        /// </summary>
        /// <param name="ckt">Circuit</param>
        /// <returns></returns>
        [SpiceName("rs"), SpiceInfo("Source resistance")]
        public double GetSOURCERESIST(Circuit ckt)
        {
            if (MOS3sourceConductance != 0.0)
                return 1.0 / MOS3sourceConductance;
            else
                return 0.0;
        }
        [SpiceName("rd"), SpiceInfo("Drain resistance")]
        public double GetDRAINRESIST(Circuit ckt)
        {
            if (MOS3drainConductance != 0.0)
                return 1.0 / MOS3drainConductance;
            else
                return 0.0;
        }

        /// <summary>
        /// Extra variables
        /// </summary>
        public double MOS3tTransconductance { get; internal set; }
        public double MOS3tSurfMob { get; internal set; }
        public double MOS3tPhi { get; internal set; }
        public double MOS3tVbi { get; internal set; }
        public double MOS3tVto { get; internal set; }
        public double MOS3tSatCur { get; internal set; }
        public double MOS3tSatCurDens { get; internal set; }
        public double MOS3tCbd { get; internal set; }
        public double MOS3tCbs { get; internal set; }
        public double MOS3tCj { get; internal set; }
        public double MOS3tCjsw { get; internal set; }
        public double MOS3tBulkPot { get; internal set; }
        public double MOS3tDepCap { get; internal set; }
        public double MOS3f2d { get; internal set; }
        public double MOS3f3d { get; internal set; }
        public double MOS3f4d { get; internal set; }
        public double MOS3f2s { get; internal set; }
        public double MOS3f3s { get; internal set; }
        public double MOS3f4s { get; internal set; }
        public double MOS3cgs { get; internal set; }
        public double MOS3cgd { get; internal set; }
        public double MOS3cgb { get; internal set; }

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
            var state = sim.State;
            double vt, ratio, fact2, kt, egfet, arg, pbfact, ratio4, phio, pbo, gmaold, capfact, gmanew, czbd, czbdsw, sarg, sargsw, czbs,
                czbssw;

            /* perform the parameter defaulting */

            if (!bp.MOS3temp.Given)
            {
                bp.MOS3temp.Value = state.Temperature;
            }
            vt = bp.MOS3temp * Circuit.CONSTKoverQ;
            ratio = bp.MOS3temp / mbp.MOS3tnom;
            fact2 = bp.MOS3temp / Circuit.CONSTRefTemp;
            kt = bp.MOS3temp * Circuit.CONSTBoltz;
            egfet = 1.16 - (7.02e-4 * bp.MOS3temp * bp.MOS3temp) / (bp.MOS3temp + 1108);
            arg = -egfet / (kt + kt) + 1.1150877 / (Circuit.CONSTBoltz * (Circuit.CONSTRefTemp + Circuit.CONSTRefTemp));
            pbfact = -2 * vt * (1.5 * Math.Log(fact2) + Circuit.CHARGE * arg);

            if (mbp.MOS3drainResistance.Given)
            {
                if (mbp.MOS3drainResistance != 0)
                {
                    MOS3drainConductance = 1 / mbp.MOS3drainResistance;
                }
                else
                {
                    MOS3drainConductance = 0;
                }
            }
            else if (mbp.MOS3sheetResistance.Given)
            {
                if (mbp.MOS3sheetResistance != 0)
                {
                    MOS3drainConductance = 1 / (mbp.MOS3sheetResistance * bp.MOS3drainSquares);
                }
                else
                {
                    MOS3drainConductance = 0;
                }
            }
            else
            {
                MOS3drainConductance = 0;
            }
            if (mbp.MOS3sourceResistance.Given)
            {
                if (mbp.MOS3sourceResistance != 0)
                {
                    MOS3sourceConductance = 1 / mbp.MOS3sourceResistance;
                }
                else
                {
                    MOS3sourceConductance = 0;
                }
            }
            else if (mbp.MOS3sheetResistance.Given)
            {
                if (mbp.MOS3sheetResistance != 0)
                {
                    MOS3sourceConductance = 1 / (mbp.MOS3sheetResistance * bp.MOS3sourceSquares);
                }
                else
                {
                    MOS3sourceConductance = 0;
                }
            }
            else
            {
                MOS3sourceConductance = 0;
            }

            if (bp.MOS3l - 2 * mbp.MOS3latDiff <= 0)
                throw new CircuitException($"{Name}: effective channel length less than zero");
            ratio4 = ratio * Math.Sqrt(ratio);
            MOS3tTransconductance = mbp.MOS3transconductance / ratio4;
            MOS3tSurfMob = mbp.MOS3surfaceMobility / ratio4;
            phio = (mbp.MOS3phi - modeltemp.pbfact1) / modeltemp.fact1;
            MOS3tPhi = fact2 * phio + pbfact;
            MOS3tVbi = mbp.MOS3vt0 - mbp.MOS3type * (mbp.MOS3gamma * Math.Sqrt(mbp.MOS3phi)) + .5 * (modeltemp.egfet1 - egfet) +
                mbp.MOS3type * .5 * (MOS3tPhi - mbp.MOS3phi);
            MOS3tVto = MOS3tVbi + mbp.MOS3type * mbp.MOS3gamma * Math.Sqrt(MOS3tPhi);
            MOS3tSatCur = mbp.MOS3jctSatCur * Math.Exp(-egfet / vt + modeltemp.egfet1 / modeltemp.vtnom);
            MOS3tSatCurDens = mbp.MOS3jctSatCurDensity * Math.Exp(-egfet / vt + modeltemp.egfet1 / modeltemp.vtnom);
            pbo = (mbp.MOS3bulkJctPotential - modeltemp.pbfact1) / modeltemp.fact1;
            gmaold = (mbp.MOS3bulkJctPotential - pbo) / pbo;
            capfact = 1 / (1 + mbp.MOS3bulkJctBotGradingCoeff * (4e-4 * (mbp.MOS3tnom - Circuit.CONSTRefTemp) - gmaold));
            MOS3tCbd = mbp.MOS3capBD * capfact;
            MOS3tCbs = mbp.MOS3capBS * capfact;
            MOS3tCj = mbp.MOS3bulkCapFactor * capfact;
            capfact = 1 / (1 + mbp.MOS3bulkJctSideGradingCoeff * (4e-4 * (mbp.MOS3tnom - Circuit.CONSTRefTemp) - gmaold));
            MOS3tCjsw = mbp.MOS3sideWallCapFactor * capfact;
            MOS3tBulkPot = fact2 * pbo + pbfact;
            gmanew = (MOS3tBulkPot - pbo) / pbo;
            capfact = (1 + mbp.MOS3bulkJctBotGradingCoeff * (4e-4 * (bp.MOS3temp - Circuit.CONSTRefTemp) - gmanew));
            MOS3tCbd *= capfact;
            MOS3tCbs *= capfact;
            MOS3tCj *= capfact;
            capfact = (1 + mbp.MOS3bulkJctSideGradingCoeff * (4e-4 * (bp.MOS3temp - Circuit.CONSTRefTemp) - gmanew));
            MOS3tCjsw *= capfact;
            MOS3tDepCap = mbp.MOS3fwdCapDepCoeff * MOS3tBulkPot;

            if ((mbp.MOS3jctSatCurDensity.Value == 0) || (bp.MOS3drainArea.Value == 0) || (bp.MOS3sourceArea.Value == 0))
            {
                MOS3sourceVcrit = MOS3drainVcrit = vt * Math.Log(vt / (Circuit.CONSTroot2 * mbp.MOS3jctSatCur));
            }
            else
            {
                MOS3drainVcrit = vt * Math.Log(vt / (Circuit.CONSTroot2 * mbp.MOS3jctSatCurDensity * bp.MOS3drainArea));
                MOS3sourceVcrit = vt * Math.Log(vt / (Circuit.CONSTroot2 * mbp.MOS3jctSatCurDensity * bp.MOS3sourceArea));
            }
            if (mbp.MOS3capBD.Given)
            {
                czbd = MOS3tCbd;
            }
            else
            {
                if (mbp.MOS3bulkCapFactor.Given)
                {
                    czbd = MOS3tCj * bp.MOS3drainArea;
                }
                else
                {
                    czbd = 0;
                }
            }
            if (mbp.MOS3sideWallCapFactor.Given)
            {
                czbdsw = MOS3tCjsw * bp.MOS3drainPerimiter;
            }
            else
            {
                czbdsw = 0;
            }
            arg = 1 - mbp.MOS3fwdCapDepCoeff;
            sarg = Math.Exp((-mbp.MOS3bulkJctBotGradingCoeff) * Math.Log(arg));
            sargsw = Math.Exp((-mbp.MOS3bulkJctSideGradingCoeff) * Math.Log(arg));
            MOS3Cbd = czbd;
            MOS3Cbdsw = czbdsw;
            MOS3f2d = czbd * (1 - mbp.MOS3fwdCapDepCoeff * (1 + mbp.MOS3bulkJctBotGradingCoeff)) * sarg / arg + czbdsw * (1 -
                mbp.MOS3fwdCapDepCoeff * (1 + mbp.MOS3bulkJctSideGradingCoeff)) * sargsw / arg;
            MOS3f3d = czbd * mbp.MOS3bulkJctBotGradingCoeff * sarg / arg / mbp.MOS3bulkJctPotential + czbdsw *
                mbp.MOS3bulkJctSideGradingCoeff * sargsw / arg / mbp.MOS3bulkJctPotential;
            MOS3f4d = czbd * mbp.MOS3bulkJctPotential * (1 - arg * sarg) / (1 - mbp.MOS3bulkJctBotGradingCoeff) + czbdsw *
                mbp.MOS3bulkJctPotential * (1 - arg * sargsw) / (1 - mbp.MOS3bulkJctSideGradingCoeff) - MOS3f3d / 2 * (MOS3tDepCap *
                MOS3tDepCap) - MOS3tDepCap * MOS3f2d;
            if (mbp.MOS3capBS.Given)
            {
                czbs = MOS3tCbs;
            }
            else
            {
                if (mbp.MOS3bulkCapFactor.Given)
                {
                    czbs = MOS3tCj * bp.MOS3sourceArea;
                }
                else
                {
                    czbs = 0;
                }
            }
            if (mbp.MOS3sideWallCapFactor.Given)
            {
                czbssw = MOS3tCjsw * bp.MOS3sourcePerimiter;
            }
            else
            {
                czbssw = 0;
            }
            arg = 1 - mbp.MOS3fwdCapDepCoeff;
            sarg = Math.Exp((-mbp.MOS3bulkJctBotGradingCoeff) * Math.Log(arg));
            sargsw = Math.Exp((-mbp.MOS3bulkJctSideGradingCoeff) * Math.Log(arg));
            MOS3Cbs = czbs;
            MOS3Cbssw = czbssw;
            MOS3f2s = czbs * (1 - mbp.MOS3fwdCapDepCoeff * (1 + mbp.MOS3bulkJctBotGradingCoeff)) * sarg / arg + czbssw * (1 -
                mbp.MOS3fwdCapDepCoeff * (1 + mbp.MOS3bulkJctSideGradingCoeff)) * sargsw / arg;
            MOS3f3s = czbs * mbp.MOS3bulkJctBotGradingCoeff * sarg / arg / mbp.MOS3bulkJctPotential + czbssw *
                mbp.MOS3bulkJctSideGradingCoeff * sargsw / arg / mbp.MOS3bulkJctPotential;
            MOS3f4s = czbs * mbp.MOS3bulkJctPotential * (1 - arg * sarg) / (1 - mbp.MOS3bulkJctBotGradingCoeff) + czbssw *
                mbp.MOS3bulkJctPotential * (1 - arg * sargsw) / (1 - mbp.MOS3bulkJctSideGradingCoeff) - MOS3f3s / 2 * (MOS3tBulkPot *
                MOS3tBulkPot) - MOS3tBulkPot * MOS3f2s;
        }
    }
}
