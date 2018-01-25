using System;
using SpiceSharp.Diagnostics;
using SpiceSharp.Attributes;
using SpiceSharp.Circuits;
using SpiceSharp.Simulations;
using SpiceSharp.Components.Mosfet.Level1;

namespace SpiceSharp.Behaviors.Mosfet.Level1
{
    /// <summary>
    /// Temperature behavior for a <see cref="Components.MOS1"/>
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
        /// Shared variables
        /// </summary>
        [PropertyName("sourcevcrit"), PropertyInfo("Critical source voltage")]
        public double MOS1sourceVcrit { get; protected set; }
        [PropertyName("drainvcrit"), PropertyInfo("Critical drain voltage")]
        public double MOS1drainVcrit { get; protected set; }
        [PropertyName("sourceconductance"), PropertyInfo("Conductance of source")]
        public double MOS1sourceConductance { get; protected set; }
        [PropertyName("drainconductance"), PropertyInfo("Conductance of drain")]
        public double MOS1drainConductance { get; protected set; }
        [PropertyName("cbd0"), PropertyInfo("Zero-Bias B-D junction capacitance")]
        public double MOS1Cbd { get; protected set; }
        [PropertyName("cbdsw0"), PropertyInfo(" ")]
        public double MOS1Cbdsw { get; protected set; }
        [PropertyName("cbs0"), PropertyInfo("Zero-Bias B-S junction capacitance")]
        public double MOS1Cbs { get; protected set; }
        [PropertyName("cbssw0"), PropertyInfo(" ")]
        public double MOS1Cbssw { get; protected set; }

        [PropertyName("rs"), PropertyInfo("Source resistance")]
        public double GetSOURCERESIST(Circuit circuit)
        {
            if (MOS1sourceConductance != 0.0)
                return 1.0 / MOS1sourceConductance;
            return 0.0;
        }
        [PropertyName("rd"), PropertyInfo("Drain conductance")]
        public double GetDRAINRESIST(Circuit circuit)
        {
            if (MOS1drainConductance != 0.0)
                return 1.0 / MOS1drainConductance;
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
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            // Get parameters
            bp = provider.GetParameterSet<BaseParameters>(0);
            mbp = provider.GetParameterSet<ModelBaseParameters>(1);

            // Get behaviors
            modeltemp = provider.GetBehavior<ModelTemperatureBehavior>(1);
        }

