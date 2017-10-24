using System;
using SpiceSharp.Behaviours;
using SpiceSharp.Diagnostics;
using SpiceSharp.Components.Transistors;

namespace SpiceSharp.Components.ComponentBehaviours
{
    /// <summary>
    /// Temperature behaviour for a <see cref="BSIM2"/>
    /// </summary>
    public class BSIM2TemperatureBehaviour : CircuitObjectBehaviourTemperature
    {
        /// <summary>
        /// Execute the behaviour
        /// </summary>
        /// <param name="ckt"></param>
        public override void Execute(Circuit ckt)
        {
            var bsim2 = ComponentTyped<BSIM2>();
            var model = bsim2.Model as BSIM2Model;
            double EffectiveLength;
            double EffectiveWidth;
            double Inv_L;
            double Inv_W;
            double tmp;
            double CoxWoverL;

            // Get the size dependent parameters
            Tuple<double, double> mysize = new Tuple<double, double>(bsim2.B2w, bsim2.B2l);
            if (model.Sizes.ContainsKey(mysize))
                bsim2.pParam = model.Sizes[mysize];

            if (bsim2.pParam == null)
            {
                bsim2.pParam = new BSIM2SizeDependParam();
                model.Sizes.Add(mysize, bsim2.pParam);

                EffectiveLength = bsim2.B2l - model.B2deltaL * 1.0e-6;
                EffectiveWidth = bsim2.B2w - model.B2deltaW * 1.0e-6;

                if (EffectiveLength <= 0)
                    throw new CircuitException($"B2: mosfet {bsim2.Name}, model {model.Name}: Effective channel length <= 0");
                if (EffectiveWidth <= 0)
                    throw new CircuitException($"B2: mosfet {bsim2.Name}, model {model.Name}: Effective channel width <= 0");

                Inv_L = 1.0e-6 / EffectiveLength;
                Inv_W = 1.0e-6 / EffectiveWidth;
                bsim2.pParam.B2vfb = model.B2vfb0 + model.B2vfbW * Inv_W + model.B2vfbL * Inv_L;
                bsim2.pParam.B2phi = model.B2phi0 + model.B2phiW * Inv_W + model.B2phiL * Inv_L;
                bsim2.pParam.B2k1 = model.B2k10 + model.B2k1W * Inv_W + model.B2k1L * Inv_L;
                bsim2.pParam.B2k2 = model.B2k20 + model.B2k2W * Inv_W + model.B2k2L * Inv_L;
                bsim2.pParam.B2eta0 = model.B2eta00 + model.B2eta0W * Inv_W + model.B2eta0L * Inv_L;
                bsim2.pParam.B2etaB = model.B2etaB0 + model.B2etaBW * Inv_W + model.B2etaBL * Inv_L;
                bsim2.pParam.B2beta0 = model.B2mob00;
                bsim2.pParam.B2beta0B = model.B2mob0B0 + model.B2mob0BW * Inv_W + model.B2mob0BL * Inv_L;
                bsim2.pParam.B2betas0 = model.B2mobs00 + model.B2mobs0W * Inv_W + model.B2mobs0L * Inv_L;
                if (bsim2.pParam.B2betas0 < 1.01 * bsim2.pParam.B2beta0)

                    bsim2.pParam.B2betas0 = 1.01 * bsim2.pParam.B2beta0;
                bsim2.pParam.B2betasB = model.B2mobsB0 + model.B2mobsBW * Inv_W + model.B2mobsBL * Inv_L;
                tmp = (bsim2.pParam.B2betas0 - bsim2.pParam.B2beta0 - bsim2.pParam.B2beta0B * model.B2vbb);
                if ((-bsim2.pParam.B2betasB * model.B2vbb) > tmp)
                    bsim2.pParam.B2betasB = -tmp / model.B2vbb;
                bsim2.pParam.B2beta20 = model.B2mob200 + model.B2mob20W * Inv_W + model.B2mob20L * Inv_L;
                bsim2.pParam.B2beta2B = model.B2mob2B0 + model.B2mob2BW * Inv_W + model.B2mob2BL * Inv_L;
                bsim2.pParam.B2beta2G = model.B2mob2G0 + model.B2mob2GW * Inv_W + model.B2mob2GL * Inv_L;
                bsim2.pParam.B2beta30 = model.B2mob300 + model.B2mob30W * Inv_W + model.B2mob30L * Inv_L;
                bsim2.pParam.B2beta3B = model.B2mob3B0 + model.B2mob3BW * Inv_W + model.B2mob3BL * Inv_L;
                bsim2.pParam.B2beta3G = model.B2mob3G0 + model.B2mob3GW * Inv_W + model.B2mob3GL * Inv_L;
                bsim2.pParam.B2beta40 = model.B2mob400 + model.B2mob40W * Inv_W + model.B2mob40L * Inv_L;
                bsim2.pParam.B2beta4B = model.B2mob4B0 + model.B2mob4BW * Inv_W + model.B2mob4BL * Inv_L;
                bsim2.pParam.B2beta4G = model.B2mob4G0 + model.B2mob4GW * Inv_W + model.B2mob4GL * Inv_L;

                CoxWoverL = model.B2Cox * EffectiveWidth / EffectiveLength;

                bsim2.pParam.B2beta0 *= CoxWoverL;
                bsim2.pParam.B2beta0B *= CoxWoverL;
                bsim2.pParam.B2betas0 *= CoxWoverL;
                bsim2.pParam.B2betasB *= CoxWoverL;
                bsim2.pParam.B2beta30 *= CoxWoverL;
                bsim2.pParam.B2beta3B *= CoxWoverL;
                bsim2.pParam.B2beta3G *= CoxWoverL;
                bsim2.pParam.B2beta40 *= CoxWoverL;
                bsim2.pParam.B2beta4B *= CoxWoverL;
                bsim2.pParam.B2beta4G *= CoxWoverL;

                bsim2.pParam.B2ua0 = model.B2ua00 + model.B2ua0W * Inv_W + model.B2ua0L * Inv_L;
                bsim2.pParam.B2uaB = model.B2uaB0 + model.B2uaBW * Inv_W + model.B2uaBL * Inv_L;
                bsim2.pParam.B2ub0 = model.B2ub00 + model.B2ub0W * Inv_W + model.B2ub0L * Inv_L;
                bsim2.pParam.B2ubB = model.B2ubB0 + model.B2ubBW * Inv_W + model.B2ubBL * Inv_L;
                bsim2.pParam.B2u10 = model.B2u100 + model.B2u10W * Inv_W + model.B2u10L * Inv_L;
                bsim2.pParam.B2u1B = model.B2u1B0 + model.B2u1BW * Inv_W + model.B2u1BL * Inv_L;
                bsim2.pParam.B2u1D = model.B2u1D0 + model.B2u1DW * Inv_W + model.B2u1DL * Inv_L;
                bsim2.pParam.B2n0 = model.B2n00 + model.B2n0W * Inv_W + model.B2n0L * Inv_L;
                bsim2.pParam.B2nB = model.B2nB0 + model.B2nBW * Inv_W + model.B2nBL * Inv_L;
                bsim2.pParam.B2nD = model.B2nD0 + model.B2nDW * Inv_W + model.B2nDL * Inv_L;
                if (bsim2.pParam.B2n0 < 0.0)

                    bsim2.pParam.B2n0 = 0.0;

                bsim2.pParam.B2vof0 = model.B2vof00 + model.B2vof0W * Inv_W + model.B2vof0L * Inv_L;
                bsim2.pParam.B2vofB = model.B2vofB0 + model.B2vofBW * Inv_W + model.B2vofBL * Inv_L;
                bsim2.pParam.B2vofD = model.B2vofD0 + model.B2vofDW * Inv_W + model.B2vofDL * Inv_L;
                bsim2.pParam.B2ai0 = model.B2ai00 + model.B2ai0W * Inv_W + model.B2ai0L * Inv_L;
                bsim2.pParam.B2aiB = model.B2aiB0 + model.B2aiBW * Inv_W + model.B2aiBL * Inv_L;
                bsim2.pParam.B2bi0 = model.B2bi00 + model.B2bi0W * Inv_W + model.B2bi0L * Inv_L;
                bsim2.pParam.B2biB = model.B2biB0 + model.B2biBW * Inv_W + model.B2biBL * Inv_L;
                bsim2.pParam.B2vghigh = model.B2vghigh0 + model.B2vghighW * Inv_W + model.B2vghighL * Inv_L;
                bsim2.pParam.B2vglow = model.B2vglow0 + model.B2vglowW * Inv_W + model.B2vglowL * Inv_L;

                bsim2.pParam.CoxWL = model.B2Cox * EffectiveLength * EffectiveWidth * 1.0e4;
                bsim2.pParam.One_Third_CoxWL = bsim2.pParam.CoxWL / 3.0;
                bsim2.pParam.Two_Third_CoxWL = 2.0 * bsim2.pParam.One_Third_CoxWL;
                bsim2.pParam.B2GSoverlapCap = model.B2gateSourceOverlapCap * EffectiveWidth;
                bsim2.pParam.B2GDoverlapCap = model.B2gateDrainOverlapCap * EffectiveWidth;
                bsim2.pParam.B2GBoverlapCap = model.B2gateBulkOverlapCap * EffectiveLength;
                bsim2.pParam.SqrtPhi = Math.Sqrt(bsim2.pParam.B2phi);
                bsim2.pParam.Phis3 = bsim2.pParam.SqrtPhi * bsim2.pParam.B2phi;
                bsim2.pParam.Arg = bsim2.pParam.B2betasB - bsim2.pParam.B2beta0B - model.B2vdd * (bsim2.pParam.B2beta3B - model.B2vdd * bsim2.pParam.B2beta4B);

            }

            /* process drain series resistance */
            if ((bsim2.B2drainConductance = model.B2sheetResistance * bsim2.B2drainSquares) != 0.0)
            {
                bsim2.B2drainConductance = 1.0 / bsim2.B2drainConductance;
            }

            /* process source series resistance */
            if ((bsim2.B2sourceConductance = model.B2sheetResistance * bsim2.B2sourceSquares) != 0.0)
            {
                bsim2.B2sourceConductance = 1.0 / bsim2.B2sourceConductance;
            }

            bsim2.pParam.B2vt0 = bsim2.pParam.B2vfb + bsim2.pParam.B2phi + bsim2.pParam.B2k1 * bsim2.pParam.SqrtPhi - bsim2.pParam.B2k2 * bsim2.pParam.B2phi;
            bsim2.B2von = bsim2.pParam.B2vt0; /* added for initialization */
        }
    }
}
