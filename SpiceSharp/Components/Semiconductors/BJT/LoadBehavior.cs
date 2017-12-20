using System;
using SpiceSharp.Circuits;
using SpiceSharp.Components.Semiconductors;
using SpiceSharp.Parameters;
using SpiceSharp.Sparse;
using SpiceSharp.Diagnostics;

namespace SpiceSharp.Behaviors.BJT
{
    /// <summary>
    /// General behavior for <see cref="BJT"/>
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
        public Parameter BJTarea { get; } = new Parameter(1);
        [SpiceName("off"), SpiceInfo("Device initially off")]
        public bool BJToff { get; set; }
        [SpiceName("icvbe"), SpiceInfo("Initial B-E voltage")]
        public Parameter BJTicVBE { get; } = new Parameter();
        [SpiceName("icvce"), SpiceInfo("Initial C-E voltage")]
        public Parameter BJTicVCE { get; } = new Parameter();
        [SpiceName("sens_area"), SpiceInfo("flag to request sensitivity WRT area")]
        public bool BJTsenParmNo { get; set; }
        [SpiceName("cpi"), SpiceInfo("Internal base to emitter capactance")]
        public double BJTcapbe { get; internal set; }
        [SpiceName("cmu"), SpiceInfo("Internal base to collector capactiance")]
        public double BJTcapbc { get; internal set; }
        [SpiceName("cbx"), SpiceInfo("Base to collector capacitance")]
        public double BJTcapbx { get; internal set; }
        [SpiceName("ccs"), SpiceInfo("Collector to substrate capacitance")]
        public double BJTcapcs { get; internal set; }

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
            if (ckt.Method != null && !(ckt.State.Domain == State.DomainTypes.Time && ckt.State.UseDC))
                value += ckt.State.States[0][BJTstate + BJTcqcs];
            return value;
        }
        [SpiceName("p"), SpiceInfo("Power dissipation")]
        public double GetPOWER(Circuit ckt)
        {
            double value = ckt.State.States[0][BJTstate + BJTcc] * ckt.State.Solution[BJTcolNode];
            value += ckt.State.States[0][BJTstate + BJTcb] * ckt.State.Solution[BJTbaseNode];
            if (ckt.Method != null && !(ckt.State.Domain == State.DomainTypes.Time && ckt.State.UseDC))
                value -= ckt.State.States[0][BJTstate + BJTcqcs] * ckt.State.Solution[BJTsubstNode];

            double tmp = -ckt.State.States[0][BJTstate + BJTcc];
            tmp -= ckt.State.States[0][BJTstate + BJTcb];
            if (ckt.Method != null && !(ckt.State.Domain == State.DomainTypes.Time && ckt.State.UseDC))
                tmp += ckt.State.States[0][BJTstate + BJTcqcs];
            value += tmp * ckt.State.Solution[BJTemitNode];
            return value;
        }

        /// <summary>
        /// Extra variables
        /// </summary>
        public int BJTstate { get; protected set; }

        /// <summary>
        /// Nodes
        /// </summary>
        private int BJTcolNode, BJTbaseNode, BJTemitNode, BJTsubstNode;
        public int BJTcolPrimeNode { get; private set; }
        public int BJTbasePrimeNode { get; private set; }
        public int BJTemitPrimeNode { get; private set; }
        protected MatrixElement BJTcolColPrimePtr { get; private set; }
        protected MatrixElement BJTbaseBasePrimePtr { get; private set; }
        protected MatrixElement BJTemitEmitPrimePtr { get; private set; }
        protected MatrixElement BJTcolPrimeColPtr { get; private set; }
        protected MatrixElement BJTcolPrimeBasePrimePtr { get; private set; }
        protected MatrixElement BJTcolPrimeEmitPrimePtr { get; private set; }
        protected MatrixElement BJTbasePrimeBasePtr { get; private set; }
        protected MatrixElement BJTbasePrimeColPrimePtr { get; private set; }
        protected MatrixElement BJTbasePrimeEmitPrimePtr { get; private set; }
        protected MatrixElement BJTemitPrimeEmitPtr { get; private set; }
        protected MatrixElement BJTemitPrimeColPrimePtr { get; private set; }
        protected MatrixElement BJTemitPrimeBasePrimePtr { get; private set; }
        protected MatrixElement BJTcolColPtr { get; private set; }
        protected MatrixElement BJTbaseBasePtr { get; private set; }
        protected MatrixElement BJTemitEmitPtr { get; private set; }
        protected MatrixElement BJTcolPrimeColPrimePtr { get; private set; }
        protected MatrixElement BJTbasePrimeBasePrimePtr { get; private set; }
        protected MatrixElement BJTemitPrimeEmitPrimePtr { get; private set; }
        protected MatrixElement BJTsubstSubstPtr { get; private set; }
        protected MatrixElement BJTcolPrimeSubstPtr { get; private set; }
        protected MatrixElement BJTsubstColPrimePtr { get; private set; }
        protected MatrixElement BJTbaseColPrimePtr { get; private set; }
        protected MatrixElement BJTcolPrimeBasePtr { get; private set; }

        /// <summary>
        /// Constants
        /// </summary>
        public const int BJTvbe = 0;
        public const int BJTvbc = 1;
        public const int BJTcc = 2;
        public const int BJTcb = 3;
        public const int BJTgpi = 4;
        public const int BJTgmu = 5;
        public const int BJTgm = 6;
        public const int BJTgo = 7;
        public const int BJTqbe = 8;
        public const int BJTcqbe = 9;
        public const int BJTqbc = 10;
        public const int BJTcqbc = 11;
        public const int BJTqcs = 12;
        public const int BJTcqcs = 13;
        public const int BJTqbx = 14;
        public const int BJTcqbx = 15;
        public const int BJTgx = 16;
        public const int BJTcexbc = 17;
        public const int BJTgeqcb = 18;
        public const int BJTgccs = 19;
        public const int BJTgeqbx = 20;

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        public override void Setup(Entity component, Circuit ckt)
        {
            var bjt = component as Components.BJT;

            // Get necessary behaviors
            temp = GetBehavior<TemperatureBehavior>(component);
            modeltemp = GetBehavior<ModelTemperatureBehavior>(bjt.Model);

            // Allocate states
            BJTstate = ckt.State.GetState(21);

            // Get connected nodes
            BJTcolNode = bjt.BJTcolNode;
            BJTbaseNode = bjt.BJTbaseNode;
            BJTemitNode = bjt.BJTemitNode;
            BJTsubstNode = bjt.BJTsubstNode;

            // Add a series collector node if necessary
            if (modeltemp.BJTcollectorResist.Value == 0)
                BJTcolPrimeNode = BJTcolNode;
            else if (BJTcolPrimeNode == 0)
                BJTcolPrimeNode = CreateNode(ckt, bjt.Name.Grow("#col")).Index;

            // Add a series base node if necessary
            if (modeltemp.BJTbaseResist.Value == 0)
                BJTbasePrimeNode = BJTbaseNode;
            else if (BJTbasePrimeNode == 0)
                BJTbasePrimeNode = CreateNode(ckt, bjt.Name.Grow("#base")).Index;

            // Add a series emitter node if necessary
            if (modeltemp.BJTemitterResist.Value == 0)
                BJTemitPrimeNode = BJTemitNode;
            else if (BJTemitPrimeNode == 0)
                BJTemitPrimeNode = CreateNode(ckt, bjt.Name.Grow("#emit")).Index;

            // Get matrix elements
            var matrix = ckt.State.Matrix;
            BJTcolColPrimePtr = matrix.GetElement(BJTcolNode, BJTcolPrimeNode);
            BJTbaseBasePrimePtr = matrix.GetElement(BJTbaseNode, BJTbasePrimeNode);
            BJTemitEmitPrimePtr = matrix.GetElement(BJTemitNode, BJTemitPrimeNode);
            BJTcolPrimeColPtr = matrix.GetElement(BJTcolPrimeNode, BJTcolNode);
            BJTcolPrimeBasePrimePtr = matrix.GetElement(BJTcolPrimeNode, BJTbasePrimeNode);
            BJTcolPrimeEmitPrimePtr = matrix.GetElement(BJTcolPrimeNode, BJTemitPrimeNode);
            BJTbasePrimeBasePtr = matrix.GetElement(BJTbasePrimeNode, BJTbaseNode);
            BJTbasePrimeColPrimePtr = matrix.GetElement(BJTbasePrimeNode, BJTcolPrimeNode);
            BJTbasePrimeEmitPrimePtr = matrix.GetElement(BJTbasePrimeNode, BJTemitPrimeNode);
            BJTemitPrimeEmitPtr = matrix.GetElement(BJTemitPrimeNode, BJTemitNode);
            BJTemitPrimeColPrimePtr = matrix.GetElement(BJTemitPrimeNode, BJTcolPrimeNode);
            BJTemitPrimeBasePrimePtr = matrix.GetElement(BJTemitPrimeNode, BJTbasePrimeNode);
            BJTcolColPtr = matrix.GetElement(BJTcolNode, BJTcolNode);
            BJTbaseBasePtr = matrix.GetElement(BJTbaseNode, BJTbaseNode);
            BJTemitEmitPtr = matrix.GetElement(BJTemitNode, BJTemitNode);
            BJTcolPrimeColPrimePtr = matrix.GetElement(BJTcolPrimeNode, BJTcolPrimeNode);
            BJTbasePrimeBasePrimePtr = matrix.GetElement(BJTbasePrimeNode, BJTbasePrimeNode);
            BJTemitPrimeEmitPrimePtr = matrix.GetElement(BJTemitPrimeNode, BJTemitPrimeNode);
            BJTsubstSubstPtr = matrix.GetElement(BJTsubstNode, BJTsubstNode);
            BJTcolPrimeSubstPtr = matrix.GetElement(BJTcolPrimeNode, BJTsubstNode);
            BJTsubstColPrimePtr = matrix.GetElement(BJTsubstNode, BJTcolPrimeNode);
            BJTbaseColPrimePtr = matrix.GetElement(BJTbaseNode, BJTcolPrimeNode);
            BJTcolPrimeBasePtr = matrix.GetElement(BJTcolPrimeNode, BJTbaseNode);
        }

        /// <summary>
        /// Unsetup
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Unsetup()
        {
            // Remove references
            BJTcolColPrimePtr = null;
            BJTbaseBasePrimePtr = null;
            BJTemitEmitPrimePtr = null;
            BJTcolPrimeColPtr = null;
            BJTcolPrimeBasePrimePtr = null;
            BJTcolPrimeEmitPrimePtr = null;
            BJTbasePrimeBasePtr = null;
            BJTbasePrimeColPrimePtr = null;
            BJTbasePrimeEmitPrimePtr = null;
            BJTemitPrimeEmitPtr = null;
            BJTemitPrimeColPrimePtr = null;
            BJTemitPrimeBasePrimePtr = null;
            BJTcolColPtr = null;
            BJTbaseBasePtr = null;
            BJTemitEmitPtr = null;
            BJTcolPrimeColPrimePtr = null;
            BJTbasePrimeBasePrimePtr = null;
            BJTemitPrimeEmitPrimePtr = null;
            BJTsubstSubstPtr = null;
            BJTcolPrimeSubstPtr = null;
            BJTsubstColPrimePtr = null;
            BJTbaseColPrimePtr = null;
            BJTcolPrimeBasePtr = null;
        }

        /// <summary>
        /// Execute behavior for DC and Transient analysis
        /// </summary>
        /// <param name="ckt"></param>
        public override void Load(Circuit ckt)
        {
            var state = ckt.State;
            var rstate = state;
            var method = ckt.Method;
            double vt;
            double gccs, ceqcs, geqbx, ceqbx, geqcb, csat, rbpr, rbpi, gcpr, gepr, oik, c2, vte, oikr, c4, vtc, td, xjrb, vbe, vbc, vbx, vcs;
            bool icheck;
            double vce;
            bool ichk1;
            double vtn, evbe, cbe, gbe, gben, evben, cben, evbc, cbc, gbc, gbcn, evbcn, cbcn, q1, dqbdve, dqbdvc, q2, arg, sqarg, qb, cc, cex,
                gex, arg1, arg2, denom, arg3, cb, gx, gpi, gmu, go, gm, tf, tr, czbe, pe, xme, cdis, ctot, czbc, czbx, pc, xmc, fcpe, czcs, ps,
                xms, xtf, ovtf, xjtf, argtf, temp, sarg, capbe, f1, f2, f3, czbef2, fcpc, capbc, czbcf2, capbx = 0.0, czbxf2, capcs = 0.0;
            double ceqbe, ceqbc, ceq;

            vt = this.temp.BJTtemp * Circuit.CONSTKoverQ;

            gccs = 0;
            ceqcs = 0;
            geqbx = 0;
            ceqbx = 0;
            geqcb = 0;

            /* 
			 * dc model paramters
			 */
            csat = this.temp.BJTtSatCur * BJTarea;
            rbpr = modeltemp.BJTminBaseResist / BJTarea;
            rbpi = modeltemp.BJTbaseResist / BJTarea - rbpr;
            gcpr = modeltemp.BJTcollectorConduct * BJTarea;
            gepr = modeltemp.BJTemitterConduct * BJTarea;
            oik = modeltemp.BJTinvRollOffF / BJTarea;
            c2 = this.temp.BJTtBEleakCur * BJTarea;
            vte = modeltemp.BJTleakBEemissionCoeff * vt;
            oikr = modeltemp.BJTinvRollOffR / BJTarea;
            c4 = this.temp.BJTtBCleakCur * BJTarea;
            vtc = modeltemp.BJTleakBCemissionCoeff * vt;
            td = modeltemp.BJTexcessPhaseFactor;
            xjrb = modeltemp.BJTbaseCurrentHalfResist * BJTarea;

            /* 
			* initialization
			*/
            icheck = true;
            if (state.UseSmallSignal)
            {
                vbe = state.States[0][BJTstate + BJTvbe];
                vbc = state.States[0][BJTstate + BJTvbc];
                vbx = modeltemp.BJTtype * (rstate.Solution[BJTbaseNode] - rstate.Solution[BJTcolPrimeNode]);
                vcs = modeltemp.BJTtype * (rstate.Solution[BJTsubstNode] - rstate.Solution[BJTcolPrimeNode]);
            }
            else if (state.Init == State.InitFlags.InitTransient)
            {
                vbe = state.States[1][BJTstate + BJTvbe];
                vbc = state.States[1][BJTstate + BJTvbc];
                vbx = modeltemp.BJTtype * (rstate.Solution[BJTbaseNode] - rstate.Solution[BJTcolPrimeNode]);
                vcs = modeltemp.BJTtype * (rstate.Solution[BJTsubstNode] - rstate.Solution[BJTcolPrimeNode]);
                if (state.UseIC)
                {
                    vbx = modeltemp.BJTtype * (BJTicVBE - BJTicVCE);
                    vcs = 0;
                }
                icheck = false; // EDIT: Spice does not check the first timepoint for convergence, but we do...
            }
            else if (state.Init == State.InitFlags.InitJct && state.Domain == State.DomainTypes.Time && state.UseDC && state.UseIC)
            {
                vbe = modeltemp.BJTtype * BJTicVBE;
                vce = modeltemp.BJTtype * BJTicVCE;
                vbc = vbe - vce;
                vbx = vbc;
                vcs = 0;
            }
            else if (state.Init == State.InitFlags.InitJct && !BJToff)
            {
                vbe = this.temp.BJTtVcrit;
                vbc = 0;
                vcs = 0;
                vbx = 0;
            }
            else if (state.Init == State.InitFlags.InitJct || (state.Init == State.InitFlags.InitFix && BJToff))
            {
                vbe = 0;
                vbc = 0;
                vcs = 0;
                vbx = 0;
            }
            else
            {
                /* 
                 * compute new nonlinear branch voltages
                 */
                vbe = modeltemp.BJTtype * (rstate.Solution[BJTbasePrimeNode] - rstate.Solution[BJTemitPrimeNode]);
                vbc = modeltemp.BJTtype * (rstate.Solution[BJTbasePrimeNode] - rstate.Solution[BJTcolPrimeNode]);
                vbx = modeltemp.BJTtype * (rstate.Solution[BJTbaseNode] - rstate.Solution[BJTcolPrimeNode]);
                vcs = modeltemp.BJTtype * (rstate.Solution[BJTsubstNode] - rstate.Solution[BJTcolPrimeNode]);

                /* 
				 * limit nonlinear branch voltages
				 */
                ichk1 = true;
                vbe = Semiconductor.DEVpnjlim(vbe, state.States[0][BJTstate + BJTvbe], vt, this.temp.BJTtVcrit, ref icheck);
                vbc = Semiconductor.DEVpnjlim(vbc, state.States[0][BJTstate + BJTvbc], vt, this.temp.BJTtVcrit, ref ichk1);
                if (ichk1 == true)
                    icheck = true;
            }

            /* 
			 * determine dc current and derivitives
			 */
            vtn = vt * modeltemp.BJTemissionCoeffF;
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
            vtn = vt * modeltemp.BJTemissionCoeffR;
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
            q1 = 1 / (1 - modeltemp.BJTinvEarlyVoltF * vbc - modeltemp.BJTinvEarlyVoltR * vbe);
            if (oik == 0 && oikr == 0)
            {
                qb = q1;
                dqbdve = q1 * qb * modeltemp.BJTinvEarlyVoltR;
                dqbdvc = q1 * qb * modeltemp.BJTinvEarlyVoltF;
            }
            else
            {
                q2 = oik * cbe + oikr * cbc;
                arg = Math.Max(0, 1 + 4 * q2);
                sqarg = 1;
                if (arg != 0)
                    sqarg = Math.Sqrt(arg);
                qb = q1 * (1 + sqarg) / 2;
                dqbdve = q1 * (qb * modeltemp.BJTinvEarlyVoltR + oik * gbe / sqarg);
                dqbdvc = q1 * (qb * modeltemp.BJTinvEarlyVoltF + oikr * gbc / sqarg);
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
                if (state.Init == State.InitFlags.InitTransient)
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
            cc = cc + (cex - cbc) / qb - cbc / this.temp.BJTtBetaR - cbcn;
            cb = cbe / this.temp.BJTtBetaF + cben + cbc / this.temp.BJTtBetaR + cbcn;
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
            gpi = gbe / this.temp.BJTtBetaF + gben;
            gmu = gbc / this.temp.BJTtBetaR + gbcn;
            go = (gbc + (cex - cbc) * dqbdvc / qb) / qb;
            gm = (gex - (cex - cbc) * dqbdve / qb) / qb - go;

            if (state.Domain == State.DomainTypes.Time || state.UseIC || state.UseSmallSignal)
            {
                /* 
				 * charge storage elements
				 */
                tf = modeltemp.BJTtransitTimeF;
                tr = modeltemp.BJTtransitTimeR;
                czbe = this.temp.BJTtBEcap * BJTarea;
                pe = this.temp.BJTtBEpot;
                xme = modeltemp.BJTjunctionExpBE;
                cdis = modeltemp.BJTbaseFractionBCcap;
                ctot = this.temp.BJTtBCcap * BJTarea;
                czbc = ctot * cdis;
                czbx = ctot - czbc;
                pc = this.temp.BJTtBCpot;
                xmc = modeltemp.BJTjunctionExpBC;
                fcpe = this.temp.BJTtDepCap;
                czcs = modeltemp.BJTcapCS * BJTarea;
                ps = modeltemp.BJTpotentialSubstrate;
                xms = modeltemp.BJTexponentialSubstrate;
                xtf = modeltemp.BJTtransitTimeBiasCoeffF;
                ovtf = modeltemp.BJTtransitTimeVBCFactor;
                xjtf = modeltemp.BJTtransitTimeHighCurrentF * BJTarea;
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
                    f1 = this.temp.BJTtf1;
                    f2 = modeltemp.BJTf2;
                    f3 = modeltemp.BJTf3;
                    czbef2 = czbe / f2;
                    state.States[0][BJTstate + BJTqbe] = tf * cbe + czbe * f1 + czbef2 * (f3 * (vbe - fcpe) + (xme / (pe + pe)) * (vbe * vbe -
                         fcpe * fcpe));
                    capbe = tf * gbe + czbef2 * (f3 + xme * vbe / pe);
                }
                fcpc = this.temp.BJTtf4;
                f1 = this.temp.BJTtf5;
                f2 = modeltemp.BJTf6;
                f3 = modeltemp.BJTf7;
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
                if (!(state.Domain == State.DomainTypes.Time && state.UseDC && state.UseIC))
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

                    if (state.Init == State.InitFlags.InitTransient)
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

                    if (state.Init == State.InitFlags.InitTransient)
                    {
                        state.States[1][BJTstate + BJTcqbe] = state.States[0][BJTstate + BJTcqbe];
                        state.States[1][BJTstate + BJTcqbc] = state.States[0][BJTstate + BJTcqbc];
                    }
                }
            }

            /* 
			 * check convergence
			 */
            if (state.Init != State.InitFlags.InitFix || !BJToff)
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
                if (state.Init == State.InitFlags.InitTransient)
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
            ceqcs = modeltemp.BJTtype * (state.States[0][BJTstate + BJTcqcs] - vcs * gccs);
            ceqbx = modeltemp.BJTtype * (state.States[0][BJTstate + BJTcqbx] - vbx * geqbx);
            ceqbe = modeltemp.BJTtype * (cc + cb - vbe * (gm + go + gpi) + vbc * (go - geqcb));
            ceqbc = modeltemp.BJTtype * (-cc + vbe * (gm + go) - vbc * (gmu + go));

            rstate.Rhs[BJTbaseNode] += (-ceqbx);
            rstate.Rhs[BJTcolPrimeNode] += (ceqcs + ceqbx + ceqbc);
            rstate.Rhs[BJTbasePrimeNode] += (-ceqbe - ceqbc);
            rstate.Rhs[BJTemitPrimeNode] += (ceqbe);
            rstate.Rhs[BJTsubstNode] += (-ceqcs);

            /* 
			 * load y matrix
			 */
            BJTcolColPtr.Add(gcpr);
            BJTbaseBasePtr.Add(gx + geqbx);
            BJTemitEmitPtr.Add(gepr);
            BJTcolPrimeColPrimePtr.Add(gmu + go + gcpr + gccs + geqbx);
            BJTbasePrimeBasePrimePtr.Add(gx + gpi + gmu + geqcb);
            BJTemitPrimeEmitPrimePtr.Add(gpi + gepr + gm + go);
            BJTcolColPrimePtr.Add(-gcpr);
            BJTbaseBasePrimePtr.Add(-gx);
            BJTemitEmitPrimePtr.Add(-gepr);
            BJTcolPrimeColPtr.Add(-gcpr);
            BJTcolPrimeBasePrimePtr.Add(-gmu + gm);
            BJTcolPrimeEmitPrimePtr.Add(-gm - go);
            BJTbasePrimeBasePtr.Add(-gx);
            BJTbasePrimeColPrimePtr.Add(-gmu - geqcb);
            BJTbasePrimeEmitPrimePtr.Add(-gpi);
            BJTemitPrimeEmitPtr.Add(-gepr);
            BJTemitPrimeColPrimePtr.Add(-go + geqcb);
            BJTemitPrimeBasePrimePtr.Add(-gpi - gm - geqcb);
            BJTsubstSubstPtr.Add(gccs);
            BJTcolPrimeSubstPtr.Add(-gccs);
            BJTsubstColPrimePtr.Add(-gccs);
            BJTbaseColPrimePtr.Add(-geqbx);
            BJTcolPrimeBasePtr.Add(-geqbx);
        }

        /// <summary>
        /// Check if the BJT is convergent
        /// </summary>
        /// <param name="ckt"></param>
        /// <returns></returns>
        public override bool IsConvergent(Circuit ckt)
        {
            var state = ckt.State;
            var config = ckt.Simulation.CurrentConfig;

            double vbe, vbc, delvbe, delvbc, cchat, cbhat, cc, cb;

            vbe = modeltemp.BJTtype * (state.Solution[BJTbasePrimeNode] - state.Solution[BJTemitPrimeNode]);
            vbc = modeltemp.BJTtype * (state.Solution[BJTbasePrimeNode] - state.Solution[BJTcolPrimeNode]);
            delvbe = vbe - state.States[0][BJTstate + BJTvbe];
            delvbc = vbc - state.States[0][BJTstate + BJTvbc];
            cchat = state.States[0][BJTstate + BJTcc] + (state.States[0][BJTstate + BJTgm] + state.States[0][BJTstate + BJTgo]) * delvbe -
                    (state.States[0][BJTstate + BJTgo] + state.States[0][BJTstate + BJTgmu]) * delvbc;
            cbhat = state.States[0][BJTstate + BJTcb] + state.States[0][BJTstate + BJTgpi] * delvbe + state.States[0][BJTstate + BJTgmu] * delvbc;
            cc = state.States[0][BJTstate + BJTcc];
            cb = state.States[0][BJTstate + BJTcb];

            /*
             *   check convergence
             */
            double tol = config.RelTol * Math.Max(Math.Abs(cchat), Math.Abs(cc)) + config.AbsTol;
            if (Math.Abs(cchat - cc) > tol)
            {
                state.IsCon = false;
                return false;
            }
            else
            {
                tol = config.RelTol * Math.Max(Math.Abs(cbhat), Math.Abs(cb)) + config.AbsTol;
                if (Math.Abs(cbhat - cb) > tol)
                {
                    state.IsCon = false;
                    return false;
                }
            }
            return true;
        }
    }
}
