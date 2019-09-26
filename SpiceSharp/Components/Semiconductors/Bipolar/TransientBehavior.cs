using SpiceSharp.Circuits;
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
        /// Gets the transient matrix elements.
        /// </summary>
        /// <value>
        /// The transient matrix elements.
        /// </value>
        protected RealMatrixElementSet TransientMatrixElements { get; private set; }

        /// <summary>
        /// Gets the transient vector elements.
        /// </summary>
        /// <value>
        /// The transient vector elements.
        /// </value>
        protected RealVectorElementSet TransientVectorElements { get; private set; }

        /// <summary>
        /// Gets the base-emitter capacitor current.
        /// </summary>
        [ParameterName("cqbe"), ParameterInfo("Capacitance current due to charges in the B-E junction")]
        public double CapCurrentBe => BiasingStateChargeBe.Derivative;

        /// <summary>
        /// Gets the base-collector capacitor current.
        /// </summary>
        [ParameterName("cqbc"), ParameterInfo("Capacitance current due to charges in the B-C junction")]
        public double CapCurrentBc => BiasingStateChargeBc.Derivative;

        /// <summary>
        /// Gets the collector-substrate capacitor current.
        /// </summary>
        [ParameterName("cqcs"), ParameterInfo("Capacitance current due to charges in the C-S junction")]
        public double CapCurrentCs => BiasingStateChargeCs.Derivative;

        /// <summary>
        /// Gets the base-X capacitor current.
        /// </summary>
        [ParameterName("cqbx"), ParameterInfo("Capacitance current due to charges in the B-X junction")]
        public double CapCurrentBx => BiasingStateChargeBx.Derivative;

        /// <summary>
        /// Gets the excess phase base-X capacitor current.
        /// </summary>
        [ParameterName("cexbc"), ParameterInfo("Total capacitance in B-X junction")]
        public double CurrentExBc => BiasingStateExcessPhaseCurrentBc.Current;

        /// <summary>
        /// Gets or sets the base-emitter charge storage.
        /// </summary>
        public sealed override double ChargeBe
        {
            get => BiasingStateChargeBe.Current;
            protected set => BiasingStateChargeBe.Current = value;
        }

        /// <summary>
        /// Gets or sets the base-collector charge storage.
        /// </summary>
        public sealed override double ChargeBc
        {
            get => BiasingStateChargeBc.Current;
            protected set => BiasingStateChargeBc.Current = value;
        }

        /// <summary>
        /// Gets or sets the collector-substract charge storage.
        /// </summary>
        public sealed override double ChargeCs
        {
            get => BiasingStateChargeCs.Current;
            protected set => BiasingStateChargeCs.Current = value;
        }

        /// <summary>
        /// Gets or sets the base-X charge storage.
        /// </summary>
        public sealed override double ChargeBx
        {
            get => BiasingStateChargeBx.Current;
            protected set => BiasingStateChargeBx.Current = value;
        }

        
        private StateDerivative BiasingStateChargeBe;
        private StateDerivative BiasingStateChargeBc;
        private StateDerivative BiasingStateChargeCs;
        private StateDerivative BiasingStateChargeBx;
        private StateHistory BiasingStateExcessPhaseCurrentBc;
        private IntegrationMethod _method;

        /// <summary>
        /// Creates a new instance of the <see cref="TransientBehavior"/> class.
        /// </summary>
        /// <param name="name">Name</param>
        public TransientBehavior(string name) : base(name) { }

        /// <summary>
        /// Bind the behavior to a simulation.
        /// </summary>
        /// <param name="context">The binding context.</param>
        public override void Bind(BindingContext context)
        {
            base.Bind(context);

            TransientVectorElements = new RealVectorElementSet(BiasingState.Solver, 
                BaseNode, SubstrateNode);
            TransientMatrixElements = new RealMatrixElementSet(BiasingState.Solver,
                new MatrixPin(BaseNode, BaseNode),
                new MatrixPin(CollectorPrimeNode, CollectorPrimeNode),
                new MatrixPin(BasePrimeNode, BasePrimeNode),
                new MatrixPin(EmitterPrimeNode, EmitterPrimeNode),
                new MatrixPin(CollectorPrimeNode, BasePrimeNode),
                new MatrixPin(BasePrimeNode, CollectorPrimeNode),
                new MatrixPin(BasePrimeNode, EmitterPrimeNode),
                new MatrixPin(EmitterPrimeNode, CollectorPrimeNode),
                new MatrixPin(EmitterPrimeNode, BasePrimeNode),
                new MatrixPin(SubstrateNode, SubstrateNode),
                new MatrixPin(CollectorPrimeNode, SubstrateNode),
                new MatrixPin(SubstrateNode, CollectorPrimeNode),
                new MatrixPin(BaseNode, CollectorPrimeNode),
                new MatrixPin(CollectorPrimeNode, BaseNode));

            _method = context.States.GetValue<TimeSimulationState>().Method;
            BiasingStateChargeBe = _method.CreateDerivative();
            BiasingStateChargeBc = _method.CreateDerivative();
            BiasingStateChargeCs = _method.CreateDerivative();
            BiasingStateChargeBx = _method.CreateDerivative(false); // Spice 3f5 does not include this state for LTE calculations
            BiasingStateExcessPhaseCurrentBc = _method.CreateHistory();
        }

        /// <summary>
        /// Unbind the behavior.
        /// </summary>
        public override void Unbind()
        {
            base.Unbind();
            TransientVectorElements?.Destroy();
            TransientVectorElements = null;
            TransientMatrixElements?.Destroy();
            TransientMatrixElements = null;
            BiasingStateChargeBe = null;
            BiasingStateChargeBc = null;
            BiasingStateChargeCs = null;
            BiasingStateChargeBx = null; // Spice 3f5 does not include this state for LTE calculations
            BiasingStateExcessPhaseCurrentBc = null;
        }

        /// <summary>
        /// Calculate state variables
        /// </summary>
        void ITimeBehavior.InitializeStates()
        {
            // Calculate capacitances
            var vbe = VoltageBe;
            var vbc = VoltageBc;
            var vbx = ModelParameters.BipolarType * (BiasingState.Solution[BaseNode] - BiasingState.Solution[CollectorPrimeNode]);
            var vcs = ModelParameters.BipolarType * (BiasingState.Solution[SubstrateNode] - BiasingState.Solution[CollectorPrimeNode]);
            CalculateCapacitances(vbe, vbc, vbx, vcs);
            BiasingStateExcessPhaseCurrentBc.Current = CapCurrentBe / BaseCharge;
        }

        /// <summary>
        /// Transient behavior
        /// </summary>
        void ITimeBehavior.Load()
        {
            var state = BiasingState;
            var gpi = 0.0;
            var gmu = 0.0;
            var cb = 0.0;
            var cc = 0.0;

            var vbe = VoltageBe;
            var vbc = VoltageBc;
            var vbx = ModelParameters.BipolarType * (state.Solution[BaseNode] - state.Solution[CollectorPrimeNode]);
            var vcs = ModelParameters.BipolarType * (state.Solution[SubstrateNode] - state.Solution[CollectorPrimeNode]);
            CalculateCapacitances(vbe, vbc, vbx, vcs);

            BiasingStateChargeBe.Integrate();
            var geqcb = BiasingStateChargeBe.Jacobian(Geqcb); // Multiplies geqcb with method.Slope (ag[0])
            gpi += BiasingStateChargeBe.Jacobian(CapBe);
            cb += BiasingStateChargeBe.Derivative;
            BiasingStateChargeBc.Integrate();
            gmu += BiasingStateChargeBc.Jacobian(CapBc);
            cb += BiasingStateChargeBc.Derivative;
            cc -= BiasingStateChargeBc.Derivative;

            // Charge storage for c-s and b-x junctions
            BiasingStateChargeCs.Integrate();
            var gccs = BiasingStateChargeCs.Jacobian(CapCs);
            BiasingStateChargeBx.Integrate();
            var geqbx = BiasingStateChargeBx.Jacobian(CapBx);

            // Load current excitation vector
            var ceqcs = ModelParameters.BipolarType * (BiasingStateChargeCs.Derivative - vcs * gccs);
            var ceqbx = ModelParameters.BipolarType * (BiasingStateChargeBx.Derivative - vbx * geqbx);
            var ceqbe = ModelParameters.BipolarType * (cc + cb - vbe * gpi + vbc * -geqcb);
            var ceqbc = ModelParameters.BipolarType * (-cc + -vbc * gmu);

            // Load Rhs-vector
            TransientVectorElements.Add(-ceqbx, -ceqcs);
            VectorElements.Add(ceqcs + ceqbx + ceqbc, -ceqbe - ceqbc, ceqbe);

            // Load Y-matrix
            TransientMatrixElements.Add(
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
                -geqbx);
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
                BiasingStateExcessPhaseCurrentBc.Current = cex;
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
            cc = (BiasingStateExcessPhaseCurrentBc[1] * (1 + delta / prevdelta + arg2) 
                - BiasingStateExcessPhaseCurrentBc[2] * delta / prevdelta) / denom;
            cex = cbe * arg3;
            gex = gbe * arg3;
            BiasingStateExcessPhaseCurrentBc.Current = cc + cex / BaseCharge;
        }
    }
}
