using System;
using SpiceSharp.Circuits;
using SpiceSharp.Diagnostics;
using SpiceSharp.Parameters;
using SpiceSharp.Components.Transistors;

namespace SpiceSharp.Components
{
    public class BSIM2Model : CircuitModel
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("vfb"), SpiceInfo("Flat band voltage")]
        public Parameter<double> B2vfb0 { get; } = new Parameter<double>();
        [SpiceName("lvfb"), SpiceInfo("Length dependence of vfb")]
        public Parameter<double> B2vfbL { get; } = new Parameter<double>();
        [SpiceName("wvfb"), SpiceInfo("Width dependence of vfb")]
        public Parameter<double> B2vfbW { get; } = new Parameter<double>();
        [SpiceName("phi"), SpiceInfo("Strong inversion surface potential ")]
        public Parameter<double> B2phi0 { get; } = new Parameter<double>();
        [SpiceName("lphi"), SpiceInfo("Length dependence of phi")]
        public Parameter<double> B2phiL { get; } = new Parameter<double>();
        [SpiceName("wphi"), SpiceInfo("Width dependence of phi")]
        public Parameter<double> B2phiW { get; } = new Parameter<double>();
        [SpiceName("k1"), SpiceInfo("Bulk effect coefficient 1")]
        public Parameter<double> B2k10 { get; } = new Parameter<double>();
        [SpiceName("lk1"), SpiceInfo("Length dependence of k1")]
        public Parameter<double> B2k1L { get; } = new Parameter<double>();
        [SpiceName("wk1"), SpiceInfo("Width dependence of k1")]
        public Parameter<double> B2k1W { get; } = new Parameter<double>();
        [SpiceName("k2"), SpiceInfo("Bulk effect coefficient 2")]
        public Parameter<double> B2k20 { get; } = new Parameter<double>();
        [SpiceName("lk2"), SpiceInfo("Length dependence of k2")]
        public Parameter<double> B2k2L { get; } = new Parameter<double>();
        [SpiceName("wk2"), SpiceInfo("Width dependence of k2")]
        public Parameter<double> B2k2W { get; } = new Parameter<double>();
        [SpiceName("eta0"), SpiceInfo("VDS dependence of threshold voltage at VDD=0")]
        public Parameter<double> B2eta00 { get; } = new Parameter<double>();
        [SpiceName("leta0"), SpiceInfo("Length dependence of eta0")]
        public Parameter<double> B2eta0L { get; } = new Parameter<double>();
        [SpiceName("weta0"), SpiceInfo("Width dependence of eta0")]
        public Parameter<double> B2eta0W { get; } = new Parameter<double>();
        [SpiceName("etab"), SpiceInfo("VBS dependence of eta")]
        public Parameter<double> B2etaB0 { get; } = new Parameter<double>();
        [SpiceName("letab"), SpiceInfo("Length dependence of etab")]
        public Parameter<double> B2etaBL { get; } = new Parameter<double>();
        [SpiceName("wetab"), SpiceInfo("Width dependence of etab")]
        public Parameter<double> B2etaBW { get; } = new Parameter<double>();
        [SpiceName("dl"), SpiceInfo("Channel length reduction in um")]
        public Parameter<double> B2deltaL { get; } = new Parameter<double>();
        [SpiceName("dw"), SpiceInfo("Channel width reduction in um")]
        public Parameter<double> B2deltaW { get; } = new Parameter<double>();
        [SpiceName("mu0"), SpiceInfo("Low-field mobility, at VDS=0 VGS=VTH")]
        public Parameter<double> B2mob00 { get; } = new Parameter<double>();
        [SpiceName("mu0b"), SpiceInfo("VBS dependence of low-field mobility")]
        public Parameter<double> B2mob0B0 { get; } = new Parameter<double>();
        [SpiceName("lmu0b"), SpiceInfo("Length dependence of mu0b")]
        public Parameter<double> B2mob0BL { get; } = new Parameter<double>();
        [SpiceName("wmu0b"), SpiceInfo("Width dependence of mu0b")]
        public Parameter<double> B2mob0BW { get; } = new Parameter<double>();
        [SpiceName("mus0"), SpiceInfo("Mobility at VDS=VDD VGS=VTH")]
        public Parameter<double> B2mobs00 { get; } = new Parameter<double>();
        [SpiceName("lmus0"), SpiceInfo("Length dependence of mus0")]
        public Parameter<double> B2mobs0L { get; } = new Parameter<double>();
        [SpiceName("wmus0"), SpiceInfo("Width dependence of mus")]
        public Parameter<double> B2mobs0W { get; } = new Parameter<double>();
        [SpiceName("musb"), SpiceInfo("VBS dependence of mus")]
        public Parameter<double> B2mobsB0 { get; } = new Parameter<double>();
        [SpiceName("lmusb"), SpiceInfo("Length dependence of musb")]
        public Parameter<double> B2mobsBL { get; } = new Parameter<double>();
        [SpiceName("wmusb"), SpiceInfo("Width dependence of musb")]
        public Parameter<double> B2mobsBW { get; } = new Parameter<double>();
        [SpiceName("mu20"), SpiceInfo("VDS dependence of mu in tanh term")]
        public Parameter<double> B2mob200 { get; } = new Parameter<double>(1.5);
        [SpiceName("lmu20"), SpiceInfo("Length dependence of mu20")]
        public Parameter<double> B2mob20L { get; } = new Parameter<double>();
        [SpiceName("wmu20"), SpiceInfo("Width dependence of mu20")]
        public Parameter<double> B2mob20W { get; } = new Parameter<double>();
        [SpiceName("mu2b"), SpiceInfo("VBS dependence of mu2")]
        public Parameter<double> B2mob2B0 { get; } = new Parameter<double>();
        [SpiceName("lmu2b"), SpiceInfo("Length dependence of mu2b")]
        public Parameter<double> B2mob2BL { get; } = new Parameter<double>();
        [SpiceName("wmu2b"), SpiceInfo("Width dependence of mu2b")]
        public Parameter<double> B2mob2BW { get; } = new Parameter<double>();
        [SpiceName("mu2g"), SpiceInfo("VGS dependence of mu2")]
        public Parameter<double> B2mob2G0 { get; } = new Parameter<double>();
        [SpiceName("lmu2g"), SpiceInfo("Length dependence of mu2g")]
        public Parameter<double> B2mob2GL { get; } = new Parameter<double>();
        [SpiceName("wmu2g"), SpiceInfo("Width dependence of mu2g")]
        public Parameter<double> B2mob2GW { get; } = new Parameter<double>();
        [SpiceName("mu30"), SpiceInfo("VDS dependence of mu in linear term")]
        public Parameter<double> B2mob300 { get; } = new Parameter<double>(10);
        [SpiceName("lmu30"), SpiceInfo("Length dependence of mu30")]
        public Parameter<double> B2mob30L { get; } = new Parameter<double>();
        [SpiceName("wmu30"), SpiceInfo("Width dependence of mu30")]
        public Parameter<double> B2mob30W { get; } = new Parameter<double>();
        [SpiceName("mu3b"), SpiceInfo("VBS dependence of mu3")]
        public Parameter<double> B2mob3B0 { get; } = new Parameter<double>();
        [SpiceName("lmu3b"), SpiceInfo("Length dependence of mu3b")]
        public Parameter<double> B2mob3BL { get; } = new Parameter<double>();
        [SpiceName("wmu3b"), SpiceInfo("Width dependence of mu3b")]
        public Parameter<double> B2mob3BW { get; } = new Parameter<double>();
        [SpiceName("mu3g"), SpiceInfo("VGS dependence of mu3")]
        public Parameter<double> B2mob3G0 { get; } = new Parameter<double>();
        [SpiceName("lmu3g"), SpiceInfo("Length dependence of mu3g")]
        public Parameter<double> B2mob3GL { get; } = new Parameter<double>();
        [SpiceName("wmu3g"), SpiceInfo("Width dependence of mu3g")]
        public Parameter<double> B2mob3GW { get; } = new Parameter<double>();
        [SpiceName("mu40"), SpiceInfo("VDS dependence of mu in linear term")]
        public Parameter<double> B2mob400 { get; } = new Parameter<double>();
        [SpiceName("lmu40"), SpiceInfo("Length dependence of mu40")]
        public Parameter<double> B2mob40L { get; } = new Parameter<double>();
        [SpiceName("wmu40"), SpiceInfo("Width dependence of mu40")]
        public Parameter<double> B2mob40W { get; } = new Parameter<double>();
        [SpiceName("mu4b"), SpiceInfo("VBS dependence of mu4")]
        public Parameter<double> B2mob4B0 { get; } = new Parameter<double>();
        [SpiceName("lmu4b"), SpiceInfo("Length dependence of mu4b")]
        public Parameter<double> B2mob4BL { get; } = new Parameter<double>();
        [SpiceName("wmu4b"), SpiceInfo("Width dependence of mu4b")]
        public Parameter<double> B2mob4BW { get; } = new Parameter<double>();
        [SpiceName("mu4g"), SpiceInfo("VGS dependence of mu4")]
        public Parameter<double> B2mob4G0 { get; } = new Parameter<double>();
        [SpiceName("lmu4g"), SpiceInfo("Length dependence of mu4g")]
        public Parameter<double> B2mob4GL { get; } = new Parameter<double>();
        [SpiceName("wmu4g"), SpiceInfo("Width dependence of mu4g")]
        public Parameter<double> B2mob4GW { get; } = new Parameter<double>();
        [SpiceName("ua0"), SpiceInfo("Linear VGS dependence of mobility")]
        public Parameter<double> B2ua00 { get; } = new Parameter<double>();
        [SpiceName("lua0"), SpiceInfo("Length dependence of ua0")]
        public Parameter<double> B2ua0L { get; } = new Parameter<double>();
        [SpiceName("wua0"), SpiceInfo("Width dependence of ua0")]
        public Parameter<double> B2ua0W { get; } = new Parameter<double>();
        [SpiceName("uab"), SpiceInfo("VBS dependence of ua")]
        public Parameter<double> B2uaB0 { get; } = new Parameter<double>();
        [SpiceName("luab"), SpiceInfo("Length dependence of uab")]
        public Parameter<double> B2uaBL { get; } = new Parameter<double>();
        [SpiceName("wuab"), SpiceInfo("Width dependence of uab")]
        public Parameter<double> B2uaBW { get; } = new Parameter<double>();
        [SpiceName("ub0"), SpiceInfo("Quadratic VGS dependence of mobility")]
        public Parameter<double> B2ub00 { get; } = new Parameter<double>();
        [SpiceName("lub0"), SpiceInfo("Length dependence of ub0")]
        public Parameter<double> B2ub0L { get; } = new Parameter<double>();
        [SpiceName("wub0"), SpiceInfo("Width dependence of ub0")]
        public Parameter<double> B2ub0W { get; } = new Parameter<double>();
        [SpiceName("ubb"), SpiceInfo("VBS dependence of ub")]
        public Parameter<double> B2ubB0 { get; } = new Parameter<double>();
        [SpiceName("lubb"), SpiceInfo("Length dependence of ubb")]
        public Parameter<double> B2ubBL { get; } = new Parameter<double>();
        [SpiceName("wubb"), SpiceInfo("Width dependence of ubb")]
        public Parameter<double> B2ubBW { get; } = new Parameter<double>();
        [SpiceName("u10"), SpiceInfo("VDS depence of mobility")]
        public Parameter<double> B2u100 { get; } = new Parameter<double>();
        [SpiceName("lu10"), SpiceInfo("Length dependence of u10")]
        public Parameter<double> B2u10L { get; } = new Parameter<double>();
        [SpiceName("wu10"), SpiceInfo("Width dependence of u10")]
        public Parameter<double> B2u10W { get; } = new Parameter<double>();
        [SpiceName("u1b"), SpiceInfo("VBS depence of u1")]
        public Parameter<double> B2u1B0 { get; } = new Parameter<double>();
        [SpiceName("lu1b"), SpiceInfo("Length depence of u1b")]
        public Parameter<double> B2u1BL { get; } = new Parameter<double>();
        [SpiceName("wu1b"), SpiceInfo("Width depence of u1b")]
        public Parameter<double> B2u1BW { get; } = new Parameter<double>();
        [SpiceName("u1d"), SpiceInfo("VDS depence of u1")]
        public Parameter<double> B2u1D0 { get; } = new Parameter<double>();
        [SpiceName("lu1d"), SpiceInfo("Length depence of u1d")]
        public Parameter<double> B2u1DL { get; } = new Parameter<double>();
        [SpiceName("wu1d"), SpiceInfo("Width depence of u1d")]
        public Parameter<double> B2u1DW { get; } = new Parameter<double>();
        [SpiceName("n0"), SpiceInfo("Subthreshold slope at VDS=0 VBS=0")]
        public Parameter<double> B2n00 { get; } = new Parameter<double>(1.4);
        [SpiceName("ln0"), SpiceInfo("Length dependence of n0")]
        public Parameter<double> B2n0L { get; } = new Parameter<double>();
        [SpiceName("wn0"), SpiceInfo("Width dependence of n0")]
        public Parameter<double> B2n0W { get; } = new Parameter<double>();
        [SpiceName("nb"), SpiceInfo("VBS dependence of n")]
        public Parameter<double> B2nB0 { get; } = new Parameter<double>();
        [SpiceName("lnb"), SpiceInfo("Length dependence of nb")]
        public Parameter<double> B2nBL { get; } = new Parameter<double>();
        [SpiceName("wnb"), SpiceInfo("Width dependence of nb")]
        public Parameter<double> B2nBW { get; } = new Parameter<double>();
        [SpiceName("nd"), SpiceInfo("VDS dependence of n")]
        public Parameter<double> B2nD0 { get; } = new Parameter<double>();
        [SpiceName("lnd"), SpiceInfo("Length dependence of nd")]
        public Parameter<double> B2nDL { get; } = new Parameter<double>();
        [SpiceName("wnd"), SpiceInfo("Width dependence of nd")]
        public Parameter<double> B2nDW { get; } = new Parameter<double>();
        [SpiceName("vof0"), SpiceInfo("Threshold voltage offset AT VDS=0 VBS=0")]
        public Parameter<double> B2vof00 { get; } = new Parameter<double>(1.8);
        [SpiceName("lvof0"), SpiceInfo("Length dependence of vof0")]
        public Parameter<double> B2vof0L { get; } = new Parameter<double>();
        [SpiceName("wvof0"), SpiceInfo("Width dependence of vof0")]
        public Parameter<double> B2vof0W { get; } = new Parameter<double>();
        [SpiceName("vofb"), SpiceInfo("VBS dependence of vof")]
        public Parameter<double> B2vofB0 { get; } = new Parameter<double>();
        [SpiceName("lvofb"), SpiceInfo("Length dependence of vofb")]
        public Parameter<double> B2vofBL { get; } = new Parameter<double>();
        [SpiceName("wvofb"), SpiceInfo("Width dependence of vofb")]
        public Parameter<double> B2vofBW { get; } = new Parameter<double>();
        [SpiceName("vofd"), SpiceInfo("VDS dependence of vof")]
        public Parameter<double> B2vofD0 { get; } = new Parameter<double>();
        [SpiceName("lvofd"), SpiceInfo("Length dependence of vofd")]
        public Parameter<double> B2vofDL { get; } = new Parameter<double>();
        [SpiceName("wvofd"), SpiceInfo("Width dependence of vofd")]
        public Parameter<double> B2vofDW { get; } = new Parameter<double>();
        [SpiceName("ai0"), SpiceInfo("Pre-factor of hot-electron effect.")]
        public Parameter<double> B2ai00 { get; } = new Parameter<double>();
        [SpiceName("lai0"), SpiceInfo("Length dependence of ai0")]
        public Parameter<double> B2ai0L { get; } = new Parameter<double>();
        [SpiceName("wai0"), SpiceInfo("Width dependence of ai0")]
        public Parameter<double> B2ai0W { get; } = new Parameter<double>();
        [SpiceName("aib"), SpiceInfo("VBS dependence of ai")]
        public Parameter<double> B2aiB0 { get; } = new Parameter<double>();
        [SpiceName("laib"), SpiceInfo("Length dependence of aib")]
        public Parameter<double> B2aiBL { get; } = new Parameter<double>();
        [SpiceName("waib"), SpiceInfo("Width dependence of aib")]
        public Parameter<double> B2aiBW { get; } = new Parameter<double>();
        [SpiceName("bi0"), SpiceInfo("Exponential factor of hot-electron effect.")]
        public Parameter<double> B2bi00 { get; } = new Parameter<double>();
        [SpiceName("lbi0"), SpiceInfo("Length dependence of bi0")]
        public Parameter<double> B2bi0L { get; } = new Parameter<double>();
        [SpiceName("wbi0"), SpiceInfo("Width dependence of bi0")]
        public Parameter<double> B2bi0W { get; } = new Parameter<double>();
        [SpiceName("bib"), SpiceInfo("VBS dependence of bi")]
        public Parameter<double> B2biB0 { get; } = new Parameter<double>();
        [SpiceName("lbib"), SpiceInfo("Length dependence of bib")]
        public Parameter<double> B2biBL { get; } = new Parameter<double>();
        [SpiceName("wbib"), SpiceInfo("Width dependence of bib")]
        public Parameter<double> B2biBW { get; } = new Parameter<double>();
        [SpiceName("vghigh"), SpiceInfo("Upper bound of the cubic spline function.")]
        public Parameter<double> B2vghigh0 { get; } = new Parameter<double>();
        [SpiceName("lvghigh"), SpiceInfo("Length dependence of vghigh")]
        public Parameter<double> B2vghighL { get; } = new Parameter<double>();
        [SpiceName("wvghigh"), SpiceInfo("Width dependence of vghigh")]
        public Parameter<double> B2vghighW { get; } = new Parameter<double>();
        [SpiceName("vglow"), SpiceInfo("Lower bound of the cubic spline function.")]
        public Parameter<double> B2vglow0 { get; } = new Parameter<double>(-0.15);
        [SpiceName("lvglow"), SpiceInfo("Length dependence of vglow")]
        public Parameter<double> B2vglowL { get; } = new Parameter<double>();
        [SpiceName("wvglow"), SpiceInfo("Width dependence of vglow")]
        public Parameter<double> B2vglowW { get; } = new Parameter<double>();
        [SpiceName("tox"), SpiceInfo("Gate oxide thickness in um")]
        public Parameter<double> B2tox { get; } = new Parameter<double>(0.03);
        [SpiceName("temp"), SpiceInfo("Temperature in degree Celcius")]
        public Parameter<double> B2temp { get; } = new Parameter<double>();
        [SpiceName("vdd"), SpiceInfo("Maximum Vds ")]
        public Parameter<double> B2vdd { get; } = new Parameter<double>();
        [SpiceName("vgg"), SpiceInfo("Maximum Vgs ")]
        public Parameter<double> B2vgg { get; } = new Parameter<double>();
        [SpiceName("vbb"), SpiceInfo("Maximum Vbs ")]
        public Parameter<double> B2vbb { get; } = new Parameter<double>();
        [SpiceName("cgso"), SpiceInfo("Gate source overlap capacitance per unit channel width(m)")]
        public Parameter<double> B2gateSourceOverlapCap { get; } = new Parameter<double>();
        [SpiceName("cgdo"), SpiceInfo("Gate drain overlap capacitance per unit channel width(m)")]
        public Parameter<double> B2gateDrainOverlapCap { get; } = new Parameter<double>();
        [SpiceName("cgbo"), SpiceInfo("Gate bulk overlap capacitance per unit channel length(m)")]
        public Parameter<double> B2gateBulkOverlapCap { get; } = new Parameter<double>();
        [SpiceName("xpart"), SpiceInfo("Flag for channel charge partitioning")]
        public Parameter<double> B2channelChargePartitionFlag { get; } = new Parameter<double>();
        [SpiceName("rsh"), SpiceInfo("Source drain diffusion sheet resistance in ohm per square")]
        public Parameter<double> B2sheetResistance { get; } = new Parameter<double>();
        [SpiceName("js"), SpiceInfo("Source drain junction saturation current per unit area")]
        public Parameter<double> B2jctSatCurDensity { get; } = new Parameter<double>();
        [SpiceName("pb"), SpiceInfo("Source drain junction built in potential")]
        public Parameter<double> B2bulkJctPotential { get; } = new Parameter<double>();
        [SpiceName("mj"), SpiceInfo("Source drain bottom junction capacitance grading coefficient")]
        public Parameter<double> B2bulkJctBotGradingCoeff { get; } = new Parameter<double>();
        [SpiceName("pbsw"), SpiceInfo("Source drain side junction capacitance built in potential")]
        public Parameter<double> B2sidewallJctPotential { get; } = new Parameter<double>();
        [SpiceName("mjsw"), SpiceInfo("Source drain side junction capacitance grading coefficient")]
        public Parameter<double> B2bulkJctSideGradingCoeff { get; } = new Parameter<double>();
        [SpiceName("cj"), SpiceInfo("Source drain bottom junction capacitance per unit area")]
        public Parameter<double> B2unitAreaJctCap { get; } = new Parameter<double>();
        [SpiceName("cjsw"), SpiceInfo("Source drain side junction capacitance per unit area")]
        public Parameter<double> B2unitLengthSidewallJctCap { get; } = new Parameter<double>();
        [SpiceName("wdf"), SpiceInfo("Default width of source drain diffusion in um")]
        public Parameter<double> B2defaultWidth { get; } = new Parameter<double>();
        [SpiceName("dell"), SpiceInfo("Length reduction of source drain diffusion")]
        public Parameter<double> B2deltaLength { get; } = new Parameter<double>();

        /// <summary>
        /// Methods
        /// </summary>
        [SpiceName("nmos"), SpiceInfo("Flag to indicate NMOS")]
        public void SetNMOS(bool value) { B2type = 1; }
        [SpiceName("pmos"), SpiceInfo("Flag to indicate PMOS")]
        public void SetPMOS(bool value) { B2type = -1; }

        /// <summary>
        /// Extra variables
        /// </summary>
        public double B2type { get; private set; } = 1;
        public double B2Cox { get; private set; }
        public double B2vdd2 { get; private set; }
        public double B2vgg2 { get; private set; }
        public double B2vbb2 { get; private set; }
        public double B2Vtm { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the device</param>
        public BSIM2Model(string name) : base(name)
        {
        }

        /// <summary>
        /// Do temperature-dependent calculations
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Temperature(Circuit ckt)
        {

            /* Default value Processing for B2 MOSFET Models */
            /* Some Limiting for Model Parameters */
            if (B2bulkJctPotential < 0.1)
            {
                B2bulkJctPotential.Value = 0.1;
            }
            if (B2sidewallJctPotential < 0.1)
            {
                B2sidewallJctPotential.Value = 0.1;
            }

            B2Cox = 3.453e-13 / (B2tox * 1.0e-4); /* in F / cm *  * 2 */
            B2vdd2 = 2.0 * B2vdd;
            B2vgg2 = 2.0 * B2vgg;
            B2vbb2 = 2.0 * B2vbb;
            B2Vtm = 8.625e-5 * (B2temp + 273.0);
        }
    }
}
