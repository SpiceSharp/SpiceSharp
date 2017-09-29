using System;
using SpiceSharp.Circuits;
using SpiceSharp.Diagnostics;
using SpiceSharp.Parameters;
using System.Numerics;
using SpiceSharp.Components.Semiconductors;
using SpiceSharp.Components.Transistors;

namespace SpiceSharp.Components
{
    /// <summary>
    /// JFET model
    /// </summary>
    [SpicePins("Drain", "Gate", "Source")]
    public class JFET : CircuitComponent<JFET>
    {
        /// <summary>
        /// Gets or sets the device model
        /// </summary>
        public void SetModel(JFETModel model) => Model = model;

        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("temp"), SpiceInfo("Instance temperature")]
        public double JFET_TEMP
        {
            get => JFETtemp - Circuit.CONSTCtoK;
            set => JFETtemp.Set(value + Circuit.CONSTCtoK);
        }
        public Parameter JFETtemp { get; } = new Parameter();
        [SpiceName("area"), SpiceInfo("Area factor")]
        public Parameter JFETarea { get; } = new Parameter(1);
        [SpiceName("ic-vds"), SpiceInfo("Initial D-S voltage")]
        public Parameter JFETicVDS { get; } = new Parameter();
        [SpiceName("ic-vgs"), SpiceInfo("Initial G-S volrage")]
        public Parameter JFETicVGS { get; } = new Parameter();
        [SpiceName("off"), SpiceInfo("Device initially off")]
        public bool JFEToff { get; set; }
        [SpiceName("drain-node"), SpiceInfo("Number of drain node")]
        public int JFETdrainNode { get; private set; }
        [SpiceName("gate-node"), SpiceInfo("Number of gate node")]
        public int JFETgateNode { get; private set; }
        [SpiceName("source-node"), SpiceInfo("Number of source node")]
        public int JFETsourceNode { get; private set; }
        [SpiceName("drain-prime-node"), SpiceInfo("Internal drain node")]
        public int JFETdrainPrimeNode { get; private set; }
        [SpiceName("source-prime-node"), SpiceInfo("Internal source node")]
        public int JFETsourcePrimeNode { get; private set; }

        /// <summary>
        /// Methods
        /// </summary>
        [SpiceName("ic"), SpiceInfo("Initial VDS,VGS vector")]
        public void SetIC(double[] value)
        {
            switch (value.Length)
            {
                case 2: JFETicVGS.Set(value[1]); goto case 1;
                case 1: JFETicVDS.Set(value[0]); break;
                default:
                    throw new CircuitException("Bad parameter");
            }
        }
        [SpiceName("vgs"), SpiceInfo("Voltage G-S")]
        public double GetVGS(Circuit ckt) => ckt.State.States[0][JFETstate + JFETvgs];
        [SpiceName("vgd"), SpiceInfo("Voltage G-D")]
        public double GetVGD(Circuit ckt) => ckt.State.States[0][JFETstate + JFETvgd];
        [SpiceName("ig"), SpiceInfo("Current at gate node")]
        public double GetCG(Circuit ckt) => ckt.State.States[0][JFETstate + JFETcg];
        [SpiceName("id"), SpiceInfo("Current at drain node")]
        public double GetCD(Circuit ckt) => ckt.State.States[0][JFETstate + JFETcd];
        [SpiceName("igd"), SpiceInfo("Current G-D")]
        public double GetCGD(Circuit ckt) => ckt.State.States[0][JFETstate + JFETcgd];
        [SpiceName("gm"), SpiceInfo("Transconductance")]
        public double GetGM(Circuit ckt) => ckt.State.States[0][JFETstate + JFETgm];
        [SpiceName("gds"), SpiceInfo("Conductance D-S")]
        public double GetGDS(Circuit ckt) => ckt.State.States[0][JFETstate + JFETgds];
        [SpiceName("ggs"), SpiceInfo("Conductance G-S")]
        public double GetGGS(Circuit ckt) => ckt.State.States[0][JFETstate + JFETggs];
        [SpiceName("ggd"), SpiceInfo("Conductance G-D")]
        public double GetGGD(Circuit ckt) => ckt.State.States[0][JFETstate + JFETggd];
        [SpiceName("qgs"), SpiceInfo("Charge storage G-S junction")]
        public double GetQGS(Circuit ckt) => ckt.State.States[0][JFETstate + JFETqgs];
        [SpiceName("cqgs"), SpiceInfo("Capacitance due to charge storage G-S junction")]
        public double GetCQGS(Circuit ckt) => ckt.State.States[0][JFETstate + JFETcqgs];
        [SpiceName("qgd"), SpiceInfo("Charge storage G-D junction")]
        public double GetQGD(Circuit ckt) => ckt.State.States[0][JFETstate + JFETqgd];
        [SpiceName("cqgd"), SpiceInfo("Capacitance due to charge storage G-D junction")]
        public double GetCQGD(Circuit ckt) => ckt.State.States[0][JFETstate + JFETcqgd];
        [SpiceName("is"), SpiceInfo("Source current")]
        public double GetCS(Circuit ckt)
        {
            double value = -ckt.State.States[0][JFETstate + JFETcd];
            value -= ckt.State.States[0][JFETstate + JFETcg];
            return value;
        }
        [SpiceName("p"), SpiceInfo("Power dissipated by the JFET")]
        public double GetPOWER(Circuit ckt)
        {
            double value = ckt.State.States[0][JFETstate + JFETcd] * ckt.State.Real.Solution[JFETdrainNode];
            value += ckt.State.States[0][JFETstate + JFETcg] * ckt.State.Real.Solution[JFETgateNode];
            value -= (ckt.State.States[0][JFETstate + JFETcd] + ckt.State.States[0][JFETstate + JFETcg]) *
                ckt.State.Real.Solution[JFETsourceNode];
            return value;
        }

