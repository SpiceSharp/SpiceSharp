using System;
using SpiceSharp.Circuits;
using SpiceSharp.Diagnostics;
using SpiceSharp.Parameters;
using System.Numerics;
using SpiceSharp.Components.Transistors;
using SpiceSharp.Components.Semiconductors;

namespace SpiceSharp.Components
{
    public class MES : CircuitComponent<MES>
    {
        /// <summary>
        /// Gets or sets the device model
        /// </summary>
        public void SetModel(MESModel model) => Model = model;

        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("area"), SpiceInfo("Area factor")]
        public Parameter MESarea { get; } = new Parameter(1);
        [SpiceName("icvds"), SpiceInfo("Initial D-S voltage")]
        public Parameter MESicVDS { get; } = new Parameter();
        [SpiceName("icvgs"), SpiceInfo("Initial G-S voltage")]
        public Parameter MESicVGS { get; } = new Parameter();
        [SpiceName("off"), SpiceInfo("Device initially off")]
        public bool MESoff { get; set; }
        [SpiceName("dnode"), SpiceInfo("Number of drain node")]
        public int MESdrainNode { get; private set; }
        [SpiceName("gnode"), SpiceInfo("Number of gate node")]
        public int MESgateNode { get; private set; }
        [SpiceName("snode"), SpiceInfo("Number of source node")]
        public int MESsourceNode { get; private set; }
        [SpiceName("dprimenode"), SpiceInfo("Number of internal drain node")]
        public int MESdrainPrimeNode { get; private set; }

        /// <summary>
        /// Methods
        /// </summary>
        [SpiceName("vgs"), SpiceInfo("Gate-Source voltage")]
        public double GetVGS(Circuit ckt) => ckt.State.States[0][MESstate + MESvgs];
        [SpiceName("vgd"), SpiceInfo("Gate-Drain voltage")]
        public double GetVGD(Circuit ckt) => ckt.State.States[0][MESstate + MESvgd];
        [SpiceName("cg"), SpiceInfo("Gate capacitance")]
        public double GetCG(Circuit ckt) => ckt.State.States[0][MESstate + MEScg];
        [SpiceName("cd"), SpiceInfo("Drain capacitance")]
        public double GetCD(Circuit ckt) => ckt.State.States[0][MESstate + MEScd];
        [SpiceName("cgd"), SpiceInfo("Gate-Drain capacitance")]
        public double GetCGD(Circuit ckt) => ckt.State.States[0][MESstate + MEScgd];
        [SpiceName("gm"), SpiceInfo("Transconductance")]
        public double GetGM(Circuit ckt) => ckt.State.States[0][MESstate + MESgm];
        [SpiceName("gds"), SpiceInfo("Drain-Source conductance")]
        public double GetGDS(Circuit ckt) => ckt.State.States[0][MESstate + MESgds];
        [SpiceName("ggs"), SpiceInfo("Gate-Source conductance")]
        public double GetGGS(Circuit ckt) => ckt.State.States[0][MESstate + MESggs];
        [SpiceName("ggd"), SpiceInfo("Gate-Drain conductance")]
        public double GetGGD(Circuit ckt) => ckt.State.States[0][MESstate + MESggd];
        [SpiceName("qgs"), SpiceInfo("Gate-Source charge storage")]
        public double GetQGS(Circuit ckt) => ckt.State.States[0][MESstate + MESqgs];
        [SpiceName("cqgs"), SpiceInfo("Capacitance due to gate-source charge storage")]
        public double GetCQGS(Circuit ckt) => ckt.State.States[0][MESstate + MEScqgs];
        [SpiceName("qgd"), SpiceInfo("Gate-Drain charge storage")]
        public double GetQGD(Circuit ckt) => ckt.State.States[0][MESstate + MESqgd];
        [SpiceName("cqgd"), SpiceInfo("Capacitance due to gate-drain charge storage")]
        public double GetCQGD(Circuit ckt) => ckt.State.States[0][MESstate + MEScqgd];
        [SpiceName("is"), SpiceInfo("Source current")]
        public double GetCS(Circuit ckt)
        {
            double value = -ckt.State.States[0][MESstate + MEScd];
            value -= ckt.State.States[0][MESstate + MEScg];
            return value;
        }
        [SpiceName("p"), SpiceInfo("Power dissipated by the mesfet")]
        public double GetPOWER(Circuit ckt)
        {
            double value = ckt.State.States[0][MESstate + MEScd] * ckt.State.Real.Solution[MESdrainNode];
            value += ckt.State.States[0][MESstate + MEScg] * ckt.State.Real.Solution[MESgateNode];
            value -= (ckt.State.States[0][MESstate + MEScd] + ckt.State.States[0][MESstate + MEScg]) *
                ckt.State.Real.Solution[MESsourceNode];
            return value;
        }
        [SpiceName("ic"), SpiceInfo("Initial conditions")]
        public void SetIC(double[] value)
        {
            switch (value.Length)
            {
                case 2: MESicVGS.Set(value[1]); goto case 1;
                case 1: MESicVDS.Set(value[0]); break;
                default:
                    throw new CircuitException("Bad parameter");
            }
        }

