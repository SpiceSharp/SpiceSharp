using System;
using SpiceSharp.Circuits;
using SpiceSharp.Diagnostics;
using SpiceSharp.Parameters;
using SpiceSharp.Components.Semiconductors;
using System.Numerics;

namespace SpiceSharp.Components
{
    /// <summary>
    /// This class represents a bipolar junction transistor (BJT)
    /// </summary>
    [SpicePins("Collector", "Base", "Emitter", "Substrate")]
    public class BJT : CircuitComponent<BJT>
    {
        /// <summary>
        /// Gets or sets the device model
        /// </summary>
        public void SetModel(BJTModel model) => Model = (ICircuitObject)model;

        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("area"), SpiceInfo("Area factor")]
        public Parameter BJTarea { get; } = new Parameter(1);
        [SpiceName("temp"), SpiceInfo("Instance temperature")]
        public double BJT_TEMP
        {
            get => BJTtemp - Circuit.CONSTCtoK;
            set => BJTtemp.Set(value + Circuit.CONSTCtoK);
        }
        public Parameter BJTtemp { get; } = new Parameter(300.15);
        [SpiceName("off"), SpiceInfo("Device initially off")]
        public bool BJToff { get; set; }
        [SpiceName("icvbe"), SpiceInfo("Initial B-E voltage")]
        public Parameter BJTicVBE { get; } = new Parameter();
        [SpiceName("icvce"), SpiceInfo("Initial C-E voltage")]
        public Parameter BJTicVCE { get; } = new Parameter();
        [SpiceName("sens_area"), SpiceInfo("flag to request sensitivity WRT area")]
        public bool BJTsenParmNo { get; set; }
        [SpiceName("colnode"), SpiceInfo("Number of collector node")]
        public int BJTcolNode { get; private set; }
        [SpiceName("basenode"), SpiceInfo("Number of base node")]
        public int BJTbaseNode { get; private set; }
        [SpiceName("emitnode"), SpiceInfo("Number of emitter node")]
        public int BJTemitNode { get; private set; }
        [SpiceName("substnode"), SpiceInfo("Number of substrate node")]
        public int BJTsubstNode { get; private set; }
        [SpiceName("colprimenode"), SpiceInfo("Internal collector node")]
        public int BJTcolPrimeNode { get; private set; }
        [SpiceName("baseprimenode"), SpiceInfo("Internal base node")]
        public int BJTbasePrimeNode { get; private set; }
        [SpiceName("emitprimenode"), SpiceInfo("Internal emitter node")]
        public int BJTemitPrimeNode { get; private set; }
        [SpiceName("cpi"), SpiceInfo("Internal base to emitter capactance")]
        public double BJTcapbe { get; private set; }
        [SpiceName("cmu"), SpiceInfo("Internal base to collector capactiance")]
        public double BJTcapbc { get; private set; }
        [SpiceName("cbx"), SpiceInfo("Base to collector capacitance")]
        public double BJTcapbx { get; private set; }
        [SpiceName("ccs"), SpiceInfo("Collector to substrate capacitance")]
        public double BJTcapcs { get; private set; }

