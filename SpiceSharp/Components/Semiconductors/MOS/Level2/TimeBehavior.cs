using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;
using SpiceSharp.Simulations.IntegrationMethods;

namespace SpiceSharp.Components.MosfetBehaviors.Level2
{
    /// <summary>
    /// Transient behavior for a <see cref="Mosfet1" />.
    /// </summary>
    /// <seealso cref="DynamicParameterBehavior" />
    /// <seealso cref="ITimeBehavior" />
    public class TimeBehavior : DynamicParameterBehavior, ITimeBehavior
    {
        private readonly int _gateNode, _bulkNode, _drainNodePrime, _sourceNodePrime;
        private readonly ITimeSimulationState _time;
        private readonly ElementSet<double> _elements;
        private readonly IDerivative _chargeBs, _chargeBd, _chargeGs, _chargeGd, _chargeGb;
        private readonly StateValue<double> _capGs, _capGd, _capGb, _voltageGs, _voltageDs, _voltageBs;

        /// <summary>
        /// Gets or sets the stored bulk-source charge.
        /// </summary>
        public override double ChargeBs
        {
            get => _chargeBs.Value;
            protected set => _chargeBs.Value = value;
        }

        /// <summary>
        /// Gets or sets the stored bulk-drain charge.
        /// </summary>
        public override double ChargeBd
        {
            get => _chargeBd.Value;
            protected set => _chargeBd.Value = value;
        }

        /// <summary>
        /// Gets or sets the capacitance due to gate-source charge storage.
        /// </summary>
        public override double CapGs
        {
            get => _capGs.Value;
            protected set => _capGs.Value = value;
        }

        /// <summary>
        /// Gets or sets the capacitance due to gate-drain charge storage.
        /// </summary>
        public override double CapGd
        {
            get => _capGd.Value;
            protected set => _capGd.Value = value;
        }

        /// <summary>
        /// Gets or sets the capacitance due to gate-bulk charge storage.
        /// </summary>
        public override double CapGb
        {
            get => _capGb.Value;
            protected set => _capGb.Value = value;
        }

        /// <summary>
        /// Gets the stored gate-source charge.
        /// </summary>
        [ParameterName("qgs"), ParameterName("Gate-Source charge storage")]
        public double ChargeGs => _chargeGs.Value;

        /// <summary>
        /// Gets the stored gate-drain charge.
        /// </summary>
        [ParameterName("qgd"), ParameterName("Gate-Drain storage")]
        public double ChargeGd => _chargeGd.Value;

        /// <summary>
        /// Gets the stored gate-bulk charge.
        /// </summary>
        [ParameterName("qgb"), ParameterInfo("Gate-Bulk charge")]
        public double ChargeGb => _chargeGb.Value;

        /// <summary>
        /// Gets the drain-source voltage.
        /// </summary>
        public override double VoltageDs
        {
            get => _voltageDs.Value;
            protected set => _voltageDs.Value = value;
        }

        /// <summary>
        /// Gets the gate-source voltage.
        /// </summary>
        public override double VoltageGs
        {
            get => _voltageGs.Value;
            protected set => _voltageGs.Value = value;
        }

