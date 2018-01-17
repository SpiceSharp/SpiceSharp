using System;
using SpiceSharp.Circuits;
using SpiceSharp.Components.Semiconductors;
using SpiceSharp.Attributes;
using SpiceSharp.Sparse;
using SpiceSharp.Diagnostics;
using SpiceSharp.Components.Bipolar;

namespace SpiceSharp.Behaviors.Bipolar
{
    /// <summary>
    /// General behavior for <see cref="Bipolar"/>
    /// </summary>
    public class LoadBehavior : Behaviors.LoadBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        BaseParameters bp;
        ModelBaseParameters mbp;
        TemperatureBehavior temp;
        ModelTemperatureBehavior modeltemp;
        
        /// <summary>
        /// Parameters
        /// </summary>
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
        [SpiceName("vbe"), SpiceInfo("B-E voltage")]
        public double BJTvbe { get; protected set; }
        [SpiceName("vbc"), SpiceInfo("B-C voltage")]
        public double BJTvbc { get; protected set; }
        [SpiceName("cc"), SpiceInfo("Current at collector node")]
        public double BJTcc { get; protected set; }
        [SpiceName("cb"), SpiceInfo("Current at base node")]
        public double BJTcb { get; protected set; }
        [SpiceName("gpi"), SpiceInfo("Small signal input conductance - pi")]
        public double BJTgpi { get; protected set; }
        [SpiceName("gmu"), SpiceInfo("Small signal conductance - mu")]
        public double BJTgmu { get; protected set; }
        [SpiceName("gm"), SpiceInfo("Small signal transconductance")]
        public double BJTgm { get; protected set; }
        [SpiceName("go"), SpiceInfo("Small signal output conductance")]
        public double BJTgo { get; protected set; }

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
        public double BJTgx { get; protected set; }
        [SpiceName("cexbc"), SpiceInfo("Total Capacitance in B-X junction")]
        public double GetCEXBC(Circuit ckt) => ckt.State.States[0][BJTstate + BJTcexbc];
        [SpiceName("geqcb"), SpiceInfo("d(Ibe)/d(Vbc)")]
        public double BJTgeqcb { get; protected set; }
        [SpiceName("gccs"), SpiceInfo("Internal C-S cap. equiv. cond.")]
        public double BJTgccs { get; protected set; }
        [SpiceName("geqbx"), SpiceInfo("Internal C-B-base cap. equiv. cond.")]
        public double BJTgeqbx { get; protected set; }
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
            double value = -BJTcc - BJTcb;
            if (ckt.Method != null && !(ckt.State.Domain == State.DomainTypes.Time && ckt.State.UseDC))
                value += ckt.State.States[0][BJTstate + BJTcqcs];
            return value;
        }
        [SpiceName("p"), SpiceInfo("Power dissipation")]
        public double GetPOWER(Circuit ckt)
        {
            double value = BJTcc * ckt.State.Solution[BJTcolNode];
            value += BJTcb * ckt.State.Solution[BJTbaseNode];
            if (ckt.Method != null && !(ckt.State.Domain == State.DomainTypes.Time && ckt.State.UseDC))
                value -= ckt.State.States[0][BJTstate + BJTcqcs] * ckt.State.Solution[BJTsubstNode];

            double tmp = -BJTcc - BJTcb;
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
        int BJTcolNode, BJTbaseNode, BJTemitNode, BJTsubstNode;
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
        /// Shared parameters
        /// </summary>
        public double cbe { get; protected set; }
        public double gbe { get; protected set; }
        public double cbc { get; protected set; }
        public double gbc { get; protected set; }
        public double qb { get; protected set; }
        public double dqbdvc { get; protected set; }
        public double dqbdve { get; protected set; }

        /// <summary>
        /// Constants
        /// </summary>
        public const int BJTqbe = 8;
        public const int BJTcqbe = 9;
        public const int BJTqbc = 10;
        public const int BJTcqbc = 11;
        public const int BJTqcs = 12;
        public const int BJTcqcs = 13;
        public const int BJTqbx = 14;
        public const int BJTcqbx = 15;
        public const int BJTcexbc = 17;

        /// <summary>
        /// Event called when excess phase calculation is needed
        /// </summary>
        public event ExcessPhaseEventHandler ExcessPhaseCalculation;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public LoadBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Setup behavior
        /// </summary>
        /// <param name="provider">Data provider</param>
        public override void Setup(SetupDataProvider provider)
        {
            // Get parameters
            bp = provider.GetParameters<BaseParameters>();
            mbp = provider.GetParameters<ModelBaseParameters>(1);

            // Get behaviors
            temp = provider.GetBehavior<TemperatureBehavior>();
            modeltemp = provider.GetBehavior<ModelTemperatureBehavior>(1);
        }

        /// <summary>
        /// Connect
        /// </summary>
        /// <param name="pins">Pins</param>
        public void Connect(params int[] pins)
        {
            BJTcolNode = pins[0];
            BJTbaseNode = pins[1];
            BJTemitNode = pins[2];
            BJTsubstNode = pins[3];
        }

        /// <summary>
        /// Get matrix pointers
        /// </summary>
        /// <param name="nodes">Nodes</param>
        /// <param name="matrix">Matrix</param>
        public override void GetMatrixPointers(Nodes nodes, Matrix matrix)
        {
            // Add a series collector node if necessary
            if (mbp.BJTcollectorResist.Value == 0)
                BJTcolPrimeNode = BJTcolNode;
            else if (BJTcolPrimeNode == 0)
                BJTcolPrimeNode = nodes.Create(Name.Grow("#col")).Index;

            // Add a series base node if necessary
            if (mbp.BJTbaseResist.Value == 0)
                BJTbasePrimeNode = BJTbaseNode;
            else if (BJTbasePrimeNode == 0)
                BJTbasePrimeNode = nodes.Create(Name.Grow("#base")).Index;

            // Add a series emitter node if necessary
            if (mbp.BJTemitterResist.Value == 0)
                BJTemitPrimeNode = BJTemitNode;
            else if (BJTemitPrimeNode == 0)
                BJTemitPrimeNode = nodes.Create(Name.Grow("#emit")).Index;

            // Get matrix pointers
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
            double vtn, evbe, gben, evben, cben, evbc, gbcn, evbcn, cbcn, q1, q2, arg, sqarg, cc, cex,
                gex, arg1, arg2, arg3, cb, gx, gpi, gmu, go, gm;
            double ceqbe, ceqbc, ceq;

            vt = temp.BJTtemp * Circuit.CONSTKoverQ;

            gccs = 0;
            ceqcs = 0;
            geqbx = 0;
            ceqbx = 0;
            geqcb = 0;

            /* 
			 * dc model paramters
			 */
            csat = temp.BJTtSatCur * bp.BJTarea;
            rbpr = mbp.BJTminBaseResist / bp.BJTarea;
            rbpi = mbp.BJTbaseResist / bp.BJTarea - rbpr;
            gcpr = modeltemp.BJTcollectorConduct * bp.BJTarea;
            gepr = modeltemp.BJTemitterConduct * bp.BJTarea;
            oik = modeltemp.BJTinvRollOffF / bp.BJTarea;
            c2 = temp.BJTtBEleakCur * bp.BJTarea;
            vte = mbp.BJTleakBEemissionCoeff * vt;
            oikr = modeltemp.BJTinvRollOffR / bp.BJTarea;
            c4 = temp.BJTtBCleakCur * bp.BJTarea;
            vtc = mbp.BJTleakBCemissionCoeff * vt;
            xjrb = mbp.BJTbaseCurrentHalfResist * bp.BJTarea;

            /* 
			* initialization
			*/
            icheck = false;
            if (state.Init == State.InitFlags.InitJct && state.Domain == State.DomainTypes.Time && state.UseDC && state.UseIC)
            {
                vbe = mbp.BJTtype * bp.BJTicVBE;
                vce = mbp.BJTtype * bp.BJTicVCE;
                vbc = vbe - vce;
                vbx = vbc;
                vcs = 0;
            }
            else if (state.Init == State.InitFlags.InitJct && !bp.BJToff)
            {
                vbe = temp.BJTtVcrit;
                vbc = 0;
                vcs = 0;
                vbx = 0;
            }
            else if (state.Init == State.InitFlags.InitJct || (state.Init == State.InitFlags.InitFix && bp.BJToff))
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
                vbe = mbp.BJTtype * (rstate.Solution[BJTbasePrimeNode] - rstate.Solution[BJTemitPrimeNode]);
                vbc = mbp.BJTtype * (rstate.Solution[BJTbasePrimeNode] - rstate.Solution[BJTcolPrimeNode]);
                vbx = mbp.BJTtype * (rstate.Solution[BJTbaseNode] - rstate.Solution[BJTcolPrimeNode]);
                vcs = mbp.BJTtype * (rstate.Solution[BJTsubstNode] - rstate.Solution[BJTcolPrimeNode]);

                /* 
				 * limit nonlinear branch voltages
				 */
                ichk1 = true;
                vbe = Semiconductor.DEVpnjlim(vbe, BJTvbe, vt, temp.BJTtVcrit, ref icheck);
                vbc = Semiconductor.DEVpnjlim(vbc, BJTvbc, vt, temp.BJTtVcrit, ref ichk1);
                if (ichk1 == true)
                    icheck = true;
            }

            /* 
			 * determine dc current and derivitives
			 */
            vtn = vt * mbp.BJTemissionCoeffF;
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

            vtn = vt * mbp.BJTemissionCoeffR;
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

            // Excess phase calculation
            BJTExcessPhaseEventArgs ep = new BJTExcessPhaseEventArgs()
            {
                cc = 0.0,
                cex = cbe,
                gex = gbe,
                qb = qb
            };
            ExcessPhaseCalculation?.Invoke(this, ep);
            cc = ep.cc;
            cex = ep.cex;
            gex = ep.gex;

            /* 
			 * determine dc incremental conductances
			 */
            cc = cc + (cex - cbc) / qb - cbc / temp.BJTtBetaR - cbcn;
            cb = cbe / temp.BJTtBetaF + cben + cbc / temp.BJTtBetaR + cbcn;
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
            gpi = gbe / temp.BJTtBetaF + gben;
            gmu = gbc / temp.BJTtBetaR + gbcn;
            go = (gbc + (cex - cbc) * dqbdvc / qb) / qb;
            gm = (gex - (cex - cbc) * dqbdve / qb) / qb - go;

            /* 
			 * check convergence
			 */
            if (state.Init != State.InitFlags.InitFix || !bp.BJToff)
            {
                if (icheck)
                    state.IsCon = false;
            }

            BJTvbe = vbe;
            BJTvbc = vbc;
            BJTcc = cc;
            BJTcb = cb;
            BJTgpi = gpi;
            BJTgmu = gmu;
            BJTgm = gm;
            BJTgo = go;
            BJTgx = gx;
            BJTgeqcb = geqcb;
            BJTgccs = gccs;
            BJTgeqbx = geqbx;

            /* 
			 * load current excitation vector
			 */
            // ceqcs = mbp.BJTtype * (state.States[0][BJTstate + BJTcqcs] - vcs * gccs);
            ceqcs = 0.0;
            // ceqbx = mbp.BJTtype * (state.States[0][BJTstate + BJTcqbx] - vbx * geqbx);
            ceqbx = 0.0;
            ceqbe = mbp.BJTtype * (cc + cb - vbe * (gm + go + gpi) + vbc * (go - geqcb));
            ceqbc = mbp.BJTtype * (-cc + vbe * (gm + go) - vbc * (gmu + go));

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

            double vbe, vbc, delvbe, delvbc, cchat, cbhat, cc, cb;

            vbe = mbp.BJTtype * (state.Solution[BJTbasePrimeNode] - state.Solution[BJTemitPrimeNode]);
            vbc = mbp.BJTtype * (state.Solution[BJTbasePrimeNode] - state.Solution[BJTcolPrimeNode]);
            delvbe = vbe - BJTvbe;
            delvbc = vbc - BJTvbe;
            cchat = BJTcc + (BJTgm + BJTgo) * delvbe - (BJTgo + BJTgmu) * delvbc;
            cbhat = BJTcb + BJTgpi * delvbe + BJTgmu * delvbc;
            cc = BJTcc;
            cb = BJTcb;

            /*
             *   check convergence
             */
            // NOTE: access configuration in some way here!
            double tol = 1e-3 * Math.Max(Math.Abs(cchat), Math.Abs(cc)) + 1e-12;
            if (Math.Abs(cchat - cc) > tol)
            {
                state.IsCon = false;
                return false;
            }

            tol = 1e-3 * Math.Max(Math.Abs(cbhat), Math.Abs(cb)) + 1e-12;
            if (Math.Abs(cbhat - cb) > tol)
            {
                state.IsCon = false;
                return false;
            }
            return true;
        }
    }

    /// <summary>
    /// Delegate for excess phase calculation
    /// </summary>
    /// <param name="sender">Sender</param>
    /// <param name="args">Arguments</param>
    public delegate void ExcessPhaseEventHandler(object sender, BJTExcessPhaseEventArgs args);
}
