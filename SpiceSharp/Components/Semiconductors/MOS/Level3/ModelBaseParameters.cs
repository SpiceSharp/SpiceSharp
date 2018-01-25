using SpiceSharp.Attributes;

namespace SpiceSharp.Components.Mosfet.Level3
{
    /// <summary>
    /// Base parameters for a <see cref="MOS3Model"/>
    /// </summary>
    public class ModelBaseParameters : ParameterSet
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [PropertyName("vto"), PropertyName("vt0"), PropertyInfo("Threshold voltage")]
        public Parameter MOS3vt0 { get; } = new Parameter();
        [PropertyName("kp"), PropertyInfo("Transconductance parameter")]
        public Parameter MOS3transconductance { get; } = new Parameter(2e-5);
        [PropertyName("gamma"), PropertyInfo("Bulk threshold parameter")]
        public Parameter MOS3gamma { get; } = new Parameter();
        [PropertyName("phi"), PropertyInfo("Surface potential")]
        public Parameter MOS3phi { get; } = new Parameter(.6);
        [PropertyName("rd"), PropertyInfo("Drain ohmic resistance")]
        public Parameter MOS3drainResistance { get; } = new Parameter();
        [PropertyName("rs"), PropertyInfo("Source ohmic resistance")]
        public Parameter MOS3sourceResistance { get; } = new Parameter();
        [PropertyName("cbd"), PropertyInfo("B-D junction capacitance")]
        public Parameter MOS3capBD { get; } = new Parameter();
        [PropertyName("cbs"), PropertyInfo("B-S junction capacitance")]
        public Parameter MOS3capBS { get; } = new Parameter();
        [PropertyName("is"), PropertyInfo("Bulk junction sat. current")]
        public Parameter MOS3jctSatCur { get; } = new Parameter(1e-14);
        [PropertyName("pb"), PropertyInfo("Bulk junction potential")]
        public Parameter MOS3bulkJctPotential { get; } = new Parameter(.8);
        [PropertyName("cgso"), PropertyInfo("Gate-source overlap cap.")]
        public Parameter MOS3gateSourceOverlapCapFactor { get; } = new Parameter();
        [PropertyName("cgdo"), PropertyInfo("Gate-drain overlap cap.")]
        public Parameter MOS3gateDrainOverlapCapFactor { get; } = new Parameter();
        [PropertyName("cgbo"), PropertyInfo("Gate-bulk overlap cap.")]
        public Parameter MOS3gateBulkOverlapCapFactor { get; } = new Parameter();
        [PropertyName("rsh"), PropertyInfo("Sheet resistance")]
        public Parameter MOS3sheetResistance { get; } = new Parameter();
        [PropertyName("cj"), PropertyInfo("Bottom junction cap per area")]
        public Parameter MOS3bulkCapFactor { get; } = new Parameter();
        [PropertyName("mj"), PropertyInfo("Bottom grading coefficient")]
        public Parameter MOS3bulkJctBotGradingCoeff { get; } = new Parameter(.5);
        [PropertyName("cjsw"), PropertyInfo("Side junction cap per area")]
        public Parameter MOS3sideWallCapFactor { get; } = new Parameter();
        [PropertyName("mjsw"), PropertyInfo("Side grading coefficient")]
        public Parameter MOS3bulkJctSideGradingCoeff { get; } = new Parameter(.33);
        [PropertyName("js"), PropertyInfo("Bulk jct. sat. current density")]
        public Parameter MOS3jctSatCurDensity { get; } = new Parameter();
        [PropertyName("tox"), PropertyInfo("Oxide thickness")]
        public Parameter MOS3oxideThickness { get; } = new Parameter(1e-7);
        [PropertyName("ld"), PropertyInfo("Lateral diffusion")]
        public Parameter MOS3latDiff { get; } = new Parameter();
        [PropertyName("u0"), PropertyName("uo"), PropertyInfo("Surface mobility")]
        public Parameter MOS3surfaceMobility { get; } = new Parameter();
        [PropertyName("fc"), PropertyInfo("Forward bias jct. fit parm.")]
        public Parameter MOS3fwdCapDepCoeff { get; } = new Parameter(.5);
        [PropertyName("nsub"), PropertyInfo("Substrate doping")]
        public Parameter MOS3substrateDoping { get; } = new Parameter();
        [PropertyName("tpg"), PropertyInfo("Gate type")]
        public Parameter MOS3gateType { get; } = new Parameter();
        [PropertyName("nss"), PropertyInfo("Surface state density")]
        public Parameter MOS3surfaceStateDensity { get; } = new Parameter();
        [PropertyName("eta"), PropertyInfo("Vds dependence of threshold voltage")]
        public Parameter MOS3eta { get; } = new Parameter();
        [PropertyName("nfs"), PropertyInfo("Fast surface state density")]
        public Parameter MOS3fastSurfaceStateDensity { get; } = new Parameter();
        [PropertyName("theta"), PropertyInfo("Vgs dependence on mobility")]
        public Parameter MOS3theta { get; } = new Parameter();
        [PropertyName("vmax"), PropertyInfo("Maximum carrier drift velocity")]
        public Parameter MOS3maxDriftVel { get; } = new Parameter();
        [PropertyName("kappa"), PropertyInfo("Kappa")]
        public Parameter MOS3kappa { get; } = new Parameter(.2);
        [PropertyName("xj"), PropertyInfo("Junction depth")]
        public Parameter MOS3junctionDepth { get; } = new Parameter();
        [PropertyName("tnom"), PropertyInfo("Parameter measurement temperature")]
        public double MOS3_TNOM
        {
            get => MOS3tnom - Circuit.CelsiusKelvin;
            set => MOS3tnom.Set(value + Circuit.CelsiusKelvin);
        }
        public Parameter MOS3tnom { get; } = new Parameter();

        /// <summary>
        /// Methods
        /// </summary>
        [PropertyName("delta"), PropertyInfo("Width effect on threshold")]
        public double DELTA
        {
            get => MOS3narrowFactor;
            set => MOS3delta = value;
        }
        [PropertyName("nmos"), PropertyInfo("N type MOSfet model")]
        public void SetNMOS(bool value)
        {
            if (value)
                MOS3type = 1.0;
        }
        [PropertyName("pmos"), PropertyInfo("P type MOSfet model")]
        public void SetPMOS(bool value)
        {
            if (value)
                MOS3type = -1.0;
        }
        [PropertyName("type"), PropertyInfo("N-channel or P-channel MOS")]
        public string GetTYPE()
        {
            if (MOS3type > 0)
                return "nmos";
            return "pmos";
        }

        [PropertyName("input_delta"), PropertyInfo("")]
        public double MOS3delta { get; protected set; }
        public double MOS3narrowFactor { get; set; }
        public double MOS3type { get; internal set; } = 1.0;

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
                MOS3type = 1.0;
            else
                MOS3type = -1.0;
        }
    }
}
