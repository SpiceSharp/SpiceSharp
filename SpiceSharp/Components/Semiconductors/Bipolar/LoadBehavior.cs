using System;
using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Circuits;
using SpiceSharp.Components.Semiconductors;
using SpiceSharp.Diagnostics;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.BipolarBehaviors
{
    /// <summary>
    /// General behavior for <see cref="BipolarJunctionTransistor"/>
    /// </summary>
    public class LoadBehavior : Behaviors.LoadBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        private BaseParameters _bp;
        private ModelBaseParameters _mbp;
        private TemperatureBehavior _temp;
        private ModelTemperatureBehavior _modeltemp;

        /// <summary>
        /// Methods
        /// </summary>
        [PropertyName("vbe"), PropertyInfo("B-E voltage")]
        public double VoltageBe { get; protected set; }
        [PropertyName("vbc"), PropertyInfo("B-C voltage")]
        public double VoltageBc { get; protected set; }
        [PropertyName("cc"), PropertyInfo("Current at collector node")]
        public double CollectorCurrent { get; protected set; }
        [PropertyName("cb"), PropertyInfo("Current at base node")]
        public double BaseCurrent { get; protected set; }
        [PropertyName("gpi"), PropertyInfo("Small signal input conductance - pi")]
        public double ConductancePi { get; protected set; }
        [PropertyName("gmu"), PropertyInfo("Small signal conductance - mu")]
        public double ConductanceMu { get; protected set; }
        [PropertyName("gm"), PropertyInfo("Small signal transconductance")]
        public double Transconductance { get; protected set; }
        [PropertyName("go"), PropertyInfo("Small signal output conductance")]
        public double OutputConductance { get; protected set; }
        public double ConductanceX { get; protected set; }

        /// <summary>
        /// Nodes
        /// </summary>
        private int _collectorNode, _baseNode, _emitterNode, _substrateNode;
        public int CollectorPrimeNode { get; private set; }
        public int BasePrimeNode { get; private set; }
        public int EmitterPrimeNode { get; private set; }
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
        protected VectorElement<double> CollectorPrimePtr { get; private set; }
        protected VectorElement<double> BasePrimePtr { get; private set; }
        protected VectorElement<double> EmitterPrimePtr { get; private set; }

        /// <summary>
        /// Shared parameters
        /// </summary>
        public double CurrentBe { get; protected set; }
        public double CondBe { get; protected set; }
        public double CurrentBc { get; protected set; }
        public double CondBc { get; protected set; }
        public double BaseCharge { get; protected set; }
        public double Dqbdvc { get; protected set; }
        public double Dqbdve { get; protected set; }

        /// <summary>
        /// Event called when excess phase calculation is needed
        /// </summary>
        public event EventHandler<ExcessPhaseEventArgs> ExcessPhaseCalculation;

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
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            // Get parameters
            _bp = provider.GetParameterSet<BaseParameters>("entity");
            _mbp = provider.GetParameterSet<ModelBaseParameters>("model");

            // Get behaviors
            _temp = provider.GetBehavior<TemperatureBehavior>("entity");
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
        /// <param name="nodes">Nodes</param>
        /// <param name="solver">Solver</param>
        public override void GetEquationPointers(Nodes nodes, Solver<double> solver)
        {
            if (nodes == null)
                throw new ArgumentNullException(nameof(nodes));
            if (solver == null)
                throw new ArgumentNullException(nameof(solver));

            // Add a series collector node if necessary
            if (_mbp.CollectorResistance.Value > 0)
                CollectorPrimeNode = nodes.Create(Name.Grow("#col")).Index;
            else
                CollectorPrimeNode = _collectorNode;

            // Add a series base node if necessary
            if (_mbp.BaseResist.Value > 0)
                BasePrimeNode = nodes.Create(Name.Grow("#base")).Index;
            else
                BasePrimeNode = _baseNode;

            // Add a series emitter node if necessary
            if (_mbp.EmitterResistance.Value > 0)
                EmitterPrimeNode = nodes.Create(Name.Grow("#emit")).Index;
            else
                EmitterPrimeNode = _emitterNode;

            // Get solver pointers
            CollectorCollectorPrimePtr = solver.GetMatrixElement(_collectorNode, CollectorPrimeNode);
            BaseBasePrimePtr = solver.GetMatrixElement(_baseNode, BasePrimeNode);
            EmitterEmitterPrimePtr = solver.GetMatrixElement(_emitterNode, EmitterPrimeNode);
            CollectorPrimeCollectorPtr = solver.GetMatrixElement(CollectorPrimeNode, _collectorNode);
            CollectorPrimeBasePrimePtr = solver.GetMatrixElement(CollectorPrimeNode, BasePrimeNode);
            CollectorPrimeEmitterPrimePtr = solver.GetMatrixElement(CollectorPrimeNode, EmitterPrimeNode);
            BasePrimeBasePtr = solver.GetMatrixElement(BasePrimeNode, _baseNode);
            BasePrimeCollectorPrimePtr = solver.GetMatrixElement(BasePrimeNode, CollectorPrimeNode);
            BasePrimeEmitterPrimePtr = solver.GetMatrixElement(BasePrimeNode, EmitterPrimeNode);
            EmitterPrimeEmitterPtr = solver.GetMatrixElement(EmitterPrimeNode, _emitterNode);
            EmitterPrimeCollectorPrimePtr = solver.GetMatrixElement(EmitterPrimeNode, CollectorPrimeNode);
            EmitterPrimeBasePrimePtr = solver.GetMatrixElement(EmitterPrimeNode, BasePrimeNode);
            CollectorCollectorPtr = solver.GetMatrixElement(_collectorNode, _collectorNode);
            BaseBasePtr = solver.GetMatrixElement(_baseNode, _baseNode);
            EmitterEmitterPtr = solver.GetMatrixElement(_emitterNode, _emitterNode);
            CollectorPrimeCollectorPrimePtr = solver.GetMatrixElement(CollectorPrimeNode, CollectorPrimeNode);
            BasePrimeBasePrimePtr = solver.GetMatrixElement(BasePrimeNode, BasePrimeNode);
            EmitterPrimeEmitterPrimePtr = solver.GetMatrixElement(EmitterPrimeNode, EmitterPrimeNode);
            SubstrateSubstratePtr = solver.GetMatrixElement(_substrateNode, _substrateNode);
            CollectorPrimeSubstratePtr = solver.GetMatrixElement(CollectorPrimeNode, _substrateNode);
            SubstrateCollectorPrimePtr = solver.GetMatrixElement(_substrateNode, CollectorPrimeNode);
            BaseCollectorPrimePtr = solver.GetMatrixElement(_baseNode, CollectorPrimeNode);
            CollectorPrimeBasePtr = solver.GetMatrixElement(CollectorPrimeNode, _baseNode);

            // Get RHS pointers
            CollectorPrimePtr = solver.GetRhsElement(CollectorPrimeNode);
            BasePrimePtr = solver.GetRhsElement(BasePrimeNode);
            EmitterPrimePtr = solver.GetRhsElement(EmitterPrimeNode);
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
        /// Execute behavior
        /// </summary>
        /// <param name="simulation">Base simulation</param>
        public override void Load(BaseSimulation simulation)
        {
            if (simulation == null)
                throw new ArgumentNullException(nameof(simulation));

            var state = simulation.RealState;
            double vt;
            double csat, rbpr, rbpi, gcpr, gepr, oik, c2, vte, oikr, c4, vtc, xjrb, vbe, vbc;
            double vce;
            double vtn, evbe, gben, evben, cben, evbc, gbcn, evbcn, cbcn, q1, q2, arg, sqarg, cc, cex,
                gex, arg1, arg2, cb, gx, gpi, gmu, go, gm;
            double ceqbe, ceqbc;

            vt = _bp.Temperature * Circuit.KOverQ;

            // DC model parameters
            csat = _temp.TempSaturationCurrent * _bp.Area;
            rbpr = _mbp.MinimumBaseResistance / _bp.Area;
            rbpi = _mbp.BaseResist / _bp.Area - rbpr;
            gcpr = _modeltemp.CollectorConduct * _bp.Area;
            gepr = _modeltemp.EmitterConduct * _bp.Area;
            oik = _modeltemp.InverseRollOffForward / _bp.Area;
            c2 = _temp.TempBeLeakageCurrent * _bp.Area;
            vte = _mbp.LeakBeEmissionCoefficient * vt;
            oikr = _modeltemp.InverseRollOffReverse / _bp.Area;
            c4 = _temp.TempBcLeakageCurrent * _bp.Area;
            vtc = _mbp.LeakBcEmissionCoefficient * vt;
            xjrb = _mbp.BaseCurrentHalfResist * _bp.Area;

            // Initialization
            if (state.Init == RealState.InitializationStates.InitJunction && state.Domain == RealState.DomainType.Time && state.UseDc && state.UseIc)
            {
                vbe = _mbp.BipolarType * _bp.InitialVoltageBe;
                vce = _mbp.BipolarType * _bp.InitialVoltageCe;
                vbc = vbe - vce;
            }
            else if (state.Init == RealState.InitializationStates.InitJunction && !_bp.Off)
            {
                vbe = _temp.TempVCritical;
                vbc = 0;
            }
            else if (state.Init == RealState.InitializationStates.InitJunction || state.Init == RealState.InitializationStates.InitFix && _bp.Off)
            {
                vbe = 0;
                vbc = 0;
            }
            else
            {
                // Compute new nonlinear branch voltages
                vbe = _mbp.BipolarType * (state.Solution[BasePrimeNode] - state.Solution[EmitterPrimeNode]);
                vbc = _mbp.BipolarType * (state.Solution[BasePrimeNode] - state.Solution[CollectorPrimeNode]);

                // Limit nonlinear branch voltages
                bool limited = false;
                vbe = Semiconductor.LimitJunction(vbe, VoltageBe, vt, _temp.TempVCritical, ref limited);
                vbc = Semiconductor.LimitJunction(vbc, VoltageBc, vt, _temp.TempVCritical, ref limited);
                if (limited)
                    state.IsConvergent = false;
            }

            // Determine dc current and derivitives
            vtn = vt * _mbp.EmissionCoefficientForward;
            if (vbe > -5 * vtn)
            {
                evbe = Math.Exp(vbe / vtn);
                CurrentBe = csat * (evbe - 1) + state.Gmin * vbe;
                CondBe = csat * evbe / vtn + state.Gmin;
                if (c2.Equals(0)) // Avoid Exp()
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
                CondBe = -csat / vbe + state.Gmin;
                CurrentBe = CondBe * vbe;
                gben = -c2 / vbe;
                cben = gben * vbe;
            }

            vtn = vt * _mbp.EmissionCoefficientReverse;
            if (vbc > -5 * vtn)
            {
                evbc = Math.Exp(vbc / vtn);
                CurrentBc = csat * (evbc - 1) + state.Gmin * vbc;
                CondBc = csat * evbc / vtn + state.Gmin;
                if (c4.Equals(0)) // Avoid Exp()
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
                CondBc = -csat / vbc + state.Gmin;
                CurrentBc = CondBc * vbc;
                gbcn = -c4 / vbc;
                cbcn = gbcn * vbc;
            }

            // Determine base charge terms
            q1 = 1 / (1 - _modeltemp.InverseEarlyVoltForward * vbc - _modeltemp.InverseEarlyVoltReverse * vbe);
            if (oik.Equals(0) && oikr.Equals(0)) // Avoid computations
            {
                BaseCharge = q1;
                Dqbdve = q1 * BaseCharge * _modeltemp.InverseEarlyVoltReverse;
                Dqbdvc = q1 * BaseCharge * _modeltemp.InverseEarlyVoltForward;
            }
            else
            {
                q2 = oik * CurrentBe + oikr * CurrentBc;
                arg = Math.Max(0, 1 + 4 * q2);
                sqarg = 1;
                if (!arg.Equals(0)) // Avoid Sqrt()
                    sqarg = Math.Sqrt(arg);
                BaseCharge = q1 * (1 + sqarg) / 2;
                Dqbdve = q1 * (BaseCharge * _modeltemp.InverseEarlyVoltReverse + oik * CondBe / sqarg);
                Dqbdvc = q1 * (BaseCharge * _modeltemp.InverseEarlyVoltForward + oikr * CondBc / sqarg);
            }

            // Excess phase calculation
            ExcessPhaseEventArgs ep = new ExcessPhaseEventArgs
            {
                CollectorCurrent = 0.0,
                ExcessPhaseCurrent = CurrentBe,
                ExcessPhaseConduct = CondBe,
                BaseCharge = BaseCharge
            };
            ExcessPhaseCalculation?.Invoke(this, ep);
            cc = ep.CollectorCurrent;
            cex = ep.ExcessPhaseCurrent;
            gex = ep.ExcessPhaseConduct;

            // Determine dc incremental conductances
            cc = cc + (cex - CurrentBc) / BaseCharge - CurrentBc / _temp.TempBetaReverse - cbcn;
            cb = CurrentBe / _temp.TempBetaForward + cben + CurrentBc / _temp.TempBetaReverse + cbcn;
            gx = rbpr + rbpi / BaseCharge;
            if (!xjrb.Equals(0)) // Avoid calculations
            {
                arg1 = Math.Max(cb / xjrb, 1e-9);
                arg2 = (-1 + Math.Sqrt(1 + 14.59025 * arg1)) / 2.4317 / Math.Sqrt(arg1);
                arg1 = Math.Tan(arg2);
                gx = rbpr + 3 * rbpi * (arg1 - arg2) / arg2 / arg1 / arg1;
            }
            if (!gx.Equals(0)) // Do not divide by 0
                gx = 1 / gx;
            gpi = CondBe / _temp.TempBetaForward + gben;
            gmu = CondBc / _temp.TempBetaReverse + gbcn;
            go = (CondBc + (cex - CurrentBc) * Dqbdvc / BaseCharge) / BaseCharge;
            gm = (gex - (cex - CurrentBc) * Dqbdve / BaseCharge) / BaseCharge - go;

            VoltageBe = vbe;
            VoltageBc = vbc;
            CollectorCurrent = cc;
            BaseCurrent = cb;
            ConductancePi = gpi;
            ConductanceMu = gmu;
            Transconductance = gm;
            OutputConductance = go;
            ConductanceX = gx;

            // Load current excitation vector
            ceqbe = _mbp.BipolarType * (cc + cb - vbe * (gm + go + gpi) + vbc * go);
            ceqbc = _mbp.BipolarType * (-cc + vbe * (gm + go) - vbc * (gmu + go));
            CollectorPrimePtr.Value += ceqbc;
            BasePrimePtr.Value += -ceqbe - ceqbc;
            EmitterPrimePtr.Value += ceqbe;

            // Load y matrix
            CollectorCollectorPtr.Value += gcpr;
            BaseBasePtr.Value += gx;
            EmitterEmitterPtr.Value += gepr;
            CollectorPrimeCollectorPrimePtr.Value += gmu + go + gcpr;
            BasePrimeBasePrimePtr.Value += gx + gpi + gmu;
            EmitterPrimeEmitterPrimePtr.Value += gpi + gepr + gm + go;
            CollectorCollectorPrimePtr.Value += -gcpr;
            BaseBasePrimePtr.Value += -gx;
            EmitterEmitterPrimePtr.Value += -gepr;
            CollectorPrimeCollectorPtr.Value += -gcpr;
            CollectorPrimeBasePrimePtr.Value += -gmu + gm;
            CollectorPrimeEmitterPrimePtr.Value += -gm - go;
            BasePrimeBasePtr.Value += -gx;
            BasePrimeCollectorPrimePtr.Value += -gmu;
            BasePrimeEmitterPrimePtr.Value += -gpi;
            EmitterPrimeEmitterPtr.Value += -gepr;
            EmitterPrimeCollectorPrimePtr.Value += -go;
            EmitterPrimeBasePrimePtr.Value += -gpi - gm;
        }

        /// <summary>
        /// Check if the BJT is convergent
        /// </summary>
        /// <param name="simulation">Base simulation</param>
        /// <returns></returns>
        public override bool IsConvergent(BaseSimulation simulation)
        {
			if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));

            var state = simulation.RealState;
            var config = simulation.BaseConfiguration;

            double vbe, vbc, delvbe, delvbc, cchat, cbhat, cc, cb;

            vbe = _mbp.BipolarType * (state.Solution[BasePrimeNode] - state.Solution[EmitterPrimeNode]);
            vbc = _mbp.BipolarType * (state.Solution[BasePrimeNode] - state.Solution[CollectorPrimeNode]);
            delvbe = vbe - VoltageBe;
            delvbc = vbc - VoltageBe;
            cchat = CollectorCurrent + (Transconductance + OutputConductance) * delvbe - (OutputConductance + ConductanceMu) * delvbc;
            cbhat = BaseCurrent + ConductancePi * delvbe + ConductanceMu * delvbc;
            cc = CollectorCurrent;
            cb = BaseCurrent;

            // Check convergence
            double tol = config.RelativeTolerance * Math.Max(Math.Abs(cchat), Math.Abs(cc)) + config.AbsoluteTolerance;
            if (Math.Abs(cchat - cc) > tol)
            {
                state.IsConvergent = false;
                return false;
            }

            tol = config.RelativeTolerance * Math.Max(Math.Abs(cbhat), Math.Abs(cb)) + config.AbsoluteTolerance;
            if (Math.Abs(cbhat - cb) > tol)
            {
                state.IsConvergent = false;
                return false;
            }
            return true;
        }
    }
}
