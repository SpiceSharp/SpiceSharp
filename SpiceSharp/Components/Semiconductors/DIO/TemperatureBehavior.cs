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
        public double TJctCap { get; protected set; }
        public double TJctPot { get; protected set; }
        public double TSatCur { get; protected set; }
        public double TF1 { get; protected set; }
        public double TDepCap { get; protected set; }
        public double TVcrit { get; protected set; }
        public double TBrkdwnV { get; protected set; }

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
                bp.Temperature.Value = simulation.State.Temperature;
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
            pbfact1 = -2 * modeltemp.Vtnom * (1.5 * Math.Log(fact1) + Circuit.Charge * arg1);
            pbo = (mbp.JunctionPot - pbfact1) / fact1;
            gmaold = (mbp.JunctionPot - pbo) / pbo;
            TJctCap = mbp.JunctionCap / (1 + mbp.GradingCoeff * (400e-6 * (mbp.NominalTemperature - Circuit.ReferenceTemperature) - gmaold));
            TJctPot = pbfact + fact2 * pbo;
            gmanew = (TJctPot - pbo) / pbo;
            TJctCap *= 1 + mbp.GradingCoeff * (400e-6 * (bp.Temperature - Circuit.ReferenceTemperature) - gmanew);

            TSatCur = mbp.SaturationCurrent * Math.Exp(((bp.Temperature / mbp.NominalTemperature) - 1) * mbp.ActivationEnergy /
                (mbp.EmissionCoeff * vt) + mbp.SaturationCurrentExp / mbp.EmissionCoeff * Math.Log(bp.Temperature / mbp.NominalTemperature));

            // the defintion of f1, just recompute after temperature adjusting all the variables used in it
            TF1 = TJctPot * (1 - Math.Exp((1 - mbp.GradingCoeff) * modeltemp.Xfc)) / (1 - mbp.GradingCoeff);

            // same for Depletion Capacitance
            TDepCap = mbp.DepletionCapCoeff * TJctPot;
            
            // and Vcrit
            vte = mbp.EmissionCoeff * vt;
            TVcrit = vte * Math.Log(vte / (Circuit.Root2 * TSatCur));

            // and now to copute the breakdown voltage, again, using temperature adjusted basic parameters
            if (mbp.BreakdownVoltage.Given)
            {
                cbv = mbp.BreakdownCurrent;
                if (cbv < TSatCur * mbp.BreakdownVoltage / vt)
                {
                    cbv = TSatCur * mbp.BreakdownVoltage / vt;
                    CircuitWarning.Warning(this, $"{Name}: breakdown current increased to {cbv.ToString("g")} to resolve incompatability with specified saturation current");
                    xbv = mbp.BreakdownVoltage;
                }
                else
                {
                    tol = simulation.BaseConfiguration.RelTol * cbv;
                    xbv = mbp.BreakdownVoltage - vt * Math.Log(1 + cbv / TSatCur);
                    iter = 0;
                    for (iter = 0; iter < 25; iter++)
                    {
                        xbv = mbp.BreakdownVoltage - vt * Math.Log(cbv / TSatCur + 1 - xbv / vt);
                        xcbv = TSatCur * (Math.Exp((mbp.BreakdownVoltage - xbv) / vt) - 1 + xbv / vt);
                        if (Math.Abs(xcbv - cbv) <= tol)
                            break;
                    }
                    if (iter >= 25)
                        CircuitWarning.Warning(this, $"{Name}: unable to match forward and reverse diode regions: bv = {xbv.ToString("g")}, ibv = {xcbv.ToString("g")}");
                }
                TBrkdwnV = xbv;
            }
        }
    }
}
