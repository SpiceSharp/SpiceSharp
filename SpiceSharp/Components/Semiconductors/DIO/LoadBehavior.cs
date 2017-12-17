using System;
using SpiceSharp.Circuits;
using SpiceSharp.Components;
using SpiceSharp.Parameters;
using SpiceSharp.Sparse;
using SpiceSharp.Components.Semiconductors;

namespace SpiceSharp.Behaviors.DIO
{
    /// <summary>
    /// General behavior for <see cref="Diode"/>
    /// </summary>
    public class LoadBehavior : Behaviors.LoadBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        private ModelTemperatureBehavior modeltemp;
        private TemperatureBehavior temp;

        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("area"), SpiceInfo("Area factor")]
        public Parameter DIOarea { get; } = new Parameter(1);
        [SpiceName("off"), SpiceInfo("Initially off")]
        public bool DIOoff { get; set; }
        [SpiceName("ic"), SpiceInfo("Initial device voltage")]
        public double DIOinitCond { get; set; }
        [SpiceName("sens_area"), SpiceInfo("flag to request sensitivity WRT area")]
        public bool DIOsenParmNo { get; set; }
        [SpiceName("cd"), SpiceInfo("Diode capacitance")]
        public double DIOcap { get; set; }

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
        // [SpiceName("p"), SpiceInfo("Diode power")]

        /// <summary>
        /// Nodes
        /// </summary>
        private int DIOposNode, DIOnegNode;
        public int DIOposPrimeNode { get; private set; }
        protected MatrixElement DIOposPosPrimePtr { get; private set; }
        protected MatrixElement DIOnegPosPrimePtr { get; private set; }
        protected MatrixElement DIOposPrimePosPtr { get; private set; }
        protected MatrixElement DIOposPrimeNegPtr { get; private set; }
        protected MatrixElement DIOposPosPtr { get; private set; }
        protected MatrixElement DIOnegNegPtr { get; private set; }
        protected MatrixElement DIOposPrimePosPrimePtr { get; private set; }

        /// <summary>
        /// Extra variables
        /// </summary>
        public int DIOstate { get; protected set; }

        /// <summary>
        /// Constants
        /// </summary>
        public const int DIOvoltage = 0;
        public const int DIOcurrent = 1;
        public const int DIOconduct = 2;
        public const int DIOcapCharge = 3;
        public const int DIOcapCurrent = 4;

        /// <summary>
        /// Setup behavior
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        /// <returns></returns>
        public override void Setup(Entity component, Circuit ckt)
        {
            var dio = component as Diode;

            // Get behaviors
            temp = GetBehavior<TemperatureBehavior>(component);
            modeltemp = GetBehavior<ModelTemperatureBehavior>(dio.Model);

            // Allocate states
            DIOstate = ckt.State.GetState(5);

            // Nodes
            DIOposNode = dio.DIOposNode;
            DIOnegNode = dio.DIOnegNode;
            if (modeltemp.DIOresist.Value == 0)
                DIOposPrimeNode = DIOposNode;
            else
                DIOposPrimeNode = CreateNode(ckt, dio.Name.Grow("#pos")).Index;

            var matrix = ckt.State.Matrix;
            DIOposPosPrimePtr = matrix.GetElement(DIOposNode, DIOposPrimeNode);
            DIOnegPosPrimePtr = matrix.GetElement(DIOnegNode, DIOposPrimeNode);
            DIOposPrimePosPtr = matrix.GetElement(DIOposPrimeNode, DIOposNode);
            DIOposPrimeNegPtr = matrix.GetElement(DIOposPrimeNode, DIOnegNode);
            DIOposPosPtr = matrix.GetElement(DIOposNode, DIOposNode);
            DIOnegNegPtr = matrix.GetElement(DIOnegNode, DIOnegNode);
            DIOposPrimePosPrimePtr = matrix.GetElement(DIOposPrimeNode, DIOposPrimeNode);
        }

        /// <summary>
        /// Unsetup the device
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Unsetup()
        {
            DIOposPosPrimePtr = null;
            DIOnegPosPrimePtr = null;
            DIOposPrimePosPtr = null;
            DIOposPrimeNegPtr = null;
            DIOposPosPtr = null;
            DIOnegNegPtr = null;
            DIOposPrimePosPrimePtr = null;
        }

        /// <summary>
        /// Behavior
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Load(Circuit ckt)
        {
            var state = ckt.State;
            var matrix = state.Matrix;
            var method = ckt.Method;
            bool Check;
            double csat, gspr, vt, vte, vd, vdtemp, evd, cd, gd, arg, evrev, czero, sarg, capd, czof2, cdeq;

            /* 
             * this routine loads diodes for dc and transient analyses.
             */
            csat = temp.DIOtSatCur * DIOarea;
            gspr = modeltemp.DIOconductance * DIOarea;
            vt = Circuit.CONSTKoverQ * temp.DIOtemp;
            vte = modeltemp.DIOemissionCoeff * vt;

            /* 
			 * initialization 
			 */
            Check = true;
            if (state.UseSmallSignal)
            {
                vd = state.States[0][DIOstate + DIOvoltage];
            }
            else if (state.Init == State.InitFlags.InitTransient)
            {
                vd = state.States[1][DIOstate + DIOvoltage];
                Check = false; // EDIT: Spice does not check the first timepoint for convergence, but we do...
            }
            else if ((state.Init == State.InitFlags.InitJct) && (state.Domain == State.DomainTypes.Time && state.UseDC) &&
              state.UseIC)
            {
                vd = DIOinitCond;
            }
            else if ((state.Init == State.InitFlags.InitJct) && DIOoff)
            {
                vd = 0;
            }
            else if (state.Init == State.InitFlags.InitJct)
            {
                vd = temp.DIOtVcrit;
            }
            else if (ckt.State.Init == State.InitFlags.InitFix && DIOoff)
            {
                vd = 0;
            }
            else
            {
                vd = state.Solution[DIOposPrimeNode] - state.Solution[DIOnegNode];

                /* 
				 * limit new junction voltage
				 */
                if ((modeltemp.DIObreakdownVoltage.Given) && (vd < Math.Min(0, -temp.DIOtBrkdwnV + 10 * vte)))
                {
                    vdtemp = -(vd + temp.DIOtBrkdwnV);
                    vdtemp = Semiconductor.DEVpnjlim(vdtemp, -(state.States[0][DIOstate + DIOvoltage] + temp.DIOtBrkdwnV), vte, temp.DIOtVcrit, ref Check);
                    vd = -(vdtemp + temp.DIOtBrkdwnV);
                }
                else
                {
                    vd = Semiconductor.DEVpnjlim(vd, state.States[0][DIOstate + DIOvoltage], vte, temp.DIOtVcrit, ref Check);
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
            else if (temp.DIOtBrkdwnV == 0.0 || vd >= -temp.DIOtBrkdwnV)
            {
                arg = 3 * vte / (vd * Circuit.CONSTE);
                arg = arg * arg * arg;
                cd = -csat * (1 + arg) + state.Gmin * vd;
                gd = csat * 3 * arg / vd + state.Gmin;
            }
            else
            {
                evrev = Math.Exp(-(temp.DIOtBrkdwnV + vd) / vte);
                cd = -csat * evrev + state.Gmin * vd;
                gd = csat * evrev / vte + state.Gmin;
            }
            if ((method != null || state.UseSmallSignal) || (state.Domain == State.DomainTypes.Time && state.UseDC) && state.UseIC)
            {
                /* 
				* charge storage elements
				*/
                czero = temp.DIOtJctCap * DIOarea;
                if (vd < temp.DIOtDepCap)
                {
                    arg = 1 - vd / modeltemp.DIOjunctionPot;
                    sarg = Math.Exp(-modeltemp.DIOgradingCoeff * Math.Log(arg));
                    state.States[0][DIOstate + DIOcapCharge] = modeltemp.DIOtransitTime * cd + modeltemp.DIOjunctionPot * czero * (1 - arg * sarg) / (1 -
                        modeltemp.DIOgradingCoeff);
                    capd = modeltemp.DIOtransitTime * gd + czero * sarg;
                }
                else
                {
                    czof2 = czero / modeltemp.DIOf2;
                    state.States[0][DIOstate + DIOcapCharge] = modeltemp.DIOtransitTime * cd + czero * temp.DIOtF1 + czof2 * (modeltemp.DIOf3 * (vd -
                        temp.DIOtDepCap) + (modeltemp.DIOgradingCoeff / (modeltemp.DIOjunctionPot + modeltemp.DIOjunctionPot)) * (vd * vd - temp.DIOtDepCap * temp.DIOtDepCap));
                    capd = modeltemp.DIOtransitTime * gd + czof2 * (modeltemp.DIOf3 + modeltemp.DIOgradingCoeff * vd / modeltemp.DIOjunctionPot);
                }
                DIOcap = capd;

                /* 
				* store small - signal parameters
				*/
                if ((!(state.Domain == State.DomainTypes.Time && state.UseDC)) || (!state.UseIC))
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
                        if (state.Init == State.InitFlags.InitTransient)
                            state.States[1][DIOstate + DIOcapCharge] = state.States[0][DIOstate + DIOcapCharge];
                        var result = method.Integrate(state, DIOstate + DIOcapCharge, capd);
                        gd = gd + result.Geq;
                        cd = cd + state.States[0][DIOstate + DIOcapCurrent];
                        if (state.Init == State.InitFlags.InitTransient)
                            state.States[1][DIOstate + DIOcapCurrent] = state.States[0][DIOstate + DIOcapCurrent];
                    }
                }
            }

            /* 
			 * check convergence
			 */
            if (((state.Init != State.InitFlags.InitFix)) || (!(DIOoff)))
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
            state.Rhs[DIOnegNode] += cdeq;
            state.Rhs[DIOposPrimeNode] -= cdeq;

            /* 
			 * load matrix
			 */
            DIOposPosPtr.Value.Real += gspr;
            DIOnegNegPtr.Value.Real += gd;
            DIOposPrimePosPrimePtr.Value.Real += gd + gspr;

            DIOposPosPrimePtr.Value.Real -= gspr;
            DIOposPrimePosPtr.Value.Real -= gspr;
            DIOnegPosPrimePtr.Value.Real -= gd;
            DIOposPrimeNegPtr.Value.Real -= gd;
        }

        /// <summary>
        /// Check convergence for the diode
        /// </summary>
        /// <param name="ckt">Circuit</param>
        /// <returns></returns>
        public override bool IsConvergent(Circuit ckt)
        {
            var state = ckt.State;
            var config = ckt.Simulation.CurrentConfig;

            double delvd, cdhat, cd;

            double vd = state.Solution[DIOposPrimeNode] - state.Solution[DIOnegNode];

            delvd = vd - state.States[0][DIOstate + DIOvoltage];
            cdhat = state.States[0][DIOstate + DIOcurrent] + state.States[0][DIOstate + DIOconduct] * delvd;

            cd = state.States[0][DIOstate + DIOcurrent];

            /*
             *   check convergence
             */
            double tol = config.RelTol * Math.Max(Math.Abs(cdhat), Math.Abs(cd)) + config.AbsTol;
            if (Math.Abs(cdhat - cd) > tol)
            {
                state.IsCon = false;
                return false;
            }
            return true;
        }
    }
}
