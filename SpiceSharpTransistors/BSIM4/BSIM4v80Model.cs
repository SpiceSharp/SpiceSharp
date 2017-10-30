using System;
using System.Collections.Generic;
using SpiceSharp.Circuits;
using SpiceSharp.Diagnostics;
using SpiceSharp.Parameters;
using SpiceSharp.Components.Transistors;

namespace SpiceSharp.Components
{
    /// <summary>
    /// The BSIM4v80 model
    /// </summary>
    public class BSIM4v80Model : CircuitModel<BSIM4v80Model>
    {
        /// <summary>
        /// Register default behaviours
        /// </summary>
        static BSIM4v80Model()
        {
            Behaviors.Behaviors.RegisterBehavior(typeof(BSIM4v80Model), typeof(ComponentBehaviors.BSIM4v80ModelTemperatureBehavior));
        }

        /// <summary>
        /// Size-dependent parameters
        /// </summary>
        public Dictionary<Tuple<double, double, double>, BSIM4SizeDependParam> Sizes { get; } = new Dictionary<Tuple<double, double, double>, BSIM4SizeDependParam>();

        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("mobmod"), SpiceInfo("Mobility model selector")]
        public Parameter BSIM4mobMod { get; } = new Parameter();
        [SpiceName("binunit"), SpiceInfo("Bin  unit  selector")]
        public Parameter BSIM4binUnit { get; } = new Parameter(1);
        [SpiceName("paramchk"), SpiceInfo("Model parameter checking selector")]
        public Parameter BSIM4paramChk { get; } = new Parameter(1);
        [SpiceName("cvchargemod"), SpiceInfo("Capacitance Charge model selector")]
        public Parameter BSIM4cvchargeMod { get; } = new Parameter();
        [SpiceName("capmod"), SpiceInfo("Capacitance model selector")]
        public Parameter BSIM4capMod { get; } = new Parameter(2);
        [SpiceName("diomod"), SpiceInfo("Diode IV model selector")]
        public Parameter BSIM4dioMod { get; } = new Parameter(1);
        [SpiceName("rdsmod"), SpiceInfo("Bias-dependent S/D resistance model selector")]
        public Parameter BSIM4rdsMod { get; } = new Parameter();
        [SpiceName("trnqsmod"), SpiceInfo("Transient NQS model selector")]
        public Parameter BSIM4trnqsMod { get; } = new Parameter();
        [SpiceName("acnqsmod"), SpiceInfo("AC NQS model selector")]
        public Parameter BSIM4acnqsMod { get; } = new Parameter();
        [SpiceName("rbodymod"), SpiceInfo("Distributed body R model selector")]
        public Parameter BSIM4rbodyMod { get; } = new Parameter();
        [SpiceName("rgatemod"), SpiceInfo("Gate R model selector")]
        public Parameter BSIM4rgateMod { get; } = new Parameter();
        [SpiceName("permod"), SpiceInfo("Pd and Ps model selector")]
        public Parameter BSIM4perMod { get; } = new Parameter(1);
        [SpiceName("geomod"), SpiceInfo("Geometry dependent parasitics model selector")]
        public Parameter BSIM4geoMod { get; } = new Parameter();
        [SpiceName("fnoimod"), SpiceInfo("Flicker noise model selector")]
        public Parameter BSIM4fnoiMod { get; } = new Parameter(1);
        [SpiceName("tnoimod"), SpiceInfo("Thermal noise model selector")]
        public Parameter BSIM4tnoiMod { get; } = new Parameter();
        [SpiceName("mtrlmod"), SpiceInfo("parameter for non-silicon substrate or metal gate selector")]
        public Parameter BSIM4mtrlMod { get; } = new Parameter();
        [SpiceName("mtrlcompatmod"), SpiceInfo("New Material Mod backward compatibility selector")]
        public Parameter BSIM4mtrlCompatMod { get; } = new Parameter();
        [SpiceName("gidlmod"), SpiceInfo("parameter for GIDL selector")]
        public Parameter BSIM4gidlMod { get; } = new Parameter();
        [SpiceName("igcmod"), SpiceInfo("Gate-to-channel Ig model selector")]
        public Parameter BSIM4igcMod { get; } = new Parameter();
        [SpiceName("igbmod"), SpiceInfo("Gate-to-body Ig model selector")]
        public Parameter BSIM4igbMod { get; } = new Parameter();
        [SpiceName("tempmod"), SpiceInfo("Temperature model selector")]
        public Parameter BSIM4tempMod { get; } = new Parameter();
        [SpiceName("version"), SpiceInfo("parameter for model version")]
        public Parameter BSIM4version { get; } = new Parameter(4.80);
        [SpiceName("toxref"), SpiceInfo("Target tox value")]
        public Parameter BSIM4toxref { get; } = new Parameter(30.0e-10);
        [SpiceName("eot"), SpiceInfo("Equivalent gate oxide thickness in meters")]
        public Parameter BSIM4eot { get; } = new Parameter(15.0e-10);
        [SpiceName("vddeot"), SpiceInfo("Voltage for extraction of Equivalent gate oxide thickness")]
        public Parameter BSIM4vddeot { get; } = new Parameter();
        [SpiceName("tempeot"), SpiceInfo(" Temperature for extraction of EOT")]
        public Parameter BSIM4tempeot { get; } = new Parameter(300.15);
        [SpiceName("leffeot"), SpiceInfo(" Effective length for extraction of EOT")]
        public Parameter BSIM4leffeot { get; } = new Parameter(1);
        [SpiceName("weffeot"), SpiceInfo("Effective width for extraction of EOT")]
        public Parameter BSIM4weffeot { get; } = new Parameter(10);
        [SpiceName("ados"), SpiceInfo("Charge centroid parameter")]
        public Parameter BSIM4ados { get; } = new Parameter(1.0);
        [SpiceName("bdos"), SpiceInfo("Charge centroid parameter")]
        public Parameter BSIM4bdos { get; } = new Parameter(1.0);
        [SpiceName("toxe"), SpiceInfo("Electrical gate oxide thickness in meters")]
        public Parameter BSIM4toxe { get; } = new Parameter(30.0e-10);
        [SpiceName("toxp"), SpiceInfo("Physical gate oxide thickness in meters")]
        public Parameter BSIM4toxp { get; } = new Parameter();
        [SpiceName("toxm"), SpiceInfo("Gate oxide thickness at which parameters are extracted")]
        public Parameter BSIM4toxm { get; } = new Parameter();
        [SpiceName("dtox"), SpiceInfo("Defined as (toxe - toxp) ")]
        public Parameter BSIM4dtox { get; } = new Parameter();
        [SpiceName("epsrox"), SpiceInfo("Dielectric constant of the gate oxide relative to vacuum")]
        public Parameter BSIM4epsrox { get; } = new Parameter(3.9);
        [SpiceName("cdsc"), SpiceInfo("Drain/Source and channel coupling capacitance")]
        public Parameter BSIM4cdsc { get; } = new Parameter(2.4e-4);
        [SpiceName("cdscb"), SpiceInfo("Body-bias dependence of cdsc")]
        public Parameter BSIM4cdscb { get; } = new Parameter();
        [SpiceName("cdscd"), SpiceInfo("Drain-bias dependence of cdsc")]
        public Parameter BSIM4cdscd { get; } = new Parameter();
        [SpiceName("cit"), SpiceInfo("Interface state capacitance")]
        public Parameter BSIM4cit { get; } = new Parameter();
        [SpiceName("nfactor"), SpiceInfo("Subthreshold swing Coefficient")]
        public Parameter BSIM4nfactor { get; } = new Parameter(1.0);
        [SpiceName("xj"), SpiceInfo("Junction depth in meters")]
        public Parameter BSIM4xj { get; } = new Parameter(.15e-6);
        [SpiceName("vsat"), SpiceInfo("Saturation velocity at tnom")]
        public Parameter BSIM4vsat { get; } = new Parameter(8.0e4);
        [SpiceName("a0"), SpiceInfo("Non-uniform depletion width effect coefficient.")]
        public Parameter BSIM4a0 { get; } = new Parameter(1.0);
        [SpiceName("ags"), SpiceInfo("Gate bias  coefficient of Abulk.")]
        public Parameter BSIM4ags { get; } = new Parameter();
        [SpiceName("a1"), SpiceInfo("Non-saturation effect coefficient")]
        public Parameter BSIM4a1 { get; } = new Parameter();
        [SpiceName("a2"), SpiceInfo("Non-saturation effect coefficient")]
        public Parameter BSIM4a2 { get; } = new Parameter(1.0);
        [SpiceName("at"), SpiceInfo("Temperature coefficient of vsat")]
        public Parameter BSIM4at { get; } = new Parameter(3.3e4);
        [SpiceName("keta"), SpiceInfo("Body-bias coefficient of non-uniform depletion width effect.")]
        public Parameter BSIM4keta { get; } = new Parameter(-0.047);
        [SpiceName("nsub"), SpiceInfo("Substrate doping concentration")]
        public Parameter BSIM4nsub { get; } = new Parameter(6.0e16);
        [SpiceName("phig"), SpiceInfo("Work function of gate")]
        public Parameter BSIM4phig { get; } = new Parameter(4.05);
        [SpiceName("epsrgate"), SpiceInfo("Dielectric constant of gate relative to vacuum")]
        public Parameter BSIM4epsrgate { get; } = new Parameter(11.7);
        [SpiceName("easub"), SpiceInfo("Electron affinity of substrate")]
        public Parameter BSIM4easub { get; } = new Parameter(4.05);
        [SpiceName("epsrsub"), SpiceInfo("Dielectric constant of substrate relative to vacuum")]
        public Parameter BSIM4epsrsub { get; } = new Parameter(11.7);
        [SpiceName("ni0sub"), SpiceInfo("Intrinsic carrier concentration of substrate at 300.15K")]
        public Parameter BSIM4ni0sub { get; } = new Parameter(1.45e10);
        [SpiceName("bg0sub"), SpiceInfo("Band-gap of substrate at T=0K")]
        public Parameter BSIM4bg0sub { get; } = new Parameter(1.16);
        [SpiceName("tbgasub"), SpiceInfo("First parameter of band-gap change due to temperature")]
        public Parameter BSIM4tbgasub { get; } = new Parameter(7.02e-4);
        [SpiceName("tbgbsub"), SpiceInfo("Second parameter of band-gap change due to temperature")]
        public Parameter BSIM4tbgbsub { get; } = new Parameter(1108.0);
        [SpiceName("ndep"), SpiceInfo("Channel doping concentration at the depletion edge")]
        public double BSIM4_NDEP
        {
            get => BSIM4ndep;
            set
            {
                BSIM4ndep.Set(value);
                if (BSIM4ndep > 1.0e20)
                    BSIM4ndep.Value *= 1.0e-6;
            }
        }
        public Parameter BSIM4ndep { get; } = new Parameter(1.7e17);
        [SpiceName("nsd"), SpiceInfo("S/D doping concentration")]
        public double BSIM4_NSD
        {
            get => BSIM4nsd;
            set
            {
                BSIM4nsd.Set(value);
                if (BSIM4nsd > 1.0e23)
                    BSIM4nsd.Value *= 1.0e-6;
            }
        }
        public Parameter BSIM4nsd { get; } = new Parameter(1.0e20);
        [SpiceName("ngate"), SpiceInfo("Poly-gate doping concentration")]
        public double BSIM4_NGATE
        {
            get => BSIM4ngate;
            set
            {
                BSIM4ngate.Set(value);
                if (BSIM4ngate > 1.0e23)
                    BSIM4ngate.Value *= 1.0e-6;
            }
        }
        public Parameter BSIM4ngate { get; } = new Parameter();
        [SpiceName("gamma1"), SpiceInfo("Vth body coefficient")]
        public Parameter BSIM4gamma1 { get; } = new Parameter();
        [SpiceName("gamma2"), SpiceInfo("Vth body coefficient")]
        public Parameter BSIM4gamma2 { get; } = new Parameter();
        [SpiceName("vbx"), SpiceInfo("Vth transition body Voltage")]
        public Parameter BSIM4vbx { get; } = new Parameter();
        [SpiceName("vbm"), SpiceInfo("Maximum body voltage")]
        public Parameter BSIM4vbm { get; } = new Parameter(-3.0);
        [SpiceName("xt"), SpiceInfo("Doping depth")]
        public Parameter BSIM4xt { get; } = new Parameter(1.55e-7);
        [SpiceName("k1"), SpiceInfo("Bulk effect coefficient 1")]
        public Parameter BSIM4k1 { get; } = new Parameter();
        [SpiceName("kt1"), SpiceInfo("Temperature coefficient of Vth")]
        public Parameter BSIM4kt1 { get; } = new Parameter(-0.11);
        [SpiceName("kt1l"), SpiceInfo("Temperature coefficient of Vth")]
        public Parameter BSIM4kt1l { get; } = new Parameter();
        [SpiceName("kt2"), SpiceInfo("Body-coefficient of kt1")]
        public Parameter BSIM4kt2 { get; } = new Parameter(0.022);
        [SpiceName("k2"), SpiceInfo("Bulk effect coefficient 2")]
        public Parameter BSIM4k2 { get; } = new Parameter();
        [SpiceName("k3"), SpiceInfo("Narrow width effect coefficient")]
        public Parameter BSIM4k3 { get; } = new Parameter(80.0);
        [SpiceName("k3b"), SpiceInfo("Body effect coefficient of k3")]
        public Parameter BSIM4k3b { get; } = new Parameter();
        [SpiceName("lpe0"), SpiceInfo("Equivalent length of pocket region at zero bias")]
        public Parameter BSIM4lpe0 { get; } = new Parameter(1.74e-7);
        [SpiceName("lpeb"), SpiceInfo("Equivalent length of pocket region accounting for body bias")]
        public Parameter BSIM4lpeb { get; } = new Parameter();
        [SpiceName("dvtp0"), SpiceInfo("First parameter for Vth shift due to pocket")]
        public Parameter BSIM4dvtp0 { get; } = new Parameter();
        [SpiceName("dvtp1"), SpiceInfo("Second parameter for Vth shift due to pocket")]
        public Parameter BSIM4dvtp1 { get; } = new Parameter();
        [SpiceName("dvtp2"), SpiceInfo("3rd parameter for Vth shift due to pocket")]
        public Parameter BSIM4dvtp2 { get; } = new Parameter();
        [SpiceName("dvtp3"), SpiceInfo("4th parameter for Vth shift due to pocket")]
        public Parameter BSIM4dvtp3 { get; } = new Parameter();
        [SpiceName("dvtp4"), SpiceInfo("5th parameter for Vth shift due to pocket")]
        public Parameter BSIM4dvtp4 { get; } = new Parameter();
        [SpiceName("dvtp5"), SpiceInfo("6th parameter for Vth shift due to pocket")]
        public Parameter BSIM4dvtp5 { get; } = new Parameter();
        [SpiceName("w0"), SpiceInfo("Narrow width effect parameter")]
        public Parameter BSIM4w0 { get; } = new Parameter(2.5e-6);
        [SpiceName("dvt0"), SpiceInfo("Short channel effect coeff. 0")]
        public Parameter BSIM4dvt0 { get; } = new Parameter(2.2);
        [SpiceName("dvt1"), SpiceInfo("Short channel effect coeff. 1")]
        public Parameter BSIM4dvt1 { get; } = new Parameter(0.53);
        [SpiceName("dvt2"), SpiceInfo("Short channel effect coeff. 2")]
        public Parameter BSIM4dvt2 { get; } = new Parameter(-0.032);
        [SpiceName("dvt0w"), SpiceInfo("Narrow Width coeff. 0")]
        public Parameter BSIM4dvt0w { get; } = new Parameter();
        [SpiceName("dvt1w"), SpiceInfo("Narrow Width effect coeff. 1")]
        public Parameter BSIM4dvt1w { get; } = new Parameter(5.3e6);
        [SpiceName("dvt2w"), SpiceInfo("Narrow Width effect coeff. 2")]
        public Parameter BSIM4dvt2w { get; } = new Parameter(-0.032);
        [SpiceName("drout"), SpiceInfo("DIBL coefficient of output resistance")]
        public Parameter BSIM4drout { get; } = new Parameter(0.56);
        [SpiceName("dsub"), SpiceInfo("DIBL coefficient in the subthreshold region")]
        public Parameter BSIM4dsub { get; } = new Parameter();
        [SpiceName("vth0"), SpiceName("vtho"), SpiceInfo("Threshold voltage")]
        public Parameter BSIM4vth0 { get; } = new Parameter();
        [SpiceName("eu"), SpiceInfo("Mobility exponent")]
        public Parameter BSIM4eu { get; } = new Parameter();
        [SpiceName("ucs"), SpiceInfo("Colombic scattering exponent")]
        public Parameter BSIM4ucs { get; } = new Parameter();
        [SpiceName("ua"), SpiceInfo("Linear gate dependence of mobility")]
        public Parameter BSIM4ua { get; } = new Parameter();
        [SpiceName("ua1"), SpiceInfo("Temperature coefficient of ua")]
        public Parameter BSIM4ua1 { get; } = new Parameter(1.0e-9);
        [SpiceName("ub"), SpiceInfo("Quadratic gate dependence of mobility")]
        public Parameter BSIM4ub { get; } = new Parameter(1.0e-19);
        [SpiceName("ub1"), SpiceInfo("Temperature coefficient of ub")]
        public Parameter BSIM4ub1 { get; } = new Parameter(-1.0e-18);
        [SpiceName("uc"), SpiceInfo("Body-bias dependence of mobility")]
        public Parameter BSIM4uc { get; } = new Parameter();
        [SpiceName("uc1"), SpiceInfo("Temperature coefficient of uc")]
        public Parameter BSIM4uc1 { get; } = new Parameter();
        [SpiceName("u0"), SpiceInfo("Low-field mobility at Tnom")]
        public Parameter BSIM4u0 { get; } = new Parameter();
        [SpiceName("ute"), SpiceInfo("Temperature coefficient of mobility")]
        public Parameter BSIM4ute { get; } = new Parameter(-1.5);
        [SpiceName("ucste"), SpiceInfo("Temperature coefficient of colombic mobility")]
        public Parameter BSIM4ucste { get; } = new Parameter(-4.775e-3);
        [SpiceName("ud"), SpiceInfo("Coulomb scattering factor of mobility")]
        public Parameter BSIM4ud { get; } = new Parameter();
        [SpiceName("ud1"), SpiceInfo("Temperature coefficient of ud")]
        public Parameter BSIM4ud1 { get; } = new Parameter();
        [SpiceName("up"), SpiceInfo("Channel length linear factor of mobility")]
        public Parameter BSIM4up { get; } = new Parameter();
        [SpiceName("lp"), SpiceInfo("Channel length exponential factor of mobility")]
        public Parameter BSIM4lp { get; } = new Parameter(1.0e-8);
        [SpiceName("lud"), SpiceInfo("Length dependence of ud")]
        public Parameter BSIM4lud { get; } = new Parameter();
        [SpiceName("lud1"), SpiceInfo("Length dependence of ud1")]
        public Parameter BSIM4lud1 { get; } = new Parameter();
        [SpiceName("lup"), SpiceInfo("Length dependence of up")]
        public Parameter BSIM4lup { get; } = new Parameter();
        [SpiceName("llp"), SpiceInfo("Length dependence of lp")]
        public Parameter BSIM4llp { get; } = new Parameter();
        [SpiceName("wud"), SpiceInfo("Width dependence of ud")]
        public Parameter BSIM4wud { get; } = new Parameter();
        [SpiceName("wud1"), SpiceInfo("Width dependence of ud1")]
        public Parameter BSIM4wud1 { get; } = new Parameter();
        [SpiceName("wup"), SpiceInfo("Width dependence of up")]
        public Parameter BSIM4wup { get; } = new Parameter();
        [SpiceName("wlp"), SpiceInfo("Width dependence of lp")]
        public Parameter BSIM4wlp { get; } = new Parameter();
        [SpiceName("pud"), SpiceInfo("Cross-term dependence of ud")]
        public Parameter BSIM4pud { get; } = new Parameter();
        [SpiceName("pud1"), SpiceInfo("Cross-term dependence of ud1")]
        public Parameter BSIM4pud1 { get; } = new Parameter();
        [SpiceName("pup"), SpiceInfo("Cross-term dependence of up")]
        public Parameter BSIM4pup { get; } = new Parameter();
        [SpiceName("plp"), SpiceInfo("Cross-term dependence of lp")]
        public Parameter BSIM4plp { get; } = new Parameter();
        [SpiceName("voff"), SpiceInfo("Threshold voltage offset")]
        public Parameter BSIM4voff { get; } = new Parameter(-0.08);
        [SpiceName("tvoff"), SpiceInfo("Temperature parameter for voff")]
        public Parameter BSIM4tvoff { get; } = new Parameter();
        [SpiceName("tnfactor"), SpiceInfo("Temperature parameter for nfactor")]
        public Parameter BSIM4tnfactor { get; } = new Parameter();
        [SpiceName("teta0"), SpiceInfo("Temperature parameter for eta0")]
        public Parameter BSIM4teta0 { get; } = new Parameter();
        [SpiceName("tvoffcv"), SpiceInfo("Temperature parameter for tvoffcv")]
        public Parameter BSIM4tvoffcv { get; } = new Parameter();
        [SpiceName("voffl"), SpiceInfo("Length dependence parameter for Vth offset")]
        public Parameter BSIM4voffl { get; } = new Parameter();
        [SpiceName("voffcvl"), SpiceInfo("Length dependence parameter for Vth offset in CV")]
        public Parameter BSIM4voffcvl { get; } = new Parameter();
        [SpiceName("minv"), SpiceInfo("Fitting parameter for moderate inversion in Vgsteff")]
        public Parameter BSIM4minv { get; } = new Parameter();
        [SpiceName("minvcv"), SpiceInfo("Fitting parameter for moderate inversion in Vgsteffcv")]
        public Parameter BSIM4minvcv { get; } = new Parameter();
        [SpiceName("fprout"), SpiceInfo("Rout degradation coefficient for pocket devices")]
        public Parameter BSIM4fprout { get; } = new Parameter();
        [SpiceName("pdits"), SpiceInfo("Coefficient for drain-induced Vth shifts")]
        public Parameter BSIM4pdits { get; } = new Parameter();
        [SpiceName("pditsd"), SpiceInfo("Vds dependence of drain-induced Vth shifts")]
        public Parameter BSIM4pditsd { get; } = new Parameter();
        [SpiceName("pditsl"), SpiceInfo("Length dependence of drain-induced Vth shifts")]
        public Parameter BSIM4pditsl { get; } = new Parameter();
        [SpiceName("delta"), SpiceInfo("Effective Vds parameter")]
        public Parameter BSIM4delta { get; } = new Parameter(0.01);
        [SpiceName("rdsw"), SpiceInfo("Source-drain resistance per width")]
        public Parameter BSIM4rdsw { get; } = new Parameter(200.0);
        [SpiceName("rdswmin"), SpiceInfo("Source-drain resistance per width at high Vg")]
        public Parameter BSIM4rdswmin { get; } = new Parameter();
        [SpiceName("rdwmin"), SpiceInfo("Drain resistance per width at high Vg")]
        public Parameter BSIM4rdwmin { get; } = new Parameter();
        [SpiceName("rswmin"), SpiceInfo("Source resistance per width at high Vg")]
        public Parameter BSIM4rswmin { get; } = new Parameter();
        [SpiceName("rdw"), SpiceInfo("Drain resistance per width")]
        public Parameter BSIM4rdw { get; } = new Parameter(100.0);
        [SpiceName("rsw"), SpiceInfo("Source resistance per width")]
        public Parameter BSIM4rsw { get; } = new Parameter(100.0);
        [SpiceName("prwg"), SpiceInfo("Gate-bias effect on parasitic resistance ")]
        public Parameter BSIM4prwg { get; } = new Parameter(1.0);
        [SpiceName("prwb"), SpiceInfo("Body-effect on parasitic resistance ")]
        public Parameter BSIM4prwb { get; } = new Parameter();
        [SpiceName("prt"), SpiceInfo("Temperature coefficient of parasitic resistance ")]
        public Parameter BSIM4prt { get; } = new Parameter();
        [SpiceName("eta0"), SpiceInfo("Subthreshold region DIBL coefficient")]
        public Parameter BSIM4eta0 { get; } = new Parameter(0.08);
        [SpiceName("etab"), SpiceInfo("Subthreshold region DIBL coefficient")]
        public Parameter BSIM4etab { get; } = new Parameter(-0.07);
        [SpiceName("pclm"), SpiceInfo("Channel length modulation Coefficient")]
        public Parameter BSIM4pclm { get; } = new Parameter(1.3);
        [SpiceName("pdiblc1"), SpiceInfo("Drain-induced barrier lowering coefficient")]
        public Parameter BSIM4pdibl1 { get; } = new Parameter(0.39);
        [SpiceName("pdiblc2"), SpiceInfo("Drain-induced barrier lowering coefficient")]
        public Parameter BSIM4pdibl2 { get; } = new Parameter(0.0086);
        [SpiceName("pdiblcb"), SpiceInfo("Body-effect on drain-induced barrier lowering")]
        public Parameter BSIM4pdiblb { get; } = new Parameter();
        [SpiceName("pscbe1"), SpiceInfo("Substrate current body-effect coefficient")]
        public Parameter BSIM4pscbe1 { get; } = new Parameter(4.24e8);
        [SpiceName("pscbe2"), SpiceInfo("Substrate current body-effect coefficient")]
        public Parameter BSIM4pscbe2 { get; } = new Parameter(1.0e-5);
        [SpiceName("pvag"), SpiceInfo("Gate dependence of output resistance parameter")]
        public Parameter BSIM4pvag { get; } = new Parameter();
        [SpiceName("wr"), SpiceInfo("Width dependence of rds")]
        public Parameter BSIM4wr { get; } = new Parameter(1.0);
        [SpiceName("dwg"), SpiceInfo("Width reduction parameter")]
        public Parameter BSIM4dwg { get; } = new Parameter();
        [SpiceName("dwb"), SpiceInfo("Width reduction parameter")]
        public Parameter BSIM4dwb { get; } = new Parameter();
        [SpiceName("b0"), SpiceInfo("Abulk narrow width parameter")]
        public Parameter BSIM4b0 { get; } = new Parameter();
        [SpiceName("b1"), SpiceInfo("Abulk narrow width parameter")]
        public Parameter BSIM4b1 { get; } = new Parameter();
        [SpiceName("alpha0"), SpiceInfo("substrate current model parameter")]
        public Parameter BSIM4alpha0 { get; } = new Parameter();
        [SpiceName("alpha1"), SpiceInfo("substrate current model parameter")]
        public Parameter BSIM4alpha1 { get; } = new Parameter();
        [SpiceName("phin"), SpiceInfo("Adjusting parameter for surface potential due to non-uniform vertical doping")]
        public Parameter BSIM4phin { get; } = new Parameter();
        [SpiceName("agidl"), SpiceInfo("Pre-exponential constant for GIDL")]
        public Parameter BSIM4agidl { get; } = new Parameter();
        [SpiceName("bgidl"), SpiceInfo("Exponential constant for GIDL")]
        public Parameter BSIM4bgidl { get; } = new Parameter(2.3e9);
        [SpiceName("cgidl"), SpiceInfo("Parameter for body-bias dependence of GIDL")]
        public Parameter BSIM4cgidl { get; } = new Parameter(0.5);
        [SpiceName("egidl"), SpiceInfo("Fitting parameter for Bandbending")]
        public Parameter BSIM4egidl { get; } = new Parameter(0.8);
        [SpiceName("fgidl"), SpiceInfo("GIDL vb parameter")]
        public Parameter BSIM4fgidl { get; } = new Parameter();
        [SpiceName("kgidl"), SpiceInfo("GIDL vb parameter")]
        public Parameter BSIM4kgidl { get; } = new Parameter();
        [SpiceName("rgidl"), SpiceInfo("GIDL vg parameter")]
        public Parameter BSIM4rgidl { get; } = new Parameter();
        [SpiceName("agisl"), SpiceInfo("Pre-exponential constant for GISL")]
        public Parameter BSIM4agisl { get; } = new Parameter();
        [SpiceName("bgisl"), SpiceInfo("Exponential constant for GISL")]
        public Parameter BSIM4bgisl { get; } = new Parameter();
        [SpiceName("cgisl"), SpiceInfo("Parameter for body-bias dependence of GISL")]
        public Parameter BSIM4cgisl { get; } = new Parameter();
        [SpiceName("egisl"), SpiceInfo("Fitting parameter for Bandbending")]
        public Parameter BSIM4egisl { get; } = new Parameter();
        [SpiceName("fgisl"), SpiceInfo("GISL vb parameter")]
        public Parameter BSIM4fgisl { get; } = new Parameter();
        [SpiceName("kgisl"), SpiceInfo("GISL vb parameter")]
        public Parameter BSIM4kgisl { get; } = new Parameter();
        [SpiceName("rgisl"), SpiceInfo("GISL vg parameter")]
        public Parameter BSIM4rgisl { get; } = new Parameter();
        [SpiceName("aigc"), SpiceInfo("Parameter for Igc")]
        public Parameter BSIM4aigc { get; } = new Parameter();
        [SpiceName("bigc"), SpiceInfo("Parameter for Igc")]
        public Parameter BSIM4bigc { get; } = new Parameter();
        [SpiceName("cigc"), SpiceInfo("Parameter for Igc")]
        public Parameter BSIM4cigc { get; } = new Parameter();
        [SpiceName("aigsd"), SpiceInfo("Parameter for Igs,d")]
        public Parameter BSIM4aigsd { get; } = new Parameter();
        [SpiceName("bigsd"), SpiceInfo("Parameter for Igs,d")]
        public Parameter BSIM4bigsd { get; } = new Parameter();
        [SpiceName("cigsd"), SpiceInfo("Parameter for Igs,d")]
        public Parameter BSIM4cigsd { get; } = new Parameter();
        [SpiceName("aigs"), SpiceInfo("Parameter for Igs")]
        public Parameter BSIM4aigs { get; } = new Parameter();
        [SpiceName("bigs"), SpiceInfo("Parameter for Igs")]
        public Parameter BSIM4bigs { get; } = new Parameter();
        [SpiceName("cigs"), SpiceInfo("Parameter for Igs")]
        public Parameter BSIM4cigs { get; } = new Parameter();
        [SpiceName("aigd"), SpiceInfo("Parameter for Igd")]
        public Parameter BSIM4aigd { get; } = new Parameter();
        [SpiceName("bigd"), SpiceInfo("Parameter for Igd")]
        public Parameter BSIM4bigd { get; } = new Parameter();
        [SpiceName("cigd"), SpiceInfo("Parameter for Igd")]
        public Parameter BSIM4cigd { get; } = new Parameter();
        [SpiceName("aigbacc"), SpiceInfo("Parameter for Igb")]
        public Parameter BSIM4aigbacc { get; } = new Parameter(1.36e-2);
        [SpiceName("bigbacc"), SpiceInfo("Parameter for Igb")]
        public Parameter BSIM4bigbacc { get; } = new Parameter(1.71e-3);
        [SpiceName("cigbacc"), SpiceInfo("Parameter for Igb")]
        public Parameter BSIM4cigbacc { get; } = new Parameter(0.075);
        [SpiceName("aigbinv"), SpiceInfo("Parameter for Igb")]
        public Parameter BSIM4aigbinv { get; } = new Parameter(1.11e-2);
        [SpiceName("bigbinv"), SpiceInfo("Parameter for Igb")]
        public Parameter BSIM4bigbinv { get; } = new Parameter(9.49e-4);
        [SpiceName("cigbinv"), SpiceInfo("Parameter for Igb")]
        public Parameter BSIM4cigbinv { get; } = new Parameter(0.006);
        [SpiceName("nigc"), SpiceInfo("Parameter for Igc slope")]
        public Parameter BSIM4nigc { get; } = new Parameter(1.0);
        [SpiceName("nigbinv"), SpiceInfo("Parameter for Igbinv slope")]
        public Parameter BSIM4nigbinv { get; } = new Parameter(3.0);
        [SpiceName("nigbacc"), SpiceInfo("Parameter for Igbacc slope")]
        public Parameter BSIM4nigbacc { get; } = new Parameter(1.0);
        [SpiceName("ntox"), SpiceInfo("Exponent for Tox ratio")]
        public Parameter BSIM4ntox { get; } = new Parameter(1.0);
        [SpiceName("eigbinv"), SpiceInfo("Parameter for the Si bandgap for Igbinv")]
        public Parameter BSIM4eigbinv { get; } = new Parameter(1.1);
        [SpiceName("pigcd"), SpiceInfo("Parameter for Igc partition")]
        public Parameter BSIM4pigcd { get; } = new Parameter(1.0);
        [SpiceName("poxedge"), SpiceInfo("Factor for the gate edge Tox")]
        public Parameter BSIM4poxedge { get; } = new Parameter(1.0);
        [SpiceName("xrcrg1"), SpiceInfo("First fitting parameter the bias-dependent Rg")]
        public Parameter BSIM4xrcrg1 { get; } = new Parameter(12.0);
        [SpiceName("xrcrg2"), SpiceInfo("Second fitting parameter the bias-dependent Rg")]
        public Parameter BSIM4xrcrg2 { get; } = new Parameter(1.0);
        [SpiceName("lambda"), SpiceInfo(" Velocity overshoot parameter")]
        public Parameter BSIM4lambda { get; } = new Parameter();
        [SpiceName("vtl"), SpiceInfo(" thermal velocity")]
        public Parameter BSIM4vtl { get; } = new Parameter(2.0e5);
        [SpiceName("xn"), SpiceInfo(" back scattering parameter")]
        public Parameter BSIM4xn { get; } = new Parameter(3.0);
        [SpiceName("lc"), SpiceInfo(" back scattering parameter")]
        public Parameter BSIM4lc { get; } = new Parameter(5.0e-9);
        [SpiceName("tnoia"), SpiceInfo("Thermal noise parameter")]
        public Parameter BSIM4tnoia { get; } = new Parameter(1.5);
        [SpiceName("tnoib"), SpiceInfo("Thermal noise parameter")]
        public Parameter BSIM4tnoib { get; } = new Parameter(3.5);
        [SpiceName("tnoic"), SpiceInfo("Thermal noise parameter")]
        public Parameter BSIM4tnoic { get; } = new Parameter();
        [SpiceName("rnoia"), SpiceInfo("Thermal noise coefficient")]
        public Parameter BSIM4rnoia { get; } = new Parameter(0.577);
        [SpiceName("rnoib"), SpiceInfo("Thermal noise coefficient")]
        public Parameter BSIM4rnoib { get; } = new Parameter(0.5164);
        [SpiceName("rnoic"), SpiceInfo("Thermal noise coefficient")]
        public Parameter BSIM4rnoic { get; } = new Parameter(0.395);
        [SpiceName("ntnoi"), SpiceInfo("Thermal noise parameter")]
        public Parameter BSIM4ntnoi { get; } = new Parameter(1.0);
        [SpiceName("vfbsdoff"), SpiceInfo("S/D flatband voltage offset")]
        public Parameter BSIM4vfbsdoff { get; } = new Parameter();
        [SpiceName("tvfbsdoff"), SpiceInfo("Temperature parameter for vfbsdoff")]
        public Parameter BSIM4tvfbsdoff { get; } = new Parameter();
        [SpiceName("lintnoi"), SpiceInfo("lint offset for noise calculation")]
        public Parameter BSIM4lintnoi { get; } = new Parameter();
        [SpiceName("saref"), SpiceInfo("Reference distance between OD edge to poly of one side")]
        public Parameter BSIM4saref { get; } = new Parameter(1e-6);
        [SpiceName("sbref"), SpiceInfo("Reference distance between OD edge to poly of the other side")]
        public Parameter BSIM4sbref { get; } = new Parameter(1e-6);
        [SpiceName("wlod"), SpiceInfo("Width parameter for stress effect")]
        public Parameter BSIM4wlod { get; } = new Parameter();
        [SpiceName("ku0"), SpiceInfo("Mobility degradation/enhancement coefficient for LOD")]
        public Parameter BSIM4ku0 { get; } = new Parameter();
        [SpiceName("kvsat"), SpiceInfo("Saturation velocity degradation/enhancement parameter for LOD")]
        public Parameter BSIM4kvsat { get; } = new Parameter();
        [SpiceName("kvth0"), SpiceInfo("Threshold degradation/enhancement parameter for LOD")]
        public Parameter BSIM4kvth0 { get; } = new Parameter();
        [SpiceName("tku0"), SpiceInfo("Temperature coefficient of KU0")]
        public Parameter BSIM4tku0 { get; } = new Parameter();
        [SpiceName("llodku0"), SpiceInfo("Length parameter for u0 LOD effect")]
        public Parameter BSIM4llodku0 { get; } = new Parameter();
        [SpiceName("wlodku0"), SpiceInfo("Width parameter for u0 LOD effect")]
        public Parameter BSIM4wlodku0 { get; } = new Parameter();
        [SpiceName("llodvth"), SpiceInfo("Length parameter for vth LOD effect")]
        public Parameter BSIM4llodvth { get; } = new Parameter();
        [SpiceName("wlodvth"), SpiceInfo("Width parameter for vth LOD effect")]
        public Parameter BSIM4wlodvth { get; } = new Parameter();
        [SpiceName("lku0"), SpiceInfo("Length dependence of ku0")]
        public Parameter BSIM4lku0 { get; } = new Parameter();
        [SpiceName("wku0"), SpiceInfo("Width dependence of ku0")]
        public Parameter BSIM4wku0 { get; } = new Parameter();
        [SpiceName("pku0"), SpiceInfo("Cross-term dependence of ku0")]
        public Parameter BSIM4pku0 { get; } = new Parameter();
        [SpiceName("lkvth0"), SpiceInfo("Length dependence of kvth0")]
        public Parameter BSIM4lkvth0 { get; } = new Parameter();
        [SpiceName("wkvth0"), SpiceInfo("Width dependence of kvth0")]
        public Parameter BSIM4wkvth0 { get; } = new Parameter();
        [SpiceName("pkvth0"), SpiceInfo("Cross-term dependence of kvth0")]
        public Parameter BSIM4pkvth0 { get; } = new Parameter();
        [SpiceName("stk2"), SpiceInfo("K2 shift factor related to stress effect on vth")]
        public Parameter BSIM4stk2 { get; } = new Parameter();
        [SpiceName("lodk2"), SpiceInfo("K2 shift modification factor for stress effect")]
        public Parameter BSIM4lodk2 { get; } = new Parameter(1.0);
        [SpiceName("steta0"), SpiceInfo("eta0 shift factor related to stress effect on vth")]
        public Parameter BSIM4steta0 { get; } = new Parameter();
        [SpiceName("lodeta0"), SpiceInfo("eta0 shift modification factor for stress effect")]
        public Parameter BSIM4lodeta0 { get; } = new Parameter(1.0);
        [SpiceName("web"), SpiceInfo("Coefficient for SCB")]
        public Parameter BSIM4web { get; } = new Parameter();
        [SpiceName("wec"), SpiceInfo("Coefficient for SCC")]
        public Parameter BSIM4wec { get; } = new Parameter();
        [SpiceName("kvth0we"), SpiceInfo("Threshold shift factor for well proximity effect")]
        public Parameter BSIM4kvth0we { get; } = new Parameter();
        [SpiceName("k2we"), SpiceInfo(" K2 shift factor for well proximity effect ")]
        public Parameter BSIM4k2we { get; } = new Parameter();
        [SpiceName("ku0we"), SpiceInfo(" Mobility degradation factor for well proximity effect ")]
        public Parameter BSIM4ku0we { get; } = new Parameter();
        [SpiceName("scref"), SpiceInfo(" Reference distance to calculate SCA, SCB and SCC")]
        public Parameter BSIM4scref { get; } = new Parameter(1.0E-6);
        [SpiceName("wpemod"), SpiceInfo(" Flag for WPE model (WPEMOD=1 to activate this model) ")]
        public Parameter BSIM4wpemod { get; } = new Parameter();
        [SpiceName("lkvth0we"), SpiceInfo("Length dependence of kvth0we")]
        public Parameter BSIM4lkvth0we { get; } = new Parameter();
        [SpiceName("lk2we"), SpiceInfo(" Length dependence of k2we ")]
        public Parameter BSIM4lk2we { get; } = new Parameter();
        [SpiceName("lku0we"), SpiceInfo(" Length dependence of ku0we ")]
        public Parameter BSIM4lku0we { get; } = new Parameter();
        [SpiceName("wkvth0we"), SpiceInfo("Width dependence of kvth0we")]
        public Parameter BSIM4wkvth0we { get; } = new Parameter();
        [SpiceName("wk2we"), SpiceInfo(" Width dependence of k2we ")]
        public Parameter BSIM4wk2we { get; } = new Parameter();
        [SpiceName("wku0we"), SpiceInfo(" Width dependence of ku0we ")]
        public Parameter BSIM4wku0we { get; } = new Parameter();
        [SpiceName("pkvth0we"), SpiceInfo("Cross-term dependence of kvth0we")]
        public Parameter BSIM4pkvth0we { get; } = new Parameter();
        [SpiceName("pk2we"), SpiceInfo(" Cross-term dependence of k2we ")]
        public Parameter BSIM4pk2we { get; } = new Parameter();
        [SpiceName("pku0we"), SpiceInfo(" Cross-term dependence of ku0we ")]
        public Parameter BSIM4pku0we { get; } = new Parameter();
        [SpiceName("beta0"), SpiceInfo("substrate current model parameter")]
        public Parameter BSIM4beta0 { get; } = new Parameter();
        [SpiceName("ijthdfwd"), SpiceInfo("Forward drain diode forward limiting current")]
        public Parameter BSIM4ijthdfwd { get; } = new Parameter();
        [SpiceName("ijthsfwd"), SpiceInfo("Forward source diode forward limiting current")]
        public Parameter BSIM4ijthsfwd { get; } = new Parameter(0.1);
        [SpiceName("ijthdrev"), SpiceInfo("Reverse drain diode forward limiting current")]
        public Parameter BSIM4ijthdrev { get; } = new Parameter();
        [SpiceName("ijthsrev"), SpiceInfo("Reverse source diode forward limiting current")]
        public Parameter BSIM4ijthsrev { get; } = new Parameter(0.1);
        [SpiceName("xjbvd"), SpiceInfo("Fitting parameter for drain diode breakdown current")]
        public Parameter BSIM4xjbvd { get; } = new Parameter();
        [SpiceName("xjbvs"), SpiceInfo("Fitting parameter for source diode breakdown current")]
        public Parameter BSIM4xjbvs { get; } = new Parameter(1.0);
        [SpiceName("bvd"), SpiceInfo("Drain diode breakdown voltage")]
        public Parameter BSIM4bvd { get; } = new Parameter();
        [SpiceName("bvs"), SpiceInfo("Source diode breakdown voltage")]
        public Parameter BSIM4bvs { get; } = new Parameter(10.0);
        [SpiceName("jtss"), SpiceInfo("Source bottom trap-assisted saturation current density")]
        public Parameter BSIM4jtss { get; } = new Parameter();
        [SpiceName("jtsd"), SpiceInfo("Drain bottom trap-assisted saturation current density")]
        public Parameter BSIM4jtsd { get; } = new Parameter();
        [SpiceName("jtssws"), SpiceInfo("Source STI sidewall trap-assisted saturation current density")]
        public Parameter BSIM4jtssws { get; } = new Parameter();
        [SpiceName("jtsswd"), SpiceInfo("Drain STI sidewall trap-assisted saturation current density")]
        public Parameter BSIM4jtsswd { get; } = new Parameter();
        [SpiceName("jtsswgs"), SpiceInfo("Source gate-edge sidewall trap-assisted saturation current density")]
        public Parameter BSIM4jtsswgs { get; } = new Parameter();
        [SpiceName("jtsswgd"), SpiceInfo("Drain gate-edge sidewall trap-assisted saturation current density")]
        public Parameter BSIM4jtsswgd { get; } = new Parameter();
        [SpiceName("jtweff"), SpiceInfo("TAT current width dependance")]
        public Parameter BSIM4jtweff { get; } = new Parameter();
        [SpiceName("njts"), SpiceInfo("Non-ideality factor for bottom junction")]
        public Parameter BSIM4njts { get; } = new Parameter(20.0);
        [SpiceName("njtssw"), SpiceInfo("Non-ideality factor for STI sidewall junction")]
        public Parameter BSIM4njtssw { get; } = new Parameter(20.0);
        [SpiceName("njtsswg"), SpiceInfo("Non-ideality factor for gate-edge sidewall junction")]
        public Parameter BSIM4njtsswg { get; } = new Parameter(20.0);
        [SpiceName("njtsd"), SpiceInfo("Non-ideality factor for bottom junction drain side")]
        public Parameter BSIM4njtsd { get; } = new Parameter();
        [SpiceName("njtsswd"), SpiceInfo("Non-ideality factor for STI sidewall junction drain side")]
        public Parameter BSIM4njtsswd { get; } = new Parameter();
        [SpiceName("njtsswgd"), SpiceInfo("Non-ideality factor for gate-edge sidewall junction drain side")]
        public Parameter BSIM4njtsswgd { get; } = new Parameter();
        [SpiceName("xtss"), SpiceInfo("Power dependence of JTSS on temperature")]
        public Parameter BSIM4xtss { get; } = new Parameter(0.02);
        [SpiceName("xtsd"), SpiceInfo("Power dependence of JTSD on temperature")]
        public Parameter BSIM4xtsd { get; } = new Parameter();
        [SpiceName("xtssws"), SpiceInfo("Power dependence of JTSSWS on temperature")]
        public Parameter BSIM4xtssws { get; } = new Parameter(0.02);
        [SpiceName("xtsswd"), SpiceInfo("Power dependence of JTSSWD on temperature")]
        public Parameter BSIM4xtsswd { get; } = new Parameter();
        [SpiceName("xtsswgs"), SpiceInfo("Power dependence of JTSSWGS on temperature")]
        public Parameter BSIM4xtsswgs { get; } = new Parameter(0.02);
        [SpiceName("xtsswgd"), SpiceInfo("Power dependence of JTSSWGD on temperature")]
        public Parameter BSIM4xtsswgd { get; } = new Parameter();
        [SpiceName("tnjts"), SpiceInfo("Temperature coefficient for NJTS")]
        public Parameter BSIM4tnjts { get; } = new Parameter();
        [SpiceName("tnjtssw"), SpiceInfo("Temperature coefficient for NJTSSW")]
        public Parameter BSIM4tnjtssw { get; } = new Parameter();
        [SpiceName("tnjtsswg"), SpiceInfo("Temperature coefficient for NJTSSWG")]
        public Parameter BSIM4tnjtsswg { get; } = new Parameter();
        [SpiceName("tnjtsd"), SpiceInfo("Temperature coefficient for NJTSD")]
        public Parameter BSIM4tnjtsd { get; } = new Parameter();
        [SpiceName("tnjtsswd"), SpiceInfo("Temperature coefficient for NJTSSWD")]
        public Parameter BSIM4tnjtsswd { get; } = new Parameter();
        [SpiceName("tnjtsswgd"), SpiceInfo("Temperature coefficient for NJTSSWGD")]
        public Parameter BSIM4tnjtsswgd { get; } = new Parameter();
        [SpiceName("vtss"), SpiceInfo("Source bottom trap-assisted voltage dependent parameter")]
        public Parameter BSIM4vtss { get; } = new Parameter(10.0);
        [SpiceName("vtsd"), SpiceInfo("Drain bottom trap-assisted voltage dependent parameter")]
        public Parameter BSIM4vtsd { get; } = new Parameter();
        [SpiceName("vtssws"), SpiceInfo("Source STI sidewall trap-assisted voltage dependent parameter")]
        public Parameter BSIM4vtssws { get; } = new Parameter(10.0);
        [SpiceName("vtsswd"), SpiceInfo("Drain STI sidewall trap-assisted voltage dependent parameter")]
        public Parameter BSIM4vtsswd { get; } = new Parameter();
        [SpiceName("vtsswgs"), SpiceInfo("Source gate-edge sidewall trap-assisted voltage dependent parameter")]
        public Parameter BSIM4vtsswgs { get; } = new Parameter(10.0);
        [SpiceName("vtsswgd"), SpiceInfo("Drain gate-edge sidewall trap-assisted voltage dependent parameter")]
        public Parameter BSIM4vtsswgd { get; } = new Parameter();
        [SpiceName("vfb"), SpiceInfo("Flat Band Voltage")]
        public Parameter BSIM4vfb { get; } = new Parameter(-1.0);
        [SpiceName("gbmin"), SpiceInfo("Minimum body conductance")]
        public Parameter BSIM4gbmin { get; } = new Parameter(1.0e-12);
        [SpiceName("rbdb"), SpiceInfo("Resistance between bNode and dbNode")]
        public Parameter BSIM4rbdb { get; } = new Parameter(50.0);
        [SpiceName("rbpb"), SpiceInfo("Resistance between bNodePrime and bNode")]
        public Parameter BSIM4rbpb { get; } = new Parameter(50.0);
        [SpiceName("rbsb"), SpiceInfo("Resistance between bNode and sbNode")]
        public Parameter BSIM4rbsb { get; } = new Parameter(50.0);
        [SpiceName("rbps"), SpiceInfo("Resistance between bNodePrime and sbNode")]
        public Parameter BSIM4rbps { get; } = new Parameter(50.0);
        [SpiceName("rbpd"), SpiceInfo("Resistance between bNodePrime and bNode")]
        public Parameter BSIM4rbpd { get; } = new Parameter(50.0);
        [SpiceName("rbps0"), SpiceInfo("Body resistance RBPS scaling")]
        public Parameter BSIM4rbps0 { get; } = new Parameter(50.0);
        [SpiceName("rbpsl"), SpiceInfo("Body resistance RBPS L scaling")]
        public Parameter BSIM4rbpsl { get; } = new Parameter();
        [SpiceName("rbpsw"), SpiceInfo("Body resistance RBPS W scaling")]
        public Parameter BSIM4rbpsw { get; } = new Parameter();
        [SpiceName("rbpsnf"), SpiceInfo("Body resistance RBPS NF scaling")]
        public Parameter BSIM4rbpsnf { get; } = new Parameter();
        [SpiceName("rbpd0"), SpiceInfo("Body resistance RBPD scaling")]
        public Parameter BSIM4rbpd0 { get; } = new Parameter(50.0);
        [SpiceName("rbpdl"), SpiceInfo("Body resistance RBPD L scaling")]
        public Parameter BSIM4rbpdl { get; } = new Parameter();
        [SpiceName("rbpdw"), SpiceInfo("Body resistance RBPD W scaling")]
        public Parameter BSIM4rbpdw { get; } = new Parameter();
        [SpiceName("rbpdnf"), SpiceInfo("Body resistance RBPD NF scaling")]
        public Parameter BSIM4rbpdnf { get; } = new Parameter();
        [SpiceName("rbpbx0"), SpiceInfo("Body resistance RBPBX  scaling")]
        public Parameter BSIM4rbpbx0 { get; } = new Parameter(100.0);
        [SpiceName("rbpbxl"), SpiceInfo("Body resistance RBPBX L scaling")]
        public Parameter BSIM4rbpbxl { get; } = new Parameter();
        [SpiceName("rbpbxw"), SpiceInfo("Body resistance RBPBX W scaling")]
        public Parameter BSIM4rbpbxw { get; } = new Parameter();
        [SpiceName("rbpbxnf"), SpiceInfo("Body resistance RBPBX NF scaling")]
        public Parameter BSIM4rbpbxnf { get; } = new Parameter();
        [SpiceName("rbpby0"), SpiceInfo("Body resistance RBPBY  scaling")]
        public Parameter BSIM4rbpby0 { get; } = new Parameter(100.0);
        [SpiceName("rbpbyl"), SpiceInfo("Body resistance RBPBY L scaling")]
        public Parameter BSIM4rbpbyl { get; } = new Parameter();
        [SpiceName("rbpbyw"), SpiceInfo("Body resistance RBPBY W scaling")]
        public Parameter BSIM4rbpbyw { get; } = new Parameter();
        [SpiceName("rbpbynf"), SpiceInfo("Body resistance RBPBY NF scaling")]
        public Parameter BSIM4rbpbynf { get; } = new Parameter();
        [SpiceName("rbsbx0"), SpiceInfo("Body resistance RBSBX  scaling")]
        public Parameter BSIM4rbsbx0 { get; } = new Parameter(100.0);
        [SpiceName("rbsby0"), SpiceInfo("Body resistance RBSBY  scaling")]
        public Parameter BSIM4rbsby0 { get; } = new Parameter(100.0);
        [SpiceName("rbdbx0"), SpiceInfo("Body resistance RBDBX  scaling")]
        public Parameter BSIM4rbdbx0 { get; } = new Parameter(100.0);
        [SpiceName("rbdby0"), SpiceInfo("Body resistance RBDBY  scaling")]
        public Parameter BSIM4rbdby0 { get; } = new Parameter(100.0);
        [SpiceName("rbsdbxl"), SpiceInfo("Body resistance RBSDBX L scaling")]
        public Parameter BSIM4rbsdbxl { get; } = new Parameter();
        [SpiceName("rbsdbxw"), SpiceInfo("Body resistance RBSDBX W scaling")]
        public Parameter BSIM4rbsdbxw { get; } = new Parameter();
        [SpiceName("rbsdbxnf"), SpiceInfo("Body resistance RBSDBX NF scaling")]
        public Parameter BSIM4rbsdbxnf { get; } = new Parameter();
        [SpiceName("rbsdbyl"), SpiceInfo("Body resistance RBSDBY L scaling")]
        public Parameter BSIM4rbsdbyl { get; } = new Parameter();
        [SpiceName("rbsdbyw"), SpiceInfo("Body resistance RBSDBY W scaling")]
        public Parameter BSIM4rbsdbyw { get; } = new Parameter();
        [SpiceName("rbsdbynf"), SpiceInfo("Body resistance RBSDBY NF scaling")]
        public Parameter BSIM4rbsdbynf { get; } = new Parameter();
        [SpiceName("cgsl"), SpiceInfo("New C-V model parameter")]
        public Parameter BSIM4cgsl { get; } = new Parameter();
        [SpiceName("cgdl"), SpiceInfo("New C-V model parameter")]
        public Parameter BSIM4cgdl { get; } = new Parameter();
        [SpiceName("ckappas"), SpiceInfo("S/G overlap C-V parameter ")]
        public Parameter BSIM4ckappas { get; } = new Parameter(0.6);
        [SpiceName("ckappad"), SpiceInfo("D/G overlap C-V parameter")]
        public Parameter BSIM4ckappad { get; } = new Parameter();
        [SpiceName("cf"), SpiceInfo("Fringe capacitance parameter")]
        public Parameter BSIM4cf { get; } = new Parameter();
        [SpiceName("clc"), SpiceInfo("Vdsat parameter for C-V model")]
        public Parameter BSIM4clc { get; } = new Parameter(0.1e-6);
        [SpiceName("cle"), SpiceInfo("Vdsat parameter for C-V model")]
        public Parameter BSIM4cle { get; } = new Parameter(0.6);
        [SpiceName("dwc"), SpiceInfo("Delta W for C-V model")]
        public Parameter BSIM4dwc { get; } = new Parameter();
        [SpiceName("dlc"), SpiceInfo("Delta L for C-V model")]
        public Parameter BSIM4dlc { get; } = new Parameter();
        [SpiceName("xw"), SpiceInfo("W offset for channel width due to mask/etch effect")]
        public Parameter BSIM4xw { get; } = new Parameter();
        [SpiceName("xl"), SpiceInfo("L offset for channel length due to mask/etch effect")]
        public Parameter BSIM4xl { get; } = new Parameter();
        [SpiceName("dlcig"), SpiceInfo("Delta L for Ig model")]
        public Parameter BSIM4dlcig { get; } = new Parameter();
        [SpiceName("dlcigd"), SpiceInfo("Delta L for Ig model drain side")]
        public Parameter BSIM4dlcigd { get; } = new Parameter();
        [SpiceName("dwj"), SpiceInfo("Delta W for S/D junctions")]
        public Parameter BSIM4dwj { get; } = new Parameter();
        [SpiceName("vfbcv"), SpiceInfo("Flat Band Voltage parameter for capmod=0 only")]
        public Parameter BSIM4vfbcv { get; } = new Parameter(-1.0);
        [SpiceName("acde"), SpiceInfo("Exponential coefficient for finite charge thickness")]
        public Parameter BSIM4acde { get; } = new Parameter(1.0);
        [SpiceName("moin"), SpiceInfo("Coefficient for gate-bias dependent surface potential")]
        public Parameter BSIM4moin { get; } = new Parameter(15.0);
        [SpiceName("noff"), SpiceInfo("C-V turn-on/off parameter")]
        public Parameter BSIM4noff { get; } = new Parameter(1.0);
        [SpiceName("voffcv"), SpiceInfo("C-V lateral-shift parameter")]
        public Parameter BSIM4voffcv { get; } = new Parameter();
        [SpiceName("dmcg"), SpiceInfo("Distance of Mid-Contact to Gate edge")]
        public Parameter BSIM4dmcg { get; } = new Parameter();
        [SpiceName("dmci"), SpiceInfo("Distance of Mid-Contact to Isolation")]
        public Parameter BSIM4dmci { get; } = new Parameter();
        [SpiceName("dmdg"), SpiceInfo("Distance of Mid-Diffusion to Gate edge")]
        public Parameter BSIM4dmdg { get; } = new Parameter();
        [SpiceName("dmcgt"), SpiceInfo("Distance of Mid-Contact to Gate edge in Test structures")]
        public Parameter BSIM4dmcgt { get; } = new Parameter();
        [SpiceName("xgw"), SpiceInfo("Distance from gate contact center to device edge")]
        public Parameter BSIM4xgw { get; } = new Parameter();
        [SpiceName("xgl"), SpiceInfo("Variation in Ldrawn")]
        public Parameter BSIM4xgl { get; } = new Parameter();
        [SpiceName("rshg"), SpiceInfo("Gate sheet resistance")]
        public Parameter BSIM4rshg { get; } = new Parameter(0.1);
        [SpiceName("ngcon"), SpiceInfo("Number of gate contacts")]
        public Parameter BSIM4ngcon { get; } = new Parameter(1.0);
        [SpiceName("tcj"), SpiceInfo("Temperature coefficient of cj")]
        public Parameter BSIM4tcj { get; } = new Parameter();
        [SpiceName("tpb"), SpiceInfo("Temperature coefficient of pb")]
        public Parameter BSIM4tpb { get; } = new Parameter();
        [SpiceName("tcjsw"), SpiceInfo("Temperature coefficient of cjsw")]
        public Parameter BSIM4tcjsw { get; } = new Parameter();
        [SpiceName("tpbsw"), SpiceInfo("Temperature coefficient of pbsw")]
        public Parameter BSIM4tpbsw { get; } = new Parameter();
        [SpiceName("tcjswg"), SpiceInfo("Temperature coefficient of cjswg")]
        public Parameter BSIM4tcjswg { get; } = new Parameter();
        [SpiceName("tpbswg"), SpiceInfo("Temperature coefficient of pbswg")]
        public Parameter BSIM4tpbswg { get; } = new Parameter();
        [SpiceName("lcdsc"), SpiceInfo("Length dependence of cdsc")]
        public Parameter BSIM4lcdsc { get; } = new Parameter();
        [SpiceName("lcdscb"), SpiceInfo("Length dependence of cdscb")]
        public Parameter BSIM4lcdscb { get; } = new Parameter();
        [SpiceName("lcdscd"), SpiceInfo("Length dependence of cdscd")]
        public Parameter BSIM4lcdscd { get; } = new Parameter();
        [SpiceName("lcit"), SpiceInfo("Length dependence of cit")]
        public Parameter BSIM4lcit { get; } = new Parameter();
        [SpiceName("lnfactor"), SpiceInfo("Length dependence of nfactor")]
        public Parameter BSIM4lnfactor { get; } = new Parameter();
        [SpiceName("lxj"), SpiceInfo("Length dependence of xj")]
        public Parameter BSIM4lxj { get; } = new Parameter();
        [SpiceName("lvsat"), SpiceInfo("Length dependence of vsat")]
        public Parameter BSIM4lvsat { get; } = new Parameter();
        [SpiceName("la0"), SpiceInfo("Length dependence of a0")]
        public Parameter BSIM4la0 { get; } = new Parameter();
        [SpiceName("lags"), SpiceInfo("Length dependence of ags")]
        public Parameter BSIM4lags { get; } = new Parameter();
        [SpiceName("la1"), SpiceInfo("Length dependence of a1")]
        public Parameter BSIM4la1 { get; } = new Parameter();
        [SpiceName("la2"), SpiceInfo("Length dependence of a2")]
        public Parameter BSIM4la2 { get; } = new Parameter();
        [SpiceName("lat"), SpiceInfo("Length dependence of at")]
        public Parameter BSIM4lat { get; } = new Parameter();
        [SpiceName("lketa"), SpiceInfo("Length dependence of keta")]
        public Parameter BSIM4lketa { get; } = new Parameter();
        [SpiceName("lnsub"), SpiceInfo("Length dependence of nsub")]
        public Parameter BSIM4lnsub { get; } = new Parameter();
        [SpiceName("lndep"), SpiceInfo("Length dependence of ndep")]
        public double BSIM4_LNDEP
        {
            get => BSIM4lndep;
            set
            {
                BSIM4lndep.Set(value);
                if (BSIM4lndep > 1.0e20)
                    BSIM4lndep.Value *= 1.0e-6;
            }
        }
        public Parameter BSIM4lndep { get; } = new Parameter();
        [SpiceName("lnsd"), SpiceInfo("Length dependence of nsd")]
        public double BSIM4_LNSD
        {
            get => BSIM4lnsd;
            set
            {
                BSIM4lnsd.Set(value);
                if (BSIM4lnsd > 1.0e23)
                    BSIM4lnsd.Value *= 1.0e-6;
            }
        }
        public Parameter BSIM4lnsd { get; } = new Parameter();
        [SpiceName("lngate"), SpiceInfo("Length dependence of ngate")]
        public double BSIM4_LNGATE
        {
            get => BSIM4lngate;
            set
            {
                BSIM4lngate.Set(value);
                if (BSIM4lngate > 1.0e23)
                    BSIM4lngate.Value *= 1.0e-6;
            }
        }
        public Parameter BSIM4lngate { get; } = new Parameter();
        [SpiceName("lgamma1"), SpiceInfo("Length dependence of gamma1")]
        public Parameter BSIM4lgamma1 { get; } = new Parameter();
        [SpiceName("lgamma2"), SpiceInfo("Length dependence of gamma2")]
        public Parameter BSIM4lgamma2 { get; } = new Parameter();
        [SpiceName("lvbx"), SpiceInfo("Length dependence of vbx")]
        public Parameter BSIM4lvbx { get; } = new Parameter();
        [SpiceName("lvbm"), SpiceInfo("Length dependence of vbm")]
        public Parameter BSIM4lvbm { get; } = new Parameter();
        [SpiceName("lxt"), SpiceInfo("Length dependence of xt")]
        public Parameter BSIM4lxt { get; } = new Parameter();
        [SpiceName("lk1"), SpiceInfo("Length dependence of k1")]
        public Parameter BSIM4lk1 { get; } = new Parameter();
        [SpiceName("lkt1"), SpiceInfo("Length dependence of kt1")]
        public Parameter BSIM4lkt1 { get; } = new Parameter();
        [SpiceName("lkt1l"), SpiceInfo("Length dependence of kt1l")]
        public Parameter BSIM4lkt1l { get; } = new Parameter();
        [SpiceName("lkt2"), SpiceInfo("Length dependence of kt2")]
        public Parameter BSIM4lkt2 { get; } = new Parameter();
        [SpiceName("lk2"), SpiceInfo("Length dependence of k2")]
        public Parameter BSIM4lk2 { get; } = new Parameter();
        [SpiceName("lk3"), SpiceInfo("Length dependence of k3")]
        public Parameter BSIM4lk3 { get; } = new Parameter();
        [SpiceName("lk3b"), SpiceInfo("Length dependence of k3b")]
        public Parameter BSIM4lk3b { get; } = new Parameter();
        [SpiceName("llpe0"), SpiceInfo("Length dependence of lpe0")]
        public Parameter BSIM4llpe0 { get; } = new Parameter();
        [SpiceName("llpeb"), SpiceInfo("Length dependence of lpeb")]
        public Parameter BSIM4llpeb { get; } = new Parameter();
        [SpiceName("ldvtp0"), SpiceInfo("Length dependence of dvtp0")]
        public Parameter BSIM4ldvtp0 { get; } = new Parameter();
        [SpiceName("ldvtp1"), SpiceInfo("Length dependence of dvtp1")]
        public Parameter BSIM4ldvtp1 { get; } = new Parameter();
        [SpiceName("ldvtp2"), SpiceInfo("Length dependence of dvtp2")]
        public Parameter BSIM4ldvtp2 { get; } = new Parameter();
        [SpiceName("ldvtp3"), SpiceInfo("Length dependence of dvtp3")]
        public Parameter BSIM4ldvtp3 { get; } = new Parameter();
        [SpiceName("ldvtp4"), SpiceInfo("Length dependence of dvtp4")]
        public Parameter BSIM4ldvtp4 { get; } = new Parameter();
        [SpiceName("ldvtp5"), SpiceInfo("Length dependence of dvtp5")]
        public Parameter BSIM4ldvtp5 { get; } = new Parameter();
        [SpiceName("lw0"), SpiceInfo("Length dependence of w0")]
        public Parameter BSIM4lw0 { get; } = new Parameter();
        [SpiceName("ldvt0"), SpiceInfo("Length dependence of dvt0")]
        public Parameter BSIM4ldvt0 { get; } = new Parameter();
        [SpiceName("ldvt1"), SpiceInfo("Length dependence of dvt1")]
        public Parameter BSIM4ldvt1 { get; } = new Parameter();
        [SpiceName("ldvt2"), SpiceInfo("Length dependence of dvt2")]
        public Parameter BSIM4ldvt2 { get; } = new Parameter();
        [SpiceName("ldvt0w"), SpiceInfo("Length dependence of dvt0w")]
        public Parameter BSIM4ldvt0w { get; } = new Parameter();
        [SpiceName("ldvt1w"), SpiceInfo("Length dependence of dvt1w")]
        public Parameter BSIM4ldvt1w { get; } = new Parameter();
        [SpiceName("ldvt2w"), SpiceInfo("Length dependence of dvt2w")]
        public Parameter BSIM4ldvt2w { get; } = new Parameter();
        [SpiceName("ldrout"), SpiceInfo("Length dependence of drout")]
        public Parameter BSIM4ldrout { get; } = new Parameter();
        [SpiceName("ldsub"), SpiceInfo("Length dependence of dsub")]
        public Parameter BSIM4ldsub { get; } = new Parameter();
        [SpiceName("lvth0"), SpiceName("lvtho"), SpiceInfo("Length dependence of vto")]
        public Parameter BSIM4lvth0 { get; } = new Parameter();
        [SpiceName("lua"), SpiceInfo("Length dependence of ua")]
        public Parameter BSIM4lua { get; } = new Parameter();
        [SpiceName("lua1"), SpiceInfo("Length dependence of ua1")]
        public Parameter BSIM4lua1 { get; } = new Parameter();
        [SpiceName("lub"), SpiceInfo("Length dependence of ub")]
        public Parameter BSIM4lub { get; } = new Parameter();
        [SpiceName("lub1"), SpiceInfo("Length dependence of ub1")]
        public Parameter BSIM4lub1 { get; } = new Parameter();
        [SpiceName("luc"), SpiceInfo("Length dependence of uc")]
        public Parameter BSIM4luc { get; } = new Parameter();
        [SpiceName("luc1"), SpiceInfo("Length dependence of uc1")]
        public Parameter BSIM4luc1 { get; } = new Parameter();
        [SpiceName("lu0"), SpiceInfo("Length dependence of u0")]
        public Parameter BSIM4lu0 { get; } = new Parameter();
        [SpiceName("lute"), SpiceInfo("Length dependence of ute")]
        public Parameter BSIM4lute { get; } = new Parameter();
        [SpiceName("lucste"), SpiceInfo("Length dependence of ucste")]
        public Parameter BSIM4lucste { get; } = new Parameter();
        [SpiceName("lvoff"), SpiceInfo("Length dependence of voff")]
        public Parameter BSIM4lvoff { get; } = new Parameter();
        [SpiceName("ltvoff"), SpiceInfo("Length dependence of tvoff")]
        public Parameter BSIM4ltvoff { get; } = new Parameter();
        [SpiceName("ltnfactor"), SpiceInfo("Length dependence of tnfactor")]
        public Parameter BSIM4ltnfactor { get; } = new Parameter();
        [SpiceName("lteta0"), SpiceInfo("Length dependence of teta0")]
        public Parameter BSIM4lteta0 { get; } = new Parameter();
        [SpiceName("ltvoffcv"), SpiceInfo("Length dependence of tvoffcv")]
        public Parameter BSIM4ltvoffcv { get; } = new Parameter();
        [SpiceName("lminv"), SpiceInfo("Length dependence of minv")]
        public Parameter BSIM4lminv { get; } = new Parameter();
        [SpiceName("lminvcv"), SpiceInfo("Length dependence of minvcv")]
        public Parameter BSIM4lminvcv { get; } = new Parameter();
        [SpiceName("lfprout"), SpiceInfo("Length dependence of pdiblcb")]
        public Parameter BSIM4lfprout { get; } = new Parameter();
        [SpiceName("lpdits"), SpiceInfo("Length dependence of pdits")]
        public Parameter BSIM4lpdits { get; } = new Parameter();
        [SpiceName("lpditsd"), SpiceInfo("Length dependence of pditsd")]
        public Parameter BSIM4lpditsd { get; } = new Parameter();
        [SpiceName("ldelta"), SpiceInfo("Length dependence of delta")]
        public Parameter BSIM4ldelta { get; } = new Parameter();
        [SpiceName("lrdsw"), SpiceInfo("Length dependence of rdsw ")]
        public Parameter BSIM4lrdsw { get; } = new Parameter();
        [SpiceName("lrdw"), SpiceInfo("Length dependence of rdw")]
        public Parameter BSIM4lrdw { get; } = new Parameter();
        [SpiceName("lrsw"), SpiceInfo("Length dependence of rsw")]
        public Parameter BSIM4lrsw { get; } = new Parameter();
        [SpiceName("lprwb"), SpiceInfo("Length dependence of prwb ")]
        public Parameter BSIM4lprwb { get; } = new Parameter();
        [SpiceName("lprwg"), SpiceInfo("Length dependence of prwg ")]
        public Parameter BSIM4lprwg { get; } = new Parameter();
        [SpiceName("lprt"), SpiceInfo("Length dependence of prt ")]
        public Parameter BSIM4lprt { get; } = new Parameter();
        [SpiceName("leta0"), SpiceInfo("Length dependence of eta0")]
        public Parameter BSIM4leta0 { get; } = new Parameter();
        [SpiceName("letab"), SpiceInfo("Length dependence of etab")]
        public Parameter BSIM4letab { get; } = new Parameter(-0.0);
        [SpiceName("lpclm"), SpiceInfo("Length dependence of pclm")]
        public Parameter BSIM4lpclm { get; } = new Parameter();
        [SpiceName("lpdiblc1"), SpiceInfo("Length dependence of pdiblc1")]
        public Parameter BSIM4lpdibl1 { get; } = new Parameter();
        [SpiceName("lpdiblc2"), SpiceInfo("Length dependence of pdiblc2")]
        public Parameter BSIM4lpdibl2 { get; } = new Parameter();
        [SpiceName("lpdiblcb"), SpiceInfo("Length dependence of pdiblcb")]
        public Parameter BSIM4lpdiblb { get; } = new Parameter();
        [SpiceName("lpscbe1"), SpiceInfo("Length dependence of pscbe1")]
        public Parameter BSIM4lpscbe1 { get; } = new Parameter();
        [SpiceName("lpscbe2"), SpiceInfo("Length dependence of pscbe2")]
        public Parameter BSIM4lpscbe2 { get; } = new Parameter();
        [SpiceName("lpvag"), SpiceInfo("Length dependence of pvag")]
        public Parameter BSIM4lpvag { get; } = new Parameter();
        [SpiceName("lwr"), SpiceInfo("Length dependence of wr")]
        public Parameter BSIM4lwr { get; } = new Parameter();
        [SpiceName("ldwg"), SpiceInfo("Length dependence of dwg")]
        public Parameter BSIM4ldwg { get; } = new Parameter();
        [SpiceName("ldwb"), SpiceInfo("Length dependence of dwb")]
        public Parameter BSIM4ldwb { get; } = new Parameter();
        [SpiceName("lb0"), SpiceInfo("Length dependence of b0")]
        public Parameter BSIM4lb0 { get; } = new Parameter();
        [SpiceName("lb1"), SpiceInfo("Length dependence of b1")]
        public Parameter BSIM4lb1 { get; } = new Parameter();
        [SpiceName("lalpha0"), SpiceInfo("Length dependence of alpha0")]
        public Parameter BSIM4lalpha0 { get; } = new Parameter();
        [SpiceName("lalpha1"), SpiceInfo("Length dependence of alpha1")]
        public Parameter BSIM4lalpha1 { get; } = new Parameter();
        [SpiceName("lbeta0"), SpiceInfo("Length dependence of beta0")]
        public Parameter BSIM4lbeta0 { get; } = new Parameter();
        [SpiceName("lphin"), SpiceInfo("Length dependence of phin")]
        public Parameter BSIM4lphin { get; } = new Parameter();
        [SpiceName("lagidl"), SpiceInfo("Length dependence of agidl")]
        public Parameter BSIM4lagidl { get; } = new Parameter();
        [SpiceName("lbgidl"), SpiceInfo("Length dependence of bgidl")]
        public Parameter BSIM4lbgidl { get; } = new Parameter();
        [SpiceName("lcgidl"), SpiceInfo("Length dependence of cgidl")]
        public Parameter BSIM4lcgidl { get; } = new Parameter();
        [SpiceName("legidl"), SpiceInfo("Length dependence of egidl")]
        public Parameter BSIM4legidl { get; } = new Parameter();
        [SpiceName("lfgidl"), SpiceInfo("Length dependence of fgidl")]
        public Parameter BSIM4lfgidl { get; } = new Parameter();
        [SpiceName("lkgidl"), SpiceInfo("Length dependence of kgidl")]
        public Parameter BSIM4lkgidl { get; } = new Parameter();
        [SpiceName("lrgidl"), SpiceInfo("Length dependence of rgidl")]
        public Parameter BSIM4lrgidl { get; } = new Parameter();
        [SpiceName("lagisl"), SpiceInfo("Length dependence of agisl")]
        public Parameter BSIM4lagisl { get; } = new Parameter();
        [SpiceName("lbgisl"), SpiceInfo("Length dependence of bgisl")]
        public Parameter BSIM4lbgisl { get; } = new Parameter();
        [SpiceName("lcgisl"), SpiceInfo("Length dependence of cgisl")]
        public Parameter BSIM4lcgisl { get; } = new Parameter();
        [SpiceName("legisl"), SpiceInfo("Length dependence of egisl")]
        public Parameter BSIM4legisl { get; } = new Parameter();
        [SpiceName("lfgisl"), SpiceInfo("Length dependence of fgisl")]
        public Parameter BSIM4lfgisl { get; } = new Parameter();
        [SpiceName("lkgisl"), SpiceInfo("Length dependence of kgisl")]
        public Parameter BSIM4lkgisl { get; } = new Parameter();
        [SpiceName("lrgisl"), SpiceInfo("Length dependence of rgisl")]
        public Parameter BSIM4lrgisl { get; } = new Parameter();
        [SpiceName("laigc"), SpiceInfo("Length dependence of aigc")]
        public Parameter BSIM4laigc { get; } = new Parameter();
        [SpiceName("lbigc"), SpiceInfo("Length dependence of bigc")]
        public Parameter BSIM4lbigc { get; } = new Parameter();
        [SpiceName("lcigc"), SpiceInfo("Length dependence of cigc")]
        public Parameter BSIM4lcigc { get; } = new Parameter();
        [SpiceName("laigsd"), SpiceInfo("Length dependence of aigsd")]
        public Parameter BSIM4laigsd { get; } = new Parameter();
        [SpiceName("lbigsd"), SpiceInfo("Length dependence of bigsd")]
        public Parameter BSIM4lbigsd { get; } = new Parameter();
        [SpiceName("lcigsd"), SpiceInfo("Length dependence of cigsd")]
        public Parameter BSIM4lcigsd { get; } = new Parameter();
        [SpiceName("laigs"), SpiceInfo("Length dependence of aigs")]
        public Parameter BSIM4laigs { get; } = new Parameter();
        [SpiceName("lbigs"), SpiceInfo("Length dependence of bigs")]
        public Parameter BSIM4lbigs { get; } = new Parameter();
        [SpiceName("lcigs"), SpiceInfo("Length dependence of cigs")]
        public Parameter BSIM4lcigs { get; } = new Parameter();
        [SpiceName("laigd"), SpiceInfo("Length dependence of aigd")]
        public Parameter BSIM4laigd { get; } = new Parameter();
        [SpiceName("lbigd"), SpiceInfo("Length dependence of bigd")]
        public Parameter BSIM4lbigd { get; } = new Parameter();
        [SpiceName("lcigd"), SpiceInfo("Length dependence of cigd")]
        public Parameter BSIM4lcigd { get; } = new Parameter();
        [SpiceName("laigbacc"), SpiceInfo("Length dependence of aigbacc")]
        public Parameter BSIM4laigbacc { get; } = new Parameter();
        [SpiceName("lbigbacc"), SpiceInfo("Length dependence of bigbacc")]
        public Parameter BSIM4lbigbacc { get; } = new Parameter();
        [SpiceName("lcigbacc"), SpiceInfo("Length dependence of cigbacc")]
        public Parameter BSIM4lcigbacc { get; } = new Parameter();
        [SpiceName("laigbinv"), SpiceInfo("Length dependence of aigbinv")]
        public Parameter BSIM4laigbinv { get; } = new Parameter();
        [SpiceName("lbigbinv"), SpiceInfo("Length dependence of bigbinv")]
        public Parameter BSIM4lbigbinv { get; } = new Parameter();
        [SpiceName("lcigbinv"), SpiceInfo("Length dependence of cigbinv")]
        public Parameter BSIM4lcigbinv { get; } = new Parameter();
        [SpiceName("lnigc"), SpiceInfo("Length dependence of nigc")]
        public Parameter BSIM4lnigc { get; } = new Parameter();
        [SpiceName("lnigbinv"), SpiceInfo("Length dependence of nigbinv")]
        public Parameter BSIM4lnigbinv { get; } = new Parameter();
        [SpiceName("lnigbacc"), SpiceInfo("Length dependence of nigbacc")]
        public Parameter BSIM4lnigbacc { get; } = new Parameter();
        [SpiceName("lntox"), SpiceInfo("Length dependence of ntox")]
        public Parameter BSIM4lntox { get; } = new Parameter();
        [SpiceName("leigbinv"), SpiceInfo("Length dependence for eigbinv")]
        public Parameter BSIM4leigbinv { get; } = new Parameter();
        [SpiceName("lpigcd"), SpiceInfo("Length dependence for pigcd")]
        public Parameter BSIM4lpigcd { get; } = new Parameter();
        [SpiceName("lpoxedge"), SpiceInfo("Length dependence for poxedge")]
        public Parameter BSIM4lpoxedge { get; } = new Parameter();
        [SpiceName("lxrcrg1"), SpiceInfo("Length dependence of xrcrg1")]
        public Parameter BSIM4lxrcrg1 { get; } = new Parameter();
        [SpiceName("lxrcrg2"), SpiceInfo("Length dependence of xrcrg2")]
        public Parameter BSIM4lxrcrg2 { get; } = new Parameter();
        [SpiceName("llambda"), SpiceInfo("Length dependence of lambda")]
        public Parameter BSIM4llambda { get; } = new Parameter();
        [SpiceName("lvtl"), SpiceInfo(" Length dependence of vtl")]
        public Parameter BSIM4lvtl { get; } = new Parameter();
        [SpiceName("lxn"), SpiceInfo(" Length dependence of xn")]
        public Parameter BSIM4lxn { get; } = new Parameter();
        [SpiceName("lvfbsdoff"), SpiceInfo("Length dependence of vfbsdoff")]
        public Parameter BSIM4lvfbsdoff { get; } = new Parameter();
        [SpiceName("ltvfbsdoff"), SpiceInfo("Length dependence of tvfbsdoff")]
        public Parameter BSIM4ltvfbsdoff { get; } = new Parameter();
        [SpiceName("leu"), SpiceInfo(" Length dependence of eu")]
        public Parameter BSIM4leu { get; } = new Parameter();
        [SpiceName("lucs"), SpiceInfo("Length dependence of lucs")]
        public Parameter BSIM4lucs { get; } = new Parameter();
        [SpiceName("lvfb"), SpiceInfo("Length dependence of vfb")]
        public Parameter BSIM4lvfb { get; } = new Parameter();
        [SpiceName("lcgsl"), SpiceInfo("Length dependence of cgsl")]
        public Parameter BSIM4lcgsl { get; } = new Parameter();
        [SpiceName("lcgdl"), SpiceInfo("Length dependence of cgdl")]
        public Parameter BSIM4lcgdl { get; } = new Parameter();
        [SpiceName("lckappas"), SpiceInfo("Length dependence of ckappas")]
        public Parameter BSIM4lckappas { get; } = new Parameter();
        [SpiceName("lckappad"), SpiceInfo("Length dependence of ckappad")]
        public Parameter BSIM4lckappad { get; } = new Parameter();
        [SpiceName("lcf"), SpiceInfo("Length dependence of cf")]
        public Parameter BSIM4lcf { get; } = new Parameter();
        [SpiceName("lclc"), SpiceInfo("Length dependence of clc")]
        public Parameter BSIM4lclc { get; } = new Parameter();
        [SpiceName("lcle"), SpiceInfo("Length dependence of cle")]
        public Parameter BSIM4lcle { get; } = new Parameter();
        [SpiceName("lvfbcv"), SpiceInfo("Length dependence of vfbcv")]
        public Parameter BSIM4lvfbcv { get; } = new Parameter();
        [SpiceName("lacde"), SpiceInfo("Length dependence of acde")]
        public Parameter BSIM4lacde { get; } = new Parameter();
        [SpiceName("lmoin"), SpiceInfo("Length dependence of moin")]
        public Parameter BSIM4lmoin { get; } = new Parameter();
        [SpiceName("lnoff"), SpiceInfo("Length dependence of noff")]
        public Parameter BSIM4lnoff { get; } = new Parameter();
        [SpiceName("lvoffcv"), SpiceInfo("Length dependence of voffcv")]
        public Parameter BSIM4lvoffcv { get; } = new Parameter();
        [SpiceName("wcdsc"), SpiceInfo("Width dependence of cdsc")]
        public Parameter BSIM4wcdsc { get; } = new Parameter();
        [SpiceName("wcdscb"), SpiceInfo("Width dependence of cdscb")]
        public Parameter BSIM4wcdscb { get; } = new Parameter();
        [SpiceName("wcdscd"), SpiceInfo("Width dependence of cdscd")]
        public Parameter BSIM4wcdscd { get; } = new Parameter();
        [SpiceName("wcit"), SpiceInfo("Width dependence of cit")]
        public Parameter BSIM4wcit { get; } = new Parameter();
        [SpiceName("wnfactor"), SpiceInfo("Width dependence of nfactor")]
        public Parameter BSIM4wnfactor { get; } = new Parameter();
        [SpiceName("wxj"), SpiceInfo("Width dependence of xj")]
        public Parameter BSIM4wxj { get; } = new Parameter();
        [SpiceName("wvsat"), SpiceInfo("Width dependence of vsat")]
        public Parameter BSIM4wvsat { get; } = new Parameter();
        [SpiceName("wa0"), SpiceInfo("Width dependence of a0")]
        public Parameter BSIM4wa0 { get; } = new Parameter();
        [SpiceName("wags"), SpiceInfo("Width dependence of ags")]
        public Parameter BSIM4wags { get; } = new Parameter();
        [SpiceName("wa1"), SpiceInfo("Width dependence of a1")]
        public Parameter BSIM4wa1 { get; } = new Parameter();
        [SpiceName("wa2"), SpiceInfo("Width dependence of a2")]
        public Parameter BSIM4wa2 { get; } = new Parameter();
        [SpiceName("wat"), SpiceInfo("Width dependence of at")]
        public Parameter BSIM4wat { get; } = new Parameter();
        [SpiceName("wketa"), SpiceInfo("Width dependence of keta")]
        public Parameter BSIM4wketa { get; } = new Parameter();
        [SpiceName("wnsub"), SpiceInfo("Width dependence of nsub")]
        public Parameter BSIM4wnsub { get; } = new Parameter();
        [SpiceName("wndep"), SpiceInfo("Width dependence of ndep")]
        public double BSIM4_WNDEP
        {
            get => BSIM4wndep;
            set
            {
                BSIM4wndep.Set(value);
                if (BSIM4wndep > 1.0e20)
                    BSIM4wndep.Value *= 1.0e-6;
            }
        }
        public Parameter BSIM4wndep { get; } = new Parameter();
        [SpiceName("wnsd"), SpiceInfo("Width dependence of nsd")]
        public double BSIM4_WNSD
        {
            get => BSIM4wnsd;
            set
            {
                BSIM4wnsd.Set(value);
                if (BSIM4wnsd > 1.0e23)
                    BSIM4wnsd.Value *= 1.0e-6;
            }
        }
        public Parameter BSIM4wnsd { get; } = new Parameter();
        [SpiceName("wngate"), SpiceInfo("Width dependence of ngate")]
        public double BSIM4_WNGATE
        {
            get => BSIM4wngate;
            set
            {
                BSIM4wngate.Set(value);
                if (BSIM4wngate > 1.0e23)
                    BSIM4wngate.Value *= 1.0e-6;
            }
        }
        public Parameter BSIM4wngate { get; } = new Parameter();
        [SpiceName("wgamma1"), SpiceInfo("Width dependence of gamma1")]
        public Parameter BSIM4wgamma1 { get; } = new Parameter();
        [SpiceName("wgamma2"), SpiceInfo("Width dependence of gamma2")]
        public Parameter BSIM4wgamma2 { get; } = new Parameter();
        [SpiceName("wvbx"), SpiceInfo("Width dependence of vbx")]
        public Parameter BSIM4wvbx { get; } = new Parameter();
        [SpiceName("wvbm"), SpiceInfo("Width dependence of vbm")]
        public Parameter BSIM4wvbm { get; } = new Parameter();
        [SpiceName("wxt"), SpiceInfo("Width dependence of xt")]
        public Parameter BSIM4wxt { get; } = new Parameter();
        [SpiceName("wk1"), SpiceInfo("Width dependence of k1")]
        public Parameter BSIM4wk1 { get; } = new Parameter();
        [SpiceName("wkt1"), SpiceInfo("Width dependence of kt1")]
        public Parameter BSIM4wkt1 { get; } = new Parameter();
        [SpiceName("wkt1l"), SpiceInfo("Width dependence of kt1l")]
        public Parameter BSIM4wkt1l { get; } = new Parameter();
        [SpiceName("wkt2"), SpiceInfo("Width dependence of kt2")]
        public Parameter BSIM4wkt2 { get; } = new Parameter();
        [SpiceName("wk2"), SpiceInfo("Width dependence of k2")]
        public Parameter BSIM4wk2 { get; } = new Parameter();
        [SpiceName("wk3"), SpiceInfo("Width dependence of k3")]
        public Parameter BSIM4wk3 { get; } = new Parameter();
        [SpiceName("wk3b"), SpiceInfo("Width dependence of k3b")]
        public Parameter BSIM4wk3b { get; } = new Parameter();
        [SpiceName("wlpe0"), SpiceInfo("Width dependence of lpe0")]
        public Parameter BSIM4wlpe0 { get; } = new Parameter();
        [SpiceName("wlpeb"), SpiceInfo("Width dependence of lpeb")]
        public Parameter BSIM4wlpeb { get; } = new Parameter();
        [SpiceName("wdvtp0"), SpiceInfo("Width dependence of dvtp0")]
        public Parameter BSIM4wdvtp0 { get; } = new Parameter();
        [SpiceName("wdvtp1"), SpiceInfo("Width dependence of dvtp1")]
        public Parameter BSIM4wdvtp1 { get; } = new Parameter();
        [SpiceName("wdvtp2"), SpiceInfo("Width dependence of dvtp2")]
        public Parameter BSIM4wdvtp2 { get; } = new Parameter();
        [SpiceName("wdvtp3"), SpiceInfo("Width dependence of dvtp3")]
        public Parameter BSIM4wdvtp3 { get; } = new Parameter();
        [SpiceName("wdvtp4"), SpiceInfo("Width dependence of dvtp4")]
        public Parameter BSIM4wdvtp4 { get; } = new Parameter();
        [SpiceName("wdvtp5"), SpiceInfo("Width dependence of dvtp5")]
        public Parameter BSIM4wdvtp5 { get; } = new Parameter();
        [SpiceName("ww0"), SpiceInfo("Width dependence of w0")]
        public Parameter BSIM4ww0 { get; } = new Parameter();
        [SpiceName("wdvt0"), SpiceInfo("Width dependence of dvt0")]
        public Parameter BSIM4wdvt0 { get; } = new Parameter();
        [SpiceName("wdvt1"), SpiceInfo("Width dependence of dvt1")]
        public Parameter BSIM4wdvt1 { get; } = new Parameter();
        [SpiceName("wdvt2"), SpiceInfo("Width dependence of dvt2")]
        public Parameter BSIM4wdvt2 { get; } = new Parameter();
        [SpiceName("wdvt0w"), SpiceInfo("Width dependence of dvt0w")]
        public Parameter BSIM4wdvt0w { get; } = new Parameter();
        [SpiceName("wdvt1w"), SpiceInfo("Width dependence of dvt1w")]
        public Parameter BSIM4wdvt1w { get; } = new Parameter();
        [SpiceName("wdvt2w"), SpiceInfo("Width dependence of dvt2w")]
        public Parameter BSIM4wdvt2w { get; } = new Parameter();
        [SpiceName("wdrout"), SpiceInfo("Width dependence of drout")]
        public Parameter BSIM4wdrout { get; } = new Parameter();
        [SpiceName("wdsub"), SpiceInfo("Width dependence of dsub")]
        public Parameter BSIM4wdsub { get; } = new Parameter();
        [SpiceName("wvth0"), SpiceName("wvtho"), SpiceInfo("Width dependence of vto")]
        public Parameter BSIM4wvth0 { get; } = new Parameter();
        [SpiceName("wua"), SpiceInfo("Width dependence of ua")]
        public Parameter BSIM4wua { get; } = new Parameter();
        [SpiceName("wua1"), SpiceInfo("Width dependence of ua1")]
        public Parameter BSIM4wua1 { get; } = new Parameter();
        [SpiceName("wub"), SpiceInfo("Width dependence of ub")]
        public Parameter BSIM4wub { get; } = new Parameter();
        [SpiceName("wub1"), SpiceInfo("Width dependence of ub1")]
        public Parameter BSIM4wub1 { get; } = new Parameter();
        [SpiceName("wuc"), SpiceInfo("Width dependence of uc")]
        public Parameter BSIM4wuc { get; } = new Parameter();
        [SpiceName("wuc1"), SpiceInfo("Width dependence of uc1")]
        public Parameter BSIM4wuc1 { get; } = new Parameter();
        [SpiceName("wu0"), SpiceInfo("Width dependence of u0")]
        public Parameter BSIM4wu0 { get; } = new Parameter();
        [SpiceName("wute"), SpiceInfo("Width dependence of ute")]
        public Parameter BSIM4wute { get; } = new Parameter();
        [SpiceName("wucste"), SpiceInfo("Width dependence of ucste")]
        public Parameter BSIM4wucste { get; } = new Parameter();
        [SpiceName("wvoff"), SpiceInfo("Width dependence of voff")]
        public Parameter BSIM4wvoff { get; } = new Parameter();
        [SpiceName("wtvoff"), SpiceInfo("Width dependence of tvoff")]
        public Parameter BSIM4wtvoff { get; } = new Parameter();
        [SpiceName("wtnfactor"), SpiceInfo("Width dependence of tnfactor")]
        public Parameter BSIM4wtnfactor { get; } = new Parameter();
        [SpiceName("wteta0"), SpiceInfo("Width dependence of teta0")]
        public Parameter BSIM4wteta0 { get; } = new Parameter();
        [SpiceName("wtvoffcv"), SpiceInfo("Width dependence of tvoffcv")]
        public Parameter BSIM4wtvoffcv { get; } = new Parameter();
        [SpiceName("wminv"), SpiceInfo("Width dependence of minv")]
        public Parameter BSIM4wminv { get; } = new Parameter();
        [SpiceName("wminvcv"), SpiceInfo("Width dependence of minvcv")]
        public Parameter BSIM4wminvcv { get; } = new Parameter();
        [SpiceName("wfprout"), SpiceInfo("Width dependence of pdiblcb")]
        public Parameter BSIM4wfprout { get; } = new Parameter();
        [SpiceName("wpdits"), SpiceInfo("Width dependence of pdits")]
        public Parameter BSIM4wpdits { get; } = new Parameter();
        [SpiceName("wpditsd"), SpiceInfo("Width dependence of pditsd")]
        public Parameter BSIM4wpditsd { get; } = new Parameter();
        [SpiceName("wdelta"), SpiceInfo("Width dependence of delta")]
        public Parameter BSIM4wdelta { get; } = new Parameter();
        [SpiceName("wrdsw"), SpiceInfo("Width dependence of rdsw ")]
        public Parameter BSIM4wrdsw { get; } = new Parameter();
        [SpiceName("wrdw"), SpiceInfo("Width dependence of rdw")]
        public Parameter BSIM4wrdw { get; } = new Parameter();
        [SpiceName("wrsw"), SpiceInfo("Width dependence of rsw")]
        public Parameter BSIM4wrsw { get; } = new Parameter();
        [SpiceName("wprwb"), SpiceInfo("Width dependence of prwb ")]
        public Parameter BSIM4wprwb { get; } = new Parameter();
        [SpiceName("wprwg"), SpiceInfo("Width dependence of prwg ")]
        public Parameter BSIM4wprwg { get; } = new Parameter();
        [SpiceName("wprt"), SpiceInfo("Width dependence of prt")]
        public Parameter BSIM4wprt { get; } = new Parameter();
        [SpiceName("weta0"), SpiceInfo("Width dependence of eta0")]
        public Parameter BSIM4weta0 { get; } = new Parameter();
        [SpiceName("wetab"), SpiceInfo("Width dependence of etab")]
        public Parameter BSIM4wetab { get; } = new Parameter();
        [SpiceName("wpclm"), SpiceInfo("Width dependence of pclm")]
        public Parameter BSIM4wpclm { get; } = new Parameter();
        [SpiceName("wpdiblc1"), SpiceInfo("Width dependence of pdiblc1")]
        public Parameter BSIM4wpdibl1 { get; } = new Parameter();
        [SpiceName("wpdiblc2"), SpiceInfo("Width dependence of pdiblc2")]
        public Parameter BSIM4wpdibl2 { get; } = new Parameter();
        [SpiceName("wpdiblcb"), SpiceInfo("Width dependence of pdiblcb")]
        public Parameter BSIM4wpdiblb { get; } = new Parameter();
        [SpiceName("wpscbe1"), SpiceInfo("Width dependence of pscbe1")]
        public Parameter BSIM4wpscbe1 { get; } = new Parameter();
        [SpiceName("wpscbe2"), SpiceInfo("Width dependence of pscbe2")]
        public Parameter BSIM4wpscbe2 { get; } = new Parameter();
        [SpiceName("wpvag"), SpiceInfo("Width dependence of pvag")]
        public Parameter BSIM4wpvag { get; } = new Parameter();
        [SpiceName("wwr"), SpiceInfo("Width dependence of wr")]
        public Parameter BSIM4wwr { get; } = new Parameter();
        [SpiceName("wdwg"), SpiceInfo("Width dependence of dwg")]
        public Parameter BSIM4wdwg { get; } = new Parameter();
        [SpiceName("wdwb"), SpiceInfo("Width dependence of dwb")]
        public Parameter BSIM4wdwb { get; } = new Parameter();
        [SpiceName("wb0"), SpiceInfo("Width dependence of b0")]
        public Parameter BSIM4wb0 { get; } = new Parameter();
        [SpiceName("wb1"), SpiceInfo("Width dependence of b1")]
        public Parameter BSIM4wb1 { get; } = new Parameter();
        [SpiceName("walpha0"), SpiceInfo("Width dependence of alpha0")]
        public Parameter BSIM4walpha0 { get; } = new Parameter();
        [SpiceName("walpha1"), SpiceInfo("Width dependence of alpha1")]
        public Parameter BSIM4walpha1 { get; } = new Parameter();
        [SpiceName("wbeta0"), SpiceInfo("Width dependence of beta0")]
        public Parameter BSIM4wbeta0 { get; } = new Parameter();
        [SpiceName("wphin"), SpiceInfo("Width dependence of phin")]
        public Parameter BSIM4wphin { get; } = new Parameter();
        [SpiceName("wagidl"), SpiceInfo("Width dependence of agidl")]
        public Parameter BSIM4wagidl { get; } = new Parameter();
        [SpiceName("wbgidl"), SpiceInfo("Width dependence of bgidl")]
        public Parameter BSIM4wbgidl { get; } = new Parameter();
        [SpiceName("wcgidl"), SpiceInfo("Width dependence of cgidl")]
        public Parameter BSIM4wcgidl { get; } = new Parameter();
        [SpiceName("wegidl"), SpiceInfo("Width dependence of egidl")]
        public Parameter BSIM4wegidl { get; } = new Parameter();
        [SpiceName("wfgidl"), SpiceInfo("Width dependence of fgidl")]
        public Parameter BSIM4wfgidl { get; } = new Parameter();
        [SpiceName("wkgidl"), SpiceInfo("Width dependence of kgidl")]
        public Parameter BSIM4wkgidl { get; } = new Parameter();
        [SpiceName("wrgidl"), SpiceInfo("Width dependence of rgidl")]
        public Parameter BSIM4wrgidl { get; } = new Parameter();
        [SpiceName("wagisl"), SpiceInfo("Width dependence of agisl")]
        public Parameter BSIM4wagisl { get; } = new Parameter();
        [SpiceName("wbgisl"), SpiceInfo("Width dependence of bgisl")]
        public Parameter BSIM4wbgisl { get; } = new Parameter();
        [SpiceName("wcgisl"), SpiceInfo("Width dependence of cgisl")]
        public Parameter BSIM4wcgisl { get; } = new Parameter();
        [SpiceName("wegisl"), SpiceInfo("Width dependence of egisl")]
        public Parameter BSIM4wegisl { get; } = new Parameter();
        [SpiceName("wfgisl"), SpiceInfo("Width dependence of fgisl")]
        public Parameter BSIM4wfgisl { get; } = new Parameter();
        [SpiceName("wkgisl"), SpiceInfo("Width dependence of kgisl")]
        public Parameter BSIM4wkgisl { get; } = new Parameter();
        [SpiceName("wrgisl"), SpiceInfo("Width dependence of rgisl")]
        public Parameter BSIM4wrgisl { get; } = new Parameter();
        [SpiceName("waigc"), SpiceInfo("Width dependence of aigc")]
        public Parameter BSIM4waigc { get; } = new Parameter();
        [SpiceName("wbigc"), SpiceInfo("Width dependence of bigc")]
        public Parameter BSIM4wbigc { get; } = new Parameter();
        [SpiceName("wcigc"), SpiceInfo("Width dependence of cigc")]
        public Parameter BSIM4wcigc { get; } = new Parameter();
        [SpiceName("waigsd"), SpiceInfo("Width dependence of aigsd")]
        public Parameter BSIM4waigsd { get; } = new Parameter();
        [SpiceName("wbigsd"), SpiceInfo("Width dependence of bigsd")]
        public Parameter BSIM4wbigsd { get; } = new Parameter();
        [SpiceName("wcigsd"), SpiceInfo("Width dependence of cigsd")]
        public Parameter BSIM4wcigsd { get; } = new Parameter();
        [SpiceName("waigs"), SpiceInfo("Width dependence of aigs")]
        public Parameter BSIM4waigs { get; } = new Parameter();
        [SpiceName("wbigs"), SpiceInfo("Width dependence of bigs")]
        public Parameter BSIM4wbigs { get; } = new Parameter();
        [SpiceName("wcigs"), SpiceInfo("Width dependence of cigs")]
        public Parameter BSIM4wcigs { get; } = new Parameter();
        [SpiceName("waigd"), SpiceInfo("Width dependence of aigd")]
        public Parameter BSIM4waigd { get; } = new Parameter();
        [SpiceName("wbigd"), SpiceInfo("Width dependence of bigd")]
        public Parameter BSIM4wbigd { get; } = new Parameter();
        [SpiceName("wcigd"), SpiceInfo("Width dependence of cigd")]
        public Parameter BSIM4wcigd { get; } = new Parameter();
        [SpiceName("waigbacc"), SpiceInfo("Width dependence of aigbacc")]
        public Parameter BSIM4waigbacc { get; } = new Parameter();
        [SpiceName("wbigbacc"), SpiceInfo("Width dependence of bigbacc")]
        public Parameter BSIM4wbigbacc { get; } = new Parameter();
        [SpiceName("wcigbacc"), SpiceInfo("Width dependence of cigbacc")]
        public Parameter BSIM4wcigbacc { get; } = new Parameter();
        [SpiceName("waigbinv"), SpiceInfo("Width dependence of aigbinv")]
        public Parameter BSIM4waigbinv { get; } = new Parameter();
        [SpiceName("wbigbinv"), SpiceInfo("Width dependence of bigbinv")]
        public Parameter BSIM4wbigbinv { get; } = new Parameter();
        [SpiceName("wcigbinv"), SpiceInfo("Width dependence of cigbinv")]
        public Parameter BSIM4wcigbinv { get; } = new Parameter();
        [SpiceName("wnigc"), SpiceInfo("Width dependence of nigc")]
        public Parameter BSIM4wnigc { get; } = new Parameter();
        [SpiceName("wnigbinv"), SpiceInfo("Width dependence of nigbinv")]
        public Parameter BSIM4wnigbinv { get; } = new Parameter();
        [SpiceName("wnigbacc"), SpiceInfo("Width dependence of nigbacc")]
        public Parameter BSIM4wnigbacc { get; } = new Parameter();
        [SpiceName("wntox"), SpiceInfo("Width dependence of ntox")]
        public Parameter BSIM4wntox { get; } = new Parameter();
        [SpiceName("weigbinv"), SpiceInfo("Width dependence for eigbinv")]
        public Parameter BSIM4weigbinv { get; } = new Parameter();
        [SpiceName("wpigcd"), SpiceInfo("Width dependence for pigcd")]
        public Parameter BSIM4wpigcd { get; } = new Parameter();
        [SpiceName("wpoxedge"), SpiceInfo("Width dependence for poxedge")]
        public Parameter BSIM4wpoxedge { get; } = new Parameter();
        [SpiceName("wxrcrg1"), SpiceInfo("Width dependence of xrcrg1")]
        public Parameter BSIM4wxrcrg1 { get; } = new Parameter();
        [SpiceName("wxrcrg2"), SpiceInfo("Width dependence of xrcrg2")]
        public Parameter BSIM4wxrcrg2 { get; } = new Parameter();
        [SpiceName("wlambda"), SpiceInfo("Width dependence of lambda")]
        public Parameter BSIM4wlambda { get; } = new Parameter();
        [SpiceName("wvtl"), SpiceInfo("Width dependence of vtl")]
        public Parameter BSIM4wvtl { get; } = new Parameter();
        [SpiceName("wxn"), SpiceInfo("Width dependence of xn")]
        public Parameter BSIM4wxn { get; } = new Parameter();
        [SpiceName("wvfbsdoff"), SpiceInfo("Width dependence of vfbsdoff")]
        public Parameter BSIM4wvfbsdoff { get; } = new Parameter();
        [SpiceName("wtvfbsdoff"), SpiceInfo("Width dependence of tvfbsdoff")]
        public Parameter BSIM4wtvfbsdoff { get; } = new Parameter();
        [SpiceName("weu"), SpiceInfo("Width dependence of eu")]
        public Parameter BSIM4weu { get; } = new Parameter();
        [SpiceName("wucs"), SpiceInfo("Width dependence of ucs")]
        public Parameter BSIM4wucs { get; } = new Parameter();
        [SpiceName("wvfb"), SpiceInfo("Width dependence of vfb")]
        public Parameter BSIM4wvfb { get; } = new Parameter();
        [SpiceName("wcgsl"), SpiceInfo("Width dependence of cgsl")]
        public Parameter BSIM4wcgsl { get; } = new Parameter();
        [SpiceName("wcgdl"), SpiceInfo("Width dependence of cgdl")]
        public Parameter BSIM4wcgdl { get; } = new Parameter();
        [SpiceName("wckappas"), SpiceInfo("Width dependence of ckappas")]
        public Parameter BSIM4wckappas { get; } = new Parameter();
        [SpiceName("wckappad"), SpiceInfo("Width dependence of ckappad")]
        public Parameter BSIM4wckappad { get; } = new Parameter();
        [SpiceName("wcf"), SpiceInfo("Width dependence of cf")]
        public Parameter BSIM4wcf { get; } = new Parameter();
        [SpiceName("wclc"), SpiceInfo("Width dependence of clc")]
        public Parameter BSIM4wclc { get; } = new Parameter();
        [SpiceName("wcle"), SpiceInfo("Width dependence of cle")]
        public Parameter BSIM4wcle { get; } = new Parameter();
        [SpiceName("wvfbcv"), SpiceInfo("Width dependence of vfbcv")]
        public Parameter BSIM4wvfbcv { get; } = new Parameter();
        [SpiceName("wacde"), SpiceInfo("Width dependence of acde")]
        public Parameter BSIM4wacde { get; } = new Parameter();
        [SpiceName("wmoin"), SpiceInfo("Width dependence of moin")]
        public Parameter BSIM4wmoin { get; } = new Parameter();
        [SpiceName("wnoff"), SpiceInfo("Width dependence of noff")]
        public Parameter BSIM4wnoff { get; } = new Parameter();
        [SpiceName("wvoffcv"), SpiceInfo("Width dependence of voffcv")]
        public Parameter BSIM4wvoffcv { get; } = new Parameter();
        [SpiceName("pcdsc"), SpiceInfo("Cross-term dependence of cdsc")]
        public Parameter BSIM4pcdsc { get; } = new Parameter();
        [SpiceName("pcdscb"), SpiceInfo("Cross-term dependence of cdscb")]
        public Parameter BSIM4pcdscb { get; } = new Parameter();
        [SpiceName("pcdscd"), SpiceInfo("Cross-term dependence of cdscd")]
        public Parameter BSIM4pcdscd { get; } = new Parameter();
        [SpiceName("pcit"), SpiceInfo("Cross-term dependence of cit")]
        public Parameter BSIM4pcit { get; } = new Parameter();
        [SpiceName("pnfactor"), SpiceInfo("Cross-term dependence of nfactor")]
        public Parameter BSIM4pnfactor { get; } = new Parameter();
        [SpiceName("pxj"), SpiceInfo("Cross-term dependence of xj")]
        public Parameter BSIM4pxj { get; } = new Parameter();
        [SpiceName("pvsat"), SpiceInfo("Cross-term dependence of vsat")]
        public Parameter BSIM4pvsat { get; } = new Parameter();
        [SpiceName("pa0"), SpiceInfo("Cross-term dependence of a0")]
        public Parameter BSIM4pa0 { get; } = new Parameter();
        [SpiceName("pags"), SpiceInfo("Cross-term dependence of ags")]
        public Parameter BSIM4pags { get; } = new Parameter();
        [SpiceName("pa1"), SpiceInfo("Cross-term dependence of a1")]
        public Parameter BSIM4pa1 { get; } = new Parameter();
        [SpiceName("pa2"), SpiceInfo("Cross-term dependence of a2")]
        public Parameter BSIM4pa2 { get; } = new Parameter();
        [SpiceName("pat"), SpiceInfo("Cross-term dependence of at")]
        public Parameter BSIM4pat { get; } = new Parameter();
        [SpiceName("pketa"), SpiceInfo("Cross-term dependence of keta")]
        public Parameter BSIM4pketa { get; } = new Parameter();
        [SpiceName("pnsub"), SpiceInfo("Cross-term dependence of nsub")]
        public Parameter BSIM4pnsub { get; } = new Parameter();
        [SpiceName("pndep"), SpiceInfo("Cross-term dependence of ndep")]
        public double BSIM4_PNDEP
        {
            get => BSIM4pndep;
            set
            {
                BSIM4pndep.Set(value);
                if (BSIM4pndep > 1.0e20)
                    BSIM4pndep.Value *= 1.0e-6;
            }
        }
        public Parameter BSIM4pndep { get; } = new Parameter();
        [SpiceName("pnsd"), SpiceInfo("Cross-term dependence of nsd")]
        public double BSIM4_PNSD
        {
            get => BSIM4pnsd;
            set
            {
                BSIM4pnsd.Set(value);
                if (BSIM4pnsd > 1.0e23)
                    BSIM4pnsd.Value *= 1.0e-6;
            }
        }
        public Parameter BSIM4pnsd { get; } = new Parameter();
        [SpiceName("pngate"), SpiceInfo("Cross-term dependence of ngate")]
        public double BSIM4_PNGATE
        {
            get => BSIM4pngate;
            set
            {
                BSIM4pngate.Set(value);
                if (BSIM4pngate > 1.0e23)
                    BSIM4pngate.Value *= 1.0e-6;
            }
        }
        public Parameter BSIM4pngate { get; } = new Parameter();
        [SpiceName("pgamma1"), SpiceInfo("Cross-term dependence of gamma1")]
        public Parameter BSIM4pgamma1 { get; } = new Parameter();
        [SpiceName("pgamma2"), SpiceInfo("Cross-term dependence of gamma2")]
        public Parameter BSIM4pgamma2 { get; } = new Parameter();
        [SpiceName("pvbx"), SpiceInfo("Cross-term dependence of vbx")]
        public Parameter BSIM4pvbx { get; } = new Parameter();
        [SpiceName("pvbm"), SpiceInfo("Cross-term dependence of vbm")]
        public Parameter BSIM4pvbm { get; } = new Parameter();
        [SpiceName("pxt"), SpiceInfo("Cross-term dependence of xt")]
        public Parameter BSIM4pxt { get; } = new Parameter();
        [SpiceName("pk1"), SpiceInfo("Cross-term dependence of k1")]
        public Parameter BSIM4pk1 { get; } = new Parameter();
        [SpiceName("pkt1"), SpiceInfo("Cross-term dependence of kt1")]
        public Parameter BSIM4pkt1 { get; } = new Parameter();
        [SpiceName("pkt1l"), SpiceInfo("Cross-term dependence of kt1l")]
        public Parameter BSIM4pkt1l { get; } = new Parameter();
        [SpiceName("pkt2"), SpiceInfo("Cross-term dependence of kt2")]
        public Parameter BSIM4pkt2 { get; } = new Parameter();
        [SpiceName("pk2"), SpiceInfo("Cross-term dependence of k2")]
        public Parameter BSIM4pk2 { get; } = new Parameter();
        [SpiceName("pk3"), SpiceInfo("Cross-term dependence of k3")]
        public Parameter BSIM4pk3 { get; } = new Parameter();
        [SpiceName("pk3b"), SpiceInfo("Cross-term dependence of k3b")]
        public Parameter BSIM4pk3b { get; } = new Parameter();
        [SpiceName("plpe0"), SpiceInfo("Cross-term dependence of lpe0")]
        public Parameter BSIM4plpe0 { get; } = new Parameter();
        [SpiceName("plpeb"), SpiceInfo("Cross-term dependence of lpeb")]
        public Parameter BSIM4plpeb { get; } = new Parameter();
        [SpiceName("pdvtp0"), SpiceInfo("Cross-term dependence of dvtp0")]
        public Parameter BSIM4pdvtp0 { get; } = new Parameter();
        [SpiceName("pdvtp1"), SpiceInfo("Cross-term dependence of dvtp1")]
        public Parameter BSIM4pdvtp1 { get; } = new Parameter();
        [SpiceName("pdvtp2"), SpiceInfo("Cross-term dependence of dvtp2")]
        public Parameter BSIM4pdvtp2 { get; } = new Parameter();
        [SpiceName("pdvtp3"), SpiceInfo("Cross-term dependence of dvtp3")]
        public Parameter BSIM4pdvtp3 { get; } = new Parameter();
        [SpiceName("pdvtp4"), SpiceInfo("Cross-term dependence of dvtp4")]
        public Parameter BSIM4pdvtp4 { get; } = new Parameter();
        [SpiceName("pdvtp5"), SpiceInfo("Cross-term dependence of dvtp5")]
        public Parameter BSIM4pdvtp5 { get; } = new Parameter();
        [SpiceName("pw0"), SpiceInfo("Cross-term dependence of w0")]
        public Parameter BSIM4pw0 { get; } = new Parameter();
        [SpiceName("pdvt0"), SpiceInfo("Cross-term dependence of dvt0")]
        public Parameter BSIM4pdvt0 { get; } = new Parameter();
        [SpiceName("pdvt1"), SpiceInfo("Cross-term dependence of dvt1")]
        public Parameter BSIM4pdvt1 { get; } = new Parameter();
        [SpiceName("pdvt2"), SpiceInfo("Cross-term dependence of dvt2")]
        public Parameter BSIM4pdvt2 { get; } = new Parameter();
        [SpiceName("pdvt0w"), SpiceInfo("Cross-term dependence of dvt0w")]
        public Parameter BSIM4pdvt0w { get; } = new Parameter();
        [SpiceName("pdvt1w"), SpiceInfo("Cross-term dependence of dvt1w")]
        public Parameter BSIM4pdvt1w { get; } = new Parameter();
        [SpiceName("pdvt2w"), SpiceInfo("Cross-term dependence of dvt2w")]
        public Parameter BSIM4pdvt2w { get; } = new Parameter();
        [SpiceName("pdrout"), SpiceInfo("Cross-term dependence of drout")]
        public Parameter BSIM4pdrout { get; } = new Parameter();
        [SpiceName("pdsub"), SpiceInfo("Cross-term dependence of dsub")]
        public Parameter BSIM4pdsub { get; } = new Parameter();
        [SpiceName("pvth0"), SpiceName("pvtho"), SpiceInfo("Cross-term dependence of vto")]
        public Parameter BSIM4pvth0 { get; } = new Parameter();
        [SpiceName("pua"), SpiceInfo("Cross-term dependence of ua")]
        public Parameter BSIM4pua { get; } = new Parameter();
        [SpiceName("pua1"), SpiceInfo("Cross-term dependence of ua1")]
        public Parameter BSIM4pua1 { get; } = new Parameter();
        [SpiceName("pub"), SpiceInfo("Cross-term dependence of ub")]
        public Parameter BSIM4pub { get; } = new Parameter();
        [SpiceName("pub1"), SpiceInfo("Cross-term dependence of ub1")]
        public Parameter BSIM4pub1 { get; } = new Parameter();
        [SpiceName("puc"), SpiceInfo("Cross-term dependence of uc")]
        public Parameter BSIM4puc { get; } = new Parameter();
        [SpiceName("puc1"), SpiceInfo("Cross-term dependence of uc1")]
        public Parameter BSIM4puc1 { get; } = new Parameter();
        [SpiceName("pu0"), SpiceInfo("Cross-term dependence of u0")]
        public Parameter BSIM4pu0 { get; } = new Parameter();
        [SpiceName("pute"), SpiceInfo("Cross-term dependence of ute")]
        public Parameter BSIM4pute { get; } = new Parameter();
        [SpiceName("pucste"), SpiceInfo("Cross-term dependence of ucste")]
        public Parameter BSIM4pucste { get; } = new Parameter();
        [SpiceName("pvoff"), SpiceInfo("Cross-term dependence of voff")]
        public Parameter BSIM4pvoff { get; } = new Parameter();
        [SpiceName("ptvoff"), SpiceInfo("Cross-term dependence of tvoff")]
        public Parameter BSIM4ptvoff { get; } = new Parameter();
        [SpiceName("ptnfactor"), SpiceInfo("Cross-term dependence of tnfactor")]
        public Parameter BSIM4ptnfactor { get; } = new Parameter();
        [SpiceName("pteta0"), SpiceInfo("Cross-term dependence of teta0")]
        public Parameter BSIM4pteta0 { get; } = new Parameter();
        [SpiceName("ptvoffcv"), SpiceInfo("Cross-term dependence of tvoffcv")]
        public Parameter BSIM4ptvoffcv { get; } = new Parameter();
        [SpiceName("pminv"), SpiceInfo("Cross-term dependence of minv")]
        public Parameter BSIM4pminv { get; } = new Parameter();
        [SpiceName("pminvcv"), SpiceInfo("Cross-term dependence of minvcv")]
        public Parameter BSIM4pminvcv { get; } = new Parameter();
        [SpiceName("pfprout"), SpiceInfo("Cross-term dependence of pdiblcb")]
        public Parameter BSIM4pfprout { get; } = new Parameter();
        [SpiceName("ppdits"), SpiceInfo("Cross-term dependence of pdits")]
        public Parameter BSIM4ppdits { get; } = new Parameter();
        [SpiceName("ppditsd"), SpiceInfo("Cross-term dependence of pditsd")]
        public Parameter BSIM4ppditsd { get; } = new Parameter();
        [SpiceName("pdelta"), SpiceInfo("Cross-term dependence of delta")]
        public Parameter BSIM4pdelta { get; } = new Parameter();
        [SpiceName("prdsw"), SpiceInfo("Cross-term dependence of rdsw ")]
        public Parameter BSIM4prdsw { get; } = new Parameter();
        [SpiceName("prdw"), SpiceInfo("Cross-term dependence of rdw")]
        public Parameter BSIM4prdw { get; } = new Parameter();
        [SpiceName("prsw"), SpiceInfo("Cross-term dependence of rsw")]
        public Parameter BSIM4prsw { get; } = new Parameter();
        [SpiceName("pprwb"), SpiceInfo("Cross-term dependence of prwb ")]
        public Parameter BSIM4pprwb { get; } = new Parameter();
        [SpiceName("pprwg"), SpiceInfo("Cross-term dependence of prwg ")]
        public Parameter BSIM4pprwg { get; } = new Parameter();
        [SpiceName("pprt"), SpiceInfo("Cross-term dependence of prt ")]
        public Parameter BSIM4pprt { get; } = new Parameter();
        [SpiceName("peta0"), SpiceInfo("Cross-term dependence of eta0")]
        public Parameter BSIM4peta0 { get; } = new Parameter();
        [SpiceName("petab"), SpiceInfo("Cross-term dependence of etab")]
        public Parameter BSIM4petab { get; } = new Parameter();
        [SpiceName("ppclm"), SpiceInfo("Cross-term dependence of pclm")]
        public Parameter BSIM4ppclm { get; } = new Parameter();
        [SpiceName("ppdiblc1"), SpiceInfo("Cross-term dependence of pdiblc1")]
        public Parameter BSIM4ppdibl1 { get; } = new Parameter();
        [SpiceName("ppdiblc2"), SpiceInfo("Cross-term dependence of pdiblc2")]
        public Parameter BSIM4ppdibl2 { get; } = new Parameter();
        [SpiceName("ppdiblcb"), SpiceInfo("Cross-term dependence of pdiblcb")]
        public Parameter BSIM4ppdiblb { get; } = new Parameter();
        [SpiceName("ppscbe1"), SpiceInfo("Cross-term dependence of pscbe1")]
        public Parameter BSIM4ppscbe1 { get; } = new Parameter();
        [SpiceName("ppscbe2"), SpiceInfo("Cross-term dependence of pscbe2")]
        public Parameter BSIM4ppscbe2 { get; } = new Parameter();
        [SpiceName("ppvag"), SpiceInfo("Cross-term dependence of pvag")]
        public Parameter BSIM4ppvag { get; } = new Parameter();
        [SpiceName("pwr"), SpiceInfo("Cross-term dependence of wr")]
        public Parameter BSIM4pwr { get; } = new Parameter();
        [SpiceName("pdwg"), SpiceInfo("Cross-term dependence of dwg")]
        public Parameter BSIM4pdwg { get; } = new Parameter();
        [SpiceName("pdwb"), SpiceInfo("Cross-term dependence of dwb")]
        public Parameter BSIM4pdwb { get; } = new Parameter();
        [SpiceName("pb0"), SpiceInfo("Cross-term dependence of b0")]
        public Parameter BSIM4pb0 { get; } = new Parameter();
        [SpiceName("pb1"), SpiceInfo("Cross-term dependence of b1")]
        public Parameter BSIM4pb1 { get; } = new Parameter();
        [SpiceName("palpha0"), SpiceInfo("Cross-term dependence of alpha0")]
        public Parameter BSIM4palpha0 { get; } = new Parameter();
        [SpiceName("palpha1"), SpiceInfo("Cross-term dependence of alpha1")]
        public Parameter BSIM4palpha1 { get; } = new Parameter();
        [SpiceName("pbeta0"), SpiceInfo("Cross-term dependence of beta0")]
        public Parameter BSIM4pbeta0 { get; } = new Parameter();
        [SpiceName("pphin"), SpiceInfo("Cross-term dependence of phin")]
        public Parameter BSIM4pphin { get; } = new Parameter();
        [SpiceName("pagidl"), SpiceInfo("Cross-term dependence of agidl")]
        public Parameter BSIM4pagidl { get; } = new Parameter();
        [SpiceName("pbgidl"), SpiceInfo("Cross-term dependence of bgidl")]
        public Parameter BSIM4pbgidl { get; } = new Parameter();
        [SpiceName("pcgidl"), SpiceInfo("Cross-term dependence of cgidl")]
        public Parameter BSIM4pcgidl { get; } = new Parameter();
        [SpiceName("pegidl"), SpiceInfo("Cross-term dependence of egidl")]
        public Parameter BSIM4pegidl { get; } = new Parameter();
        [SpiceName("pfgidl"), SpiceInfo("Cross-term dependence of fgidl")]
        public Parameter BSIM4pfgidl { get; } = new Parameter();
        [SpiceName("pkgidl"), SpiceInfo("Cross-term dependence of kgidl")]
        public Parameter BSIM4pkgidl { get; } = new Parameter();
        [SpiceName("prgidl"), SpiceInfo("Cross-term dependence of rgidl")]
        public Parameter BSIM4prgidl { get; } = new Parameter();
        [SpiceName("pagisl"), SpiceInfo("Cross-term dependence of agisl")]
        public Parameter BSIM4pagisl { get; } = new Parameter();
        [SpiceName("pbgisl"), SpiceInfo("Cross-term dependence of bgisl")]
        public Parameter BSIM4pbgisl { get; } = new Parameter();
        [SpiceName("pcgisl"), SpiceInfo("Cross-term dependence of cgisl")]
        public Parameter BSIM4pcgisl { get; } = new Parameter();
        [SpiceName("pegisl"), SpiceInfo("Cross-term dependence of egisl")]
        public Parameter BSIM4pegisl { get; } = new Parameter();
        [SpiceName("pfgisl"), SpiceInfo("Cross-term dependence of fgisl")]
        public Parameter BSIM4pfgisl { get; } = new Parameter();
        [SpiceName("pkgisl"), SpiceInfo("Cross-term dependence of kgisl")]
        public Parameter BSIM4pkgisl { get; } = new Parameter();
        [SpiceName("prgisl"), SpiceInfo("Cross-term dependence of rgisl")]
        public Parameter BSIM4prgisl { get; } = new Parameter();
        [SpiceName("paigc"), SpiceInfo("Cross-term dependence of aigc")]
        public Parameter BSIM4paigc { get; } = new Parameter();
        [SpiceName("pbigc"), SpiceInfo("Cross-term dependence of bigc")]
        public Parameter BSIM4pbigc { get; } = new Parameter();
        [SpiceName("pcigc"), SpiceInfo("Cross-term dependence of cigc")]
        public Parameter BSIM4pcigc { get; } = new Parameter();
        [SpiceName("paigsd"), SpiceInfo("Cross-term dependence of aigsd")]
        public Parameter BSIM4paigsd { get; } = new Parameter();
        [SpiceName("pbigsd"), SpiceInfo("Cross-term dependence of bigsd")]
        public Parameter BSIM4pbigsd { get; } = new Parameter();
        [SpiceName("pcigsd"), SpiceInfo("Cross-term dependence of cigsd")]
        public Parameter BSIM4pcigsd { get; } = new Parameter();
        [SpiceName("paigs"), SpiceInfo("Cross-term dependence of aigs")]
        public Parameter BSIM4paigs { get; } = new Parameter();
        [SpiceName("pbigs"), SpiceInfo("Cross-term dependence of bigs")]
        public Parameter BSIM4pbigs { get; } = new Parameter();
        [SpiceName("pcigs"), SpiceInfo("Cross-term dependence of cigs")]
        public Parameter BSIM4pcigs { get; } = new Parameter();
        [SpiceName("paigd"), SpiceInfo("Cross-term dependence of aigd")]
        public Parameter BSIM4paigd { get; } = new Parameter();
        [SpiceName("pbigd"), SpiceInfo("Cross-term dependence of bigd")]
        public Parameter BSIM4pbigd { get; } = new Parameter();
        [SpiceName("pcigd"), SpiceInfo("Cross-term dependence of cigd")]
        public Parameter BSIM4pcigd { get; } = new Parameter();
        [SpiceName("paigbacc"), SpiceInfo("Cross-term dependence of aigbacc")]
        public Parameter BSIM4paigbacc { get; } = new Parameter();
        [SpiceName("pbigbacc"), SpiceInfo("Cross-term dependence of bigbacc")]
        public Parameter BSIM4pbigbacc { get; } = new Parameter();
        [SpiceName("pcigbacc"), SpiceInfo("Cross-term dependence of cigbacc")]
        public Parameter BSIM4pcigbacc { get; } = new Parameter();
        [SpiceName("paigbinv"), SpiceInfo("Cross-term dependence of aigbinv")]
        public Parameter BSIM4paigbinv { get; } = new Parameter();
        [SpiceName("pbigbinv"), SpiceInfo("Cross-term dependence of bigbinv")]
        public Parameter BSIM4pbigbinv { get; } = new Parameter();
        [SpiceName("pcigbinv"), SpiceInfo("Cross-term dependence of cigbinv")]
        public Parameter BSIM4pcigbinv { get; } = new Parameter();
        [SpiceName("pnigc"), SpiceInfo("Cross-term dependence of nigc")]
        public Parameter BSIM4pnigc { get; } = new Parameter();
        [SpiceName("pnigbinv"), SpiceInfo("Cross-term dependence of nigbinv")]
        public Parameter BSIM4pnigbinv { get; } = new Parameter();
        [SpiceName("pnigbacc"), SpiceInfo("Cross-term dependence of nigbacc")]
        public Parameter BSIM4pnigbacc { get; } = new Parameter();
        [SpiceName("pntox"), SpiceInfo("Cross-term dependence of ntox")]
        public Parameter BSIM4pntox { get; } = new Parameter();
        [SpiceName("peigbinv"), SpiceInfo("Cross-term dependence for eigbinv")]
        public Parameter BSIM4peigbinv { get; } = new Parameter();
        [SpiceName("ppigcd"), SpiceInfo("Cross-term dependence for pigcd")]
        public Parameter BSIM4ppigcd { get; } = new Parameter();
        [SpiceName("ppoxedge"), SpiceInfo("Cross-term dependence for poxedge")]
        public Parameter BSIM4ppoxedge { get; } = new Parameter();
        [SpiceName("pxrcrg1"), SpiceInfo("Cross-term dependence of xrcrg1")]
        public Parameter BSIM4pxrcrg1 { get; } = new Parameter();
        [SpiceName("pxrcrg2"), SpiceInfo("Cross-term dependence of xrcrg2")]
        public Parameter BSIM4pxrcrg2 { get; } = new Parameter();
        [SpiceName("plambda"), SpiceInfo("Cross-term dependence of lambda")]
        public Parameter BSIM4plambda { get; } = new Parameter();
        [SpiceName("pvtl"), SpiceInfo("Cross-term dependence of vtl")]
        public Parameter BSIM4pvtl { get; } = new Parameter();
        [SpiceName("pxn"), SpiceInfo("Cross-term dependence of xn")]
        public Parameter BSIM4pxn { get; } = new Parameter();
        [SpiceName("pvfbsdoff"), SpiceInfo("Cross-term dependence of vfbsdoff")]
        public Parameter BSIM4pvfbsdoff { get; } = new Parameter();
        [SpiceName("ptvfbsdoff"), SpiceInfo("Cross-term dependence of tvfbsdoff")]
        public Parameter BSIM4ptvfbsdoff { get; } = new Parameter();
        [SpiceName("peu"), SpiceInfo("Cross-term dependence of eu")]
        public Parameter BSIM4peu { get; } = new Parameter();
        [SpiceName("pucs"), SpiceInfo("Cross-term dependence of ucs")]
        public Parameter BSIM4pucs { get; } = new Parameter();
        [SpiceName("pvfb"), SpiceInfo("Cross-term dependence of vfb")]
        public Parameter BSIM4pvfb { get; } = new Parameter();
        [SpiceName("pcgsl"), SpiceInfo("Cross-term dependence of cgsl")]
        public Parameter BSIM4pcgsl { get; } = new Parameter();
        [SpiceName("pcgdl"), SpiceInfo("Cross-term dependence of cgdl")]
        public Parameter BSIM4pcgdl { get; } = new Parameter();
        [SpiceName("pckappas"), SpiceInfo("Cross-term dependence of ckappas")]
        public Parameter BSIM4pckappas { get; } = new Parameter();
        [SpiceName("pckappad"), SpiceInfo("Cross-term dependence of ckappad")]
        public Parameter BSIM4pckappad { get; } = new Parameter();
        [SpiceName("pcf"), SpiceInfo("Cross-term dependence of cf")]
        public Parameter BSIM4pcf { get; } = new Parameter();
        [SpiceName("pclc"), SpiceInfo("Cross-term dependence of clc")]
        public Parameter BSIM4pclc { get; } = new Parameter();
        [SpiceName("pcle"), SpiceInfo("Cross-term dependence of cle")]
        public Parameter BSIM4pcle { get; } = new Parameter();
        [SpiceName("pvfbcv"), SpiceInfo("Cross-term dependence of vfbcv")]
        public Parameter BSIM4pvfbcv { get; } = new Parameter();
        [SpiceName("pacde"), SpiceInfo("Cross-term dependence of acde")]
        public Parameter BSIM4pacde { get; } = new Parameter();
        [SpiceName("pmoin"), SpiceInfo("Cross-term dependence of moin")]
        public Parameter BSIM4pmoin { get; } = new Parameter();
        [SpiceName("pnoff"), SpiceInfo("Cross-term dependence of noff")]
        public Parameter BSIM4pnoff { get; } = new Parameter();
        [SpiceName("pvoffcv"), SpiceInfo("Cross-term dependence of voffcv")]
        public Parameter BSIM4pvoffcv { get; } = new Parameter();
        [SpiceName("tnom"), SpiceInfo("Parameter measurement temperature")]
        public double BSIM4_TNOM
        {
            get => BSIM4tnom - Circuit.CONSTCtoK;
            set => BSIM4tnom.Set(value + Circuit.CONSTCtoK);
        }
        public Parameter BSIM4tnom { get; } = new Parameter();
        [SpiceName("cgso"), SpiceInfo("Gate-source overlap capacitance per width")]
        public Parameter BSIM4cgso { get; } = new Parameter();
        [SpiceName("cgdo"), SpiceInfo("Gate-drain overlap capacitance per width")]
        public Parameter BSIM4cgdo { get; } = new Parameter();
        [SpiceName("cgbo"), SpiceInfo("Gate-bulk overlap capacitance per length")]
        public Parameter BSIM4cgbo { get; } = new Parameter();
        [SpiceName("xpart"), SpiceInfo("Channel charge partitioning")]
        public Parameter BSIM4xpart { get; } = new Parameter();
        [SpiceName("rsh"), SpiceInfo("Source-drain sheet resistance")]
        public Parameter BSIM4sheetResistance { get; } = new Parameter();
        [SpiceName("jss"), SpiceInfo("Bottom source junction reverse saturation current density")]
        public Parameter BSIM4SjctSatCurDensity { get; } = new Parameter(1.0E-4);
        [SpiceName("jsws"), SpiceInfo("Isolation edge sidewall source junction reverse saturation current density")]
        public Parameter BSIM4SjctSidewallSatCurDensity { get; } = new Parameter();
        [SpiceName("jswgs"), SpiceInfo("Gate edge source junction reverse saturation current density")]
        public Parameter BSIM4SjctGateSidewallSatCurDensity { get; } = new Parameter();
        [SpiceName("pbs"), SpiceInfo("Source junction built-in potential")]
        public Parameter BSIM4SbulkJctPotential { get; } = new Parameter(1.0);
        [SpiceName("mjs"), SpiceInfo("Source bottom junction capacitance grading coefficient")]
        public Parameter BSIM4SbulkJctBotGradingCoeff { get; } = new Parameter(0.5);
        [SpiceName("pbsws"), SpiceInfo("Source sidewall junction capacitance built in potential")]
        public Parameter BSIM4SsidewallJctPotential { get; } = new Parameter(1.0);
        [SpiceName("mjsws"), SpiceInfo("Source sidewall junction capacitance grading coefficient")]
        public Parameter BSIM4SbulkJctSideGradingCoeff { get; } = new Parameter(0.33);
        [SpiceName("cjs"), SpiceInfo("Source bottom junction capacitance per unit area")]
        public Parameter BSIM4SunitAreaJctCap { get; } = new Parameter(5.0E-4);
        [SpiceName("cjsws"), SpiceInfo("Source sidewall junction capacitance per unit periphery")]
        public Parameter BSIM4SunitLengthSidewallJctCap { get; } = new Parameter(5.0E-10);
        [SpiceName("njs"), SpiceInfo("Source junction emission coefficient")]
        public Parameter BSIM4SjctEmissionCoeff { get; } = new Parameter(1.0);
        [SpiceName("pbswgs"), SpiceInfo("Source (gate side) sidewall junction capacitance built in potential")]
        public Parameter BSIM4SGatesidewallJctPotential { get; } = new Parameter();
        [SpiceName("mjswgs"), SpiceInfo("Source (gate side) sidewall junction capacitance grading coefficient")]
        public Parameter BSIM4SbulkJctGateSideGradingCoeff { get; } = new Parameter();
        [SpiceName("cjswgs"), SpiceInfo("Source (gate side) sidewall junction capacitance per unit width")]
        public Parameter BSIM4SunitLengthGateSidewallJctCap { get; } = new Parameter();
        [SpiceName("xtis"), SpiceInfo("Source junction current temperature exponent")]
        public Parameter BSIM4SjctTempExponent { get; } = new Parameter(3.0);
        [SpiceName("jsd"), SpiceInfo("Bottom drain junction reverse saturation current density")]
        public Parameter BSIM4DjctSatCurDensity { get; } = new Parameter();
        [SpiceName("jswd"), SpiceInfo("Isolation edge sidewall drain junction reverse saturation current density")]
        public Parameter BSIM4DjctSidewallSatCurDensity { get; } = new Parameter();
        [SpiceName("jswgd"), SpiceInfo("Gate edge drain junction reverse saturation current density")]
        public Parameter BSIM4DjctGateSidewallSatCurDensity { get; } = new Parameter();
        [SpiceName("pbd"), SpiceInfo("Drain junction built-in potential")]
        public Parameter BSIM4DbulkJctPotential { get; } = new Parameter();
        [SpiceName("mjd"), SpiceInfo("Drain bottom junction capacitance grading coefficient")]
        public Parameter BSIM4DbulkJctBotGradingCoeff { get; } = new Parameter();
        [SpiceName("pbswd"), SpiceInfo("Drain sidewall junction capacitance built in potential")]
        public Parameter BSIM4DsidewallJctPotential { get; } = new Parameter();
        [SpiceName("mjswd"), SpiceInfo("Drain sidewall junction capacitance grading coefficient")]
        public Parameter BSIM4DbulkJctSideGradingCoeff { get; } = new Parameter();
        [SpiceName("cjd"), SpiceInfo("Drain bottom junction capacitance per unit area")]
        public Parameter BSIM4DunitAreaJctCap { get; } = new Parameter();
        [SpiceName("cjswd"), SpiceInfo("Drain sidewall junction capacitance per unit periphery")]
        public Parameter BSIM4DunitLengthSidewallJctCap { get; } = new Parameter();
        [SpiceName("njd"), SpiceInfo("Drain junction emission coefficient")]
        public Parameter BSIM4DjctEmissionCoeff { get; } = new Parameter();
        [SpiceName("pbswgd"), SpiceInfo("Drain (gate side) sidewall junction capacitance built in potential")]
        public Parameter BSIM4DGatesidewallJctPotential { get; } = new Parameter();
        [SpiceName("mjswgd"), SpiceInfo("Drain (gate side) sidewall junction capacitance grading coefficient")]
        public Parameter BSIM4DbulkJctGateSideGradingCoeff { get; } = new Parameter();
        [SpiceName("cjswgd"), SpiceInfo("Drain (gate side) sidewall junction capacitance per unit width")]
        public Parameter BSIM4DunitLengthGateSidewallJctCap { get; } = new Parameter();
        [SpiceName("xtid"), SpiceInfo("Drainjunction current temperature exponent")]
        public Parameter BSIM4DjctTempExponent { get; } = new Parameter();
        [SpiceName("lint"), SpiceInfo("Length reduction parameter")]
        public Parameter BSIM4Lint { get; } = new Parameter();
        [SpiceName("ll"), SpiceInfo("Length reduction parameter")]
        public Parameter BSIM4Ll { get; } = new Parameter();
        [SpiceName("llc"), SpiceInfo("Length reduction parameter for CV")]
        public Parameter BSIM4Llc { get; } = new Parameter();
        [SpiceName("lln"), SpiceInfo("Length reduction parameter")]
        public Parameter BSIM4Lln { get; } = new Parameter(1.0);
        [SpiceName("lw"), SpiceInfo("Length reduction parameter")]
        public Parameter BSIM4Lw { get; } = new Parameter();
        [SpiceName("lwc"), SpiceInfo("Length reduction parameter for CV")]
        public Parameter BSIM4Lwc { get; } = new Parameter();
        [SpiceName("lwn"), SpiceInfo("Length reduction parameter")]
        public Parameter BSIM4Lwn { get; } = new Parameter(1.0);
        [SpiceName("lwl"), SpiceInfo("Length reduction parameter")]
        public Parameter BSIM4Lwl { get; } = new Parameter();
        [SpiceName("lwlc"), SpiceInfo("Length reduction parameter for CV")]
        public Parameter BSIM4Lwlc { get; } = new Parameter();
        [SpiceName("lmin"), SpiceInfo("Minimum length for the model")]
        public Parameter BSIM4Lmin { get; } = new Parameter();
        [SpiceName("lmax"), SpiceInfo("Maximum length for the model")]
        public Parameter BSIM4Lmax { get; } = new Parameter(1.0);
        [SpiceName("wint"), SpiceInfo("Width reduction parameter")]
        public Parameter BSIM4Wint { get; } = new Parameter();
        [SpiceName("wl"), SpiceInfo("Width reduction parameter")]
        public Parameter BSIM4Wl { get; } = new Parameter();
        [SpiceName("wlc"), SpiceInfo("Width reduction parameter for CV")]
        public Parameter BSIM4Wlc { get; } = new Parameter();
        [SpiceName("wln"), SpiceInfo("Width reduction parameter")]
        public Parameter BSIM4Wln { get; } = new Parameter(1.0);
        [SpiceName("ww"), SpiceInfo("Width reduction parameter")]
        public Parameter BSIM4Ww { get; } = new Parameter();
        [SpiceName("wwc"), SpiceInfo("Width reduction parameter for CV")]
        public Parameter BSIM4Wwc { get; } = new Parameter();
        [SpiceName("wwn"), SpiceInfo("Width reduction parameter")]
        public Parameter BSIM4Wwn { get; } = new Parameter(1.0);
        [SpiceName("wwl"), SpiceInfo("Width reduction parameter")]
        public Parameter BSIM4Wwl { get; } = new Parameter();
        [SpiceName("wwlc"), SpiceInfo("Width reduction parameter for CV")]
        public Parameter BSIM4Wwlc { get; } = new Parameter();
        [SpiceName("wmin"), SpiceInfo("Minimum width for the model")]
        public Parameter BSIM4Wmin { get; } = new Parameter();
        [SpiceName("wmax"), SpiceInfo("Maximum width for the model")]
        public Parameter BSIM4Wmax { get; } = new Parameter(1.0);
        [SpiceName("noia"), SpiceInfo("Flicker noise parameter")]
        public Parameter BSIM4oxideTrapDensityA { get; } = new Parameter();
        [SpiceName("noib"), SpiceInfo("Flicker noise parameter")]
        public Parameter BSIM4oxideTrapDensityB { get; } = new Parameter();
        [SpiceName("noic"), SpiceInfo("Flicker noise parameter")]
        public Parameter BSIM4oxideTrapDensityC { get; } = new Parameter(8.75e9);
        [SpiceName("em"), SpiceInfo("Flicker noise parameter")]
        public Parameter BSIM4em { get; } = new Parameter(4.1e7);
        [SpiceName("ef"), SpiceInfo("Flicker noise frequency exponent")]
        public Parameter BSIM4ef { get; } = new Parameter(1.0);
        [SpiceName("af"), SpiceInfo("Flicker noise exponent")]
        public Parameter BSIM4af { get; } = new Parameter(1.0);
        [SpiceName("kf"), SpiceInfo("Flicker noise coefficient")]
        public Parameter BSIM4kf { get; } = new Parameter();