        /// <summary>
        /// Gets the bulk-source voltage.
        /// </summary>
        public override double VoltageBs
        {
            get => _voltageBs.Value;
            protected set => _voltageBs.Value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public TimeBehavior(string name, ComponentBindingContext context) : base(name, context)
        {
            _time = context.GetState<ITimeSimulationState>();

            _gateNode = BiasingState.Map[BiasingState.MapNode(context.Nodes[1])];
            _bulkNode = BiasingState.Map[BiasingState.MapNode(context.Nodes[3])];
            _drainNodePrime = BiasingState.Map[DrainPrime];
            _sourceNodePrime = BiasingState.Map[SourcePrime];
            
            _elements = new ElementSet<double>(BiasingState.Solver, new[] {
                new MatrixLocation(_gateNode, _gateNode),
                new MatrixLocation(_bulkNode, _bulkNode),
                new MatrixLocation(_drainNodePrime, _drainNodePrime),
                new MatrixLocation(_sourceNodePrime, _sourceNodePrime),
                new MatrixLocation(_gateNode, _bulkNode),
                new MatrixLocation(_gateNode, _drainNodePrime),
                new MatrixLocation(_gateNode, _sourceNodePrime),
                new MatrixLocation(_bulkNode, _gateNode),
                new MatrixLocation(_bulkNode, _drainNodePrime),
                new MatrixLocation(_bulkNode, _sourceNodePrime),
                new MatrixLocation(_drainNodePrime, _gateNode),
                new MatrixLocation(_drainNodePrime, _bulkNode),
                new MatrixLocation(_sourceNodePrime, _gateNode),
                new MatrixLocation(_sourceNodePrime, _bulkNode)
            }, new[] { _gateNode, _bulkNode, _drainNodePrime, _sourceNodePrime });

            var method = context.GetState<IIntegrationMethod>();
            _voltageGs = new StateValue<double>(2); method.RegisterState(_voltageGs);
            _voltageDs = new StateValue<double>(2); method.RegisterState(_voltageDs);
            _voltageBs = new StateValue<double>(2); method.RegisterState(_voltageBs);
            _capGs = new StateValue<double>(2); method.RegisterState(_capGs);
            _capGd = new StateValue<double>(2); method.RegisterState(_capGd);
            _capGb = new StateValue<double>(2); method.RegisterState(_capGb);
            _chargeGs = method.CreateDerivative();
            _chargeGd = method.CreateDerivative();
            _chargeGb = method.CreateDerivative();
            _chargeBd = method.CreateDerivative();
            _chargeBs = method.CreateDerivative();
        }

        /// <summary>
        /// Calculates the state values from the current DC solution.
        /// </summary>
        /// <remarks>
        /// In this method, the initial value is calculated based on the operating point solution,
        /// and the result is stored in each respective <see cref="IDerivative" /> or <see cref="StateValue{T}" />.
        /// </remarks>
        void ITimeBehavior.InitializeStates()
        {
            var vgs = VoltageGs;
            var vds = VoltageDs;
            var vbs = VoltageBs;
            var vgd = vgs - vds;
            var vgb = vgs - vbs;

            CalculateBaseCapacitances();
            CalculateCapacitances(vgs, vds, vbs);

            // Calculate Meyer capacitance
            CalculateMeyerCharges(vgs, vgd);

            var gateSourceOverlapCap = ModelParameters.GateSourceOverlapCapFactor * Parameters.Width;
            var gateDrainOverlapCap = ModelParameters.GateDrainOverlapCapFactor * Parameters.Width;
            var gateBulkOverlapCap = ModelParameters.GateBulkOverlapCapFactor * EffectiveLength;

            var capgs = 2 * CapGs + gateSourceOverlapCap;
            var capgd = 2 * CapGd + gateDrainOverlapCap;
            var capgb = 2 * CapGb + gateBulkOverlapCap;

            _chargeGs.Value = capgs * vgs;
            _chargeGd.Value = capgd * vgd;
            _chargeGb.Value = capgb * vgb;
            _voltageGs.Value = vgs;
            _voltageDs.Value = vds;
            _voltageBs.Value = vbs;
        }

        /// <summary>
        /// Perform time-dependent calculations.
        /// </summary>
        protected override void Load()
        {
            base.Load();
            if (_time.UseDc)
                return;
            var vbd = VoltageBd;
            var vbs = VoltageBs;
            var vgs = VoltageGs;
            var vds = VoltageDs;
            var vgd = vgs - vds;
            var vgb = vgs - vbs;

            CalculateCapacitances(vgs, vds, vbs);

            _chargeBd.Integrate();
            var gbd = _chargeBd.GetContributions(CapBd).Jacobian;
            var cbd = _chargeBd.Derivative;
            _chargeBs.Integrate();
            var gbs = _chargeBs.GetContributions(CapBs).Jacobian;
            var cbs = _chargeBs.Derivative;

            // Calculate Meyer's capacitors
            CalculateMeyerCharges(vgs, vgd);

            var gateSourceOverlapCap = ModelParameters.GateSourceOverlapCapFactor * Parameters.Width;
            var gateDrainOverlapCap = ModelParameters.GateDrainOverlapCapFactor * Parameters.Width;
            var gateBulkOverlapCap = ModelParameters.GateBulkOverlapCapFactor * EffectiveLength;

            var vgs1 = _voltageGs.GetPreviousValue(1);
            var vgd1 = vgs1 - _voltageDs.GetPreviousValue(1);
            var vgb1 = vgs1 - _voltageBs.GetPreviousValue(1);
            var capgs = _capGs.GetPreviousValue(0) + _capGs.GetPreviousValue(1) + gateSourceOverlapCap;
            var capgd = _capGd.GetPreviousValue(0) + _capGd.GetPreviousValue(1) + gateDrainOverlapCap;
            var capgb = _capGb.GetPreviousValue(0) + _capGb.GetPreviousValue(1) + gateBulkOverlapCap;

            _chargeGs.Value = (vgs - vgs1) * capgs + _chargeGs.GetPreviousValue(1);
            _chargeGd.Value = (vgd - vgd1) * capgd + _chargeGd.GetPreviousValue(1);
            _chargeGb.Value = (vgb1 - vgb1) * capgb + _chargeGb.GetPreviousValue(1);

            _chargeGs.Integrate();
            var info = _chargeGs.GetContributions(capgs, vgs);
            var gcgs = info.Jacobian;
            var ceqgs = info.Rhs;
            _chargeGd.Integrate();
            info = _chargeGd.GetContributions(capgd, vgd);
            var gcgd = info.Jacobian;
            var ceqgd = info.Rhs;
            _chargeGb.Integrate();
            info = _chargeGb.GetContributions(capgb, vgb);
            var gcgb = info.Jacobian;
            var ceqgb = info.Rhs;

            // Load current vector
            var ceqbs = ModelParameters.MosfetType * (cbs - gbs * vbs);
            var ceqbd = ModelParameters.MosfetType * (cbd - gbd * vbd);

            _elements.Add(
                // Y-matrix
                gcgd + gcgs + gcgb,
                gbd + gbs + gcgb,
                gbd + gcgd,
                gbs + gcgs,
                -gcgb,
                -gcgd,
                -gcgs,
                -gcgb,
                -gbd,
                -gbs,
                -gcgd,
                -gbd,
                -gcgs,
                -gbs,
                // RHS vector
                -ModelParameters.MosfetType * (ceqgs + ceqgb + ceqgd),
                -(ceqbs + ceqbd - ModelParameters.MosfetType * ceqgb),
                ceqbd + ModelParameters.MosfetType * ceqgd,
                ceqbs + ModelParameters.MosfetType * ceqgs);
        }
    }
}
