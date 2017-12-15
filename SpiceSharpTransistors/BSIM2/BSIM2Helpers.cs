using System;
using SpiceSharp.Circuits;

namespace SpiceSharp.Components.Transistors
{
    /// <summary>
    /// Helper methods for the BSIM2 model
    /// </summary>
    internal static class BSIM2Helpers
    {
        /// <summary>
        /// This routine evaluates the drain current, its derivatives and the
        /// charges associated with the gate,bulk and drain terminal
        /// using the B2(Berkeley Short-Channel IGFET Model) Equations.
        /// </summary>
        /// <param name="bsim2"></param>
        /// <param name="Vds"></param>
        /// <param name="Vbs"></param>
        /// <param name="Vgs"></param>
        /// <param name="gm"></param>
        /// <param name="gds"></param>
        /// <param name="gmb"></param>
        /// <param name="qg"></param>
        /// <param name="qb"></param>
        /// <param name="qd"></param>
        /// <param name="cgg"></param>
        /// <param name="cgd"></param>
        /// <param name="cgs"></param>
        /// <param name="cbg"></param>
        /// <param name="cbd"></param>
        /// <param name="cbs"></param>
        /// <param name="cdg"></param>
        /// <param name="cdd"></param>
        /// <param name="cds"></param>
        /// <param name="Ids"></param>
        /// <param name="von"></param>
        /// <param name="vdsat"></param>
        /// <param name="ckt"></param>
        public static void B2evaluate(this BSIM2 bsim2, double Vds, double Vbs, double Vgs, out double gm, out double gds, out double gmb, out double qg,
            out double qb, out double qd, out double cgg, out double cgd, out double cgs,
            out double cbg, out double cbd, out double cbs, out double cdg, out double cdd,
            out double cds, out double Ids, out double von, out double vdsat, Circuit ckt)
        {
            var model = bsim2.Model as BSIM2Model;
            double Vth, Vdsat = 0.0;
            double Phisb, T1s, Eta, Gg, Aa, Inv_Aa, U1, U1s, Vc, Kk, SqrtKk;
            double dPhisb_dVb, dT1s_dVb, dVth_dVb, dVth_dVd, dAa_dVb, dVc_dVd;
            double dVc_dVg, dVc_dVb, dKk_dVc, dVdsat_dVd = 0.0, dVdsat_dVg = 0.0, dVdsat_dVb = 0.0;
            double dUvert_dVg, dUvert_dVd, dUvert_dVb, Inv_Kk;
            double dUtot_dVd, dUtot_dVb, dUtot_dVg, Ai, Bi, Vghigh, Vglow, Vgeff, Vof;
            double Vbseff, Vgst, Vgdt, Qbulk, Utot;
            double T0, T1, T2, T3, T4, T5, Arg1, Arg2, Exp0 = 0.0;
            double tmp, tmp1, tmp2, tmp3, Uvert, Beta1, Beta2, Beta0, dGg_dVb, Exp1 = 0.0;
            double T6, T7, T8, T9, n = 0.0, ExpArg, ExpArg1;
            double Beta, dQbulk_dVb, dVgdt_dVg, dVgdt_dVd;
            double dVbseff_dVb, Ua, Ub, dVgdt_dVb, dQbulk_dVd;
            double Con1, Con3, Con4, SqrVghigh, SqrVglow, CubVghigh, CubVglow;
            double delta, Coeffa, Coeffb, Coeffc, Coeffd, Inv_Uvert, Inv_Utot;
            double Inv_Vdsat, tanh, Sqrsech, dBeta1_dVb, dU1_dVd, dU1_dVg, dU1_dVb;
            double Betaeff, FR, dFR_dVd, dFR_dVg, dFR_dVb, Betas, Beta3, Beta4;
            double dBeta_dVd, dBeta_dVg, dBeta_dVb, dVgeff_dVg, dVgeff_dVd, dVgeff_dVb;
            double dCon3_dVd, dCon3_dVb, dCon4_dVd, dCon4_dVb, dCoeffa_dVd, dCoeffa_dVb;
            double dCoeffb_dVd, dCoeffb_dVb, dCoeffc_dVd, dCoeffc_dVb;
            double dCoeffd_dVd, dCoeffd_dVb;
            bool ChargeComputationNeeded;
            int valuetypeflag;			/* added  3/19/90 JSD   */

            if (ckt.Method != null || (ckt.State.Domain == State.DomainTypes.Time && ckt.State.UseDC && ckt.State.UseIC) || ckt.State.UseSmallSignal)
                ChargeComputationNeeded = true;
            else
                ChargeComputationNeeded = false;

            if (Vbs < model.B2vbb2) Vbs = model.B2vbb2;
            if (Vgs > model.B2vgg2) Vgs = model.B2vgg2;
            if (Vds > model.B2vdd2) Vds = model.B2vdd2;

            /* Threshold Voltage. */
            if (Vbs <= 0.0)
            {
                Phisb = bsim2.pParam.B2phi - Vbs;
                dPhisb_dVb = -1.0;
                T1s = Math.Sqrt(Phisb);
                dT1s_dVb = -0.5 / T1s;
            }
            else
            {
                tmp = bsim2.pParam.B2phi / (bsim2.pParam.B2phi + Vbs);
                Phisb = bsim2.pParam.B2phi * tmp;
                dPhisb_dVb = -tmp * tmp;
                T1s = bsim2.pParam.Phis3 / (bsim2.pParam.B2phi + 0.5 * Vbs);
                dT1s_dVb = -0.5 * T1s * T1s / bsim2.pParam.Phis3;
            }

            Eta = bsim2.pParam.B2eta0 + bsim2.pParam.B2etaB * Vbs;
            Ua = bsim2.pParam.B2ua0 + bsim2.pParam.B2uaB * Vbs;
            Ub = bsim2.pParam.B2ub0 + bsim2.pParam.B2ubB * Vbs;
            U1s = bsim2.pParam.B2u10 + bsim2.pParam.B2u1B * Vbs;

            Vth = bsim2.pParam.B2vfb + bsim2.pParam.B2phi + bsim2.pParam.B2k1
                * T1s - bsim2.pParam.B2k2 * Phisb - Eta * Vds;
            dVth_dVd = -Eta;
            dVth_dVb = bsim2.pParam.B2k1 * dT1s_dVb + bsim2.pParam.B2k2
                 - bsim2.pParam.B2etaB * Vds;

            Vgst = Vgs - Vth;

            tmp = 1.0 / (1.744 + 0.8364 * Phisb);
            Gg = 1.0 - tmp;
            dGg_dVb = 0.8364 * tmp * tmp * dPhisb_dVb;
            T0 = Gg / T1s;
            tmp1 = 0.5 * T0 * bsim2.pParam.B2k1;
            Aa = 1.0 + tmp1;
            dAa_dVb = (Aa - 1.0) * (dGg_dVb / Gg - dT1s_dVb / T1s);
            Inv_Aa = 1.0 / Aa;

            Vghigh = bsim2.pParam.B2vghigh;
            Vglow = bsim2.pParam.B2vglow;

            if ((Vgst >= Vghigh) || (bsim2.pParam.B2n0 == 0.0))
            {
                Vgeff = Vgst;
                dVgeff_dVg = 1.0;
                dVgeff_dVd = -dVth_dVd;
                dVgeff_dVb = -dVth_dVb;
            }
            else
            {
                Vof = bsim2.pParam.B2vof0 + bsim2.pParam.B2vofB * Vbs
                + bsim2.pParam.B2vofD * Vds;
                n = bsim2.pParam.B2n0 + bsim2.pParam.B2nB / T1s
                      + bsim2.pParam.B2nD * Vds;
                tmp = 0.5 / (n * model.B2Vtm);

                ExpArg1 = -Vds / model.B2Vtm;
                ExpArg1 = Math.Max(ExpArg1, -30.0);
                Exp1 = Math.Exp(ExpArg1);
                tmp1 = 1.0 - Exp1;
                tmp1 = Math.Max(tmp1, 1.0e-18);
                tmp2 = 2.0 * Aa * tmp1;

                if (Vgst <= Vglow)
                {
                    ExpArg = Vgst * tmp;
                    ExpArg = Math.Max(ExpArg, -30.0);
                    Exp0 = Math.Exp(0.5 * Vof + ExpArg);
                    Vgeff = Math.Sqrt(tmp2) * model.B2Vtm * Exp0;
                    T0 = n * model.B2Vtm;
                    dVgeff_dVg = Vgeff * tmp;
                    dVgeff_dVd = dVgeff_dVg * (n / tmp1 * Exp1 - dVth_dVd - Vgst
                         * bsim2.pParam.B2nD / n + T0 * bsim2.pParam.B2vofD);
                    dVgeff_dVb = dVgeff_dVg * (bsim2.pParam.B2vofB * T0
                           - dVth_dVb + bsim2.pParam.B2nB * Vgst
                           / (n * T1s * T1s) * dT1s_dVb + T0 * Inv_Aa * dAa_dVb);
                }
                else
                {
                    ExpArg = Vglow * tmp;
                    ExpArg = Math.Max(ExpArg, -30.0);
                    Exp0 = Math.Exp(0.5 * Vof + ExpArg);
                    Vgeff = Math.Sqrt(2.0 * Aa * (1.0 - Exp1)) * model.B2Vtm * Exp0;
                    Con1 = Vghigh;
                    Con3 = Vgeff;
                    Con4 = Con3 * tmp;
                    SqrVghigh = Vghigh * Vghigh;
                    SqrVglow = Vglow * Vglow;
                    CubVghigh = Vghigh * SqrVghigh;
                    CubVglow = Vglow * SqrVglow;
                    T0 = 2.0 * Vghigh;
                    T1 = 2.0 * Vglow;
                    T2 = 3.0 * SqrVghigh;
                    T3 = 3.0 * SqrVglow;
                    T4 = Vghigh - Vglow;
                    T5 = SqrVghigh - SqrVglow;
                    T6 = CubVghigh - CubVglow;
                    T7 = Con1 - Con3;
                    delta = (T1 - T0) * T6 + (T2 - T3) * T5 + (T0 * T3 - T1 * T2) * T4;
                    delta = 1.0 / delta;
                    Coeffb = (T1 - Con4 * T0) * T6 + (Con4 * T2 - T3) * T5
                   + (T0 * T3 - T1 * T2) * T7;
                    Coeffc = (Con4 - 1.0) * T6 + (T2 - T3) * T7 + (T3 - Con4 * T2) * T4;
                    Coeffd = (T1 - T0) * T7 + (1.0 - Con4) * T5 + (Con4 * T0 - T1) * T4;
                    Coeffa = SqrVghigh * (Coeffc + Coeffd * T0);
                    Vgeff = (Coeffa + Vgst * (Coeffb + Vgst * (Coeffc + Vgst * Coeffd)))

                  * delta;
                    dVgeff_dVg = (Coeffb + Vgst * (2.0 * Coeffc + 3.0 * Vgst * Coeffd))

                                   * delta;
                    T7 = Con3 * tmp;
                    T8 = dT1s_dVb * bsim2.pParam.B2nB / (T1s * T1s * n);
                    T9 = n * model.B2Vtm;
                    dCon3_dVd = T7 * (n * Exp1 / tmp1 - Vglow * bsim2.pParam.B2nD
                          / n + T9 * bsim2.pParam.B2vofD);
                    dCon3_dVb = T7 * (T9 * Inv_Aa * dAa_dVb + Vglow * T8
                          + T9 * bsim2.pParam.B2vofB);
                    dCon4_dVd = tmp * dCon3_dVd - T7 * bsim2.pParam.B2nD / n;
                    dCon4_dVb = tmp * dCon3_dVb + T7 * T8;

                    dCoeffb_dVd = dCon4_dVd * (T2 * T5 - T0 * T6) + dCon3_dVd
                    * (T1 * T2 - T0 * T3);
                    dCoeffc_dVd = dCon4_dVd * (T6 - T2 * T4) + dCon3_dVd * (T3 - T2);
                    dCoeffd_dVd = dCon4_dVd * (T0 * T4 - T5) + dCon3_dVd * (T0 - T1);
                    dCoeffa_dVd = SqrVghigh * (dCoeffc_dVd + dCoeffd_dVd * T0);

                    dVgeff_dVd = -dVgeff_dVg * dVth_dVd + (dCoeffa_dVd + Vgst
                          * (dCoeffb_dVd + Vgst * (dCoeffc_dVd + Vgst
                         * dCoeffd_dVd))) * delta;

                    dCoeffb_dVb = dCon4_dVb * (T2 * T5 - T0 * T6) + dCon3_dVb
                    * (T1 * T2 - T0 * T3);
                    dCoeffc_dVb = dCon4_dVb * (T6 - T2 * T4) + dCon3_dVb * (T3 - T2);
                    dCoeffd_dVb = dCon4_dVb * (T0 * T4 - T5) + dCon3_dVb * (T0 - T1);
                    dCoeffa_dVb = SqrVghigh * (dCoeffc_dVb + dCoeffd_dVb * T0);

                    dVgeff_dVb = -dVgeff_dVg * dVth_dVb + (dCoeffa_dVb + Vgst
                          * (dCoeffb_dVb + Vgst * (dCoeffc_dVb + Vgst
                         * dCoeffd_dVb))) * delta;
                }
            }

            if (Vgeff > 0.0)
            {
                Uvert = 1.0 + Vgeff * (Ua + Vgeff * Ub);
                Uvert = Math.Max(Uvert, 0.2);
                Inv_Uvert = 1.0 / Uvert;
                T8 = Ua + 2.0 * Ub * Vgeff;
                dUvert_dVg = T8 * dVgeff_dVg;
                dUvert_dVd = T8 * dVgeff_dVd;
                dUvert_dVb = T8 * dVgeff_dVb + Vgeff * (bsim2.pParam.B2uaB
                               + Vgeff * bsim2.pParam.B2ubB);

                T8 = U1s * Inv_Aa * Inv_Uvert;
                Vc = T8 * Vgeff;
                T9 = Vc * Inv_Uvert;
                dVc_dVg = T8 * dVgeff_dVg - T9 * dUvert_dVg;
                dVc_dVd = T8 * dVgeff_dVd - T9 * dUvert_dVd;
                dVc_dVb = T8 * dVgeff_dVb + bsim2.pParam.B2u1B * Vgeff * Inv_Aa

                            * Inv_Uvert - Vc * Inv_Aa * dAa_dVb - T9 * dUvert_dVb;


                tmp2 = Math.Sqrt(1.0 + 2.0 * Vc);
                Kk = 0.5 * (1.0 + Vc + tmp2);
                Inv_Kk = 1.0 / Kk;
                dKk_dVc = 0.5 + 0.5 / tmp2;
                SqrtKk = Math.Sqrt(Kk);

                T8 = Inv_Aa / SqrtKk;
                Vdsat = Vgeff * T8;
                Vdsat = Math.Max(Vdsat, 1.0e-18);
                Inv_Vdsat = 1.0 / Vdsat;
                T9 = 0.5 * Vdsat * Inv_Kk * dKk_dVc;
                dVdsat_dVd = T8 * dVgeff_dVd - T9 * dVc_dVd;
                dVdsat_dVg = T8 * dVgeff_dVg - T9 * dVc_dVg;
                dVdsat_dVb = T8 * dVgeff_dVb - T9 * dVc_dVb - Vdsat * Inv_Aa * dAa_dVb;

                Beta0 = bsim2.pParam.B2beta0 + bsim2.pParam.B2beta0B * Vbs;
                Betas = bsim2.pParam.B2betas0 + bsim2.pParam.B2betasB * Vbs;
                Beta2 = bsim2.pParam.B2beta20 + bsim2.pParam.B2beta2B * Vbs
                          + bsim2.pParam.B2beta2G * Vgs;
                Beta3 = bsim2.pParam.B2beta30 + bsim2.pParam.B2beta3B * Vbs
                          + bsim2.pParam.B2beta3G * Vgs;
                Beta4 = bsim2.pParam.B2beta40 + bsim2.pParam.B2beta4B * Vbs
                          + bsim2.pParam.B2beta4G * Vgs;
                Beta1 = Betas - (Beta0 + model.B2vdd * (Beta3 - model.B2vdd
                 * Beta4));

                T0 = Vds * Beta2 * Inv_Vdsat;
                T0 = Math.Min(T0, 30.0);
                T1 = Math.Exp(T0);
                T2 = T1 * T1;
                T3 = T2 + 1.0;
                tanh = (T2 - 1.0) / T3;
                Sqrsech = 4.0 * T2 / (T3 * T3);

                Beta = Beta0 + Beta1 * tanh + Vds * (Beta3 - Beta4 * Vds);
                T4 = Beta1 * Sqrsech * Inv_Vdsat;
                T5 = model.B2vdd * tanh;
                dBeta_dVd = Beta3 - 2.0 * Beta4 * Vds + T4 * (Beta2 - T0 * dVdsat_dVd);
                dBeta_dVg = T4 * (bsim2.pParam.B2beta2G * Vds - T0 * dVdsat_dVg)
                      + bsim2.pParam.B2beta3G * (Vds - T5)
                  - bsim2.pParam.B2beta4G * (Vds * Vds - model.B2vdd * T5);
                dBeta1_dVb = bsim2.pParam.Arg;
                dBeta_dVb = bsim2.pParam.B2beta0B + dBeta1_dVb * tanh + Vds
                     * (bsim2.pParam.B2beta3B - Vds * bsim2.pParam.B2beta4B)
                      + T4 * (bsim2.pParam.B2beta2B * Vds - T0 * dVdsat_dVb);


                if (Vgst > Vglow)
                {
                    if (Vds <= Vdsat) /* triode region */
                    {
                        T3 = Vds * Inv_Vdsat;
                        T4 = T3 - 1.0;
                        T2 = 1.0 - bsim2.pParam.B2u1D * T4 * T4;
                        U1 = U1s * T2;
                        Utot = Uvert + U1 * Vds;
                        Utot = Math.Max(Utot, 0.5);
                        Inv_Utot = 1.0 / Utot;
                        T5 = 2.0 * U1s * bsim2.pParam.B2u1D * Inv_Vdsat * T4;
                        dU1_dVd = T5 * (T3 * dVdsat_dVd - 1.0);
                        dU1_dVg = T5 * T3 * dVdsat_dVg;
                        dU1_dVb = T5 * T3 * dVdsat_dVb + bsim2.pParam.B2u1B * T2;
                        dUtot_dVd = dUvert_dVd + U1 + Vds * dU1_dVd;
                        dUtot_dVg = dUvert_dVg + Vds * dU1_dVg;
                        dUtot_dVb = dUvert_dVb + Vds * dU1_dVb;

                        tmp1 = (Vgeff - 0.5 * Aa * Vds);
                        tmp3 = tmp1 * Vds;
                        Betaeff = Beta * Inv_Utot;
                        Ids = Betaeff * tmp3;
                        T6 = Ids / Betaeff * Inv_Utot;

                        gds = T6 * (dBeta_dVd - Betaeff * dUtot_dVd) + Betaeff * (tmp1
                            + (dVgeff_dVd - 0.5 * Aa) * Vds);
                        gm = T6 * (dBeta_dVg - Betaeff * dUtot_dVg) + Betaeff * Vds * dVgeff_dVg;

                        gmb = T6 * (dBeta_dVb - Betaeff * dUtot_dVb) + Betaeff * Vds
                            * (dVgeff_dVb - 0.5 * Vds * dAa_dVb);
                    }
                    else  /* Saturation */
                    {
                        tmp1 = Vgeff * Inv_Aa * Inv_Kk;
                        tmp3 = 0.5 * Vgeff * tmp1;
                        Betaeff = Beta * Inv_Uvert;
                        Ids = Betaeff * tmp3;
                        T0 = Ids / Betaeff * Inv_Uvert;
                        T1 = Betaeff * Vgeff * Inv_Aa * Inv_Kk;
                        T2 = Ids * Inv_Kk * dKk_dVc;

                        if (bsim2.pParam.B2ai0 != 0.0)
                        {
                            Ai = bsim2.pParam.B2ai0 + bsim2.pParam.B2aiB * Vbs;
                            Bi = bsim2.pParam.B2bi0 + bsim2.pParam.B2biB * Vbs;
                            T5 = Bi / (Vds - Vdsat);
                            T5 = Math.Min(T5, 30.0);
                            T6 = Math.Exp(-T5);
                            FR = 1.0 + Ai * T6;
                            T7 = T5 / (Vds - Vdsat);
                            T8 = (1.0 - FR) * T7;
                            dFR_dVd = T8 * (dVdsat_dVd - 1.0);
                            dFR_dVg = T8 * dVdsat_dVg;
                            dFR_dVb = T8 * dVdsat_dVb + T6 * (bsim2.pParam.B2aiB - Ai
                              * bsim2.pParam.B2biB / (Vds - Vdsat));


                            gds = (T0 * (dBeta_dVd - Betaeff * dUvert_dVd) + T1
                             * dVgeff_dVd - T2 * dVc_dVd) * FR + Ids * dFR_dVd;

                            gm = (T0 * (dBeta_dVg - Betaeff * dUvert_dVg)
                            + T1 * dVgeff_dVg - T2 * dVc_dVg) * FR + Ids * dFR_dVg;

                            gmb = (T0 * (dBeta_dVb - Betaeff * dUvert_dVb) + T1
                             * dVgeff_dVb - T2 * dVc_dVb - Ids * Inv_Aa * dAa_dVb)
                             * FR + Ids * dFR_dVb;

                            Ids *= FR;
                        }
                        else
                        {
                            gds = T0 * (dBeta_dVd - Betaeff * dUvert_dVd) + T1
                             * dVgeff_dVd - T2 * dVc_dVd;

                            gm = T0 * (dBeta_dVg - Betaeff * dUvert_dVg) + T1 * dVgeff_dVg
                            - T2 * dVc_dVg;

                            gmb = T0 * (dBeta_dVb - Betaeff * dUvert_dVb) + T1
                             * dVgeff_dVb - T2 * dVc_dVb - Ids * Inv_Aa * dAa_dVb;
                        }
                    } /* end of Saturation */
                }
                else
                {
                    T0 = Exp0 * Exp0;
                    T1 = Exp1;
                    Ids = Beta * model.B2Vtm * model.B2Vtm * T0 * (1.0 - T1);
                    T2 = Ids / Beta;
                    T4 = n * model.B2Vtm;
                    T3 = Ids / T4;
                    if ((Vds > Vdsat) && bsim2.pParam.B2ai0 != 0.0)
                    {
                        Ai = bsim2.pParam.B2ai0 + bsim2.pParam.B2aiB * Vbs;
                        Bi = bsim2.pParam.B2bi0 + bsim2.pParam.B2biB * Vbs;
                        T5 = Bi / (Vds - Vdsat);
                        T5 = Math.Min(T5, 30.0);
                        T6 = Math.Exp(-T5);
                        FR = 1.0 + Ai * T6;
                        T7 = T5 / (Vds - Vdsat);
                        T8 = (1.0 - FR) * T7;
                        dFR_dVd = T8 * (dVdsat_dVd - 1.0);
                        dFR_dVg = T8 * dVdsat_dVg;
                        dFR_dVb = T8 * dVdsat_dVb + T6 * (bsim2.pParam.B2aiB - Ai
                          * bsim2.pParam.B2biB / (Vds - Vdsat));
                    }
                    else
                    {
                        FR = 1.0;
                        dFR_dVd = 0.0;
                        dFR_dVg = 0.0;
                        dFR_dVb = 0.0;
                    }

                    gds = (T2 * dBeta_dVd + T3 * (bsim2.pParam.B2vofD * T4 - dVth_dVd
                     - bsim2.pParam.B2nD * Vgst / n) + Beta * model.B2Vtm
                 * T0 * T1) * FR + Ids * dFR_dVd;
                    gm = (T2 * dBeta_dVg + T3) * FR + Ids * dFR_dVg;
                    gmb = (T2 * dBeta_dVb + T3 * (bsim2.pParam.B2vofB * T4 - dVth_dVb
                     + bsim2.pParam.B2nB * Vgst / (n * T1s * T1s) * dT1s_dVb)) * FR
                     + Ids * dFR_dVb;
                    Ids *= FR;
                }
            }
            else
            {
                Ids = 0.0;
                gm = 0.0;
                gds = 0.0;
                gmb = 0.0;
            }

            /* Some Limiting of DC Parameters */
            gds = Math.Max(gds, 1.0e-20);


            if ((model.B2channelChargePartitionFlag > 1)
             || ((!ChargeComputationNeeded) &&
             (model.B2channelChargePartitionFlag > -5)))
            {
                qg = 0.0;
                qd = 0.0;
                qb = 0.0;
                cgg = 0.0;
                cgs = 0.0;
                cgd = 0.0;
                cdg = 0.0;
                cds = 0.0;
                cdd = 0.0;
                cbg = 0.0;
                cbs = 0.0;
                cbd = 0.0;
                goto finished;
            }
            else
            {
                if (Vbs < 0.0)
                {
                    Vbseff = Vbs;
                    dVbseff_dVb = 1.0;
                }
                else
                {
                    Vbseff = bsim2.pParam.B2phi - Phisb;
                    dVbseff_dVb = -dPhisb_dVb;
                }
                Arg1 = Vgs - Vbseff - bsim2.pParam.B2vfb;
                Arg2 = Arg1 - Vgst;
                Qbulk = bsim2.pParam.One_Third_CoxWL * Arg2;
                dQbulk_dVb = bsim2.pParam.One_Third_CoxWL * (dVth_dVb - dVbseff_dVb);
                dQbulk_dVd = bsim2.pParam.One_Third_CoxWL * dVth_dVd;
                if (Arg1 <= 0.0)
                {
                    qg = bsim2.pParam.CoxWL * Arg1;
                    qb = -(qg);
                    qd = 0.0;


                    cgg = bsim2.pParam.CoxWL;

                    cgd = 0.0;

                    cgs = -cgg * (1.0 - dVbseff_dVb);


                    cdg = 0.0;

                    cdd = 0.0;

                    cds = 0.0;


                    cbg = -bsim2.pParam.CoxWL;

                    cbd = 0.0;

                    cbs = -cgs;
                }
                else if (Vgst <= 0.0)
                {
                    T2 = Arg1 / Arg2;
                    T3 = T2 * T2 * (bsim2.pParam.CoxWL - bsim2.pParam.Two_Third_CoxWL
                  * T2);


                    qg = bsim2.pParam.CoxWL * Arg1 * (1.0 - T2 * (1.0 - T2 / 3.0));
                    qb = -(qg);
                    qd = 0.0;


                    cgg = bsim2.pParam.CoxWL * (1.0 - T2 * (2.0 - T2));
                    tmp = T3 * dVth_dVb - (cgg + T3) * dVbseff_dVb;

                    cgd = T3 * dVth_dVd;

                    cgs = -(cgg + cgd + tmp);


                    cdg = 0.0;

                    cdd = 0.0;

                    cds = 0.0;


                    cbg = -cgg;

                    cbd = -cgd;

                    cbs = -cgs;
                }
                else
                {
                    if (Vgst < bsim2.pParam.B2vghigh)
                    {
                        Uvert = 1.0 + Vgst * (Ua + Vgst * Ub);
                        Uvert = Math.Max(Uvert, 0.2);
                        Inv_Uvert = 1.0 / Uvert;
                        dUvert_dVg = Ua + 2.0 * Ub * Vgst;
                        dUvert_dVd = -dUvert_dVg * dVth_dVd;
                        dUvert_dVb = -dUvert_dVg * dVth_dVb + Vgst
                        * (bsim2.pParam.B2uaB + Vgst * bsim2.pParam.B2ubB);

                        T8 = U1s * Inv_Aa * Inv_Uvert;
                        Vc = T8 * Vgst;
                        T9 = Vc * Inv_Uvert;
                        dVc_dVg = T8 - T9 * dUvert_dVg;
                        dVc_dVd = -T8 * dVth_dVd - T9 * dUvert_dVd;
                        dVc_dVb = -T8 * dVth_dVb + bsim2.pParam.B2u1B * Vgst * Inv_Aa

                                           * Inv_Uvert - Vc * Inv_Aa * dAa_dVb - T9 * dUvert_dVb;

                        tmp2 = Math.Sqrt(1.0 + 2.0 * Vc);
                        Kk = 0.5 * (1.0 + Vc + tmp2);
                        Inv_Kk = 1.0 / Kk;
                        dKk_dVc = 0.5 + 0.5 / tmp2;
                        SqrtKk = Math.Sqrt(Kk);

                        T8 = Inv_Aa / SqrtKk;
                        Vdsat = Vgst * T8;
                        T9 = 0.5 * Vdsat * Inv_Kk * dKk_dVc;
                        dVdsat_dVd = -T8 * dVth_dVd - T9 * dVc_dVd;
                        dVdsat_dVg = T8 - T9 * dVc_dVg;
                        dVdsat_dVb = -T8 * dVth_dVb - T9 * dVc_dVb
                                      - Vdsat * Inv_Aa * dAa_dVb;
                    }
                    if (Vds >= Vdsat)
                    {       /* saturation region */

                        cgg = bsim2.pParam.Two_Third_CoxWL;

                        cgd = -cgg * dVth_dVd + dQbulk_dVd;
                        tmp = -cgg * dVth_dVb + dQbulk_dVb;

                        cgs = -(cgg + cgd + tmp);


                        cbg = 0.0;

                        cbd = -dQbulk_dVd;

                        cbs = dQbulk_dVd + dQbulk_dVb;


                        cdg = -0.4 * cgg;
                        tmp = -cdg * dVth_dVb;

                        cdd = -cdg * dVth_dVd;

                        cds = -(cdg + cdd + tmp);


                        qb = -Qbulk;

                        qg = bsim2.pParam.Two_Third_CoxWL * Vgst + Qbulk;

                        qd = cdg * Vgst;
                    }
                    else
                    {       /* linear region  */
                        T7 = Vds / Vdsat;
                        T8 = Vgst / Vdsat;
                        T6 = T7 * T8;
                        T9 = 1.0 - T7;
                        Vgdt = Vgst * T9;
                        T0 = Vgst / (Vgst + Vgdt);
                        T1 = Vgdt / (Vgst + Vgdt);
                        T5 = T0 * T1;
                        T2 = 1.0 - T1 + T5;
                        T3 = 1.0 - T0 + T5;

                        dVgdt_dVg = T9 + T6 * dVdsat_dVg;
                        dVgdt_dVd = T6 * dVdsat_dVd - T8 - T9 * dVth_dVd;
                        dVgdt_dVb = T6 * dVdsat_dVb - T9 * dVth_dVb;


                        qg = bsim2.pParam.Two_Third_CoxWL * (Vgst + Vgdt
                          - Vgdt * T0) + Qbulk;

                        qb = -Qbulk;

                        qd = -bsim2.pParam.One_Third_CoxWL * (0.2 * Vgdt
                      + 0.8 * Vgst + Vgdt * T1
                      + 0.2 * T5 * (Vgdt - Vgst));


                        cgg = bsim2.pParam.Two_Third_CoxWL * (T2 + T3 * dVgdt_dVg);
                        tmp = dQbulk_dVb + bsim2.pParam.Two_Third_CoxWL * (T3 * dVgdt_dVb
                                      - T2 * dVth_dVb);

                        cgd = bsim2.pParam.Two_Third_CoxWL * (T3 * dVgdt_dVd
                         - T2 * dVth_dVd) + dQbulk_dVd;

                        cgs = -(cgg + cgd + tmp);

                        T2 = 0.8 - 0.4 * T1 * (2.0 * T1 + T0 + T0 * (T1 - T0));
                        T3 = 0.2 + T1 + T0 * (1.0 - 0.4 * T0 * (T1 + 3.0 * T0));

                        cdg = -bsim2.pParam.One_Third_CoxWL * (T2 + T3 * dVgdt_dVg);
                        tmp = bsim2.pParam.One_Third_CoxWL * (T2 * dVth_dVb
                                  - T3 * dVgdt_dVb);

                        cdd = bsim2.pParam.One_Third_CoxWL * (T2 * dVth_dVd
                         - T3 * dVgdt_dVd);

                        cds = -(cdg + tmp + cdd);


                        cbg = 0.0;

                        cbd = -dQbulk_dVd;

                        cbs = dQbulk_dVd + dQbulk_dVb;
                    }
                }
            }

            finished:       /* returning Values to Calling Routine */
            valuetypeflag = (int)model.B2channelChargePartitionFlag;
            switch (valuetypeflag)
            {
                case 0:
                    Ids = Math.Max(Ids, 1e-50);
                    break;
                case -1:
                    Ids = Math.Max(Ids, 1e-50);
                    break;
                case -2:
                    Ids = gm;
                    break;
                case -3:
                    Ids = gds;
                    break;
                case -4:
                    Ids = 1.0 / gds;
                    break;
                case -5:
                    Ids = gmb;
                    break;
                case -6:
                    Ids = qg / 1.0e-12;
                    break;
                case -7:
                    Ids = qb / 1.0e-12;
                    break;
                case -8:
                    Ids = qd / 1.0e-12;
                    break;
                case -9:
                    Ids = -(qb + qg + qd) / 1.0e-12;
                    break;
                case -10:
                    Ids = cgg / 1.0e-12;
                    break;
                case -11:
                    Ids = cgd / 1.0e-12;
                    break;
                case -12:
                    Ids = cgs / 1.0e-12;
                    break;
                case -13:
                    Ids = -(cgg + cgd + cgs) / 1.0e-12;
                    break;
                case -14:
                    Ids = cbg / 1.0e-12;
                    break;
                case -15:
                    Ids = cbd / 1.0e-12;
                    break;
                case -16:
                    Ids = cbs / 1.0e-12;
                    break;
                case -17:
                    Ids = -(cbg + cbd + cbs) / 1.0e-12;
                    break;
                case -18:
                    Ids = cdg / 1.0e-12;
                    break;
                case -19:
                    Ids = cdd / 1.0e-12;
                    break;
                case -20:
                    Ids = cds / 1.0e-12;
                    break;
                case -21:
                    Ids = -(cdg + cdd + cds) / 1.0e-12;
                    break;
                case -22:
                    Ids = -(cgg + cdg + cbg) / 1.0e-12;
                    break;
                case -23:
                    Ids = -(cgd + cdd + cbd) / 1.0e-12;
                    break;
                case -24:
                    Ids = -(cgs + cds + cbs) / 1.0e-12;
                    break;
                default:
                    Ids = Math.Max(Ids, 1.0e-50);
                    break;
            }
            von = Vth;
            vdsat = Vdsat;
        }

