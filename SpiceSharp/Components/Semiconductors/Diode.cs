using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using SpiceSharp.Circuits;
using SpiceSharp.Parameters;
using SpiceSharp.Diagnostics;
using SpiceSharp.Components.Semiconductors;

namespace SpiceSharp.Components
{
    /// <summary>
    /// This class represents a diode
    /// </summary>
    public class Diode : CircuitComponent
    {
        /// <summary>
        /// Gets or sets the diode model
        /// </summary>
        public DiodeModel Model { get; set; }

        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("off"), SpiceInfo("Initially off", Interesting = false)]
        public bool DIOoff { get; set; } = false;
        [SpiceName("temp"), SpiceInfo("Instance temperature", Interesting = false)]
        public ParameterMethod<double> DIOtemp { get; } = new ParameterMethod<double>(300.15, (double celsius) => celsius + Circuit.CONSTCtoK, (double kelvin) => kelvin - Circuit.CONSTCtoK);
        [SpiceName("ic"), SpiceInfo("Initial device voltage", Interesting = false)]
        public Parameter<double> DIOinitCond { get; } = new Parameter<double>();
        [SpiceName("area"), SpiceInfo("Area factor", Interesting = false)]
        public Parameter<double> DIOarea { get; } = new Parameter<double>(1.0);
        [SpiceName("vd"), SpiceInfo("Diode voltage")]
        public double GetVoltage(Circuit ckt) => ckt.State.States[0][DIOstate + DIOvoltage];
        [SpiceName("id"), SpiceName("c"), SpiceInfo("Diode current")]
        public double GetCurrent(Circuit ckt) => ckt.State.States[0][DIOstate + DIOcurrent];
        [SpiceName("cd"), SpiceInfo("Diode capacitance")]
        public double DIOcap { get; private set; }
        [SpiceName("charge"), SpiceInfo("Diode capacitor charge", Interesting = false)]
        public double GetCapCharge(Circuit ckt) => ckt.State.States[0][DIOstate + DIOcapCharge];
        [SpiceName("capcur"), SpiceInfo("Diode capacitor current", Interesting = false)]
        public double GetCapCurrent(Circuit ckt) => ckt.State.States[0][DIOstate + DIOcapCurrent];
        [SpiceName("gd"), SpiceInfo("Diode conductance")]
        public double GetConduct(Circuit ckt) => ckt.State.States[0][DIOstate + DIOconduct];
        [SpiceName("p"), SpiceInfo("Diode power")]
        public double GetPower(Circuit ckt) => ckt.State.States[0][DIOstate + DIOcurrent] * ckt.State.States[0][DIOstate + DIOvoltage];

        /// <summary>
        /// Constants
        /// </summary>
        private const int DIOvoltage = 0;
        private const int DIOcurrent = 1;
        private const int DIOconduct = 2;
        private const int DIOcapCharge = 3;
        private const int DIOcapCurrent = 4;

        /// <summary>
        /// Nodes
        /// </summary>
        public int DIOstate { get; private set; }
        public int DIOposNode { get; private set; }
        public int DIOnegNode { get; private set; }

        /// <summary>
        /// Private variables
        /// </summary>
        private int DIOposPrimeNode;
        private double DIOtJctPot; // temperature adjusted junction potential
        private double DIOtJctCap; // temperature adjusted junction capacitance
        private double DIOtDepCap; // temperature adjusted transition point in the curve matching (Fc * Vj )
        private double DIOtSatCur; // temperature adjusted saturation current
        private double DIOtVcrit; // temperature adjusted V crit
        private double DIOtF1; // temperature adjusted f1
        private double DIOtBrkdwnV; // temperature adjusted breakdown voltage */

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name"></param>
        public Diode(string name) : base(name, "D+", "D-") { }

        /// <summary>
        /// Setup the diode
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Setup(Circuit ckt)
        {
            var nodes = BindNodes(ckt);
            DIOposNode = nodes[0].Index;
            DIOnegNode = nodes[1].Index;

            // Add the extra node
            if (Model.DIOresist == 0.0)
                DIOposPrimeNode = DIOposNode;
            else
                DIOposPrimeNode = CreateNode(ckt).Index;

            // Allocate states
            DIOstate = ckt.State.GetState(5);
        }

        /// <summary>
        /// Get the model
        /// </summary>
        /// <returns></returns>
        public override CircuitModel GetModel() => Model;

