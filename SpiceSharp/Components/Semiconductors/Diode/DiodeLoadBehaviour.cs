using System;
using SpiceSharp.Circuits;
using SpiceSharp.Behaviours;
using SpiceSharp.Components.Semiconductors;

namespace SpiceSharp.Components.ComponentBehaviours
{
    /// <summary>
    /// General behaviour for <see cref="Diode"/>
    /// </summary>
    public class DiodeLoadBehaviour : CircuitObjectBehaviourLoad
    {
        /// <summary>
        /// Behaviour
        /// </summary>
        /// <param name="ckt"></param>
        public override void Execute(Circuit ckt)
        {
            var diode = ComponentTyped<Diode>();

            var model = diode.Model as DiodeModel;
            var state = ckt.State;
            var rstate = state.Real;
            var method = ckt.Method;
            bool Check;
            double csat, gspr, vt, vte, vd, delvd, cdhat, vdtemp, evd, cd, gd, arg, evrev, czero, sarg, capd, czof2, cdeq;

            /* 
             * this routine loads diodes for dc and transient analyses.
             */
            csat = diode.DIOtSatCur * diode.DIOarea;
            gspr = model.DIOconductance * diode.DIOarea;
            vt = Circuit.CONSTKoverQ * diode.DIOtemp;
            vte = model.DIOemissionCoeff * vt;

            /* 
			 * initialization 
			 */
            Check = true;
            if (state.UseSmallSignal)
            {
                vd = state.States[0][diode.DIOstate + Diode.DIOvoltage];
            }
            else if (method != null && method.SavedTime == 0.0)
            {
                vd = state.States[1][diode.DIOstate + Diode.DIOvoltage];
                Check = false; // EDIT: Spice does not check the first timepoint for convergence, but we do...
            }
            else if ((state.Init == CircuitState.InitFlags.InitJct) && (state.Domain == CircuitState.DomainTypes.Time && state.UseDC) &&
              state.UseIC)
            {
                vd = diode.DIOinitCond;
            }
            else if ((state.Init == CircuitState.InitFlags.InitJct) && diode.DIOoff)
            {
                vd = 0;
            }
            else if (state.Init == CircuitState.InitFlags.InitJct)
            {
                vd = diode.DIOtVcrit;
            }
            else if (ckt.State.Init == CircuitState.InitFlags.InitFix && diode.DIOoff)
            {
                vd = 0;
            }
            else
            {
                vd = rstate.OldSolution[diode.DIOposPrimeNode] - rstate.OldSolution[diode.DIOnegNode];
                delvd = vd - state.States[0][diode.DIOstate + Diode.DIOvoltage];
                cdhat = state.States[0][diode.DIOstate + Diode.DIOcurrent] + state.States[0][diode.DIOstate + Diode.DIOconduct] * delvd;

                /* 
				 * limit new junction voltage
				 */
                if ((model.DIObreakdownVoltage.Given) && (vd < Math.Min(0, -diode.DIOtBrkdwnV + 10 * vte)))
                {
                    vdtemp = -(vd + diode.DIOtBrkdwnV);
                    vdtemp = Semiconductor.DEVpnjlim(vdtemp, -(state.States[0][diode.DIOstate + Diode.DIOvoltage] + diode.DIOtBrkdwnV), vte, diode.DIOtVcrit, ref Check);
                    vd = -(vdtemp + diode.DIOtBrkdwnV);
                }
                else
                {
                    vd = Semiconductor.DEVpnjlim(vd, state.States[0][diode.DIOstate + Diode.DIOvoltage], vte, diode.DIOtVcrit, ref Check);
                }
            }
            /* 
			 * compute dc current and derivitives
			 */
            if (vd >= -3 * vte)
            {
                evd = Math.Exp(vd / vte);
                cd = csat * (evd - 1) + state.Gmin * vd;
                gd = csat * evd / vte + state.Gmin;
            }
            else if (diode.DIOtBrkdwnV == 0.0 || vd >= -diode.DIOtBrkdwnV)
            {
                arg = 3 * vte / (vd * Circuit.CONSTE);
                arg = arg * arg * arg;
                cd = -csat * (1 + arg) + state.Gmin * vd;
                gd = csat * 3 * arg / vd + state.Gmin;
            }
            else
            {
                evrev = Math.Exp(-(diode.DIOtBrkdwnV + vd) / vte);
                cd = -csat * evrev + state.Gmin * vd;
                gd = csat * evrev / vte + state.Gmin;
            }
            if ((method != null || state.UseSmallSignal) || (state.Domain == CircuitState.DomainTypes.Time && state.UseDC) && state.UseIC)
            {
                /* 
				* charge storage elements
				*/
                czero = diode.DIOtJctCap * diode.DIOarea;
                if (vd < diode.DIOtDepCap)
                {
                    arg = 1 - vd / model.DIOjunctionPot;
                    sarg = Math.Exp(-model.DIOgradingCoeff * Math.Log(arg));
                    state.States[0][diode.DIOstate + Diode.DIOcapCharge] = model.DIOtransitTime * cd + model.DIOjunctionPot * czero * (1 - arg * sarg) / (1 -
                        model.DIOgradingCoeff);
                    capd = model.DIOtransitTime * gd + czero * sarg;
                }
                else
                {
                    czof2 = czero / model.DIOf2;
                    state.States[0][diode.DIOstate + Diode.DIOcapCharge] = model.DIOtransitTime * cd + czero * diode.DIOtF1 + czof2 * (model.DIOf3 * (vd -
                        diode.DIOtDepCap) + (model.DIOgradingCoeff / (model.DIOjunctionPot + model.DIOjunctionPot)) * (vd * vd - diode.DIOtDepCap * diode.DIOtDepCap));
                    capd = model.DIOtransitTime * gd + czof2 * (model.DIOf3 + model.DIOgradingCoeff * vd / model.DIOjunctionPot);
                }
                diode.DIOcap = capd;

                /* 
				* store small - signal parameters
				*/
                if ((!(state.Domain == CircuitState.DomainTypes.Time && state.UseDC)) || (!state.UseIC))
                {
                    if (state.UseSmallSignal)
                    {
                        state.States[0][diode.DIOstate + Diode.DIOcapCurrent] = capd;
                        return;
                    }

                    /* 
					 * transient analysis
					 */
                    if (method != null)
                    {
                        if (method.SavedTime == 0.0)
                            state.States[1][diode.DIOstate + Diode.DIOcapCharge] = state.States[0][diode.DIOstate + Diode.DIOcapCharge];
                        var result = method.Integrate(state, diode.DIOstate + Diode.DIOcapCharge, capd);
                        gd = gd + result.Geq;
                        cd = cd + state.States[0][diode.DIOstate + Diode.DIOcapCurrent];
                        if (method != null && method.SavedTime == 0.0)
                            state.States[1][diode.DIOstate + Diode.DIOcapCurrent] = state.States[0][diode.DIOstate + Diode.DIOcapCurrent];
                    }
                }
            }

            /* 
			 * check convergence
			 */
            if (((state.Init != CircuitState.InitFlags.InitFix)) || (!(diode.DIOoff)))
            {
                if (Check)
                    ckt.State.IsCon = false;
            }
            state.States[0][diode.DIOstate + Diode.DIOvoltage] = vd;
            state.States[0][diode.DIOstate + Diode.DIOcurrent] = cd;
            state.States[0][diode.DIOstate + Diode.DIOconduct] = gd;

            /* 
			 * load current vector
			 */
            cdeq = cd - gd * vd;
            rstate.Rhs[diode.DIOnegNode] += cdeq;
            rstate.Rhs[diode.DIOposPrimeNode] -= cdeq;

            /* 
			 * load matrix
			 */
            rstate.Matrix[diode.DIOposNode, diode.DIOposNode] += gspr;
            rstate.Matrix[diode.DIOnegNode, diode.DIOnegNode] += gd;
            rstate.Matrix[diode.DIOposPrimeNode, diode.DIOposPrimeNode] += (gd + gspr);
            rstate.Matrix[diode.DIOposNode, diode.DIOposPrimeNode] -= gspr;
            rstate.Matrix[diode.DIOnegNode, diode.DIOposPrimeNode] -= gd;
            rstate.Matrix[diode.DIOposPrimeNode, diode.DIOposNode] -= gspr;
            rstate.Matrix[diode.DIOposPrimeNode, diode.DIOnegNode] -= gd;
        }
    }
}
