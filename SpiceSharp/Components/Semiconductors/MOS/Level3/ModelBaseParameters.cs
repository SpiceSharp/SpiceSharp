using SpiceSharp.Attributes;

namespace SpiceSharp.Components.Mosfet.Level3
{
    /// <summary>
    /// Base parameters for a <see cref="MOS3Model"/>
    /// </summary>
    public class ModelBaseParameters : Parameters
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("vto"), SpiceName("vt0"), SpiceInfo("Threshold voltage")]
        public Parameter MOS3vt0 { get; } = new Parameter();
        [SpiceName("kp"), SpiceInfo("Transconductance parameter")]
        public Parameter MOS3transconductance { get; } = new Parameter(2e-5);
        [SpiceName("gamma"), SpiceInfo("Bulk threshold parameter")]
        public Parameter MOS3gamma { get; } = new Parameter();
        [SpiceName("phi"), SpiceInfo("Surface potential")]
        public Parameter MOS3phi { get; } = new Parameter(.6);
        [SpiceName("rd"), SpiceInfo("Drain ohmic resistance")]
        public Parameter MOS3drainResistance { get; } = new Parameter();
        [SpiceName("rs"), SpiceInfo("Source ohmic resistance")]
        public Parameter MOS3sourceResistance { get; } = new Parameter();
        [SpiceName("cbd"), SpiceInfo("B-D junction capacitance")]
        public Parameter MOS3capBD { get; } = new Parameter();
        [SpiceName("cbs"), SpiceInfo("B-S junction capacitance")]
        public Parameter MOS3capBS { get; } = new Parameter();
        [SpiceName("is"), SpiceInfo("Bulk junction sat. current")]
        public Parameter MOS3jctSatCur { get; } = new Parameter(1e-14);
        [SpiceName("pb"), SpiceInfo("Bulk junction potential")]
        public Parameter MOS3bulkJctPotential { get; } = new Parameter(.8);
        [SpiceName("cgso"), SpiceInfo("Gate-source overlap cap.")]
        public Parameter MOS3gateSourceOverlapCapFactor { get; } = new Parameter();
        [SpiceName("cgdo"), SpiceInfo("Gate-drain overlap cap.")]
        public Parameter MOS3gateDrainOverlapCapFactor { get; } = new Parameter();
        [SpiceName("cgbo"), SpiceInfo("Gate-bulk overlap cap.")]
        public Parameter MOS3gateBulkOverlapCapFactor { get; } = new Parameter();
        [SpiceName("rsh"), SpiceInfo("Sheet resistance")]
        public Parameter MOS3sheetResistance { get; } = new Parameter();
        [SpiceName("cj"), SpiceInfo("Bottom junction cap per area")]
        public Parameter MOS3bulkCapFactor { get; } = new Parameter();
        [SpiceName("mj"), SpiceInfo("Bottom grading coefficient")]
        public Parameter MOS3bulkJctBotGradingCoeff { get; } = new Parameter(.5);
        [SpiceName("cjsw"), SpiceInfo("Side junction cap per area")]
        public Parameter MOS3sideWallCapFactor { get; } = new Parameter();
        [SpiceName("mjsw"), SpiceInfo("Side grading coefficient")]
        public Parameter MOS3bulkJctSideGradingCoeff { get; } = new Parameter(.33);
        [SpiceName("js"), SpiceInfo("Bulk jct. sat. current density")]
        public Parameter MOS3jctSatCurDensity { get; } = new Parameter();
        [SpiceName("tox"), SpiceInfo("Oxide thickness")]
        public Parameter MOS3oxideThickness { get; } = new Parameter(1e-7);
        [SpiceName("ld"), SpiceInfo("Lateral diffusion")]
        public Parameter MOS3latDiff { get; } = new Parameter();
        [SpiceName("u0"), SpiceName("uo"), SpiceInfo("Surface mobility")]
        public Parameter MOS3surfaceMobility { get; } = new Parameter();
        [SpiceName("fc"), SpiceInfo("Forward bias jct. fit parm.")]
        public Parameter MOS3fwdCapDepCoeff { get; } = new Parameter(.5);
        [SpiceName("nsub"), SpiceInfo("Substrate doping")]
        public Parameter MOS3substrateDoping { get; } = new Parameter();
        [SpiceName("tpg"), SpiceInfo("Gate type")]
        public Parameter MOS3gateType { get; } = new Parameter();
        [SpiceName("nss"), SpiceInfo("Surface state density")]
        public Parameter MOS3surfaceStateDensity { get; } = new Parameter();
        [SpiceName("eta"), SpiceInfo("Vds dependence of threshold voltage")]
        public Parameter MOS3eta { get; } = new Parameter();
        [SpiceName("nfs"), SpiceInfo("Fast surface state density")]
        public Parameter MOS3fastSurfaceStateDensity { get; } = new Parameter();
        [SpiceName("theta"), SpiceInfo("Vgs dependence on mobility")]
        public Parameter MOS3theta { get; } = new Parameter();
        [SpiceName("vmax"), SpiceInfo("Maximum carrier drift velocity")]
        public Parameter MOS3maxDriftVel { get; } = new Parameter();
        [SpiceName("kappa"), SpiceInfo("Kappa")]
        public Parameter MOS3kappa { get; } = new Parameter(.2);
        [SpiceName("xj"), SpiceInfo("Junction depth")]
        public Parameter MOS3junctionDepth { get; } = new Parameter();
        [SpiceName("tnom"), SpiceInfo("Parameter measurement temperature")]
        public double MOS3_TNOM
        {
            get => MOS3tnom - Circuit.CONSTCtoK;
            set => MOS3tnom.Set(value + Circuit.CONSTCtoK);
        }
        public Parameter MOS3tnom { get; } = new Parameter();

        /// <summary>
        /// Methods
        /// </summary>
        [SpiceName("delta"), SpiceInfo("Width effect on threshold")]
        public double DELTA
        {
            get => MOS3narrowFactor;
            set => MOS3delta = value;
        }
        [SpiceName("nmos"), SpiceInfo("N type MOSfet model")]
        public void SetNMOS(bool value)
        {
            if (value)
                MOS3type = 1.0;
        }
        [SpiceName("pmos"), SpiceInfo("P type MOSfet model")]
        public void SetPMOS(bool value)
        {
            if (value)
                MOS3type = -1.0;
        }
        [SpiceName("type"), SpiceInfo("N-channel or P-channel MOS")]
        public string GetTYPE()
        {
            if (MOS3type > 0)
                return "nmos";
            return "pmos";
        }

        [SpiceName("input_delta"), SpiceInfo("")]
        public double MOS3delta { get; protected set; }
        public double MOS3narrowFactor { get; set; }
        public double MOS3type { get; internal set; } = 1.0;

    }
}
