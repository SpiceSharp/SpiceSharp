using System;
using SpiceSharp.Behaviors;
using SpiceSharp.Diagnostics;
using SpiceSharp.Components.Transistors;
using static SpiceSharp.Components.Transistors.BSIM4v80Helpers;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// Temperature behaviour for a <see cref="BSIM4v80"/>
    /// </summary>
    public class BSIM4v80TemperatureBehavior : CircuitObjectBehaviorTemperature
    {
        /// <summary>
        /// Execute behaviour
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Execute(Circuit ckt)
        {
            var bsim4 = ComponentTyped<BSIM4v80>();
            var model = bsim4.Model as BSIM4v80Model;
            double Ldrn, Wdrn, Lnew = 0, Wnew, T0, T1, tmp1, tmp2, T2, T3, Inv_L, Inv_W, Inv_LW,
                PowWeffWr, T10, T4, T5, tmp, T8, T9, wlod, W_tmp, Inv_saref, Inv_sbref, Theta0, tmp3, n0, Inv_sa,
                Inv_sb, kvsat, i, Inv_ODeff, rho, OD_offset, dvth0_lod, dk2_lod, deta0_lod, sceff, lnl, lnw,
                lnnf, bodymode, rbsby, rbsbx, rbdbx, rbdby, rbpbx, rbpby, DMCGeff, DMCIeff, DMDGeff, Nvtms, SourceSatCurrent, Nvtmd,
                DrainSatCurrent, T7, T11, Vtmeot, vbieot, phieot, vddeot, T6, Vgs_eff, lt1, ltw, Delt_vth, TempRatioeot, Vth_NarrowW,
                Lpe_Vb, Vth, n, Vgsteff, vtfbphi2eot, niter, toxpf, toxpi, Tcen;
            double dumPs, dumPd, dumAs, dumAd;

            /* stress effect */
            Ldrn = bsim4.BSIM4l;
            Wdrn = bsim4.BSIM4w / bsim4.BSIM4nf;
            Tuple<double, double, double> size = new Tuple<double, double, double>(bsim4.BSIM4w, bsim4.BSIM4l, bsim4.BSIM4nf);
            if (model.Sizes.ContainsKey(size))
                bsim4.pParam = model.Sizes[size];
            else
            {
                bsim4.pParam = new BSIM4SizeDependParam();
                model.Sizes.Add(size, bsim4.pParam);

                bsim4.pParam.NFinger = bsim4.BSIM4nf;
                Lnew = bsim4.BSIM4l + model.BSIM4xl;
                Wnew = bsim4.BSIM4w / bsim4.BSIM4nf + model.BSIM4xw;

                T0 = Math.Pow(Lnew, model.BSIM4Lln);
                T1 = Math.Pow(Wnew, model.BSIM4Lwn);
                tmp1 = model.BSIM4Ll / T0 + model.BSIM4Lw / T1 + model.BSIM4Lwl / (T0 * T1);
                bsim4.pParam.BSIM4dl = model.BSIM4Lint + tmp1;
                tmp2 = model.BSIM4Llc / T0 + model.BSIM4Lwc / T1 + model.BSIM4Lwlc / (T0 * T1);
                bsim4.pParam.BSIM4dlc = model.BSIM4dlc + tmp2;

                T2 = Math.Pow(Lnew, model.BSIM4Wln);
                T3 = Math.Pow(Wnew, model.BSIM4Wwn);
                tmp1 = model.BSIM4Wl / T2 + model.BSIM4Ww / T3 + model.BSIM4Wwl / (T2 * T3);
                bsim4.pParam.BSIM4dw = model.BSIM4Wint + tmp1;
                tmp2 = model.BSIM4Wlc / T2 + model.BSIM4Wwc / T3 + model.BSIM4Wwlc / (T2 * T3);
                bsim4.pParam.BSIM4dwc = model.BSIM4dwc + tmp2;
                bsim4.pParam.BSIM4dwj = model.BSIM4dwj + tmp2;

                bsim4.pParam.BSIM4leff = Lnew - 2.0 * bsim4.pParam.BSIM4dl;
                if (bsim4.pParam.BSIM4leff <= 0.0)
                    throw new CircuitException($"bsim4.BSIM4v80: mosfet {bsim4.Name}, model {model.Name}: Effective channel length <= 0");
                bsim4.pParam.BSIM4weff = Wnew - 2.0 * bsim4.pParam.BSIM4dw;
                if (bsim4.pParam.BSIM4weff <= 0.0)
                    throw new CircuitException($"bsim4.BSIM4v80: mosfet {bsim4.Name}, model {model.Name}: Effective channel width <= 0");
                bsim4.pParam.BSIM4leffCV = Lnew - 2.0 * bsim4.pParam.BSIM4dlc;
                if (bsim4.pParam.BSIM4leffCV <= 0.0)
                    throw new CircuitException($"bsim4.BSIM4v80: mosfet {bsim4.Name}, model {model.Name}: Effective channel length for C-V <= 0");
                bsim4.pParam.BSIM4weffCV = Wnew - 2.0 * bsim4.pParam.BSIM4dwc;
                if (bsim4.pParam.BSIM4weffCV <= 0.0)
                    throw new CircuitException($"bsim4.BSIM4v80: mosfet {bsim4.Name}, model {model.Name}: Effective channel width for C-V <= 0");
                bsim4.pParam.BSIM4weffCJ = Wnew - 2.0 * bsim4.pParam.BSIM4dwj;
                if (bsim4.pParam.BSIM4weffCJ <= 0.0)
                    throw new CircuitException($"bsim4.BSIM4v80: mosfet {bsim4.Name}, model {model.Name}: Effective channel width for S/D junctions <= 0");

                if (model.BSIM4binUnit.Value == 1)
                {
                    Inv_L = 1.0e-6 / bsim4.pParam.BSIM4leff;
                    Inv_W = 1.0e-6 / bsim4.pParam.BSIM4weff;
                    Inv_LW = 1.0e-12 / (bsim4.pParam.BSIM4leff * bsim4.pParam.BSIM4weff);
                }
                else
                {
                    Inv_L = 1.0 / bsim4.pParam.BSIM4leff;
                    Inv_W = 1.0 / bsim4.pParam.BSIM4weff;
                    Inv_LW = 1.0 / (bsim4.pParam.BSIM4leff * bsim4.pParam.BSIM4weff);
                }
                bsim4.pParam.BSIM4cdsc = model.BSIM4cdsc + model.BSIM4lcdsc * Inv_L + model.BSIM4wcdsc * Inv_W + model.BSIM4pcdsc * Inv_LW;
                bsim4.pParam.BSIM4cdscb = model.BSIM4cdscb + model.BSIM4lcdscb * Inv_L + model.BSIM4wcdscb * Inv_W + model.BSIM4pcdscb * Inv_LW;

                bsim4.pParam.BSIM4cdscd = model.BSIM4cdscd + model.BSIM4lcdscd * Inv_L + model.BSIM4wcdscd * Inv_W + model.BSIM4pcdscd * Inv_LW;

                bsim4.pParam.BSIM4cit = model.BSIM4cit + model.BSIM4lcit * Inv_L + model.BSIM4wcit * Inv_W + model.BSIM4pcit * Inv_LW;
                bsim4.pParam.BSIM4nfactor = model.BSIM4nfactor + model.BSIM4lnfactor * Inv_L + model.BSIM4wnfactor * Inv_W + model.BSIM4pnfactor *
                  Inv_LW;
                bsim4.pParam.BSIM4tnfactor = model.BSIM4tnfactor /* v4.7 */  + model.BSIM4ltnfactor * Inv_L + model.BSIM4wtnfactor * Inv_W +
                                    model.BSIM4ptnfactor * Inv_LW;
                bsim4.pParam.BSIM4xj = model.BSIM4xj + model.BSIM4lxj * Inv_L + model.BSIM4wxj * Inv_W + model.BSIM4pxj * Inv_LW;
                bsim4.pParam.BSIM4vsat = model.BSIM4vsat + model.BSIM4lvsat * Inv_L + model.BSIM4wvsat * Inv_W + model.BSIM4pvsat * Inv_LW;
                bsim4.pParam.BSIM4at = model.BSIM4at + model.BSIM4lat * Inv_L + model.BSIM4wat * Inv_W + model.BSIM4pat * Inv_LW;
                bsim4.pParam.BSIM4a0 = model.BSIM4a0 + model.BSIM4la0 * Inv_L + model.BSIM4wa0 * Inv_W + model.BSIM4pa0 * Inv_LW;

                bsim4.pParam.BSIM4ags = model.BSIM4ags + model.BSIM4lags * Inv_L + model.BSIM4wags * Inv_W + model.BSIM4pags * Inv_LW;

                bsim4.pParam.BSIM4a1 = model.BSIM4a1 + model.BSIM4la1 * Inv_L + model.BSIM4wa1 * Inv_W + model.BSIM4pa1 * Inv_LW;
                bsim4.pParam.BSIM4a2 = model.BSIM4a2 + model.BSIM4la2 * Inv_L + model.BSIM4wa2 * Inv_W + model.BSIM4pa2 * Inv_LW;
                bsim4.pParam.BSIM4keta = model.BSIM4keta + model.BSIM4lketa * Inv_L + model.BSIM4wketa * Inv_W + model.BSIM4pketa * Inv_LW;
                bsim4.pParam.BSIM4nsub = model.BSIM4nsub + model.BSIM4lnsub * Inv_L + model.BSIM4wnsub * Inv_W + model.BSIM4pnsub * Inv_LW;
                bsim4.pParam.BSIM4ndep = model.BSIM4ndep + model.BSIM4lndep * Inv_L + model.BSIM4wndep * Inv_W + model.BSIM4pndep * Inv_LW;
                bsim4.pParam.BSIM4nsd = model.BSIM4nsd + model.BSIM4lnsd * Inv_L + model.BSIM4wnsd * Inv_W + model.BSIM4pnsd * Inv_LW;
                bsim4.pParam.BSIM4phin = model.BSIM4phin + model.BSIM4lphin * Inv_L + model.BSIM4wphin * Inv_W + model.BSIM4pphin * Inv_LW;
                bsim4.pParam.BSIM4ngate = model.BSIM4ngate + model.BSIM4lngate * Inv_L + model.BSIM4wngate * Inv_W + model.BSIM4pngate * Inv_LW;
                bsim4.pParam.BSIM4gamma1 = model.BSIM4gamma1 + model.BSIM4lgamma1 * Inv_L + model.BSIM4wgamma1 * Inv_W + model.BSIM4pgamma1 * Inv_LW;
                bsim4.pParam.BSIM4gamma2 = model.BSIM4gamma2 + model.BSIM4lgamma2 * Inv_L + model.BSIM4wgamma2 * Inv_W + model.BSIM4pgamma2 * Inv_LW;
                bsim4.pParam.BSIM4vbx = model.BSIM4vbx + model.BSIM4lvbx * Inv_L + model.BSIM4wvbx * Inv_W + model.BSIM4pvbx * Inv_LW;
                bsim4.pParam.BSIM4vbm = model.BSIM4vbm + model.BSIM4lvbm * Inv_L + model.BSIM4wvbm * Inv_W + model.BSIM4pvbm * Inv_LW;
                bsim4.pParam.BSIM4xt = model.BSIM4xt + model.BSIM4lxt * Inv_L + model.BSIM4wxt * Inv_W + model.BSIM4pxt * Inv_LW;
                bsim4.pParam.BSIM4vfb = model.BSIM4vfb + model.BSIM4lvfb * Inv_L + model.BSIM4wvfb * Inv_W + model.BSIM4pvfb * Inv_LW;
                bsim4.pParam.BSIM4k1 = model.BSIM4k1 + model.BSIM4lk1 * Inv_L + model.BSIM4wk1 * Inv_W + model.BSIM4pk1 * Inv_LW;
                bsim4.pParam.BSIM4kt1 = model.BSIM4kt1 + model.BSIM4lkt1 * Inv_L + model.BSIM4wkt1 * Inv_W + model.BSIM4pkt1 * Inv_LW;
                bsim4.pParam.BSIM4kt1l = model.BSIM4kt1l + model.BSIM4lkt1l * Inv_L + model.BSIM4wkt1l * Inv_W + model.BSIM4pkt1l * Inv_LW;
                bsim4.pParam.BSIM4k2 = model.BSIM4k2 + model.BSIM4lk2 * Inv_L + model.BSIM4wk2 * Inv_W + model.BSIM4pk2 * Inv_LW;
                bsim4.pParam.BSIM4kt2 = model.BSIM4kt2 + model.BSIM4lkt2 * Inv_L + model.BSIM4wkt2 * Inv_W + model.BSIM4pkt2 * Inv_LW;
                bsim4.pParam.BSIM4k3 = model.BSIM4k3 + model.BSIM4lk3 * Inv_L + model.BSIM4wk3 * Inv_W + model.BSIM4pk3 * Inv_LW;
                bsim4.pParam.BSIM4k3b = model.BSIM4k3b + model.BSIM4lk3b * Inv_L + model.BSIM4wk3b * Inv_W + model.BSIM4pk3b * Inv_LW;
                bsim4.pParam.BSIM4w0 = model.BSIM4w0 + model.BSIM4lw0 * Inv_L + model.BSIM4ww0 * Inv_W + model.BSIM4pw0 * Inv_LW;
                bsim4.pParam.BSIM4lpe0 = model.BSIM4lpe0 + model.BSIM4llpe0 * Inv_L + model.BSIM4wlpe0 * Inv_W + model.BSIM4plpe0 * Inv_LW;
                bsim4.pParam.BSIM4lpeb = model.BSIM4lpeb + model.BSIM4llpeb * Inv_L + model.BSIM4wlpeb * Inv_W + model.BSIM4plpeb * Inv_LW;
                bsim4.pParam.BSIM4dvtp0 = model.BSIM4dvtp0 + model.BSIM4ldvtp0 * Inv_L + model.BSIM4wdvtp0 * Inv_W + model.BSIM4pdvtp0 * Inv_LW;
                bsim4.pParam.BSIM4dvtp1 = model.BSIM4dvtp1 + model.BSIM4ldvtp1 * Inv_L + model.BSIM4wdvtp1 * Inv_W + model.BSIM4pdvtp1 * Inv_LW;
                bsim4.pParam.BSIM4dvtp2 = model.BSIM4dvtp2 /* v4.7 */  + model.BSIM4ldvtp2 * Inv_L + model.BSIM4wdvtp2 * Inv_W + model.BSIM4pdvtp2 *
                  Inv_LW;
                bsim4.pParam.BSIM4dvtp3 = model.BSIM4dvtp3 /* v4.7 */  + model.BSIM4ldvtp3 * Inv_L + model.BSIM4wdvtp3 * Inv_W + model.BSIM4pdvtp3 *
                  Inv_LW;
                bsim4.pParam.BSIM4dvtp4 = model.BSIM4dvtp4 /* v4.7 */  + model.BSIM4ldvtp4 * Inv_L + model.BSIM4wdvtp4 * Inv_W + model.BSIM4pdvtp4 *
                  Inv_LW;
                bsim4.pParam.BSIM4dvtp5 = model.BSIM4dvtp5 /* v4.7 */  + model.BSIM4ldvtp5 * Inv_L + model.BSIM4wdvtp5 * Inv_W + model.BSIM4pdvtp5 *
                  Inv_LW;
                bsim4.pParam.BSIM4dvt0 = model.BSIM4dvt0 + model.BSIM4ldvt0 * Inv_L + model.BSIM4wdvt0 * Inv_W + model.BSIM4pdvt0 * Inv_LW;
                bsim4.pParam.BSIM4dvt1 = model.BSIM4dvt1 + model.BSIM4ldvt1 * Inv_L + model.BSIM4wdvt1 * Inv_W + model.BSIM4pdvt1 * Inv_LW;
                bsim4.pParam.BSIM4dvt2 = model.BSIM4dvt2 + model.BSIM4ldvt2 * Inv_L + model.BSIM4wdvt2 * Inv_W + model.BSIM4pdvt2 * Inv_LW;
                bsim4.pParam.BSIM4dvt0w = model.BSIM4dvt0w + model.BSIM4ldvt0w * Inv_L + model.BSIM4wdvt0w * Inv_W + model.BSIM4pdvt0w * Inv_LW;
                bsim4.pParam.BSIM4dvt1w = model.BSIM4dvt1w + model.BSIM4ldvt1w * Inv_L + model.BSIM4wdvt1w * Inv_W + model.BSIM4pdvt1w * Inv_LW;
                bsim4.pParam.BSIM4dvt2w = model.BSIM4dvt2w + model.BSIM4ldvt2w * Inv_L + model.BSIM4wdvt2w * Inv_W + model.BSIM4pdvt2w * Inv_LW;
                bsim4.pParam.BSIM4drout = model.BSIM4drout + model.BSIM4ldrout * Inv_L + model.BSIM4wdrout * Inv_W + model.BSIM4pdrout * Inv_LW;
                bsim4.pParam.BSIM4dsub = model.BSIM4dsub + model.BSIM4ldsub * Inv_L + model.BSIM4wdsub * Inv_W + model.BSIM4pdsub * Inv_LW;
                bsim4.pParam.BSIM4vth0 = model.BSIM4vth0 + model.BSIM4lvth0 * Inv_L + model.BSIM4wvth0 * Inv_W + model.BSIM4pvth0 * Inv_LW;
                bsim4.pParam.BSIM4ua = model.BSIM4ua + model.BSIM4lua * Inv_L + model.BSIM4wua * Inv_W + model.BSIM4pua * Inv_LW;
                bsim4.pParam.BSIM4ua1 = model.BSIM4ua1 + model.BSIM4lua1 * Inv_L + model.BSIM4wua1 * Inv_W + model.BSIM4pua1 * Inv_LW;
                bsim4.pParam.BSIM4ub = model.BSIM4ub + model.BSIM4lub * Inv_L + model.BSIM4wub * Inv_W + model.BSIM4pub * Inv_LW;
                bsim4.pParam.BSIM4ub1 = model.BSIM4ub1 + model.BSIM4lub1 * Inv_L + model.BSIM4wub1 * Inv_W + model.BSIM4pub1 * Inv_LW;
                bsim4.pParam.BSIM4uc = model.BSIM4uc + model.BSIM4luc * Inv_L + model.BSIM4wuc * Inv_W + model.BSIM4puc * Inv_LW;
                bsim4.pParam.BSIM4uc1 = model.BSIM4uc1 + model.BSIM4luc1 * Inv_L + model.BSIM4wuc1 * Inv_W + model.BSIM4puc1 * Inv_LW;
                bsim4.pParam.BSIM4ud = model.BSIM4ud + model.BSIM4lud * Inv_L + model.BSIM4wud * Inv_W + model.BSIM4pud * Inv_LW;
                bsim4.pParam.BSIM4ud1 = model.BSIM4ud1 + model.BSIM4lud1 * Inv_L + model.BSIM4wud1 * Inv_W + model.BSIM4pud1 * Inv_LW;
                bsim4.pParam.BSIM4up = model.BSIM4up + model.BSIM4lup * Inv_L + model.BSIM4wup * Inv_W + model.BSIM4pup * Inv_LW;
                bsim4.pParam.BSIM4lp = model.BSIM4lp + model.BSIM4llp * Inv_L + model.BSIM4wlp * Inv_W + model.BSIM4plp * Inv_LW;
                bsim4.pParam.BSIM4eu = model.BSIM4eu + model.BSIM4leu * Inv_L + model.BSIM4weu * Inv_W + model.BSIM4peu * Inv_LW;
                bsim4.pParam.BSIM4u0 = model.BSIM4u0 + model.BSIM4lu0 * Inv_L + model.BSIM4wu0 * Inv_W + model.BSIM4pu0 * Inv_LW;
                bsim4.pParam.BSIM4ute = model.BSIM4ute + model.BSIM4lute * Inv_L + model.BSIM4wute * Inv_W + model.BSIM4pute * Inv_LW;
                /* high k mobility */
                bsim4.pParam.BSIM4ucs = model.BSIM4ucs + model.BSIM4lucs * Inv_L + model.BSIM4wucs * Inv_W + model.BSIM4pucs * Inv_LW;
                bsim4.pParam.BSIM4ucste = model.BSIM4ucste + model.BSIM4lucste * Inv_L + model.BSIM4wucste * Inv_W + model.BSIM4pucste * Inv_LW;

                bsim4.pParam.BSIM4voff = model.BSIM4voff + model.BSIM4lvoff * Inv_L + model.BSIM4wvoff * Inv_W + model.BSIM4pvoff * Inv_LW;
                bsim4.pParam.BSIM4tvoff = model.BSIM4tvoff + model.BSIM4ltvoff * Inv_L + model.BSIM4wtvoff * Inv_W + model.BSIM4ptvoff * Inv_LW;
                bsim4.pParam.BSIM4minv = model.BSIM4minv + model.BSIM4lminv * Inv_L + model.BSIM4wminv * Inv_W + model.BSIM4pminv * Inv_LW;
                bsim4.pParam.BSIM4minvcv = model.BSIM4minvcv + model.BSIM4lminvcv * Inv_L + model.BSIM4wminvcv * Inv_W + model.BSIM4pminvcv * Inv_LW;
                bsim4.pParam.BSIM4fprout = model.BSIM4fprout + model.BSIM4lfprout * Inv_L + model.BSIM4wfprout * Inv_W + model.BSIM4pfprout * Inv_LW;
                bsim4.pParam.BSIM4pdits = model.BSIM4pdits + model.BSIM4lpdits * Inv_L + model.BSIM4wpdits * Inv_W + model.BSIM4ppdits * Inv_LW;
                bsim4.pParam.BSIM4pditsd = model.BSIM4pditsd + model.BSIM4lpditsd * Inv_L + model.BSIM4wpditsd * Inv_W + model.BSIM4ppditsd * Inv_LW;
                bsim4.pParam.BSIM4delta = model.BSIM4delta + model.BSIM4ldelta * Inv_L + model.BSIM4wdelta * Inv_W + model.BSIM4pdelta * Inv_LW;
                bsim4.pParam.BSIM4rdsw = model.BSIM4rdsw + model.BSIM4lrdsw * Inv_L + model.BSIM4wrdsw * Inv_W + model.BSIM4prdsw * Inv_LW;
                bsim4.pParam.BSIM4rdw = model.BSIM4rdw + model.BSIM4lrdw * Inv_L + model.BSIM4wrdw * Inv_W + model.BSIM4prdw * Inv_LW;
                bsim4.pParam.BSIM4rsw = model.BSIM4rsw + model.BSIM4lrsw * Inv_L + model.BSIM4wrsw * Inv_W + model.BSIM4prsw * Inv_LW;
                bsim4.pParam.BSIM4prwg = model.BSIM4prwg + model.BSIM4lprwg * Inv_L + model.BSIM4wprwg * Inv_W + model.BSIM4pprwg * Inv_LW;
                bsim4.pParam.BSIM4prwb = model.BSIM4prwb + model.BSIM4lprwb * Inv_L + model.BSIM4wprwb * Inv_W + model.BSIM4pprwb * Inv_LW;
                bsim4.pParam.BSIM4prt = model.BSIM4prt + model.BSIM4lprt * Inv_L + model.BSIM4wprt * Inv_W + model.BSIM4pprt * Inv_LW;
                bsim4.pParam.BSIM4eta0 = model.BSIM4eta0 + model.BSIM4leta0 * Inv_L + model.BSIM4weta0 * Inv_W + model.BSIM4peta0 * Inv_LW;
                bsim4.pParam.BSIM4teta0 = model.BSIM4teta0 /* v4.7 */  + model.BSIM4lteta0 * Inv_L + model.BSIM4wteta0 * Inv_W + model.BSIM4pteta0 *
                  Inv_LW;
                bsim4.pParam.BSIM4tvoffcv = model.BSIM4tvoffcv /* v4.8.0 */  + model.BSIM4ltvoffcv * Inv_L + model.BSIM4wtvoffcv * Inv_W +
                                    model.BSIM4ptvoffcv * Inv_LW;
                bsim4.pParam.BSIM4etab = model.BSIM4etab + model.BSIM4letab * Inv_L + model.BSIM4wetab * Inv_W + model.BSIM4petab * Inv_LW;
                bsim4.pParam.BSIM4pclm = model.BSIM4pclm + model.BSIM4lpclm * Inv_L + model.BSIM4wpclm * Inv_W + model.BSIM4ppclm * Inv_LW;
                bsim4.pParam.BSIM4pdibl1 = model.BSIM4pdibl1 + model.BSIM4lpdibl1 * Inv_L + model.BSIM4wpdibl1 * Inv_W + model.BSIM4ppdibl1 * Inv_LW;
                bsim4.pParam.BSIM4pdibl2 = model.BSIM4pdibl2 + model.BSIM4lpdibl2 * Inv_L + model.BSIM4wpdibl2 * Inv_W + model.BSIM4ppdibl2 * Inv_LW;
                bsim4.pParam.BSIM4pdiblb = model.BSIM4pdiblb + model.BSIM4lpdiblb * Inv_L + model.BSIM4wpdiblb * Inv_W + model.BSIM4ppdiblb * Inv_LW;
                bsim4.pParam.BSIM4pscbe1 = model.BSIM4pscbe1 + model.BSIM4lpscbe1 * Inv_L + model.BSIM4wpscbe1 * Inv_W + model.BSIM4ppscbe1 * Inv_LW;
                bsim4.pParam.BSIM4pscbe2 = model.BSIM4pscbe2 + model.BSIM4lpscbe2 * Inv_L + model.BSIM4wpscbe2 * Inv_W + model.BSIM4ppscbe2 * Inv_LW;
                bsim4.pParam.BSIM4pvag = model.BSIM4pvag + model.BSIM4lpvag * Inv_L + model.BSIM4wpvag * Inv_W + model.BSIM4ppvag * Inv_LW;
                bsim4.pParam.BSIM4wr = model.BSIM4wr + model.BSIM4lwr * Inv_L + model.BSIM4wwr * Inv_W + model.BSIM4pwr * Inv_LW;
                bsim4.pParam.BSIM4dwg = model.BSIM4dwg + model.BSIM4ldwg * Inv_L + model.BSIM4wdwg * Inv_W + model.BSIM4pdwg * Inv_LW;
                bsim4.pParam.BSIM4dwb = model.BSIM4dwb + model.BSIM4ldwb * Inv_L + model.BSIM4wdwb * Inv_W + model.BSIM4pdwb * Inv_LW;
                bsim4.pParam.BSIM4b0 = model.BSIM4b0 + model.BSIM4lb0 * Inv_L + model.BSIM4wb0 * Inv_W + model.BSIM4pb0 * Inv_LW;
                bsim4.pParam.BSIM4b1 = model.BSIM4b1 + model.BSIM4lb1 * Inv_L + model.BSIM4wb1 * Inv_W + model.BSIM4pb1 * Inv_LW;
                bsim4.pParam.BSIM4alpha0 = model.BSIM4alpha0 + model.BSIM4lalpha0 * Inv_L + model.BSIM4walpha0 * Inv_W + model.BSIM4palpha0 * Inv_LW;
                bsim4.pParam.BSIM4alpha1 = model.BSIM4alpha1 + model.BSIM4lalpha1 * Inv_L + model.BSIM4walpha1 * Inv_W + model.BSIM4palpha1 * Inv_LW;
                bsim4.pParam.BSIM4beta0 = model.BSIM4beta0 + model.BSIM4lbeta0 * Inv_L + model.BSIM4wbeta0 * Inv_W + model.BSIM4pbeta0 * Inv_LW;
                bsim4.pParam.BSIM4agidl = model.BSIM4agidl + model.BSIM4lagidl * Inv_L + model.BSIM4wagidl * Inv_W + model.BSIM4pagidl * Inv_LW;
                bsim4.pParam.BSIM4bgidl = model.BSIM4bgidl + model.BSIM4lbgidl * Inv_L + model.BSIM4wbgidl * Inv_W + model.BSIM4pbgidl * Inv_LW;
                bsim4.pParam.BSIM4cgidl = model.BSIM4cgidl + model.BSIM4lcgidl * Inv_L + model.BSIM4wcgidl * Inv_W + model.BSIM4pcgidl * Inv_LW;
                bsim4.pParam.BSIM4egidl = model.BSIM4egidl + model.BSIM4legidl * Inv_L + model.BSIM4wegidl * Inv_W + model.BSIM4pegidl * Inv_LW;
                bsim4.pParam.BSIM4rgidl = model.BSIM4rgidl /* v4.7 New GIDL / GISL */  + model.BSIM4lrgidl * Inv_L + model.BSIM4wrgidl * Inv_W +
                                    model.BSIM4prgidl * Inv_LW;
                bsim4.pParam.BSIM4kgidl = model.BSIM4kgidl /* v4.7 New GIDL / GISL */  + model.BSIM4lkgidl * Inv_L + model.BSIM4wkgidl * Inv_W +
                                    model.BSIM4pkgidl * Inv_LW;
                bsim4.pParam.BSIM4fgidl = model.BSIM4fgidl /* v4.7 New GIDL / GISL */  + model.BSIM4lfgidl * Inv_L + model.BSIM4wfgidl * Inv_W +
                                    model.BSIM4pfgidl * Inv_LW;
                bsim4.pParam.BSIM4agisl = model.BSIM4agisl + model.BSIM4lagisl * Inv_L + model.BSIM4wagisl * Inv_W + model.BSIM4pagisl * Inv_LW;
                bsim4.pParam.BSIM4bgisl = model.BSIM4bgisl + model.BSIM4lbgisl * Inv_L + model.BSIM4wbgisl * Inv_W + model.BSIM4pbgisl * Inv_LW;
                bsim4.pParam.BSIM4cgisl = model.BSIM4cgisl + model.BSIM4lcgisl * Inv_L + model.BSIM4wcgisl * Inv_W + model.BSIM4pcgisl * Inv_LW;
                bsim4.pParam.BSIM4egisl = model.BSIM4egisl + model.BSIM4legisl * Inv_L + model.BSIM4wegisl * Inv_W + model.BSIM4pegisl * Inv_LW;
                bsim4.pParam.BSIM4rgisl = model.BSIM4rgisl /* v4.7 New GIDL / GISL */  + model.BSIM4lrgisl * Inv_L + model.BSIM4wrgisl * Inv_W +
                                    model.BSIM4prgisl * Inv_LW;
                bsim4.pParam.BSIM4kgisl = model.BSIM4kgisl /* v4.7 New GIDL / GISL */  + model.BSIM4lkgisl * Inv_L + model.BSIM4wkgisl * Inv_W +
                                    model.BSIM4pkgisl * Inv_LW;
                bsim4.pParam.BSIM4fgisl = model.BSIM4fgisl /* v4.7 New GIDL / GISL */  + model.BSIM4lfgisl * Inv_L + model.BSIM4wfgisl * Inv_W +
                                    model.BSIM4pfgisl * Inv_LW;
                bsim4.pParam.BSIM4aigc = model.BSIM4aigc + model.BSIM4laigc * Inv_L + model.BSIM4waigc * Inv_W + model.BSIM4paigc * Inv_LW;
                bsim4.pParam.BSIM4bigc = model.BSIM4bigc + model.BSIM4lbigc * Inv_L + model.BSIM4wbigc * Inv_W + model.BSIM4pbigc * Inv_LW;
                bsim4.pParam.BSIM4cigc = model.BSIM4cigc + model.BSIM4lcigc * Inv_L + model.BSIM4wcigc * Inv_W + model.BSIM4pcigc * Inv_LW;
                bsim4.pParam.BSIM4aigsd = model.BSIM4aigsd + model.BSIM4laigsd * Inv_L + model.BSIM4waigsd * Inv_W + model.BSIM4paigsd * Inv_LW;
                bsim4.pParam.BSIM4bigsd = model.BSIM4bigsd + model.BSIM4lbigsd * Inv_L + model.BSIM4wbigsd * Inv_W + model.BSIM4pbigsd * Inv_LW;
                bsim4.pParam.BSIM4cigsd = model.BSIM4cigsd + model.BSIM4lcigsd * Inv_L + model.BSIM4wcigsd * Inv_W + model.BSIM4pcigsd * Inv_LW;
                bsim4.pParam.BSIM4aigs = model.BSIM4aigs + model.BSIM4laigs * Inv_L + model.BSIM4waigs * Inv_W + model.BSIM4paigs * Inv_LW;
                bsim4.pParam.BSIM4bigs = model.BSIM4bigs + model.BSIM4lbigs * Inv_L + model.BSIM4wbigs * Inv_W + model.BSIM4pbigs * Inv_LW;
                bsim4.pParam.BSIM4cigs = model.BSIM4cigs + model.BSIM4lcigs * Inv_L + model.BSIM4wcigs * Inv_W + model.BSIM4pcigs * Inv_LW;
                bsim4.pParam.BSIM4aigd = model.BSIM4aigd + model.BSIM4laigd * Inv_L + model.BSIM4waigd * Inv_W + model.BSIM4paigd * Inv_LW;
                bsim4.pParam.BSIM4bigd = model.BSIM4bigd + model.BSIM4lbigd * Inv_L + model.BSIM4wbigd * Inv_W + model.BSIM4pbigd * Inv_LW;
                bsim4.pParam.BSIM4cigd = model.BSIM4cigd + model.BSIM4lcigd * Inv_L + model.BSIM4wcigd * Inv_W + model.BSIM4pcigd * Inv_LW;
                bsim4.pParam.BSIM4aigbacc = model.BSIM4aigbacc + model.BSIM4laigbacc * Inv_L + model.BSIM4waigbacc * Inv_W + model.BSIM4paigbacc *
                  Inv_LW;
                bsim4.pParam.BSIM4bigbacc = model.BSIM4bigbacc + model.BSIM4lbigbacc * Inv_L + model.BSIM4wbigbacc * Inv_W + model.BSIM4pbigbacc *
                  Inv_LW;
                bsim4.pParam.BSIM4cigbacc = model.BSIM4cigbacc + model.BSIM4lcigbacc * Inv_L + model.BSIM4wcigbacc * Inv_W + model.BSIM4pcigbacc *
                  Inv_LW;
                bsim4.pParam.BSIM4aigbinv = model.BSIM4aigbinv + model.BSIM4laigbinv * Inv_L + model.BSIM4waigbinv * Inv_W + model.BSIM4paigbinv *
                  Inv_LW;
                bsim4.pParam.BSIM4bigbinv = model.BSIM4bigbinv + model.BSIM4lbigbinv * Inv_L + model.BSIM4wbigbinv * Inv_W + model.BSIM4pbigbinv *
                  Inv_LW;
                bsim4.pParam.BSIM4cigbinv = model.BSIM4cigbinv + model.BSIM4lcigbinv * Inv_L + model.BSIM4wcigbinv * Inv_W + model.BSIM4pcigbinv *
                  Inv_LW;
                bsim4.pParam.BSIM4nigc = model.BSIM4nigc + model.BSIM4lnigc * Inv_L + model.BSIM4wnigc * Inv_W + model.BSIM4pnigc * Inv_LW;
                bsim4.pParam.BSIM4nigbacc = model.BSIM4nigbacc + model.BSIM4lnigbacc * Inv_L + model.BSIM4wnigbacc * Inv_W + model.BSIM4pnigbacc *
                  Inv_LW;
                bsim4.pParam.BSIM4nigbinv = model.BSIM4nigbinv + model.BSIM4lnigbinv * Inv_L + model.BSIM4wnigbinv * Inv_W + model.BSIM4pnigbinv *
                  Inv_LW;
                bsim4.pParam.BSIM4ntox = model.BSIM4ntox + model.BSIM4lntox * Inv_L + model.BSIM4wntox * Inv_W + model.BSIM4pntox * Inv_LW;
                bsim4.pParam.BSIM4eigbinv = model.BSIM4eigbinv + model.BSIM4leigbinv * Inv_L + model.BSIM4weigbinv * Inv_W + model.BSIM4peigbinv *
                  Inv_LW;
                bsim4.pParam.BSIM4pigcd = model.BSIM4pigcd + model.BSIM4lpigcd * Inv_L + model.BSIM4wpigcd * Inv_W + model.BSIM4ppigcd * Inv_LW;
                bsim4.pParam.BSIM4poxedge = model.BSIM4poxedge + model.BSIM4lpoxedge * Inv_L + model.BSIM4wpoxedge * Inv_W + model.BSIM4ppoxedge *
                  Inv_LW;
                bsim4.pParam.BSIM4xrcrg1 = model.BSIM4xrcrg1 + model.BSIM4lxrcrg1 * Inv_L + model.BSIM4wxrcrg1 * Inv_W + model.BSIM4pxrcrg1 * Inv_LW;
                bsim4.pParam.BSIM4xrcrg2 = model.BSIM4xrcrg2 + model.BSIM4lxrcrg2 * Inv_L + model.BSIM4wxrcrg2 * Inv_W + model.BSIM4pxrcrg2 * Inv_LW;
                bsim4.pParam.BSIM4lambda = model.BSIM4lambda + model.BSIM4llambda * Inv_L + model.BSIM4wlambda * Inv_W + model.BSIM4plambda * Inv_LW;
                bsim4.pParam.BSIM4vtl = model.BSIM4vtl + model.BSIM4lvtl * Inv_L + model.BSIM4wvtl * Inv_W + model.BSIM4pvtl * Inv_LW;
                bsim4.pParam.BSIM4xn = model.BSIM4xn + model.BSIM4lxn * Inv_L + model.BSIM4wxn * Inv_W + model.BSIM4pxn * Inv_LW;
                bsim4.pParam.BSIM4vfbsdoff = model.BSIM4vfbsdoff + model.BSIM4lvfbsdoff * Inv_L + model.BSIM4wvfbsdoff * Inv_W +
                                    model.BSIM4pvfbsdoff * Inv_LW;
                bsim4.pParam.BSIM4tvfbsdoff = model.BSIM4tvfbsdoff + model.BSIM4ltvfbsdoff * Inv_L + model.BSIM4wtvfbsdoff * Inv_W +
                                    model.BSIM4ptvfbsdoff * Inv_LW;

                bsim4.pParam.BSIM4cgsl = model.BSIM4cgsl + model.BSIM4lcgsl * Inv_L + model.BSIM4wcgsl * Inv_W + model.BSIM4pcgsl * Inv_LW;
                bsim4.pParam.BSIM4cgdl = model.BSIM4cgdl + model.BSIM4lcgdl * Inv_L + model.BSIM4wcgdl * Inv_W + model.BSIM4pcgdl * Inv_LW;
                bsim4.pParam.BSIM4ckappas = model.BSIM4ckappas + model.BSIM4lckappas * Inv_L + model.BSIM4wckappas * Inv_W + model.BSIM4pckappas *
                  Inv_LW;
                bsim4.pParam.BSIM4ckappad = model.BSIM4ckappad + model.BSIM4lckappad * Inv_L + model.BSIM4wckappad * Inv_W + model.BSIM4pckappad *
                  Inv_LW;
                bsim4.pParam.BSIM4cf = model.BSIM4cf + model.BSIM4lcf * Inv_L + model.BSIM4wcf * Inv_W + model.BSIM4pcf * Inv_LW;
                bsim4.pParam.BSIM4clc = model.BSIM4clc + model.BSIM4lclc * Inv_L + model.BSIM4wclc * Inv_W + model.BSIM4pclc * Inv_LW;
                bsim4.pParam.BSIM4cle = model.BSIM4cle + model.BSIM4lcle * Inv_L + model.BSIM4wcle * Inv_W + model.BSIM4pcle * Inv_LW;
                bsim4.pParam.BSIM4vfbcv = model.BSIM4vfbcv + model.BSIM4lvfbcv * Inv_L + model.BSIM4wvfbcv * Inv_W + model.BSIM4pvfbcv * Inv_LW;
                bsim4.pParam.BSIM4acde = model.BSIM4acde + model.BSIM4lacde * Inv_L + model.BSIM4wacde * Inv_W + model.BSIM4pacde * Inv_LW;
                bsim4.pParam.BSIM4moin = model.BSIM4moin + model.BSIM4lmoin * Inv_L + model.BSIM4wmoin * Inv_W + model.BSIM4pmoin * Inv_LW;
                bsim4.pParam.BSIM4noff = model.BSIM4noff + model.BSIM4lnoff * Inv_L + model.BSIM4wnoff * Inv_W + model.BSIM4pnoff * Inv_LW;
                bsim4.pParam.BSIM4voffcv = model.BSIM4voffcv + model.BSIM4lvoffcv * Inv_L + model.BSIM4wvoffcv * Inv_W + model.BSIM4pvoffcv * Inv_LW;
                bsim4.pParam.BSIM4kvth0we = model.BSIM4kvth0we + model.BSIM4lkvth0we * Inv_L + model.BSIM4wkvth0we * Inv_W + model.BSIM4pkvth0we *
                  Inv_LW;
                bsim4.pParam.BSIM4k2we = model.BSIM4k2we + model.BSIM4lk2we * Inv_L + model.BSIM4wk2we * Inv_W + model.BSIM4pk2we * Inv_LW;
                bsim4.pParam.BSIM4ku0we = model.BSIM4ku0we + model.BSIM4lku0we * Inv_L + model.BSIM4wku0we * Inv_W + model.BSIM4pku0we * Inv_LW;

                bsim4.pParam.BSIM4abulkCVfactor = 1.0 + Math.Pow((bsim4.pParam.BSIM4clc / bsim4.pParam.BSIM4leffCV), bsim4.pParam.BSIM4cle);

                T0 = (model.TRatio - 1.0);

                PowWeffWr = Math.Pow(bsim4.pParam.BSIM4weffCJ * 1.0e6, bsim4.pParam.BSIM4wr) * bsim4.BSIM4nf;

                T1 = T2 = T3 = T4 = 0.0;
                bsim4.pParam.BSIM4ucs = bsim4.pParam.BSIM4ucs * Math.Pow(model.TRatio, bsim4.pParam.BSIM4ucste);
                if (model.BSIM4tempMod.Value == 0)
                {
                    bsim4.pParam.BSIM4ua = bsim4.pParam.BSIM4ua + bsim4.pParam.BSIM4ua1 * T0;
                    bsim4.pParam.BSIM4ub = bsim4.pParam.BSIM4ub + bsim4.pParam.BSIM4ub1 * T0;
                    bsim4.pParam.BSIM4uc = bsim4.pParam.BSIM4uc + bsim4.pParam.BSIM4uc1 * T0;
                    bsim4.pParam.BSIM4ud = bsim4.pParam.BSIM4ud + bsim4.pParam.BSIM4ud1 * T0;
                    bsim4.pParam.BSIM4vsattemp = bsim4.pParam.BSIM4vsat - bsim4.pParam.BSIM4at * T0;
                    T10 = bsim4.pParam.BSIM4prt * T0;
                    if (model.BSIM4rdsMod != 0)
                    {
                        /* External Rd(V) */
                        T1 = bsim4.pParam.BSIM4rdw + T10;
                        T2 = model.BSIM4rdwmin + T10;
                        /* External Rs(V) */
                        T3 = bsim4.pParam.BSIM4rsw + T10;
                        T4 = model.BSIM4rswmin + T10;
                    }
                    /* Internal Rds(V) in IV */
                    bsim4.pParam.BSIM4rds0 = (bsim4.pParam.BSIM4rdsw + T10) * bsim4.BSIM4nf / PowWeffWr;
                    bsim4.pParam.BSIM4rdswmin = (model.BSIM4rdswmin + T10) * bsim4.BSIM4nf / PowWeffWr;
                }
                else
                {
                    if (model.BSIM4tempMod.Value == 3)
                    {
                        bsim4.pParam.BSIM4ua = bsim4.pParam.BSIM4ua * Math.Pow(model.TRatio, bsim4.pParam.BSIM4ua1);
                        bsim4.pParam.BSIM4ub = bsim4.pParam.BSIM4ub * Math.Pow(model.TRatio, bsim4.pParam.BSIM4ub1);
                        bsim4.pParam.BSIM4uc = bsim4.pParam.BSIM4uc * Math.Pow(model.TRatio, bsim4.pParam.BSIM4uc1);
                        bsim4.pParam.BSIM4ud = bsim4.pParam.BSIM4ud * Math.Pow(model.TRatio, bsim4.pParam.BSIM4ud1);
                    }
                    else
                    {
                        /* tempMod = 1, 2 */
                        bsim4.pParam.BSIM4ua = bsim4.pParam.BSIM4ua * (1.0 + bsim4.pParam.BSIM4ua1 * model.delTemp);
                        bsim4.pParam.BSIM4ub = bsim4.pParam.BSIM4ub * (1.0 + bsim4.pParam.BSIM4ub1 * model.delTemp);
                        bsim4.pParam.BSIM4uc = bsim4.pParam.BSIM4uc * (1.0 + bsim4.pParam.BSIM4uc1 * model.delTemp);
                        bsim4.pParam.BSIM4ud = bsim4.pParam.BSIM4ud * (1.0 + bsim4.pParam.BSIM4ud1 * model.delTemp);
                    }
                    bsim4.pParam.BSIM4vsattemp = bsim4.pParam.BSIM4vsat * (1.0 - bsim4.pParam.BSIM4at * model.delTemp);
                    T10 = 1.0 + bsim4.pParam.BSIM4prt * model.delTemp;
                    if (model.BSIM4rdsMod != 0)
                    {
                        /* External Rd(V) */
                        T1 = bsim4.pParam.BSIM4rdw * T10;
                        T2 = model.BSIM4rdwmin * T10;

                        /* External Rs(V) */
                        T3 = bsim4.pParam.BSIM4rsw * T10;
                        T4 = model.BSIM4rswmin * T10;
                    }
                    /* Internal Rds(V) in IV */
                    bsim4.pParam.BSIM4rds0 = bsim4.pParam.BSIM4rdsw * T10 * bsim4.BSIM4nf / PowWeffWr;
                    bsim4.pParam.BSIM4rdswmin = model.BSIM4rdswmin * T10 * bsim4.BSIM4nf / PowWeffWr;
                }

                if (T1 < 0.0)
                {
                    T1 = 0.0;

                    CircuitWarning.Warning(this, "Warning: Rdw at current temperature is negative; set to 0.");
                }
                if (T2 < 0.0)
                {
                    T2 = 0.0;

                    CircuitWarning.Warning(this, "Warning: Rdwmin at current temperature is negative; set to 0.");
                }
                bsim4.pParam.BSIM4rd0 = T1 / PowWeffWr;
                bsim4.pParam.BSIM4rdwmin = T2 / PowWeffWr;
                if (T3 < 0.0)
                {
                    T3 = 0.0;

                    CircuitWarning.Warning(this, "Warning: Rsw at current temperature is negative; set to 0.");
                }
                if (T4 < 0.0)
                {
                    T4 = 0.0;

                    CircuitWarning.Warning(this, "Warning: Rswmin at current temperature is negative; set to 0.");
                }
                bsim4.pParam.BSIM4rs0 = T3 / PowWeffWr;
                bsim4.pParam.BSIM4rswmin = T4 / PowWeffWr;

                if (bsim4.pParam.BSIM4u0 > 1.0)
                    bsim4.pParam.BSIM4u0 = bsim4.pParam.BSIM4u0 / 1.0e4;

                /* mobility channel length dependence */
                T5 = 1.0 - bsim4.pParam.BSIM4up * Math.Exp(-bsim4.pParam.BSIM4leff / bsim4.pParam.BSIM4lp);
                bsim4.pParam.BSIM4u0temp = bsim4.pParam.BSIM4u0 * T5 * Math.Pow(model.TRatio, bsim4.pParam.BSIM4ute);
                if (bsim4.pParam.BSIM4eu < 0.0)
                {
                    bsim4.pParam.BSIM4eu = 0.0;

                    CircuitWarning.Warning(this, "Warning: eu has been negative; reset to 0.0.");
                }
                if (bsim4.pParam.BSIM4ucs < 0.0)
                {
                    bsim4.pParam.BSIM4ucs = 0.0;

                    CircuitWarning.Warning(this, "Warning: ucs has been negative; reset to 0.0.");
                }

                bsim4.pParam.BSIM4vfbsdoff = bsim4.pParam.BSIM4vfbsdoff * (1.0 + bsim4.pParam.BSIM4tvfbsdoff * model.delTemp);
                bsim4.pParam.BSIM4voff = bsim4.pParam.BSIM4voff * (1.0 + bsim4.pParam.BSIM4tvoff * model.delTemp);

                bsim4.pParam.BSIM4nfactor = bsim4.pParam.BSIM4nfactor + bsim4.pParam.BSIM4tnfactor * model.delTemp / model.Tnom; /*
					v4.7 temp dep of leakage currents */
                bsim4.pParam.BSIM4voffcv = bsim4.pParam.BSIM4voffcv * (1.0 + bsim4.pParam.BSIM4tvoffcv * model.delTemp); /* v4.7 temp dep of leakage currents */
                bsim4.pParam.BSIM4eta0 = bsim4.pParam.BSIM4eta0 + bsim4.pParam.BSIM4teta0 * model.delTemp / model.Tnom; /* v4.7 temp dep of leakage currents */

                /* Source End Velocity Limit */
                if ((model.BSIM4vtl.Given) && (model.BSIM4vtl > 0.0))
                {
                    if (model.BSIM4lc < 0.0)

                        bsim4.pParam.BSIM4lc = 0.0;
                    else bsim4.pParam.BSIM4lc = model.BSIM4lc;
                    T0 = bsim4.pParam.BSIM4leff / (bsim4.pParam.BSIM4xn * bsim4.pParam.BSIM4leff + bsim4.pParam.BSIM4lc);
                    bsim4.pParam.BSIM4tfactor = (1.0 - T0) / (1.0 + T0);
                }

                bsim4.pParam.BSIM4cgdo = (model.BSIM4cgdo + bsim4.pParam.BSIM4cf) * bsim4.pParam.BSIM4weffCV;
                bsim4.pParam.BSIM4cgso = (model.BSIM4cgso + bsim4.pParam.BSIM4cf) * bsim4.pParam.BSIM4weffCV;
                bsim4.pParam.BSIM4cgbo = model.BSIM4cgbo * bsim4.pParam.BSIM4leffCV * bsim4.BSIM4nf;

                if (!model.BSIM4ndep.Given && model.BSIM4gamma1.Given)
                {
                    T0 = bsim4.pParam.BSIM4gamma1 * model.BSIM4coxe;
                    bsim4.pParam.BSIM4ndep = 3.01248e22 * T0 * T0;
                }

                bsim4.pParam.BSIM4phi = model.Vtm0 * Math.Log(bsim4.pParam.BSIM4ndep / model.ni) + bsim4.pParam.BSIM4phin + 0.4;

                bsim4.pParam.BSIM4sqrtPhi = Math.Sqrt(bsim4.pParam.BSIM4phi);
                bsim4.pParam.BSIM4phis3 = bsim4.pParam.BSIM4sqrtPhi * bsim4.pParam.BSIM4phi;

                bsim4.pParam.BSIM4Xdep0 = Math.Sqrt(2.0 * model.epssub / (Transistor.Charge_q * bsim4.pParam.BSIM4ndep * 1.0e6)) * bsim4.pParam.BSIM4sqrtPhi;
                bsim4.pParam.BSIM4sqrtXdep0 = Math.Sqrt(bsim4.pParam.BSIM4Xdep0);

                if (model.BSIM4mtrlMod.Value == 0)
                    bsim4.pParam.BSIM4litl = Math.Sqrt(3.0 * 3.9 / model.epsrox * bsim4.pParam.BSIM4xj * model.toxe);
                else
                    bsim4.pParam.BSIM4litl = Math.Sqrt(model.BSIM4epsrsub / model.epsrox * bsim4.pParam.BSIM4xj * model.toxe);

                bsim4.pParam.BSIM4vbi = model.Vtm0 * Math.Log(bsim4.pParam.BSIM4nsd * bsim4.pParam.BSIM4ndep / (model.ni * model.ni));

                if (model.BSIM4mtrlMod.Value == 0)
                {
                    if (bsim4.pParam.BSIM4ngate > 0.0)
                    {
                        bsim4.pParam.BSIM4vfbsd = model.Vtm0 * Math.Log(bsim4.pParam.BSIM4ngate / bsim4.pParam.BSIM4nsd);
                    }
                    else
                        bsim4.pParam.BSIM4vfbsd = 0.0;
                }
                else
                {
                    T0 = model.Vtm0 * Math.Log(bsim4.pParam.BSIM4nsd / model.ni);
                    T1 = 0.5 * model.Eg0;
                    if (T0 > T1)
                        T0 = T1;
                    T2 = model.BSIM4easub + T1 - model.BSIM4type * T0;
                    bsim4.pParam.BSIM4vfbsd = model.BSIM4phig - T2;
                }

                bsim4.pParam.BSIM4cdep0 = Math.Sqrt(Transistor.Charge_q * model.epssub * bsim4.pParam.BSIM4ndep * 1.0e6 / 2.0 / bsim4.pParam.BSIM4phi);

                bsim4.pParam.BSIM4ToxRatio = Math.Exp(bsim4.pParam.BSIM4ntox * Math.Log(model.BSIM4toxref / model.toxe)) / model.toxe / model.toxe;
                bsim4.pParam.BSIM4ToxRatioEdge = Math.Exp(bsim4.pParam.BSIM4ntox * Math.Log(model.BSIM4toxref / (model.toxe * bsim4.pParam.BSIM4poxedge))) /
                    model.toxe / model.toxe / bsim4.pParam.BSIM4poxedge / bsim4.pParam.BSIM4poxedge;
                bsim4.pParam.BSIM4Aechvb = (model.BSIM4type == BSIM4v80.NMOS) ? 4.97232e-7 : 3.42537e-7;
                bsim4.pParam.BSIM4Bechvb = (model.BSIM4type == BSIM4v80.NMOS) ? 7.45669e11 : 1.16645e12;
                bsim4.pParam.BSIM4AechvbEdgeS = bsim4.pParam.BSIM4Aechvb * bsim4.pParam.BSIM4weff * model.BSIM4dlcig * bsim4.pParam.BSIM4ToxRatioEdge;
                bsim4.pParam.BSIM4AechvbEdgeD = bsim4.pParam.BSIM4Aechvb * bsim4.pParam.BSIM4weff * model.BSIM4dlcigd * bsim4.pParam.BSIM4ToxRatioEdge;
                bsim4.pParam.BSIM4BechvbEdge = -bsim4.pParam.BSIM4Bechvb * model.toxe * bsim4.pParam.BSIM4poxedge;
                bsim4.pParam.BSIM4Aechvb *= bsim4.pParam.BSIM4weff * bsim4.pParam.BSIM4leff * bsim4.pParam.BSIM4ToxRatio;
                bsim4.pParam.BSIM4Bechvb *= -model.toxe;

                bsim4.pParam.BSIM4mstar = 0.5 + Math.Atan(bsim4.pParam.BSIM4minv) / Circuit.CONSTPI;
                bsim4.pParam.BSIM4mstarcv = 0.5 + Math.Atan(bsim4.pParam.BSIM4minvcv) / Circuit.CONSTPI;
                bsim4.pParam.BSIM4voffcbn = bsim4.pParam.BSIM4voff + model.BSIM4voffl / bsim4.pParam.BSIM4leff;
                bsim4.pParam.BSIM4voffcbncv = bsim4.pParam.BSIM4voffcv + model.BSIM4voffcvl / bsim4.pParam.BSIM4leff;

                bsim4.pParam.BSIM4ldeb = Math.Sqrt(model.epssub * model.Vtm0 / (Transistor.Charge_q * bsim4.pParam.BSIM4ndep * 1.0e6)) / 3.0;
                bsim4.pParam.BSIM4acde *= Math.Pow((bsim4.pParam.BSIM4ndep / 2.0e16), -0.25);

                if (model.BSIM4k1.Given || model.BSIM4k2.Given)
                {
                    if (!model.BSIM4k1.Given)
                    {

                        CircuitWarning.Warning(this, "Warning: k1 should be specified with k2.");
                        bsim4.pParam.BSIM4k1 = 0.53;
                    }
                    if (!model.BSIM4k2.Given)
                    {

                        CircuitWarning.Warning(this, "Warning: k2 should be specified with k1.");
                        bsim4.pParam.BSIM4k2 = -0.0186;
                    }
                    if (model.BSIM4nsub.Given)
                        CircuitWarning.Warning(this, "Warning: nsub is ignored because k1 or k2 is given.");
                    if (model.BSIM4xt.Given)
                        CircuitWarning.Warning(this, "Warning: xt is ignored because k1 or k2 is given.");
                    if (model.BSIM4vbx.Given)
                        CircuitWarning.Warning(this, "Warning: vbx is ignored because k1 or k2 is given.");
                    if (model.BSIM4gamma1.Given)
                        CircuitWarning.Warning(this, "Warning: gamma1 is ignored because k1 or k2 is given.");
                    if (model.BSIM4gamma2.Given)
                        CircuitWarning.Warning(this, "Warning: gamma2 is ignored because k1 or k2 is given.");
                }
                else
                {
                    if (!model.BSIM4vbx.Given)
                        bsim4.pParam.BSIM4vbx = bsim4.pParam.BSIM4phi - 7.7348e-4 * bsim4.pParam.BSIM4ndep * bsim4.pParam.BSIM4xt * bsim4.pParam.BSIM4xt;
                    if (bsim4.pParam.BSIM4vbx > 0.0)
                        bsim4.pParam.BSIM4vbx = -bsim4.pParam.BSIM4vbx;
                    if (bsim4.pParam.BSIM4vbm > 0.0)
                        bsim4.pParam.BSIM4vbm = -bsim4.pParam.BSIM4vbm;

                    if (!model.BSIM4gamma1.Given)
                        bsim4.pParam.BSIM4gamma1 = 5.753e-12 * Math.Sqrt(bsim4.pParam.BSIM4ndep) / model.BSIM4coxe;
                    if (!model.BSIM4gamma2.Given)
                        bsim4.pParam.BSIM4gamma2 = 5.753e-12 * Math.Sqrt(bsim4.pParam.BSIM4nsub) / model.BSIM4coxe;

                    T0 = bsim4.pParam.BSIM4gamma1 - bsim4.pParam.BSIM4gamma2;
                    T1 = Math.Sqrt(bsim4.pParam.BSIM4phi - bsim4.pParam.BSIM4vbx) - bsim4.pParam.BSIM4sqrtPhi;
                    T2 = Math.Sqrt(bsim4.pParam.BSIM4phi * (bsim4.pParam.BSIM4phi - bsim4.pParam.BSIM4vbm)) - bsim4.pParam.BSIM4phi;
                    bsim4.pParam.BSIM4k2 = T0 * T1 / (2.0 * T2 + bsim4.pParam.BSIM4vbm);
                    bsim4.pParam.BSIM4k1 = bsim4.pParam.BSIM4gamma2 - 2.0 * bsim4.pParam.BSIM4k2 * Math.Sqrt(bsim4.pParam.BSIM4phi - bsim4.pParam.BSIM4vbm);
                }

                if (!model.BSIM4vfb.Given)
                {
                    if (model.BSIM4vth0.Given)
                    {
                        bsim4.pParam.BSIM4vfb = model.BSIM4type * bsim4.pParam.BSIM4vth0 - bsim4.pParam.BSIM4phi - bsim4.pParam.BSIM4k1 * bsim4.pParam.BSIM4sqrtPhi;
                    }
                    else
                    {
                        if ((model.BSIM4mtrlMod != 0) && (model.BSIM4phig.Given) && (model.BSIM4nsub.Given))
                        {
                            T0 = model.Vtm0 * Math.Log(bsim4.pParam.BSIM4nsub / model.ni);
                            T1 = 0.5 * model.Eg0;
                            if (T0 > T1)
                                T0 = T1;
                            T2 = model.BSIM4easub + T1 + model.BSIM4type * T0;
                            bsim4.pParam.BSIM4vfb = model.BSIM4phig - T2;
                        }
                        else
                        {
                            bsim4.pParam.BSIM4vfb = -1.0;
                        }
                    }
                }
                if (!model.BSIM4vth0.Given)
                {
                    bsim4.pParam.BSIM4vth0 = model.BSIM4type * (bsim4.pParam.BSIM4vfb + bsim4.pParam.BSIM4phi + bsim4.pParam.BSIM4k1 * bsim4.pParam.BSIM4sqrtPhi);
                }

                bsim4.pParam.BSIM4k1ox = bsim4.pParam.BSIM4k1 * model.toxe / model.BSIM4toxm;

                tmp = Math.Sqrt(model.epssub / (model.epsrox * Transistor.EPS0) * model.toxe * bsim4.pParam.BSIM4Xdep0);
                T0 = bsim4.pParam.BSIM4dsub * bsim4.pParam.BSIM4leff / tmp;
                if (T0 < Transistor.EXP_THRESHOLD)
                {
                    T1 = Math.Exp(T0);
                    T2 = T1 - 1.0;
                    T3 = T2 * T2;
                    T4 = T3 + 2.0 * T1 * Transistor.MIN_EXP;
                    bsim4.pParam.BSIM4theta0vb0 = T1 / T4;
                }
                else
                    bsim4.pParam.BSIM4theta0vb0 = 1.0 / (Transistor.MAX_EXP - 2.0);

                T0 = bsim4.pParam.BSIM4drout * bsim4.pParam.BSIM4leff / tmp;
                if (T0 < Transistor.EXP_THRESHOLD)
                {
                    T1 = Math.Exp(T0);
                    T2 = T1 - 1.0;
                    T3 = T2 * T2;
                    T4 = T3 + 2.0 * T1 * Transistor.MIN_EXP;
                    T5 = T1 / T4;
                }
                else
                    T5 = 1.0 / (Transistor.MAX_EXP - 2.0); /* 3.0 * Transistor.MIN_EXP omitted */
                bsim4.pParam.BSIM4thetaRout = bsim4.pParam.BSIM4pdibl1 * T5 + bsim4.pParam.BSIM4pdibl2;

                tmp = Math.Sqrt(bsim4.pParam.BSIM4Xdep0);
                tmp1 = bsim4.pParam.BSIM4vbi - bsim4.pParam.BSIM4phi;
                tmp2 = model.BSIM4factor1 * tmp;

                T0 = bsim4.pParam.BSIM4dvt1w * bsim4.pParam.BSIM4weff * bsim4.pParam.BSIM4leff / tmp2;
                if (T0 < Transistor.EXP_THRESHOLD)
                {
                    T1 = Math.Exp(T0);
                    T2 = T1 - 1.0;
                    T3 = T2 * T2;
                    T4 = T3 + 2.0 * T1 * Transistor.MIN_EXP;
                    T8 = T1 / T4;
                }
                else
                    T8 = 1.0 / (Transistor.MAX_EXP - 2.0);
                T0 = bsim4.pParam.BSIM4dvt0w * T8;
                T8 = T0 * tmp1;

                T0 = bsim4.pParam.BSIM4dvt1 * bsim4.pParam.BSIM4leff / tmp2;
                if (T0 < Transistor.EXP_THRESHOLD)
                {
                    T1 = Math.Exp(T0);
                    T2 = T1 - 1.0;
                    T3 = T2 * T2;
                    T4 = T3 + 2.0 * T1 * Transistor.MIN_EXP;
                    T9 = T1 / T4;
                }
                else
                    T9 = 1.0 / (Transistor.MAX_EXP - 2.0);
                T9 = bsim4.pParam.BSIM4dvt0 * T9 * tmp1;

                T4 = model.toxe * bsim4.pParam.BSIM4phi / (bsim4.pParam.BSIM4weff + bsim4.pParam.BSIM4w0);

                T0 = Math.Sqrt(1.0 + bsim4.pParam.BSIM4lpe0 / bsim4.pParam.BSIM4leff);
                if ((model.BSIM4tempMod.Value == 1) || (model.BSIM4tempMod.Value == 0))
                    T3 = (bsim4.pParam.BSIM4kt1 + bsim4.pParam.BSIM4kt1l / bsim4.pParam.BSIM4leff) * (model.TRatio - 1.0);
                if ((model.BSIM4tempMod.Value == 2) || (model.BSIM4tempMod.Value == 3))
                    T3 = -bsim4.pParam.BSIM4kt1 * (model.TRatio - 1.0);

                T5 = bsim4.pParam.BSIM4k1ox * (T0 - 1.0) * bsim4.pParam.BSIM4sqrtPhi + T3;
                bsim4.pParam.BSIM4vfbzbfactor = -T8 - T9 + bsim4.pParam.BSIM4k3 * T4 + T5 - bsim4.pParam.BSIM4phi - bsim4.pParam.BSIM4k1 * bsim4.pParam.BSIM4sqrtPhi;

                /* stress effect */

                wlod = model.BSIM4wlod;
                if (model.BSIM4wlod < 0.0)
                {

                    CircuitWarning.Warning(this, $"Warning: WLOD = {model.BSIM4wlod} is less than 0. 0.0 is used");
                    wlod = 0.0;
                }
                T0 = Math.Pow(Lnew, model.BSIM4llodku0);
                W_tmp = Wnew + wlod;
                T1 = Math.Pow(W_tmp, model.BSIM4wlodku0);
                tmp1 = model.BSIM4lku0 / T0 + model.BSIM4wku0 / T1 + model.BSIM4pku0 / (T0 * T1);
                bsim4.pParam.BSIM4ku0 = 1.0 + tmp1;

                T0 = Math.Pow(Lnew, model.BSIM4llodvth);
                T1 = Math.Pow(W_tmp, model.BSIM4wlodvth);
                tmp1 = model.BSIM4lkvth0 / T0 + model.BSIM4wkvth0 / T1 + model.BSIM4pkvth0 / (T0 * T1);
                bsim4.pParam.BSIM4kvth0 = 1.0 + tmp1;
                bsim4.pParam.BSIM4kvth0 = Math.Sqrt(bsim4.pParam.BSIM4kvth0 * bsim4.pParam.BSIM4kvth0 + Transistor.DELTA);

                T0 = (model.TRatio - 1.0);
                bsim4.pParam.BSIM4ku0temp = bsim4.pParam.BSIM4ku0 * (1.0 + model.BSIM4tku0 * T0) + Transistor.DELTA;

                Inv_saref = 1.0 / (model.BSIM4saref + 0.5 * Ldrn);
                Inv_sbref = 1.0 / (model.BSIM4sbref + 0.5 * Ldrn);
                bsim4.pParam.BSIM4inv_od_ref = Inv_saref + Inv_sbref;
                bsim4.pParam.BSIM4rho_ref = model.BSIM4ku0 / bsim4.pParam.BSIM4ku0temp * bsim4.pParam.BSIM4inv_od_ref;

                /* high k */
                /* Calculate VgsteffVth for mobMod = 3 */
                if (model.BSIM4mobMod.Value == 3)
                {
                    /* Calculate n @ Vbs = Vds = 0 */
                    lt1 = model.BSIM4factor1 * bsim4.pParam.BSIM4sqrtXdep0;
                    T0 = bsim4.pParam.BSIM4dvt1 * bsim4.pParam.BSIM4leff / lt1;
                    if (T0 < Transistor.EXP_THRESHOLD)
                    {
                        T1 = Math.Exp(T0);
                        T2 = T1 - 1.0;
                        T3 = T2 * T2;
                        T4 = T3 + 2.0 * T1 * Transistor.MIN_EXP;
                        Theta0 = T1 / T4;
                    }
                    else
                        Theta0 = 1.0 / (Transistor.MAX_EXP - 2.0);

                    tmp1 = model.epssub / bsim4.pParam.BSIM4Xdep0;
                    tmp2 = bsim4.pParam.BSIM4nfactor * tmp1;
                    tmp3 = (tmp2 + bsim4.pParam.BSIM4cdsc * Theta0 + bsim4.pParam.BSIM4cit) / model.BSIM4coxe;
                    if (tmp3 >= -0.5)
                        n0 = 1.0 + tmp3;
                    else
                    {
                        T0 = 1.0 / (3.0 + 8.0 * tmp3);
                        n0 = (1.0 + 3.0 * tmp3) * T0;
                    }

                    T0 = n0 * model.BSIM4vtm;
                    T1 = bsim4.pParam.BSIM4voffcbn;
                    T2 = T1 / T0;
                    if (T2 < -Transistor.EXP_THRESHOLD)
                    {
                        T3 = model.BSIM4coxe * Transistor.MIN_EXP / bsim4.pParam.BSIM4cdep0;
                        T4 = bsim4.pParam.BSIM4mstar + T3 * n0;
                    }
                    else if (T2 > Transistor.EXP_THRESHOLD)
                    {
                        T3 = model.BSIM4coxe * Transistor.MAX_EXP / bsim4.pParam.BSIM4cdep0;
                        T4 = bsim4.pParam.BSIM4mstar + T3 * n0;
                    }
                    else
                    {
                        T3 = Math.Exp(T2) * model.BSIM4coxe / bsim4.pParam.BSIM4cdep0;
                        T4 = bsim4.pParam.BSIM4mstar + T3 * n0;
                    }
                    bsim4.pParam.BSIM4VgsteffVth = T0 * Math.Log(2.0) / T4;
                }

                /* New DITS term added in 4.7 */
                T0 = -bsim4.pParam.BSIM4dvtp3 * Math.Log(bsim4.pParam.BSIM4leff);
                T1 = Dexp(T0);
                bsim4.pParam.BSIM4dvtp2factor = bsim4.pParam.BSIM4dvtp5 + bsim4.pParam.BSIM4dvtp2 * T1;

            } /* End of SizeNotFound */

            /* stress effect */
            if ((bsim4.BSIM4sa > 0.0) && (bsim4.BSIM4sb > 0.0) && ((bsim4.BSIM4nf.Value == 1.0) || ((bsim4.BSIM4nf > 1.0) && (bsim4.BSIM4sd > 0.0))))
            {
                Inv_sa = 0;
                Inv_sb = 0;

                kvsat = model.BSIM4kvsat;
                if (model.BSIM4kvsat < -1.0)
                {

                    CircuitWarning.Warning(this, $"Warning: KVSAT = {model.BSIM4kvsat} is too small; - 1.0 is used.");
                    kvsat = -1.0;
                }
                if (model.BSIM4kvsat > 1.0)
                {

                    CircuitWarning.Warning(this, $"Warning: KVSAT = {model.BSIM4kvsat} is too big; 1.0 is used.");
                    kvsat = 1.0;
                }

                for (i = 0; i < bsim4.BSIM4nf; i++)
                {
                    T0 = 1.0 / bsim4.BSIM4nf / (bsim4.BSIM4sa + 0.5 * Ldrn + i * (bsim4.BSIM4sd + Ldrn));
                    T1 = 1.0 / bsim4.BSIM4nf / (bsim4.BSIM4sb + 0.5 * Ldrn + i * (bsim4.BSIM4sd + Ldrn));
                    Inv_sa += T0;
                    Inv_sb += T1;
                }
                Inv_ODeff = Inv_sa + Inv_sb;
                rho = model.BSIM4ku0 / bsim4.pParam.BSIM4ku0temp * Inv_ODeff;
                T0 = (1.0 + rho) / (1.0 + bsim4.pParam.BSIM4rho_ref);
                bsim4.BSIM4u0temp = bsim4.pParam.BSIM4u0temp * T0;

                T1 = (1.0 + kvsat * rho) / (1.0 + kvsat * bsim4.pParam.BSIM4rho_ref);
                bsim4.BSIM4vsattemp = bsim4.pParam.BSIM4vsattemp * T1;

                OD_offset = Inv_ODeff - bsim4.pParam.BSIM4inv_od_ref;
                dvth0_lod = model.BSIM4kvth0 / bsim4.pParam.BSIM4kvth0 * OD_offset;
                dk2_lod = model.BSIM4stk2 / Math.Pow(bsim4.pParam.BSIM4kvth0, model.BSIM4lodk2) * OD_offset;
                deta0_lod = model.BSIM4steta0 / Math.Pow(bsim4.pParam.BSIM4kvth0, model.BSIM4lodeta0) * OD_offset;
                bsim4.BSIM4vth0 = bsim4.pParam.BSIM4vth0 + dvth0_lod;

                bsim4.BSIM4eta0 = bsim4.pParam.BSIM4eta0 + deta0_lod;
                bsim4.BSIM4k2 = bsim4.pParam.BSIM4k2 + dk2_lod;
            }
            else
            {
                bsim4.BSIM4u0temp = bsim4.pParam.BSIM4u0temp;
                bsim4.BSIM4vth0 = bsim4.pParam.BSIM4vth0;
                bsim4.BSIM4vsattemp = bsim4.pParam.BSIM4vsattemp;
                bsim4.BSIM4eta0 = bsim4.pParam.BSIM4eta0;
                bsim4.BSIM4k2 = bsim4.pParam.BSIM4k2;
            }

            /* Well Proximity Effect */
            if (model.BSIM4wpemod != 0)
            {
                if ((!bsim4.BSIM4sca.Given) && (!bsim4.BSIM4scb.Given) && (!bsim4.BSIM4scc.Given))
                {
                    if ((bsim4.BSIM4sc.Given) && (bsim4.BSIM4sc > 0.0))
                    {
                        T1 = bsim4.BSIM4sc + Wdrn;
                        T2 = 1.0 / model.BSIM4scref;
                        bsim4.BSIM4sca.Value = model.BSIM4scref * model.BSIM4scref / (bsim4.BSIM4sc * T1);
                        bsim4.BSIM4scb.Value = ((0.1 * bsim4.BSIM4sc + 0.01 * model.BSIM4scref) * Math.Exp(-10.0 * bsim4.BSIM4sc * T2) - (0.1 * T1 + 0.01 *
                                                    model.BSIM4scref) * Math.Exp(-10.0 * T1 * T2)) / Wdrn;
                        bsim4.BSIM4scc.Value = ((0.05 * bsim4.BSIM4sc + 0.0025 * model.BSIM4scref) * Math.Exp(-20.0 * bsim4.BSIM4sc * T2) - (0.05 * T1 +
                            0.0025 * model.BSIM4scref) * Math.Exp(-20.0 * T1 * T2)) / Wdrn;
                    }
                    else
                    {

                        CircuitWarning.Warning(this, "Warning: No WPE as none of SCA, SCB, SCC, SC is given and / or SC not positive.");
                    }
                }

                if (bsim4.BSIM4sca < 0.0)
                {

                    CircuitWarning.Warning(this, $"Warning: SCA = {bsim4.BSIM4sca} is negative. Set to 0.0.");
                    bsim4.BSIM4sca.Value = 0.0;
                }
                if (bsim4.BSIM4scb < 0.0)
                {

                    CircuitWarning.Warning(this, $"Warning: SCB = {bsim4.BSIM4scb} is negative. Set to 0.0.");
                    bsim4.BSIM4scb.Value = 0.0;
                }
                if (bsim4.BSIM4scc < 0.0)
                {

                    CircuitWarning.Warning(this, $"Warning: SCC = {bsim4.BSIM4scc} is negative. Set to 0.0.");
                    bsim4.BSIM4scc.Value = 0.0;
                }
                if (bsim4.BSIM4sc < 0.0)
                {

                    CircuitWarning.Warning(this, $"Warning: SC = {bsim4.BSIM4sc} is negative. Set to 0.0.");
                    bsim4.BSIM4sc.Value = 0.0;
                }
                /* 4.6.2 */
                sceff = bsim4.BSIM4sca + model.BSIM4web * bsim4.BSIM4scb + model.BSIM4wec * bsim4.BSIM4scc;
                bsim4.BSIM4vth0 += bsim4.pParam.BSIM4kvth0we * sceff;
                bsim4.BSIM4k2 += bsim4.pParam.BSIM4k2we * sceff;
                T3 = 1.0 + bsim4.pParam.BSIM4ku0we * sceff;
                if (T3 <= 0.0)
                {
                    T3 = 0.0;

                    CircuitWarning.Warning(this, $"Warning: ku0we = {bsim4.pParam.BSIM4ku0we} is negatively too high. Negative mobility! ");
                }
                bsim4.BSIM4u0temp *= T3;
            }

            /* adding delvto */
            bsim4.BSIM4vth0 += bsim4.BSIM4delvto;
            bsim4.BSIM4vfb = bsim4.pParam.BSIM4vfb + model.BSIM4type * bsim4.BSIM4delvto;

            /* Instance variables calculation */
            T3 = model.BSIM4type * bsim4.BSIM4vth0 - bsim4.BSIM4vfb - bsim4.pParam.BSIM4phi;
            T4 = T3 + T3;
            T5 = 2.5 * T3;
            bsim4.BSIM4vtfbphi1 = (model.BSIM4type == BSIM4v80.NMOS) ? T4 : T5;
            if (bsim4.BSIM4vtfbphi1 < 0.0)

                bsim4.BSIM4vtfbphi1 = 0.0;

            bsim4.BSIM4vtfbphi2 = 4.0 * T3;
            if (bsim4.BSIM4vtfbphi2 < 0.0)

                bsim4.BSIM4vtfbphi2 = 0.0;

            if (bsim4.BSIM4k2 < 0.0)
            {
                T0 = 0.5 * bsim4.pParam.BSIM4k1 / bsim4.BSIM4k2;
                bsim4.BSIM4vbsc = 0.9 * (bsim4.pParam.BSIM4phi - T0 * T0);
                if (bsim4.BSIM4vbsc > -3.0)
                    bsim4.BSIM4vbsc = -3.0;
                else if (bsim4.BSIM4vbsc < -30.0)

                    bsim4.BSIM4vbsc = -30.0;
            }
            else
                bsim4.BSIM4vbsc = -30.0;
            if (bsim4.BSIM4vbsc > bsim4.pParam.BSIM4vbm)
                bsim4.BSIM4vbsc = bsim4.pParam.BSIM4vbm;
            bsim4.BSIM4k2ox = bsim4.BSIM4k2 * model.toxe / model.BSIM4toxm;

            bsim4.BSIM4vfbzb = bsim4.pParam.BSIM4vfbzbfactor + model.BSIM4type * bsim4.BSIM4vth0;

            bsim4.BSIM4cgso = bsim4.pParam.BSIM4cgso;
            bsim4.BSIM4cgdo = bsim4.pParam.BSIM4cgdo;

            lnl = Math.Log(bsim4.pParam.BSIM4leff * 1.0e6);
            lnw = Math.Log(bsim4.pParam.BSIM4weff * 1.0e6);
            lnnf = Math.Log(bsim4.BSIM4nf);

            bodymode = 5;
            if ((!model.BSIM4rbps0.Given) || (!model.BSIM4rbpd0.Given))
                bodymode = 1;
            else
            if ((!model.BSIM4rbsbx0.Given && !model.BSIM4rbsby0.Given) || (!model.BSIM4rbdbx0.Given && !model.BSIM4rbdby0.Given))
                bodymode = 3;

            if (bsim4.BSIM4rbodyMod.Value == 2)
            {
                if (bodymode == 5)
                {
                    /* rbsbx = Math.Exp(Math.Log(model.BSIM4rbsbx0) + model.BSIM4rbsdbxl * lnl + model.BSIM4rbsdbxw * lnw + model.BSIM4rbsdbxnf *
						lnnf);
					rbsby = Math.Exp(Math.Log(model.BSIM4rbsby0) + model.BSIM4rbsdbyl * lnl + model.BSIM4rbsdbyw * lnw + model.BSIM4rbsdbynf *
						lnnf);
					*/
                    rbsbx = model.BSIM4rbsbx0 * Math.Exp(model.BSIM4rbsdbxl * lnl + model.BSIM4rbsdbxw * lnw + model.BSIM4rbsdbxnf * lnnf);
                    rbsby = model.BSIM4rbsby0 * Math.Exp(model.BSIM4rbsdbyl * lnl + model.BSIM4rbsdbyw * lnw + model.BSIM4rbsdbynf *
                        lnnf);
                    bsim4.BSIM4rbsb.Value = rbsbx * rbsby / (rbsbx + rbsby);

                    /* rbdbx = Math.Exp(Math.Log(model.BSIM4rbdbx0) + model.BSIM4rbsdbxl * lnl + model.BSIM4rbsdbxw * lnw + model.BSIM4rbsdbxnf *
						lnnf);
					rbdby = Math.Exp(Math.Log(model.BSIM4rbdby0) + model.BSIM4rbsdbyl * lnl + model.BSIM4rbsdbyw * lnw + model.BSIM4rbsdbynf *
						lnnf);
					*/

                    rbdbx = model.BSIM4rbdbx0 * Math.Exp(model.BSIM4rbsdbxl * lnl + model.BSIM4rbsdbxw * lnw + model.BSIM4rbsdbxnf * lnnf);
                    rbdby = model.BSIM4rbdby0 * Math.Exp(model.BSIM4rbsdbyl * lnl + model.BSIM4rbsdbyw * lnw + model.BSIM4rbsdbynf * lnnf);

                    bsim4.BSIM4rbdb.Value = rbdbx * rbdby / (rbdbx + rbdby);
                }

                if ((bodymode == 3) || (bodymode == 5))
                {
                    /* bsim4.BSIM4rbps.Value = Math.Exp(Math.Log(model.BSIM4rbps0) + model.BSIM4rbpsl * lnl + model.BSIM4rbpsw * lnw + model.BSIM4rbpsnf *
						lnnf);
					bsim4.BSIM4rbpd.Value = Math.Exp(Math.Log(model.BSIM4rbpd0) + model.BSIM4rbpdl * lnl + model.BSIM4rbpdw * lnw + model.BSIM4rbpdnf *
						lnnf);
					*/
                    bsim4.BSIM4rbps.Value = model.BSIM4rbps0 * Math.Exp(model.BSIM4rbpsl * lnl + model.BSIM4rbpsw * lnw + model.BSIM4rbpsnf * lnnf);
                    bsim4.BSIM4rbpd.Value = model.BSIM4rbpd0 * Math.Exp(model.BSIM4rbpdl * lnl + model.BSIM4rbpdw * lnw + model.BSIM4rbpdnf * lnnf);

                }

                /* rbpbx = Math.Exp(Math.Log(model.BSIM4rbpbx0) + model.BSIM4rbpbxl * lnl + model.BSIM4rbpbxw * lnw + model.BSIM4rbpbxnf *
					lnnf);
				rbpby = Math.Exp(Math.Log(model.BSIM4rbpby0) + model.BSIM4rbpbyl * lnl + model.BSIM4rbpbyw * lnw + model.BSIM4rbpbynf * lnnf);
				*/
                rbpbx = model.BSIM4rbpbx0 * Math.Exp(model.BSIM4rbpbxl * lnl + model.BSIM4rbpbxw * lnw + model.BSIM4rbpbxnf * lnnf);
                rbpby = model.BSIM4rbpby0 * Math.Exp(model.BSIM4rbpbyl * lnl + model.BSIM4rbpbyw * lnw + model.BSIM4rbpbynf * lnnf);

                bsim4.BSIM4rbpb.Value = rbpbx * rbpby / (rbpbx + rbpby);
            }

            if ((bsim4.BSIM4rbodyMod.Value == 1) || ((bsim4.BSIM4rbodyMod.Value == 2) && (bodymode == 5)))
            {
                if (bsim4.BSIM4rbdb < 1.0e-3)

                    bsim4.BSIM4grbdb = 1.0e3; /* in mho */
                else
                    bsim4.BSIM4grbdb = model.BSIM4gbmin + 1.0 / bsim4.BSIM4rbdb;
                if (bsim4.BSIM4rbpb < 1.0e-3)

                    bsim4.BSIM4grbpb = 1.0e3;
                else
                    bsim4.BSIM4grbpb = model.BSIM4gbmin + 1.0 / bsim4.BSIM4rbpb;
                if (bsim4.BSIM4rbps < 1.0e-3)

                    bsim4.BSIM4grbps = 1.0e3;
                else
                    bsim4.BSIM4grbps = model.BSIM4gbmin + 1.0 / bsim4.BSIM4rbps;
                if (bsim4.BSIM4rbsb < 1.0e-3)

                    bsim4.BSIM4grbsb = 1.0e3;
                else
                    bsim4.BSIM4grbsb = model.BSIM4gbmin + 1.0 / bsim4.BSIM4rbsb;
                if (bsim4.BSIM4rbpd < 1.0e-3)

                    bsim4.BSIM4grbpd = 1.0e3;
                else
                    bsim4.BSIM4grbpd = model.BSIM4gbmin + 1.0 / bsim4.BSIM4rbpd;

            }

            if ((bsim4.BSIM4rbodyMod.Value == 2) && (bodymode == 3))
            {
                bsim4.BSIM4grbdb = bsim4.BSIM4grbsb = model.BSIM4gbmin;
                if (bsim4.BSIM4rbpb < 1.0e-3)

                    bsim4.BSIM4grbpb = 1.0e3;
                else
                    bsim4.BSIM4grbpb = model.BSIM4gbmin + 1.0 / bsim4.BSIM4rbpb;
                if (bsim4.BSIM4rbps < 1.0e-3)

                    bsim4.BSIM4grbps = 1.0e3;
                else
                    bsim4.BSIM4grbps = model.BSIM4gbmin + 1.0 / bsim4.BSIM4rbps;
                if (bsim4.BSIM4rbpd < 1.0e-3)

                    bsim4.BSIM4grbpd = 1.0e3;
                else
                    bsim4.BSIM4grbpd = model.BSIM4gbmin + 1.0 / bsim4.BSIM4rbpd;
            }

            if ((bsim4.BSIM4rbodyMod.Value == 2) && (bodymode == 1))
            {
                bsim4.BSIM4grbdb = bsim4.BSIM4grbsb = model.BSIM4gbmin;
                bsim4.BSIM4grbps = bsim4.BSIM4grbpd = 1.0e3;
                if (bsim4.BSIM4rbpb < 1.0e-3)

                    bsim4.BSIM4grbpb = 1.0e3;
                else
                    bsim4.BSIM4grbpb = model.BSIM4gbmin + 1.0 / bsim4.BSIM4rbpb;
            }

            /* 
			* Process geomertry dependent parasitics
			*/

            bsim4.BSIM4grgeltd = model.BSIM4rshg * (bsim4.BSIM4xgw + bsim4.pParam.BSIM4weffCJ / 3.0 / bsim4.BSIM4ngcon) / (bsim4.BSIM4ngcon * bsim4.BSIM4nf * (Lnew -
                model.BSIM4xgl));
            if (bsim4.BSIM4grgeltd > 0.0)
                bsim4.BSIM4grgeltd = 1.0 / bsim4.BSIM4grgeltd;
            else
            {
                bsim4.BSIM4grgeltd = 1.0e3; /* mho */
                if (bsim4.BSIM4rgateMod != 0) CircuitWarning.Warning(this, "Warning: The gate conductance reset to 1.0e3 mho.");
            }

            DMCGeff = model.BSIM4dmcg - model.BSIM4dmcgt;
            DMCIeff = model.BSIM4dmci;
            DMDGeff = model.BSIM4dmdg - model.BSIM4dmcgt;

            /* if (bsim4.BSIM4sourcePerimeter.Given)
			{
				if (model.BSIM4perMod.Value == 0)
				bsim4.BSIM4Pseff = bsim4.BSIM4sourcePerimeter;
				else
				bsim4.BSIM4Pseff = bsim4.BSIM4sourcePerimeter - bsim4.pParam.BSIM4weffCJ * bsim4.BSIM4nf;
			}
			else
			bsim4.BSIM4PAeffGeo(bsim4.BSIM4nf, bsim4.BSIM4geoMod, bsim4.BSIM4min, 
			bsim4.pParam.BSIM4weffCJ, DMCGeff, DMCIeff, DMDGeff, 
			&(bsim4.BSIM4Pseff), &dumPd, &dumAs, &dumAd);
			if (bsim4.BSIM4Pseff< 0.0)
			/ 4.6.2 / 
			bsim4.BSIM4Pseff = 0.0; */

            /* New Diode Model v4.7 */
            if (bsim4.BSIM4sourcePerimeter.Given)
            {
                /* given */
                if (bsim4.BSIM4sourcePerimeter.Value == 0.0)
                    bsim4.BSIM4Pseff = 0.0;
                else if (bsim4.BSIM4sourcePerimeter < 0.0)
                {

                    CircuitWarning.Warning(this, "Warning: Source Perimeter is specified as negative, it is set to zero.");
                    bsim4.BSIM4Pseff = 0.0;
                }
                else
                {
                    if (model.BSIM4perMod.Value == 0)
                        bsim4.BSIM4Pseff = bsim4.BSIM4sourcePerimeter;
                    else
                        bsim4.BSIM4Pseff = bsim4.BSIM4sourcePerimeter - bsim4.pParam.BSIM4weffCJ * bsim4.BSIM4nf;
                }
            }
            else
            {
                /* not given */
                double iseff;
                bsim4.BSIM4PAeffGeo(bsim4.BSIM4nf, bsim4.BSIM4geoMod, bsim4.BSIM4min,
                bsim4.pParam.BSIM4weffCJ, DMCGeff, DMCIeff, DMDGeff,
                out iseff, out dumPd, out dumAs, out dumAd);
                bsim4.BSIM4Pseff = iseff;
            }

            if (bsim4.BSIM4Pseff < 0.0)
            {
                /* v4.7 final check */
                bsim4.BSIM4Pseff = 0.0;

                CircuitWarning.Warning(this, "Warning: Pseff is negative, it is set to zero.");
            }
            /* if (bsim4.BSIM4drainPerimeter.Given)
			{
				if (model.BSIM4perMod.Value == 0)
				bsim4.BSIM4Pdeff = bsim4.BSIM4drainPerimeter;
				else
				bsim4.BSIM4Pdeff = bsim4.BSIM4drainPerimeter - bsim4.pParam.BSIM4weffCJ * bsim4.BSIM4nf;
			}
			else
			bsim4.BSIM4PAeffGeo(bsim4.BSIM4nf, bsim4.BSIM4geoMod, bsim4.BSIM4min, 
			bsim4.pParam.BSIM4weffCJ, DMCGeff, DMCIeff, DMDGeff, 
			&dumPs, &(bsim4.BSIM4Pdeff), &dumAs, &dumAd);
			if (bsim4.BSIM4Pdeff< 0.0)
			/ 4.6.2 / 
			bsim4.BSIM4Pdeff = 0.0; */

            if (bsim4.BSIM4drainPerimeter.Given)
            {
                /* given */
                if (bsim4.BSIM4drainPerimeter.Value == 0.0)
                    bsim4.BSIM4Pdeff = 0.0;
                else if (bsim4.BSIM4drainPerimeter < 0.0)
                {

                    CircuitWarning.Warning(this, "Warning: Drain Perimeter is specified as negative, it is set to zero.");
                    bsim4.BSIM4Pdeff = 0.0;
                }
                else
                {
                    if (model.BSIM4perMod.Value == 0)
                        bsim4.BSIM4Pdeff = bsim4.BSIM4drainPerimeter;
                    else
                        bsim4.BSIM4Pdeff = bsim4.BSIM4drainPerimeter - bsim4.pParam.BSIM4weffCJ * bsim4.BSIM4nf;
                }
            }
            else
            {
                /* not given */
                double ideff;
                bsim4.BSIM4PAeffGeo(bsim4.BSIM4nf, bsim4.BSIM4geoMod, bsim4.BSIM4min,
                    bsim4.pParam.BSIM4weffCJ, DMCGeff, DMCIeff, DMDGeff,
                    out dumPs, out ideff, out dumAs, out dumAd);
                bsim4.BSIM4Pdeff = ideff;
            }

            if (bsim4.BSIM4Pdeff < 0.0)
            {
                bsim4.BSIM4Pdeff = 0.0; /* New Diode v4.7 */
                CircuitWarning.Warning(this, "Warning: Pdeff is negative, it is set to zero.");
            }
            if (bsim4.BSIM4sourceArea.Given)
                bsim4.BSIM4Aseff = bsim4.BSIM4sourceArea;
            else
            {
                double iaseff;
                bsim4.BSIM4PAeffGeo(bsim4.BSIM4nf, bsim4.BSIM4geoMod, bsim4.BSIM4min,
                bsim4.pParam.BSIM4weffCJ, DMCGeff, DMCIeff, DMDGeff,
                out dumPs, out dumPd, out iaseff, out dumAd);
                bsim4.BSIM4Aseff = iaseff;
            }
            if (bsim4.BSIM4Aseff < 0.0)
            {
                bsim4.BSIM4Aseff = 0.0; /* v4.7 */

                CircuitWarning.Warning(this, "Warning: Aseff is negative, it is set to zero.");
            }
            if (bsim4.BSIM4drainArea.Given)
                bsim4.BSIM4Adeff = bsim4.BSIM4drainArea;
            else
            {
                double iadeff;
                bsim4.BSIM4PAeffGeo(bsim4.BSIM4nf, bsim4.BSIM4geoMod, bsim4.BSIM4min,
                bsim4.pParam.BSIM4weffCJ, DMCGeff, DMCIeff, DMDGeff,
                out dumPs, out dumPd, out dumAs, out iadeff);
                bsim4.BSIM4Adeff = iadeff;
            }
            if (bsim4.BSIM4Adeff < 0.0)
            {
                bsim4.BSIM4Adeff = 0.0; /* v4.7 */
                CircuitWarning.Warning(this, "Warning: Adeff is negative, it is set to zero.");
            }
            /* Processing S / D resistance and conductance below */
            if (bsim4.BSIM4sNodePrime != bsim4.BSIM4sNode)
            {
                bsim4.BSIM4sourceConductance = 0.0;
                if (bsim4.BSIM4sourceSquares.Given)
                {
                    bsim4.BSIM4sourceConductance = model.BSIM4sheetResistance * bsim4.BSIM4sourceSquares;
                }
                else if (bsim4.BSIM4rgeoMod > 0)
                {
                    double igsrc;
                    bsim4.BSIM4RdseffGeo(bsim4.BSIM4nf, bsim4.BSIM4geoMod,
                        bsim4.BSIM4rgeoMod, bsim4.BSIM4min,
                        bsim4.pParam.BSIM4weffCJ, model.BSIM4sheetResistance,
                        DMCGeff, DMCIeff, DMDGeff, 1, out igsrc);
                    bsim4.BSIM4sourceConductance = igsrc;
                }
                else
                {
                    bsim4.BSIM4sourceConductance = 0.0;
                }

                if (bsim4.BSIM4sourceConductance > 0.0)
                    bsim4.BSIM4sourceConductance = 1.0 / bsim4.BSIM4sourceConductance;
                else
                {
                    bsim4.BSIM4sourceConductance = 1.0e3; /* mho */

                    CircuitWarning.Warning(this, "Warning: Source conductance reset to 1.0e3 mho.");
                }
            }
            else
            {
                bsim4.BSIM4sourceConductance = 0.0;
            }

            if (bsim4.BSIM4dNodePrime != bsim4.BSIM4dNode)
            {
                bsim4.BSIM4drainConductance = 0.0;
                if (bsim4.BSIM4drainSquares.Given)
                {
                    bsim4.BSIM4drainConductance = model.BSIM4sheetResistance * bsim4.BSIM4drainSquares;
                }
                else if (bsim4.BSIM4rgeoMod > 0)
                {
                    double igdrn;
                    bsim4.BSIM4RdseffGeo(bsim4.BSIM4nf, bsim4.BSIM4geoMod,
                        bsim4.BSIM4rgeoMod, bsim4.BSIM4min,
                        bsim4.pParam.BSIM4weffCJ, model.BSIM4sheetResistance,
                        DMCGeff, DMCIeff, DMDGeff, 0, out igdrn);
                    bsim4.BSIM4drainConductance = igdrn;
                }
                else
                {
                    bsim4.BSIM4drainConductance = 0.0;
                }

                if (bsim4.BSIM4drainConductance > 0.0)
                    bsim4.BSIM4drainConductance = 1.0 / bsim4.BSIM4drainConductance;
                else
                {
                    bsim4.BSIM4drainConductance = 1.0e3; /* mho */
                    CircuitWarning.Warning(this, "Warning: Drain conductance reset to 1.0e3 mho.");
                }
            }
            else
            {
                bsim4.BSIM4drainConductance = 0.0;
            }

            /* End of Rsd processing */

            Nvtms = model.BSIM4vtm * model.BSIM4SjctEmissionCoeff;
            if ((bsim4.BSIM4Aseff <= 0.0) && (bsim4.BSIM4Pseff <= 0.0))
            {
                SourceSatCurrent = 0.0; /* v4.7 */
                                        /* SourceSatCurrent = 1.0e-14; */
            }
            else
            {
                SourceSatCurrent = bsim4.BSIM4Aseff * model.BSIM4SjctTempSatCurDensity + bsim4.BSIM4Pseff * model.BSIM4SjctSidewallTempSatCurDensity +

                    bsim4.pParam.BSIM4weffCJ * bsim4.BSIM4nf * model.BSIM4SjctGateSidewallTempSatCurDensity;
            }
            if (SourceSatCurrent > 0.0)
            {
                switch (model.BSIM4dioMod.Value)
                {
                    case 0:
                        if ((model.BSIM4bvs / Nvtms) > Transistor.EXP_THRESHOLD)
                            bsim4.BSIM4XExpBVS = model.BSIM4xjbvs * Transistor.MIN_EXP;
                        else
                            bsim4.BSIM4XExpBVS = model.BSIM4xjbvs * Math.Exp(-model.BSIM4bvs / Nvtms);
                        break;
                    case 1:
                        bsim4.BSIM4vjsmFwd = BSIM4DioIjthVjmEval(Nvtms, model.BSIM4ijthsfwd, SourceSatCurrent, 0.0);
                        bsim4.BSIM4IVjsmFwd = SourceSatCurrent * Math.Exp(bsim4.BSIM4vjsmFwd / Nvtms);
                        break;
                    case 2:
                        if ((model.BSIM4bvs / Nvtms) > Transistor.EXP_THRESHOLD)
                        {
                            bsim4.BSIM4XExpBVS = model.BSIM4xjbvs * Transistor.MIN_EXP;
                            tmp = Transistor.MIN_EXP;
                        }
                        else
                        {
                            bsim4.BSIM4XExpBVS = Math.Exp(-model.BSIM4bvs / Nvtms);
                            tmp = bsim4.BSIM4XExpBVS;
                            bsim4.BSIM4XExpBVS *= model.BSIM4xjbvs;
                        }


                        bsim4.BSIM4vjsmFwd = BSIM4DioIjthVjmEval(Nvtms, model.BSIM4ijthsfwd, SourceSatCurrent, bsim4.BSIM4XExpBVS);
                        T0 = Math.Exp(bsim4.BSIM4vjsmFwd / Nvtms);
                        bsim4.BSIM4IVjsmFwd = SourceSatCurrent * (T0 - bsim4.BSIM4XExpBVS / T0 + bsim4.BSIM4XExpBVS - 1.0);
                        bsim4.BSIM4SslpFwd = SourceSatCurrent * (T0 + bsim4.BSIM4XExpBVS / T0) / Nvtms;

                        T2 = model.BSIM4ijthsrev / SourceSatCurrent;
                        if (T2 < 1.0)
                        {
                            T2 = 10.0;
                            CircuitWarning.Warning(this, "Warning: ijthsrev too small and set to 10 times IsbSat.");
                        }
                        bsim4.BSIM4vjsmRev = -model.BSIM4bvs - Nvtms * Math.Log((T2 - 1.0) / model.BSIM4xjbvs);
                        T1 = model.BSIM4xjbvs * Math.Exp(-(model.BSIM4bvs + bsim4.BSIM4vjsmRev) / Nvtms);
                        bsim4.BSIM4IVjsmRev = SourceSatCurrent * (1.0 + T1);
                        bsim4.BSIM4SslpRev = -SourceSatCurrent * T1 / Nvtms;
                        break;
                    default:
                        CircuitWarning.Warning(this, $"Specified dioMod = {model.BSIM4dioMod} not matched");
                        break;
                }
            }

            Nvtmd = model.BSIM4vtm * model.BSIM4DjctEmissionCoeff;
            if ((bsim4.BSIM4Adeff <= 0.0) && (bsim4.BSIM4Pdeff <= 0.0))
            {
                /* DrainSatCurrent = 1.0e-14; 	v4.7 */
                DrainSatCurrent = 0.0;
            }
            else
            {
                DrainSatCurrent = bsim4.BSIM4Adeff * model.BSIM4DjctTempSatCurDensity + bsim4.BSIM4Pdeff * model.BSIM4DjctSidewallTempSatCurDensity +

                    bsim4.pParam.BSIM4weffCJ * bsim4.BSIM4nf * model.BSIM4DjctGateSidewallTempSatCurDensity;
            }
            if (DrainSatCurrent > 0.0)
            {
                switch (model.BSIM4dioMod.Value)
                {
                    case 0:
                        if ((model.BSIM4bvd / Nvtmd) > Transistor.EXP_THRESHOLD)
                            bsim4.BSIM4XExpBVD = model.BSIM4xjbvd * Transistor.MIN_EXP;
                        else
                            bsim4.BSIM4XExpBVD = model.BSIM4xjbvd * Math.Exp(-model.BSIM4bvd / Nvtmd);
                        break;
                    case 1:
                        bsim4.BSIM4vjdmFwd = BSIM4DioIjthVjmEval(Nvtmd, model.BSIM4ijthdfwd, DrainSatCurrent, 0.0);
                        bsim4.BSIM4IVjdmFwd = DrainSatCurrent * Math.Exp(bsim4.BSIM4vjdmFwd / Nvtmd);
                        break;
                    case 2:
                        if ((model.BSIM4bvd / Nvtmd) > Transistor.EXP_THRESHOLD)
                        {
                            bsim4.BSIM4XExpBVD = model.BSIM4xjbvd * Transistor.MIN_EXP;
                            tmp = Transistor.MIN_EXP;
                        }
                        else
                        {
                            bsim4.BSIM4XExpBVD = Math.Exp(-model.BSIM4bvd / Nvtmd);
                            tmp = bsim4.BSIM4XExpBVD;
                            bsim4.BSIM4XExpBVD *= model.BSIM4xjbvd;
                        }


                        bsim4.BSIM4vjdmFwd = BSIM4DioIjthVjmEval(Nvtmd, model.BSIM4ijthdfwd, DrainSatCurrent, bsim4.BSIM4XExpBVD);
                        T0 = Math.Exp(bsim4.BSIM4vjdmFwd / Nvtmd);
                        bsim4.BSIM4IVjdmFwd = DrainSatCurrent * (T0 - bsim4.BSIM4XExpBVD / T0 + bsim4.BSIM4XExpBVD - 1.0);
                        bsim4.BSIM4DslpFwd = DrainSatCurrent * (T0 + bsim4.BSIM4XExpBVD / T0) / Nvtmd;

                        T2 = model.BSIM4ijthdrev / DrainSatCurrent;
                        if (T2 < 1.0)
                        {
                            T2 = 10.0;

                            CircuitWarning.Warning(this, "Warning: ijthdrev too small and set to 10 times IdbSat.");
                        }
                        bsim4.BSIM4vjdmRev = -model.BSIM4bvd - Nvtmd * Math.Log((T2 - 1.0) / model.BSIM4xjbvd); /* bugfix */
                        T1 = model.BSIM4xjbvd * Math.Exp(-(model.BSIM4bvd + bsim4.BSIM4vjdmRev) / Nvtmd);
                        bsim4.BSIM4IVjdmRev = DrainSatCurrent * (1.0 + T1);
                        bsim4.BSIM4DslpRev = -DrainSatCurrent * T1 / Nvtmd;
                        break;
                    default:
                        CircuitWarning.Warning(this, $"Specified dioMod = {model.BSIM4dioMod} not matched");
                        break;
                }
            }

            /* GEDL current reverse bias */
            T0 = (model.TRatio - 1.0);
            T7 = model.Eg0 / model.BSIM4vtm * T0;
            T9 = model.BSIM4xtss * T7;
            T1 = Dexp(T9);
            T9 = model.BSIM4xtsd * T7;
            T2 = Dexp(T9);
            T9 = model.BSIM4xtssws * T7;
            T3 = Dexp(T9);
            T9 = model.BSIM4xtsswd * T7;
            T4 = Dexp(T9);
            T9 = model.BSIM4xtsswgs * T7;
            T5 = Dexp(T9);
            T9 = model.BSIM4xtsswgd * T7;
            T6 = Dexp(T9);

            /* IBM TAT */
            if (model.BSIM4jtweff < 0.0)
            {
                model.BSIM4jtweff.Value = 0.0;
                CircuitWarning.Warning(this, "TAT width dependence effect is negative. Jtweff is clamped to zero.");
            }
            T11 = Math.Sqrt(model.BSIM4jtweff / bsim4.pParam.BSIM4weffCJ) + 1.0;

            T10 = bsim4.pParam.BSIM4weffCJ * bsim4.BSIM4nf;
            bsim4.BSIM4SjctTempRevSatCur = T1 * bsim4.BSIM4Aseff * model.BSIM4jtss;
            bsim4.BSIM4DjctTempRevSatCur = T2 * bsim4.BSIM4Adeff * model.BSIM4jtsd;
            bsim4.BSIM4SswTempRevSatCur = T3 * bsim4.BSIM4Pseff * model.BSIM4jtssws;
            bsim4.BSIM4DswTempRevSatCur = T4 * bsim4.BSIM4Pdeff * model.BSIM4jtsswd;
            bsim4.BSIM4SswgTempRevSatCur = T5 * T10 * T11 * model.BSIM4jtsswgs;
            bsim4.BSIM4DswgTempRevSatCur = T6 * T10 * T11 * model.BSIM4jtsswgd;

            if (model.BSIM4mtrlMod != 0 && model.BSIM4mtrlCompatMod.Value == 0)
            {
                /* Calculate TOXP from EOT */
                /* Calculate Vgs_eff @ Vgs = VDD with Poly Depletion Effect */
                double Vtm0eot = Transistor.KboQ * model.BSIM4tempeot;
                Vtmeot = Vtm0eot;
                vbieot = Vtm0eot * Math.Log(bsim4.pParam.BSIM4nsd * bsim4.pParam.BSIM4ndep / (model.ni * model.ni));
                phieot = Vtm0eot * Math.Log(bsim4.pParam.BSIM4ndep / model.ni) + bsim4.pParam.BSIM4phin + 0.4;
                tmp2 = bsim4.BSIM4vfb + phieot;
                vddeot = model.BSIM4type * model.BSIM4vddeot;
                T0 = model.BSIM4epsrgate * Transistor.EPS0;
                if ((bsim4.pParam.BSIM4ngate > 1.0e18) && (bsim4.pParam.BSIM4ngate < 1.0e25) && (vddeot > tmp2) && (T0 != 0))
                {
                    T1 = 1.0e6 * Circuit.CHARGE * T0 * bsim4.pParam.BSIM4ngate / (model.BSIM4coxe * model.BSIM4coxe);
                    T8 = vddeot - tmp2;
                    T4 = Math.Sqrt(1.0 + 2.0 * T8 / T1);
                    T2 = 2.0 * T8 / (T4 + 1.0);
                    T3 = 0.5 * T2 * T2 / T1;
                    T7 = 1.12 - T3 - 0.05;
                    T6 = Math.Sqrt(T7 * T7 + 0.224);
                    T5 = 1.12 - 0.5 * (T7 + T6);
                    Vgs_eff = vddeot - T5;
                }
                else
                    Vgs_eff = vddeot;

                /* Calculate Vth @ Vds = Vbs = 0 */
                double V0 = vbieot - phieot;
                lt1 = model.BSIM4factor1 * bsim4.pParam.BSIM4sqrtXdep0;
                ltw = lt1;
                T0 = bsim4.pParam.BSIM4dvt1 * model.BSIM4leffeot / lt1;
                if (T0 < Transistor.EXP_THRESHOLD)
                {
                    T1 = Math.Exp(T0);
                    T2 = T1 - 1.0;
                    T3 = T2 * T2;
                    T4 = T3 + 2.0 * T1 * Transistor.MIN_EXP;
                    Theta0 = T1 / T4;
                }
                else
                    Theta0 = 1.0 / (Transistor.MAX_EXP - 2.0);
                Delt_vth = bsim4.pParam.BSIM4dvt0 * Theta0 * V0;
                T0 = bsim4.pParam.BSIM4dvt1w * model.BSIM4weffeot * model.BSIM4leffeot / ltw;
                if (T0 < Transistor.EXP_THRESHOLD)
                {
                    T1 = Math.Exp(T0);
                    T2 = T1 - 1.0;
                    T3 = T2 * T2;
                    T4 = T3 + 2.0 * T1 * Transistor.MIN_EXP;
                    T5 = T1 / T4;
                }
                else
                    T5 = 1.0 / (Transistor.MAX_EXP - 2.0); /* 3.0 * Transistor.MIN_EXP omitted */
                T2 = bsim4.pParam.BSIM4dvt0w * T5 * V0;
                TempRatioeot = model.BSIM4tempeot / model.BSIM4tnom - 1.0;
                T0 = Math.Sqrt(1.0 + bsim4.pParam.BSIM4lpe0 / model.BSIM4leffeot);
                T1 = bsim4.pParam.BSIM4k1ox * (T0 - 1.0) * Math.Sqrt(phieot) + (bsim4.pParam.BSIM4kt1 + bsim4.pParam.BSIM4kt1l / model.BSIM4leffeot) *
                   TempRatioeot;
                Vth_NarrowW = model.toxe * phieot / (model.BSIM4weffeot + bsim4.pParam.BSIM4w0);
                Lpe_Vb = Math.Sqrt(1.0 + bsim4.pParam.BSIM4lpeb / model.BSIM4leffeot);
                Vth = model.BSIM4type * bsim4.BSIM4vth0 + (bsim4.pParam.BSIM4k1ox - bsim4.pParam.BSIM4k1) * Math.Sqrt(phieot) * Lpe_Vb - Delt_vth - T2 +
                    bsim4.pParam.BSIM4k3 * Vth_NarrowW + T1;

                /* Calculate n */
                tmp1 = model.epssub / bsim4.pParam.BSIM4Xdep0;
                tmp2 = bsim4.pParam.BSIM4nfactor * tmp1;
                tmp3 = (tmp2 + bsim4.pParam.BSIM4cdsc * Theta0 + bsim4.pParam.BSIM4cit) / model.BSIM4coxe;
                if (tmp3 >= -0.5)
                    n = 1.0 + tmp3;
                else
                {
                    T0 = 1.0 / (3.0 + 8.0 * tmp3);
                    n = (1.0 + 3.0 * tmp3) * T0;
                }

                /* Vth correction for Pocket implant */
                if (bsim4.pParam.BSIM4dvtp0 > 0.0)
                {
                    T3 = model.BSIM4leffeot + bsim4.pParam.BSIM4dvtp0 * 2.0;
                    if (model.BSIM4tempMod < 2)
                        T4 = Vtmeot * Math.Log(model.BSIM4leffeot / T3);
                    else
                        T4 = Vtm0eot * Math.Log(model.BSIM4leffeot / T3);
                    Vth -= n * T4;
                }
                Vgsteff = Vgs_eff - Vth;
                /* calculating Toxp */
                T3 = model.BSIM4type * bsim4.BSIM4vth0 - bsim4.BSIM4vfb - phieot;
                T4 = T3 + T3;
                T5 = 2.5 * T3;

                vtfbphi2eot = 4.0 * T3;
                if (vtfbphi2eot < 0.0)

                    vtfbphi2eot = 0.0;

                niter = 0;
                toxpf = model.toxe;
                do
                {
                    toxpi = toxpf;
                    tmp2 = 2.0e8 * toxpf;
                    T0 = (Vgsteff + vtfbphi2eot) / tmp2;
                    T1 = 1.0 + Math.Exp(model.BSIM4bdos * 0.7 * Math.Log(T0));
                    Tcen = model.BSIM4ados * 1.9e-9 / T1;
                    toxpf = model.toxe - model.epsrox / model.BSIM4epsrsub * Tcen;
                    niter++;
                } while ((niter <= 4) && (Math.Abs(toxpf - toxpi) > 1e-12));
                bsim4.BSIM4toxp = toxpf;
                bsim4.BSIM4coxp = model.epsrox * Transistor.EPS0 / model.BSIM4toxp;
            }
            else
            {
                bsim4.BSIM4toxp = model.BSIM4toxp;
                bsim4.BSIM4coxp = model.BSIM4coxp;
            }

            if (bsim4.BSIM4checkModel(ckt))
                throw new CircuitException($"Fatal error(s) detected during bsim4.BSIM4v8.0 parameter checking for {bsim4.Name} in model {model.Name}");
        }
    }
}