        /// <summary>
        /// Methods
        /// </summary>
        [SpiceName("ic"), SpiceInfo("Initial condition vector")]
        public void SetIC(double[] value)
        {
            switch (value.Length)
            {
                case 2: BJTicVCE.Set(value[1]); goto case 1;
                case 1: BJTicVBE.Set(value[0]); break;
                default:
                    throw new CircuitException("Bad parameter");
            }
        }
        [SpiceName("vbe"), SpiceInfo("B-E voltage")]
        public double GetVBE(Circuit ckt) => ckt.State.States[0][BJTstate + BJTvbe];
        [SpiceName("vbc"), SpiceInfo("B-C voltage")]
        public double GetVBC(Circuit ckt) => ckt.State.States[0][BJTstate + BJTvbc];
        [SpiceName("cc"), SpiceInfo("Current at collector node")]
        public double GetCC(Circuit ckt) => ckt.State.States[0][BJTstate + BJTcc];
        [SpiceName("cb"), SpiceInfo("Current at base node")]
        public double GetCB(Circuit ckt) => ckt.State.States[0][BJTstate + BJTcb];
        [SpiceName("gpi"), SpiceInfo("Small signal input conductance - pi")]
        public double GetGPI(Circuit ckt) => ckt.State.States[0][BJTstate + BJTgpi];
        [SpiceName("gmu"), SpiceInfo("Small signal conductance - mu")]
        public double GetGMU(Circuit ckt) => ckt.State.States[0][BJTstate + BJTgmu];
        [SpiceName("gm"), SpiceInfo("Small signal transconductance")]
        public double GetGM(Circuit ckt) => ckt.State.States[0][BJTstate + BJTgm];
        [SpiceName("go"), SpiceInfo("Small signal output conductance")]
        public double GetGO(Circuit ckt) => ckt.State.States[0][BJTstate + BJTgo];
        [SpiceName("qbe"), SpiceInfo("Charge storage B-E junction")]
        public double GetQBE(Circuit ckt) => ckt.State.States[0][BJTstate + BJTqbe];
        [SpiceName("cqbe"), SpiceInfo("Cap. due to charge storage in B-E jct.")]
        public double GetCQBE(Circuit ckt) => ckt.State.States[0][BJTstate + BJTcqbe];
        [SpiceName("qbc"), SpiceInfo("Charge storage B-C junction")]
        public double GetQBC(Circuit ckt) => ckt.State.States[0][BJTstate + BJTqbc];
        [SpiceName("cqbc"), SpiceInfo("Cap. due to charge storage in B-C jct.")]
        public double GetCQBC(Circuit ckt) => ckt.State.States[0][BJTstate + BJTcqbc];
        [SpiceName("qcs"), SpiceInfo("Charge storage C-S junction")]
        public double GetQCS(Circuit ckt) => ckt.State.States[0][BJTstate + BJTqcs];
        [SpiceName("cqcs"), SpiceInfo("Cap. due to charge storage in C-S jct.")]
        public double GetCQCS(Circuit ckt) => ckt.State.States[0][BJTstate + BJTcqcs];
        [SpiceName("qbx"), SpiceInfo("Charge storage B-X junction")]
        public double GetQBX(Circuit ckt) => ckt.State.States[0][BJTstate + BJTqbx];
        [SpiceName("cqbx"), SpiceInfo("Cap. due to charge storage in B-X jct.")]
        public double GetCQBX(Circuit ckt) => ckt.State.States[0][BJTstate + BJTcqbx];
        [SpiceName("gx"), SpiceInfo("Conductance from base to internal base")]
        public double GetGX(Circuit ckt) => ckt.State.States[0][BJTstate + BJTgx];
        [SpiceName("cexbc"), SpiceInfo("Total Capacitance in B-X junction")]
        public double GetCEXBC(Circuit ckt) => ckt.State.States[0][BJTstate + BJTcexbc];
        [SpiceName("geqcb"), SpiceInfo("d(Ibe)/d(Vbc)")]
        public double GetGEQCB(Circuit ckt) => ckt.State.States[0][BJTstate + BJTgeqcb];
        [SpiceName("gccs"), SpiceInfo("Internal C-S cap. equiv. cond.")]
        public double GetGCCS(Circuit ckt) => ckt.State.States[0][BJTstate + BJTgccs];
        [SpiceName("geqbx"), SpiceInfo("Internal C-B-base cap. equiv. cond.")]
        public double GetGEQBX(Circuit ckt) => ckt.State.States[0][BJTstate + BJTgeqbx];
        [SpiceName("cs"), SpiceInfo("Substrate current")]
        public double GetCS(Circuit ckt)
        {
            if (ckt.State.UseDC)
                return 0.0;
            else
                return -ckt.State.States[0][BJTstate + BJTcqcs];
        }
        [SpiceName("ce"), SpiceInfo("Emitter current")]
        public double GetCE(Circuit ckt)
        {
            double value = -ckt.State.States[0][BJTstate + BJTcc];
            value -= ckt.State.States[0][BJTstate + BJTcb];
            if (ckt.Method != null && !(ckt.State.Domain == CircuitState.DomainTypes.Time && ckt.State.UseDC))
                value += ckt.State.States[0][BJTstate + BJTcqcs];
            return value;
        }
        [SpiceName("p"), SpiceInfo("Power dissipation")]
        public double GetPOWER(Circuit ckt)
        {
            double value = ckt.State.States[0][BJTstate + BJTcc] * ckt.State.Real.Solution[BJTcolNode];
            value += ckt.State.States[0][BJTstate + BJTcb] * ckt.State.Real.Solution[BJTbaseNode];
            if (ckt.Method != null && !(ckt.State.Domain == CircuitState.DomainTypes.Time && ckt.State.UseDC))
                value -= ckt.State.States[0][BJTstate + BJTcqcs] * ckt.State.Real.Solution[BJTsubstNode];
            
            double tmp = -ckt.State.States[0][BJTstate + BJTcc];
            tmp -= ckt.State.States[0][BJTstate + BJTcb];
            if (ckt.Method != null && !(ckt.State.Domain == CircuitState.DomainTypes.Time && ckt.State.UseDC))
                tmp += ckt.State.States[0][BJTstate + BJTcqcs];
            value += tmp * ckt.State.Real.Solution[BJTemitNode];
            return value;
        }

        /// <summary>
        /// Extra variables
        /// </summary>
        public double BJTtSatCur { get; private set; }
        public double BJTtBetaF { get; private set; }
        public double BJTtBetaR { get; private set; }
        public double BJTtBEleakCur { get; private set; }
        public double BJTtBCleakCur { get; private set; }
        public double BJTtBEcap { get; private set; }
        public double BJTtBEpot { get; private set; }
        public double BJTtBCcap { get; private set; }
        public double BJTtBCpot { get; private set; }
        public double BJTtDepCap { get; private set; }
        public double BJTtf1 { get; private set; }
        public double BJTtf4 { get; private set; }
        public double BJTtf5 { get; private set; }
        public double BJTtVcrit { get; private set; }
        public int BJTstate { get; private set; }

        /// <summary>
        /// Constants
        /// </summary>
        private const int BJTvbe = 0;
        private const int BJTvbc = 1;
        private const int BJTcc = 2;
        private const int BJTcb = 3;
        private const int BJTgpi = 4;
        private const int BJTgmu = 5;
        private const int BJTgm = 6;
        private const int BJTgo = 7;
        private const int BJTqbe = 8;
        private const int BJTcqbe = 9;
        private const int BJTqbc = 10;
        private const int BJTcqbc = 11;
        private const int BJTqcs = 12;
        private const int BJTcqcs = 13;
        private const int BJTqbx = 14;
        private const int BJTcqbx = 15;
        private const int BJTgx = 16;
        private const int BJTcexbc = 17;
        private const int BJTgeqcb = 18;
        private const int BJTgccs = 19;
        private const int BJTgeqbx = 20;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the device</param>
        public BJT(string name) : base(name)
        {
        }

