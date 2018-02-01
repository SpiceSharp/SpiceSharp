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
        BaseParameters bp;
        ModelBaseParameters mbp;
        ModelTemperatureBehavior modeltemp;

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

            double vt, fact2, egfet, arg, pbfact, egfet1, arg1, fact1, pbfact1, pbo, gmaold, gmanew, vte, cbv, xbv, tol, iter, xcbv = 0.0;

            // loop through all the instances
            if (!bp.Temperature.Given)
                bp.Temperature.Value = simulation.RealState.Temperature;
            vt = Circuit.KOverQ * bp.Temperature;

            // this part gets really ugly - I won't even try to explain these equations
            fact2 = bp.Temperature / Circuit.ReferenceTemperature;
            egfet = 1.16 - (7.02e-4 * bp.Temperature * bp.Temperature) / (bp.Temperature + 1108);
            arg = -egfet / (2 * Circuit.Boltzmann * bp.Temperature) + 1.1150877 / (Circuit.Boltzmann * (Circuit.ReferenceTemperature +
                Circuit.ReferenceTemperature));
            pbfact = -2 * vt * (1.5 * Math.Log(fact2) + Circuit.Charge * arg);
            egfet1 = 1.16 - (7.02e-4 * mbp.NominalTemperature * mbp.NominalTemperature) / (mbp.NominalTemperature + 1108);
            arg1 = -egfet1 / (Circuit.Boltzmann * 2 * mbp.NominalTemperature) + 1.1150877 / (2 * Circuit.Boltzmann * Circuit.ReferenceTemperature);
            fact1 = mbp.NominalTemperature / Circuit.ReferenceTemperature;
            pbfact1 = -2 * modeltemp.VtNominal * (1.5 * Math.Log(fact1) + Circuit.Charge * arg1);
            pbo = (mbp.JunctionPotential - pbfact1) / fact1;
            gmaold = (mbp.JunctionPotential - pbo) / pbo;
            TempJunctionCap = mbp.JunctionCap / (1 + mbp.GradingCoefficient * (400e-6 * (mbp.NominalTemperature - Circuit.ReferenceTemperature) - gmaold));
            TempJunctionPot = pbfact + fact2 * pbo;
            gmanew = (TempJunctionPot - pbo) / pbo;
            TempJunctionCap *= 1 + mbp.GradingCoefficient * (400e-6 * (bp.Temperature - Circuit.ReferenceTemperature) - gmanew);

            TempSaturationCurrent = mbp.SaturationCurrent * Math.Exp(((bp.Temperature / mbp.NominalTemperature) - 1) * mbp.ActivationEnergy /
                (mbp.EmissionCoefficient * vt) + mbp.SaturationCurrentExp / mbp.EmissionCoefficient * Math.Log(bp.Temperature / mbp.NominalTemperature));

            // the defintion of f1, just recompute after temperature adjusting all the variables used in it
            TempFactor1 = TempJunctionPot * (1 - Math.Exp((1 - mbp.GradingCoefficient) * modeltemp.Xfc)) / (1 - mbp.GradingCoefficient);

            // same for Depletion Capacitance
            TempDepletionCap = mbp.DepletionCapCoefficient * TempJunctionPot;
            
            // and Vcrit
            vte = mbp.EmissionCoefficient * vt;
            TempVCritical = vte * Math.Log(vte / (Circuit.Root2 * TempSaturationCurrent));

            // and now to copute the breakdown voltage, again, using temperature adjusted basic parameters
            if (mbp.BreakdownVoltage.Given)
            {
                cbv = mbp.BreakdownCurrent;
                if (cbv < TempSaturationCurrent * mbp.BreakdownVoltage / vt)
                {
                    cbv = TempSaturationCurrent * mbp.BreakdownVoltage / vt;
                    CircuitWarning.Warning(this, "{0}: breakdown current increased to {1:g} to resolve incompatability with specified saturation current".FormatString(Name, cbv));
                    xbv = mbp.BreakdownVoltage;
                }
                else
                {
                    tol = simulation.BaseConfiguration.RelativeTolerance * cbv;
                    xbv = mbp.BreakdownVoltage - vt * Math.Log(1 + cbv / TempSaturationCurrent);
                    iter = 0;
                    for (iter = 0; iter < 25; iter++)
                    {
                        xbv = mbp.BreakdownVoltage - vt * Math.Log(cbv / TempSaturationCurrent + 1 - xbv / vt);
                        xcbv = TempSaturationCurrent * (Math.Exp((mbp.BreakdownVoltage - xbv) / vt) - 1 + xbv / vt);
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