        /// <summary>
        /// Methods
        /// </summary>
        [SpiceName("nmos"), SpiceInfo("Flag to indicate NMOS")]
        public void SetNMOS(bool value)
        {
            if (value)
                BSIM4type = 1.0;
        }
        [SpiceName("pmos"), SpiceInfo("Flag to indicate PMOS")]
        public void SetPMOS(bool value)
        {
            if (value)
                BSIM4type = -1.0;
        }

        /// <summary>
        /// Shared parameters
        /// </summary>
        internal double DMCGeff, DMCIeff, DMDGeff, Temp, epsrox, toxe, 
            epssub, Tnom, TRatio, Vtm0, Eg0, ni, delTemp;

        /// <summary>
        /// Extra variables
        /// </summary>
        public double BSIM4type { get; internal set; } = 1.0;
        public double BSIM4coxe { get; internal set; }
        public double BSIM4factor1 { get; internal set; }
        public double BSIM4vtm { get; internal set; }
        public double BSIM4SjctTempSatCurDensity { get; internal set; }
        public double BSIM4SjctSidewallTempSatCurDensity { get; internal set; }
        public double BSIM4SjctGateSidewallTempSatCurDensity { get; internal set; }
        public double BSIM4DjctTempSatCurDensity { get; internal set; }
        public double BSIM4DjctSidewallTempSatCurDensity { get; internal set; }
        public double BSIM4DjctGateSidewallTempSatCurDensity { get; internal set; }
        public double BSIM4njtsstemp { get; internal set; }
        public double BSIM4njtsswstemp { get; internal set; }
        public double BSIM4njtsswgstemp { get; internal set; }
        public double BSIM4njtsdtemp { get; internal set; }
        public double BSIM4njtsswdtemp { get; internal set; }
        public double BSIM4njtsswgdtemp { get; internal set; }
        public double BSIM4coxp { get; internal set; }
        public double BSIM4vcrit { get; internal set; }
        public double BSIM4vtm0 { get; internal set; }
        public double BSIM4Eg0 { get; internal set; }
        public double BSIM4SunitAreaTempJctCap { get; internal set; }
        public double BSIM4DunitAreaTempJctCap { get; internal set; }
        public double BSIM4SunitLengthSidewallTempJctCap { get; internal set; }
        public double BSIM4DunitLengthSidewallTempJctCap { get; internal set; }
        public double BSIM4SunitLengthGateSidewallTempJctCap { get; internal set; }
        public double BSIM4DunitLengthGateSidewallTempJctCap { get; internal set; }
        public double BSIM4PhiBS { get; internal set; }
        public double BSIM4PhiBD { get; internal set; }
        public double BSIM4PhiBSWS { get; internal set; }
        public double BSIM4PhiBSWD { get; internal set; }
        public double BSIM4PhiBSWGS { get; internal set; }
        public double BSIM4PhiBSWGD { get; internal set; }

