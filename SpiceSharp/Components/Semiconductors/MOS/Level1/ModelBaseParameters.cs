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
        [PropertyName("tnom"), PropertyInfo("Parameter measurement temperature")]
        public double MOS1_TNOM
        {
            get => MOS1tnom - Circuit.CelsiusKelvin;
            set => MOS1tnom.Set(value + Circuit.CelsiusKelvin);
        }
        public Parameter MOS1tnom { get; } = new Parameter();
        [PropertyName("vto"), PropertyName("vt0"), PropertyInfo("Threshold voltage")]
        public Parameter MOS1vt0 { get; } = new Parameter();
        [PropertyName("kp"), PropertyInfo("Transconductance parameter")]
        public Parameter MOS1transconductance { get; } = new Parameter(2e-5);
        [PropertyName("gamma"), PropertyInfo("Bulk threshold parameter")]
        public Parameter MOS1gamma { get; } = new Parameter();
        [PropertyName("phi"), PropertyInfo("Surface potential")]
        public Parameter MOS1phi { get; } = new Parameter(.6);
        [PropertyName("lambda"), PropertyInfo("Channel length modulation")]
        public Parameter MOS1lambda { get; } = new Parameter();
        [PropertyName("rd"), PropertyInfo("Drain ohmic resistance")]
        public Parameter MOS1drainResistance { get; } = new Parameter();
        [PropertyName("rs"), PropertyInfo("Source ohmic resistance")]
        public Parameter MOS1sourceResistance { get; } = new Parameter();
        [PropertyName("cbd"), PropertyInfo("B-D junction capacitance")]
        public Parameter MOS1capBD { get; } = new Parameter();
        [PropertyName("cbs"), PropertyInfo("B-S junction capacitance")]
        public Parameter MOS1capBS { get; } = new Parameter();
        [PropertyName("is"), PropertyInfo("Bulk junction sat. current")]
        public Parameter MOS1jctSatCur { get; } = new Parameter(1e-14);
        [PropertyName("pb"), PropertyInfo("Bulk junction potential")]
        public Parameter MOS1bulkJctPotential { get; } = new Parameter(.8);
        [PropertyName("cgso"), PropertyInfo("Gate-source overlap cap.")]
        public Parameter MOS1gateSourceOverlapCapFactor { get; } = new Parameter();
        [PropertyName("cgdo"), PropertyInfo("Gate-drain overlap cap.")]
        public Parameter MOS1gateDrainOverlapCapFactor { get; } = new Parameter();
        [PropertyName("cgbo"), PropertyInfo("Gate-bulk overlap cap.")]
        public Parameter MOS1gateBulkOverlapCapFactor { get; } = new Parameter();
        [PropertyName("cj"), PropertyInfo("Bottom junction cap per area")]
        public Parameter MOS1bulkCapFactor { get; } = new Parameter();
        [PropertyName("mj"), PropertyInfo("Bottom grading coefficient")]
        public Parameter MOS1bulkJctBotGradingCoeff { get; } = new Parameter(.5);
        [PropertyName("cjsw"), PropertyInfo("Side junction cap per area")]
        public Parameter MOS1sideWallCapFactor { get; } = new Parameter();
        [PropertyName("mjsw"), PropertyInfo("Side grading coefficient")]
        public Parameter MOS1bulkJctSideGradingCoeff { get; } = new Parameter(.5);
        [PropertyName("js"), PropertyInfo("Bulk jct. sat. current density")]
        public Parameter MOS1jctSatCurDensity { get; } = new Parameter();
        [PropertyName("tox"), PropertyInfo("Oxide thickness")]
        public Parameter MOS1oxideThickness { get; } = new Parameter();
        [PropertyName("ld"), PropertyInfo("Lateral diffusion")]
        public Parameter MOS1latDiff { get; } = new Parameter();
        [PropertyName("rsh"), PropertyInfo("Sheet resistance")]
        public Parameter MOS1sheetResistance { get; } = new Parameter();
        [PropertyName("u0"), PropertyName("uo"), PropertyInfo("Surface mobility")]
        public Parameter MOS1surfaceMobility { get; } = new Parameter();
        [PropertyName("fc"), PropertyInfo("Forward bias jct. fit parm.")]
        public Parameter MOS1fwdCapDepCoeff { get; } = new Parameter(.5);
        [PropertyName("nss"), PropertyInfo("Surface state density")]
        public Parameter MOS1surfaceStateDensity { get; } = new Parameter();
        [PropertyName("nsub"), PropertyInfo("Substrate doping")]
        public Parameter MOS1substrateDoping { get; } = new Parameter();
        [PropertyName("tpg"), PropertyInfo("Gate type")]
        public Parameter MOS1gateType { get; } = new Parameter();

        /// <summary>
        /// Methods
        /// </summary>
        [PropertyName("nmos"), PropertyInfo("N type MOSfet model")]
        public void SetNMOS(bool value)
        {
            if (value)
                MOS1type = 1.0;
        }
        [PropertyName("pmos"), PropertyInfo("P type MOSfet model")]
        public void SetPMOS(bool value)
        {
            if (value)
                MOS1type = -1.0;
        }
        [PropertyName("type"), PropertyInfo("N-channel or P-channel MOS")]
        public string GetTYPE()
        {
            if (MOS1type > 0)
                return "nmos";
            return "pmos";
        }
        public double MOS1type { get; protected set; } = 1.0;

    }
}
