using System;
using SpiceSharp.Behaviors;
using SpiceSharp.Diagnostics;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// Temperature behaviour for a <see cref="Diode"/>
    /// </summary>
    public class DiodeTemperatureBehavior : CircuitObjectBehaviorTemperature
    {
        /// <summary>
        /// Execute behaviour
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Execute(Circuit ckt)
        {
            var dio = ComponentTyped<Diode>();
            var model = dio.Model as DiodeModel;
            double vt, fact2, egfet, arg, pbfact, egfet1, arg1, fact1, pbfact1, pbo, gmaold, gmanew, vte, cbv, xbv, tol, iter, xcbv = 0.0;

            // loop through all the instances
            if (!dio.DIOtemp.Given)
                dio.DIOtemp.Value = ckt.State.Temperature;
            vt = Circuit.CONSTKoverQ * dio.DIOtemp;
            /* this part gets really ugly - I won't even try to
			 * explain these equations */
            fact2 = dio.DIOtemp / Circuit.CONSTRefTemp;
            egfet = 1.16 - (7.02e-4 * dio.DIOtemp * dio.DIOtemp) / (dio.DIOtemp + 1108);
            arg = -egfet / (2 * Circuit.CONSTBoltz * dio.DIOtemp) + 1.1150877 / (Circuit.CONSTBoltz * (Circuit.CONSTRefTemp +
                Circuit.CONSTRefTemp));
            pbfact = -2 * vt * (1.5 * Math.Log(fact2) + Circuit.CHARGE * arg);
            egfet1 = 1.16 - (7.02e-4 * model.DIOnomTemp * model.DIOnomTemp) / (model.DIOnomTemp + 1108);
            arg1 = -egfet1 / (Circuit.CONSTBoltz * 2 * model.DIOnomTemp) + 1.1150877 / (2 * Circuit.CONSTBoltz * Circuit.CONSTRefTemp);
            fact1 = model.DIOnomTemp / Circuit.CONSTRefTemp;
            pbfact1 = -2 * model.vtnom * (1.5 * Math.Log(fact1) + Circuit.CHARGE * arg1);
            pbo = (model.DIOjunctionPot - pbfact1) / fact1;
            gmaold = (model.DIOjunctionPot - pbo) / pbo;
            dio.DIOtJctCap = model.DIOjunctionCap / (1 + model.DIOgradingCoeff * (400e-6 * (model.DIOnomTemp - Circuit.CONSTRefTemp) - gmaold));
            dio.DIOtJctPot = pbfact + fact2 * pbo;
            gmanew = (dio.DIOtJctPot - pbo) / pbo;
            dio.DIOtJctCap *= 1 + model.DIOgradingCoeff * (400e-6 * (dio.DIOtemp - Circuit.CONSTRefTemp) - gmanew);

            dio.DIOtSatCur = model.DIOsatCur * Math.Exp(((dio.DIOtemp / model.DIOnomTemp) - 1) * model.DIOactivationEnergy /
                (model.DIOemissionCoeff * vt) + model.DIOsaturationCurrentExp / model.DIOemissionCoeff * Math.Log(dio.DIOtemp / model.DIOnomTemp));
            /* the defintion of f1, just recompute after temperature adjusting
			 * all the variables used in it */
            dio.DIOtF1 = dio.DIOtJctPot * (1 - Math.Exp((1 - model.DIOgradingCoeff) * model.xfc)) / (1 - model.DIOgradingCoeff);
            /* same for Depletion Capacitance */
            dio.DIOtDepCap = model.DIOdepletionCapCoeff * dio.DIOtJctPot;
            /* and Vcrit */
            vte = model.DIOemissionCoeff * vt;
            dio.DIOtVcrit = vte * Math.Log(vte / (Circuit.CONSTroot2 * dio.DIOtSatCur));
            /* and now to copute the breakdown voltage, again, using
			 * temperature adjusted basic parameters */
            if (model.DIObreakdownVoltage.Given)
            {
                cbv = model.DIObreakdownCurrent;
                if (cbv < dio.DIOtSatCur * model.DIObreakdownVoltage / vt)
                {
                    cbv = dio.DIOtSatCur * model.DIObreakdownVoltage / vt;
                    CircuitWarning.Warning(this, $"{dio.Name}: breakdown current increased to {cbv.ToString("g")} to resolve incompatability with specified saturation current");
                    xbv = model.DIObreakdownVoltage;
                }
                else
                {
                    tol = ckt.Simulation.Config.RelTol * cbv;
                    xbv = model.DIObreakdownVoltage - vt * Math.Log(1 + cbv / dio.DIOtSatCur);
                    iter = 0;
                    for (iter = 0; iter < 25; iter++)
                    {
                        xbv = model.DIObreakdownVoltage - vt * Math.Log(cbv / dio.DIOtSatCur + 1 - xbv / vt);
                        xcbv = dio.DIOtSatCur * (Math.Exp((model.DIObreakdownVoltage - xbv) / vt) - 1 + xbv / vt);
                        if (Math.Abs(xcbv - cbv) <= tol) goto matched;
                    }
                    CircuitWarning.Warning(this, $"{dio.Name}: unable to match forward and reverse diode regions: bv = {xbv.ToString("g")}, ibv = {xcbv.ToString("g")}");
                }
                matched:
                dio.DIOtBrkdwnV = xbv;
            }
        }
    }
}