        /// <summary>
        /// Setup the device
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Setup(Circuit ckt)
        {
            BJTModel model = Model as BJTModel;

            // Allocate nodes
            var nodes = BindNodes(ckt);
            BJTcolNode = nodes[0].Index;
            BJTbaseNode = nodes[1].Index;
            BJTemitNode = nodes[2].Index;
            BJTsubstNode = nodes[3].Index;

            // Allocate states
            BJTstate = ckt.State.GetState(21);

            if (model.BJTcollectorResist.Value == 0)
                BJTcolPrimeNode = BJTcolNode;
            else if (BJTcolPrimeNode == 0)
                BJTcolPrimeNode = CreateNode(ckt).Index;

            if (model.BJTbaseResist.Value == 0)
                BJTbasePrimeNode = BJTbaseNode;
            else if (BJTbasePrimeNode == 0)
                BJTbasePrimeNode = CreateNode(ckt).Index;
            if (model.BJTemitterResist.Value == 0)
                BJTemitPrimeNode = BJTemitNode;
            else if (BJTemitPrimeNode == 0)
                BJTemitPrimeNode = CreateNode(ckt).Index;

            /* macro to make elements with built in test for out of memory */
        }

        /// <summary>
        /// Do temperature-dependent calculations
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Temperature(Circuit ckt)
        {
            BJTModel model = Model as BJTModel;

            double vt, fact2, egfet, arg, pbfact, ratlog, ratio1, factlog, factor, bfactor, pbo, gmaold, gmanew;

            if (!BJTtemp.Given)
                BJTtemp.Value = ckt.State.Temperature;
            vt = BJTtemp * Circuit.CONSTKoverQ;
            fact2 = BJTtemp / Circuit.CONSTRefTemp;
            egfet = 1.16 - (7.02e-4 * BJTtemp * BJTtemp) / (BJTtemp + 1108);
            arg = -egfet / (2 * Circuit.CONSTBoltz * BJTtemp) + 1.1150877 / (Circuit.CONSTBoltz * (Circuit.CONSTRefTemp +
                 Circuit.CONSTRefTemp));
            pbfact = -2 * vt * (1.5 * Math.Log(fact2) + Circuit.CHARGE * arg);

            ratlog = Math.Log(BJTtemp / model.BJTtnom);
            ratio1 = BJTtemp / model.BJTtnom - 1;
            factlog = ratio1 * model.BJTenergyGap / vt + model.BJTtempExpIS * ratlog;
            factor = Math.Exp(factlog);
            BJTtSatCur = model.BJTsatCur * factor;
            bfactor = Math.Exp(ratlog * model.BJTbetaExp);
            BJTtBetaF = model.BJTbetaF * bfactor;
            BJTtBetaR = model.BJTbetaR * bfactor;
            BJTtBEleakCur = model.BJTleakBEcurrent * Math.Exp(factlog / model.BJTleakBEemissionCoeff) / bfactor;
            BJTtBCleakCur = model.BJTleakBCcurrent * Math.Exp(factlog / model.BJTleakBCemissionCoeff) / bfactor;

            pbo = (model.BJTpotentialBE - pbfact) / model.fact1;
            gmaold = (model.BJTpotentialBE - pbo) / pbo;
            BJTtBEcap = model.BJTdepletionCapBE / (1 + model.BJTjunctionExpBE * (4e-4 * (model.BJTtnom - Circuit.CONSTRefTemp) - gmaold));
            BJTtBEpot = fact2 * pbo + pbfact;
            gmanew = (BJTtBEpot - pbo) / pbo;
            BJTtBEcap *= 1 + model.BJTjunctionExpBE * (4e-4 * (BJTtemp - Circuit.CONSTRefTemp) - gmanew);

            pbo = (model.BJTpotentialBC - pbfact) / model.fact1;
            gmaold = (model.BJTpotentialBC - pbo) / pbo;
            BJTtBCcap = model.BJTdepletionCapBC / (1 + model.BJTjunctionExpBC * (4e-4 * (model.BJTtnom - Circuit.CONSTRefTemp) - gmaold));
            BJTtBCpot = fact2 * pbo + pbfact;
            gmanew = (BJTtBCpot - pbo) / pbo;
            BJTtBCcap *= 1 + model.BJTjunctionExpBC * (4e-4 * (BJTtemp - Circuit.CONSTRefTemp) - gmanew);

            BJTtDepCap = model.BJTdepletionCapCoeff * BJTtBEpot;
            BJTtf1 = BJTtBEpot * (1 - Math.Exp((1 - model.BJTjunctionExpBE) * model.xfc)) / (1 - model.BJTjunctionExpBE);
            BJTtf4 = model.BJTdepletionCapCoeff * BJTtBCpot;
            BJTtf5 = BJTtBCpot * (1 - Math.Exp((1 - model.BJTjunctionExpBC) * model.xfc)) / (1 - model.BJTjunctionExpBC);
            BJTtVcrit = vt * Math.Log(vt / (Circuit.CONSTroot2 * model.BJTsatCur));
        }

