using SpiceSharp.Attributes;

namespace SpiceSharp.Components.MosfetBehaviors.Level3
{
    /// <summary>
    /// Base parameters for a <see cref="Mosfet3Model"/>
    /// </summary>
    public class ModelBaseParameters : ParameterSet
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [PropertyName("vto"), PropertyName("vt0"), PropertyInfo("Threshold voltage")]
        public Parameter Vt0 { get; } = new Parameter();
        [PropertyName("kp"), PropertyInfo("Transconductance parameter")]
        public Parameter Transconductance { get; } = new Parameter(2e-5);
        [PropertyName("gamma"), PropertyInfo("Bulk threshold parameter")]
        public Parameter Gamma { get; } = new Parameter();
        [PropertyName("phi"), PropertyInfo("Surface potential")]
        public Parameter Phi { get; } = new Parameter(.6);
        [PropertyName("rd"), PropertyInfo("Drain ohmic resistance")]
        public Parameter DrainResistance { get; } = new Parameter();
        [PropertyName("rs"), PropertyInfo("Source ohmic resistance")]
        public Parameter SourceResistance { get; } = new Parameter();
        [PropertyName("cbd"), PropertyInfo("B-D junction capacitance")]
        public Parameter CapBD { get; } = new Parameter();
        [PropertyName("cbs"), PropertyInfo("B-S junction capacitance")]
        public Parameter CapBS { get; } = new Parameter();
        [PropertyName("is"), PropertyInfo("Bulk junction sat. current")]
        public Parameter JctSatCur { get; } = new Parameter(1e-14);
        [PropertyName("pb"), PropertyInfo("Bulk junction potential")]
        public Parameter BulkJctPotential { get; } = new Parameter(.8);
        [PropertyName("cgso"), PropertyInfo("Gate-source overlap cap.")]
        public Parameter GateSourceOverlapCapFactor { get; } = new Parameter();
        [PropertyName("cgdo"), PropertyInfo("Gate-drain overlap cap.")]
        public Parameter GateDrainOverlapCapFactor { get; } = new Parameter();
        [PropertyName("cgbo"), PropertyInfo("Gate-bulk overlap cap.")]
        public Parameter GateBulkOverlapCapFactor { get; } = new Parameter();
        [PropertyName("rsh"), PropertyInfo("Sheet resistance")]
        public Parameter SheetResistance { get; } = new Parameter();
        [PropertyName("cj"), PropertyInfo("Bottom junction cap per area")]
        public Parameter BulkCapFactor { get; } = new Parameter();
        [PropertyName("mj"), PropertyInfo("Bottom grading coefficient")]
        public Parameter BulkJctBotGradingCoefficient { get; } = new Parameter(.5);
        [PropertyName("cjsw"), PropertyInfo("Side junction cap per area")]
        public Parameter SidewallCapFactor { get; } = new Parameter();
        [PropertyName("mjsw"), PropertyInfo("Side grading coefficient")]
        public Parameter BulkJctSideGradingCoefficient { get; } = new Parameter(.33);
        [PropertyName("js"), PropertyInfo("Bulk jct. sat. current density")]
        public Parameter JctSatCurDensity { get; } = new Parameter();
        [PropertyName("tox"), PropertyInfo("Oxide thickness")]
        public Parameter OxideThickness { get; } = new Parameter(1e-7);
        [PropertyName("ld"), PropertyInfo("Lateral diffusion")]
        public Parameter LatDiff { get; } = new Parameter();
        [PropertyName("u0"), PropertyName("uo"), PropertyInfo("Surface mobility")]
        public Parameter SurfaceMobility { get; } = new Parameter();
        [PropertyName("fc"), PropertyInfo("Forward bias jct. fit parm.")]
        public Parameter ForwardCapDepCoefficient { get; } = new Parameter(.5);
        [PropertyName("nsub"), PropertyInfo("Substrate doping")]
        public Parameter SubstrateDoping { get; } = new Parameter();
        [PropertyName("tpg"), PropertyInfo("Gate type")]
        public Parameter GateType { get; } = new Parameter();
        [PropertyName("nss"), PropertyInfo("Surface state density")]
        public Parameter SurfaceStateDensity { get; } = new Parameter();
        [PropertyName("eta"), PropertyInfo("Vds dependence of threshold voltage")]
        public Parameter Eta { get; } = new Parameter();
        [PropertyName("nfs"), PropertyInfo("Fast surface state density")]
        public Parameter FastSurfaceStateDensity { get; } = new Parameter();
        [PropertyName("theta"), PropertyInfo("Vgs dependence on mobility")]
        public Parameter Theta { get; } = new Parameter();
        [PropertyName("vmax"), PropertyInfo("Maximum carrier drift velocity")]
        public Parameter MaxDriftVel { get; } = new Parameter();
        [PropertyName("kappa"), PropertyInfo("Kappa")]
        public Parameter Kappa { get; } = new Parameter(.2);
        [PropertyName("xj"), PropertyInfo("Junction depth")]
        public Parameter JunctionDepth { get; } = new Parameter();
        [PropertyName("tnom"), PropertyInfo("Parameter measurement temperature")]
        public double NominalTemperatureCelsius
        {
            get => NominalTemperature - Circuit.CelsiusKelvin;
            set => NominalTemperature.Set(value + Circuit.CelsiusKelvin);
        }
        public Parameter NominalTemperature { get; } = new Parameter();

        /// <summary>
        /// Methods
        /// </summary>
        [PropertyName("delta"), PropertyInfo("Width effect on threshold")]
        public double DeltaWidth
        {
            get => NarrowFactor;
            set => Delta = value;
        }
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

        [PropertyName("input_delta"), PropertyInfo("")]
        public double Delta { get; protected set; }
        public double NarrowFactor { get; set; }
        public double MosfetType { get; internal set; } = 1.0;

        /// <summary>
        /// Constructor
        /// </summary>
        public ModelBaseParameters()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="nmos">True for NMOS, false for PMOS</param>
        public ModelBaseParameters(bool nmos)
        {
            if (nmos)
                MosfetType = 1.0;
            else
                MosfetType = -1.0;
        }
    }
}
