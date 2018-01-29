using System;
using System.Numerics;
using SpiceSharp.Behaviors;
using SpiceSharp.Sparse;
using SpiceSharp.Simulations;
using SpiceSharp.Attributes;

namespace SpiceSharp.Components.BipolarBehaviors
{
    /// <summary>
    /// AC behavior for <see cref="BipolarJunctionTransistor"/>
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
        int colNode, baseNode, emitNode, substNode, colPrimeNode, basePrimeNode, emitPrimeNode;
        protected MatrixElement ColColPrimePtr { get; private set; }
        protected MatrixElement BaseBasePrimePtr { get; private set; }
        protected MatrixElement EmitEmitPrimePtr { get; private set; }
        protected MatrixElement ColPrimeColPtr { get; private set; }
        protected MatrixElement ColPrimeBasePrimePtr { get; private set; }
        protected MatrixElement ColPrimeEmitPrimePtr { get; private set; }
        protected MatrixElement BasePrimeBasePtr { get; private set; }
        protected MatrixElement BasePrimeColPrimePtr { get; private set; }
        protected MatrixElement BasePrimeEmitPrimePtr { get; private set; }
        protected MatrixElement EmitPrimeEmitPtr { get; private set; }
        protected MatrixElement EmitPrimeColPrimePtr { get; private set; }
        protected MatrixElement EmitPrimeBasePrimePtr { get; private set; }
        protected MatrixElement ColColPtr { get; private set; }
        protected MatrixElement BaseBasePtr { get; private set; }
        protected MatrixElement EmitEmitPtr { get; private set; }
        protected MatrixElement ColPrimeColPrimePtr { get; private set; }
        protected MatrixElement BasePrimeBasePrimePtr { get; private set; }
        protected MatrixElement EmitPrimeEmitPrimePtr { get; private set; }
        protected MatrixElement SubstSubstPtr { get; private set; }
        protected MatrixElement ColPrimeSubstPtr { get; private set; }
        protected MatrixElement SubstColPrimePtr { get; private set; }
        protected MatrixElement BaseColPrimePtr { get; private set; }
        protected MatrixElement ColPrimeBasePtr { get; private set; }
        
        /// <summary>
        /// Properties
        /// </summary>
        [PropertyName("cpi"), PropertyInfo("Internal base to emitter capactance")]
        public double Capbe { get; protected set; }
        [PropertyName("cmu"), PropertyInfo("Internal base to collector capactiance")]
        public double Capbc { get; protected set; }
        [PropertyName("cbx"), PropertyInfo("Base to collector capacitance")]
        public double Capbx { get; protected set; }
        [PropertyName("ccs"), PropertyInfo("Collector to substrate capacitance")]
        public double Capcs { get; protected set; }

        public double Geqcb { get; protected set; }

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
                throw new Diagnostics.CircuitException("Pin count mismatch: 4 pins expected, {0} given".FormatString(pins.Length));
            colNode = pins[0];
            baseNode = pins[1];
            emitNode = pins[2];
            substNode = pins[3];
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
            colPrimeNode = load.ColPrimeNode;
            basePrimeNode = load.BasePrimeNode;
            emitPrimeNode = load.EmitPrimeNode;