        /// <summary>
        /// Load the device
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Load(Circuit ckt)
        {
            BJTModel model = Model as BJTModel;
            var state = ckt.State;
            var rstate = state.Real;
            var method = ckt.Method;
            double vt;
            double gccs, ceqcs, geqbx, ceqbx, geqcb, csat, rbpr, rbpi, gcpr, gepr, oik, c2, vte, oikr, c4, vtc, td, xjrb, vbe, vbc, vbx, vcs;
            bool icheck;
            double vce, delvbe, delvbc, cchat, cbhat;
            bool ichk1;
            double vtn, evbe, cbe, gbe, gben, evben, cben, evbc, cbc, gbc, gbcn, evbcn, cbcn, q1, dqbdve, dqbdvc, q2, arg, sqarg, qb, cc, cex,
                gex, arg1, arg2, denom, arg3, cb, gx, gpi, gmu, go, gm, tf, tr, czbe, pe, xme, cdis, ctot, czbc, czbx, pc, xmc, fcpe, czcs, ps,
                xms, xtf, ovtf, xjtf, argtf, temp, sarg, capbe, f1, f2, f3, czbef2, fcpc, capbc, czbcf2, capbx = 0.0, czbxf2, capcs = 0.0;
            double ceqbe, ceqbc, ceq;

            vt = BJTtemp * Circuit.CONSTKoverQ;

            gccs = 0;
            ceqcs = 0;
            geqbx = 0;
            ceqbx = 0;
            geqcb = 0;

            /* 
			 * dc model paramters
			 */
            csat = BJTtSatCur * BJTarea;
            rbpr = model.BJTminBaseResist / BJTarea;
            rbpi = model.BJTbaseResist / BJTarea - rbpr;
            gcpr = model.BJTcollectorConduct * BJTarea;
            gepr = model.BJTemitterConduct * BJTarea;
            oik = model.BJTinvRollOffF / BJTarea;
            c2 = BJTtBEleakCur * BJTarea;
            vte = model.BJTleakBEemissionCoeff * vt;
            oikr = model.BJTinvRollOffR / BJTarea;
            c4 = BJTtBCleakCur * BJTarea;
            vtc = model.BJTleakBCemissionCoeff * vt;
            td = model.BJTexcessPhaseFactor;
            xjrb = model.BJTbaseCurrentHalfResist * BJTarea;

            /* 
			* initialization
			*/
            icheck = true;
            if (state.UseSmallSignal)
            {
                vbe = state.States[0][BJTstate + BJTvbe];
                vbc = state.States[0][BJTstate + BJTvbc];
                vbx = model.BJTtype * (rstate.OldSolution[BJTbaseNode] - rstate.OldSolution[BJTcolPrimeNode]);
                vcs = model.BJTtype * (rstate.OldSolution[BJTsubstNode] - rstate.OldSolution[BJTcolPrimeNode]);
            }
            else if (method != null && method.SavedTime == 0.0)
            {
                vbe = state.States[1][BJTstate + BJTvbe];
                vbc = state.States[1][BJTstate + BJTvbc];
                vbx = model.BJTtype * (rstate.OldSolution[BJTbaseNode] - rstate.OldSolution[BJTcolPrimeNode]);
                vcs = model.BJTtype * (rstate.OldSolution[BJTsubstNode] - rstate.OldSolution[BJTcolPrimeNode]);
                if (state.UseIC)
                {
                    vbx = model.BJTtype * (BJTicVBE - BJTicVCE);
                    vcs = 0;
                }
                icheck = false; // EDIT: Spice does not check the first timepoint for convergence, but we do...
            }
            else if (state.Init == CircuitState.InitFlags.InitJct && state.Domain == CircuitState.DomainTypes.Time && state.UseDC && state.UseIC)
            {
                vbe = model.BJTtype * BJTicVBE;
                vce = model.BJTtype * BJTicVCE;
                vbc = vbe - vce;
                vbx = vbc;
                vcs = 0;
            }
            else if (state.Init == CircuitState.InitFlags.InitJct && !BJToff)
            {
                vbe = BJTtVcrit;
                vbc = 0;
                /* ERROR:  need to initialize VCS, VBX here */
                vcs = vbx = 0;
            }
            else if (state.Init == CircuitState.InitFlags.InitJct || (state.Init == CircuitState.InitFlags.InitFix && BJToff))
            {
                vbe = 0;
                vbc = 0;
                /* ERROR:  need to initialize VCS, VBX here */
                vcs = vbx = 0;
            }
            else
            {
                /* 
                 * compute new nonlinear branch voltages
                 */
                vbe = model.BJTtype * (rstate.OldSolution[BJTbasePrimeNode] - rstate.OldSolution[BJTemitPrimeNode]);
                vbc = model.BJTtype * (rstate.OldSolution[BJTbasePrimeNode] - rstate.OldSolution[BJTcolPrimeNode]);

                /* PREDICTOR */
                delvbe = vbe - state.States[0][BJTstate + BJTvbe];
                delvbc = vbc - state.States[0][BJTstate + BJTvbc];
                vbx = model.BJTtype * (rstate.OldSolution[BJTbaseNode] - rstate.OldSolution[BJTcolPrimeNode]);
                vcs = model.BJTtype * (rstate.OldSolution[BJTsubstNode] - rstate.OldSolution[BJTcolPrimeNode]);
                cchat = state.States[0][BJTstate + BJTcc] + (state.States[0][BJTstate + BJTgm] + state.States[0][BJTstate + BJTgo]) * delvbe -
                     (state.States[0][BJTstate + BJTgo] + state.States[0][BJTstate + BJTgmu]) * delvbc;
                cbhat = state.States[0][BJTstate + BJTcb] + state.States[0][BJTstate + BJTgpi] * delvbe + state.States[0][BJTstate + BJTgmu] *
                     delvbc;
                /* NOBYPASS */
                /* 
				 * limit nonlinear branch voltages
				 */
                ichk1 = true;
                vbe = Semiconductor.DEVpnjlim(vbe, state.States[0][BJTstate + BJTvbe], vt, BJTtVcrit, ref icheck);
                vbc = Semiconductor.DEVpnjlim(vbc, state.States[0][BJTstate + BJTvbc], vt, BJTtVcrit, ref ichk1);
                if (ichk1 == true)
                    icheck = true;
            }

            /* 
			 * determine dc current and derivitives
			 */
            vtn = vt * model.BJTemissionCoeffF;
            if (vbe > -5 * vtn)
            {
                evbe = Math.Exp(vbe / vtn);
                cbe = csat * (evbe - 1) + state.Gmin * vbe;
                gbe = csat * evbe / vtn + state.Gmin;
                if (c2 == 0)
                {
                    cben = 0;
                    gben = 0;
                }
                else
                {
                    evben = Math.Exp(vbe / vte);
                    cben = c2 * (evben - 1);
                    gben = c2 * evben / vte;
                }
            }
            else
            {
                gbe = -csat / vbe + state.Gmin;
                cbe = gbe * vbe;
                gben = -c2 / vbe;
                cben = gben * vbe;
            }
            vtn = vt * model.BJTemissionCoeffR;
            if (vbc > -5 * vtn)
            {
                evbc = Math.Exp(vbc / vtn);
                cbc = csat * (evbc - 1) + state.Gmin * vbc;
                gbc = csat * evbc / vtn + state.Gmin;
                if (c4 == 0)
                {
                    cbcn = 0;
                    gbcn = 0;
                }
                else
                {
                    evbcn = Math.Exp(vbc / vtc);
                    cbcn = c4 * (evbcn - 1);
                    gbcn = c4 * evbcn / vtc;
                }
            }
            else
            {
                gbc = -csat / vbc + state.Gmin;
                cbc = gbc * vbc;
                gbcn = -c4 / vbc;
                cbcn = gbcn * vbc;
            }
            /* 
			 * determine base charge terms
			 */
            q1 = 1 / (1 - model.BJTinvEarlyVoltF * vbc - model.BJTinvEarlyVoltR * vbe);
            if (oik == 0 && oikr == 0)
            {
                qb = q1;
                dqbdve = q1 * qb * model.BJTinvEarlyVoltR;
                dqbdvc = q1 * qb * model.BJTinvEarlyVoltF;
            }
            else
            {
                q2 = oik * cbe + oikr * cbc;
                arg = Math.Max(0, 1 + 4 * q2);
                sqarg = 1;
                if (arg != 0)
                    sqarg = Math.Sqrt(arg);
                qb = q1 * (1 + sqarg) / 2;
                dqbdve = q1 * (qb * model.BJTinvEarlyVoltR + oik * gbe / sqarg);
                dqbdvc = q1 * (qb * model.BJTinvEarlyVoltF + oikr * gbc / sqarg);
            }
            /* 
			 * weil's approx. for excess phase applied with backward - 
			 * euler integration
			 */
            cc = 0;
            cex = cbe;
            gex = gbe;
            if (method != null && td != 0)
            {
                arg1 = method.Delta / td;
                arg2 = 3 * arg1;
                arg1 = arg2 * arg1;
                denom = 1 + arg1 + arg2;
                arg3 = arg1 / denom;
                if (method.SavedTime == 0.0)
                {
                    state.States[1][BJTstate + BJTcexbc] = cbe / qb;
                    state.States[2][BJTstate + BJTcexbc] = state.States[1][BJTstate + BJTcexbc];
                }
                cc = (state.States[1][BJTstate + BJTcexbc] * (1 + method.Delta / method.DeltaOld[1] + arg2) - state.States[2][BJTstate +
                     BJTcexbc] * method.Delta / method.DeltaOld[1]) / denom;
                cex = cbe * arg3;
                gex = gbe * arg3;
                state.States[0][BJTstate + BJTcexbc] = cc + cex / qb;
            }

            /* 
			 * determine dc incremental conductances
			 */
            cc = cc + (cex - cbc) / qb - cbc / BJTtBetaR - cbcn;
            cb = cbe / BJTtBetaF + cben + cbc / BJTtBetaR + cbcn;
            gx = rbpr + rbpi / qb;
            if (xjrb != 0)
            {
                arg1 = Math.Max(cb / xjrb, 1e-9);
                arg2 = (-1 + Math.Sqrt(1 + 14.59025 * arg1)) / 2.4317 / Math.Sqrt(arg1);
                arg1 = Math.Tan(arg2);
                gx = rbpr + 3 * rbpi * (arg1 - arg2) / arg2 / arg1 / arg1;
            }
            if (gx != 0)
                gx = 1 / gx;
            gpi = gbe / BJTtBetaF + gben;
            gmu = gbc / BJTtBetaR + gbcn;
            go = (gbc + (cex - cbc) * dqbdvc / qb) / qb;
            gm = (gex - (cex - cbc) * dqbdve / qb) / qb - go;

            if (state.Domain == CircuitState.DomainTypes.Time || state.UseIC || state.UseSmallSignal)
            {
                /* 
				 * charge storage elements
				 */
                tf = model.BJTtransitTimeF;
                tr = model.BJTtransitTimeR;
                czbe = BJTtBEcap * BJTarea;
                pe = BJTtBEpot;
                xme = model.BJTjunctionExpBE;
                cdis = model.BJTbaseFractionBCcap;
                ctot = BJTtBCcap * BJTarea;
                czbc = ctot * cdis;
                czbx = ctot - czbc;
                pc = BJTtBCpot;
                xmc = model.BJTjunctionExpBC;
                fcpe = BJTtDepCap;
                czcs = model.BJTcapCS * BJTarea;
                ps = model.BJTpotentialSubstrate;
                xms = model.BJTexponentialSubstrate;
                xtf = model.BJTtransitTimeBiasCoeffF;
                ovtf = model.BJTtransitTimeVBCFactor;
                xjtf = model.BJTtransitTimeHighCurrentF * BJTarea;
                if (tf != 0 && vbe > 0)
                {
                    argtf = 0;
                    arg2 = 0;
                    arg3 = 0;
                    if (xtf != 0)
                    {
                        argtf = xtf;
                        if (ovtf != 0)
                        {
                            argtf = argtf * Math.Exp(vbc * ovtf);
                        }
                        arg2 = argtf;
                        if (xjtf != 0)
                        {
                            temp = cbe / (cbe + xjtf);
                            argtf = argtf * temp * temp;
                            arg2 = argtf * (3 - temp - temp);
                        }
                        arg3 = cbe * argtf * ovtf;
                    }
                    cbe = cbe * (1 + argtf) / qb;
                    gbe = (gbe * (1 + arg2) - cbe * dqbdve) / qb;
                    geqcb = tf * (arg3 - cbe * dqbdvc) / qb;
                }
                if (vbe < fcpe)
                {
                    arg = 1 - vbe / pe;
                    sarg = Math.Exp(-xme * Math.Log(arg));
                    state.States[0][BJTstate + BJTqbe] = tf * cbe + pe * czbe * (1 - arg * sarg) / (1 - xme);
                    capbe = tf * gbe + czbe * sarg;
                }
                else
                {
                    f1 = BJTtf1;
                    f2 = model.BJTf2;
                    f3 = model.BJTf3;
                    czbef2 = czbe / f2;
                    state.States[0][BJTstate + BJTqbe] = tf * cbe + czbe * f1 + czbef2 * (f3 * (vbe - fcpe) + (xme / (pe + pe)) * (vbe * vbe -
                         fcpe * fcpe));
                    capbe = tf * gbe + czbef2 * (f3 + xme * vbe / pe);
                }
                fcpc = BJTtf4;
                f1 = BJTtf5;
                f2 = model.BJTf6;
                f3 = model.BJTf7;
                if (vbc < fcpc)
                {
                    arg = 1 - vbc / pc;
                    sarg = Math.Exp(-xmc * Math.Log(arg));
                    state.States[0][BJTstate + BJTqbc] = tr * cbc + pc * czbc * (1 - arg * sarg) / (1 - xmc);
                    capbc = tr * gbc + czbc * sarg;
                }
                else
                {
                    czbcf2 = czbc / f2;
                    state.States[0][BJTstate + BJTqbc] = tr * cbc + czbc * f1 + czbcf2 * (f3 * (vbc - fcpc) + (xmc / (pc + pc)) * (vbc * vbc -
                         fcpc * fcpc));
                    capbc = tr * gbc + czbcf2 * (f3 + xmc * vbc / pc);
                }
                if (vbx < fcpc)
                {
                    arg = 1 - vbx / pc;
                    sarg = Math.Exp(-xmc * Math.Log(arg));
                    state.States[0][BJTstate + BJTqbx] = pc * czbx * (1 - arg * sarg) / (1 - xmc);
                    capbx = czbx * sarg;
                }
                else
                {
                    czbxf2 = czbx / f2;
                    state.States[0][BJTstate + BJTqbx] = czbx * f1 + czbxf2 * (f3 * (vbx - fcpc) + (xmc / (pc + pc)) * (vbx * vbx - fcpc * fcpc));
                    capbx = czbxf2 * (f3 + xmc * vbx / pc);
                }
                if (vcs < 0)
                {
                    arg = 1 - vcs / ps;
                    sarg = Math.Exp(-xms * Math.Log(arg));
                    state.States[0][BJTstate + BJTqcs] = ps * czcs * (1 - arg * sarg) / (1 - xms);
                    capcs = czcs * sarg;
                }
                else
                {
                    state.States[0][BJTstate + BJTqcs] = vcs * czcs * (1 + xms * vcs / (2 * ps));
                    capcs = czcs * (1 + xms * vcs / ps);
                }
                BJTcapbe = capbe;
                BJTcapbc = capbc;
                BJTcapcs = capcs;
                BJTcapbx = capbx;

                /* 
				 * store small - signal parameters
				 */
                if (!(state.Domain == CircuitState.DomainTypes.Time && state.UseDC && state.UseIC))
                {
                    if (state.UseSmallSignal)
                    {
                        state.States[0][BJTstate + BJTcqbe] = capbe;
                        state.States[0][BJTstate + BJTcqbc] = capbc;
                        state.States[0][BJTstate + BJTcqcs] = capcs;
                        state.States[0][BJTstate + BJTcqbx] = capbx;
                        state.States[0][BJTstate + BJTcexbc] = geqcb;

                        /* SENSDEBUG */
                        return; /* go to 1000 */
                    }
                    /* 
					 * transient analysis
					 */

                    if (method != null && method.SavedTime == 0.0)
                    {
                        state.States[1][BJTstate + BJTqbe] = state.States[0][BJTstate + BJTqbe];
                        state.States[1][BJTstate + BJTqbc] = state.States[0][BJTstate + BJTqbc];
                        state.States[1][BJTstate + BJTqbx] = state.States[0][BJTstate + BJTqbx];
                        state.States[1][BJTstate + BJTqcs] = state.States[0][BJTstate + BJTqcs];
                    }

                    if (method != null)
                    {
                        var result = method.Integrate(state, BJTstate + BJTqbe, capbe);
                        geqcb = geqcb * method.Slope;
                        gpi = gpi + result.Geq;
                        result = method.Integrate(state, BJTstate + BJTqbc, capbc);
                        gmu = gmu + result.Geq;
                    }
                    cb = cb + state.States[0][BJTstate + BJTcqbe];
                    cb = cb + state.States[0][BJTstate + BJTcqbc];
                    cc = cc - state.States[0][BJTstate + BJTcqbc];

                    if (method != null && method.SavedTime == 0.0)
                    {
                        state.States[1][BJTstate + BJTcqbe] = state.States[0][BJTstate + BJTcqbe];
                        state.States[1][BJTstate + BJTcqbc] = state.States[0][BJTstate + BJTcqbc];
                    }
                }
            }

            /* 
			 * check convergence
			 */
            if (state.Init != CircuitState.InitFlags.InitFix || !BJToff)
            {
                if (icheck)
                    state.IsCon = false;
            }

            /* 
			 * charge storage for c - s and b - x junctions
			 */
            if (method != null)
            {
                method.Integrate(state, out gccs, out ceq, BJTstate + BJTqcs, capcs);
                method.Integrate(state, out geqbx, out ceq, BJTstate + BJTqbx, capbx);
                if (method.SavedTime == 0.0)
                {
                    state.States[1][BJTstate + BJTcqbx] = state.States[0][BJTstate + BJTcqbx];
                    state.States[1][BJTstate + BJTcqcs] = state.States[0][BJTstate + BJTcqcs];
                }
            }

            state.States[0][BJTstate + BJTvbe] = vbe;
            state.States[0][BJTstate + BJTvbc] = vbc;
            state.States[0][BJTstate + BJTcc] = cc;
            state.States[0][BJTstate + BJTcb] = cb;
            state.States[0][BJTstate + BJTgpi] = gpi;
            state.States[0][BJTstate + BJTgmu] = gmu;
            state.States[0][BJTstate + BJTgm] = gm;
            state.States[0][BJTstate + BJTgo] = go;
            state.States[0][BJTstate + BJTgx] = gx;
            state.States[0][BJTstate + BJTgeqcb] = geqcb;
            state.States[0][BJTstate + BJTgccs] = gccs;
            state.States[0][BJTstate + BJTgeqbx] = geqbx;

            /* Do not load the Jacobian and the rhs if
			   perturbation is being carried out */

            /* 
			 * load current excitation vector
			 */
            ceqcs = model.BJTtype * (state.States[0][BJTstate + BJTcqcs] - vcs * gccs);
            ceqbx = model.BJTtype * (state.States[0][BJTstate + BJTcqbx] - vbx * geqbx);
            ceqbe = model.BJTtype * (cc + cb - vbe * (gm + go + gpi) + vbc * (go - geqcb));
            ceqbc = model.BJTtype * (-cc + vbe * (gm + go) - vbc * (gmu + go));

            rstate.Rhs[BJTbaseNode] += (-ceqbx);
            rstate.Rhs[BJTcolPrimeNode] += (ceqcs + ceqbx + ceqbc);
            rstate.Rhs[BJTbasePrimeNode] += (-ceqbe - ceqbc);
            rstate.Rhs[BJTemitPrimeNode] += (ceqbe);
            rstate.Rhs[BJTsubstNode] += (-ceqcs);
            /* 
			 * load y matrix
			 */
            rstate.Matrix[BJTcolNode, BJTcolNode] += (gcpr);
            rstate.Matrix[BJTbaseNode, BJTbaseNode] += (gx + geqbx);
            rstate.Matrix[BJTemitNode, BJTemitNode] += (gepr);
            rstate.Matrix[BJTcolPrimeNode, BJTcolPrimeNode] += (gmu + go + gcpr + gccs + geqbx);
            rstate.Matrix[BJTbasePrimeNode, BJTbasePrimeNode] += (gx + gpi + gmu + geqcb);
            rstate.Matrix[BJTemitPrimeNode, BJTemitPrimeNode] += (gpi + gepr + gm + go);
            rstate.Matrix[BJTcolNode, BJTcolPrimeNode] += (-gcpr);
            rstate.Matrix[BJTbaseNode, BJTbasePrimeNode] += (-gx);
            rstate.Matrix[BJTemitNode, BJTemitPrimeNode] += (-gepr);
            rstate.Matrix[BJTcolPrimeNode, BJTcolNode] += (-gcpr);
            rstate.Matrix[BJTcolPrimeNode, BJTbasePrimeNode] += (-gmu + gm);
            rstate.Matrix[BJTcolPrimeNode, BJTemitPrimeNode] += (-gm - go);
            rstate.Matrix[BJTbasePrimeNode, BJTbaseNode] += (-gx);
            rstate.Matrix[BJTbasePrimeNode, BJTcolPrimeNode] += (-gmu - geqcb);
            rstate.Matrix[BJTbasePrimeNode, BJTemitPrimeNode] += (-gpi);
            rstate.Matrix[BJTemitPrimeNode, BJTemitNode] += (-gepr);
            rstate.Matrix[BJTemitPrimeNode, BJTcolPrimeNode] += (-go + geqcb);
            rstate.Matrix[BJTemitPrimeNode, BJTbasePrimeNode] += (-gpi - gm - geqcb);
            rstate.Matrix[BJTsubstNode, BJTsubstNode] += (gccs);
            rstate.Matrix[BJTcolPrimeNode, BJTsubstNode] += (-gccs);
            rstate.Matrix[BJTsubstNode, BJTcolPrimeNode] += (-gccs);
            rstate.Matrix[BJTbaseNode, BJTcolPrimeNode] += (-geqbx);
            rstate.Matrix[BJTcolPrimeNode, BJTbaseNode] += (-geqbx);
        }

