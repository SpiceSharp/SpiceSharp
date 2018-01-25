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
        [PropertyNameAttribute("vto"), PropertyNameAttribute("vt0"), PropertyInfoAttribute("Threshold voltage")]
        public Parameter MOS3vt0 { get; } = new Parameter();
        [PropertyNameAttribute("kp"), PropertyInfoAttribute("Transconductance parameter")]
        public Parameter MOS3transconductance { get; } = new Parameter(2e-5);
        [PropertyNameAttribute("gamma"), PropertyInfoAttribute("Bulk threshold parameter")]
        public Parameter MOS3gamma { get; } = new Parameter();
        [PropertyNameAttribute("phi"), PropertyInfoAttribute("Surface potential")]
        public Parameter MOS3phi { get; } = new Parameter(.6);
        [PropertyNameAttribute("rd"), PropertyInfoAttribute("Drain ohmic resistance")]
        public Parameter MOS3drainResistance { get; } = new Parameter();
        [PropertyNameAttribute("rs"), PropertyInfoAttribute("Source ohmic resistance")]
        public Parameter MOS3sourceResistance { get; } = new Parameter();
        [PropertyNameAttribute("cbd"), PropertyInfoAttribute("B-D junction capacitance")]
        public Parameter MOS3capBD { get; } = new Parameter();
        [PropertyNameAttribute("cbs"), PropertyInfoAttribute("B-S junction capacitance")]
        public Parameter MOS3capBS { get; } = new Parameter();
        [PropertyNameAttribute("is"), PropertyInfoAttribute("Bulk junction sat. current")]
        public Parameter MOS3jctSatCur { get; } = new Parameter(1e-14);
        [PropertyNameAttribute("pb"), PropertyInfoAttribute("Bulk junction potential")]
        public Parameter MOS3bulkJctPotential { get; } = new Parameter(.8);
        [PropertyNameAttribute("cgso"), PropertyInfoAttribute("Gate-source overlap cap.")]
        public Parameter MOS3gateSourceOverlapCapFactor { get; } = new Parameter();
        [PropertyNameAttribute("cgdo"), PropertyInfoAttribute("Gate-drain overlap cap.")]
        public Parameter MOS3gateDrainOverlapCapFactor { get; } = new Parameter();
        [PropertyNameAttribute("cgbo"), PropertyInfoAttribute("Gate-bulk overlap cap.")]
        public Parameter MOS3gateBulkOverlapCapFactor { get; } = new Parameter();
        [PropertyNameAttribute("rsh"), PropertyInfoAttribute("Sheet resistance")]
        public Parameter MOS3sheetResistance { get; } = new Parameter();
        [PropertyNameAttribute("cj"), PropertyInfoAttribute("Bottom junction cap per area")]
        public Parameter MOS3bulkCapFactor { get; } = new Parameter();
        [PropertyNameAttribute("mj"), PropertyInfoAttribute("Bottom grading coefficient")]
        public Parameter MOS3bulkJctBotGradingCoeff { get; } = new Parameter(.5);
        [PropertyNameAttribute("cjsw"), PropertyInfoAttribute("Side junction cap per area")]
        public Parameter MOS3sideWallCapFactor { get; } = new Parameter();
        [PropertyNameAttribute("mjsw"), PropertyInfoAttribute("Side grading coefficient")]
        public Parameter MOS3bulkJctSideGradingCoeff { get; } = new Parameter(.33);
        [PropertyNameAttribute("js"), PropertyInfoAttribute("Bulk jct. sat. current density")]
        public Parameter MOS3jctSatCurDensity { get; } = new Parameter();
        [PropertyNameAttribute("tox"), PropertyInfoAttribute("Oxide thickness")]
        public Parameter MOS3oxideThickness { get; } = new Parameter(1e-7);
        [PropertyNameAttribute("ld"), PropertyInfoAttribute("Lateral diffusion")]
        public Parameter MOS3latDiff { get; } = new Parameter();
        [PropertyNameAttribute("u0"), PropertyNameAttribute("uo"), PropertyInfoAttribute("Surface mobility")]
        public Parameter MOS3surfaceMobility { get; } = new Parameter();
        [PropertyNameAttribute("fc"), PropertyInfoAttribute("Forward bias jct. fit parm.")]
        public Parameter MOS3fwdCapDepCoeff { get; } = new Parameter(.5);
        [PropertyNameAttribute("nsub"), PropertyInfoAttribute("Substrate doping")]
        public Parameter MOS3substrateDoping { get; } = new Parameter();
        [PropertyNameAttribute("tpg"), PropertyInfoAttribute("Gate type")]
        public Parameter MOS3gateType { get; } = new Parameter();
        [PropertyNameAttribute("nss"), PropertyInfoAttribute("Surface state density")]
        public Parameter MOS3surfaceStateDensity { get; } = new Parameter();
        [PropertyNameAttribute("eta"), PropertyInfoAttribute("Vds dependence of threshold voltage")]
        public Parameter MOS3eta { get; } = new Parameter();
        [PropertyNameAttribute("nfs"), PropertyInfoAttribute("Fast surface state density")]
        public Parameter MOS3fastSurfaceStateDensity { get; } = new Parameter();
        [PropertyNameAttribute("theta"), PropertyInfoAttribute("Vgs dependence on mobility")]
        public Parameter MOS3theta { get; } = new Parameter();
        [PropertyNameAttribute("vmax"), PropertyInfoAttribute("Maximum carrier drift velocity")]
        public Parameter MOS3maxDriftVel { get; } = new Parameter();
        [PropertyNameAttribute("kappa"), PropertyInfoAttribute("Kappa")]
        public Parameter MOS3kappa { get; } = new Parameter(.2);
        [PropertyNameAttribute("xj"), PropertyInfoAttribute("Junction depth")]
        public Parameter MOS3junctionDepth { get; } = new Parameter();
        [PropertyNameAttribute("tnom"), PropertyInfoAttribute("Parameter measurement temperature")]
        public double MOS3_TNOM
        {
            get => MOS3tnom - Circuit.CONSTCtoK;
            set => MOS3tnom.Set(value + Circuit.CONSTCtoK);
        }
        public Parameter MOS3tnom { get; } = new Parameter();

        /// <summary>
        /// Methods
        /// </summary>
        [PropertyNameAttribute("delta"), PropertyInfoAttribute("Width effect on threshold")]
        public double DELTA
        {
            get => MOS3narrowFactor;
            set => MOS3delta = value;
        }
        [PropertyNameAttribute("nmos"), PropertyInfoAttribute("N type MOSfet model")]
        public void SetNMOS(bool value)
        {
            if (value)
                MOS3type = 1.0;
        }
        [PropertyNameAttribute("pmos"), PropertyInfoAttribute("P type MOSfet model")]
        public void SetPMOS(bool value)
        {
            if (value)
                MOS3type = -1.0;
        }
        [PropertyNameAttribute("type"), PropertyInfoAttribute("N-channel or P-channel MOS")]
        public string GetTYPE()
        {
            if (MOS3type > 0)
                return "nmos";
            return "pmos";
        }

        [PropertyNameAttribute("input_delta"), PropertyInfoAttribute("")]
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
