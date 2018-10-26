using System;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.MosfetBehaviors.Common
{
    public abstract class TemperatureBehavior : BaseTemperatureBehavior
    {
        private BaseParameters _bp;
        private ModelBaseParameters _mbp;
        private ModelTemperatureBehavior _modeltemp;

        /// <summary>
        /// Gets or sets the source conductance.
        /// </summary>
        /// <value>
        /// The source conductance.
        /// </value>
        [ParameterName("sourceconductance"), ParameterInfo("Conductance of source")]
        public double SourceConductance { get; protected set; }

        /// <summary>
        /// Gets or sets the drain conductance.
        /// </summary>
        /// <value>
        /// The drain conductance.
        /// </value>
        [ParameterName("drainconductance"), ParameterInfo("Conductance of drain")]
        public double DrainConductance { get; protected set; }

        public double TempSurfaceMobility { get; protected set; }
        public double TempPhi { get; protected set; }
        public double TempVoltageBi { get; protected set; }
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

        [ParameterName("cbd0"), ParameterInfo("Zero-Bias B-D junction capacitance")]
        public double CapBd { get; protected set; }
        [ParameterName("cbdsw0"), ParameterInfo("Zero-Bias B-D sidewall capacitance")]
        public double CapBdSidewall { get; protected set; }
        [ParameterName("cbs0"), ParameterInfo("Zero-Bias B-S junction capacitance")]
        public double CapBs { get; protected set; }
        [ParameterName("cbssw0"), ParameterInfo("Zero-Bias B-S sidewall capacitance")]
        public double CapBsSidewall { get; protected set; }

        /// <summary>
        /// Gets the source resistance.
        /// </summary>
        /// <value>
        /// The source resistance.
        /// </value>
        [ParameterName("rs"), ParameterInfo("Source resistance")]
        public double SourceResistance
        {
            get
            {
                if (SourceConductance > 0.0)
                    return 1.0 / SourceConductance;
                return 0.0;
            }
        }

        /// <summary>
        /// Gets the drain resistance.
        /// </summary>
        /// <value>
        /// The drain resistance.
        /// </value>
        [ParameterName("rd"), ParameterInfo("Drain conductance")]
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
        /// Gets or sets the critical source voltage.
        /// </summary>
        /// <value>
        /// The critical source voltage.
        /// </value>
        [ParameterName("sourcevcrit"), ParameterInfo("Critical source voltage")]
        public double SourceVCritical { get; protected set; }

        /// <summary>
        /// Gets or sets the critical drain voltage.
        /// </summary>
        /// <value>
        /// The drain drain voltage.
        /// </value>
        [ParameterName("drainvcrit"), ParameterInfo("Critical drain voltage")]
        public double DrainVCritical { get; protected set; }

        public double TempSaturationCurrent { get; protected set; }
        public double TempSaturationCurrentDensity { get; protected set; }
        public double TempTransconductance { get; protected set; }
        public double TempVt0 { get; protected set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="TemperatureBehavior"/> class.
        /// </summary>
        /// <param name="name">The identifier of the behavior.</param>
        /// <remarks>
        /// The identifier of the behavior should be the same as that of the entity creating it.
        /// </remarks>
        protected TemperatureBehavior(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Setup the behavior.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="provider">The data provider.</param>
        public override void Setup(Simulation simulation, SetupDataProvider provider)
        {
            _bp = provider.GetParameterSet<BaseParameters>();
            _mbp = provider.GetParameterSet<ModelBaseParameters>("model");
            _modeltemp = provider.GetBehavior<ModelTemperatureBehavior>("model");
        }

        /// <summary>
        /// Destroy the behavior.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public override void Unsetup(Simulation simulation)
        {
            _bp = null;
            _mbp = null;
            _modeltemp = null;
        }

        /// <summary>
        /// Perform temperature-dependent calculations.
        /// </summary>
        /// <param name="simulation">The base simulation.</param>
        public override void Temperature(BaseSimulation simulation)
        {
            if (!_bp.Temperature.Given)
                _bp.Temperature.RawValue = simulation.RealState.Temperature;
            var vt = _bp.Temperature * Circuit.KOverQ;
            var ratio = _bp.Temperature / _mbp.NominalTemperature;
            var fact2 = _bp.Temperature / Circuit.ReferenceTemperature;
            var kt = _bp.Temperature * Circuit.Boltzmann;
            var egfet = 1.16 - 7.02e-4 * _bp.Temperature * _bp.Temperature / (_bp.Temperature + 1108);
            var arg = -egfet / (kt + kt) + 1.1150877 / (Circuit.Boltzmann * (Circuit.ReferenceTemperature + Circuit.ReferenceTemperature));
            var pbfact = -2 * vt * (1.5 * Math.Log(fact2) + Circuit.Charge * arg);

            if (_mbp.DrainResistance.Given)
            {
                if (!_mbp.DrainResistance.Value.Equals(0.0))
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
                if (!_mbp.SheetResistance.Value.Equals(0.0))
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
                if (!_mbp.SourceResistance.Value.Equals(0.0))
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
                if (!_mbp.SheetResistance.Value.Equals(0.0))
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
            var ratio4 = ratio * Math.Sqrt(ratio);
            TempTransconductance = _mbp.Transconductance / ratio4;
            TempSurfaceMobility = _mbp.SurfaceMobility / ratio4;
            var phio = (_mbp.Phi - _modeltemp.PbFactor1) / _modeltemp.Factor1;
            TempPhi = fact2 * phio + pbfact;
            TempVoltageBi = _mbp.Vt0 - _mbp.MosfetType * (_mbp.Gamma * Math.Sqrt(_mbp.Phi)) + .5 * (_modeltemp.EgFet1 - egfet) +
                _mbp.MosfetType * .5 * (TempPhi - _mbp.Phi);
            TempVt0 = TempVoltageBi + _mbp.MosfetType * _mbp.Gamma * Math.Sqrt(TempPhi);
            TempSaturationCurrent = _mbp.JunctionSatCur * Math.Exp(-egfet / vt + _modeltemp.EgFet1 / _modeltemp.VtNominal);
            TempSaturationCurrentDensity = _mbp.JunctionSatCurDensity * Math.Exp(-egfet / vt + _modeltemp.EgFet1 / _modeltemp.VtNominal);
            var pbo = (_mbp.BulkJunctionPotential - _modeltemp.PbFactor1) / _modeltemp.Factor1;
            var gmaold = (_mbp.BulkJunctionPotential - pbo) / pbo;
            var capfact = 1 / (1 + _mbp.BulkJunctionBotGradingCoefficient * (4e-4 * (_mbp.NominalTemperature - Circuit.ReferenceTemperature) - gmaold));
            TempCapBd = _mbp.CapBd * capfact;
            TempCapBs = _mbp.CapBs * capfact;
            TempJunctionCap = _mbp.BulkCapFactor * capfact;
            capfact = 1 / (1 + _mbp.BulkJunctionSideGradingCoefficient * (4e-4 * (_mbp.NominalTemperature - Circuit.ReferenceTemperature) - gmaold));
            TempJunctionCapSidewall = _mbp.SidewallCapFactor * capfact;
            TempBulkPotential = fact2 * pbo + pbfact;
            var gmanew = (TempBulkPotential - pbo) / pbo;
            capfact = 1 + _mbp.BulkJunctionBotGradingCoefficient * (4e-4 * (_bp.Temperature - Circuit.ReferenceTemperature) - gmanew);
            TempCapBd *= capfact;
            TempCapBs *= capfact;
            TempJunctionCap *= capfact;
            capfact = 1 + _mbp.BulkJunctionSideGradingCoefficient * (4e-4 * (_bp.Temperature - Circuit.ReferenceTemperature) - gmanew);
            TempJunctionCapSidewall *= capfact;
            TempDepletionCap = _mbp.ForwardCapDepletionCoefficient * TempBulkPotential;

            CalculateCriticalVoltages();

            double cz, czsw;
            if (_mbp.CapBd.Given)
                cz = TempCapBd;
            else
            {
                if (_mbp.BulkCapFactor.Given)
                    cz = TempJunctionCap * _bp.DrainArea;
                else
                    cz = 0;
            }
            if (_mbp.SidewallCapFactor.Given)
                czsw = TempJunctionCapSidewall * _bp.DrainPerimeter;
            else
                czsw = 0;
            CapBd = cz;
            CapBdSidewall = czsw;

            if (_mbp.CapBs.Given)
                cz = TempCapBs;
            else
            {
                if (_mbp.BulkCapFactor.Given)
                    cz = TempJunctionCap * _bp.SourceArea;
                else
                    cz = 0;
            }
            if (_mbp.SidewallCapFactor.Given)
                czsw = TempJunctionCapSidewall * _bp.SourcePerimeter;
            else
                czsw = 0;
            CapBs = cz;
            CapBsSidewall = czsw;

            CalculateCapacitanceFactors();
        }

        /// <summary>
        /// Calculates the critical voltages.
        /// </summary>
        protected virtual void CalculateCriticalVoltages()
        {
            var vt = _bp.Temperature * Circuit.KOverQ;
            if (TempSaturationCurrentDensity <= 0 || _bp.DrainArea.Value <= 0 || _bp.SourceArea.Value <= 0)
            {
                SourceVCritical = DrainVCritical = vt * Math.Log(vt / (Circuit.Root2 * TempSaturationCurrent));
            }
            else
            {
                DrainVCritical = vt * Math.Log(vt / (Circuit.Root2 * TempSaturationCurrentDensity * _bp.DrainArea));
                SourceVCritical = vt * Math.Log(vt / (Circuit.Root2 * TempSaturationCurrentDensity * _bp.SourceArea));
            }
        }

        /// <summary>
        /// Calculates the capacitances.
        /// </summary>
        protected virtual void CalculateCapacitanceFactors()
        {
            var arg = 1 - _mbp.ForwardCapDepletionCoefficient;
            var sarg = Math.Exp(-_mbp.BulkJunctionBotGradingCoefficient * Math.Log(arg));
            var sargsw = Math.Exp(-_mbp.BulkJunctionSideGradingCoefficient * Math.Log(arg));
            F2D = CapBd * (1 - _mbp.ForwardCapDepletionCoefficient * (1 + _mbp.BulkJunctionBotGradingCoefficient)) * sarg / arg + CapBdSidewall * (1 -
                _mbp.ForwardCapDepletionCoefficient * (1 + _mbp.BulkJunctionSideGradingCoefficient)) * sargsw / arg;
            F3D = CapBd * _mbp.BulkJunctionBotGradingCoefficient * sarg / arg / TempBulkPotential + CapBdSidewall * _mbp.BulkJunctionSideGradingCoefficient *
                sargsw / arg / TempBulkPotential;
            F4D = CapBd * TempBulkPotential * (1 - arg * sarg) / (1 - _mbp.BulkJunctionBotGradingCoefficient) + CapBdSidewall * TempBulkPotential * (1 - arg *
                sargsw) / (1 - _mbp.BulkJunctionSideGradingCoefficient) - F3D / 2 * (TempDepletionCap * TempDepletionCap) - TempDepletionCap * F2D;
            
            F2S = CapBs * (1 - _mbp.ForwardCapDepletionCoefficient * (1 + _mbp.BulkJunctionBotGradingCoefficient)) * sarg / arg + CapBsSidewall * (1 -
                _mbp.ForwardCapDepletionCoefficient * (1 + _mbp.BulkJunctionSideGradingCoefficient)) * sargsw / arg;
            F3S = CapBs * _mbp.BulkJunctionBotGradingCoefficient * sarg / arg / TempBulkPotential + CapBsSidewall * _mbp.BulkJunctionSideGradingCoefficient *
                sargsw / arg / TempBulkPotential;
            F4S = CapBs * TempBulkPotential * (1 - arg * sarg) / (1 - _mbp.BulkJunctionBotGradingCoefficient) + CapBsSidewall * TempBulkPotential * (1 - arg *
                sargsw) / (1 - _mbp.BulkJunctionSideGradingCoefficient) - F3S / 2 * (TempDepletionCap * TempDepletionCap) - TempDepletionCap * F2S;
        }
    }
}