            // Get matrix pointers
            ColColPrimePtr = matrix.GetElement(colNode, colPrimeNode);
            BaseBasePrimePtr = matrix.GetElement(baseNode, basePrimeNode);
            EmitEmitPrimePtr = matrix.GetElement(emitNode, emitPrimeNode);
            ColPrimeColPtr = matrix.GetElement(colPrimeNode, colNode);
            ColPrimeBasePrimePtr = matrix.GetElement(colPrimeNode, basePrimeNode);
            ColPrimeEmitPrimePtr = matrix.GetElement(colPrimeNode, emitPrimeNode);
            BasePrimeBasePtr = matrix.GetElement(basePrimeNode, baseNode);
            BasePrimeColPrimePtr = matrix.GetElement(basePrimeNode, colPrimeNode);
            BasePrimeEmitPrimePtr = matrix.GetElement(basePrimeNode, emitPrimeNode);
            EmitPrimeEmitPtr = matrix.GetElement(emitPrimeNode, emitNode);
            EmitPrimeColPrimePtr = matrix.GetElement(emitPrimeNode, colPrimeNode);
            EmitPrimeBasePrimePtr = matrix.GetElement(emitPrimeNode, basePrimeNode);
            ColColPtr = matrix.GetElement(colNode, colNode);
            BaseBasePtr = matrix.GetElement(baseNode, baseNode);
            EmitEmitPtr = matrix.GetElement(emitNode, emitNode);
            ColPrimeColPrimePtr = matrix.GetElement(colPrimeNode, colPrimeNode);
            BasePrimeBasePrimePtr = matrix.GetElement(basePrimeNode, basePrimeNode);
            EmitPrimeEmitPrimePtr = matrix.GetElement(emitPrimeNode, emitPrimeNode);
            SubstSubstPtr = matrix.GetElement(substNode, substNode);
            ColPrimeSubstPtr = matrix.GetElement(colPrimeNode, substNode);
            SubstColPrimePtr = matrix.GetElement(substNode, colPrimeNode);
            BaseColPrimePtr = matrix.GetElement(baseNode, colPrimeNode);
            ColPrimeBasePtr = matrix.GetElement(colPrimeNode, baseNode);
        }

        /// <summary>
        /// Unsetup
        /// </summary>
        public override void Unsetup()
        {
            // Remove references
            ColColPrimePtr = null;
            BaseBasePrimePtr = null;
            EmitEmitPrimePtr = null;
            ColPrimeColPtr = null;
            ColPrimeBasePrimePtr = null;
            ColPrimeEmitPrimePtr = null;
            BasePrimeBasePtr = null;
            BasePrimeColPrimePtr = null;
            BasePrimeEmitPrimePtr = null;
            EmitPrimeEmitPtr = null;
            EmitPrimeColPrimePtr = null;
            EmitPrimeBasePrimePtr = null;
            ColColPtr = null;
            BaseBasePtr = null;
            EmitEmitPtr = null;
            ColPrimeColPrimePtr = null;
            BasePrimeBasePrimePtr = null;
            EmitPrimeEmitPrimePtr = null;
            SubstSubstPtr = null;
            ColPrimeSubstPtr = null;
            SubstColPrimePtr = null;
            BaseColPrimePtr = null;
            ColPrimeBasePtr = null;
        }

        /// <summary>
        /// Initialize AC parameters
        /// </summary>
        /// <param name="simulation">Frequency-based simulation</param>
        public override void InitializeParameters(FrequencySimulation simulation)
        {
			if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));

            double tf, tr, czbe, pe, xme, cdis, ctot, czbc, czbx, pc, xmc, fcpe, czcs, ps, xms, xtf, ovtf, xjtf;
            double arg, sarg, argtf, arg2, tmp, f2, f3, czbef2, fcpc, czbcf2, czbxf2;

            // Get voltages
            var state = simulation.State;
            double vbe = load.Vbe;
            double vbc = load.Vbc;
            double vbx = vbx = mbp.BipolarType * (state.Solution[baseNode] - state.Solution[colPrimeNode]);
            double vcs = mbp.BipolarType * (state.Solution[substNode] - state.Solution[colPrimeNode]);

            // Get shared parameters
            double cbe = load.Cbe;
            double gbe = load.Gbe;
            double gbc = load.Gbc;
            double qb = load.Qb;
            double dqbdve = load.DqbDve;

            /* 
             * charge storage elements
             */
            tf = mbp.TransitTimeF;
            tr = mbp.TransitTimeR;
            czbe = temp.TBEcap * bp.Area;
            pe = temp.TBEpot;
            xme = mbp.JunctionExpBE;
            cdis = mbp.BaseFractionBCcap;
            ctot = temp.TBCcap * bp.Area;
            czbc = ctot * cdis;
            czbx = ctot - czbc;
            pc = temp.TBCpot;
            xmc = mbp.JunctionExpBC;
            fcpe = temp.TDepCap;
            czcs = mbp.CapCS * bp.Area;
            ps = mbp.PotentialSubstrate;
            xms = mbp.ExponentialSubstrate;
            xtf = mbp.TransitTimeBiasCoeffF;
            ovtf = modeltemp.TransitTimeVbcFactor;
            xjtf = mbp.TransitTimeHighCurrentF * bp.Area;
            if (tf != 0 && vbe > 0)
            {
                argtf = 0;
                arg2 = 0;
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
                }
                cbe = cbe * (1 + argtf) / qb;
                gbe = (gbe * (1 + arg2) - cbe * dqbdve) / qb;
            }
            if (vbe < fcpe)
            {
                arg = 1 - vbe / pe;
                sarg = Math.Exp(-xme * Math.Log(arg));
                Capbe = tf * gbe + czbe * sarg;
            }
            else
            {
                f2 = modeltemp.F2;
                f3 = modeltemp.F3;
                czbef2 = czbe / f2;
                Capbe = tf * gbe + czbef2 * (f3 + xme * vbe / pe);
            }

            fcpc = temp.Tf4;
            f2 = modeltemp.F6;
            f3 = modeltemp.F7;
            if (vbc < fcpc)
            {
                arg = 1 - vbc / pc;
                sarg = Math.Exp(-xmc * Math.Log(arg));
                Capbc = tr * gbc + czbc * sarg;
            }
            else
            {
                czbcf2 = czbc / f2;
                Capbc = tr * gbc + czbcf2 * (f3 + xmc * vbc / pc);
            }
            if (vbx < fcpc)
            {
                arg = 1 - vbx / pc;
                sarg = Math.Exp(-xmc * Math.Log(arg));
                Capbx = czbx * sarg;
            }
            else
            {
                czbxf2 = czbx / f2;
                Capbx = czbxf2 * (f3 + xmc * vbx / pc);
            }
            if (vcs < 0)
            {
                arg = 1 - vcs / ps;
                sarg = Math.Exp(-xms * Math.Log(arg));
                Capcs = czcs * sarg;
            }
            else
            {
                Capcs = czcs * (1 + xms * vcs / ps);
            }
        }

        /// <summary>
        /// Execute behavior for AC analysis
        /// </summary>
        /// <param name="simulation">Frequency-based simulation</param>
        public override void Load(FrequencySimulation simulation)
        {
			if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));

            var state = simulation.State;
            var cstate = state;
            double gcpr, gepr, gpi, gmu, go, td, gx;
            Complex gm, xcpi, xcmu, xcbx, xccs, xcmcb;

            gcpr = modeltemp.CollectorConduct * bp.Area;
            gepr = modeltemp.EmitterConduct * bp.Area;
            gpi = load.Gpi;
            gmu = load.Gmu;
            gm = load.Gm;
            go = load.Go;
            td = modeltemp.ExcessPhaseFactor;
            if (td != 0)
            {
                Complex arg = td * cstate.Laplace;

                gm = gm + go;
                gm = gm * Complex.Exp(-arg);
                gm = gm - go;
            }
            gx = load.Gx;
            xcpi = Capbe * cstate.Laplace;
            xcmu = Capbc * cstate.Laplace;
            xcbx = Capbx * cstate.Laplace;
            xccs = Capcs * cstate.Laplace;
            xcmcb = Geqcb * cstate.Laplace;

            ColColPtr.Add(gcpr);
            BaseBasePtr.Add(gx + xcbx);
            EmitEmitPtr.Add(gepr);
            ColPrimeColPrimePtr.Add(gmu + go + gcpr + xcmu + xccs + xcbx);
            BasePrimeBasePrimePtr.Add(gx + gpi + gmu + xcpi + xcmu + xcmcb);
            EmitPrimeEmitPrimePtr.Add(gpi + gepr + gm + go + xcpi);
            ColColPrimePtr.Add(-gcpr);
            BaseBasePrimePtr.Add(-gx);
            EmitEmitPrimePtr.Add(-gepr);
            ColPrimeColPtr.Add(-gcpr);
            ColPrimeBasePrimePtr.Add(-gmu + gm - xcmu);
            ColPrimeEmitPrimePtr.Add(-gm - go);
            BasePrimeBasePtr.Add(-gx);
            BasePrimeColPrimePtr.Add(-gmu - xcmu - xcmcb);
            BasePrimeEmitPrimePtr.Add(-gpi - xcpi);
            EmitPrimeEmitPtr.Add(-gepr);
            EmitPrimeColPrimePtr.Add(-go + xcmcb);
            EmitPrimeBasePrimePtr.Add(-gpi - gm - xcpi - xcmcb);
            SubstSubstPtr.Add(xccs);
            ColPrimeSubstPtr.Add(-xccs);
            SubstColPrimePtr.Add(-xccs);
            BaseColPrimePtr.Add(-xcbx);
            ColPrimeBasePtr.Add(-xcbx);
        }
    }
}
