using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Simulations.IntegrationMethods;
using System;

namespace SpiceSharp.Components.Bipolars
{
    /// <summary>
    /// Transient behavior for a <see cref="BipolarJunctionTransistor"/>.
    /// </summary>
    /// <seealso cref="Dynamic"/>
    /// <seealso cref="ITimeBehavior"/>
    [BehaviorFor(typeof(BipolarJunctionTransistor)), AddBehaviorIfNo(typeof(ITimeBehavior))]
    [GeneratedParameters]
    public partial class Time : Dynamic,
        ITimeBehavior
    {
        private readonly IDerivative _biasingStateChargeBe, _biasingStateChargeBc, _biasingStateChargeCs, _biasingStateChargeBx;
        private readonly StateValue<double> _biasingStateExcessPhaseCurrentBc;
        private readonly IIntegrationMethod _method;
        private readonly int _baseNode, _substrateNode, _collectorPrimeNode, _basePrimeNode, _emitterPrimeNode;
        private readonly ITimeSimulationState _time;
        private readonly ElementSet<double> _elements;

        /// <summary>
        /// Gets the base-emitter capacitor current.
        /// </summary>
        /// <value>
        /// The base-emitter capacitor current.
        /// </value>
        [ParameterName("cqbe"), ParameterInfo("Capacitance current due to charges in the B-E junction")]
        public double CapCurrentBe => _biasingStateChargeBe.Derivative;

        /// <summary>
        /// Gets the base-collector capacitor current.
        /// </summary>
        /// <value>
        /// The base-collector capacitor current.
        /// </value>
        [ParameterName("cqbc"), ParameterInfo("Capacitance current due to charges in the B-C junction")]
        public double CapCurrentBc => _biasingStateChargeBc.Derivative;

        /// <summary>
        /// Gets the collector-substrate capacitor current.
        /// </summary>
        /// <value>
        /// The collector-substrate capacitor current.
        /// </value>
        [ParameterName("cqcs"), ParameterInfo("Capacitance current due to charges in the C-S junction")]
        public double CapCurrentCs => _biasingStateChargeCs.Derivative;

        /// <summary>
        /// Gets the base-X capacitor current.
        /// </summary>
        /// <value>
        /// The base-X capacitor current.
        /// </value>
        [ParameterName("cqbx"), ParameterInfo("Capacitance current due to charges in the B-X junction")]
        public double CapCurrentBx => _biasingStateChargeBx.Derivative;

        /// <summary>
        /// Gets the excess phase base-X capacitor current.
        /// </summary>
        /// <value>
        /// The excess phase base-X capacitor current.
        /// </value>
        [ParameterName("cexbc"), ParameterInfo("Total capacitance in B-X junction")]
        public double CurrentExBc => _biasingStateExcessPhaseCurrentBc.Value;

        /// <inheritdoc/>
        public sealed override double ChargeBe
        {
            get => _biasingStateChargeBe.Value;
            protected set => _biasingStateChargeBe.Value = value;
        }

        /// <inheritdoc/>
        public sealed override double ChargeBc
        {
            get => _biasingStateChargeBc.Value;
            protected set => _biasingStateChargeBc.Value = value;
        }

        /// <inheritdoc/>
        public sealed override double ChargeCs
        {
            get => _biasingStateChargeCs.Value;
            protected set => _biasingStateChargeCs.Value = value;
        }

        /// <inheritdoc/>
        public sealed override double ChargeBx
        {
            get => _biasingStateChargeBx.Value;
            protected set => _biasingStateChargeBx.Value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Time"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
        public Time(IComponentBindingContext context)
            : base(context)
        {
            _time = context.GetState<ITimeSimulationState>();

            _baseNode = BiasingState.Map[BiasingState.GetSharedVariable(context.Nodes[1])];
            _substrateNode = BiasingState.Map[BiasingState.GetSharedVariable(context.Nodes[3])];
            _collectorPrimeNode = BiasingState.Map[CollectorPrime];
            _basePrimeNode = BiasingState.Map[BasePrime];
            _emitterPrimeNode = BiasingState.Map[EmitterPrime];

            _elements = new ElementSet<double>(BiasingState.Solver, [
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
            ], [_baseNode, _substrateNode, _collectorPrimeNode, _basePrimeNode, _emitterPrimeNode]);

            _method = context.GetState<IIntegrationMethod>();
            _biasingStateChargeBe = _method.CreateDerivative();
            _biasingStateChargeBc = _method.CreateDerivative();
            _biasingStateChargeCs = _method.CreateDerivative();
            _biasingStateChargeBx = _method.CreateDerivative(false); // Spice 3f5 does not include this state for LTE calculations
            _biasingStateExcessPhaseCurrentBc = new StateValue<double>(3);
            _method.RegisterState(_biasingStateExcessPhaseCurrentBc);
        }

        /// <inheritdoc/>
        void ITimeBehavior.InitializeStates()
        {
            // Calculate capacitances
            double vbe, vbc, vbx, vcs;
            if (_time.UseIc)
            {
                vbe = Parameters.InitialVoltageBe.Given ?
                    Parameters.InitialVoltageBe.Value :
                    ModelParameters.BipolarType * (BiasingState.Solution[_basePrimeNode] - BiasingState.Solution[_collectorPrimeNode]);
                double vce = Parameters.InitialVoltageCe.Given ?
                    Parameters.InitialVoltageCe.Value :
                    ModelParameters.BipolarType * (BiasingState.Solution[_collectorPrimeNode] - BiasingState.Solution[_emitterPrimeNode]);
                vbc = vbe - vce;
                vbx = vbc;
                vcs = 0;
            }
            else
            {
                vbe = VoltageBe;
                vbc = VoltageBc;
                vbx = ModelParameters.BipolarType * (BiasingState.Solution[_baseNode] - BiasingState.Solution[_collectorPrimeNode]);
                vcs = ModelParameters.BipolarType * (BiasingState.Solution[_substrateNode] - BiasingState.Solution[_collectorPrimeNode]);
            }
            CalculateCapacitances(vbe, vbc, vbx, vcs);
            _biasingStateExcessPhaseCurrentBc.Value = CapCurrentBe / BaseCharge;
        }

        /// <inheritdoc/>
        protected override void Load()
        {
            base.Load();
            if (_time.UseDc)
                return;
            var state = BiasingState;
            double gpi = 0.0;
            double gmu = 0.0;
            double cb = 0.0;
            double cc = 0.0;

            double vbe = VoltageBe;
            double vbc = VoltageBc;
            double vbx = ModelParameters.BipolarType * (state.Solution[_baseNode] - state.Solution[_collectorPrimeNode]);
            double vcs = ModelParameters.BipolarType * (state.Solution[_substrateNode] - state.Solution[_collectorPrimeNode]);
            CalculateCapacitances(vbe, vbc, vbx, vcs);

            _biasingStateChargeBe.Derive();
            var info = _biasingStateChargeBe.GetContributions(Geqcb);
            double geqcb = info.Jacobian; // Multiplies geqcb with method.Slope (ag[0])
            gpi += _biasingStateChargeBe.GetContributions(CapBe).Jacobian;
            cb += _biasingStateChargeBe.Derivative;
            _biasingStateChargeBc.Derive();
            gmu += _biasingStateChargeBc.GetContributions(CapBc).Jacobian;
            cb += _biasingStateChargeBc.Derivative;
            cc -= _biasingStateChargeBc.Derivative;

            // Charge storage for c-s and b-x junctions
            _biasingStateChargeCs.Derive();
            double gccs = _biasingStateChargeCs.GetContributions(CapCs).Jacobian;
            _biasingStateChargeBx.Derive();
            double geqbx = _biasingStateChargeBx.GetContributions(CapBx).Jacobian;

            // Load current excitation vector
            double ceqcs = ModelParameters.BipolarType * (_biasingStateChargeCs.Derivative - vcs * gccs);
            double ceqbx = ModelParameters.BipolarType * (_biasingStateChargeBx.Derivative - vbx * geqbx);
            double ceqbe = ModelParameters.BipolarType * (cc + cb - vbe * gpi + vbc * -geqcb);
            double ceqbc = ModelParameters.BipolarType * (-cc + -vbc * gmu);

            double m = Parameters.ParallelMultiplier;
            _elements.Add(
                // Y-matrix
                geqbx * m,
                (gmu + gccs + geqbx) * m,
                (gpi + gmu + geqcb) * m,
                gpi * m,
                -gmu * m,
                (-gmu - geqcb) * m,
                -gpi * m,
                geqcb * m,
                (-gpi - geqcb) * m,
                gccs * m,
                -gccs * m,
                -gccs * m,
                -geqbx * m,
                -geqbx * m,
                // RHS vector
                -ceqbx * m,
                -ceqcs * m,
                (ceqcs + ceqbx + ceqbc) * m,
                (-ceqbe - ceqbc) * m,
                ceqbe * m);
        }

        /// <inheritdoc/>
        protected override void Initialize(out double vbe, out double vbc)
        {
            if (Iteration.Mode == IterationModes.Junction && _time.UseDc && _time.UseIc)
            {
                vbe = ModelParameters.BipolarType * Parameters.InitialVoltageBe;
                double vce = ModelParameters.BipolarType * Parameters.InitialVoltageCe;
                vbc = vbe - vce;
                return;
            }
            base.Initialize(out vbe, out vbc);
        }

        /// <inheritdoc/>
        protected override void ExcessPhaseCalculation(ref double cc, ref double cex, ref double gex)
        {
            double td = ModelTemperature.ExcessPhaseFactor;
            if (td.Equals(0))
            {
                _biasingStateExcessPhaseCurrentBc.Value = cex;
                return;
            }

            /* 
             * weil's approx. for excess phase applied with backward - 
             * euler integration
             */
            double cbe = cex;
            double gbe = gex;

            double delta = _method.GetPreviousTimestep(0);
            double prevdelta = _method.GetPreviousTimestep(1);
            double arg1 = delta / td;
            double arg2 = 3 * arg1;
            arg1 = arg2 * arg1;
            double denom = 1 + arg1 + arg2;
            double arg3 = arg1 / denom;
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
