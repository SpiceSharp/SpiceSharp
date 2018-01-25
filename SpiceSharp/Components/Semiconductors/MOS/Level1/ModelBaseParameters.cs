using SpiceSharp.Attributes;

namespace SpiceSharp.Components.Mosfet.Level1
{
    /// <summary>
    /// Base parameters for a <see cref="MOS1Model"/>
    /// </summary>
    public class ModelBaseParameters : ParameterSet
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [NameAttribute("tnom"), InfoAttribute("Parameter measurement temperature")]
        public double MOS1_TNOM
        {
            get => MOS1tnom - Circuit.CONSTCtoK;
            set => MOS1tnom.Set(value + Circuit.CONSTCtoK);
        }
        public Parameter MOS1tnom { get; } = new Parameter();
        [NameAttribute("vto"), NameAttribute("vt0"), InfoAttribute("Threshold voltage")]
        public Parameter MOS1vt0 { get; } = new Parameter();
        [NameAttribute("kp"), InfoAttribute("Transconductance parameter")]
        public Parameter MOS1transconductance { get; } = new Parameter(2e-5);
        [NameAttribute("gamma"), InfoAttribute("Bulk threshold parameter")]
        public Parameter MOS1gamma { get; } = new Parameter();
        [NameAttribute("phi"), InfoAttribute("Surface potential")]
        public Parameter MOS1phi { get; } = new Parameter(.6);
        [NameAttribute("lambda"), InfoAttribute("Channel length modulation")]
        public Parameter MOS1lambda { get; } = new Parameter();
        [NameAttribute("rd"), InfoAttribute("Drain ohmic resistance")]
        public Parameter MOS1drainResistance { get; } = new Parameter();
        [NameAttribute("rs"), InfoAttribute("Source ohmic resistance")]
        public Parameter MOS1sourceResistance { get; } = new Parameter();
        [NameAttribute("cbd"), InfoAttribute("B-D junction capacitance")]
        public Parameter MOS1capBD { get; } = new Parameter();
        [NameAttribute("cbs"), InfoAttribute("B-S junction capacitance")]
        public Parameter MOS1capBS { get; } = new Parameter();
        [NameAttribute("is"), InfoAttribute("Bulk junction sat. current")]
        public Parameter MOS1jctSatCur { get; } = new Parameter(1e-14);
        [NameAttribute("pb"), InfoAttribute("Bulk junction potential")]
        public Parameter MOS1bulkJctPotential { get; } = new Parameter(.8);
        [NameAttribute("cgso"), InfoAttribute("Gate-source overlap cap.")]
        public Parameter MOS1gateSourceOverlapCapFactor { get; } = new Parameter();
        [NameAttribute("cgdo"), InfoAttribute("Gate-drain overlap cap.")]
        public Parameter MOS1gateDrainOverlapCapFactor { get; } = new Parameter();
        [NameAttribute("cgbo"), InfoAttribute("Gate-bulk overlap cap.")]
        public Parameter MOS1gateBulkOverlapCapFactor { get; } = new Parameter();
        [NameAttribute("cj"), InfoAttribute("Bottom junction cap per area")]
        public Parameter MOS1bulkCapFactor { get; } = new Parameter();
        [NameAttribute("mj"), InfoAttribute("Bottom grading coefficient")]
        public Parameter MOS1bulkJctBotGradingCoeff { get; } = new Parameter(.5);
        [NameAttribute("cjsw"), InfoAttribute("Side junction cap per area")]
        public Parameter MOS1sideWallCapFactor { get; } = new Parameter();
        [NameAttribute("mjsw"), InfoAttribute("Side grading coefficient")]
        public Parameter MOS1bulkJctSideGradingCoeff { get; } = new Parameter(.5);
        [NameAttribute("js"), InfoAttribute("Bulk jct. sat. current density")]
        public Parameter MOS1jctSatCurDensity { get; } = new Parameter();
        [NameAttribute("tox"), InfoAttribute("Oxide thickness")]
        public Parameter MOS1oxideThickness { get; } = new Parameter();
        [NameAttribute("ld"), InfoAttribute("Lateral diffusion")]
        public Parameter MOS1latDiff { get; } = new Parameter();
        [NameAttribute("rsh"), InfoAttribute("Sheet resistance")]
        public Parameter MOS1sheetResistance { get; } = new Parameter();
        [NameAttribute("u0"), NameAttribute("uo"), InfoAttribute("Surface mobility")]
        public Parameter MOS1surfaceMobility { get; } = new Parameter();
        [NameAttribute("fc"), InfoAttribute("Forward bias jct. fit parm.")]
        public Parameter MOS1fwdCapDepCoeff { get; } = new Parameter(.5);
        [NameAttribute("nss"), InfoAttribute("Surface state density")]
        public Parameter MOS1surfaceStateDensity { get; } = new Parameter();
        [NameAttribute("nsub"), InfoAttribute("Substrate doping")]
        public Parameter MOS1substrateDoping { get; } = new Parameter();
        [NameAttribute("tpg"), InfoAttribute("Gate type")]
        public Parameter MOS1gateType { get; } = new Parameter();

        /// <summary>
        /// Methods
        /// </summary>
        [NameAttribute("nmos"), InfoAttribute("N type MOSfet model")]
        public void SetNMOS(bool value)
        {
            if (value)
                MOS1type = 1.0;
        }
        [NameAttribute("pmos"), InfoAttribute("P type MOSfet model")]
        public void SetPMOS(bool value)
        {
            if (value)
                MOS1type = -1.0;
        }
        [NameAttribute("type"), InfoAttribute("N-channel or P-channel MOS")]
        public string GetTYPE()
        {
            if (MOS1type > 0)
                return "nmos";
            return "pmos";
        }
        public double MOS1type { get; protected set; } = 1.0;

    }
}