        /// <summary>
        /// Extra variables
        /// </summary>
        public double JFETtSatCur { get; private set; }
        public double JFETtCGS { get; private set; }
        public double JFETtCGD { get; private set; }
        public double JFETtGatePot { get; private set; }
        public double JFETcorDepCap { get; private set; }
        public double JFETf1 { get; private set; }
        public double JFETvcrit { get; private set; }
        public int JFETstate { get; private set; }

        /// <summary>
        /// Constants
        /// </summary>
        private const int JFETvgs = 0;
        private const int JFETvgd = 1;
        private const int JFETcg = 2;
        private const int JFETcd = 3;
        private const int JFETcgd = 4;
        private const int JFETgm = 5;
        private const int JFETgds = 6;
        private const int JFETggs = 7;
        private const int JFETggd = 8;
        private const int JFETqgs = 9;
        private const int JFETcqgs = 10;
        private const int JFETqgd = 11;
        private const int JFETcqgd = 12;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the device</param>
        public JFET(string name) : base(name)
        {
        }

        /// <summary>
        /// Setup the device
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Setup(Circuit ckt)
        {
            var model = Model as JFETModel;

            // Allocate nodes
            var nodes = BindNodes(ckt);
            JFETdrainNode = nodes[0].Index;
            JFETgateNode = nodes[1].Index;
            JFETsourceNode = nodes[2].Index;

            // Allocate states
            JFETstate = ckt.State.GetState(13);

            if (model.JFETsourceResist != 0)
                JFETsourcePrimeNode = CreateNode(ckt).Index;
            else
                JFETsourcePrimeNode = JFETsourceNode;
            if (model.JFETdrainResist != 0)
                JFETdrainPrimeNode = CreateNode(ckt).Index;
            else
                JFETdrainPrimeNode = JFETdrainNode;
        }

        /// <summary>
        /// Do temperature-dependent calculations
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Temperature(Circuit ckt)
        {
            var model = Model as JFETModel;
            double vt, fact2, ratio1, kt, egfet, arg, pbfact, gmanew, cjfact1;

            if (!(JFETtemp.Given))
            {
                JFETtemp.Value = ckt.State.Temperature;
            }
            vt = JFETtemp * Circuit.CONSTKoverQ;
            fact2 = JFETtemp / Circuit.CONSTRefTemp;
            ratio1 = JFETtemp / model.JFETtnom - 1;
            JFETtSatCur = model.JFETgateSatCurrent * Math.Exp(ratio1 * 1.11 / vt);
            JFETtCGS = model.JFETcapGS * model.cjfact;
            JFETtCGD = model.JFETcapGD * model.cjfact;
            kt = Circuit.CONSTBoltz * JFETtemp;
            egfet = 1.16 - (7.02e-4 * JFETtemp * JFETtemp) / (JFETtemp + 1108);
            arg = -egfet / (kt + kt) + 1.1150877 / (Circuit.CONSTBoltz * (Circuit.CONSTRefTemp + Circuit.CONSTRefTemp));
            pbfact = -2 * vt * (1.5 * Math.Log(fact2) + Circuit.CHARGE * arg);
            JFETtGatePot = fact2 * model.pbo + pbfact;
            gmanew = (JFETtGatePot - model.pbo) / model.pbo;
            cjfact1 = 1 + .5 * (4e-4 * (JFETtemp - Circuit.CONSTRefTemp) - gmanew);
            JFETtCGS *= cjfact1;
            JFETtCGD *= cjfact1;

            JFETcorDepCap = model.JFETdepletionCapCoeff * JFETtGatePot;
            JFETf1 = JFETtGatePot * (1 - Math.Exp((1 - .5) * model.xfc)) / (1 - .5);
            JFETvcrit = vt * Math.Log(vt / (Circuit.CONSTroot2 * JFETtSatCur));
        }

