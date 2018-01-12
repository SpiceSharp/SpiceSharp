using System;
using SpiceSharp.Circuits;
using SpiceSharp.Attributes;
using SpiceSharp.Sparse;
using SpiceSharp.IntegrationMethods;
using SpiceSharp.Simulations;
using SpiceSharp.Components.Bipolar;

namespace SpiceSharp.Behaviors.Bipolar
{
    /// <summary>
    /// Transient behavior for a <see cref="Components.BJT"/>
    /// </summary>
    public class TransientBehavior : Behaviors.TransientBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary behaviors and parameters
        /// </summary>
        BaseParameters bp;
        ModelBaseParameters mbp;
        TemperatureBehavior temp;
        LoadBehavior load;
        ModelTemperatureBehavior modeltemp;

        /// <summary>
        /// Nodes
        /// </summary>
        int BJTcolNode, BJTbaseNode, BJTemitNode, BJTsubstNode, BJTcolPrimeNode, BJTbasePrimeNode, BJTemitPrimeNode;
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

        /*
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
        [SpiceName("cexbc"), SpiceInfo("Total Capacitance in B-X junction")]
        public double GetCEXBC(Circuit ckt) => ckt.State.States[0][BJTstate + BJTcexbc];
        */

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
        /// States
        /// </summary>
        protected StateVariable BJTqbe { get; private set; }
        protected StateVariable BJTqbc { get; private set; }
        protected StateVariable BJTqcs { get; private set; }
        protected StateVariable BJTqbx { get; private set; }
        protected StateVariable BJTcexbc { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public TransientBehavior(Identifier name) : base(name) { }

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
            load = provider.GetBehavior<LoadBehavior>();
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
        /// <param name="matrix">Matrix</param>
        public override void GetMatrixPointers(Matrix matrix)
        {
            // Get extra equations
            BJTcolPrimeNode = load.BJTcolPrimeNode;
            BJTbasePrimeNode = load.BJTbasePrimeNode;
            BJTemitPrimeNode = load.BJTemitPrimeNode;

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
        /// Create states
        /// </summary>
        /// <param name="states">Pool of all states</param>
        public override void CreateStates(StatePool states)
        {
            // We just need a history without integration here
            BJTqbe = states.Create();
            BJTqbc = states.Create();
            BJTqcs = states.Create();
            BJTqbx = states.Create();
            BJTcexbc = states.Create(0);
        }

        /// <summary>
        /// Calculate state variables
        /// </summary>
        /// <param name="sim">Time-based simulation</param>
        public override void GetDCstate(TimeSimulation sim)
        {
            BJTcexbc.Value = load.cbe / load.qb;

            // Register for excess phase calculations
            load.ExcessPhaseCalculation += CalculateExcessPhase;
        }

        /// <summary>
        /// Transient behavior
        /// </summary>
        /// <param name="sim">Time-based simulation</param>
        public override void Transient(TimeSimulation sim)
        {
            var state = sim.Circuit.State;
            double tf, tr, czbe, pe, xme, cdis, ctot, czbc, czbx, pc, xmc, fcpe, czcs, ps, arg, arg2, arg3, capbx, capcs,
                xms, xtf, ovtf, xjtf, argtf, tmp, sarg, capbe, f1, f2, f3, czbef2, fcpc, capbc, czbcf2, czbxf2;

            double cbe = load.cbe;
            double cbc = load.cbc;
            double gbe = load.gbe;
            double gbc = load.gbc;
            double qb = load.qb;
            double dqbdvc = load.dqbdvc;
            double dqbdve = load.dqbdve;
            double geqcb = 0;

            double vbe = load.BJTvbe;
            double vbc = load.BJTvbc;
            double vbx = vbx = mbp.BJTtype * (state.Solution[BJTbaseNode] - state.Solution[BJTcolPrimeNode]);
            double vcs = mbp.BJTtype * (state.Solution[BJTsubstNode] - state.Solution[BJTcolPrimeNode]);
            double td = modeltemp.BJTexcessPhaseFactor;

            /* 
             * charge storage elements
             */
            tf = mbp.BJTtransitTimeF;
            tr = mbp.BJTtransitTimeR;
            czbe = temp.BJTtBEcap * bp.BJTarea;
            pe = temp.BJTtBEpot;
            xme = mbp.BJTjunctionExpBE;
            cdis = mbp.BJTbaseFractionBCcap;
            ctot = temp.BJTtBCcap * bp.BJTarea;
            czbc = ctot * cdis;
            czbx = ctot - czbc;
            pc = temp.BJTtBCpot;
            xmc = mbp.BJTjunctionExpBC;
            fcpe = temp.BJTtDepCap;
            czcs = mbp.BJTcapCS * bp.BJTarea;
            ps = mbp.BJTpotentialSubstrate;
            xms = mbp.BJTexponentialSubstrate;
            xtf = mbp.BJTtransitTimeBiasCoeffF;
            ovtf = modeltemp.BJTtransitTimeVBCFactor;
            xjtf = mbp.BJTtransitTimeHighCurrentF * bp.BJTarea;
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
                        tmp = cbe / (cbe + xjtf);
                        argtf = argtf * tmp * tmp;
                        arg2 = argtf * (3 - tmp - tmp);
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
                BJTqbe.Value = tf * cbe + pe * czbe * (1 - arg * sarg) / (1 - xme);
                capbe = tf * gbe + czbe * sarg;
            }
            else
            {
                f1 = temp.BJTtf1;
                f2 = modeltemp.BJTf2;
                f3 = modeltemp.BJTf3;
                czbef2 = czbe / f2;
                BJTqbe.Value = tf * cbe + czbe * f1 + czbef2 * (f3 * (vbe - fcpe) + (xme / (pe + pe)) * (vbe * vbe -
                     fcpe * fcpe));
                capbe = tf * gbe + czbef2 * (f3 + xme * vbe / pe);
            }
            fcpc = temp.BJTtf4;
            f1 = temp.BJTtf5;
            f2 = modeltemp.BJTf6;
            f3 = modeltemp.BJTf7;
            if (vbc < fcpc)
            {
                arg = 1 - vbc / pc;
                sarg = Math.Exp(-xmc * Math.Log(arg));
                BJTqbc.Value = tr * cbc + pc * czbc * (1 - arg * sarg) / (1 - xmc);
                capbc = tr * gbc + czbc * sarg;
            }
            else
            {
                czbcf2 = czbc / f2;
                BJTqbc.Value = tr * cbc + czbc * f1 + czbcf2 * (f3 * (vbc - fcpc) + (xmc / (pc + pc)) * (vbc * vbc -
                     fcpc * fcpc));
                capbc = tr * gbc + czbcf2 * (f3 + xmc * vbc / pc);
            }
            if (vbx < fcpc)
            {
                arg = 1 - vbx / pc;
                sarg = Math.Exp(-xmc * Math.Log(arg));
                BJTqbx.Value = pc * czbx * (1 - arg * sarg) / (1 - xmc);
                capbx = czbx * sarg;
            }
            else
            {
                czbxf2 = czbx / f2;
                BJTqbx.Value = czbx * f1 + czbxf2 * (f3 * (vbx - fcpc) + (xmc / (pc + pc)) * (vbx * vbx - fcpc * fcpc));
                capbx = czbxf2 * (f3 + xmc * vbx / pc);
            }
            if (vcs < 0)
            {
                arg = 1 - vcs / ps;
                sarg = Math.Exp(-xms * Math.Log(arg));
                BJTqcs.Value = ps * czcs * (1 - arg * sarg) / (1 - xms);
                capcs = czcs * sarg;
            }
            else
            {
                BJTqcs.Value = vcs * czcs * (1 + xms * vcs / (2 * ps));
                capcs = czcs * (1 + xms * vcs / ps);
            }

            BJTcapbe = capbe;
            BJTcapbc = capbc;
            BJTcapcs = capcs;
            BJTcapbx = capbx;

            var eqbe = BJTqbe.Integrate(capbe, vbe);
            geqcb *= eqbe.Geq / capbe; // Note: ugly fix to multiply with method.Slope :-(
            var eqbc = BJTqbc.Integrate(capbc, vbc);

            /* 
             * charge storage for c - s and b - x junctions
             */
            var eqcs = BJTqcs.Integrate(capcs, vcs);
            var eqbx = BJTqbx.Integrate(capbx, vbx);

            /* 
			 * load current excitation vector
			 */
            double ceqcs = mbp.BJTtype * eqbe.Ceq;
            double ceqbx = mbp.BJTtype * eqbx.Ceq;
            double ceqbe = mbp.BJTtype * (eqbe.Ceq + vbc * geqcb);
            double ceqbc = mbp.BJTtype * eqbc.Ceq;

            state.Rhs[BJTbaseNode] -= (-ceqbx);
            state.Rhs[BJTcolPrimeNode] += (ceqcs + ceqbx + ceqbc);
            state.Rhs[BJTbasePrimeNode] += (-ceqbe - ceqbc);
            state.Rhs[BJTemitPrimeNode] += (ceqbe);
            state.Rhs[BJTsubstNode] += (-ceqcs);

            /* 
			 * load y matrix
			 */
            BJTbaseBasePtr.Add(eqbx.Geq);
            BJTcolPrimeColPrimePtr.Add(eqbc.Geq + eqcs.Geq + eqbx.Geq);
            BJTbasePrimeBasePrimePtr.Add(eqbe.Geq + eqbc.Geq + geqcb);
            BJTemitPrimeEmitPrimePtr.Add(eqbe.Geq);
            BJTcolPrimeBasePrimePtr.Add(-eqbc.Geq);
            BJTbasePrimeColPrimePtr.Add(-eqbc.Geq - geqcb);
            BJTbasePrimeEmitPrimePtr.Add(-eqbe.Geq);
            BJTemitPrimeColPrimePtr.Add(geqcb);
            BJTemitPrimeBasePrimePtr.Add(-eqbe.Geq - geqcb);
            BJTsubstSubstPtr.Add(eqcs.Geq);
            BJTcolPrimeSubstPtr.Add(-eqcs.Geq);
            BJTsubstColPrimePtr.Add(-eqcs.Geq);
            BJTbaseColPrimePtr.Add(-eqbx.Geq);
            BJTcolPrimeBasePtr.Add(-eqbx.Geq);
        }

        /// <summary>
        /// Truncate the timestep
        /// </summary>
        /// <param name="timestep">Current timestep</param>
        public override void Truncate(ref double timestep)
        {
            BJTqbe.LocalTruncationError(ref timestep);
            BJTqbc.LocalTruncationError(ref timestep);
            BJTqcs.LocalTruncationError(ref timestep);
        }

        /// <summary>
        /// Calculate excess phase
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="args">Arguments</param>
        public void CalculateExcessPhase(object sender, BJTExcessPhaseEventArgs args)
        {
            double arg1, arg2, denom, arg3;
            double td = modeltemp.BJTexcessPhaseFactor;

            /* 
             * weil's approx. for excess phase applied with backward - 
             * euler integration
             */
            double cbe = args.cex;
            double gbe = args.gex;

            double delta = BJTcexbc.GetTimestep();
            double prevdelta = BJTcexbc.GetTimestep(1);
            arg1 = delta / td;
            arg2 = 3 * arg1;
            arg1 = arg2 * arg1;
            denom = 1 + arg1 + arg2;
            arg3 = arg1 / denom;
            /* Still need a place for this...
            if (state.Init == State.InitFlags.InitTransient)
            {
                state.States[1][BJTstate + BJTcexbc] = cbe / qb;
                state.States[2][BJTstate + BJTcexbc] = state.States[1][BJTstate + BJTcexbc];
            } */
            args.cc = (BJTcexbc.GetPreviousValue(1) * (1 + delta / prevdelta + arg2) 
                - BJTcexbc.GetPreviousValue(2) * delta / prevdelta) / denom;
            args.cex = cbe * arg3;
            args.gex = gbe * arg3;
            BJTcexbc.Value = args.cc + args.cex / args.qb;
        }
    }
}
