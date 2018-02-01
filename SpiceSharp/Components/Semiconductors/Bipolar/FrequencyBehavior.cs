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
        int collectorNode, baseNode, emitterNode, substrateNode, colPrimeNode, basePrimeNode, emitPrimeNode;
        protected MatrixElement CollectorCollectorPrimePtr { get; private set; }
        protected MatrixElement BaseBasePrimePtr { get; private set; }
        protected MatrixElement EmitterEmitterPrimePtr { get; private set; }
        protected MatrixElement CollectorPrimeCollectorPtr { get; private set; }
        protected MatrixElement CollectorPrimeBasePrimePtr { get; private set; }
        protected MatrixElement CollectorPrimeEmitterPrimePtr { get; private set; }
        protected MatrixElement BasePrimeBasePtr { get; private set; }
        protected MatrixElement BasePrimeCollectorPrimePtr { get; private set; }
        protected MatrixElement BasePrimeEmitterPrimePtr { get; private set; }
        protected MatrixElement EmitterPrimeEmitterPtr { get; private set; }
        protected MatrixElement EmitterPrimeCollectorPrimePtr { get; private set; }
        protected MatrixElement EmitterPrimeBasePrimePtr { get; private set; }
        protected MatrixElement CollectorCollectorPtr { get; private set; }
        protected MatrixElement BaseBasePtr { get; private set; }
        protected MatrixElement EmitterEmitterPtr { get; private set; }
        protected MatrixElement CollectorPrimeCollectorPrimePtr { get; private set; }
        protected MatrixElement BasePrimeBasePrimePtr { get; private set; }
        protected MatrixElement EmitterPrimeEmitterPrimePtr { get; private set; }
        protected MatrixElement SubstrateSubstratePtr { get; private set; }
        protected MatrixElement CollectorPrimeSubstratePtr { get; private set; }
        protected MatrixElement SubstrateCollectorPrimePtr { get; private set; }
        protected MatrixElement BaseCollectorPrimePtr { get; private set; }
        protected MatrixElement CollectorPrimeBasePtr { get; private set; }
        
        /// <summary>
        /// Properties
        /// </summary>
        [PropertyName("cpi"), PropertyInfo("Internal base to emitter capactance")]
        public double CapBE { get; protected set; }
        [PropertyName("cmu"), PropertyInfo("Internal base to collector capactiance")]
        public double CapBC { get; protected set; }
        [PropertyName("cbx"), PropertyInfo("Base to collector capacitance")]
        public double CapBX { get; protected set; }
        [PropertyName("ccs"), PropertyInfo("Collector to substrate capacitance")]
        public double CapCS { get; protected set; }

        public double CondCB { get; protected set; }

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
            collectorNode = pins[0];
            baseNode = pins[1];
            emitterNode = pins[2];
            substrateNode = pins[3];
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
            colPrimeNode = load.CollectorPrimeNode;
            basePrimeNode = load.BasePrimeNode;
            emitPrimeNode = load.EmitterPrimeNode;

            // Get matrix pointers
            CollectorCollectorPrimePtr = matrix.GetElement(collectorNode, colPrimeNode);
            BaseBasePrimePtr = matrix.GetElement(baseNode, basePrimeNode);
            EmitterEmitterPrimePtr = matrix.GetElement(emitterNode, emitPrimeNode);
            CollectorPrimeCollectorPtr = matrix.GetElement(colPrimeNode, collectorNode);
            CollectorPrimeBasePrimePtr = matrix.GetElement(colPrimeNode, basePrimeNode);
            CollectorPrimeEmitterPrimePtr = matrix.GetElement(colPrimeNode, emitPrimeNode);
            BasePrimeBasePtr = matrix.GetElement(basePrimeNode, baseNode);
            BasePrimeCollectorPrimePtr = matrix.GetElement(basePrimeNode, colPrimeNode);
            BasePrimeEmitterPrimePtr = matrix.GetElement(basePrimeNode, emitPrimeNode);
            EmitterPrimeEmitterPtr = matrix.GetElement(emitPrimeNode, emitterNode);
            EmitterPrimeCollectorPrimePtr = matrix.GetElement(emitPrimeNode, colPrimeNode);
            EmitterPrimeBasePrimePtr = matrix.GetElement(emitPrimeNode, basePrimeNode);
            CollectorCollectorPtr = matrix.GetElement(collectorNode, collectorNode);
            BaseBasePtr = matrix.GetElement(baseNode, baseNode);
            EmitterEmitterPtr = matrix.GetElement(emitterNode, emitterNode);
            CollectorPrimeCollectorPrimePtr = matrix.GetElement(colPrimeNode, colPrimeNode);
            BasePrimeBasePrimePtr = matrix.GetElement(basePrimeNode, basePrimeNode);
            EmitterPrimeEmitterPrimePtr = matrix.GetElement(emitPrimeNode, emitPrimeNode);
            SubstrateSubstratePtr = matrix.GetElement(substrateNode, substrateNode);
            CollectorPrimeSubstratePtr = matrix.GetElement(colPrimeNode, substrateNode);
            SubstrateCollectorPrimePtr = matrix.GetElement(substrateNode, colPrimeNode);
            BaseCollectorPrimePtr = matrix.GetElement(baseNode, colPrimeNode);
            CollectorPrimeBasePtr = matrix.GetElement(colPrimeNode, baseNode);
        }

        /// <summary>
        /// Unsetup
        /// </summary>
        public override void Unsetup()
        {
            // Remove references
            CollectorCollectorPrimePtr = null;
            BaseBasePrimePtr = null;
            EmitterEmitterPrimePtr = null;
            CollectorPrimeCollectorPtr = null;
            CollectorPrimeBasePrimePtr = null;
            CollectorPrimeEmitterPrimePtr = null;
            BasePrimeBasePtr = null;
            BasePrimeCollectorPrimePtr = null;
            BasePrimeEmitterPrimePtr = null;
            EmitterPrimeEmitterPtr = null;
            EmitterPrimeCollectorPrimePtr = null;
            EmitterPrimeBasePrimePtr = null;
            CollectorCollectorPtr = null;
            BaseBasePtr = null;
            EmitterEmitterPtr = null;
            CollectorPrimeCollectorPrimePtr = null;
            BasePrimeBasePrimePtr = null;
            EmitterPrimeEmitterPrimePtr = null;
            SubstrateSubstratePtr = null;
            CollectorPrimeSubstratePtr = null;
            SubstrateCollectorPrimePtr = null;
            BaseCollectorPrimePtr = null;
            CollectorPrimeBasePtr = null;
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
            var state = simulation.RealState;
            double vbe = load.VoltageBE;
            double vbc = load.VoltageBC;
            double vbx = vbx = mbp.BipolarType * (state.Solution[baseNode] - state.Solution[colPrimeNode]);
            double vcs = mbp.BipolarType * (state.Solution[substrateNode] - state.Solution[colPrimeNode]);

            // Get shared parameters
            double cbe = load.CurrentBE;
            double gbe = load.CondBE;
            double gbc = load.CondBC;
            double qb = load.BaseCharge;
            double dqbdve = load.Dqbdve;

            /* 
             * charge storage elements
             */
            tf = mbp.TransitTimeForward;
            tr = mbp.TransitTimeReverse;
            czbe = temp.TempBECap * bp.Area;
            pe = temp.TempBEPotential;
            xme = mbp.JunctionExpBE;
            cdis = mbp.BaseFractionBCCap;
            ctot = temp.TempBCCap * bp.Area;
            czbc = ctot * cdis;
            czbx = ctot - czbc;
            pc = temp.TempBCPotential;
            xmc = mbp.JunctionExpBC;
            fcpe = temp.TempDepletionCap;
            czcs = mbp.CapCS * bp.Area;
            ps = mbp.PotentialSubstrate;
            xms = mbp.ExponentialSubstrate;
            xtf = mbp.TransitTimeBiasCoefficientForward;
            ovtf = modeltemp.TransitTimeVoltageBCFactor;
            xjtf = mbp.TransitTimeHighCurrentForward * bp.Area;
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
                CapBE = tf * gbe + czbe * sarg;
            }
            else
            {
                f2 = modeltemp.F2;
                f3 = modeltemp.F3;
                czbef2 = czbe / f2;
                CapBE = tf * gbe + czbef2 * (f3 + xme * vbe / pe);
            }

            fcpc = temp.TempFactor4;
            f2 = modeltemp.F6;
            f3 = modeltemp.F7;
            if (vbc < fcpc)
            {
                arg = 1 - vbc / pc;
                sarg = Math.Exp(-xmc * Math.Log(arg));
                CapBC = tr * gbc + czbc * sarg;
            }
            else
            {
                czbcf2 = czbc / f2;
                CapBC = tr * gbc + czbcf2 * (f3 + xmc * vbc / pc);
            }
            if (vbx < fcpc)
            {
                arg = 1 - vbx / pc;
                sarg = Math.Exp(-xmc * Math.Log(arg));
                CapBX = czbx * sarg;
            }
            else
            {
                czbxf2 = czbx / f2;
                CapBX = czbxf2 * (f3 + xmc * vbx / pc);
            }
            if (vcs < 0)
            {
                arg = 1 - vcs / ps;
                sarg = Math.Exp(-xms * Math.Log(arg));
                CapCS = czcs * sarg;
            }
            else
            {
                CapCS = czcs * (1 + xms * vcs / ps);
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

            var state = simulation.RealState;
            var cstate = simulation.ComplexState;
            double gcpr, gepr, gpi, gmu, go, td, gx;
            Complex gm, xcpi, xcmu, xcbx, xccs, xcmcb;

            gcpr = modeltemp.CollectorConduct * bp.Area;
            gepr = modeltemp.EmitterConduct * bp.Area;
            gpi = load.ConductancePi;
            gmu = load.ConductanceMu;
            gm = load.Transconductance;
            go = load.OutputConductance;
            td = modeltemp.ExcessPhaseFactor;
            if (td != 0)
            {
                Complex arg = td * cstate.Laplace;

                gm = gm + go;
                gm = gm * Complex.Exp(-arg);
                gm = gm - go;
            }
            gx = load.ConductanceX;
            xcpi = CapBE * cstate.Laplace;
            xcmu = CapBC * cstate.Laplace;
            xcbx = CapBX * cstate.Laplace;
            xccs = CapCS * cstate.Laplace;
            xcmcb = CondCB * cstate.Laplace;

            CollectorCollectorPtr.Add(gcpr);
            BaseBasePtr.Add(gx + xcbx);
            EmitterEmitterPtr.Add(gepr);
            CollectorPrimeCollectorPrimePtr.Add(gmu + go + gcpr + xcmu + xccs + xcbx);
            BasePrimeBasePrimePtr.Add(gx + gpi + gmu + xcpi + xcmu + xcmcb);
            EmitterPrimeEmitterPrimePtr.Add(gpi + gepr + gm + go + xcpi);
            CollectorCollectorPrimePtr.Add(-gcpr);
            BaseBasePrimePtr.Add(-gx);
            EmitterEmitterPrimePtr.Add(-gepr);
            CollectorPrimeCollectorPtr.Add(-gcpr);
            CollectorPrimeBasePrimePtr.Add(-gmu + gm - xcmu);
            CollectorPrimeEmitterPrimePtr.Add(-gm - go);
            BasePrimeBasePtr.Add(-gx);
            BasePrimeCollectorPrimePtr.Add(-gmu - xcmu - xcmcb);
            BasePrimeEmitterPrimePtr.Add(-gpi - xcpi);
            EmitterPrimeEmitterPtr.Add(-gepr);
            EmitterPrimeCollectorPrimePtr.Add(-go + xcmcb);
            EmitterPrimeBasePrimePtr.Add(-gpi - gm - xcpi - xcmcb);
            SubstrateSubstratePtr.Add(xccs);
            CollectorPrimeSubstratePtr.Add(-xccs);
            SubstrateCollectorPrimePtr.Add(-xccs);
            BaseCollectorPrimePtr.Add(-xcbx);
            CollectorPrimeBasePtr.Add(-xcbx);
        }
    }
}