        /// <summary>
        /// Do temperature-dependent calculations
        /// </summary>
        /// <param name="sim">Base simulation</param>
        public override void Temperature(BaseSimulation sim)
        {
			if (sim == null)
				throw new ArgumentNullException(nameof(sim));

            double vt, ratio, fact2, kt, egfet, arg, pbfact, ratio4, phio, pbo, gmaold, capfact, gmanew, czbd, czbdsw, sarg, sargsw, czbs,
                czbssw;

            /* perform the parameter defaulting */
            if (!bp.MOS1temp.Given)
            {
                bp.MOS1temp.Value = sim.State.Temperature;
            }
            vt = bp.MOS1temp * Circuit.KOverQ;
            ratio = bp.MOS1temp / mbp.MOS1tnom;
            fact2 = bp.MOS1temp / Circuit.ReferenceTemperature;
            kt = bp.MOS1temp * Circuit.Boltzmann;
            egfet = 1.16 - (7.02e-4 * bp.MOS1temp * bp.MOS1temp) / (bp.MOS1temp + 1108);
            arg = -egfet / (kt + kt) + 1.1150877 / (Circuit.Boltzmann * (Circuit.ReferenceTemperature + Circuit.ReferenceTemperature));
            pbfact = -2 * vt * (1.5 * Math.Log(fact2) + Circuit.Charge * arg);

            if (bp.MOS1l - 2 * mbp.MOS1latDiff <= 0)
                CircuitWarning.Warning(this, $"{Name}: effective channel length less than zero");
            ratio4 = ratio * Math.Sqrt(ratio);
            MOS1tTransconductance = mbp.MOS1transconductance / ratio4;
            MOS1tSurfMob = mbp.MOS1surfaceMobility / ratio4;
            phio = (mbp.MOS1phi - modeltemp.pbfact1) / modeltemp.fact1;
            MOS1tPhi = fact2 * phio + pbfact;
            MOS1tVbi = mbp.MOS1vt0 - mbp.MOS1type * (mbp.MOS1gamma * Math.Sqrt(mbp.MOS1phi)) + .5 * (modeltemp.egfet1 - egfet) +
                mbp.MOS1type * .5 * (MOS1tPhi - mbp.MOS1phi);
            MOS1tVto = MOS1tVbi + mbp.MOS1type * mbp.MOS1gamma * Math.Sqrt(MOS1tPhi);
            MOS1tSatCur = mbp.MOS1jctSatCur * Math.Exp(-egfet / vt + modeltemp.egfet1 / modeltemp.vtnom);
            MOS1tSatCurDens = mbp.MOS1jctSatCurDensity * Math.Exp(-egfet / vt + modeltemp.egfet1 / modeltemp.vtnom);
            pbo = (mbp.MOS1bulkJctPotential - modeltemp.pbfact1) / modeltemp.fact1;
            gmaold = (mbp.MOS1bulkJctPotential - pbo) / pbo;
            capfact = 1 / (1 + mbp.MOS1bulkJctBotGradingCoeff * (4e-4 * (mbp.MOS1tnom - Circuit.ReferenceTemperature) - gmaold));
            MOS1tCbd = mbp.MOS1capBD * capfact;
            MOS1tCbs = mbp.MOS1capBS * capfact;
            MOS1tCj = mbp.MOS1bulkCapFactor * capfact;
            capfact = 1 / (1 + mbp.MOS1bulkJctSideGradingCoeff * (4e-4 * (mbp.MOS1tnom - Circuit.ReferenceTemperature) - gmaold));
            MOS1tCjsw = mbp.MOS1sideWallCapFactor * capfact;
            MOS1tBulkPot = fact2 * pbo + pbfact;
            gmanew = (MOS1tBulkPot - pbo) / pbo;
            capfact = (1 + mbp.MOS1bulkJctBotGradingCoeff * (4e-4 * (bp.MOS1temp - Circuit.ReferenceTemperature) - gmanew));
            MOS1tCbd *= capfact;
            MOS1tCbs *= capfact;
            MOS1tCj *= capfact;
            capfact = (1 + mbp.MOS1bulkJctSideGradingCoeff * (4e-4 * (bp.MOS1temp - Circuit.ReferenceTemperature) - gmanew));
            MOS1tCjsw *= capfact;
            MOS1tDepCap = mbp.MOS1fwdCapDepCoeff * MOS1tBulkPot;
            if ((MOS1tSatCurDens == 0) || (bp.MOS1drainArea.Value == 0) || (bp.MOS1sourceArea.Value == 0))
            {
                MOS1sourceVcrit = MOS1drainVcrit = vt * Math.Log(vt / (Circuit.Root2 * MOS1tSatCur));
            }
            else
            {
                MOS1drainVcrit = vt * Math.Log(vt / (Circuit.Root2 * MOS1tSatCurDens * bp.MOS1drainArea));
                MOS1sourceVcrit = vt * Math.Log(vt / (Circuit.Root2 * MOS1tSatCurDens * bp.MOS1sourceArea));
            }

            if (mbp.MOS1capBD.Given)
            {
                czbd = MOS1tCbd;
            }
            else
            {
                if (mbp.MOS1bulkCapFactor.Given)
                {
                    czbd = MOS1tCj * bp.MOS1drainArea;
                }
                else
                {
                    czbd = 0;
                }
            }
            if (mbp.MOS1sideWallCapFactor.Given)
            {
                czbdsw = MOS1tCjsw * bp.MOS1drainPerimiter;
            }
            else
            {
                czbdsw = 0;
            }
            arg = 1 - mbp.MOS1fwdCapDepCoeff;
            sarg = Math.Exp((-mbp.MOS1bulkJctBotGradingCoeff) * Math.Log(arg));
            sargsw = Math.Exp((-mbp.MOS1bulkJctSideGradingCoeff) * Math.Log(arg));
            MOS1Cbd = czbd;
            MOS1Cbdsw = czbdsw;
            MOS1f2d = czbd * (1 - mbp.MOS1fwdCapDepCoeff * (1 + mbp.MOS1bulkJctBotGradingCoeff)) * sarg / arg + czbdsw * (1 -
                mbp.MOS1fwdCapDepCoeff * (1 + mbp.MOS1bulkJctSideGradingCoeff)) * sargsw / arg;
            MOS1f3d = czbd * mbp.MOS1bulkJctBotGradingCoeff * sarg / arg / MOS1tBulkPot + czbdsw * mbp.MOS1bulkJctSideGradingCoeff *
                sargsw / arg / MOS1tBulkPot;
            MOS1f4d = czbd * MOS1tBulkPot * (1 - arg * sarg) / (1 - mbp.MOS1bulkJctBotGradingCoeff) + czbdsw * MOS1tBulkPot * (1 - arg *
                sargsw) / (1 - mbp.MOS1bulkJctSideGradingCoeff) - MOS1f3d / 2 * (MOS1tDepCap * MOS1tDepCap) - MOS1tDepCap * MOS1f2d;
            if (mbp.MOS1capBS.Given)
            {
                czbs = MOS1tCbs;
            }
            else
            {
                if (mbp.MOS1bulkCapFactor.Given)
                {
                    czbs = MOS1tCj * bp.MOS1sourceArea;
                }
                else
                {
                    czbs = 0;
                }
            }
            if (mbp.MOS1sideWallCapFactor.Given)
            {
                czbssw = MOS1tCjsw * bp.MOS1sourcePerimiter;
            }
            else
            {
                czbssw = 0;
            }
            arg = 1 - mbp.MOS1fwdCapDepCoeff;
            sarg = Math.Exp((-mbp.MOS1bulkJctBotGradingCoeff) * Math.Log(arg));
            sargsw = Math.Exp((-mbp.MOS1bulkJctSideGradingCoeff) * Math.Log(arg));
            MOS1Cbs = czbs;
            MOS1Cbssw = czbssw;
            MOS1f2s = czbs * (1 - mbp.MOS1fwdCapDepCoeff * (1 + mbp.MOS1bulkJctBotGradingCoeff)) * sarg / arg + czbssw * (1 -
                mbp.MOS1fwdCapDepCoeff * (1 + mbp.MOS1bulkJctSideGradingCoeff)) * sargsw / arg;
            MOS1f3s = czbs * mbp.MOS1bulkJctBotGradingCoeff * sarg / arg / MOS1tBulkPot + czbssw * mbp.MOS1bulkJctSideGradingCoeff *
                sargsw / arg / MOS1tBulkPot;
            MOS1f4s = czbs * MOS1tBulkPot * (1 - arg * sarg) / (1 - mbp.MOS1bulkJctBotGradingCoeff) + czbssw * MOS1tBulkPot * (1 - arg *
                sargsw) / (1 - mbp.MOS1bulkJctSideGradingCoeff) - MOS1f3s / 2 * (MOS1tDepCap * MOS1tDepCap) - MOS1tDepCap * MOS1f2s;

            if (mbp.MOS1drainResistance.Given)
            {
                if (mbp.MOS1drainResistance != 0)
                {
                    MOS1drainConductance = 1 / mbp.MOS1drainResistance;
                }
                else
                {
                    MOS1drainConductance = 0;
                }
            }
            else if (mbp.MOS1sheetResistance.Given)
            {
                if (mbp.MOS1sheetResistance != 0)
                {
                    MOS1drainConductance = 1 / (mbp.MOS1sheetResistance * bp.MOS1drainSquares);
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
            if (mbp.MOS1sourceResistance.Given)
            {
                if (mbp.MOS1sourceResistance != 0)
                {
                    MOS1sourceConductance = 1 / mbp.MOS1sourceResistance;
                }
                else
                {
                    MOS1sourceConductance = 0;
                }
            }
            else if (mbp.MOS1sheetResistance.Given)
            {
                if (mbp.MOS1sheetResistance != 0)
                {
                    MOS1sourceConductance = 1 / (mbp.MOS1sheetResistance * bp.MOS1sourceSquares);
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
