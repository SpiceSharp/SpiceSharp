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
        public GivenParameter<double> NominalTemperature { get; } = new GivenParameter<double>(Circuit.ReferenceTemperature);
        [ParameterName("vto"), ParameterName("vt0"), ParameterInfo("Threshold voltage")]
        public GivenParameter<double> Vt0 { get; } = new GivenParameter<double>();
        [ParameterName("kp"), ParameterInfo("Transconductance parameter")]
        public GivenParameter<double> Transconductance { get; } = new GivenParameter<double>(2e-5);
        [ParameterName("gamma"), ParameterInfo("Bulk threshold parameter")]
        public GivenParameter<double> Gamma { get; } = new GivenParameter<double>();
        [ParameterName("phi"), ParameterInfo("Surface potential")]
        public GivenParameter<double> Phi { get; } = new GivenParameter<double>(.6);
        [ParameterName("lambda"), ParameterInfo("Channel length modulation")]
        public GivenParameter<double> Lambda { get; } = new GivenParameter<double>();
        [ParameterName("rd"), ParameterInfo("Drain ohmic resistance")]
        public GivenParameter<double> DrainResistance { get; } = new GivenParameter<double>();
        [ParameterName("rs"), ParameterInfo("Source ohmic resistance")]
        public GivenParameter<double> SourceResistance { get; } = new GivenParameter<double>();
        [ParameterName("cbd"), ParameterInfo("B-D junction capacitance")]
        public GivenParameter<double> CapBd { get; } = new GivenParameter<double>();
        [ParameterName("cbs"), ParameterInfo("B-S junction capacitance")]
        public GivenParameter<double> CapBs { get; } = new GivenParameter<double>();
        [ParameterName("is"), ParameterInfo("Bulk junction sat. current")]
        public GivenParameter<double> JunctionSatCur { get; } = new GivenParameter<double>(1e-14);
        [ParameterName("pb"), ParameterInfo("Bulk junction potential")]
        public GivenParameter<double> BulkJunctionPotential { get; } = new GivenParameter<double>(.8);
        [ParameterName("cgso"), ParameterInfo("Gate-source overlap cap.")]
        public GivenParameter<double> GateSourceOverlapCapFactor { get; } = new GivenParameter<double>();
        [ParameterName("cgdo"), ParameterInfo("Gate-drain overlap cap.")]
        public GivenParameter<double> GateDrainOverlapCapFactor { get; } = new GivenParameter<double>();
        [ParameterName("cgbo"), ParameterInfo("Gate-bulk overlap cap.")]
        public GivenParameter<double> GateBulkOverlapCapFactor { get; } = new GivenParameter<double>();
        [ParameterName("cj"), ParameterInfo("Bottom junction cap per area")]
        public GivenParameter<double> BulkCapFactor { get; } = new GivenParameter<double>();
        [ParameterName("mj"), ParameterInfo("Bottom grading coefficient")]
        public GivenParameter<double> BulkJunctionBotGradingCoefficient { get; } = new GivenParameter<double>(.5);
        [ParameterName("cjsw"), ParameterInfo("Side junction cap per area")]
        public GivenParameter<double> SidewallCapFactor { get; } = new GivenParameter<double>();
        [ParameterName("mjsw"), ParameterInfo("Side grading coefficient")]
        public GivenParameter<double> BulkJunctionSideGradingCoefficient { get; } = new GivenParameter<double>(.5);
        [ParameterName("js"), ParameterInfo("Bulk jct. sat. current density")]
        public GivenParameter<double> JunctionSatCurDensity { get; } = new GivenParameter<double>();
        [ParameterName("tox"), ParameterInfo("Oxide thickness")]
        public GivenParameter<double> OxideThickness { get; } = new GivenParameter<double>();
        [ParameterName("ld"), ParameterInfo("Lateral diffusion")]
        public GivenParameter<double> LateralDiffusion { get; } = new GivenParameter<double>();
        [ParameterName("rsh"), ParameterInfo("Sheet resistance")]
        public GivenParameter<double> SheetResistance { get; } = new GivenParameter<double>();
        [ParameterName("u0"), ParameterName("uo"), ParameterInfo("Surface mobility")]
        public GivenParameter<double> SurfaceMobility { get; } = new GivenParameter<double>();
        [ParameterName("fc"), ParameterInfo("Forward bias jct. fit parm.")]
        public GivenParameter<double> ForwardCapDepletionCoefficient { get; } = new GivenParameter<double>(.5);
        [ParameterName("nss"), ParameterInfo("Surface state density")]
        public GivenParameter<double> SurfaceStateDensity { get; } = new GivenParameter<double>();
        [ParameterName("nsub"), ParameterInfo("Substrate doping")]
        public GivenParameter<double> SubstrateDoping { get; } = new GivenParameter<double>();
        [ParameterName("tpg"), ParameterInfo("Gate type")]
        public GivenParameter<double> GateType { get; } = new GivenParameter<double>();

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
