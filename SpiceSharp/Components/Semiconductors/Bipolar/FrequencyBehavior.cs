using System;
using System.Numerics;
using SpiceSharp.Behaviors;
using SpiceSharp.Algebra;
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
        protected MatrixElement<Complex> CollectorCollectorPrimePtr { get; private set; }
        protected MatrixElement<Complex> BaseBasePrimePtr { get; private set; }
        protected MatrixElement<Complex> EmitterEmitterPrimePtr { get; private set; }
        protected MatrixElement<Complex> CollectorPrimeCollectorPtr { get; private set; }
        protected MatrixElement<Complex> CollectorPrimeBasePrimePtr { get; private set; }
        protected MatrixElement<Complex> CollectorPrimeEmitterPrimePtr { get; private set; }
        protected MatrixElement<Complex> BasePrimeBasePtr { get; private set; }
        protected MatrixElement<Complex> BasePrimeCollectorPrimePtr { get; private set; }
        protected MatrixElement<Complex> BasePrimeEmitterPrimePtr { get; private set; }
        protected MatrixElement<Complex> EmitterPrimeEmitterPtr { get; private set; }
        protected MatrixElement<Complex> EmitterPrimeCollectorPrimePtr { get; private set; }
        protected MatrixElement<Complex> EmitterPrimeBasePrimePtr { get; private set; }
        protected MatrixElement<Complex> CollectorCollectorPtr { get; private set; }
        protected MatrixElement<Complex> BaseBasePtr { get; private set; }
        protected MatrixElement<Complex> EmitterEmitterPtr { get; private set; }
        protected MatrixElement<Complex> CollectorPrimeCollectorPrimePtr { get; private set; }
        protected MatrixElement<Complex> BasePrimeBasePrimePtr { get; private set; }
        protected MatrixElement<Complex> EmitterPrimeEmitterPrimePtr { get; private set; }
        protected MatrixElement<Complex> SubstrateSubstratePtr { get; private set; }
        protected MatrixElement<Complex> CollectorPrimeSubstratePtr { get; private set; }
        protected MatrixElement<Complex> SubstrateCollectorPrimePtr { get; private set; }
        protected MatrixElement<Complex> BaseCollectorPrimePtr { get; private set; }
        protected MatrixElement<Complex> CollectorPrimeBasePtr { get; private set; }
        
        /// <summary>
        /// Device methods and properties
        /// </summary>
        [PropertyName("cpi"), PropertyInfo("Internal base to emitter capactance")]
        public double CapBE { get; protected set; }
        [PropertyName("cmu"), PropertyInfo("Internal base to collector capactiance")]
        public double CapBC { get; protected set; }
        [PropertyName("cbx"), PropertyInfo("Base to collector capacitance")]
        public double CapBX { get; protected set; }
        [PropertyName("ccs"), PropertyInfo("Collector to substrate capacitance")]
        public double CapCS { get; protected set; }
        [PropertyName("gcb"), PropertyInfo("Conductance of the C-B junction")]
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
            bp = provider.GetParameterSet<BaseParameters>("entity");
            mbp = provider.GetParameterSet<ModelBaseParameters>("model");

            // Get behaviors
            temp = provider.GetBehavior<TemperatureBehavior>("entity");
            load = provider.GetBehavior<LoadBehavior>("entity");
            modeltemp = provider.GetBehavior<ModelTemperatureBehavior>("model");
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
        /// Gets matrix pointers
        /// </summary>
        /// <param name="solver">Solver</param>
        public override void GetEquationPointers(Solver<Complex> solver)
        {
			if (solver == null)
				throw new ArgumentNullException(nameof(solver));

            // Get extra equations
            colPrimeNode = load.CollectorPrimeNode;
            basePrimeNode = load.BasePrimeNode;
            emitPrimeNode = load.EmitterPrimeNode;

            // Get matrix pointers
            CollectorCollectorPrimePtr = solver.GetMatrixElement(collectorNode, colPrimeNode);
            BaseBasePrimePtr = solver.GetMatrixElement(baseNode, basePrimeNode);
            EmitterEmitterPrimePtr = solver.GetMatrixElement(emitterNode, emitPrimeNode);
            CollectorPrimeCollectorPtr = solver.GetMatrixElement(colPrimeNode, collectorNode);
            CollectorPrimeBasePrimePtr = solver.GetMatrixElement(colPrimeNode, basePrimeNode);
            CollectorPrimeEmitterPrimePtr = solver.GetMatrixElement(colPrimeNode, emitPrimeNode);
            BasePrimeBasePtr = solver.GetMatrixElement(basePrimeNode, baseNode);
            BasePrimeCollectorPrimePtr = solver.GetMatrixElement(basePrimeNode, colPrimeNode);
            BasePrimeEmitterPrimePtr = solver.GetMatrixElement(basePrimeNode, emitPrimeNode);
            EmitterPrimeEmitterPtr = solver.GetMatrixElement(emitPrimeNode, emitterNode);
            EmitterPrimeCollectorPrimePtr = solver.GetMatrixElement(emitPrimeNode, colPrimeNode);
            EmitterPrimeBasePrimePtr = solver.GetMatrixElement(emitPrimeNode, basePrimeNode);
            CollectorCollectorPtr = solver.GetMatrixElement(collectorNode, collectorNode);
            BaseBasePtr = solver.GetMatrixElement(baseNode, baseNode);
            EmitterEmitterPtr = solver.GetMatrixElement(emitterNode, emitterNode);
            CollectorPrimeCollectorPrimePtr = solver.GetMatrixElement(colPrimeNode, colPrimeNode);
            BasePrimeBasePrimePtr = solver.GetMatrixElement(basePrimeNode, basePrimeNode);
            EmitterPrimeEmitterPrimePtr = solver.GetMatrixElement(emitPrimeNode, emitPrimeNode);
            SubstrateSubstratePtr = solver.GetMatrixElement(substrateNode, substrateNode);
            CollectorPrimeSubstratePtr = solver.GetMatrixElement(colPrimeNode, substrateNode);
            SubstrateCollectorPrimePtr = solver.GetMatrixElement(substrateNode, colPrimeNode);
            BaseCollectorPrimePtr = solver.GetMatrixElement(baseNode, colPrimeNode);
            CollectorPrimeBasePtr = solver.GetMatrixElement(colPrimeNode, baseNode);
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
            double arg, sarg, argtf, arg2, arg3, tmp, f2, f3, czbef2, fcpc, czbcf2, czbxf2;

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
            double dqbdvc = load.Dqbdvc;

            // Charge storage elements
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
            if (!tf.Equals(0) && vbe > 0) // Avoid computations
            {
                argtf = 0;
                arg2 = 0;
                arg3 = 0;
                if (!xtf.Equals(0)) // Avoid computations
                {
                    argtf = xtf;
                    if (!ovtf.Equals(0)) // Avoid expensive Exp()
                    {
                        argtf = argtf * Math.Exp(vbc * ovtf);
                    }
                    arg2 = argtf;
                    if (!xjtf.Equals(0)) // Avoid computations
                    {
                        tmp = cbe / (cbe + xjtf);
                        argtf = argtf * tmp * tmp;
                        arg2 = argtf * (3 - tmp - tmp);
                    }
                    arg3 = cbe * argtf * ovtf;
                }
                cbe = cbe * (1 + argtf) / qb;
                gbe = (gbe * (1 + arg2) - cbe * dqbdve) / qb;
                CondCB = tf * (arg3 - cbe * dqbdvc) / qb;
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
            if (!td.Equals(0)) // Avoid computations
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

            CollectorCollectorPtr.Value += gcpr;
            BaseBasePtr.Value += gx + xcbx;
            EmitterEmitterPtr.Value += gepr;
            CollectorPrimeCollectorPrimePtr.Value += gmu + go + gcpr + xcmu + xccs + xcbx;
            BasePrimeBasePrimePtr.Value += gx + gpi + gmu + xcpi + xcmu + xcmcb;
            EmitterPrimeEmitterPrimePtr.Value += gpi + gepr + gm + go + xcpi;
            CollectorCollectorPrimePtr.Value += -gcpr;
            BaseBasePrimePtr.Value += -gx;
            EmitterEmitterPrimePtr.Value += -gepr;
            CollectorPrimeCollectorPtr.Value += -gcpr;
            CollectorPrimeBasePrimePtr.Value += -gmu + gm - xcmu;
            CollectorPrimeEmitterPrimePtr.Value += -gm - go;
            BasePrimeBasePtr.Value += -gx;
            BasePrimeCollectorPrimePtr.Value += -gmu - xcmu - xcmcb;
            BasePrimeEmitterPrimePtr.Value += -gpi - xcpi;
            EmitterPrimeEmitterPtr.Value += -gepr;
            EmitterPrimeCollectorPrimePtr.Value += -go + xcmcb;
            EmitterPrimeBasePrimePtr.Value += -gpi - gm - xcpi - xcmcb;
            SubstrateSubstratePtr.Value += xccs;
            CollectorPrimeSubstratePtr.Value += -xccs;
            SubstrateCollectorPrimePtr.Value += -xccs;
            BaseCollectorPrimePtr.Value += -xcbx;
            CollectorPrimeBasePtr.Value += -xcbx;
        }
    }
}
