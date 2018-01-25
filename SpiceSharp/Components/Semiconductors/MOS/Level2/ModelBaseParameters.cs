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
        [PropertyName("tnom"), PropertyInfo("Parameter measurement temperature")]
        public double MOS2_TNOM
        {
            get => MOS2tnom - Circuit.CelsiusKelvin;
            set => MOS2tnom.Set(value + Circuit.CelsiusKelvin);
        }
        public Parameter MOS2tnom { get; } = new Parameter();
        [PropertyName("vto"), PropertyName("vt0"), PropertyInfo("Threshold voltage")]
        public Parameter MOS2vt0 { get; } = new Parameter();
        [PropertyName("kp"), PropertyInfo("Transconductance parameter")]
        public Parameter MOS2transconductance { get; } = new Parameter();
        [PropertyName("gamma"), PropertyInfo("Bulk threshold parameter")]
        public Parameter MOS2gamma { get; } = new Parameter();
        [PropertyName("phi"), PropertyInfo("Surface potential")]
        public Parameter MOS2phi { get; } = new Parameter(.6);
        [PropertyName("lambda"), PropertyInfo("Channel length modulation")]
        public Parameter MOS2lambda { get; } = new Parameter();
        [PropertyName("rd"), PropertyInfo("Drain ohmic resistance")]
        public Parameter MOS2drainResistance { get; } = new Parameter();
        [PropertyName("rs"), PropertyInfo("Source ohmic resistance")]
        public Parameter MOS2sourceResistance { get; } = new Parameter();
        [PropertyName("cbd"), PropertyInfo("B-D junction capacitance")]
        public Parameter MOS2capBD { get; } = new Parameter();
        [PropertyName("cbs"), PropertyInfo("B-S junction capacitance")]
        public Parameter MOS2capBS { get; } = new Parameter();
        [PropertyName("is"), PropertyInfo("Bulk junction sat. current")]
        public Parameter MOS2jctSatCur { get; } = new Parameter(1e-14);
        [PropertyName("pb"), PropertyInfo("Bulk junction potential")]
        public Parameter MOS2bulkJctPotential { get; } = new Parameter(.8);
        [PropertyName("cgso"), PropertyInfo("Gate-source overlap cap.")]
        public Parameter MOS2gateSourceOverlapCapFactor { get; } = new Parameter();
        [PropertyName("cgdo"), PropertyInfo("Gate-drain overlap cap.")]
        public Parameter MOS2gateDrainOverlapCapFactor { get; } = new Parameter();
        [PropertyName("cgbo"), PropertyInfo("Gate-bulk overlap cap.")]
        public Parameter MOS2gateBulkOverlapCapFactor { get; } = new Parameter();
        [PropertyName("cj"), PropertyInfo("Bottom junction cap per area")]
        public Parameter MOS2bulkCapFactor { get; } = new Parameter();
        [PropertyName("mj"), PropertyInfo("Bottom grading coefficient")]
        public Parameter MOS2bulkJctBotGradingCoeff { get; } = new Parameter(.5);
        [PropertyName("cjsw"), PropertyInfo("Side junction cap per area")]
        public Parameter MOS2sideWallCapFactor { get; } = new Parameter();
        [PropertyName("mjsw"), PropertyInfo("Side grading coefficient")]
        public Parameter MOS2bulkJctSideGradingCoeff { get; } = new Parameter(.33);
        [PropertyName("js"), PropertyInfo("Bulk jct. sat. current density")]
        public Parameter MOS2jctSatCurDensity { get; } = new Parameter();
        [PropertyName("tox"), PropertyInfo("Oxide thickness")]
        public Parameter MOS2oxideThickness { get; } = new Parameter();
        [PropertyName("ld"), PropertyInfo("Lateral diffusion")]
        public Parameter MOS2latDiff { get; } = new Parameter();
        [PropertyName("rsh"), PropertyInfo("Sheet resistance")]
        public Parameter MOS2sheetResistance { get; } = new Parameter();
        [PropertyName("u0"), PropertyName("uo"), PropertyInfo("Surface mobility")]
        public Parameter MOS2surfaceMobility { get; } = new Parameter();
        [PropertyName("fc"), PropertyInfo("Forward bias jct. fit parm.")]
        public Parameter MOS2fwdCapDepCoeff { get; } = new Parameter(.5);
        [PropertyName("nsub"), PropertyInfo("Substrate doping")]
        public Parameter MOS2substrateDoping { get; } = new Parameter();
        [PropertyName("tpg"), PropertyInfo("Gate type")]
        public Parameter MOS2gateType { get; } = new Parameter();
        [PropertyName("nss"), PropertyInfo("Surface state density")]
        public Parameter MOS2surfaceStateDensity { get; } = new Parameter();
        [PropertyName("nfs"), PropertyInfo("Fast surface state density")]
        public Parameter MOS2fastSurfaceStateDensity { get; } = new Parameter();
        [PropertyName("delta"), PropertyInfo("Width effect on threshold")]
        public Parameter MOS2narrowFactor { get; } = new Parameter();
        [PropertyName("uexp"), PropertyInfo("Crit. field exp for mob. deg.")]
        public Parameter MOS2critFieldExp { get; } = new Parameter();
        [PropertyName("vmax"), PropertyInfo("Maximum carrier drift velocity")]
        public Parameter MOS2maxDriftVel { get; } = new Parameter();
        [PropertyName("xj"), PropertyInfo("Junction depth")]
        public Parameter MOS2junctionDepth { get; } = new Parameter();
        [PropertyName("neff"), PropertyInfo("Total channel charge coeff.")]
        public Parameter MOS2channelCharge { get; } = new Parameter(1);
        [PropertyName("ucrit"), PropertyInfo("Crit. field for mob. degradation")]
        public Parameter MOS2critField { get; } = new Parameter(1e4);

        /// <summary>
        /// Methods
        /// </summary>
        [PropertyName("nmos"), PropertyInfo("N type MOSfet model")]
        public void SetNMOS(bool value)
        {
            if (value)
                MOS2type = 1.0;
        }
        [PropertyName("pmos"), PropertyInfo("P type MOSfet model")]
        public void SetPMOS(bool value)
        {
            if (value)
                MOS2type = -1.0;
        }
        [PropertyName("type"), PropertyInfo("N-channel or P-channel MOS")]
        public string GetTYPE(Circuit ckt)
        {
            if (MOS2type > 0)
                return "nmos";
            return "pmos";
        }
        public double MOS2type { get; internal set; } = 1.0;

    }
}