        /// <summary>
        /// Load the device
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Load(Circuit ckt)
        {
            var model = Model as JFETModel;
            var state = ckt.State;
            var rstate = state.Real;
            var method = ckt.Method;
            double beta, gdpr, gspr, csat, vgs, vgd, vds, delvgs, delvgd, delvds, cghat, cdhat, cg, cd, cgd, gm, gds, ggs, ggd, evgs, evgd,
                vto, vgst, cdrain, betap, Bfac, apart, cpart, vgdt, czgs, czgd, twop, fcpb2, czgsf2, czgdf2, sarg, capgs, capgd, ceqgd, ceqgs,
                cdreq;
            bool icheck, ichk1;

            /* 
           * dc model parameters 
           */
            beta = model.JFETbeta * JFETarea;
            gdpr = model.JFETdrainConduct * JFETarea;
            gspr = model.JFETsourceConduct * JFETarea;
            csat = JFETtSatCur * JFETarea;
            /* 
			* initialization
			*/
            icheck = true;
            if (state.UseSmallSignal)
            {
                vgs = state.States[0][JFETstate + JFETvgs];
                vgd = state.States[0][JFETstate + JFETvgd];
            }
            else if (method != null && method.SavedTime == 0.0)
            {
                vgs = state.States[1][JFETstate + JFETvgs];
                vgd = state.States[1][JFETstate + JFETvgd];
            }
            else if ((state.Init == CircuitState.InitFlags.InitJct) && (state.Domain == CircuitState.DomainTypes.Time && state.UseDC) &&
              state.UseIC)
            {
                vds = model.JFETtype * JFETicVDS;
                vgs = model.JFETtype * JFETicVGS;
                vgd = vgs - vds;
            }
            else if ((state.Init == CircuitState.InitFlags.InitJct) && !JFEToff)
            {
                vgs = -1;
                vgd = -1;
            }
            else if ((state.Init == CircuitState.InitFlags.InitJct) || ((state.Init == CircuitState.InitFlags.InitFix) && (JFEToff)))
            {
                vgs = 0;
                vgd = 0;
            }
            else
            {
                /* PREDICTOR */
                /* 
				* compute new nonlinear branch voltages 
				*/
                vgs = model.JFETtype * rstate.OldSolution[JFETgateNode] - rstate.OldSolution[JFETsourcePrimeNode];
                vgd = model.JFETtype * rstate.OldSolution[JFETgateNode] - rstate.OldSolution[JFETdrainPrimeNode];
                /* PREDICTOR */
                delvgs = vgs - state.States[0][JFETstate + JFETvgs];
                delvgd = vgd - state.States[0][JFETstate + JFETvgd];
                delvds = delvgs - delvgd;
                cghat = state.States[0][JFETstate + JFETcg] + state.States[0][JFETstate + JFETggd] * delvgd + state.States[0][JFETstate +
                    JFETggs] * delvgs;
                cdhat = state.States[0][JFETstate + JFETcd] + state.States[0][JFETstate + JFETgm] * delvgs + state.States[0][JFETstate +
                    JFETgds] * delvds - state.States[0][JFETstate + JFETggd] * delvgd;

                /* 
				* limit nonlinear branch voltages 
				*/
                ichk1 = true;
                vgs = Semiconductor.DEVpnjlim(vgs, state.States[0][JFETstate + JFETvgs], (JFETtemp * Circuit.CONSTKoverQ), JFETvcrit, ref icheck);
                vgd = Semiconductor.DEVpnjlim(vgd, state.States[0][JFETstate + JFETvgd], (JFETtemp * Circuit.CONSTKoverQ), JFETvcrit, ref ichk1);
                if (ichk1)
                    icheck = true;
                vgs = Transistor.DEVfetlim(vgs, state.States[0][JFETstate + JFETvgs], model.JFETthreshold);
                vgd = Transistor.DEVfetlim(vgd, state.States[0][JFETstate + JFETvgd], model.JFETthreshold);
            }
            /* 
			* determine dc current and derivatives 
			*/
            vds = vgs - vgd;
            if (vgs <= -5 * JFETtemp * Circuit.CONSTKoverQ)
            {
                ggs = -csat / vgs + state.Gmin;
                cg = ggs * vgs;
            }
            else
            {
                evgs = Math.Exp(vgs / (JFETtemp * Circuit.CONSTKoverQ));
                ggs = csat * evgs / (JFETtemp * Circuit.CONSTKoverQ) + state.Gmin;
                cg = csat * (evgs - 1) + state.Gmin * vgs;
            }
            if (vgd <= -5 * (JFETtemp * Circuit.CONSTKoverQ))
            {
                ggd = -csat / vgd + state.Gmin;
                cgd = ggd * vgd;
            }
            else
            {
                evgd = Math.Exp(vgd / (JFETtemp * Circuit.CONSTKoverQ));
                ggd = csat * evgd / (JFETtemp * Circuit.CONSTKoverQ) + state.Gmin;
                cgd = csat * (evgd - 1) + state.Gmin * vgd;
            }
            cg = cg + cgd;

            /* Modification for Sydney University JFET model */
            vto = model.JFETthreshold;
            if (vds >= 0)
            {
                vgst = vgs - vto;
                /* 
				* compute drain current and derivatives for normal mode
				*/
                if (vgst <= 0)
                {
                    /* 
					* normal mode, cutoff region
					*/
                    cdrain = 0;
                    gm = 0;
                    gds = 0;
                }
                else
                {
                    betap = beta * (1 + model.JFETlModulation * vds);
                    Bfac = model.JFETbFac;
                    if (vgst >= vds)
                    {
                        /* 
						* normal mode, linear region
						*/
                        apart = 2 * model.JFETb + 3 * Bfac * (vgst - vds);
                        cpart = vds * (vds * (Bfac * vds - model.JFETb) + vgst * apart);
                        cdrain = betap * cpart;
                        gm = betap * vds * (apart + 3 * Bfac * vgst);
                        gds = betap * (vgst - vds) * apart + beta * model.JFETlModulation * cpart;
                    }
                    else
                    {
                        Bfac = vgst * Bfac;
                        gm = betap * vgst * (2 * model.JFETb + 3 * Bfac);
                        /* 
						* normal mode, saturation region
						*/
                        cpart = vgst * vgst * (model.JFETb + Bfac);
                        cdrain = betap * cpart;
                        gds = model.JFETlModulation * beta * cpart;
                    }
                }
            }
            else
            {
                vgdt = vgd - vto;
                /* 
				* compute drain current and derivatives for inverse mode
				*/
                if (vgdt <= 0)
                {
                    /* 
					* inverse mode, cutoff region
					*/
                    cdrain = 0;
                    gm = 0;
                    gds = 0;
                }
                else
                {
                    betap = beta * (1 - model.JFETlModulation * vds);
                    Bfac = model.JFETbFac;
                    if (vgdt + vds >= 0)
                    {
                        /* 
						* inverse mode, linear region
						*/
                        apart = 2 * model.JFETb + 3 * Bfac * (vgdt + vds);
                        cpart = vds * (-vds * (-Bfac * vds - model.JFETb) + vgdt * apart);
                        cdrain = betap * cpart;
                        gm = betap * vds * (apart + 3 * Bfac * vgdt);
                        gds = betap * (vgdt + vds) * apart - beta * model.JFETlModulation * cpart - gm;
                    }
                    else
                    {
                        Bfac = vgdt * Bfac;
                        gm = -betap * vgdt * (2 * model.JFETb + 3 * Bfac);
                        /* 
						* inverse mode, saturation region
						*/
                        cpart = vgdt * vgdt * (model.JFETb + Bfac);
                        cdrain = -betap * cpart;
                        gds = model.JFETlModulation * beta * cpart - gm;
                    }
                }
            }

            /* 
			* compute equivalent drain current source 
			*/
            cd = cdrain - cgd;
            if ((method != null || state.UseSmallSignal) || ((state.Domain == CircuitState.DomainTypes.Time && state.UseDC) && state.UseIC))
            {
                /* 
				* charge storage elements 
				*/
                czgs = JFETtCGS * JFETarea;
                czgd = JFETtCGD * JFETarea;
                twop = JFETtGatePot + JFETtGatePot;
                fcpb2 = JFETcorDepCap * JFETcorDepCap;
                czgsf2 = czgs / model.JFETf2;
                czgdf2 = czgd / model.JFETf2;
                if (vgs < JFETcorDepCap)
                {
                    sarg = Math.Sqrt(1 - vgs / JFETtGatePot);
                    state.States[0][JFETstate + JFETqgs] = twop * czgs * (1 - sarg);
                    capgs = czgs / sarg;
                }
                else
                {
                    state.States[0][JFETstate + JFETqgs] = czgs * JFETf1 + czgsf2 * (model.JFETf3 * (vgs - JFETcorDepCap) + (vgs * vgs - fcpb2) /
                        (twop + twop));
                    capgs = czgsf2 * (model.JFETf3 + vgs / twop);
                }
                if (vgd < JFETcorDepCap)
                {
                    sarg = Math.Sqrt(1 - vgd / JFETtGatePot);
                    state.States[0][JFETstate + JFETqgd] = twop * czgd * (1 - sarg);
                    capgd = czgd / sarg;
                }
                else
                {
                    state.States[0][JFETstate + JFETqgd] = czgd * JFETf1 + czgdf2 * (model.JFETf3 * (vgd - JFETcorDepCap) + (vgd * vgd - fcpb2) /
                        (twop + twop));
                    capgd = czgdf2 * (model.JFETf3 + vgd / twop);
                }
                /* 
				* store small - signal parameters 
				*/
                if ((!(state.Domain == CircuitState.DomainTypes.Time && state.UseDC)) || (!state.UseIC))
                {
                    if (state.UseSmallSignal)
                    {
                        state.States[0][JFETstate + JFETqgs] = capgs;
                        state.States[0][JFETstate + JFETqgd] = capgd;
                        return; /* go to 1000 */
                    }
                    /* 
					* transient analysis 
					*/
                    if (method != null && method.SavedTime == 0.0)
                    {
                        state.States[1][JFETstate + JFETqgs] = state.States[0][JFETstate + JFETqgs];
                        state.States[1][JFETstate + JFETqgd] = state.States[0][JFETstate + JFETqgd];
                    }

                    var result = method.Integrate(state, JFETstate + JFETqgs, capgs);
                    ggs = ggs + result.Geq;
                    result = method.Integrate(state, JFETstate + JFETqgd, capgd);
                    ggd = ggd + result.Geq;
                    cg = cg + state.States[0][JFETstate + JFETcqgd];
                    cd = cd - state.States[0][JFETstate + JFETcqgd];
                    cgd = cgd + state.States[0][JFETstate + JFETcqgd];
                    if (method != null && method.SavedTime == 0.0)
                    {
                        state.States[1][JFETstate + JFETcqgs] = state.States[0][JFETstate + JFETcqgs];
                        state.States[1][JFETstate + JFETcqgd] = state.States[0][JFETstate + JFETcqgd];
                    }
                }
            }
            /* 
			* check convergence 
			*/
            if (((state.Init != CircuitState.InitFlags.InitFix)) | (!state.UseIC))
            {
                if (icheck)
                    state.IsCon = false;
            }
            state.States[0][JFETstate + JFETvgs] = vgs;
            state.States[0][JFETstate + JFETvgd] = vgd;
            state.States[0][JFETstate + JFETcg] = cg;
            state.States[0][JFETstate + JFETcd] = cd;
            state.States[0][JFETstate + JFETcgd] = cgd;
            state.States[0][JFETstate + JFETgm] = gm;
            state.States[0][JFETstate + JFETgds] = gds;
            state.States[0][JFETstate + JFETggs] = ggs;
            state.States[0][JFETstate + JFETggd] = ggd;

            /* 
			* load current vector
			*/
            ceqgd = model.JFETtype * (cgd - ggd * vgd);
            ceqgs = model.JFETtype * ((cg - cgd) - ggs * vgs);
            cdreq = model.JFETtype * ((cd + cgd) - gds * vds - gm * vgs);
            rstate.Rhs[JFETgateNode] += (-ceqgs - ceqgd);
            rstate.Rhs[JFETdrainPrimeNode] += (-cdreq + ceqgd);
            rstate.Rhs[JFETsourcePrimeNode] += (cdreq + ceqgs);

            /* 
			* load y matrix 
			*/
            rstate.Matrix[JFETdrainNode, JFETdrainPrimeNode] += (-gdpr);
            rstate.Matrix[JFETgateNode, JFETdrainPrimeNode] += (-ggd);
            rstate.Matrix[JFETgateNode, JFETsourcePrimeNode] += (-ggs);
            rstate.Matrix[JFETsourceNode, JFETsourcePrimeNode] += (-gspr);
            rstate.Matrix[JFETdrainPrimeNode, JFETdrainNode] += (-gdpr);
            rstate.Matrix[JFETdrainPrimeNode, JFETgateNode] += (gm - ggd);
            rstate.Matrix[JFETdrainPrimeNode, JFETsourcePrimeNode] += (-gds - gm);
            rstate.Matrix[JFETsourcePrimeNode, JFETgateNode] += (-ggs - gm);
            rstate.Matrix[JFETsourcePrimeNode, JFETsourceNode] += (-gspr);
            rstate.Matrix[JFETsourcePrimeNode, JFETdrainPrimeNode] += (-gds);
            rstate.Matrix[JFETdrainNode, JFETdrainNode] += (gdpr);
            rstate.Matrix[JFETgateNode, JFETgateNode] += (ggd + ggs);
            rstate.Matrix[JFETsourceNode, JFETsourceNode] += (gspr);
            rstate.Matrix[JFETdrainPrimeNode, JFETdrainPrimeNode] += (gdpr + gds + ggd);
            rstate.Matrix[JFETsourcePrimeNode, JFETsourcePrimeNode] += (gspr + gds + gm + ggs);
        }

