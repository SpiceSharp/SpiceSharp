using System;
using SpiceSharp.Circuits;
using SpiceSharp.Diagnostics;
using SpiceSharp.Parameters;
using SpiceSharp.Components.Transistors;

namespace SpiceSharp.Components
{
    public class BSIM3Model : CircuitModel
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("mobmod"), SpiceInfo("Mobility model selector")]
        public Parameter<int> BSIM3mobMod { get; } = new Parameter<int>(1);
        [SpiceName("binunit"), SpiceInfo("Bin  unit  selector")]
        public Parameter<int> BSIM3binUnit { get; } = new Parameter<int>(1);
        [SpiceName("paramchk"), SpiceInfo("Model parameter checking selector")]
        public Parameter<int> BSIM3paramChk { get; } = new Parameter<int>();
        [SpiceName("capmod"), SpiceInfo("Capacitance model selector")]
        public Parameter<int> BSIM3capMod { get; } = new Parameter<int>(3);
        [SpiceName("noimod"), SpiceInfo("Noise model selector")]
        public Parameter<int> BSIM3noiMod { get; } = new Parameter<int>(1);
        [SpiceName("acnqsmod"), SpiceInfo("AC NQS model selector")]
        public Parameter<int> BSIM3acnqsMod { get; } = new Parameter<int>();
        [SpiceName("version"), SpiceInfo(" parameter for model version")]
        public Parameter<string> BSIM3version { get; } = new Parameter<string>("3.3.0");
        [SpiceName("tox"), SpiceInfo("Gate oxide thickness in meters")]
        public Parameter<double> BSIM3tox { get; } = new Parameter<double>(150.0e-10);
        [SpiceName("toxm"), SpiceInfo("Gate oxide thickness used in extraction")]
        public Parameter<double> BSIM3toxm { get; } = new Parameter<double>();
        [SpiceName("cdsc"), SpiceInfo("Drain/Source and channel coupling capacitance")]
        public Parameter<double> BSIM3cdsc { get; } = new Parameter<double>(2.4e-4);
        [SpiceName("cdscb"), SpiceInfo("Body-bias dependence of cdsc")]
        public Parameter<double> BSIM3cdscb { get; } = new Parameter<double>();
        [SpiceName("cdscd"), SpiceInfo("Drain-bias dependence of cdsc")]
        public Parameter<double> BSIM3cdscd { get; } = new Parameter<double>();
        [SpiceName("cit"), SpiceInfo("Interface state capacitance")]
        public Parameter<double> BSIM3cit { get; } = new Parameter<double>();
        [SpiceName("nfactor"), SpiceInfo("Subthreshold swing Coefficient")]
        public Parameter<double> BSIM3nfactor { get; } = new Parameter<double>(1);
        [SpiceName("xj"), SpiceInfo("Junction depth in meters")]
        public Parameter<double> BSIM3xj { get; } = new Parameter<double>(.15e-6);
        [SpiceName("vsat"), SpiceInfo("Saturation velocity at tnom")]
        public Parameter<double> BSIM3vsat { get; } = new Parameter<double>(8.0e4);
        [SpiceName("a0"), SpiceInfo("Non-uniform depletion width effect coefficient.")]
        public Parameter<double> BSIM3a0 { get; } = new Parameter<double>();
        [SpiceName("ags"), SpiceInfo("Gate bias  coefficient of Abulk.")]
        public Parameter<double> BSIM3ags { get; } = new Parameter<double>();
        [SpiceName("a1"), SpiceInfo("Non-saturation effect coefficient")]
        public Parameter<double> BSIM3a1 { get; } = new Parameter<double>();
        [SpiceName("a2"), SpiceInfo("Non-saturation effect coefficient")]
        public Parameter<double> BSIM3a2 { get; } = new Parameter<double>();
        [SpiceName("at"), SpiceInfo("Temperature coefficient of vsat")]
        public Parameter<double> BSIM3at { get; } = new Parameter<double>(3.3e4);
        [SpiceName("keta"), SpiceInfo("Body-bias coefficient of non-uniform depletion width effect.")]
        public Parameter<double> BSIM3keta { get; } = new Parameter<double>(-0.047);
        [SpiceName("nsub"), SpiceInfo("Substrate doping concentration")]
        public Parameter<double> BSIM3nsub { get; } = new Parameter<double>(6.0e16);
        [SpiceName("nch"), SpiceInfo("Channel doping concentration")]
        public ParameterMethod<double> BSIM3npeak { get; } = new ParameterMethod<double>(1.7e17, (double v) => v > 1.0e20 ? v * 1e-6 : v, null);
        [SpiceName("ngate"), SpiceInfo("Poly-gate doping concentration")]
        public ParameterMethod<double> BSIM3ngate { get; } = new ParameterMethod<double>(0.0, (double v) => v > 1.0e23 ? v * 1e-6 : v, null);
        [SpiceName("gamma1"), SpiceInfo("Vth body coefficient")]
        public Parameter<double> BSIM3gamma1 { get; } = new Parameter<double>();
        [SpiceName("gamma2"), SpiceInfo("Vth body coefficient")]
        public Parameter<double> BSIM3gamma2 { get; } = new Parameter<double>();
        [SpiceName("vbx"), SpiceInfo("Vth transition body Voltage")]
        public Parameter<double> BSIM3vbx { get; } = new Parameter<double>();
        [SpiceName("vbm"), SpiceInfo("Maximum body voltage")]
        public Parameter<double> BSIM3vbm { get; } = new Parameter<double>();
        [SpiceName("xt"), SpiceInfo("Doping depth")]
        public Parameter<double> BSIM3xt { get; } = new Parameter<double>(1.55e-7);
        [SpiceName("k1"), SpiceInfo("Bulk effect coefficient 1")]
        public Parameter<double> BSIM3k1 { get; } = new Parameter<double>();
        [SpiceName("kt1"), SpiceInfo("Temperature coefficient of Vth")]
        public Parameter<double> BSIM3kt1 { get; } = new Parameter<double>(-0.11);
        [SpiceName("kt1l"), SpiceInfo("Temperature coefficient of Vth")]
        public Parameter<double> BSIM3kt1l { get; } = new Parameter<double>();
        [SpiceName("kt2"), SpiceInfo("Body-coefficient of kt1")]
        public Parameter<double> BSIM3kt2 { get; } = new Parameter<double>();
        [SpiceName("k2"), SpiceInfo("Bulk effect coefficient 2")]
        public Parameter<double> BSIM3k2 { get; } = new Parameter<double>();
        [SpiceName("k3"), SpiceInfo("Narrow width effect coefficient")]
        public Parameter<double> BSIM3k3 { get; } = new Parameter<double>();
        [SpiceName("k3b"), SpiceInfo("Body effect coefficient of k3")]
        public Parameter<double> BSIM3k3b { get; } = new Parameter<double>();
        [SpiceName("nlx"), SpiceInfo("Lateral non-uniform doping effect")]
        public Parameter<double> BSIM3nlx { get; } = new Parameter<double>(1.74e-7);
        [SpiceName("w0"), SpiceInfo("Narrow width effect parameter")]
        public Parameter<double> BSIM3w0 { get; } = new Parameter<double>(2.5e-6);
        [SpiceName("dvt0"), SpiceInfo("Short channel effect coeff. 0")]
        public Parameter<double> BSIM3dvt0 { get; } = new Parameter<double>(2.2);
        [SpiceName("dvt1"), SpiceInfo("Short channel effect coeff. 1")]
        public Parameter<double> BSIM3dvt1 { get; } = new Parameter<double>();
        [SpiceName("dvt2"), SpiceInfo("Short channel effect coeff. 2")]
        public Parameter<double> BSIM3dvt2 { get; } = new Parameter<double>(-0.032);
        [SpiceName("dvt0w"), SpiceInfo("Narrow Width coeff. 0")]
        public Parameter<double> BSIM3dvt0w { get; } = new Parameter<double>();
        [SpiceName("dvt1w"), SpiceInfo("Narrow Width effect coeff. 1")]
        public Parameter<double> BSIM3dvt1w { get; } = new Parameter<double>(5.3e6);
        [SpiceName("dvt2w"), SpiceInfo("Narrow Width effect coeff. 2")]
        public Parameter<double> BSIM3dvt2w { get; } = new Parameter<double>(-0.032);
        [SpiceName("drout"), SpiceInfo("DIBL coefficient of output resistance")]
        public Parameter<double> BSIM3drout { get; } = new Parameter<double>();
        [SpiceName("dsub"), SpiceInfo("DIBL coefficient in the subthreshold region")]
        public Parameter<double> BSIM3dsub { get; } = new Parameter<double>();
        [SpiceName("vth0"), SpiceName("vtho"), SpiceInfo("Threshold voltage")]
        public Parameter<double> BSIM3vth0 { get; } = new Parameter<double>();
        [SpiceName("ua"), SpiceInfo("Linear gate dependence of mobility")]
        public Parameter<double> BSIM3ua { get; } = new Parameter<double>(2.25e-9);
        [SpiceName("ua1"), SpiceInfo("Temperature coefficient of ua")]
        public Parameter<double> BSIM3ua1 { get; } = new Parameter<double>(4.31e-9);
        [SpiceName("ub"), SpiceInfo("Quadratic gate dependence of mobility")]
        public Parameter<double> BSIM3ub { get; } = new Parameter<double>(5.87e-19);
        [SpiceName("ub1"), SpiceInfo("Temperature coefficient of ub")]
        public Parameter<double> BSIM3ub1 { get; } = new Parameter<double>(-7.61e-18);
        [SpiceName("uc"), SpiceInfo("Body-bias dependence of mobility")]
        public Parameter<double> BSIM3uc { get; } = new Parameter<double>();
        [SpiceName("uc1"), SpiceInfo("Temperature coefficient of uc")]
        public Parameter<double> BSIM3uc1 { get; } = new Parameter<double>();
        [SpiceName("u0"), SpiceInfo("Low-field mobility at Tnom")]
        public Parameter<double> BSIM3u0 { get; } = new Parameter<double>();
        [SpiceName("ute"), SpiceInfo("Temperature coefficient of mobility")]
        public Parameter<double> BSIM3ute { get; } = new Parameter<double>(-1.5);
        [SpiceName("voff"), SpiceInfo("Threshold voltage offset")]
        public Parameter<double> BSIM3voff { get; } = new Parameter<double>(-0.08);
        [SpiceName("delta"), SpiceInfo("Effective Vds parameter")]
        public Parameter<double> BSIM3delta { get; } = new Parameter<double>();
        [SpiceName("rdsw"), SpiceInfo("Source-drain resistance per width")]
        public Parameter<double> BSIM3rdsw { get; } = new Parameter<double>();
        [SpiceName("prwg"), SpiceInfo("Gate-bias effect on parasitic resistance ")]
        public Parameter<double> BSIM3prwg { get; } = new Parameter<double>();
        [SpiceName("prwb"), SpiceInfo("Body-effect on parasitic resistance ")]
        public Parameter<double> BSIM3prwb { get; } = new Parameter<double>();
        [SpiceName("prt"), SpiceInfo("Temperature coefficient of parasitic resistance ")]
        public Parameter<double> BSIM3prt { get; } = new Parameter<double>();
        [SpiceName("eta0"), SpiceInfo("Subthreshold region DIBL coefficient")]
        public Parameter<double> BSIM3eta0 { get; } = new Parameter<double>();
        [SpiceName("etab"), SpiceInfo("Subthreshold region DIBL coefficient")]
        public Parameter<double> BSIM3etab { get; } = new Parameter<double>(-0.07);
        [SpiceName("pclm"), SpiceInfo("Channel length modulation Coefficient")]
        public Parameter<double> BSIM3pclm { get; } = new Parameter<double>(1.3);
        [SpiceName("pdiblc1"), SpiceInfo("Drain-induced barrier lowering coefficient")]
        public Parameter<double> BSIM3pdibl1 { get; } = new Parameter<double>(.39);
        [SpiceName("pdiblc2"), SpiceInfo("Drain-induced barrier lowering coefficient")]
        public Parameter<double> BSIM3pdibl2 { get; } = new Parameter<double>();
        [SpiceName("pdiblcb"), SpiceInfo("Body-effect on drain-induced barrier lowering")]
        public Parameter<double> BSIM3pdiblb { get; } = new Parameter<double>();
        [SpiceName("pscbe1"), SpiceInfo("Substrate current body-effect coefficient")]
        public Parameter<double> BSIM3pscbe1 { get; } = new Parameter<double>(4.24e8);
        [SpiceName("pscbe2"), SpiceInfo("Substrate current body-effect coefficient")]
        public Parameter<double> BSIM3pscbe2 { get; } = new Parameter<double>(1.0e-5);
        [SpiceName("pvag"), SpiceInfo("Gate dependence of output resistance parameter")]
        public Parameter<double> BSIM3pvag { get; } = new Parameter<double>();
        [SpiceName("wr"), SpiceInfo("Width dependence of rds")]
        public Parameter<double> BSIM3wr { get; } = new Parameter<double>();
        [SpiceName("dwg"), SpiceInfo("Width reduction parameter")]
        public Parameter<double> BSIM3dwg { get; } = new Parameter<double>();
        [SpiceName("dwb"), SpiceInfo("Width reduction parameter")]
        public Parameter<double> BSIM3dwb { get; } = new Parameter<double>();
        [SpiceName("b0"), SpiceInfo("Abulk narrow width parameter")]
        public Parameter<double> BSIM3b0 { get; } = new Parameter<double>();
        [SpiceName("b1"), SpiceInfo("Abulk narrow width parameter")]
        public Parameter<double> BSIM3b1 { get; } = new Parameter<double>();
        [SpiceName("alpha0"), SpiceInfo("substrate current model parameter")]
        public Parameter<double> BSIM3alpha0 { get; } = new Parameter<double>();
        [SpiceName("alpha1"), SpiceInfo("substrate current model parameter")]
        public Parameter<double> BSIM3alpha1 { get; } = new Parameter<double>();
        [SpiceName("beta0"), SpiceInfo("substrate current model parameter")]
        public Parameter<double> BSIM3beta0 { get; } = new Parameter<double>();
        [SpiceName("ijth"), SpiceInfo("Diode limiting current")]
        public Parameter<double> BSIM3ijth { get; } = new Parameter<double>();
        [SpiceName("vfb"), SpiceInfo("Flat Band Voltage")]
        public Parameter<double> BSIM3vfb { get; } = new Parameter<double>();
        [SpiceName("elm"), SpiceInfo("Non-quasi-static Elmore Constant Parameter")]
        public Parameter<double> BSIM3elm { get; } = new Parameter<double>();
        [SpiceName("cgsl"), SpiceInfo("New C-V model parameter")]
        public Parameter<double> BSIM3cgsl { get; } = new Parameter<double>();
        [SpiceName("cgdl"), SpiceInfo("New C-V model parameter")]
        public Parameter<double> BSIM3cgdl { get; } = new Parameter<double>();
        [SpiceName("ckappa"), SpiceInfo("New C-V model parameter")]
        public Parameter<double> BSIM3ckappa { get; } = new Parameter<double>();
        [SpiceName("cf"), SpiceInfo("Fringe capacitance parameter")]
        public Parameter<double> BSIM3cf { get; } = new Parameter<double>();
        [SpiceName("clc"), SpiceInfo("Vdsat parameter for C-V model")]
        public Parameter<double> BSIM3clc { get; } = new Parameter<double>();
        [SpiceName("cle"), SpiceInfo("Vdsat parameter for C-V model")]
        public Parameter<double> BSIM3cle { get; } = new Parameter<double>();
        [SpiceName("dwc"), SpiceInfo("Delta W for C-V model")]
        public Parameter<double> BSIM3dwc { get; } = new Parameter<double>();
        [SpiceName("dlc"), SpiceInfo("Delta L for C-V model")]
        public Parameter<double> BSIM3dlc { get; } = new Parameter<double>();
        [SpiceName("vfbcv"), SpiceInfo("Flat Band Voltage parameter for capmod=0 only")]
        public Parameter<double> BSIM3vfbcv { get; } = new Parameter<double>();
        [SpiceName("acde"), SpiceInfo("Exponential coefficient for finite charge thickness")]
        public Parameter<double> BSIM3acde { get; } = new Parameter<double>();
        [SpiceName("moin"), SpiceInfo("Coefficient for gate-bias dependent surface potential")]
        public Parameter<double> BSIM3moin { get; } = new Parameter<double>();
        [SpiceName("noff"), SpiceInfo("C-V turn-on/off parameter")]
        public Parameter<double> BSIM3noff { get; } = new Parameter<double>();
        [SpiceName("voffcv"), SpiceInfo("C-V lateral-shift parameter")]
        public Parameter<double> BSIM3voffcv { get; } = new Parameter<double>();
        [SpiceName("tcj"), SpiceInfo("Temperature coefficient of cj")]
        public Parameter<double> BSIM3tcj { get; } = new Parameter<double>();
        [SpiceName("tpb"), SpiceInfo("Temperature coefficient of pb")]
        public Parameter<double> BSIM3tpb { get; } = new Parameter<double>();
        [SpiceName("tcjsw"), SpiceInfo("Temperature coefficient of cjsw")]
        public Parameter<double> BSIM3tcjsw { get; } = new Parameter<double>();
        [SpiceName("tpbsw"), SpiceInfo("Temperature coefficient of pbsw")]
        public Parameter<double> BSIM3tpbsw { get; } = new Parameter<double>();
        [SpiceName("tcjswg"), SpiceInfo("Temperature coefficient of cjswg")]
        public Parameter<double> BSIM3tcjswg { get; } = new Parameter<double>();
        [SpiceName("tpbswg"), SpiceInfo("Temperature coefficient of pbswg")]
        public Parameter<double> BSIM3tpbswg { get; } = new Parameter<double>();
        [SpiceName("lcdsc"), SpiceInfo("Length dependence of cdsc")]
        public Parameter<double> BSIM3lcdsc { get; } = new Parameter<double>();
        [SpiceName("lcdscb"), SpiceInfo("Length dependence of cdscb")]
        public Parameter<double> BSIM3lcdscb { get; } = new Parameter<double>();
        [SpiceName("lcdscd"), SpiceInfo("Length dependence of cdscd")]
        public Parameter<double> BSIM3lcdscd { get; } = new Parameter<double>();
        [SpiceName("lcit"), SpiceInfo("Length dependence of cit")]
        public Parameter<double> BSIM3lcit { get; } = new Parameter<double>();
        [SpiceName("lnfactor"), SpiceInfo("Length dependence of nfactor")]
        public Parameter<double> BSIM3lnfactor { get; } = new Parameter<double>();
        [SpiceName("lxj"), SpiceInfo("Length dependence of xj")]
        public Parameter<double> BSIM3lxj { get; } = new Parameter<double>();
        [SpiceName("lvsat"), SpiceInfo("Length dependence of vsat")]
        public Parameter<double> BSIM3lvsat { get; } = new Parameter<double>();
        [SpiceName("la0"), SpiceInfo("Length dependence of a0")]
        public Parameter<double> BSIM3la0 { get; } = new Parameter<double>();
        [SpiceName("lags"), SpiceInfo("Length dependence of ags")]
        public Parameter<double> BSIM3lags { get; } = new Parameter<double>();
        [SpiceName("la1"), SpiceInfo("Length dependence of a1")]
        public Parameter<double> BSIM3la1 { get; } = new Parameter<double>();
        [SpiceName("la2"), SpiceInfo("Length dependence of a2")]
        public Parameter<double> BSIM3la2 { get; } = new Parameter<double>();
        [SpiceName("lat"), SpiceInfo("Length dependence of at")]
        public Parameter<double> BSIM3lat { get; } = new Parameter<double>();
        [SpiceName("lketa"), SpiceInfo("Length dependence of keta")]
        public Parameter<double> BSIM3lketa { get; } = new Parameter<double>();
        [SpiceName("lnsub"), SpiceInfo("Length dependence of nsub")]
        public Parameter<double> BSIM3lnsub { get; } = new Parameter<double>();
        [SpiceName("lnch"), SpiceInfo("Length dependence of nch")]
        public ParameterMethod<double> BSIM3lnpeak { get; } = new ParameterMethod<double>(0.0, (double v) => v > 1.0e20 ? v * 1e-6 : v, null);
        [SpiceName("lngate"), SpiceInfo("Length dependence of ngate")]
        public ParameterMethod<double> BSIM3lngate { get; } = new ParameterMethod<double>(0.0, (double v) => v > 1.0e23 ? v * 1e-6 : v, null);
        [SpiceName("lgamma1"), SpiceInfo("Length dependence of gamma1")]
        public Parameter<double> BSIM3lgamma1 { get; } = new Parameter<double>();
        [SpiceName("lgamma2"), SpiceInfo("Length dependence of gamma2")]
        public Parameter<double> BSIM3lgamma2 { get; } = new Parameter<double>();
        [SpiceName("lvbx"), SpiceInfo("Length dependence of vbx")]
        public Parameter<double> BSIM3lvbx { get; } = new Parameter<double>();
        [SpiceName("lvbm"), SpiceInfo("Length dependence of vbm")]
        public Parameter<double> BSIM3lvbm { get; } = new Parameter<double>();
        [SpiceName("lxt"), SpiceInfo("Length dependence of xt")]
        public Parameter<double> BSIM3lxt { get; } = new Parameter<double>();
        [SpiceName("lk1"), SpiceInfo("Length dependence of k1")]
        public Parameter<double> BSIM3lk1 { get; } = new Parameter<double>();
        [SpiceName("lkt1"), SpiceInfo("Length dependence of kt1")]
        public Parameter<double> BSIM3lkt1 { get; } = new Parameter<double>();
        [SpiceName("lkt1l"), SpiceInfo("Length dependence of kt1l")]
        public Parameter<double> BSIM3lkt1l { get; } = new Parameter<double>();
        [SpiceName("lkt2"), SpiceInfo("Length dependence of kt2")]
        public Parameter<double> BSIM3lkt2 { get; } = new Parameter<double>();
        [SpiceName("lk2"), SpiceInfo("Length dependence of k2")]
        public Parameter<double> BSIM3lk2 { get; } = new Parameter<double>();
        [SpiceName("lk3"), SpiceInfo("Length dependence of k3")]
        public Parameter<double> BSIM3lk3 { get; } = new Parameter<double>();
        [SpiceName("lk3b"), SpiceInfo("Length dependence of k3b")]
        public Parameter<double> BSIM3lk3b { get; } = new Parameter<double>();
        [SpiceName("lnlx"), SpiceInfo("Length dependence of nlx")]
        public Parameter<double> BSIM3lnlx { get; } = new Parameter<double>();
        [SpiceName("lw0"), SpiceInfo("Length dependence of w0")]
        public Parameter<double> BSIM3lw0 { get; } = new Parameter<double>();
        [SpiceName("ldvt0"), SpiceInfo("Length dependence of dvt0")]
        public Parameter<double> BSIM3ldvt0 { get; } = new Parameter<double>();
        [SpiceName("ldvt1"), SpiceInfo("Length dependence of dvt1")]
        public Parameter<double> BSIM3ldvt1 { get; } = new Parameter<double>();
        [SpiceName("ldvt2"), SpiceInfo("Length dependence of dvt2")]
        public Parameter<double> BSIM3ldvt2 { get; } = new Parameter<double>();
        [SpiceName("ldvt0w"), SpiceInfo("Length dependence of dvt0w")]
        public Parameter<double> BSIM3ldvt0w { get; } = new Parameter<double>();
        [SpiceName("ldvt1w"), SpiceInfo("Length dependence of dvt1w")]
        public Parameter<double> BSIM3ldvt1w { get; } = new Parameter<double>();
        [SpiceName("ldvt2w"), SpiceInfo("Length dependence of dvt2w")]
        public Parameter<double> BSIM3ldvt2w { get; } = new Parameter<double>();
        [SpiceName("ldrout"), SpiceInfo("Length dependence of drout")]
        public Parameter<double> BSIM3ldrout { get; } = new Parameter<double>();
        [SpiceName("ldsub"), SpiceInfo("Length dependence of dsub")]
        public Parameter<double> BSIM3ldsub { get; } = new Parameter<double>();
        [SpiceName("lvth0"), SpiceName("lvtho"), SpiceInfo("Length dependence of vto")]
        public Parameter<double> BSIM3lvth0 { get; } = new Parameter<double>();
        [SpiceName("lua"), SpiceInfo("Length dependence of ua")]
        public Parameter<double> BSIM3lua { get; } = new Parameter<double>();
        [SpiceName("lua1"), SpiceInfo("Length dependence of ua1")]
        public Parameter<double> BSIM3lua1 { get; } = new Parameter<double>();
        [SpiceName("lub"), SpiceInfo("Length dependence of ub")]
        public Parameter<double> BSIM3lub { get; } = new Parameter<double>();
        [SpiceName("lub1"), SpiceInfo("Length dependence of ub1")]
        public Parameter<double> BSIM3lub1 { get; } = new Parameter<double>();
        [SpiceName("luc"), SpiceInfo("Length dependence of uc")]
        public Parameter<double> BSIM3luc { get; } = new Parameter<double>();
        [SpiceName("luc1"), SpiceInfo("Length dependence of uc1")]
        public Parameter<double> BSIM3luc1 { get; } = new Parameter<double>();
        [SpiceName("lu0"), SpiceInfo("Length dependence of u0")]
        public Parameter<double> BSIM3lu0 { get; } = new Parameter<double>();
        [SpiceName("lute"), SpiceInfo("Length dependence of ute")]
        public Parameter<double> BSIM3lute { get; } = new Parameter<double>();
        [SpiceName("lvoff"), SpiceInfo("Length dependence of voff")]
        public Parameter<double> BSIM3lvoff { get; } = new Parameter<double>();
        [SpiceName("ldelta"), SpiceInfo("Length dependence of delta")]
        public Parameter<double> BSIM3ldelta { get; } = new Parameter<double>();
        [SpiceName("lrdsw"), SpiceInfo("Length dependence of rdsw ")]
        public Parameter<double> BSIM3lrdsw { get; } = new Parameter<double>();
        [SpiceName("lprwb"), SpiceInfo("Length dependence of prwb ")]
        public Parameter<double> BSIM3lprwb { get; } = new Parameter<double>();
        [SpiceName("lprwg"), SpiceInfo("Length dependence of prwg ")]
        public Parameter<double> BSIM3lprwg { get; } = new Parameter<double>();
        [SpiceName("lprt"), SpiceInfo("Length dependence of prt ")]
        public Parameter<double> BSIM3lprt { get; } = new Parameter<double>();
        [SpiceName("leta0"), SpiceInfo("Length dependence of eta0")]
        public Parameter<double> BSIM3leta0 { get; } = new Parameter<double>();
        [SpiceName("letab"), SpiceInfo("Length dependence of etab")]
        public Parameter<double> BSIM3letab { get; } = new Parameter<double>();
        [SpiceName("lpclm"), SpiceInfo("Length dependence of pclm")]
        public Parameter<double> BSIM3lpclm { get; } = new Parameter<double>();
        [SpiceName("lpdiblc1"), SpiceInfo("Length dependence of pdiblc1")]
        public Parameter<double> BSIM3lpdibl1 { get; } = new Parameter<double>();
        [SpiceName("lpdiblc2"), SpiceInfo("Length dependence of pdiblc2")]
        public Parameter<double> BSIM3lpdibl2 { get; } = new Parameter<double>();
        [SpiceName("lpdiblcb"), SpiceInfo("Length dependence of pdiblcb")]
        public Parameter<double> BSIM3lpdiblb { get; } = new Parameter<double>();
        [SpiceName("lpscbe1"), SpiceInfo("Length dependence of pscbe1")]
        public Parameter<double> BSIM3lpscbe1 { get; } = new Parameter<double>();
        [SpiceName("lpscbe2"), SpiceInfo("Length dependence of pscbe2")]
        public Parameter<double> BSIM3lpscbe2 { get; } = new Parameter<double>();
        [SpiceName("lpvag"), SpiceInfo("Length dependence of pvag")]
        public Parameter<double> BSIM3lpvag { get; } = new Parameter<double>();
        [SpiceName("lwr"), SpiceInfo("Length dependence of wr")]
        public Parameter<double> BSIM3lwr { get; } = new Parameter<double>();
        [SpiceName("ldwg"), SpiceInfo("Length dependence of dwg")]
        public Parameter<double> BSIM3ldwg { get; } = new Parameter<double>();
        [SpiceName("ldwb"), SpiceInfo("Length dependence of dwb")]
        public Parameter<double> BSIM3ldwb { get; } = new Parameter<double>();
        [SpiceName("lb0"), SpiceInfo("Length dependence of b0")]
        public Parameter<double> BSIM3lb0 { get; } = new Parameter<double>();
        [SpiceName("lb1"), SpiceInfo("Length dependence of b1")]
        public Parameter<double> BSIM3lb1 { get; } = new Parameter<double>();
        [SpiceName("lalpha0"), SpiceInfo("Length dependence of alpha0")]
        public Parameter<double> BSIM3lalpha0 { get; } = new Parameter<double>();
        [SpiceName("lalpha1"), SpiceInfo("Length dependence of alpha1")]
        public Parameter<double> BSIM3lalpha1 { get; } = new Parameter<double>();
        [SpiceName("lbeta0"), SpiceInfo("Length dependence of beta0")]
        public Parameter<double> BSIM3lbeta0 { get; } = new Parameter<double>();
        [SpiceName("lvfb"), SpiceInfo("Length dependence of vfb")]
        public Parameter<double> BSIM3lvfb { get; } = new Parameter<double>();
        [SpiceName("lelm"), SpiceInfo("Length dependence of elm")]
        public Parameter<double> BSIM3lelm { get; } = new Parameter<double>();
        [SpiceName("lcgsl"), SpiceInfo("Length dependence of cgsl")]
        public Parameter<double> BSIM3lcgsl { get; } = new Parameter<double>();
        [SpiceName("lcgdl"), SpiceInfo("Length dependence of cgdl")]
        public Parameter<double> BSIM3lcgdl { get; } = new Parameter<double>();
        [SpiceName("lckappa"), SpiceInfo("Length dependence of ckappa")]
        public Parameter<double> BSIM3lckappa { get; } = new Parameter<double>();
        [SpiceName("lcf"), SpiceInfo("Length dependence of cf")]
        public Parameter<double> BSIM3lcf { get; } = new Parameter<double>();
        [SpiceName("lclc"), SpiceInfo("Length dependence of clc")]
        public Parameter<double> BSIM3lclc { get; } = new Parameter<double>();
        [SpiceName("lcle"), SpiceInfo("Length dependence of cle")]
        public Parameter<double> BSIM3lcle { get; } = new Parameter<double>();
        [SpiceName("lvfbcv"), SpiceInfo("Length dependence of vfbcv")]
        public Parameter<double> BSIM3lvfbcv { get; } = new Parameter<double>();
        [SpiceName("lacde"), SpiceInfo("Length dependence of acde")]
        public Parameter<double> BSIM3lacde { get; } = new Parameter<double>();
        [SpiceName("lmoin"), SpiceInfo("Length dependence of moin")]
        public Parameter<double> BSIM3lmoin { get; } = new Parameter<double>();
        [SpiceName("lnoff"), SpiceInfo("Length dependence of noff")]
        public Parameter<double> BSIM3lnoff { get; } = new Parameter<double>();
        [SpiceName("lvoffcv"), SpiceInfo("Length dependence of voffcv")]
        public Parameter<double> BSIM3lvoffcv { get; } = new Parameter<double>();
        [SpiceName("wcdsc"), SpiceInfo("Width dependence of cdsc")]
        public Parameter<double> BSIM3wcdsc { get; } = new Parameter<double>();
        [SpiceName("wcdscb"), SpiceInfo("Width dependence of cdscb")]
        public Parameter<double> BSIM3wcdscb { get; } = new Parameter<double>();
        [SpiceName("wcdscd"), SpiceInfo("Width dependence of cdscd")]
        public Parameter<double> BSIM3wcdscd { get; } = new Parameter<double>();
        [SpiceName("wcit"), SpiceInfo("Width dependence of cit")]
        public Parameter<double> BSIM3wcit { get; } = new Parameter<double>();
        [SpiceName("wnfactor"), SpiceInfo("Width dependence of nfactor")]
        public Parameter<double> BSIM3wnfactor { get; } = new Parameter<double>();
        [SpiceName("wxj"), SpiceInfo("Width dependence of xj")]
        public Parameter<double> BSIM3wxj { get; } = new Parameter<double>();
        [SpiceName("wvsat"), SpiceInfo("Width dependence of vsat")]
        public Parameter<double> BSIM3wvsat { get; } = new Parameter<double>();
        [SpiceName("wa0"), SpiceInfo("Width dependence of a0")]
        public Parameter<double> BSIM3wa0 { get; } = new Parameter<double>();
        [SpiceName("wags"), SpiceInfo("Width dependence of ags")]
        public Parameter<double> BSIM3wags { get; } = new Parameter<double>();
        [SpiceName("wa1"), SpiceInfo("Width dependence of a1")]
        public Parameter<double> BSIM3wa1 { get; } = new Parameter<double>();
        [SpiceName("wa2"), SpiceInfo("Width dependence of a2")]
        public Parameter<double> BSIM3wa2 { get; } = new Parameter<double>();
        [SpiceName("wat"), SpiceInfo("Width dependence of at")]
        public Parameter<double> BSIM3wat { get; } = new Parameter<double>();
        [SpiceName("wketa"), SpiceInfo("Width dependence of keta")]
        public Parameter<double> BSIM3wketa { get; } = new Parameter<double>();
        [SpiceName("wnsub"), SpiceInfo("Width dependence of nsub")]
        public Parameter<double> BSIM3wnsub { get; } = new Parameter<double>();
        [SpiceName("wnch"), SpiceInfo("Width dependence of nch")]
        public ParameterMethod<double> BSIM3wnpeak { get; } = new ParameterMethod<double>(0.0, (double v) => v > 1.0e20 ? v * 1e-6 : v, null);
        [SpiceName("wngate"), SpiceInfo("Width dependence of ngate")]
        public ParameterMethod<double> BSIM3wngate { get; } = new ParameterMethod<double>(0.0, (double v) => v > 1.0e23 ? v * 1e-6 : v, null);
        [SpiceName("wgamma1"), SpiceInfo("Width dependence of gamma1")]
        public Parameter<double> BSIM3wgamma1 { get; } = new Parameter<double>();
        [SpiceName("wgamma2"), SpiceInfo("Width dependence of gamma2")]
        public Parameter<double> BSIM3wgamma2 { get; } = new Parameter<double>();
        [SpiceName("wvbx"), SpiceInfo("Width dependence of vbx")]
        public Parameter<double> BSIM3wvbx { get; } = new Parameter<double>();
        [SpiceName("wvbm"), SpiceInfo("Width dependence of vbm")]
        public Parameter<double> BSIM3wvbm { get; } = new Parameter<double>();
        [SpiceName("wxt"), SpiceInfo("Width dependence of xt")]
        public Parameter<double> BSIM3wxt { get; } = new Parameter<double>();
        [SpiceName("wk1"), SpiceInfo("Width dependence of k1")]
        public Parameter<double> BSIM3wk1 { get; } = new Parameter<double>();
        [SpiceName("wkt1"), SpiceInfo("Width dependence of kt1")]
        public Parameter<double> BSIM3wkt1 { get; } = new Parameter<double>();
        [SpiceName("wkt1l"), SpiceInfo("Width dependence of kt1l")]
        public Parameter<double> BSIM3wkt1l { get; } = new Parameter<double>();
        [SpiceName("wkt2"), SpiceInfo("Width dependence of kt2")]
        public Parameter<double> BSIM3wkt2 { get; } = new Parameter<double>();
        [SpiceName("wk2"), SpiceInfo("Width dependence of k2")]
        public Parameter<double> BSIM3wk2 { get; } = new Parameter<double>();
        [SpiceName("wk3"), SpiceInfo("Width dependence of k3")]
        public Parameter<double> BSIM3wk3 { get; } = new Parameter<double>();
        [SpiceName("wk3b"), SpiceInfo("Width dependence of k3b")]
        public Parameter<double> BSIM3wk3b { get; } = new Parameter<double>();
        [SpiceName("wnlx"), SpiceInfo("Width dependence of nlx")]
        public Parameter<double> BSIM3wnlx { get; } = new Parameter<double>();
        [SpiceName("ww0"), SpiceInfo("Width dependence of w0")]
        public Parameter<double> BSIM3ww0 { get; } = new Parameter<double>();
        [SpiceName("wdvt0"), SpiceInfo("Width dependence of dvt0")]
        public Parameter<double> BSIM3wdvt0 { get; } = new Parameter<double>();
        [SpiceName("wdvt1"), SpiceInfo("Width dependence of dvt1")]
        public Parameter<double> BSIM3wdvt1 { get; } = new Parameter<double>();
        [SpiceName("wdvt2"), SpiceInfo("Width dependence of dvt2")]
        public Parameter<double> BSIM3wdvt2 { get; } = new Parameter<double>();
        [SpiceName("wdvt0w"), SpiceInfo("Width dependence of dvt0w")]
        public Parameter<double> BSIM3wdvt0w { get; } = new Parameter<double>();
        [SpiceName("wdvt1w"), SpiceInfo("Width dependence of dvt1w")]
        public Parameter<double> BSIM3wdvt1w { get; } = new Parameter<double>();
        [SpiceName("wdvt2w"), SpiceInfo("Width dependence of dvt2w")]
        public Parameter<double> BSIM3wdvt2w { get; } = new Parameter<double>();
        [SpiceName("wdrout"), SpiceInfo("Width dependence of drout")]
        public Parameter<double> BSIM3wdrout { get; } = new Parameter<double>();
        [SpiceName("wdsub"), SpiceInfo("Width dependence of dsub")]
        public Parameter<double> BSIM3wdsub { get; } = new Parameter<double>();
        [SpiceName("wvth0"), SpiceName("wvtho"), SpiceInfo("Width dependence of vto")]
        public Parameter<double> BSIM3wvth0 { get; } = new Parameter<double>();
        [SpiceName("wua"), SpiceInfo("Width dependence of ua")]
        public Parameter<double> BSIM3wua { get; } = new Parameter<double>();
        [SpiceName("wua1"), SpiceInfo("Width dependence of ua1")]
        public Parameter<double> BSIM3wua1 { get; } = new Parameter<double>();
        [SpiceName("wub"), SpiceInfo("Width dependence of ub")]
        public Parameter<double> BSIM3wub { get; } = new Parameter<double>();
        [SpiceName("wub1"), SpiceInfo("Width dependence of ub1")]
        public Parameter<double> BSIM3wub1 { get; } = new Parameter<double>();
        [SpiceName("wuc"), SpiceInfo("Width dependence of uc")]
        public Parameter<double> BSIM3wuc { get; } = new Parameter<double>();
        [SpiceName("wuc1"), SpiceInfo("Width dependence of uc1")]
        public Parameter<double> BSIM3wuc1 { get; } = new Parameter<double>();
        [SpiceName("wu0"), SpiceInfo("Width dependence of u0")]
        public Parameter<double> BSIM3wu0 { get; } = new Parameter<double>();
        [SpiceName("wute"), SpiceInfo("Width dependence of ute")]
        public Parameter<double> BSIM3wute { get; } = new Parameter<double>();
        [SpiceName("wvoff"), SpiceInfo("Width dependence of voff")]
        public Parameter<double> BSIM3wvoff { get; } = new Parameter<double>();
        [SpiceName("wdelta"), SpiceInfo("Width dependence of delta")]
        public Parameter<double> BSIM3wdelta { get; } = new Parameter<double>();
        [SpiceName("wrdsw"), SpiceInfo("Width dependence of rdsw ")]
        public Parameter<double> BSIM3wrdsw { get; } = new Parameter<double>();
        [SpiceName("wprwb"), SpiceInfo("Width dependence of prwb ")]
        public Parameter<double> BSIM3wprwb { get; } = new Parameter<double>();
        [SpiceName("wprwg"), SpiceInfo("Width dependence of prwg ")]
        public Parameter<double> BSIM3wprwg { get; } = new Parameter<double>();
        [SpiceName("wprt"), SpiceInfo("Width dependence of prt")]
        public Parameter<double> BSIM3wprt { get; } = new Parameter<double>();
        [SpiceName("weta0"), SpiceInfo("Width dependence of eta0")]
        public Parameter<double> BSIM3weta0 { get; } = new Parameter<double>();
        [SpiceName("wetab"), SpiceInfo("Width dependence of etab")]
        public Parameter<double> BSIM3wetab { get; } = new Parameter<double>();
        [SpiceName("wpclm"), SpiceInfo("Width dependence of pclm")]
        public Parameter<double> BSIM3wpclm { get; } = new Parameter<double>();
        [SpiceName("wpdiblc1"), SpiceInfo("Width dependence of pdiblc1")]
        public Parameter<double> BSIM3wpdibl1 { get; } = new Parameter<double>();
        [SpiceName("wpdiblc2"), SpiceInfo("Width dependence of pdiblc2")]
        public Parameter<double> BSIM3wpdibl2 { get; } = new Parameter<double>();
        [SpiceName("wpdiblcb"), SpiceInfo("Width dependence of pdiblcb")]
        public Parameter<double> BSIM3wpdiblb { get; } = new Parameter<double>();
        [SpiceName("wpscbe1"), SpiceInfo("Width dependence of pscbe1")]
        public Parameter<double> BSIM3wpscbe1 { get; } = new Parameter<double>();
        [SpiceName("wpscbe2"), SpiceInfo("Width dependence of pscbe2")]
        public Parameter<double> BSIM3wpscbe2 { get; } = new Parameter<double>();
        [SpiceName("wpvag"), SpiceInfo("Width dependence of pvag")]
        public Parameter<double> BSIM3wpvag { get; } = new Parameter<double>();
        [SpiceName("wwr"), SpiceInfo("Width dependence of wr")]
        public Parameter<double> BSIM3wwr { get; } = new Parameter<double>();
        [SpiceName("wdwg"), SpiceInfo("Width dependence of dwg")]
        public Parameter<double> BSIM3wdwg { get; } = new Parameter<double>();
        [SpiceName("wdwb"), SpiceInfo("Width dependence of dwb")]
        public Parameter<double> BSIM3wdwb { get; } = new Parameter<double>();
        [SpiceName("wb0"), SpiceInfo("Width dependence of b0")]
        public Parameter<double> BSIM3wb0 { get; } = new Parameter<double>();
        [SpiceName("wb1"), SpiceInfo("Width dependence of b1")]
        public Parameter<double> BSIM3wb1 { get; } = new Parameter<double>();
        [SpiceName("walpha0"), SpiceInfo("Width dependence of alpha0")]
        public Parameter<double> BSIM3walpha0 { get; } = new Parameter<double>();
        [SpiceName("walpha1"), SpiceInfo("Width dependence of alpha1")]
        public Parameter<double> BSIM3walpha1 { get; } = new Parameter<double>();
        [SpiceName("wbeta0"), SpiceInfo("Width dependence of beta0")]
        public Parameter<double> BSIM3wbeta0 { get; } = new Parameter<double>();
        [SpiceName("wvfb"), SpiceInfo("Width dependence of vfb")]
        public Parameter<double> BSIM3wvfb { get; } = new Parameter<double>();
        [SpiceName("welm"), SpiceInfo("Width dependence of elm")]
        public Parameter<double> BSIM3welm { get; } = new Parameter<double>();
        [SpiceName("wcgsl"), SpiceInfo("Width dependence of cgsl")]
        public Parameter<double> BSIM3wcgsl { get; } = new Parameter<double>();
        [SpiceName("wcgdl"), SpiceInfo("Width dependence of cgdl")]
        public Parameter<double> BSIM3wcgdl { get; } = new Parameter<double>();
        [SpiceName("wckappa"), SpiceInfo("Width dependence of ckappa")]
        public Parameter<double> BSIM3wckappa { get; } = new Parameter<double>();
        [SpiceName("wcf"), SpiceInfo("Width dependence of cf")]
        public Parameter<double> BSIM3wcf { get; } = new Parameter<double>();
        [SpiceName("wclc"), SpiceInfo("Width dependence of clc")]
        public Parameter<double> BSIM3wclc { get; } = new Parameter<double>();
        [SpiceName("wcle"), SpiceInfo("Width dependence of cle")]
        public Parameter<double> BSIM3wcle { get; } = new Parameter<double>();
        [SpiceName("wvfbcv"), SpiceInfo("Width dependence of vfbcv")]
        public Parameter<double> BSIM3wvfbcv { get; } = new Parameter<double>();
        [SpiceName("wacde"), SpiceInfo("Width dependence of acde")]
        public Parameter<double> BSIM3wacde { get; } = new Parameter<double>();
        [SpiceName("wmoin"), SpiceInfo("Width dependence of moin")]
        public Parameter<double> BSIM3wmoin { get; } = new Parameter<double>();
        [SpiceName("wnoff"), SpiceInfo("Width dependence of noff")]
        public Parameter<double> BSIM3wnoff { get; } = new Parameter<double>();
        [SpiceName("wvoffcv"), SpiceInfo("Width dependence of voffcv")]
        public Parameter<double> BSIM3wvoffcv { get; } = new Parameter<double>();
        [SpiceName("pcdsc"), SpiceInfo("Cross-term dependence of cdsc")]
        public Parameter<double> BSIM3pcdsc { get; } = new Parameter<double>();
        [SpiceName("pcdscb"), SpiceInfo("Cross-term dependence of cdscb")]
        public Parameter<double> BSIM3pcdscb { get; } = new Parameter<double>();
        [SpiceName("pcdscd"), SpiceInfo("Cross-term dependence of cdscd")]
        public Parameter<double> BSIM3pcdscd { get; } = new Parameter<double>();
        [SpiceName("pcit"), SpiceInfo("Cross-term dependence of cit")]
        public Parameter<double> BSIM3pcit { get; } = new Parameter<double>();
        [SpiceName("pnfactor"), SpiceInfo("Cross-term dependence of nfactor")]
        public Parameter<double> BSIM3pnfactor { get; } = new Parameter<double>();
        [SpiceName("pxj"), SpiceInfo("Cross-term dependence of xj")]
        public Parameter<double> BSIM3pxj { get; } = new Parameter<double>();
        [SpiceName("pvsat"), SpiceInfo("Cross-term dependence of vsat")]
        public Parameter<double> BSIM3pvsat { get; } = new Parameter<double>();
        [SpiceName("pa0"), SpiceInfo("Cross-term dependence of a0")]
        public Parameter<double> BSIM3pa0 { get; } = new Parameter<double>();
        [SpiceName("pags"), SpiceInfo("Cross-term dependence of ags")]
        public Parameter<double> BSIM3pags { get; } = new Parameter<double>();
        [SpiceName("pa1"), SpiceInfo("Cross-term dependence of a1")]
        public Parameter<double> BSIM3pa1 { get; } = new Parameter<double>();
        [SpiceName("pa2"), SpiceInfo("Cross-term dependence of a2")]
        public Parameter<double> BSIM3pa2 { get; } = new Parameter<double>();
        [SpiceName("pat"), SpiceInfo("Cross-term dependence of at")]
        public Parameter<double> BSIM3pat { get; } = new Parameter<double>();
        [SpiceName("pketa"), SpiceInfo("Cross-term dependence of keta")]
        public Parameter<double> BSIM3pketa { get; } = new Parameter<double>();
        [SpiceName("pnsub"), SpiceInfo("Cross-term dependence of nsub")]
        public Parameter<double> BSIM3pnsub { get; } = new Parameter<double>();
        [SpiceName("pnch"), SpiceInfo("Cross-term dependence of nch")]
        public ParameterMethod<double> BSIM3pnpeak { get; } = new ParameterMethod<double>(0.0, (double v) => v > 1.0e20 ? v * 1e-6 : v, null);
        [SpiceName("pngate"), SpiceInfo("Cross-term dependence of ngate")]
        public ParameterMethod<double> BSIM3pngate { get; } = new ParameterMethod<double>(0.0, (double v) => v > 1.0e23 ? v * 1e-6 : v, null);
        [SpiceName("pgamma1"), SpiceInfo("Cross-term dependence of gamma1")]
        public Parameter<double> BSIM3pgamma1 { get; } = new Parameter<double>();
        [SpiceName("pgamma2"), SpiceInfo("Cross-term dependence of gamma2")]
        public Parameter<double> BSIM3pgamma2 { get; } = new Parameter<double>();
        [SpiceName("pvbx"), SpiceInfo("Cross-term dependence of vbx")]
        public Parameter<double> BSIM3pvbx { get; } = new Parameter<double>();
        [SpiceName("pvbm"), SpiceInfo("Cross-term dependence of vbm")]
        public Parameter<double> BSIM3pvbm { get; } = new Parameter<double>();
        [SpiceName("pxt"), SpiceInfo("Cross-term dependence of xt")]
        public Parameter<double> BSIM3pxt { get; } = new Parameter<double>();
        [SpiceName("pk1"), SpiceInfo("Cross-term dependence of k1")]
        public Parameter<double> BSIM3pk1 { get; } = new Parameter<double>();
        [SpiceName("pkt1"), SpiceInfo("Cross-term dependence of kt1")]
        public Parameter<double> BSIM3pkt1 { get; } = new Parameter<double>();
        [SpiceName("pkt1l"), SpiceInfo("Cross-term dependence of kt1l")]
        public Parameter<double> BSIM3pkt1l { get; } = new Parameter<double>();
        [SpiceName("pkt2"), SpiceInfo("Cross-term dependence of kt2")]
        public Parameter<double> BSIM3pkt2 { get; } = new Parameter<double>();
        [SpiceName("pk2"), SpiceInfo("Cross-term dependence of k2")]
        public Parameter<double> BSIM3pk2 { get; } = new Parameter<double>();
        [SpiceName("pk3"), SpiceInfo("Cross-term dependence of k3")]
        public Parameter<double> BSIM3pk3 { get; } = new Parameter<double>();
        [SpiceName("pk3b"), SpiceInfo("Cross-term dependence of k3b")]
        public Parameter<double> BSIM3pk3b { get; } = new Parameter<double>();
        [SpiceName("pnlx"), SpiceInfo("Cross-term dependence of nlx")]
        public Parameter<double> BSIM3pnlx { get; } = new Parameter<double>();
        [SpiceName("pw0"), SpiceInfo("Cross-term dependence of w0")]
        public Parameter<double> BSIM3pw0 { get; } = new Parameter<double>();
        [SpiceName("pdvt0"), SpiceInfo("Cross-term dependence of dvt0")]
        public Parameter<double> BSIM3pdvt0 { get; } = new Parameter<double>();
        [SpiceName("pdvt1"), SpiceInfo("Cross-term dependence of dvt1")]
        public Parameter<double> BSIM3pdvt1 { get; } = new Parameter<double>();
        [SpiceName("pdvt2"), SpiceInfo("Cross-term dependence of dvt2")]
        public Parameter<double> BSIM3pdvt2 { get; } = new Parameter<double>();
        [SpiceName("pdvt0w"), SpiceInfo("Cross-term dependence of dvt0w")]
        public Parameter<double> BSIM3pdvt0w { get; } = new Parameter<double>();
        [SpiceName("pdvt1w"), SpiceInfo("Cross-term dependence of dvt1w")]
        public Parameter<double> BSIM3pdvt1w { get; } = new Parameter<double>();
        [SpiceName("pdvt2w"), SpiceInfo("Cross-term dependence of dvt2w")]
        public Parameter<double> BSIM3pdvt2w { get; } = new Parameter<double>();
        [SpiceName("pdrout"), SpiceInfo("Cross-term dependence of drout")]
        public Parameter<double> BSIM3pdrout { get; } = new Parameter<double>();
        [SpiceName("pdsub"), SpiceInfo("Cross-term dependence of dsub")]
        public Parameter<double> BSIM3pdsub { get; } = new Parameter<double>();
        [SpiceName("pvth0"), SpiceName("pvtho"), SpiceInfo("Cross-term dependence of vto")]
        public Parameter<double> BSIM3pvth0 { get; } = new Parameter<double>();
        [SpiceName("pua"), SpiceInfo("Cross-term dependence of ua")]
        public Parameter<double> BSIM3pua { get; } = new Parameter<double>();
        [SpiceName("pua1"), SpiceInfo("Cross-term dependence of ua1")]
        public Parameter<double> BSIM3pua1 { get; } = new Parameter<double>();
        [SpiceName("pub"), SpiceInfo("Cross-term dependence of ub")]
        public Parameter<double> BSIM3pub { get; } = new Parameter<double>();
        [SpiceName("pub1"), SpiceInfo("Cross-term dependence of ub1")]
        public Parameter<double> BSIM3pub1 { get; } = new Parameter<double>();
        [SpiceName("puc"), SpiceInfo("Cross-term dependence of uc")]
        public Parameter<double> BSIM3puc { get; } = new Parameter<double>();
        [SpiceName("puc1"), SpiceInfo("Cross-term dependence of uc1")]
        public Parameter<double> BSIM3puc1 { get; } = new Parameter<double>();
        [SpiceName("pu0"), SpiceInfo("Cross-term dependence of u0")]
        public Parameter<double> BSIM3pu0 { get; } = new Parameter<double>();
        [SpiceName("pute"), SpiceInfo("Cross-term dependence of ute")]
        public Parameter<double> BSIM3pute { get; } = new Parameter<double>();
        [SpiceName("pvoff"), SpiceInfo("Cross-term dependence of voff")]
        public Parameter<double> BSIM3pvoff { get; } = new Parameter<double>();
        [SpiceName("pdelta"), SpiceInfo("Cross-term dependence of delta")]
        public Parameter<double> BSIM3pdelta { get; } = new Parameter<double>();
        [SpiceName("prdsw"), SpiceInfo("Cross-term dependence of rdsw ")]
        public Parameter<double> BSIM3prdsw { get; } = new Parameter<double>();
        [SpiceName("pprwb"), SpiceInfo("Cross-term dependence of prwb ")]
        public Parameter<double> BSIM3pprwb { get; } = new Parameter<double>();
        [SpiceName("pprwg"), SpiceInfo("Cross-term dependence of prwg ")]
        public Parameter<double> BSIM3pprwg { get; } = new Parameter<double>();
        [SpiceName("pprt"), SpiceInfo("Cross-term dependence of prt ")]
        public Parameter<double> BSIM3pprt { get; } = new Parameter<double>();
        [SpiceName("peta0"), SpiceInfo("Cross-term dependence of eta0")]
        public Parameter<double> BSIM3peta0 { get; } = new Parameter<double>();
        [SpiceName("petab"), SpiceInfo("Cross-term dependence of etab")]
        public Parameter<double> BSIM3petab { get; } = new Parameter<double>();
        [SpiceName("ppclm"), SpiceInfo("Cross-term dependence of pclm")]
        public Parameter<double> BSIM3ppclm { get; } = new Parameter<double>();
        [SpiceName("ppdiblc1"), SpiceInfo("Cross-term dependence of pdiblc1")]
        public Parameter<double> BSIM3ppdibl1 { get; } = new Parameter<double>();
        [SpiceName("ppdiblc2"), SpiceInfo("Cross-term dependence of pdiblc2")]
        public Parameter<double> BSIM3ppdibl2 { get; } = new Parameter<double>();
        [SpiceName("ppdiblcb"), SpiceInfo("Cross-term dependence of pdiblcb")]
        public Parameter<double> BSIM3ppdiblb { get; } = new Parameter<double>();
        [SpiceName("ppscbe1"), SpiceInfo("Cross-term dependence of pscbe1")]
        public Parameter<double> BSIM3ppscbe1 { get; } = new Parameter<double>();
        [SpiceName("ppscbe2"), SpiceInfo("Cross-term dependence of pscbe2")]
        public Parameter<double> BSIM3ppscbe2 { get; } = new Parameter<double>();
        [SpiceName("ppvag"), SpiceInfo("Cross-term dependence of pvag")]
        public Parameter<double> BSIM3ppvag { get; } = new Parameter<double>();
        [SpiceName("pwr"), SpiceInfo("Cross-term dependence of wr")]
        public Parameter<double> BSIM3pwr { get; } = new Parameter<double>();
        [SpiceName("pdwg"), SpiceInfo("Cross-term dependence of dwg")]
        public Parameter<double> BSIM3pdwg { get; } = new Parameter<double>();
        [SpiceName("pdwb"), SpiceInfo("Cross-term dependence of dwb")]
        public Parameter<double> BSIM3pdwb { get; } = new Parameter<double>();
        [SpiceName("pb0"), SpiceInfo("Cross-term dependence of b0")]
        public Parameter<double> BSIM3pb0 { get; } = new Parameter<double>();
        [SpiceName("pb1"), SpiceInfo("Cross-term dependence of b1")]
        public Parameter<double> BSIM3pb1 { get; } = new Parameter<double>();
        [SpiceName("palpha0"), SpiceInfo("Cross-term dependence of alpha0")]
        public Parameter<double> BSIM3palpha0 { get; } = new Parameter<double>();
        [SpiceName("palpha1"), SpiceInfo("Cross-term dependence of alpha1")]
        public Parameter<double> BSIM3palpha1 { get; } = new Parameter<double>();
        [SpiceName("pbeta0"), SpiceInfo("Cross-term dependence of beta0")]
        public Parameter<double> BSIM3pbeta0 { get; } = new Parameter<double>();
        [SpiceName("pvfb"), SpiceInfo("Cross-term dependence of vfb")]
        public Parameter<double> BSIM3pvfb { get; } = new Parameter<double>();
        [SpiceName("pelm"), SpiceInfo("Cross-term dependence of elm")]
        public Parameter<double> BSIM3pelm { get; } = new Parameter<double>();
        [SpiceName("pcgsl"), SpiceInfo("Cross-term dependence of cgsl")]
        public Parameter<double> BSIM3pcgsl { get; } = new Parameter<double>();
        [SpiceName("pcgdl"), SpiceInfo("Cross-term dependence of cgdl")]
        public Parameter<double> BSIM3pcgdl { get; } = new Parameter<double>();
        [SpiceName("pckappa"), SpiceInfo("Cross-term dependence of ckappa")]
        public Parameter<double> BSIM3pckappa { get; } = new Parameter<double>();
        [SpiceName("pcf"), SpiceInfo("Cross-term dependence of cf")]
        public Parameter<double> BSIM3pcf { get; } = new Parameter<double>();
        [SpiceName("pclc"), SpiceInfo("Cross-term dependence of clc")]
        public Parameter<double> BSIM3pclc { get; } = new Parameter<double>();
        [SpiceName("pcle"), SpiceInfo("Cross-term dependence of cle")]
        public Parameter<double> BSIM3pcle { get; } = new Parameter<double>();
        [SpiceName("pvfbcv"), SpiceInfo("Cross-term dependence of vfbcv")]
        public Parameter<double> BSIM3pvfbcv { get; } = new Parameter<double>();
        [SpiceName("pacde"), SpiceInfo("Cross-term dependence of acde")]
        public Parameter<double> BSIM3pacde { get; } = new Parameter<double>();
        [SpiceName("pmoin"), SpiceInfo("Cross-term dependence of moin")]
        public Parameter<double> BSIM3pmoin { get; } = new Parameter<double>();
        [SpiceName("pnoff"), SpiceInfo("Cross-term dependence of noff")]
        public Parameter<double> BSIM3pnoff { get; } = new Parameter<double>();
        [SpiceName("pvoffcv"), SpiceInfo("Cross-term dependence of voffcv")]
        public Parameter<double> BSIM3pvoffcv { get; } = new Parameter<double>();
        [SpiceName("tnom"), SpiceInfo("Parameter measurement temperature")]
        public ParameterMethod<double> BSIM3tnom { get; } = new ParameterMethod<double>(300.15, (double celsius) => celsius + Circuit.CONSTCtoK, (double kelvin) => kelvin - Circuit.CONSTCtoK);
        [SpiceName("cgso"), SpiceInfo("Gate-source overlap capacitance per width")]
        public Parameter<double> BSIM3cgso { get; } = new Parameter<double>();
        [SpiceName("cgdo"), SpiceInfo("Gate-drain overlap capacitance per width")]
        public Parameter<double> BSIM3cgdo { get; } = new Parameter<double>();
        [SpiceName("cgbo"), SpiceInfo("Gate-bulk overlap capacitance per length")]
        public Parameter<double> BSIM3cgbo { get; } = new Parameter<double>();
        [SpiceName("xpart"), SpiceInfo("Channel charge partitioning")]
        public Parameter<double> BSIM3xpart { get; } = new Parameter<double>();
        [SpiceName("rsh"), SpiceInfo("Source-drain sheet resistance")]
        public Parameter<double> BSIM3sheetResistance { get; } = new Parameter<double>();
        [SpiceName("js"), SpiceInfo("Source/drain junction reverse saturation current density")]
        public Parameter<double> BSIM3jctSatCurDensity { get; } = new Parameter<double>(1.0E-4);
        [SpiceName("jsw"), SpiceInfo("Sidewall junction reverse saturation current density")]
        public Parameter<double> BSIM3jctSidewallSatCurDensity { get; } = new Parameter<double>();
        [SpiceName("pb"), SpiceInfo("Source/drain junction built-in potential")]
        public Parameter<double> BSIM3bulkJctPotential { get; } = new Parameter<double>();
        [SpiceName("mj"), SpiceInfo("Source/drain bottom junction capacitance grading coefficient")]
        public Parameter<double> BSIM3bulkJctBotGradingCoeff { get; } = new Parameter<double>();
        [SpiceName("pbsw"), SpiceInfo("Source/drain sidewall junction capacitance built in potential")]
        public Parameter<double> BSIM3sidewallJctPotential { get; } = new Parameter<double>();
        [SpiceName("mjsw"), SpiceInfo("Source/drain sidewall junction capacitance grading coefficient")]
        public Parameter<double> BSIM3bulkJctSideGradingCoeff { get; } = new Parameter<double>();
        [SpiceName("cj"), SpiceInfo("Source/drain bottom junction capacitance per unit area")]
        public Parameter<double> BSIM3unitAreaJctCap { get; } = new Parameter<double>(5.0E-4);
        [SpiceName("cjsw"), SpiceInfo("Source/drain sidewall junction capacitance per unit periphery")]
        public Parameter<double> BSIM3unitLengthSidewallJctCap { get; } = new Parameter<double>(5.0E-10);
        [SpiceName("nj"), SpiceInfo("Source/drain junction emission coefficient")]
        public Parameter<double> BSIM3jctEmissionCoeff { get; } = new Parameter<double>();
        [SpiceName("pbswg"), SpiceInfo("Source/drain (gate side) sidewall junction capacitance built in potential")]
        public Parameter<double> BSIM3GatesidewallJctPotential { get; } = new Parameter<double>();
        [SpiceName("mjswg"), SpiceInfo("Source/drain (gate side) sidewall junction capacitance grading coefficient")]
        public Parameter<double> BSIM3bulkJctGateSideGradingCoeff { get; } = new Parameter<double>();
        [SpiceName("cjswg"), SpiceInfo("Source/drain (gate side) sidewall junction capacitance per unit width")]
        public Parameter<double> BSIM3unitLengthGateSidewallJctCap { get; } = new Parameter<double>();
        [SpiceName("xti"), SpiceInfo("Junction current temperature exponent")]
        public Parameter<double> BSIM3jctTempExponent { get; } = new Parameter<double>();
        [SpiceName("lintnoi"), SpiceInfo("lint offset for noise calculation")]
        public Parameter<double> BSIM3lintnoi { get; } = new Parameter<double>();
        [SpiceName("lint"), SpiceInfo("Length reduction parameter")]
        public Parameter<double> BSIM3Lint { get; } = new Parameter<double>();
        [SpiceName("ll"), SpiceInfo("Length reduction parameter")]
        public Parameter<double> BSIM3Ll { get; } = new Parameter<double>();
        [SpiceName("llc"), SpiceInfo("Length reduction parameter for CV")]
        public Parameter<double> BSIM3Llc { get; } = new Parameter<double>();
        [SpiceName("lln"), SpiceInfo("Length reduction parameter")]
        public Parameter<double> BSIM3Lln { get; } = new Parameter<double>();
        [SpiceName("lw"), SpiceInfo("Length reduction parameter")]
        public Parameter<double> BSIM3Lw { get; } = new Parameter<double>();
        [SpiceName("lwc"), SpiceInfo("Length reduction parameter for CV")]
        public Parameter<double> BSIM3Lwc { get; } = new Parameter<double>();
        [SpiceName("lwn"), SpiceInfo("Length reduction parameter")]
        public Parameter<double> BSIM3Lwn { get; } = new Parameter<double>();
        [SpiceName("lwl"), SpiceInfo("Length reduction parameter")]
        public Parameter<double> BSIM3Lwl { get; } = new Parameter<double>();
        [SpiceName("lwlc"), SpiceInfo("Length reduction parameter for CV")]
        public Parameter<double> BSIM3Lwlc { get; } = new Parameter<double>();
        [SpiceName("lmin"), SpiceInfo("Minimum length for the model")]
        public Parameter<double> BSIM3Lmin { get; } = new Parameter<double>();
        [SpiceName("lmax"), SpiceInfo("Maximum length for the model")]
        public Parameter<double> BSIM3Lmax { get; } = new Parameter<double>();
        [SpiceName("wint"), SpiceInfo("Width reduction parameter")]
        public Parameter<double> BSIM3Wint { get; } = new Parameter<double>();
        [SpiceName("wl"), SpiceInfo("Width reduction parameter")]
        public Parameter<double> BSIM3Wl { get; } = new Parameter<double>();
        [SpiceName("wlc"), SpiceInfo("Width reduction parameter for CV")]
        public Parameter<double> BSIM3Wlc { get; } = new Parameter<double>();
        [SpiceName("wln"), SpiceInfo("Width reduction parameter")]
        public Parameter<double> BSIM3Wln { get; } = new Parameter<double>();
        [SpiceName("ww"), SpiceInfo("Width reduction parameter")]
        public Parameter<double> BSIM3Ww { get; } = new Parameter<double>();
        [SpiceName("wwc"), SpiceInfo("Width reduction parameter for CV")]
        public Parameter<double> BSIM3Wwc { get; } = new Parameter<double>();
        [SpiceName("wwn"), SpiceInfo("Width reduction parameter")]
        public Parameter<double> BSIM3Wwn { get; } = new Parameter<double>();
        [SpiceName("wwl"), SpiceInfo("Width reduction parameter")]
        public Parameter<double> BSIM3Wwl { get; } = new Parameter<double>();
        [SpiceName("wwlc"), SpiceInfo("Width reduction parameter for CV")]
        public Parameter<double> BSIM3Wwlc { get; } = new Parameter<double>();
        [SpiceName("wmin"), SpiceInfo("Minimum width for the model")]
        public Parameter<double> BSIM3Wmin { get; } = new Parameter<double>();
        [SpiceName("wmax"), SpiceInfo("Maximum width for the model")]
        public Parameter<double> BSIM3Wmax { get; } = new Parameter<double>();
        [SpiceName("noia"), SpiceInfo("Flicker noise parameter")]
        public Parameter<double> BSIM3oxideTrapDensityA { get; } = new Parameter<double>();
        [SpiceName("noib"), SpiceInfo("Flicker noise parameter")]
        public Parameter<double> BSIM3oxideTrapDensityB { get; } = new Parameter<double>();
        [SpiceName("noic"), SpiceInfo("Flicker noise parameter")]
        public Parameter<double> BSIM3oxideTrapDensityC { get; } = new Parameter<double>();
        [SpiceName("em"), SpiceInfo("Flicker noise parameter")]
        public Parameter<double> BSIM3em { get; } = new Parameter<double>(4.1e7);
        [SpiceName("ef"), SpiceInfo("Flicker noise frequency exponent")]
        public Parameter<double> BSIM3ef { get; } = new Parameter<double>();
        [SpiceName("af"), SpiceInfo("Flicker noise exponent")]
        public Parameter<double> BSIM3af { get; } = new Parameter<double>();
        [SpiceName("kf"), SpiceInfo("Flicker noise coefficient")]
        public Parameter<double> BSIM3kf { get; } = new Parameter<double>();

        /// <summary>
        /// Methods
        /// </summary>
        [SpiceName("nmos"), SpiceInfo("Flag to indicate NMOS")]
        public void SetNMOS(bool value)
        {
            if (value)
                BSIM3type = 1;
        }
        [SpiceName("pmos"), SpiceInfo("Flag to indicate PMOS")]
        public void SetPMOS(bool value)
        {
            if (value)
                BSIM3type = -1;
        }

        /// <summary>
        /// Shared parameters
        /// </summary>
        public double TRatio { get; private set; }
        public double Vtm0 { get; private set; }
        public double ni { get; private set; }

        /// <summary>
        /// Extra variables
        /// </summary>
        public double BSIM3type { get; private set; } = 1;
        public double BSIM3cox { get; private set; }
        public double BSIM3factor1 { get; private set; }
        public double BSIM3vtm { get; private set; }
        public double BSIM3jctTempSatCurDensity { get; private set; }
        public double BSIM3jctSidewallTempSatCurDensity { get; private set; }
        public double BSIM3vcrit { get; private set; }
        public double BSIM3unitAreaTempJctCap { get; private set; }
        public double BSIM3unitLengthSidewallTempJctCap { get; private set; }
        public double BSIM3unitLengthGateSidewallTempJctCap { get; private set; }
        public double BSIM3PhiB { get; private set; }
        public double BSIM3PhiBSW { get; private set; }
        public double BSIM3PhiBSWG { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the device</param>
        public BSIM3Model(string name) : base(name)
        {
        }

        /// <summary>
        /// Setup the device
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Setup(Circuit ckt)
        {
            /* Default value Processing for BSIM3 MOSFET Models */
            if (!BSIM3acnqsMod.Given)
                BSIM3acnqsMod.Value = 0;
            else if ((BSIM3acnqsMod != 0) && (BSIM3acnqsMod != 1))
            {
                BSIM3acnqsMod.Value = 0;
                CircuitWarning.Warning(this, "Warning: acnqsMod has been set to its default value: 0.\n");
            }
            BSIM3cox = 3.453133e-11 / BSIM3tox;
            if (!BSIM3toxm.Given)
                BSIM3toxm.Value = BSIM3tox;

            if (!BSIM3dsub.Given)
                BSIM3dsub.Value = BSIM3drout;
            if (!BSIM3vth0.Given)
                BSIM3vth0.Value = (BSIM3type == 1) ? 0.7 : -0.7;
            if (!BSIM3uc.Given)
                BSIM3uc.Value = (BSIM3mobMod.Value == 3) ? -0.0465 : -0.0465e-9;
            if (!BSIM3uc1.Given)
                BSIM3uc1.Value = (BSIM3mobMod.Value == 3) ? -0.056 : -0.056e-9;
            if (!BSIM3u0.Given)
                BSIM3u0.Value = (BSIM3type == 1) ? 0.067 : 0.025;
            if (!BSIM3tnom.Given)
                BSIM3tnom.Value = ckt.State.NominalTemperature;
            if (!BSIM3Llc.Given)
                BSIM3Llc.Value = BSIM3Ll;
            if (!BSIM3Lwc.Given)
                BSIM3Lwc.Value = BSIM3Lw;
            if (!BSIM3Lwlc.Given)
                BSIM3Lwlc.Value = BSIM3Lwl;
            if (!BSIM3Wlc.Given)
                BSIM3Wlc.Value = BSIM3Wl;
            if (!BSIM3Wwc.Given)
                BSIM3Wwc.Value = BSIM3Ww;
            if (!BSIM3Wwlc.Given)
                BSIM3Wwlc.Value = BSIM3Wwl;
            if (!BSIM3dwc.Given)
                BSIM3dwc.Value = BSIM3Wint;
            if (!BSIM3dlc.Given)
                BSIM3dlc.Value = BSIM3Lint;
            if (!BSIM3cf.Given)
                BSIM3cf.Value = 2.0 * Transistor.EPSOX / Circuit.CONSTPI * Math.Log(1.0 + 0.4e-6 / BSIM3tox);
            if (!BSIM3cgdo.Given)
            {
                if (BSIM3dlc.Given && (BSIM3dlc > 0.0))
                {
                    BSIM3cgdo.Value = BSIM3dlc * BSIM3cox - BSIM3cgdl;
                }
                else
                    BSIM3cgdo.Value = 0.6 * BSIM3xj * BSIM3cox;
            }
            if (!BSIM3cgso.Given)
            {
                if (BSIM3dlc.Given && (BSIM3dlc > 0.0))
                {
                    BSIM3cgso.Value = BSIM3dlc * BSIM3cox - BSIM3cgsl;
                }
                else
                    BSIM3cgso.Value = 0.6 * BSIM3xj * BSIM3cox;
            }

            if (!BSIM3cgbo.Given)
            {
                BSIM3cgbo.Value = 2.0 * BSIM3dwc * BSIM3cox;
            }
            if (!BSIM3unitLengthGateSidewallJctCap.Given)
                BSIM3unitLengthGateSidewallJctCap.Value = BSIM3unitLengthSidewallJctCap;
            if (!BSIM3GatesidewallJctPotential.Given)
                BSIM3GatesidewallJctPotential.Value = BSIM3sidewallJctPotential;
            if (!BSIM3bulkJctGateSideGradingCoeff.Given)
                BSIM3bulkJctGateSideGradingCoeff.Value = BSIM3bulkJctSideGradingCoeff;
            if (!BSIM3oxideTrapDensityA.Given)
            {
                if (BSIM3type == 1)
                    BSIM3oxideTrapDensityA.Value = 1e20;
                else
                    BSIM3oxideTrapDensityA.Value = 9.9e18;
            }
            if (!BSIM3oxideTrapDensityB.Given)
            {
                if (BSIM3type == 1)
                    BSIM3oxideTrapDensityB.Value = 5e4;
                else
                    BSIM3oxideTrapDensityB.Value = 2.4e3;
            }
            if (!BSIM3oxideTrapDensityC.Given)
            {
                if (BSIM3type == 1)
                    BSIM3oxideTrapDensityC.Value = -1.4e-12;
                else
                    BSIM3oxideTrapDensityC.Value = 1.4e-12;

            }
        }

        /// <summary>
        /// Do temperature-dependent calculations
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Temperature(Circuit ckt)
        {
            double Temp, Tnom, Eg0, Eg, delTemp, T0, T1;

            Temp = ckt.State.Temperature;
            if (BSIM3bulkJctPotential < 0.1)
            {
                BSIM3bulkJctPotential.Value = 0.1;
                CircuitWarning.Warning(this, "Given pb is less than 0.1. Pb is set to 0.1.\n");
            }
            if (BSIM3sidewallJctPotential < 0.1)
            {
                BSIM3sidewallJctPotential.Value = 0.1;
                CircuitWarning.Warning(this, "Given pbsw is less than 0.1. Pbsw is set to 0.1.\n");
            }
            if (BSIM3GatesidewallJctPotential < 0.1)
            {
                BSIM3GatesidewallJctPotential.Value = 0.1;
                CircuitWarning.Warning(this, "Given pbswg is less than 0.1. Pbswg is set to 0.1.\n");
            }

            Tnom = BSIM3tnom;
            TRatio = Temp / Tnom;

            BSIM3vcrit = Circuit.CONSTvt0 * Math.Log(Circuit.CONSTvt0 / (Circuit.CONSTroot2 * 1.0e-14));
            BSIM3factor1 = Math.Sqrt(Transistor.EPSSI / Transistor.EPSOX * BSIM3tox);

            Vtm0 = Transistor.KboQ * Tnom;
            Eg0 = 1.16 - 7.02e-4 * Tnom * Tnom / (Tnom + 1108.0);
            ni = 1.45e10 * (Tnom / 300.15) * Math.Sqrt(Tnom / 300.15) * Math.Exp(21.5565981 - Eg0 / (2.0 * Vtm0));

            BSIM3vtm = Transistor.KboQ * Temp;
            Eg = 1.16 - 7.02e-4 * Temp * Temp / (Temp + 1108.0);
            if (Temp != Tnom)
            {
                T0 = Eg0 / Vtm0 - Eg / BSIM3vtm + BSIM3jctTempExponent * Math.Log(Temp / Tnom);
                T1 = Math.Exp(T0 / BSIM3jctEmissionCoeff);
                BSIM3jctTempSatCurDensity = BSIM3jctSatCurDensity * T1;
                BSIM3jctSidewallTempSatCurDensity = BSIM3jctSidewallSatCurDensity * T1;
            }
            else
            {
                BSIM3jctTempSatCurDensity = BSIM3jctSatCurDensity;
                BSIM3jctSidewallTempSatCurDensity = BSIM3jctSidewallSatCurDensity;
            }

            if (BSIM3jctTempSatCurDensity < 0.0)
                BSIM3jctTempSatCurDensity = 0.0;
            if (BSIM3jctSidewallTempSatCurDensity < 0.0)
                BSIM3jctSidewallTempSatCurDensity = 0.0;

            /* Temperature dependence of D / B and S / B diode capacitance begins */
            delTemp = ckt.State.Temperature - BSIM3tnom;
            T0 = BSIM3tcj * delTemp;
            if (T0 >= -1.0)
            {
                BSIM3unitAreaTempJctCap = BSIM3unitAreaJctCap * (1.0 + T0);
            }
            else if (BSIM3unitAreaJctCap > 0.0)
            {
                BSIM3unitAreaTempJctCap = 0.0;
                CircuitWarning.Warning(this, "Temperature effect has caused cj to be negative. Cj is clamped to zero.\n");
            }
            T0 = BSIM3tcjsw * delTemp;
            if (T0 >= -1.0)
            {
                BSIM3unitLengthSidewallTempJctCap = BSIM3unitLengthSidewallJctCap * (1.0 + T0);
            }
            else if (BSIM3unitLengthSidewallJctCap > 0.0)
            {
                BSIM3unitLengthSidewallTempJctCap = 0.0;
                CircuitWarning.Warning(this, "Temperature effect has caused cjsw to be negative. Cjsw is clamped to zero.\n");
            }
            T0 = BSIM3tcjswg * delTemp;
            if (T0 >= -1.0)
            {
                BSIM3unitLengthGateSidewallTempJctCap = BSIM3unitLengthGateSidewallJctCap * (1.0 + T0);
            }
            else if (BSIM3unitLengthGateSidewallJctCap > 0.0)
            {
                BSIM3unitLengthGateSidewallTempJctCap = 0.0;
                CircuitWarning.Warning(this, "Temperature effect has caused cjswg to be negative. Cjswg is clamped to zero.\n");
            }

            BSIM3PhiB = BSIM3bulkJctPotential - BSIM3tpb * delTemp;
            if (BSIM3PhiB < 0.01)
            {
                BSIM3PhiB = 0.01;
                CircuitWarning.Warning(this, "Temperature effect has caused pb to be less than 0.01. Pb is clamped to 0.01.\n");
            }
            BSIM3PhiBSW = BSIM3sidewallJctPotential - BSIM3tpbsw * delTemp;
            if (BSIM3PhiBSW <= 0.01)
            {
                BSIM3PhiBSW = 0.01;
                CircuitWarning.Warning(this, "Temperature effect has caused pbsw to be less than 0.01. Pbsw is clamped to 0.01.\n");
            }
            BSIM3PhiBSWG = BSIM3GatesidewallJctPotential - BSIM3tpbswg * delTemp;
            if (BSIM3PhiBSWG <= 0.01)
            {
                BSIM3PhiBSWG = 0.01;
                CircuitWarning.Warning(this, "Temperature effect has caused pbswg to be less than 0.01. Pbswg is clamped to 0.01.\n");
            }
        }
    }
}
