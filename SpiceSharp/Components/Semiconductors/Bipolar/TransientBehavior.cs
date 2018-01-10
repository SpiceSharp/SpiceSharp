using System;
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
    public class TransientBehavior : Behaviors.TransientBehavior
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

        /// <summary>
        /// States
        /// </summary>
        protected StateVariable BJTcexbc { get; private set; }

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
        /// Create states
        /// </summary>
        /// <param name="states">Pool of all states</param>
        public override void CreateStates(StatePool states)
        {
            // We just need a history without integration here
            BJTcexbc = states.Create(0);
        }

        /// <summary>
        /// Calculate state variables
        /// </summary>
        /// <param name="sim">Time-based simulation</param>
        public override void GetDCstate(TimeSimulation sim)
        {
            // BJTcexbc.Value = load.cbe / load.qb;

            // Register for excess phase calculations
            load.ExcessPhaseCalculation += CalculateExcessPhase;
        }

        /// <summary>
        /// Transient behavior
        /// </summary>
        /// <param name="sim">Time-based simulation</param>
        public override void Transient(TimeSimulation sim)
        {
            double tf, tr, czbe, pe, xme, cdis, ctot, czbc, czbx, pc, xmc, fcpe, czcs, ps,
                xms, xtf, ovtf, xjtf, argtf, temp, sarg, capbe, f1, f2, f3, czbef2, fcpc, capbc, czbcf2, czbxf2;
            double arg, arg1, arg2, arg3, denom;
            double trancc, trancbe, trangbe;

            double vbe = load.BJTvbe;
            double vbc = load.BJTvbc;
            double td = modeltemp.BJTexcessPhaseFactor;

            /* 
             * weil's approx. for excess phase applied with backward - 
             * euler integration
             */
            trancc = 0;
            trancbe = 0;
            trangbe = 0;
            if (td != 0)
            {
                arg1 = BJTcexbc.GetTimestep() / td;
                arg2 = 3 * arg1;
                arg1 = arg2 * arg1;
                denom = 1 + arg1 + arg2;
                arg3 = arg1 / denom - 1.0;
                trancc = (BJTcexbc.GetPreviousValue() * (1 + BJTcexbc.GetTimestep() / BJTcexbc.GetTimestep(1) + arg2)
                    - BJTcexbc.GetPreviousValue(2) * BJTcexbc.GetTimestep() / BJTcexbc.GetTimestep(1)) / denom;
                trancbe = load.cbe * arg3;
                trangbe = load.gbe * arg3;
                BJTcexbc.Value = trancc + trancbe / load.qb;
            }

            /* 
             * charge storage elements
             */
            tf = mbp.BJTtransitTimeF;
            tr = mbp.BJTtransitTimeR;
            czbe = this.temp.BJTtBEcap * bp.BJTarea;
            pe = this.temp.BJTtBEpot;
            xme = mbp.BJTjunctionExpBE;
            cdis = mbp.BJTbaseFractionBCcap;
            ctot = this.temp.BJTtBCcap * bp.BJTarea;
            czbc = ctot * cdis;
            czbx = ctot - czbc;
            pc = this.temp.BJTtBCpot;
            xmc = mbp.BJTjunctionExpBC;
            fcpe = this.temp.BJTtDepCap;
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
                        temp = load.cbe / (load.cbe + xjtf);
                        argtf = argtf * temp * temp;
                        arg2 = argtf * (3 - temp - temp);
                    }
                    arg3 = load.cbe * argtf * ovtf;
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
