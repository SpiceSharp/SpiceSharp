using System;
using SpiceSharp.Diagnostics;
using SpiceSharp.Attributes;
using SpiceSharp.Simulations;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.MosfetBehaviors.Level1
{
    /// <summary>
    /// Temperature behavior for a <see cref="Mosfet1"/>
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
        public double SourceVCritical { get; protected set; }
        [PropertyName("drainvcrit"), PropertyInfo("Critical drain voltage")]
        public double DrainVCritical { get; protected set; }
        [PropertyName("sourceconductance"), PropertyInfo("Conductance of source")]
        public double SourceConductance { get; protected set; }
        [PropertyName("drainconductance"), PropertyInfo("Conductance of drain")]
        public double DrainConductance { get; protected set; }
        [PropertyName("cbd0"), PropertyInfo("Zero-Bias B-D junction capacitance")]
        public double CapBD { get; protected set; }
        [PropertyName("cbdsw0"), PropertyInfo(" ")]
        public double CapBDSidewall { get; protected set; }
        [PropertyName("cbs0"), PropertyInfo("Zero-Bias B-S junction capacitance")]
        public double CapBS { get; protected set; }
        [PropertyName("cbssw0"), PropertyInfo(" ")]
        public double CapBSSidewall { get; protected set; }

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
        [PropertyName("rd"), PropertyInfo("Drain conductance")]
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
        public double TempSurfMob { get; protected set; }
        public double TempPhi { get; protected set; }
        public double TempVoltageBI { get; protected set; }
        public double TempVt0 { get; protected set; }
        public double TempSaturationCurrent { get; protected set; }
        public double TempSaturationCurrentDensity { get; protected set; }
        public double TempCapBD { get; protected set; }
        public double TempCapBS { get; protected set; }
        public double TempJunctionCap { get; protected set; }
        public double TempJunctionCapSidewall { get; protected set; }
        public double TempBulkPotential { get; protected set; }
        public double TempDepletionCap { get; protected set; }
        public double F2D { get; protected set; }
        public double F3D{ get; protected set; }
        public double F4D { get; protected set; }
        public double F2S { get; protected set; }
        public double F3S { get; protected set; }
        public double F4S { get; protected set; }
        public double CapGS { get; protected set; }
        public double CapGD { get; protected set; }
        public double CapGB { get; protected set; }

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
            bp = provider.GetParameterSet<BaseParameters>("entity");
            mbp = provider.GetParameterSet<ModelBaseParameters>("model");

            // Get behaviors
            modeltemp = provider.GetBehavior<ModelTemperatureBehavior>("model");
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
                bp.Temperature.Value = simulation.RealState.Temperature;
            }
            vt = bp.Temperature * Circuit.KOverQ;
            ratio = bp.Temperature / mbp.NominalTemperature;
            fact2 = bp.Temperature / Circuit.ReferenceTemperature;
            kt = bp.Temperature * Circuit.Boltzmann;
            egfet = 1.16 - (7.02e-4 * bp.Temperature * bp.Temperature) / (bp.Temperature + 1108);
            arg = -egfet / (kt + kt) + 1.1150877 / (Circuit.Boltzmann * (Circuit.ReferenceTemperature + Circuit.ReferenceTemperature));
            pbfact = -2 * vt * (1.5 * Math.Log(fact2) + Circuit.Charge * arg);

            if (bp.Length - 2 * mbp.LateralDiffusion <= 0)
                CircuitWarning.Warning(this, "{0}: effective channel length less than zero".FormatString(Name));
            ratio4 = ratio * Math.Sqrt(ratio);
            TempTransconductance = mbp.Transconductance / ratio4;
            TempSurfMob = mbp.SurfaceMobility / ratio4;
            phio = (mbp.Phi - modeltemp.PbFactor1) / modeltemp.Fact1;
            TempPhi = fact2 * phio + pbfact;
            TempVoltageBI = mbp.VT0 - mbp.MosfetType * (mbp.Gamma * Math.Sqrt(mbp.Phi)) + .5 * (modeltemp.EgFet1 - egfet) +
                mbp.MosfetType * .5 * (TempPhi - mbp.Phi);
            TempVt0 = TempVoltageBI + mbp.MosfetType * mbp.Gamma * Math.Sqrt(TempPhi);
            TempSaturationCurrent = mbp.JunctionSatCur * Math.Exp(-egfet / vt + modeltemp.EgFet1 / modeltemp.VtNominal);
            TempSaturationCurrentDensity = mbp.JunctionSatCurDensity * Math.Exp(-egfet / vt + modeltemp.EgFet1 / modeltemp.VtNominal);
            pbo = (mbp.BulkJunctionPotential - modeltemp.PbFactor1) / modeltemp.Fact1;
            gmaold = (mbp.BulkJunctionPotential - pbo) / pbo;
            capfact = 1 / (1 + mbp.BulkJunctionBotGradingCoefficient * (4e-4 * (mbp.NominalTemperature - Circuit.ReferenceTemperature) - gmaold));
            TempCapBD = mbp.CapBD * capfact;
            TempCapBS = mbp.CapBS * capfact;
            TempJunctionCap = mbp.BulkCapFactor * capfact;
            capfact = 1 / (1 + mbp.BulkJunctionSideGradingCoefficient * (4e-4 * (mbp.NominalTemperature - Circuit.ReferenceTemperature) - gmaold));
            TempJunctionCapSidewall = mbp.SidewallCapFactor * capfact;
            TempBulkPotential = fact2 * pbo + pbfact;
            gmanew = (TempBulkPotential - pbo) / pbo;
            capfact = (1 + mbp.BulkJunctionBotGradingCoefficient * (4e-4 * (bp.Temperature - Circuit.ReferenceTemperature) - gmanew));
            TempCapBD *= capfact;
            TempCapBS *= capfact;
            TempJunctionCap *= capfact;
            capfact = (1 + mbp.BulkJunctionSideGradingCoefficient * (4e-4 * (bp.Temperature - Circuit.ReferenceTemperature) - gmanew));
            TempJunctionCapSidewall *= capfact;
            TempDepletionCap = mbp.ForwardCapDepletionCoefficient * TempBulkPotential;
            if ((TempSaturationCurrentDensity == 0) || (bp.DrainArea.Value == 0) || (bp.SourceArea.Value == 0))
            {
                SourceVCritical = DrainVCritical = vt * Math.Log(vt / (Circuit.Root2 * TempSaturationCurrent));
            }
            else
            {
                DrainVCritical = vt * Math.Log(vt / (Circuit.Root2 * TempSaturationCurrentDensity * bp.DrainArea));
                SourceVCritical = vt * Math.Log(vt / (Circuit.Root2 * TempSaturationCurrentDensity * bp.SourceArea));
            }

            if (mbp.CapBD.Given)
            {
                czbd = TempCapBD;
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
            arg = 1 - mbp.ForwardCapDepletionCoefficient;
            sarg = Math.Exp((-mbp.BulkJunctionBotGradingCoefficient) * Math.Log(arg));
            sargsw = Math.Exp((-mbp.BulkJunctionSideGradingCoefficient) * Math.Log(arg));
            CapBD = czbd;
            CapBDSidewall = czbdsw;
            F2D = czbd * (1 - mbp.ForwardCapDepletionCoefficient * (1 + mbp.BulkJunctionBotGradingCoefficient)) * sarg / arg + czbdsw * (1 -
                mbp.ForwardCapDepletionCoefficient * (1 + mbp.BulkJunctionSideGradingCoefficient)) * sargsw / arg;
            F3D = czbd * mbp.BulkJunctionBotGradingCoefficient * sarg / arg / TempBulkPotential + czbdsw * mbp.BulkJunctionSideGradingCoefficient *
                sargsw / arg / TempBulkPotential;
            F4D = czbd * TempBulkPotential * (1 - arg * sarg) / (1 - mbp.BulkJunctionBotGradingCoefficient) + czbdsw * TempBulkPotential * (1 - arg *
                sargsw) / (1 - mbp.BulkJunctionSideGradingCoefficient) - F3D / 2 * (TempDepletionCap * TempDepletionCap) - TempDepletionCap * F2D;
            if (mbp.CapBS.Given)
            {
                czbs = TempCapBS;
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
            arg = 1 - mbp.ForwardCapDepletionCoefficient;
            sarg = Math.Exp((-mbp.BulkJunctionBotGradingCoefficient) * Math.Log(arg));
            sargsw = Math.Exp((-mbp.BulkJunctionSideGradingCoefficient) * Math.Log(arg));
            CapBS = czbs;
            CapBSSidewall = czbssw;
            F2S = czbs * (1 - mbp.ForwardCapDepletionCoefficient * (1 + mbp.BulkJunctionBotGradingCoefficient)) * sarg / arg + czbssw * (1 -
                mbp.ForwardCapDepletionCoefficient * (1 + mbp.BulkJunctionSideGradingCoefficient)) * sargsw / arg;
            F3S = czbs * mbp.BulkJunctionBotGradingCoefficient * sarg / arg / TempBulkPotential + czbssw * mbp.BulkJunctionSideGradingCoefficient *
                sargsw / arg / TempBulkPotential;
            F4S = czbs * TempBulkPotential * (1 - arg * sarg) / (1 - mbp.BulkJunctionBotGradingCoefficient) + czbssw * TempBulkPotential * (1 - arg *
                sargsw) / (1 - mbp.BulkJunctionSideGradingCoefficient) - F3S / 2 * (TempDepletionCap * TempDepletionCap) - TempDepletionCap * F2S;

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
        }
    }
}
