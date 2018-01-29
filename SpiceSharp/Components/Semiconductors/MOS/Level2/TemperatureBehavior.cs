using System;
using SpiceSharp.Diagnostics;
using SpiceSharp.Attributes;
using SpiceSharp.Simulations;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.MosfetBehaviors.Level2
{
    /// <summary>
    /// Temperature behavior for a <see cref="Mosfet2"/>
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
        [PropertyName("sourceconductance"), PropertyInfo("Source conductance")]
        public double SourceConductance { get; internal set; }
        [PropertyName("drainconductance"), PropertyInfo("Drain conductance")]
        public double DrainConductance { get; internal set; }
        [PropertyName("sourcevcrit"), PropertyInfo("Critical source voltage")]
        public double SourceVcrit { get; internal set; }
        [PropertyName("drainvcrit"), PropertyInfo("Critical drain voltage")]
        public double DrainVcrit { get; internal set; }
        [PropertyName("cbd0"), PropertyInfo("Zero-Bias B-D junction capacitance")]
        public double Cbd { get; internal set; }
        [PropertyName("cbdsw0"), PropertyInfo(" ")]
        public double Cbdsw { get; internal set; }
        [PropertyName("cbs0"), PropertyInfo("Zero-Bias B-S junction capacitance")]
        public double Cbs { get; internal set; }
        [PropertyName("cbssw0"), PropertyInfo(" ")]
        public double Cbssw { get; internal set; }
        [PropertyName("rs"), PropertyInfo("Source resistance")]
        public double GetSOURCERESIST()
        {
            if (SourceConductance > 0.0)
                return 1.0 / SourceConductance;
            return 0.0;
        }
        [PropertyName("rd"), PropertyInfo("Drain resistance")]
        public double GetDRAINRESIST()
        {
            if (DrainConductance > 0.0)
                return 1.0 / DrainConductance;
            return 0.0;
        }
        public double TTransconductance { get; protected set; }
        public double TSurfMob { get; protected set; }
        public double TPhi { get; protected set; }
        public double TVbi { get; protected set; }
        public double TVto { get; protected set; }
        public double TSatCur { get; protected set; }
        public double TSatCurDens { get; protected set; }
        public double TCbd { get; protected set; }
        public double TCbs { get; protected set; }
        public double TCj { get; protected set; }
        public double TCjsw { get; protected set; }
        public double TBulkPot { get; protected set; }
        public double TDepCap { get; protected set; }
        public double F2d { get; protected set; }
        public double F3d { get; protected set; }
        public double F4d { get; protected set; }
        public double F2s { get; protected set; }
        public double F3s { get; protected set; }
        public double F4s { get; protected set; }
        public double Cgs { get; protected set; }
        public double Cgd { get; protected set; }
        public double Cgb { get; protected set; }

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
        /// <param name="simulation">Base simulation</param>
        public override void Temperature(BaseSimulation simulation)
        {
			if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));

            double vt, ratio, fact2, kt, egfet, arg, pbfact, ratio4, phio, pbo, gmaold, capfact, gmanew, czbd, czbdsw, sarg, sargsw, czbs,
                czbssw;

            /* perform the parameter defaulting */
            if (!bp.Temperature.Given)
            {
                bp.Temperature.Value = simulation.State.Temperature;
            }

            vt = bp.Temperature * Circuit.KOverQ;
            ratio = bp.Temperature / mbp.NominalTemperature;
            fact2 = bp.Temperature / Circuit.ReferenceTemperature;
            kt = bp.Temperature * Circuit.Boltzmann;
            egfet = 1.16 - (7.02e-4 * bp.Temperature * bp.Temperature) / (bp.Temperature + 1108);
            arg = -egfet / (kt + kt) + 1.1150877 / (Circuit.Boltzmann * (Circuit.ReferenceTemperature + Circuit.ReferenceTemperature));
            pbfact = -2 * vt * (1.5 * Math.Log(fact2) + Circuit.Charge * arg);

            if (mbp.DrainResistance.Given)
            {
                if (mbp.DrainResistance != 0)
                {
                    DrainConductance = 1 / mbp.DrainResistance;
                }
                else
                {
                    DrainConductance = 0;
                }
            }
            else if (mbp.SheetResistance.Given)
            {
                if (mbp.SheetResistance != 0)
                {
                    DrainConductance = 1 / (mbp.SheetResistance * bp.DrainSquares);
                }
                else
                {
                    DrainConductance = 0;
                }
            }
            else
            {
                DrainConductance = 0;
            }
            if (mbp.SourceResistance.Given)
            {
                if (mbp.SourceResistance != 0)
                {
                    SourceConductance = 1 / mbp.SourceResistance;
                }
                else
                {
                    SourceConductance = 0;
                }
            }
            else if (mbp.SheetResistance.Given)
            {
                if (mbp.SheetResistance != 0)
                {
                    SourceConductance = 1 / (mbp.SheetResistance * bp.SourceSquares);
                }
                else
                {
                    SourceConductance = 0;
                }
            }
            else
            {
                SourceConductance = 0;
            }
            if (bp.Length - 2 * mbp.LatDiff <= 0)
                CircuitWarning.Warning(this, "{0}: effective channel length less than zero".FormatString(Name));

            ratio4 = ratio * Math.Sqrt(ratio);
            TTransconductance = mbp.Transconductance / ratio4;
            TSurfMob = mbp.SurfaceMobility / ratio4;
            phio = (mbp.Phi - modeltemp.pbfact1) / modeltemp.fact1;
            TPhi = fact2 * phio + pbfact;
            TVbi = mbp.Vt0 - mbp.MosfetType * (mbp.Gamma * Math.Sqrt(mbp.Phi)) + .5 * (modeltemp.egfet1 - egfet) +
                mbp.MosfetType * .5 * (TPhi - mbp.Phi);
            TVto = TVbi + mbp.MosfetType * mbp.Gamma * Math.Sqrt(TPhi);
            TSatCur = mbp.JctSatCur * Math.Exp(-egfet / vt + modeltemp.egfet1 / modeltemp.vtnom);
            TSatCurDens = mbp.JctSatCurDensity * Math.Exp(-egfet / vt + modeltemp.egfet1 / modeltemp.vtnom);
            pbo = (mbp.BulkJctPotential - modeltemp.pbfact1) / modeltemp.fact1;
            gmaold = (mbp.BulkJctPotential - pbo) / pbo;
            capfact = 1 / (1 + mbp.BulkJctBotGradingCoeff * (4e-4 * (mbp.NominalTemperature - Circuit.ReferenceTemperature) - gmaold));
            TCbd = mbp.CapBD * capfact;
            TCbs = mbp.CapBS * capfact;
            TCj = mbp.BulkCapFactor * capfact;
            capfact = 1 / (1 + mbp.BulkJctSideGradingCoeff * (4e-4 * (mbp.NominalTemperature - Circuit.ReferenceTemperature) - gmaold));
            TCjsw = mbp.SidewallCapFactor * capfact;
            TBulkPot = fact2 * pbo + pbfact;
            gmanew = (TBulkPot - pbo) / pbo;
            capfact = (1 + mbp.BulkJctBotGradingCoeff * (4e-4 * (bp.Temperature - Circuit.ReferenceTemperature) - gmanew));
            TCbd *= capfact;
            TCbs *= capfact;
            TCj *= capfact;
            capfact = (1 + mbp.BulkJctSideGradingCoeff * (4e-4 * (bp.Temperature - Circuit.ReferenceTemperature) - gmanew));
            TCjsw *= capfact;
            TDepCap = mbp.FwdCapDepCoeff * TBulkPot;

            if ((TSatCurDens == 0) || (bp.DrainArea.Value == 0) || (bp.SourceArea.Value == 0))
            {
                SourceVcrit = DrainVcrit = vt * Math.Log(vt / (Circuit.Root2 * TSatCur));
            }
            else
            {
                DrainVcrit = vt * Math.Log(vt / (Circuit.Root2 * TSatCurDens * bp.DrainArea));
                SourceVcrit = vt * Math.Log(vt / (Circuit.Root2 * TSatCurDens * bp.SourceArea));
            }
            if (mbp.CapBD.Given)
            {
                czbd = TCbd;
            }
            else
            {
                if (mbp.BulkCapFactor.Given)
                {
                    czbd = TCj * bp.DrainArea;
                }
                else
                {
                    czbd = 0;
                }
            }
            if (mbp.SidewallCapFactor.Given)
            {
                czbdsw = TCjsw * bp.DrainPerimiter;
            }
            else
            {
                czbdsw = 0;
            }
            arg = 1 - mbp.FwdCapDepCoeff;
            sarg = Math.Exp((-mbp.BulkJctBotGradingCoeff) * Math.Log(arg));
            sargsw = Math.Exp((-mbp.BulkJctSideGradingCoeff) * Math.Log(arg));
            Cbd = czbd;
            Cbdsw = czbdsw;
            F2d = czbd * (1 - mbp.FwdCapDepCoeff * (1 + mbp.BulkJctBotGradingCoeff)) * sarg / arg + czbdsw * (1 -
                mbp.FwdCapDepCoeff * (1 + mbp.BulkJctSideGradingCoeff)) * sargsw / arg;
            F3d = czbd * mbp.BulkJctBotGradingCoeff * sarg / arg / TBulkPot + czbdsw * mbp.BulkJctSideGradingCoeff *
                sargsw / arg / TBulkPot;
            F4d = czbd * TBulkPot * (1 - arg * sarg) / (1 - mbp.BulkJctBotGradingCoeff) + czbdsw * TBulkPot * (1 - arg *
                sargsw) / (1 - mbp.BulkJctSideGradingCoeff) - F3d / 2 * (TDepCap * TDepCap) - TDepCap * F2d;
            if (mbp.CapBS.Given)
            {
                czbs = TCbs;
            }
            else
            {
                if (mbp.BulkCapFactor.Given)
                {
                    czbs = TCj * bp.SourceArea;
                }
                else
                {
                    czbs = 0;
                }
            }
            if (mbp.SidewallCapFactor.Given)
            {
                czbssw = TCjsw * bp.SourcePerimiter;
            }
            else
            {
                czbssw = 0;
            }
            arg = 1 - mbp.FwdCapDepCoeff;
            sarg = Math.Exp((-mbp.BulkJctBotGradingCoeff) * Math.Log(arg));
            sargsw = Math.Exp((-mbp.BulkJctSideGradingCoeff) * Math.Log(arg));
            Cbs = czbs;
            Cbssw = czbssw;
            F2s = czbs * (1 - mbp.FwdCapDepCoeff * (1 + mbp.BulkJctBotGradingCoeff)) * sarg / arg + czbssw * (1 -
                mbp.FwdCapDepCoeff * (1 + mbp.BulkJctSideGradingCoeff)) * sargsw / arg;
            F3s = czbs * mbp.BulkJctBotGradingCoeff * sarg / arg / TBulkPot + czbssw * mbp.BulkJctSideGradingCoeff *
                sargsw / arg / TBulkPot;
            F4s = czbs * TBulkPot * (1 - arg * sarg) / (1 - mbp.BulkJctBotGradingCoeff) + czbssw * TBulkPot * (1 - arg *
                sargsw) / (1 - mbp.BulkJctSideGradingCoeff) - F3s / 2 * (TDepCap * TDepCap) - TDepCap * F2s;
        }
    }
}
