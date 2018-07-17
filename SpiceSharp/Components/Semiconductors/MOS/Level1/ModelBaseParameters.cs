using SpiceSharp.Attributes;

namespace SpiceSharp.Components.MosfetBehaviors.Level1
{
    /// <summary>
    /// Base parameters for a <see cref="Model"/>
    /// </summary>
    public class ModelBaseParameters : ParameterSet
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [ParameterName("tnom"), DerivedProperty(), ParameterInfo("Parameter measurement temperature")]
        public double NominalTemperatureCelsius
        {
            get => NominalTemperature - Circuit.CelsiusKelvin;
            set => NominalTemperature.Value = value + Circuit.CelsiusKelvin;
        }
        public GivenParameter NominalTemperature { get; } = new GivenParameter(Circuit.ReferenceTemperature);
        [ParameterName("vto"), ParameterName("vt0"), ParameterInfo("Threshold voltage")]
        public GivenParameter Vt0 { get; } = new GivenParameter();
        [ParameterName("kp"), ParameterInfo("Transconductance parameter")]
        public GivenParameter Transconductance { get; } = new GivenParameter(2e-5);
        [ParameterName("gamma"), ParameterInfo("Bulk threshold parameter")]
        public GivenParameter Gamma { get; } = new GivenParameter();
        [ParameterName("phi"), ParameterInfo("Surface potential")]
        public GivenParameter Phi { get; } = new GivenParameter(.6);
        [ParameterName("lambda"), ParameterInfo("Channel length modulation")]
        public GivenParameter Lambda { get; } = new GivenParameter();
        [ParameterName("rd"), ParameterInfo("Drain ohmic resistance")]
        public GivenParameter DrainResistance { get; } = new GivenParameter();
        [ParameterName("rs"), ParameterInfo("Source ohmic resistance")]
        public GivenParameter SourceResistance { get; } = new GivenParameter();
        [ParameterName("cbd"), ParameterInfo("B-D junction capacitance")]
        public GivenParameter CapBd { get; } = new GivenParameter();
        [ParameterName("cbs"), ParameterInfo("B-S junction capacitance")]
        public GivenParameter CapBs { get; } = new GivenParameter();
        [ParameterName("is"), ParameterInfo("Bulk junction sat. current")]
        public GivenParameter JunctionSatCur { get; } = new GivenParameter(1e-14);
        [ParameterName("pb"), ParameterInfo("Bulk junction potential")]
        public GivenParameter BulkJunctionPotential { get; } = new GivenParameter(.8);
        [ParameterName("cgso"), ParameterInfo("Gate-source overlap cap.")]
        public GivenParameter GateSourceOverlapCapFactor { get; } = new GivenParameter();
        [ParameterName("cgdo"), ParameterInfo("Gate-drain overlap cap.")]
        public GivenParameter GateDrainOverlapCapFactor { get; } = new GivenParameter();
        [ParameterName("cgbo"), ParameterInfo("Gate-bulk overlap cap.")]
        public GivenParameter GateBulkOverlapCapFactor { get; } = new GivenParameter();
        [ParameterName("cj"), ParameterInfo("Bottom junction cap per area")]
        public GivenParameter BulkCapFactor { get; } = new GivenParameter();
        [ParameterName("mj"), ParameterInfo("Bottom grading coefficient")]
        public GivenParameter BulkJunctionBotGradingCoefficient { get; } = new GivenParameter(.5);
        [ParameterName("cjsw"), ParameterInfo("Side junction cap per area")]
        public GivenParameter SidewallCapFactor { get; } = new GivenParameter();
        [ParameterName("mjsw"), ParameterInfo("Side grading coefficient")]
        public GivenParameter BulkJunctionSideGradingCoefficient { get; } = new GivenParameter(.5);
        [ParameterName("js"), ParameterInfo("Bulk jct. sat. current density")]
        public GivenParameter JunctionSatCurDensity { get; } = new GivenParameter();
        [ParameterName("tox"), ParameterInfo("Oxide thickness")]
        public GivenParameter OxideThickness { get; } = new GivenParameter();
        [ParameterName("ld"), ParameterInfo("Lateral diffusion")]
        public GivenParameter LateralDiffusion { get; } = new GivenParameter();
        [ParameterName("rsh"), ParameterInfo("Sheet resistance")]
        public GivenParameter SheetResistance { get; } = new GivenParameter();
        [ParameterName("u0"), ParameterName("uo"), ParameterInfo("Surface mobility")]
        public GivenParameter SurfaceMobility { get; } = new GivenParameter();
        [ParameterName("fc"), ParameterInfo("Forward bias jct. fit parm.")]
        public GivenParameter ForwardCapDepletionCoefficient { get; } = new GivenParameter(.5);
        [ParameterName("nss"), ParameterInfo("Surface state density")]
        public GivenParameter SurfaceStateDensity { get; } = new GivenParameter();
        [ParameterName("nsub"), ParameterInfo("Substrate doping")]
        public GivenParameter SubstrateDoping { get; } = new GivenParameter();
        [ParameterName("tpg"), ParameterInfo("Gate type")]
        public GivenParameter GateType { get; } = new GivenParameter();

        /// <summary>
        /// Methods
        /// </summary>
        [ParameterName("nmos"), ParameterInfo("N type mosfet model")]
        public void SetNmos(bool value)
        {
            if (value)
                MosfetType = 1.0;
        }
        [ParameterName("pmos"), ParameterInfo("P type mosfet model")]
        public void SetPmos(bool value)
        {
            if (value)
                MosfetType = -1.0;
        }
        [ParameterName("type"), ParameterInfo("N-channel or P-channel mosfet")]
        public string TypeName
        {
            get
            {
                if (MosfetType > 0)
                    return "nmos";
                return "pmos";
            }
        }
        public double MosfetType { get; protected set; } = 1.0;
    }
}
