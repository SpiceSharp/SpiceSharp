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
        [NameAttribute("vto"), NameAttribute("vt0"), InfoAttribute("Threshold voltage")]
        public Parameter MOS3vt0 { get; } = new Parameter();
        [NameAttribute("kp"), InfoAttribute("Transconductance parameter")]
        public Parameter MOS3transconductance { get; } = new Parameter(2e-5);
        [NameAttribute("gamma"), InfoAttribute("Bulk threshold parameter")]
        public Parameter MOS3gamma { get; } = new Parameter();
        [NameAttribute("phi"), InfoAttribute("Surface potential")]
        public Parameter MOS3phi { get; } = new Parameter(.6);
        [NameAttribute("rd"), InfoAttribute("Drain ohmic resistance")]
        public Parameter MOS3drainResistance { get; } = new Parameter();
        [NameAttribute("rs"), InfoAttribute("Source ohmic resistance")]
        public Parameter MOS3sourceResistance { get; } = new Parameter();
        [NameAttribute("cbd"), InfoAttribute("B-D junction capacitance")]
        public Parameter MOS3capBD { get; } = new Parameter();
        [NameAttribute("cbs"), InfoAttribute("B-S junction capacitance")]
        public Parameter MOS3capBS { get; } = new Parameter();
        [NameAttribute("is"), InfoAttribute("Bulk junction sat. current")]
        public Parameter MOS3jctSatCur { get; } = new Parameter(1e-14);
        [NameAttribute("pb"), InfoAttribute("Bulk junction potential")]
        public Parameter MOS3bulkJctPotential { get; } = new Parameter(.8);
        [NameAttribute("cgso"), InfoAttribute("Gate-source overlap cap.")]
        public Parameter MOS3gateSourceOverlapCapFactor { get; } = new Parameter();
        [NameAttribute("cgdo"), InfoAttribute("Gate-drain overlap cap.")]
        public Parameter MOS3gateDrainOverlapCapFactor { get; } = new Parameter();
        [NameAttribute("cgbo"), InfoAttribute("Gate-bulk overlap cap.")]
        public Parameter MOS3gateBulkOverlapCapFactor { get; } = new Parameter();
        [NameAttribute("rsh"), InfoAttribute("Sheet resistance")]
        public Parameter MOS3sheetResistance { get; } = new Parameter();
        [NameAttribute("cj"), InfoAttribute("Bottom junction cap per area")]
        public Parameter MOS3bulkCapFactor { get; } = new Parameter();
        [NameAttribute("mj"), InfoAttribute("Bottom grading coefficient")]
        public Parameter MOS3bulkJctBotGradingCoeff { get; } = new Parameter(.5);
        [NameAttribute("cjsw"), InfoAttribute("Side junction cap per area")]
        public Parameter MOS3sideWallCapFactor { get; } = new Parameter();
        [NameAttribute("mjsw"), InfoAttribute("Side grading coefficient")]
        public Parameter MOS3bulkJctSideGradingCoeff { get; } = new Parameter(.33);
        [NameAttribute("js"), InfoAttribute("Bulk jct. sat. current density")]
        public Parameter MOS3jctSatCurDensity { get; } = new Parameter();
        [NameAttribute("tox"), InfoAttribute("Oxide thickness")]
        public Parameter MOS3oxideThickness { get; } = new Parameter(1e-7);
        [NameAttribute("ld"), InfoAttribute("Lateral diffusion")]
        public Parameter MOS3latDiff { get; } = new Parameter();
        [NameAttribute("u0"), NameAttribute("uo"), InfoAttribute("Surface mobility")]
        public Parameter MOS3surfaceMobility { get; } = new Parameter();
        [NameAttribute("fc"), InfoAttribute("Forward bias jct. fit parm.")]
        public Parameter MOS3fwdCapDepCoeff { get; } = new Parameter(.5);
        [NameAttribute("nsub"), InfoAttribute("Substrate doping")]
        public Parameter MOS3substrateDoping { get; } = new Parameter();
        [NameAttribute("tpg"), InfoAttribute("Gate type")]
        public Parameter MOS3gateType { get; } = new Parameter();
        [NameAttribute("nss"), InfoAttribute("Surface state density")]
        public Parameter MOS3surfaceStateDensity { get; } = new Parameter();
        [NameAttribute("eta"), InfoAttribute("Vds dependence of threshold voltage")]
        public Parameter MOS3eta { get; } = new Parameter();
        [NameAttribute("nfs"), InfoAttribute("Fast surface state density")]
        public Parameter MOS3fastSurfaceStateDensity { get; } = new Parameter();
        [NameAttribute("theta"), InfoAttribute("Vgs dependence on mobility")]
        public Parameter MOS3theta { get; } = new Parameter();
        [NameAttribute("vmax"), InfoAttribute("Maximum carrier drift velocity")]
        public Parameter MOS3maxDriftVel { get; } = new Parameter();
        [NameAttribute("kappa"), InfoAttribute("Kappa")]
        public Parameter MOS3kappa { get; } = new Parameter(.2);
        [NameAttribute("xj"), InfoAttribute("Junction depth")]
        public Parameter MOS3junctionDepth { get; } = new Parameter();
        [NameAttribute("tnom"), InfoAttribute("Parameter measurement temperature")]
        public double MOS3_TNOM
        {
            get => MOS3tnom - Circuit.CONSTCtoK;
            set => MOS3tnom.Set(value + Circuit.CONSTCtoK);
        }
        public Parameter MOS3tnom { get; } = new Parameter();

        /// <summary>
        /// Methods
        /// </summary>
        [NameAttribute("delta"), InfoAttribute("Width effect on threshold")]
        public double DELTA
        {
            get => MOS3narrowFactor;
            set => MOS3delta = value;
        }
        [NameAttribute("nmos"), InfoAttribute("N type MOSfet model")]
        public void SetNMOS(bool value)
        {
            if (value)
                MOS3type = 1.0;
        }
        [NameAttribute("pmos"), InfoAttribute("P type MOSfet model")]
        public void SetPMOS(bool value)
        {
            if (value)
                MOS3type = -1.0;
        }
        [NameAttribute("type"), InfoAttribute("N-channel or P-channel MOS")]
        public string GetTYPE()
        {
            if (MOS3type > 0)
                return "nmos";
            return "pmos";
        }

        [NameAttribute("input_delta"), InfoAttribute("")]
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
