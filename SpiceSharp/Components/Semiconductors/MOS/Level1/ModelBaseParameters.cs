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
        [PropertyNameAttribute("tnom"), PropertyInfoAttribute("Parameter measurement temperature")]
        public double MOS1_TNOM
        {
            get => MOS1tnom - Circuit.CONSTCtoK;
            set => MOS1tnom.Set(value + Circuit.CONSTCtoK);
        }
        public Parameter MOS1tnom { get; } = new Parameter();
        [PropertyNameAttribute("vto"), PropertyNameAttribute("vt0"), PropertyInfoAttribute("Threshold voltage")]
        public Parameter MOS1vt0 { get; } = new Parameter();
        [PropertyNameAttribute("kp"), PropertyInfoAttribute("Transconductance parameter")]
        public Parameter MOS1transconductance { get; } = new Parameter(2e-5);
        [PropertyNameAttribute("gamma"), PropertyInfoAttribute("Bulk threshold parameter")]
        public Parameter MOS1gamma { get; } = new Parameter();
        [PropertyNameAttribute("phi"), PropertyInfoAttribute("Surface potential")]
        public Parameter MOS1phi { get; } = new Parameter(.6);
        [PropertyNameAttribute("lambda"), PropertyInfoAttribute("Channel length modulation")]
        public Parameter MOS1lambda { get; } = new Parameter();
        [PropertyNameAttribute("rd"), PropertyInfoAttribute("Drain ohmic resistance")]
        public Parameter MOS1drainResistance { get; } = new Parameter();
        [PropertyNameAttribute("rs"), PropertyInfoAttribute("Source ohmic resistance")]
        public Parameter MOS1sourceResistance { get; } = new Parameter();
        [PropertyNameAttribute("cbd"), PropertyInfoAttribute("B-D junction capacitance")]
        public Parameter MOS1capBD { get; } = new Parameter();
        [PropertyNameAttribute("cbs"), PropertyInfoAttribute("B-S junction capacitance")]
        public Parameter MOS1capBS { get; } = new Parameter();
        [PropertyNameAttribute("is"), PropertyInfoAttribute("Bulk junction sat. current")]
        public Parameter MOS1jctSatCur { get; } = new Parameter(1e-14);
        [PropertyNameAttribute("pb"), PropertyInfoAttribute("Bulk junction potential")]
        public Parameter MOS1bulkJctPotential { get; } = new Parameter(.8);
        [PropertyNameAttribute("cgso"), PropertyInfoAttribute("Gate-source overlap cap.")]
        public Parameter MOS1gateSourceOverlapCapFactor { get; } = new Parameter();
        [PropertyNameAttribute("cgdo"), PropertyInfoAttribute("Gate-drain overlap cap.")]
        public Parameter MOS1gateDrainOverlapCapFactor { get; } = new Parameter();
        [PropertyNameAttribute("cgbo"), PropertyInfoAttribute("Gate-bulk overlap cap.")]
        public Parameter MOS1gateBulkOverlapCapFactor { get; } = new Parameter();
        [PropertyNameAttribute("cj"), PropertyInfoAttribute("Bottom junction cap per area")]
        public Parameter MOS1bulkCapFactor { get; } = new Parameter();
        [PropertyNameAttribute("mj"), PropertyInfoAttribute("Bottom grading coefficient")]
        public Parameter MOS1bulkJctBotGradingCoeff { get; } = new Parameter(.5);
        [PropertyNameAttribute("cjsw"), PropertyInfoAttribute("Side junction cap per area")]
        public Parameter MOS1sideWallCapFactor { get; } = new Parameter();
        [PropertyNameAttribute("mjsw"), PropertyInfoAttribute("Side grading coefficient")]
        public Parameter MOS1bulkJctSideGradingCoeff { get; } = new Parameter(.5);
        [PropertyNameAttribute("js"), PropertyInfoAttribute("Bulk jct. sat. current density")]
        public Parameter MOS1jctSatCurDensity { get; } = new Parameter();
        [PropertyNameAttribute("tox"), PropertyInfoAttribute("Oxide thickness")]
        public Parameter MOS1oxideThickness { get; } = new Parameter();
        [PropertyNameAttribute("ld"), PropertyInfoAttribute("Lateral diffusion")]
        public Parameter MOS1latDiff { get; } = new Parameter();
        [PropertyNameAttribute("rsh"), PropertyInfoAttribute("Sheet resistance")]
        public Parameter MOS1sheetResistance { get; } = new Parameter();
        [PropertyNameAttribute("u0"), PropertyNameAttribute("uo"), PropertyInfoAttribute("Surface mobility")]
        public Parameter MOS1surfaceMobility { get; } = new Parameter();
        [PropertyNameAttribute("fc"), PropertyInfoAttribute("Forward bias jct. fit parm.")]
        public Parameter MOS1fwdCapDepCoeff { get; } = new Parameter(.5);
        [PropertyNameAttribute("nss"), PropertyInfoAttribute("Surface state density")]
        public Parameter MOS1surfaceStateDensity { get; } = new Parameter();
        [PropertyNameAttribute("nsub"), PropertyInfoAttribute("Substrate doping")]
        public Parameter MOS1substrateDoping { get; } = new Parameter();
        [PropertyNameAttribute("tpg"), PropertyInfoAttribute("Gate type")]
        public Parameter MOS1gateType { get; } = new Parameter();

        /// <summary>
        /// Methods
        /// </summary>
        [PropertyNameAttribute("nmos"), PropertyInfoAttribute("N type MOSfet model")]
        public void SetNMOS(bool value)
        {
            if (value)
                MOS1type = 1.0;
        }
        [PropertyNameAttribute("pmos"), PropertyInfoAttribute("P type MOSfet model")]
        public void SetPMOS(bool value)
        {
            if (value)
                MOS1type = -1.0;
        }
        [PropertyNameAttribute("type"), PropertyInfoAttribute("N-channel or P-channel MOS")]
        public string GetTYPE()
        {
            if (MOS1type > 0)
                return "nmos";
            return "pmos";
        }
        public double MOS1type { get; protected set; } = 1.0;

    }
}
