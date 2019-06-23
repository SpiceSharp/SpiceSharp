using System;
using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.IntegrationMethods;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.MosfetBehaviors.Level2
{
    /// <summary>
    /// Transient behavior for a <see cref="Mosfet1" />.
    /// </summary>
    /// <seealso cref="SpiceSharp.Components.MosfetBehaviors.Level2.DynamicParameterBehavior" />
    /// <seealso cref="SpiceSharp.Behaviors.ITimeBehavior" />
    public class TransientBehavior : DynamicParameterBehavior, ITimeBehavior
    {
        /// <summary>
        /// Gets or sets the stored bulk-source charge.
        /// </summary>
        /// <value>
        /// The stored bulk-source charge.
        /// </value>
        public override double ChargeBs
        {
            get => _chargeBs.Current;
            protected set => _chargeBs.Current = value;
        }
        private StateDerivative _chargeBs;

        /// <summary>
        /// Gets or sets the stored bulk-drain charge.
        /// </summary>
        /// <value>
        /// The stored bulk-drain charge.
        /// </value>
        public override double ChargeBd
        {
            get => _chargeBd.Current;
            protected set => _chargeBd.Current = value;
        }
        private StateDerivative _chargeBd;

        /// <summary>
        /// Gets or sets the capacitance due to gate-source charge storage.
        /// </summary>
        /// <value>
        /// The gate-source capacitance.
        /// </value>
        public override double CapGs
        {
            get => _capGs.Current;
            protected set => _capGs.Current = value;
        }
        private StateHistory _capGs;

        /// <summary>
        /// Gets or sets the capacitance due to gate-drain charge storage.
        /// </summary>
        /// <value>
        /// The gate-drain capacitance.
        /// </value>
        public override double CapGd
        {
            get => _capGd.Current;
            protected set => _capGd.Current = value;
        }
        private StateHistory _capGd;

        /// <summary>
        /// Gets or sets the capacitance due to gate-bulk charge storage.
        /// </summary>
        /// <value>
        /// The gate-bulk capacitance.
        /// </value>
        public override double CapGb
        {
            get => _capGb.Current;
            protected set => _capGb.Current = value;
        }
        private StateHistory _capGb;

        /// <summary>
        /// Gets the stored gate-source charge.
        /// </summary>
        /// <value>
        /// The gate-source charge.
        /// </value>
        [ParameterName("qgs"), ParameterName("Gate-Source charge storage")]
        public double ChargeGs => _chargeGs.Current;
        private StateDerivative _chargeGs;

        /// <summary>
        /// Gets the stored gate-drain charge.
        /// </summary>
        /// <value>
        /// The gate-drain charge.
        /// </value>
        [ParameterName("qgd"), ParameterName("Gate-Drain storage")]
        public double ChargeGd => _chargeGd.Current;
        private StateDerivative _chargeGd;

        /// <summary>
        /// Gets the stored gate-bulk charge.
        /// </summary>
        /// <value>
        /// The gate-bulk charge.
        /// </value>
        [ParameterName("qgb"), ParameterInfo("Gate-Bulk charge")]
        public double ChargeGb => _chargeGb.Current;
        private StateDerivative _chargeGb;

        /// <summary>
        /// Gets the drain-source voltage.
        /// </summary>
        /// <value>
        /// The drain-source voltage.
        /// </value>
        public override double VoltageDs
        {
            get => _voltageDs.Current;
            protected set => _voltageDs.Current = value;
        }
        private StateHistory _voltageDs;

        /// <summary>
        /// Gets the gate-source voltage.
        /// </summary>
        /// <value>
        /// The gate-source voltage.
        /// </value>
        public override double VoltageGs
        {
            get => _voltageGs.Current;
            protected set => _voltageGs.Current = value;
        }
        private StateHistory _voltageGs;

        /// <summary>
        /// Gets the bulk-source voltage.
        /// </summary>
        /// <value>
        /// The bulk-source voltage.
        /// </value>
        public override double VoltageBs
        {
            get => _voltageBs.Current;
            protected set => _voltageBs.Current = value;
        }
        private StateHistory _voltageBs;

        /// <summary>
        /// Elements needed for transient behavior
        /// </summary>
        protected VectorElement<double> GatePtr { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransientBehavior"/> class.
        /// </summary>
        /// <param name="name">Name</param>
        public TransientBehavior(string name) : base(name)
        {
        }

        /// <summary>
        /// Creates all necessary states for the transient behavior.
        /// </summary>
        /// <param name="method">The integration method.</param>
        /// <exception cref="NotImplementedException"></exception>
        public void CreateStates(IntegrationMethod method)
        {
            method.ThrowIfNull(nameof(method));
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
        /// Calculates the state values from the current DC solution.
        /// </summary>
        /// <param name="simulation">Time-based simulation</param>
        /// <remarks>
        /// In this method, the initial value is calculated based on the operating point solution,
        /// and the result is stored in each respective <see cref="T:SpiceSharp.IntegrationMethods.StateDerivative" /> or <see cref="T:SpiceSharp.IntegrationMethods.StateHistory" />.
        /// </remarks>
        public void GetDcState(TimeSimulation simulation)
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
        /// Allocate elements in the Y-matrix and Rhs-vector to populate during loading. Additional
        /// equations can also be allocated here.
        /// </summary>
        /// <param name="solver">The solver.</param>
        /// <exception cref="ArgumentNullException">solver</exception>
        public void GetEquationPointers(Solver<double> solver)
        {
            solver.ThrowIfNull(nameof(solver));
            GatePtr = solver.GetRhsElement(GateNode);
        }

        /// <summary>
        /// Perform time-dependent calculations.
        /// </summary>
        /// <param name="simulation">The time-based simulation.</param>
        /// <exception cref="ArgumentNullException">simulation</exception>
        public void Transient(TimeSimulation simulation)
        {
            simulation.ThrowIfNull(nameof(simulation));

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
            GatePtr.Value -= ModelParameters.MosfetType * (ceqgs + ceqgb + ceqgd);
            BulkPtr.Value -= ceqbs + ceqbd - ModelParameters.MosfetType * ceqgb;
            DrainPrimePtr.Value += ceqbd + ModelParameters.MosfetType * ceqgd;
            SourcePrimePtr.Value += ceqbs + ModelParameters.MosfetType * ceqgs;

            // Load Y-matrix
            GateGatePtr.Value += gcgd + gcgs + gcgb;
            BulkBulkPtr.Value += gbd + gbs + gcgb;
            DrainPrimeDrainPrimePtr.Value += gbd + gcgd;
            SourcePrimeSourcePrimePtr.Value += gbs + gcgs;
            GateBulkPtr.Value -= gcgb;
            GateDrainPrimePtr.Value -= gcgd;
            GateSourcePrimePtr.Value -= gcgs;
            BulkGatePtr.Value -= gcgb;
            BulkDrainPrimePtr.Value -= gbd;
            BulkSourcePrimePtr.Value -= gbs;
            DrainPrimeGatePtr.Value -= gcgd;
            DrainPrimeBulkPtr.Value -= gbd;
            SourcePrimeGatePtr.Value -= gcgs;
            SourcePrimeBulkPtr.Value -= gbs;
        }
    }
}
