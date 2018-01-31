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
        [PropertyName("tnom"), PropertyInfo("Parameter measurement temperature")]
        public double NominalTemperatureCelsius
        {
            get => NominalTemperature - Circuit.CelsiusKelvin;
            set => NominalTemperature.Set(value + Circuit.CelsiusKelvin);
        }
        public Parameter NominalTemperature { get; } = new Parameter();
        [PropertyName("vto"), PropertyName("vt0"), PropertyInfo("Threshold voltage")]
        public Parameter VT0 { get; } = new Parameter();
        [PropertyName("kp"), PropertyInfo("Transconductance parameter")]
        public Parameter Transconductance { get; } = new Parameter(2e-5);
        [PropertyName("gamma"), PropertyInfo("Bulk threshold parameter")]
        public Parameter Gamma { get; } = new Parameter();
        [PropertyName("phi"), PropertyInfo("Surface potential")]
        public Parameter Phi { get; } = new Parameter(.6);
        [PropertyName("lambda"), PropertyInfo("Channel length modulation")]
        public Parameter Lambda { get; } = new Parameter();
        [PropertyName("rd"), PropertyInfo("Drain ohmic resistance")]
        public Parameter DrainResistance { get; } = new Parameter();
        [PropertyName("rs"), PropertyInfo("Source ohmic resistance")]
        public Parameter SourceResistance { get; } = new Parameter();
        [PropertyName("cbd"), PropertyInfo("B-D junction capacitance")]
        public Parameter CapBD { get; } = new Parameter();
        [PropertyName("cbs"), PropertyInfo("B-S junction capacitance")]
        public Parameter CapBS { get; } = new Parameter();
        [PropertyName("is"), PropertyInfo("Bulk junction sat. current")]
        public Parameter JunctionSatCur { get; } = new Parameter(1e-14);
        [PropertyName("pb"), PropertyInfo("Bulk junction potential")]
        public Parameter BulkJunctionPotential { get; } = new Parameter(.8);
        [PropertyName("cgso"), PropertyInfo("Gate-source overlap cap.")]
        public Parameter GateSourceOverlapCapFactor { get; } = new Parameter();
        [PropertyName("cgdo"), PropertyInfo("Gate-drain overlap cap.")]
        public Parameter GateDrainOverlapCapFactor { get; } = new Parameter();
        [PropertyName("cgbo"), PropertyInfo("Gate-bulk overlap cap.")]
        public Parameter GateBulkOverlapCapFactor { get; } = new Parameter();
        [PropertyName("cj"), PropertyInfo("Bottom junction cap per area")]
        public Parameter BulkCapFactor { get; } = new Parameter();
        [PropertyName("mj"), PropertyInfo("Bottom grading coefficient")]
        public Parameter BulkJunctionBotGradingCoefficient { get; } = new Parameter(.5);
        [PropertyName("cjsw"), PropertyInfo("Side junction cap per area")]
        public Parameter SidewallCapFactor { get; } = new Parameter();
        [PropertyName("mjsw"), PropertyInfo("Side grading coefficient")]
        public Parameter BulkJunctionSideGradingCoefficient { get; } = new Parameter(.5);
        [PropertyName("js"), PropertyInfo("Bulk jct. sat. current density")]
        public Parameter JunctionSatCurDensity { get; } = new Parameter();
        [PropertyName("tox"), PropertyInfo("Oxide thickness")]
        public Parameter OxideThickness { get; } = new Parameter();
        [PropertyName("ld"), PropertyInfo("Lateral diffusion")]
        public Parameter LateralDiffusion { get; } = new Parameter();
        [PropertyName("rsh"), PropertyInfo("Sheet resistance")]
        public Parameter SheetResistance { get; } = new Parameter();
        [PropertyName("u0"), PropertyName("uo"), PropertyInfo("Surface mobility")]
        public Parameter SurfaceMobility { get; } = new Parameter();
        [PropertyName("fc"), PropertyInfo("Forward bias jct. fit parm.")]
        public Parameter ForwardCapDepletionCoefficient { get; } = new Parameter(.5);
        [PropertyName("nss"), PropertyInfo("Surface state density")]
        public Parameter SurfaceStateDensity { get; } = new Parameter();
        [PropertyName("nsub"), PropertyInfo("Substrate doping")]
        public Parameter SubstrateDoping { get; } = new Parameter();
        [PropertyName("tpg"), PropertyInfo("Gate type")]
        public Parameter GateType { get; } = new Parameter();

        /// <summary>
        /// Methods
        /// </summary>
        [PropertyName("nmos"), PropertyInfo("N type MOSfet model")]
        public void SetNmos(bool value)
        {
            if (value)
                MosfetType = 1.0;
        }
        [PropertyName("pmos"), PropertyInfo("P type MOSfet model")]
        public void SetPmos(bool value)
        {
            if (value)
                MosfetType = -1.0;
        }
        [PropertyName("type"), PropertyInfo("N-channel or P-channel MOS")]
        public string GetTypeName()
        {
            if (MosfetType > 0)
                return "nmos";
            return "pmos";
        }
        public double MosfetType { get; protected set; } = 1.0;

    }
}
