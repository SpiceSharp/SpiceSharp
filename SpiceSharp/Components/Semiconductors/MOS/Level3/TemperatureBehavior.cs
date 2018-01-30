using System;
using SpiceSharp.Diagnostics;
using SpiceSharp.Attributes;
using SpiceSharp.Simulations;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.MosfetBehaviors.Level3
{
    /// <summary>
    /// Temperature behavior for a <see cref="Mosfet3"/>
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
        [PropertyName("cbdsw0"), PropertyInfo("Zero-Bias B-D sidewall capacitance")]
        public double Cbdsw { get; internal set; }
        [PropertyName("cbs0"), PropertyInfo("Zero-Bias B-S junction capacitance")]
        public double Cbs { get; internal set; }
        [PropertyName("cbssw0"), PropertyInfo("Zero-Bias B-S sidewall capacitance")]
        public double Cbssw { get; internal set; }

        [PropertyName("rs"), PropertyInfo("Source resistance")]
        public double SourceResistance
        {
            get
            {
                if (SourceConductance > 0.0)
                    return 1.0 / SourceConductance;
                return 0.0;
            }
        }
        [PropertyName("rd"), PropertyInfo("Drain resistance")]
        public double DrainResistance
        {
            get
            {
                if (DrainConductance > 0.0)
                    return 1.0 / DrainConductance;
                return 0.0;
            }
        }

        /// <summary>
        /// Extra variables
        /// </summary>
        public double TTransconductance { get; internal set; }
        public double TSurfMob { get; internal set; }
        public double TPhi { get; internal set; }
        public double TVbi { get; internal set; }
        public double TVto { get; internal set; }
        public double TSatCur { get; internal set; }
        public double TSatCurDens { get; internal set; }
        public double TCbd { get; internal set; }
        public double TCbs { get; internal set; }
        public double TCj { get; internal set; }
        public double TCjsw { get; internal set; }
        public double TBulkPot { get; internal set; }
        public double TDepCap { get; internal set; }
        public double F2d { get; internal set; }
        public double F3d { get; internal set; }
        public double F4d { get; internal set; }
        public double F2s { get; internal set; }
        public double F3s { get; internal set; }
        public double F4s { get; internal set; }
        public double Cgs { get; internal set; }
        public double Cgd { get; internal set; }
        public double Cgb { get; internal set; }

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

            var state = simulation.State;
            double vt, ratio, fact2, kt, egfet, arg, pbfact, ratio4, phio, pbo, gmaold, capfact, gmanew, czbd, czbdsw, sarg, sargsw, czbs,
                czbssw;

            /* perform the parameter defaulting */

            if (!bp.Temperature.Given)
            {
                bp.Temperature.Value = state.Temperature;
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
                throw new CircuitException("{0}: effective channel length less than zero".FormatString(Name));
            ratio4 = ratio * Math.Sqrt(ratio);
            TTransconductance = mbp.Transconductance / ratio4;
            TSurfMob = mbp.SurfaceMobility / ratio4;
            phio = (mbp.Phi - modeltemp.Pbfact1) / modeltemp.Fact1;
            TPhi = fact2 * phio + pbfact;
            TVbi = mbp.Vt0 - mbp.MosfetType * (mbp.Gamma * Math.Sqrt(mbp.Phi)) + .5 * (modeltemp.Egfet1 - egfet) +
                mbp.MosfetType * .5 * (TPhi - mbp.Phi);
            TVto = TVbi + mbp.MosfetType * mbp.Gamma * Math.Sqrt(TPhi);
            TSatCur = mbp.JctSatCur * Math.Exp(-egfet / vt + modeltemp.Egfet1 / modeltemp.Vtnom);
            TSatCurDens = mbp.JctSatCurDensity * Math.Exp(-egfet / vt + modeltemp.Egfet1 / modeltemp.Vtnom);
            pbo = (mbp.BulkJctPotential - modeltemp.Pbfact1) / modeltemp.Fact1;
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

            if ((mbp.JctSatCurDensity.Value == 0) || (bp.DrainArea.Value == 0) || (bp.SourceArea.Value == 0))
            {
                SourceVcrit = DrainVcrit = vt * Math.Log(vt / (Circuit.Root2 * mbp.JctSatCur));
            }
            else
            {
                DrainVcrit = vt * Math.Log(vt / (Circuit.Root2 * mbp.JctSatCurDensity * bp.DrainArea));
                SourceVcrit = vt * Math.Log(vt / (Circuit.Root2 * mbp.JctSatCurDensity * bp.SourceArea));
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
                czbdsw = TCjsw * bp.DrainPerimeter;
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
            F3d = czbd * mbp.BulkJctBotGradingCoeff * sarg / arg / mbp.BulkJctPotential + czbdsw *
                mbp.BulkJctSideGradingCoeff * sargsw / arg / mbp.BulkJctPotential;
            F4d = czbd * mbp.BulkJctPotential * (1 - arg * sarg) / (1 - mbp.BulkJctBotGradingCoeff) + czbdsw *
                mbp.BulkJctPotential * (1 - arg * sargsw) / (1 - mbp.BulkJctSideGradingCoeff) - F3d / 2 * (TDepCap *
                TDepCap) - TDepCap * F2d;
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
                czbssw = TCjsw * bp.SourcePerimeter;
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
            F3s = czbs * mbp.BulkJctBotGradingCoeff * sarg / arg / mbp.BulkJctPotential + czbssw *
                mbp.BulkJctSideGradingCoeff * sargsw / arg / mbp.BulkJctPotential;
            F4s = czbs * mbp.BulkJctPotential * (1 - arg * sarg) / (1 - mbp.BulkJctBotGradingCoeff) + czbssw *
                mbp.BulkJctPotential * (1 - arg * sargsw) / (1 - mbp.BulkJctSideGradingCoeff) - F3s / 2 * (TBulkPot *
                TBulkPot) - TBulkPot * F2s;
        }
    }
}
