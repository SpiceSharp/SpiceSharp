using System;
using SpiceSharp.Diagnostics;
using SpiceSharp.Behaviors;
using SpiceSharp.Parameters;
using SpiceSharp.Circuits;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// Temperature behaviour for a <see cref="MOS1"/>
    /// </summary>
    public class MOS1TemperatureBehavior : CircuitObjectBehaviorTemperature
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        private MOS1ModelTemperatureBehavior modeltemp;

        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("temp"), SpiceInfo("Instance temperature")]
        public double MOS1_TEMP
        {
            get => MOS1temp - Circuit.CONSTCtoK;
            set => MOS1temp.Set(value + Circuit.CONSTCtoK);
        }
        public Parameter MOS1temp { get; } = new Parameter();
        [SpiceName("w"), SpiceInfo("Width")]
        public Parameter MOS1w { get; } = new Parameter(1e-4);
        [SpiceName("l"), SpiceInfo("Length")]
        public Parameter MOS1l { get; } = new Parameter(1e-4);
        [SpiceName("as"), SpiceInfo("Source area")]
        public Parameter MOS1sourceArea { get; } = new Parameter();
        [SpiceName("ad"), SpiceInfo("Drain area")]
        public Parameter MOS1drainArea { get; } = new Parameter();
        [SpiceName("ps"), SpiceInfo("Source perimeter")]
        public Parameter MOS1sourcePerimiter { get; } = new Parameter();
        [SpiceName("pd"), SpiceInfo("Drain perimeter")]
        public Parameter MOS1drainPerimiter { get; } = new Parameter();
        [SpiceName("nrs"), SpiceInfo("Source squares")]
        public Parameter MOS1sourceSquares { get; } = new Parameter(1);
        [SpiceName("nrd"), SpiceInfo("Drain squares")]
        public Parameter MOS1drainSquares { get; } = new Parameter(1);
        [SpiceName("sourcevcrit"), SpiceInfo("Critical source voltage")]
        public double MOS1sourceVcrit { get; protected set; }
        [SpiceName("drainvcrit"), SpiceInfo("Critical drain voltage")]
        public double MOS1drainVcrit { get; protected set; }
        [SpiceName("sourceconductance"), SpiceInfo("Conductance of source")]
        public double MOS1sourceConductance { get; protected set; }
        [SpiceName("drainconductance"), SpiceInfo("Conductance of drain")]
        public double MOS1drainConductance { get; protected set; }
        [SpiceName("cbd0"), SpiceInfo("Zero-Bias B-D junction capacitance")]
        public double MOS1Cbd { get; protected set; }
        [SpiceName("cbdsw0"), SpiceInfo(" ")]
        public double MOS1Cbdsw { get; protected set; }
        [SpiceName("cbs0"), SpiceInfo("Zero-Bias B-S junction capacitance")]
        public double MOS1Cbs { get; protected set; }
        [SpiceName("cbssw0"), SpiceInfo(" ")]
        public double MOS1Cbssw { get; protected set; }

        [SpiceName("rs"), SpiceInfo("Source resistance")]
        public double GetSOURCERESIST(Circuit ckt)
        {
            if (MOS1sourceConductance != 0.0)
                return 1.0 / MOS1sourceConductance;
            else
                return 0.0;
        }
        [SpiceName("rd"), SpiceInfo("Drain conductance")]
        public double GetDRAINRESIST(Circuit ckt)
        {
            if (MOS1drainConductance != 0.0)
                return 1.0 / MOS1drainConductance;
            else
                return 0.0;
        }

        /// <summary>
        /// Extra variables
        /// </summary>
        public double MOS1tTransconductance { get; internal set; }
        public double MOS1tSurfMob { get; internal set; }
        public double MOS1tPhi { get; internal set; }
        public double MOS1tVbi { get; internal set; }
        public double MOS1tVto { get; internal set; }
        public double MOS1tSatCur { get; internal set; }
        public double MOS1tSatCurDens { get; internal set; }
        public double MOS1tCbd { get; internal set; }
        public double MOS1tCbs { get; internal set; }
        public double MOS1tCj { get; internal set; }
        public double MOS1tCjsw { get; internal set; }
        public double MOS1tBulkPot { get; internal set; }
        public double MOS1tDepCap { get; internal set; }
        public double MOS1f2d { get; internal set; }
        public double MOS1f3d { get; internal set; }
        public double MOS1f4d { get; internal set; }
        public double MOS1f2s { get; internal set; }
        public double MOS1f3s { get; internal set; }
        public double MOS1f4s { get; internal set; }
        public double MOS1cgs { get; internal set; }
        public double MOS1cgd { get; internal set; }
        public double MOS1cgb { get; internal set; }

        private CircuitIdentifier name;

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        /// <returns></returns>
        public override bool Setup(CircuitObject component, Circuit ckt)
        {
            var mos1 = component as MOS1;
            var model = mos1.Model as MOS1Model;

            // Get necessary behaviors
            modeltemp = model.GetBehavior(typeof(CircuitObjectBehaviorTemperature)) as MOS1ModelTemperatureBehavior;

            name = component.Name;
            return true;
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
            if (!MOS1temp.Given)
            {
                MOS1temp.Value = ckt.State.Temperature;
            }
            vt = MOS1temp * Circuit.CONSTKoverQ;
            ratio = MOS1temp / modeltemp.MOS1tnom;
            fact2 = MOS1temp / Circuit.CONSTRefTemp;
            kt = MOS1temp * Circuit.CONSTBoltz;
            egfet = 1.16 - (7.02e-4 * MOS1temp * MOS1temp) / (MOS1temp + 1108);
            arg = -egfet / (kt + kt) + 1.1150877 / (Circuit.CONSTBoltz * (Circuit.CONSTRefTemp + Circuit.CONSTRefTemp));
            pbfact = -2 * vt * (1.5 * Math.Log(fact2) + Circuit.CHARGE * arg);

            if (MOS1l - 2 * modeltemp.MOS1latDiff <= 0)
                CircuitWarning.Warning(this, $"{name}: effective channel length less than zero");
            ratio4 = ratio * Math.Sqrt(ratio);
            MOS1tTransconductance = modeltemp.MOS1transconductance / ratio4;
            MOS1tSurfMob = modeltemp.MOS1surfaceMobility / ratio4;
            phio = (modeltemp.MOS1phi - modeltemp.pbfact1) / modeltemp.fact1;
            MOS1tPhi = fact2 * phio + pbfact;
            MOS1tVbi = modeltemp.MOS1vt0 - modeltemp.MOS1type * (modeltemp.MOS1gamma * Math.Sqrt(modeltemp.MOS1phi)) + .5 * (modeltemp.egfet1 - egfet) +
                modeltemp.MOS1type * .5 * (MOS1tPhi - modeltemp.MOS1phi);
            MOS1tVto = MOS1tVbi + modeltemp.MOS1type * modeltemp.MOS1gamma * Math.Sqrt(MOS1tPhi);
            MOS1tSatCur = modeltemp.MOS1jctSatCur * Math.Exp(-egfet / vt + modeltemp.egfet1 / modeltemp.vtnom);
            MOS1tSatCurDens = modeltemp.MOS1jctSatCurDensity * Math.Exp(-egfet / vt + modeltemp.egfet1 / modeltemp.vtnom);
            pbo = (modeltemp.MOS1bulkJctPotential - modeltemp.pbfact1) / modeltemp.fact1;
            gmaold = (modeltemp.MOS1bulkJctPotential - pbo) / pbo;
            capfact = 1 / (1 + modeltemp.MOS1bulkJctBotGradingCoeff * (4e-4 * (modeltemp.MOS1tnom - Circuit.CONSTRefTemp) - gmaold));
            MOS1tCbd = modeltemp.MOS1capBD * capfact;
            MOS1tCbs = modeltemp.MOS1capBS * capfact;
            MOS1tCj = modeltemp.MOS1bulkCapFactor * capfact;
            capfact = 1 / (1 + modeltemp.MOS1bulkJctSideGradingCoeff * (4e-4 * (modeltemp.MOS1tnom - Circuit.CONSTRefTemp) - gmaold));
            MOS1tCjsw = modeltemp.MOS1sideWallCapFactor * capfact;
            MOS1tBulkPot = fact2 * pbo + pbfact;
            gmanew = (MOS1tBulkPot - pbo) / pbo;
            capfact = (1 + modeltemp.MOS1bulkJctBotGradingCoeff * (4e-4 * (MOS1temp - Circuit.CONSTRefTemp) - gmanew));
            MOS1tCbd *= capfact;
            MOS1tCbs *= capfact;
            MOS1tCj *= capfact;
            capfact = (1 + modeltemp.MOS1bulkJctSideGradingCoeff * (4e-4 * (MOS1temp - Circuit.CONSTRefTemp) - gmanew));
            MOS1tCjsw *= capfact;
            MOS1tDepCap = modeltemp.MOS1fwdCapDepCoeff * MOS1tBulkPot;
            if ((MOS1tSatCurDens == 0) || (MOS1drainArea.Value == 0) || (MOS1sourceArea.Value == 0))
            {
                MOS1sourceVcrit = MOS1drainVcrit = vt * Math.Log(vt / (Circuit.CONSTroot2 * MOS1tSatCur));
            }
            else
            {
                MOS1drainVcrit = vt * Math.Log(vt / (Circuit.CONSTroot2 * MOS1tSatCurDens * MOS1drainArea));
                MOS1sourceVcrit = vt * Math.Log(vt / (Circuit.CONSTroot2 * MOS1tSatCurDens * MOS1sourceArea));
            }

            if (modeltemp.MOS1capBD.Given)
            {
                czbd = MOS1tCbd;
            }
            else
            {
                if (modeltemp.MOS1bulkCapFactor.Given)
                {
                    czbd = MOS1tCj * MOS1drainArea;
                }
                else
                {
                    czbd = 0;
                }
            }
            if (modeltemp.MOS1sideWallCapFactor.Given)
            {
                czbdsw = MOS1tCjsw * MOS1drainPerimiter;
            }
            else
            {
                czbdsw = 0;
            }
            arg = 1 - modeltemp.MOS1fwdCapDepCoeff;
            sarg = Math.Exp((-modeltemp.MOS1bulkJctBotGradingCoeff) * Math.Log(arg));
            sargsw = Math.Exp((-modeltemp.MOS1bulkJctSideGradingCoeff) * Math.Log(arg));
            MOS1Cbd = czbd;
            MOS1Cbdsw = czbdsw;
            MOS1f2d = czbd * (1 - modeltemp.MOS1fwdCapDepCoeff * (1 + modeltemp.MOS1bulkJctBotGradingCoeff)) * sarg / arg + czbdsw * (1 -
                modeltemp.MOS1fwdCapDepCoeff * (1 + modeltemp.MOS1bulkJctSideGradingCoeff)) * sargsw / arg;
            MOS1f3d = czbd * modeltemp.MOS1bulkJctBotGradingCoeff * sarg / arg / MOS1tBulkPot + czbdsw * modeltemp.MOS1bulkJctSideGradingCoeff *
                sargsw / arg / MOS1tBulkPot;
            MOS1f4d = czbd * MOS1tBulkPot * (1 - arg * sarg) / (1 - modeltemp.MOS1bulkJctBotGradingCoeff) + czbdsw * MOS1tBulkPot * (1 - arg *
                sargsw) / (1 - modeltemp.MOS1bulkJctSideGradingCoeff) - MOS1f3d / 2 * (MOS1tDepCap * MOS1tDepCap) - MOS1tDepCap * MOS1f2d;
            if (modeltemp.MOS1capBS.Given)
            {
                czbs = MOS1tCbs;
            }
            else
            {
                if (modeltemp.MOS1bulkCapFactor.Given)
                {
                    czbs = MOS1tCj * MOS1sourceArea;
                }
                else
                {
                    czbs = 0;
                }
            }
            if (modeltemp.MOS1sideWallCapFactor.Given)
            {
                czbssw = MOS1tCjsw * MOS1sourcePerimiter;
            }
            else
            {
                czbssw = 0;
            }
            arg = 1 - modeltemp.MOS1fwdCapDepCoeff;
            sarg = Math.Exp((-modeltemp.MOS1bulkJctBotGradingCoeff) * Math.Log(arg));
            sargsw = Math.Exp((-modeltemp.MOS1bulkJctSideGradingCoeff) * Math.Log(arg));
            MOS1Cbs = czbs;
            MOS1Cbssw = czbssw;
            MOS1f2s = czbs * (1 - modeltemp.MOS1fwdCapDepCoeff * (1 + modeltemp.MOS1bulkJctBotGradingCoeff)) * sarg / arg + czbssw * (1 -
                modeltemp.MOS1fwdCapDepCoeff * (1 + modeltemp.MOS1bulkJctSideGradingCoeff)) * sargsw / arg;
            MOS1f3s = czbs * modeltemp.MOS1bulkJctBotGradingCoeff * sarg / arg / MOS1tBulkPot + czbssw * modeltemp.MOS1bulkJctSideGradingCoeff *
                sargsw / arg / MOS1tBulkPot;
            MOS1f4s = czbs * MOS1tBulkPot * (1 - arg * sarg) / (1 - modeltemp.MOS1bulkJctBotGradingCoeff) + czbssw * MOS1tBulkPot * (1 - arg *
                sargsw) / (1 - modeltemp.MOS1bulkJctSideGradingCoeff) - MOS1f3s / 2 * (MOS1tDepCap * MOS1tDepCap) - MOS1tDepCap * MOS1f2s;

            if (modeltemp.MOS1drainResistance.Given)
            {
                if (modeltemp.MOS1drainResistance != 0)
                {
                    MOS1drainConductance = 1 / modeltemp.MOS1drainResistance;
                }
                else
                {
                    MOS1drainConductance = 0;
                }
            }
            else if (modeltemp.MOS1sheetResistance.Given)
            {
                if (modeltemp.MOS1sheetResistance != 0)
                {
                    MOS1drainConductance = 1 / (modeltemp.MOS1sheetResistance * MOS1drainSquares);
                }
                else
                {
                    MOS1drainConductance = 0;
                }
            }
            else
            {
                MOS1drainConductance = 0;
            }
            if (modeltemp.MOS1sourceResistance.Given)
            {
                if (modeltemp.MOS1sourceResistance != 0)
                {
                    MOS1sourceConductance = 1 / modeltemp.MOS1sourceResistance;
                }
                else
                {
                    MOS1sourceConductance = 0;
                }
            }
            else if (modeltemp.MOS1sheetResistance.Given)
            {
                if (modeltemp.MOS1sheetResistance != 0)
                {
                    MOS1sourceConductance = 1 / (modeltemp.MOS1sheetResistance * MOS1sourceSquares);
                }
                else
                {
                    MOS1sourceConductance = 0;
                }
            }
            else
            {
                MOS1sourceConductance = 0;
            }
        }
    }
}
