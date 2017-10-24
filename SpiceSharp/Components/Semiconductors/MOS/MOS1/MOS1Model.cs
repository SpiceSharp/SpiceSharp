using SpiceSharp.Circuits;
using SpiceSharp.Parameters;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A model for a <see cref="MOS1"/>
    /// </summary>
    public class MOS1Model : CircuitModel<MOS1Model>
    {
        /// <summary>
        /// Register default behaviours
        /// </summary>
        static MOS1Model()
        {
            Behaviours.Behaviours.RegisterBehaviour(typeof(MOS1Model), typeof(ComponentBehaviours.MOS1ModelTemperatureBehaviour));
        }

        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("tnom"), SpiceInfo("Parameter measurement temperature")]
        public double MOS1_TNOM
        {
            get => MOS1tnom - Circuit.CONSTCtoK;
            set => MOS1tnom.Set(value + Circuit.CONSTCtoK);
        }
        public Parameter MOS1tnom { get; } = new Parameter();
        [SpiceName("vto"), SpiceName("vt0"), SpiceInfo("Threshold voltage")]
        public Parameter MOS1vt0 { get; } = new Parameter();
        [SpiceName("kp"), SpiceInfo("Transconductance parameter")]
        public Parameter MOS1transconductance { get; } = new Parameter(2e-5);
        [SpiceName("gamma"), SpiceInfo("Bulk threshold parameter")]
        public Parameter MOS1gamma { get; } = new Parameter();
        [SpiceName("phi"), SpiceInfo("Surface potential")]
        public Parameter MOS1phi { get; } = new Parameter(.6);
        [SpiceName("lambda"), SpiceInfo("Channel length modulation")]
        public Parameter MOS1lambda { get; } = new Parameter();
        [SpiceName("rd"), SpiceInfo("Drain ohmic resistance")]
        public Parameter MOS1drainResistance { get; } = new Parameter();
        [SpiceName("rs"), SpiceInfo("Source ohmic resistance")]
        public Parameter MOS1sourceResistance { get; } = new Parameter();
        [SpiceName("cbd"), SpiceInfo("B-D junction capacitance")]
        public Parameter MOS1capBD { get; } = new Parameter();
        [SpiceName("cbs"), SpiceInfo("B-S junction capacitance")]
        public Parameter MOS1capBS { get; } = new Parameter();
        [SpiceName("is"), SpiceInfo("Bulk junction sat. current")]
        public Parameter MOS1jctSatCur { get; } = new Parameter(1e-14);
        [SpiceName("pb"), SpiceInfo("Bulk junction potential")]
        public Parameter MOS1bulkJctPotential { get; } = new Parameter(.8);
        [SpiceName("cgso"), SpiceInfo("Gate-source overlap cap.")]
        public Parameter MOS1gateSourceOverlapCapFactor { get; } = new Parameter();
        [SpiceName("cgdo"), SpiceInfo("Gate-drain overlap cap.")]
        public Parameter MOS1gateDrainOverlapCapFactor { get; } = new Parameter();
        [SpiceName("cgbo"), SpiceInfo("Gate-bulk overlap cap.")]
        public Parameter MOS1gateBulkOverlapCapFactor { get; } = new Parameter();
        [SpiceName("cj"), SpiceInfo("Bottom junction cap per area")]
        public Parameter MOS1bulkCapFactor { get; } = new Parameter();
        [SpiceName("mj"), SpiceInfo("Bottom grading coefficient")]
        public Parameter MOS1bulkJctBotGradingCoeff { get; } = new Parameter(.5);
        [SpiceName("cjsw"), SpiceInfo("Side junction cap per area")]
        public Parameter MOS1sideWallCapFactor { get; } = new Parameter();
        [SpiceName("mjsw"), SpiceInfo("Side grading coefficient")]
        public Parameter MOS1bulkJctSideGradingCoeff { get; } = new Parameter(.5);
        [SpiceName("js"), SpiceInfo("Bulk jct. sat. current density")]
        public Parameter MOS1jctSatCurDensity { get; } = new Parameter();
        [SpiceName("tox"), SpiceInfo("Oxide thickness")]
        public Parameter MOS1oxideThickness { get; } = new Parameter();
        [SpiceName("ld"), SpiceInfo("Lateral diffusion")]
        public Parameter MOS1latDiff { get; } = new Parameter();
        [SpiceName("rsh"), SpiceInfo("Sheet resistance")]
        public Parameter MOS1sheetResistance { get; } = new Parameter();
        [SpiceName("u0"), SpiceName("uo"), SpiceInfo("Surface mobility")]
        public Parameter MOS1surfaceMobility { get; } = new Parameter();
        [SpiceName("fc"), SpiceInfo("Forward bias jct. fit parm.")]
        public Parameter MOS1fwdCapDepCoeff { get; } = new Parameter(.5);
        [SpiceName("nss"), SpiceInfo("Surface state density")]
        public Parameter MOS1surfaceStateDensity { get; } = new Parameter();
        [SpiceName("nsub"), SpiceInfo("Substrate doping")]
        public Parameter MOS1substrateDoping { get; } = new Parameter();
        [SpiceName("tpg"), SpiceInfo("Gate type")]
        public Parameter MOS1gateType { get; } = new Parameter();
        [SpiceName("kf"), SpiceInfo("Flicker noise coefficient")]
        public Parameter MOS1fNcoef { get; } = new Parameter();
        [SpiceName("af"), SpiceInfo("Flicker noise exponent")]
        public Parameter MOS1fNexp { get; } = new Parameter(1);

        /// <summary>
        /// Methods
        /// </summary>
        [SpiceName("nmos"), SpiceInfo("N type MOSfet model")]
        public void SetNMOS(bool value)
        {
            if (value)
                MOS1type = 1.0;
        }
        [SpiceName("pmos"), SpiceInfo("P type MOSfet model")]
        public void SetPMOS(bool value)
        {
            if (value)
                MOS1type = -1.0;
        }
        [SpiceName("type"), SpiceInfo("N-channel or P-channel MOS")]
        public string GetTYPE(Circuit ckt)
        {
            if (MOS1type > 0)
                return "nmos";
            else
                return "pmos";
        }

        /// <summary>
        /// Shared parameters
        /// </summary>
        internal double fact1, vtnom, egfet1, pbfact1;

        /// <summary>
        /// Extra variables
        /// </summary>
        public double MOS1type { get; internal set; } = 1.0;
        public double MOS1oxideCapFactor { get; internal set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the device</param>
        public MOS1Model(CircuitIdentifier name) : base(name)
        {
        }
    }
}
