using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;
using SpiceSharp.Simulations.IntegrationMethods;

namespace SpiceSharp.Components.BipolarBehaviors
{
    /// <summary>
    /// Transient behavior for a <see cref="BipolarJunctionTransistor"/>
    /// </summary>
    public class TimeBehavior : DynamicParameterBehavior, ITimeBehavior
    {
        /// <summary>
        /// Gets the transient matrix elements.
        /// </summary>
        /// <value>
        /// The transient matrix elements.
        /// </value>
        protected ElementSet<double> TransientElements { get; private set; }

        /// <summary>
        /// Gets the base-emitter capacitor current.
        /// </summary>
        [ParameterName("cqbe"), ParameterInfo("Capacitance current due to charges in the B-E junction")]
        public double CapCurrentBe => _biasingStateChargeBe.Derivative;

        /// <summary>
        /// Gets the base-collector capacitor current.
        /// </summary>
        [ParameterName("cqbc"), ParameterInfo("Capacitance current due to charges in the B-C junction")]
        public double CapCurrentBc => _biasingStateChargeBc.Derivative;

        /// <summary>
        /// Gets the collector-substrate capacitor current.
        /// </summary>
        [ParameterName("cqcs"), ParameterInfo("Capacitance current due to charges in the C-S junction")]
        public double CapCurrentCs => _biasingStateChargeCs.Derivative;

        /// <summary>
        /// Gets the base-X capacitor current.
        /// </summary>
        [ParameterName("cqbx"), ParameterInfo("Capacitance current due to charges in the B-X junction")]
        public double CapCurrentBx => _biasingStateChargeBx.Derivative;

        /// <summary>
        /// Gets the excess phase base-X capacitor current.
        /// </summary>
        [ParameterName("cexbc"), ParameterInfo("Total capacitance in B-X junction")]
        public double CurrentExBc => _biasingStateExcessPhaseCurrentBc.Value;

        /// <summary>
        /// Gets or sets the base-emitter charge storage.
        /// </summary>
        public sealed override double ChargeBe
        {
            get => _biasingStateChargeBe.Value;
            protected set => _biasingStateChargeBe.Value = value;
        }

        /// <summary>
        /// Gets or sets the base-collector charge storage.
        /// </summary>
        public sealed override double ChargeBc
        {
            get => _biasingStateChargeBc.Value;
            protected set => _biasingStateChargeBc.Value = value;
        }

        /// <summary>
        /// Gets or sets the collector-substract charge storage.
        /// </summary>
        public sealed override double ChargeCs
        {
            get => _biasingStateChargeCs.Value;
            protected set => _biasingStateChargeCs.Value = value;
        }

        /// <summary>
        /// Gets or sets the base-X charge storage.
        /// </summary>
        public sealed override double ChargeBx
        {
            get => _biasingStateChargeBx.Value;
            protected set => _biasingStateChargeBx.Value = value;
        }

        private IDerivative _biasingStateChargeBe;
        private IDerivative _biasingStateChargeBc;
        private IDerivative _biasingStateChargeCs;
        private IDerivative _biasingStateChargeBx;
        private StateValue<double> _biasingStateExcessPhaseCurrentBc;
        private IIntegrationMethod _method;
        private int _baseNode, _substrateNode, _collectorPrimeNode, _basePrimeNode, _emitterPrimeNode;
        private readonly ITimeSimulationState _time;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public TimeBehavior(string name, ComponentBindingContext context) : base(name, context) 
        {
            _time = context.GetState<ITimeSimulationState>();
            _baseNode = BiasingState.Map[context.Nodes[1]];
            _substrateNode = BiasingState.Map[context.Nodes[3]];
            _collectorPrimeNode = BiasingState.Map[CollectorPrime];
            _basePrimeNode = BiasingState.Map[BasePrime];
            _emitterPrimeNode = BiasingState.Map[EmitterPrime];
            TransientElements = new ElementSet<double>(BiasingState.Solver, new[] {
                new MatrixLocation(_baseNode, _baseNode),
                new MatrixLocation(_collectorPrimeNode, _collectorPrimeNode),
                new MatrixLocation(_basePrimeNode, _basePrimeNode),
                new MatrixLocation(_emitterPrimeNode, _emitterPrimeNode),
                new MatrixLocation(_collectorPrimeNode, _basePrimeNode),
                new MatrixLocation(_basePrimeNode, _collectorPrimeNode),
                new MatrixLocation(_basePrimeNode, _emitterPrimeNode),
                new MatrixLocation(_emitterPrimeNode, _collectorPrimeNode),
                new MatrixLocation(_emitterPrimeNode, _basePrimeNode),
                new MatrixLocation(_substrateNode, _substrateNode),
                new MatrixLocation(_collectorPrimeNode, _substrateNode),
                new MatrixLocation(_substrateNode, _collectorPrimeNode),
                new MatrixLocation(_baseNode, _collectorPrimeNode),
                new MatrixLocation(_collectorPrimeNode, _baseNode)
            }, new[] { _baseNode, _substrateNode, _collectorPrimeNode, _basePrimeNode, _emitterPrimeNode });

            _method = context.GetState<IIntegrationMethod>();
            _biasingStateChargeBe = _method.CreateDerivative();
            _biasingStateChargeBc = _method.CreateDerivative();
            _biasingStateChargeCs = _method.CreateDerivative();
            _biasingStateChargeBx = _method.CreateDerivative(false); // Spice 3f5 does not include this state for LTE calculations
            _biasingStateExcessPhaseCurrentBc = new StateValue<double>(3);
            _method.RegisterState(_biasingStateExcessPhaseCurrentBc);
        }