        /// <summary>
        /// Extra variables
        /// </summary>
        public double MESname { get; private set; }
        public int MESsourcePrimeNode { get; private set; }
        public int MESstate { get; private set; }

        /// <summary>
        /// Constants
        /// </summary>
        private const int MESvgs = 0;
        private const int MESvgd = 1;
        private const int MEScg = 2;
        private const int MEScd = 3;
        private const int MEScgd = 4;
        private const int MESgm = 5;
        private const int MESgds = 6;
        private const int MESggs = 7;
        private const int MESggd = 8;
        private const int MESqgs = 9;
        private const int MEScqgs = 10;
        private const int MESqgd = 11;
        private const int MEScqgd = 12;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the device</param>
        public MES(string name) : base(name)
        {
        }

        /// <summary>
        /// Setup the device
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Setup(Circuit ckt)
        {
            var model = Model as MESModel;

            // Allocate nodes
            var nodes = BindNodes(ckt);
            MESdrainNode = nodes[0].Index;
            MESgateNode = nodes[1].Index;
            MESsourceNode = nodes[2].Index;

            // Allocate states
            MESstate = ckt.State.GetState(13);

            if (model.MESsourceResist != 0)
                MESsourcePrimeNode = CreateNode(ckt).Index;
            else
                MESsourcePrimeNode = MESsourceNode;
            if (model.MESdrainResist != 0)
                MESdrainPrimeNode = CreateNode(ckt).Index;
            else
                MESdrainPrimeNode = MESdrainNode;
        }

        /// <summary>
        /// Temperature-dependent calculations
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Temperature(Circuit ckt)
        {
            // Do nothing
        }

