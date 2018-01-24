using System;
using SpiceSharp.Components.DIO;
using SpiceSharp.Components;
using SpiceSharp.Circuits;
using SpiceSharp.Diagnostics;
using SpiceSharp.Simulations;

namespace SpiceSharp.Behaviors.DIO
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
        public double DIOtJctCap { get; protected set; }
        public double DIOtJctPot { get; protected set; }
        public double DIOtSatCur { get; protected set; }
        public double DIOtF1 { get; protected set; }
        public double DIOtDepCap { get; protected set; }
        public double DIOtVcrit { get; protected set; }
        public double DIOtBrkdwnV { get; protected set; }

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
            // Get parameters
            bp = provider.GetParameters<BaseParameters>();
            mbp = provider.GetParameters<ModelBaseParameters>(1);

            // Get behaviors
            modeltemp = provider.GetBehavior<ModelTemperatureBehavior>(1);
        }
        
        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        /// <returns></returns>
        public override void Setup(Entity component, Circuit ckt)
        {
            var dio = component as Diode;

            // Get behaviors
            modeltemp = GetBehavior<ModelTemperatureBehavior>(dio.Model);
        }

        /// <summary>
        /// Do temperature-dependent calculations
        /// </summary>
        /// <param name="sim">Base simulation</param>
        public override void Temperature(BaseSimulation sim)
        {
            double vt, fact2, egfet, arg, pbfact, egfet1, arg1, fact1, pbfact1, pbo, gmaold, gmanew, vte, cbv, xbv, tol, iter, xcbv = 0.0;

            // loop through all the instances
            if (!bp.DIOtemp.Given)
                bp.DIOtemp.Value = sim.State.Temperature;
            vt = Circuit.CONSTKoverQ * bp.DIOtemp;

            // this part gets really ugly - I won't even try to explain these equations
            fact2 = bp.DIOtemp / Circuit.CONSTRefTemp;
            egfet = 1.16 - (7.02e-4 * bp.DIOtemp * bp.DIOtemp) / (bp.DIOtemp + 1108);
            arg = -egfet / (2 * Circuit.CONSTBoltz * bp.DIOtemp) + 1.1150877 / (Circuit.CONSTBoltz * (Circuit.CONSTRefTemp +
                Circuit.CONSTRefTemp));
            pbfact = -2 * vt * (1.5 * Math.Log(fact2) + Circuit.CHARGE * arg);
            egfet1 = 1.16 - (7.02e-4 * mbp.DIOnomTemp * mbp.DIOnomTemp) / (mbp.DIOnomTemp + 1108);
            arg1 = -egfet1 / (Circuit.CONSTBoltz * 2 * mbp.DIOnomTemp) + 1.1150877 / (2 * Circuit.CONSTBoltz * Circuit.CONSTRefTemp);
            fact1 = mbp.DIOnomTemp / Circuit.CONSTRefTemp;
            pbfact1 = -2 * modeltemp.vtnom * (1.5 * Math.Log(fact1) + Circuit.CHARGE * arg1);
            pbo = (mbp.DIOjunctionPot - pbfact1) / fact1;
            gmaold = (mbp.DIOjunctionPot - pbo) / pbo;
            DIOtJctCap = mbp.DIOjunctionCap / (1 + mbp.DIOgradingCoeff * (400e-6 * (mbp.DIOnomTemp - Circuit.CONSTRefTemp) - gmaold));
            DIOtJctPot = pbfact + fact2 * pbo;
            gmanew = (DIOtJctPot - pbo) / pbo;
            DIOtJctCap *= 1 + mbp.DIOgradingCoeff * (400e-6 * (bp.DIOtemp - Circuit.CONSTRefTemp) - gmanew);

            DIOtSatCur = mbp.DIOsatCur * Math.Exp(((bp.DIOtemp / mbp.DIOnomTemp) - 1) * mbp.DIOactivationEnergy /
                (mbp.DIOemissionCoeff * vt) + mbp.DIOsaturationCurrentExp / mbp.DIOemissionCoeff * Math.Log(bp.DIOtemp / mbp.DIOnomTemp));

            // the defintion of f1, just recompute after temperature adjusting all the variables used in it
            DIOtF1 = DIOtJctPot * (1 - Math.Exp((1 - mbp.DIOgradingCoeff) * modeltemp.xfc)) / (1 - mbp.DIOgradingCoeff);

            // same for Depletion Capacitance
            DIOtDepCap = mbp.DIOdepletionCapCoeff * DIOtJctPot;
            
            // and Vcrit
            vte = mbp.DIOemissionCoeff * vt;
            DIOtVcrit = vte * Math.Log(vte / (Circuit.CONSTroot2 * DIOtSatCur));

            // and now to copute the breakdown voltage, again, using temperature adjusted basic parameters
            if (mbp.DIObreakdownVoltage.Given)
            {
                cbv = mbp.DIObreakdownCurrent;
                if (cbv < DIOtSatCur * mbp.DIObreakdownVoltage / vt)
                {
                    cbv = DIOtSatCur * mbp.DIObreakdownVoltage / vt;
                    CircuitWarning.Warning(this, $"{Name}: breakdown current increased to {cbv.ToString("g")} to resolve incompatability with specified saturation current");
                    xbv = mbp.DIObreakdownVoltage;
                }
                else
                {
                    tol = sim.CurrentConfig.RelTol * cbv;
                    xbv = mbp.DIObreakdownVoltage - vt * Math.Log(1 + cbv / DIOtSatCur);
                    iter = 0;
                    for (iter = 0; iter < 25; iter++)
                    {
                        xbv = mbp.DIObreakdownVoltage - vt * Math.Log(cbv / DIOtSatCur + 1 - xbv / vt);
                        xcbv = DIOtSatCur * (Math.Exp((mbp.DIObreakdownVoltage - xbv) / vt) - 1 + xbv / vt);
                        if (Math.Abs(xcbv - cbv) <= tol)
                            break;
                    }
                    if (iter >= 25)
                        CircuitWarning.Warning(this, $"{Name}: unable to match forward and reverse diode regions: bv = {xbv.ToString("g")}, ibv = {xcbv.ToString("g")}");
                }
                DIOtBrkdwnV = xbv;
            }
        }
    }
}