        /// <summary>
        /// Calculate state variables
        /// </summary>
        void ITimeBehavior.InitializeStates()
        {
            // Calculate capacitances
            var vbe = VoltageBe;
            var vbc = VoltageBc;
            var vbx = ModelParameters.BipolarType * (BiasingState.Solution[_baseNode] - BiasingState.Solution[_collectorPrimeNode]);
            var vcs = ModelParameters.BipolarType * (BiasingState.Solution[_substrateNode] - BiasingState.Solution[_collectorPrimeNode]);
            CalculateCapacitances(vbe, vbc, vbx, vcs);
            _biasingStateExcessPhaseCurrentBc.Value = CapCurrentBe / BaseCharge;
        }

        /// <summary>
        /// Transient behavior
        /// </summary>
        protected override void Load()
        {
            base.Load();
            if (_time.UseDc)
                return;
            var state = BiasingState;
            var gpi = 0.0;
            var gmu = 0.0;
            var cb = 0.0;
            var cc = 0.0;

            var vbe = VoltageBe;
            var vbc = VoltageBc;
            var vbx = ModelParameters.BipolarType * (state.Solution[_baseNode] - state.Solution[_collectorPrimeNode]);
            var vcs = ModelParameters.BipolarType * (state.Solution[_substrateNode] - state.Solution[_collectorPrimeNode]);
            CalculateCapacitances(vbe, vbc, vbx, vcs);

            _biasingStateChargeBe.Integrate();
            var info = _biasingStateChargeBe.GetContributions(Geqcb);
            var geqcb = info.Jacobian; // Multiplies geqcb with method.Slope (ag[0])
            gpi += _biasingStateChargeBe.GetContributions(CapBe).Jacobian;
            cb += _biasingStateChargeBe.Derivative;
            _biasingStateChargeBc.Integrate();
            gmu += _biasingStateChargeBc.GetContributions(CapBc).Jacobian;
            cb += _biasingStateChargeBc.Derivative;
            cc -= _biasingStateChargeBc.Derivative;

            // Charge storage for c-s and b-x junctions
            _biasingStateChargeCs.Integrate();
            var gccs = _biasingStateChargeCs.GetContributions(CapCs).Jacobian;
            _biasingStateChargeBx.Integrate();
            var geqbx = _biasingStateChargeBx.GetContributions(CapBx).Jacobian;

            // Load current excitation vector
            var ceqcs = ModelParameters.BipolarType * (_biasingStateChargeCs.Derivative - vcs * gccs);
            var ceqbx = ModelParameters.BipolarType * (_biasingStateChargeBx.Derivative - vbx * geqbx);
            var ceqbe = ModelParameters.BipolarType * (cc + cb - vbe * gpi + vbc * -geqcb);
            var ceqbc = ModelParameters.BipolarType * (-cc + -vbc * gmu);

            TransientElements.Add(
                // Y-matrix
                geqbx,
                gmu + gccs + geqbx,
                gpi + gmu + geqcb,
                gpi,
                -gmu,
                -gmu - geqcb,
                -gpi,
                geqcb,
                -gpi - geqcb,
                gccs,
                -gccs,
                -gccs,
                -geqbx,
                -geqbx,
                // RHS vector
                -ceqbx, 
                -ceqcs,
                ceqcs + ceqbx + ceqbc,
                -ceqbe - ceqbc, 
                ceqbe);
        }

        /// <summary>
        /// Initializes the voltages for the current iteration.
        /// </summary>
        /// <param name="vbe">The base-emitter voltage.</param>
        /// <param name="vbc">The base-collector voltage.</param>
        protected override void Initialize(out double vbe, out double vbc)
        {
            if (Iteration.Mode == IterationModes.Junction && _time.UseDc && _time.UseIc)
            {
                vbe = ModelParameters.BipolarType * BaseParameters.InitialVoltageBe;
                var vce = ModelParameters.BipolarType * BaseParameters.InitialVoltageCe;
                vbc = vbe - vce;
                return;
            }
            base.Initialize(out vbe, out vbc);
        }

        /// <summary>
        /// Excess phase calculation.
        /// </summary>
        /// <param name="cc">The collector current.</param>
        /// <param name="cex">The excess phase current.</param>
        /// <param name="gex">The excess phase conductance.</param>
        protected override void ExcessPhaseCalculation(ref double cc, ref double cex, ref double gex)
        {
            var td = ModelTemperature.ExcessPhaseFactor;
            if (td.Equals(0))
            {
                _biasingStateExcessPhaseCurrentBc.Value = cex;
                return;
            }
            
            /* 
             * weil's approx. for excess phase applied with backward - 
             * euler integration
             */
            var cbe = cex;
            var gbe = gex;

            var delta = _method.GetPreviousTimestep(0);
            var prevdelta = _method.GetPreviousTimestep(1);
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

            cc = (_biasingStateExcessPhaseCurrentBc.GetPreviousValue(1) * (1 + delta / prevdelta + arg2) 
                - _biasingStateExcessPhaseCurrentBc.GetPreviousValue(2) * delta / prevdelta) / denom;
            cex = cbe * arg3;
            gex = gbe * arg3;
            _biasingStateExcessPhaseCurrentBc.Value = cc + cex / BaseCharge;
        }
    }
}
