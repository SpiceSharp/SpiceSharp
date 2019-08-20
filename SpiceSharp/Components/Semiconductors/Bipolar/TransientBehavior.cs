using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.IntegrationMethods;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.BipolarBehaviors
{
    /// <summary>
    /// Transient behavior for a <see cref="BipolarJunctionTransistor"/>
    /// </summary>
    public class TransientBehavior : DynamicParameterBehavior, ITimeBehavior
    {
        /// <summary>
        /// Gets the base RHS element.
        /// </summary>
        protected VectorElement<double> BasePtr { get; private set; }

        /// <summary>
        /// Gets the substrate RHS element.
        /// </summary>
        protected VectorElement<double> SubstratePtr { get; private set; }

        /// <summary>
        /// Gets the base-emitter capacitor current.
        /// </summary>
        [ParameterName("cqbe"), ParameterInfo("Capacitance current due to charges in the B-E junction")]
        public double CapCurrentBe => _stateChargeBe.Derivative;

        /// <summary>
        /// Gets the base-collector capacitor current.
        /// </summary>
        [ParameterName("cqbc"), ParameterInfo("Capacitance current due to charges in the B-C junction")]
        public double CapCurrentBc => _stateChargeBc.Derivative;

        /// <summary>
        /// Gets the collector-substrate capacitor current.
        /// </summary>
        [ParameterName("cqcs"), ParameterInfo("Capacitance current due to charges in the C-S junction")]
        public double CapCurrentCs => _stateChargeCs.Derivative;

        /// <summary>
        /// Gets the base-X capacitor current.
        /// </summary>
        [ParameterName("cqbx"), ParameterInfo("Capacitance current due to charges in the B-X junction")]
        public double CapCurrentBx => _stateChargeBx.Derivative;

        /// <summary>
        /// Gets the excess phase base-X capacitor current.
        /// </summary>
        [ParameterName("cexbc"), ParameterInfo("Total capacitance in B-X junction")]
        public double CurrentExBc => _stateExcessPhaseCurrentBc.Current;

        /// <summary>
        /// Gets or sets the base-emitter charge storage.
        /// </summary>
        public sealed override double ChargeBe
        {
            get => _stateChargeBe.Current;
            protected set => _stateChargeBe.Current = value;
        }

        /// <summary>
        /// Gets or sets the base-collector charge storage.
        /// </summary>
        public sealed override double ChargeBc
        {
            get => _stateChargeBc.Current;
            protected set => _stateChargeBc.Current = value;
        }

        /// <summary>
        /// Gets or sets the collector-substract charge storage.
        /// </summary>
        public sealed override double ChargeCs
        {
            get => _stateChargeCs.Current;
            protected set => _stateChargeCs.Current = value;
        }

        /// <summary>
        /// Gets or sets the base-X charge storage.
        /// </summary>
        public sealed override double ChargeBx
        {
            get => _stateChargeBx.Current;
            protected set => _stateChargeBx.Current = value;
        }

        // Cache
        private StateDerivative _stateChargeBe;
        private StateDerivative _stateChargeBc;
        private StateDerivative _stateChargeCs;
        private StateDerivative _stateChargeBx;
        private StateHistory _stateExcessPhaseCurrentBc;
        private IntegrationMethod _method;

        /// <summary>
        /// Creates a new instance of the <see cref="TransientBehavior"/> class.
        /// </summary>
        /// <param name="name">Name</param>
        public TransientBehavior(string name) : base(name) { }

        /// <summary>
        /// Bind the behavior.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="context">The context.</param>
        public override void Bind(Simulation simulation, BindingContext context)
        {
            base.Bind(simulation, context);

            var solver = State.Solver;
            BasePtr = solver.GetRhsElement(BaseNode);
            SubstratePtr = solver.GetRhsElement(SubstrateNode);

            _method = ((TimeSimulation)simulation).Method;
            _stateChargeBe = _method.CreateDerivative();
            _stateChargeBc = _method.CreateDerivative();
            _stateChargeCs = _method.CreateDerivative();
            _stateChargeBx = _method.CreateDerivative(false); // Spice 3f5 does not include this state for LTE calculations
            _stateExcessPhaseCurrentBc = _method.CreateHistory();
        }

        /// <summary>
        /// Unbind the behavior.
        /// </summary>
        public override void Unbind()
        {
            base.Unbind();

            BasePtr = null;
            SubstratePtr = null;
            _stateChargeBe = null;
            _stateChargeBc = null;
            _stateChargeCs = null;
            _stateChargeBx = null; // Spice 3f5 does not include this state for LTE calculations
            _stateExcessPhaseCurrentBc = null;
        }

        /// <summary>
        /// Calculate state variables
        /// </summary>
        void ITimeBehavior.InitializeStates()
        {
            // Calculate capacitances
            var vbe = VoltageBe;
            var vbc = VoltageBc;
            var vbx = ModelParameters.BipolarType * (State.Solution[BaseNode] - State.Solution[CollectorPrimeNode]);
            var vcs = ModelParameters.BipolarType * (State.Solution[SubstrateNode] - State.Solution[CollectorPrimeNode]);
            CalculateCapacitances(vbe, vbc, vbx, vcs);
            _stateExcessPhaseCurrentBc.Current = CapCurrentBe / BaseCharge;
        }

        /// <summary>
        /// Transient behavior
        /// </summary>
        void ITimeBehavior.Load()
        {
            var state = State;
            var gpi = 0.0;
            var gmu = 0.0;
            var cb = 0.0;
            var cc = 0.0;

            var vbe = VoltageBe;
            var vbc = VoltageBc;
            var vbx = ModelParameters.BipolarType * (state.Solution[BaseNode] - state.Solution[CollectorPrimeNode]);
            var vcs = ModelParameters.BipolarType * (state.Solution[SubstrateNode] - state.Solution[CollectorPrimeNode]);
            CalculateCapacitances(vbe, vbc, vbx, vcs);

            _stateChargeBe.Integrate();
            var geqcb = _stateChargeBe.Jacobian(Geqcb); // Multiplies geqcb with method.Slope (ag[0])
            gpi += _stateChargeBe.Jacobian(CapBe);
            cb += _stateChargeBe.Derivative;
            _stateChargeBc.Integrate();
            gmu += _stateChargeBc.Jacobian(CapBc);
            cb += _stateChargeBc.Derivative;
            cc -= _stateChargeBc.Derivative;

            // Charge storage for c-s and b-x junctions
            _stateChargeCs.Integrate();
            var gccs = _stateChargeCs.Jacobian(CapCs);
            _stateChargeBx.Integrate();
            var geqbx = _stateChargeBx.Jacobian(CapBx);

            // Load current excitation vector
            var ceqcs = ModelParameters.BipolarType * (_stateChargeCs.Derivative - vcs * gccs);
            var ceqbx = ModelParameters.BipolarType * (_stateChargeBx.Derivative - vbx * geqbx);
            var ceqbe = ModelParameters.BipolarType * (cc + cb - vbe * gpi + vbc * -geqcb);
            var ceqbc = ModelParameters.BipolarType * (-cc + - vbc * gmu);

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
                _stateExcessPhaseCurrentBc.Current = cex;
                return;
            }
            
            /* 
             * weil's approx. for excess phase applied with backward - 
             * euler integration
             */
            var cbe = cex;
            var gbe = gex;

            var delta = _method.GetTimestep(0);
            var prevdelta = _method.GetTimestep(1);
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
            cc = (_stateExcessPhaseCurrentBc[1] * (1 + delta / prevdelta + arg2) 
                - _stateExcessPhaseCurrentBc[2] * delta / prevdelta) / denom;
            cex = cbe * arg3;
            gex = gbe * arg3;
            _stateExcessPhaseCurrentBc.Current = cc + cex / BaseCharge;
        }
    }
}
