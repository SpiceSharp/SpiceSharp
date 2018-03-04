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
        private BaseParameters _bp;
        private ModelBaseParameters _mbp;
        private ModelTemperatureBehavior _modeltemp;

        /// <summary>
        /// Extra variables
        /// </summary>
        [PropertyName("sourceconductance"), PropertyInfo("Source conductance")]
        public double SourceConductance { get; internal set; }
        [PropertyName("drainconductance"), PropertyInfo("Drain conductance")]
        public double DrainConductance { get; internal set; }
        [PropertyName("sourcevcrit"), PropertyInfo("Critical source voltage")]
        public double SourceVCritical { get; internal set; }
        [PropertyName("drainvcrit"), PropertyInfo("Critical drain voltage")]
        public double DrainVCritical { get; internal set; }
        [PropertyName("cbd0"), PropertyInfo("Zero-Bias B-D junction capacitance")]
        public double CapBd { get; internal set; }
        [PropertyName("cbdsw0"), PropertyInfo(" ")]
        public double CapBdSidewall { get; internal set; }
        [PropertyName("cbs0"), PropertyInfo("Zero-Bias B-S junction capacitance")]
        public double CapBs { get; internal set; }
        [PropertyName("cbssw0"), PropertyInfo(" ")]
        public double CapBsSidewall { get; internal set; }
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
        public double TempTransconductance { get; protected set; }
        public double TempSurfaceMobility { get; protected set; }
        public double TempPhi { get; protected set; }
        public double TempVoltageBi { get; protected set; }
        public double TempVt0 { get; protected set; }
        public double TempSaturationCurrent { get; protected set; }
        public double TempSaturationCurrentDensity { get; protected set; }
        public double TempCapBd { get; protected set; }
        public double TempCapBs { get; protected set; }
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
        public double CapGs { get; protected set; }
        public double CapGd { get; protected set; }
        public double CapGb { get; protected set; }

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
            _bp = provider.GetParameterSet<BaseParameters>("entity");
            _mbp = provider.GetParameterSet<ModelBaseParameters>("model");

            // Get behaviors
            _modeltemp = provider.GetBehavior<ModelTemperatureBehavior>("model");
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
            if (!_bp.Temperature.Given)
            {
                _bp.Temperature.Value = simulation.RealState.Temperature;
            }

            vt = _bp.Temperature * Circuit.KOverQ;
            ratio = _bp.Temperature / _mbp.NominalTemperature;
            fact2 = _bp.Temperature / Circuit.ReferenceTemperature;
            kt = _bp.Temperature * Circuit.Boltzmann;
            egfet = 1.16 - (7.02e-4 * _bp.Temperature * _bp.Temperature) / (_bp.Temperature + 1108);
            arg = -egfet / (kt + kt) + 1.1150877 / (Circuit.Boltzmann * (Circuit.ReferenceTemperature + Circuit.ReferenceTemperature));
            pbfact = -2 * vt * (1.5 * Math.Log(fact2) + Circuit.Charge * arg);

            if (_mbp.DrainResistance.Given)
            {
                if (_mbp.DrainResistance != 0)
                {
                    DrainConductance = 1 / _mbp.DrainResistance;
                }
                else
                {
                    DrainConductance = 0;
                }
            }
            else if (_mbp.SheetResistance.Given)
            {
                if (_mbp.SheetResistance != 0)
                {
                    DrainConductance = 1 / (_mbp.SheetResistance * _bp.DrainSquares);
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
            if (_mbp.SourceResistance.Given)
            {
                if (_mbp.SourceResistance != 0)
                {
                    SourceConductance = 1 / _mbp.SourceResistance;
                }
                else
                {
                    SourceConductance = 0;
                }
            }
            else if (_mbp.SheetResistance.Given)
            {
                if (_mbp.SheetResistance != 0)
                {
                    SourceConductance = 1 / (_mbp.SheetResistance * _bp.SourceSquares);
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
            if (_bp.Length - 2 * _mbp.LateralDiffusion <= 0)
                CircuitWarning.Warning(this, "{0}: effective channel length less than zero".FormatString(Name));

            ratio4 = ratio * Math.Sqrt(ratio);
            TempTransconductance = _mbp.Transconductance / ratio4;
            TempSurfaceMobility = _mbp.SurfaceMobility / ratio4;
            phio = (_mbp.Phi - _modeltemp.PbFactor1) / _modeltemp.Factor1;
            TempPhi = fact2 * phio + pbfact;
            TempVoltageBi = _mbp.Vt0 - _mbp.MosfetType * (_mbp.Gamma * Math.Sqrt(_mbp.Phi)) + .5 * (_modeltemp.EgFet1 - egfet) +
                _mbp.MosfetType * .5 * (TempPhi - _mbp.Phi);
            TempVt0 = TempVoltageBi + _mbp.MosfetType * _mbp.Gamma * Math.Sqrt(TempPhi);
            TempSaturationCurrent = _mbp.JunctionSatCur * Math.Exp(-egfet / vt + _modeltemp.EgFet1 / _modeltemp.VtNominal);
            TempSaturationCurrentDensity = _mbp.JunctionSatCurDensity * Math.Exp(-egfet / vt + _modeltemp.EgFet1 / _modeltemp.VtNominal);
            pbo = (_mbp.BulkJunctionPotential - _modeltemp.PbFactor1) / _modeltemp.Factor1;
            gmaold = (_mbp.BulkJunctionPotential - pbo) / pbo;
            capfact = 1 / (1 + _mbp.BulkJunctionBotGradingCoefficient * (4e-4 * (_mbp.NominalTemperature - Circuit.ReferenceTemperature) - gmaold));
            TempCapBd = _mbp.CapBd * capfact;
            TempCapBs = _mbp.CapBs * capfact;
            TempJunctionCap = _mbp.BulkCapFactor * capfact;
            capfact = 1 / (1 + _mbp.BulkJunctionSideGradingCoefficient * (4e-4 * (_mbp.NominalTemperature - Circuit.ReferenceTemperature) - gmaold));
            TempJunctionCapSidewall = _mbp.SidewallCapFactor * capfact;
            TempBulkPotential = fact2 * pbo + pbfact;
            gmanew = (TempBulkPotential - pbo) / pbo;
            capfact = (1 + _mbp.BulkJunctionBotGradingCoefficient * (4e-4 * (_bp.Temperature - Circuit.ReferenceTemperature) - gmanew));
            TempCapBd *= capfact;
            TempCapBs *= capfact;
            TempJunctionCap *= capfact;
            capfact = (1 + _mbp.BulkJunctionSideGradingCoefficient * (4e-4 * (_bp.Temperature - Circuit.ReferenceTemperature) - gmanew));
            TempJunctionCapSidewall *= capfact;
            TempDepletionCap = _mbp.ForwardCapDepletionCoefficient * TempBulkPotential;

            if ((TempSaturationCurrentDensity == 0) || (_bp.DrainArea.Value == 0) || (_bp.SourceArea.Value == 0))
            {
                SourceVCritical = DrainVCritical = vt * Math.Log(vt / (Circuit.Root2 * TempSaturationCurrent));
            }
            else
            {
                DrainVCritical = vt * Math.Log(vt / (Circuit.Root2 * TempSaturationCurrentDensity * _bp.DrainArea));
                SourceVCritical = vt * Math.Log(vt / (Circuit.Root2 * TempSaturationCurrentDensity * _bp.SourceArea));
            }
            if (_mbp.CapBd.Given)
            {
                czbd = TempCapBd;
            }
            else
            {
                if (_mbp.BulkCapFactor.Given)
                {
                    czbd = TempJunctionCap * _bp.DrainArea;
                }
                else
                {
                    czbd = 0;
                }
            }
            if (_mbp.SidewallCapFactor.Given)
            {
                czbdsw = TempJunctionCapSidewall * _bp.DrainPerimeter;
            }
            else
            {
                czbdsw = 0;
            }
            arg = 1 - _mbp.ForwardCapDepletionCoefficient;
            sarg = Math.Exp((-_mbp.BulkJunctionBotGradingCoefficient) * Math.Log(arg));
            sargsw = Math.Exp((-_mbp.BulkJunctionSideGradingCoefficient) * Math.Log(arg));
            CapBd = czbd;
            CapBdSidewall = czbdsw;
            F2D = czbd * (1 - _mbp.ForwardCapDepletionCoefficient * (1 + _mbp.BulkJunctionBotGradingCoefficient)) * sarg / arg + czbdsw * (1 -
                _mbp.ForwardCapDepletionCoefficient * (1 + _mbp.BulkJunctionSideGradingCoefficient)) * sargsw / arg;
            F3D = czbd * _mbp.BulkJunctionBotGradingCoefficient * sarg / arg / TempBulkPotential + czbdsw * _mbp.BulkJunctionSideGradingCoefficient *
                sargsw / arg / TempBulkPotential;
            F4D = czbd * TempBulkPotential * (1 - arg * sarg) / (1 - _mbp.BulkJunctionBotGradingCoefficient) + czbdsw * TempBulkPotential * (1 - arg *
                sargsw) / (1 - _mbp.BulkJunctionSideGradingCoefficient) - F3D / 2 * (TempDepletionCap * TempDepletionCap) - TempDepletionCap * F2D;
            if (_mbp.CapBs.Given)
            {
                czbs = TempCapBs;
            }
            else
            {
                if (_mbp.BulkCapFactor.Given)
                {
                    czbs = TempJunctionCap * _bp.SourceArea;
                }
                else
                {
                    czbs = 0;
                }
            }
            if (_mbp.SidewallCapFactor.Given)
            {
                czbssw = TempJunctionCapSidewall * _bp.SourcePerimeter;
            }
            else
            {
                czbssw = 0;
            }
            arg = 1 - _mbp.ForwardCapDepletionCoefficient;
            sarg = Math.Exp((-_mbp.BulkJunctionBotGradingCoefficient) * Math.Log(arg));
            sargsw = Math.Exp((-_mbp.BulkJunctionSideGradingCoefficient) * Math.Log(arg));
            CapBs = czbs;
            CapBsSidewall = czbssw;
            F2S = czbs * (1 - _mbp.ForwardCapDepletionCoefficient * (1 + _mbp.BulkJunctionBotGradingCoefficient)) * sarg / arg + czbssw * (1 -
                _mbp.ForwardCapDepletionCoefficient * (1 + _mbp.BulkJunctionSideGradingCoefficient)) * sargsw / arg;
            F3S = czbs * _mbp.BulkJunctionBotGradingCoefficient * sarg / arg / TempBulkPotential + czbssw * _mbp.BulkJunctionSideGradingCoefficient *
                sargsw / arg / TempBulkPotential;
            F4S = czbs * TempBulkPotential * (1 - arg * sarg) / (1 - _mbp.BulkJunctionBotGradingCoefficient) + czbssw * TempBulkPotential * (1 - arg *
                sargsw) / (1 - _mbp.BulkJunctionSideGradingCoefficient) - F3S / 2 * (TempDepletionCap * TempDepletionCap) - TempDepletionCap * F2S;
        }
    }
}