        /// <summary>
        /// Do temperature-dependent calculations
        /// </summary>
        /// <param name="ckt"></param>
        public override void Temperature(Circuit ckt)
        {
            // Update the Model with new values if necessary
            var state = ckt.State;

            // NOTE: This is an almost exact replication of the original Spice diode model code

            double vte, cbv, xbv, xcbv = 0.0, tol, vt;
            double egfet1, arg1, fact1, pbfact1, pbo, gmaold;
            double fact2, pbfact, arg, egfet, gmanew;
            int iter;

            // loop through all the instances
            if (!DIOtemp.Given)
                DIOtemp.Value = state.Temperature;
            vt = Circuit.CONSTKoverQ * DIOtemp;

            // this part gets really ugly - I won't even try to explain these equations
            fact2 = DIOtemp / Circuit.CONSTRefTemp;
            egfet = 1.16 - (7.02e-4 * DIOtemp * DIOtemp) /
                    (DIOtemp + 1108);
            arg = -egfet / (2 * Circuit.CONSTBoltz * DIOtemp) +
                    1.1150877 / (Circuit.CONSTBoltz * (Circuit.CONSTRefTemp + Circuit.CONSTRefTemp));
            pbfact = -2 * vt * (1.5 * Math.Log(fact2) + Circuit.CHARGE * arg);
            egfet1 = 1.16 - (7.02e-4 * Model.DIOnomTemp * Model.DIOnomTemp) /
                    (Model.DIOnomTemp + 1108);
            arg1 = -egfet1 / (Circuit.CONSTBoltz * 2 * Model.DIOnomTemp) +
                    1.1150877 / (2 * Circuit.CONSTBoltz * Circuit.CONSTRefTemp);
            fact1 = Model.DIOnomTemp / Circuit.CONSTRefTemp;
            pbfact1 = -2 * Model.vtnom * (1.5 * Math.Log(fact1) + Circuit.CHARGE * arg1);
            pbo = (Model.DIOjunctionPot - pbfact1) / fact1;
            gmaold = (Model.DIOjunctionPot - pbo) / pbo;
            DIOtJctCap = Model.DIOjunctionCap /
                    (1 + Model.DIOgradingCoeff *
                    (400e-6 * (Model.DIOnomTemp - Circuit.CONSTRefTemp) - gmaold));
            DIOtJctPot = pbfact + fact2 * pbo;
            gmanew = (DIOtJctPot - pbo) / pbo;
            DIOtJctCap *= 1 + Model.DIOgradingCoeff *
                    (400e-6 * (DIOtemp - Circuit.CONSTRefTemp) - gmanew);

            DIOtSatCur = Model.DIOsatCur * Math.Exp(
                    ((DIOtemp / Model.DIOnomTemp) - 1) *
                    Model.DIOactivationEnergy / (Model.DIOemissionCoeff * vt) +
                    Model.DIOsaturationCurrentExp / Model.DIOemissionCoeff *
                    Math.Log(DIOtemp / Model.DIOnomTemp));

            // the defintion of f1, just recompute after temperature adjusting all the variables used in it
            DIOtF1 = DIOtJctPot *
                    (1 - Math.Exp((1 - Model.DIOgradingCoeff) * Model.xfc)) /
                    (1 - Model.DIOgradingCoeff);
            // same for Depletion Capacitance
            DIOtDepCap = Model.DIOdepletionCapCoeff *
                    DIOtJctPot;
            // and Vcrit
            vte = Model.DIOemissionCoeff * vt;
            DIOtVcrit = vte * Math.Log(vte / (Circuit.CONSTroot2 * DIOtSatCur));

            // and now to copute the breakdown voltage, again, using temperature adjusted basic parameters
            if (Model.DIObreakdownVoltage.Given)
            {
                cbv = Model.DIObreakdownCurrent;
                if (cbv < DIOtSatCur * Model.DIObreakdownVoltage / vt)
                {
                    cbv = DIOtSatCur * Model.DIObreakdownVoltage / vt;
                    CircuitWarning.Warning(this, string.Format("Diode {0}, Model {1}: Breakdown current increased to {2} to resolve incompatibility with specified saturation current", Name, Model.Name, cbv));
                    xbv = Model.DIObreakdownVoltage;
                }
                else
                {
                    tol = ckt.Simulation.Config.RelTol * cbv;
                    xbv = Model.DIObreakdownVoltage - vt * Math.Log(1 + cbv /
                            DIOtSatCur);
                    iter = 0;
                    for (iter = 0; iter < 25; iter++)
                    {
                        xbv = Model.DIObreakdownVoltage - vt * Math.Log(cbv /
                                DIOtSatCur + 1 - xbv / vt);
                        xcbv = DIOtSatCur * (Math.Exp((Model.DIObreakdownVoltage
                                - xbv) / vt) - 1 + xbv / vt);
                        if (Math.Abs(xcbv - cbv) <= tol) goto matched;
                    }
                    CircuitWarning.Warning(this, string.Format("Diode {0}, Model {1}: Unable to match forward and reverse diode regions: bv = {2}, ibv = {3}", Name, Model.Name, xbv, xcbv));
                }
                matched:
                DIOtBrkdwnV = xbv;
            }
        }