        /// <summary>
        /// Load the device for AC simulation
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void AcLoad(Circuit ckt)
        {
            var model = Model as JFETModel;
            var state = ckt.State;
            var cstate = state.Complex;
            double gdpr, gspr, gm, gds, ggs, xgs, ggd, xgd;

            gdpr = model.JFETdrainConduct * JFETarea;
            gspr = model.JFETsourceConduct * JFETarea;
            gm = state.States[0][JFETstate + JFETgm];
            gds = state.States[0][JFETstate + JFETgds];
            ggs = state.States[0][JFETstate + JFETggs];
            xgs = state.States[0][JFETstate + JFETqgs] * cstate.Laplace.Imaginary;
            ggd = state.States[0][JFETstate + JFETggd];
            xgd = state.States[0][JFETstate + JFETqgd] * cstate.Laplace.Imaginary;
            cstate.Matrix[JFETdrainNode, JFETdrainNode] += gdpr;
            cstate.Matrix[JFETgateNode, JFETgateNode] += new Complex(ggd + ggs, xgd + xgs);

            cstate.Matrix[JFETsourceNode, JFETsourceNode] += gspr;
            cstate.Matrix[JFETdrainPrimeNode, JFETdrainPrimeNode] += new Complex(gdpr + gds + ggd, xgd);

            cstate.Matrix[JFETsourcePrimeNode, JFETsourcePrimeNode] += new Complex(gspr + gds + gm + ggs, xgs);

            cstate.Matrix[JFETdrainNode, JFETdrainPrimeNode] -= gdpr;
            cstate.Matrix[JFETgateNode, JFETdrainPrimeNode] -= new Complex(ggd, xgd);

            cstate.Matrix[JFETgateNode, JFETsourcePrimeNode] -= new Complex(ggs, xgs);

            cstate.Matrix[JFETsourceNode, JFETsourcePrimeNode] -= gspr;
            cstate.Matrix[JFETdrainPrimeNode, JFETdrainNode] -= gdpr;
            cstate.Matrix[JFETdrainPrimeNode, JFETgateNode] += new Complex((-ggd + gm), -xgd);

            cstate.Matrix[JFETdrainPrimeNode, JFETsourcePrimeNode] += (-gds - gm);
            cstate.Matrix[JFETsourcePrimeNode, JFETgateNode] += new Complex((-ggs - gm), -xgs);

            cstate.Matrix[JFETsourcePrimeNode, JFETsourceNode] -= gspr;
            cstate.Matrix[JFETsourcePrimeNode, JFETdrainPrimeNode] -= gds;
        }

        /// <summary>
        /// Truncate the timestep
        /// </summary>
        /// <param name="ckt">The circuit</param>
        /// <param name="timeStep">The timestep</param>
        public override void Truncate(Circuit ckt, ref double timeStep)
        {
            var method = ckt.Method;
            method.Terr(JFETstate + JFETqgs, ckt, ref timeStep);
            method.Terr(JFETstate + JFETqgd, ckt, ref timeStep);
        }
    }
}
