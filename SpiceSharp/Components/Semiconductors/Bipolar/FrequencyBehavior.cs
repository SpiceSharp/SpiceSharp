using System;
using System.Numerics;
using SpiceSharp.Components.Bipolar;
using SpiceSharp.Sparse;
using SpiceSharp.Simulations;
using SpiceSharp.Attributes;

namespace SpiceSharp.Behaviors.Bipolar
{
    /// <summary>
    /// AC behavior for <see cref="Components.BJT"/>
    /// </summary>
    public class FrequencyBehavior : Behaviors.FrequencyBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        BaseParameters bp;
        LoadBehavior load;
        TemperatureBehavior temp;
        ModelBaseParameters mbp;
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
        
        /// <summary>
        /// Properties
        /// </summary>
        [PropertyName("cpi"), PropertyInfo("Internal base to emitter capactance")]
        public double BJTcapbe { get; protected set; }
        [PropertyName("cmu"), PropertyInfo("Internal base to collector capactiance")]
        public double BJTcapbc { get; protected set; }
        [PropertyName("cbx"), PropertyInfo("Base to collector capacitance")]
        public double BJTcapbx { get; protected set; }
        [PropertyName("ccs"), PropertyInfo("Collector to substrate capacitance")]
        public double BJTcapcs { get; protected set; }

