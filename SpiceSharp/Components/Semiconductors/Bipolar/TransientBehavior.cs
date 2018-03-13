using System;
using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Diagnostics;
using SpiceSharp.IntegrationMethods;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.BipolarBehaviors
{
    /// <summary>
    /// Transient behavior for a <see cref="BipolarJunctionTransistor"/>
    /// </summary>
    public class TransientBehavior : BaseTransientBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary behaviors and parameters
        /// </summary>
        private BaseParameters _bp;
        private ModelBaseParameters _mbp;
        private TemperatureBehavior _temp;
        private LoadBehavior _load;
        private ModelTemperatureBehavior _modeltemp;

        /// <summary>
        /// Nodes
        /// </summary>
        private int _collectorNode, _baseNode, _emitterNode, _substrateNode, _colPrimeNode, _basePrimeNode, _emitPrimeNode;
        protected MatrixElement<double> CollectorCollectorPrimePtr { get; private set; }
        protected MatrixElement<double> BaseBasePrimePtr { get; private set; }
        protected MatrixElement<double> EmitterEmitterPrimePtr { get; private set; }
        protected MatrixElement<double> CollectorPrimeCollectorPtr { get; private set; }
        protected MatrixElement<double> CollectorPrimeBasePrimePtr { get; private set; }
        protected MatrixElement<double> CollectorPrimeEmitterPrimePtr { get; private set; }
        protected MatrixElement<double> BasePrimeBasePtr { get; private set; }
        protected MatrixElement<double> BasePrimeCollectorPrimePtr { get; private set; }
        protected MatrixElement<double> BasePrimeEmitterPrimePtr { get; private set; }
        protected MatrixElement<double> EmitterPrimeEmitterPtr { get; private set; }
        protected MatrixElement<double> EmitterPrimeCollectorPrimePtr { get; private set; }
        protected MatrixElement<double> EmitterPrimeBasePrimePtr { get; private set; }
        protected MatrixElement<double> CollectorCollectorPtr { get; private set; }
        protected MatrixElement<double> BaseBasePtr { get; private set; }
        protected MatrixElement<double> EmitterEmitterPtr { get; private set; }
        protected MatrixElement<double> CollectorPrimeCollectorPrimePtr { get; private set; }
        protected MatrixElement<double> BasePrimeBasePrimePtr { get; private set; }
        protected MatrixElement<double> EmitterPrimeEmitterPrimePtr { get; private set; }
        protected MatrixElement<double> SubstrateSubstratePtr { get; private set; }
        protected MatrixElement<double> CollectorPrimeSubstratePtr { get; private set; }
        protected MatrixElement<double> SubstrateCollectorPrimePtr { get; private set; }
        protected MatrixElement<double> BaseCollectorPrimePtr { get; private set; }
        protected MatrixElement<double> CollectorPrimeBasePtr { get; private set; }
        protected VectorElement<double> BasePtr { get; private set; }
        protected VectorElement<double> SubstratePtr { get; private set; }
        protected VectorElement<double> CollectorPrimePtr { get; private set; }
        protected VectorElement<double> BasePrimePtr { get; private set; }
        protected VectorElement<double> EmitterPrimePtr { get; private set; }

        /// <summary>
        /// Device methods and properties
        /// </summary>
        [ParameterName("qbe"), PropertyInfo("Charge storage B-E junction")]
        public double ChargeBe => StateChargeBe.Current;
        [ParameterName("cqbe"), PropertyInfo("Capacitance current due to charges in the B-E junction")]
        public double CurrentBe => StateChargeBe.Derivative;
        [ParameterName("qbc"), PropertyInfo("Charge storage B-C junction")]
        public double ChargeBc => StateChargeBc.Current;
        [ParameterName("cqbc"), PropertyInfo("Capacitance current due to charges in the B-C junction")]
        public double CurrentBc => StateChargeBc.Derivative;
        [ParameterName("qcs"), PropertyInfo("Charge storage C-S junction")]
        public double ChargeCs => StateChargeCs.Current;
        [ParameterName("cqcs"), PropertyInfo("Capacitance current due to charges in the C-S junction")]
        public double CurrentCs => StateChargeCs.Derivative;
        [ParameterName("qbx"), PropertyInfo("Charge storage B-X junction")]
        public double ChargeBx => StateChargeBx.Current;
        [ParameterName("cqbx"), PropertyInfo("Capacitance current due to charges in the B-X junction")]
        public double CurrentBx => StateChargeBx.Derivative;
        [ParameterName("cexbc"), PropertyInfo("Total capacitance in B-X junction")]
        public double CurrentExBc => StateExcessPhaseCurrentBc.Current;
        [ParameterName("cpi"), PropertyInfo("Internal base to emitter capactance")]
        public double CapBe { get; internal set; }
        [ParameterName("cmu"), PropertyInfo("Internal base to collector capactiance")]
        public double CapBc { get; internal set; }
        [ParameterName("cbx"), PropertyInfo("Base to collector capacitance")]
        public double CapBx { get; internal set; }
        [ParameterName("ccs"), PropertyInfo("Collector to substrate capacitance")]
        public double CapCs { get; internal set; }

        /// <summary>
        /// States
        /// </summary>
        protected StateDerivative StateChargeBe { get; private set; }
        protected StateDerivative StateChargeBc { get; private set; }
        protected StateDerivative StateChargeCs { get; private set; }
        protected StateDerivative StateChargeBx { get; private set; }
        protected StateHistory StateExcessPhaseCurrentBc { get; private set; }

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
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            // Get parameters
            _bp = provider.GetParameterSet<BaseParameters>("entity");
            _mbp = provider.GetParameterSet<ModelBaseParameters>("model");

            // Get behaviors
            _temp = provider.GetBehavior<TemperatureBehavior>("entity");
            _load = provider.GetBehavior<LoadBehavior>("entity");
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
        /// <param name="solver">Matrix</param>
        public override void GetEquationPointers(Solver<double> solver)
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

            // Get rhs pointers
            BasePtr = solver.GetRhsElement(_baseNode);
            SubstratePtr = solver.GetRhsElement(_substrateNode);
            CollectorPrimePtr = solver.GetRhsElement(_colPrimeNode);
            BasePrimePtr = solver.GetRhsElement(_basePrimeNode);
            EmitterPrimePtr = solver.GetRhsElement(_emitPrimeNode);
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
        /// Create states
        /// </summary>
        /// <param name="states">Pool of all states</param>
        public override void CreateStates(StatePool states)
        {
			if (states == null)
				throw new ArgumentNullException(nameof(states));

            // We just need a history without integration here
            StateChargeBe = states.CreateDerivative();
            StateChargeBc = states.CreateDerivative();
            StateChargeCs = states.CreateDerivative();
            StateChargeBx = states.CreateDerivative();
            StateExcessPhaseCurrentBc = states.CreateHistory();
        }

        /// <summary>
        /// Calculate state variables
        /// </summary>
        /// <param name="simulation">Time-based simulation</param>
        public override void GetDcState(TimeSimulation simulation)
        {
			if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));

            var state = simulation.RealState;
            double arg;
            double sarg, f1, f2, f3;

            double cbe = _load.CurrentBe;
            double cbc = _load.CurrentBc;
            double qb = _load.BaseCharge;

            double vbe = _load.VoltageBe;
            double vbc = _load.VoltageBc;
            double vbx = _mbp.BipolarType * (state.Solution[_baseNode] - state.Solution[_colPrimeNode]);
            double vcs = _mbp.BipolarType * (state.Solution[_substrateNode] - state.Solution[_colPrimeNode]);

            StateExcessPhaseCurrentBc.Current = _load.CurrentBe / _load.BaseCharge;

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
                if (!xtf.Equals(0)) // Avoid computations
                {
                    argtf = xtf;
                    if (!ovtf.Equals(0)) // Avoid expensive Exp()
                    {
                        argtf = argtf * Math.Exp(vbc * ovtf);
                    }

                    if (!xjtf.Equals(0)) // Avoid computations
                    {
                        var tmp = cbe / (cbe + xjtf);
                        argtf = argtf * tmp * tmp;
                    }
                }
                cbe = cbe * (1 + argtf) / qb;
            }
            if (vbe < fcpe)
            {
                arg = 1 - vbe / pe;
                sarg = Math.Exp(-xme * Math.Log(arg));
                StateChargeBe.Current = tf * cbe + pe * czbe * (1 - arg * sarg) / (1 - xme);
            }
            else
            {
                f1 = _temp.TempFactor1;
                f2 = _modeltemp.F2;
                f3 = _modeltemp.F3;
                var czbef2 = czbe / f2;
                StateChargeBe.Current = tf * cbe + czbe * f1 + czbef2 * (f3 * (vbe - fcpe) + xme / (pe + pe) * (vbe * vbe -
                     fcpe * fcpe));
            }
            var fcpc = _temp.TempFactor4;
            f1 = _temp.TempFactor5;
            f2 = _modeltemp.F6;
            f3 = _modeltemp.F7;
            if (vbc < fcpc)
            {
                arg = 1 - vbc / pc;
                sarg = Math.Exp(-xmc * Math.Log(arg));
                StateChargeBc.Current = tr * cbc + pc * czbc * (1 - arg * sarg) / (1 - xmc);
            }
            else
            {
                var czbcf2 = czbc / f2;
                StateChargeBc.Current = tr * cbc + czbc * f1 + czbcf2 * (f3 * (vbc - fcpc) + xmc / (pc + pc) * (vbc * vbc -
                     fcpc * fcpc));
            }
            if (vbx < fcpc)
            {
                arg = 1 - vbx / pc;
                sarg = Math.Exp(-xmc * Math.Log(arg));
                StateChargeBx.Current = pc * czbx * (1 - arg * sarg) / (1 - xmc);
            }
            else
            {
                var czbxf2 = czbx / f2;
                StateChargeBx.Current = czbx * f1 + czbxf2 * (f3 * (vbx - fcpc) + xmc / (pc + pc) * (vbx * vbx - fcpc * fcpc));
            }
            if (vcs < 0)
            {
                arg = 1 - vcs / ps;
                sarg = Math.Exp(-xms * Math.Log(arg));
                StateChargeCs.Current = ps * czcs * (1 - arg * sarg) / (1 - xms);
            }
            else
            {
                StateChargeCs.Current = vcs * czcs * (1 + xms * vcs / (2 * ps));
            }

            // Register for excess phase calculations
            if (_modeltemp.ExcessPhaseFactor > 0.0)
            {
                _load.ExcessPhaseCalculation += CalculateExcessPhase;
            }
        }

        /// <summary>
        /// Transient behavior
        /// </summary>
        /// <param name="simulation">Time-based simulation</param>
        public override void Transient(TimeSimulation simulation)
        {
			if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));

            var state = simulation.RealState;
            double arg;
            double sarg, f1, f2, f3;

            double cbe = _load.CurrentBe;
            double cbc = _load.CurrentBc;
            double gbe = _load.CondBe;
            double gbc = _load.CondBc;
            double qb = _load.BaseCharge;
            double geqcb = 0;

            double gpi = 0.0;
            double gmu = 0.0;
            double cb = 0.0;
            double cc = 0.0;

            double vbe = _load.VoltageBe;
            double vbc = _load.VoltageBc;
            double vbx = _mbp.BipolarType * (state.Solution[_baseNode] - state.Solution[_colPrimeNode]);
            double vcs = _mbp.BipolarType * (state.Solution[_substrateNode] - state.Solution[_colPrimeNode]);

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
                gbe = (gbe * (1 + arg2) - cbe * _load.Dqbdve) / qb;
                geqcb = tf * (arg3 - cbe * _load.Dqbdvc) / qb;
            }
            if (vbe < fcpe)
            {
                arg = 1 - vbe / pe;
                sarg = Math.Exp(-xme * Math.Log(arg));
                StateChargeBe.Current = tf * cbe + pe * czbe * (1 - arg * sarg) / (1 - xme);
                CapBe = tf * gbe + czbe * sarg;
            }
            else
            {
                f1 = _temp.TempFactor1;
                f2 = _modeltemp.F2;
                f3 = _modeltemp.F3;
                var czbef2 = czbe / f2;
                StateChargeBe.Current = tf * cbe + czbe * f1 + czbef2 * (f3 * (vbe - fcpe) + xme / (pe + pe) * (vbe * vbe -
                     fcpe * fcpe));
                CapBe = tf * gbe + czbef2 * (f3 + xme * vbe / pe);
            }
            var fcpc = _temp.TempFactor4;
            f1 = _temp.TempFactor5;
            f2 = _modeltemp.F6;
            f3 = _modeltemp.F7;
            if (vbc < fcpc)
            {
                arg = 1 - vbc / pc;
                sarg = Math.Exp(-xmc * Math.Log(arg));
                StateChargeBc.Current = tr * cbc + pc * czbc * (1 - arg * sarg) / (1 - xmc);
                CapBc = tr * gbc + czbc * sarg;
            }
            else
            {
                var czbcf2 = czbc / f2;
                StateChargeBc.Current = tr * cbc + czbc * f1 + czbcf2 * (f3 * (vbc - fcpc) + xmc / (pc + pc) * (vbc * vbc -
                     fcpc * fcpc));
                CapBc = tr * gbc + czbcf2 * (f3 + xmc * vbc / pc);
            }
            if (vbx < fcpc)
            {
                arg = 1 - vbx / pc;
                sarg = Math.Exp(-xmc * Math.Log(arg));
                StateChargeBx.Current = pc * czbx * (1 - arg * sarg) / (1 - xmc);
                CapBx = czbx * sarg;
            }
            else
            {
                var czbxf2 = czbx / f2;
                StateChargeBx.Current = czbx * f1 + czbxf2 * (f3 * (vbx - fcpc) + xmc / (pc + pc) * (vbx * vbx - fcpc * fcpc));
                CapBx = czbxf2 * (f3 + xmc * vbx / pc);
            }
            if (vcs < 0)
            {
                arg = 1 - vcs / ps;
                sarg = Math.Exp(-xms * Math.Log(arg));
                StateChargeCs.Current = ps * czcs * (1 - arg * sarg) / (1 - xms);
                CapCs = czcs * sarg;
            }
            else
            {
                StateChargeCs.Current = vcs * czcs * (1 + xms * vcs / (2 * ps));
                CapCs = czcs * (1 + xms * vcs / ps);
            }

            StateChargeBe.Integrate();
            geqcb = StateChargeBe.Jacobian(geqcb); // Multiplies geqcb with method.Slope (ag[0])
            gpi += StateChargeBe.Jacobian(CapBe);
            cb += StateChargeBe.Derivative;
            StateChargeBc.Integrate();
            gmu += StateChargeBc.Jacobian(CapBc);
            cb += StateChargeBc.Derivative;
            cc -= StateChargeBc.Derivative;

            // Charge storage for c-s and b-x junctions
            StateChargeCs.Integrate();
            double gccs = StateChargeCs.Jacobian(CapCs);
            StateChargeBx.Integrate();
            double geqbx = StateChargeBx.Jacobian(CapBx);

            // Load current excitation vector
            double ceqcs = _mbp.BipolarType * (StateChargeCs.Derivative - vcs * gccs);
            double ceqbx = _mbp.BipolarType * (StateChargeBx.Derivative - vbx * geqbx);
            double ceqbe = _mbp.BipolarType * (cc + cb - vbe * gpi + vbc * -geqcb);
            double ceqbc = _mbp.BipolarType * (-cc + - vbc * gmu);

            // Load Rhs-vector
            BasePtr.Value += -ceqbx;
            CollectorPrimePtr.Value += ceqcs + ceqbx + ceqbc;
            BasePrimePtr.Value += -ceqbe - ceqbc;
            EmitterPrimePtr.Value += ceqbe;
            SubstratePtr.Value += -ceqcs;

            // Load Y-matrix
            BaseBasePtr.Value += geqbx;
            CollectorPrimeCollectorPrimePtr.Value += gmu + gccs + geqbx;
            BasePrimeBasePrimePtr.Value += gpi + gmu + geqcb;
            EmitterPrimeEmitterPrimePtr.Value += gpi;
            CollectorPrimeBasePrimePtr.Value += -gmu;
            BasePrimeCollectorPrimePtr.Value += -gmu - geqcb;
            BasePrimeEmitterPrimePtr.Value += -gpi;
            EmitterPrimeCollectorPrimePtr.Value += geqcb;
            EmitterPrimeBasePrimePtr.Value += -gpi - geqcb;
            SubstrateSubstratePtr.Value += gccs;
            CollectorPrimeSubstratePtr.Value += -gccs;
            SubstrateCollectorPrimePtr.Value += -gccs;
            BaseCollectorPrimePtr.Value += -geqbx;
            CollectorPrimeBasePtr.Value += -geqbx;
        }

        /// <summary>
        /// Truncate the timestep
        /// </summary>
        /// <param name="timestep">Current timestep</param>
        public override void Truncate(ref double timestep)
        {
            StateChargeBe.LocalTruncationError(ref timestep);
            StateChargeBc.LocalTruncationError(ref timestep);
            StateChargeCs.LocalTruncationError(ref timestep);
        }

        /// <summary>
        /// Calculate excess phase
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="args">Arguments</param>
        public void CalculateExcessPhase(object sender, ExcessPhaseEventArgs args)
        {
            if (args == null)
                throw new ArgumentNullException(nameof(args));

            double td = _modeltemp.ExcessPhaseFactor;
            if (td.Equals(0))
            {
                StateExcessPhaseCurrentBc.Current = args.ExcessPhaseCurrent;
                return;
            }
            
            /* 
             * weil's approx. for excess phase applied with backward - 
             * euler integration
             */
            double cbe = args.ExcessPhaseCurrent;
            double gbe = args.ExcessPhaseConduct;

            double delta = StateExcessPhaseCurrentBc.Timesteps[0];
            double prevdelta = StateExcessPhaseCurrentBc.Timesteps[1];
            var arg1 = delta / td;
            var arg2 = 3 * arg1;
            arg1 = arg2 * arg1;
            var denom = 1 + arg1 + arg2;
            var arg3 = arg1 / denom;
            /* Still need a place for this...
            if (state.Init == State.InitFlags.InitTransient)
            {
                state.States[1][State + Cexbc] = cbe / qb;
                state.States[2][State + Cexbc] = state.States[1][State + Cexbc];
            } */
            args.CollectorCurrent = (StateExcessPhaseCurrentBc[1] * (1 + delta / prevdelta + arg2) 
                - StateExcessPhaseCurrentBc[2] * delta / prevdelta) / denom;
            args.ExcessPhaseCurrent = cbe * arg3;
            args.ExcessPhaseConduct = gbe * arg3;
            StateExcessPhaseCurrentBc.Current = args.CollectorCurrent + args.ExcessPhaseCurrent / args.BaseCharge;
        }
    }
}
