using System;
using System.Collections.Generic;
using SpiceSharp.Circuits;
using SpiceSharp.Diagnostics;
using SpiceSharp.Parameters;
using SpiceSharp.Components.Transistors;

namespace SpiceSharp.Components
{
    public class BSIM3v24Model : CircuitModel<BSIM3v24Model>
    {
        /// <summary>
        /// Get the size-dependent parameters
        /// </summary>
        public Dictionary<Tuple<double, double>, BSIM3SizeDependParam> Sizes { get; } = new Dictionary<Tuple<double, double>, BSIM3SizeDependParam>();

        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("mobmod"), SpiceInfo("Mobility model selector")]
        public Parameter BSIM3mobMod { get; } = new Parameter(1);
        [SpiceName("binunit"), SpiceInfo("Bin  unit  selector")]
        public Parameter BSIM3binUnit { get; } = new Parameter(1);
        [SpiceName("paramchk"), SpiceInfo("Model parameter checking selector")]
        public Parameter BSIM3paramChk { get; } = new Parameter();
        [SpiceName("capmod"), SpiceInfo("Capacitance model selector")]
        public Parameter BSIM3capMod { get; } = new Parameter(3);
        [SpiceName("noimod"), SpiceInfo("Noise model selector")]
        public Parameter BSIM3noiMod { get; } = new Parameter(1);
        [SpiceName("version"), SpiceInfo(" parameter for model version")]
        public string BSIM3version { get; } = "3.2.4";
        [SpiceName("tox"), SpiceInfo("Gate oxide thickness in meters")]
        public Parameter BSIM3tox { get; } = new Parameter(150.0e-10);
        [SpiceName("toxm"), SpiceInfo("Gate oxide thickness used in extraction")]
        public Parameter BSIM3toxm { get; } = new Parameter();
        [SpiceName("cdsc"), SpiceInfo("Drain/Source and channel coupling capacitance")]
        public Parameter BSIM3cdsc { get; } = new Parameter(2.4e-4);
        [SpiceName("cdscb"), SpiceInfo("Body-bias dependence of cdsc")]
        public Parameter BSIM3cdscb { get; } = new Parameter();
        [SpiceName("cdscd"), SpiceInfo("Drain-bias dependence of cdsc")]
        public Parameter BSIM3cdscd { get; } = new Parameter();
        [SpiceName("cit"), SpiceInfo("Interface state capacitance")]
        public Parameter BSIM3cit { get; } = new Parameter();
        [SpiceName("nfactor"), SpiceInfo("Subthreshold swing Coefficient")]
        public Parameter BSIM3nfactor { get; } = new Parameter(1);
        [SpiceName("xj"), SpiceInfo("Junction depth in meters")]
        public Parameter BSIM3xj { get; } = new Parameter(.15e-6);
        [SpiceName("vsat"), SpiceInfo("Saturation velocity at tnom")]
        public Parameter BSIM3vsat { get; } = new Parameter(8.0e4);
        [SpiceName("a0"), SpiceInfo("Non-uniform depletion width effect coefficient.")]
        public Parameter BSIM3a0 { get; } = new Parameter(1.0);
        [SpiceName("ags"), SpiceInfo("Gate bias  coefficient of Abulk.")]
        public Parameter BSIM3ags { get; } = new Parameter();
        [SpiceName("a1"), SpiceInfo("Non-saturation effect coefficient")]
        public Parameter BSIM3a1 { get; } = new Parameter();
        [SpiceName("a2"), SpiceInfo("Non-saturation effect coefficient")]
        public Parameter BSIM3a2 { get; } = new Parameter(1.0);
        [SpiceName("at"), SpiceInfo("Temperature coefficient of vsat")]
        public Parameter BSIM3at { get; } = new Parameter(3.3e4);
        [SpiceName("keta"), SpiceInfo("Body-bias coefficient of non-uniform depletion width effect.")]
        public Parameter BSIM3keta { get; } = new Parameter(-0.047);
        [SpiceName("nsub"), SpiceInfo("Substrate doping concentration")]
        public Parameter BSIM3nsub { get; } = new Parameter(6.0e16);
        [SpiceName("nch"), SpiceInfo("Channel doping concentration")]
        public double BSIM3_NPEAK
        {
            get => BSIM3npeak;
            set
            {
                BSIM3npeak.Set(value);
                if (BSIM3npeak > 1.0e20)
                    BSIM3npeak.Value *= 1.0e-6;
            }
        }
        public Parameter BSIM3npeak { get; } = new Parameter(1.7e17);
        [SpiceName("ngate"), SpiceInfo("Poly-gate doping concentration")]
        public double BSIM3_NGATE
        {
            get => BSIM3ngate;
            set
            {
                BSIM3ngate.Set(value);
                if (BSIM3ngate > 1.0e23)
                    BSIM3ngate.Value *= 1.0e-6;
            }
        }
        public Parameter BSIM3ngate { get; } = new Parameter();
        [SpiceName("gamma1"), SpiceInfo("Vth body coefficient")]
        public Parameter BSIM3gamma1 { get; } = new Parameter();
        [SpiceName("gamma2"), SpiceInfo("Vth body coefficient")]
        public Parameter BSIM3gamma2 { get; } = new Parameter();
        [SpiceName("vbx"), SpiceInfo("Vth transition body Voltage")]
        public Parameter BSIM3vbx { get; } = new Parameter();
        [SpiceName("vbm"), SpiceInfo("Maximum body voltage")]
        public Parameter BSIM3vbm { get; } = new Parameter(-3.0);
        [SpiceName("xt"), SpiceInfo("Doping depth")]
        public Parameter BSIM3xt { get; } = new Parameter(1.55e-7);
        [SpiceName("k1"), SpiceInfo("Bulk effect coefficient 1")]
        public Parameter BSIM3k1 { get; } = new Parameter();
        [SpiceName("kt1"), SpiceInfo("Temperature coefficient of Vth")]
        public Parameter BSIM3kt1 { get; } = new Parameter(-0.11);
        [SpiceName("kt1l"), SpiceInfo("Temperature coefficient of Vth")]
        public Parameter BSIM3kt1l { get; } = new Parameter();
        [SpiceName("kt2"), SpiceInfo("Body-coefficient of kt1")]
        public Parameter BSIM3kt2 { get; } = new Parameter(0.022);
        [SpiceName("k2"), SpiceInfo("Bulk effect coefficient 2")]
        public Parameter BSIM3k2 { get; } = new Parameter();
        [SpiceName("k3"), SpiceInfo("Narrow width effect coefficient")]
        public Parameter BSIM3k3 { get; } = new Parameter(80.0);
        [SpiceName("k3b"), SpiceInfo("Body effect coefficient of k3")]
        public Parameter BSIM3k3b { get; } = new Parameter();
        [SpiceName("nlx"), SpiceInfo("Lateral non-uniform doping effect")]
        public Parameter BSIM3nlx { get; } = new Parameter(1.74e-7);
        [SpiceName("w0"), SpiceInfo("Narrow width effect parameter")]
        public Parameter BSIM3w0 { get; } = new Parameter(2.5e-6);
        [SpiceName("dvt0"), SpiceInfo("Short channel effect coeff. 0")]
        public Parameter BSIM3dvt0 { get; } = new Parameter(2.2);
        [SpiceName("dvt1"), SpiceInfo("Short channel effect coeff. 1")]
        public Parameter BSIM3dvt1 { get; } = new Parameter(0.53);
        [SpiceName("dvt2"), SpiceInfo("Short channel effect coeff. 2")]
        public Parameter BSIM3dvt2 { get; } = new Parameter(-0.032);
        [SpiceName("dvt0w"), SpiceInfo("Narrow Width coeff. 0")]
        public Parameter BSIM3dvt0w { get; } = new Parameter();
        [SpiceName("dvt1w"), SpiceInfo("Narrow Width effect coeff. 1")]
        public Parameter BSIM3dvt1w { get; } = new Parameter(5.3e6);
        [SpiceName("dvt2w"), SpiceInfo("Narrow Width effect coeff. 2")]
        public Parameter BSIM3dvt2w { get; } = new Parameter(-0.032);
        [SpiceName("drout"), SpiceInfo("DIBL coefficient of output resistance")]
        public Parameter BSIM3drout { get; } = new Parameter(0.56);
        [SpiceName("dsub"), SpiceInfo("DIBL coefficient in the subthreshold region")]
        public Parameter BSIM3dsub { get; } = new Parameter();
        [SpiceName("vth0"), SpiceName("vtho"), SpiceInfo("Threshold voltage")]
        public Parameter BSIM3vth0 { get; } = new Parameter();
        [SpiceName("ua"), SpiceInfo("Linear gate dependence of mobility")]
        public Parameter BSIM3ua { get; } = new Parameter(2.25e-9);
        [SpiceName("ua1"), SpiceInfo("Temperature coefficient of ua")]
        public Parameter BSIM3ua1 { get; } = new Parameter(4.31e-9);
        [SpiceName("ub"), SpiceInfo("Quadratic gate dependence of mobility")]
        public Parameter BSIM3ub { get; } = new Parameter(5.87e-19);
        [SpiceName("ub1"), SpiceInfo("Temperature coefficient of ub")]
        public Parameter BSIM3ub1 { get; } = new Parameter(-7.61e-18);
        [SpiceName("uc"), SpiceInfo("Body-bias dependence of mobility")]
        public Parameter BSIM3uc { get; } = new Parameter();
        [SpiceName("uc1"), SpiceInfo("Temperature coefficient of uc")]
        public Parameter BSIM3uc1 { get; } = new Parameter();
        [SpiceName("u0"), SpiceInfo("Low-field mobility at Tnom")]
        public Parameter BSIM3u0 { get; } = new Parameter();
        [SpiceName("ute"), SpiceInfo("Temperature coefficient of mobility")]
        public Parameter BSIM3ute { get; } = new Parameter(-1.5);
        [SpiceName("voff"), SpiceInfo("Threshold voltage offset")]
        public Parameter BSIM3voff { get; } = new Parameter(-0.08);
        [SpiceName("delta"), SpiceInfo("Effective Vds parameter")]
        public Parameter BSIM3delta { get; } = new Parameter(0.01);
        [SpiceName("rdsw"), SpiceInfo("Source-drain resistance per width")]
        public Parameter BSIM3rdsw { get; } = new Parameter();
        [SpiceName("prwg"), SpiceInfo("Gate-bias effect on parasitic resistance ")]
        public Parameter BSIM3prwg { get; } = new Parameter();
        [SpiceName("prwb"), SpiceInfo("Body-effect on parasitic resistance ")]
        public Parameter BSIM3prwb { get; } = new Parameter();
        [SpiceName("prt"), SpiceInfo("Temperature coefficient of parasitic resistance ")]
        public Parameter BSIM3prt { get; } = new Parameter();
        [SpiceName("eta0"), SpiceInfo("Subthreshold region DIBL coefficient")]
        public Parameter BSIM3eta0 { get; } = new Parameter(0.08);
        [SpiceName("etab"), SpiceInfo("Subthreshold region DIBL coefficient")]
        public Parameter BSIM3etab { get; } = new Parameter(-0.07);
        [SpiceName("pclm"), SpiceInfo("Channel length modulation Coefficient")]
        public Parameter BSIM3pclm { get; } = new Parameter(1.3);
        [SpiceName("pdiblc1"), SpiceInfo("Drain-induced barrier lowering coefficient")]
        public Parameter BSIM3pdibl1 { get; } = new Parameter(.39);
        [SpiceName("pdiblc2"), SpiceInfo("Drain-induced barrier lowering coefficient")]
        public Parameter BSIM3pdibl2 { get; } = new Parameter(0.0086);
        [SpiceName("pdiblcb"), SpiceInfo("Body-effect on drain-induced barrier lowering")]
        public Parameter BSIM3pdiblb { get; } = new Parameter();
        [SpiceName("pscbe1"), SpiceInfo("Substrate current body-effect coefficient")]
        public Parameter BSIM3pscbe1 { get; } = new Parameter(4.24e8);
        [SpiceName("pscbe2"), SpiceInfo("Substrate current body-effect coefficient")]
        public Parameter BSIM3pscbe2 { get; } = new Parameter(1.0e-5);
        [SpiceName("pvag"), SpiceInfo("Gate dependence of output resistance parameter")]
        public Parameter BSIM3pvag { get; } = new Parameter();
        [SpiceName("wr"), SpiceInfo("Width dependence of rds")]
        public Parameter BSIM3wr { get; } = new Parameter(1.0);
        [SpiceName("dwg"), SpiceInfo("Width reduction parameter")]
        public Parameter BSIM3dwg { get; } = new Parameter();
        [SpiceName("dwb"), SpiceInfo("Width reduction parameter")]
        public Parameter BSIM3dwb { get; } = new Parameter();
        [SpiceName("b0"), SpiceInfo("Abulk narrow width parameter")]
        public Parameter BSIM3b0 { get; } = new Parameter();
        [SpiceName("b1"), SpiceInfo("Abulk narrow width parameter")]
        public Parameter BSIM3b1 { get; } = new Parameter();
        [SpiceName("alpha0"), SpiceInfo("substrate current model parameter")]
        public Parameter BSIM3alpha0 { get; } = new Parameter();
        [SpiceName("alpha1"), SpiceInfo("substrate current model parameter")]
        public Parameter BSIM3alpha1 { get; } = new Parameter();
        [SpiceName("beta0"), SpiceInfo("substrate current model parameter")]
        public Parameter BSIM3beta0 { get; } = new Parameter(30.0);
        [SpiceName("ijth"), SpiceInfo("Diode limiting current")]
        public Parameter BSIM3ijth { get; } = new Parameter(0.1);
        [SpiceName("vfb"), SpiceInfo("Flat Band Voltage")]
        public Parameter BSIM3vfb { get; } = new Parameter();
        [SpiceName("elm"), SpiceInfo("Non-quasi-static Elmore Constant Parameter")]
        public Parameter BSIM3elm { get; } = new Parameter(5.0);
        [SpiceName("cgsl"), SpiceInfo("New C-V model parameter")]
        public Parameter BSIM3cgsl { get; } = new Parameter();
        [SpiceName("cgdl"), SpiceInfo("New C-V model parameter")]
        public Parameter BSIM3cgdl { get; } = new Parameter();
        [SpiceName("ckappa"), SpiceInfo("New C-V model parameter")]
        public Parameter BSIM3ckappa { get; } = new Parameter(0.6);
        [SpiceName("cf"), SpiceInfo("Fringe capacitance parameter")]
        public Parameter BSIM3cf { get; } = new Parameter();
        [SpiceName("clc"), SpiceInfo("Vdsat parameter for C-V model")]
        public Parameter BSIM3clc { get; } = new Parameter(0.1e-6);
        [SpiceName("cle"), SpiceInfo("Vdsat parameter for C-V model")]
        public Parameter BSIM3cle { get; } = new Parameter(0.6);
        [SpiceName("dwc"), SpiceInfo("Delta W for C-V model")]
        public Parameter BSIM3dwc { get; } = new Parameter();
        [SpiceName("dlc"), SpiceInfo("Delta L for C-V model")]
        public Parameter BSIM3dlc { get; } = new Parameter();
        [SpiceName("vfbcv"), SpiceInfo("Flat Band Voltage parameter for capmod=0 only")]
        public Parameter BSIM3vfbcv { get; } = new Parameter(-1.0);
        [SpiceName("acde"), SpiceInfo("Exponential coefficient for finite charge thickness")]
        public Parameter BSIM3acde { get; } = new Parameter(1.0);
        [SpiceName("moin"), SpiceInfo("Coefficient for gate-bias dependent surface potential")]
        public Parameter BSIM3moin { get; } = new Parameter(15.0);
        [SpiceName("noff"), SpiceInfo("C-V turn-on/off parameter")]
        public Parameter BSIM3noff { get; } = new Parameter(1.0);
        [SpiceName("voffcv"), SpiceInfo("C-V lateral-shift parameter")]
        public Parameter BSIM3voffcv { get; } = new Parameter();
        [SpiceName("tcj"), SpiceInfo("Temperature coefficient of cj")]
        public Parameter BSIM3tcj { get; } = new Parameter();
        [SpiceName("tpb"), SpiceInfo("Temperature coefficient of pb")]
        public Parameter BSIM3tpb { get; } = new Parameter();
        [SpiceName("tcjsw"), SpiceInfo("Temperature coefficient of cjsw")]
        public Parameter BSIM3tcjsw { get; } = new Parameter();
        [SpiceName("tpbsw"), SpiceInfo("Temperature coefficient of pbsw")]
        public Parameter BSIM3tpbsw { get; } = new Parameter();
        [SpiceName("tcjswg"), SpiceInfo("Temperature coefficient of cjswg")]
        public Parameter BSIM3tcjswg { get; } = new Parameter();
        [SpiceName("tpbswg"), SpiceInfo("Temperature coefficient of pbswg")]
        public Parameter BSIM3tpbswg { get; } = new Parameter();
        [SpiceName("lcdsc"), SpiceInfo("Length dependence of cdsc")]
        public Parameter BSIM3lcdsc { get; } = new Parameter();
        [SpiceName("lcdscb"), SpiceInfo("Length dependence of cdscb")]
        public Parameter BSIM3lcdscb { get; } = new Parameter();
        [SpiceName("lcdscd"), SpiceInfo("Length dependence of cdscd")]
        public Parameter BSIM3lcdscd { get; } = new Parameter();
        [SpiceName("lcit"), SpiceInfo("Length dependence of cit")]
        public Parameter BSIM3lcit { get; } = new Parameter();
        [SpiceName("lnfactor"), SpiceInfo("Length dependence of nfactor")]
        public Parameter BSIM3lnfactor { get; } = new Parameter();
        [SpiceName("lxj"), SpiceInfo("Length dependence of xj")]
        public Parameter BSIM3lxj { get; } = new Parameter();
        [SpiceName("lvsat"), SpiceInfo("Length dependence of vsat")]
        public Parameter BSIM3lvsat { get; } = new Parameter();
        [SpiceName("la0"), SpiceInfo("Length dependence of a0")]
        public Parameter BSIM3la0 { get; } = new Parameter();
        [SpiceName("lags"), SpiceInfo("Length dependence of ags")]
        public Parameter BSIM3lags { get; } = new Parameter();
        [SpiceName("la1"), SpiceInfo("Length dependence of a1")]
        public Parameter BSIM3la1 { get; } = new Parameter();
        [SpiceName("la2"), SpiceInfo("Length dependence of a2")]
        public Parameter BSIM3la2 { get; } = new Parameter();
        [SpiceName("lat"), SpiceInfo("Length dependence of at")]
        public Parameter BSIM3lat { get; } = new Parameter();
        [SpiceName("lketa"), SpiceInfo("Length dependence of keta")]
        public Parameter BSIM3lketa { get; } = new Parameter();
        [SpiceName("lnsub"), SpiceInfo("Length dependence of nsub")]
        public Parameter BSIM3lnsub { get; } = new Parameter();
        [SpiceName("lnch"), SpiceInfo("Length dependence of nch")]
        public double BSIM3_LNPEAK
        {
            get => BSIM3lnpeak;
            set
            {
                BSIM3lnpeak.Set(value);
                if (BSIM3lnpeak > 1.0e20)
                    BSIM3lnpeak.Value *= 1.0e-6;
            }
        }
        public Parameter BSIM3lnpeak { get; } = new Parameter();
        [SpiceName("lngate"), SpiceInfo("Length dependence of ngate")]
        public double BSIM3_LNGATE
        {
            get => BSIM3lngate;
            set
            {
                BSIM3lngate.Set(value);
                if (BSIM3lngate > 1.0e23)
                    BSIM3lngate.Value *= 1.0e-6;
            }
        }
        public Parameter BSIM3lngate { get; } = new Parameter();
        [SpiceName("lgamma1"), SpiceInfo("Length dependence of gamma1")]
        public Parameter BSIM3lgamma1 { get; } = new Parameter();
        [SpiceName("lgamma2"), SpiceInfo("Length dependence of gamma2")]
        public Parameter BSIM3lgamma2 { get; } = new Parameter();
        [SpiceName("lvbx"), SpiceInfo("Length dependence of vbx")]
        public Parameter BSIM3lvbx { get; } = new Parameter();
        [SpiceName("lvbm"), SpiceInfo("Length dependence of vbm")]
        public Parameter BSIM3lvbm { get; } = new Parameter();
        [SpiceName("lxt"), SpiceInfo("Length dependence of xt")]
        public Parameter BSIM3lxt { get; } = new Parameter();
        [SpiceName("lk1"), SpiceInfo("Length dependence of k1")]
        public Parameter BSIM3lk1 { get; } = new Parameter();
        [SpiceName("lkt1"), SpiceInfo("Length dependence of kt1")]
        public Parameter BSIM3lkt1 { get; } = new Parameter();
        [SpiceName("lkt1l"), SpiceInfo("Length dependence of kt1l")]
        public Parameter BSIM3lkt1l { get; } = new Parameter();
        [SpiceName("lkt2"), SpiceInfo("Length dependence of kt2")]
        public Parameter BSIM3lkt2 { get; } = new Parameter();
        [SpiceName("lk2"), SpiceInfo("Length dependence of k2")]
        public Parameter BSIM3lk2 { get; } = new Parameter();
        [SpiceName("lk3"), SpiceInfo("Length dependence of k3")]
        public Parameter BSIM3lk3 { get; } = new Parameter();
        [SpiceName("lk3b"), SpiceInfo("Length dependence of k3b")]
        public Parameter BSIM3lk3b { get; } = new Parameter();
        [SpiceName("lnlx"), SpiceInfo("Length dependence of nlx")]
        public Parameter BSIM3lnlx { get; } = new Parameter();
        [SpiceName("lw0"), SpiceInfo("Length dependence of w0")]
        public Parameter BSIM3lw0 { get; } = new Parameter();
        [SpiceName("ldvt0"), SpiceInfo("Length dependence of dvt0")]
        public Parameter BSIM3ldvt0 { get; } = new Parameter();
        [SpiceName("ldvt1"), SpiceInfo("Length dependence of dvt1")]
        public Parameter BSIM3ldvt1 { get; } = new Parameter();
        [SpiceName("ldvt2"), SpiceInfo("Length dependence of dvt2")]
        public Parameter BSIM3ldvt2 { get; } = new Parameter();
        [SpiceName("ldvt0w"), SpiceInfo("Length dependence of dvt0w")]
        public Parameter BSIM3ldvt0w { get; } = new Parameter();
        [SpiceName("ldvt1w"), SpiceInfo("Length dependence of dvt1w")]
        public Parameter BSIM3ldvt1w { get; } = new Parameter();
        [SpiceName("ldvt2w"), SpiceInfo("Length dependence of dvt2w")]
        public Parameter BSIM3ldvt2w { get; } = new Parameter();
        [SpiceName("ldrout"), SpiceInfo("Length dependence of drout")]
        public Parameter BSIM3ldrout { get; } = new Parameter();
        [SpiceName("ldsub"), SpiceInfo("Length dependence of dsub")]
        public Parameter BSIM3ldsub { get; } = new Parameter();
        [SpiceName("lvth0"), SpiceName("lvtho"), SpiceInfo("Length dependence of vto")]
        public Parameter BSIM3lvth0 { get; } = new Parameter();
        [SpiceName("lua"), SpiceInfo("Length dependence of ua")]
        public Parameter BSIM3lua { get; } = new Parameter();
        [SpiceName("lua1"), SpiceInfo("Length dependence of ua1")]
        public Parameter BSIM3lua1 { get; } = new Parameter();
        [SpiceName("lub"), SpiceInfo("Length dependence of ub")]
        public Parameter BSIM3lub { get; } = new Parameter();
        [SpiceName("lub1"), SpiceInfo("Length dependence of ub1")]
        public Parameter BSIM3lub1 { get; } = new Parameter();
        [SpiceName("luc"), SpiceInfo("Length dependence of uc")]
        public Parameter BSIM3luc { get; } = new Parameter();
        [SpiceName("luc1"), SpiceInfo("Length dependence of uc1")]
        public Parameter BSIM3luc1 { get; } = new Parameter();
        [SpiceName("lu0"), SpiceInfo("Length dependence of u0")]
        public Parameter BSIM3lu0 { get; } = new Parameter();
        [SpiceName("lute"), SpiceInfo("Length dependence of ute")]
        public Parameter BSIM3lute { get; } = new Parameter();
        [SpiceName("lvoff"), SpiceInfo("Length dependence of voff")]
        public Parameter BSIM3lvoff { get; } = new Parameter();
        [SpiceName("ldelta"), SpiceInfo("Length dependence of delta")]
        public Parameter BSIM3ldelta { get; } = new Parameter();
        [SpiceName("lrdsw"), SpiceInfo("Length dependence of rdsw ")]
        public Parameter BSIM3lrdsw { get; } = new Parameter();
        [SpiceName("lprwb"), SpiceInfo("Length dependence of prwb ")]
        public Parameter BSIM3lprwb { get; } = new Parameter();
        [SpiceName("lprwg"), SpiceInfo("Length dependence of prwg ")]
        public Parameter BSIM3lprwg { get; } = new Parameter();
        [SpiceName("lprt"), SpiceInfo("Length dependence of prt ")]
        public Parameter BSIM3lprt { get; } = new Parameter();
        [SpiceName("leta0"), SpiceInfo("Length dependence of eta0")]
        public Parameter BSIM3leta0 { get; } = new Parameter();
        [SpiceName("letab"), SpiceInfo("Length dependence of etab")]
        public Parameter BSIM3letab { get; } = new Parameter(-0.0);
        [SpiceName("lpclm"), SpiceInfo("Length dependence of pclm")]
        public Parameter BSIM3lpclm { get; } = new Parameter();
        [SpiceName("lpdiblc1"), SpiceInfo("Length dependence of pdiblc1")]
        public Parameter BSIM3lpdibl1 { get; } = new Parameter();
        [SpiceName("lpdiblc2"), SpiceInfo("Length dependence of pdiblc2")]
        public Parameter BSIM3lpdibl2 { get; } = new Parameter();
        [SpiceName("lpdiblcb"), SpiceInfo("Length dependence of pdiblcb")]
        public Parameter BSIM3lpdiblb { get; } = new Parameter();
        [SpiceName("lpscbe1"), SpiceInfo("Length dependence of pscbe1")]
        public Parameter BSIM3lpscbe1 { get; } = new Parameter();
        [SpiceName("lpscbe2"), SpiceInfo("Length dependence of pscbe2")]
        public Parameter BSIM3lpscbe2 { get; } = new Parameter();
        [SpiceName("lpvag"), SpiceInfo("Length dependence of pvag")]
        public Parameter BSIM3lpvag { get; } = new Parameter();
        [SpiceName("lwr"), SpiceInfo("Length dependence of wr")]
        public Parameter BSIM3lwr { get; } = new Parameter();
        [SpiceName("ldwg"), SpiceInfo("Length dependence of dwg")]
        public Parameter BSIM3ldwg { get; } = new Parameter();
        [SpiceName("ldwb"), SpiceInfo("Length dependence of dwb")]
        public Parameter BSIM3ldwb { get; } = new Parameter();
        [SpiceName("lb0"), SpiceInfo("Length dependence of b0")]
        public Parameter BSIM3lb0 { get; } = new Parameter();
        [SpiceName("lb1"), SpiceInfo("Length dependence of b1")]
        public Parameter BSIM3lb1 { get; } = new Parameter();
        [SpiceName("lalpha0"), SpiceInfo("Length dependence of alpha0")]
        public Parameter BSIM3lalpha0 { get; } = new Parameter();
        [SpiceName("lalpha1"), SpiceInfo("Length dependence of alpha1")]
        public Parameter BSIM3lalpha1 { get; } = new Parameter();
        [SpiceName("lbeta0"), SpiceInfo("Length dependence of beta0")]
        public Parameter BSIM3lbeta0 { get; } = new Parameter();
        [SpiceName("lvfb"), SpiceInfo("Length dependence of vfb")]
        public Parameter BSIM3lvfb { get; } = new Parameter();
        [SpiceName("lelm"), SpiceInfo("Length dependence of elm")]
        public Parameter BSIM3lelm { get; } = new Parameter();
        [SpiceName("lcgsl"), SpiceInfo("Length dependence of cgsl")]
        public Parameter BSIM3lcgsl { get; } = new Parameter();
        [SpiceName("lcgdl"), SpiceInfo("Length dependence of cgdl")]
        public Parameter BSIM3lcgdl { get; } = new Parameter();
        [SpiceName("lckappa"), SpiceInfo("Length dependence of ckappa")]
        public Parameter BSIM3lckappa { get; } = new Parameter();
        [SpiceName("lcf"), SpiceInfo("Length dependence of cf")]
        public Parameter BSIM3lcf { get; } = new Parameter();
        [SpiceName("lclc"), SpiceInfo("Length dependence of clc")]
        public Parameter BSIM3lclc { get; } = new Parameter();
        [SpiceName("lcle"), SpiceInfo("Length dependence of cle")]
        public Parameter BSIM3lcle { get; } = new Parameter();
        [SpiceName("lvfbcv"), SpiceInfo("Length dependence of vfbcv")]
        public Parameter BSIM3lvfbcv { get; } = new Parameter();
        [SpiceName("lacde"), SpiceInfo("Length dependence of acde")]
        public Parameter BSIM3lacde { get; } = new Parameter();
        [SpiceName("lmoin"), SpiceInfo("Length dependence of moin")]
        public Parameter BSIM3lmoin { get; } = new Parameter();
        [SpiceName("lnoff"), SpiceInfo("Length dependence of noff")]
        public Parameter BSIM3lnoff { get; } = new Parameter();
        [SpiceName("lvoffcv"), SpiceInfo("Length dependence of voffcv")]
        public Parameter BSIM3lvoffcv { get; } = new Parameter();
        [SpiceName("wcdsc"), SpiceInfo("Width dependence of cdsc")]
        public Parameter BSIM3wcdsc { get; } = new Parameter();
        [SpiceName("wcdscb"), SpiceInfo("Width dependence of cdscb")]
        public Parameter BSIM3wcdscb { get; } = new Parameter();
        [SpiceName("wcdscd"), SpiceInfo("Width dependence of cdscd")]
        public Parameter BSIM3wcdscd { get; } = new Parameter();
        [SpiceName("wcit"), SpiceInfo("Width dependence of cit")]
        public Parameter BSIM3wcit { get; } = new Parameter();
        [SpiceName("wnfactor"), SpiceInfo("Width dependence of nfactor")]
        public Parameter BSIM3wnfactor { get; } = new Parameter();
        [SpiceName("wxj"), SpiceInfo("Width dependence of xj")]
        public Parameter BSIM3wxj { get; } = new Parameter();
        [SpiceName("wvsat"), SpiceInfo("Width dependence of vsat")]
        public Parameter BSIM3wvsat { get; } = new Parameter();
        [SpiceName("wa0"), SpiceInfo("Width dependence of a0")]
        public Parameter BSIM3wa0 { get; } = new Parameter();
        [SpiceName("wags"), SpiceInfo("Width dependence of ags")]
        public Parameter BSIM3wags { get; } = new Parameter();
        [SpiceName("wa1"), SpiceInfo("Width dependence of a1")]
        public Parameter BSIM3wa1 { get; } = new Parameter();
        [SpiceName("wa2"), SpiceInfo("Width dependence of a2")]
        public Parameter BSIM3wa2 { get; } = new Parameter();
        [SpiceName("wat"), SpiceInfo("Width dependence of at")]
        public Parameter BSIM3wat { get; } = new Parameter();
        [SpiceName("wketa"), SpiceInfo("Width dependence of keta")]
        public Parameter BSIM3wketa { get; } = new Parameter();
        [SpiceName("wnsub"), SpiceInfo("Width dependence of nsub")]
        public Parameter BSIM3wnsub { get; } = new Parameter();
        [SpiceName("wnch"), SpiceInfo("Width dependence of nch")]
        public double BSIM3_WNPEAK
        {
            get => BSIM3wnpeak;
            set
            {
                BSIM3wnpeak.Set(value);
                if (BSIM3wnpeak > 1.0e20)
                    BSIM3wnpeak.Value *= 1.0e-6;
            }
        }
        public Parameter BSIM3wnpeak { get; } = new Parameter();
        [SpiceName("wngate"), SpiceInfo("Width dependence of ngate")]
        public double BSIM3_WNGATE
        {
            get => BSIM3wngate;
            set
            {
                BSIM3wngate.Set(value);
                if (BSIM3wngate > 1.0e23)
                    BSIM3wngate.Value *= 1.0e-6;
            }
        }
        public Parameter BSIM3wngate { get; } = new Parameter();
        [SpiceName("wgamma1"), SpiceInfo("Width dependence of gamma1")]
        public Parameter BSIM3wgamma1 { get; } = new Parameter();
        [SpiceName("wgamma2"), SpiceInfo("Width dependence of gamma2")]
        public Parameter BSIM3wgamma2 { get; } = new Parameter();
        [SpiceName("wvbx"), SpiceInfo("Width dependence of vbx")]
        public Parameter BSIM3wvbx { get; } = new Parameter();
        [SpiceName("wvbm"), SpiceInfo("Width dependence of vbm")]
        public Parameter BSIM3wvbm { get; } = new Parameter();
        [SpiceName("wxt"), SpiceInfo("Width dependence of xt")]
        public Parameter BSIM3wxt { get; } = new Parameter();
        [SpiceName("wk1"), SpiceInfo("Width dependence of k1")]
        public Parameter BSIM3wk1 { get; } = new Parameter();
        [SpiceName("wkt1"), SpiceInfo("Width dependence of kt1")]
        public Parameter BSIM3wkt1 { get; } = new Parameter();
        [SpiceName("wkt1l"), SpiceInfo("Width dependence of kt1l")]
        public Parameter BSIM3wkt1l { get; } = new Parameter();
        [SpiceName("wkt2"), SpiceInfo("Width dependence of kt2")]
        public Parameter BSIM3wkt2 { get; } = new Parameter();
        [SpiceName("wk2"), SpiceInfo("Width dependence of k2")]
        public Parameter BSIM3wk2 { get; } = new Parameter();
        [SpiceName("wk3"), SpiceInfo("Width dependence of k3")]
        public Parameter BSIM3wk3 { get; } = new Parameter();
        [SpiceName("wk3b"), SpiceInfo("Width dependence of k3b")]
        public Parameter BSIM3wk3b { get; } = new Parameter();
        [SpiceName("wnlx"), SpiceInfo("Width dependence of nlx")]
        public Parameter BSIM3wnlx { get; } = new Parameter();
        [SpiceName("ww0"), SpiceInfo("Width dependence of w0")]
        public Parameter BSIM3ww0 { get; } = new Parameter();
        [SpiceName("wdvt0"), SpiceInfo("Width dependence of dvt0")]
        public Parameter BSIM3wdvt0 { get; } = new Parameter();
        [SpiceName("wdvt1"), SpiceInfo("Width dependence of dvt1")]
        public Parameter BSIM3wdvt1 { get; } = new Parameter();
        [SpiceName("wdvt2"), SpiceInfo("Width dependence of dvt2")]
        public Parameter BSIM3wdvt2 { get; } = new Parameter();
        [SpiceName("wdvt0w"), SpiceInfo("Width dependence of dvt0w")]
        public Parameter BSIM3wdvt0w { get; } = new Parameter();
        [SpiceName("wdvt1w"), SpiceInfo("Width dependence of dvt1w")]
        public Parameter BSIM3wdvt1w { get; } = new Parameter();
        [SpiceName("wdvt2w"), SpiceInfo("Width dependence of dvt2w")]
        public Parameter BSIM3wdvt2w { get; } = new Parameter();
        [SpiceName("wdrout"), SpiceInfo("Width dependence of drout")]
        public Parameter BSIM3wdrout { get; } = new Parameter();
        [SpiceName("wdsub"), SpiceInfo("Width dependence of dsub")]
        public Parameter BSIM3wdsub { get; } = new Parameter();
        [SpiceName("wvth0"), SpiceName("wvtho"), SpiceInfo("Width dependence of vto")]
        public Parameter BSIM3wvth0 { get; } = new Parameter();
        [SpiceName("wua"), SpiceInfo("Width dependence of ua")]
        public Parameter BSIM3wua { get; } = new Parameter();
        [SpiceName("wua1"), SpiceInfo("Width dependence of ua1")]
        public Parameter BSIM3wua1 { get; } = new Parameter();
        [SpiceName("wub"), SpiceInfo("Width dependence of ub")]
        public Parameter BSIM3wub { get; } = new Parameter();
        [SpiceName("wub1"), SpiceInfo("Width dependence of ub1")]
        public Parameter BSIM3wub1 { get; } = new Parameter();
        [SpiceName("wuc"), SpiceInfo("Width dependence of uc")]
        public Parameter BSIM3wuc { get; } = new Parameter();
        [SpiceName("wuc1"), SpiceInfo("Width dependence of uc1")]
        public Parameter BSIM3wuc1 { get; } = new Parameter();
        [SpiceName("wu0"), SpiceInfo("Width dependence of u0")]
        public Parameter BSIM3wu0 { get; } = new Parameter();
        [SpiceName("wute"), SpiceInfo("Width dependence of ute")]
        public Parameter BSIM3wute { get; } = new Parameter();
        [SpiceName("wvoff"), SpiceInfo("Width dependence of voff")]
        public Parameter BSIM3wvoff { get; } = new Parameter();
        [SpiceName("wdelta"), SpiceInfo("Width dependence of delta")]
        public Parameter BSIM3wdelta { get; } = new Parameter();
        [SpiceName("wrdsw"), SpiceInfo("Width dependence of rdsw ")]
        public Parameter BSIM3wrdsw { get; } = new Parameter();
        [SpiceName("wprwb"), SpiceInfo("Width dependence of prwb ")]
        public Parameter BSIM3wprwb { get; } = new Parameter();
        [SpiceName("wprwg"), SpiceInfo("Width dependence of prwg ")]
        public Parameter BSIM3wprwg { get; } = new Parameter();
        [SpiceName("wprt"), SpiceInfo("Width dependence of prt")]
        public Parameter BSIM3wprt { get; } = new Parameter();
        [SpiceName("weta0"), SpiceInfo("Width dependence of eta0")]
        public Parameter BSIM3weta0 { get; } = new Parameter();
        [SpiceName("wetab"), SpiceInfo("Width dependence of etab")]
        public Parameter BSIM3wetab { get; } = new Parameter();
        [SpiceName("wpclm"), SpiceInfo("Width dependence of pclm")]
        public Parameter BSIM3wpclm { get; } = new Parameter();
        [SpiceName("wpdiblc1"), SpiceInfo("Width dependence of pdiblc1")]
        public Parameter BSIM3wpdibl1 { get; } = new Parameter();
        [SpiceName("wpdiblc2"), SpiceInfo("Width dependence of pdiblc2")]
        public Parameter BSIM3wpdibl2 { get; } = new Parameter();
        [SpiceName("wpdiblcb"), SpiceInfo("Width dependence of pdiblcb")]
        public Parameter BSIM3wpdiblb { get; } = new Parameter();
        [SpiceName("wpscbe1"), SpiceInfo("Width dependence of pscbe1")]
        public Parameter BSIM3wpscbe1 { get; } = new Parameter();
        [SpiceName("wpscbe2"), SpiceInfo("Width dependence of pscbe2")]
        public Parameter BSIM3wpscbe2 { get; } = new Parameter();
        [SpiceName("wpvag"), SpiceInfo("Width dependence of pvag")]
        public Parameter BSIM3wpvag { get; } = new Parameter();
        [SpiceName("wwr"), SpiceInfo("Width dependence of wr")]
        public Parameter BSIM3wwr { get; } = new Parameter();
        [SpiceName("wdwg"), SpiceInfo("Width dependence of dwg")]
        public Parameter BSIM3wdwg { get; } = new Parameter();
        [SpiceName("wdwb"), SpiceInfo("Width dependence of dwb")]
        public Parameter BSIM3wdwb { get; } = new Parameter();
        [SpiceName("wb0"), SpiceInfo("Width dependence of b0")]
        public Parameter BSIM3wb0 { get; } = new Parameter();
        [SpiceName("wb1"), SpiceInfo("Width dependence of b1")]
        public Parameter BSIM3wb1 { get; } = new Parameter();
        [SpiceName("walpha0"), SpiceInfo("Width dependence of alpha0")]
        public Parameter BSIM3walpha0 { get; } = new Parameter();
        [SpiceName("walpha1"), SpiceInfo("Width dependence of alpha1")]
        public Parameter BSIM3walpha1 { get; } = new Parameter();
        [SpiceName("wbeta0"), SpiceInfo("Width dependence of beta0")]
        public Parameter BSIM3wbeta0 { get; } = new Parameter();
        [SpiceName("wvfb"), SpiceInfo("Width dependence of vfb")]
        public Parameter BSIM3wvfb { get; } = new Parameter();
        [SpiceName("welm"), SpiceInfo("Width dependence of elm")]
        public Parameter BSIM3welm { get; } = new Parameter();
        [SpiceName("wcgsl"), SpiceInfo("Width dependence of cgsl")]
        public Parameter BSIM3wcgsl { get; } = new Parameter();
        [SpiceName("wcgdl"), SpiceInfo("Width dependence of cgdl")]
        public Parameter BSIM3wcgdl { get; } = new Parameter();
        [SpiceName("wckappa"), SpiceInfo("Width dependence of ckappa")]
        public Parameter BSIM3wckappa { get; } = new Parameter();
        [SpiceName("wcf"), SpiceInfo("Width dependence of cf")]
        public Parameter BSIM3wcf { get; } = new Parameter();
        [SpiceName("wclc"), SpiceInfo("Width dependence of clc")]
        public Parameter BSIM3wclc { get; } = new Parameter();
        [SpiceName("wcle"), SpiceInfo("Width dependence of cle")]
        public Parameter BSIM3wcle { get; } = new Parameter();
        [SpiceName("wvfbcv"), SpiceInfo("Width dependence of vfbcv")]
        public Parameter BSIM3wvfbcv { get; } = new Parameter();
        [SpiceName("wacde"), SpiceInfo("Width dependence of acde")]
        public Parameter BSIM3wacde { get; } = new Parameter();
        [SpiceName("wmoin"), SpiceInfo("Width dependence of moin")]
        public Parameter BSIM3wmoin { get; } = new Parameter();
        [SpiceName("wnoff"), SpiceInfo("Width dependence of noff")]
        public Parameter BSIM3wnoff { get; } = new Parameter();
        [SpiceName("wvoffcv"), SpiceInfo("Width dependence of voffcv")]
        public Parameter BSIM3wvoffcv { get; } = new Parameter();
        [SpiceName("pcdsc"), SpiceInfo("Cross-term dependence of cdsc")]
        public Parameter BSIM3pcdsc { get; } = new Parameter();
        [SpiceName("pcdscb"), SpiceInfo("Cross-term dependence of cdscb")]
        public Parameter BSIM3pcdscb { get; } = new Parameter();
        [SpiceName("pcdscd"), SpiceInfo("Cross-term dependence of cdscd")]
        public Parameter BSIM3pcdscd { get; } = new Parameter();
        [SpiceName("pcit"), SpiceInfo("Cross-term dependence of cit")]
        public Parameter BSIM3pcit { get; } = new Parameter();
        [SpiceName("pnfactor"), SpiceInfo("Cross-term dependence of nfactor")]
        public Parameter BSIM3pnfactor { get; } = new Parameter();
        [SpiceName("pxj"), SpiceInfo("Cross-term dependence of xj")]
        public Parameter BSIM3pxj { get; } = new Parameter();
        [SpiceName("pvsat"), SpiceInfo("Cross-term dependence of vsat")]
        public Parameter BSIM3pvsat { get; } = new Parameter();
        [SpiceName("pa0"), SpiceInfo("Cross-term dependence of a0")]
        public Parameter BSIM3pa0 { get; } = new Parameter();
        [SpiceName("pags"), SpiceInfo("Cross-term dependence of ags")]
        public Parameter BSIM3pags { get; } = new Parameter();
        [SpiceName("pa1"), SpiceInfo("Cross-term dependence of a1")]
        public Parameter BSIM3pa1 { get; } = new Parameter();
        [SpiceName("pa2"), SpiceInfo("Cross-term dependence of a2")]
        public Parameter BSIM3pa2 { get; } = new Parameter();
        [SpiceName("pat"), SpiceInfo("Cross-term dependence of at")]
        public Parameter BSIM3pat { get; } = new Parameter();
        [SpiceName("pketa"), SpiceInfo("Cross-term dependence of keta")]
        public Parameter BSIM3pketa { get; } = new Parameter();
        [SpiceName("pnsub"), SpiceInfo("Cross-term dependence of nsub")]
        public Parameter BSIM3pnsub { get; } = new Parameter();
        [SpiceName("pnch"), SpiceInfo("Cross-term dependence of nch")]
        public double BSIM3_PNPEAK
        {
            get => BSIM3pnpeak;
            set
            {
                BSIM3pnpeak.Set(value);
                if (BSIM3pnpeak > 1.0e20)
                    BSIM3pnpeak.Value *= 1.0e-6;
            }
        }
        public Parameter BSIM3pnpeak { get; } = new Parameter();
        [SpiceName("pngate"), SpiceInfo("Cross-term dependence of ngate")]
        public double BSIM3_PNGATE
        {
            get => BSIM3pngate;
            set
            {
                BSIM3pngate.Set(value);
                if (BSIM3pngate > 1.0e23)
                    BSIM3pngate.Value *= 1.0e-6;
            }
        }
        public Parameter BSIM3pngate { get; } = new Parameter();
        [SpiceName("pgamma1"), SpiceInfo("Cross-term dependence of gamma1")]
        public Parameter BSIM3pgamma1 { get; } = new Parameter();
        [SpiceName("pgamma2"), SpiceInfo("Cross-term dependence of gamma2")]
        public Parameter BSIM3pgamma2 { get; } = new Parameter();
        [SpiceName("pvbx"), SpiceInfo("Cross-term dependence of vbx")]
        public Parameter BSIM3pvbx { get; } = new Parameter();
        [SpiceName("pvbm"), SpiceInfo("Cross-term dependence of vbm")]
        public Parameter BSIM3pvbm { get; } = new Parameter();
        [SpiceName("pxt"), SpiceInfo("Cross-term dependence of xt")]
        public Parameter BSIM3pxt { get; } = new Parameter();
        [SpiceName("pk1"), SpiceInfo("Cross-term dependence of k1")]
        public Parameter BSIM3pk1 { get; } = new Parameter();
        [SpiceName("pkt1"), SpiceInfo("Cross-term dependence of kt1")]
        public Parameter BSIM3pkt1 { get; } = new Parameter();
        [SpiceName("pkt1l"), SpiceInfo("Cross-term dependence of kt1l")]
        public Parameter BSIM3pkt1l { get; } = new Parameter();
        [SpiceName("pkt2"), SpiceInfo("Cross-term dependence of kt2")]
        public Parameter BSIM3pkt2 { get; } = new Parameter();
        [SpiceName("pk2"), SpiceInfo("Cross-term dependence of k2")]
        public Parameter BSIM3pk2 { get; } = new Parameter();
        [SpiceName("pk3"), SpiceInfo("Cross-term dependence of k3")]
        public Parameter BSIM3pk3 { get; } = new Parameter();
        [SpiceName("pk3b"), SpiceInfo("Cross-term dependence of k3b")]
        public Parameter BSIM3pk3b { get; } = new Parameter();
        [SpiceName("pnlx"), SpiceInfo("Cross-term dependence of nlx")]
        public Parameter BSIM3pnlx { get; } = new Parameter();
        [SpiceName("pw0"), SpiceInfo("Cross-term dependence of w0")]
        public Parameter BSIM3pw0 { get; } = new Parameter();
        [SpiceName("pdvt0"), SpiceInfo("Cross-term dependence of dvt0")]
        public Parameter BSIM3pdvt0 { get; } = new Parameter();
        [SpiceName("pdvt1"), SpiceInfo("Cross-term dependence of dvt1")]
        public Parameter BSIM3pdvt1 { get; } = new Parameter();
        [SpiceName("pdvt2"), SpiceInfo("Cross-term dependence of dvt2")]
        public Parameter BSIM3pdvt2 { get; } = new Parameter();
        [SpiceName("pdvt0w"), SpiceInfo("Cross-term dependence of dvt0w")]
        public Parameter BSIM3pdvt0w { get; } = new Parameter();
        [SpiceName("pdvt1w"), SpiceInfo("Cross-term dependence of dvt1w")]
        public Parameter BSIM3pdvt1w { get; } = new Parameter();
        [SpiceName("pdvt2w"), SpiceInfo("Cross-term dependence of dvt2w")]
        public Parameter BSIM3pdvt2w { get; } = new Parameter();
        [SpiceName("pdrout"), SpiceInfo("Cross-term dependence of drout")]
        public Parameter BSIM3pdrout { get; } = new Parameter();
        [SpiceName("pdsub"), SpiceInfo("Cross-term dependence of dsub")]
        public Parameter BSIM3pdsub { get; } = new Parameter();
        [SpiceName("pvth0"), SpiceName("pvtho"), SpiceInfo("Cross-term dependence of vto")]
        public Parameter BSIM3pvth0 { get; } = new Parameter();
        [SpiceName("pua"), SpiceInfo("Cross-term dependence of ua")]
        public Parameter BSIM3pua { get; } = new Parameter();
        [SpiceName("pua1"), SpiceInfo("Cross-term dependence of ua1")]
        public Parameter BSIM3pua1 { get; } = new Parameter();
        [SpiceName("pub"), SpiceInfo("Cross-term dependence of ub")]
        public Parameter BSIM3pub { get; } = new Parameter();
        [SpiceName("pub1"), SpiceInfo("Cross-term dependence of ub1")]
        public Parameter BSIM3pub1 { get; } = new Parameter();
        [SpiceName("puc"), SpiceInfo("Cross-term dependence of uc")]
        public Parameter BSIM3puc { get; } = new Parameter();
        [SpiceName("puc1"), SpiceInfo("Cross-term dependence of uc1")]
        public Parameter BSIM3puc1 { get; } = new Parameter();
        [SpiceName("pu0"), SpiceInfo("Cross-term dependence of u0")]
        public Parameter BSIM3pu0 { get; } = new Parameter();
        [SpiceName("pute"), SpiceInfo("Cross-term dependence of ute")]
        public Parameter BSIM3pute { get; } = new Parameter();
        [SpiceName("pvoff"), SpiceInfo("Cross-term dependence of voff")]
        public Parameter BSIM3pvoff { get; } = new Parameter();
        [SpiceName("pdelta"), SpiceInfo("Cross-term dependence of delta")]
        public Parameter BSIM3pdelta { get; } = new Parameter();
        [SpiceName("prdsw"), SpiceInfo("Cross-term dependence of rdsw ")]
        public Parameter BSIM3prdsw { get; } = new Parameter();
        [SpiceName("pprwb"), SpiceInfo("Cross-term dependence of prwb ")]
        public Parameter BSIM3pprwb { get; } = new Parameter();
        [SpiceName("pprwg"), SpiceInfo("Cross-term dependence of prwg ")]
        public Parameter BSIM3pprwg { get; } = new Parameter();
        [SpiceName("pprt"), SpiceInfo("Cross-term dependence of prt ")]
        public Parameter BSIM3pprt { get; } = new Parameter();
        [SpiceName("peta0"), SpiceInfo("Cross-term dependence of eta0")]
        public Parameter BSIM3peta0 { get; } = new Parameter();
        [SpiceName("petab"), SpiceInfo("Cross-term dependence of etab")]
        public Parameter BSIM3petab { get; } = new Parameter();
        [SpiceName("ppclm"), SpiceInfo("Cross-term dependence of pclm")]
        public Parameter BSIM3ppclm { get; } = new Parameter();
        [SpiceName("ppdiblc1"), SpiceInfo("Cross-term dependence of pdiblc1")]
        public Parameter BSIM3ppdibl1 { get; } = new Parameter();
        [SpiceName("ppdiblc2"), SpiceInfo("Cross-term dependence of pdiblc2")]
        public Parameter BSIM3ppdibl2 { get; } = new Parameter();
        [SpiceName("ppdiblcb"), SpiceInfo("Cross-term dependence of pdiblcb")]
        public Parameter BSIM3ppdiblb { get; } = new Parameter();
        [SpiceName("ppscbe1"), SpiceInfo("Cross-term dependence of pscbe1")]
        public Parameter BSIM3ppscbe1 { get; } = new Parameter();
        [SpiceName("ppscbe2"), SpiceInfo("Cross-term dependence of pscbe2")]
        public Parameter BSIM3ppscbe2 { get; } = new Parameter();
        [SpiceName("ppvag"), SpiceInfo("Cross-term dependence of pvag")]
        public Parameter BSIM3ppvag { get; } = new Parameter();
        [SpiceName("pwr"), SpiceInfo("Cross-term dependence of wr")]
        public Parameter BSIM3pwr { get; } = new Parameter();
        [SpiceName("pdwg"), SpiceInfo("Cross-term dependence of dwg")]
        public Parameter BSIM3pdwg { get; } = new Parameter();
        [SpiceName("pdwb"), SpiceInfo("Cross-term dependence of dwb")]
        public Parameter BSIM3pdwb { get; } = new Parameter();
        [SpiceName("pb0"), SpiceInfo("Cross-term dependence of b0")]
        public Parameter BSIM3pb0 { get; } = new Parameter();
        [SpiceName("pb1"), SpiceInfo("Cross-term dependence of b1")]
        public Parameter BSIM3pb1 { get; } = new Parameter();
        [SpiceName("palpha0"), SpiceInfo("Cross-term dependence of alpha0")]
        public Parameter BSIM3palpha0 { get; } = new Parameter();
        [SpiceName("palpha1"), SpiceInfo("Cross-term dependence of alpha1")]
        public Parameter BSIM3palpha1 { get; } = new Parameter();
        [SpiceName("pbeta0"), SpiceInfo("Cross-term dependence of beta0")]
        public Parameter BSIM3pbeta0 { get; } = new Parameter();
        [SpiceName("pvfb"), SpiceInfo("Cross-term dependence of vfb")]
        public Parameter BSIM3pvfb { get; } = new Parameter();
        [SpiceName("pelm"), SpiceInfo("Cross-term dependence of elm")]
        public Parameter BSIM3pelm { get; } = new Parameter();
        [SpiceName("pcgsl"), SpiceInfo("Cross-term dependence of cgsl")]
        public Parameter BSIM3pcgsl { get; } = new Parameter();
        [SpiceName("pcgdl"), SpiceInfo("Cross-term dependence of cgdl")]
        public Parameter BSIM3pcgdl { get; } = new Parameter();
        [SpiceName("pckappa"), SpiceInfo("Cross-term dependence of ckappa")]
        public Parameter BSIM3pckappa { get; } = new Parameter();
        [SpiceName("pcf"), SpiceInfo("Cross-term dependence of cf")]
        public Parameter BSIM3pcf { get; } = new Parameter();
        [SpiceName("pclc"), SpiceInfo("Cross-term dependence of clc")]
        public Parameter BSIM3pclc { get; } = new Parameter();
        [SpiceName("pcle"), SpiceInfo("Cross-term dependence of cle")]
        public Parameter BSIM3pcle { get; } = new Parameter();
        [SpiceName("pvfbcv"), SpiceInfo("Cross-term dependence of vfbcv")]
        public Parameter BSIM3pvfbcv { get; } = new Parameter();
        [SpiceName("pacde"), SpiceInfo("Cross-term dependence of acde")]
        public Parameter BSIM3pacde { get; } = new Parameter();
        [SpiceName("pmoin"), SpiceInfo("Cross-term dependence of moin")]
        public Parameter BSIM3pmoin { get; } = new Parameter();
        [SpiceName("pnoff"), SpiceInfo("Cross-term dependence of noff")]
        public Parameter BSIM3pnoff { get; } = new Parameter();
        [SpiceName("pvoffcv"), SpiceInfo("Cross-term dependence of voffcv")]
        public Parameter BSIM3pvoffcv { get; } = new Parameter();
        [SpiceName("tnom"), SpiceInfo("Parameter measurement temperature")]
        public Parameter BSIM3tnom { get; } = new Parameter();
        [SpiceName("cgso"), SpiceInfo("Gate-source overlap capacitance per width")]
        public Parameter BSIM3cgso { get; } = new Parameter();
        [SpiceName("cgdo"), SpiceInfo("Gate-drain overlap capacitance per width")]
        public Parameter BSIM3cgdo { get; } = new Parameter();
        [SpiceName("cgbo"), SpiceInfo("Gate-bulk overlap capacitance per length")]
        public Parameter BSIM3cgbo { get; } = new Parameter();
        [SpiceName("xpart"), SpiceInfo("Channel charge partitioning")]
        public Parameter BSIM3xpart { get; } = new Parameter();
        [SpiceName("rsh"), SpiceInfo("Source-drain sheet resistance")]
        public Parameter BSIM3sheetResistance { get; } = new Parameter();
        [SpiceName("js"), SpiceInfo("Source/drain junction reverse saturation current density")]
        public Parameter BSIM3jctSatCurDensity { get; } = new Parameter(1.0E-4);
        [SpiceName("jsw"), SpiceInfo("Sidewall junction reverse saturation current density")]
        public Parameter BSIM3jctSidewallSatCurDensity { get; } = new Parameter();
        [SpiceName("pb"), SpiceInfo("Source/drain junction built-in potential")]
        public Parameter BSIM3bulkJctPotential { get; } = new Parameter(1.0);
        [SpiceName("mj"), SpiceInfo("Source/drain bottom junction capacitance grading coefficient")]
        public Parameter BSIM3bulkJctBotGradingCoeff { get; } = new Parameter(0.5);
        [SpiceName("pbsw"), SpiceInfo("Source/drain sidewall junction capacitance built in potential")]
        public Parameter BSIM3sidewallJctPotential { get; } = new Parameter(1.0);
        [SpiceName("mjsw"), SpiceInfo("Source/drain sidewall junction capacitance grading coefficient")]
        public Parameter BSIM3bulkJctSideGradingCoeff { get; } = new Parameter(0.33);
        [SpiceName("cj"), SpiceInfo("Source/drain bottom junction capacitance per unit area")]
        public Parameter BSIM3unitAreaJctCap { get; } = new Parameter(5.0E-4);
        [SpiceName("cjsw"), SpiceInfo("Source/drain sidewall junction capacitance per unit periphery")]
        public Parameter BSIM3unitLengthSidewallJctCap { get; } = new Parameter(5.0E-10);
        [SpiceName("nj"), SpiceInfo("Source/drain junction emission coefficient")]
        public Parameter BSIM3jctEmissionCoeff { get; } = new Parameter(1.0);
        [SpiceName("pbswg"), SpiceInfo("Source/drain (gate side) sidewall junction capacitance built in potential")]
        public Parameter BSIM3GatesidewallJctPotential { get; } = new Parameter();
        [SpiceName("mjswg"), SpiceInfo("Source/drain (gate side) sidewall junction capacitance grading coefficient")]
        public Parameter BSIM3bulkJctGateSideGradingCoeff { get; } = new Parameter();
        [SpiceName("cjswg"), SpiceInfo("Source/drain (gate side) sidewall junction capacitance per unit width")]
        public Parameter BSIM3unitLengthGateSidewallJctCap { get; } = new Parameter();
        [SpiceName("xti"), SpiceInfo("Junction current temperature exponent")]
        public Parameter BSIM3jctTempExponent { get; } = new Parameter(3.0);
        [SpiceName("lint"), SpiceInfo("Length reduction parameter")]
        public Parameter BSIM3Lint { get; } = new Parameter();
        [SpiceName("ll"), SpiceInfo("Length reduction parameter")]
        public Parameter BSIM3Ll { get; } = new Parameter();
        [SpiceName("llc"), SpiceInfo("Length reduction parameter for CV")]
        public Parameter BSIM3Llc { get; } = new Parameter();
        [SpiceName("lln"), SpiceInfo("Length reduction parameter")]
        public Parameter BSIM3Lln { get; } = new Parameter(1.0);
        [SpiceName("lw"), SpiceInfo("Length reduction parameter")]
        public Parameter BSIM3Lw { get; } = new Parameter();
        [SpiceName("lwc"), SpiceInfo("Length reduction parameter for CV")]
        public Parameter BSIM3Lwc { get; } = new Parameter();
        [SpiceName("lwn"), SpiceInfo("Length reduction parameter")]
        public Parameter BSIM3Lwn { get; } = new Parameter(1.0);
        [SpiceName("lwl"), SpiceInfo("Length reduction parameter")]
        public Parameter BSIM3Lwl { get; } = new Parameter();
        [SpiceName("lwlc"), SpiceInfo("Length reduction parameter for CV")]
        public Parameter BSIM3Lwlc { get; } = new Parameter();
        [SpiceName("lmin"), SpiceInfo("Minimum length for the model")]
        public Parameter BSIM3Lmin { get; } = new Parameter();
        [SpiceName("lmax"), SpiceInfo("Maximum length for the model")]
        public Parameter BSIM3Lmax { get; } = new Parameter(1.0);
        [SpiceName("wint"), SpiceInfo("Width reduction parameter")]
        public Parameter BSIM3Wint { get; } = new Parameter();
        [SpiceName("wl"), SpiceInfo("Width reduction parameter")]
        public Parameter BSIM3Wl { get; } = new Parameter();
        [SpiceName("wlc"), SpiceInfo("Width reduction parameter for CV")]
        public Parameter BSIM3Wlc { get; } = new Parameter();
        [SpiceName("wln"), SpiceInfo("Width reduction parameter")]
        public Parameter BSIM3Wln { get; } = new Parameter(1.0);
        [SpiceName("ww"), SpiceInfo("Width reduction parameter")]
        public Parameter BSIM3Ww { get; } = new Parameter();
        [SpiceName("wwc"), SpiceInfo("Width reduction parameter for CV")]
        public Parameter BSIM3Wwc { get; } = new Parameter();
        [SpiceName("wwn"), SpiceInfo("Width reduction parameter")]
        public Parameter BSIM3Wwn { get; } = new Parameter(1.0);
        [SpiceName("wwl"), SpiceInfo("Width reduction parameter")]
        public Parameter BSIM3Wwl { get; } = new Parameter();
        [SpiceName("wwlc"), SpiceInfo("Width reduction parameter for CV")]
        public Parameter BSIM3Wwlc { get; } = new Parameter();
        [SpiceName("wmin"), SpiceInfo("Minimum width for the model")]
        public Parameter BSIM3Wmin { get; } = new Parameter();
        [SpiceName("wmax"), SpiceInfo("Maximum width for the model")]
        public Parameter BSIM3Wmax { get; } = new Parameter(1.0);
        [SpiceName("noia"), SpiceInfo("Flicker noise parameter")]
        public Parameter BSIM3oxideTrapDensityA { get; } = new Parameter();
        [SpiceName("noib"), SpiceInfo("Flicker noise parameter")]
        public Parameter BSIM3oxideTrapDensityB { get; } = new Parameter();
        [SpiceName("noic"), SpiceInfo("Flicker noise parameter")]
        public Parameter BSIM3oxideTrapDensityC { get; } = new Parameter();
        [SpiceName("em"), SpiceInfo("Flicker noise parameter")]
        public Parameter BSIM3em { get; } = new Parameter(4.1e7);
        [SpiceName("ef"), SpiceInfo("Flicker noise frequency exponent")]
        public Parameter BSIM3ef { get; } = new Parameter(1.0);
        [SpiceName("af"), SpiceInfo("Flicker noise exponent")]
        public Parameter BSIM3af { get; } = new Parameter(1.0);
        [SpiceName("kf"), SpiceInfo("Flicker noise coefficient")]
        public Parameter BSIM3kf { get; } = new Parameter();

