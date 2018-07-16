using System;
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
        public GivenParameter Vt0 { get; } = new GivenParameter();
        [ParameterName("kp"), ParameterInfo("Transconductance parameter")]
        public GivenParameter Transconductance { get; } = new GivenParameter(2e-5);
        [ParameterName("gamma"), ParameterInfo("Bulk threshold parameter")]
        public GivenParameter Gamma { get; } = new GivenParameter();
        [ParameterName("phi"), ParameterInfo("Surface potential")]
        public GivenParameter Phi { get; } = new GivenParameter(.6);
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
        [ParameterName("rsh"), ParameterInfo("Sheet resistance")]
        public GivenParameter SheetResistance { get; } = new GivenParameter();
        [ParameterName("cj"), ParameterInfo("Bottom junction cap per area")]
        public GivenParameter BulkCapFactor { get; } = new GivenParameter();
        [ParameterName("mj"), ParameterInfo("Bottom grading coefficient")]
        public GivenParameter BulkJunctionBotGradingCoefficient { get; } = new GivenParameter(.5);
        [ParameterName("cjsw"), ParameterInfo("Side junction cap per area")]
        public GivenParameter SidewallCapFactor { get; } = new GivenParameter();
        [ParameterName("mjsw"), ParameterInfo("Side grading coefficient")]
        public GivenParameter BulkJunctionSideGradingCoefficient { get; } = new GivenParameter(.33);
        [ParameterName("js"), ParameterInfo("Bulk jct. sat. current density")]
        public GivenParameter JunctionSatCurDensity { get; } = new GivenParameter();
        [ParameterName("tox"), ParameterInfo("Oxide thickness")]
        public GivenParameter OxideThickness { get; } = new GivenParameter(1e-7);
        [ParameterName("ld"), ParameterInfo("Lateral diffusion")]
        public GivenParameter LateralDiffusion { get; } = new GivenParameter();
        [ParameterName("u0"), ParameterName("uo"), ParameterInfo("Surface mobility")]
        public GivenParameter SurfaceMobility { get; } = new GivenParameter(600);
        [ParameterName("fc"), ParameterInfo("Forward bias jct. fit parm.")]
        public GivenParameter ForwardCapDepletionCoefficient { get; } = new GivenParameter(.5);
        [ParameterName("nsub"), ParameterInfo("Substrate doping")]
        public GivenParameter SubstrateDoping { get; } = new GivenParameter();
        [ParameterName("tpg"), ParameterInfo("Gate type")]
        public GivenParameter GateType { get; } = new GivenParameter();
        [ParameterName("nss"), ParameterInfo("Surface state density")]
        public GivenParameter SurfaceStateDensity { get; } = new GivenParameter();
        [ParameterName("eta"), ParameterInfo("Vds dependence of threshold voltage")]
        public GivenParameter Eta { get; } = new GivenParameter();
        [ParameterName("nfs"), ParameterInfo("Fast surface state density")]
        public GivenParameter FastSurfaceStateDensity { get; } = new GivenParameter();
        [ParameterName("theta"), ParameterInfo("Vgs dependence on mobility")]
        public GivenParameter Theta { get; } = new GivenParameter();
        [ParameterName("vmax"), ParameterInfo("Maximum carrier drift velocity")]
        public GivenParameter MaxDriftVelocity { get; } = new GivenParameter();
        [ParameterName("kappa"), ParameterInfo("Kappa")]
        public GivenParameter Kappa { get; } = new GivenParameter(.2);
        [ParameterName("xj"), ParameterInfo("Junction depth")]
        public GivenParameter JunctionDepth { get; } = new GivenParameter();
        [ParameterName("tnom"), DerivedProperty(), ParameterInfo("Parameter measurement temperature")]
        public double NominalTemperatureCelsius
        {
            get => NominalTemperature - Circuit.CelsiusKelvin;
            set => NominalTemperature.Value = value + Circuit.CelsiusKelvin;
        }
        public GivenParameter NominalTemperature { get; } = new GivenParameter(Circuit.ReferenceTemperature);

        /// <summary>
        /// Methods
        /// </summary>
        [ParameterName("delta"), ParameterInfo("Width effect on threshold")]
        public double DeltaWidth
        {
            get => NarrowFactor;
            set => Delta = value;
        }
        [ParameterName("nmos"), ParameterInfo("N type Mosfet model")]
        public void SetNmos(bool value)
        {
            if (value)
                MosfetType = 1.0;
        }
        [ParameterName("pmos"), ParameterInfo("P type Mosfet model")]
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
        public double MosfetType { get; protected set; } = 1.0;
        public double OxideCapFactor { get; private set; }

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

        /// <summary>
        /// Calculate dependent parameters
        /// </summary>
        public override void CalculateDefaults()
        {
            OxideCapFactor = 3.9 * 8.854214871e-12 / OxideThickness;
            if (!Transconductance.Given)
                Transconductance.RawValue = SurfaceMobility * OxideCapFactor * 1e-4;

            // now model parameter preprocessing
            NarrowFactor = Delta * 0.5 * Math.PI * Transistor.EpsilonSilicon / OxideCapFactor;
        }
    }
}
