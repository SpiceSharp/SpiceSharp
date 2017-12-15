using System;
using SpiceSharp.Components;
using SpiceSharp.Circuits;
using SpiceSharp.Diagnostics;
using SpiceSharp.Parameters;

namespace SpiceSharp.Behaviors.DIO
{
    /// <summary>
    /// Temperature behaviour for a <see cref="Diode"/>
    /// </summary>
    public class TemperatureBehavior : CircuitObjectBehaviorTemperature
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        private ModelTemperatureBehavior modeltemp;

        /// <summary>
        /// Extra variables
        /// </summary>
        public double DIOtJctCap { get; private set; }
        public double DIOtJctPot { get; private set; }
        public double DIOtSatCur { get; private set; }
        public double DIOtF1 { get; private set; }
        public double DIOtDepCap { get; private set; }
        public double DIOtVcrit { get; private set; }
        public double DIOtBrkdwnV { get; private set; }

        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("temp"), SpiceInfo("Instance temperature")]
        public double DIO_TEMP
        {
            get => DIOtemp - Circuit.CONSTCtoK;
            set => DIOtemp.Set(value + Circuit.CONSTCtoK);
        }
        public Parameter DIOtemp { get; } = new Parameter();

        /// <summary>
        /// Private variables
        /// </summary>
        private CircuitIdentifier name;

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        /// <returns></returns>
        public override void Setup(CircuitObject component, Circuit ckt)
        {
            var dio = component as Diode;

            // Get behaviors
            modeltemp = GetBehavior<ModelTemperatureBehavior>(dio.Model);
            
            // Get name
            name = dio.Name;
        }

        /// <summary>
        /// Execute behaviour
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Temperature(Circuit ckt)
        {
            double vt, fact2, egfet, arg, pbfact, egfet1, arg1, fact1, pbfact1, pbo, gmaold, gmanew, vte, cbv, xbv, tol, iter, xcbv = 0.0;

            // loop through all the instances
            if (!DIOtemp.Given)
                DIOtemp.Value = ckt.State.Temperature;
            vt = Circuit.CONSTKoverQ * DIOtemp;
            /* this part gets really ugly - I won't even try to
			 * explain these equations */
            fact2 = DIOtemp / Circuit.CONSTRefTemp;
            egfet = 1.16 - (7.02e-4 * DIOtemp * DIOtemp) / (DIOtemp + 1108);
            arg = -egfet / (2 * Circuit.CONSTBoltz * DIOtemp) + 1.1150877 / (Circuit.CONSTBoltz * (Circuit.CONSTRefTemp +
                Circuit.CONSTRefTemp));
            pbfact = -2 * vt * (1.5 * Math.Log(fact2) + Circuit.CHARGE * arg);
            egfet1 = 1.16 - (7.02e-4 * modeltemp.DIOnomTemp * modeltemp.DIOnomTemp) / (modeltemp.DIOnomTemp + 1108);
            arg1 = -egfet1 / (Circuit.CONSTBoltz * 2 * modeltemp.DIOnomTemp) + 1.1150877 / (2 * Circuit.CONSTBoltz * Circuit.CONSTRefTemp);
            fact1 = modeltemp.DIOnomTemp / Circuit.CONSTRefTemp;
            pbfact1 = -2 * modeltemp.vtnom * (1.5 * Math.Log(fact1) + Circuit.CHARGE * arg1);
            pbo = (modeltemp.DIOjunctionPot - pbfact1) / fact1;
            gmaold = (modeltemp.DIOjunctionPot - pbo) / pbo;
            DIOtJctCap = modeltemp.DIOjunctionCap / (1 + modeltemp.DIOgradingCoeff * (400e-6 * (modeltemp.DIOnomTemp - Circuit.CONSTRefTemp) - gmaold));
            DIOtJctPot = pbfact + fact2 * pbo;
            gmanew = (DIOtJctPot - pbo) / pbo;
            DIOtJctCap *= 1 + modeltemp.DIOgradingCoeff * (400e-6 * (DIOtemp - Circuit.CONSTRefTemp) - gmanew);

            DIOtSatCur = modeltemp.DIOsatCur * Math.Exp(((DIOtemp / modeltemp.DIOnomTemp) - 1) * modeltemp.DIOactivationEnergy /
                (modeltemp.DIOemissionCoeff * vt) + modeltemp.DIOsaturationCurrentExp / modeltemp.DIOemissionCoeff * Math.Log(DIOtemp / modeltemp.DIOnomTemp));
            /* the defintion of f1, just recompute after temperature adjusting
			 * all the variables used in it */
            DIOtF1 = DIOtJctPot * (1 - Math.Exp((1 - modeltemp.DIOgradingCoeff) * modeltemp.xfc)) / (1 - modeltemp.DIOgradingCoeff);
            /* same for Depletion Capacitance */
            DIOtDepCap = modeltemp.DIOdepletionCapCoeff * DIOtJctPot;
            /* and Vcrit */
            vte = modeltemp.DIOemissionCoeff * vt;
            DIOtVcrit = vte * Math.Log(vte / (Circuit.CONSTroot2 * DIOtSatCur));
            /* and now to copute the breakdown voltage, again, using
			 * temperature adjusted basic parameters */
            if (modeltemp.DIObreakdownVoltage.Given)
            {
                cbv = modeltemp.DIObreakdownCurrent;
                if (cbv < DIOtSatCur * modeltemp.DIObreakdownVoltage / vt)
                {
                    cbv = DIOtSatCur * modeltemp.DIObreakdownVoltage / vt;
                    CircuitWarning.Warning(this, $"{name}: breakdown current increased to {cbv.ToString("g")} to resolve incompatability with specified saturation current");
                    xbv = modeltemp.DIObreakdownVoltage;
                }
                else
                {
                    tol = ckt.Simulation.CurrentConfig.RelTol * cbv;
                    xbv = modeltemp.DIObreakdownVoltage - vt * Math.Log(1 + cbv / DIOtSatCur);
                    iter = 0;
                    for (iter = 0; iter < 25; iter++)
                    {
                        xbv = modeltemp.DIObreakdownVoltage - vt * Math.Log(cbv / DIOtSatCur + 1 - xbv / vt);
                        xcbv = DIOtSatCur * (Math.Exp((modeltemp.DIObreakdownVoltage - xbv) / vt) - 1 + xbv / vt);
                        if (Math.Abs(xcbv - cbv) <= tol) goto matched;
                    }
                    CircuitWarning.Warning(this, $"{name}: unable to match forward and reverse diode regions: bv = {xbv.ToString("g")}, ibv = {xcbv.ToString("g")}");
                }
                matched:
                DIOtBrkdwnV = xbv;
            }
        }
    }
}