        /// <summary>
        /// Load the diode
        /// </summary>
        /// <param name="ckt"></param>
        public override void Load(Circuit ckt)
        {
            double arg, capd, cd, cdeq, cdhat, csat, czero, czof2, delvd, evd, evrev, gd, gspr, sarg, 
                vd, vdtemp, vt, vte;
            bool Check;

            var state = ckt.State;
            var rstate = state.Real;
            var method = ckt.Method;

            csat = DIOtSatCur * DIOarea;
            gspr = Model.DIOconductance * DIOarea;
            vt = Circuit.CONSTKoverQ * DIOtemp;
            vte = Model.DIOemissionCoeff * vt;

            // initialization 
            Check = true;
            if (state.UseSmallSignal)
                vd = state.States[0][DIOstate + DIOvoltage];
            else if (method != null && method.SavedTime == 0.0)
                vd = state.States[1][DIOstate + DIOvoltage];
            else if ((state.Init == CircuitState.InitFlags.InitJct) && (state.Domain == CircuitState.DomainTypes.Time && state.UseDC && state.UseIC))
                vd = DIOinitCond;
            else if (state.Init == CircuitState.InitFlags.InitJct && DIOoff)
                vd = 0.0;
            else if (state.Init == CircuitState.InitFlags.InitJct)
                vd = DIOtVcrit;
            else if (state.Init == CircuitState.InitFlags.InitFix && DIOoff)
                vd = 0.0;
            else
            {
                // Default initialization: use the previous iteration
                vd = rstate.OldSolution[DIOposPrimeNode] - rstate.OldSolution[DIOnegNode];
                delvd = vd - state.States[0][DIOstate + DIOvoltage];
                cdhat = state.States[0][DIOstate + DIOcurrent] + state.States[0][DIOstate + DIOconduct] * delvd;

                // limit new junction voltage
                if ((Model.DIObreakdownVoltage.Given) && (vd < Math.Min(0, -DIOtBrkdwnV + 10 * vte)))
                {
                    vdtemp = -(vd + DIOtBrkdwnV);
                    vdtemp = Semiconductor.DEVpnjlim(vdtemp, -(state.States[0][DIOstate + DIOvoltage] + DIOtBrkdwnV), vte, DIOtVcrit, ref Check);
                    vd = -(vdtemp + DIOtBrkdwnV);
                }
                else
                {
                    vd = Semiconductor.DEVpnjlim(vd, state.States[0][DIOstate + DIOvoltage], vte, DIOtVcrit, ref Check);
                }
            }

            // compute dc current and derivatives
            if (vd >= -3 * vte)
            {
                evd = Math.Exp(vd / vte);
                cd = csat * (evd - 1) + state.Gmin * vd;
                gd = csat * evd / vte + state.Gmin;
            }
            else if ((DIOtBrkdwnV == 0.0) || vd >= -DIOtBrkdwnV)
            {
                arg = 3 * vte / (vd * Circuit.CONSTE);
                arg = arg * arg * arg;
                cd = -csat * (1 + arg) + state.Gmin * vd;
                gd = csat * 3 * arg / vd + state.Gmin;
            }
            else
            {
                evrev = Math.Exp(-(DIOtBrkdwnV + vd) / vte);
                cd = -csat * evrev + state.Gmin * vd;
                gd = csat * evrev / vte + state.Gmin;
            }

            // Calculate the charge of the junction capacitance if we're going to need it
            if (method != null || state.UseSmallSignal || state.UseIC)
            {
                // Charge storage elements and junction capacitance
                czero = DIOtJctCap * DIOarea;
                if (vd < DIOtDepCap)
                {
                    arg = 1 - vd / Model.DIOjunctionPot;
                    sarg = Math.Exp(-Model.DIOgradingCoeff * Math.Log(arg));
                    state.States[0][DIOstate + DIOcapCharge] = Model.DIOtransitTime * cd + Model.DIOjunctionPot * czero * (1 - arg * sarg) / (1 - Model.DIOgradingCoeff);
                    capd = Model.DIOtransitTime * gd + czero * sarg;
                }
                else
                {
                    czof2 = czero / Model.DIOf2;
                    state.States[0][DIOstate + DIOcapCharge] = Model.DIOtransitTime * cd + czero * DIOtF1 + czof2 * (Model.DIOf3 * (vd - DIOtDepCap) +
                        (Model.DIOgradingCoeff / (Model.DIOjunctionPot + Model.DIOjunctionPot)) * (vd * vd - DIOtDepCap * DIOtDepCap));
                    capd = Model.DIOtransitTime * gd + czof2 * (Model.DIOf3 + Model.DIOgradingCoeff * vd / Model.DIOjunctionPot);
                }
                DIOcap = capd;

                // Store small-signal parameters
                if (state.UseSmallSignal)
                {
                    state.States[0][DIOstate + DIOcapCurrent] = capd;
                    return;
                }

                if (method != null && method.SavedTime == 0.0)
                    state.CopyDC(DIOstate + DIOcapCharge);

                // Time-domain analysis
                if (method != null)
                {
                    var result = method.Integrate(state, DIOstate + DIOcapCharge, capd);
                    gd = gd + result.Geq;
                    cd = cd + state.States[0][DIOstate + DIOcapCurrent];
                }
            }

            // check convergence
            if ((state.Init != CircuitState.InitFlags.InitFix) || !DIOoff)
            {
                if (Check)
                    state.IsCon = false;
            }
    
            state.States[0][DIOstate + DIOvoltage] = vd;
            state.States[0][DIOstate + DIOcurrent] = cd;
            state.States[0][DIOstate + DIOconduct] = gd;

            // load current vector
            cdeq = cd - gd * vd;
            rstate.Rhs[DIOnegNode] += cdeq;
            rstate.Rhs[DIOposPrimeNode] -= cdeq;

            // load matrix
            rstate.Matrix[DIOposNode, DIOposNode] += gspr;
            rstate.Matrix[DIOnegNode, DIOnegNode] += gd;
            rstate.Matrix[DIOposPrimeNode, DIOposPrimeNode] += (gd + gspr);
            rstate.Matrix[DIOposNode, DIOposPrimeNode] -= gspr;
            rstate.Matrix[DIOnegNode, DIOposPrimeNode] -= gd;
            rstate.Matrix[DIOposPrimeNode, DIOposNode] -= gspr;
            rstate.Matrix[DIOposPrimeNode, DIOnegNode] -= gd;
        }

