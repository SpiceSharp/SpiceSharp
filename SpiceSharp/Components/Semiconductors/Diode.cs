using System;
using SpiceSharp.Circuits;
using SpiceSharp.Diagnostics;
using SpiceSharp.Parameters;
using SpiceSharp.Components.Semiconductors;
using System.Numerics;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A diode
    /// </summary>
    [SpicePins("D+", "D-")]
    public class Diode : CircuitComponent<Diode>
    {
        /// <summary>
        /// Set the model for the diode
        /// </summary>
        public void SetModel(DiodeModel model) => Model = model;

        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("area"), SpiceInfo("Area factor")]
        public Parameter DIOarea { get; } = new Parameter(1);
        [SpiceName("temp"), SpiceInfo("Instance temperature")]
        public double DIO_TEMP
        {
            get => DIOtemp - Circuit.CONSTCtoK;
            set => DIOtemp.Set(value + Circuit.CONSTCtoK);
        }
        public Parameter DIOtemp { get; } = new Parameter();
        [SpiceName("off"), SpiceInfo("Initially off")]
        public bool DIOoff { get; set; }
        [SpiceName("ic"), SpiceInfo("Initial device voltage")]
        public double DIOinitCond { get; set; }
        [SpiceName("sens_area"), SpiceInfo("flag to request sensitivity WRT area")]
        public bool DIOsenParmNo { get; set; }
        [SpiceName("cd"), SpiceInfo("Diode capacitance")]
        public double DIOcap { get; private set; }

        /// <summary>
        /// Methods
        /// </summary>
        [SpiceName("vd"), SpiceInfo("Diode voltage")]
        public double GetDIO_VOLTAGE(Circuit ckt) => ckt.State.States[0][DIOstate + DIOvoltage];
        [SpiceName("id"), SpiceName("c"), SpiceInfo("Diode current")]
        public double GetDIO_CURRENT(Circuit ckt) => ckt.State.States[0][DIOstate + DIOcurrent];
        [SpiceName("charge"), SpiceInfo("Diode capacitor charge")]
        public double GetDIO_CHARGE(Circuit ckt) => ckt.State.States[0][DIOstate + DIOcapCharge];
        [SpiceName("capcur"), SpiceInfo("Diode capacitor current")]
        public double GetDIO_CAPCUR(Circuit ckt) => ckt.State.States[0][DIOstate + DIOcapCurrent];
        [SpiceName("gd"), SpiceInfo("Diode conductance")]
        public double GetDIO_CONDUCT(Circuit ckt) => ckt.State.States[0][DIOstate + DIOconduct];
        [SpiceName("p"), SpiceInfo("Diode power")]

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
        public int DIOposNode { get; private set; }
        public int DIOposPrimeNode { get; private set; }
        public int DIOnegNode { get; private set; }
        public int DIOstate { get; private set; }

        /// <summary>
        /// Noise generators
        /// </summary>
        public ComponentNoise DIOnoise = new ComponentNoise(
            new Noise.NoiseThermal("rs", 0, 1),
            new Noise.NoiseShot("id", 1, 2),
            new Noise.NoiseGain("1overf", 1, 2));

        /// <summary>
        /// Constants
        /// </summary>
        private const int DIOvoltage = 0;
        private const int DIOcurrent = 1;
        private const int DIOconduct = 2;
        private const int DIOcapCharge = 3;
        private const int DIOcapCurrent = 4;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the device</param>
        public Diode(CircuitIdentifier name) : base(name)
        {
        }

        /// <summary>
        /// Setup the device
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Setup(Circuit ckt)
        {
            var model = Model as DiodeModel;

            // Allocate nodes
            var nodes = BindNodes(ckt);
            DIOposNode = nodes[0].Index;
            DIOnegNode = nodes[1].Index;

            // Allocate states
            DIOstate = ckt.State.GetState(5);

            if (model.DIOresist.Value == 0)
                DIOposPrimeNode = DIOposNode;
            else
                DIOposPrimeNode = CreateNode(ckt, Name.Grow("#pos")).Index;

            // Connect noise sources
            DIOnoise.Setup(ckt, DIOposNode, DIOposPrimeNode, DIOnegNode);
        }

        /// <summary>
        /// Do temperature-dependent calculations
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Temperature(Circuit ckt)
        {
            var model = Model as DiodeModel;
            double vt, fact2, egfet, arg, pbfact, egfet1, arg1, fact1, pbfact1, pbo, gmaold, gmanew, vte, cbv, xbv, tol, iter, xcbv = 0.0;

            /* loop through all the instances */
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
            egfet1 = 1.16 - (7.02e-4 * model.DIOnomTemp * model.DIOnomTemp) / (model.DIOnomTemp + 1108);
            arg1 = -egfet1 / (Circuit.CONSTBoltz * 2 * model.DIOnomTemp) + 1.1150877 / (2 * Circuit.CONSTBoltz * Circuit.CONSTRefTemp);
            fact1 = model.DIOnomTemp / Circuit.CONSTRefTemp;
            pbfact1 = -2 * model.vtnom * (1.5 * Math.Log(fact1) + Circuit.CHARGE * arg1);
            pbo = (model.DIOjunctionPot - pbfact1) / fact1;
            gmaold = (model.DIOjunctionPot - pbo) / pbo;
            DIOtJctCap = model.DIOjunctionCap / (1 + model.DIOgradingCoeff * (400e-6 * (model.DIOnomTemp - Circuit.CONSTRefTemp) - gmaold));
            DIOtJctPot = pbfact + fact2 * pbo;
            gmanew = (DIOtJctPot - pbo) / pbo;
            DIOtJctCap *= 1 + model.DIOgradingCoeff * (400e-6 * (DIOtemp - Circuit.CONSTRefTemp) - gmanew);

            DIOtSatCur = model.DIOsatCur * Math.Exp(((DIOtemp / model.DIOnomTemp) - 1) * model.DIOactivationEnergy /
                (model.DIOemissionCoeff * vt) + model.DIOsaturationCurrentExp / model.DIOemissionCoeff * Math.Log(DIOtemp / model.DIOnomTemp));
            /* the defintion of f1, just recompute after temperature adjusting
			* all the variables used in it */
            DIOtF1 = DIOtJctPot * (1 - Math.Exp((1 - model.DIOgradingCoeff) * model.xfc)) / (1 - model.DIOgradingCoeff);
            /* same for Depletion Capacitance */
            DIOtDepCap = model.DIOdepletionCapCoeff * DIOtJctPot;
            /* and Vcrit */
            vte = model.DIOemissionCoeff * vt;
            DIOtVcrit = vte * Math.Log(vte / (Circuit.CONSTroot2 * DIOtSatCur));
            /* and now to copute the breakdown voltage, again, using
			* temperature adjusted basic parameters */
            if (model.DIObreakdownVoltage.Given)
            {
                cbv = model.DIObreakdownCurrent;
                if (cbv < DIOtSatCur * model.DIObreakdownVoltage / vt)
                {
                    cbv = DIOtSatCur * model.DIObreakdownVoltage / vt;
                    CircuitWarning.Warning(this, $"{Name}: breakdown current increased to {cbv.ToString("g")} to resolve incompatability with specified saturation current");
                    xbv = model.DIObreakdownVoltage;
                }
                else
                {
                    tol = ckt.Simulation.Config.RelTol * cbv;
                    xbv = model.DIObreakdownVoltage - vt * Math.Log(1 + cbv / DIOtSatCur);
                    iter = 0;
                    for (iter = 0; iter < 25; iter++)
                    {
                        xbv = model.DIObreakdownVoltage - vt * Math.Log(cbv / DIOtSatCur + 1 - xbv / vt);
                        xcbv = DIOtSatCur * (Math.Exp((model.DIObreakdownVoltage - xbv) / vt) - 1 + xbv / vt);
                        if (Math.Abs(xcbv - cbv) <= tol) goto matched;
                    }
                    CircuitWarning.Warning(this, $"{Name}: unable to match forward and reverse diode regions: bv = {xbv.ToString("g")}, ibv = {xcbv.ToString("g")}");
                }
                matched:
                DIOtBrkdwnV = xbv;
            }
        }

        /// <summary>
        /// Load the device
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Load(Circuit ckt)
        {
            var model = Model as DiodeModel;
            var state = ckt.State;
            var rstate = state.Real;
            var method = ckt.Method;
            bool Check;
            double csat, gspr, vt, vte, vd, delvd, cdhat, vdtemp, evd, cd, gd, arg, evrev, czero, sarg, capd, czof2, cdeq;

            /* 
             * this routine loads diodes for dc and transient analyses.
             */

            csat = DIOtSatCur * DIOarea;
            gspr = model.DIOconductance * DIOarea;
            vt = Circuit.CONSTKoverQ * DIOtemp;
            vte = model.DIOemissionCoeff * vt;
            /* 
			 * initialization 
			 */
             
            Check = true;
            if (state.UseSmallSignal)
            {
                vd = state.States[0][DIOstate + DIOvoltage];
            }
            else if (method != null && method.SavedTime == 0.0)
            {
                vd = state.States[1][DIOstate + DIOvoltage];
                Check = false; // EDIT: Spice does not check the first timepoint for convergence, but we do...
            }
            else if ((state.Init == CircuitState.InitFlags.InitJct) && (state.Domain == CircuitState.DomainTypes.Time && state.UseDC) &&
              state.UseIC)
            {
                vd = DIOinitCond;
            }
            else if ((state.Init == CircuitState.InitFlags.InitJct) && DIOoff)
            {
                vd = 0;
            }
            else if (state.Init == CircuitState.InitFlags.InitJct)
            {
                vd = DIOtVcrit;
            }
            else if (ckt.State.Init == CircuitState.InitFlags.InitFix && DIOoff)
            {
                vd = 0;
            }
            else
            {
                vd = rstate.OldSolution[DIOposPrimeNode] - rstate.OldSolution[DIOnegNode];
                delvd = vd - state.States[0][DIOstate + DIOvoltage];
                cdhat = state.States[0][DIOstate + DIOcurrent] + state.States[0][DIOstate + DIOconduct] * delvd;

                /* 
				 * limit new junction voltage
				 */
                if ((model.DIObreakdownVoltage.Given) && (vd < Math.Min(0, -DIOtBrkdwnV + 10 * vte)))
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
            /* 
			 * compute dc current and derivitives
			 */
            if (vd >= -3 * vte)
            {
                evd = Math.Exp(vd / vte);
                cd = csat * (evd - 1) + state.Gmin * vd;
                gd = csat * evd / vte + state.Gmin;
            }
            else if (DIOtBrkdwnV == 0.0 || vd >= -DIOtBrkdwnV)
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
            if ((method != null || state.UseSmallSignal) || (state.Domain == CircuitState.DomainTypes.Time && state.UseDC) && state.UseIC)
            {
                /* 
				* charge storage elements
				*/
                czero = DIOtJctCap * DIOarea;
                if (vd < DIOtDepCap)
                {
                    arg = 1 - vd / model.DIOjunctionPot;
                    sarg = Math.Exp(-model.DIOgradingCoeff * Math.Log(arg));
                    state.States[0][DIOstate + DIOcapCharge] = model.DIOtransitTime * cd + model.DIOjunctionPot * czero * (1 - arg * sarg) / (1 -
                        model.DIOgradingCoeff);
                    capd = model.DIOtransitTime * gd + czero * sarg;
                }
                else
                {
                    czof2 = czero / model.DIOf2;
                    state.States[0][DIOstate + DIOcapCharge] = model.DIOtransitTime * cd + czero * DIOtF1 + czof2 * (model.DIOf3 * (vd -
                        DIOtDepCap) + (model.DIOgradingCoeff / (model.DIOjunctionPot + model.DIOjunctionPot)) * (vd * vd - DIOtDepCap * DIOtDepCap));
                    capd = model.DIOtransitTime * gd + czof2 * (model.DIOf3 + model.DIOgradingCoeff * vd / model.DIOjunctionPot);
                }
                DIOcap = capd;

                /* 
				* store small - signal parameters
				*/
                if ((!(state.Domain == CircuitState.DomainTypes.Time && state.UseDC)) || (!state.UseIC))
                {
                    if (state.UseSmallSignal)
                    {
                        state.States[0][DIOstate + DIOcapCurrent] = capd;
                        return;
                    }

                    /* 
					 * transient analysis
					 */
                    if (method != null)
                    {
                        if (method.SavedTime == 0.0)
                            state.States[1][DIOstate + DIOcapCharge] = state.States[0][DIOstate + DIOcapCharge];
                            var result = method.Integrate(state, DIOstate + DIOcapCharge, capd);
                            gd = gd + result.Geq;
                            cd = cd + state.States[0][DIOstate + DIOcapCurrent];
                        if (method != null && method.SavedTime == 0.0)
                            state.States[1][DIOstate + DIOcapCurrent] = state.States[0][DIOstate + DIOcapCurrent];
                    }
                }
            }

            /* 
			 * check convergence
			 */
            if (((state.Init != CircuitState.InitFlags.InitFix)) || (!(DIOoff)))
            {
                if (Check)
                    ckt.State.IsCon = false;
            }
            state.States[0][DIOstate + DIOvoltage] = vd;
            state.States[0][DIOstate + DIOcurrent] = cd;
            state.States[0][DIOstate + DIOconduct] = gd;
            
            /* 
			 * load current vector
			 */
            cdeq = cd - gd * vd;
            rstate.Rhs[DIOnegNode] += cdeq;
            rstate.Rhs[DIOposPrimeNode] -= cdeq;
            /* 
			 * load matrix
			 */
            rstate.Matrix[DIOposNode, DIOposNode] += gspr;
            rstate.Matrix[DIOnegNode, DIOnegNode] += gd;
            rstate.Matrix[DIOposPrimeNode, DIOposPrimeNode] += (gd + gspr);
            rstate.Matrix[DIOposNode, DIOposPrimeNode] -= gspr;
            rstate.Matrix[DIOnegNode, DIOposPrimeNode] -= gd;
            rstate.Matrix[DIOposPrimeNode, DIOposNode] -= gspr;
            rstate.Matrix[DIOposPrimeNode, DIOnegNode] -= gd;
        }

        /// <summary>
        /// Load the device for AC simulation
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void AcLoad(Circuit ckt)
        {
            var model = Model as DiodeModel;
            var state = ckt.State;
            var cstate = state.Complex;
            double gspr, geq, xceq;

            gspr = model.DIOconductance * DIOarea;
            geq = state.States[0][DIOstate + DIOconduct];
            xceq = state.States[0][DIOstate + DIOcapCurrent] * cstate.Laplace.Imaginary;
            cstate.Matrix[DIOposNode, DIOposNode] += gspr;
            cstate.Matrix[DIOnegNode, DIOnegNode] += new Complex(geq, xceq);

            cstate.Matrix[DIOposPrimeNode, DIOposPrimeNode] += new Complex(geq + gspr, xceq);

            cstate.Matrix[DIOposNode, DIOposPrimeNode] -= gspr;
            cstate.Matrix[DIOnegNode, DIOposPrimeNode] -= new Complex(geq, xceq);

            cstate.Matrix[DIOposPrimeNode, DIOposNode] -= gspr;
            cstate.Matrix[DIOposPrimeNode, DIOnegNode] -= new Complex(geq, xceq);
        }

        /// <summary>
        /// Truncate
        /// </summary>
        /// <param name="ckt">Circuit</param>
        /// <param name="timeStep">Timestep</param>
        public override void Truncate(Circuit ckt, ref double timeStep)
        {
            ckt.Method.Terr(DIOstate + DIOcapCharge, ckt, ref timeStep);
        }

        /// <summary>
        /// Perform noise calculations
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Noise(Circuit ckt)
        {
            var model = Model as DiodeModel;
            var state = ckt.State;
            var noise = ckt.State.Noise;

            double Kf = model.DIOfNcoef * Math.Exp(model.DIOfNexp * Math.Log(Math.Max(Math.Abs(state.States[0][DIOstate + DIOcurrent]), 1e-38)));

            DIOnoise.Evaluate(ckt,
                model.DIOconductance * DIOarea, // Thermal noise
                state.States[0][DIOstate + DIOcurrent], // Shot noise
                Kf / noise.Freq); // 1 over f noise
        }
    }
}
