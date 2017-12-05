using System;
using SpiceSharp.Behaviors;
using SpiceSharp.Diagnostics;
using SpiceSharp.Components.Transistors;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// Temperature behaviour for a <see cref="BSIM3v24"/>
    /// </summary>
    public class BSIM3v24TemperatureBehavior : CircuitObjectBehaviorTemperature
    {
        /// <summary>
        /// Execute the behaviour
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Temperature(Circuit ckt)
        {
            var bsim3 = ComponentTyped<BSIM3v24>();
            var model = bsim3.Model as BSIM3v24Model;

            double Ldrn, Wdrn, T0, T1, tmp1, tmp2, T2, T3, Inv_L, Inv_W, Inv_LW, tmp, T4, T5, tmp3,
                Nvtm, SourceSatCurrent, DrainSatCurrent;

            Tuple<double, double> mysize = new Tuple<double, double>(bsim3.BSIM3w, bsim3.BSIM3l);
            if (model.Sizes.ContainsKey(mysize))
                bsim3.pParam = model.Sizes[mysize];
            else
            {
                bsim3.pParam = new BSIM3SizeDependParam();
                model.Sizes.Add(mysize, bsim3.pParam);

                Ldrn = bsim3.BSIM3l;
                Wdrn = bsim3.BSIM3w;

                T0 = Math.Pow(Ldrn, model.BSIM3Lln);
                T1 = Math.Pow(Wdrn, model.BSIM3Lwn);
                tmp1 = model.BSIM3Ll / T0 + model.BSIM3Lw / T1 + model.BSIM3Lwl / (T0 * T1);
                bsim3.pParam.BSIM3dl = model.BSIM3Lint + tmp1;
                tmp2 = model.BSIM3Llc / T0 + model.BSIM3Lwc / T1 + model.BSIM3Lwlc / (T0 * T1);
                bsim3.pParam.BSIM3dlc = model.BSIM3dlc + tmp2;

                T2 = Math.Pow(Ldrn, model.BSIM3Wln);
                T3 = Math.Pow(Wdrn, model.BSIM3Wwn);
                tmp1 = model.BSIM3Wl / T2 + model.BSIM3Ww / T3 + model.BSIM3Wwl / (T2 * T3);
                bsim3.pParam.BSIM3dw = model.BSIM3Wint + tmp1;
                tmp2 = model.BSIM3Wlc / T2 + model.BSIM3Wwc / T3 + model.BSIM3Wwlc / (T2 * T3);
                bsim3.pParam.BSIM3dwc = model.BSIM3dwc + tmp2;

                bsim3.pParam.BSIM3leff = bsim3.BSIM3l - 2.0 * bsim3.pParam.BSIM3dl;
                if (bsim3.pParam.BSIM3leff <= 0.0)
                    throw new CircuitException($"bsim3.BSIM3v24: mosfet {bsim3.Name}, model {model.Name}: Effective channel length <= 0");

                bsim3.pParam.BSIM3weff = bsim3.BSIM3w - 2.0 * bsim3.pParam.BSIM3dw;
                if (bsim3.pParam.BSIM3weff <= 0.0)
                    throw new CircuitException($"bsim3.BSIM3v24: mosfet {bsim3.Name}, model {model.Name}: Effective channel width <= 0");

                bsim3.pParam.BSIM3leffCV = bsim3.BSIM3l - 2.0 * bsim3.pParam.BSIM3dlc;
                if (bsim3.pParam.BSIM3leffCV <= 0.0)
                    throw new CircuitException($"bsim3.BSIM3v24: mosfet {bsim3.Name}, model {model.Name}: Effective channel length for C-V <= 0");

                bsim3.pParam.BSIM3weffCV = bsim3.BSIM3w - 2.0 * bsim3.pParam.BSIM3dwc;
                if (bsim3.pParam.BSIM3weffCV <= 0.0)
                    throw new CircuitException($"bsim3.BSIM3v24: mosfet {bsim3.Name}, model {model.Name}: Effective channel width for C-V <= 0");

                if (model.BSIM3binUnit.Value == 1)
                {
                    Inv_L = 1.0e-6 / bsim3.pParam.BSIM3leff;
                    Inv_W = 1.0e-6 / bsim3.pParam.BSIM3weff;
                    Inv_LW = 1.0e-12 / (bsim3.pParam.BSIM3leff * bsim3.pParam.BSIM3weff);
                }
                else
                {
                    Inv_L = 1.0 / bsim3.pParam.BSIM3leff;
                    Inv_W = 1.0 / bsim3.pParam.BSIM3weff;
                    Inv_LW = 1.0 / (bsim3.pParam.BSIM3leff * bsim3.pParam.BSIM3weff);
                }
                bsim3.pParam.BSIM3cdsc = model.BSIM3cdsc + model.BSIM3lcdsc * Inv_L + model.BSIM3wcdsc * Inv_W + model.BSIM3pcdsc * Inv_LW;
                bsim3.pParam.BSIM3cdscb = model.BSIM3cdscb + model.BSIM3lcdscb * Inv_L + model.BSIM3wcdscb * Inv_W + model.BSIM3pcdscb * Inv_LW;

                bsim3.pParam.BSIM3cdscd = model.BSIM3cdscd + model.BSIM3lcdscd * Inv_L + model.BSIM3wcdscd * Inv_W + model.BSIM3pcdscd * Inv_LW;

                bsim3.pParam.BSIM3cit = model.BSIM3cit + model.BSIM3lcit * Inv_L + model.BSIM3wcit * Inv_W + model.BSIM3pcit * Inv_LW;
                bsim3.pParam.BSIM3nfactor = model.BSIM3nfactor + model.BSIM3lnfactor * Inv_L + model.BSIM3wnfactor * Inv_W + model.BSIM3pnfactor *
                  Inv_LW;
                bsim3.pParam.BSIM3xj = model.BSIM3xj + model.BSIM3lxj * Inv_L + model.BSIM3wxj * Inv_W + model.BSIM3pxj * Inv_LW;
                bsim3.pParam.BSIM3vsat = model.BSIM3vsat + model.BSIM3lvsat * Inv_L + model.BSIM3wvsat * Inv_W + model.BSIM3pvsat * Inv_LW;
                bsim3.pParam.BSIM3at = model.BSIM3at + model.BSIM3lat * Inv_L + model.BSIM3wat * Inv_W + model.BSIM3pat * Inv_LW;
                bsim3.pParam.BSIM3a0 = model.BSIM3a0 + model.BSIM3la0 * Inv_L + model.BSIM3wa0 * Inv_W + model.BSIM3pa0 * Inv_LW;

                bsim3.pParam.BSIM3ags = model.BSIM3ags + model.BSIM3lags * Inv_L + model.BSIM3wags * Inv_W + model.BSIM3pags * Inv_LW;

                bsim3.pParam.BSIM3a1 = model.BSIM3a1 + model.BSIM3la1 * Inv_L + model.BSIM3wa1 * Inv_W + model.BSIM3pa1 * Inv_LW;
                bsim3.pParam.BSIM3a2 = model.BSIM3a2 + model.BSIM3la2 * Inv_L + model.BSIM3wa2 * Inv_W + model.BSIM3pa2 * Inv_LW;
                bsim3.pParam.BSIM3keta = model.BSIM3keta + model.BSIM3lketa * Inv_L + model.BSIM3wketa * Inv_W + model.BSIM3pketa * Inv_LW;
                bsim3.pParam.BSIM3nsub = model.BSIM3nsub + model.BSIM3lnsub * Inv_L + model.BSIM3wnsub * Inv_W + model.BSIM3pnsub * Inv_LW;
                bsim3.pParam.BSIM3npeak = model.BSIM3npeak + model.BSIM3lnpeak * Inv_L + model.BSIM3wnpeak * Inv_W + model.BSIM3pnpeak * Inv_LW;
                bsim3.pParam.BSIM3ngate = model.BSIM3ngate + model.BSIM3lngate * Inv_L + model.BSIM3wngate * Inv_W + model.BSIM3pngate * Inv_LW;
                bsim3.pParam.BSIM3gamma1 = model.BSIM3gamma1 + model.BSIM3lgamma1 * Inv_L + model.BSIM3wgamma1 * Inv_W + model.BSIM3pgamma1 * Inv_LW;
                bsim3.pParam.BSIM3gamma2 = model.BSIM3gamma2 + model.BSIM3lgamma2 * Inv_L + model.BSIM3wgamma2 * Inv_W + model.BSIM3pgamma2 * Inv_LW;
                bsim3.pParam.BSIM3vbx = model.BSIM3vbx + model.BSIM3lvbx * Inv_L + model.BSIM3wvbx * Inv_W + model.BSIM3pvbx * Inv_LW;
                bsim3.pParam.BSIM3vbm = model.BSIM3vbm + model.BSIM3lvbm * Inv_L + model.BSIM3wvbm * Inv_W + model.BSIM3pvbm * Inv_LW;
                bsim3.pParam.BSIM3xt = model.BSIM3xt + model.BSIM3lxt * Inv_L + model.BSIM3wxt * Inv_W + model.BSIM3pxt * Inv_LW;
                bsim3.pParam.BSIM3vfb = model.BSIM3vfb + model.BSIM3lvfb * Inv_L + model.BSIM3wvfb * Inv_W + model.BSIM3pvfb * Inv_LW;
                bsim3.pParam.BSIM3k1 = model.BSIM3k1 + model.BSIM3lk1 * Inv_L + model.BSIM3wk1 * Inv_W + model.BSIM3pk1 * Inv_LW;
                bsim3.pParam.BSIM3kt1 = model.BSIM3kt1 + model.BSIM3lkt1 * Inv_L + model.BSIM3wkt1 * Inv_W + model.BSIM3pkt1 * Inv_LW;
                bsim3.pParam.BSIM3kt1l = model.BSIM3kt1l + model.BSIM3lkt1l * Inv_L + model.BSIM3wkt1l * Inv_W + model.BSIM3pkt1l * Inv_LW;
                bsim3.pParam.BSIM3k2 = model.BSIM3k2 + model.BSIM3lk2 * Inv_L + model.BSIM3wk2 * Inv_W + model.BSIM3pk2 * Inv_LW;
                bsim3.pParam.BSIM3kt2 = model.BSIM3kt2 + model.BSIM3lkt2 * Inv_L + model.BSIM3wkt2 * Inv_W + model.BSIM3pkt2 * Inv_LW;
                bsim3.pParam.BSIM3k3 = model.BSIM3k3 + model.BSIM3lk3 * Inv_L + model.BSIM3wk3 * Inv_W + model.BSIM3pk3 * Inv_LW;
                bsim3.pParam.BSIM3k3b = model.BSIM3k3b + model.BSIM3lk3b * Inv_L + model.BSIM3wk3b * Inv_W + model.BSIM3pk3b * Inv_LW;
                bsim3.pParam.BSIM3w0 = model.BSIM3w0 + model.BSIM3lw0 * Inv_L + model.BSIM3ww0 * Inv_W + model.BSIM3pw0 * Inv_LW;
                bsim3.pParam.BSIM3nlx = model.BSIM3nlx + model.BSIM3lnlx * Inv_L + model.BSIM3wnlx * Inv_W + model.BSIM3pnlx * Inv_LW;
                bsim3.pParam.BSIM3dvt0 = model.BSIM3dvt0 + model.BSIM3ldvt0 * Inv_L + model.BSIM3wdvt0 * Inv_W + model.BSIM3pdvt0 * Inv_LW;
                bsim3.pParam.BSIM3dvt1 = model.BSIM3dvt1 + model.BSIM3ldvt1 * Inv_L + model.BSIM3wdvt1 * Inv_W + model.BSIM3pdvt1 * Inv_LW;
                bsim3.pParam.BSIM3dvt2 = model.BSIM3dvt2 + model.BSIM3ldvt2 * Inv_L + model.BSIM3wdvt2 * Inv_W + model.BSIM3pdvt2 * Inv_LW;
                bsim3.pParam.BSIM3dvt0w = model.BSIM3dvt0w + model.BSIM3ldvt0w * Inv_L + model.BSIM3wdvt0w * Inv_W + model.BSIM3pdvt0w * Inv_LW;
                bsim3.pParam.BSIM3dvt1w = model.BSIM3dvt1w + model.BSIM3ldvt1w * Inv_L + model.BSIM3wdvt1w * Inv_W + model.BSIM3pdvt1w * Inv_LW;
                bsim3.pParam.BSIM3dvt2w = model.BSIM3dvt2w + model.BSIM3ldvt2w * Inv_L + model.BSIM3wdvt2w * Inv_W + model.BSIM3pdvt2w * Inv_LW;
                bsim3.pParam.BSIM3drout = model.BSIM3drout + model.BSIM3ldrout * Inv_L + model.BSIM3wdrout * Inv_W + model.BSIM3pdrout * Inv_LW;
                bsim3.pParam.BSIM3dsub = model.BSIM3dsub + model.BSIM3ldsub * Inv_L + model.BSIM3wdsub * Inv_W + model.BSIM3pdsub * Inv_LW;
                bsim3.pParam.BSIM3vth0 = model.BSIM3vth0 + model.BSIM3lvth0 * Inv_L + model.BSIM3wvth0 * Inv_W + model.BSIM3pvth0 * Inv_LW;
                bsim3.pParam.BSIM3ua = model.BSIM3ua + model.BSIM3lua * Inv_L + model.BSIM3wua * Inv_W + model.BSIM3pua * Inv_LW;
                bsim3.pParam.BSIM3ua1 = model.BSIM3ua1 + model.BSIM3lua1 * Inv_L + model.BSIM3wua1 * Inv_W + model.BSIM3pua1 * Inv_LW;
                bsim3.pParam.BSIM3ub = model.BSIM3ub + model.BSIM3lub * Inv_L + model.BSIM3wub * Inv_W + model.BSIM3pub * Inv_LW;
                bsim3.pParam.BSIM3ub1 = model.BSIM3ub1 + model.BSIM3lub1 * Inv_L + model.BSIM3wub1 * Inv_W + model.BSIM3pub1 * Inv_LW;
                bsim3.pParam.BSIM3uc = model.BSIM3uc + model.BSIM3luc * Inv_L + model.BSIM3wuc * Inv_W + model.BSIM3puc * Inv_LW;
                bsim3.pParam.BSIM3uc1 = model.BSIM3uc1 + model.BSIM3luc1 * Inv_L + model.BSIM3wuc1 * Inv_W + model.BSIM3puc1 * Inv_LW;
                bsim3.pParam.BSIM3u0 = model.BSIM3u0 + model.BSIM3lu0 * Inv_L + model.BSIM3wu0 * Inv_W + model.BSIM3pu0 * Inv_LW;
                bsim3.pParam.BSIM3ute = model.BSIM3ute + model.BSIM3lute * Inv_L + model.BSIM3wute * Inv_W + model.BSIM3pute * Inv_LW;
                bsim3.pParam.BSIM3voff = model.BSIM3voff + model.BSIM3lvoff * Inv_L + model.BSIM3wvoff * Inv_W + model.BSIM3pvoff * Inv_LW;
                bsim3.pParam.BSIM3delta = model.BSIM3delta + model.BSIM3ldelta * Inv_L + model.BSIM3wdelta * Inv_W + model.BSIM3pdelta * Inv_LW;
                bsim3.pParam.BSIM3rdsw = model.BSIM3rdsw + model.BSIM3lrdsw * Inv_L + model.BSIM3wrdsw * Inv_W + model.BSIM3prdsw * Inv_LW;
                bsim3.pParam.BSIM3prwg = model.BSIM3prwg + model.BSIM3lprwg * Inv_L + model.BSIM3wprwg * Inv_W + model.BSIM3pprwg * Inv_LW;
                bsim3.pParam.BSIM3prwb = model.BSIM3prwb + model.BSIM3lprwb * Inv_L + model.BSIM3wprwb * Inv_W + model.BSIM3pprwb * Inv_LW;
                bsim3.pParam.BSIM3prt = model.BSIM3prt + model.BSIM3lprt * Inv_L + model.BSIM3wprt * Inv_W + model.BSIM3pprt * Inv_LW;
                bsim3.pParam.BSIM3eta0 = model.BSIM3eta0 + model.BSIM3leta0 * Inv_L + model.BSIM3weta0 * Inv_W + model.BSIM3peta0 * Inv_LW;
                bsim3.pParam.BSIM3etab = model.BSIM3etab + model.BSIM3letab * Inv_L + model.BSIM3wetab * Inv_W + model.BSIM3petab * Inv_LW;
                bsim3.pParam.BSIM3pclm = model.BSIM3pclm + model.BSIM3lpclm * Inv_L + model.BSIM3wpclm * Inv_W + model.BSIM3ppclm * Inv_LW;
                bsim3.pParam.BSIM3pdibl1 = model.BSIM3pdibl1 + model.BSIM3lpdibl1 * Inv_L + model.BSIM3wpdibl1 * Inv_W + model.BSIM3ppdibl1 * Inv_LW;
                bsim3.pParam.BSIM3pdibl2 = model.BSIM3pdibl2 + model.BSIM3lpdibl2 * Inv_L + model.BSIM3wpdibl2 * Inv_W + model.BSIM3ppdibl2 * Inv_LW;
                bsim3.pParam.BSIM3pdiblb = model.BSIM3pdiblb + model.BSIM3lpdiblb * Inv_L + model.BSIM3wpdiblb * Inv_W + model.BSIM3ppdiblb * Inv_LW;
                bsim3.pParam.BSIM3pscbe1 = model.BSIM3pscbe1 + model.BSIM3lpscbe1 * Inv_L + model.BSIM3wpscbe1 * Inv_W + model.BSIM3ppscbe1 * Inv_LW;
                bsim3.pParam.BSIM3pscbe2 = model.BSIM3pscbe2 + model.BSIM3lpscbe2 * Inv_L + model.BSIM3wpscbe2 * Inv_W + model.BSIM3ppscbe2 * Inv_LW;
                bsim3.pParam.BSIM3pvag = model.BSIM3pvag + model.BSIM3lpvag * Inv_L + model.BSIM3wpvag * Inv_W + model.BSIM3ppvag * Inv_LW;
                bsim3.pParam.BSIM3wr = model.BSIM3wr + model.BSIM3lwr * Inv_L + model.BSIM3wwr * Inv_W + model.BSIM3pwr * Inv_LW;
                bsim3.pParam.BSIM3dwg = model.BSIM3dwg + model.BSIM3ldwg * Inv_L + model.BSIM3wdwg * Inv_W + model.BSIM3pdwg * Inv_LW;
                bsim3.pParam.BSIM3dwb = model.BSIM3dwb + model.BSIM3ldwb * Inv_L + model.BSIM3wdwb * Inv_W + model.BSIM3pdwb * Inv_LW;
                bsim3.pParam.BSIM3b0 = model.BSIM3b0 + model.BSIM3lb0 * Inv_L + model.BSIM3wb0 * Inv_W + model.BSIM3pb0 * Inv_LW;
                bsim3.pParam.BSIM3b1 = model.BSIM3b1 + model.BSIM3lb1 * Inv_L + model.BSIM3wb1 * Inv_W + model.BSIM3pb1 * Inv_LW;
                bsim3.pParam.BSIM3alpha0 = model.BSIM3alpha0 + model.BSIM3lalpha0 * Inv_L + model.BSIM3walpha0 * Inv_W + model.BSIM3palpha0 * Inv_LW;
                bsim3.pParam.BSIM3alpha1 = model.BSIM3alpha1 + model.BSIM3lalpha1 * Inv_L + model.BSIM3walpha1 * Inv_W + model.BSIM3palpha1 * Inv_LW;
                bsim3.pParam.BSIM3beta0 = model.BSIM3beta0 + model.BSIM3lbeta0 * Inv_L + model.BSIM3wbeta0 * Inv_W + model.BSIM3pbeta0 * Inv_LW;
                /* CV model */
                bsim3.pParam.BSIM3elm = model.BSIM3elm + model.BSIM3lelm * Inv_L + model.BSIM3welm * Inv_W + model.BSIM3pelm * Inv_LW;
                bsim3.pParam.BSIM3cgsl = model.BSIM3cgsl + model.BSIM3lcgsl * Inv_L + model.BSIM3wcgsl * Inv_W + model.BSIM3pcgsl * Inv_LW;
                bsim3.pParam.BSIM3cgdl = model.BSIM3cgdl + model.BSIM3lcgdl * Inv_L + model.BSIM3wcgdl * Inv_W + model.BSIM3pcgdl * Inv_LW;
                bsim3.pParam.BSIM3ckappa = model.BSIM3ckappa + model.BSIM3lckappa * Inv_L + model.BSIM3wckappa * Inv_W + model.BSIM3pckappa * Inv_LW;
                bsim3.pParam.BSIM3cf = model.BSIM3cf + model.BSIM3lcf * Inv_L + model.BSIM3wcf * Inv_W + model.BSIM3pcf * Inv_LW;
                bsim3.pParam.BSIM3clc = model.BSIM3clc + model.BSIM3lclc * Inv_L + model.BSIM3wclc * Inv_W + model.BSIM3pclc * Inv_LW;
                bsim3.pParam.BSIM3cle = model.BSIM3cle + model.BSIM3lcle * Inv_L + model.BSIM3wcle * Inv_W + model.BSIM3pcle * Inv_LW;
                bsim3.pParam.BSIM3vfbcv = model.BSIM3vfbcv + model.BSIM3lvfbcv * Inv_L + model.BSIM3wvfbcv * Inv_W + model.BSIM3pvfbcv * Inv_LW;
                bsim3.pParam.BSIM3acde = model.BSIM3acde + model.BSIM3lacde * Inv_L + model.BSIM3wacde * Inv_W + model.BSIM3pacde * Inv_LW;
                bsim3.pParam.BSIM3moin = model.BSIM3moin + model.BSIM3lmoin * Inv_L + model.BSIM3wmoin * Inv_W + model.BSIM3pmoin * Inv_LW;
                bsim3.pParam.BSIM3noff = model.BSIM3noff + model.BSIM3lnoff * Inv_L + model.BSIM3wnoff * Inv_W + model.BSIM3pnoff * Inv_LW;
                bsim3.pParam.BSIM3voffcv = model.BSIM3voffcv + model.BSIM3lvoffcv * Inv_L + model.BSIM3wvoffcv * Inv_W + model.BSIM3pvoffcv * Inv_LW;

                bsim3.pParam.BSIM3abulkCVfactor = 1.0 + Math.Pow((bsim3.pParam.BSIM3clc / bsim3.pParam.BSIM3leffCV), bsim3.pParam.BSIM3cle);

                T0 = (model.TRatio - 1.0);
                bsim3.pParam.BSIM3ua = bsim3.pParam.BSIM3ua + bsim3.pParam.BSIM3ua1 * T0;
                bsim3.pParam.BSIM3ub = bsim3.pParam.BSIM3ub + bsim3.pParam.BSIM3ub1 * T0;
                bsim3.pParam.BSIM3uc = bsim3.pParam.BSIM3uc + bsim3.pParam.BSIM3uc1 * T0;
                if (bsim3.pParam.BSIM3u0 > 1.0)
                    bsim3.pParam.BSIM3u0 = bsim3.pParam.BSIM3u0 / 1.0e4;

                bsim3.pParam.BSIM3u0temp = bsim3.pParam.BSIM3u0 * Math.Pow(model.TRatio, bsim3.pParam.BSIM3ute);
                bsim3.pParam.BSIM3vsattemp = bsim3.pParam.BSIM3vsat - bsim3.pParam.BSIM3at * T0;
                bsim3.pParam.BSIM3rds0 = (bsim3.pParam.BSIM3rdsw + bsim3.pParam.BSIM3prt * T0) / Math.Pow(bsim3.pParam.BSIM3weff * 1E6, bsim3.pParam.BSIM3wr);

                if (bsim3.BSIM3checkModel())
                    throw new CircuitException("Fatal error(s) detected during bsim3.BSIM3v24 parameter checking");

                bsim3.pParam.BSIM3cgdo = (model.BSIM3cgdo + bsim3.pParam.BSIM3cf) * bsim3.pParam.BSIM3weffCV;
                bsim3.pParam.BSIM3cgso = (model.BSIM3cgso + bsim3.pParam.BSIM3cf) * bsim3.pParam.BSIM3weffCV;
                bsim3.pParam.BSIM3cgbo = model.BSIM3cgbo * bsim3.pParam.BSIM3leffCV;

                T0 = bsim3.pParam.BSIM3leffCV * bsim3.pParam.BSIM3leffCV;
                bsim3.pParam.BSIM3tconst = bsim3.pParam.BSIM3u0temp * bsim3.pParam.BSIM3elm / (model.BSIM3cox * bsim3.pParam.BSIM3weffCV * bsim3.pParam.BSIM3leffCV *
                    T0);

                if (!model.BSIM3npeak.Given && model.BSIM3gamma1.Given)
                {
                    T0 = bsim3.pParam.BSIM3gamma1 * model.BSIM3cox;
                    bsim3.pParam.BSIM3npeak = 3.021E22 * T0 * T0;
                }

                bsim3.pParam.BSIM3phi = 2.0 * model.Vtm0 * Math.Log(bsim3.pParam.BSIM3npeak / model.ni);

                bsim3.pParam.BSIM3sqrtPhi = Math.Sqrt(bsim3.pParam.BSIM3phi);
                bsim3.pParam.BSIM3phis3 = bsim3.pParam.BSIM3sqrtPhi * bsim3.pParam.BSIM3phi;

                bsim3.pParam.BSIM3Xdep0 = Math.Sqrt(2.0 * Transistor.EPSSI / (Transistor.Charge_q * bsim3.pParam.BSIM3npeak * 1.0e6)) * bsim3.pParam.BSIM3sqrtPhi;
                bsim3.pParam.BSIM3sqrtXdep0 = Math.Sqrt(bsim3.pParam.BSIM3Xdep0);
                bsim3.pParam.BSIM3litl = Math.Sqrt(3.0 * bsim3.pParam.BSIM3xj * model.BSIM3tox);
                bsim3.pParam.BSIM3vbi = model.Vtm0 * Math.Log(1.0e20 * bsim3.pParam.BSIM3npeak / (model.ni * model.ni));
                bsim3.pParam.BSIM3cdep0 = Math.Sqrt(Transistor.Charge_q * Transistor.EPSSI * bsim3.pParam.BSIM3npeak * 1.0e6 / 2.0 / bsim3.pParam.BSIM3phi);

                bsim3.pParam.BSIM3ldeb = Math.Sqrt(Transistor.EPSSI * model.Vtm0 / (Transistor.Charge_q * bsim3.pParam.BSIM3npeak * 1.0e6)) / 3.0;
                bsim3.pParam.BSIM3acde *= Math.Pow((bsim3.pParam.BSIM3npeak / 2.0e16), -0.25);

                if (model.BSIM3k1.Given || model.BSIM3k2.Given)
                {
                    if (!model.BSIM3k1.Given)
                    {
                        CircuitWarning.Warning(this, "Warning: k1 should be specified with k2.");
                        bsim3.pParam.BSIM3k1 = 0.53;
                    }
                    if (!model.BSIM3k2.Given)
                    {
                        CircuitWarning.Warning(this, "Warning: k2 should be specified with k1.");
                        bsim3.pParam.BSIM3k2 = -0.0186;
                    }
                    if (model.BSIM3nsub.Given)
                        CircuitWarning.Warning(this, "Warning: nsub is ignored because k1 or k2 is given.");
                    if (model.BSIM3xt.Given)
                        CircuitWarning.Warning(this, "Warning: xt is ignored because k1 or k2 is given.");
                    if (model.BSIM3vbx.Given)
                        CircuitWarning.Warning(this, "Warning: vbx is ignored because k1 or k2 is given.");
                    if (model.BSIM3gamma1.Given)
                        CircuitWarning.Warning(this, "Warning: gamma1 is ignored because k1 or k2 is given.");
                    if (model.BSIM3gamma2.Given)
                        CircuitWarning.Warning(this, "Warning: gamma2 is ignored because k1 or k2 is given.");
                }
                else
                {
                    if (!model.BSIM3vbx.Given)
                        bsim3.pParam.BSIM3vbx = bsim3.pParam.BSIM3phi - 7.7348e-4 * bsim3.pParam.BSIM3npeak * bsim3.pParam.BSIM3xt * bsim3.pParam.BSIM3xt;
                    if (bsim3.pParam.BSIM3vbx > 0.0)
                        bsim3.pParam.BSIM3vbx = -bsim3.pParam.BSIM3vbx;
                    if (bsim3.pParam.BSIM3vbm > 0.0)
                        bsim3.pParam.BSIM3vbm = -bsim3.pParam.BSIM3vbm;

                    if (!model.BSIM3gamma1.Given)
                        bsim3.pParam.BSIM3gamma1 = 5.753e-12 * Math.Sqrt(bsim3.pParam.BSIM3npeak) / model.BSIM3cox;
                    if (!model.BSIM3gamma2.Given)
                        bsim3.pParam.BSIM3gamma2 = 5.753e-12 * Math.Sqrt(bsim3.pParam.BSIM3nsub) / model.BSIM3cox;

                    T0 = bsim3.pParam.BSIM3gamma1 - bsim3.pParam.BSIM3gamma2;
                    T1 = Math.Sqrt(bsim3.pParam.BSIM3phi - bsim3.pParam.BSIM3vbx) - bsim3.pParam.BSIM3sqrtPhi;
                    T2 = Math.Sqrt(bsim3.pParam.BSIM3phi * (bsim3.pParam.BSIM3phi - bsim3.pParam.BSIM3vbm)) - bsim3.pParam.BSIM3phi;
                    bsim3.pParam.BSIM3k2 = T0 * T1 / (2.0 * T2 + bsim3.pParam.BSIM3vbm);
                    bsim3.pParam.BSIM3k1 = bsim3.pParam.BSIM3gamma2 - 2.0 * bsim3.pParam.BSIM3k2 * Math.Sqrt(bsim3.pParam.BSIM3phi - bsim3.pParam.BSIM3vbm);
                }

                if (bsim3.pParam.BSIM3k2 < 0.0)
                {
                    T0 = 0.5 * bsim3.pParam.BSIM3k1 / bsim3.pParam.BSIM3k2;
                    bsim3.pParam.BSIM3vbsc = 0.9 * (bsim3.pParam.BSIM3phi - T0 * T0);
                    if (bsim3.pParam.BSIM3vbsc > -3.0)
                        bsim3.pParam.BSIM3vbsc = -3.0;
                    else if (bsim3.pParam.BSIM3vbsc < -30.0)

                        bsim3.pParam.BSIM3vbsc = -30.0;
                }
                else
                {
                    bsim3.pParam.BSIM3vbsc = -30.0;
                }
                if (bsim3.pParam.BSIM3vbsc > bsim3.pParam.BSIM3vbm)
                    bsim3.pParam.BSIM3vbsc = bsim3.pParam.BSIM3vbm;

                if (!model.BSIM3vfb.Given)
                {
                    if (model.BSIM3vth0.Given)
                    {
                        bsim3.pParam.BSIM3vfb = model.BSIM3type * bsim3.pParam.BSIM3vth0 - bsim3.pParam.BSIM3phi - bsim3.pParam.BSIM3k1 * bsim3.pParam.BSIM3sqrtPhi;
                    }
                    else
                    {
                        bsim3.pParam.BSIM3vfb = -1.0;
                    }
                }
                if (!model.BSIM3vth0.Given)
                {
                    bsim3.pParam.BSIM3vth0 = model.BSIM3type * (bsim3.pParam.BSIM3vfb + bsim3.pParam.BSIM3phi + bsim3.pParam.BSIM3k1 * bsim3.pParam.BSIM3sqrtPhi);
                }

                bsim3.pParam.BSIM3k1ox = bsim3.pParam.BSIM3k1 * model.BSIM3tox / model.BSIM3toxm;
                bsim3.pParam.BSIM3k2ox = bsim3.pParam.BSIM3k2 * model.BSIM3tox / model.BSIM3toxm;

                T1 = Math.Sqrt(Transistor.EPSSI / Transistor.EPSOX * model.BSIM3tox * bsim3.pParam.BSIM3Xdep0);
                T0 = Math.Exp(-0.5 * bsim3.pParam.BSIM3dsub * bsim3.pParam.BSIM3leff / T1);
                bsim3.pParam.BSIM3theta0vb0 = (T0 + 2.0 * T0 * T0);

                T0 = Math.Exp(-0.5 * bsim3.pParam.BSIM3drout * bsim3.pParam.BSIM3leff / T1);
                T2 = (T0 + 2.0 * T0 * T0);
                bsim3.pParam.BSIM3thetaRout = bsim3.pParam.BSIM3pdibl1 * T2 + bsim3.pParam.BSIM3pdibl2;

                tmp = Math.Sqrt(bsim3.pParam.BSIM3Xdep0);
                tmp1 = bsim3.pParam.BSIM3vbi - bsim3.pParam.BSIM3phi;
                tmp2 = model.BSIM3factor1 * tmp;

                T0 = -0.5 * bsim3.pParam.BSIM3dvt1w * bsim3.pParam.BSIM3weff * bsim3.pParam.BSIM3leff / tmp2;
                if (T0 > -Transistor.EXP_THRESHOLD)
                {
                    T1 = Math.Exp(T0);
                    T2 = T1 * (1.0 + 2.0 * T1);
                }
                else
                {
                    T1 = Transistor.MIN_EXP;
                    T2 = T1 * (1.0 + 2.0 * T1);
                }
                T0 = bsim3.pParam.BSIM3dvt0w * T2;
                T2 = T0 * tmp1;

                T0 = -0.5 * bsim3.pParam.BSIM3dvt1 * bsim3.pParam.BSIM3leff / tmp2;
                if (T0 > -Transistor.EXP_THRESHOLD)
                {
                    T1 = Math.Exp(T0);
                    T3 = T1 * (1.0 + 2.0 * T1);
                }
                else
                {
                    T1 = Transistor.MIN_EXP;
                    T3 = T1 * (1.0 + 2.0 * T1);
                }
                T3 = bsim3.pParam.BSIM3dvt0 * T3 * tmp1;

                T4 = model.BSIM3tox * bsim3.pParam.BSIM3phi / (bsim3.pParam.BSIM3weff + bsim3.pParam.BSIM3w0);

                T0 = Math.Sqrt(1.0 + bsim3.pParam.BSIM3nlx / bsim3.pParam.BSIM3leff);
                T5 = bsim3.pParam.BSIM3k1ox * (T0 - 1.0) * bsim3.pParam.BSIM3sqrtPhi + (bsim3.pParam.BSIM3kt1 + bsim3.pParam.BSIM3kt1l / bsim3.pParam.BSIM3leff) *
                    (model.TRatio - 1.0);

                tmp3 = model.BSIM3type * bsim3.pParam.BSIM3vth0 - T2 - T3 + bsim3.pParam.BSIM3k3 * T4 + T5;
                bsim3.pParam.BSIM3vfbzb = tmp3 - bsim3.pParam.BSIM3phi - bsim3.pParam.BSIM3k1 * bsim3.pParam.BSIM3sqrtPhi;
                /* End of vfbzb */
            }

            /* process source / drain series resistance */
            bsim3.BSIM3drainConductance = model.BSIM3sheetResistance * bsim3.BSIM3drainSquares;
            if (bsim3.BSIM3drainConductance > 0.0)
                bsim3.BSIM3drainConductance = 1.0 / bsim3.BSIM3drainConductance;
            else
                bsim3.BSIM3drainConductance = 0.0;

            bsim3.BSIM3sourceConductance = model.BSIM3sheetResistance * bsim3.BSIM3sourceSquares;
            if (bsim3.BSIM3sourceConductance > 0.0)
                bsim3.BSIM3sourceConductance = 1.0 / bsim3.BSIM3sourceConductance;
            else
                bsim3.BSIM3sourceConductance = 0.0;
            bsim3.BSIM3cgso = bsim3.pParam.BSIM3cgso;
            bsim3.BSIM3cgdo = bsim3.pParam.BSIM3cgdo;

            Nvtm = model.BSIM3vtm * model.BSIM3jctEmissionCoeff;
            if ((bsim3.BSIM3sourceArea <= 0.0) && (bsim3.BSIM3sourcePerimeter <= 0.0))
            {
                SourceSatCurrent = 1.0e-14;
            }
            else
            {
                SourceSatCurrent = bsim3.BSIM3sourceArea * model.BSIM3jctTempSatCurDensity + bsim3.BSIM3sourcePerimeter *
                    model.BSIM3jctSidewallTempSatCurDensity;
            }
            if ((SourceSatCurrent > 0.0) && (model.BSIM3ijth > 0.0))
            {
                bsim3.BSIM3vjsm = Nvtm * Math.Log(model.BSIM3ijth / SourceSatCurrent + 1.0);
                bsim3.BSIM3IsEvjsm = SourceSatCurrent * Math.Exp(bsim3.BSIM3vjsm / Nvtm);
            }

            if ((bsim3.BSIM3drainArea <= 0.0) && (bsim3.BSIM3drainPerimeter <= 0.0))
            {
                DrainSatCurrent = 1.0e-14;
            }
            else
            {
                DrainSatCurrent = bsim3.BSIM3drainArea * model.BSIM3jctTempSatCurDensity + bsim3.BSIM3drainPerimeter *
                    model.BSIM3jctSidewallTempSatCurDensity;
            }
            if ((DrainSatCurrent > 0.0) && (model.BSIM3ijth > 0.0))
            {
                bsim3.BSIM3vjdm = Nvtm * Math.Log(model.BSIM3ijth / DrainSatCurrent + 1.0);
                bsim3.BSIM3IsEvjdm = DrainSatCurrent * Math.Exp(bsim3.BSIM3vjdm / Nvtm);
            }
        }
    }
}
