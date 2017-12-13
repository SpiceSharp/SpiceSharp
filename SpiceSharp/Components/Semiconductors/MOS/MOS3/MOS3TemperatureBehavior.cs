using System;
using SpiceSharp.Diagnostics;
using SpiceSharp.Behaviors;
using SpiceSharp.Parameters;
using SpiceSharp.Circuits;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// Temperature behaviour for a <see cref="mos3.MOS3"/>
    /// </summary>
    public class MOS3TemperatureBehavior : CircuitObjectBehaviorTemperature
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        private MOS3ModelTemperatureBehavior modeltemp;

        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("w"), SpiceInfo("Width")]
        public Parameter MOS3w { get; } = new Parameter(1e-4);
        [SpiceName("l"), SpiceInfo("Length")]
        public Parameter MOS3l { get; } = new Parameter(1e-4);
        [SpiceName("as"), SpiceInfo("Source area")]
        public Parameter MOS3sourceArea { get; } = new Parameter();
        [SpiceName("ad"), SpiceInfo("Drain area")]
        public Parameter MOS3drainArea { get; } = new Parameter();
        [SpiceName("ps"), SpiceInfo("Source perimeter")]
        public Parameter MOS3sourcePerimiter { get; } = new Parameter();
        [SpiceName("pd"), SpiceInfo("Drain perimeter")]
        public Parameter MOS3drainPerimiter { get; } = new Parameter();
        [SpiceName("nrs"), SpiceInfo("Source squares")]
        public Parameter MOS3sourceSquares { get; } = new Parameter(1);
        [SpiceName("nrd"), SpiceInfo("Drain squares")]
        public Parameter MOS3drainSquares { get; } = new Parameter(1);

        [SpiceName("temp"), SpiceInfo("Instance operating temperature")]
        public double MOS3_TEMP
        {
            get => MOS3temp - Circuit.CONSTCtoK;
            set => MOS3temp.Set(value + Circuit.CONSTCtoK);
        }
        public Parameter MOS3temp { get; } = new Parameter();
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
        /// Name
        /// </summary>
        private CircuitIdentifier name;

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        /// <returns></returns>
        public override void Setup(CircuitObject component, Circuit ckt)
        {
            var mos3 = component as MOS3;
            modeltemp = GetBehavior<MOS3ModelTemperatureBehavior>(mos3.Model);
            name = component.Name;
        }

        /// <summary>
        /// Execute behaviour
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Temperature(Circuit ckt)
        {
            double vt, ratio, fact2, kt, egfet, arg, pbfact, ratio4, phio, pbo, gmaold, capfact, gmanew, czbd, czbdsw, sarg, sargsw, czbs,
                czbssw;

            /* perform the parameter defaulting */

            if (!MOS3temp.Given)
            {
                MOS3temp.Value = ckt.State.Temperature;
            }
            vt = MOS3temp * Circuit.CONSTKoverQ;
            ratio = MOS3temp / modeltemp.MOS3tnom;
            fact2 = MOS3temp / Circuit.CONSTRefTemp;
            kt = MOS3temp * Circuit.CONSTBoltz;
            egfet = 1.16 - (7.02e-4 * MOS3temp * MOS3temp) / (MOS3temp + 1108);
            arg = -egfet / (kt + kt) + 1.1150877 / (Circuit.CONSTBoltz * (Circuit.CONSTRefTemp + Circuit.CONSTRefTemp));
            pbfact = -2 * vt * (1.5 * Math.Log(fact2) + Circuit.CHARGE * arg);

            if (modeltemp.MOS3drainResistance.Given)
            {
                if (modeltemp.MOS3drainResistance != 0)
                {
                    MOS3drainConductance = 1 / modeltemp.MOS3drainResistance;
                }
                else
                {
                    MOS3drainConductance = 0;
                }
            }
            else if (modeltemp.MOS3sheetResistance.Given)
            {
                if (modeltemp.MOS3sheetResistance != 0)
                {
                    MOS3drainConductance = 1 / (modeltemp.MOS3sheetResistance * MOS3drainSquares);
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
            if (modeltemp.MOS3sourceResistance.Given)
            {
                if (modeltemp.MOS3sourceResistance != 0)
                {
                    MOS3sourceConductance = 1 / modeltemp.MOS3sourceResistance;
                }
                else
                {
                    MOS3sourceConductance = 0;
                }
            }
            else if (modeltemp.MOS3sheetResistance.Given)
            {
                if (modeltemp.MOS3sheetResistance != 0)
                {
                    MOS3sourceConductance = 1 / (modeltemp.MOS3sheetResistance * MOS3sourceSquares);
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

            if (MOS3l - 2 * modeltemp.MOS3latDiff <= 0)
                throw new CircuitException($"{name}: effective channel length less than zero");
            ratio4 = ratio * Math.Sqrt(ratio);
            MOS3tTransconductance = modeltemp.MOS3transconductance / ratio4;
            MOS3tSurfMob = modeltemp.MOS3surfaceMobility / ratio4;
            phio = (modeltemp.MOS3phi - modeltemp.pbfact1) / modeltemp.fact1;
            MOS3tPhi = fact2 * phio + pbfact;
            MOS3tVbi = modeltemp.MOS3vt0 - modeltemp.MOS3type * (modeltemp.MOS3gamma * Math.Sqrt(modeltemp.MOS3phi)) + .5 * (modeltemp.egfet1 - egfet) +
                modeltemp.MOS3type * .5 * (MOS3tPhi - modeltemp.MOS3phi);
            MOS3tVto = MOS3tVbi + modeltemp.MOS3type * modeltemp.MOS3gamma * Math.Sqrt(MOS3tPhi);
            MOS3tSatCur = modeltemp.MOS3jctSatCur * Math.Exp(-egfet / vt + modeltemp.egfet1 / modeltemp.vtnom);
            MOS3tSatCurDens = modeltemp.MOS3jctSatCurDensity * Math.Exp(-egfet / vt + modeltemp.egfet1 / modeltemp.vtnom);
            pbo = (modeltemp.MOS3bulkJctPotential - modeltemp.pbfact1) / modeltemp.fact1;
            gmaold = (modeltemp.MOS3bulkJctPotential - pbo) / pbo;
            capfact = 1 / (1 + modeltemp.MOS3bulkJctBotGradingCoeff * (4e-4 * (modeltemp.MOS3tnom - Circuit.CONSTRefTemp) - gmaold));
            MOS3tCbd = modeltemp.MOS3capBD * capfact;
            MOS3tCbs = modeltemp.MOS3capBS * capfact;
            MOS3tCj = modeltemp.MOS3bulkCapFactor * capfact;
            capfact = 1 / (1 + modeltemp.MOS3bulkJctSideGradingCoeff * (4e-4 * (modeltemp.MOS3tnom - Circuit.CONSTRefTemp) - gmaold));
            MOS3tCjsw = modeltemp.MOS3sideWallCapFactor * capfact;
            MOS3tBulkPot = fact2 * pbo + pbfact;
            gmanew = (MOS3tBulkPot - pbo) / pbo;
            capfact = (1 + modeltemp.MOS3bulkJctBotGradingCoeff * (4e-4 * (MOS3temp - Circuit.CONSTRefTemp) - gmanew));
            MOS3tCbd *= capfact;
            MOS3tCbs *= capfact;
            MOS3tCj *= capfact;
            capfact = (1 + modeltemp.MOS3bulkJctSideGradingCoeff * (4e-4 * (MOS3temp - Circuit.CONSTRefTemp) - gmanew));
            MOS3tCjsw *= capfact;
            MOS3tDepCap = modeltemp.MOS3fwdCapDepCoeff * MOS3tBulkPot;

            if ((modeltemp.MOS3jctSatCurDensity.Value == 0) || (MOS3drainArea.Value == 0) || (MOS3sourceArea.Value == 0))
            {
                MOS3sourceVcrit = MOS3drainVcrit = vt * Math.Log(vt / (Circuit.CONSTroot2 * modeltemp.MOS3jctSatCur));
            }
            else
            {
                MOS3drainVcrit = vt * Math.Log(vt / (Circuit.CONSTroot2 * modeltemp.MOS3jctSatCurDensity * MOS3drainArea));
                MOS3sourceVcrit = vt * Math.Log(vt / (Circuit.CONSTroot2 * modeltemp.MOS3jctSatCurDensity * MOS3sourceArea));
            }
            if (modeltemp.MOS3capBD.Given)
            {
                czbd = MOS3tCbd;
            }
            else
            {
                if (modeltemp.MOS3bulkCapFactor.Given)
                {
                    czbd = MOS3tCj * MOS3drainArea;
                }
                else
                {
                    czbd = 0;
                }
            }
            if (modeltemp.MOS3sideWallCapFactor.Given)
            {
                czbdsw = MOS3tCjsw * MOS3drainPerimiter;
            }
            else
            {
                czbdsw = 0;
            }
            arg = 1 - modeltemp.MOS3fwdCapDepCoeff;
            sarg = Math.Exp((-modeltemp.MOS3bulkJctBotGradingCoeff) * Math.Log(arg));
            sargsw = Math.Exp((-modeltemp.MOS3bulkJctSideGradingCoeff) * Math.Log(arg));
            MOS3Cbd = czbd;
            MOS3Cbdsw = czbdsw;
            MOS3f2d = czbd * (1 - modeltemp.MOS3fwdCapDepCoeff * (1 + modeltemp.MOS3bulkJctBotGradingCoeff)) * sarg / arg + czbdsw * (1 -
                modeltemp.MOS3fwdCapDepCoeff * (1 + modeltemp.MOS3bulkJctSideGradingCoeff)) * sargsw / arg;
            MOS3f3d = czbd * modeltemp.MOS3bulkJctBotGradingCoeff * sarg / arg / modeltemp.MOS3bulkJctPotential + czbdsw *
                modeltemp.MOS3bulkJctSideGradingCoeff * sargsw / arg / modeltemp.MOS3bulkJctPotential;
            MOS3f4d = czbd * modeltemp.MOS3bulkJctPotential * (1 - arg * sarg) / (1 - modeltemp.MOS3bulkJctBotGradingCoeff) + czbdsw *
                modeltemp.MOS3bulkJctPotential * (1 - arg * sargsw) / (1 - modeltemp.MOS3bulkJctSideGradingCoeff) - MOS3f3d / 2 * (MOS3tDepCap *
                MOS3tDepCap) - MOS3tDepCap * MOS3f2d;
            if (modeltemp.MOS3capBS.Given)
            {
                czbs = MOS3tCbs;
            }
            else
            {
                if (modeltemp.MOS3bulkCapFactor.Given)
                {
                    czbs = MOS3tCj * MOS3sourceArea;
                }
                else
                {
                    czbs = 0;
                }
            }
            if (modeltemp.MOS3sideWallCapFactor.Given)
            {
                czbssw = MOS3tCjsw * MOS3sourcePerimiter;
            }
            else
            {
                czbssw = 0;
            }
            arg = 1 - modeltemp.MOS3fwdCapDepCoeff;
            sarg = Math.Exp((-modeltemp.MOS3bulkJctBotGradingCoeff) * Math.Log(arg));
            sargsw = Math.Exp((-modeltemp.MOS3bulkJctSideGradingCoeff) * Math.Log(arg));
            MOS3Cbs = czbs;
            MOS3Cbssw = czbssw;
            MOS3f2s = czbs * (1 - modeltemp.MOS3fwdCapDepCoeff * (1 + modeltemp.MOS3bulkJctBotGradingCoeff)) * sarg / arg + czbssw * (1 -
                modeltemp.MOS3fwdCapDepCoeff * (1 + modeltemp.MOS3bulkJctSideGradingCoeff)) * sargsw / arg;
            MOS3f3s = czbs * modeltemp.MOS3bulkJctBotGradingCoeff * sarg / arg / modeltemp.MOS3bulkJctPotential + czbssw *
                modeltemp.MOS3bulkJctSideGradingCoeff * sargsw / arg / modeltemp.MOS3bulkJctPotential;
            MOS3f4s = czbs * modeltemp.MOS3bulkJctPotential * (1 - arg * sarg) / (1 - modeltemp.MOS3bulkJctBotGradingCoeff) + czbssw *
                modeltemp.MOS3bulkJctPotential * (1 - arg * sargsw) / (1 - modeltemp.MOS3bulkJctSideGradingCoeff) - MOS3f3s / 2 * (MOS3tBulkPot *
                MOS3tBulkPot) - MOS3tBulkPot * MOS3f2s;
        }
    }
}
