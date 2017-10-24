using SpiceSharp.Circuits;
using SpiceSharp.Parameters;

namespace SpiceSharp.Components
{
    /// <summary>
    /// The BSIM1 Model
    /// </summary>
    public class BSIM1Model : CircuitModel<BSIM1Model>
    {
        /// <summary>
        /// Register default behaviours
        /// </summary>
        static BSIM1Model()
        {
            Behaviours.Behaviours.RegisterBehaviour(typeof(BSIM1Model), typeof(ComponentBehaviours.BSIM1ModelTemperatureBehaviour));
        }

        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("vfb"), SpiceInfo("Flat band voltage")]
        public Parameter B1vfb0 { get; } = new Parameter();
        [SpiceName("lvfb"), SpiceInfo("Length dependence of vfb")]
        public Parameter B1vfbL { get; } = new Parameter();
        [SpiceName("wvfb"), SpiceInfo("Width dependence of vfb")]
        public Parameter B1vfbW { get; } = new Parameter();
        [SpiceName("phi"), SpiceInfo("Strong inversion surface potential ")]
        public Parameter B1phi0 { get; } = new Parameter();
        [SpiceName("lphi"), SpiceInfo("Length dependence of phi")]
        public Parameter B1phiL { get; } = new Parameter();
        [SpiceName("wphi"), SpiceInfo("Width dependence of phi")]
        public Parameter B1phiW { get; } = new Parameter();
        [SpiceName("k1"), SpiceInfo("Bulk effect coefficient 1")]
        public Parameter B1K10 { get; } = new Parameter();
        [SpiceName("lk1"), SpiceInfo("Length dependence of k1")]
        public Parameter B1K1L { get; } = new Parameter();
        [SpiceName("wk1"), SpiceInfo("Width dependence of k1")]
        public Parameter B1K1W { get; } = new Parameter();
        [SpiceName("k2"), SpiceInfo("Bulk effect coefficient 2")]
        public Parameter B1K20 { get; } = new Parameter();
        [SpiceName("lk2"), SpiceInfo("Length dependence of k2")]
        public Parameter B1K2L { get; } = new Parameter();
        [SpiceName("wk2"), SpiceInfo("Width dependence of k2")]
        public Parameter B1K2W { get; } = new Parameter();
        [SpiceName("eta"), SpiceInfo("VDS dependence of threshold voltage")]
        public Parameter B1eta0 { get; } = new Parameter();
        [SpiceName("leta"), SpiceInfo("Length dependence of eta")]
        public Parameter B1etaL { get; } = new Parameter();
        [SpiceName("weta"), SpiceInfo("Width dependence of eta")]
        public Parameter B1etaW { get; } = new Parameter();
        [SpiceName("x2e"), SpiceInfo("VBS dependence of eta")]
        public Parameter B1etaB0 { get; } = new Parameter();
        [SpiceName("lx2e"), SpiceInfo("Length dependence of x2e")]
        public Parameter B1etaBl { get; } = new Parameter();
        [SpiceName("wx2e"), SpiceInfo("Width dependence of x2e")]
        public Parameter B1etaBw { get; } = new Parameter();
        [SpiceName("x3e"), SpiceInfo("VDS dependence of eta")]
        public Parameter B1etaD0 { get; } = new Parameter();
        [SpiceName("lx3e"), SpiceInfo("Length dependence of x3e")]
        public Parameter B1etaDl { get; } = new Parameter();
        [SpiceName("wx3e"), SpiceInfo("Width dependence of x3e")]
        public Parameter B1etaDw { get; } = new Parameter();
        [SpiceName("dl"), SpiceInfo("Channel length reduction in um")]
        public Parameter B1deltaL { get; } = new Parameter();
        [SpiceName("dw"), SpiceInfo("Channel width reduction in um")]
        public Parameter B1deltaW { get; } = new Parameter();
        [SpiceName("muz"), SpiceInfo("Zero field mobility at VDS=0 VGS=VTH")]
        public Parameter B1mobZero { get; } = new Parameter();
        [SpiceName("x2mz"), SpiceInfo("VBS dependence of muz")]
        public Parameter B1mobZeroB0 { get; } = new Parameter();
        [SpiceName("lx2mz"), SpiceInfo("Length dependence of x2mz")]
        public Parameter B1mobZeroBl { get; } = new Parameter();
        [SpiceName("wx2mz"), SpiceInfo("Width dependence of x2mz")]
        public Parameter B1mobZeroBw { get; } = new Parameter();
        [SpiceName("mus"), SpiceInfo("Mobility at VDS=VDD VGS=VTH, channel length modulation")]
        public Parameter B1mobVdd0 { get; } = new Parameter();
        [SpiceName("lmus"), SpiceInfo("Length dependence of mus")]
        public Parameter B1mobVddl { get; } = new Parameter();
        [SpiceName("wmus"), SpiceInfo("Width dependence of mus")]
        public Parameter B1mobVddw { get; } = new Parameter();
        [SpiceName("x2ms"), SpiceInfo("VBS dependence of mus")]
        public Parameter B1mobVddB0 { get; } = new Parameter();
        [SpiceName("lx2ms"), SpiceInfo("Length dependence of x2ms")]
        public Parameter B1mobVddBl { get; } = new Parameter();
        [SpiceName("wx2ms"), SpiceInfo("Width dependence of x2ms")]
        public Parameter B1mobVddBw { get; } = new Parameter();
        [SpiceName("x3ms"), SpiceInfo("VDS dependence of mus")]
        public Parameter B1mobVddD0 { get; } = new Parameter();
        [SpiceName("lx3ms"), SpiceInfo("Length dependence of x3ms")]
        public Parameter B1mobVddDl { get; } = new Parameter();
        [SpiceName("wx3ms"), SpiceInfo("Width dependence of x3ms")]
        public Parameter B1mobVddDw { get; } = new Parameter();
        [SpiceName("u0"), SpiceInfo("VGS dependence of mobility")]
        public Parameter B1ugs0 { get; } = new Parameter();
        [SpiceName("lu0"), SpiceInfo("Length dependence of u0")]
        public Parameter B1ugsL { get; } = new Parameter();
        [SpiceName("wu0"), SpiceInfo("Width dependence of u0")]
        public Parameter B1ugsW { get; } = new Parameter();
        [SpiceName("x2u0"), SpiceInfo("VBS dependence of u0")]
        public Parameter B1ugsB0 { get; } = new Parameter();
        [SpiceName("lx2u0"), SpiceInfo("Length dependence of x2u0")]
        public Parameter B1ugsBL { get; } = new Parameter();
        [SpiceName("wx2u0"), SpiceInfo("Width dependence of x2u0")]
        public Parameter B1ugsBW { get; } = new Parameter();
        [SpiceName("u1"), SpiceInfo("VDS depence of mobility, velocity saturation")]
        public Parameter B1uds0 { get; } = new Parameter();
        [SpiceName("lu1"), SpiceInfo("Length dependence of u1")]
        public Parameter B1udsL { get; } = new Parameter();
        [SpiceName("wu1"), SpiceInfo("Width dependence of u1")]
        public Parameter B1udsW { get; } = new Parameter();
        [SpiceName("x2u1"), SpiceInfo("VBS depence of u1")]
        public Parameter B1udsB0 { get; } = new Parameter();
        [SpiceName("lx2u1"), SpiceInfo("Length depence of x2u1")]
        public Parameter B1udsBL { get; } = new Parameter();
        [SpiceName("wx2u1"), SpiceInfo("Width depence of x2u1")]
        public Parameter B1udsBW { get; } = new Parameter();
        [SpiceName("x3u1"), SpiceInfo("VDS depence of u1")]
        public Parameter B1udsD0 { get; } = new Parameter();
        [SpiceName("lx3u1"), SpiceInfo("Length dependence of x3u1")]
        public Parameter B1udsDL { get; } = new Parameter();
        [SpiceName("wx3u1"), SpiceInfo("Width depence of x3u1")]
        public Parameter B1udsDW { get; } = new Parameter();
        [SpiceName("n0"), SpiceInfo("Subthreshold slope")]
        public Parameter B1subthSlope0 { get; } = new Parameter();
        [SpiceName("ln0"), SpiceInfo("Length dependence of n0")]
        public Parameter B1subthSlopeL { get; } = new Parameter();
        [SpiceName("wn0"), SpiceInfo("Width dependence of n0")]
        public Parameter B1subthSlopeW { get; } = new Parameter();
        [SpiceName("nb"), SpiceInfo("VBS dependence of subthreshold slope")]
        public Parameter B1subthSlopeB0 { get; } = new Parameter();
        [SpiceName("lnb"), SpiceInfo("Length dependence of nb")]
        public Parameter B1subthSlopeBL { get; } = new Parameter();
        [SpiceName("wnb"), SpiceInfo("Width dependence of nb")]
        public Parameter B1subthSlopeBW { get; } = new Parameter();
        [SpiceName("nd"), SpiceInfo("VDS dependence of subthreshold slope")]
        public Parameter B1subthSlopeD0 { get; } = new Parameter();
        [SpiceName("lnd"), SpiceInfo("Length dependence of nd")]
        public Parameter B1subthSlopeDL { get; } = new Parameter();
        [SpiceName("wnd"), SpiceInfo("Width dependence of nd")]
        public Parameter B1subthSlopeDW { get; } = new Parameter();
        [SpiceName("tox"), SpiceInfo("Gate oxide thickness in um")]
        public Parameter B1oxideThickness { get; } = new Parameter();
        [SpiceName("temp"), SpiceInfo("Temperature")]
        public double B1_TEMP
        {
            get => B1temp - Circuit.CONSTCtoK;
            set => B1temp.Set(value + Circuit.CONSTCtoK);
        }
        public Parameter B1temp { get; } = new Parameter(300.15);
        [SpiceName("vdd"), SpiceInfo("Supply voltage to specify mus")]
        public Parameter B1vdd { get; } = new Parameter();
        [SpiceName("cgso"), SpiceInfo("Gate source overlap capacitance per unit channel width(m)")]
        public Parameter B1gateSourceOverlapCap { get; } = new Parameter();
        [SpiceName("cgdo"), SpiceInfo("Gate drain overlap capacitance per unit channel width(m)")]
        public Parameter B1gateDrainOverlapCap { get; } = new Parameter();
        [SpiceName("cgbo"), SpiceInfo("Gate bulk overlap capacitance per unit channel length(m)")]
        public Parameter B1gateBulkOverlapCap { get; } = new Parameter();
        [SpiceName("xpart"), SpiceInfo("Flag for channel charge partitioning")]
        public Parameter B1channelChargePartitionFlag { get; } = new Parameter();
        [SpiceName("rsh"), SpiceInfo("Source drain diffusion sheet resistance in ohm per square")]
        public Parameter B1sheetResistance { get; } = new Parameter();
        [SpiceName("js"), SpiceInfo("Source drain junction saturation current per unit area")]
        public Parameter B1jctSatCurDensity { get; } = new Parameter();
        [SpiceName("pb"), SpiceInfo("Source drain junction built in potential")]
        public Parameter B1bulkJctPotential { get; } = new Parameter();
        [SpiceName("mj"), SpiceInfo("Source drain bottom junction capacitance grading coefficient")]
        public Parameter B1bulkJctBotGradingCoeff { get; } = new Parameter();
        [SpiceName("pbsw"), SpiceInfo("Source drain side junction capacitance built in potential")]
        public Parameter B1sidewallJctPotential { get; } = new Parameter();
        [SpiceName("mjsw"), SpiceInfo("Source drain side junction capacitance grading coefficient")]
        public Parameter B1bulkJctSideGradingCoeff { get; } = new Parameter();
        [SpiceName("cj"), SpiceInfo("Source drain bottom junction capacitance per unit area")]
        public Parameter B1unitAreaJctCap { get; } = new Parameter();
        [SpiceName("cjsw"), SpiceInfo("Source drain side junction capacitance per unit area")]
        public Parameter B1unitLengthSidewallJctCap { get; } = new Parameter();
        [SpiceName("wdf"), SpiceInfo("Default width of source drain diffusion in um")]
        public Parameter B1defaultWidth { get; } = new Parameter();
        [SpiceName("dell"), SpiceInfo("Length reduction of source drain diffusion")]
        public Parameter B1deltaLength { get; } = new Parameter();

        /// <summary>
        /// Methods
        /// </summary>
        [SpiceName("nmos"), SpiceInfo("Flag to indicate NMOS")]
        public void SetNMOS(bool value) { B1type = 1; }
        [SpiceName("pmos"), SpiceInfo("Flag to indicate PMOS")]
        public void SetPMOS(bool value) { B1type = -1; }

        /// <summary>
        /// Shared parameters
        /// </summary>
        internal double Cox;

        /// <summary>
        /// Extra variables
        /// </summary>
        public int B1type { get; internal set; } = 1;
        public double B1Cox { get; internal set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the device</param>
        public BSIM1Model(CircuitIdentifier name) : base(name)
        {
        }
    }
}
