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
        [PropertyNameAttribute("tnom"), PropertyInfoAttribute("Parameter measurement temperature")]
        public double MOS2_TNOM
        {
            get => MOS2tnom - Circuit.CONSTCtoK;
            set => MOS2tnom.Set(value + Circuit.CONSTCtoK);
        }
        public Parameter MOS2tnom { get; } = new Parameter();
        [PropertyNameAttribute("vto"), PropertyNameAttribute("vt0"), PropertyInfoAttribute("Threshold voltage")]
        public Parameter MOS2vt0 { get; } = new Parameter();
        [PropertyNameAttribute("kp"), PropertyInfoAttribute("Transconductance parameter")]
        public Parameter MOS2transconductance { get; } = new Parameter();
        [PropertyNameAttribute("gamma"), PropertyInfoAttribute("Bulk threshold parameter")]
        public Parameter MOS2gamma { get; } = new Parameter();
        [PropertyNameAttribute("phi"), PropertyInfoAttribute("Surface potential")]
        public Parameter MOS2phi { get; } = new Parameter(.6);
        [PropertyNameAttribute("lambda"), PropertyInfoAttribute("Channel length modulation")]
        public Parameter MOS2lambda { get; } = new Parameter();
        [PropertyNameAttribute("rd"), PropertyInfoAttribute("Drain ohmic resistance")]
        public Parameter MOS2drainResistance { get; } = new Parameter();
        [PropertyNameAttribute("rs"), PropertyInfoAttribute("Source ohmic resistance")]
        public Parameter MOS2sourceResistance { get; } = new Parameter();
        [PropertyNameAttribute("cbd"), PropertyInfoAttribute("B-D junction capacitance")]
        public Parameter MOS2capBD { get; } = new Parameter();
        [PropertyNameAttribute("cbs"), PropertyInfoAttribute("B-S junction capacitance")]
        public Parameter MOS2capBS { get; } = new Parameter();
        [PropertyNameAttribute("is"), PropertyInfoAttribute("Bulk junction sat. current")]
        public Parameter MOS2jctSatCur { get; } = new Parameter(1e-14);
        [PropertyNameAttribute("pb"), PropertyInfoAttribute("Bulk junction potential")]
        public Parameter MOS2bulkJctPotential { get; } = new Parameter(.8);
        [PropertyNameAttribute("cgso"), PropertyInfoAttribute("Gate-source overlap cap.")]
        public Parameter MOS2gateSourceOverlapCapFactor { get; } = new Parameter();
        [PropertyNameAttribute("cgdo"), PropertyInfoAttribute("Gate-drain overlap cap.")]
        public Parameter MOS2gateDrainOverlapCapFactor { get; } = new Parameter();
        [PropertyNameAttribute("cgbo"), PropertyInfoAttribute("Gate-bulk overlap cap.")]
        public Parameter MOS2gateBulkOverlapCapFactor { get; } = new Parameter();
        [PropertyNameAttribute("cj"), PropertyInfoAttribute("Bottom junction cap per area")]
        public Parameter MOS2bulkCapFactor { get; } = new Parameter();
        [PropertyNameAttribute("mj"), PropertyInfoAttribute("Bottom grading coefficient")]
        public Parameter MOS2bulkJctBotGradingCoeff { get; } = new Parameter(.5);
        [PropertyNameAttribute("cjsw"), PropertyInfoAttribute("Side junction cap per area")]
        public Parameter MOS2sideWallCapFactor { get; } = new Parameter();
        [PropertyNameAttribute("mjsw"), PropertyInfoAttribute("Side grading coefficient")]
        public Parameter MOS2bulkJctSideGradingCoeff { get; } = new Parameter(.33);
        [PropertyNameAttribute("js"), PropertyInfoAttribute("Bulk jct. sat. current density")]
        public Parameter MOS2jctSatCurDensity { get; } = new Parameter();
        [PropertyNameAttribute("tox"), PropertyInfoAttribute("Oxide thickness")]
        public Parameter MOS2oxideThickness { get; } = new Parameter();
        [PropertyNameAttribute("ld"), PropertyInfoAttribute("Lateral diffusion")]
        public Parameter MOS2latDiff { get; } = new Parameter();
        [PropertyNameAttribute("rsh"), PropertyInfoAttribute("Sheet resistance")]
        public Parameter MOS2sheetResistance { get; } = new Parameter();
        [PropertyNameAttribute("u0"), PropertyNameAttribute("uo"), PropertyInfoAttribute("Surface mobility")]
        public Parameter MOS2surfaceMobility { get; } = new Parameter();
        [PropertyNameAttribute("fc"), PropertyInfoAttribute("Forward bias jct. fit parm.")]
        public Parameter MOS2fwdCapDepCoeff { get; } = new Parameter(.5);
        [PropertyNameAttribute("nsub"), PropertyInfoAttribute("Substrate doping")]
        public Parameter MOS2substrateDoping { get; } = new Parameter();
        [PropertyNameAttribute("tpg"), PropertyInfoAttribute("Gate type")]
        public Parameter MOS2gateType { get; } = new Parameter();
        [PropertyNameAttribute("nss"), PropertyInfoAttribute("Surface state density")]
        public Parameter MOS2surfaceStateDensity { get; } = new Parameter();
        [PropertyNameAttribute("nfs"), PropertyInfoAttribute("Fast surface state density")]
        public Parameter MOS2fastSurfaceStateDensity { get; } = new Parameter();
        [PropertyNameAttribute("delta"), PropertyInfoAttribute("Width effect on threshold")]
        public Parameter MOS2narrowFactor { get; } = new Parameter();
        [PropertyNameAttribute("uexp"), PropertyInfoAttribute("Crit. field exp for mob. deg.")]
        public Parameter MOS2critFieldExp { get; } = new Parameter();
        [PropertyNameAttribute("vmax"), PropertyInfoAttribute("Maximum carrier drift velocity")]
        public Parameter MOS2maxDriftVel { get; } = new Parameter();
        [PropertyNameAttribute("xj"), PropertyInfoAttribute("Junction depth")]
        public Parameter MOS2junctionDepth { get; } = new Parameter();
        [PropertyNameAttribute("neff"), PropertyInfoAttribute("Total channel charge coeff.")]
        public Parameter MOS2channelCharge { get; } = new Parameter(1);
        [PropertyNameAttribute("ucrit"), PropertyInfoAttribute("Crit. field for mob. degradation")]
        public Parameter MOS2critField { get; } = new Parameter(1e4);

        /// <summary>
        /// Methods
        /// </summary>
        [PropertyNameAttribute("nmos"), PropertyInfoAttribute("N type MOSfet model")]
        public void SetNMOS(bool value)
        {
            if (value)
                MOS2type = 1.0;
        }
        [PropertyNameAttribute("pmos"), PropertyInfoAttribute("P type MOSfet model")]
        public void SetPMOS(bool value)
        {
            if (value)
                MOS2type = -1.0;
        }
        [PropertyNameAttribute("type"), PropertyInfoAttribute("N-channel or P-channel MOS")]
        public string GetTYPE(Circuit ckt)
        {
            if (MOS2type > 0)
                return "nmos";
            return "pmos";
        }
        public double MOS2type { get; internal set; } = 1.0;

    }
}
