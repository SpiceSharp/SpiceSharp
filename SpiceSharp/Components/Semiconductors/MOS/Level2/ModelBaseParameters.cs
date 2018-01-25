using SpiceSharp.Attributes;

namespace SpiceSharp.Components.Mosfet.Level2
{
    /// <summary>
    /// Base parameters for a <see cref="MOS2Model"/>
    /// </summary>
    public class ModelBaseParameters : ParameterSet
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [NameAttribute("tnom"), InfoAttribute("Parameter measurement temperature")]
        public double MOS2_TNOM
        {
            get => MOS2tnom - Circuit.CONSTCtoK;
            set => MOS2tnom.Set(value + Circuit.CONSTCtoK);
        }
        public Parameter MOS2tnom { get; } = new Parameter();
        [NameAttribute("vto"), NameAttribute("vt0"), InfoAttribute("Threshold voltage")]
        public Parameter MOS2vt0 { get; } = new Parameter();
        [NameAttribute("kp"), InfoAttribute("Transconductance parameter")]
        public Parameter MOS2transconductance { get; } = new Parameter();
        [NameAttribute("gamma"), InfoAttribute("Bulk threshold parameter")]
        public Parameter MOS2gamma { get; } = new Parameter();
        [NameAttribute("phi"), InfoAttribute("Surface potential")]
        public Parameter MOS2phi { get; } = new Parameter(.6);
        [NameAttribute("lambda"), InfoAttribute("Channel length modulation")]
        public Parameter MOS2lambda { get; } = new Parameter();
        [NameAttribute("rd"), InfoAttribute("Drain ohmic resistance")]
        public Parameter MOS2drainResistance { get; } = new Parameter();
        [NameAttribute("rs"), InfoAttribute("Source ohmic resistance")]
        public Parameter MOS2sourceResistance { get; } = new Parameter();
        [NameAttribute("cbd"), InfoAttribute("B-D junction capacitance")]
        public Parameter MOS2capBD { get; } = new Parameter();
        [NameAttribute("cbs"), InfoAttribute("B-S junction capacitance")]
        public Parameter MOS2capBS { get; } = new Parameter();
        [NameAttribute("is"), InfoAttribute("Bulk junction sat. current")]
        public Parameter MOS2jctSatCur { get; } = new Parameter(1e-14);
        [NameAttribute("pb"), InfoAttribute("Bulk junction potential")]
        public Parameter MOS2bulkJctPotential { get; } = new Parameter(.8);
        [NameAttribute("cgso"), InfoAttribute("Gate-source overlap cap.")]
        public Parameter MOS2gateSourceOverlapCapFactor { get; } = new Parameter();
        [NameAttribute("cgdo"), InfoAttribute("Gate-drain overlap cap.")]
        public Parameter MOS2gateDrainOverlapCapFactor { get; } = new Parameter();
        [NameAttribute("cgbo"), InfoAttribute("Gate-bulk overlap cap.")]
        public Parameter MOS2gateBulkOverlapCapFactor { get; } = new Parameter();
        [NameAttribute("cj"), InfoAttribute("Bottom junction cap per area")]
        public Parameter MOS2bulkCapFactor { get; } = new Parameter();
        [NameAttribute("mj"), InfoAttribute("Bottom grading coefficient")]
        public Parameter MOS2bulkJctBotGradingCoeff { get; } = new Parameter(.5);
        [NameAttribute("cjsw"), InfoAttribute("Side junction cap per area")]
        public Parameter MOS2sideWallCapFactor { get; } = new Parameter();
        [NameAttribute("mjsw"), InfoAttribute("Side grading coefficient")]
        public Parameter MOS2bulkJctSideGradingCoeff { get; } = new Parameter(.33);
        [NameAttribute("js"), InfoAttribute("Bulk jct. sat. current density")]
        public Parameter MOS2jctSatCurDensity { get; } = new Parameter();
        [NameAttribute("tox"), InfoAttribute("Oxide thickness")]
        public Parameter MOS2oxideThickness { get; } = new Parameter();
        [NameAttribute("ld"), InfoAttribute("Lateral diffusion")]
        public Parameter MOS2latDiff { get; } = new Parameter();
        [NameAttribute("rsh"), InfoAttribute("Sheet resistance")]
        public Parameter MOS2sheetResistance { get; } = new Parameter();
        [NameAttribute("u0"), NameAttribute("uo"), InfoAttribute("Surface mobility")]
        public Parameter MOS2surfaceMobility { get; } = new Parameter();
        [NameAttribute("fc"), InfoAttribute("Forward bias jct. fit parm.")]
        public Parameter MOS2fwdCapDepCoeff { get; } = new Parameter(.5);
        [NameAttribute("nsub"), InfoAttribute("Substrate doping")]
        public Parameter MOS2substrateDoping { get; } = new Parameter();
        [NameAttribute("tpg"), InfoAttribute("Gate type")]
        public Parameter MOS2gateType { get; } = new Parameter();
        [NameAttribute("nss"), InfoAttribute("Surface state density")]
        public Parameter MOS2surfaceStateDensity { get; } = new Parameter();
        [NameAttribute("nfs"), InfoAttribute("Fast surface state density")]
        public Parameter MOS2fastSurfaceStateDensity { get; } = new Parameter();
        [NameAttribute("delta"), InfoAttribute("Width effect on threshold")]
        public Parameter MOS2narrowFactor { get; } = new Parameter();
        [NameAttribute("uexp"), InfoAttribute("Crit. field exp for mob. deg.")]
        public Parameter MOS2critFieldExp { get; } = new Parameter();
        [NameAttribute("vmax"), InfoAttribute("Maximum carrier drift velocity")]
        public Parameter MOS2maxDriftVel { get; } = new Parameter();
        [NameAttribute("xj"), InfoAttribute("Junction depth")]
        public Parameter MOS2junctionDepth { get; } = new Parameter();
        [NameAttribute("neff"), InfoAttribute("Total channel charge coeff.")]
        public Parameter MOS2channelCharge { get; } = new Parameter(1);
        [NameAttribute("ucrit"), InfoAttribute("Crit. field for mob. degradation")]
        public Parameter MOS2critField { get; } = new Parameter(1e4);

        /// <summary>
        /// Methods
        /// </summary>
        [NameAttribute("nmos"), InfoAttribute("N type MOSfet model")]
        public void SetNMOS(bool value)
        {
            if (value)
                MOS2type = 1.0;
        }
        [NameAttribute("pmos"), InfoAttribute("P type MOSfet model")]
        public void SetPMOS(bool value)
        {
            if (value)
                MOS2type = -1.0;
        }
        [NameAttribute("type"), InfoAttribute("N-channel or P-channel MOS")]
        public string GetTYPE(Circuit ckt)
        {
            if (MOS2type > 0)
                return "nmos";
            return "pmos";
        }
        public double MOS2type { get; internal set; } = 1.0;

    }
}