        /// <summary>
        /// Load the device
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Load(Circuit ckt)
        {
            var model = Model as MESModel;
            var state = ckt.State;
            var rstate = state.Real;
            var method = ckt.Method;
            double beta, gdpr, gspr, csat, vcrit, vto, vgs, vgd, vds, delvgs, delvgd, delvds, cghat, cdhat, cg, cd, cgd, gm, gds, ggs, ggd,
                evgs, evgd, vgst, cdrain, prod, betap, denom, invdenom, afact, lfact, vgdt, czgs, czgd, phib, vgs1, vgd1, vcap, qgga, qggb,
                qggc, qggd, capgs, capgd, ceqgd, ceqgs, cdreq;
            double cgsna, cgdna, cgsnb, cgdnb, cgsnc, cgdnc, cgsnd, cgdnd;
            bool icheck, ichk1;

            /* 
             * dc model parameters 
             */
            beta = model.MESbeta * MESarea;
            gdpr = model.MESdrainConduct * MESarea;
            gspr = model.MESsourceConduct * MESarea;
            csat = model.MESgateSatCurrent * MESarea;
            vcrit = model.MESvcrit;
            vto = model.MESthreshold;
            /* 
			 * initialization
			 */
            icheck = true;
            if (state.UseSmallSignal)
            {
                vgs = state.States[0][MESstate + MESvgs];
                vgd = state.States[0][MESstate + MESvgd];
            }
            else if (method != null && method.SavedTime == 0.0)
            {
                vgs = state.States[1][MESstate + MESvgs];
                vgd = state.States[1][MESstate + MESvgd];
            }
            else if ((state.Init == CircuitState.InitFlags.InitJct) && (state.Domain == CircuitState.DomainTypes.Time && state.UseDC) &&
              state.UseIC)
            {
                vds = model.MEStype * MESicVDS;
                vgs = model.MEStype * MESicVGS;
                vgd = vgs - vds;
            }
            else if ((state.Init == CircuitState.InitFlags.InitJct) && !MESoff)
            {
                vgs = -1;
                vgd = -1;
            }
            else if ((state.Init == CircuitState.InitFlags.InitJct) || ((state.Init == CircuitState.InitFlags.InitFix) && (MESoff)))
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
                vgs = model.MEStype * rstate.OldSolution[MESgateNode] - rstate.OldSolution[MESsourcePrimeNode];
                vgd = model.MEStype * rstate.OldSolution[MESgateNode] - rstate.OldSolution[MESdrainPrimeNode];
                /* PREDICTOR */
                delvgs = vgs - state.States[0][MESstate + MESvgs];
                delvgd = vgd - state.States[0][MESstate + MESvgd];
                delvds = delvgs - delvgd;
                cghat = state.States[0][MESstate + MEScg] + state.States[0][MESstate + MESggd] * delvgd + state.States[0][MESstate + MESggs] *
                    delvgs;
                cdhat = state.States[0][MESstate + MEScd] + state.States[0][MESstate + MESgm] * delvgs + state.States[0][MESstate + MESgds] *
                    delvds - state.States[0][MESstate + MESggd] * delvgd;
                
                /* 
				 * limit nonlinear branch voltages 
				 */
                ichk1 = true;
                vgs = Semiconductor.DEVpnjlim(vgs, state.States[0][MESstate + MESvgs], Circuit.CONSTvt0, vcrit, ref icheck);
                vgd = Semiconductor.DEVpnjlim(vgd, state.States[0][MESstate + MESvgd], Circuit.CONSTvt0, vcrit, ref ichk1);
                if (ichk1)
                    icheck = true;
                vgs = Transistor.DEVfetlim(vgs, state.States[0][MESstate + MESvgs], model.MESthreshold);
                vgd = Transistor.DEVfetlim(vgd, state.States[0][MESstate + MESvgd], model.MESthreshold);
            }

            /* 
			 * determine dc current and derivatives 
			 */
            vds = vgs - vgd;
            if (vgs <= -5 * Circuit.CONSTvt0)
            {
                ggs = -csat / vgs + state.Gmin;
                cg = ggs * vgs;
            }
            else
            {
                evgs = Math.Exp(vgs / Circuit.CONSTvt0);
                ggs = csat * evgs / Circuit.CONSTvt0 + state.Gmin;
                cg = csat * (evgs - 1) + state.Gmin * vgs;
            }
            if (vgd <= -5 * Circuit.CONSTvt0)
            {
                ggd = -csat / vgd + state.Gmin;
                cgd = ggd * vgd;
            }
            else
            {
                evgd = Math.Exp(vgd / Circuit.CONSTvt0);
                ggd = csat * evgd / Circuit.CONSTvt0 + state.Gmin;
                cgd = csat * (evgd - 1) + state.Gmin * vgd;
            }
            cg = cg + cgd;

