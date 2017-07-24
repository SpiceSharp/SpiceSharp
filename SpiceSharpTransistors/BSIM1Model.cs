using System;
using SpiceSharp.Circuits;
using SpiceSharp.Diagnostics;
using SpiceSharp.Parameters;
using SpiceSharp.Components.Transistors;

namespace SpiceSharp.Components
{
    public class BSIM1Model : CircuitModel
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("vfb"), SpiceInfo("Flat band voltage")]
        public Parameter<double> B1vfb0 { get; } = new Parameter<double>();
        [SpiceName("lvfb"), SpiceInfo("Length dependence of vfb")]
        public Parameter<double> B1vfbL { get; } = new Parameter<double>();
        [SpiceName("wvfb"), SpiceInfo("Width dependence of vfb")]
        public Parameter<double> B1vfbW { get; } = new Parameter<double>();
        [SpiceName("phi"), SpiceInfo("Strong inversion surface potential ")]
        public Parameter<double> B1phi0 { get; } = new Parameter<double>();
        [SpiceName("lphi"), SpiceInfo("Length dependence of phi")]
        public Parameter<double> B1phiL { get; } = new Parameter<double>();
        [SpiceName("wphi"), SpiceInfo("Width dependence of phi")]
        public Parameter<double> B1phiW { get; } = new Parameter<double>();
        [SpiceName("k1"), SpiceInfo("Bulk effect coefficient 1")]
        public Parameter<double> B1K10 { get; } = new Parameter<double>();
        [SpiceName("lk1"), SpiceInfo("Length dependence of k1")]
        public Parameter<double> B1K1L { get; } = new Parameter<double>();
        [SpiceName("wk1"), SpiceInfo("Width dependence of k1")]
        public Parameter<double> B1K1W { get; } = new Parameter<double>();
        [SpiceName("k2"), SpiceInfo("Bulk effect coefficient 2")]
        public Parameter<double> B1K20 { get; } = new Parameter<double>();
        [SpiceName("lk2"), SpiceInfo("Length dependence of k2")]
        public Parameter<double> B1K2L { get; } = new Parameter<double>();
        [SpiceName("wk2"), SpiceInfo("Width dependence of k2")]
        public Parameter<double> B1K2W { get; } = new Parameter<double>();
        [SpiceName("eta"), SpiceInfo("VDS dependence of threshold voltage")]
        public Parameter<double> B1eta0 { get; } = new Parameter<double>();
        [SpiceName("leta"), SpiceInfo("Length dependence of eta")]
        public Parameter<double> B1etaL { get; } = new Parameter<double>();
        [SpiceName("weta"), SpiceInfo("Width dependence of eta")]
        public Parameter<double> B1etaW { get; } = new Parameter<double>();
        [SpiceName("x2e"), SpiceInfo("VBS dependence of eta")]
        public Parameter<double> B1etaB0 { get; } = new Parameter<double>();
        [SpiceName("lx2e"), SpiceInfo("Length dependence of x2e")]
        public Parameter<double> B1etaBl { get; } = new Parameter<double>();
        [SpiceName("wx2e"), SpiceInfo("Width dependence of x2e")]
        public Parameter<double> B1etaBw { get; } = new Parameter<double>();
        [SpiceName("x3e"), SpiceInfo("VDS dependence of eta")]
        public Parameter<double> B1etaD0 { get; } = new Parameter<double>();
        [SpiceName("lx3e"), SpiceInfo("Length dependence of x3e")]
        public Parameter<double> B1etaDl { get; } = new Parameter<double>();
        [SpiceName("wx3e"), SpiceInfo("Width dependence of x3e")]
        public Parameter<double> B1etaDw { get; } = new Parameter<double>();
        [SpiceName("dl"), SpiceInfo("Channel length reduction in um")]
        public Parameter<double> B1deltaL { get; } = new Parameter<double>();
        [SpiceName("dw"), SpiceInfo("Channel width reduction in um")]
        public Parameter<double> B1deltaW { get; } = new Parameter<double>();
        [SpiceName("muz"), SpiceInfo("Zero field mobility at VDS=0 VGS=VTH")]
        public Parameter<double> B1mobZero { get; } = new Parameter<double>();
        [SpiceName("x2mz"), SpiceInfo("VBS dependence of muz")]
        public Parameter<double> B1mobZeroB0 { get; } = new Parameter<double>();
        [SpiceName("lx2mz"), SpiceInfo("Length dependence of x2mz")]
        public Parameter<double> B1mobZeroBl { get; } = new Parameter<double>();
        [SpiceName("wx2mz"), SpiceInfo("Width dependence of x2mz")]
        public Parameter<double> B1mobZeroBw { get; } = new Parameter<double>();
        [SpiceName("mus"), SpiceInfo("Mobility at VDS=VDD VGS=VTH, channel length modulation")]
        public Parameter<double> B1mobVdd0 { get; } = new Parameter<double>();
        [SpiceName("lmus"), SpiceInfo("Length dependence of mus")]
        public Parameter<double> B1mobVddl { get; } = new Parameter<double>();
        [SpiceName("wmus"), SpiceInfo("Width dependence of mus")]
        public Parameter<double> B1mobVddw { get; } = new Parameter<double>();
        [SpiceName("x2ms"), SpiceInfo("VBS dependence of mus")]
        public Parameter<double> B1mobVddB0 { get; } = new Parameter<double>();
        [SpiceName("lx2ms"), SpiceInfo("Length dependence of x2ms")]
        public Parameter<double> B1mobVddBl { get; } = new Parameter<double>();
        [SpiceName("wx2ms"), SpiceInfo("Width dependence of x2ms")]
        public Parameter<double> B1mobVddBw { get; } = new Parameter<double>();
        [SpiceName("x3ms"), SpiceInfo("VDS dependence of mus")]
        public Parameter<double> B1mobVddD0 { get; } = new Parameter<double>();
        [SpiceName("lx3ms"), SpiceInfo("Length dependence of x3ms")]
        public Parameter<double> B1mobVddDl { get; } = new Parameter<double>();
        [SpiceName("wx3ms"), SpiceInfo("Width dependence of x3ms")]
        public Parameter<double> B1mobVddDw { get; } = new Parameter<double>();
        [SpiceName("u0"), SpiceInfo("VGS dependence of mobility")]
        public Parameter<double> B1ugs0 { get; } = new Parameter<double>();
        [SpiceName("lu0"), SpiceInfo("Length dependence of u0")]
        public Parameter<double> B1ugsL { get; } = new Parameter<double>();
        [SpiceName("wu0"), SpiceInfo("Width dependence of u0")]
        public Parameter<double> B1ugsW { get; } = new Parameter<double>();
        [SpiceName("x2u0"), SpiceInfo("VBS dependence of u0")]
        public Parameter<double> B1ugsB0 { get; } = new Parameter<double>();
        [SpiceName("lx2u0"), SpiceInfo("Length dependence of x2u0")]
        public Parameter<double> B1ugsBL { get; } = new Parameter<double>();
        [SpiceName("wx2u0"), SpiceInfo("Width dependence of x2u0")]
        public Parameter<double> B1ugsBW { get; } = new Parameter<double>();
        [SpiceName("u1"), SpiceInfo("VDS depence of mobility, velocity saturation")]
        public Parameter<double> B1uds0 { get; } = new Parameter<double>();
        [SpiceName("lu1"), SpiceInfo("Length dependence of u1")]
        public Parameter<double> B1udsL { get; } = new Parameter<double>();
        [SpiceName("wu1"), SpiceInfo("Width dependence of u1")]
        public Parameter<double> B1udsW { get; } = new Parameter<double>();
        [SpiceName("x2u1"), SpiceInfo("VBS depence of u1")]
        public Parameter<double> B1udsB0 { get; } = new Parameter<double>();
        [SpiceName("lx2u1"), SpiceInfo("Length depence of x2u1")]
        public Parameter<double> B1udsBL { get; } = new Parameter<double>();
        [SpiceName("wx2u1"), SpiceInfo("Width depence of x2u1")]
        public Parameter<double> B1udsBW { get; } = new Parameter<double>();
        [SpiceName("x3u1"), SpiceInfo("VDS depence of u1")]
        public Parameter<double> B1udsD0 { get; } = new Parameter<double>();
        [SpiceName("lx3u1"), SpiceInfo("Length dependence of x3u1")]
        public Parameter<double> B1udsDL { get; } = new Parameter<double>();
        [SpiceName("wx3u1"), SpiceInfo("Width depence of x3u1")]
        public Parameter<double> B1udsDW { get; } = new Parameter<double>();
        [SpiceName("n0"), SpiceInfo("Subthreshold slope")]
        public Parameter<double> B1subthSlope0 { get; } = new Parameter<double>();
        [SpiceName("ln0"), SpiceInfo("Length dependence of n0")]
        public Parameter<double> B1subthSlopeL { get; } = new Parameter<double>();
        [SpiceName("wn0"), SpiceInfo("Width dependence of n0")]
        public Parameter<double> B1subthSlopeW { get; } = new Parameter<double>();
        [SpiceName("nb"), SpiceInfo("VBS dependence of subthreshold slope")]
        public Parameter<double> B1subthSlopeB0 { get; } = new Parameter<double>();
        [SpiceName("lnb"), SpiceInfo("Length dependence of nb")]
        public Parameter<double> B1subthSlopeBL { get; } = new Parameter<double>();
        [SpiceName("wnb"), SpiceInfo("Width dependence of nb")]
        public Parameter<double> B1subthSlopeBW { get; } = new Parameter<double>();
        [SpiceName("nd"), SpiceInfo("VDS dependence of subthreshold slope")]
        public Parameter<double> B1subthSlopeD0 { get; } = new Parameter<double>();
        [SpiceName("lnd"), SpiceInfo("Length dependence of nd")]
        public Parameter<double> B1subthSlopeDL { get; } = new Parameter<double>();
        [SpiceName("wnd"), SpiceInfo("Width dependence of nd")]
        public Parameter<double> B1subthSlopeDW { get; } = new Parameter<double>();
        [SpiceName("tox"), SpiceInfo("Gate oxide thickness in um")]
        public Parameter<double> B1oxideThickness { get; } = new Parameter<double>();
        [SpiceName("temp"), SpiceInfo("Temperature in degree Celcius")]
        public Parameter<double> B1temp { get; } = new Parameter<double>();
        [SpiceName("vdd"), SpiceInfo("Supply voltage to specify mus")]
        public Parameter<double> B1vdd { get; } = new Parameter<double>();
        [SpiceName("cgso"), SpiceInfo("Gate source overlap capacitance per unit channel width(m)")]
        public Parameter<double> B1gateSourceOverlapCap { get; } = new Parameter<double>();
        [SpiceName("cgdo"), SpiceInfo("Gate drain overlap capacitance per unit channel width(m)")]
        public Parameter<double> B1gateDrainOverlapCap { get; } = new Parameter<double>();
        [SpiceName("cgbo"), SpiceInfo("Gate bulk overlap capacitance per unit channel length(m)")]
        public Parameter<double> B1gateBulkOverlapCap { get; } = new Parameter<double>();
        [SpiceName("xpart"), SpiceInfo("Flag for channel charge partitioning")]
        public Parameter<double> B1channelChargePartitionFlag { get; } = new Parameter<double>();
        [SpiceName("rsh"), SpiceInfo("Source drain diffusion sheet resistance in ohm per square")]
        public Parameter<double> B1sheetResistance { get; } = new Parameter<double>();
        [SpiceName("js"), SpiceInfo("Source drain junction saturation current per unit area")]
        public Parameter<double> B1jctSatCurDensity { get; } = new Parameter<double>();
        [SpiceName("pb"), SpiceInfo("Source drain junction built in potential")]
        public Parameter<double> B1bulkJctPotential { get; } = new Parameter<double>();
        [SpiceName("mj"), SpiceInfo("Source drain bottom junction capacitance grading coefficient")]
        public Parameter<double> B1bulkJctBotGradingCoeff { get; } = new Parameter<double>();
        [SpiceName("pbsw"), SpiceInfo("Source drain side junction capacitance built in potential")]
        public Parameter<double> B1sidewallJctPotential { get; } = new Parameter<double>();
        [SpiceName("mjsw"), SpiceInfo("Source drain side junction capacitance grading coefficient")]
        public Parameter<double> B1bulkJctSideGradingCoeff { get; } = new Parameter<double>();
        [SpiceName("cj"), SpiceInfo("Source drain bottom junction capacitance per unit area")]
        public Parameter<double> B1unitAreaJctCap { get; } = new Parameter<double>();
        [SpiceName("cjsw"), SpiceInfo("Source drain side junction capacitance per unit area")]
        public Parameter<double> B1unitLengthSidewallJctCap { get; } = new Parameter<double>();
        [SpiceName("wdf"), SpiceInfo("Default width of source drain diffusion in um")]
        public Parameter<double> B1defaultWidth { get; } = new Parameter<double>();
        [SpiceName("dell"), SpiceInfo("Length reduction of source drain diffusion")]
        public Parameter<double> B1deltaLength { get; } = new Parameter<double>();

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
        public double Cox { get; private set; }

        /// <summary>
        /// Extra variables
        /// </summary>
        public int B1type { get; private set; } = 1;
        public double B1Cox { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the device</param>
        public BSIM1Model(string name) : base(name)
        {
        }

        /// <summary>
        /// Do temperature-dependent calculations
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Temperature(Circuit ckt)
        {

            /* Default value Processing for B1 MOSFET Models */
            /* Some Limiting for Model Parameters */
            if (B1bulkJctPotential < 0.1)
            {
                B1bulkJctPotential.Value = 0.1;
            }
            if (B1sidewallJctPotential < 0.1)
            {
                B1sidewallJctPotential.Value = 0.1;
            }

            Cox = 3.453e-13 / (B1oxideThickness * 1.0e-4); /* in F / cm *  * 2 */
            B1Cox = Cox; /* unit:  F / cm *  * 2 */
        }
    }
}
