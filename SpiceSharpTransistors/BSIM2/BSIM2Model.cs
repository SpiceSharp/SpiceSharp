using System;
using System.Collections.Generic;
using SpiceSharp.Circuits;
using SpiceSharp.Parameters;
using SpiceSharp.Components.Transistors;

namespace SpiceSharp.Components
{
    /// <summary>
    /// The BSIM2 Model
    /// </summary>
    public class BSIM2Model : CircuitModel<BSIM2Model>
    {
        /// <summary>
        /// Register default behaviours
        /// </summary>
        static BSIM2Model()
        {
            Behaviours.Behaviours.RegisterBehaviour(typeof(BSIM2Model), typeof(ComponentBehaviours.BSIM2ModelTemperatureBehaviour));
        }

        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("vfb"), SpiceInfo("Flat band voltage")]
        public Parameter B2vfb0 { get; } = new Parameter();
        [SpiceName("lvfb"), SpiceInfo("Length dependence of vfb")]
        public Parameter B2vfbL { get; } = new Parameter();
        [SpiceName("wvfb"), SpiceInfo("Width dependence of vfb")]
        public Parameter B2vfbW { get; } = new Parameter();
        [SpiceName("phi"), SpiceInfo("Strong inversion surface potential ")]
        public Parameter B2phi0 { get; } = new Parameter();
        [SpiceName("lphi"), SpiceInfo("Length dependence of phi")]
        public Parameter B2phiL { get; } = new Parameter();
        [SpiceName("wphi"), SpiceInfo("Width dependence of phi")]
        public Parameter B2phiW { get; } = new Parameter();
        [SpiceName("k1"), SpiceInfo("Bulk effect coefficient 1")]
        public Parameter B2k10 { get; } = new Parameter();
        [SpiceName("lk1"), SpiceInfo("Length dependence of k1")]
        public Parameter B2k1L { get; } = new Parameter();
        [SpiceName("wk1"), SpiceInfo("Width dependence of k1")]
        public Parameter B2k1W { get; } = new Parameter();
        [SpiceName("k2"), SpiceInfo("Bulk effect coefficient 2")]
        public Parameter B2k20 { get; } = new Parameter();
        [SpiceName("lk2"), SpiceInfo("Length dependence of k2")]
        public Parameter B2k2L { get; } = new Parameter();
        [SpiceName("wk2"), SpiceInfo("Width dependence of k2")]
        public Parameter B2k2W { get; } = new Parameter();
        [SpiceName("eta0"), SpiceInfo("VDS dependence of threshold voltage at VDD=0")]
        public Parameter B2eta00 { get; } = new Parameter();
        [SpiceName("leta0"), SpiceInfo("Length dependence of eta0")]
        public Parameter B2eta0L { get; } = new Parameter();
        [SpiceName("weta0"), SpiceInfo("Width dependence of eta0")]
        public Parameter B2eta0W { get; } = new Parameter();
        [SpiceName("etab"), SpiceInfo("VBS dependence of eta")]
        public Parameter B2etaB0 { get; } = new Parameter();
        [SpiceName("letab"), SpiceInfo("Length dependence of etab")]
        public Parameter B2etaBL { get; } = new Parameter();
        [SpiceName("wetab"), SpiceInfo("Width dependence of etab")]
        public Parameter B2etaBW { get; } = new Parameter();
        [SpiceName("dl"), SpiceInfo("Channel length reduction in um")]
        public Parameter B2deltaL { get; } = new Parameter();
        [SpiceName("dw"), SpiceInfo("Channel width reduction in um")]
        public Parameter B2deltaW { get; } = new Parameter();
        [SpiceName("mu0"), SpiceInfo("Low-field mobility, at VDS=0 VGS=VTH")]
        public Parameter B2mob00 { get; } = new Parameter();
        [SpiceName("mu0b"), SpiceInfo("VBS dependence of low-field mobility")]
        public Parameter B2mob0B0 { get; } = new Parameter();
        [SpiceName("lmu0b"), SpiceInfo("Length dependence of mu0b")]
        public Parameter B2mob0BL { get; } = new Parameter();
        [SpiceName("wmu0b"), SpiceInfo("Width dependence of mu0b")]
        public Parameter B2mob0BW { get; } = new Parameter();
        [SpiceName("mus0"), SpiceInfo("Mobility at VDS=VDD VGS=VTH")]
        public Parameter B2mobs00 { get; } = new Parameter();
        [SpiceName("lmus0"), SpiceInfo("Length dependence of mus0")]
        public Parameter B2mobs0L { get; } = new Parameter();
        [SpiceName("wmus0"), SpiceInfo("Width dependence of mus")]
        public Parameter B2mobs0W { get; } = new Parameter();
        [SpiceName("musb"), SpiceInfo("VBS dependence of mus")]
        public Parameter B2mobsB0 { get; } = new Parameter();
        [SpiceName("lmusb"), SpiceInfo("Length dependence of musb")]
        public Parameter B2mobsBL { get; } = new Parameter();
        [SpiceName("wmusb"), SpiceInfo("Width dependence of musb")]
        public Parameter B2mobsBW { get; } = new Parameter();
        [SpiceName("mu20"), SpiceInfo("VDS dependence of mu in tanh term")]
        public Parameter B2mob200 { get; } = new Parameter(1.5);
        [SpiceName("lmu20"), SpiceInfo("Length dependence of mu20")]
        public Parameter B2mob20L { get; } = new Parameter();
        [SpiceName("wmu20"), SpiceInfo("Width dependence of mu20")]
        public Parameter B2mob20W { get; } = new Parameter();
        [SpiceName("mu2b"), SpiceInfo("VBS dependence of mu2")]
        public Parameter B2mob2B0 { get; } = new Parameter();
        [SpiceName("lmu2b"), SpiceInfo("Length dependence of mu2b")]
        public Parameter B2mob2BL { get; } = new Parameter();
        [SpiceName("wmu2b"), SpiceInfo("Width dependence of mu2b")]
        public Parameter B2mob2BW { get; } = new Parameter();
        [SpiceName("mu2g"), SpiceInfo("VGS dependence of mu2")]
        public Parameter B2mob2G0 { get; } = new Parameter();
        [SpiceName("lmu2g"), SpiceInfo("Length dependence of mu2g")]
        public Parameter B2mob2GL { get; } = new Parameter();
        [SpiceName("wmu2g"), SpiceInfo("Width dependence of mu2g")]
        public Parameter B2mob2GW { get; } = new Parameter();
        [SpiceName("mu30"), SpiceInfo("VDS dependence of mu in linear term")]
        public Parameter B2mob300 { get; } = new Parameter(10);
        [SpiceName("lmu30"), SpiceInfo("Length dependence of mu30")]
        public Parameter B2mob30L { get; } = new Parameter();
        [SpiceName("wmu30"), SpiceInfo("Width dependence of mu30")]
        public Parameter B2mob30W { get; } = new Parameter();
        [SpiceName("mu3b"), SpiceInfo("VBS dependence of mu3")]
        public Parameter B2mob3B0 { get; } = new Parameter();
        [SpiceName("lmu3b"), SpiceInfo("Length dependence of mu3b")]
        public Parameter B2mob3BL { get; } = new Parameter();
        [SpiceName("wmu3b"), SpiceInfo("Width dependence of mu3b")]
        public Parameter B2mob3BW { get; } = new Parameter();
        [SpiceName("mu3g"), SpiceInfo("VGS dependence of mu3")]
        public Parameter B2mob3G0 { get; } = new Parameter();
        [SpiceName("lmu3g"), SpiceInfo("Length dependence of mu3g")]
        public Parameter B2mob3GL { get; } = new Parameter();
        [SpiceName("wmu3g"), SpiceInfo("Width dependence of mu3g")]
        public Parameter B2mob3GW { get; } = new Parameter();
        [SpiceName("mu40"), SpiceInfo("VDS dependence of mu in linear term")]
        public Parameter B2mob400 { get; } = new Parameter();
        [SpiceName("lmu40"), SpiceInfo("Length dependence of mu40")]
        public Parameter B2mob40L { get; } = new Parameter();
        [SpiceName("wmu40"), SpiceInfo("Width dependence of mu40")]
        public Parameter B2mob40W { get; } = new Parameter();
        [SpiceName("mu4b"), SpiceInfo("VBS dependence of mu4")]
        public Parameter B2mob4B0 { get; } = new Parameter();
        [SpiceName("lmu4b"), SpiceInfo("Length dependence of mu4b")]
        public Parameter B2mob4BL { get; } = new Parameter();
        [SpiceName("wmu4b"), SpiceInfo("Width dependence of mu4b")]
        public Parameter B2mob4BW { get; } = new Parameter();
        [SpiceName("mu4g"), SpiceInfo("VGS dependence of mu4")]
        public Parameter B2mob4G0 { get; } = new Parameter();
        [SpiceName("lmu4g"), SpiceInfo("Length dependence of mu4g")]
        public Parameter B2mob4GL { get; } = new Parameter();
        [SpiceName("wmu4g"), SpiceInfo("Width dependence of mu4g")]
        public Parameter B2mob4GW { get; } = new Parameter();
        [SpiceName("ua0"), SpiceInfo("Linear VGS dependence of mobility")]
        public Parameter B2ua00 { get; } = new Parameter();
        [SpiceName("lua0"), SpiceInfo("Length dependence of ua0")]
        public Parameter B2ua0L { get; } = new Parameter();
        [SpiceName("wua0"), SpiceInfo("Width dependence of ua0")]
        public Parameter B2ua0W { get; } = new Parameter();
        [SpiceName("uab"), SpiceInfo("VBS dependence of ua")]
        public Parameter B2uaB0 { get; } = new Parameter();
        [SpiceName("luab"), SpiceInfo("Length dependence of uab")]
        public Parameter B2uaBL { get; } = new Parameter();
        [SpiceName("wuab"), SpiceInfo("Width dependence of uab")]
        public Parameter B2uaBW { get; } = new Parameter();
        [SpiceName("ub0"), SpiceInfo("Quadratic VGS dependence of mobility")]
        public Parameter B2ub00 { get; } = new Parameter();
        [SpiceName("lub0"), SpiceInfo("Length dependence of ub0")]
        public Parameter B2ub0L { get; } = new Parameter();
        [SpiceName("wub0"), SpiceInfo("Width dependence of ub0")]
        public Parameter B2ub0W { get; } = new Parameter();
        [SpiceName("ubb"), SpiceInfo("VBS dependence of ub")]
        public Parameter B2ubB0 { get; } = new Parameter();
        [SpiceName("lubb"), SpiceInfo("Length dependence of ubb")]
        public Parameter B2ubBL { get; } = new Parameter();
        [SpiceName("wubb"), SpiceInfo("Width dependence of ubb")]
        public Parameter B2ubBW { get; } = new Parameter();
        [SpiceName("u10"), SpiceInfo("VDS depence of mobility")]
        public Parameter B2u100 { get; } = new Parameter();
        [SpiceName("lu10"), SpiceInfo("Length dependence of u10")]
        public Parameter B2u10L { get; } = new Parameter();
        [SpiceName("wu10"), SpiceInfo("Width dependence of u10")]
        public Parameter B2u10W { get; } = new Parameter();
        [SpiceName("u1b"), SpiceInfo("VBS depence of u1")]
        public Parameter B2u1B0 { get; } = new Parameter();
        [SpiceName("lu1b"), SpiceInfo("Length depence of u1b")]
        public Parameter B2u1BL { get; } = new Parameter();
        [SpiceName("wu1b"), SpiceInfo("Width depence of u1b")]
        public Parameter B2u1BW { get; } = new Parameter();
        [SpiceName("u1d"), SpiceInfo("VDS depence of u1")]
        public Parameter B2u1D0 { get; } = new Parameter();
        [SpiceName("lu1d"), SpiceInfo("Length depence of u1d")]
        public Parameter B2u1DL { get; } = new Parameter();
        [SpiceName("wu1d"), SpiceInfo("Width depence of u1d")]
        public Parameter B2u1DW { get; } = new Parameter();
        [SpiceName("n0"), SpiceInfo("Subthreshold slope at VDS=0 VBS=0")]
        public Parameter B2n00 { get; } = new Parameter(1.4);
        [SpiceName("ln0"), SpiceInfo("Length dependence of n0")]
        public Parameter B2n0L { get; } = new Parameter();
        [SpiceName("wn0"), SpiceInfo("Width dependence of n0")]
        public Parameter B2n0W { get; } = new Parameter();
        [SpiceName("nb"), SpiceInfo("VBS dependence of n")]
        public Parameter B2nB0 { get; } = new Parameter();
        [SpiceName("lnb"), SpiceInfo("Length dependence of nb")]
        public Parameter B2nBL { get; } = new Parameter();
        [SpiceName("wnb"), SpiceInfo("Width dependence of nb")]
        public Parameter B2nBW { get; } = new Parameter();
        [SpiceName("nd"), SpiceInfo("VDS dependence of n")]
        public Parameter B2nD0 { get; } = new Parameter();
        [SpiceName("lnd"), SpiceInfo("Length dependence of nd")]
        public Parameter B2nDL { get; } = new Parameter();
        [SpiceName("wnd"), SpiceInfo("Width dependence of nd")]
        public Parameter B2nDW { get; } = new Parameter();
        [SpiceName("vof0"), SpiceInfo("Threshold voltage offset AT VDS=0 VBS=0")]
        public Parameter B2vof00 { get; } = new Parameter(1.8);
        [SpiceName("lvof0"), SpiceInfo("Length dependence of vof0")]
        public Parameter B2vof0L { get; } = new Parameter();
        [SpiceName("wvof0"), SpiceInfo("Width dependence of vof0")]
        public Parameter B2vof0W { get; } = new Parameter();
        [SpiceName("vofb"), SpiceInfo("VBS dependence of vof")]
        public Parameter B2vofB0 { get; } = new Parameter();
        [SpiceName("lvofb"), SpiceInfo("Length dependence of vofb")]
        public Parameter B2vofBL { get; } = new Parameter();
        [SpiceName("wvofb"), SpiceInfo("Width dependence of vofb")]
        public Parameter B2vofBW { get; } = new Parameter();
        [SpiceName("vofd"), SpiceInfo("VDS dependence of vof")]
        public Parameter B2vofD0 { get; } = new Parameter();
        [SpiceName("lvofd"), SpiceInfo("Length dependence of vofd")]
        public Parameter B2vofDL { get; } = new Parameter();
        [SpiceName("wvofd"), SpiceInfo("Width dependence of vofd")]
        public Parameter B2vofDW { get; } = new Parameter();
        [SpiceName("ai0"), SpiceInfo("Pre-factor of hot-electron effect.")]
        public Parameter B2ai00 { get; } = new Parameter();
        [SpiceName("lai0"), SpiceInfo("Length dependence of ai0")]
        public Parameter B2ai0L { get; } = new Parameter();
        [SpiceName("wai0"), SpiceInfo("Width dependence of ai0")]
        public Parameter B2ai0W { get; } = new Parameter();
        [SpiceName("aib"), SpiceInfo("VBS dependence of ai")]
        public Parameter B2aiB0 { get; } = new Parameter();
        [SpiceName("laib"), SpiceInfo("Length dependence of aib")]
        public Parameter B2aiBL { get; } = new Parameter();
        [SpiceName("waib"), SpiceInfo("Width dependence of aib")]
        public Parameter B2aiBW { get; } = new Parameter();
        [SpiceName("bi0"), SpiceInfo("Exponential factor of hot-electron effect.")]
        public Parameter B2bi00 { get; } = new Parameter();
        [SpiceName("lbi0"), SpiceInfo("Length dependence of bi0")]
        public Parameter B2bi0L { get; } = new Parameter();
        [SpiceName("wbi0"), SpiceInfo("Width dependence of bi0")]
        public Parameter B2bi0W { get; } = new Parameter();
        [SpiceName("bib"), SpiceInfo("VBS dependence of bi")]
        public Parameter B2biB0 { get; } = new Parameter();
        [SpiceName("lbib"), SpiceInfo("Length dependence of bib")]
        public Parameter B2biBL { get; } = new Parameter();
        [SpiceName("wbib"), SpiceInfo("Width dependence of bib")]
        public Parameter B2biBW { get; } = new Parameter();
        [SpiceName("vghigh"), SpiceInfo("Upper bound of the cubic spline function.")]
        public Parameter B2vghigh0 { get; } = new Parameter();
        [SpiceName("lvghigh"), SpiceInfo("Length dependence of vghigh")]
        public Parameter B2vghighL { get; } = new Parameter();
        [SpiceName("wvghigh"), SpiceInfo("Width dependence of vghigh")]
        public Parameter B2vghighW { get; } = new Parameter();
        [SpiceName("vglow"), SpiceInfo("Lower bound of the cubic spline function.")]
        public Parameter B2vglow0 { get; } = new Parameter(-0.15);
        [SpiceName("lvglow"), SpiceInfo("Length dependence of vglow")]
        public Parameter B2vglowL { get; } = new Parameter();
        [SpiceName("wvglow"), SpiceInfo("Width dependence of vglow")]
        public Parameter B2vglowW { get; } = new Parameter();
        [SpiceName("tox"), SpiceInfo("Gate oxide thickness in um")]
        public Parameter B2tox { get; } = new Parameter(0.03);
        [SpiceName("temp"), SpiceInfo("Temperature in degree Celcius")]
        public Parameter B2temp { get; } = new Parameter(300.15);
        [SpiceName("vdd"), SpiceInfo("Maximum Vds ")]
        public Parameter B2vdd { get; } = new Parameter();
        [SpiceName("vgg"), SpiceInfo("Maximum Vgs ")]
        public Parameter B2vgg { get; } = new Parameter();
        [SpiceName("vbb"), SpiceInfo("Maximum Vbs ")]
        public Parameter B2vbb { get; } = new Parameter();
        [SpiceName("cgso"), SpiceInfo("Gate source overlap capacitance per unit channel width(m)")]
        public Parameter B2gateSourceOverlapCap { get; } = new Parameter();
        [SpiceName("cgdo"), SpiceInfo("Gate drain overlap capacitance per unit channel width(m)")]
        public Parameter B2gateDrainOverlapCap { get; } = new Parameter();
        [SpiceName("cgbo"), SpiceInfo("Gate bulk overlap capacitance per unit channel length(m)")]
        public Parameter B2gateBulkOverlapCap { get; } = new Parameter();
        [SpiceName("xpart"), SpiceInfo("Flag for channel charge partitioning")]
        public Parameter B2channelChargePartitionFlag { get; } = new Parameter();
        [SpiceName("rsh"), SpiceInfo("Source drain diffusion sheet resistance in ohm per square")]
        public Parameter B2sheetResistance { get; } = new Parameter();
        [SpiceName("js"), SpiceInfo("Source drain junction saturation current per unit area")]
        public Parameter B2jctSatCurDensity { get; } = new Parameter();
        [SpiceName("pb"), SpiceInfo("Source drain junction built in potential")]
        public Parameter B2bulkJctPotential { get; } = new Parameter();
        [SpiceName("mj"), SpiceInfo("Source drain bottom junction capacitance grading coefficient")]
        public Parameter B2bulkJctBotGradingCoeff { get; } = new Parameter();
        [SpiceName("pbsw"), SpiceInfo("Source drain side junction capacitance built in potential")]
        public Parameter B2sidewallJctPotential { get; } = new Parameter();
        [SpiceName("mjsw"), SpiceInfo("Source drain side junction capacitance grading coefficient")]
        public Parameter B2bulkJctSideGradingCoeff { get; } = new Parameter();
        [SpiceName("cj"), SpiceInfo("Source drain bottom junction capacitance per unit area")]
        public Parameter B2unitAreaJctCap { get; } = new Parameter();
        [SpiceName("cjsw"), SpiceInfo("Source drain side junction capacitance per unit area")]
        public Parameter B2unitLengthSidewallJctCap { get; } = new Parameter();
        [SpiceName("wdf"), SpiceInfo("Default width of source drain diffusion in um")]
        public Parameter B2defaultWidth { get; } = new Parameter();
        [SpiceName("dell"), SpiceInfo("Length reduction of source drain diffusion")]
        public Parameter B2deltaLength { get; } = new Parameter();

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
        public double B2type { get; internal set; } = 1;
        public double B2Cox { get; internal set; }
        public double B2vdd2 { get; internal set; }
        public double B2vgg2 { get; internal set; }
        public double B2vbb2 { get; internal set; }
        public double B2Vtm { get; internal set; }

        /// <summary>
        /// A cache of sizes
        /// </summary>
        internal Dictionary<Tuple<double, double>, BSIM2SizeDependParam> Sizes { get; } = new Dictionary<Tuple<double, double>, BSIM2SizeDependParam>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the device</param>
        public BSIM2Model(CircuitIdentifier name) : base(name)
        {
        }
    }
}
