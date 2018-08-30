using System;
using System.Numerics;
using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.BipolarBehaviors
{
    /// <summary>
    /// AC behavior for <see cref="BipolarJunctionTransistor"/>
    /// </summary>
    public class FrequencyBehavior : BaseFrequencyBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        private BaseParameters _bp;
        private LoadBehavior _load;
        private TemperatureBehavior _temp;
        private ModelBaseParameters _mbp;
        private ModelTemperatureBehavior _modeltemp;

        /// <summary>
        /// Nodes
        /// </summary>
        private int _collectorNode, _baseNode, _emitterNode, _substrateNode, _colPrimeNode, _basePrimeNode, _emitPrimeNode;
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
        [ParameterName("cpi"), ParameterInfo("Internal base to emitter capactance")]
        public double CapBe { get; protected set; }
        [ParameterName("cmu"), ParameterInfo("Internal base to collector capactiance")]
        public double CapBc { get; protected set; }
        [ParameterName("cbx"), ParameterInfo("Base to collector capacitance")]
        public double CapBx { get; protected set; }
        [ParameterName("ccs"), ParameterInfo("Collector to substrate capacitance")]
        public double CapCs { get; protected set; }
        [ParameterName("gcb"), ParameterInfo("Conductance of the C-B junction")]
        public double CondCb { get; protected set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public FrequencyBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Setup behavior
        /// </summary>
        /// <param name="simulation">Simulation</param>
        /// <param name="provider">Data provider</param>
        public override void Setup(Simulation simulation, SetupDataProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            // Get parameters
            _bp = provider.GetParameterSet<BaseParameters>();
            _mbp = provider.GetParameterSet<ModelBaseParameters>("model");

            // Get behaviors
            _temp = provider.GetBehavior<TemperatureBehavior>();
            _load = provider.GetBehavior<LoadBehavior>();
            _modeltemp = provider.GetBehavior<ModelTemperatureBehavior>("model");
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
                throw new CircuitException("Pin count mismatch: 4 pins expected, {0} given".FormatString(pins.Length));
            _collectorNode = pins[0];
            _baseNode = pins[1];
            _emitterNode = pins[2];
            _substrateNode = pins[3];
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
            _colPrimeNode = _load.CollectorPrimeNode;
            _basePrimeNode = _load.BasePrimeNode;
            _emitPrimeNode = _load.EmitterPrimeNode;

            // Get matrix pointers
            CollectorCollectorPrimePtr = solver.GetMatrixElement(_collectorNode, _colPrimeNode);
            BaseBasePrimePtr = solver.GetMatrixElement(_baseNode, _basePrimeNode);
            EmitterEmitterPrimePtr = solver.GetMatrixElement(_emitterNode, _emitPrimeNode);
            CollectorPrimeCollectorPtr = solver.GetMatrixElement(_colPrimeNode, _collectorNode);
            CollectorPrimeBasePrimePtr = solver.GetMatrixElement(_colPrimeNode, _basePrimeNode);
            CollectorPrimeEmitterPrimePtr = solver.GetMatrixElement(_colPrimeNode, _emitPrimeNode);
            BasePrimeBasePtr = solver.GetMatrixElement(_basePrimeNode, _baseNode);
            BasePrimeCollectorPrimePtr = solver.GetMatrixElement(_basePrimeNode, _colPrimeNode);
            BasePrimeEmitterPrimePtr = solver.GetMatrixElement(_basePrimeNode, _emitPrimeNode);
            EmitterPrimeEmitterPtr = solver.GetMatrixElement(_emitPrimeNode, _emitterNode);
            EmitterPrimeCollectorPrimePtr = solver.GetMatrixElement(_emitPrimeNode, _colPrimeNode);
            EmitterPrimeBasePrimePtr = solver.GetMatrixElement(_emitPrimeNode, _basePrimeNode);
            CollectorCollectorPtr = solver.GetMatrixElement(_collectorNode, _collectorNode);
            BaseBasePtr = solver.GetMatrixElement(_baseNode, _baseNode);
            EmitterEmitterPtr = solver.GetMatrixElement(_emitterNode, _emitterNode);
            CollectorPrimeCollectorPrimePtr = solver.GetMatrixElement(_colPrimeNode, _colPrimeNode);
            BasePrimeBasePrimePtr = solver.GetMatrixElement(_basePrimeNode, _basePrimeNode);
            EmitterPrimeEmitterPrimePtr = solver.GetMatrixElement(_emitPrimeNode, _emitPrimeNode);
            SubstrateSubstratePtr = solver.GetMatrixElement(_substrateNode, _substrateNode);
            CollectorPrimeSubstratePtr = solver.GetMatrixElement(_colPrimeNode, _substrateNode);
            SubstrateCollectorPrimePtr = solver.GetMatrixElement(_substrateNode, _colPrimeNode);
            BaseCollectorPrimePtr = solver.GetMatrixElement(_baseNode, _colPrimeNode);
            CollectorPrimeBasePtr = solver.GetMatrixElement(_colPrimeNode, _baseNode);
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

            double arg, sarg;
            double f2, f3;

            // Get voltages
            var state = simulation.RealState;
            var vbe = _load.VoltageBe;
            var vbc = _load.VoltageBc;
            var vbx = _mbp.BipolarType * (state.Solution[_baseNode] - state.Solution[_colPrimeNode]);
            var vcs = _mbp.BipolarType * (state.Solution[_substrateNode] - state.Solution[_colPrimeNode]);

            // Get shared parameters
            var cbe = _load.CurrentBe;
            var gbe = _load.CondBe;
            var gbc = _load.CondBc;
            var qb = _load.BaseCharge;
            var dqbdve = _load.Dqbdve;
            var dqbdvc = _load.Dqbdvc;

            // Charge storage elements
            double tf = _mbp.TransitTimeForward;
            double tr = _mbp.TransitTimeReverse;
            var czbe = _temp.TempBeCap * _bp.Area;
            var pe = _temp.TempBePotential;
            double xme = _mbp.JunctionExpBe;
            double cdis = _mbp.BaseFractionBcCap;
            var ctot = _temp.TempBcCap * _bp.Area;
            var czbc = ctot * cdis;
            var czbx = ctot - czbc;
            var pc = _temp.TempBcPotential;
            double xmc = _mbp.JunctionExpBc;
            var fcpe = _temp.TempDepletionCap;
            var czcs = _mbp.CapCs * _bp.Area;
            double ps = _mbp.PotentialSubstrate;
            double xms = _mbp.ExponentialSubstrate;
            double xtf = _mbp.TransitTimeBiasCoefficientForward;
            var ovtf = _modeltemp.TransitTimeVoltageBcFactor;
            var xjtf = _mbp.TransitTimeHighCurrentForward * _bp.Area;
            if (!tf.Equals(0) && vbe > 0) // Avoid computations
            {
                double argtf = 0;
                double arg2 = 0;
                double arg3 = 0;
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
                        var tmp = cbe / (cbe + xjtf);
                        argtf = argtf * tmp * tmp;
                        arg2 = argtf * (3 - tmp - tmp);
                    }
                    arg3 = cbe * argtf * ovtf;
                }
                cbe = cbe * (1 + argtf) / qb;
                gbe = (gbe * (1 + arg2) - cbe * dqbdve) / qb;
                CondCb = tf * (arg3 - cbe * dqbdvc) / qb;
            }
            if (vbe < fcpe)
            {
                arg = 1 - vbe / pe;
                sarg = Math.Exp(-xme * Math.Log(arg));
                CapBe = tf * gbe + czbe * sarg;
            }
            else
            {
                f2 = _modeltemp.F2;
                f3 = _modeltemp.F3;
                var czbef2 = czbe / f2;
                CapBe = tf * gbe + czbef2 * (f3 + xme * vbe / pe);
            }

            var fcpc = _temp.TempFactor4;
            f2 = _modeltemp.F6;
            f3 = _modeltemp.F7;
            if (vbc < fcpc)
            {
                arg = 1 - vbc / pc;
                sarg = Math.Exp(-xmc * Math.Log(arg));
                CapBc = tr * gbc + czbc * sarg;
            }
            else
            {
                var czbcf2 = czbc / f2;
                CapBc = tr * gbc + czbcf2 * (f3 + xmc * vbc / pc);
            }
            if (vbx < fcpc)
            {
                arg = 1 - vbx / pc;
                sarg = Math.Exp(-xmc * Math.Log(arg));
                CapBx = czbx * sarg;
            }
            else
            {
                var czbxf2 = czbx / f2;
                CapBx = czbxf2 * (f3 + xmc * vbx / pc);
            }
            if (vcs < 0)
            {
                arg = 1 - vcs / ps;
                sarg = Math.Exp(-xms * Math.Log(arg));
                CapCs = czcs * sarg;
            }
            else
            {
                CapCs = czcs * (1 + xms * vcs / ps);
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
            var gcpr = _modeltemp.CollectorConduct * _bp.Area;
            var gepr = _modeltemp.EmitterConduct * _bp.Area;
            var gpi = _load.ConductancePi;
            var gmu = _load.ConductanceMu;
            Complex gm = _load.Transconductance;
            var go = _load.OutputConductance;
            var td = _modeltemp.ExcessPhaseFactor;
            if (!td.Equals(0)) // Avoid computations
            {
                var arg = td * cstate.Laplace;

                gm = gm + go;
                gm = gm * Complex.Exp(-arg);
                gm = gm - go;
            }
            var gx = _load.ConductanceX;
            var xcpi = CapBe * cstate.Laplace;
            var xcmu = CapBc * cstate.Laplace;
            var xcbx = CapBx * cstate.Laplace;
            var xccs = CapCs * cstate.Laplace;
            var xcmcb = CondCb * cstate.Laplace;

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
