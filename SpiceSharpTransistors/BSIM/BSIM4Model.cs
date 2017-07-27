using System;
using SpiceSharp.Circuits;
using SpiceSharp.Diagnostics;
using SpiceSharp.Parameters;
using SpiceSharp.Components.Transistors;

namespace SpiceSharp.Components
{
    public class BSIM4Model : CircuitModel
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("mobmod"), SpiceInfo("Mobility model selector")]
        public Parameter<int> BSIM4mobMod { get; } = new Parameter<int>();
        [SpiceName("binunit"), SpiceInfo("Bin  unit  selector")]
        public Parameter<int> BSIM4binUnit { get; } = new Parameter<int>(1);
        [SpiceName("paramchk"), SpiceInfo("Model parameter checking selector")]
        public Parameter<int> BSIM4paramChk { get; } = new Parameter<int>(1);
        [SpiceName("cvchargemod"), SpiceInfo("Capacitance Charge model selector")]
        public Parameter<int> BSIM4cvchargeMod { get; } = new Parameter<int>();
        [SpiceName("capmod"), SpiceInfo("Capacitance model selector")]
        public Parameter<int> BSIM4capMod { get; } = new Parameter<int>(2);
        [SpiceName("diomod"), SpiceInfo("Diode IV model selector")]
        public Parameter<int> BSIM4dioMod { get; } = new Parameter<int>(1);
        [SpiceName("rdsmod"), SpiceInfo("Bias-dependent S/D resistance model selector")]
        public Parameter<int> BSIM4rdsMod { get; } = new Parameter<int>();
        [SpiceName("trnqsmod"), SpiceInfo("Transient NQS model selector")]
        public Parameter<int> BSIM4trnqsMod { get; } = new Parameter<int>();
        [SpiceName("acnqsmod"), SpiceInfo("AC NQS model selector")]
        public Parameter<int> BSIM4acnqsMod { get; } = new Parameter<int>();
        [SpiceName("rbodymod"), SpiceInfo("Distributed body R model selector")]
        public Parameter<int> BSIM4rbodyMod { get; } = new Parameter<int>();
        [SpiceName("rgatemod"), SpiceInfo("Gate R model selector")]
        public Parameter<int> BSIM4rgateMod { get; } = new Parameter<int>();
        [SpiceName("permod"), SpiceInfo("Pd and Ps model selector")]
        public Parameter<int> BSIM4perMod { get; } = new Parameter<int>(1);
        [SpiceName("geomod"), SpiceInfo("Geometry dependent parasitics model selector")]
        public Parameter<int> BSIM4geoMod { get; } = new Parameter<int>();
        [SpiceName("fnoimod"), SpiceInfo("Flicker noise model selector")]
        public Parameter<int> BSIM4fnoiMod { get; } = new Parameter<int>(1);
        [SpiceName("tnoimod"), SpiceInfo("Thermal noise model selector")]
        public Parameter<int> BSIM4tnoiMod { get; } = new Parameter<int>();
        [SpiceName("mtrlmod"), SpiceInfo("parameter for non-silicon substrate or metal gate selector")]
        public Parameter<int> BSIM4mtrlMod { get; } = new Parameter<int>();
        [SpiceName("mtrlcompatmod"), SpiceInfo("New Material Mod backward compatibility selector")]
        public Parameter<int> BSIM4mtrlCompatMod { get; } = new Parameter<int>();
        [SpiceName("gidlmod"), SpiceInfo("parameter for GIDL selector")]
        public Parameter<int> BSIM4gidlMod { get; } = new Parameter<int>();
        [SpiceName("igcmod"), SpiceInfo("Gate-to-channel Ig model selector")]
        public Parameter<int> BSIM4igcMod { get; } = new Parameter<int>();
        [SpiceName("igbmod"), SpiceInfo("Gate-to-body Ig model selector")]
        public Parameter<int> BSIM4igbMod { get; } = new Parameter<int>();
        [SpiceName("tempmod"), SpiceInfo("Temperature model selector")]
        public Parameter<int> BSIM4tempMod { get; } = new Parameter<int>();
        [SpiceName("version"), SpiceInfo("parameter for model version")]
        public Parameter<double> BSIM4version { get; } = new Parameter<double>(4.80);
        [SpiceName("toxref"), SpiceInfo("Target tox value")]
        public Parameter<double> BSIM4toxref { get; } = new Parameter<double>(30.0e-10);
        [SpiceName("eot"), SpiceInfo("Equivalent gate oxide thickness in meters")]
        public Parameter<double> BSIM4eot { get; } = new Parameter<double>(15.0e-10);
        [SpiceName("vddeot"), SpiceInfo("Voltage for extraction of Equivalent gate oxide thickness")]
        public Parameter<double> BSIM4vddeot { get; } = new Parameter<double>();
        [SpiceName("tempeot"), SpiceInfo(" Temperature for extraction of EOT")]
        public Parameter<double> BSIM4tempeot { get; } = new Parameter<double>(300.15);
        [SpiceName("leffeot"), SpiceInfo(" Effective length for extraction of EOT")]
        public Parameter<double> BSIM4leffeot { get; } = new Parameter<double>(1);
        [SpiceName("weffeot"), SpiceInfo("Effective width for extraction of EOT")]
        public Parameter<double> BSIM4weffeot { get; } = new Parameter<double>(10);
        [SpiceName("ados"), SpiceInfo("Charge centroid parameter")]
        public Parameter<double> BSIM4ados { get; } = new Parameter<double>();
        [SpiceName("bdos"), SpiceInfo("Charge centroid parameter")]
        public Parameter<double> BSIM4bdos { get; } = new Parameter<double>();
        [SpiceName("toxe"), SpiceInfo("Electrical gate oxide thickness in meters")]
        public Parameter<double> BSIM4toxe { get; } = new Parameter<double>(30.0e-10);
        [SpiceName("toxp"), SpiceInfo("Physical gate oxide thickness in meters")]
        public Parameter<double> BSIM4toxp { get; } = new Parameter<double>();
        [SpiceName("toxm"), SpiceInfo("Gate oxide thickness at which parameters are extracted")]
        public Parameter<double> BSIM4toxm { get; } = new Parameter<double>();
        [SpiceName("dtox"), SpiceInfo("Defined as (toxe - toxp) ")]
        public Parameter<double> BSIM4dtox { get; } = new Parameter<double>();
        [SpiceName("epsrox"), SpiceInfo("Dielectric constant of the gate oxide relative to vacuum")]
        public Parameter<double> BSIM4epsrox { get; } = new Parameter<double>(3.9);
        [SpiceName("cdsc"), SpiceInfo("Drain/Source and channel coupling capacitance")]
        public Parameter<double> BSIM4cdsc { get; } = new Parameter<double>(2.4e-4);
        [SpiceName("cdscb"), SpiceInfo("Body-bias dependence of cdsc")]
        public Parameter<double> BSIM4cdscb { get; } = new Parameter<double>();
        [SpiceName("cdscd"), SpiceInfo("Drain-bias dependence of cdsc")]
        public Parameter<double> BSIM4cdscd { get; } = new Parameter<double>();
        [SpiceName("cit"), SpiceInfo("Interface state capacitance")]
        public Parameter<double> BSIM4cit { get; } = new Parameter<double>();
        [SpiceName("nfactor"), SpiceInfo("Subthreshold swing Coefficient")]
        public Parameter<double> BSIM4nfactor { get; } = new Parameter<double>();
        [SpiceName("xj"), SpiceInfo("Junction depth in meters")]
        public Parameter<double> BSIM4xj { get; } = new Parameter<double>(.15e-6);
        [SpiceName("vsat"), SpiceInfo("Saturation velocity at tnom")]
        public Parameter<double> BSIM4vsat { get; } = new Parameter<double>(8.0e4);
        [SpiceName("a0"), SpiceInfo("Non-uniform depletion width effect coefficient.")]
        public Parameter<double> BSIM4a0 { get; } = new Parameter<double>();
        [SpiceName("ags"), SpiceInfo("Gate bias  coefficient of Abulk.")]
        public Parameter<double> BSIM4ags { get; } = new Parameter<double>();
        [SpiceName("a1"), SpiceInfo("Non-saturation effect coefficient")]
        public Parameter<double> BSIM4a1 { get; } = new Parameter<double>();
        [SpiceName("a2"), SpiceInfo("Non-saturation effect coefficient")]
        public Parameter<double> BSIM4a2 { get; } = new Parameter<double>();
        [SpiceName("at"), SpiceInfo("Temperature coefficient of vsat")]
        public Parameter<double> BSIM4at { get; } = new Parameter<double>(3.3e4);
        [SpiceName("keta"), SpiceInfo("Body-bias coefficient of non-uniform depletion width effect.")]
        public Parameter<double> BSIM4keta { get; } = new Parameter<double>(-0.047);
        [SpiceName("nsub"), SpiceInfo("Substrate doping concentration")]
        public Parameter<double> BSIM4nsub { get; } = new Parameter<double>(6.0e16);
        [SpiceName("phig"), SpiceInfo("Work function of gate")]
        public Parameter<double> BSIM4phig { get; } = new Parameter<double>(4.05);
        [SpiceName("epsrgate"), SpiceInfo("Dielectric constant of gate relative to vacuum")]
        public Parameter<double> BSIM4epsrgate { get; } = new Parameter<double>(11.7);
        [SpiceName("easub"), SpiceInfo("Electron affinity of substrate")]
        public Parameter<double> BSIM4easub { get; } = new Parameter<double>(4.05);
        [SpiceName("epsrsub"), SpiceInfo("Dielectric constant of substrate relative to vacuum")]
        public Parameter<double> BSIM4epsrsub { get; } = new Parameter<double>(11.7);
        [SpiceName("ni0sub"), SpiceInfo("Intrinsic carrier concentration of substrate at 300.15K")]
        public Parameter<double> BSIM4ni0sub { get; } = new Parameter<double>(1.45e10);
        [SpiceName("bg0sub"), SpiceInfo("Band-gap of substrate at T=0K")]
        public Parameter<double> BSIM4bg0sub { get; } = new Parameter<double>(1.16);
        [SpiceName("tbgasub"), SpiceInfo("First parameter of band-gap change due to temperature")]
        public Parameter<double> BSIM4tbgasub { get; } = new Parameter<double>(7.02e-4);
        [SpiceName("tbgbsub"), SpiceInfo("Second parameter of band-gap change due to temperature")]
        public Parameter<double> BSIM4tbgbsub { get; } = new Parameter<double>();
        [SpiceName("ndep"), SpiceInfo("Channel doping concentration at the depletion edge")]
        public ParameterMethod<double> BSIM4ndep { get; } = new ParameterMethod<double>(1.7e17, (double v) => v > 1.0e20 ? v * 1e-6 : v, null);
        [SpiceName("nsd"), SpiceInfo("S/D doping concentration")]
        public ParameterMethod<double> BSIM4nsd { get; } = new ParameterMethod<double>(1.0e20, (double v) => v > 1.0e20 ? v * 1e-6 : v, null);
        [SpiceName("ngate"), SpiceInfo("Poly-gate doping concentration")]
        public ParameterMethod<double> BSIM4ngate { get; } = new ParameterMethod<double>(0.0, (double v) => v > 1.0e23 ? v * 1e-6 : v, null);
        [SpiceName("gamma1"), SpiceInfo("Vth body coefficient")]
        public Parameter<double> BSIM4gamma1 { get; } = new Parameter<double>();
        [SpiceName("gamma2"), SpiceInfo("Vth body coefficient")]
        public Parameter<double> BSIM4gamma2 { get; } = new Parameter<double>();
        [SpiceName("vbx"), SpiceInfo("Vth transition body Voltage")]
        public Parameter<double> BSIM4vbx { get; } = new Parameter<double>();
        [SpiceName("vbm"), SpiceInfo("Maximum body voltage")]
        public Parameter<double> BSIM4vbm { get; } = new Parameter<double>();
        [SpiceName("xt"), SpiceInfo("Doping depth")]
        public Parameter<double> BSIM4xt { get; } = new Parameter<double>(1.55e-7);
        [SpiceName("k1"), SpiceInfo("Bulk effect coefficient 1")]
        public Parameter<double> BSIM4k1 { get; } = new Parameter<double>();
        [SpiceName("kt1"), SpiceInfo("Temperature coefficient of Vth")]
        public Parameter<double> BSIM4kt1 { get; } = new Parameter<double>(-0.11);
        [SpiceName("kt1l"), SpiceInfo("Temperature coefficient of Vth")]
        public Parameter<double> BSIM4kt1l { get; } = new Parameter<double>();
        [SpiceName("kt2"), SpiceInfo("Body-coefficient of kt1")]
        public Parameter<double> BSIM4kt2 { get; } = new Parameter<double>();
        [SpiceName("k2"), SpiceInfo("Bulk effect coefficient 2")]
        public Parameter<double> BSIM4k2 { get; } = new Parameter<double>();
        [SpiceName("k3"), SpiceInfo("Narrow width effect coefficient")]
        public Parameter<double> BSIM4k3 { get; } = new Parameter<double>();
        [SpiceName("k3b"), SpiceInfo("Body effect coefficient of k3")]
        public Parameter<double> BSIM4k3b { get; } = new Parameter<double>();
        [SpiceName("lpe0"), SpiceInfo("Equivalent length of pocket region at zero bias")]
        public Parameter<double> BSIM4lpe0 { get; } = new Parameter<double>(1.74e-7);
        [SpiceName("lpeb"), SpiceInfo("Equivalent length of pocket region accounting for body bias")]
        public Parameter<double> BSIM4lpeb { get; } = new Parameter<double>();
        [SpiceName("dvtp0"), SpiceInfo("First parameter for Vth shift due to pocket")]
        public Parameter<double> BSIM4dvtp0 { get; } = new Parameter<double>();
        [SpiceName("dvtp1"), SpiceInfo("Second parameter for Vth shift due to pocket")]
        public Parameter<double> BSIM4dvtp1 { get; } = new Parameter<double>();
        [SpiceName("dvtp2"), SpiceInfo("3rd parameter for Vth shift due to pocket")]
        public Parameter<double> BSIM4dvtp2 { get; } = new Parameter<double>();
        [SpiceName("dvtp3"), SpiceInfo("4th parameter for Vth shift due to pocket")]
        public Parameter<double> BSIM4dvtp3 { get; } = new Parameter<double>();
        [SpiceName("dvtp4"), SpiceInfo("5th parameter for Vth shift due to pocket")]
        public Parameter<double> BSIM4dvtp4 { get; } = new Parameter<double>();
        [SpiceName("dvtp5"), SpiceInfo("6th parameter for Vth shift due to pocket")]
        public Parameter<double> BSIM4dvtp5 { get; } = new Parameter<double>();
        [SpiceName("w0"), SpiceInfo("Narrow width effect parameter")]
        public Parameter<double> BSIM4w0 { get; } = new Parameter<double>(2.5e-6);
        [SpiceName("dvt0"), SpiceInfo("Short channel effect coeff. 0")]
        public Parameter<double> BSIM4dvt0 { get; } = new Parameter<double>(2.2);
        [SpiceName("dvt1"), SpiceInfo("Short channel effect coeff. 1")]
        public Parameter<double> BSIM4dvt1 { get; } = new Parameter<double>();
        [SpiceName("dvt2"), SpiceInfo("Short channel effect coeff. 2")]
        public Parameter<double> BSIM4dvt2 { get; } = new Parameter<double>(-0.032);
        [SpiceName("dvt0w"), SpiceInfo("Narrow Width coeff. 0")]
        public Parameter<double> BSIM4dvt0w { get; } = new Parameter<double>();
        [SpiceName("dvt1w"), SpiceInfo("Narrow Width effect coeff. 1")]
        public Parameter<double> BSIM4dvt1w { get; } = new Parameter<double>(5.3e6);
        [SpiceName("dvt2w"), SpiceInfo("Narrow Width effect coeff. 2")]
        public Parameter<double> BSIM4dvt2w { get; } = new Parameter<double>(-0.032);
        [SpiceName("drout"), SpiceInfo("DIBL coefficient of output resistance")]
        public Parameter<double> BSIM4drout { get; } = new Parameter<double>();
        [SpiceName("dsub"), SpiceInfo("DIBL coefficient in the subthreshold region")]
        public Parameter<double> BSIM4dsub { get; } = new Parameter<double>();
        [SpiceName("vth0"), SpiceName("vtho"), SpiceInfo("Threshold voltage")]
        public Parameter<double> BSIM4vth0 { get; } = new Parameter<double>();
        [SpiceName("eu"), SpiceInfo("Mobility exponent")]
        public Parameter<double> BSIM4eu { get; } = new Parameter<double>();
        [SpiceName("ucs"), SpiceInfo("Colombic scattering exponent")]
        public Parameter<double> BSIM4ucs { get; } = new Parameter<double>();
        [SpiceName("ua"), SpiceInfo("Linear gate dependence of mobility")]
        public Parameter<double> BSIM4ua { get; } = new Parameter<double>();
        [SpiceName("ua1"), SpiceInfo("Temperature coefficient of ua")]
        public Parameter<double> BSIM4ua1 { get; } = new Parameter<double>(1.0e-9);
        [SpiceName("ub"), SpiceInfo("Quadratic gate dependence of mobility")]
        public Parameter<double> BSIM4ub { get; } = new Parameter<double>(1.0e-19);
        [SpiceName("ub1"), SpiceInfo("Temperature coefficient of ub")]
        public Parameter<double> BSIM4ub1 { get; } = new Parameter<double>(-1.0e-18);
        [SpiceName("uc"), SpiceInfo("Body-bias dependence of mobility")]
        public Parameter<double> BSIM4uc { get; } = new Parameter<double>();
        [SpiceName("uc1"), SpiceInfo("Temperature coefficient of uc")]
        public Parameter<double> BSIM4uc1 { get; } = new Parameter<double>();
        [SpiceName("u0"), SpiceInfo("Low-field mobility at Tnom")]
        public Parameter<double> BSIM4u0 { get; } = new Parameter<double>();
        [SpiceName("ute"), SpiceInfo("Temperature coefficient of mobility")]
        public Parameter<double> BSIM4ute { get; } = new Parameter<double>(-1.5);
        [SpiceName("ucste"), SpiceInfo("Temperature coefficient of colombic mobility")]
        public Parameter<double> BSIM4ucste { get; } = new Parameter<double>(-4.775e-3);
        [SpiceName("ud"), SpiceInfo("Coulomb scattering factor of mobility")]
        public Parameter<double> BSIM4ud { get; } = new Parameter<double>();
        [SpiceName("ud1"), SpiceInfo("Temperature coefficient of ud")]
        public Parameter<double> BSIM4ud1 { get; } = new Parameter<double>();
        [SpiceName("up"), SpiceInfo("Channel length linear factor of mobility")]
        public Parameter<double> BSIM4up { get; } = new Parameter<double>();
        [SpiceName("lp"), SpiceInfo("Channel length exponential factor of mobility")]
        public Parameter<double> BSIM4lp { get; } = new Parameter<double>(1.0e-8);
        [SpiceName("lud"), SpiceInfo("Length dependence of ud")]
        public Parameter<double> BSIM4lud { get; } = new Parameter<double>();
        [SpiceName("lud1"), SpiceInfo("Length dependence of ud1")]
        public Parameter<double> BSIM4lud1 { get; } = new Parameter<double>();
        [SpiceName("lup"), SpiceInfo("Length dependence of up")]
        public Parameter<double> BSIM4lup { get; } = new Parameter<double>();
        [SpiceName("llp"), SpiceInfo("Length dependence of lp")]
        public Parameter<double> BSIM4llp { get; } = new Parameter<double>();
        [SpiceName("wud"), SpiceInfo("Width dependence of ud")]
        public Parameter<double> BSIM4wud { get; } = new Parameter<double>();
        [SpiceName("wud1"), SpiceInfo("Width dependence of ud1")]
        public Parameter<double> BSIM4wud1 { get; } = new Parameter<double>();
        [SpiceName("wup"), SpiceInfo("Width dependence of up")]
        public Parameter<double> BSIM4wup { get; } = new Parameter<double>();
        [SpiceName("wlp"), SpiceInfo("Width dependence of lp")]
        public Parameter<double> BSIM4wlp { get; } = new Parameter<double>();
        [SpiceName("pud"), SpiceInfo("Cross-term dependence of ud")]
        public Parameter<double> BSIM4pud { get; } = new Parameter<double>();
        [SpiceName("pud1"), SpiceInfo("Cross-term dependence of ud1")]
        public Parameter<double> BSIM4pud1 { get; } = new Parameter<double>();
        [SpiceName("pup"), SpiceInfo("Cross-term dependence of up")]
        public Parameter<double> BSIM4pup { get; } = new Parameter<double>();
        [SpiceName("plp"), SpiceInfo("Cross-term dependence of lp")]
        public Parameter<double> BSIM4plp { get; } = new Parameter<double>();
        [SpiceName("voff"), SpiceInfo("Threshold voltage offset")]
        public Parameter<double> BSIM4voff { get; } = new Parameter<double>(-0.08);
        [SpiceName("tvoff"), SpiceInfo("Temperature parameter for voff")]
        public Parameter<double> BSIM4tvoff { get; } = new Parameter<double>();
        [SpiceName("tnfactor"), SpiceInfo("Temperature parameter for nfactor")]
        public Parameter<double> BSIM4tnfactor { get; } = new Parameter<double>();
        [SpiceName("teta0"), SpiceInfo("Temperature parameter for eta0")]
        public Parameter<double> BSIM4teta0 { get; } = new Parameter<double>();
        [SpiceName("tvoffcv"), SpiceInfo("Temperature parameter for tvoffcv")]
        public Parameter<double> BSIM4tvoffcv { get; } = new Parameter<double>();
        [SpiceName("voffl"), SpiceInfo("Length dependence parameter for Vth offset")]
        public Parameter<double> BSIM4voffl { get; } = new Parameter<double>();
        [SpiceName("voffcvl"), SpiceInfo("Length dependence parameter for Vth offset in CV")]
        public Parameter<double> BSIM4voffcvl { get; } = new Parameter<double>();
        [SpiceName("minv"), SpiceInfo("Fitting parameter for moderate inversion in Vgsteff")]
        public Parameter<double> BSIM4minv { get; } = new Parameter<double>();
        [SpiceName("minvcv"), SpiceInfo("Fitting parameter for moderate inversion in Vgsteffcv")]
        public Parameter<double> BSIM4minvcv { get; } = new Parameter<double>();
        [SpiceName("fprout"), SpiceInfo("Rout degradation coefficient for pocket devices")]
        public Parameter<double> BSIM4fprout { get; } = new Parameter<double>();
        [SpiceName("pdits"), SpiceInfo("Coefficient for drain-induced Vth shifts")]
        public Parameter<double> BSIM4pdits { get; } = new Parameter<double>();
        [SpiceName("pditsd"), SpiceInfo("Vds dependence of drain-induced Vth shifts")]
        public Parameter<double> BSIM4pditsd { get; } = new Parameter<double>();
        [SpiceName("pditsl"), SpiceInfo("Length dependence of drain-induced Vth shifts")]
        public Parameter<double> BSIM4pditsl { get; } = new Parameter<double>();
        [SpiceName("delta"), SpiceInfo("Effective Vds parameter")]
        public Parameter<double> BSIM4delta { get; } = new Parameter<double>();
        [SpiceName("rdsw"), SpiceInfo("Source-drain resistance per width")]
        public Parameter<double> BSIM4rdsw { get; } = new Parameter<double>();
        [SpiceName("rdswmin"), SpiceInfo("Source-drain resistance per width at high Vg")]
        public Parameter<double> BSIM4rdswmin { get; } = new Parameter<double>();
        [SpiceName("rdwmin"), SpiceInfo("Drain resistance per width at high Vg")]
        public Parameter<double> BSIM4rdwmin { get; } = new Parameter<double>();
        [SpiceName("rswmin"), SpiceInfo("Source resistance per width at high Vg")]
        public Parameter<double> BSIM4rswmin { get; } = new Parameter<double>();
        [SpiceName("rdw"), SpiceInfo("Drain resistance per width")]
        public Parameter<double> BSIM4rdw { get; } = new Parameter<double>();
        [SpiceName("rsw"), SpiceInfo("Source resistance per width")]
        public Parameter<double> BSIM4rsw { get; } = new Parameter<double>();
        [SpiceName("prwg"), SpiceInfo("Gate-bias effect on parasitic resistance ")]
        public Parameter<double> BSIM4prwg { get; } = new Parameter<double>();
        [SpiceName("prwb"), SpiceInfo("Body-effect on parasitic resistance ")]
        public Parameter<double> BSIM4prwb { get; } = new Parameter<double>();
        [SpiceName("prt"), SpiceInfo("Temperature coefficient of parasitic resistance ")]
        public Parameter<double> BSIM4prt { get; } = new Parameter<double>();
        [SpiceName("eta0"), SpiceInfo("Subthreshold region DIBL coefficient")]
        public Parameter<double> BSIM4eta0 { get; } = new Parameter<double>();
        [SpiceName("etab"), SpiceInfo("Subthreshold region DIBL coefficient")]
        public Parameter<double> BSIM4etab { get; } = new Parameter<double>(-0.07);
        [SpiceName("pclm"), SpiceInfo("Channel length modulation Coefficient")]
        public Parameter<double> BSIM4pclm { get; } = new Parameter<double>(1.3);
        [SpiceName("pdiblc1"), SpiceInfo("Drain-induced barrier lowering coefficient")]
        public Parameter<double> BSIM4pdibl1 { get; } = new Parameter<double>();
        [SpiceName("pdiblc2"), SpiceInfo("Drain-induced barrier lowering coefficient")]
        public Parameter<double> BSIM4pdibl2 { get; } = new Parameter<double>();
        [SpiceName("pdiblcb"), SpiceInfo("Body-effect on drain-induced barrier lowering")]
        public Parameter<double> BSIM4pdiblb { get; } = new Parameter<double>();
        [SpiceName("pscbe1"), SpiceInfo("Substrate current body-effect coefficient")]
        public Parameter<double> BSIM4pscbe1 { get; } = new Parameter<double>(4.24e8);
        [SpiceName("pscbe2"), SpiceInfo("Substrate current body-effect coefficient")]
        public Parameter<double> BSIM4pscbe2 { get; } = new Parameter<double>(1.0e-5);
        [SpiceName("pvag"), SpiceInfo("Gate dependence of output resistance parameter")]
        public Parameter<double> BSIM4pvag { get; } = new Parameter<double>();
        [SpiceName("wr"), SpiceInfo("Width dependence of rds")]
        public Parameter<double> BSIM4wr { get; } = new Parameter<double>();
        [SpiceName("dwg"), SpiceInfo("Width reduction parameter")]
        public Parameter<double> BSIM4dwg { get; } = new Parameter<double>();
        [SpiceName("dwb"), SpiceInfo("Width reduction parameter")]
        public Parameter<double> BSIM4dwb { get; } = new Parameter<double>();
        [SpiceName("b0"), SpiceInfo("Abulk narrow width parameter")]
        public Parameter<double> BSIM4b0 { get; } = new Parameter<double>();
        [SpiceName("b1"), SpiceInfo("Abulk narrow width parameter")]
        public Parameter<double> BSIM4b1 { get; } = new Parameter<double>();
        [SpiceName("alpha0"), SpiceInfo("substrate current model parameter")]
        public Parameter<double> BSIM4alpha0 { get; } = new Parameter<double>();
        [SpiceName("alpha1"), SpiceInfo("substrate current model parameter")]
        public Parameter<double> BSIM4alpha1 { get; } = new Parameter<double>();
        [SpiceName("phin"), SpiceInfo("Adjusting parameter for surface potential due to non-uniform vertical doping")]
        public Parameter<double> BSIM4phin { get; } = new Parameter<double>();
        [SpiceName("agidl"), SpiceInfo("Pre-exponential constant for GIDL")]
        public Parameter<double> BSIM4agidl { get; } = new Parameter<double>();
        [SpiceName("bgidl"), SpiceInfo("Exponential constant for GIDL")]
        public Parameter<double> BSIM4bgidl { get; } = new Parameter<double>(2.3e9);
        [SpiceName("cgidl"), SpiceInfo("Parameter for body-bias dependence of GIDL")]
        public Parameter<double> BSIM4cgidl { get; } = new Parameter<double>();
        [SpiceName("egidl"), SpiceInfo("Fitting parameter for Bandbending")]
        public Parameter<double> BSIM4egidl { get; } = new Parameter<double>();
        [SpiceName("fgidl"), SpiceInfo("GIDL vb parameter")]
        public Parameter<double> BSIM4fgidl { get; } = new Parameter<double>(1.0);
        [SpiceName("kgidl"), SpiceInfo("GIDL vb parameter")]
        public Parameter<double> BSIM4kgidl { get; } = new Parameter<double>();
        [SpiceName("rgidl"), SpiceInfo("GIDL vg parameter")]
        public Parameter<double> BSIM4rgidl { get; } = new Parameter<double>(1.0);
        [SpiceName("agisl"), SpiceInfo("Pre-exponential constant for GISL")]
        public Parameter<double> BSIM4agisl { get; } = new Parameter<double>();
        [SpiceName("bgisl"), SpiceInfo("Exponential constant for GISL")]
        public Parameter<double> BSIM4bgisl { get; } = new Parameter<double>();
        [SpiceName("cgisl"), SpiceInfo("Parameter for body-bias dependence of GISL")]
        public Parameter<double> BSIM4cgisl { get; } = new Parameter<double>();
        [SpiceName("egisl"), SpiceInfo("Fitting parameter for Bandbending")]
        public Parameter<double> BSIM4egisl { get; } = new Parameter<double>();
        [SpiceName("fgisl"), SpiceInfo("GISL vb parameter")]
        public Parameter<double> BSIM4fgisl { get; } = new Parameter<double>();
        [SpiceName("kgisl"), SpiceInfo("GISL vb parameter")]
        public Parameter<double> BSIM4kgisl { get; } = new Parameter<double>();
        [SpiceName("rgisl"), SpiceInfo("GISL vg parameter")]
        public Parameter<double> BSIM4rgisl { get; } = new Parameter<double>();
        [SpiceName("aigc"), SpiceInfo("Parameter for Igc")]
        public Parameter<double> BSIM4aigc { get; } = new Parameter<double>();
        [SpiceName("bigc"), SpiceInfo("Parameter for Igc")]
        public Parameter<double> BSIM4bigc { get; } = new Parameter<double>();
        [SpiceName("cigc"), SpiceInfo("Parameter for Igc")]
        public Parameter<double> BSIM4cigc { get; } = new Parameter<double>();
        [SpiceName("aigsd"), SpiceInfo("Parameter for Igs,d")]
        public Parameter<double> BSIM4aigsd { get; } = new Parameter<double>();
        [SpiceName("bigsd"), SpiceInfo("Parameter for Igs,d")]
        public Parameter<double> BSIM4bigsd { get; } = new Parameter<double>();
        [SpiceName("cigsd"), SpiceInfo("Parameter for Igs,d")]
        public Parameter<double> BSIM4cigsd { get; } = new Parameter<double>();
        [SpiceName("aigs"), SpiceInfo("Parameter for Igs")]
        public Parameter<double> BSIM4aigs { get; } = new Parameter<double>();
        [SpiceName("bigs"), SpiceInfo("Parameter for Igs")]
        public Parameter<double> BSIM4bigs { get; } = new Parameter<double>();
        [SpiceName("cigs"), SpiceInfo("Parameter for Igs")]
        public Parameter<double> BSIM4cigs { get; } = new Parameter<double>();
        [SpiceName("aigd"), SpiceInfo("Parameter for Igd")]
        public Parameter<double> BSIM4aigd { get; } = new Parameter<double>();
        [SpiceName("bigd"), SpiceInfo("Parameter for Igd")]
        public Parameter<double> BSIM4bigd { get; } = new Parameter<double>();
        [SpiceName("cigd"), SpiceInfo("Parameter for Igd")]
        public Parameter<double> BSIM4cigd { get; } = new Parameter<double>();
        [SpiceName("aigbacc"), SpiceInfo("Parameter for Igb")]
        public Parameter<double> BSIM4aigbacc { get; } = new Parameter<double>(1.36e-2);
        [SpiceName("bigbacc"), SpiceInfo("Parameter for Igb")]
        public Parameter<double> BSIM4bigbacc { get; } = new Parameter<double>(1.71e-3);
        [SpiceName("cigbacc"), SpiceInfo("Parameter for Igb")]
        public Parameter<double> BSIM4cigbacc { get; } = new Parameter<double>();
        [SpiceName("aigbinv"), SpiceInfo("Parameter for Igb")]
        public Parameter<double> BSIM4aigbinv { get; } = new Parameter<double>(1.11e-2);
        [SpiceName("bigbinv"), SpiceInfo("Parameter for Igb")]
        public Parameter<double> BSIM4bigbinv { get; } = new Parameter<double>(9.49e-4);
        [SpiceName("cigbinv"), SpiceInfo("Parameter for Igb")]
        public Parameter<double> BSIM4cigbinv { get; } = new Parameter<double>();
        [SpiceName("nigc"), SpiceInfo("Parameter for Igc slope")]
        public Parameter<double> BSIM4nigc { get; } = new Parameter<double>();
        [SpiceName("nigbinv"), SpiceInfo("Parameter for Igbinv slope")]
        public Parameter<double> BSIM4nigbinv { get; } = new Parameter<double>();
        [SpiceName("nigbacc"), SpiceInfo("Parameter for Igbacc slope")]
        public Parameter<double> BSIM4nigbacc { get; } = new Parameter<double>();
        [SpiceName("ntox"), SpiceInfo("Exponent for Tox ratio")]
        public Parameter<double> BSIM4ntox { get; } = new Parameter<double>();
        [SpiceName("eigbinv"), SpiceInfo("Parameter for the Si bandgap for Igbinv")]
        public Parameter<double> BSIM4eigbinv { get; } = new Parameter<double>(1.1);
        [SpiceName("pigcd"), SpiceInfo("Parameter for Igc partition")]
        public Parameter<double> BSIM4pigcd { get; } = new Parameter<double>();
        [SpiceName("poxedge"), SpiceInfo("Factor for the gate edge Tox")]
        public Parameter<double> BSIM4poxedge { get; } = new Parameter<double>();
        [SpiceName("xrcrg1"), SpiceInfo("First fitting parameter the bias-dependent Rg")]
        public Parameter<double> BSIM4xrcrg1 { get; } = new Parameter<double>();
        [SpiceName("xrcrg2"), SpiceInfo("Second fitting parameter the bias-dependent Rg")]
        public Parameter<double> BSIM4xrcrg2 { get; } = new Parameter<double>();
        [SpiceName("lambda"), SpiceInfo(" Velocity overshoot parameter")]
        public Parameter<double> BSIM4lambda { get; } = new Parameter<double>();
        [SpiceName("vtl"), SpiceInfo(" thermal velocity")]
        public Parameter<double> BSIM4vtl { get; } = new Parameter<double>(2.0e5);
        [SpiceName("xn"), SpiceInfo(" back scattering parameter")]
        public Parameter<double> BSIM4xn { get; } = new Parameter<double>();
        [SpiceName("lc"), SpiceInfo(" back scattering parameter")]
        public Parameter<double> BSIM4lc { get; } = new Parameter<double>(5.0e-9);
        [SpiceName("tnoia"), SpiceInfo("Thermal noise parameter")]
        public Parameter<double> BSIM4tnoia { get; } = new Parameter<double>(1.5);
        [SpiceName("tnoib"), SpiceInfo("Thermal noise parameter")]
        public Parameter<double> BSIM4tnoib { get; } = new Parameter<double>(3.5);
        [SpiceName("tnoic"), SpiceInfo("Thermal noise parameter")]
        public Parameter<double> BSIM4tnoic { get; } = new Parameter<double>();
        [SpiceName("rnoia"), SpiceInfo("Thermal noise coefficient")]
        public Parameter<double> BSIM4rnoia { get; } = new Parameter<double>();
        [SpiceName("rnoib"), SpiceInfo("Thermal noise coefficient")]
        public Parameter<double> BSIM4rnoib { get; } = new Parameter<double>();
        [SpiceName("rnoic"), SpiceInfo("Thermal noise coefficient")]
        public Parameter<double> BSIM4rnoic { get; } = new Parameter<double>();
        [SpiceName("ntnoi"), SpiceInfo("Thermal noise parameter")]
        public Parameter<double> BSIM4ntnoi { get; } = new Parameter<double>();
        [SpiceName("vfbsdoff"), SpiceInfo("S/D flatband voltage offset")]
        public Parameter<double> BSIM4vfbsdoff { get; } = new Parameter<double>();
        [SpiceName("tvfbsdoff"), SpiceInfo("Temperature parameter for vfbsdoff")]
        public Parameter<double> BSIM4tvfbsdoff { get; } = new Parameter<double>();
        [SpiceName("lintnoi"), SpiceInfo("lint offset for noise calculation")]
        public Parameter<double> BSIM4lintnoi { get; } = new Parameter<double>();
        [SpiceName("saref"), SpiceInfo("Reference distance between OD edge to poly of one side")]
        public Parameter<double> BSIM4saref { get; } = new Parameter<double>(1e-6);
        [SpiceName("sbref"), SpiceInfo("Reference distance between OD edge to poly of the other side")]
        public Parameter<double> BSIM4sbref { get; } = new Parameter<double>(1e-6);
        [SpiceName("wlod"), SpiceInfo("Width parameter for stress effect")]
        public Parameter<double> BSIM4wlod { get; } = new Parameter<double>();
        [SpiceName("ku0"), SpiceInfo("Mobility degradation/enhancement coefficient for LOD")]
        public Parameter<double> BSIM4ku0 { get; } = new Parameter<double>();
        [SpiceName("kvsat"), SpiceInfo("Saturation velocity degradation/enhancement parameter for LOD")]
        public Parameter<double> BSIM4kvsat { get; } = new Parameter<double>();
        [SpiceName("kvth0"), SpiceInfo("Threshold degradation/enhancement parameter for LOD")]
        public Parameter<double> BSIM4kvth0 { get; } = new Parameter<double>();
        [SpiceName("tku0"), SpiceInfo("Temperature coefficient of KU0")]
        public Parameter<double> BSIM4tku0 { get; } = new Parameter<double>();
        [SpiceName("llodku0"), SpiceInfo("Length parameter for u0 LOD effect")]
        public Parameter<double> BSIM4llodku0 { get; } = new Parameter<double>();
        [SpiceName("wlodku0"), SpiceInfo("Width parameter for u0 LOD effect")]
        public Parameter<double> BSIM4wlodku0 { get; } = new Parameter<double>();
        [SpiceName("llodvth"), SpiceInfo("Length parameter for vth LOD effect")]
        public Parameter<double> BSIM4llodvth { get; } = new Parameter<double>();
        [SpiceName("wlodvth"), SpiceInfo("Width parameter for vth LOD effect")]
        public Parameter<double> BSIM4wlodvth { get; } = new Parameter<double>();
        [SpiceName("lku0"), SpiceInfo("Length dependence of ku0")]
        public Parameter<double> BSIM4lku0 { get; } = new Parameter<double>();
        [SpiceName("wku0"), SpiceInfo("Width dependence of ku0")]
        public Parameter<double> BSIM4wku0 { get; } = new Parameter<double>();
        [SpiceName("pku0"), SpiceInfo("Cross-term dependence of ku0")]
        public Parameter<double> BSIM4pku0 { get; } = new Parameter<double>();
        [SpiceName("lkvth0"), SpiceInfo("Length dependence of kvth0")]
        public Parameter<double> BSIM4lkvth0 { get; } = new Parameter<double>();
        [SpiceName("wkvth0"), SpiceInfo("Width dependence of kvth0")]
        public Parameter<double> BSIM4wkvth0 { get; } = new Parameter<double>();
        [SpiceName("pkvth0"), SpiceInfo("Cross-term dependence of kvth0")]
        public Parameter<double> BSIM4pkvth0 { get; } = new Parameter<double>();
        [SpiceName("stk2"), SpiceInfo("K2 shift factor related to stress effect on vth")]
        public Parameter<double> BSIM4stk2 { get; } = new Parameter<double>();
        [SpiceName("lodk2"), SpiceInfo("K2 shift modification factor for stress effect")]
        public Parameter<double> BSIM4lodk2 { get; } = new Parameter<double>();
        [SpiceName("steta0"), SpiceInfo("eta0 shift factor related to stress effect on vth")]
        public Parameter<double> BSIM4steta0 { get; } = new Parameter<double>();
        [SpiceName("lodeta0"), SpiceInfo("eta0 shift modification factor for stress effect")]
        public Parameter<double> BSIM4lodeta0 { get; } = new Parameter<double>();
        [SpiceName("web"), SpiceInfo("Coefficient for SCB")]
        public Parameter<double> BSIM4web { get; } = new Parameter<double>();
        [SpiceName("wec"), SpiceInfo("Coefficient for SCC")]
        public Parameter<double> BSIM4wec { get; } = new Parameter<double>();
        [SpiceName("kvth0we"), SpiceInfo("Threshold shift factor for well proximity effect")]
        public Parameter<double> BSIM4kvth0we { get; } = new Parameter<double>();
        [SpiceName("k2we"), SpiceInfo(" K2 shift factor for well proximity effect ")]
        public Parameter<double> BSIM4k2we { get; } = new Parameter<double>();
        [SpiceName("ku0we"), SpiceInfo(" Mobility degradation factor for well proximity effect ")]
        public Parameter<double> BSIM4ku0we { get; } = new Parameter<double>();
        [SpiceName("scref"), SpiceInfo(" Reference distance to calculate SCA, SCB and SCC")]
        public Parameter<double> BSIM4scref { get; } = new Parameter<double>(1.0E-6);
        [SpiceName("wpemod"), SpiceInfo(" Flag for WPE model (WPEMOD=1 to activate this model) ")]
        public Parameter<double> BSIM4wpemod { get; } = new Parameter<double>();
        [SpiceName("lkvth0we"), SpiceInfo("Length dependence of kvth0we")]
        public Parameter<double> BSIM4lkvth0we { get; } = new Parameter<double>();
        [SpiceName("lk2we"), SpiceInfo(" Length dependence of k2we ")]
        public Parameter<double> BSIM4lk2we { get; } = new Parameter<double>();
        [SpiceName("lku0we"), SpiceInfo(" Length dependence of ku0we ")]
        public Parameter<double> BSIM4lku0we { get; } = new Parameter<double>();
        [SpiceName("wkvth0we"), SpiceInfo("Width dependence of kvth0we")]
        public Parameter<double> BSIM4wkvth0we { get; } = new Parameter<double>();
        [SpiceName("wk2we"), SpiceInfo(" Width dependence of k2we ")]
        public Parameter<double> BSIM4wk2we { get; } = new Parameter<double>();
        [SpiceName("wku0we"), SpiceInfo(" Width dependence of ku0we ")]
        public Parameter<double> BSIM4wku0we { get; } = new Parameter<double>();
        [SpiceName("pkvth0we"), SpiceInfo("Cross-term dependence of kvth0we")]
        public Parameter<double> BSIM4pkvth0we { get; } = new Parameter<double>();
        [SpiceName("pk2we"), SpiceInfo(" Cross-term dependence of k2we ")]
        public Parameter<double> BSIM4pk2we { get; } = new Parameter<double>();
        [SpiceName("pku0we"), SpiceInfo(" Cross-term dependence of ku0we ")]
        public Parameter<double> BSIM4pku0we { get; } = new Parameter<double>();
        [SpiceName("beta0"), SpiceInfo("substrate current model parameter")]
        public Parameter<double> BSIM4beta0 { get; } = new Parameter<double>();
        [SpiceName("ijthdfwd"), SpiceInfo("Forward drain diode forward limiting current")]
        public Parameter<double> BSIM4ijthdfwd { get; } = new Parameter<double>();
        [SpiceName("ijthsfwd"), SpiceInfo("Forward source diode forward limiting current")]
        public Parameter<double> BSIM4ijthsfwd { get; } = new Parameter<double>();
        [SpiceName("ijthdrev"), SpiceInfo("Reverse drain diode forward limiting current")]
        public Parameter<double> BSIM4ijthdrev { get; } = new Parameter<double>();
        [SpiceName("ijthsrev"), SpiceInfo("Reverse source diode forward limiting current")]
        public Parameter<double> BSIM4ijthsrev { get; } = new Parameter<double>();
        [SpiceName("xjbvd"), SpiceInfo("Fitting parameter for drain diode breakdown current")]
        public Parameter<double> BSIM4xjbvd { get; } = new Parameter<double>();
        [SpiceName("xjbvs"), SpiceInfo("Fitting parameter for source diode breakdown current")]
        public Parameter<double> BSIM4xjbvs { get; } = new Parameter<double>();
        [SpiceName("bvd"), SpiceInfo("Drain diode breakdown voltage")]
        public Parameter<double> BSIM4bvd { get; } = new Parameter<double>();
        [SpiceName("bvs"), SpiceInfo("Source diode breakdown voltage")]
        public Parameter<double> BSIM4bvs { get; } = new Parameter<double>();
        [SpiceName("jtss"), SpiceInfo("Source bottom trap-assisted saturation current density")]
        public Parameter<double> BSIM4jtss { get; } = new Parameter<double>();
        [SpiceName("jtsd"), SpiceInfo("Drain bottom trap-assisted saturation current density")]
        public Parameter<double> BSIM4jtsd { get; } = new Parameter<double>();
        [SpiceName("jtssws"), SpiceInfo("Source STI sidewall trap-assisted saturation current density")]
        public Parameter<double> BSIM4jtssws { get; } = new Parameter<double>();
        [SpiceName("jtsswd"), SpiceInfo("Drain STI sidewall trap-assisted saturation current density")]
        public Parameter<double> BSIM4jtsswd { get; } = new Parameter<double>();
        [SpiceName("jtsswgs"), SpiceInfo("Source gate-edge sidewall trap-assisted saturation current density")]
        public Parameter<double> BSIM4jtsswgs { get; } = new Parameter<double>();
        [SpiceName("jtsswgd"), SpiceInfo("Drain gate-edge sidewall trap-assisted saturation current density")]
        public Parameter<double> BSIM4jtsswgd { get; } = new Parameter<double>();
        [SpiceName("jtweff"), SpiceInfo("TAT current width dependance")]
        public Parameter<double> BSIM4jtweff { get; } = new Parameter<double>();
        [SpiceName("njts"), SpiceInfo("Non-ideality factor for bottom junction")]
        public Parameter<double> BSIM4njts { get; } = new Parameter<double>();
        [SpiceName("njtssw"), SpiceInfo("Non-ideality factor for STI sidewall junction")]
        public Parameter<double> BSIM4njtssw { get; } = new Parameter<double>();
        [SpiceName("njtsswg"), SpiceInfo("Non-ideality factor for gate-edge sidewall junction")]
        public Parameter<double> BSIM4njtsswg { get; } = new Parameter<double>();
        [SpiceName("njtsd"), SpiceInfo("Non-ideality factor for bottom junction drain side")]
        public Parameter<double> BSIM4njtsd { get; } = new Parameter<double>();
        [SpiceName("njtsswd"), SpiceInfo("Non-ideality factor for STI sidewall junction drain side")]
        public Parameter<double> BSIM4njtsswd { get; } = new Parameter<double>();
        [SpiceName("njtsswgd"), SpiceInfo("Non-ideality factor for gate-edge sidewall junction drain side")]
        public Parameter<double> BSIM4njtsswgd { get; } = new Parameter<double>();
        [SpiceName("xtss"), SpiceInfo("Power dependence of JTSS on temperature")]
        public Parameter<double> BSIM4xtss { get; } = new Parameter<double>();
        [SpiceName("xtsd"), SpiceInfo("Power dependence of JTSD on temperature")]
        public Parameter<double> BSIM4xtsd { get; } = new Parameter<double>();
        [SpiceName("xtssws"), SpiceInfo("Power dependence of JTSSWS on temperature")]
        public Parameter<double> BSIM4xtssws { get; } = new Parameter<double>();
        [SpiceName("xtsswd"), SpiceInfo("Power dependence of JTSSWD on temperature")]
        public Parameter<double> BSIM4xtsswd { get; } = new Parameter<double>();
        [SpiceName("xtsswgs"), SpiceInfo("Power dependence of JTSSWGS on temperature")]
        public Parameter<double> BSIM4xtsswgs { get; } = new Parameter<double>();
        [SpiceName("xtsswgd"), SpiceInfo("Power dependence of JTSSWGD on temperature")]
        public Parameter<double> BSIM4xtsswgd { get; } = new Parameter<double>();
        [SpiceName("tnjts"), SpiceInfo("Temperature coefficient for NJTS")]
        public Parameter<double> BSIM4tnjts { get; } = new Parameter<double>();
        [SpiceName("tnjtssw"), SpiceInfo("Temperature coefficient for NJTSSW")]
        public Parameter<double> BSIM4tnjtssw { get; } = new Parameter<double>();
        [SpiceName("tnjtsswg"), SpiceInfo("Temperature coefficient for NJTSSWG")]
        public Parameter<double> BSIM4tnjtsswg { get; } = new Parameter<double>();
        [SpiceName("tnjtsd"), SpiceInfo("Temperature coefficient for NJTSD")]
        public Parameter<double> BSIM4tnjtsd { get; } = new Parameter<double>();
        [SpiceName("tnjtsswd"), SpiceInfo("Temperature coefficient for NJTSSWD")]
        public Parameter<double> BSIM4tnjtsswd { get; } = new Parameter<double>();
        [SpiceName("tnjtsswgd"), SpiceInfo("Temperature coefficient for NJTSSWGD")]
        public Parameter<double> BSIM4tnjtsswgd { get; } = new Parameter<double>();
        [SpiceName("vtss"), SpiceInfo("Source bottom trap-assisted voltage dependent parameter")]
        public Parameter<double> BSIM4vtss { get; } = new Parameter<double>();
        [SpiceName("vtsd"), SpiceInfo("Drain bottom trap-assisted voltage dependent parameter")]
        public Parameter<double> BSIM4vtsd { get; } = new Parameter<double>();
        [SpiceName("vtssws"), SpiceInfo("Source STI sidewall trap-assisted voltage dependent parameter")]
        public Parameter<double> BSIM4vtssws { get; } = new Parameter<double>();
        [SpiceName("vtsswd"), SpiceInfo("Drain STI sidewall trap-assisted voltage dependent parameter")]
        public Parameter<double> BSIM4vtsswd { get; } = new Parameter<double>();
        [SpiceName("vtsswgs"), SpiceInfo("Source gate-edge sidewall trap-assisted voltage dependent parameter")]
        public Parameter<double> BSIM4vtsswgs { get; } = new Parameter<double>();
        [SpiceName("vtsswgd"), SpiceInfo("Drain gate-edge sidewall trap-assisted voltage dependent parameter")]
        public Parameter<double> BSIM4vtsswgd { get; } = new Parameter<double>();
        [SpiceName("vfb"), SpiceInfo("Flat Band Voltage")]
        public Parameter<double> BSIM4vfb { get; } = new Parameter<double>();
        [SpiceName("gbmin"), SpiceInfo("Minimum body conductance")]
        public Parameter<double> BSIM4gbmin { get; } = new Parameter<double>(1.0e-12);
        [SpiceName("rbdb"), SpiceInfo("Resistance between bNode and dbNode")]
        public Parameter<double> BSIM4rbdb { get; } = new Parameter<double>();
        [SpiceName("rbpb"), SpiceInfo("Resistance between bNodePrime and bNode")]
        public Parameter<double> BSIM4rbpb { get; } = new Parameter<double>();
        [SpiceName("rbsb"), SpiceInfo("Resistance between bNode and sbNode")]
        public Parameter<double> BSIM4rbsb { get; } = new Parameter<double>();
        [SpiceName("rbps"), SpiceInfo("Resistance between bNodePrime and sbNode")]
        public Parameter<double> BSIM4rbps { get; } = new Parameter<double>();
        [SpiceName("rbpd"), SpiceInfo("Resistance between bNodePrime and bNode")]
        public Parameter<double> BSIM4rbpd { get; } = new Parameter<double>();
        [SpiceName("rbps0"), SpiceInfo("Body resistance RBPS scaling")]
        public Parameter<double> BSIM4rbps0 { get; } = new Parameter<double>();
        [SpiceName("rbpsl"), SpiceInfo("Body resistance RBPS L scaling")]
        public Parameter<double> BSIM4rbpsl { get; } = new Parameter<double>();
        [SpiceName("rbpsw"), SpiceInfo("Body resistance RBPS W scaling")]
        public Parameter<double> BSIM4rbpsw { get; } = new Parameter<double>();
        [SpiceName("rbpsnf"), SpiceInfo("Body resistance RBPS NF scaling")]
        public Parameter<double> BSIM4rbpsnf { get; } = new Parameter<double>();
        [SpiceName("rbpd0"), SpiceInfo("Body resistance RBPD scaling")]
        public Parameter<double> BSIM4rbpd0 { get; } = new Parameter<double>();
        [SpiceName("rbpdl"), SpiceInfo("Body resistance RBPD L scaling")]
        public Parameter<double> BSIM4rbpdl { get; } = new Parameter<double>();
        [SpiceName("rbpdw"), SpiceInfo("Body resistance RBPD W scaling")]
        public Parameter<double> BSIM4rbpdw { get; } = new Parameter<double>();
        [SpiceName("rbpdnf"), SpiceInfo("Body resistance RBPD NF scaling")]
        public Parameter<double> BSIM4rbpdnf { get; } = new Parameter<double>();
        [SpiceName("rbpbx0"), SpiceInfo("Body resistance RBPBX  scaling")]
        public Parameter<double> BSIM4rbpbx0 { get; } = new Parameter<double>();
        [SpiceName("rbpbxl"), SpiceInfo("Body resistance RBPBX L scaling")]
        public Parameter<double> BSIM4rbpbxl { get; } = new Parameter<double>();
        [SpiceName("rbpbxw"), SpiceInfo("Body resistance RBPBX W scaling")]
        public Parameter<double> BSIM4rbpbxw { get; } = new Parameter<double>();
        [SpiceName("rbpbxnf"), SpiceInfo("Body resistance RBPBX NF scaling")]
        public Parameter<double> BSIM4rbpbxnf { get; } = new Parameter<double>();
        [SpiceName("rbpby0"), SpiceInfo("Body resistance RBPBY  scaling")]
        public Parameter<double> BSIM4rbpby0 { get; } = new Parameter<double>();
        [SpiceName("rbpbyl"), SpiceInfo("Body resistance RBPBY L scaling")]
        public Parameter<double> BSIM4rbpbyl { get; } = new Parameter<double>();
        [SpiceName("rbpbyw"), SpiceInfo("Body resistance RBPBY W scaling")]
        public Parameter<double> BSIM4rbpbyw { get; } = new Parameter<double>();
        [SpiceName("rbpbynf"), SpiceInfo("Body resistance RBPBY NF scaling")]
        public Parameter<double> BSIM4rbpbynf { get; } = new Parameter<double>();
        [SpiceName("rbsbx0"), SpiceInfo("Body resistance RBSBX  scaling")]
        public Parameter<double> BSIM4rbsbx0 { get; } = new Parameter<double>();
        [SpiceName("rbsby0"), SpiceInfo("Body resistance RBSBY  scaling")]
        public Parameter<double> BSIM4rbsby0 { get; } = new Parameter<double>();
        [SpiceName("rbdbx0"), SpiceInfo("Body resistance RBDBX  scaling")]
        public Parameter<double> BSIM4rbdbx0 { get; } = new Parameter<double>();
        [SpiceName("rbdby0"), SpiceInfo("Body resistance RBDBY  scaling")]
        public Parameter<double> BSIM4rbdby0 { get; } = new Parameter<double>();
        [SpiceName("rbsdbxl"), SpiceInfo("Body resistance RBSDBX L scaling")]
        public Parameter<double> BSIM4rbsdbxl { get; } = new Parameter<double>();
        [SpiceName("rbsdbxw"), SpiceInfo("Body resistance RBSDBX W scaling")]
        public Parameter<double> BSIM4rbsdbxw { get; } = new Parameter<double>();
        [SpiceName("rbsdbxnf"), SpiceInfo("Body resistance RBSDBX NF scaling")]
        public Parameter<double> BSIM4rbsdbxnf { get; } = new Parameter<double>();
        [SpiceName("rbsdbyl"), SpiceInfo("Body resistance RBSDBY L scaling")]
        public Parameter<double> BSIM4rbsdbyl { get; } = new Parameter<double>();
        [SpiceName("rbsdbyw"), SpiceInfo("Body resistance RBSDBY W scaling")]
        public Parameter<double> BSIM4rbsdbyw { get; } = new Parameter<double>();
        [SpiceName("rbsdbynf"), SpiceInfo("Body resistance RBSDBY NF scaling")]
        public Parameter<double> BSIM4rbsdbynf { get; } = new Parameter<double>();
        [SpiceName("cgsl"), SpiceInfo("New C-V model parameter")]
        public Parameter<double> BSIM4cgsl { get; } = new Parameter<double>();
        [SpiceName("cgdl"), SpiceInfo("New C-V model parameter")]
        public Parameter<double> BSIM4cgdl { get; } = new Parameter<double>();
        [SpiceName("ckappas"), SpiceInfo("S/G overlap C-V parameter ")]
        public Parameter<double> BSIM4ckappas { get; } = new Parameter<double>();
        [SpiceName("ckappad"), SpiceInfo("D/G overlap C-V parameter")]
        public Parameter<double> BSIM4ckappad { get; } = new Parameter<double>();
        [SpiceName("cf"), SpiceInfo("Fringe capacitance parameter")]
        public Parameter<double> BSIM4cf { get; } = new Parameter<double>();
        [SpiceName("clc"), SpiceInfo("Vdsat parameter for C-V model")]
        public Parameter<double> BSIM4clc { get; } = new Parameter<double>();
        [SpiceName("cle"), SpiceInfo("Vdsat parameter for C-V model")]
        public Parameter<double> BSIM4cle { get; } = new Parameter<double>();
        [SpiceName("dwc"), SpiceInfo("Delta W for C-V model")]
        public Parameter<double> BSIM4dwc { get; } = new Parameter<double>();
        [SpiceName("dlc"), SpiceInfo("Delta L for C-V model")]
        public Parameter<double> BSIM4dlc { get; } = new Parameter<double>();
        [SpiceName("xw"), SpiceInfo("W offset for channel width due to mask/etch effect")]
        public Parameter<double> BSIM4xw { get; } = new Parameter<double>();
        [SpiceName("xl"), SpiceInfo("L offset for channel length due to mask/etch effect")]
        public Parameter<double> BSIM4xl { get; } = new Parameter<double>();
        [SpiceName("dlcig"), SpiceInfo("Delta L for Ig model")]
        public Parameter<double> BSIM4dlcig { get; } = new Parameter<double>();
        [SpiceName("dlcigd"), SpiceInfo("Delta L for Ig model drain side")]
        public Parameter<double> BSIM4dlcigd { get; } = new Parameter<double>();
        [SpiceName("dwj"), SpiceInfo("Delta W for S/D junctions")]
        public Parameter<double> BSIM4dwj { get; } = new Parameter<double>();
        [SpiceName("vfbcv"), SpiceInfo("Flat Band Voltage parameter for capmod=0 only")]
        public Parameter<double> BSIM4vfbcv { get; } = new Parameter<double>();
        [SpiceName("acde"), SpiceInfo("Exponential coefficient for finite charge thickness")]
        public Parameter<double> BSIM4acde { get; } = new Parameter<double>();
        [SpiceName("moin"), SpiceInfo("Coefficient for gate-bias dependent surface potential")]
        public Parameter<double> BSIM4moin { get; } = new Parameter<double>();
        [SpiceName("noff"), SpiceInfo("C-V turn-on/off parameter")]
        public Parameter<double> BSIM4noff { get; } = new Parameter<double>();
        [SpiceName("voffcv"), SpiceInfo("C-V lateral-shift parameter")]
        public Parameter<double> BSIM4voffcv { get; } = new Parameter<double>();
        [SpiceName("dmcg"), SpiceInfo("Distance of Mid-Contact to Gate edge")]
        public Parameter<double> BSIM4dmcg { get; } = new Parameter<double>();
        [SpiceName("dmci"), SpiceInfo("Distance of Mid-Contact to Isolation")]
        public Parameter<double> BSIM4dmci { get; } = new Parameter<double>();
        [SpiceName("dmdg"), SpiceInfo("Distance of Mid-Diffusion to Gate edge")]
        public Parameter<double> BSIM4dmdg { get; } = new Parameter<double>();
        [SpiceName("dmcgt"), SpiceInfo("Distance of Mid-Contact to Gate edge in Test structures")]
        public Parameter<double> BSIM4dmcgt { get; } = new Parameter<double>();
        [SpiceName("xgw"), SpiceInfo("Distance from gate contact center to device edge")]
        public Parameter<double> BSIM4xgw { get; } = new Parameter<double>();
        [SpiceName("xgl"), SpiceInfo("Variation in Ldrawn")]
        public Parameter<double> BSIM4xgl { get; } = new Parameter<double>();
        [SpiceName("rshg"), SpiceInfo("Gate sheet resistance")]
        public Parameter<double> BSIM4rshg { get; } = new Parameter<double>();
        [SpiceName("ngcon"), SpiceInfo("Number of gate contacts")]
        public Parameter<double> BSIM4ngcon { get; } = new Parameter<double>();
        [SpiceName("tcj"), SpiceInfo("Temperature coefficient of cj")]
        public Parameter<double> BSIM4tcj { get; } = new Parameter<double>();
        [SpiceName("tpb"), SpiceInfo("Temperature coefficient of pb")]
        public Parameter<double> BSIM4tpb { get; } = new Parameter<double>();
        [SpiceName("tcjsw"), SpiceInfo("Temperature coefficient of cjsw")]
        public Parameter<double> BSIM4tcjsw { get; } = new Parameter<double>();
        [SpiceName("tpbsw"), SpiceInfo("Temperature coefficient of pbsw")]
        public Parameter<double> BSIM4tpbsw { get; } = new Parameter<double>();
        [SpiceName("tcjswg"), SpiceInfo("Temperature coefficient of cjswg")]
        public Parameter<double> BSIM4tcjswg { get; } = new Parameter<double>();
        [SpiceName("tpbswg"), SpiceInfo("Temperature coefficient of pbswg")]
        public Parameter<double> BSIM4tpbswg { get; } = new Parameter<double>();
        [SpiceName("lcdsc"), SpiceInfo("Length dependence of cdsc")]
        public Parameter<double> BSIM4lcdsc { get; } = new Parameter<double>();
        [SpiceName("lcdscb"), SpiceInfo("Length dependence of cdscb")]
        public Parameter<double> BSIM4lcdscb { get; } = new Parameter<double>();
        [SpiceName("lcdscd"), SpiceInfo("Length dependence of cdscd")]
        public Parameter<double> BSIM4lcdscd { get; } = new Parameter<double>();
        [SpiceName("lcit"), SpiceInfo("Length dependence of cit")]
        public Parameter<double> BSIM4lcit { get; } = new Parameter<double>();
        [SpiceName("lnfactor"), SpiceInfo("Length dependence of nfactor")]
        public Parameter<double> BSIM4lnfactor { get; } = new Parameter<double>();
        [SpiceName("lxj"), SpiceInfo("Length dependence of xj")]
        public Parameter<double> BSIM4lxj { get; } = new Parameter<double>();
        [SpiceName("lvsat"), SpiceInfo("Length dependence of vsat")]
        public Parameter<double> BSIM4lvsat { get; } = new Parameter<double>();
        [SpiceName("la0"), SpiceInfo("Length dependence of a0")]
        public Parameter<double> BSIM4la0 { get; } = new Parameter<double>();
        [SpiceName("lags"), SpiceInfo("Length dependence of ags")]
        public Parameter<double> BSIM4lags { get; } = new Parameter<double>();
        [SpiceName("la1"), SpiceInfo("Length dependence of a1")]
        public Parameter<double> BSIM4la1 { get; } = new Parameter<double>();
        [SpiceName("la2"), SpiceInfo("Length dependence of a2")]
        public Parameter<double> BSIM4la2 { get; } = new Parameter<double>();
        [SpiceName("lat"), SpiceInfo("Length dependence of at")]
        public Parameter<double> BSIM4lat { get; } = new Parameter<double>();
        [SpiceName("lketa"), SpiceInfo("Length dependence of keta")]
        public Parameter<double> BSIM4lketa { get; } = new Parameter<double>();
        [SpiceName("lnsub"), SpiceInfo("Length dependence of nsub")]
        public Parameter<double> BSIM4lnsub { get; } = new Parameter<double>();
        [SpiceName("lndep"), SpiceInfo("Length dependence of ndep")]
        public ParameterMethod<double> BSIM4lndep { get; } = new ParameterMethod<double>(0.0, (double v) => v > 1.0e20 ? v * 1e-6 : v, null);
        [SpiceName("lnsd"), SpiceInfo("Length dependence of nsd")]
        public ParameterMethod<double> BSIM4lnsd { get; } = new ParameterMethod<double>(0.0, (double v) => v > 1.0e23 ? v * 1e-6 : v, null);
        [SpiceName("lngate"), SpiceInfo("Length dependence of ngate")]
        public ParameterMethod<double> BSIM4lngate { get; } = new ParameterMethod<double>(0.0, (double v) => v > 1.0e23 ? v * 1e-6 : v, null);
        [SpiceName("lgamma1"), SpiceInfo("Length dependence of gamma1")]
        public Parameter<double> BSIM4lgamma1 { get; } = new Parameter<double>();
        [SpiceName("lgamma2"), SpiceInfo("Length dependence of gamma2")]
        public Parameter<double> BSIM4lgamma2 { get; } = new Parameter<double>();
        [SpiceName("lvbx"), SpiceInfo("Length dependence of vbx")]
        public Parameter<double> BSIM4lvbx { get; } = new Parameter<double>();
        [SpiceName("lvbm"), SpiceInfo("Length dependence of vbm")]
        public Parameter<double> BSIM4lvbm { get; } = new Parameter<double>();
        [SpiceName("lxt"), SpiceInfo("Length dependence of xt")]
        public Parameter<double> BSIM4lxt { get; } = new Parameter<double>();
        [SpiceName("lk1"), SpiceInfo("Length dependence of k1")]
        public Parameter<double> BSIM4lk1 { get; } = new Parameter<double>();
        [SpiceName("lkt1"), SpiceInfo("Length dependence of kt1")]
        public Parameter<double> BSIM4lkt1 { get; } = new Parameter<double>();
        [SpiceName("lkt1l"), SpiceInfo("Length dependence of kt1l")]
        public Parameter<double> BSIM4lkt1l { get; } = new Parameter<double>();
        [SpiceName("lkt2"), SpiceInfo("Length dependence of kt2")]
        public Parameter<double> BSIM4lkt2 { get; } = new Parameter<double>();
        [SpiceName("lk2"), SpiceInfo("Length dependence of k2")]
        public Parameter<double> BSIM4lk2 { get; } = new Parameter<double>();
        [SpiceName("lk3"), SpiceInfo("Length dependence of k3")]
        public Parameter<double> BSIM4lk3 { get; } = new Parameter<double>();
        [SpiceName("lk3b"), SpiceInfo("Length dependence of k3b")]
        public Parameter<double> BSIM4lk3b { get; } = new Parameter<double>();
        [SpiceName("llpe0"), SpiceInfo("Length dependence of lpe0")]
        public Parameter<double> BSIM4llpe0 { get; } = new Parameter<double>();
        [SpiceName("llpeb"), SpiceInfo("Length dependence of lpeb")]
        public Parameter<double> BSIM4llpeb { get; } = new Parameter<double>();
        [SpiceName("ldvtp0"), SpiceInfo("Length dependence of dvtp0")]
        public Parameter<double> BSIM4ldvtp0 { get; } = new Parameter<double>();
        [SpiceName("ldvtp1"), SpiceInfo("Length dependence of dvtp1")]
        public Parameter<double> BSIM4ldvtp1 { get; } = new Parameter<double>();
        [SpiceName("ldvtp2"), SpiceInfo("Length dependence of dvtp2")]
        public Parameter<double> BSIM4ldvtp2 { get; } = new Parameter<double>();
        [SpiceName("ldvtp3"), SpiceInfo("Length dependence of dvtp3")]
        public Parameter<double> BSIM4ldvtp3 { get; } = new Parameter<double>();
        [SpiceName("ldvtp4"), SpiceInfo("Length dependence of dvtp4")]
        public Parameter<double> BSIM4ldvtp4 { get; } = new Parameter<double>();
        [SpiceName("ldvtp5"), SpiceInfo("Length dependence of dvtp5")]
        public Parameter<double> BSIM4ldvtp5 { get; } = new Parameter<double>();
        [SpiceName("lw0"), SpiceInfo("Length dependence of w0")]
        public Parameter<double> BSIM4lw0 { get; } = new Parameter<double>();
        [SpiceName("ldvt0"), SpiceInfo("Length dependence of dvt0")]
        public Parameter<double> BSIM4ldvt0 { get; } = new Parameter<double>();
        [SpiceName("ldvt1"), SpiceInfo("Length dependence of dvt1")]
        public Parameter<double> BSIM4ldvt1 { get; } = new Parameter<double>();
        [SpiceName("ldvt2"), SpiceInfo("Length dependence of dvt2")]
        public Parameter<double> BSIM4ldvt2 { get; } = new Parameter<double>();
        [SpiceName("ldvt0w"), SpiceInfo("Length dependence of dvt0w")]
        public Parameter<double> BSIM4ldvt0w { get; } = new Parameter<double>();
        [SpiceName("ldvt1w"), SpiceInfo("Length dependence of dvt1w")]
        public Parameter<double> BSIM4ldvt1w { get; } = new Parameter<double>();
        [SpiceName("ldvt2w"), SpiceInfo("Length dependence of dvt2w")]
        public Parameter<double> BSIM4ldvt2w { get; } = new Parameter<double>();
        [SpiceName("ldrout"), SpiceInfo("Length dependence of drout")]
        public Parameter<double> BSIM4ldrout { get; } = new Parameter<double>();
        [SpiceName("ldsub"), SpiceInfo("Length dependence of dsub")]
        public Parameter<double> BSIM4ldsub { get; } = new Parameter<double>();
        [SpiceName("lvth0"), SpiceName("lvtho"), SpiceInfo("Length dependence of vto")]
        public Parameter<double> BSIM4lvth0 { get; } = new Parameter<double>();
        [SpiceName("lua"), SpiceInfo("Length dependence of ua")]
        public Parameter<double> BSIM4lua { get; } = new Parameter<double>();
        [SpiceName("lua1"), SpiceInfo("Length dependence of ua1")]
        public Parameter<double> BSIM4lua1 { get; } = new Parameter<double>();
        [SpiceName("lub"), SpiceInfo("Length dependence of ub")]
        public Parameter<double> BSIM4lub { get; } = new Parameter<double>();
        [SpiceName("lub1"), SpiceInfo("Length dependence of ub1")]
        public Parameter<double> BSIM4lub1 { get; } = new Parameter<double>();
        [SpiceName("luc"), SpiceInfo("Length dependence of uc")]
        public Parameter<double> BSIM4luc { get; } = new Parameter<double>();
        [SpiceName("luc1"), SpiceInfo("Length dependence of uc1")]
        public Parameter<double> BSIM4luc1 { get; } = new Parameter<double>();
        [SpiceName("lu0"), SpiceInfo("Length dependence of u0")]
        public Parameter<double> BSIM4lu0 { get; } = new Parameter<double>();
        [SpiceName("lute"), SpiceInfo("Length dependence of ute")]
        public Parameter<double> BSIM4lute { get; } = new Parameter<double>();
        [SpiceName("lucste"), SpiceInfo("Length dependence of ucste")]
        public Parameter<double> BSIM4lucste { get; } = new Parameter<double>();
        [SpiceName("lvoff"), SpiceInfo("Length dependence of voff")]
        public Parameter<double> BSIM4lvoff { get; } = new Parameter<double>();
        [SpiceName("ltvoff"), SpiceInfo("Length dependence of tvoff")]
        public Parameter<double> BSIM4ltvoff { get; } = new Parameter<double>();
        [SpiceName("ltnfactor"), SpiceInfo("Length dependence of tnfactor")]
        public Parameter<double> BSIM4ltnfactor { get; } = new Parameter<double>();
        [SpiceName("lteta0"), SpiceInfo("Length dependence of teta0")]
        public Parameter<double> BSIM4lteta0 { get; } = new Parameter<double>();
        [SpiceName("ltvoffcv"), SpiceInfo("Length dependence of tvoffcv")]
        public Parameter<double> BSIM4ltvoffcv { get; } = new Parameter<double>();
        [SpiceName("lminv"), SpiceInfo("Length dependence of minv")]
        public Parameter<double> BSIM4lminv { get; } = new Parameter<double>();
        [SpiceName("lminvcv"), SpiceInfo("Length dependence of minvcv")]
        public Parameter<double> BSIM4lminvcv { get; } = new Parameter<double>();
        [SpiceName("lfprout"), SpiceInfo("Length dependence of pdiblcb")]
        public Parameter<double> BSIM4lfprout { get; } = new Parameter<double>();
        [SpiceName("lpdits"), SpiceInfo("Length dependence of pdits")]
        public Parameter<double> BSIM4lpdits { get; } = new Parameter<double>();
        [SpiceName("lpditsd"), SpiceInfo("Length dependence of pditsd")]
        public Parameter<double> BSIM4lpditsd { get; } = new Parameter<double>();
        [SpiceName("ldelta"), SpiceInfo("Length dependence of delta")]
        public Parameter<double> BSIM4ldelta { get; } = new Parameter<double>();
        [SpiceName("lrdsw"), SpiceInfo("Length dependence of rdsw ")]
        public Parameter<double> BSIM4lrdsw { get; } = new Parameter<double>();
        [SpiceName("lrdw"), SpiceInfo("Length dependence of rdw")]
        public Parameter<double> BSIM4lrdw { get; } = new Parameter<double>();
        [SpiceName("lrsw"), SpiceInfo("Length dependence of rsw")]
        public Parameter<double> BSIM4lrsw { get; } = new Parameter<double>();
        [SpiceName("lprwb"), SpiceInfo("Length dependence of prwb ")]
        public Parameter<double> BSIM4lprwb { get; } = new Parameter<double>();
        [SpiceName("lprwg"), SpiceInfo("Length dependence of prwg ")]
        public Parameter<double> BSIM4lprwg { get; } = new Parameter<double>();
        [SpiceName("lprt"), SpiceInfo("Length dependence of prt ")]
        public Parameter<double> BSIM4lprt { get; } = new Parameter<double>();
        [SpiceName("leta0"), SpiceInfo("Length dependence of eta0")]
        public Parameter<double> BSIM4leta0 { get; } = new Parameter<double>();
        [SpiceName("letab"), SpiceInfo("Length dependence of etab")]
        public Parameter<double> BSIM4letab { get; } = new Parameter<double>();
        [SpiceName("lpclm"), SpiceInfo("Length dependence of pclm")]
        public Parameter<double> BSIM4lpclm { get; } = new Parameter<double>();
        [SpiceName("lpdiblc1"), SpiceInfo("Length dependence of pdiblc1")]
        public Parameter<double> BSIM4lpdibl1 { get; } = new Parameter<double>();
        [SpiceName("lpdiblc2"), SpiceInfo("Length dependence of pdiblc2")]
        public Parameter<double> BSIM4lpdibl2 { get; } = new Parameter<double>();
        [SpiceName("lpdiblcb"), SpiceInfo("Length dependence of pdiblcb")]
        public Parameter<double> BSIM4lpdiblb { get; } = new Parameter<double>();
        [SpiceName("lpscbe1"), SpiceInfo("Length dependence of pscbe1")]
        public Parameter<double> BSIM4lpscbe1 { get; } = new Parameter<double>();
        [SpiceName("lpscbe2"), SpiceInfo("Length dependence of pscbe2")]
        public Parameter<double> BSIM4lpscbe2 { get; } = new Parameter<double>();
        [SpiceName("lpvag"), SpiceInfo("Length dependence of pvag")]
        public Parameter<double> BSIM4lpvag { get; } = new Parameter<double>();
        [SpiceName("lwr"), SpiceInfo("Length dependence of wr")]
        public Parameter<double> BSIM4lwr { get; } = new Parameter<double>();
        [SpiceName("ldwg"), SpiceInfo("Length dependence of dwg")]
        public Parameter<double> BSIM4ldwg { get; } = new Parameter<double>();
        [SpiceName("ldwb"), SpiceInfo("Length dependence of dwb")]
        public Parameter<double> BSIM4ldwb { get; } = new Parameter<double>();
        [SpiceName("lb0"), SpiceInfo("Length dependence of b0")]
        public Parameter<double> BSIM4lb0 { get; } = new Parameter<double>();
        [SpiceName("lb1"), SpiceInfo("Length dependence of b1")]
        public Parameter<double> BSIM4lb1 { get; } = new Parameter<double>();
        [SpiceName("lalpha0"), SpiceInfo("Length dependence of alpha0")]
        public Parameter<double> BSIM4lalpha0 { get; } = new Parameter<double>();
        [SpiceName("lalpha1"), SpiceInfo("Length dependence of alpha1")]
        public Parameter<double> BSIM4lalpha1 { get; } = new Parameter<double>();
        [SpiceName("lbeta0"), SpiceInfo("Length dependence of beta0")]
        public Parameter<double> BSIM4lbeta0 { get; } = new Parameter<double>();
        [SpiceName("lphin"), SpiceInfo("Length dependence of phin")]
        public Parameter<double> BSIM4lphin { get; } = new Parameter<double>();
        [SpiceName("lagidl"), SpiceInfo("Length dependence of agidl")]
        public Parameter<double> BSIM4lagidl { get; } = new Parameter<double>();
        [SpiceName("lbgidl"), SpiceInfo("Length dependence of bgidl")]
        public Parameter<double> BSIM4lbgidl { get; } = new Parameter<double>();
        [SpiceName("lcgidl"), SpiceInfo("Length dependence of cgidl")]
        public Parameter<double> BSIM4lcgidl { get; } = new Parameter<double>();
        [SpiceName("legidl"), SpiceInfo("Length dependence of egidl")]
        public Parameter<double> BSIM4legidl { get; } = new Parameter<double>();
        [SpiceName("lfgidl"), SpiceInfo("Length dependence of fgidl")]
        public Parameter<double> BSIM4lfgidl { get; } = new Parameter<double>();
        [SpiceName("lkgidl"), SpiceInfo("Length dependence of kgidl")]
        public Parameter<double> BSIM4lkgidl { get; } = new Parameter<double>();
        [SpiceName("lrgidl"), SpiceInfo("Length dependence of rgidl")]
        public Parameter<double> BSIM4lrgidl { get; } = new Parameter<double>();
        [SpiceName("lagisl"), SpiceInfo("Length dependence of agisl")]
        public Parameter<double> BSIM4lagisl { get; } = new Parameter<double>();
        [SpiceName("lbgisl"), SpiceInfo("Length dependence of bgisl")]
        public Parameter<double> BSIM4lbgisl { get; } = new Parameter<double>();
        [SpiceName("lcgisl"), SpiceInfo("Length dependence of cgisl")]
        public Parameter<double> BSIM4lcgisl { get; } = new Parameter<double>();
        [SpiceName("legisl"), SpiceInfo("Length dependence of egisl")]
        public Parameter<double> BSIM4legisl { get; } = new Parameter<double>();
        [SpiceName("lfgisl"), SpiceInfo("Length dependence of fgisl")]
        public Parameter<double> BSIM4lfgisl { get; } = new Parameter<double>();
        [SpiceName("lkgisl"), SpiceInfo("Length dependence of kgisl")]
        public Parameter<double> BSIM4lkgisl { get; } = new Parameter<double>();
        [SpiceName("lrgisl"), SpiceInfo("Length dependence of rgisl")]
        public Parameter<double> BSIM4lrgisl { get; } = new Parameter<double>();
        [SpiceName("laigc"), SpiceInfo("Length dependence of aigc")]
        public Parameter<double> BSIM4laigc { get; } = new Parameter<double>();
        [SpiceName("lbigc"), SpiceInfo("Length dependence of bigc")]
        public Parameter<double> BSIM4lbigc { get; } = new Parameter<double>();
        [SpiceName("lcigc"), SpiceInfo("Length dependence of cigc")]
        public Parameter<double> BSIM4lcigc { get; } = new Parameter<double>();
        [SpiceName("laigsd"), SpiceInfo("Length dependence of aigsd")]
        public Parameter<double> BSIM4laigsd { get; } = new Parameter<double>();
        [SpiceName("lbigsd"), SpiceInfo("Length dependence of bigsd")]
        public Parameter<double> BSIM4lbigsd { get; } = new Parameter<double>();
        [SpiceName("lcigsd"), SpiceInfo("Length dependence of cigsd")]
        public Parameter<double> BSIM4lcigsd { get; } = new Parameter<double>();
        [SpiceName("laigs"), SpiceInfo("Length dependence of aigs")]
        public Parameter<double> BSIM4laigs { get; } = new Parameter<double>();
        [SpiceName("lbigs"), SpiceInfo("Length dependence of bigs")]
        public Parameter<double> BSIM4lbigs { get; } = new Parameter<double>();
        [SpiceName("lcigs"), SpiceInfo("Length dependence of cigs")]
        public Parameter<double> BSIM4lcigs { get; } = new Parameter<double>();
        [SpiceName("laigd"), SpiceInfo("Length dependence of aigd")]
        public Parameter<double> BSIM4laigd { get; } = new Parameter<double>();
        [SpiceName("lbigd"), SpiceInfo("Length dependence of bigd")]
        public Parameter<double> BSIM4lbigd { get; } = new Parameter<double>();
        [SpiceName("lcigd"), SpiceInfo("Length dependence of cigd")]
        public Parameter<double> BSIM4lcigd { get; } = new Parameter<double>();
        [SpiceName("laigbacc"), SpiceInfo("Length dependence of aigbacc")]
        public Parameter<double> BSIM4laigbacc { get; } = new Parameter<double>();
        [SpiceName("lbigbacc"), SpiceInfo("Length dependence of bigbacc")]
        public Parameter<double> BSIM4lbigbacc { get; } = new Parameter<double>();
        [SpiceName("lcigbacc"), SpiceInfo("Length dependence of cigbacc")]
        public Parameter<double> BSIM4lcigbacc { get; } = new Parameter<double>();
        [SpiceName("laigbinv"), SpiceInfo("Length dependence of aigbinv")]
        public Parameter<double> BSIM4laigbinv { get; } = new Parameter<double>();
        [SpiceName("lbigbinv"), SpiceInfo("Length dependence of bigbinv")]
        public Parameter<double> BSIM4lbigbinv { get; } = new Parameter<double>();
        [SpiceName("lcigbinv"), SpiceInfo("Length dependence of cigbinv")]
        public Parameter<double> BSIM4lcigbinv { get; } = new Parameter<double>();
        [SpiceName("lnigc"), SpiceInfo("Length dependence of nigc")]
        public Parameter<double> BSIM4lnigc { get; } = new Parameter<double>();
        [SpiceName("lnigbinv"), SpiceInfo("Length dependence of nigbinv")]
        public Parameter<double> BSIM4lnigbinv { get; } = new Parameter<double>();
        [SpiceName("lnigbacc"), SpiceInfo("Length dependence of nigbacc")]
        public Parameter<double> BSIM4lnigbacc { get; } = new Parameter<double>();
        [SpiceName("lntox"), SpiceInfo("Length dependence of ntox")]
        public Parameter<double> BSIM4lntox { get; } = new Parameter<double>();
        [SpiceName("leigbinv"), SpiceInfo("Length dependence for eigbinv")]
        public Parameter<double> BSIM4leigbinv { get; } = new Parameter<double>();
        [SpiceName("lpigcd"), SpiceInfo("Length dependence for pigcd")]
        public Parameter<double> BSIM4lpigcd { get; } = new Parameter<double>();
        [SpiceName("lpoxedge"), SpiceInfo("Length dependence for poxedge")]
        public Parameter<double> BSIM4lpoxedge { get; } = new Parameter<double>();
        [SpiceName("lxrcrg1"), SpiceInfo("Length dependence of xrcrg1")]
        public Parameter<double> BSIM4lxrcrg1 { get; } = new Parameter<double>();
        [SpiceName("lxrcrg2"), SpiceInfo("Length dependence of xrcrg2")]
        public Parameter<double> BSIM4lxrcrg2 { get; } = new Parameter<double>();
        [SpiceName("llambda"), SpiceInfo("Length dependence of lambda")]
        public Parameter<double> BSIM4llambda { get; } = new Parameter<double>();
        [SpiceName("lvtl"), SpiceInfo(" Length dependence of vtl")]
        public Parameter<double> BSIM4lvtl { get; } = new Parameter<double>();
        [SpiceName("lxn"), SpiceInfo(" Length dependence of xn")]
        public Parameter<double> BSIM4lxn { get; } = new Parameter<double>();
        [SpiceName("lvfbsdoff"), SpiceInfo("Length dependence of vfbsdoff")]
        public Parameter<double> BSIM4lvfbsdoff { get; } = new Parameter<double>();
        [SpiceName("ltvfbsdoff"), SpiceInfo("Length dependence of tvfbsdoff")]
        public Parameter<double> BSIM4ltvfbsdoff { get; } = new Parameter<double>();
        [SpiceName("leu"), SpiceInfo(" Length dependence of eu")]
        public Parameter<double> BSIM4leu { get; } = new Parameter<double>();
        [SpiceName("lucs"), SpiceInfo("Length dependence of lucs")]
        public Parameter<double> BSIM4lucs { get; } = new Parameter<double>();
        [SpiceName("lvfb"), SpiceInfo("Length dependence of vfb")]
        public Parameter<double> BSIM4lvfb { get; } = new Parameter<double>();
        [SpiceName("lcgsl"), SpiceInfo("Length dependence of cgsl")]
        public Parameter<double> BSIM4lcgsl { get; } = new Parameter<double>();
        [SpiceName("lcgdl"), SpiceInfo("Length dependence of cgdl")]
        public Parameter<double> BSIM4lcgdl { get; } = new Parameter<double>();
        [SpiceName("lckappas"), SpiceInfo("Length dependence of ckappas")]
        public Parameter<double> BSIM4lckappas { get; } = new Parameter<double>();
        [SpiceName("lckappad"), SpiceInfo("Length dependence of ckappad")]
        public Parameter<double> BSIM4lckappad { get; } = new Parameter<double>();
        [SpiceName("lcf"), SpiceInfo("Length dependence of cf")]
        public Parameter<double> BSIM4lcf { get; } = new Parameter<double>();
        [SpiceName("lclc"), SpiceInfo("Length dependence of clc")]
        public Parameter<double> BSIM4lclc { get; } = new Parameter<double>();
        [SpiceName("lcle"), SpiceInfo("Length dependence of cle")]
        public Parameter<double> BSIM4lcle { get; } = new Parameter<double>();
        [SpiceName("lvfbcv"), SpiceInfo("Length dependence of vfbcv")]
        public Parameter<double> BSIM4lvfbcv { get; } = new Parameter<double>();
        [SpiceName("lacde"), SpiceInfo("Length dependence of acde")]
        public Parameter<double> BSIM4lacde { get; } = new Parameter<double>();
        [SpiceName("lmoin"), SpiceInfo("Length dependence of moin")]
        public Parameter<double> BSIM4lmoin { get; } = new Parameter<double>();
        [SpiceName("lnoff"), SpiceInfo("Length dependence of noff")]
        public Parameter<double> BSIM4lnoff { get; } = new Parameter<double>();
        [SpiceName("lvoffcv"), SpiceInfo("Length dependence of voffcv")]
        public Parameter<double> BSIM4lvoffcv { get; } = new Parameter<double>();
        [SpiceName("wcdsc"), SpiceInfo("Width dependence of cdsc")]
        public Parameter<double> BSIM4wcdsc { get; } = new Parameter<double>();
        [SpiceName("wcdscb"), SpiceInfo("Width dependence of cdscb")]
        public Parameter<double> BSIM4wcdscb { get; } = new Parameter<double>();
        [SpiceName("wcdscd"), SpiceInfo("Width dependence of cdscd")]
        public Parameter<double> BSIM4wcdscd { get; } = new Parameter<double>();
        [SpiceName("wcit"), SpiceInfo("Width dependence of cit")]
        public Parameter<double> BSIM4wcit { get; } = new Parameter<double>();
        [SpiceName("wnfactor"), SpiceInfo("Width dependence of nfactor")]
        public Parameter<double> BSIM4wnfactor { get; } = new Parameter<double>();
        [SpiceName("wxj"), SpiceInfo("Width dependence of xj")]
        public Parameter<double> BSIM4wxj { get; } = new Parameter<double>();
        [SpiceName("wvsat"), SpiceInfo("Width dependence of vsat")]
        public Parameter<double> BSIM4wvsat { get; } = new Parameter<double>();
        [SpiceName("wa0"), SpiceInfo("Width dependence of a0")]
        public Parameter<double> BSIM4wa0 { get; } = new Parameter<double>();
        [SpiceName("wags"), SpiceInfo("Width dependence of ags")]
        public Parameter<double> BSIM4wags { get; } = new Parameter<double>();
        [SpiceName("wa1"), SpiceInfo("Width dependence of a1")]
        public Parameter<double> BSIM4wa1 { get; } = new Parameter<double>();
        [SpiceName("wa2"), SpiceInfo("Width dependence of a2")]
        public Parameter<double> BSIM4wa2 { get; } = new Parameter<double>();
        [SpiceName("wat"), SpiceInfo("Width dependence of at")]
        public Parameter<double> BSIM4wat { get; } = new Parameter<double>();
        [SpiceName("wketa"), SpiceInfo("Width dependence of keta")]
        public Parameter<double> BSIM4wketa { get; } = new Parameter<double>();
        [SpiceName("wnsub"), SpiceInfo("Width dependence of nsub")]
        public Parameter<double> BSIM4wnsub { get; } = new Parameter<double>();
        [SpiceName("wndep"), SpiceInfo("Width dependence of ndep")]
        public ParameterMethod<double> BSIM4wndep { get; } = new ParameterMethod<double>(0.0, (double v) => v > 1.0e20 ? v * 1e-6 : v, null);
        [SpiceName("wnsd"), SpiceInfo("Width dependence of nsd")]
        public ParameterMethod<double> BSIM4wnsd { get; } = new ParameterMethod<double>(0.0, (double v) => v > 1.0e23 ? v * 1e-6 : v, null);
        [SpiceName("wngate"), SpiceInfo("Width dependence of ngate")]
        public ParameterMethod<double> BSIM4wngate { get; } = new ParameterMethod<double>(0.0, (double v) => v > 1.0e23 ? v * 1e-6 : v, null);
        [SpiceName("wgamma1"), SpiceInfo("Width dependence of gamma1")]
        public Parameter<double> BSIM4wgamma1 { get; } = new Parameter<double>();
        [SpiceName("wgamma2"), SpiceInfo("Width dependence of gamma2")]
        public Parameter<double> BSIM4wgamma2 { get; } = new Parameter<double>();
        [SpiceName("wvbx"), SpiceInfo("Width dependence of vbx")]
        public Parameter<double> BSIM4wvbx { get; } = new Parameter<double>();
        [SpiceName("wvbm"), SpiceInfo("Width dependence of vbm")]
        public Parameter<double> BSIM4wvbm { get; } = new Parameter<double>();
        [SpiceName("wxt"), SpiceInfo("Width dependence of xt")]
        public Parameter<double> BSIM4wxt { get; } = new Parameter<double>();
        [SpiceName("wk1"), SpiceInfo("Width dependence of k1")]
        public Parameter<double> BSIM4wk1 { get; } = new Parameter<double>();
        [SpiceName("wkt1"), SpiceInfo("Width dependence of kt1")]
        public Parameter<double> BSIM4wkt1 { get; } = new Parameter<double>();
        [SpiceName("wkt1l"), SpiceInfo("Width dependence of kt1l")]
        public Parameter<double> BSIM4wkt1l { get; } = new Parameter<double>();
        [SpiceName("wkt2"), SpiceInfo("Width dependence of kt2")]
        public Parameter<double> BSIM4wkt2 { get; } = new Parameter<double>();
        [SpiceName("wk2"), SpiceInfo("Width dependence of k2")]
        public Parameter<double> BSIM4wk2 { get; } = new Parameter<double>();
        [SpiceName("wk3"), SpiceInfo("Width dependence of k3")]
        public Parameter<double> BSIM4wk3 { get; } = new Parameter<double>();
        [SpiceName("wk3b"), SpiceInfo("Width dependence of k3b")]
        public Parameter<double> BSIM4wk3b { get; } = new Parameter<double>();
        [SpiceName("wlpe0"), SpiceInfo("Width dependence of lpe0")]
        public Parameter<double> BSIM4wlpe0 { get; } = new Parameter<double>();
        [SpiceName("wlpeb"), SpiceInfo("Width dependence of lpeb")]
        public Parameter<double> BSIM4wlpeb { get; } = new Parameter<double>();
        [SpiceName("wdvtp0"), SpiceInfo("Width dependence of dvtp0")]
        public Parameter<double> BSIM4wdvtp0 { get; } = new Parameter<double>();
        [SpiceName("wdvtp1"), SpiceInfo("Width dependence of dvtp1")]
        public Parameter<double> BSIM4wdvtp1 { get; } = new Parameter<double>();
        [SpiceName("wdvtp2"), SpiceInfo("Width dependence of dvtp2")]
        public Parameter<double> BSIM4wdvtp2 { get; } = new Parameter<double>();
        [SpiceName("wdvtp3"), SpiceInfo("Width dependence of dvtp3")]
        public Parameter<double> BSIM4wdvtp3 { get; } = new Parameter<double>();
        [SpiceName("wdvtp4"), SpiceInfo("Width dependence of dvtp4")]
        public Parameter<double> BSIM4wdvtp4 { get; } = new Parameter<double>();
        [SpiceName("wdvtp5"), SpiceInfo("Width dependence of dvtp5")]
        public Parameter<double> BSIM4wdvtp5 { get; } = new Parameter<double>();
        [SpiceName("ww0"), SpiceInfo("Width dependence of w0")]
        public Parameter<double> BSIM4ww0 { get; } = new Parameter<double>();
        [SpiceName("wdvt0"), SpiceInfo("Width dependence of dvt0")]
        public Parameter<double> BSIM4wdvt0 { get; } = new Parameter<double>();
        [SpiceName("wdvt1"), SpiceInfo("Width dependence of dvt1")]
        public Parameter<double> BSIM4wdvt1 { get; } = new Parameter<double>();
        [SpiceName("wdvt2"), SpiceInfo("Width dependence of dvt2")]
        public Parameter<double> BSIM4wdvt2 { get; } = new Parameter<double>();
        [SpiceName("wdvt0w"), SpiceInfo("Width dependence of dvt0w")]
        public Parameter<double> BSIM4wdvt0w { get; } = new Parameter<double>();
        [SpiceName("wdvt1w"), SpiceInfo("Width dependence of dvt1w")]
        public Parameter<double> BSIM4wdvt1w { get; } = new Parameter<double>();
        [SpiceName("wdvt2w"), SpiceInfo("Width dependence of dvt2w")]
        public Parameter<double> BSIM4wdvt2w { get; } = new Parameter<double>();
        [SpiceName("wdrout"), SpiceInfo("Width dependence of drout")]
        public Parameter<double> BSIM4wdrout { get; } = new Parameter<double>();
        [SpiceName("wdsub"), SpiceInfo("Width dependence of dsub")]
        public Parameter<double> BSIM4wdsub { get; } = new Parameter<double>();
        [SpiceName("wvth0"), SpiceName("wvtho"), SpiceInfo("Width dependence of vto")]
        public Parameter<double> BSIM4wvth0 { get; } = new Parameter<double>();
        [SpiceName("wua"), SpiceInfo("Width dependence of ua")]
        public Parameter<double> BSIM4wua { get; } = new Parameter<double>();
        [SpiceName("wua1"), SpiceInfo("Width dependence of ua1")]
        public Parameter<double> BSIM4wua1 { get; } = new Parameter<double>();
        [SpiceName("wub"), SpiceInfo("Width dependence of ub")]
        public Parameter<double> BSIM4wub { get; } = new Parameter<double>();
        [SpiceName("wub1"), SpiceInfo("Width dependence of ub1")]
        public Parameter<double> BSIM4wub1 { get; } = new Parameter<double>();
        [SpiceName("wuc"), SpiceInfo("Width dependence of uc")]
        public Parameter<double> BSIM4wuc { get; } = new Parameter<double>();
        [SpiceName("wuc1"), SpiceInfo("Width dependence of uc1")]
        public Parameter<double> BSIM4wuc1 { get; } = new Parameter<double>();
        [SpiceName("wu0"), SpiceInfo("Width dependence of u0")]
        public Parameter<double> BSIM4wu0 { get; } = new Parameter<double>();
        [SpiceName("wute"), SpiceInfo("Width dependence of ute")]
        public Parameter<double> BSIM4wute { get; } = new Parameter<double>();
        [SpiceName("wucste"), SpiceInfo("Width dependence of ucste")]
        public Parameter<double> BSIM4wucste { get; } = new Parameter<double>();
        [SpiceName("wvoff"), SpiceInfo("Width dependence of voff")]
        public Parameter<double> BSIM4wvoff { get; } = new Parameter<double>();
        [SpiceName("wtvoff"), SpiceInfo("Width dependence of tvoff")]
        public Parameter<double> BSIM4wtvoff { get; } = new Parameter<double>();
        [SpiceName("wtnfactor"), SpiceInfo("Width dependence of tnfactor")]
        public Parameter<double> BSIM4wtnfactor { get; } = new Parameter<double>();
        [SpiceName("wteta0"), SpiceInfo("Width dependence of teta0")]
        public Parameter<double> BSIM4wteta0 { get; } = new Parameter<double>();
        [SpiceName("wtvoffcv"), SpiceInfo("Width dependence of tvoffcv")]
        public Parameter<double> BSIM4wtvoffcv { get; } = new Parameter<double>();
        [SpiceName("wminv"), SpiceInfo("Width dependence of minv")]
        public Parameter<double> BSIM4wminv { get; } = new Parameter<double>();
        [SpiceName("wminvcv"), SpiceInfo("Width dependence of minvcv")]
        public Parameter<double> BSIM4wminvcv { get; } = new Parameter<double>();
        [SpiceName("wfprout"), SpiceInfo("Width dependence of pdiblcb")]
        public Parameter<double> BSIM4wfprout { get; } = new Parameter<double>();
        [SpiceName("wpdits"), SpiceInfo("Width dependence of pdits")]
        public Parameter<double> BSIM4wpdits { get; } = new Parameter<double>();
        [SpiceName("wpditsd"), SpiceInfo("Width dependence of pditsd")]
        public Parameter<double> BSIM4wpditsd { get; } = new Parameter<double>();
        [SpiceName("wdelta"), SpiceInfo("Width dependence of delta")]
        public Parameter<double> BSIM4wdelta { get; } = new Parameter<double>();
        [SpiceName("wrdsw"), SpiceInfo("Width dependence of rdsw ")]
        public Parameter<double> BSIM4wrdsw { get; } = new Parameter<double>();
        [SpiceName("wrdw"), SpiceInfo("Width dependence of rdw")]
        public Parameter<double> BSIM4wrdw { get; } = new Parameter<double>();
        [SpiceName("wrsw"), SpiceInfo("Width dependence of rsw")]
        public Parameter<double> BSIM4wrsw { get; } = new Parameter<double>();
        [SpiceName("wprwb"), SpiceInfo("Width dependence of prwb ")]
        public Parameter<double> BSIM4wprwb { get; } = new Parameter<double>();
        [SpiceName("wprwg"), SpiceInfo("Width dependence of prwg ")]
        public Parameter<double> BSIM4wprwg { get; } = new Parameter<double>();
        [SpiceName("wprt"), SpiceInfo("Width dependence of prt")]
        public Parameter<double> BSIM4wprt { get; } = new Parameter<double>();
        [SpiceName("weta0"), SpiceInfo("Width dependence of eta0")]
        public Parameter<double> BSIM4weta0 { get; } = new Parameter<double>();
        [SpiceName("wetab"), SpiceInfo("Width dependence of etab")]
        public Parameter<double> BSIM4wetab { get; } = new Parameter<double>();
        [SpiceName("wpclm"), SpiceInfo("Width dependence of pclm")]
        public Parameter<double> BSIM4wpclm { get; } = new Parameter<double>();
        [SpiceName("wpdiblc1"), SpiceInfo("Width dependence of pdiblc1")]
        public Parameter<double> BSIM4wpdibl1 { get; } = new Parameter<double>();
        [SpiceName("wpdiblc2"), SpiceInfo("Width dependence of pdiblc2")]
        public Parameter<double> BSIM4wpdibl2 { get; } = new Parameter<double>();
        [SpiceName("wpdiblcb"), SpiceInfo("Width dependence of pdiblcb")]
        public Parameter<double> BSIM4wpdiblb { get; } = new Parameter<double>();
        [SpiceName("wpscbe1"), SpiceInfo("Width dependence of pscbe1")]
        public Parameter<double> BSIM4wpscbe1 { get; } = new Parameter<double>();
        [SpiceName("wpscbe2"), SpiceInfo("Width dependence of pscbe2")]
        public Parameter<double> BSIM4wpscbe2 { get; } = new Parameter<double>();
        [SpiceName("wpvag"), SpiceInfo("Width dependence of pvag")]
        public Parameter<double> BSIM4wpvag { get; } = new Parameter<double>();
        [SpiceName("wwr"), SpiceInfo("Width dependence of wr")]
        public Parameter<double> BSIM4wwr { get; } = new Parameter<double>();
        [SpiceName("wdwg"), SpiceInfo("Width dependence of dwg")]
        public Parameter<double> BSIM4wdwg { get; } = new Parameter<double>();
        [SpiceName("wdwb"), SpiceInfo("Width dependence of dwb")]
        public Parameter<double> BSIM4wdwb { get; } = new Parameter<double>();
        [SpiceName("wb0"), SpiceInfo("Width dependence of b0")]
        public Parameter<double> BSIM4wb0 { get; } = new Parameter<double>();
        [SpiceName("wb1"), SpiceInfo("Width dependence of b1")]
        public Parameter<double> BSIM4wb1 { get; } = new Parameter<double>();
        [SpiceName("walpha0"), SpiceInfo("Width dependence of alpha0")]
        public Parameter<double> BSIM4walpha0 { get; } = new Parameter<double>();
        [SpiceName("walpha1"), SpiceInfo("Width dependence of alpha1")]
        public Parameter<double> BSIM4walpha1 { get; } = new Parameter<double>();
        [SpiceName("wbeta0"), SpiceInfo("Width dependence of beta0")]
        public Parameter<double> BSIM4wbeta0 { get; } = new Parameter<double>();
        [SpiceName("wphin"), SpiceInfo("Width dependence of phin")]
        public Parameter<double> BSIM4wphin { get; } = new Parameter<double>();
        [SpiceName("wagidl"), SpiceInfo("Width dependence of agidl")]
        public Parameter<double> BSIM4wagidl { get; } = new Parameter<double>();
        [SpiceName("wbgidl"), SpiceInfo("Width dependence of bgidl")]
        public Parameter<double> BSIM4wbgidl { get; } = new Parameter<double>();
        [SpiceName("wcgidl"), SpiceInfo("Width dependence of cgidl")]
        public Parameter<double> BSIM4wcgidl { get; } = new Parameter<double>();
        [SpiceName("wegidl"), SpiceInfo("Width dependence of egidl")]
        public Parameter<double> BSIM4wegidl { get; } = new Parameter<double>();
        [SpiceName("wfgidl"), SpiceInfo("Width dependence of fgidl")]
        public Parameter<double> BSIM4wfgidl { get; } = new Parameter<double>();
        [SpiceName("wkgidl"), SpiceInfo("Width dependence of kgidl")]
        public Parameter<double> BSIM4wkgidl { get; } = new Parameter<double>();
        [SpiceName("wrgidl"), SpiceInfo("Width dependence of rgidl")]
        public Parameter<double> BSIM4wrgidl { get; } = new Parameter<double>();
        [SpiceName("wagisl"), SpiceInfo("Width dependence of agisl")]
        public Parameter<double> BSIM4wagisl { get; } = new Parameter<double>();
        [SpiceName("wbgisl"), SpiceInfo("Width dependence of bgisl")]
        public Parameter<double> BSIM4wbgisl { get; } = new Parameter<double>();
        [SpiceName("wcgisl"), SpiceInfo("Width dependence of cgisl")]
        public Parameter<double> BSIM4wcgisl { get; } = new Parameter<double>();
        [SpiceName("wegisl"), SpiceInfo("Width dependence of egisl")]
        public Parameter<double> BSIM4wegisl { get; } = new Parameter<double>();
        [SpiceName("wfgisl"), SpiceInfo("Width dependence of fgisl")]
        public Parameter<double> BSIM4wfgisl { get; } = new Parameter<double>();
        [SpiceName("wkgisl"), SpiceInfo("Width dependence of kgisl")]
        public Parameter<double> BSIM4wkgisl { get; } = new Parameter<double>();
        [SpiceName("wrgisl"), SpiceInfo("Width dependence of rgisl")]
        public Parameter<double> BSIM4wrgisl { get; } = new Parameter<double>();
        [SpiceName("waigc"), SpiceInfo("Width dependence of aigc")]
        public Parameter<double> BSIM4waigc { get; } = new Parameter<double>();
        [SpiceName("wbigc"), SpiceInfo("Width dependence of bigc")]
        public Parameter<double> BSIM4wbigc { get; } = new Parameter<double>();
        [SpiceName("wcigc"), SpiceInfo("Width dependence of cigc")]
        public Parameter<double> BSIM4wcigc { get; } = new Parameter<double>();
        [SpiceName("waigsd"), SpiceInfo("Width dependence of aigsd")]
        public Parameter<double> BSIM4waigsd { get; } = new Parameter<double>();
        [SpiceName("wbigsd"), SpiceInfo("Width dependence of bigsd")]
        public Parameter<double> BSIM4wbigsd { get; } = new Parameter<double>();
        [SpiceName("wcigsd"), SpiceInfo("Width dependence of cigsd")]
        public Parameter<double> BSIM4wcigsd { get; } = new Parameter<double>();
        [SpiceName("waigs"), SpiceInfo("Width dependence of aigs")]
        public Parameter<double> BSIM4waigs { get; } = new Parameter<double>();
        [SpiceName("wbigs"), SpiceInfo("Width dependence of bigs")]
        public Parameter<double> BSIM4wbigs { get; } = new Parameter<double>();
        [SpiceName("wcigs"), SpiceInfo("Width dependence of cigs")]
        public Parameter<double> BSIM4wcigs { get; } = new Parameter<double>();
        [SpiceName("waigd"), SpiceInfo("Width dependence of aigd")]
        public Parameter<double> BSIM4waigd { get; } = new Parameter<double>();
        [SpiceName("wbigd"), SpiceInfo("Width dependence of bigd")]
        public Parameter<double> BSIM4wbigd { get; } = new Parameter<double>();
        [SpiceName("wcigd"), SpiceInfo("Width dependence of cigd")]
        public Parameter<double> BSIM4wcigd { get; } = new Parameter<double>();
        [SpiceName("waigbacc"), SpiceInfo("Width dependence of aigbacc")]
        public Parameter<double> BSIM4waigbacc { get; } = new Parameter<double>();
        [SpiceName("wbigbacc"), SpiceInfo("Width dependence of bigbacc")]
        public Parameter<double> BSIM4wbigbacc { get; } = new Parameter<double>();
        [SpiceName("wcigbacc"), SpiceInfo("Width dependence of cigbacc")]
        public Parameter<double> BSIM4wcigbacc { get; } = new Parameter<double>();
        [SpiceName("waigbinv"), SpiceInfo("Width dependence of aigbinv")]
        public Parameter<double> BSIM4waigbinv { get; } = new Parameter<double>();
        [SpiceName("wbigbinv"), SpiceInfo("Width dependence of bigbinv")]
        public Parameter<double> BSIM4wbigbinv { get; } = new Parameter<double>();
        [SpiceName("wcigbinv"), SpiceInfo("Width dependence of cigbinv")]
        public Parameter<double> BSIM4wcigbinv { get; } = new Parameter<double>();
        [SpiceName("wnigc"), SpiceInfo("Width dependence of nigc")]
        public Parameter<double> BSIM4wnigc { get; } = new Parameter<double>();
        [SpiceName("wnigbinv"), SpiceInfo("Width dependence of nigbinv")]
        public Parameter<double> BSIM4wnigbinv { get; } = new Parameter<double>();
        [SpiceName("wnigbacc"), SpiceInfo("Width dependence of nigbacc")]
        public Parameter<double> BSIM4wnigbacc { get; } = new Parameter<double>();
        [SpiceName("wntox"), SpiceInfo("Width dependence of ntox")]
        public Parameter<double> BSIM4wntox { get; } = new Parameter<double>();
        [SpiceName("weigbinv"), SpiceInfo("Width dependence for eigbinv")]
        public Parameter<double> BSIM4weigbinv { get; } = new Parameter<double>();
        [SpiceName("wpigcd"), SpiceInfo("Width dependence for pigcd")]
        public Parameter<double> BSIM4wpigcd { get; } = new Parameter<double>();
        [SpiceName("wpoxedge"), SpiceInfo("Width dependence for poxedge")]
        public Parameter<double> BSIM4wpoxedge { get; } = new Parameter<double>();
        [SpiceName("wxrcrg1"), SpiceInfo("Width dependence of xrcrg1")]
        public Parameter<double> BSIM4wxrcrg1 { get; } = new Parameter<double>();
        [SpiceName("wxrcrg2"), SpiceInfo("Width dependence of xrcrg2")]
        public Parameter<double> BSIM4wxrcrg2 { get; } = new Parameter<double>();
        [SpiceName("wlambda"), SpiceInfo("Width dependence of lambda")]
        public Parameter<double> BSIM4wlambda { get; } = new Parameter<double>();
        [SpiceName("wvtl"), SpiceInfo("Width dependence of vtl")]
        public Parameter<double> BSIM4wvtl { get; } = new Parameter<double>();
        [SpiceName("wxn"), SpiceInfo("Width dependence of xn")]
        public Parameter<double> BSIM4wxn { get; } = new Parameter<double>();
        [SpiceName("wvfbsdoff"), SpiceInfo("Width dependence of vfbsdoff")]
        public Parameter<double> BSIM4wvfbsdoff { get; } = new Parameter<double>();
        [SpiceName("wtvfbsdoff"), SpiceInfo("Width dependence of tvfbsdoff")]
        public Parameter<double> BSIM4wtvfbsdoff { get; } = new Parameter<double>();
        [SpiceName("weu"), SpiceInfo("Width dependence of eu")]
        public Parameter<double> BSIM4weu { get; } = new Parameter<double>();
        [SpiceName("wucs"), SpiceInfo("Width dependence of ucs")]
        public Parameter<double> BSIM4wucs { get; } = new Parameter<double>();
        [SpiceName("wvfb"), SpiceInfo("Width dependence of vfb")]
        public Parameter<double> BSIM4wvfb { get; } = new Parameter<double>();
        [SpiceName("wcgsl"), SpiceInfo("Width dependence of cgsl")]
        public Parameter<double> BSIM4wcgsl { get; } = new Parameter<double>();
        [SpiceName("wcgdl"), SpiceInfo("Width dependence of cgdl")]
        public Parameter<double> BSIM4wcgdl { get; } = new Parameter<double>();
        [SpiceName("wckappas"), SpiceInfo("Width dependence of ckappas")]
        public Parameter<double> BSIM4wckappas { get; } = new Parameter<double>();
        [SpiceName("wckappad"), SpiceInfo("Width dependence of ckappad")]
        public Parameter<double> BSIM4wckappad { get; } = new Parameter<double>();
        [SpiceName("wcf"), SpiceInfo("Width dependence of cf")]
        public Parameter<double> BSIM4wcf { get; } = new Parameter<double>();
        [SpiceName("wclc"), SpiceInfo("Width dependence of clc")]
        public Parameter<double> BSIM4wclc { get; } = new Parameter<double>();
        [SpiceName("wcle"), SpiceInfo("Width dependence of cle")]
        public Parameter<double> BSIM4wcle { get; } = new Parameter<double>();
        [SpiceName("wvfbcv"), SpiceInfo("Width dependence of vfbcv")]
        public Parameter<double> BSIM4wvfbcv { get; } = new Parameter<double>();
        [SpiceName("wacde"), SpiceInfo("Width dependence of acde")]
        public Parameter<double> BSIM4wacde { get; } = new Parameter<double>();
        [SpiceName("wmoin"), SpiceInfo("Width dependence of moin")]
        public Parameter<double> BSIM4wmoin { get; } = new Parameter<double>();
        [SpiceName("wnoff"), SpiceInfo("Width dependence of noff")]
        public Parameter<double> BSIM4wnoff { get; } = new Parameter<double>();
        [SpiceName("wvoffcv"), SpiceInfo("Width dependence of voffcv")]
        public Parameter<double> BSIM4wvoffcv { get; } = new Parameter<double>();
        [SpiceName("pcdsc"), SpiceInfo("Cross-term dependence of cdsc")]
        public Parameter<double> BSIM4pcdsc { get; } = new Parameter<double>();
        [SpiceName("pcdscb"), SpiceInfo("Cross-term dependence of cdscb")]
        public Parameter<double> BSIM4pcdscb { get; } = new Parameter<double>();
        [SpiceName("pcdscd"), SpiceInfo("Cross-term dependence of cdscd")]
        public Parameter<double> BSIM4pcdscd { get; } = new Parameter<double>();
        [SpiceName("pcit"), SpiceInfo("Cross-term dependence of cit")]
        public Parameter<double> BSIM4pcit { get; } = new Parameter<double>();
        [SpiceName("pnfactor"), SpiceInfo("Cross-term dependence of nfactor")]
        public Parameter<double> BSIM4pnfactor { get; } = new Parameter<double>();
        [SpiceName("pxj"), SpiceInfo("Cross-term dependence of xj")]
        public Parameter<double> BSIM4pxj { get; } = new Parameter<double>();
        [SpiceName("pvsat"), SpiceInfo("Cross-term dependence of vsat")]
        public Parameter<double> BSIM4pvsat { get; } = new Parameter<double>();
        [SpiceName("pa0"), SpiceInfo("Cross-term dependence of a0")]
        public Parameter<double> BSIM4pa0 { get; } = new Parameter<double>();
        [SpiceName("pags"), SpiceInfo("Cross-term dependence of ags")]
        public Parameter<double> BSIM4pags { get; } = new Parameter<double>();
        [SpiceName("pa1"), SpiceInfo("Cross-term dependence of a1")]
        public Parameter<double> BSIM4pa1 { get; } = new Parameter<double>();
        [SpiceName("pa2"), SpiceInfo("Cross-term dependence of a2")]
        public Parameter<double> BSIM4pa2 { get; } = new Parameter<double>();
        [SpiceName("pat"), SpiceInfo("Cross-term dependence of at")]
        public Parameter<double> BSIM4pat { get; } = new Parameter<double>();
        [SpiceName("pketa"), SpiceInfo("Cross-term dependence of keta")]
        public Parameter<double> BSIM4pketa { get; } = new Parameter<double>();
        [SpiceName("pnsub"), SpiceInfo("Cross-term dependence of nsub")]
        public Parameter<double> BSIM4pnsub { get; } = new Parameter<double>();
        [SpiceName("pndep"), SpiceInfo("Cross-term dependence of ndep")]
        public ParameterMethod<double> BSIM4pndep { get; } = new ParameterMethod<double>(0.0, (double v) => v > 1.0e20 ? v * 1e-6 : v, null);
        [SpiceName("pnsd"), SpiceInfo("Cross-term dependence of nsd")]
        public ParameterMethod<double> BSIM4pnsd { get; } = new ParameterMethod<double>(0.0, (double v) => v > 1.0e23 ? v * 1e-6 : v, null);
        [SpiceName("pngate"), SpiceInfo("Cross-term dependence of ngate")]
        public ParameterMethod<double> BSIM4pngate { get; } = new ParameterMethod<double>(0.0, (double v) => v > 1.0e23 ? v * 1e-6 : v, null);
        [SpiceName("pgamma1"), SpiceInfo("Cross-term dependence of gamma1")]
        public Parameter<double> BSIM4pgamma1 { get; } = new Parameter<double>();
        [SpiceName("pgamma2"), SpiceInfo("Cross-term dependence of gamma2")]
        public Parameter<double> BSIM4pgamma2 { get; } = new Parameter<double>();
        [SpiceName("pvbx"), SpiceInfo("Cross-term dependence of vbx")]
        public Parameter<double> BSIM4pvbx { get; } = new Parameter<double>();
        [SpiceName("pvbm"), SpiceInfo("Cross-term dependence of vbm")]
        public Parameter<double> BSIM4pvbm { get; } = new Parameter<double>();
        [SpiceName("pxt"), SpiceInfo("Cross-term dependence of xt")]
        public Parameter<double> BSIM4pxt { get; } = new Parameter<double>();
        [SpiceName("pk1"), SpiceInfo("Cross-term dependence of k1")]
        public Parameter<double> BSIM4pk1 { get; } = new Parameter<double>();
        [SpiceName("pkt1"), SpiceInfo("Cross-term dependence of kt1")]
        public Parameter<double> BSIM4pkt1 { get; } = new Parameter<double>();
        [SpiceName("pkt1l"), SpiceInfo("Cross-term dependence of kt1l")]
        public Parameter<double> BSIM4pkt1l { get; } = new Parameter<double>();
        [SpiceName("pkt2"), SpiceInfo("Cross-term dependence of kt2")]
        public Parameter<double> BSIM4pkt2 { get; } = new Parameter<double>();
        [SpiceName("pk2"), SpiceInfo("Cross-term dependence of k2")]
        public Parameter<double> BSIM4pk2 { get; } = new Parameter<double>();
        [SpiceName("pk3"), SpiceInfo("Cross-term dependence of k3")]
        public Parameter<double> BSIM4pk3 { get; } = new Parameter<double>();
        [SpiceName("pk3b"), SpiceInfo("Cross-term dependence of k3b")]
        public Parameter<double> BSIM4pk3b { get; } = new Parameter<double>();
        [SpiceName("plpe0"), SpiceInfo("Cross-term dependence of lpe0")]
        public Parameter<double> BSIM4plpe0 { get; } = new Parameter<double>();
        [SpiceName("plpeb"), SpiceInfo("Cross-term dependence of lpeb")]
        public Parameter<double> BSIM4plpeb { get; } = new Parameter<double>();
        [SpiceName("pdvtp0"), SpiceInfo("Cross-term dependence of dvtp0")]
        public Parameter<double> BSIM4pdvtp0 { get; } = new Parameter<double>();
        [SpiceName("pdvtp1"), SpiceInfo("Cross-term dependence of dvtp1")]
        public Parameter<double> BSIM4pdvtp1 { get; } = new Parameter<double>();
        [SpiceName("pdvtp2"), SpiceInfo("Cross-term dependence of dvtp2")]
        public Parameter<double> BSIM4pdvtp2 { get; } = new Parameter<double>();
        [SpiceName("pdvtp3"), SpiceInfo("Cross-term dependence of dvtp3")]
        public Parameter<double> BSIM4pdvtp3 { get; } = new Parameter<double>();
        [SpiceName("pdvtp4"), SpiceInfo("Cross-term dependence of dvtp4")]
        public Parameter<double> BSIM4pdvtp4 { get; } = new Parameter<double>();
        [SpiceName("pdvtp5"), SpiceInfo("Cross-term dependence of dvtp5")]
        public Parameter<double> BSIM4pdvtp5 { get; } = new Parameter<double>();
        [SpiceName("pw0"), SpiceInfo("Cross-term dependence of w0")]
        public Parameter<double> BSIM4pw0 { get; } = new Parameter<double>();
        [SpiceName("pdvt0"), SpiceInfo("Cross-term dependence of dvt0")]
        public Parameter<double> BSIM4pdvt0 { get; } = new Parameter<double>();
        [SpiceName("pdvt1"), SpiceInfo("Cross-term dependence of dvt1")]
        public Parameter<double> BSIM4pdvt1 { get; } = new Parameter<double>();
        [SpiceName("pdvt2"), SpiceInfo("Cross-term dependence of dvt2")]
        public Parameter<double> BSIM4pdvt2 { get; } = new Parameter<double>();
        [SpiceName("pdvt0w"), SpiceInfo("Cross-term dependence of dvt0w")]
        public Parameter<double> BSIM4pdvt0w { get; } = new Parameter<double>();
        [SpiceName("pdvt1w"), SpiceInfo("Cross-term dependence of dvt1w")]
        public Parameter<double> BSIM4pdvt1w { get; } = new Parameter<double>();
        [SpiceName("pdvt2w"), SpiceInfo("Cross-term dependence of dvt2w")]
        public Parameter<double> BSIM4pdvt2w { get; } = new Parameter<double>();
        [SpiceName("pdrout"), SpiceInfo("Cross-term dependence of drout")]
        public Parameter<double> BSIM4pdrout { get; } = new Parameter<double>();
        [SpiceName("pdsub"), SpiceInfo("Cross-term dependence of dsub")]
        public Parameter<double> BSIM4pdsub { get; } = new Parameter<double>();
        [SpiceName("pvth0"), SpiceName("pvtho"), SpiceInfo("Cross-term dependence of vto")]
        public Parameter<double> BSIM4pvth0 { get; } = new Parameter<double>();
        [SpiceName("pua"), SpiceInfo("Cross-term dependence of ua")]
        public Parameter<double> BSIM4pua { get; } = new Parameter<double>();
        [SpiceName("pua1"), SpiceInfo("Cross-term dependence of ua1")]
        public Parameter<double> BSIM4pua1 { get; } = new Parameter<double>();
        [SpiceName("pub"), SpiceInfo("Cross-term dependence of ub")]
        public Parameter<double> BSIM4pub { get; } = new Parameter<double>();
        [SpiceName("pub1"), SpiceInfo("Cross-term dependence of ub1")]
        public Parameter<double> BSIM4pub1 { get; } = new Parameter<double>();
        [SpiceName("puc"), SpiceInfo("Cross-term dependence of uc")]
        public Parameter<double> BSIM4puc { get; } = new Parameter<double>();
        [SpiceName("puc1"), SpiceInfo("Cross-term dependence of uc1")]
        public Parameter<double> BSIM4puc1 { get; } = new Parameter<double>();
        [SpiceName("pu0"), SpiceInfo("Cross-term dependence of u0")]
        public Parameter<double> BSIM4pu0 { get; } = new Parameter<double>();
        [SpiceName("pute"), SpiceInfo("Cross-term dependence of ute")]
        public Parameter<double> BSIM4pute { get; } = new Parameter<double>();
        [SpiceName("pucste"), SpiceInfo("Cross-term dependence of ucste")]
        public Parameter<double> BSIM4pucste { get; } = new Parameter<double>();
        [SpiceName("pvoff"), SpiceInfo("Cross-term dependence of voff")]
        public Parameter<double> BSIM4pvoff { get; } = new Parameter<double>();
        [SpiceName("ptvoff"), SpiceInfo("Cross-term dependence of tvoff")]
        public Parameter<double> BSIM4ptvoff { get; } = new Parameter<double>();
        [SpiceName("ptnfactor"), SpiceInfo("Cross-term dependence of tnfactor")]
        public Parameter<double> BSIM4ptnfactor { get; } = new Parameter<double>();
        [SpiceName("pteta0"), SpiceInfo("Cross-term dependence of teta0")]
        public Parameter<double> BSIM4pteta0 { get; } = new Parameter<double>();
        [SpiceName("ptvoffcv"), SpiceInfo("Cross-term dependence of tvoffcv")]
        public Parameter<double> BSIM4ptvoffcv { get; } = new Parameter<double>();
        [SpiceName("pminv"), SpiceInfo("Cross-term dependence of minv")]
        public Parameter<double> BSIM4pminv { get; } = new Parameter<double>();
        [SpiceName("pminvcv"), SpiceInfo("Cross-term dependence of minvcv")]
        public Parameter<double> BSIM4pminvcv { get; } = new Parameter<double>();
        [SpiceName("pfprout"), SpiceInfo("Cross-term dependence of pdiblcb")]
        public Parameter<double> BSIM4pfprout { get; } = new Parameter<double>();
        [SpiceName("ppdits"), SpiceInfo("Cross-term dependence of pdits")]
        public Parameter<double> BSIM4ppdits { get; } = new Parameter<double>();
        [SpiceName("ppditsd"), SpiceInfo("Cross-term dependence of pditsd")]
        public Parameter<double> BSIM4ppditsd { get; } = new Parameter<double>();
        [SpiceName("pdelta"), SpiceInfo("Cross-term dependence of delta")]
        public Parameter<double> BSIM4pdelta { get; } = new Parameter<double>();
        [SpiceName("prdsw"), SpiceInfo("Cross-term dependence of rdsw ")]
        public Parameter<double> BSIM4prdsw { get; } = new Parameter<double>();
        [SpiceName("prdw"), SpiceInfo("Cross-term dependence of rdw")]
        public Parameter<double> BSIM4prdw { get; } = new Parameter<double>();
        [SpiceName("prsw"), SpiceInfo("Cross-term dependence of rsw")]
        public Parameter<double> BSIM4prsw { get; } = new Parameter<double>();
        [SpiceName("pprwb"), SpiceInfo("Cross-term dependence of prwb ")]
        public Parameter<double> BSIM4pprwb { get; } = new Parameter<double>();
        [SpiceName("pprwg"), SpiceInfo("Cross-term dependence of prwg ")]
        public Parameter<double> BSIM4pprwg { get; } = new Parameter<double>();
        [SpiceName("pprt"), SpiceInfo("Cross-term dependence of prt ")]
        public Parameter<double> BSIM4pprt { get; } = new Parameter<double>();
        [SpiceName("peta0"), SpiceInfo("Cross-term dependence of eta0")]
        public Parameter<double> BSIM4peta0 { get; } = new Parameter<double>();
        [SpiceName("petab"), SpiceInfo("Cross-term dependence of etab")]
        public Parameter<double> BSIM4petab { get; } = new Parameter<double>();
        [SpiceName("ppclm"), SpiceInfo("Cross-term dependence of pclm")]
        public Parameter<double> BSIM4ppclm { get; } = new Parameter<double>();
        [SpiceName("ppdiblc1"), SpiceInfo("Cross-term dependence of pdiblc1")]
        public Parameter<double> BSIM4ppdibl1 { get; } = new Parameter<double>();
        [SpiceName("ppdiblc2"), SpiceInfo("Cross-term dependence of pdiblc2")]
        public Parameter<double> BSIM4ppdibl2 { get; } = new Parameter<double>();
        [SpiceName("ppdiblcb"), SpiceInfo("Cross-term dependence of pdiblcb")]
        public Parameter<double> BSIM4ppdiblb { get; } = new Parameter<double>();
        [SpiceName("ppscbe1"), SpiceInfo("Cross-term dependence of pscbe1")]
        public Parameter<double> BSIM4ppscbe1 { get; } = new Parameter<double>();
        [SpiceName("ppscbe2"), SpiceInfo("Cross-term dependence of pscbe2")]
        public Parameter<double> BSIM4ppscbe2 { get; } = new Parameter<double>();
        [SpiceName("ppvag"), SpiceInfo("Cross-term dependence of pvag")]
        public Parameter<double> BSIM4ppvag { get; } = new Parameter<double>();
        [SpiceName("pwr"), SpiceInfo("Cross-term dependence of wr")]
        public Parameter<double> BSIM4pwr { get; } = new Parameter<double>();
        [SpiceName("pdwg"), SpiceInfo("Cross-term dependence of dwg")]
        public Parameter<double> BSIM4pdwg { get; } = new Parameter<double>();
        [SpiceName("pdwb"), SpiceInfo("Cross-term dependence of dwb")]
        public Parameter<double> BSIM4pdwb { get; } = new Parameter<double>();
        [SpiceName("pb0"), SpiceInfo("Cross-term dependence of b0")]
        public Parameter<double> BSIM4pb0 { get; } = new Parameter<double>();
        [SpiceName("pb1"), SpiceInfo("Cross-term dependence of b1")]
        public Parameter<double> BSIM4pb1 { get; } = new Parameter<double>();
        [SpiceName("palpha0"), SpiceInfo("Cross-term dependence of alpha0")]
        public Parameter<double> BSIM4palpha0 { get; } = new Parameter<double>();
        [SpiceName("palpha1"), SpiceInfo("Cross-term dependence of alpha1")]
        public Parameter<double> BSIM4palpha1 { get; } = new Parameter<double>();
        [SpiceName("pbeta0"), SpiceInfo("Cross-term dependence of beta0")]
        public Parameter<double> BSIM4pbeta0 { get; } = new Parameter<double>();
        [SpiceName("pphin"), SpiceInfo("Cross-term dependence of phin")]
        public Parameter<double> BSIM4pphin { get; } = new Parameter<double>();
        [SpiceName("pagidl"), SpiceInfo("Cross-term dependence of agidl")]
        public Parameter<double> BSIM4pagidl { get; } = new Parameter<double>();
        [SpiceName("pbgidl"), SpiceInfo("Cross-term dependence of bgidl")]
        public Parameter<double> BSIM4pbgidl { get; } = new Parameter<double>();
        [SpiceName("pcgidl"), SpiceInfo("Cross-term dependence of cgidl")]
        public Parameter<double> BSIM4pcgidl { get; } = new Parameter<double>();
        [SpiceName("pegidl"), SpiceInfo("Cross-term dependence of egidl")]
        public Parameter<double> BSIM4pegidl { get; } = new Parameter<double>();
        [SpiceName("pfgidl"), SpiceInfo("Cross-term dependence of fgidl")]
        public Parameter<double> BSIM4pfgidl { get; } = new Parameter<double>();
        [SpiceName("pkgidl"), SpiceInfo("Cross-term dependence of kgidl")]
        public Parameter<double> BSIM4pkgidl { get; } = new Parameter<double>();
        [SpiceName("prgidl"), SpiceInfo("Cross-term dependence of rgidl")]
        public Parameter<double> BSIM4prgidl { get; } = new Parameter<double>();
        [SpiceName("pagisl"), SpiceInfo("Cross-term dependence of agisl")]
        public Parameter<double> BSIM4pagisl { get; } = new Parameter<double>();
        [SpiceName("pbgisl"), SpiceInfo("Cross-term dependence of bgisl")]
        public Parameter<double> BSIM4pbgisl { get; } = new Parameter<double>();
        [SpiceName("pcgisl"), SpiceInfo("Cross-term dependence of cgisl")]
        public Parameter<double> BSIM4pcgisl { get; } = new Parameter<double>();
        [SpiceName("pegisl"), SpiceInfo("Cross-term dependence of egisl")]
        public Parameter<double> BSIM4pegisl { get; } = new Parameter<double>();
        [SpiceName("pfgisl"), SpiceInfo("Cross-term dependence of fgisl")]
        public Parameter<double> BSIM4pfgisl { get; } = new Parameter<double>();
        [SpiceName("pkgisl"), SpiceInfo("Cross-term dependence of kgisl")]
        public Parameter<double> BSIM4pkgisl { get; } = new Parameter<double>();
        [SpiceName("prgisl"), SpiceInfo("Cross-term dependence of rgisl")]
        public Parameter<double> BSIM4prgisl { get; } = new Parameter<double>();
        [SpiceName("paigc"), SpiceInfo("Cross-term dependence of aigc")]
        public Parameter<double> BSIM4paigc { get; } = new Parameter<double>();
        [SpiceName("pbigc"), SpiceInfo("Cross-term dependence of bigc")]
        public Parameter<double> BSIM4pbigc { get; } = new Parameter<double>();
        [SpiceName("pcigc"), SpiceInfo("Cross-term dependence of cigc")]
        public Parameter<double> BSIM4pcigc { get; } = new Parameter<double>();
        [SpiceName("paigsd"), SpiceInfo("Cross-term dependence of aigsd")]
        public Parameter<double> BSIM4paigsd { get; } = new Parameter<double>();
        [SpiceName("pbigsd"), SpiceInfo("Cross-term dependence of bigsd")]
        public Parameter<double> BSIM4pbigsd { get; } = new Parameter<double>();
        [SpiceName("pcigsd"), SpiceInfo("Cross-term dependence of cigsd")]
        public Parameter<double> BSIM4pcigsd { get; } = new Parameter<double>();
        [SpiceName("paigs"), SpiceInfo("Cross-term dependence of aigs")]
        public Parameter<double> BSIM4paigs { get; } = new Parameter<double>();
        [SpiceName("pbigs"), SpiceInfo("Cross-term dependence of bigs")]
        public Parameter<double> BSIM4pbigs { get; } = new Parameter<double>();
        [SpiceName("pcigs"), SpiceInfo("Cross-term dependence of cigs")]
        public Parameter<double> BSIM4pcigs { get; } = new Parameter<double>();
        [SpiceName("paigd"), SpiceInfo("Cross-term dependence of aigd")]
        public Parameter<double> BSIM4paigd { get; } = new Parameter<double>();
        [SpiceName("pbigd"), SpiceInfo("Cross-term dependence of bigd")]
        public Parameter<double> BSIM4pbigd { get; } = new Parameter<double>();
        [SpiceName("pcigd"), SpiceInfo("Cross-term dependence of cigd")]
        public Parameter<double> BSIM4pcigd { get; } = new Parameter<double>();
        [SpiceName("paigbacc"), SpiceInfo("Cross-term dependence of aigbacc")]
        public Parameter<double> BSIM4paigbacc { get; } = new Parameter<double>();
        [SpiceName("pbigbacc"), SpiceInfo("Cross-term dependence of bigbacc")]
        public Parameter<double> BSIM4pbigbacc { get; } = new Parameter<double>();
        [SpiceName("pcigbacc"), SpiceInfo("Cross-term dependence of cigbacc")]
        public Parameter<double> BSIM4pcigbacc { get; } = new Parameter<double>();
        [SpiceName("paigbinv"), SpiceInfo("Cross-term dependence of aigbinv")]
        public Parameter<double> BSIM4paigbinv { get; } = new Parameter<double>();
        [SpiceName("pbigbinv"), SpiceInfo("Cross-term dependence of bigbinv")]
        public Parameter<double> BSIM4pbigbinv { get; } = new Parameter<double>();
        [SpiceName("pcigbinv"), SpiceInfo("Cross-term dependence of cigbinv")]
        public Parameter<double> BSIM4pcigbinv { get; } = new Parameter<double>();
        [SpiceName("pnigc"), SpiceInfo("Cross-term dependence of nigc")]
        public Parameter<double> BSIM4pnigc { get; } = new Parameter<double>();
        [SpiceName("pnigbinv"), SpiceInfo("Cross-term dependence of nigbinv")]
        public Parameter<double> BSIM4pnigbinv { get; } = new Parameter<double>();
        [SpiceName("pnigbacc"), SpiceInfo("Cross-term dependence of nigbacc")]
        public Parameter<double> BSIM4pnigbacc { get; } = new Parameter<double>();
        [SpiceName("pntox"), SpiceInfo("Cross-term dependence of ntox")]
        public Parameter<double> BSIM4pntox { get; } = new Parameter<double>();
        [SpiceName("peigbinv"), SpiceInfo("Cross-term dependence for eigbinv")]
        public Parameter<double> BSIM4peigbinv { get; } = new Parameter<double>();
        [SpiceName("ppigcd"), SpiceInfo("Cross-term dependence for pigcd")]
        public Parameter<double> BSIM4ppigcd { get; } = new Parameter<double>();
        [SpiceName("ppoxedge"), SpiceInfo("Cross-term dependence for poxedge")]
        public Parameter<double> BSIM4ppoxedge { get; } = new Parameter<double>();
        [SpiceName("pxrcrg1"), SpiceInfo("Cross-term dependence of xrcrg1")]
        public Parameter<double> BSIM4pxrcrg1 { get; } = new Parameter<double>();
        [SpiceName("pxrcrg2"), SpiceInfo("Cross-term dependence of xrcrg2")]
        public Parameter<double> BSIM4pxrcrg2 { get; } = new Parameter<double>();
        [SpiceName("plambda"), SpiceInfo("Cross-term dependence of lambda")]
        public Parameter<double> BSIM4plambda { get; } = new Parameter<double>();
        [SpiceName("pvtl"), SpiceInfo("Cross-term dependence of vtl")]
        public Parameter<double> BSIM4pvtl { get; } = new Parameter<double>();
        [SpiceName("pxn"), SpiceInfo("Cross-term dependence of xn")]
        public Parameter<double> BSIM4pxn { get; } = new Parameter<double>();
        [SpiceName("pvfbsdoff"), SpiceInfo("Cross-term dependence of vfbsdoff")]
        public Parameter<double> BSIM4pvfbsdoff { get; } = new Parameter<double>();
        [SpiceName("ptvfbsdoff"), SpiceInfo("Cross-term dependence of tvfbsdoff")]
        public Parameter<double> BSIM4ptvfbsdoff { get; } = new Parameter<double>();
        [SpiceName("peu"), SpiceInfo("Cross-term dependence of eu")]
        public Parameter<double> BSIM4peu { get; } = new Parameter<double>();
        [SpiceName("pucs"), SpiceInfo("Cross-term dependence of ucs")]
        public Parameter<double> BSIM4pucs { get; } = new Parameter<double>();
        [SpiceName("pvfb"), SpiceInfo("Cross-term dependence of vfb")]
        public Parameter<double> BSIM4pvfb { get; } = new Parameter<double>();
        [SpiceName("pcgsl"), SpiceInfo("Cross-term dependence of cgsl")]
        public Parameter<double> BSIM4pcgsl { get; } = new Parameter<double>();
        [SpiceName("pcgdl"), SpiceInfo("Cross-term dependence of cgdl")]
        public Parameter<double> BSIM4pcgdl { get; } = new Parameter<double>();
        [SpiceName("pckappas"), SpiceInfo("Cross-term dependence of ckappas")]
        public Parameter<double> BSIM4pckappas { get; } = new Parameter<double>();
        [SpiceName("pckappad"), SpiceInfo("Cross-term dependence of ckappad")]
        public Parameter<double> BSIM4pckappad { get; } = new Parameter<double>();
        [SpiceName("pcf"), SpiceInfo("Cross-term dependence of cf")]
        public Parameter<double> BSIM4pcf { get; } = new Parameter<double>();
        [SpiceName("pclc"), SpiceInfo("Cross-term dependence of clc")]
        public Parameter<double> BSIM4pclc { get; } = new Parameter<double>();
        [SpiceName("pcle"), SpiceInfo("Cross-term dependence of cle")]
        public Parameter<double> BSIM4pcle { get; } = new Parameter<double>();
        [SpiceName("pvfbcv"), SpiceInfo("Cross-term dependence of vfbcv")]
        public Parameter<double> BSIM4pvfbcv { get; } = new Parameter<double>();
        [SpiceName("pacde"), SpiceInfo("Cross-term dependence of acde")]
        public Parameter<double> BSIM4pacde { get; } = new Parameter<double>();
        [SpiceName("pmoin"), SpiceInfo("Cross-term dependence of moin")]
        public Parameter<double> BSIM4pmoin { get; } = new Parameter<double>();
        [SpiceName("pnoff"), SpiceInfo("Cross-term dependence of noff")]
        public Parameter<double> BSIM4pnoff { get; } = new Parameter<double>();
        [SpiceName("pvoffcv"), SpiceInfo("Cross-term dependence of voffcv")]
        public Parameter<double> BSIM4pvoffcv { get; } = new Parameter<double>();
        [SpiceName("tnom"), SpiceInfo("Parameter measurement temperature")]
        public ParameterMethod<double> BSIM4tnom { get; } = new ParameterMethod<double>(300.15, (double celsius) => celsius + Circuit.CONSTCtoK, (double kelvin) => kelvin - Circuit.CONSTCtoK);
        [SpiceName("cgso"), SpiceInfo("Gate-source overlap capacitance per width")]
        public Parameter<double> BSIM4cgso { get; } = new Parameter<double>();
        [SpiceName("cgdo"), SpiceInfo("Gate-drain overlap capacitance per width")]
        public Parameter<double> BSIM4cgdo { get; } = new Parameter<double>();
        [SpiceName("cgbo"), SpiceInfo("Gate-bulk overlap capacitance per length")]
        public Parameter<double> BSIM4cgbo { get; } = new Parameter<double>();
        [SpiceName("xpart"), SpiceInfo("Channel charge partitioning")]
        public Parameter<double> BSIM4xpart { get; } = new Parameter<double>();
        [SpiceName("rsh"), SpiceInfo("Source-drain sheet resistance")]
        public Parameter<double> BSIM4sheetResistance { get; } = new Parameter<double>();
        [SpiceName("jss"), SpiceInfo("Bottom source junction reverse saturation current density")]
        public Parameter<double> BSIM4SjctSatCurDensity { get; } = new Parameter<double>(1.0E-4);
        [SpiceName("jsws"), SpiceInfo("Isolation edge sidewall source junction reverse saturation current density")]
        public Parameter<double> BSIM4SjctSidewallSatCurDensity { get; } = new Parameter<double>();
        [SpiceName("jswgs"), SpiceInfo("Gate edge source junction reverse saturation current density")]
        public Parameter<double> BSIM4SjctGateSidewallSatCurDensity { get; } = new Parameter<double>();
        [SpiceName("pbs"), SpiceInfo("Source junction built-in potential")]
        public Parameter<double> BSIM4SbulkJctPotential { get; } = new Parameter<double>();
        [SpiceName("mjs"), SpiceInfo("Source bottom junction capacitance grading coefficient")]
        public Parameter<double> BSIM4SbulkJctBotGradingCoeff { get; } = new Parameter<double>();
        [SpiceName("pbsws"), SpiceInfo("Source sidewall junction capacitance built in potential")]
        public Parameter<double> BSIM4SsidewallJctPotential { get; } = new Parameter<double>();
        [SpiceName("mjsws"), SpiceInfo("Source sidewall junction capacitance grading coefficient")]
        public Parameter<double> BSIM4SbulkJctSideGradingCoeff { get; } = new Parameter<double>();
        [SpiceName("cjs"), SpiceInfo("Source bottom junction capacitance per unit area")]
        public Parameter<double> BSIM4SunitAreaJctCap { get; } = new Parameter<double>(5.0E-4);
        [SpiceName("cjsws"), SpiceInfo("Source sidewall junction capacitance per unit periphery")]
        public Parameter<double> BSIM4SunitLengthSidewallJctCap { get; } = new Parameter<double>(5.0E-10);
        [SpiceName("njs"), SpiceInfo("Source junction emission coefficient")]
        public Parameter<double> BSIM4SjctEmissionCoeff { get; } = new Parameter<double>();
        [SpiceName("pbswgs"), SpiceInfo("Source (gate side) sidewall junction capacitance built in potential")]
        public Parameter<double> BSIM4SGatesidewallJctPotential { get; } = new Parameter<double>();
        [SpiceName("mjswgs"), SpiceInfo("Source (gate side) sidewall junction capacitance grading coefficient")]
        public Parameter<double> BSIM4SbulkJctGateSideGradingCoeff { get; } = new Parameter<double>();
        [SpiceName("cjswgs"), SpiceInfo("Source (gate side) sidewall junction capacitance per unit width")]
        public Parameter<double> BSIM4SunitLengthGateSidewallJctCap { get; } = new Parameter<double>();
        [SpiceName("xtis"), SpiceInfo("Source junction current temperature exponent")]
        public Parameter<double> BSIM4SjctTempExponent { get; } = new Parameter<double>();
        [SpiceName("jsd"), SpiceInfo("Bottom drain junction reverse saturation current density")]
        public Parameter<double> BSIM4DjctSatCurDensity { get; } = new Parameter<double>();
        [SpiceName("jswd"), SpiceInfo("Isolation edge sidewall drain junction reverse saturation current density")]
        public Parameter<double> BSIM4DjctSidewallSatCurDensity { get; } = new Parameter<double>();
        [SpiceName("jswgd"), SpiceInfo("Gate edge drain junction reverse saturation current density")]
        public Parameter<double> BSIM4DjctGateSidewallSatCurDensity { get; } = new Parameter<double>();
        [SpiceName("pbd"), SpiceInfo("Drain junction built-in potential")]
        public Parameter<double> BSIM4DbulkJctPotential { get; } = new Parameter<double>();
        [SpiceName("mjd"), SpiceInfo("Drain bottom junction capacitance grading coefficient")]
        public Parameter<double> BSIM4DbulkJctBotGradingCoeff { get; } = new Parameter<double>();
        [SpiceName("pbswd"), SpiceInfo("Drain sidewall junction capacitance built in potential")]
        public Parameter<double> BSIM4DsidewallJctPotential { get; } = new Parameter<double>();
        [SpiceName("mjswd"), SpiceInfo("Drain sidewall junction capacitance grading coefficient")]
        public Parameter<double> BSIM4DbulkJctSideGradingCoeff { get; } = new Parameter<double>();
        [SpiceName("cjd"), SpiceInfo("Drain bottom junction capacitance per unit area")]
        public Parameter<double> BSIM4DunitAreaJctCap { get; } = new Parameter<double>();
        [SpiceName("cjswd"), SpiceInfo("Drain sidewall junction capacitance per unit periphery")]
        public Parameter<double> BSIM4DunitLengthSidewallJctCap { get; } = new Parameter<double>();
        [SpiceName("njd"), SpiceInfo("Drain junction emission coefficient")]
        public Parameter<double> BSIM4DjctEmissionCoeff { get; } = new Parameter<double>();
        [SpiceName("pbswgd"), SpiceInfo("Drain (gate side) sidewall junction capacitance built in potential")]
        public Parameter<double> BSIM4DGatesidewallJctPotential { get; } = new Parameter<double>();
        [SpiceName("mjswgd"), SpiceInfo("Drain (gate side) sidewall junction capacitance grading coefficient")]
        public Parameter<double> BSIM4DbulkJctGateSideGradingCoeff { get; } = new Parameter<double>();
        [SpiceName("cjswgd"), SpiceInfo("Drain (gate side) sidewall junction capacitance per unit width")]
        public Parameter<double> BSIM4DunitLengthGateSidewallJctCap { get; } = new Parameter<double>();
        [SpiceName("xtid"), SpiceInfo("Drainjunction current temperature exponent")]
        public Parameter<double> BSIM4DjctTempExponent { get; } = new Parameter<double>();
        [SpiceName("lint"), SpiceInfo("Length reduction parameter")]
        public Parameter<double> BSIM4Lint { get; } = new Parameter<double>();
        [SpiceName("ll"), SpiceInfo("Length reduction parameter")]
        public Parameter<double> BSIM4Ll { get; } = new Parameter<double>();
        [SpiceName("llc"), SpiceInfo("Length reduction parameter for CV")]
        public Parameter<double> BSIM4Llc { get; } = new Parameter<double>();
        [SpiceName("lln"), SpiceInfo("Length reduction parameter")]
        public Parameter<double> BSIM4Lln { get; } = new Parameter<double>();
        [SpiceName("lw"), SpiceInfo("Length reduction parameter")]
        public Parameter<double> BSIM4Lw { get; } = new Parameter<double>();
        [SpiceName("lwc"), SpiceInfo("Length reduction parameter for CV")]
        public Parameter<double> BSIM4Lwc { get; } = new Parameter<double>();
        [SpiceName("lwn"), SpiceInfo("Length reduction parameter")]
        public Parameter<double> BSIM4Lwn { get; } = new Parameter<double>();
        [SpiceName("lwl"), SpiceInfo("Length reduction parameter")]
        public Parameter<double> BSIM4Lwl { get; } = new Parameter<double>();
        [SpiceName("lwlc"), SpiceInfo("Length reduction parameter for CV")]
        public Parameter<double> BSIM4Lwlc { get; } = new Parameter<double>();
        [SpiceName("lmin"), SpiceInfo("Minimum length for the model")]
        public Parameter<double> BSIM4Lmin { get; } = new Parameter<double>();
        [SpiceName("lmax"), SpiceInfo("Maximum length for the model")]
        public Parameter<double> BSIM4Lmax { get; } = new Parameter<double>();
        [SpiceName("wint"), SpiceInfo("Width reduction parameter")]
        public Parameter<double> BSIM4Wint { get; } = new Parameter<double>();
        [SpiceName("wl"), SpiceInfo("Width reduction parameter")]
        public Parameter<double> BSIM4Wl { get; } = new Parameter<double>();
        [SpiceName("wlc"), SpiceInfo("Width reduction parameter for CV")]
        public Parameter<double> BSIM4Wlc { get; } = new Parameter<double>();
        [SpiceName("wln"), SpiceInfo("Width reduction parameter")]
        public Parameter<double> BSIM4Wln { get; } = new Parameter<double>();
        [SpiceName("ww"), SpiceInfo("Width reduction parameter")]
        public Parameter<double> BSIM4Ww { get; } = new Parameter<double>();
        [SpiceName("wwc"), SpiceInfo("Width reduction parameter for CV")]
        public Parameter<double> BSIM4Wwc { get; } = new Parameter<double>();
        [SpiceName("wwn"), SpiceInfo("Width reduction parameter")]
        public Parameter<double> BSIM4Wwn { get; } = new Parameter<double>();
        [SpiceName("wwl"), SpiceInfo("Width reduction parameter")]
        public Parameter<double> BSIM4Wwl { get; } = new Parameter<double>();
        [SpiceName("wwlc"), SpiceInfo("Width reduction parameter for CV")]
        public Parameter<double> BSIM4Wwlc { get; } = new Parameter<double>();
        [SpiceName("wmin"), SpiceInfo("Minimum width for the model")]
        public Parameter<double> BSIM4Wmin { get; } = new Parameter<double>();
        [SpiceName("wmax"), SpiceInfo("Maximum width for the model")]
        public Parameter<double> BSIM4Wmax { get; } = new Parameter<double>();
        [SpiceName("noia"), SpiceInfo("Flicker noise parameter")]
        public Parameter<double> BSIM4oxideTrapDensityA { get; } = new Parameter<double>();
        [SpiceName("noib"), SpiceInfo("Flicker noise parameter")]
        public Parameter<double> BSIM4oxideTrapDensityB { get; } = new Parameter<double>();
        [SpiceName("noic"), SpiceInfo("Flicker noise parameter")]
        public Parameter<double> BSIM4oxideTrapDensityC { get; } = new Parameter<double>(8.75e9);
        [SpiceName("em"), SpiceInfo("Flicker noise parameter")]
        public Parameter<double> BSIM4em { get; } = new Parameter<double>(4.1e7);
        [SpiceName("ef"), SpiceInfo("Flicker noise frequency exponent")]
        public Parameter<double> BSIM4ef { get; } = new Parameter<double>();
        [SpiceName("af"), SpiceInfo("Flicker noise exponent")]
        public Parameter<double> BSIM4af { get; } = new Parameter<double>();
        [SpiceName("kf"), SpiceInfo("Flicker noise coefficient")]
        public Parameter<double> BSIM4kf { get; } = new Parameter<double>();

        /// <summary>
        /// Methods
        /// </summary>
        [SpiceName("nmos"), SpiceInfo("Flag to indicate NMOS")]
        public void SetNMOS(bool value)
        {
            if (value)
            {
                BSIM4type = 1;
            }
        }
        [SpiceName("pmos"), SpiceInfo("Flag to indicate PMOS")]
        public void SetPMOS(bool value)
        {
            if (value)
            {
                BSIM4type = -1;
            }
        }

        /// <summary>
        /// Shared parameters
        /// </summary>
        public double tnoiMod { get; private set; }
        public double DMCGeff { get; private set; }
        public double DMCIeff { get; private set; }
        public double DMDGeff { get; private set; }
        public double Temp { get; private set; }
        public double epsrox { get; private set; }
        public double toxe { get; private set; }
        public double epssub { get; private set; }
        public double pLastKnot { get; private set; }
        public double Tnom { get; private set; }
        public double TRatio { get; private set; }
        public double Vtm0 { get; private set; }
        public double Eg0 { get; private set; }
        public double ni { get; private set; }
        public double delTemp { get; private set; }

        /// <summary>
        /// Extra variables
        /// </summary>
        public int BSIM4type { get; private set; } = 1;
        public double BSIM4coxe { get; private set; }
        public double BSIM4factor1 { get; private set; }
        public double BSIM4vtm { get; private set; }
        public double BSIM4SjctTempSatCurDensity { get; private set; }
        public double BSIM4SjctSidewallTempSatCurDensity { get; private set; }
        public double BSIM4SjctGateSidewallTempSatCurDensity { get; private set; }
        public double BSIM4DjctTempSatCurDensity { get; private set; }
        public double BSIM4DjctSidewallTempSatCurDensity { get; private set; }
        public double BSIM4DjctGateSidewallTempSatCurDensity { get; private set; }
        public double BSIM4njtsstemp { get; private set; }
        public double BSIM4njtsswstemp { get; private set; }
        public double BSIM4njtsswgstemp { get; private set; }
        public double BSIM4njtsdtemp { get; private set; }
        public double BSIM4njtsswdtemp { get; private set; }
        public double BSIM4njtsswgdtemp { get; private set; }
        public double BSIM4coxp { get; private set; }
        public double BSIM4vcrit { get; private set; }
        public double BSIM4vtm0 { get; private set; }
        public double BSIM4Eg0 { get; private set; }
        public double BSIM4SunitAreaTempJctCap { get; private set; }
        public double BSIM4DunitAreaTempJctCap { get; private set; }
        public double BSIM4SunitLengthSidewallTempJctCap { get; private set; }
        public double BSIM4DunitLengthSidewallTempJctCap { get; private set; }
        public double BSIM4SunitLengthGateSidewallTempJctCap { get; private set; }
        public double BSIM4DunitLengthGateSidewallTempJctCap { get; private set; }
        public double BSIM4PhiBS { get; private set; }
        public double BSIM4PhiBD { get; private set; }
        public double BSIM4PhiBSWS { get; private set; }
        public double BSIM4PhiBSWD { get; private set; }
        public double BSIM4PhiBSWGS { get; private set; }
        public double BSIM4PhiBSWGD { get; private set; }

        private const int NMOS = 1;
        private const int PMOS = -1;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the device</param>
        public BSIM4Model(string name) : base(name)
        {
        }

        /// <summary>
        /// Setup the device
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Setup(Circuit ckt)
        {

            /* process defaults of model parameters */
            if (!BSIM4mobMod.Given)
                BSIM4mobMod.Value = 0;
            else if ((BSIM4mobMod != 0) && (BSIM4mobMod != 1) && (BSIM4mobMod != 2) && (BSIM4mobMod != 3) && (BSIM4mobMod != 4) &&
                 (BSIM4mobMod != 5) && (BSIM4mobMod != 6))
            /* Synopsys 08 / 30 / 2013 modify */
            {
                BSIM4mobMod.Value = 0;
                CircuitWarning.Warning(this, "Warning: mobMod has been set to its default value: 0.");
            }

            if (!BSIM4dioMod.Given)
                BSIM4dioMod.Value = 1;
            else if ((BSIM4dioMod != 0) && (BSIM4dioMod != 1) && (BSIM4dioMod != 2))
            {
                BSIM4dioMod.Value = 1;
                CircuitWarning.Warning(this, "Warning: dioMod has been set to its default value: 1.");
            }

            if (!BSIM4capMod.Given)
                BSIM4capMod.Value = 2;
            else if ((BSIM4capMod != 0) && (BSIM4capMod != 1) && (BSIM4capMod != 2))
            {
                BSIM4capMod.Value = 2;
                CircuitWarning.Warning(this, "Warning: capMod has been set to its default value: 2.");
            }

            if (!BSIM4rdsMod.Given)
                BSIM4rdsMod.Value = 0;
            else if ((BSIM4rdsMod != 0) && (BSIM4rdsMod != 1))
            {
                BSIM4rdsMod.Value = 0;
                CircuitWarning.Warning(this, "Warning: rdsMod has been set to its default value: 0.");
            }

            if (!BSIM4rbodyMod.Given)
                BSIM4rbodyMod.Value = 0;
            else if ((BSIM4rbodyMod != 0) && (BSIM4rbodyMod != 1) && (BSIM4rbodyMod != 2))
            {
                BSIM4rbodyMod.Value = 0;
                CircuitWarning.Warning(this, "Warning: rbodyMod has been set to its default value: 0.");
            }

            if (!BSIM4rgateMod.Given)
                BSIM4rgateMod.Value = 0;
            else if ((BSIM4rgateMod != 0) && (BSIM4rgateMod != 1) && (BSIM4rgateMod != 2) && (BSIM4rgateMod != 3))
            {
                BSIM4rgateMod.Value = 0;
                CircuitWarning.Warning(this, "Warning: rgateMod has been set to its default value: 0.");
            }

            if (!BSIM4perMod.Given)
                BSIM4perMod.Value = 1;
            else if ((BSIM4perMod != 0) && (BSIM4perMod != 1))
            {
                BSIM4perMod.Value = 1;
                CircuitWarning.Warning(this, "Warning: perMod has been set to its default value: 1.");
            }

            if (!BSIM4fnoiMod.Given)
                BSIM4fnoiMod.Value = 1;
            else if ((BSIM4fnoiMod != 0) && (BSIM4fnoiMod != 1))
            {
                BSIM4fnoiMod.Value = 1;
                CircuitWarning.Warning(this, "Warning: fnoiMod has been set to its default value: 1.");
            }

            if (!BSIM4tnoiMod.Given)
                BSIM4tnoiMod.Value = 0;/* WDLiu: tnoiMod = 1 needs to set internal S / D nodes */
            else if ((BSIM4tnoiMod != 0) && (BSIM4tnoiMod != 1) && (BSIM4tnoiMod != 2))
            /* v4.7 */
            {
                BSIM4tnoiMod.Value = 0;
                CircuitWarning.Warning(this, "Warning: tnoiMod has been set to its default value: 0.");
            }

            if (!BSIM4trnqsMod.Given)
                BSIM4trnqsMod.Value = 0;
            else if ((BSIM4trnqsMod != 0) && (BSIM4trnqsMod != 1))
            {
                BSIM4trnqsMod.Value = 0;
                CircuitWarning.Warning(this, "Warning: trnqsMod has been set to its default value: 0.");
            }

            if (!BSIM4acnqsMod.Given)
                BSIM4acnqsMod.Value = 0;
            else if ((BSIM4acnqsMod != 0) && (BSIM4acnqsMod != 1))
            {
                BSIM4acnqsMod.Value = 0;
                CircuitWarning.Warning(this, "Warning: acnqsMod has been set to its default value: 0.");
            }

            if (!BSIM4mtrlMod.Given)
                BSIM4mtrlMod.Value = 0;
            else if ((BSIM4mtrlMod != 0) && (BSIM4mtrlMod != 1))
            {
                BSIM4mtrlMod.Value = 0;
                CircuitWarning.Warning(this, "Warning: mtrlMod has been set to its default value: 0.");
            }

            if (!BSIM4mtrlCompatMod.Given)
                BSIM4mtrlCompatMod.Value = 0;
            else if ((BSIM4mtrlCompatMod != 0) && (BSIM4mtrlCompatMod != 1))
            {
                BSIM4mtrlCompatMod.Value = 0;
                CircuitWarning.Warning(this, "Warning: mtrlCompatMod has been set to its default value: 0.");
            }

            if (!BSIM4igcMod.Given)
                BSIM4igcMod.Value = 0;
            else if ((BSIM4igcMod != 0) && (BSIM4igcMod != 1) && (BSIM4igcMod != 2))
            {
                BSIM4igcMod.Value = 0;
                CircuitWarning.Warning(this, "Warning: igcMod has been set to its default value: 0.");
            }

            if (!BSIM4igbMod.Given)
                BSIM4igbMod.Value = 0;
            else if ((BSIM4igbMod != 0) && (BSIM4igbMod != 1))
            {
                BSIM4igbMod.Value = 0;
                CircuitWarning.Warning(this, "Warning: igbMod has been set to its default value: 0.");
            }

            if (!BSIM4tempMod.Given)
                BSIM4tempMod.Value = 0;
            else if ((BSIM4tempMod != 0) && (BSIM4tempMod != 1) && (BSIM4tempMod != 2) && (BSIM4tempMod != 3))
            {
                BSIM4tempMod.Value = 0;
                CircuitWarning.Warning(this, "Warning: tempMod has been set to its default value: 0.");
            }

            if (!BSIM4vddeot.Given)
                BSIM4vddeot.Value = (BSIM4type == NMOS) ? 1.5 : -1.5;
            if (!BSIM4toxp.Given)
                BSIM4toxp.Value = BSIM4toxe;
            if (!BSIM4toxm.Given)
                BSIM4toxm.Value = BSIM4toxe;
            if (!BSIM4dvtp2.Given)
                /* New DIBL / Rout */
                BSIM4dvtp2.Value = 0.0;

            if (!BSIM4dsub.Given)
                BSIM4dsub.Value = BSIM4drout;
            if (!BSIM4vth0.Given)
                BSIM4vth0.Value = (BSIM4type == NMOS) ? 0.7 : -0.7;
            if (!BSIM4eu.Given)
                BSIM4eu.Value = (BSIM4type == NMOS) ? 1.67 : 1.0;
            if (!BSIM4ucs.Given)
                BSIM4ucs.Value = (BSIM4type == NMOS) ? 1.67 : 1.0;
            if (!BSIM4ua.Given)
                BSIM4ua.Value = ((BSIM4mobMod.Value == 2)) ? 1.0e-15 : 1.0e-9;
            if (!BSIM4uc.Given)
                BSIM4uc.Value = (BSIM4mobMod.Value == 1) ? -0.0465 : -0.0465e-9;
            if (!BSIM4uc1.Given)
                BSIM4uc1.Value = (BSIM4mobMod.Value == 1) ? -0.056 : -0.056e-9;
            /* unit m *  * (-2) */
            if (!BSIM4u0.Given)
                BSIM4u0.Value = (BSIM4type == NMOS) ? 0.067 : 0.025;

            /* Default value of agisl, bgisl, cgisl, egisl, rgisl, kgisl, and fgisl are set as follows */
            if (!BSIM4agisl.Given)
                BSIM4agisl.Value = BSIM4agidl;
            if (!BSIM4bgisl.Given)
                BSIM4bgisl.Value = BSIM4bgidl;
            if (!BSIM4cgisl.Given)
                BSIM4cgisl.Value = BSIM4cgidl;
            if (!BSIM4egisl.Given)
                BSIM4egisl.Value = BSIM4egidl;
            if (!BSIM4rgisl.Given)
                /* v4.7 New GIDL / GISL */
                BSIM4rgisl.Value = BSIM4rgidl;
            if (!BSIM4kgisl.Given)
                /* v4.7 New GIDL / GISL */
                BSIM4kgisl.Value = BSIM4kgidl;
            if (!BSIM4fgisl.Given)
                /* v4.7 New GIDL / GISL */
                BSIM4fgisl.Value = BSIM4fgidl;

            if (!BSIM4aigc.Given)
                BSIM4aigc.Value = (BSIM4type == NMOS) ? 1.36e-2 : 9.80e-3;
            if (!BSIM4bigc.Given)
                BSIM4bigc.Value = (BSIM4type == NMOS) ? 1.71e-3 : 7.59e-4;
            if (!BSIM4cigc.Given)
                BSIM4cigc.Value = (BSIM4type == NMOS) ? 0.075 : 0.03;
            if (BSIM4aigsd.Given)
            {
                BSIM4aigs.Value = BSIM4aigd.Value = BSIM4aigsd;
            }
            else
            {
                BSIM4aigsd.Value = (BSIM4type == NMOS) ? 1.36e-2 : 9.80e-3;
                if (!BSIM4aigs.Given)
                    BSIM4aigs.Value = (BSIM4type == NMOS) ? 1.36e-2 : 9.80e-3;
                if (!BSIM4aigd.Given)
                    BSIM4aigd.Value = (BSIM4type == NMOS) ? 1.36e-2 : 9.80e-3;
            }
            if (BSIM4bigsd.Given)
            {
                BSIM4bigs.Value = BSIM4bigd.Value = BSIM4bigsd;
            }
            else
            {
                BSIM4bigsd.Value = (BSIM4type == NMOS) ? 1.71e-3 : 7.59e-4;
                if (!BSIM4bigs.Given)
                    BSIM4bigs.Value = (BSIM4type == NMOS) ? 1.71e-3 : 7.59e-4;
                if (!BSIM4bigd.Given)
                    BSIM4bigd.Value = (BSIM4type == NMOS) ? 1.71e-3 : 7.59e-4;
            }
            if (BSIM4cigsd.Given)
            {
                BSIM4cigs.Value = BSIM4cigd.Value = BSIM4cigsd;
            }
            else
            {
                BSIM4cigsd.Value = (BSIM4type == NMOS) ? 0.075 : 0.03;
                if (!BSIM4cigs.Given)
                    BSIM4cigs.Value = (BSIM4type == NMOS) ? 0.075 : 0.03;
                if (!BSIM4cigd.Given)
                    BSIM4cigd.Value = (BSIM4type == NMOS) ? 0.075 : 0.03;
            }
            if (!BSIM4ijthdfwd.Given)
                BSIM4ijthdfwd.Value = BSIM4ijthsfwd;
            if (!BSIM4ijthdrev.Given)
                BSIM4ijthdrev.Value = BSIM4ijthsrev;

            if (!BSIM4xjbvd.Given)
                BSIM4xjbvd.Value = BSIM4xjbvs;
            if (!BSIM4bvd.Given)
                BSIM4bvd.Value = BSIM4bvs;

            if (!BSIM4ckappad.Given)
                BSIM4ckappad.Value = BSIM4ckappas;
            if (!BSIM4dmci.Given)
                BSIM4dmci.Value = BSIM4dmcg;

            /* Default value of lagisl, lbgisl, lcgisl, legisl, lrgisl, lkgisl, and lfgisl are set as follows */
            if (!BSIM4lagisl.Given)
                BSIM4lagisl.Value = BSIM4lagidl;
            if (!BSIM4lbgisl.Given)
                BSIM4lbgisl.Value = BSIM4lbgidl;
            if (!BSIM4lcgisl.Given)
                BSIM4lcgisl.Value = BSIM4lcgidl;
            if (!BSIM4legisl.Given)
                BSIM4legisl.Value = BSIM4legidl;
            if (!BSIM4lrgisl.Given)
                /* v4.7 New GIDL / GISL */
                BSIM4lrgisl.Value = BSIM4lrgidl;
            if (!BSIM4lkgisl.Given)
                /* v4.7 New GIDL / GISL */
                BSIM4lkgisl.Value = BSIM4lkgidl;
            if (!BSIM4lfgisl.Given)
                /* v4.7 New GIDL / GISL */
                BSIM4lfgisl.Value = BSIM4lfgidl;

            if (!BSIM4aigsd.Given && (BSIM4aigs.Given || BSIM4aigd.Given))
            {
                if (!BSIM4aigs.Given)
                    BSIM4aigs.Value = 0.0;
                if (!BSIM4aigd.Given)
                    BSIM4aigd.Value = 0.0;
            }
            else
            {
                if (!BSIM4laigsd.Given)
                    BSIM4laigsd.Value = 0.0;
                BSIM4laigs.Value = BSIM4laigd.Value = BSIM4laigsd;
            }
            if (!BSIM4bigsd.Given && (BSIM4bigs.Given || BSIM4bigd.Given))
            {
                if (!BSIM4lcigs.Given)
                    BSIM4lcigs.Value = 0.0;
                if (!BSIM4lcigd.Given)
                    BSIM4lcigd.Value = 0.0;
            }
            else
            {
                if (!BSIM4lcigsd.Given)
                    BSIM4lcigsd.Value = 0.0;
                BSIM4lbigs.Value = BSIM4lbigd.Value = BSIM4lbigsd;
            }
            if (!BSIM4cigsd.Given && (BSIM4cigs.Given || BSIM4cigd.Given))
            {
                if (!BSIM4lcigs.Given)
                    BSIM4lcigs.Value = 0.0;
                if (!BSIM4lcigd.Given)
                    BSIM4lcigd.Value = 0.0;
            }
            else
            {
                if (!BSIM4cigsd.Given)
                    BSIM4cigsd.Value = 0.0;
                BSIM4lcigs.Value = BSIM4lcigd.Value = BSIM4lcigsd;
            }

            if (!BSIM4ltnfactor.Given)
                /* v4.7 temp dep of leakage current */
                BSIM4ltnfactor.Value = 0.0;
            if (!BSIM4lteta0.Given)
                /* v4.7 temp dep of leakage current */
                BSIM4lteta0.Value = 0.0;
            if (!BSIM4ltvoffcv.Given)
                /* v4.7 temp dep of leakage current */
                BSIM4ltvoffcv.Value = 0.0;

            /* Width dependence */
            if (!BSIM4wdvtp2.Given)
                /* New DIBL / Rout */
                BSIM4wdvtp2.Value = 0.0;
            if (!BSIM4wrgidl.Given)
                /* v4.7 New GIDL / GISL */
                BSIM4wrgidl.Value = 0.0;
            if (!BSIM4wkgidl.Given)
                /* v4.7 New GIDL / GISL */
                BSIM4wkgidl.Value = 0.0;
            if (!BSIM4wfgidl.Given)
                /* v4.7 New GIDL / GISL */
                BSIM4wfgidl.Value = 0.0;

            /* Default value of wagisl, wbgisl, wcgisl, wegisl, wrgisl, wkgisl, and wfgisl are set as follows */
            if (!BSIM4wagisl.Given)
                BSIM4wagisl.Value = BSIM4wagidl;
            if (!BSIM4wbgisl.Given)
                BSIM4wbgisl.Value = BSIM4wbgidl;
            if (!BSIM4wcgisl.Given)
                BSIM4wcgisl.Value = BSIM4wcgidl;
            if (!BSIM4wegisl.Given)
                BSIM4wegisl.Value = BSIM4wegidl;
            if (!BSIM4wrgisl.Given)
                /* v4.7 New GIDL / GISL */
                BSIM4wrgisl.Value = BSIM4wrgidl;
            if (!BSIM4wkgisl.Given)
                /* v4.7 New GIDL / GISL */
                BSIM4wkgisl.Value = BSIM4wkgidl;
            if (!BSIM4wfgisl.Given)
                /* v4.7 New GIDL / GISL */
                BSIM4wfgisl.Value = BSIM4wfgidl;

            if (!BSIM4aigsd.Given && (BSIM4aigs.Given || BSIM4aigd.Given))
            {
                if (!BSIM4waigs.Given)
                    BSIM4waigs.Value = 0.0;
                if (!BSIM4waigd.Given)
                    BSIM4waigd.Value = 0.0;
            }
            else
            {
                if (!BSIM4waigsd.Given)
                    BSIM4waigsd.Value = 0.0;
                BSIM4waigs.Value = BSIM4waigd.Value = BSIM4waigsd;
            }
            if (!BSIM4bigsd.Given && (BSIM4bigs.Given || BSIM4bigd.Given))
            {
                if (!BSIM4wbigs.Given)
                    BSIM4wbigs.Value = 0.0;
                if (!BSIM4wbigd.Given)
                    BSIM4wbigd.Value = 0.0;
            }
            else
            {
                if (!BSIM4wbigsd.Given)
                    BSIM4wbigsd.Value = 0.0;
                BSIM4wbigs.Value = BSIM4wbigd.Value = BSIM4wbigsd;
            }
            if (!BSIM4cigsd.Given && (BSIM4cigs.Given || BSIM4cigd.Given))
            {
                if (!BSIM4wcigs.Given)
                    BSIM4wcigs.Value = 0.0;
                if (!BSIM4wcigd.Given)
                    BSIM4wcigd.Value = 0.0;
            }
            else
            {
                if (!BSIM4wcigsd.Given)
                    BSIM4wcigsd.Value = 0.0;
                BSIM4wcigs.Value = BSIM4wcigd.Value = BSIM4wcigsd;
            }

            if (!BSIM4wtnfactor.Given)
                /* v4.7 temp dep of leakage current */
                BSIM4wtnfactor.Value = 0.0;
            if (!BSIM4wteta0.Given)
                /* v4.7 temp dep of leakage current */
                BSIM4wteta0.Value = 0.0;
            if (!BSIM4wtvoffcv.Given)
                /* v4.7 temp dep of leakage current */
                BSIM4wtvoffcv.Value = 0.0;

            /* Cross - term dependence */
            if (!BSIM4pdvtp2.Given)
                /* New DIBL / Rout */
                BSIM4pdvtp2.Value = 0.0;
            if (!BSIM4prgidl.Given)
                /* v4.7 New GIDL / GISL */
                BSIM4prgidl.Value = 0.0;
            if (!BSIM4pkgidl.Given)
                /* v4.7 New GIDL / GISL */
                BSIM4pkgidl.Value = 0.0;
            if (!BSIM4pfgidl.Given)
                /* v4.7 New GIDL / GISL */
                BSIM4pfgidl.Value = 0.0;

            /* Default value of pagisl, pbgisl, pcgisl, pegisl, prgisl, pkgisl, and pfgisl are set as follows */
            if (!BSIM4pagisl.Given)
                BSIM4pagisl.Value = BSIM4pagidl;
            if (!BSIM4pbgisl.Given)
                BSIM4pbgisl.Value = BSIM4pbgidl;
            if (!BSIM4pcgisl.Given)
                BSIM4pcgisl.Value = BSIM4pcgidl;
            if (!BSIM4pegisl.Given)
                BSIM4pegisl.Value = BSIM4pegidl;
            if (!BSIM4prgisl.Given)
                /* v4.7 New GIDL / GISL */
                BSIM4prgisl.Value = BSIM4prgidl;
            if (!BSIM4pkgisl.Given)
                /* v4.7 New GIDL / GISL */
                BSIM4pkgisl.Value = BSIM4pkgidl;
            if (!BSIM4pfgisl.Given)
                /* v4.7 New GIDL / GISL */
                BSIM4pfgisl.Value = BSIM4pfgidl;

            if (!BSIM4aigsd.Given && (BSIM4aigs.Given || BSIM4aigd.Given))
            {
                if (!BSIM4paigs.Given)
                    BSIM4paigs.Value = 0.0;
                if (!BSIM4paigd.Given)
                    BSIM4paigd.Value = 0.0;
            }
            else
            {
                if (!BSIM4paigsd.Given)
                    BSIM4paigsd.Value = 0.0;
                BSIM4paigs.Value = BSIM4paigd.Value = BSIM4paigsd;
            }
            if (!BSIM4bigsd.Given && (BSIM4bigs.Given || BSIM4bigd.Given))
            {
                if (!BSIM4pbigs.Given)
                    BSIM4pbigs.Value = 0.0;
                if (!BSIM4pbigd.Given)
                    BSIM4pbigd.Value = 0.0;
            }
            else
            {
                if (!BSIM4pbigsd.Given)
                    BSIM4pbigsd.Value = 0.0;
                BSIM4pbigs.Value = BSIM4pbigd.Value = BSIM4pbigsd;
            }

            if (!BSIM4cigsd.Given && (BSIM4cigs.Given || BSIM4cigd.Given))
            {
                if (!BSIM4pcigs.Given)
                    BSIM4pcigs.Value = 0.0;
                if (!BSIM4pcigd.Given)
                    BSIM4pcigd.Value = 0.0;
            }
            else
            {
                if (!BSIM4pcigsd.Given)
                    BSIM4pcigsd.Value = 0.0;
                BSIM4pcigs.Value = BSIM4pcigd.Value = BSIM4pcigsd;
            }
            if (!BSIM4ptnfactor.Given)
                /* v4.7 temp dep of leakage current */
                BSIM4ptnfactor.Value = 0.0;
            if (!BSIM4pteta0.Given)
                /* v4.7 temp dep of leakage current */
                BSIM4pteta0.Value = 0.0;
            if (!BSIM4ptvoffcv.Given)
                /* v4.7 temp dep of leakage current */
                BSIM4ptvoffcv.Value = 0.0;
            /* unit degree celcius */
            if (!BSIM4tnom.Given)
                BSIM4tnom.Value = ckt.State.NominalTemperature;
            if (!BSIM4Llc.Given)
                BSIM4Llc.Value = BSIM4Ll;
            if (!BSIM4Lwc.Given)
                BSIM4Lwc.Value = BSIM4Lw;
            if (!BSIM4Lwlc.Given)
                BSIM4Lwlc.Value = BSIM4Lwl;
            if (!BSIM4Wlc.Given)
                BSIM4Wlc.Value = BSIM4Wl;
            if (!BSIM4Wwc.Given)
                BSIM4Wwc.Value = BSIM4Ww;
            if (!BSIM4Wwlc.Given)
                BSIM4Wwlc.Value = BSIM4Wwl;
            if (!BSIM4dwc.Given)
                BSIM4dwc.Value = BSIM4Wint;
            if (!BSIM4dlc.Given)
                BSIM4dlc.Value = BSIM4Lint;
            if (!BSIM4dlcig.Given)
                BSIM4dlcig.Value = BSIM4Lint;
            if (!BSIM4dlcigd.Given)
            {
                if (BSIM4dlcig.Given)
                    BSIM4dlcigd.Value = BSIM4dlcig;
                else
                    BSIM4dlcigd.Value = BSIM4Lint;
            }
            if (!BSIM4dwj.Given)
                BSIM4dwj.Value = BSIM4dwc;
            if (!BSIM4cf.Given)
                BSIM4cf.Value = 2.0 * BSIM4epsrox * Transistor.EPS0 / Circuit.CONSTPI * Math.Log(1.0 + 0.4e-6 / BSIM4toxe);

            if (!BSIM4DunitAreaJctCap.Given)
                BSIM4DunitAreaJctCap.Value = BSIM4SunitAreaJctCap;
            if (!BSIM4DunitLengthSidewallJctCap.Given)
                BSIM4DunitLengthSidewallJctCap.Value = BSIM4SunitLengthSidewallJctCap;
            if (!BSIM4SunitLengthGateSidewallJctCap.Given)
                BSIM4SunitLengthGateSidewallJctCap.Value = BSIM4SunitLengthSidewallJctCap;
            if (!BSIM4DunitLengthGateSidewallJctCap.Given)
                BSIM4DunitLengthGateSidewallJctCap.Value = BSIM4SunitLengthGateSidewallJctCap;
            if (!BSIM4DjctSatCurDensity.Given)
                BSIM4DjctSatCurDensity.Value = BSIM4SjctSatCurDensity;
            if (!BSIM4DjctSidewallSatCurDensity.Given)
                BSIM4DjctSidewallSatCurDensity.Value = BSIM4SjctSidewallSatCurDensity;
            if (!BSIM4DjctGateSidewallSatCurDensity.Given)
                BSIM4DjctGateSidewallSatCurDensity.Value = BSIM4SjctGateSidewallSatCurDensity;
            if (!BSIM4DbulkJctPotential.Given)
                BSIM4DbulkJctPotential.Value = BSIM4SbulkJctPotential;
            if (!BSIM4DsidewallJctPotential.Given)
                BSIM4DsidewallJctPotential.Value = BSIM4SsidewallJctPotential;
            if (!BSIM4SGatesidewallJctPotential.Given)
                BSIM4SGatesidewallJctPotential.Value = BSIM4SsidewallJctPotential;
            if (!BSIM4DGatesidewallJctPotential.Given)
                BSIM4DGatesidewallJctPotential.Value = BSIM4SGatesidewallJctPotential;
            if (!BSIM4DbulkJctBotGradingCoeff.Given)
                BSIM4DbulkJctBotGradingCoeff.Value = BSIM4SbulkJctBotGradingCoeff;
            if (!BSIM4DbulkJctSideGradingCoeff.Given)
                BSIM4DbulkJctSideGradingCoeff.Value = BSIM4SbulkJctSideGradingCoeff;
            if (!BSIM4SbulkJctGateSideGradingCoeff.Given)
                BSIM4SbulkJctGateSideGradingCoeff.Value = BSIM4SbulkJctSideGradingCoeff;
            if (!BSIM4DbulkJctGateSideGradingCoeff.Given)
                BSIM4DbulkJctGateSideGradingCoeff.Value = BSIM4SbulkJctGateSideGradingCoeff;
            if (!BSIM4DjctEmissionCoeff.Given)
                BSIM4DjctEmissionCoeff.Value = BSIM4SjctEmissionCoeff;
            if (!BSIM4DjctTempExponent.Given)
                BSIM4DjctTempExponent.Value = BSIM4SjctTempExponent;

            if (!BSIM4jtsd.Given)
                BSIM4jtsd.Value = BSIM4jtss;
            if (!BSIM4jtsswd.Given)
                BSIM4jtsswd.Value = BSIM4jtssws;
            if (!BSIM4jtsswgd.Given)
                BSIM4jtsswgd.Value = BSIM4jtsswgs;
            if (!BSIM4njtsd.Given)
            {
                if (BSIM4njts.Given)
                    BSIM4njtsd.Value = BSIM4njts;
                else
                    BSIM4njtsd.Value = 20.0;
            }
            if (!BSIM4njtsswd.Given)
            {
                if (BSIM4njtssw.Given)
                    BSIM4njtsswd.Value = BSIM4njtssw;
                else
                    BSIM4njtsswd.Value = 20.0;
            }
            if (!BSIM4njtsswgd.Given)
            {
                if (BSIM4njtsswg.Given)
                    BSIM4njtsswgd.Value = BSIM4njtsswg;
                else
                    BSIM4njtsswgd.Value = 20.0;
            }
            if (!BSIM4xtsd.Given)
                BSIM4xtsd.Value = BSIM4xtss;
            if (!BSIM4xtsswd.Given)
                BSIM4xtsswd.Value = BSIM4xtssws;
            if (!BSIM4xtsswgd.Given)
                BSIM4xtsswgd.Value = BSIM4xtsswgs;
            if (!BSIM4tnjtsd.Given)
            {
                if (BSIM4tnjts.Given)
                    BSIM4tnjtsd.Value = BSIM4tnjts;
                else
                    BSIM4tnjtsd.Value = 0.0;
            }
            if (!BSIM4tnjtsswd.Given)
            {
                if (BSIM4tnjtssw.Given)
                    BSIM4tnjtsswd.Value = BSIM4tnjtssw;
                else
                    BSIM4tnjtsswd.Value = 0.0;
            }
            if (!BSIM4tnjtsswgd.Given)
            {
                if (BSIM4tnjtsswg.Given)
                    BSIM4tnjtsswgd.Value = BSIM4tnjtsswg;
                else
                    BSIM4tnjtsswgd.Value = 0.0;
            }
            if (!BSIM4vtsd.Given)
                BSIM4vtsd.Value = BSIM4vtss;
            if (!BSIM4vtsswd.Given)
                BSIM4vtsswd.Value = BSIM4vtssws;
            if (!BSIM4vtsswgd.Given)
                BSIM4vtsswgd.Value = BSIM4vtsswgs;

            if (!BSIM4oxideTrapDensityA.Given)
            {
                if (BSIM4type == NMOS)
                    BSIM4oxideTrapDensityA.Value = 6.25e41;
                else
                    BSIM4oxideTrapDensityA.Value = 6.188e40;
            }
            if (!BSIM4oxideTrapDensityB.Given)
            {
                if (BSIM4type == NMOS)
                    BSIM4oxideTrapDensityB.Value = 3.125e26;
                else
                    BSIM4oxideTrapDensityB.Value = 1.5e25;
            }

            if (!BSIM4wpemod.Given)
                BSIM4wpemod.Value = 0;
            else if ((BSIM4wpemod != 0) && (BSIM4wpemod != 1))
            {
                BSIM4wpemod.Value = 0;
                CircuitWarning.Warning(this, "Warning: wpemod has been set to its default value: 0.");
            }
            DMCGeff = BSIM4dmcg - BSIM4dmcgt;
            DMCIeff = BSIM4dmci;
            DMDGeff = BSIM4dmdg - BSIM4dmcgt;

            /* 
            * End processing models and begin to loop
            * through all the instances of the model
            */
        }

        /// <summary>
        /// Do temperature-dependent calculations
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Temperature(Circuit ckt)
        {
            double Eg;
            double T0, T1, T2, T3;

            Temp = ckt.State.Temperature;
            if (BSIM4SbulkJctPotential < 0.1)
            {
                BSIM4SbulkJctPotential.Value = 0.1;
                CircuitWarning.Warning(this, "Given pbs is less than 0.1. Pbs is set to 0.1.");
            }
            if (BSIM4SsidewallJctPotential < 0.1)
            {
                BSIM4SsidewallJctPotential.Value = 0.1;
                CircuitWarning.Warning(this, "Given pbsws is less than 0.1. Pbsws is set to 0.1.");
            }
            if (BSIM4SGatesidewallJctPotential < 0.1)
            {
                BSIM4SGatesidewallJctPotential.Value = 0.1;
                CircuitWarning.Warning(this, "Given pbswgs is less than 0.1. Pbswgs is set to 0.1.");
            }

            if (BSIM4DbulkJctPotential < 0.1)
            {
                BSIM4DbulkJctPotential.Value = 0.1;
                CircuitWarning.Warning(this, "Given pbd is less than 0.1. Pbd is set to 0.1.");
            }
            if (BSIM4DsidewallJctPotential < 0.1)
            {
                BSIM4DsidewallJctPotential.Value = 0.1;
                CircuitWarning.Warning(this, "Given pbswd is less than 0.1. Pbswd is set to 0.1.");
            }
            if (BSIM4DGatesidewallJctPotential < 0.1)
            {
                BSIM4DGatesidewallJctPotential.Value = 0.1;
                CircuitWarning.Warning(this, "Given pbswgd is less than 0.1. Pbswgd is set to 0.1.");
            }

            if (BSIM4mtrlMod.Value == 0)
            {
                if ((BSIM4toxe.Given) && (BSIM4toxp.Given) && (BSIM4dtox.Given) && (BSIM4toxe != (BSIM4toxp + BSIM4dtox)))
                    CircuitWarning.Warning(this, "Warning: toxe, toxp and dtox all given and toxe != toxp + dtox; dtox ignored.");
                else if ((BSIM4toxe.Given) && (!BSIM4toxp.Given))
                    BSIM4toxp.Value = BSIM4toxe - BSIM4dtox;
                else if ((!BSIM4toxe.Given) && (BSIM4toxp.Given))
                {
                    BSIM4toxe.Value = BSIM4toxp + BSIM4dtox;
                    if (!BSIM4toxm.Given)
                        /* v4.7 */
                        BSIM4toxm.Value = BSIM4toxe;
                }
            }
            else if (BSIM4mtrlCompatMod != 0)
            /* v4.7 */
            {
                T0 = BSIM4epsrox / 3.9;
                if ((BSIM4eot.Given) && (BSIM4toxp.Given) && (BSIM4dtox.Given) && (Math.Abs(BSIM4eot * T0 - (BSIM4toxp + BSIM4dtox)) > 1.0e-20))
                {
                    CircuitWarning.Warning(this, "Warning: eot, toxp and dtox all given and eot * EPSROX / 3.9 != toxp + dtox; dtox ignored.");
                }
                else if ((BSIM4eot.Given) && (!BSIM4toxp.Given))
                    BSIM4toxp.Value = T0 * BSIM4eot - BSIM4dtox;
                else if ((!BSIM4eot.Given) && (BSIM4toxp.Given))
                {
                    BSIM4eot.Value = (BSIM4toxp + BSIM4dtox) / T0;
                    if (!BSIM4toxm.Given)
                        BSIM4toxm.Value = BSIM4eot;
                }
            }

            if (BSIM4mtrlMod > 0)
            {
                epsrox = 3.9;
                toxe = BSIM4eot;
                epssub = Transistor.EPS0 * BSIM4epsrsub;
            }
            else
            {
                epsrox = BSIM4epsrox;
                toxe = BSIM4toxe;
                epssub = Transistor.EPSSI;
            }

            BSIM4coxe = epsrox * Transistor.EPS0 / toxe;
            if (BSIM4mtrlMod.Value == 0 || BSIM4mtrlCompatMod != 0)
                BSIM4coxp = BSIM4epsrox * Transistor.EPS0 / BSIM4toxp;

            if (!BSIM4cgdo.Given)
            {
                if (BSIM4dlc.Given && (BSIM4dlc > 0.0))
                    BSIM4cgdo.Value = BSIM4dlc * BSIM4coxe - BSIM4cgdl;
                else
                    BSIM4cgdo.Value = 0.6 * BSIM4xj * BSIM4coxe;
            }
            if (!BSIM4cgso.Given)
            {
                if (BSIM4dlc.Given && (BSIM4dlc > 0.0))
                    BSIM4cgso.Value = BSIM4dlc * BSIM4coxe - BSIM4cgsl;
                else
                    BSIM4cgso.Value = 0.6 * BSIM4xj * BSIM4coxe;
            }
            if (!BSIM4cgbo.Given)
                BSIM4cgbo.Value = 2.0 * BSIM4dwc * BSIM4coxe;

            Tnom = BSIM4tnom;
            TRatio = Temp / Tnom;

            BSIM4vcrit = Circuit.CONSTvt0 * Math.Log(Circuit.CONSTvt0 / (Circuit.CONSTroot2 * 1.0e-14));
            BSIM4factor1 = Math.Sqrt(epssub / (epsrox * Transistor.EPS0) * toxe);

            Vtm0 = BSIM4vtm0 = Transistor.KboQ * Tnom;

            if (BSIM4mtrlMod.Value == 0)
            {
                Eg0 = 1.16 - 7.02e-4 * Tnom * Tnom / (Tnom + 1108.0);
                ni = 1.45e10 * (Tnom / 300.15) * Math.Sqrt(Tnom / 300.15) * Math.Exp(21.5565981 - Eg0 / (2.0 * Vtm0));
            }
            else
            {
                Eg0 = BSIM4bg0sub - BSIM4tbgasub * Tnom * Tnom / (Tnom + BSIM4tbgbsub);
                T0 = BSIM4bg0sub - BSIM4tbgasub * 90090.0225 / (300.15 + BSIM4tbgbsub);
                ni = BSIM4ni0sub * (Tnom / 300.15) * Math.Sqrt(Tnom / 300.15) * Math.Exp((T0 - Eg0) / (2.0 * Vtm0));
            }

            BSIM4Eg0 = Eg0;
            BSIM4vtm = Transistor.KboQ * Temp;
            if (BSIM4mtrlMod.Value == 0)
                Eg = 1.16 - 7.02e-4 * Temp * Temp / (Temp + 1108.0);
            else
                Eg = BSIM4bg0sub - BSIM4tbgasub * Temp * Temp / (Temp + BSIM4tbgbsub);
            if (Temp != Tnom)
            {
                T0 = Eg0 / Vtm0 - Eg / BSIM4vtm;
                T1 = Math.Log(Temp / Tnom);
                T2 = T0 + BSIM4SjctTempExponent * T1;
                T3 = Math.Exp(T2 / BSIM4SjctEmissionCoeff);
                BSIM4SjctTempSatCurDensity = BSIM4SjctSatCurDensity * T3;
                BSIM4SjctSidewallTempSatCurDensity = BSIM4SjctSidewallSatCurDensity * T3;
                BSIM4SjctGateSidewallTempSatCurDensity = BSIM4SjctGateSidewallSatCurDensity * T3;

                T2 = T0 + BSIM4DjctTempExponent * T1;
                T3 = Math.Exp(T2 / BSIM4DjctEmissionCoeff);
                BSIM4DjctTempSatCurDensity = BSIM4DjctSatCurDensity * T3;
                BSIM4DjctSidewallTempSatCurDensity = BSIM4DjctSidewallSatCurDensity * T3;
                BSIM4DjctGateSidewallTempSatCurDensity = BSIM4DjctGateSidewallSatCurDensity * T3;
            }
            else
            {
                BSIM4SjctTempSatCurDensity = BSIM4SjctSatCurDensity;
                BSIM4SjctSidewallTempSatCurDensity = BSIM4SjctSidewallSatCurDensity;
                BSIM4SjctGateSidewallTempSatCurDensity = BSIM4SjctGateSidewallSatCurDensity;
                BSIM4DjctTempSatCurDensity = BSIM4DjctSatCurDensity;
                BSIM4DjctSidewallTempSatCurDensity = BSIM4DjctSidewallSatCurDensity;
                BSIM4DjctGateSidewallTempSatCurDensity = BSIM4DjctGateSidewallSatCurDensity;
            }

            if (BSIM4SjctTempSatCurDensity < 0.0)
                BSIM4SjctTempSatCurDensity = 0.0;
            if (BSIM4SjctSidewallTempSatCurDensity < 0.0)
                BSIM4SjctSidewallTempSatCurDensity = 0.0;
            if (BSIM4SjctGateSidewallTempSatCurDensity < 0.0)
                BSIM4SjctGateSidewallTempSatCurDensity = 0.0;
            if (BSIM4DjctTempSatCurDensity < 0.0)
                BSIM4DjctTempSatCurDensity = 0.0;
            if (BSIM4DjctSidewallTempSatCurDensity < 0.0)
                BSIM4DjctSidewallTempSatCurDensity = 0.0;
            if (BSIM4DjctGateSidewallTempSatCurDensity < 0.0)
                BSIM4DjctGateSidewallTempSatCurDensity = 0.0;

            T0 = (TRatio - 1.0);
            BSIM4njtsstemp = BSIM4njts * (1.0 + BSIM4tnjts * T0);
            BSIM4njtsswstemp = BSIM4njtssw * (1.0 + BSIM4tnjtssw * T0);
            BSIM4njtsswgstemp = BSIM4njtsswg * (1.0 + BSIM4tnjtsswg * T0);
            BSIM4njtsdtemp = BSIM4njtsd * (1.0 + BSIM4tnjtsd * T0);
            BSIM4njtsswdtemp = BSIM4njtsswd * (1.0 + BSIM4tnjtsswd * T0);
            BSIM4njtsswgdtemp = BSIM4njtsswgd * (1.0 + BSIM4tnjtsswgd * T0);

            /* Temperature dependence of D / B and S / B diode capacitance begins */
            delTemp = ckt.State.Temperature - BSIM4tnom;
            T0 = BSIM4tcj * delTemp;
            if (T0 >= -1.0)
            {
                BSIM4SunitAreaTempJctCap = BSIM4SunitAreaJctCap * (1.0 + T0); /* bug_fix - JX */
                BSIM4DunitAreaTempJctCap = BSIM4DunitAreaJctCap * (1.0 + T0);
            }
            else
            {
                if (BSIM4SunitAreaJctCap > 0.0)
                {
                    BSIM4SunitAreaTempJctCap = 0.0;
                    CircuitWarning.Warning(this, "Temperature effect has caused cjs to be negative. Cjs is clamped to zero.");
                }
                if (BSIM4DunitAreaJctCap > 0.0)
                {
                    BSIM4DunitAreaTempJctCap = 0.0;
                    CircuitWarning.Warning(this, "Temperature effect has caused cjd to be negative. Cjd is clamped to zero.");
                }
            }
            T0 = BSIM4tcjsw * delTemp;
            if (BSIM4SunitLengthSidewallJctCap < 0.0)
            /* 4.6.2 */
            {
                BSIM4SunitLengthSidewallJctCap.Value = 0.0;
                CircuitWarning.Warning(this, "CJSWS is negative. Cjsws is clamped to zero.");
            }
            if (BSIM4DunitLengthSidewallJctCap < 0.0)
            {
                BSIM4DunitLengthSidewallJctCap.Value = 0.0;
                CircuitWarning.Warning(this, "CJSWD is negative. Cjswd is clamped to zero.");
            }
            if (T0 >= -1.0)
            {
                BSIM4SunitLengthSidewallTempJctCap = BSIM4SunitLengthSidewallJctCap * (1.0 + T0);
                BSIM4DunitLengthSidewallTempJctCap = BSIM4DunitLengthSidewallJctCap * (1.0 + T0);
            }
            else
            {
                if (BSIM4SunitLengthSidewallJctCap > 0.0)
                {
                    BSIM4SunitLengthSidewallTempJctCap = 0.0;
                    CircuitWarning.Warning(this, "Temperature effect has caused cjsws to be negative. Cjsws is clamped to zero.");
                }
                if (BSIM4DunitLengthSidewallJctCap > 0.0)
                {
                    BSIM4DunitLengthSidewallTempJctCap = 0.0;
                    CircuitWarning.Warning(this, "Temperature effect has caused cjswd to be negative. Cjswd is clamped to zero.");
                }
            }
            T0 = BSIM4tcjswg * delTemp;
            if (T0 >= -1.0)
            {
                BSIM4SunitLengthGateSidewallTempJctCap = BSIM4SunitLengthGateSidewallJctCap * (1.0 + T0);
                BSIM4DunitLengthGateSidewallTempJctCap = BSIM4DunitLengthGateSidewallJctCap * (1.0 + T0);
            }
            else
            {
                if (BSIM4SunitLengthGateSidewallJctCap > 0.0)
                {
                    BSIM4SunitLengthGateSidewallTempJctCap = 0.0;
                    CircuitWarning.Warning(this, "Temperature effect has caused cjswgs to be negative. Cjswgs is clamped to zero.");
                }
                if (BSIM4DunitLengthGateSidewallJctCap > 0.0)
                {
                    BSIM4DunitLengthGateSidewallTempJctCap = 0.0;
                    CircuitWarning.Warning(this, "Temperature effect has caused cjswgd to be negative. Cjswgd is clamped to zero.");
                }
            }

            BSIM4PhiBS = BSIM4SbulkJctPotential - BSIM4tpb * delTemp;
            if (BSIM4PhiBS < 0.01)
            {
                BSIM4PhiBS = 0.01;
                CircuitWarning.Warning(this, "Temperature effect has caused pbs to be less than 0.01. Pbs is clamped to 0.01.");
            }
            BSIM4PhiBD = BSIM4DbulkJctPotential - BSIM4tpb * delTemp;
            if (BSIM4PhiBD < 0.01)
            {
                BSIM4PhiBD = 0.01;
                CircuitWarning.Warning(this, "Temperature effect has caused pbd to be less than 0.01. Pbd is clamped to 0.01.");
            }

            BSIM4PhiBSWS = BSIM4SsidewallJctPotential - BSIM4tpbsw * delTemp;
            if (BSIM4PhiBSWS <= 0.01)
            {
                BSIM4PhiBSWS = 0.01;
                CircuitWarning.Warning(this, "Temperature effect has caused pbsws to be less than 0.01. Pbsws is clamped to 0.01.");
            }
            BSIM4PhiBSWD = BSIM4DsidewallJctPotential - BSIM4tpbsw * delTemp;
            if (BSIM4PhiBSWD <= 0.01)
            {
                BSIM4PhiBSWD = 0.01;
                CircuitWarning.Warning(this, "Temperature effect has caused pbswd to be less than 0.01. Pbswd is clamped to 0.01.");
            }

            BSIM4PhiBSWGS = BSIM4SGatesidewallJctPotential - BSIM4tpbswg * delTemp;
            if (BSIM4PhiBSWGS <= 0.01)
            {
                BSIM4PhiBSWGS = 0.01;
                CircuitWarning.Warning(this, "Temperature effect has caused pbswgs to be less than 0.01. Pbswgs is clamped to 0.01.");
            }
            BSIM4PhiBSWGD = BSIM4DGatesidewallJctPotential - BSIM4tpbswg * delTemp;
            if (BSIM4PhiBSWGD <= 0.01)
            {
                BSIM4PhiBSWGD = 0.01;
                CircuitWarning.Warning(this, "Temperature effect has caused pbswgd to be less than 0.01. Pbswgd is clamped to 0.01.");
            } /* End of junction capacitance */

            if (BSIM4ijthdfwd <= 0.0)
            {
                BSIM4ijthdfwd.Value = 0.0;
                CircuitWarning.Warning(this, $"Ijthdfwd reset to {BSIM4ijthdfwd}.");
            }
            if (BSIM4ijthsfwd <= 0.0)
            {
                BSIM4ijthsfwd.Value = 0.0;
                CircuitWarning.Warning(this, $"Ijthsfwd reset to {BSIM4ijthsfwd}.");
            }
            if (BSIM4ijthdrev <= 0.0)
            {
                BSIM4ijthdrev.Value = 0.0;
                CircuitWarning.Warning(this, $"Ijthdrev reset to {BSIM4ijthdrev}.");
            }
            if (BSIM4ijthsrev <= 0.0)
            {
                BSIM4ijthsrev.Value = 0.0;
                CircuitWarning.Warning(this, $"Ijthsrev reset to {BSIM4ijthsrev}.");
            }

            if ((BSIM4xjbvd <= 0.0) && (BSIM4dioMod.Value == 2))
            {
                BSIM4xjbvd.Value = 0.0;
                CircuitWarning.Warning(this, $"Xjbvd reset to {BSIM4xjbvd}.");
            }
            else if ((BSIM4xjbvd < 0.0) && (BSIM4dioMod.Value == 0))
            {
                BSIM4xjbvd.Value = 0.0;
                CircuitWarning.Warning(this, $"Xjbvd reset to {BSIM4xjbvd}.");
            }

            if (BSIM4bvd <= 0.0)
            /* 4.6.2 */
            {
                BSIM4bvd.Value = 0.0;
                CircuitWarning.Warning(this, $"BVD reset to {BSIM4bvd}.");
            }

            if ((BSIM4xjbvs <= 0.0) && (BSIM4dioMod.Value == 2))
            {
                BSIM4xjbvs.Value = 0.0;
                CircuitWarning.Warning(this, $"Xjbvs reset to {BSIM4xjbvs}.");
            }
            else if ((BSIM4xjbvs < 0.0) && (BSIM4dioMod.Value == 0))
            {
                BSIM4xjbvs.Value = 0.0;
                CircuitWarning.Warning(this, $"Xjbvs reset to {BSIM4xjbvs}.");
            }

            if (BSIM4bvs <= 0.0)
            {
                BSIM4bvs.Value = 0.0;
                CircuitWarning.Warning(this, $"BVS reset to {BSIM4bvs}.");
            }
        }
    }
}
