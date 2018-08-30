using System;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.BipolarBehaviors
{
    /// <summary>
    /// Temperature behavior for a <see cref="BipolarJunctionTransistor"/>
    /// </summary>
    public class TemperatureBehavior : BaseTemperatureBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        private BaseParameters _bp;
        private ModelBaseParameters _mbp;
        private ModelTemperatureBehavior _modeltemp;

        /// <summary>
        /// Shared parameters
        /// </summary>
        public double TempSaturationCurrent { get; protected set; }
        public double TempBetaForward { get; protected set; }
        public double TempBetaReverse { get; protected set; }
        public double TempBeLeakageCurrent { get; protected set; }
        public double TempBcLeakageCurrent { get; protected set; }
        public double TempBeCap { get; protected set; }
        public double TempBePotential { get; protected set; }
        public double TempBcCap { get; protected set; }
        public double TempBcPotential { get; protected set; }
        public double TempDepletionCap { get; protected set; }
        public double TempFactor1 { get; protected set; }
        public double TempFactor4 { get; protected set; }
        public double TempFactor5 { get; protected set; }
        public double TempVCritical { get; protected set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public TemperatureBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Setup behavior
        /// </summary>
        /// <param name="simulation">Simulation</param>
        /// <param name="provider">Data provider</param>
        public override void Setup(Simulation simulation, SetupDataProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            // Get parameters
            _bp = provider.GetParameterSet<BaseParameters>();
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

            if (!_bp.Temperature.Given)
                _bp.Temperature.RawValue = simulation.RealState.Temperature;
            var vt = _bp.Temperature * Circuit.KOverQ;
            var fact2 = _bp.Temperature / Circuit.ReferenceTemperature;
            var egfet = 1.16 - 7.02e-4 * _bp.Temperature * _bp.Temperature / (_bp.Temperature + 1108);
            var arg = -egfet / (2 * Circuit.Boltzmann * _bp.Temperature) + 1.1150877 / (Circuit.Boltzmann * (Circuit.ReferenceTemperature +
                                                                                                                Circuit.ReferenceTemperature));
            var pbfact = -2 * vt * (1.5 * Math.Log(fact2) + Circuit.Charge * arg);

            var ratlog = Math.Log(_bp.Temperature / _mbp.NominalTemperature);
            var ratio1 = _bp.Temperature / _mbp.NominalTemperature - 1;
            var factlog = ratio1 * _mbp.EnergyGap / vt + _mbp.TempExpIs * ratlog;
            var factor = Math.Exp(factlog);
            TempSaturationCurrent = _mbp.SatCur * factor;
            var bfactor = Math.Exp(ratlog * _mbp.BetaExponent);
            TempBetaForward = _mbp.BetaF * bfactor;
            TempBetaReverse = _mbp.BetaR * bfactor;
            TempBeLeakageCurrent = _mbp.LeakBeCurrent * Math.Exp(factlog / _mbp.LeakBeEmissionCoefficient) / bfactor;
            TempBcLeakageCurrent = _mbp.LeakBcCurrent * Math.Exp(factlog / _mbp.LeakBcEmissionCoefficient) / bfactor;

            var pbo = (_mbp.PotentialBe - pbfact) / _modeltemp.Factor1;
            var gmaold = (_mbp.PotentialBe - pbo) / pbo;
            TempBeCap = _mbp.DepletionCapBe / (1 + _mbp.JunctionExpBe * (4e-4 * (_mbp.NominalTemperature - Circuit.ReferenceTemperature) - gmaold));
            TempBePotential = fact2 * pbo + pbfact;
            var gmanew = (TempBePotential - pbo) / pbo;
            TempBeCap *= 1 + _mbp.JunctionExpBe * (4e-4 * (_bp.Temperature - Circuit.ReferenceTemperature) - gmanew);

            pbo = (_mbp.PotentialBc - pbfact) / _modeltemp.Factor1;
            gmaold = (_mbp.PotentialBc - pbo) / pbo;
            TempBcCap = _mbp.DepletionCapBc / (1 + _mbp.JunctionExpBc * (4e-4 * (_mbp.NominalTemperature - Circuit.ReferenceTemperature) - gmaold));
            TempBcPotential = fact2 * pbo + pbfact;
            gmanew = (TempBcPotential - pbo) / pbo;
            TempBcCap *= 1 + _mbp.JunctionExpBc * (4e-4 * (_bp.Temperature - Circuit.ReferenceTemperature) - gmanew);

            TempDepletionCap = _mbp.DepletionCapCoefficient * TempBePotential;
            TempFactor1 = TempBePotential * (1 - Math.Exp((1 - _mbp.JunctionExpBe) * _modeltemp.Xfc)) / (1 - _mbp.JunctionExpBe);
            TempFactor4 = _mbp.DepletionCapCoefficient * TempBcPotential;
            TempFactor5 = TempBcPotential * (1 - Math.Exp((1 - _mbp.JunctionExpBc) * _modeltemp.Xfc)) / (1 - _mbp.JunctionExpBc);
            TempVCritical = vt * Math.Log(vt / (Circuit.Root2 * _mbp.SatCur));
        }
    }
}