        public double BJTgeqcb { get; protected set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public FrequencyBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Setup behavior
        /// </summary>
        /// <param name="provider">Data provider</param>
        public override void Setup(SetupDataProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            // Get parameters
            bp = provider.GetParameterSet<BaseParameters>(0);
            mbp = provider.GetParameterSet<ModelBaseParameters>(1);

            // Get behaviors
            temp = provider.GetBehavior<TemperatureBehavior>(0);
            load = provider.GetBehavior<LoadBehavior>(0);
            modeltemp = provider.GetBehavior<ModelTemperatureBehavior>(1);
        }

        /// <summary>
        /// Connect
        /// </summary>
        /// <param name="pins">Pins</param>
        public void Connect(params int[] pins)
        {
            if (pins == null)
                throw new ArgumentNullException(nameof(pins));
            if (pins.Length != 4)
                throw new Diagnostics.CircuitException($"Pin count mismatch: 4 pins expected, {pins.Length} given");
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
			if (matrix == null)
				throw new ArgumentNullException(nameof(matrix));

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
        /// Initialize AC parameters
        /// </summary>
        /// <param name="sim">Frequency-based simulation</param>
        public override void InitializeParameters(FrequencySimulation sim)
        {
			if (sim == null)
				throw new ArgumentNullException(nameof(sim));

            double tf, tr, czbe, pe, xme, cdis, ctot, czbc, czbx, pc, xmc, fcpe, czcs, ps, xms, xtf, ovtf, xjtf;
            double arg, sarg, argtf, arg2, arg3, tmp, f1, f2, f3, czbef2, fcpc, czbcf2, czbxf2;
            double geqcb;

            // Get voltages
            var state = sim.State;
            double vbe = load.BJTvbe;
            double vbc = load.BJTvbc;
            double vbx = vbx = mbp.BJTtype * (state.Solution[BJTbaseNode] - state.Solution[BJTcolPrimeNode]);
            double vcs = mbp.BJTtype * (state.Solution[BJTsubstNode] - state.Solution[BJTcolPrimeNode]);

            // Get shared parameters
            double cbe = load.cbe;
            double gbe = load.gbe;
            double cbc = load.cbc;
            double gbc = load.gbc;
            double qb = load.qb;
            double dqbdvc = load.dqbdvc;
            double dqbdve = load.dqbdve;

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
                BJTcapbe = tf * gbe + czbe * sarg;
            }
            else
            {
                f1 = temp.BJTtf1;
                f2 = modeltemp.BJTf2;
                f3 = modeltemp.BJTf3;
                czbef2 = czbe / f2;
                BJTcapbe = tf * gbe + czbef2 * (f3 + xme * vbe / pe);
            }

            fcpc = temp.BJTtf4;
            f1 = temp.BJTtf5;
            f2 = modeltemp.BJTf6;
            f3 = modeltemp.BJTf7;
            if (vbc < fcpc)
            {
                arg = 1 - vbc / pc;
                sarg = Math.Exp(-xmc * Math.Log(arg));
                BJTcapbc = tr * gbc + czbc * sarg;
            }
            else
            {
                czbcf2 = czbc / f2;
                BJTcapbc = tr * gbc + czbcf2 * (f3 + xmc * vbc / pc);
            }
            if (vbx < fcpc)
            {
                arg = 1 - vbx / pc;
                sarg = Math.Exp(-xmc * Math.Log(arg));
                BJTcapbx = czbx * sarg;
            }
            else
            {
                czbxf2 = czbx / f2;
                BJTcapbx = czbxf2 * (f3 + xmc * vbx / pc);
            }
            if (vcs < 0)
            {
                arg = 1 - vcs / ps;
                sarg = Math.Exp(-xms * Math.Log(arg));
                BJTcapcs = czcs * sarg;
            }
            else
            {
                BJTcapcs = czcs * (1 + xms * vcs / ps);
            }
        }

        /// <summary>
        /// Execute behavior for AC analysis
        /// </summary>
        /// <param name="sim">Frequency-based simulation</param>
        public override void Load(FrequencySimulation sim)
        {
			if (sim == null)
				throw new ArgumentNullException(nameof(sim));

            var state = sim.State;
            var cstate = state;
            double gcpr, gepr, gpi, gmu, go, td, gx;
            Complex gm, xcpi, xcmu, xcbx, xccs, xcmcb;

            gcpr = modeltemp.BJTcollectorConduct * bp.BJTarea;
            gepr = modeltemp.BJTemitterConduct * bp.BJTarea;
            gpi = load.BJTgpi;
            gmu = load.BJTgmu;
            gm = load.BJTgm;
            go = load.BJTgo;
            td = modeltemp.BJTexcessPhaseFactor;
            if (td != 0)
            {
                Complex arg = td * cstate.Laplace;

                gm = gm + go;
                gm = gm * Complex.Exp(-arg);
                gm = gm - go;
            }
            gx = load.BJTgx;
            xcpi = BJTcapbe * cstate.Laplace;
            xcmu = BJTcapbc * cstate.Laplace;
            xcbx = BJTcapbx * cstate.Laplace;
            xccs = BJTcapcs * cstate.Laplace;
            xcmcb = BJTgeqcb * cstate.Laplace;

            BJTcolColPtr.Add(gcpr);
            BJTbaseBasePtr.Add(gx + xcbx);
            BJTemitEmitPtr.Add(gepr);
            BJTcolPrimeColPrimePtr.Add(gmu + go + gcpr + xcmu + xccs + xcbx);
            BJTbasePrimeBasePrimePtr.Add(gx + gpi + gmu + xcpi + xcmu + xcmcb);
            BJTemitPrimeEmitPrimePtr.Add(gpi + gepr + gm + go + xcpi);
            BJTcolColPrimePtr.Add(-gcpr);
            BJTbaseBasePrimePtr.Add(-gx);
            BJTemitEmitPrimePtr.Add(-gepr);
            BJTcolPrimeColPtr.Add(-gcpr);
            BJTcolPrimeBasePrimePtr.Add(-gmu + gm - xcmu);
            BJTcolPrimeEmitPrimePtr.Add(-gm - go);
            BJTbasePrimeBasePtr.Add(-gx);
            BJTbasePrimeColPrimePtr.Add(-gmu - xcmu - xcmcb);
            BJTbasePrimeEmitPrimePtr.Add(-gpi - xcpi);
            BJTemitPrimeEmitPtr.Add(-gepr);
            BJTemitPrimeColPrimePtr.Add(-go + xcmcb);
            BJTemitPrimeBasePrimePtr.Add(-gpi - gm - xcpi - xcmcb);
            BJTsubstSubstPtr.Add(xccs);
            BJTcolPrimeSubstPtr.Add(-xccs);
            BJTsubstColPrimePtr.Add(-xccs);
            BJTbaseColPrimePtr.Add(-xcbx);
            BJTcolPrimeBasePtr.Add(-xcbx);
        }
    }
}