        /// <summary>
        /// Load the device for AC simulation
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void AcLoad(Circuit ckt)
        {
            BJTModel model = Model as BJTModel;
            var state = ckt.State;
            var cstate = state.Complex;
            double gcpr, gepr, gpi, gmu, go, td, gx;
            Complex gm, xcpi, xcmu, xcbx, xccs, xcmcb;

            gcpr = model.BJTcollectorConduct * BJTarea;
            gepr = model.BJTemitterConduct * BJTarea;
            gpi = state.States[0][BJTstate + BJTgpi];
            gmu = state.States[0][BJTstate + BJTgmu];
            gm = state.States[0][BJTstate + BJTgm];
            go = state.States[0][BJTstate + BJTgo];
            td = model.BJTexcessPhaseFactor;
            if (td != 0)
            {
                Complex arg = td * cstate.Laplace;

                gm = gm + go;
                gm = gm * Complex.Exp(-arg);
                gm = gm - go;
            }
            gx = state.States[0][BJTstate + BJTgx];
            xcpi = state.States[0][BJTstate + BJTcqbe] * cstate.Laplace;
            xcmu = state.States[0][BJTstate + BJTcqbc] * cstate.Laplace;
            xcbx = state.States[0][BJTstate + BJTcqbx] * cstate.Laplace;
            xccs = state.States[0][BJTstate + BJTcqcs] * cstate.Laplace;
            xcmcb = state.States[0][BJTstate + BJTcexbc] * cstate.Laplace;

            cstate.Matrix[BJTcolNode, BJTcolNode] += gcpr;
            cstate.Matrix[BJTbaseNode, BJTbaseNode] += gx + xcbx;
            cstate.Matrix[BJTemitNode, BJTemitNode] += gepr;
            cstate.Matrix[BJTcolPrimeNode, BJTcolPrimeNode] += (gmu + go + gcpr) + (xcmu + xccs + xcbx);
            cstate.Matrix[BJTbasePrimeNode, BJTbasePrimeNode] += (gx + gpi + gmu) + (xcpi + xcmu + xcmcb);
            cstate.Matrix[BJTemitPrimeNode, BJTemitPrimeNode] += (gpi + gepr + gm + go) + xcpi;

            cstate.Matrix[BJTcolNode, BJTcolPrimeNode] -= gcpr;
            cstate.Matrix[BJTbaseNode, BJTbasePrimeNode] -= gx;
            cstate.Matrix[BJTemitNode, BJTemitPrimeNode] -= gepr;

            cstate.Matrix[BJTcolPrimeNode, BJTcolNode] -= gcpr;
            cstate.Matrix[BJTcolPrimeNode, BJTbasePrimeNode] += (-gmu + gm) + (-xcmu);
            cstate.Matrix[BJTcolPrimeNode, BJTemitPrimeNode] += (-gm - go);
            cstate.Matrix[BJTbasePrimeNode, BJTbaseNode] -= gx;
            cstate.Matrix[BJTbasePrimeNode, BJTcolPrimeNode] -= gmu + xcmu + xcmcb;
            cstate.Matrix[BJTbasePrimeNode, BJTemitPrimeNode] -= gpi + xcpi;
            cstate.Matrix[BJTemitPrimeNode, BJTemitNode] -= gepr;
            cstate.Matrix[BJTemitPrimeNode, BJTcolPrimeNode] += -go + xcmcb;
            cstate.Matrix[BJTemitPrimeNode, BJTbasePrimeNode] -= (gpi + gm) + (xcpi + xcmcb);

            cstate.Matrix[BJTsubstNode, BJTsubstNode] += xccs;
            cstate.Matrix[BJTcolPrimeNode, BJTsubstNode] -= xccs;
            cstate.Matrix[BJTsubstNode, BJTcolPrimeNode] -= xccs;
            cstate.Matrix[BJTbaseNode, BJTcolPrimeNode] -= xcbx;
            cstate.Matrix[BJTcolPrimeNode, BJTbaseNode] -= xcbx;
        }
    }
}