        private const double NMOS = 1.0;
        private const double PMOS = -1.0;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the device</param>
        public BSIM4v80Model(CircuitIdentifier name) : base(name)
        {
        }

        /// <summary>
        /// Setup the device
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Setup(Circuit ckt)
        {
            Sizes.Clear();

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
            /* WDLiu: tnoiMod = 1 needs to set internal S / D nodes */
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
            /* unit Q / V / m^2 */
            /* unit Q / V / m^2 */
            /* unit Q / V / m^2 */
            /* unit Q / V / m^2 */
            /* unit m / s */
            /* unit m / s */
            /* unit / V */
            /* unit 1 / cm3 */
            /* unit 1 / cm3 */
            /* unit eV */
            /* unit 1 / cm3 */
            /* unit 1 / cm3 */
            /* unit V */
            /* unit 1 / cm3 */
            /* unit V */
            /* unit V * m */
            /* No unit */
            if (!BSIM4dvtp2.Given)
                /* New DIBL / Rout */
                BSIM4dvtp2.Value = 0.0;
            /* unit 1 / V */

            if (!BSIM4dsub.Given)
                BSIM4dsub.Value = BSIM4drout;
            if (!BSIM4vth0.Given)
                BSIM4vth0.Value = (BSIM4type == NMOS) ? 0.7 : -0.7;
            if (!BSIM4eu.Given)
                BSIM4eu.Value = (BSIM4type == NMOS) ? 1.67 : 1.0;
            if (!BSIM4ucs.Given)
                BSIM4ucs.Value = (BSIM4type == NMOS) ? 1.67 : 1.0;
            if (!BSIM4ua.Given)
                BSIM4ua.Value = ((BSIM4mobMod.Value == 2)) ? 1.0e-15 : 1.0e-9; /* unit m / V */
                                                                               /* unit m / V */
                                                                               /* unit (m / V) *  * 2 */
                                                                               /* unit (m / V) *  * 2 */
            if (!BSIM4uc.Given)
                BSIM4uc.Value = (BSIM4mobMod.Value == 1) ? -0.0465 : -0.0465e-9;
            if (!BSIM4uc1.Given)
                BSIM4uc1.Value = (BSIM4mobMod.Value == 1) ? -0.056 : -0.056e-9;
            /* unit m *  * (-2) */
            if (!BSIM4u0.Given)
                BSIM4u0.Value = (BSIM4type == NMOS) ? 0.067 : 0.025;
            /* in ohm * um */
            /* in 1 / V */
            /* no unit */
            /* unit  1 / V */
            /* no unit */
            /* no unit */
            /* no unit */
            /* 1 / V */
            /* v4.7 New GIDL / GISL */
            /* V / m */
            /* V^3 */
            /* V */
            if (!BSIM4rgidl.Given)
                /* v4.7 New GIDL / GISL */
                BSIM4rgidl.Value = 1.0;
            if (!BSIM4kgidl.Given)
                /* v4.7 New GIDL / GISL */
                BSIM4kgidl.Value = 0.0;
            if (!BSIM4fgidl.Given)
                /* v4.7 New GIDL / GISL */
                /* BSIM4fgidl.Value = 0.0;
                /* Default value of fgdil set to 1 in BSIM4.8.0 */
                BSIM4fgidl.Value = 1.0;

            /* if (!BSIM4agisl.Given)
			{
				if (BSIM4agidl.Given)
				BSIM4agisl.Value = BSIM4agidl;
				else
				BSIM4agisl.Value = 0.0;
			} */

            /* Default value of agidl being 0, agisl set as follows */

            /* if (!BSIM4bgisl.Given)
			{
				if (BSIM4bgidl.Given)
				BSIM4bgisl.Value = BSIM4bgidl;
				else
				BSIM4bgisl.Value = 2.3e9; 
			} */

            /* Default value of bgidl being 2.3e9, bgisl set as follows */

            /* if (!BSIM4cgisl.Given)
			{
				if (BSIM4cgidl.Given)
				BSIM4cgisl.Value = BSIM4cgidl;
				else
				BSIM4cgisl.Value = 0.5; 
			} */

            /* Default value of cgidl being 0.5, cgisl set as follows */

            /* if (!BSIM4egisl.Given)
			{
				if (BSIM4egidl.Given)
				BSIM4egisl.Value = BSIM4egidl;
				else
				BSIM4egisl.Value = 0.8; 
			} */

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
            /* unit A */
            if (!BSIM4ijthdfwd.Given)
                BSIM4ijthdfwd.Value = BSIM4ijthsfwd;
            /* unit A */
            if (!BSIM4ijthdrev.Given)
                BSIM4ijthdrev.Value = BSIM4ijthsrev;
            /* unit m / s */
            /* unit v */
            if (!BSIM4tnfactor.Given)
                /* v4.7 temp dep of leakage current */
                BSIM4tnfactor.Value = 0.0;
            if (!BSIM4teta0.Given)
                /* v4.7 temp dep of leakage current */
                BSIM4teta0.Value = 0.0;
            if (!BSIM4tvoffcv.Given)
                /* v4.7 temp dep of leakage current */
                BSIM4tvoffcv.Value = 0.0;

            /* unit m */

            /* no unit */
            if (!BSIM4xjbvd.Given)
                BSIM4xjbvd.Value = BSIM4xjbvs;
            /* V */
            if (!BSIM4bvd.Given)
                BSIM4bvd.Value = BSIM4bvs;

            /* in mho */
            /* in ohm */
            if (!BSIM4ckappad.Given)
                BSIM4ckappad.Value = BSIM4ckappas;
            if (!BSIM4dmci.Given)
                BSIM4dmci.Value = BSIM4dmcg;
            /* Length dependence */
            if (!BSIM4lk1.Given)
                BSIM4lkt1.Value = 0.0;
            if (!BSIM4ldvtp2.Given)
                /* New DIBL / Rout */
                BSIM4ldvtp2.Value = 0.0;
            if (!BSIM4lrgidl.Given)
                /* v4.7 New GIDL / GISL */
                BSIM4lrgidl.Value = 0.0;
            if (!BSIM4lkgidl.Given)
                /* v4.7 New GIDL / GISL */
                BSIM4lkgidl.Value = 0.0;
            if (!BSIM4lfgidl.Given)
                /* v4.7 New GIDL / GISL */
                BSIM4lfgidl.Value = 0.0;
            /* if (!BSIM4lagisl.Given)
			{
				if (BSIM4lagidl.Given)
				BSIM4lagisl.Value = BSIM4lagidl;
				else
				BSIM4lagisl.Value = 0.0;
			}
			if (!BSIM4lbgisl.Given)
			{
				if (BSIM4lbgidl.Given)
				BSIM4lbgisl.Value = BSIM4lbgidl;
				else
				BSIM4lbgisl.Value = 0.0;
			}
			if (!BSIM4lcgisl.Given)
			{
				if (BSIM4lcgidl.Given)
				BSIM4lcgisl.Value = BSIM4lcgidl;
				else
				BSIM4lcgisl.Value = 0.0;
			}
			if (!BSIM4legisl.Given)
			{
				if (BSIM4legidl.Given)
				BSIM4legisl.Value = BSIM4legidl;
				else
				BSIM4legisl.Value = 0.0; 
			} */
            /* if (!BSIM4lrgisl.Given)
			{
				if (BSIM4lrgidl.Given)
				BSIM4lrgisl.Value = BSIM4lrgidl;
			} /* if (!BSIM4lkgisl.Given)
			{
				if (BSIM4lkgidl.Given)
				BSIM4lkgisl.Value = BSIM4lkgidl;
			}
			if (!BSIM4lfgisl.Given)
			{
				if (BSIM4lfgidl.Given)
				BSIM4lfgisl.Value = BSIM4lfgidl;
			} */

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
            }
            else
            {
                BSIM4laigs.Value = BSIM4laigd.Value = BSIM4laigsd;
            }
            if (!BSIM4bigsd.Given && (BSIM4bigs.Given || BSIM4bigd.Given))
            {
            }
            else
            {
                BSIM4lbigs.Value = BSIM4lbigd.Value = BSIM4lbigsd;
            }
            if (!BSIM4cigsd.Given && (BSIM4cigs.Given || BSIM4cigd.Given))
            {
            }
            else
            {
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
            /* if (!BSIM4wagisl.Given)
			{
				if (BSIM4wagidl.Given)
				BSIM4wagisl.Value = BSIM4wagidl;
				else
				BSIM4wagisl.Value = 0.0;
			}
			if (!BSIM4wbgisl.Given)
			{
				if (BSIM4wbgidl.Given)
				BSIM4wbgisl.Value = BSIM4wbgidl;
				else
				BSIM4wbgisl.Value = 0.0;
			}
			if (!BSIM4wcgisl.Given)
			{
				if (BSIM4wcgidl.Given)
				BSIM4wcgisl.Value = BSIM4wcgidl;
				else
				BSIM4wcgisl.Value = 0.0;
			}
			if (!BSIM4wegisl.Given)
			{
				if (BSIM4wegidl.Given)
				BSIM4wegisl.Value = BSIM4wegidl;
				else
				BSIM4wegisl.Value = 0.0; 
			} */
            /* if (!BSIM4wrgisl.Given)
			{
				if (BSIM4wrgidl.Given)
				BSIM4wrgisl.Value = BSIM4wrgidl;
			}
			if (!BSIM4wkgisl.Given)
			{
				if (BSIM4wkgidl.Given)
				BSIM4wkgisl.Value = BSIM4wkgidl;
			}
			if (!BSIM4wfgisl.Given)
			{
				if (BSIM4wfgidl.Given)
				BSIM4wfgisl.Value = BSIM4wfgidl;
			} */

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
            }
            else
            {
                BSIM4waigs.Value = BSIM4waigd.Value = BSIM4waigsd;
            }
            if (!BSIM4bigsd.Given && (BSIM4bigs.Given || BSIM4bigd.Given))
            {
            }
            else
            {
                BSIM4wbigs.Value = BSIM4wbigd.Value = BSIM4wbigsd;
            }
            if (!BSIM4cigsd.Given && (BSIM4cigs.Given || BSIM4cigd.Given))
            {
            }
            else
            {
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

            /* if (!BSIM4pagisl.Given)
			{
				if (BSIM4pagidl.Given)
				BSIM4pagisl.Value = BSIM4pagidl;
				else
				BSIM4pagisl.Value = 0.0;
			}
			if (!BSIM4pbgisl.Given)
			{
				if (BSIM4pbgidl.Given)
				BSIM4pbgisl.Value = BSIM4pbgidl;
				else
				BSIM4pbgisl.Value = 0.0;
			}
			if (!BSIM4pcgisl.Given)
			{
				if (BSIM4pcgidl.Given)
				BSIM4pcgisl.Value = BSIM4pcgidl;
				else
				BSIM4pcgisl.Value = 0.0;
			}
			if (!BSIM4pegisl.Given)
			{
				if (BSIM4pegidl.Given)
				BSIM4pegisl.Value = BSIM4pegidl;
				else
				BSIM4pegisl.Value = 0.0; 
			} */

            /* if (!BSIM4prgisl.Given)
			{
				if (BSIM4prgidl.Given)
				BSIM4prgisl.Value = BSIM4prgidl;
			}
			if (!BSIM4pkgisl.Given)
			{
				if (BSIM4pkgidl.Given)
				BSIM4pkgisl.Value = BSIM4pkgidl;
			}
			if (!BSIM4pfgisl.Given)
			{
				if (BSIM4pfgidl.Given)
				BSIM4pfgisl.Value = BSIM4pfgidl;
			} */

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
            }
            else
            {
                BSIM4paigs.Value = BSIM4paigd.Value = BSIM4paigsd;
            }
            if (!BSIM4bigsd.Given && (BSIM4bigs.Given || BSIM4bigd.Given))
            {
            }
            else
            {
                BSIM4pbigs.Value = BSIM4pbigd.Value = BSIM4pbigsd;
            }
            if (!BSIM4cigsd.Given && (BSIM4cigs.Given || BSIM4cigd.Given))
            {
            }
            else
            {
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
        }
    }
}
