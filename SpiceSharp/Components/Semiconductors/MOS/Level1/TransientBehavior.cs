using SpiceSharp.Circuits;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.IntegrationMethods;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;

namespace SpiceSharp.Components.MosfetBehaviors.Level1
{
    /// <summary>
    /// Transient behavior for a <see cref="Mosfet1" />.
    /// </summary>
    public class TransientBehavior : DynamicParameterBehavior, ITimeBehavior
    {
        /// <summary>
        /// Gets or sets the stored bulk-source charge.
        /// </summary>
        public override double ChargeBs
        {
            get => _chargeBs.Current;
            protected set => _chargeBs.Current = value;
        }
        private StateDerivative _chargeBs;

        /// <summary>
        /// Gets or sets the stored bulk-drain charge.
        /// </summary>
        public override double ChargeBd
        {
            get => _chargeBd.Current;
            protected set => _chargeBd.Current = value;
        }
        private StateDerivative _chargeBd;

        /// <summary>
        /// Gets or sets the capacitance due to gate-source charge storage.
        /// </summary>
        public override double CapGs
        {
            get => _capGs.Current;
            protected set => _capGs.Current = value;
        }
        private StateHistory _capGs;

        /// <summary>
        /// Gets or sets the capacitance due to gate-drain charge storage.
        /// </summary>
        public override double CapGd
        {
            get => _capGd.Current;
            protected set => _capGd.Current = value;
        }
        private StateHistory _capGd;

        /// <summary>
        /// Gets or sets the capacitance due to gate-bulk charge storage.
        /// </summary>
        public override double CapGb
        {
            get => _capGb.Current;
            protected set => _capGb.Current = value;
        }
        private StateHistory _capGb;

        /// <summary>
        /// Gets the stored gate-source charge.
        /// </summary>
        [ParameterName("qgs"), ParameterName("Gate-Source charge storage")]
        public double ChargeGs => _chargeGs.Current;
        private StateDerivative _chargeGs;

        /// <summary>
        /// Gets the stored gate-drain charge.
        /// </summary>
        [ParameterName("qgd"), ParameterName("Gate-Drain storage")]
        public double ChargeGd => _chargeGd.Current;
        private StateDerivative _chargeGd;

        /// <summary>
        /// Gets the stored gate-bulk charge.
        /// </summary>
        [ParameterName("qgb"), ParameterInfo("Gate-Bulk charge")]
        public double ChargeGb => _chargeGb.Current;
        private StateDerivative _chargeGb;

        /// <summary>
        /// Gets the drain-source voltage.
        /// </summary>
        public override double VoltageDs
        {
            get => _voltageDs.Current;
            protected set => _voltageDs.Current = value;
        }
        private StateHistory _voltageDs;

        /// <summary>
        /// Gets the gate-source voltage.
        /// </summary>
        public override double VoltageGs
        {
            get => _voltageGs.Current;
            protected set => _voltageGs.Current = value;
        }
        private StateHistory _voltageGs;

        /// <summary>
        /// Gets the bulk-source voltage.
        /// </summary>
        public override double VoltageBs
        {
            get => _voltageBs.Current;
            protected set => _voltageBs.Current = value;
        }
        private StateHistory _voltageBs;