        /// <summary>
        /// routine to calculate equivalent conductance and total terminal 
        /// charges
        /// </summary>
        /// <param name="ckt"></param>
        /// <param name="vgd"></param>
        /// <param name="vgs"></param>
        /// <param name="vgb"></param>
        /// <param name="args"></param>
        /// <param name="cbgb"></param>
        /// <param name="cbdb"></param>
        /// <param name="cbsb"></param>
        /// <param name="cdgb"></param>
        /// <param name="cddb"></param>
        /// <param name="cdsb"></param>
        /// <param name="gcggbPointer"></param>
        /// <param name="gcgdbPointer"></param>
        /// <param name="gcgsbPointer"></param>
        /// <param name="gcbgbPointer"></param>
        /// <param name="gcbdbPointer"></param>
        /// <param name="gcbsbPointer"></param>
        /// <param name="gcdgbPointer"></param>
        /// <param name="gcddbPointer"></param>
        /// <param name="gcdsbPointer"></param>
        /// <param name="gcsgbPointer"></param>
        /// <param name="gcsdbPointer"></param>
        /// <param name="gcssbPointer"></param>
        /// <param name="qGatePointer"></param>
        /// <param name="qBulkPointer"></param>
        /// <param name="qDrainPointer"></param>
        /// <param name="qSourcePointer"></param>
        public static void B2mosCap(Circuit ckt, double vgd, double vgs, double vgb, double[] args, double cbgb, double cbdb, double cbsb, double cdgb, double cddb, double cdsb,
            out double gcggbPointer, out double gcgdbPointer, out double gcgsbPointer, out double gcbgbPointer, out double gcbdbPointer,
            out double gcbsbPointer, out double gcdgbPointer, out double gcddbPointer, out double gcdsbPointer,
            out double gcsgbPointer, out double gcsdbPointer, out double gcssbPointer, ref double qGatePointer, ref double qBulkPointer,
            ref double qDrainPointer, out double qSourcePointer)
        {
            double qgd;
            double qgs;
            double qgb;
            double ag0;

            ag0 = ckt.Method.Slope;
            /* compute equivalent conductance */
            gcdgbPointer = (cdgb - args[0]) * ag0;
            gcddbPointer = (cddb + args[3] + args[0]) * ag0;
            gcdsbPointer = cdsb * ag0;
            gcsgbPointer = -(args[5] + cbgb + cdgb + args[1]) * ag0;
            gcsdbPointer = -(args[6] + cbdb + cddb) * ag0;
            gcssbPointer = (args[4] + args[1] -
                (args[7] + cbsb + cdsb)) * ag0;
            gcggbPointer = (args[5] + args[0] +
                args[1] + args[2]) * ag0;
            gcgdbPointer = (args[6] - args[0]) * ag0;
            gcgsbPointer = (args[7] - args[1]) * ag0;
            gcbgbPointer = (cbgb - args[2]) * ag0;
            gcbdbPointer = (cbdb - args[3]) * ag0;
            gcbsbPointer = (cbsb - args[4]) * ag0;

            /* compute total terminal charge */
            qgd = args[0] * vgd;
            qgs = args[1] * vgs;
            qgb = args[2] * vgb;
            qGatePointer = qGatePointer + qgd + qgs + qgb;
            qBulkPointer = qBulkPointer - qgb;
            qDrainPointer = qDrainPointer - qgd;
            qSourcePointer = -(qGatePointer + qBulkPointer + qDrainPointer);

        }
    }
}
