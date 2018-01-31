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
        public double SourceConductance { get; protected set; }
        [PropertyName("drainconductance"), PropertyInfo("Drain conductance")]
        public double DrainConductance { get; protected set; }
        [PropertyName("sourcevcrit"), PropertyInfo("Critical source voltage")]
        public double SourceVcrit { get; protected set; }
        [PropertyName("drainvcrit"), PropertyInfo("Critical drain voltage")]
        public double DrainVcrit { get; protected set; }
        [PropertyName("cbd0"), PropertyInfo("Zero-Bias B-D junction capacitance")]
        public double Cbd { get; protected set; }
        [PropertyName("cbdsw0"), PropertyInfo("Zero-Bias B-D sidewall capacitance")]
        public double Cbdsw { get; protected set; }
        [PropertyName("cbs0"), PropertyInfo("Zero-Bias B-S junction capacitance")]
        public double Cbs { get; protected set; }
        [PropertyName("cbssw0"), PropertyInfo("Zero-Bias B-S sidewall capacitance")]
        public double Cbssw { get; protected set; }

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
        public double TempTransconductance { get; protected set; }
        public double TempSurfaceMobility { get; protected set; }
        public double TempPhi { get; protected set; }
        public double TempVbi { get; protected set; }
        public double TempVto { get; protected set; }
        public double TempSaturationCurrent { get; protected set; }
        public double TempSaturationCurrentDensity { get; protected set; }
        public double TempCbd { get; protected set; }
        public double TempCbs { get; protected set; }
        public double TempJunctionCap { get; protected set; }
        public double TempJunctionCapSidewall { get; protected set; }
        public double TempBulkPotential { get; protected set; }
        public double TempDepletionCap { get; protected set; }
        public double F2D { get; protected set; }
        public double F3D { get; protected set; }
        public double F4D { get; protected set; }
        public double F2S { get; protected set; }
        public double F3S { get; protected set; }
        public double F4S { get; protected set; }
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
            TempTransconductance = mbp.Transconductance / ratio4;
            TempSurfaceMobility = mbp.SurfaceMobility / ratio4;
            phio = (mbp.Phi - modeltemp.Pbfact1) / modeltemp.Fact1;
            TempPhi = fact2 * phio + pbfact;
            TempVbi = mbp.Vt0 - mbp.MosfetType * (mbp.Gamma * Math.Sqrt(mbp.Phi)) + .5 * (modeltemp.Egfet1 - egfet) +
                mbp.MosfetType * .5 * (TempPhi - mbp.Phi);
            TempVto = TempVbi + mbp.MosfetType * mbp.Gamma * Math.Sqrt(TempPhi);
            TempSaturationCurrent = mbp.JunctionSatCur * Math.Exp(-egfet / vt + modeltemp.Egfet1 / modeltemp.Vtnom);
            TempSaturationCurrentDensity = mbp.JunctionSatCurDensity * Math.Exp(-egfet / vt + modeltemp.Egfet1 / modeltemp.Vtnom);
            pbo = (mbp.BulkJunctionPotential - modeltemp.Pbfact1) / modeltemp.Fact1;
            gmaold = (mbp.BulkJunctionPotential - pbo) / pbo;
            capfact = 1 / (1 + mbp.BulkJunctionBotGradingCoefficient * (4e-4 * (mbp.NominalTemperature - Circuit.ReferenceTemperature) - gmaold));
            TempCbd = mbp.CapBD * capfact;
            TempCbs = mbp.CapBS * capfact;
            TempJunctionCap = mbp.BulkCapFactor * capfact;
            capfact = 1 / (1 + mbp.BulkJunctionSideGradingCoefficient * (4e-4 * (mbp.NominalTemperature - Circuit.ReferenceTemperature) - gmaold));
            TempJunctionCapSidewall = mbp.SidewallCapFactor * capfact;
            TempBulkPotential = fact2 * pbo + pbfact;
            gmanew = (TempBulkPotential - pbo) / pbo;
            capfact = (1 + mbp.BulkJunctionBotGradingCoefficient * (4e-4 * (bp.Temperature - Circuit.ReferenceTemperature) - gmanew));
            TempCbd *= capfact;
            TempCbs *= capfact;
            TempJunctionCap *= capfact;
            capfact = (1 + mbp.BulkJunctionSideGradingCoefficient * (4e-4 * (bp.Temperature - Circuit.ReferenceTemperature) - gmanew));
            TempJunctionCapSidewall *= capfact;
            TempDepletionCap = mbp.ForwardCapDepCoefficient * TempBulkPotential;

            if ((mbp.JunctionSatCurDensity.Value == 0) || (bp.DrainArea.Value == 0) || (bp.SourceArea.Value == 0))
            {
                SourceVcrit = DrainVcrit = vt * Math.Log(vt / (Circuit.Root2 * mbp.JunctionSatCur));
            }
            else
            {
                DrainVcrit = vt * Math.Log(vt / (Circuit.Root2 * mbp.JunctionSatCurDensity * bp.DrainArea));
                SourceVcrit = vt * Math.Log(vt / (Circuit.Root2 * mbp.JunctionSatCurDensity * bp.SourceArea));
            }
            if (mbp.CapBD.Given)
            {
                czbd = TempCbd;
            }
            else
            {
                if (mbp.BulkCapFactor.Given)
                {
                    czbd = TempJunctionCap * bp.DrainArea;
                }
                else
                {
                    czbd = 0;
                }
            }
            if (mbp.SidewallCapFactor.Given)
            {
                czbdsw = TempJunctionCapSidewall * bp.DrainPerimeter;
            }
            else
            {
                czbdsw = 0;
            }
            arg = 1 - mbp.ForwardCapDepCoefficient;
            sarg = Math.Exp((-mbp.BulkJunctionBotGradingCoefficient) * Math.Log(arg));
            sargsw = Math.Exp((-mbp.BulkJunctionSideGradingCoefficient) * Math.Log(arg));
            Cbd = czbd;
            Cbdsw = czbdsw;
            F2D = czbd * (1 - mbp.ForwardCapDepCoefficient * (1 + mbp.BulkJunctionBotGradingCoefficient)) * sarg / arg + czbdsw * (1 -
                mbp.ForwardCapDepCoefficient * (1 + mbp.BulkJunctionSideGradingCoefficient)) * sargsw / arg;
            F3D = czbd * mbp.BulkJunctionBotGradingCoefficient * sarg / arg / mbp.BulkJunctionPotential + czbdsw *
                mbp.BulkJunctionSideGradingCoefficient * sargsw / arg / mbp.BulkJunctionPotential;
            F4D = czbd * mbp.BulkJunctionPotential * (1 - arg * sarg) / (1 - mbp.BulkJunctionBotGradingCoefficient) + czbdsw *
                mbp.BulkJunctionPotential * (1 - arg * sargsw) / (1 - mbp.BulkJunctionSideGradingCoefficient) - F3D / 2 * (TempDepletionCap *
                TempDepletionCap) - TempDepletionCap * F2D;
            if (mbp.CapBS.Given)
            {
                czbs = TempCbs;
            }
            else
            {
                if (mbp.BulkCapFactor.Given)
                {
                    czbs = TempJunctionCap * bp.SourceArea;
                }
                else
                {
                    czbs = 0;
                }
            }
            if (mbp.SidewallCapFactor.Given)
            {
                czbssw = TempJunctionCapSidewall * bp.SourcePerimeter;
            }
            else
            {
                czbssw = 0;
            }
            arg = 1 - mbp.ForwardCapDepCoefficient;
            sarg = Math.Exp((-mbp.BulkJunctionBotGradingCoefficient) * Math.Log(arg));
            sargsw = Math.Exp((-mbp.BulkJunctionSideGradingCoefficient) * Math.Log(arg));
            Cbs = czbs;
            Cbssw = czbssw;
            F2S = czbs * (1 - mbp.ForwardCapDepCoefficient * (1 + mbp.BulkJunctionBotGradingCoefficient)) * sarg / arg + czbssw * (1 -
                mbp.ForwardCapDepCoefficient * (1 + mbp.BulkJunctionSideGradingCoefficient)) * sargsw / arg;
            F3S = czbs * mbp.BulkJunctionBotGradingCoefficient * sarg / arg / mbp.BulkJunctionPotential + czbssw *
                mbp.BulkJunctionSideGradingCoefficient * sargsw / arg / mbp.BulkJunctionPotential;
            F4S = czbs * mbp.BulkJunctionPotential * (1 - arg * sarg) / (1 - mbp.BulkJunctionBotGradingCoefficient) + czbssw *
                mbp.BulkJunctionPotential * (1 - arg * sargsw) / (1 - mbp.BulkJunctionSideGradingCoefficient) - F3S / 2 * (TempBulkPotential *
                TempBulkPotential) - TempBulkPotential * F2S;
        }
    }
}