        /// <summary>
        /// Gets the matrix elements.
        /// </summary>
        /// <value>
        /// The matrix elements.
        /// </value>
        protected ElementSet<double> TransientElements { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransientBehavior"/> class.
        /// </summary>
        /// <param name="name">Name</param>
        public TransientBehavior(string name) : base(name)
        {
        }

        /// <summary>
        /// Bind the behavior to a simulation.
        /// </summary>
        /// <param name="context">The binding context.</param>
        public override void Bind(BindingContext context)
        {
            base.Bind(context);

            TransientElements = new ElementSet<double>(BiasingState.Solver, new[] {
                new MatrixLocation(GateNode, GateNode),
                new MatrixLocation(BulkNode, BulkNode),
                new MatrixLocation(DrainNodePrime, DrainNodePrime),
                new MatrixLocation(SourceNodePrime, SourceNodePrime),
                new MatrixLocation(GateNode, BulkNode),
                new MatrixLocation(GateNode, DrainNodePrime),
                new MatrixLocation(GateNode, SourceNodePrime),
                new MatrixLocation(BulkNode, GateNode),
                new MatrixLocation(BulkNode, DrainNodePrime),
                new MatrixLocation(BulkNode, SourceNodePrime),
                new MatrixLocation(DrainNodePrime, GateNode),
                new MatrixLocation(DrainNodePrime, BulkNode),
                new MatrixLocation(SourceNodePrime, GateNode),
                new MatrixLocation(SourceNodePrime, BulkNode)
            }, new[] { GateNode, BulkNode, DrainNodePrime, SourceNodePrime });

            var method = context.States.GetValue<TimeSimulationState>().Method;
            _voltageGs = method.CreateHistory();
            _voltageDs = method.CreateHistory();
            _voltageBs = method.CreateHistory();
            _capGs = method.CreateHistory();
            _capGd = method.CreateHistory();
            _capGb = method.CreateHistory();
            _chargeGs = method.CreateDerivative();
            _chargeGd = method.CreateDerivative();
            _chargeGb = method.CreateDerivative();
            _chargeBd = method.CreateDerivative();
            _chargeBs = method.CreateDerivative();
        }

        /// <summary>
        /// Unbind the behavior.
        /// </summary>
        public override void Unbind()
        {
            base.Unbind();
            TransientElements?.Destroy();
            TransientElements = null;

            _voltageGs = null;
            _voltageDs = null;
            _voltageBs = null;
            _capGs = null;
            _capGd = null;
            _capGb = null;
            _chargeGs = null;
            _chargeGd = null;
            _chargeGb = null;
            _chargeBd = null;
            _chargeBs = null;
        }

        /// <summary>
        /// Calculates the state values from the current DC solution.
        /// </summary>
        /// <remarks>
        /// In this method, the initial value is calculated based on the operating point solution,
        /// and the result is stored in each respective <see cref="SpiceSharp.IntegrationMethods.StateDerivative" /> or <see cref="SpiceSharp.IntegrationMethods.StateHistory" />.
        /// </remarks>
        void ITimeBehavior.InitializeStates()
        {
            BiasingState.ThrowIfNotBound(this);
            var vgs = VoltageGs;
            var vds = VoltageDs;
            var vbs = VoltageBs;
            var vgd = vgs - vds;
            var vgb = vgs - vbs;

            CalculateBaseCapacitances();
            CalculateCapacitances(vgs, vds, vbs);

            // Calculate Meyer capacitance
            CalculateMeyerCharges(vgs, vgd);

            var gateSourceOverlapCap = ModelParameters.GateSourceOverlapCapFactor * BaseParameters.Width;
            var gateDrainOverlapCap = ModelParameters.GateDrainOverlapCapFactor * BaseParameters.Width;
            var gateBulkOverlapCap = ModelParameters.GateBulkOverlapCapFactor * EffectiveLength;

            var capgs = 2 * CapGs + gateSourceOverlapCap;
            var capgd = 2 * CapGd + gateDrainOverlapCap;
            var capgb = 2 * CapGb + gateBulkOverlapCap;

            _chargeGs.Current = capgs * vgs;
            _chargeGd.Current = capgd * vgd;
            _chargeGb.Current = capgb * vgb;
            _voltageGs.Current = vgs;
            _voltageDs.Current = vds;
            _voltageBs.Current = vbs;
        }

        /// <summary>
        /// Perform time-dependent calculations.
        /// </summary>
        void ITimeBehavior.Load()
        {
            BiasingState.ThrowIfNotBound(this);
            var vbd = VoltageBd;
            var vbs = VoltageBs;
            var vgs = VoltageGs;
            var vds = VoltageDs;
            var vgd = vgs - vds;
            var vgb = vgs - vbs;

            CalculateCapacitances(vgs, vds, vbs);
            
            _chargeBd.Integrate();
            var gbd = _chargeBd.Jacobian(CapBd);
            var cbd = _chargeBd.Derivative;
            _chargeBs.Integrate();
            var gbs = _chargeBs.Jacobian(CapBs);
            var cbs = _chargeBs.Derivative;

            // Calculate Meyer's capacitors
            CalculateMeyerCharges(vgs, vgd);

            var gateSourceOverlapCap = ModelParameters.GateSourceOverlapCapFactor * BaseParameters.Width;
            var gateDrainOverlapCap = ModelParameters.GateDrainOverlapCapFactor * BaseParameters.Width;
            var gateBulkOverlapCap = ModelParameters.GateBulkOverlapCapFactor * EffectiveLength;

            var vgs1 = _voltageGs[1];
            var vgd1 = vgs1 - _voltageDs[1];
            var vgb1 = vgs1 - _voltageBs[1];
            var capgs = _capGs[0] + _capGs[1] + gateSourceOverlapCap;
            var capgd = _capGd[0] + _capGd[1] + gateDrainOverlapCap;
            var capgb = _capGb[0] + _capGb[1] + gateBulkOverlapCap;

            _chargeGs.Current = (vgs - vgs1) * capgs + _chargeGs[1];
            _chargeGd.Current = (vgd - vgd1) * capgd + _chargeGd[1];
            _chargeGb.Current = (vgb1 - vgb1) * capgb + _chargeGb[1];

            _chargeGs.Integrate();
            var gcgs = _chargeGs.Jacobian(capgs);
            var ceqgs = _chargeGs.RhsCurrent(gcgs, vgs);
            _chargeGd.Integrate();
            var gcgd = _chargeGd.Jacobian(capgd);
            var ceqgd = _chargeGd.RhsCurrent(gcgd, vgd);
            _chargeGb.Integrate();
            var gcgb = _chargeGb.Jacobian(capgb);
            var ceqgb = _chargeGb.RhsCurrent(gcgb, vgb);

            // Load current vector
            var ceqbs = ModelParameters.MosfetType * (cbs - gbs * vbs);
            var ceqbd = ModelParameters.MosfetType * (cbd - gbd * vbd);

            TransientElements.Add(
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
                ceqbs + ModelParameters.MosfetType * ceqgs
                );
        }
    }
}