            /* 
			 * compute drain current and derivitives for normal mode 
			 */
            if (vds >= 0)
            {
                vgst = vgs - model.MESthreshold;
                /* 
				 * normal mode, cutoff region 
				 */
                if (vgst <= 0)
                {
                    cdrain = 0;
                    gm = 0;
                    gds = 0;
                }
                else
                {
                    prod = 1 + model.MESlModulation * vds;
                    betap = beta * prod;
                    denom = 1 + model.MESb * vgst;
                    invdenom = 1 / denom;
                    if (vds >= (3 / model.MESalpha))
                    {
                        /* 
						 * normal mode, saturation region 
						 */
                        cdrain = betap * vgst * vgst * invdenom;
                        gm = betap * vgst * (1 + denom) * invdenom * invdenom;
                        gds = model.MESlModulation * beta * vgst * vgst * invdenom;
                    }
                    else
                    {
                        /* 
						 * normal mode, linear region 
						 */
                        afact = 1 - model.MESalpha * vds / 3;
                        lfact = 1 - afact * afact * afact;
                        cdrain = betap * vgst * vgst * invdenom * lfact;
                        gm = betap * vgst * (1 + denom) * invdenom * invdenom * lfact;
                        gds = beta * vgst * vgst * invdenom * (model.MESalpha * afact * afact * prod + lfact * model.MESlModulation);
                    }
                }
            }
            else
            {
                /* 
				 * compute drain current and derivitives for inverse mode 
				 */
                vgdt = vgd - model.MESthreshold;
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
                    /* 
					 * inverse mode, saturation region 
					 */
                    prod = 1 - model.MESlModulation * vds;
                    betap = beta * prod;
                    denom = 1 + model.MESb * vgdt;
                    invdenom = 1 / denom;
                    if (-vds >= (3 / model.MESalpha))
                    {
                        cdrain = -betap * vgdt * vgdt * invdenom;
                        gm = -betap * vgdt * (1 + denom) * invdenom * invdenom;
                        gds = model.MESlModulation * beta * vgdt * vgdt * invdenom - gm;
                    }
                    else
                    {
                        /* 
						 * inverse mode, linear region 
						 */
                        afact = 1 + model.MESalpha * vds / 3;
                        lfact = 1 - afact * afact * afact;
                        cdrain = -betap * vgdt * vgdt * invdenom * lfact;
                        gm = -betap * vgdt * (1 + denom) * invdenom * invdenom * lfact;
                        gds = beta * vgdt * vgdt * invdenom * (model.MESalpha * afact * afact * prod + lfact * model.MESlModulation) - gm;
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
                czgs = model.MEScapGS * MESarea;
                czgd = model.MEScapGD * MESarea;
                phib = model.MESgatePotential;
                vgs1 = state.States[1][MESstate + MESvgs];
                vgd1 = state.States[1][MESstate + MESvgd];
                vcap = 1 / model.MESalpha;

                qgga = qggnew(vgs, vgd, phib, vcap, vto, czgs, czgd, out cgsna, out cgdna);
                qggb = qggnew(vgs1, vgd, phib, vcap, vto, czgs, czgd, out cgsnb, out cgdnb);
                qggc = qggnew(vgs, vgd1, phib, vcap, vto, czgs, czgd, out cgsnc, out cgdnc);
                qggd = qggnew(vgs1, vgd1, phib, vcap, vto, czgs, czgd, out cgsnd, out cgdnd);

                if (method != null && method.SavedTime == 0.0)
                {
                    state.States[1][MESstate + MESqgs] = qgga;
                    state.States[1][MESstate + MESqgd] = qgga;
                }
                state.States[0][MESstate + MESqgs] = state.States[1][MESstate + MESqgs] + 0.5 * (qgga - qggb + qggc - qggd);
                state.States[0][MESstate + MESqgd] = state.States[1][MESstate + MESqgd] + 0.5 * (qgga - qggc + qggb - qggd);
                capgs = cgsna;
                capgd = cgdna;

                /* 
				* store small - signal parameters 
				*/
                if ((!(state.Domain == CircuitState.DomainTypes.Time && state.UseDC)) || (!state.UseIC))
                {
                    if (state.UseSmallSignal)
                    {
                        state.States[0][MESstate + MESqgs] = capgs;
                        state.States[0][MESstate + MESqgd] = capgd;
                        return; /* go to 1000 */
                    }
                    /* 
					* transient analysis 
					*/
                    if (method != null && method.SavedTime == 0.0)
                    {
                        state.States[1][MESstate + MESqgs] = state.States[0][MESstate + MESqgs];
                        state.States[1][MESstate + MESqgd] = state.States[0][MESstate + MESqgd];
                    }

                    var result = method.Integrate(state, MESstate + MESqgs, capgs);
                    ggs = ggs + result.Geq;
                    cg = cg + state.States[0][MESstate + MEScqgs];

                    result = method.Integrate(state, MESstate + MESqgd, capgd);
                    ggd = ggd + result.Geq;
                    cg = cg + state.States[0][MESstate + MEScqgd];
                    cd = cd - state.States[0][MESstate + MEScqgd];
                    cgd = cgd + state.States[0][MESstate + MEScqgd];
                    if (method != null && method.SavedTime == 0.0)
                    {
                        state.States[1][MESstate + MEScqgs] = state.States[0][MESstate + MEScqgs];
                        state.States[1][MESstate + MEScqgd] = state.States[0][MESstate + MEScqgd];
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
            state.States[0][MESstate + MESvgs] = vgs;
            state.States[0][MESstate + MESvgd] = vgd;
            state.States[0][MESstate + MEScg] = cg;
            state.States[0][MESstate + MEScd] = cd;
            state.States[0][MESstate + MEScgd] = cgd;
            state.States[0][MESstate + MESgm] = gm;
            state.States[0][MESstate + MESgds] = gds;
            state.States[0][MESstate + MESggs] = ggs;
            state.States[0][MESstate + MESggd] = ggd;

            /* 
			 * load current vector
			 */
            ceqgd = model.MEStype * (cgd - ggd * vgd);
            ceqgs = model.MEStype * ((cg - cgd) - ggs * vgs);
            cdreq = model.MEStype * ((cd + cgd) - gds * vds - gm * vgs);
            rstate.Rhs[MESgateNode] += (-ceqgs - ceqgd);
            rstate.Rhs[MESdrainPrimeNode] += (-cdreq + ceqgd);
            rstate.Rhs[MESsourcePrimeNode] += (cdreq + ceqgs);

            /* 
			 * load y matrix 
			 */
            rstate.Matrix[MESdrainNode, MESdrainPrimeNode] += (-gdpr);
            rstate.Matrix[MESgateNode, MESdrainPrimeNode] += (-ggd);
            rstate.Matrix[MESgateNode, MESsourcePrimeNode] += (-ggs);
            rstate.Matrix[MESsourceNode, MESsourcePrimeNode] += (-gspr);
            rstate.Matrix[MESdrainPrimeNode, MESdrainNode] += (-gdpr);
            rstate.Matrix[MESdrainPrimeNode, MESgateNode] += (gm - ggd);
            rstate.Matrix[MESdrainPrimeNode, MESsourcePrimeNode] += (-gds - gm);
            rstate.Matrix[MESsourcePrimeNode, MESgateNode] += (-ggs - gm);
            rstate.Matrix[MESsourcePrimeNode, MESsourceNode] += (-gspr);
            rstate.Matrix[MESsourcePrimeNode, MESdrainPrimeNode] += (-gds);
            rstate.Matrix[MESdrainNode, MESdrainNode] += (gdpr);
            rstate.Matrix[MESgateNode, MESgateNode] += (ggd + ggs);
            rstate.Matrix[MESsourceNode, MESsourceNode] += (gspr);
            rstate.Matrix[MESdrainPrimeNode, MESdrainPrimeNode] += (gdpr + gds + ggd);
            rstate.Matrix[MESsourcePrimeNode, MESsourcePrimeNode] += (gspr + gds + gm + ggs);
        }

        /// <summary>
        /// Load the device for AC simulation
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void AcLoad(Circuit ckt)
        {
            var model = Model as MESModel;
            var state = ckt.State;
            var cstate = state.Complex;
            double gdpr, gspr, gm, gds, ggs, xgs, ggd, xgd;

            gdpr = model.MESdrainConduct * MESarea;
            gspr = model.MESsourceConduct * MESarea;
            gm = state.States[0][MESstate + MESgm];
            gds = state.States[0][MESstate + MESgds];
            ggs = state.States[0][MESstate + MESggs];
            xgs = state.States[0][MESstate + MESqgs] * cstate.Laplace.Imaginary;
            ggd = state.States[0][MESstate + MESggd];
            xgd = state.States[0][MESstate + MESqgd] * cstate.Laplace.Imaginary;
            cstate.Matrix[MESdrainNode, MESdrainNode] += gdpr;
            cstate.Matrix[MESgateNode, MESgateNode] += new Complex(ggd + ggs, xgd + xgs);

            cstate.Matrix[MESsourceNode, MESsourceNode] += gspr;
            cstate.Matrix[MESdrainPrimeNode, MESdrainPrimeNode] += new Complex(gdpr + gds + ggd, xgd);

            cstate.Matrix[MESsourcePrimeNode, MESsourcePrimeNode] += new Complex(gspr + gds + gm + ggs, xgs);

            cstate.Matrix[MESdrainNode, MESdrainPrimeNode] -= gdpr;
            cstate.Matrix[MESgateNode, MESdrainPrimeNode] -= new Complex(ggd, xgd);

            cstate.Matrix[MESgateNode, MESsourcePrimeNode] -= new Complex(ggs, xgs);

            cstate.Matrix[MESsourceNode, MESsourcePrimeNode] -= gspr;
            cstate.Matrix[MESdrainPrimeNode, MESdrainNode] -= gdpr;
            cstate.Matrix[MESdrainPrimeNode, MESgateNode] += new Complex((-ggd + gm), -xgd);

            cstate.Matrix[MESdrainPrimeNode, MESsourcePrimeNode] += (-gds - gm);
            cstate.Matrix[MESsourcePrimeNode, MESgateNode] += new Complex((-ggs - gm), -xgs);

            cstate.Matrix[MESsourcePrimeNode, MESsourceNode] -= gspr;
            cstate.Matrix[MESsourcePrimeNode, MESdrainPrimeNode] -= gds;
        }

        /// <summary>
        /// Truncate the timestep
        /// </summary>
        /// <param name="ckt">The circuit</param>
        /// <param name="timeStep">The timestep</param>
        public override void Truncate(Circuit ckt, ref double timeStep)
        {
            var method = ckt.Method;

            method.Terr(MESstate + MESqgs, ckt, ref timeStep);
            method.Terr(MESstate + MESqgd, ckt, ref timeStep);
        }

        /// <summary>
        /// Function qggnew - private, used by MESload
        /// ref mesload.c
        /// </summary>
        /// <param name="vgs">Vgs</param>
        /// <param name="vgd">Vgd</param>
        /// <param name="phib">PhiB</param>
        /// <param name="vcap">Vcap</param>
        /// <param name="vto">Vto</param>
        /// <param name="cgs">Cgs</param>
        /// <param name="cgd">Cgd</param>
        /// <param name="cgsnew">New Cgs</param>
        /// <param name="cgdnew">New Cgd</param>
        /// <returns></returns>
        private double qggnew(double vgs, double vgd, double phib, double vcap, double vto, double cgs, double cgd, out double cgsnew, out double cgdnew)
        {
            double veroot, veff1, veff2, del, vnroot, vnew1, vnew3, vmax, ext;
            double qroot, qggval, par1, cfact, cplus, cminus;

            veroot = Math.Sqrt((vgs - vgd) * (vgs - vgd) + vcap * vcap);
            veff1 = 0.5 * (vgs + vgd + veroot);
            veff2 = veff1 - veroot;
            del = 0.2;
            vnroot = Math.Sqrt((veff1 - vto) * (veff1 - vto) + del * del);
            vnew1 = 0.5 * (veff1 + vto + vnroot);
            vnew3 = vnew1;
            vmax = 0.5;
            if (vnew1 < vmax)
            {
                ext = 0;
            }
            else
            {
                vnew1 = vmax;
                ext = (vnew3 - vmax) / Math.Sqrt(1 - vmax / phib);
            }

            qroot = Math.Sqrt(1 - vnew1 / phib);
            qggval = cgs * (2 * phib * (1 - qroot) + ext) + cgd * veff2;
            par1 = 0.5 * (1 + (veff1 - vto) / vnroot);
            cfact = (vgs - vgd) / veroot;
            cplus = 0.5 * (1 + cfact);
            cminus = cplus - cfact;
            cgsnew = cgs / qroot * par1 * cplus + cgd * cminus;
            cgdnew = cgs / qroot * par1 * cminus + cgd * cplus;
            return (qggval);
        }
    }
}
