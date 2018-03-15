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
        [ParameterName("vto"), ParameterName("vt0"), ParameterInfo("Threshold voltage")]
        public Parameter Vt0 { get; } = new Parameter();
        [ParameterName("kp"), ParameterInfo("Transconductance parameter")]
        public Parameter Transconductance { get; } = new Parameter(2e-5);
        [ParameterName("gamma"), ParameterInfo("Bulk threshold parameter")]
        public Parameter Gamma { get; } = new Parameter();
        [ParameterName("phi"), ParameterInfo("Surface potential")]
        public Parameter Phi { get; } = new Parameter(.6);
        [ParameterName("rd"), ParameterInfo("Drain ohmic resistance")]
        public Parameter DrainResistance { get; } = new Parameter();
        [ParameterName("rs"), ParameterInfo("Source ohmic resistance")]
        public Parameter SourceResistance { get; } = new Parameter();
        [ParameterName("cbd"), ParameterInfo("B-D junction capacitance")]
        public Parameter CapBd { get; } = new Parameter();
        [ParameterName("cbs"), ParameterInfo("B-S junction capacitance")]
        public Parameter CapBs { get; } = new Parameter();
        [ParameterName("is"), ParameterInfo("Bulk junction sat. current")]
        public Parameter JunctionSatCur { get; } = new Parameter(1e-14);
        [ParameterName("pb"), ParameterInfo("Bulk junction potential")]
        public Parameter BulkJunctionPotential { get; } = new Parameter(.8);
        [ParameterName("cgso"), ParameterInfo("Gate-source overlap cap.")]
        public Parameter GateSourceOverlapCapFactor { get; } = new Parameter();
        [ParameterName("cgdo"), ParameterInfo("Gate-drain overlap cap.")]
        public Parameter GateDrainOverlapCapFactor { get; } = new Parameter();
        [ParameterName("cgbo"), ParameterInfo("Gate-bulk overlap cap.")]
        public Parameter GateBulkOverlapCapFactor { get; } = new Parameter();
        [ParameterName("rsh"), ParameterInfo("Sheet resistance")]
        public Parameter SheetResistance { get; } = new Parameter();
        [ParameterName("cj"), ParameterInfo("Bottom junction cap per area")]
        public Parameter BulkCapFactor { get; } = new Parameter();
        [ParameterName("mj"), ParameterInfo("Bottom grading coefficient")]
        public Parameter BulkJunctionBotGradingCoefficient { get; } = new Parameter(.5);
        [ParameterName("cjsw"), ParameterInfo("Side junction cap per area")]
        public Parameter SidewallCapFactor { get; } = new Parameter();
        [ParameterName("mjsw"), ParameterInfo("Side grading coefficient")]
        public Parameter BulkJunctionSideGradingCoefficient { get; } = new Parameter(.33);
        [ParameterName("js"), ParameterInfo("Bulk jct. sat. current density")]
        public Parameter JunctionSatCurDensity { get; } = new Parameter();
        [ParameterName("tox"), ParameterInfo("Oxide thickness")]
        public Parameter OxideThickness { get; } = new Parameter(1e-7);
        [ParameterName("ld"), ParameterInfo("Lateral diffusion")]
        public Parameter LateralDiffusion { get; } = new Parameter();
        [ParameterName("u0"), ParameterName("uo"), ParameterInfo("Surface mobility")]
        public Parameter SurfaceMobility { get; } = new Parameter();
        [ParameterName("fc"), ParameterInfo("Forward bias jct. fit parm.")]
        public Parameter ForwardCapDepletionCoefficient { get; } = new Parameter(.5);
        [ParameterName("nsub"), ParameterInfo("Substrate doping")]
        public Parameter SubstrateDoping { get; } = new Parameter();
        [ParameterName("tpg"), ParameterInfo("Gate type")]
        public Parameter GateType { get; } = new Parameter();
        [ParameterName("nss"), ParameterInfo("Surface state density")]
        public Parameter SurfaceStateDensity { get; } = new Parameter();
        [ParameterName("eta"), ParameterInfo("Vds dependence of threshold voltage")]
        public Parameter Eta { get; } = new Parameter();
        [ParameterName("nfs"), ParameterInfo("Fast surface state density")]
        public Parameter FastSurfaceStateDensity { get; } = new Parameter();
        [ParameterName("theta"), ParameterInfo("Vgs dependence on mobility")]
        public Parameter Theta { get; } = new Parameter();
        [ParameterName("vmax"), ParameterInfo("Maximum carrier drift velocity")]
        public Parameter MaxDriftVelocity { get; } = new Parameter();
        [ParameterName("kappa"), ParameterInfo("Kappa")]
        public Parameter Kappa { get; } = new Parameter(.2);
        [ParameterName("xj"), ParameterInfo("Junction depth")]
        public Parameter JunctionDepth { get; } = new Parameter();
        [ParameterName("tnom"), ParameterInfo("Parameter measurement temperature")]
        public double NominalTemperatureCelsius
        {
            get => NominalTemperature - Circuit.CelsiusKelvin;
            set => NominalTemperature.Set(value + Circuit.CelsiusKelvin);
        }
        public Parameter NominalTemperature { get; } = new Parameter();

        /// <summary>
        /// Methods
        /// </summary>
        [ParameterName("delta"), ParameterInfo("Width effect on threshold")]
        public double DeltaWidth
        {
            get => NarrowFactor;
            set => Delta = value;
        }
        [ParameterName("nmos"), ParameterInfo("N type MOSfet model")]
        public void SetNmos(bool value)
        {
            if (value)
                MosfetType = 1.0;
        }
        [ParameterName("pmos"), ParameterInfo("P type MOSfet model")]
        public void SetPmos(bool value)
        {
            if (value)
                MosfetType = -1.0;
        }
        [ParameterName("type"), ParameterInfo("N-channel or P-channel MOS")]
        public string TypeName
        {
            get
            {
                if (MosfetType > 0)
                    return "nmos";
                return "pmos";
            }
        }

        [ParameterName("input_delta"), ParameterInfo("")]
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