        /// <summary>
        /// Methods
        /// </summary>
        [SpiceName("nmos"), SpiceInfo("Flag to indicate NMOS")]
        public void SetNMOS(bool value)
        {
            if (value)
                BSIM3type = 1.0;
        }
        [SpiceName("pmos"), SpiceInfo("Flag to indicate PMOS")]
        public void SetPMOS(bool value)
        {
            if (value)
                BSIM3type = -1.0;
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
        public double BSIM3type { get; private set; } = 1.0;
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

        private const double NMOS = 1.0;
        private const double PMOS = 1.0;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the device</param>
        public BSIM3v24Model(CircuitIdentifier name) : base(name)
        {
        }

        /// <summary>
        /// Setup the device
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Setup(Circuit ckt)
        {
            Sizes.Clear();

            /* Default value Processing for BSIM3v30 MOSFET Models */
            BSIM3cox = 3.453133e-11 / BSIM3tox;
            if (!BSIM3toxm.Given)
                BSIM3toxm.Value = BSIM3tox;

            if (!BSIM3dsub.Given)
                BSIM3dsub.Value = BSIM3drout;
            if (!BSIM3vth0.Given)
                BSIM3vth0.Value = (BSIM3type == NMOS) ? 0.7 : -0.7;
            if (!BSIM3uc.Given)
                BSIM3uc.Value = (BSIM3mobMod.Value == 3) ? -0.0465 : -0.0465e-9;
            if (!BSIM3uc1.Given)
                BSIM3uc1.Value = (BSIM3mobMod.Value == 3) ? -0.056 : -0.056e-9;
            if (!BSIM3u0.Given)
                BSIM3u0.Value = (BSIM3type == NMOS) ? 0.067 : 0.025;

            if (!BSIM3tnom.Given)
                BSIM3tnom.Value = ckt.State.NominalTemperature;
            else
                BSIM3tnom.Value = BSIM3tnom + 273.15;
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
                if (BSIM3type == NMOS)
                    BSIM3oxideTrapDensityA.Value = 1e20;
                else
                    BSIM3oxideTrapDensityA.Value = 9.9e18;
            }
            if (!BSIM3oxideTrapDensityB.Given)
            {
                if (BSIM3type == NMOS)
                    BSIM3oxideTrapDensityB.Value = 5e4;
                else
                    BSIM3oxideTrapDensityB.Value = 2.4e3;
            }
            if (!BSIM3oxideTrapDensityC.Given)
            {
                if (BSIM3type == NMOS)
                    BSIM3oxideTrapDensityC.Value = -1.4e-12;
                else
                    BSIM3oxideTrapDensityC.Value = 1.4e-12;

            }
            /* V / m */
            /* loop through all the instances of the model */
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
                CircuitWarning.Warning(this, "Given pb is less than 0.1. Pb is set to 0.1");
            }
            if (BSIM3sidewallJctPotential < 0.1)
            {
                BSIM3sidewallJctPotential.Value = 0.1;
                CircuitWarning.Warning(this, "Given pbsw is less than 0.1. Pbsw is set to 0.1");
            }
            if (BSIM3GatesidewallJctPotential < 0.1)
            {
                BSIM3GatesidewallJctPotential.Value = 0.1;
                CircuitWarning.Warning(this, "Given pbswg is less than 0.1. Pbswg is set to 0.1");
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
                CircuitWarning.Warning(this, "Temperature effect has caused cj to be negative. Cj is clamped to zero");
            }
            T0 = BSIM3tcjsw * delTemp;
            if (T0 >= -1.0)
            {
                BSIM3unitLengthSidewallTempJctCap = BSIM3unitLengthSidewallJctCap * (1.0 + T0);
            }
            else if (BSIM3unitLengthSidewallJctCap > 0.0)
            {
                BSIM3unitLengthSidewallTempJctCap = 0.0;
                CircuitWarning.Warning(this, "Temperature effect has caused cjsw to be negative. Cjsw is clamped to zero");
            }
            T0 = BSIM3tcjswg * delTemp;
            if (T0 >= -1.0)
            {
                BSIM3unitLengthGateSidewallTempJctCap = BSIM3unitLengthGateSidewallJctCap * (1.0 + T0);
            }
            else if (BSIM3unitLengthGateSidewallJctCap > 0.0)
            {
                BSIM3unitLengthGateSidewallTempJctCap = 0.0;
                CircuitWarning.Warning(this, "Temperature effect has caused cjswg to be negative. Cjswg is clamped to zero");
            }

            BSIM3PhiB = BSIM3bulkJctPotential - BSIM3tpb * delTemp;
            if (BSIM3PhiB < 0.01)
            {
                BSIM3PhiB = 0.01;
                CircuitWarning.Warning(this, "Temperature effect has caused pb to be less than 0.01. Pb is clamped to 0.01");
            }
            BSIM3PhiBSW = BSIM3sidewallJctPotential - BSIM3tpbsw * delTemp;
            if (BSIM3PhiBSW <= 0.01)
            {
                BSIM3PhiBSW = 0.01;
                CircuitWarning.Warning(this, "Temperature effect has caused pbsw to be less than 0.01. Pbsw is clamped to 0.01");
            }
            BSIM3PhiBSWG = BSIM3GatesidewallJctPotential - BSIM3tpbswg * delTemp;
            if (BSIM3PhiBSWG <= 0.01)
            {
                BSIM3PhiBSWG = 0.01;
                CircuitWarning.Warning(this, "Temperature effect has caused pbswg to be less than 0.01. Pbswg is clamped to 0.01");
            }
        }
    }
}
