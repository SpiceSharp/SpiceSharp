using SpiceSharp.ParameterSets;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Simulations.IntegrationMethods;

namespace SpiceSharp.Components.Mosfets.Level2
{
    /// <summary>
    /// Transient behavior for a <see cref="Mosfet1" />.
    /// </summary>
    /// <seealso cref="Biasing"/>
    /// <seealso cref="ITimeBehavior"/>
    public class Time : Biasing,
        ITimeBehavior
    {
        private readonly ITimeSimulationState _time;
        private readonly IDerivative _qbs, _qbd, _qgs, _qgd, _qgb;
        private readonly StateValue<double> _cgs, _cgd, _cgb, _vgs, _vbs, _vds;
        private readonly MosfetCharges _charges = new MosfetCharges();

        /// <include file='../common/docs.xml' path='docs/members/GateSourceCharge/*'/>
        [ParameterName("qgs"), ParameterInfo("Gate-source charge storage", Units = "C")]
        public double Qgs => _qgs.Value;

        /// <include file='../common/docs.xml' path='docs/members/GateSourceCapacitance/*'/>
        [ParameterName("cgs"), ParameterInfo("Gate-source capacitance", Units = "F")]
        public double Cgs => _charges.Cgs;

        /// <include file='../common/docs.xml' path='docs/members/GateDrainCharges/*'/>
        [ParameterName("qgd"), ParameterInfo("Gate-drain charge storage", Units = "C")]
        public double Qgd => _qgd.Value;

        /// <include file='../common/docs.xml' path='docs/members/GateDrainCapacitance/*'/>
        [ParameterName("cgd"), ParameterInfo("Gate-drain capacitance", Units = "F")]
        public double Cgd => _charges.Cgd;

        /// <include file='../common/docs.xml' path='docs/members/GateBulkCharge/*'/>
        [ParameterName("qgb"), ParameterInfo("Gate-bulk charge storage", Units = "C")]
        public double Qgb => _qgb.Value;

        /// <include file='../common/docs.xml' path='docs/members/GateBulkCapacitance/*'/>
        [ParameterName("cgb"), ParameterInfo("Gate-bulk capacitance", Units = "F")]
        public double Cgb => _charges.Cgb;

        /// <include file='../common/docs.xml' path='docs/members/BulkSourceCharge/*'/>
        [ParameterName("qbs"), ParameterInfo("Bulk-source charge storage", Units = "C")]
        public double Qbs => _charges.Qbs;

        /// <include file='../common/docs.xml' path='docs/members/BulkSourceCapacitance/*'/>
        [ParameterName("cbs"), ParameterInfo("Bulk-source capacitance", Units = "F")]
        public double Cbs => _charges.Cbs;

        /// <include file='../common/docs.xml' path='docs/members/BulkDrainCharge/*'/>
        [ParameterName("qbd"), ParameterInfo("Bulk-drain charge storage", Units = "C")]
        public double Qbd => _charges.Qbd;

        /// <include file='../common/docs.xml' path='docs/members/BulkDrainCapacitance/*'/>
        [ParameterName("cbd"), ParameterInfo("Bulk-drain capacitance", Units = "F")]
        public double Cbd => _charges.Cbd;

        /// <summary>
        /// Initializes a new instance of the <see cref="Time"/> class.
        /// </summary>
        /// <param name="name">The name of the behavior.</param>
        /// <param name="context">The binding context.</param>
        public Time(string name, ComponentBindingContext context)
            : base(name, context)
        {
            _time = context.GetState<ITimeSimulationState>();
            var method = context.GetState<IIntegrationMethod>();
            _vgs = new StateValue<double>(2); method.RegisterState(_vgs);
            _vds = new StateValue<double>(2); method.RegisterState(_vds);
            _vbs = new StateValue<double>(2); method.RegisterState(_vbs);
            _cgs = new StateValue<double>(2); method.RegisterState(_cgs);
            _cgd = new StateValue<double>(2); method.RegisterState(_cgd);
            _cgb = new StateValue<double>(2); method.RegisterState(_cgb);
            _qgs = method.CreateDerivative();
            _qgd = method.CreateDerivative();
            _qgb = method.CreateDerivative();
            _qbd = method.CreateDerivative();
            _qbs = method.CreateDerivative();
        }

        void ITimeBehavior.InitializeStates()
        {
            var vgs = Vgs;
            var vds = Vds;
            var vbs = Vbs;
            var vgd = vgs - vds;
            var vgb = vgs - vbs;

            _charges.Update(Mode, vgs, vds, vbs,
                ModelParameters.MosfetType * Von,
                ModelParameters.MosfetType * Vdsat,
                ModelParameters,
                Properties);

            var gateSourceOverlapCap = ModelParameters.GateSourceOverlapCapFactor * Parameters.Width;
            var gateDrainOverlapCap = ModelParameters.GateDrainOverlapCapFactor * Parameters.Width;
            var gateBulkOverlapCap = ModelParameters.GateBulkOverlapCapFactor * Properties.EffectiveLength;

            var capgs = 2 * _charges.Cgs + gateSourceOverlapCap;
            var capgd = 2 * _charges.Cgs + gateDrainOverlapCap;
            var capgb = 2 * _charges.Cgb + gateBulkOverlapCap;

            _qgs.Value = capgs * vgs;
            _qgd.Value = capgd * vgd;
            _qgb.Value = capgb * vgb;
            _vgs.Value = vgs;
            _vds.Value = vds;
            _vbs.Value = vbs;
        }

        /// <inheritdoc/>
        protected override void UpdateTime(double vgs, double vds, double vbs, ref Contributions<double> c)
        {
            if (_time.UseDc)
                return;
            var vgd = vgs - vds;
            var vgb = vgs - vbs;

            // Update the charges and capacitances
            _charges.Update(Mode, vgs, vds, vbs,
                ModelParameters.MosfetType * Von,
                ModelParameters.MosfetType * Vdsat,
                ModelParameters,
                Properties);

            // Bulk junction capacitances
            _qbd.Value = _charges.Qbd;
            _qbd.Integrate();
            c.Bd.G += _qbd.GetContributions(_charges.Cbd).Jacobian;
            c.Bd.C += _qbd.Derivative;
            c.Ds.C -= _qbd.Derivative;

            _qbs.Value = _charges.Qbs;
            _qbs.Integrate();
            c.Bs.G += _qbs.GetContributions(_charges.Cbs).Jacobian;
            c.Bs.C += _qbs.Derivative;

            // Gate capacitances
            var gateSourceOverlapCap = ModelParameters.GateSourceOverlapCapFactor * Parameters.Width;
            var gateDrainOverlapCap = ModelParameters.GateDrainOverlapCapFactor * Parameters.Width;
            var gateBulkOverlapCap = ModelParameters.GateBulkOverlapCapFactor * Properties.EffectiveLength;

            var vgs1 = _vgs.GetPreviousValue(1);
            var vgd1 = vgs1 - _vds.GetPreviousValue(1);
            var vgb1 = vgs1 - _vbs.GetPreviousValue(1);
            _cgs.Value = _charges.Cgs;
            var capgs = _cgs.GetPreviousValue(0) + _cgs.GetPreviousValue(1) + gateSourceOverlapCap;
            _cgd.Value = _charges.Cgd;
            var capgd = _cgd.GetPreviousValue(0) + _cgd.GetPreviousValue(1) + gateDrainOverlapCap;
            _cgb.Value = _charges.Cgb;
            var capgb = _cgb.GetPreviousValue(0) + _cgb.GetPreviousValue(1) + gateBulkOverlapCap;
            _vgs.Value = vgs;
            _vds.Value = vds;
            _vbs.Value = vbs;

            _qgs.Value = (vgs - vgs1) * capgs + _qgs.GetPreviousValue(1);
            _qgd.Value = (vgd - vgd1) * capgd + _qgd.GetPreviousValue(1);
            _qgb.Value = (vgb1 - vgb1) * capgb + _qgb.GetPreviousValue(1);

            _qgs.Integrate();
            var info = _qgs.GetContributions(capgs, vgs);
            c.Gs.G += info.Jacobian;
            c.Gs.C += info.Rhs;

            _qgd.Integrate();
            info = _qgd.GetContributions(capgd, vgd);
            c.Gd.G += info.Jacobian;
            c.Gd.C += info.Rhs;

            _qgb.Integrate();
            info = _qgb.GetContributions(capgb, vgb);
            c.Gb.G += info.Jacobian;
            c.Gb.C += info.Rhs;
        }
    }
}