        /// <summary>
        /// Accept the current timepoint
        /// </summary>
        /// <param name="ckt"></param>
        public override void Accept(Circuit ckt)
        {
            var method = ckt.Method;
            var state = ckt.State;
            if (method != null && method.SavedTime == 0.0)
            {
                state.CopyDC(DIOstate + DIOcapCharge);
                state.CopyDC(DIOstate + DIOcapCurrent);
            }
        }

        /// <summary>
        /// Load the diode for AC analysis
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void AcLoad(Circuit ckt)
        {
            var state = ckt.State;
            var cstate = state.Complex;

            double gspr = Model.DIOconductance * DIOarea;
            Complex geq = state.States[0][DIOstate + DIOconduct] + state.States[0][DIOstate + DIOcapCurrent] * cstate.Laplace;

            // Load matrix
            cstate.Matrix[DIOposNode, DIOposNode] += gspr;
            cstate.Matrix[DIOnegNode, DIOnegNode] += geq;
            cstate.Matrix[DIOposPrimeNode, DIOposPrimeNode] += geq + gspr;
            cstate.Matrix[DIOposNode, DIOposPrimeNode] -= gspr;
            cstate.Matrix[DIOnegNode, DIOposPrimeNode] -= geq;
            cstate.Matrix[DIOposPrimeNode, DIOposNode] -= gspr;
            cstate.Matrix[DIOposPrimeNode, DIOnegNode] -= geq;
        }
    }
}
