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
        [ParameterName("vto"), ParameterName("vt0"), PropertyInfo("Threshold voltage")]
        public Parameter Vt0 { get; } = new Parameter();
        [ParameterName("kp"), PropertyInfo("Transconductance parameter")]
        public Parameter Transconductance { get; } = new Parameter(2e-5);
        [ParameterName("gamma"), PropertyInfo("Bulk threshold parameter")]
        public Parameter Gamma { get; } = new Parameter();
        [ParameterName("phi"), PropertyInfo("Surface potential")]
        public Parameter Phi { get; } = new Parameter(.6);
        [ParameterName("rd"), PropertyInfo("Drain ohmic resistance")]
        public Parameter DrainResistance { get; } = new Parameter();
        [ParameterName("rs"), PropertyInfo("Source ohmic resistance")]
        public Parameter SourceResistance { get; } = new Parameter();
        [ParameterName("cbd"), PropertyInfo("B-D junction capacitance")]
        public Parameter CapBd { get; } = new Parameter();
        [ParameterName("cbs"), PropertyInfo("B-S junction capacitance")]
        public Parameter CapBs { get; } = new Parameter();
        [ParameterName("is"), PropertyInfo("Bulk junction sat. current")]
        public Parameter JunctionSatCur { get; } = new Parameter(1e-14);
        [ParameterName("pb"), PropertyInfo("Bulk junction potential")]
        public Parameter BulkJunctionPotential { get; } = new Parameter(.8);
        [ParameterName("cgso"), PropertyInfo("Gate-source overlap cap.")]
        public Parameter GateSourceOverlapCapFactor { get; } = new Parameter();
        [ParameterName("cgdo"), PropertyInfo("Gate-drain overlap cap.")]
        public Parameter GateDrainOverlapCapFactor { get; } = new Parameter();
        [ParameterName("cgbo"), PropertyInfo("Gate-bulk overlap cap.")]
        public Parameter GateBulkOverlapCapFactor { get; } = new Parameter();
        [ParameterName("rsh"), PropertyInfo("Sheet resistance")]
        public Parameter SheetResistance { get; } = new Parameter();
        [ParameterName("cj"), PropertyInfo("Bottom junction cap per area")]
        public Parameter BulkCapFactor { get; } = new Parameter();
        [ParameterName("mj"), PropertyInfo("Bottom grading coefficient")]
        public Parameter BulkJunctionBotGradingCoefficient { get; } = new Parameter(.5);
        [ParameterName("cjsw"), PropertyInfo("Side junction cap per area")]
        public Parameter SidewallCapFactor { get; } = new Parameter();
        [ParameterName("mjsw"), PropertyInfo("Side grading coefficient")]
        public Parameter BulkJunctionSideGradingCoefficient { get; } = new Parameter(.33);
        [ParameterName("js"), PropertyInfo("Bulk jct. sat. current density")]
        public Parameter JunctionSatCurDensity { get; } = new Parameter();
        [ParameterName("tox"), PropertyInfo("Oxide thickness")]
        public Parameter OxideThickness { get; } = new Parameter(1e-7);
        [ParameterName("ld"), PropertyInfo("Lateral diffusion")]
        public Parameter LateralDiffusion { get; } = new Parameter();
        [ParameterName("u0"), ParameterName("uo"), PropertyInfo("Surface mobility")]
        public Parameter SurfaceMobility { get; } = new Parameter();
        [ParameterName("fc"), PropertyInfo("Forward bias jct. fit parm.")]
        public Parameter ForwardCapDepletionCoefficient { get; } = new Parameter(.5);
        [ParameterName("nsub"), PropertyInfo("Substrate doping")]
        public Parameter SubstrateDoping { get; } = new Parameter();
        [ParameterName("tpg"), PropertyInfo("Gate type")]
        public Parameter GateType { get; } = new Parameter();
        [ParameterName("nss"), PropertyInfo("Surface state density")]
        public Parameter SurfaceStateDensity { get; } = new Parameter();
        [ParameterName("eta"), PropertyInfo("Vds dependence of threshold voltage")]
        public Parameter Eta { get; } = new Parameter();
        [ParameterName("nfs"), PropertyInfo("Fast surface state density")]
        public Parameter FastSurfaceStateDensity { get; } = new Parameter();
        [ParameterName("theta"), PropertyInfo("Vgs dependence on mobility")]
        public Parameter Theta { get; } = new Parameter();
        [ParameterName("vmax"), PropertyInfo("Maximum carrier drift velocity")]
        public Parameter MaxDriftVelocity { get; } = new Parameter();
        [ParameterName("kappa"), PropertyInfo("Kappa")]
        public Parameter Kappa { get; } = new Parameter(.2);
        [ParameterName("xj"), PropertyInfo("Junction depth")]
        public Parameter JunctionDepth { get; } = new Parameter();
        [ParameterName("tnom"), PropertyInfo("Parameter measurement temperature")]
        public double NominalTemperatureCelsius
        {
            get => NominalTemperature - Circuit.CelsiusKelvin;
            set => NominalTemperature.Set(value + Circuit.CelsiusKelvin);
        }
        public Parameter NominalTemperature { get; } = new Parameter();

        /// <summary>
        /// Methods
        /// </summary>
        [ParameterName("delta"), PropertyInfo("Width effect on threshold")]
        public double DeltaWidth
        {
            get => NarrowFactor;
            set => Delta = value;
        }
        [ParameterName("nmos"), PropertyInfo("N type MOSfet model")]
        public void SetNmos(bool value)
        {
            if (value)
                MosfetType = 1.0;
        }
        [ParameterName("pmos"), PropertyInfo("P type MOSfet model")]
        public void SetPmos(bool value)
        {
            if (value)
                MosfetType = -1.0;
        }
        [ParameterName("type"), PropertyInfo("N-channel or P-channel MOS")]
        public string TypeName
        {
            get
            {
                if (MosfetType > 0)
                    return "nmos";
                return "pmos";
            }
        }

        [ParameterName("input_delta"), PropertyInfo("")]
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
