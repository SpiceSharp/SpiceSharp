using System;
using SpiceSharp.Diagnostics;
using SpiceSharp.Simulations;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.DiodeBehaviors
{
    /// <summary>
    /// Temperature behavior for a <see cref="Diode"/>
    /// </summary>
    public class TemperatureBehavior : Behaviors.TemperatureBehavior
    {
        /// <summary>
        /// Necessary parameters and behaviors
        /// </summary>
        private BaseParameters _bp;
        private ModelBaseParameters _mbp;
        private ModelTemperatureBehavior _modeltemp;

        /// <summary>
        /// Extra variables
        /// </summary>
        public double TempJunctionCap { get; protected set; }
        public double TempJunctionPot { get; protected set; }
        public double TempSaturationCurrent { get; protected set; }
        public double TempFactor1 { get; protected set; }
        public double TempDepletionCap { get; protected set; }
        public double TempVCritical { get; protected set; }
        public double TempBreakdownVoltage { get; protected set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public TemperatureBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Setup the behavior
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

            double vt, fact2, egfet, arg, pbfact, egfet1, arg1, fact1, pbfact1, pbo, gmaold, gmanew, vte, cbv, xbv, tol, iter, xcbv = 0.0;

            // loop through all the instances
            if (!_bp.Temperature.Given)
                _bp.Temperature.Value = simulation.RealState.Temperature;
            vt = Circuit.KOverQ * _bp.Temperature;

            // this part gets really ugly - I won't even try to explain these equations
            fact2 = _bp.Temperature / Circuit.ReferenceTemperature;
            egfet = 1.16 - (7.02e-4 * _bp.Temperature * _bp.Temperature) / (_bp.Temperature + 1108);
            arg = -egfet / (2 * Circuit.Boltzmann * _bp.Temperature) + 1.1150877 / (Circuit.Boltzmann * (Circuit.ReferenceTemperature +
                Circuit.ReferenceTemperature));
            pbfact = -2 * vt * (1.5 * Math.Log(fact2) + Circuit.Charge * arg);
            egfet1 = 1.16 - (7.02e-4 * _mbp.NominalTemperature * _mbp.NominalTemperature) / (_mbp.NominalTemperature + 1108);
            arg1 = -egfet1 / (Circuit.Boltzmann * 2 * _mbp.NominalTemperature) + 1.1150877 / (2 * Circuit.Boltzmann * Circuit.ReferenceTemperature);
            fact1 = _mbp.NominalTemperature / Circuit.ReferenceTemperature;
            pbfact1 = -2 * _modeltemp.VtNominal * (1.5 * Math.Log(fact1) + Circuit.Charge * arg1);
            pbo = (_mbp.JunctionPotential - pbfact1) / fact1;
            gmaold = (_mbp.JunctionPotential - pbo) / pbo;
            TempJunctionCap = _mbp.JunctionCap / (1 + _mbp.GradingCoefficient * (400e-6 * (_mbp.NominalTemperature - Circuit.ReferenceTemperature) - gmaold));
            TempJunctionPot = pbfact + fact2 * pbo;
            gmanew = (TempJunctionPot - pbo) / pbo;
            TempJunctionCap *= 1 + _mbp.GradingCoefficient * (400e-6 * (_bp.Temperature - Circuit.ReferenceTemperature) - gmanew);

            TempSaturationCurrent = _mbp.SaturationCurrent * Math.Exp(((_bp.Temperature / _mbp.NominalTemperature) - 1) * _mbp.ActivationEnergy /
                (_mbp.EmissionCoefficient * vt) + _mbp.SaturationCurrentExp / _mbp.EmissionCoefficient * Math.Log(_bp.Temperature / _mbp.NominalTemperature));

            // the defintion of f1, just recompute after temperature adjusting all the variables used in it
            TempFactor1 = TempJunctionPot * (1 - Math.Exp((1 - _mbp.GradingCoefficient) * _modeltemp.Xfc)) / (1 - _mbp.GradingCoefficient);

            // same for Depletion Capacitance
            TempDepletionCap = _mbp.DepletionCapCoefficient * TempJunctionPot;

            // and Vcrit
            vte = _mbp.EmissionCoefficient * vt;
            TempVCritical = vte * Math.Log(vte / (Circuit.Root2 * TempSaturationCurrent));

            // and now to copute the breakdown voltage, again, using temperature adjusted basic parameters
            if (_mbp.BreakdownVoltage.Given)
            {
                cbv = _mbp.BreakdownCurrent;
                if (cbv < TempSaturationCurrent * _mbp.BreakdownVoltage / vt)
                {
                    cbv = TempSaturationCurrent * _mbp.BreakdownVoltage / vt;
                    CircuitWarning.Warning(this, "{0}: breakdown current increased to {1:g} to resolve incompatability with specified saturation current".FormatString(Name, cbv));
                    xbv = _mbp.BreakdownVoltage;
                }
                else
                {
                    tol = simulation.BaseConfiguration.RelativeTolerance * cbv;
                    xbv = _mbp.BreakdownVoltage - vt * Math.Log(1 + cbv / TempSaturationCurrent);
                    iter = 0;
                    for (iter = 0; iter < 25; iter++)
                    {
                        xbv = _mbp.BreakdownVoltage - vt * Math.Log(cbv / TempSaturationCurrent + 1 - xbv / vt);
                        xcbv = TempSaturationCurrent * (Math.Exp((_mbp.BreakdownVoltage - xbv) / vt) - 1 + xbv / vt);
                        if (Math.Abs(xcbv - cbv) <= tol)
                            break;
                    }
                    if (iter >= 25)
                        CircuitWarning.Warning(this, "{0}: unable to match forward and reverse diode regions: bv = {1:g}, ibv = {2:g}".FormatString(Name, xbv, xcbv));
                }
                TempBreakdownVoltage = xbv;
            }
        }
    }
}
