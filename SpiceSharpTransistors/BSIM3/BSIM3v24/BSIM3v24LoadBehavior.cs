using System;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.Transistors;
using SpiceSharp.Circuits;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// General behaviour for a <see cref="BSIM3v24"/>
    /// </summary>
    public class BSIM3v24LoadBehavior : CircuitObjectBehaviorLoad
    {
        /// <summary>
        /// Execute the behaviour
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Execute(Circuit ckt)
        {
            var bsim3 = ComponentTyped<BSIM3v24>();
            var model = bsim3.Model as BSIM3v24Model;
            var state = ckt.State;
            var rstate = state.Real;
            var method = ckt.Method;

            int Check;
            double vbs, vgs, vds, qdef, vbd, vgd, vgdo, delvbs, delvbd, delvgs, delvds, delvgd, Idtot, cdhat, Ibtot, cbhat, von,
                vgb, Nvtm, SourceSatCurrent, evbs, T0, DrainSatCurrent, evbd, Vds, Vgs, Vbs, T1, Vbseff, dVbseff_dVb, Phis, dPhis_dVb,
                sqrtPhis, dsqrtPhis_dVb, Xdep, dXdep_dVb, Leff, Vtm, T3, V0, T2, T4, lt1, dlt1_dVb, ltw, dltw_dVb, Theta0, dT1_dVb,
                dTheta0_dVb, Delt_vth, dDelt_vth_dVb, dT2_dVb, TempRatio, tmp2, T9, dDIBL_Sft_dVd, DIBL_Sft, Vth, dVth_dVb, dVth_dVd, tmp3,
                tmp4, n, dn_dVb, dn_dVd, T6, T5, Vgs_eff, dVgs_eff_dVg, Vgst, T10, VgstNVt, ExpArg, Vgsteff, dVgsteff_dVg, dVgsteff_dVd,
                dVgsteff_dVb, ExpVgst, dT1_dVg, dT1_dVd, dT2_dVg, dT2_dVd, Weff, dWeff_dVg, dWeff_dVb, Rds, dRds_dVg, dRds_dVb, tmp1, T7,
                Abulk0, dAbulk0_dVb, T8, dAbulk_dVg, Abulk, dAbulk_dVb, dT0_dVb, dDenomi_dVg, dDenomi_dVd, dDenomi_dVb, Denomi, ueff,
                dueff_dVg, dueff_dVd, dueff_dVb, WVCox, WVCoxRds, Esat, EsatL, dEsatL_dVg, dEsatL_dVd, dEsatL_dVb, a1, dLambda_dVg, Lambda,
                Vgst2Vtm, Vdsat, dT0_dVg, dT0_dVd, dVdsat_dVg, dVdsat_dVd, dVdsat_dVb, dT3_dVg, dT3_dVd, dT3_dVb, Vdseff, dVdseff_dVg,
                dVdseff_dVd, dVdseff_dVb, Vasat, dVasat_dVg, dVasat_dVb, dVasat_dVd, diffVds, VACLM, dVACLM_dVg, dVACLM_dVb, dVACLM_dVd,
                VADIBL, dVADIBL_dVg, dVADIBL_dVb, dVADIBL_dVd, Va, dVa_dVg, dVa_dVd, dVa_dVb, VASCBE, dVASCBE_dVg, dVASCBE_dVd, dVASCBE_dVb,
                CoxWovL, beta, dbeta_dVg, dbeta_dVd, dbeta_dVb, fgche1, dfgche1_dVg, dfgche1_dVd, dfgche1_dVb, fgche2, dfgche2_dVg,
                dfgche2_dVd, dfgche2_dVb, gche, dgche_dVg, dgche_dVd, dgche_dVb, Idl, dIdl_dVg, dIdl_dVd, dIdl_dVb, Idsa, dIdsa_dVg, dIdsa_dVd,
                dIdsa_dVb, Ids, Gm, Gds, Gmb, tmp, Isub, Gbg, Gbd, Gbb, cdrain, qgate = 0.0, Vfb, dVgst_dVb, dVgst_dVg, CoxWL, Arg1, qbulk = 0.0, qdrn = 0.0,
                One_Third_CoxWL, Two_Third_CoxWL, AbulkCV, dAbulkCV_dVb, Alphaz, T11, dAlphaz_dVg, dAlphaz_dVb, T12, VbseffCV, dVbseffCV_dVb,
                noff, dnoff_dVd, dnoff_dVb, voffcv, Cgg, Cgd, Cgb, Cbg, Cbd, Cbb, VdsatCV, dVdsatCV_dVg, dVdsatCV_dVb, Cgg1, Cgb1, Cgd1, Cbg1,
                Cbb1, Cbd1, qsrc, Csg, Csb, Csd, V3, Vfbeff, dVfbeff_dVg, dVfbeff_dVb, Qac0, dQac0_dVg, dQac0_dVb, Qsub0, dQsub0_dVg,
                dQsub0_dVd, dQsub0_dVb, V4, VdseffCV, dVdseffCV_dVg, dVdseffCV_dVd, dVdseffCV_dVb, qinoi, Cox, Tox, Tcen, dTcen_dVg, dTcen_dVb,
                LINK, Ccen, Coxeff, dCoxeff_dVg, dCoxeff_dVb, CoxWLcen, QovCox, DeltaPhi, dDeltaPhi_dVg, dDeltaPhi_dVd, dDeltaPhi_dVb,
                dTcen_dVd, dCoxeff_dVd, czbd, czbs, czbdswg, czbdsw, czbssw, czbsswg, MJ, MJSW, MJSWG, arg, sarg, qcheq = 0.0, gtau_drift, gtau_diff,
                cgdo, qgdo, cgso, qgso, ag0, gcggb, gcgdb, gcgsb, gcdgb, gcddb, gcdsb, gcsgb, gcsdb, gcssb, gcbgb, gcbdb, gcbsb, qgd, qgs, qgb,
                ggtg, sxpart, dxpart, ddxpart_dVd, dsxpart_dVd, ggtd, ggts, ggtb, gqdef = 0.0, gcqgb = 0.0, gcqdb = 0.0, gcqsb = 0.0, gcqbb = 0.0, Cdd, Cdg, ddxpart_dVg,
                Cds, Css, ddxpart_dVs, ddxpart_dVb, dsxpart_dVg, dsxpart_dVs, dsxpart_dVb, cqdef, ceqqg, cqcheq, cqgate, cqbulk, cqdrn, ceqqb,
                ceqqd, Gmbs, FwdSum, RevSum, cdreq, ceqbd, ceqbs, gbbdp, gbbsp, gbdpg, gbdpdp, gbdpb, gbdpsp, gbspg, gbspdp, gbspb, gbspsp;
            bool ChargeComputationNeeded = ((method != null || state.UseSmallSignal) || ((state.Domain == CircuitState.DomainTypes.Time &&
                state.UseDC) && state.UseIC)) ? true : false;

            Check = 1;
            if (state.UseSmallSignal)
            {
                vbs = state.States[0][bsim3.BSIM3states + BSIM3v24.BSIM3vbs];
                vgs = state.States[0][bsim3.BSIM3states + BSIM3v24.BSIM3vgs];
                vds = state.States[0][bsim3.BSIM3states + BSIM3v24.BSIM3vds];
                qdef = state.States[0][bsim3.BSIM3states + BSIM3v24.BSIM3qdef];
            }
            else if ((method != null && method.SavedTime == 0.0))
            {
                vbs = state.States[1][bsim3.BSIM3states + BSIM3v24.BSIM3vbs];
                vgs = state.States[1][bsim3.BSIM3states + BSIM3v24.BSIM3vgs];
                vds = state.States[1][bsim3.BSIM3states + BSIM3v24.BSIM3vds];
                qdef = state.States[1][bsim3.BSIM3states + BSIM3v24.BSIM3qdef];
            }
            else if ((state.Init == CircuitState.InitFlags.InitJct) && !bsim3.BSIM3off)
            {
                vds = model.BSIM3type * bsim3.BSIM3icVDS;
                vgs = model.BSIM3type * bsim3.BSIM3icVGS;
                vbs = model.BSIM3type * bsim3.BSIM3icVBS;
                qdef = 0.0;

                if ((vds == 0.0) && (vgs == 0.0) && (vbs == 0.0) && ((method != null || state.UseDC ||
                    state.Domain == CircuitState.DomainTypes.None) || (!state.UseIC)))
                {
                    vbs = 0.0;
                    vgs = model.BSIM3type * bsim3.pParam.BSIM3vth0 + 0.1;
                    vds = 0.1;
                }
            }
            else if ((state.Init == CircuitState.InitFlags.InitJct || state.Init == CircuitState.InitFlags.InitFix) && (bsim3.BSIM3off))
            {
                qdef = vbs = vgs = vds = 0.0;
            }
            else
            {
                /* PREDICTOR */
                vbs = model.BSIM3type * (rstate.OldSolution[bsim3.BSIM3bNode] - rstate.OldSolution[bsim3.BSIM3sNodePrime]);
                vgs = model.BSIM3type * (rstate.OldSolution[bsim3.BSIM3gNode] - rstate.OldSolution[bsim3.BSIM3sNodePrime]);
                vds = model.BSIM3type * (rstate.OldSolution[bsim3.BSIM3dNodePrime] - rstate.OldSolution[bsim3.BSIM3sNodePrime]);
                qdef = model.BSIM3type * (rstate.OldSolution[bsim3.BSIM3qNode]);
                /* PREDICTOR */

                vbd = vbs - vds;
                vgd = vgs - vds;
                vgdo = state.States[0][bsim3.BSIM3states + BSIM3v24.BSIM3vgs] - state.States[0][bsim3.BSIM3states + BSIM3v24.BSIM3vds];
                delvbs = vbs - state.States[0][bsim3.BSIM3states + BSIM3v24.BSIM3vbs];
                delvbd = vbd - state.States[0][bsim3.BSIM3states + BSIM3v24.BSIM3vbd];
                delvgs = vgs - state.States[0][bsim3.BSIM3states + BSIM3v24.BSIM3vgs];
                delvds = vds - state.States[0][bsim3.BSIM3states + BSIM3v24.BSIM3vds];
                delvgd = vgd - vgdo;

                if (bsim3.BSIM3mode >= 0)
                {
                    Idtot = bsim3.BSIM3cd + bsim3.BSIM3csub - bsim3.BSIM3cbd;
                    cdhat = Idtot - bsim3.BSIM3gbd * delvbd + (bsim3.BSIM3gmbs + bsim3.BSIM3gbbs) * delvbs + (bsim3.BSIM3gm + bsim3.BSIM3gbgs) * delvgs + (bsim3.BSIM3gds + bsim3.BSIM3gbds) *
                        delvds;
                    Ibtot = bsim3.BSIM3cbs + bsim3.BSIM3cbd - bsim3.BSIM3csub;
                    cbhat = Ibtot + bsim3.BSIM3gbd * delvbd + (bsim3.BSIM3gbs - bsim3.BSIM3gbbs) * delvbs - bsim3.BSIM3gbgs * delvgs - bsim3.BSIM3gbds * delvds;
                }
                else
                {
                    Idtot = bsim3.BSIM3cd - bsim3.BSIM3cbd;
                    cdhat = Idtot - (bsim3.BSIM3gbd - bsim3.BSIM3gmbs) * delvbd + bsim3.BSIM3gm * delvgd - bsim3.BSIM3gds * delvds;
                    Ibtot = bsim3.BSIM3cbs + bsim3.BSIM3cbd - bsim3.BSIM3csub;
                    cbhat = Ibtot + bsim3.BSIM3gbs * delvbs + (bsim3.BSIM3gbd - bsim3.BSIM3gbbs) * delvbd - bsim3.BSIM3gbgs * delvgd + bsim3.BSIM3gbds * delvds;
                }

                /* NOBYPASS */
                von = bsim3.BSIM3von;
                if (state.States[0][bsim3.BSIM3states + BSIM3v24.BSIM3vds] >= 0.0)
                {
                    vgs = Transistor.DEVfetlim(vgs, state.States[0][bsim3.BSIM3states + BSIM3v24.BSIM3vgs], von);
                    vds = vgs - vgd;
                    vds = Transistor.DEVlimvds(vds, state.States[0][bsim3.BSIM3states + BSIM3v24.BSIM3vds]);
                    vgd = vgs - vds;

                }
                else
                {
                    vgd = Transistor.DEVfetlim(vgd, vgdo, von);
                    vds = vgs - vgd;
                    vds = -Transistor.DEVlimvds(-vds, -(state.States[0][bsim3.BSIM3states + BSIM3v24.BSIM3vds]));
                    vgs = vgd + vds;
                }

                if (vds >= 0.0)
                {
                    vbs = Transistor.DEVpnjlim(vbs, state.States[0][bsim3.BSIM3states + BSIM3v24.BSIM3vbs], Circuit.CONSTvt0, model.BSIM3vcrit, ref Check);
                    vbd = vbs - vds;
                }
                else
                {
                    vbd = Transistor.DEVpnjlim(vbd, state.States[0][bsim3.BSIM3states + BSIM3v24.BSIM3vbd], Circuit.CONSTvt0, model.BSIM3vcrit, ref Check);
                    vbs = vbd + vds;
                }
            }

            /* determine DC current and derivatives */
            vbd = vbs - vds;
            vgd = vgs - vds;
            vgb = vgs - vbs;

            /* Source / drain junction diode DC model begins */
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
            if (SourceSatCurrent <= 0.0)
            {
                bsim3.BSIM3gbs = state.Gmin;
                bsim3.BSIM3cbs = bsim3.BSIM3gbs * vbs;
            }
            else
            {
                if (model.BSIM3ijth.Value == 0.0)
                {
                    evbs = Math.Exp(vbs / Nvtm);
                    bsim3.BSIM3gbs = SourceSatCurrent * evbs / Nvtm + state.Gmin;
                    bsim3.BSIM3cbs = SourceSatCurrent * (evbs - 1.0) + state.Gmin * vbs;
                }
                else
                {
                    if (vbs < bsim3.BSIM3vjsm)
                    {
                        evbs = Math.Exp(vbs / Nvtm);
                        bsim3.BSIM3gbs = SourceSatCurrent * evbs / Nvtm + state.Gmin;
                        bsim3.BSIM3cbs = SourceSatCurrent * (evbs - 1.0) + state.Gmin * vbs;
                    }
                    else
                    {
                        T0 = bsim3.BSIM3IsEvjsm / Nvtm;
                        bsim3.BSIM3gbs = T0 + state.Gmin;
                        bsim3.BSIM3cbs = bsim3.BSIM3IsEvjsm - SourceSatCurrent + T0 * (vbs - bsim3.BSIM3vjsm) + state.Gmin * vbs;
                    }
                }
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
            if (DrainSatCurrent <= 0.0)
            {
                bsim3.BSIM3gbd = state.Gmin;
                bsim3.BSIM3cbd = bsim3.BSIM3gbd * vbd;
            }
            else
            {
                if (model.BSIM3ijth.Value == 0.0)
                {
                    evbd = Math.Exp(vbd / Nvtm);
                    bsim3.BSIM3gbd = DrainSatCurrent * evbd / Nvtm + state.Gmin;
                    bsim3.BSIM3cbd = DrainSatCurrent * (evbd - 1.0) + state.Gmin * vbd;
                }
                else
                {
                    if (vbd < bsim3.BSIM3vjdm)
                    {
                        evbd = Math.Exp(vbd / Nvtm);
                        bsim3.BSIM3gbd = DrainSatCurrent * evbd / Nvtm + state.Gmin;
                        bsim3.BSIM3cbd = DrainSatCurrent * (evbd - 1.0) + state.Gmin * vbd;
                    }
                    else
                    {
                        T0 = bsim3.BSIM3IsEvjdm / Nvtm;
                        bsim3.BSIM3gbd = T0 + state.Gmin;
                        bsim3.BSIM3cbd = bsim3.BSIM3IsEvjdm - DrainSatCurrent + T0 * (vbd - bsim3.BSIM3vjdm) + state.Gmin * vbd;
                    }
                }
            }
            /* End of diode DC model */

            if (vds >= 0.0)
            {
                /* normal mode */
                bsim3.BSIM3mode = 1;
                Vds = vds;
                Vgs = vgs;
                Vbs = vbs;
            }
            else
            {
                /* inverse mode */
                bsim3.BSIM3mode = -1;
                Vds = -vds;
                Vgs = vgd;
                Vbs = vbd;
            }

            T0 = Vbs - bsim3.pParam.BSIM3vbsc - 0.001;
            T1 = Math.Sqrt(T0 * T0 - 0.004 * bsim3.pParam.BSIM3vbsc);
            Vbseff = bsim3.pParam.BSIM3vbsc + 0.5 * (T0 + T1);
            dVbseff_dVb = 0.5 * (1.0 + T0 / T1);
            if (Vbseff < Vbs)
            {
                Vbseff = Vbs;
            }

            if (Vbseff > 0.0)
            {
                T0 = bsim3.pParam.BSIM3phi / (bsim3.pParam.BSIM3phi + Vbseff);
                Phis = bsim3.pParam.BSIM3phi * T0;
                dPhis_dVb = -T0 * T0;
                sqrtPhis = bsim3.pParam.BSIM3phis3 / (bsim3.pParam.BSIM3phi + 0.5 * Vbseff);
                dsqrtPhis_dVb = -0.5 * sqrtPhis * sqrtPhis / bsim3.pParam.BSIM3phis3;
            }
            else
            {
                Phis = bsim3.pParam.BSIM3phi - Vbseff;
                dPhis_dVb = -1.0;
                sqrtPhis = Math.Sqrt(Phis);
                dsqrtPhis_dVb = -0.5 / sqrtPhis;
            }
            Xdep = bsim3.pParam.BSIM3Xdep0 * sqrtPhis / bsim3.pParam.BSIM3sqrtPhi;
            dXdep_dVb = (bsim3.pParam.BSIM3Xdep0 / bsim3.pParam.BSIM3sqrtPhi) * dsqrtPhis_dVb;

            Leff = bsim3.pParam.BSIM3leff;
            Vtm = model.BSIM3vtm;
            /* Vth Calculation */
            T3 = Math.Sqrt(Xdep);
            V0 = bsim3.pParam.BSIM3vbi - bsim3.pParam.BSIM3phi;

            T0 = bsim3.pParam.BSIM3dvt2 * Vbseff;
            if (T0 >= -0.5)
            {
                T1 = 1.0 + T0;
                T2 = bsim3.pParam.BSIM3dvt2;
            }
            else /* Added to avoid any discontinuity problems caused by dvt2 */
            {
                T4 = 1.0 / (3.0 + 8.0 * T0);
                T1 = (1.0 + 3.0 * T0) * T4;
                T2 = bsim3.pParam.BSIM3dvt2 * T4 * T4;
            }
            lt1 = model.BSIM3factor1 * T3 * T1;
            dlt1_dVb = model.BSIM3factor1 * (0.5 / T3 * T1 * dXdep_dVb + T3 * T2);

            T0 = bsim3.pParam.BSIM3dvt2w * Vbseff;
            if (T0 >= -0.5)
            {
                T1 = 1.0 + T0;
                T2 = bsim3.pParam.BSIM3dvt2w;
            }
            else /* Added to avoid any discontinuity problems caused by dvt2w */
            {
                T4 = 1.0 / (3.0 + 8.0 * T0);
                T1 = (1.0 + 3.0 * T0) * T4;
                T2 = bsim3.pParam.BSIM3dvt2w * T4 * T4;
            }
            ltw = model.BSIM3factor1 * T3 * T1;
            dltw_dVb = model.BSIM3factor1 * (0.5 / T3 * T1 * dXdep_dVb + T3 * T2);

            T0 = -0.5 * bsim3.pParam.BSIM3dvt1 * Leff / lt1;
            if (T0 > -Transistor.EXP_THRESHOLD)
            {
                T1 = Math.Exp(T0);
                Theta0 = T1 * (1.0 + 2.0 * T1);
                dT1_dVb = -T0 / lt1 * T1 * dlt1_dVb;
                dTheta0_dVb = (1.0 + 4.0 * T1) * dT1_dVb;
            }
            else
            {
                T1 = Transistor.MIN_EXP;
                Theta0 = T1 * (1.0 + 2.0 * T1);
                dTheta0_dVb = 0.0;
            }

            bsim3.BSIM3thetavth = bsim3.pParam.BSIM3dvt0 * Theta0;
            Delt_vth = bsim3.BSIM3thetavth * V0;
            dDelt_vth_dVb = bsim3.pParam.BSIM3dvt0 * dTheta0_dVb * V0;

            T0 = -0.5 * bsim3.pParam.BSIM3dvt1w * bsim3.pParam.BSIM3weff * Leff / ltw;
            if (T0 > -Transistor.EXP_THRESHOLD)
            {
                T1 = Math.Exp(T0);
                T2 = T1 * (1.0 + 2.0 * T1);
                dT1_dVb = -T0 / ltw * T1 * dltw_dVb;
                dT2_dVb = (1.0 + 4.0 * T1) * dT1_dVb;
            }
            else
            {
                T1 = Transistor.MIN_EXP;
                T2 = T1 * (1.0 + 2.0 * T1);
                dT2_dVb = 0.0;
            }

            T0 = bsim3.pParam.BSIM3dvt0w * T2;
            T2 = T0 * V0;
            dT2_dVb = bsim3.pParam.BSIM3dvt0w * dT2_dVb * V0;

            TempRatio = state.Temperature / model.BSIM3tnom - 1.0;
            T0 = Math.Sqrt(1.0 + bsim3.pParam.BSIM3nlx / Leff);
            T1 = bsim3.pParam.BSIM3k1ox * (T0 - 1.0) * bsim3.pParam.BSIM3sqrtPhi + (bsim3.pParam.BSIM3kt1 + bsim3.pParam.BSIM3kt1l / Leff + bsim3.pParam.BSIM3kt2 *
                Vbseff) * TempRatio;
            tmp2 = model.BSIM3tox * bsim3.pParam.BSIM3phi / (bsim3.pParam.BSIM3weff + bsim3.pParam.BSIM3w0);

            T3 = bsim3.pParam.BSIM3eta0 + bsim3.pParam.BSIM3etab * Vbseff;
            if (T3 < 1.0e-4)
            /* avoid  discontinuity problems caused by etab */
            {
                T9 = 1.0 / (3.0 - 2.0e4 * T3);
                T3 = (2.0e-4 - T3) * T9;
                T4 = T9 * T9;
            }
            else
            {
                T4 = 1.0;
            }
            dDIBL_Sft_dVd = T3 * bsim3.pParam.BSIM3theta0vb0;
            DIBL_Sft = dDIBL_Sft_dVd * Vds;

            Vth = model.BSIM3type * bsim3.pParam.BSIM3vth0 - bsim3.pParam.BSIM3k1 * bsim3.pParam.BSIM3sqrtPhi + bsim3.pParam.BSIM3k1ox * sqrtPhis -
                bsim3.pParam.BSIM3k2ox * Vbseff - Delt_vth - T2 + (bsim3.pParam.BSIM3k3 + bsim3.pParam.BSIM3k3b * Vbseff) * tmp2 + T1 - DIBL_Sft;

            bsim3.BSIM3von = Vth;

            dVth_dVb = bsim3.pParam.BSIM3k1ox * dsqrtPhis_dVb - bsim3.pParam.BSIM3k2ox - dDelt_vth_dVb - dT2_dVb + bsim3.pParam.BSIM3k3b * tmp2 -
                bsim3.pParam.BSIM3etab * Vds * bsim3.pParam.BSIM3theta0vb0 * T4 + bsim3.pParam.BSIM3kt2 * TempRatio;
            dVth_dVd = -dDIBL_Sft_dVd;

            /* Calculate n */
            tmp2 = bsim3.pParam.BSIM3nfactor * Transistor.EPSSI / Xdep;
            tmp3 = bsim3.pParam.BSIM3cdsc + bsim3.pParam.BSIM3cdscb * Vbseff + bsim3.pParam.BSIM3cdscd * Vds;
            tmp4 = (tmp2 + tmp3 * Theta0 + bsim3.pParam.BSIM3cit) / model.BSIM3cox;
            if (tmp4 >= -0.5)
            {
                n = 1.0 + tmp4;
                dn_dVb = (-tmp2 / Xdep * dXdep_dVb + tmp3 * dTheta0_dVb + bsim3.pParam.BSIM3cdscb * Theta0) / model.BSIM3cox;
                dn_dVd = bsim3.pParam.BSIM3cdscd * Theta0 / model.BSIM3cox;
            }
            else
            /* avoid  discontinuity problems caused by tmp4 */
            {
                T0 = 1.0 / (3.0 + 8.0 * tmp4);
                n = (1.0 + 3.0 * tmp4) * T0;
                T0 *= T0;
                dn_dVb = (-tmp2 / Xdep * dXdep_dVb + tmp3 * dTheta0_dVb + bsim3.pParam.BSIM3cdscb * Theta0) / model.BSIM3cox * T0;
                dn_dVd = bsim3.pParam.BSIM3cdscd * Theta0 / model.BSIM3cox * T0;
            }

            /* Poly Gate Si Depletion Effect */
            T0 = bsim3.pParam.BSIM3vfb + bsim3.pParam.BSIM3phi;
            if ((bsim3.pParam.BSIM3ngate > 1.0e18) && (bsim3.pParam.BSIM3ngate < 1.0e25) && (Vgs > T0))
            /* added to avoid the problem caused by ngate */
            {
                T1 = 1.0e6 * Transistor.Charge_q * Transistor.EPSSI * bsim3.pParam.BSIM3ngate / (model.BSIM3cox * model.BSIM3cox);
                T4 = Math.Sqrt(1.0 + 2.0 * (Vgs - T0) / T1);
                T2 = T1 * (T4 - 1.0);
                T3 = 0.5 * T2 * T2 / T1; /* T3 = Vpoly */
                T7 = 1.12 - T3 - 0.05;
                T6 = Math.Sqrt(T7 * T7 + 0.224);
                T5 = 1.12 - 0.5 * (T7 + T6);
                Vgs_eff = Vgs - T5;
                dVgs_eff_dVg = 1.0 - (0.5 - 0.5 / T4) * (1.0 + T7 / T6);
            }
            else
            {
                Vgs_eff = Vgs;
                dVgs_eff_dVg = 1.0;
            }
            Vgst = Vgs_eff - Vth;

            /* Effective Vgst (Vgsteff) Calculation */

            T10 = 2.0 * n * Vtm;
            VgstNVt = Vgst / T10;
            ExpArg = (2.0 * bsim3.pParam.BSIM3voff - Vgst) / T10;

            /* MCJ: Very small Vgst */
            if (VgstNVt > Transistor.EXP_THRESHOLD)
            {
                Vgsteff = Vgst;
                dVgsteff_dVg = dVgs_eff_dVg;
                dVgsteff_dVd = -dVth_dVd;
                dVgsteff_dVb = -dVth_dVb;
            }
            else if (ExpArg > Transistor.EXP_THRESHOLD)
            {
                T0 = (Vgst - bsim3.pParam.BSIM3voff) / (n * Vtm);
                ExpVgst = Math.Exp(T0);
                Vgsteff = Vtm * bsim3.pParam.BSIM3cdep0 / model.BSIM3cox * ExpVgst;
                dVgsteff_dVg = Vgsteff / (n * Vtm);
                dVgsteff_dVd = -dVgsteff_dVg * (dVth_dVd + T0 * Vtm * dn_dVd);
                dVgsteff_dVb = -dVgsteff_dVg * (dVth_dVb + T0 * Vtm * dn_dVb);
                dVgsteff_dVg *= dVgs_eff_dVg;
            }
            else
            {
                ExpVgst = Math.Exp(VgstNVt);
                T1 = T10 * Math.Log(1.0 + ExpVgst);
                dT1_dVg = ExpVgst / (1.0 + ExpVgst);
                dT1_dVb = -dT1_dVg * (dVth_dVb + Vgst / n * dn_dVb) + T1 / n * dn_dVb;
                dT1_dVd = -dT1_dVg * (dVth_dVd + Vgst / n * dn_dVd) + T1 / n * dn_dVd;

                dT2_dVg = -model.BSIM3cox / (Vtm * bsim3.pParam.BSIM3cdep0) * Math.Exp(ExpArg);
                T2 = 1.0 - T10 * dT2_dVg;
                dT2_dVd = -dT2_dVg * (dVth_dVd - 2.0 * Vtm * ExpArg * dn_dVd) + (T2 - 1.0) / n * dn_dVd;
                dT2_dVb = -dT2_dVg * (dVth_dVb - 2.0 * Vtm * ExpArg * dn_dVb) + (T2 - 1.0) / n * dn_dVb;

                Vgsteff = T1 / T2;
                T3 = T2 * T2;
                dVgsteff_dVg = (T2 * dT1_dVg - T1 * dT2_dVg) / T3 * dVgs_eff_dVg;
                dVgsteff_dVd = (T2 * dT1_dVd - T1 * dT2_dVd) / T3;
                dVgsteff_dVb = (T2 * dT1_dVb - T1 * dT2_dVb) / T3;
            }
            bsim3.BSIM3Vgsteff = Vgsteff;

            /* Calculate Effective Channel Geometry */
            T9 = sqrtPhis - bsim3.pParam.BSIM3sqrtPhi;
            Weff = bsim3.pParam.BSIM3weff - 2.0 * (bsim3.pParam.BSIM3dwg * Vgsteff + bsim3.pParam.BSIM3dwb * T9);
            dWeff_dVg = -2.0 * bsim3.pParam.BSIM3dwg;
            dWeff_dVb = -2.0 * bsim3.pParam.BSIM3dwb * dsqrtPhis_dVb;

            if (Weff < 2.0e-8)
            /* to avoid the discontinuity problem due to Weff */
            {
                T0 = 1.0 / (6.0e-8 - 2.0 * Weff);
                Weff = 2.0e-8 * (4.0e-8 - Weff) * T0;
                T0 *= T0 * 4.0e-16;
                dWeff_dVg *= T0;
                dWeff_dVb *= T0;
            }

            T0 = bsim3.pParam.BSIM3prwg * Vgsteff + bsim3.pParam.BSIM3prwb * T9;
            if (T0 >= -0.9)
            {
                Rds = bsim3.pParam.BSIM3rds0 * (1.0 + T0);
                dRds_dVg = bsim3.pParam.BSIM3rds0 * bsim3.pParam.BSIM3prwg;
                dRds_dVb = bsim3.pParam.BSIM3rds0 * bsim3.pParam.BSIM3prwb * dsqrtPhis_dVb;
            }
            else
            /* to avoid the discontinuity problem due to prwg and prwb */
            {
                T1 = 1.0 / (17.0 + 20.0 * T0);
                Rds = bsim3.pParam.BSIM3rds0 * (0.8 + T0) * T1;
                T1 *= T1;
                dRds_dVg = bsim3.pParam.BSIM3rds0 * bsim3.pParam.BSIM3prwg * T1;
                dRds_dVb = bsim3.pParam.BSIM3rds0 * bsim3.pParam.BSIM3prwb * dsqrtPhis_dVb * T1;
            }
            bsim3.BSIM3rds = Rds; /* Noise Bugfix */

            /* Calculate Abulk */
            T1 = 0.5 * bsim3.pParam.BSIM3k1ox / sqrtPhis;
            dT1_dVb = -T1 / sqrtPhis * dsqrtPhis_dVb;

            T9 = Math.Sqrt(bsim3.pParam.BSIM3xj * Xdep);
            tmp1 = Leff + 2.0 * T9;
            T5 = Leff / tmp1;
            tmp2 = bsim3.pParam.BSIM3a0 * T5;
            tmp3 = bsim3.pParam.BSIM3weff + bsim3.pParam.BSIM3b1;
            tmp4 = bsim3.pParam.BSIM3b0 / tmp3;
            T2 = tmp2 + tmp4;
            dT2_dVb = -T9 / tmp1 / Xdep * dXdep_dVb;
            T6 = T5 * T5;
            T7 = T5 * T6;

            Abulk0 = 1.0 + T1 * T2;
            dAbulk0_dVb = T1 * tmp2 * dT2_dVb + T2 * dT1_dVb;

            T8 = bsim3.pParam.BSIM3ags * bsim3.pParam.BSIM3a0 * T7;
            dAbulk_dVg = -T1 * T8;
            Abulk = Abulk0 + dAbulk_dVg * Vgsteff;
            dAbulk_dVb = dAbulk0_dVb - T8 * Vgsteff * (dT1_dVb + 3.0 * T1 * dT2_dVb);

            if (Abulk0 < 0.1)
            /* added to avoid the problems caused by Abulk0 */
            {
                T9 = 1.0 / (3.0 - 20.0 * Abulk0);
                Abulk0 = (0.2 - Abulk0) * T9;
                dAbulk0_dVb *= T9 * T9;
            }

            if (Abulk < 0.1)
            /* added to avoid the problems caused by Abulk */
            {
                T9 = 1.0 / (3.0 - 20.0 * Abulk);
                Abulk = (0.2 - Abulk) * T9;
                T10 = T9 * T9;
                dAbulk_dVb *= T10;
                dAbulk_dVg *= T10;
            }
            bsim3.BSIM3Abulk = Abulk;

            T2 = bsim3.pParam.BSIM3keta * Vbseff;
            if (T2 >= -0.9)
            {
                T0 = 1.0 / (1.0 + T2);
                dT0_dVb = -bsim3.pParam.BSIM3keta * T0 * T0;
            }
            else
            /* added to avoid the problems caused by Keta */
            {
                T1 = 1.0 / (0.8 + T2);
                T0 = (17.0 + 20.0 * T2) * T1;
                dT0_dVb = -bsim3.pParam.BSIM3keta * T1 * T1;
            }
            dAbulk_dVg *= T0;
            dAbulk_dVb = dAbulk_dVb * T0 + Abulk * dT0_dVb;
            dAbulk0_dVb = dAbulk0_dVb * T0 + Abulk0 * dT0_dVb;
            Abulk *= T0;
            Abulk0 *= T0;

            /* Mobility calculation */
            if (model.BSIM3mobMod.Value == 1)
            {
                T0 = Vgsteff + Vth + Vth;
                T2 = bsim3.pParam.BSIM3ua + bsim3.pParam.BSIM3uc * Vbseff;
                T3 = T0 / model.BSIM3tox;
                T5 = T3 * (T2 + bsim3.pParam.BSIM3ub * T3);
                dDenomi_dVg = (T2 + 2.0 * bsim3.pParam.BSIM3ub * T3) / model.BSIM3tox;
                dDenomi_dVd = dDenomi_dVg * 2.0 * dVth_dVd;
                dDenomi_dVb = dDenomi_dVg * 2.0 * dVth_dVb + bsim3.pParam.BSIM3uc * T3;
            }
            else if (model.BSIM3mobMod.Value == 2)
            {
                T5 = Vgsteff / model.BSIM3tox * (bsim3.pParam.BSIM3ua + bsim3.pParam.BSIM3uc * Vbseff + bsim3.pParam.BSIM3ub * Vgsteff / model.BSIM3tox);
                dDenomi_dVg = (bsim3.pParam.BSIM3ua + bsim3.pParam.BSIM3uc * Vbseff + 2.0 * bsim3.pParam.BSIM3ub * Vgsteff / model.BSIM3tox) / model.BSIM3tox;
                dDenomi_dVd = 0.0;
                dDenomi_dVb = Vgsteff * bsim3.pParam.BSIM3uc / model.BSIM3tox;
            }
            else
            {
                T0 = Vgsteff + Vth + Vth;
                T2 = 1.0 + bsim3.pParam.BSIM3uc * Vbseff;
                T3 = T0 / model.BSIM3tox;
                T4 = T3 * (bsim3.pParam.BSIM3ua + bsim3.pParam.BSIM3ub * T3);
                T5 = T4 * T2;
                dDenomi_dVg = (bsim3.pParam.BSIM3ua + 2.0 * bsim3.pParam.BSIM3ub * T3) * T2 / model.BSIM3tox;
                dDenomi_dVd = dDenomi_dVg * 2.0 * dVth_dVd;
                dDenomi_dVb = dDenomi_dVg * 2.0 * dVth_dVb + bsim3.pParam.BSIM3uc * T4;
            }

            if (T5 >= -0.8)
            {
                Denomi = 1.0 + T5;
            }
            else /* Added to avoid the discontinuity problem caused by ua and ub */
            {
                T9 = 1.0 / (7.0 + 10.0 * T5);
                Denomi = (0.6 + T5) * T9;
                T9 *= T9;
                dDenomi_dVg *= T9;
                dDenomi_dVd *= T9;
                dDenomi_dVb *= T9;
            }

            bsim3.BSIM3ueff = ueff = bsim3.pParam.BSIM3u0temp / Denomi;
            T9 = -ueff / Denomi;
            dueff_dVg = T9 * dDenomi_dVg;
            dueff_dVd = T9 * dDenomi_dVd;
            dueff_dVb = T9 * dDenomi_dVb;

            /* Saturation Drain Voltage  Vdsat */
            WVCox = Weff * bsim3.pParam.BSIM3vsattemp * model.BSIM3cox;
            WVCoxRds = WVCox * Rds;

            Esat = 2.0 * bsim3.pParam.BSIM3vsattemp / ueff;
            EsatL = Esat * Leff;
            T0 = -EsatL / ueff;
            dEsatL_dVg = T0 * dueff_dVg;
            dEsatL_dVd = T0 * dueff_dVd;
            dEsatL_dVb = T0 * dueff_dVb;

            /* Sqrt() */
            a1 = bsim3.pParam.BSIM3a1;
            if (a1 == 0.0)
            {
                Lambda = bsim3.pParam.BSIM3a2;
                dLambda_dVg = 0.0;
            }
            else if (a1 > 0.0)
            /* Added to avoid the discontinuity problem
            caused by a1 and a2 (Lambda) */
            {
                T0 = 1.0 - bsim3.pParam.BSIM3a2;
                T1 = T0 - bsim3.pParam.BSIM3a1 * Vgsteff - 0.0001;
                T2 = Math.Sqrt(T1 * T1 + 0.0004 * T0);
                Lambda = bsim3.pParam.BSIM3a2 + T0 - 0.5 * (T1 + T2);
                dLambda_dVg = 0.5 * bsim3.pParam.BSIM3a1 * (1.0 + T1 / T2);
            }
            else
            {
                T1 = bsim3.pParam.BSIM3a2 + bsim3.pParam.BSIM3a1 * Vgsteff - 0.0001;
                T2 = Math.Sqrt(T1 * T1 + 0.0004 * bsim3.pParam.BSIM3a2);
                Lambda = 0.5 * (T1 + T2);
                dLambda_dVg = 0.5 * bsim3.pParam.BSIM3a1 * (1.0 + T1 / T2);
            }

            Vgst2Vtm = Vgsteff + 2.0 * Vtm;
            bsim3.BSIM3AbovVgst2Vtm = Abulk / Vgst2Vtm;

            if (Rds > 0)
            {
                tmp2 = dRds_dVg / Rds + dWeff_dVg / Weff;
                tmp3 = dRds_dVb / Rds + dWeff_dVb / Weff;
            }
            else
            {
                tmp2 = dWeff_dVg / Weff;
                tmp3 = dWeff_dVb / Weff;
            }
            if ((Rds == 0.0) && (Lambda == 1.0))
            {
                T0 = 1.0 / (Abulk * EsatL + Vgst2Vtm);
                tmp1 = 0.0;
                T1 = T0 * T0;
                T2 = Vgst2Vtm * T0;
                T3 = EsatL * Vgst2Vtm;
                Vdsat = T3 * T0;

                dT0_dVg = -(Abulk * dEsatL_dVg + EsatL * dAbulk_dVg + 1.0) * T1;
                dT0_dVd = -(Abulk * dEsatL_dVd) * T1;
                dT0_dVb = -(Abulk * dEsatL_dVb + dAbulk_dVb * EsatL) * T1;

                dVdsat_dVg = T3 * dT0_dVg + T2 * dEsatL_dVg + EsatL * T0;
                dVdsat_dVd = T3 * dT0_dVd + T2 * dEsatL_dVd;
                dVdsat_dVb = T3 * dT0_dVb + T2 * dEsatL_dVb;
            }
            else
            {
                tmp1 = dLambda_dVg / (Lambda * Lambda);
                T9 = Abulk * WVCoxRds;
                T8 = Abulk * T9;
                T7 = Vgst2Vtm * T9;
                T6 = Vgst2Vtm * WVCoxRds;
                T0 = 2.0 * Abulk * (T9 - 1.0 + 1.0 / Lambda);
                dT0_dVg = 2.0 * (T8 * tmp2 - Abulk * tmp1 + (2.0 * T9 + 1.0 / Lambda - 1.0) * dAbulk_dVg);

                dT0_dVb = 2.0 * (T8 * (2.0 / Abulk * dAbulk_dVb + tmp3) + (1.0 / Lambda - 1.0) * dAbulk_dVb);
                dT0_dVd = 0.0;
                T1 = Vgst2Vtm * (2.0 / Lambda - 1.0) + Abulk * EsatL + 3.0 * T7;

                dT1_dVg = (2.0 / Lambda - 1.0) - 2.0 * Vgst2Vtm * tmp1 + Abulk * dEsatL_dVg + EsatL * dAbulk_dVg + 3.0 * (T9 + T7 * tmp2 + T6 *
                    dAbulk_dVg);
                dT1_dVb = Abulk * dEsatL_dVb + EsatL * dAbulk_dVb + 3.0 * (T6 * dAbulk_dVb + T7 * tmp3);
                dT1_dVd = Abulk * dEsatL_dVd;

                T2 = Vgst2Vtm * (EsatL + 2.0 * T6);
                dT2_dVg = EsatL + Vgst2Vtm * dEsatL_dVg + T6 * (4.0 + 2.0 * Vgst2Vtm * tmp2);
                dT2_dVb = Vgst2Vtm * (dEsatL_dVb + 2.0 * T6 * tmp3);
                dT2_dVd = Vgst2Vtm * dEsatL_dVd;

                T3 = Math.Sqrt(T1 * T1 - 2.0 * T0 * T2);
                Vdsat = (T1 - T3) / T0;

                dT3_dVg = (T1 * dT1_dVg - 2.0 * (T0 * dT2_dVg + T2 * dT0_dVg)) / T3;
                dT3_dVd = (T1 * dT1_dVd - 2.0 * (T0 * dT2_dVd + T2 * dT0_dVd)) / T3;
                dT3_dVb = (T1 * dT1_dVb - 2.0 * (T0 * dT2_dVb + T2 * dT0_dVb)) / T3;

                dVdsat_dVg = (dT1_dVg - (T1 * dT1_dVg - dT0_dVg * T2 - T0 * dT2_dVg) / T3 - Vdsat * dT0_dVg) / T0;
                dVdsat_dVb = (dT1_dVb - (T1 * dT1_dVb - dT0_dVb * T2 - T0 * dT2_dVb) / T3 - Vdsat * dT0_dVb) / T0;
                dVdsat_dVd = (dT1_dVd - (T1 * dT1_dVd - T0 * dT2_dVd) / T3) / T0;
            }
            bsim3.BSIM3vdsat = Vdsat;

            /* Effective Vds (Vdseff) Calculation */
            T1 = Vdsat - Vds - bsim3.pParam.BSIM3delta;
            dT1_dVg = dVdsat_dVg;
            dT1_dVd = dVdsat_dVd - 1.0;
            dT1_dVb = dVdsat_dVb;

            T2 = Math.Sqrt(T1 * T1 + 4.0 * bsim3.pParam.BSIM3delta * Vdsat);
            T0 = T1 / T2;
            T3 = 2.0 * bsim3.pParam.BSIM3delta / T2;
            dT2_dVg = T0 * dT1_dVg + T3 * dVdsat_dVg;
            dT2_dVd = T0 * dT1_dVd + T3 * dVdsat_dVd;
            dT2_dVb = T0 * dT1_dVb + T3 * dVdsat_dVb;

            Vdseff = Vdsat - 0.5 * (T1 + T2);
            dVdseff_dVg = dVdsat_dVg - 0.5 * (dT1_dVg + dT2_dVg);
            dVdseff_dVd = dVdsat_dVd - 0.5 * (dT1_dVd + dT2_dVd);
            dVdseff_dVb = dVdsat_dVb - 0.5 * (dT1_dVb + dT2_dVb);
            /* Added to eliminate non - zero Vdseff at Vds = 0.0 */
            if (Vds == 0.0)
            {
                Vdseff = 0.0;
                dVdseff_dVg = 0.0;
                dVdseff_dVb = 0.0;
            }

            /* Calculate VAsat */
            tmp4 = 1.0 - 0.5 * Abulk * Vdsat / Vgst2Vtm;
            T9 = WVCoxRds * Vgsteff;
            T8 = T9 / Vgst2Vtm;
            T0 = EsatL + Vdsat + 2.0 * T9 * tmp4;

            T7 = 2.0 * WVCoxRds * tmp4;
            dT0_dVg = dEsatL_dVg + dVdsat_dVg + T7 * (1.0 + tmp2 * Vgsteff) - T8 * (Abulk * dVdsat_dVg - Abulk * Vdsat / Vgst2Vtm + Vdsat *
                dAbulk_dVg);

            dT0_dVb = dEsatL_dVb + dVdsat_dVb + T7 * tmp3 * Vgsteff - T8 * (dAbulk_dVb * Vdsat + Abulk * dVdsat_dVb);
            dT0_dVd = dEsatL_dVd + dVdsat_dVd - T8 * Abulk * dVdsat_dVd;

            T9 = WVCoxRds * Abulk;
            T1 = 2.0 / Lambda - 1.0 + T9;
            dT1_dVg = -2.0 * tmp1 + WVCoxRds * (Abulk * tmp2 + dAbulk_dVg);
            dT1_dVb = dAbulk_dVb * WVCoxRds + T9 * tmp3;

            Vasat = T0 / T1;
            dVasat_dVg = (dT0_dVg - Vasat * dT1_dVg) / T1;
            dVasat_dVb = (dT0_dVb - Vasat * dT1_dVb) / T1;
            dVasat_dVd = dT0_dVd / T1;

            if (Vdseff > Vds)
                Vdseff = Vds;
            diffVds = Vds - Vdseff;
            bsim3.BSIM3Vdseff = Vdseff;

            /* Calculate VACLM */
            if ((bsim3.pParam.BSIM3pclm > 0.0) && (diffVds > 1.0e-10))
            {
                T0 = 1.0 / (bsim3.pParam.BSIM3pclm * Abulk * bsim3.pParam.BSIM3litl);
                dT0_dVb = -T0 / Abulk * dAbulk_dVb;
                dT0_dVg = -T0 / Abulk * dAbulk_dVg;

                T2 = Vgsteff / EsatL;
                T1 = Leff * (Abulk + T2);
                dT1_dVg = Leff * ((1.0 - T2 * dEsatL_dVg) / EsatL + dAbulk_dVg);
                dT1_dVb = Leff * (dAbulk_dVb - T2 * dEsatL_dVb / EsatL);
                dT1_dVd = -T2 * dEsatL_dVd / Esat;

                T9 = T0 * T1;
                VACLM = T9 * diffVds;
                dVACLM_dVg = T0 * dT1_dVg * diffVds - T9 * dVdseff_dVg + T1 * diffVds * dT0_dVg;
                dVACLM_dVb = (dT0_dVb * T1 + T0 * dT1_dVb) * diffVds - T9 * dVdseff_dVb;
                dVACLM_dVd = T0 * dT1_dVd * diffVds + T9 * (1.0 - dVdseff_dVd);
            }
            else
            {
                VACLM = Transistor.MAX_EXP;
                dVACLM_dVd = dVACLM_dVg = dVACLM_dVb = 0.0;
            }

            /* Calculate VADIBL */
            if (bsim3.pParam.BSIM3thetaRout > 0.0)
            {
                T8 = Abulk * Vdsat;
                T0 = Vgst2Vtm * T8;
                dT0_dVg = Vgst2Vtm * Abulk * dVdsat_dVg + T8 + Vgst2Vtm * Vdsat * dAbulk_dVg;
                dT0_dVb = Vgst2Vtm * (dAbulk_dVb * Vdsat + Abulk * dVdsat_dVb);
                dT0_dVd = Vgst2Vtm * Abulk * dVdsat_dVd;

                T1 = Vgst2Vtm + T8;
                dT1_dVg = 1.0 + Abulk * dVdsat_dVg + Vdsat * dAbulk_dVg;
                dT1_dVb = Abulk * dVdsat_dVb + dAbulk_dVb * Vdsat;
                dT1_dVd = Abulk * dVdsat_dVd;

                T9 = T1 * T1;
                T2 = bsim3.pParam.BSIM3thetaRout;
                VADIBL = (Vgst2Vtm - T0 / T1) / T2;
                dVADIBL_dVg = (1.0 - dT0_dVg / T1 + T0 * dT1_dVg / T9) / T2;
                dVADIBL_dVb = (-dT0_dVb / T1 + T0 * dT1_dVb / T9) / T2;
                dVADIBL_dVd = (-dT0_dVd / T1 + T0 * dT1_dVd / T9) / T2;

                T7 = bsim3.pParam.BSIM3pdiblb * Vbseff;
                if (T7 >= -0.9)
                {
                    T3 = 1.0 / (1.0 + T7);
                    VADIBL *= T3;
                    dVADIBL_dVg *= T3;
                    dVADIBL_dVb = (dVADIBL_dVb - VADIBL * bsim3.pParam.BSIM3pdiblb) * T3;
                    dVADIBL_dVd *= T3;
                }
                else
                /* Added to avoid the discontinuity problem caused by pdiblcb */
                {
                    T4 = 1.0 / (0.8 + T7);
                    T3 = (17.0 + 20.0 * T7) * T4;
                    dVADIBL_dVg *= T3;
                    dVADIBL_dVb = dVADIBL_dVb * T3 - VADIBL * bsim3.pParam.BSIM3pdiblb * T4 * T4;
                    dVADIBL_dVd *= T3;
                    VADIBL *= T3;
                }
            }
            else
            {
                VADIBL = Transistor.MAX_EXP;
                dVADIBL_dVd = dVADIBL_dVg = dVADIBL_dVb = 0.0;
            }

            /* Calculate VA */

            T8 = bsim3.pParam.BSIM3pvag / EsatL;
            T9 = T8 * Vgsteff;
            if (T9 > -0.9)
            {
                T0 = 1.0 + T9;
                dT0_dVg = T8 * (1.0 - Vgsteff * dEsatL_dVg / EsatL);
                dT0_dVb = -T9 * dEsatL_dVb / EsatL;
                dT0_dVd = -T9 * dEsatL_dVd / EsatL;
            }
            else /* Added to avoid the discontinuity problems caused by pvag */
            {
                T1 = 1.0 / (17.0 + 20.0 * T9);
                T0 = (0.8 + T9) * T1;
                T1 *= T1;
                dT0_dVg = T8 * (1.0 - Vgsteff * dEsatL_dVg / EsatL) * T1;

                T9 *= T1 / EsatL;
                dT0_dVb = -T9 * dEsatL_dVb;
                dT0_dVd = -T9 * dEsatL_dVd;
            }

            tmp1 = VACLM * VACLM;
            tmp2 = VADIBL * VADIBL;
            tmp3 = VACLM + VADIBL;

            T1 = VACLM * VADIBL / tmp3;
            tmp3 *= tmp3;
            dT1_dVg = (tmp1 * dVADIBL_dVg + tmp2 * dVACLM_dVg) / tmp3;
            dT1_dVd = (tmp1 * dVADIBL_dVd + tmp2 * dVACLM_dVd) / tmp3;
            dT1_dVb = (tmp1 * dVADIBL_dVb + tmp2 * dVACLM_dVb) / tmp3;

            Va = Vasat + T0 * T1;
            dVa_dVg = dVasat_dVg + T1 * dT0_dVg + T0 * dT1_dVg;
            dVa_dVd = dVasat_dVd + T1 * dT0_dVd + T0 * dT1_dVd;
            dVa_dVb = dVasat_dVb + T1 * dT0_dVb + T0 * dT1_dVb;

            /* Calculate VASCBE */
            if (bsim3.pParam.BSIM3pscbe2 > 0.0)
            {
                if (diffVds > bsim3.pParam.BSIM3pscbe1 * bsim3.pParam.BSIM3litl / Transistor.EXP_THRESHOLD)
                {
                    T0 = bsim3.pParam.BSIM3pscbe1 * bsim3.pParam.BSIM3litl / diffVds;
                    VASCBE = Leff * Math.Exp(T0) / bsim3.pParam.BSIM3pscbe2;
                    T1 = T0 * VASCBE / diffVds;
                    dVASCBE_dVg = T1 * dVdseff_dVg;
                    dVASCBE_dVd = -T1 * (1.0 - dVdseff_dVd);
                    dVASCBE_dVb = T1 * dVdseff_dVb;
                }
                else
                {
                    VASCBE = Transistor.MAX_EXP * Leff / bsim3.pParam.BSIM3pscbe2;
                    dVASCBE_dVg = dVASCBE_dVd = dVASCBE_dVb = 0.0;
                }
            }
            else
            {
                VASCBE = Transistor.MAX_EXP;
                dVASCBE_dVg = dVASCBE_dVd = dVASCBE_dVb = 0.0;
            }

            /* Calculate Ids */
            CoxWovL = model.BSIM3cox * Weff / Leff;
            beta = ueff * CoxWovL;
            dbeta_dVg = CoxWovL * dueff_dVg + beta * dWeff_dVg / Weff;
            dbeta_dVd = CoxWovL * dueff_dVd;
            dbeta_dVb = CoxWovL * dueff_dVb + beta * dWeff_dVb / Weff;

            T0 = 1.0 - 0.5 * Abulk * Vdseff / Vgst2Vtm;
            dT0_dVg = -0.5 * (Abulk * dVdseff_dVg - Abulk * Vdseff / Vgst2Vtm + Vdseff * dAbulk_dVg) / Vgst2Vtm;
            dT0_dVd = -0.5 * Abulk * dVdseff_dVd / Vgst2Vtm;
            dT0_dVb = -0.5 * (Abulk * dVdseff_dVb + dAbulk_dVb * Vdseff) / Vgst2Vtm;

            fgche1 = Vgsteff * T0;
            dfgche1_dVg = Vgsteff * dT0_dVg + T0;
            dfgche1_dVd = Vgsteff * dT0_dVd;
            dfgche1_dVb = Vgsteff * dT0_dVb;

            T9 = Vdseff / EsatL;
            fgche2 = 1.0 + T9;
            dfgche2_dVg = (dVdseff_dVg - T9 * dEsatL_dVg) / EsatL;
            dfgche2_dVd = (dVdseff_dVd - T9 * dEsatL_dVd) / EsatL;
            dfgche2_dVb = (dVdseff_dVb - T9 * dEsatL_dVb) / EsatL;

            gche = beta * fgche1 / fgche2;
            dgche_dVg = (beta * dfgche1_dVg + fgche1 * dbeta_dVg - gche * dfgche2_dVg) / fgche2;
            dgche_dVd = (beta * dfgche1_dVd + fgche1 * dbeta_dVd - gche * dfgche2_dVd) / fgche2;
            dgche_dVb = (beta * dfgche1_dVb + fgche1 * dbeta_dVb - gche * dfgche2_dVb) / fgche2;

            T0 = 1.0 + gche * Rds;
            T9 = Vdseff / T0;
            Idl = gche * T9;

            dIdl_dVg = (gche * dVdseff_dVg + T9 * dgche_dVg) / T0 - Idl * gche / T0 * dRds_dVg;

            dIdl_dVd = (gche * dVdseff_dVd + T9 * dgche_dVd) / T0;
            dIdl_dVb = (gche * dVdseff_dVb + T9 * dgche_dVb - Idl * dRds_dVb * gche) / T0;

            T9 = diffVds / Va;
            T0 = 1.0 + T9;
            Idsa = Idl * T0;
            dIdsa_dVg = T0 * dIdl_dVg - Idl * (dVdseff_dVg + T9 * dVa_dVg) / Va;
            dIdsa_dVd = T0 * dIdl_dVd + Idl * (1.0 - dVdseff_dVd - T9 * dVa_dVd) / Va;
            dIdsa_dVb = T0 * dIdl_dVb - Idl * (dVdseff_dVb + T9 * dVa_dVb) / Va;

            T9 = diffVds / VASCBE;
            T0 = 1.0 + T9;
            Ids = Idsa * T0;

            Gm = T0 * dIdsa_dVg - Idsa * (dVdseff_dVg + T9 * dVASCBE_dVg) / VASCBE;
            Gds = T0 * dIdsa_dVd + Idsa * (1.0 - dVdseff_dVd - T9 * dVASCBE_dVd) / VASCBE;
            Gmb = T0 * dIdsa_dVb - Idsa * (dVdseff_dVb + T9 * dVASCBE_dVb) / VASCBE;

            Gds += Gm * dVgsteff_dVd;
            Gmb += Gm * dVgsteff_dVb;
            Gm *= dVgsteff_dVg;
            Gmb *= dVbseff_dVb;

            /* Substrate current begins */
            tmp = bsim3.pParam.BSIM3alpha0 + bsim3.pParam.BSIM3alpha1 * Leff;
            if ((tmp <= 0.0) || (bsim3.pParam.BSIM3beta0 <= 0.0))
            {
                Isub = Gbd = Gbb = Gbg = 0.0;
            }
            else
            {
                T2 = tmp / Leff;
                if (diffVds > bsim3.pParam.BSIM3beta0 / Transistor.EXP_THRESHOLD)
                {
                    T0 = -bsim3.pParam.BSIM3beta0 / diffVds;
                    T1 = T2 * diffVds * Math.Exp(T0);
                    T3 = T1 / diffVds * (T0 - 1.0);
                    dT1_dVg = T3 * dVdseff_dVg;
                    dT1_dVd = T3 * (dVdseff_dVd - 1.0);
                    dT1_dVb = T3 * dVdseff_dVb;
                }
                else
                {
                    T3 = T2 * Transistor.MIN_EXP;
                    T1 = T3 * diffVds;
                    dT1_dVg = -T3 * dVdseff_dVg;
                    dT1_dVd = T3 * (1.0 - dVdseff_dVd);
                    dT1_dVb = -T3 * dVdseff_dVb;
                }
                Isub = T1 * Idsa;
                Gbg = T1 * dIdsa_dVg + Idsa * dT1_dVg;
                Gbd = T1 * dIdsa_dVd + Idsa * dT1_dVd;
                Gbb = T1 * dIdsa_dVb + Idsa * dT1_dVb;

                Gbd += Gbg * dVgsteff_dVd;
                Gbb += Gbg * dVgsteff_dVb;
                Gbg *= dVgsteff_dVg;
                Gbb *= dVbseff_dVb; /* bug fixing */
            }

            cdrain = Ids;
            bsim3.BSIM3gds = Gds;
            bsim3.BSIM3gm = Gm;
            bsim3.BSIM3gmbs = Gmb;

            bsim3.BSIM3gbbs = Gbb;
            bsim3.BSIM3gbgs = Gbg;
            bsim3.BSIM3gbds = Gbd;

            bsim3.BSIM3csub = Isub;

            /* bsim3.BSIM3v24 thermal noise Qinv calculated from all capMod 
            * 0, 1, 2 & 3 stored in bsim3.BSIM3qinv1 / 1998 */

            if ((model.BSIM3xpart < 0) || (!ChargeComputationNeeded))
            {
                qgate = qdrn = qsrc = qbulk = 0.0;
                bsim3.BSIM3cggb = bsim3.BSIM3cgsb = bsim3.BSIM3cgdb = 0.0;
                bsim3.BSIM3cdgb = bsim3.BSIM3cdsb = bsim3.BSIM3cddb = 0.0;
                bsim3.BSIM3cbgb = bsim3.BSIM3cbsb = bsim3.BSIM3cbdb = 0.0;
                bsim3.BSIM3cqdb = bsim3.BSIM3cqsb = bsim3.BSIM3cqgb = bsim3.BSIM3cqbb = 0.0;
                bsim3.BSIM3gtau = 0.0;
                goto finished;
            }
            else if (model.BSIM3capMod.Value == 0)
            {
                if (Vbseff < 0.0)
                {
                    Vbseff = Vbs;
                    dVbseff_dVb = 1.0;
                }
                else
                {
                    Vbseff = bsim3.pParam.BSIM3phi - Phis;
                    dVbseff_dVb = -dPhis_dVb;
                }

                Vfb = bsim3.pParam.BSIM3vfbcv;
                Vth = Vfb + bsim3.pParam.BSIM3phi + bsim3.pParam.BSIM3k1ox * sqrtPhis;
                Vgst = Vgs_eff - Vth;
                dVth_dVb = bsim3.pParam.BSIM3k1ox * dsqrtPhis_dVb;
                dVgst_dVb = -dVth_dVb;
                dVgst_dVg = dVgs_eff_dVg;

                CoxWL = model.BSIM3cox * bsim3.pParam.BSIM3weffCV * bsim3.pParam.BSIM3leffCV;
                Arg1 = Vgs_eff - Vbseff - Vfb;

                if (Arg1 <= 0.0)
                {
                    qgate = CoxWL * Arg1;
                    qbulk = -qgate;
                    qdrn = 0.0;

                    bsim3.BSIM3cggb = CoxWL * dVgs_eff_dVg;
                    bsim3.BSIM3cgdb = 0.0;
                    bsim3.BSIM3cgsb = CoxWL * (dVbseff_dVb - dVgs_eff_dVg);

                    bsim3.BSIM3cdgb = 0.0;
                    bsim3.BSIM3cddb = 0.0;
                    bsim3.BSIM3cdsb = 0.0;

                    bsim3.BSIM3cbgb = -CoxWL * dVgs_eff_dVg;
                    bsim3.BSIM3cbdb = 0.0;
                    bsim3.BSIM3cbsb = -bsim3.BSIM3cgsb;
                    bsim3.BSIM3qinv = 0.0;
                }
                else if (Vgst <= 0.0)
                {
                    T1 = 0.5 * bsim3.pParam.BSIM3k1ox;
                    T2 = Math.Sqrt(T1 * T1 + Arg1);
                    qgate = CoxWL * bsim3.pParam.BSIM3k1ox * (T2 - T1);
                    qbulk = -qgate;
                    qdrn = 0.0;

                    T0 = CoxWL * T1 / T2;
                    bsim3.BSIM3cggb = T0 * dVgs_eff_dVg;
                    bsim3.BSIM3cgdb = 0.0;
                    bsim3.BSIM3cgsb = T0 * (dVbseff_dVb - dVgs_eff_dVg);

                    bsim3.BSIM3cdgb = 0.0;
                    bsim3.BSIM3cddb = 0.0;
                    bsim3.BSIM3cdsb = 0.0;

                    bsim3.BSIM3cbgb = -bsim3.BSIM3cggb;
                    bsim3.BSIM3cbdb = 0.0;
                    bsim3.BSIM3cbsb = -bsim3.BSIM3cgsb;
                    bsim3.BSIM3qinv = 0.0;
                }
                else
                {
                    One_Third_CoxWL = CoxWL / 3.0;
                    Two_Third_CoxWL = 2.0 * One_Third_CoxWL;

                    AbulkCV = Abulk0 * bsim3.pParam.BSIM3abulkCVfactor;
                    dAbulkCV_dVb = bsim3.pParam.BSIM3abulkCVfactor * dAbulk0_dVb;
                    Vdsat = Vgst / AbulkCV;
                    dVdsat_dVg = dVgs_eff_dVg / AbulkCV;
                    dVdsat_dVb = -(Vdsat * dAbulkCV_dVb + dVth_dVb) / AbulkCV;

                    if (model.BSIM3xpart > 0.5)
                    {
                        /* 0 / 100 Charge partition model */
                        if (Vdsat <= Vds)
                        {
                            /* saturation region */
                            T1 = Vdsat / 3.0;
                            qgate = CoxWL * (Vgs_eff - Vfb - bsim3.pParam.BSIM3phi - T1);
                            T2 = -Two_Third_CoxWL * Vgst;
                            qbulk = -(qgate + T2);
                            qdrn = 0.0;

                            bsim3.BSIM3cggb = One_Third_CoxWL * (3.0 - dVdsat_dVg) * dVgs_eff_dVg;
                            T2 = -One_Third_CoxWL * dVdsat_dVb;
                            bsim3.BSIM3cgsb = -(bsim3.BSIM3cggb + T2);
                            bsim3.BSIM3cgdb = 0.0;

                            bsim3.BSIM3cdgb = 0.0;
                            bsim3.BSIM3cddb = 0.0;
                            bsim3.BSIM3cdsb = 0.0;

                            bsim3.BSIM3cbgb = -(bsim3.BSIM3cggb - Two_Third_CoxWL * dVgs_eff_dVg);
                            T3 = -(T2 + Two_Third_CoxWL * dVth_dVb);
                            bsim3.BSIM3cbsb = -(bsim3.BSIM3cbgb + T3);
                            bsim3.BSIM3cbdb = 0.0;
                            bsim3.BSIM3qinv = -(qgate + qbulk);
                        }
                        else
                        {
                            /* linear region */
                            Alphaz = Vgst / Vdsat;
                            T1 = 2.0 * Vdsat - Vds;
                            T2 = Vds / (3.0 * T1);
                            T3 = T2 * Vds;
                            T9 = 0.25 * CoxWL;
                            T4 = T9 * Alphaz;
                            T7 = 2.0 * Vds - T1 - 3.0 * T3;
                            T8 = T3 - T1 - 2.0 * Vds;
                            qgate = CoxWL * (Vgs_eff - Vfb - bsim3.pParam.BSIM3phi - 0.5 * (Vds - T3));
                            T10 = T4 * T8;
                            qdrn = T4 * T7;
                            qbulk = -(qgate + qdrn + T10);

                            T5 = T3 / T1;
                            bsim3.BSIM3cggb = CoxWL * (1.0 - T5 * dVdsat_dVg) * dVgs_eff_dVg;
                            T11 = -CoxWL * T5 * dVdsat_dVb;
                            bsim3.BSIM3cgdb = CoxWL * (T2 - 0.5 + 0.5 * T5);
                            bsim3.BSIM3cgsb = -(bsim3.BSIM3cggb + T11 + bsim3.BSIM3cgdb);
                            T6 = 1.0 / Vdsat;
                            dAlphaz_dVg = T6 * (1.0 - Alphaz * dVdsat_dVg);
                            dAlphaz_dVb = -T6 * (dVth_dVb + Alphaz * dVdsat_dVb);
                            T7 = T9 * T7;
                            T8 = T9 * T8;
                            T9 = 2.0 * T4 * (1.0 - 3.0 * T5);
                            bsim3.BSIM3cdgb = (T7 * dAlphaz_dVg - T9 * dVdsat_dVg) * dVgs_eff_dVg;
                            T12 = T7 * dAlphaz_dVb - T9 * dVdsat_dVb;
                            bsim3.BSIM3cddb = T4 * (3.0 - 6.0 * T2 - 3.0 * T5);
                            bsim3.BSIM3cdsb = -(bsim3.BSIM3cdgb + T12 + bsim3.BSIM3cddb);

                            T9 = 2.0 * T4 * (1.0 + T5);
                            T10 = (T8 * dAlphaz_dVg - T9 * dVdsat_dVg) * dVgs_eff_dVg;
                            T11 = T8 * dAlphaz_dVb - T9 * dVdsat_dVb;
                            T12 = T4 * (2.0 * T2 + T5 - 1.0);
                            T0 = -(T10 + T11 + T12);

                            bsim3.BSIM3cbgb = -(bsim3.BSIM3cggb + bsim3.BSIM3cdgb + T10);
                            bsim3.BSIM3cbdb = -(bsim3.BSIM3cgdb + bsim3.BSIM3cddb + T12);
                            bsim3.BSIM3cbsb = -(bsim3.BSIM3cgsb + bsim3.BSIM3cdsb + T0);
                            bsim3.BSIM3qinv = -(qgate + qbulk);
                        }
                    }
                    else if (model.BSIM3xpart < 0.5)
                    {
                        /* 40 / 60 Charge partition model */
                        if (Vds >= Vdsat)
                        {
                            /* saturation region */
                            T1 = Vdsat / 3.0;
                            qgate = CoxWL * (Vgs_eff - Vfb - bsim3.pParam.BSIM3phi - T1);
                            T2 = -Two_Third_CoxWL * Vgst;
                            qbulk = -(qgate + T2);
                            qdrn = 0.4 * T2;

                            bsim3.BSIM3cggb = One_Third_CoxWL * (3.0 - dVdsat_dVg) * dVgs_eff_dVg;
                            T2 = -One_Third_CoxWL * dVdsat_dVb;
                            bsim3.BSIM3cgsb = -(bsim3.BSIM3cggb + T2);
                            bsim3.BSIM3cgdb = 0.0;

                            T3 = 0.4 * Two_Third_CoxWL;
                            bsim3.BSIM3cdgb = -T3 * dVgs_eff_dVg;
                            bsim3.BSIM3cddb = 0.0;
                            T4 = T3 * dVth_dVb;
                            bsim3.BSIM3cdsb = -(T4 + bsim3.BSIM3cdgb);

                            bsim3.BSIM3cbgb = -(bsim3.BSIM3cggb - Two_Third_CoxWL * dVgs_eff_dVg);
                            T3 = -(T2 + Two_Third_CoxWL * dVth_dVb);
                            bsim3.BSIM3cbsb = -(bsim3.BSIM3cbgb + T3);
                            bsim3.BSIM3cbdb = 0.0;
                            bsim3.BSIM3qinv = -(qgate + qbulk);
                        }
                        else
                        {
                            /* linear region */
                            Alphaz = Vgst / Vdsat;
                            T1 = 2.0 * Vdsat - Vds;
                            T2 = Vds / (3.0 * T1);
                            T3 = T2 * Vds;
                            T9 = 0.25 * CoxWL;
                            T4 = T9 * Alphaz;
                            qgate = CoxWL * (Vgs_eff - Vfb - bsim3.pParam.BSIM3phi - 0.5 * (Vds - T3));

                            T5 = T3 / T1;
                            bsim3.BSIM3cggb = CoxWL * (1.0 - T5 * dVdsat_dVg) * dVgs_eff_dVg;
                            tmp = -CoxWL * T5 * dVdsat_dVb;
                            bsim3.BSIM3cgdb = CoxWL * (T2 - 0.5 + 0.5 * T5);
                            bsim3.BSIM3cgsb = -(bsim3.BSIM3cggb + bsim3.BSIM3cgdb + tmp);

                            T6 = 1.0 / Vdsat;
                            dAlphaz_dVg = T6 * (1.0 - Alphaz * dVdsat_dVg);
                            dAlphaz_dVb = -T6 * (dVth_dVb + Alphaz * dVdsat_dVb);

                            T6 = 8.0 * Vdsat * Vdsat - 6.0 * Vdsat * Vds + 1.2 * Vds * Vds;
                            T8 = T2 / T1;
                            T7 = Vds - T1 - T8 * T6;
                            qdrn = T4 * T7;
                            T7 *= T9;
                            tmp = T8 / T1;
                            tmp1 = T4 * (2.0 - 4.0 * tmp * T6 + T8 * (16.0 * Vdsat - 6.0 * Vds));

                            bsim3.BSIM3cdgb = (T7 * dAlphaz_dVg - tmp1 * dVdsat_dVg) * dVgs_eff_dVg;
                            T10 = T7 * dAlphaz_dVb - tmp1 * dVdsat_dVb;
                            bsim3.BSIM3cddb = T4 * (2.0 - (1.0 / (3.0 * T1 * T1) + 2.0 * tmp) * T6 + T8 * (6.0 * Vdsat - 2.4 * Vds));
                            bsim3.BSIM3cdsb = -(bsim3.BSIM3cdgb + T10 + bsim3.BSIM3cddb);

                            T7 = 2.0 * (T1 + T3);
                            qbulk = -(qgate - T4 * T7);
                            T7 *= T9;
                            T0 = 4.0 * T4 * (1.0 - T5);
                            T12 = (-T7 * dAlphaz_dVg - bsim3.BSIM3cdgb - T0 * dVdsat_dVg) * dVgs_eff_dVg;
                            T11 = -T7 * dAlphaz_dVb - T10 - T0 * dVdsat_dVb;
                            T10 = -4.0 * T4 * (T2 - 0.5 + 0.5 * T5) - bsim3.BSIM3cddb;
                            tmp = -(T10 + T11 + T12);

                            bsim3.BSIM3cbgb = -(bsim3.BSIM3cggb + bsim3.BSIM3cdgb + T12);
                            bsim3.BSIM3cbdb = -(bsim3.BSIM3cgdb + bsim3.BSIM3cddb + T10); /* bug fix */
                            bsim3.BSIM3cbsb = -(bsim3.BSIM3cgsb + bsim3.BSIM3cdsb + tmp);
                            bsim3.BSIM3qinv = -(qgate + qbulk);
                        }
                    }
                    else
                    {
                        /* 50 / 50 partitioning */
                        if (Vds >= Vdsat)
                        {
                            /* saturation region */
                            T1 = Vdsat / 3.0;
                            qgate = CoxWL * (Vgs_eff - Vfb - bsim3.pParam.BSIM3phi - T1);
                            T2 = -Two_Third_CoxWL * Vgst;
                            qbulk = -(qgate + T2);
                            qdrn = 0.5 * T2;

                            bsim3.BSIM3cggb = One_Third_CoxWL * (3.0 - dVdsat_dVg) * dVgs_eff_dVg;
                            T2 = -One_Third_CoxWL * dVdsat_dVb;
                            bsim3.BSIM3cgsb = -(bsim3.BSIM3cggb + T2);
                            bsim3.BSIM3cgdb = 0.0;

                            bsim3.BSIM3cdgb = -One_Third_CoxWL * dVgs_eff_dVg;
                            bsim3.BSIM3cddb = 0.0;
                            T4 = One_Third_CoxWL * dVth_dVb;
                            bsim3.BSIM3cdsb = -(T4 + bsim3.BSIM3cdgb);

                            bsim3.BSIM3cbgb = -(bsim3.BSIM3cggb - Two_Third_CoxWL * dVgs_eff_dVg);
                            T3 = -(T2 + Two_Third_CoxWL * dVth_dVb);
                            bsim3.BSIM3cbsb = -(bsim3.BSIM3cbgb + T3);
                            bsim3.BSIM3cbdb = 0.0;
                            bsim3.BSIM3qinv = -(qgate + qbulk);
                        }
                        else
                        {
                            /* linear region */
                            Alphaz = Vgst / Vdsat;
                            T1 = 2.0 * Vdsat - Vds;
                            T2 = Vds / (3.0 * T1);
                            T3 = T2 * Vds;
                            T9 = 0.25 * CoxWL;
                            T4 = T9 * Alphaz;
                            qgate = CoxWL * (Vgs_eff - Vfb - bsim3.pParam.BSIM3phi - 0.5 * (Vds - T3));

                            T5 = T3 / T1;
                            bsim3.BSIM3cggb = CoxWL * (1.0 - T5 * dVdsat_dVg) * dVgs_eff_dVg;
                            tmp = -CoxWL * T5 * dVdsat_dVb;
                            bsim3.BSIM3cgdb = CoxWL * (T2 - 0.5 + 0.5 * T5);
                            bsim3.BSIM3cgsb = -(bsim3.BSIM3cggb + bsim3.BSIM3cgdb + tmp);

                            T6 = 1.0 / Vdsat;
                            dAlphaz_dVg = T6 * (1.0 - Alphaz * dVdsat_dVg);
                            dAlphaz_dVb = -T6 * (dVth_dVb + Alphaz * dVdsat_dVb);

                            T7 = T1 + T3;
                            qdrn = -T4 * T7;
                            qbulk = -(qgate + qdrn + qdrn);
                            T7 *= T9;
                            T0 = T4 * (2.0 * T5 - 2.0);

                            bsim3.BSIM3cdgb = (T0 * dVdsat_dVg - T7 * dAlphaz_dVg) * dVgs_eff_dVg;
                            T12 = T0 * dVdsat_dVb - T7 * dAlphaz_dVb;
                            bsim3.BSIM3cddb = T4 * (1.0 - 2.0 * T2 - T5);
                            bsim3.BSIM3cdsb = -(bsim3.BSIM3cdgb + T12 + bsim3.BSIM3cddb);

                            bsim3.BSIM3cbgb = -(bsim3.BSIM3cggb + 2.0 * bsim3.BSIM3cdgb);
                            bsim3.BSIM3cbdb = -(bsim3.BSIM3cgdb + 2.0 * bsim3.BSIM3cddb);
                            bsim3.BSIM3cbsb = -(bsim3.BSIM3cgsb + 2.0 * bsim3.BSIM3cdsb);
                            bsim3.BSIM3qinv = -(qgate + qbulk);
                        }
                    }
                }
            }
            else
            {
                if (Vbseff < 0.0)
                {
                    VbseffCV = Vbseff;
                    dVbseffCV_dVb = 1.0;
                }
                else
                {
                    VbseffCV = bsim3.pParam.BSIM3phi - Phis;
                    dVbseffCV_dVb = -dPhis_dVb;
                }

                CoxWL = model.BSIM3cox * bsim3.pParam.BSIM3weffCV * bsim3.pParam.BSIM3leffCV;

                /* Seperate VgsteffCV with noff and voffcv */
                noff = n * bsim3.pParam.BSIM3noff;
                dnoff_dVd = bsim3.pParam.BSIM3noff * dn_dVd;
                dnoff_dVb = bsim3.pParam.BSIM3noff * dn_dVb;
                T0 = Vtm * noff;
                voffcv = bsim3.pParam.BSIM3voffcv;
                VgstNVt = (Vgst - voffcv) / T0;

                if (VgstNVt > Transistor.EXP_THRESHOLD)
                {
                    Vgsteff = Vgst - voffcv;
                    dVgsteff_dVg = dVgs_eff_dVg;
                    dVgsteff_dVd = -dVth_dVd;
                    dVgsteff_dVb = -dVth_dVb;
                }
                else if (VgstNVt < -Transistor.EXP_THRESHOLD)
                {
                    Vgsteff = T0 * Math.Log(1.0 + Transistor.MIN_EXP);
                    dVgsteff_dVg = 0.0;
                    dVgsteff_dVd = Vgsteff / noff;
                    dVgsteff_dVb = dVgsteff_dVd * dnoff_dVb;
                    dVgsteff_dVd *= dnoff_dVd;
                }
                else
                {
                    ExpVgst = Math.Exp(VgstNVt);
                    Vgsteff = T0 * Math.Log(1.0 + ExpVgst);
                    dVgsteff_dVg = ExpVgst / (1.0 + ExpVgst);
                    dVgsteff_dVd = -dVgsteff_dVg * (dVth_dVd + (Vgst - voffcv) / noff * dnoff_dVd) + Vgsteff / noff * dnoff_dVd;
                    dVgsteff_dVb = -dVgsteff_dVg * (dVth_dVb + (Vgst - voffcv) / noff * dnoff_dVb) + Vgsteff / noff * dnoff_dVb;
                    dVgsteff_dVg *= dVgs_eff_dVg;
                } /* End of VgsteffCV */

                if (model.BSIM3capMod.Value == 1)
                {
                    Vfb = bsim3.pParam.BSIM3vfbzb;
                    Arg1 = Vgs_eff - VbseffCV - Vfb - Vgsteff;

                    if (Arg1 <= 0.0)
                    {
                        qgate = CoxWL * Arg1;
                        Cgg = CoxWL * (dVgs_eff_dVg - dVgsteff_dVg);
                        Cgd = -CoxWL * dVgsteff_dVd;
                        Cgb = -CoxWL * (dVbseffCV_dVb + dVgsteff_dVb);
                    }
                    else
                    {
                        T0 = 0.5 * bsim3.pParam.BSIM3k1ox;
                        T1 = Math.Sqrt(T0 * T0 + Arg1);
                        T2 = CoxWL * T0 / T1;

                        qgate = CoxWL * bsim3.pParam.BSIM3k1ox * (T1 - T0);

                        Cgg = T2 * (dVgs_eff_dVg - dVgsteff_dVg);
                        Cgd = -T2 * dVgsteff_dVd;
                        Cgb = -T2 * (dVbseffCV_dVb + dVgsteff_dVb);
                    }
                    qbulk = -qgate;
                    Cbg = -Cgg;
                    Cbd = -Cgd;
                    Cbb = -Cgb;

                    One_Third_CoxWL = CoxWL / 3.0;
                    Two_Third_CoxWL = 2.0 * One_Third_CoxWL;
                    AbulkCV = Abulk0 * bsim3.pParam.BSIM3abulkCVfactor;
                    dAbulkCV_dVb = bsim3.pParam.BSIM3abulkCVfactor * dAbulk0_dVb;
                    VdsatCV = Vgsteff / AbulkCV;
                    if (VdsatCV < Vds)
                    {
                        dVdsatCV_dVg = 1.0 / AbulkCV;
                        dVdsatCV_dVb = -VdsatCV * dAbulkCV_dVb / AbulkCV;
                        T0 = Vgsteff - VdsatCV / 3.0;
                        dT0_dVg = 1.0 - dVdsatCV_dVg / 3.0;
                        dT0_dVb = -dVdsatCV_dVb / 3.0;
                        qgate += CoxWL * T0;
                        Cgg1 = CoxWL * dT0_dVg;
                        Cgb1 = CoxWL * dT0_dVb + Cgg1 * dVgsteff_dVb;
                        Cgd1 = Cgg1 * dVgsteff_dVd;
                        Cgg1 *= dVgsteff_dVg;
                        Cgg += Cgg1;
                        Cgb += Cgb1;
                        Cgd += Cgd1;

                        T0 = VdsatCV - Vgsteff;
                        dT0_dVg = dVdsatCV_dVg - 1.0;
                        dT0_dVb = dVdsatCV_dVb;
                        qbulk += One_Third_CoxWL * T0;
                        Cbg1 = One_Third_CoxWL * dT0_dVg;
                        Cbb1 = One_Third_CoxWL * dT0_dVb + Cbg1 * dVgsteff_dVb;
                        Cbd1 = Cbg1 * dVgsteff_dVd;
                        Cbg1 *= dVgsteff_dVg;
                        Cbg += Cbg1;
                        Cbb += Cbb1;
                        Cbd += Cbd1;

                        if (model.BSIM3xpart > 0.5)
                            T0 = -Two_Third_CoxWL;
                        else if (model.BSIM3xpart < 0.5)
                            T0 = -0.4 * CoxWL;
                        else
                            T0 = -One_Third_CoxWL;

                        qsrc = T0 * Vgsteff;
                        Csg = T0 * dVgsteff_dVg;
                        Csb = T0 * dVgsteff_dVb;
                        Csd = T0 * dVgsteff_dVd;
                        Cgb *= dVbseff_dVb;
                        Cbb *= dVbseff_dVb;
                        Csb *= dVbseff_dVb;
                    }
                    else
                    {
                        T0 = AbulkCV * Vds;
                        T1 = 12.0 * (Vgsteff - 0.5 * T0 + 1.0e-20);
                        T2 = Vds / T1;
                        T3 = T0 * T2;
                        dT3_dVg = -12.0 * T2 * T2 * AbulkCV;
                        dT3_dVd = 6.0 * T0 * (4.0 * Vgsteff - T0) / T1 / T1 - 0.5;
                        dT3_dVb = 12.0 * T2 * T2 * dAbulkCV_dVb * Vgsteff;

                        qgate += CoxWL * (Vgsteff - 0.5 * Vds + T3);
                        Cgg1 = CoxWL * (1.0 + dT3_dVg);
                        Cgb1 = CoxWL * dT3_dVb + Cgg1 * dVgsteff_dVb;
                        Cgd1 = CoxWL * dT3_dVd + Cgg1 * dVgsteff_dVd;
                        Cgg1 *= dVgsteff_dVg;
                        Cgg += Cgg1;
                        Cgb += Cgb1;
                        Cgd += Cgd1;

                        qbulk += CoxWL * (1.0 - AbulkCV) * (0.5 * Vds - T3);
                        Cbg1 = -CoxWL * ((1.0 - AbulkCV) * dT3_dVg);
                        Cbb1 = -CoxWL * ((1.0 - AbulkCV) * dT3_dVb + (0.5 * Vds - T3) * dAbulkCV_dVb) + Cbg1 * dVgsteff_dVb;
                        Cbd1 = -CoxWL * (1.0 - AbulkCV) * dT3_dVd + Cbg1 * dVgsteff_dVd;
                        Cbg1 *= dVgsteff_dVg;
                        Cbg += Cbg1;
                        Cbb += Cbb1;
                        Cbd += Cbd1;

                        if (model.BSIM3xpart > 0.5)
                        {
                            /* 0 / 100 Charge petition model */
                            T1 = T1 + T1;
                            qsrc = -CoxWL * (0.5 * Vgsteff + 0.25 * T0 - T0 * T0 / T1);
                            Csg = -CoxWL * (0.5 + 24.0 * T0 * Vds / T1 / T1 * AbulkCV);
                            Csb = -CoxWL * (0.25 * Vds * dAbulkCV_dVb - 12.0 * T0 * Vds / T1 / T1 * (4.0 * Vgsteff - T0) * dAbulkCV_dVb) + Csg *
                                dVgsteff_dVb;
                            Csd = -CoxWL * (0.25 * AbulkCV - 12.0 * AbulkCV * T0 / T1 / T1 * (4.0 * Vgsteff - T0)) + Csg * dVgsteff_dVd;
                            Csg *= dVgsteff_dVg;
                        }
                        else if (model.BSIM3xpart < 0.5)
                        {
                            /* 40 / 60 Charge petition model */
                            T1 = T1 / 12.0;
                            T2 = 0.5 * CoxWL / (T1 * T1);
                            T3 = Vgsteff * (2.0 * T0 * T0 / 3.0 + Vgsteff * (Vgsteff - 4.0 * T0 / 3.0)) - 2.0 * T0 * T0 * T0 / 15.0;
                            qsrc = -T2 * T3;
                            T4 = 4.0 / 3.0 * Vgsteff * (Vgsteff - T0) + 0.4 * T0 * T0;
                            Csg = -2.0 * qsrc / T1 - T2 * (Vgsteff * (3.0 * Vgsteff - 8.0 * T0 / 3.0) + 2.0 * T0 * T0 / 3.0);
                            Csb = (qsrc / T1 * Vds + T2 * T4 * Vds) * dAbulkCV_dVb + Csg * dVgsteff_dVb;
                            Csd = (qsrc / T1 + T2 * T4) * AbulkCV + Csg * dVgsteff_dVd;
                            Csg *= dVgsteff_dVg;
                        }
                        else
                        {
                            /* 50 / 50 Charge petition model */
                            qsrc = -0.5 * (qgate + qbulk);
                            Csg = -0.5 * (Cgg1 + Cbg1);
                            Csb = -0.5 * (Cgb1 + Cbb1);
                            Csd = -0.5 * (Cgd1 + Cbd1);
                        }
                        Cgb *= dVbseff_dVb;
                        Cbb *= dVbseff_dVb;
                        Csb *= dVbseff_dVb;
                    }
                    qdrn = -(qgate + qbulk + qsrc);
                    bsim3.BSIM3cggb = Cgg;
                    bsim3.BSIM3cgsb = -(Cgg + Cgd + Cgb);
                    bsim3.BSIM3cgdb = Cgd;
                    bsim3.BSIM3cdgb = -(Cgg + Cbg + Csg);
                    bsim3.BSIM3cdsb = (Cgg + Cgd + Cgb + Cbg + Cbd + Cbb + Csg + Csd + Csb);
                    bsim3.BSIM3cddb = -(Cgd + Cbd + Csd);
                    bsim3.BSIM3cbgb = Cbg;
                    bsim3.BSIM3cbsb = -(Cbg + Cbd + Cbb);
                    bsim3.BSIM3cbdb = Cbd;
                    bsim3.BSIM3qinv = -(qgate + qbulk);
                }

                else if (model.BSIM3capMod.Value == 2)
                {
                    Vfb = bsim3.pParam.BSIM3vfbzb;
                    V3 = Vfb - Vgs_eff + VbseffCV - Transistor.DELTA_3;
                    if (Vfb <= 0.0)
                    {
                        T0 = Math.Sqrt(V3 * V3 - 4.0 * Transistor.DELTA_3 * Vfb);
                        T2 = -Transistor.DELTA_3 / T0;
                    }
                    else
                    {
                        T0 = Math.Sqrt(V3 * V3 + 4.0 * Transistor.DELTA_3 * Vfb);
                        T2 = Transistor.DELTA_3 / T0;
                    }

                    T1 = 0.5 * (1.0 + V3 / T0);
                    Vfbeff = Vfb - 0.5 * (V3 + T0);
                    dVfbeff_dVg = T1 * dVgs_eff_dVg;
                    dVfbeff_dVb = -T1 * dVbseffCV_dVb;
                    Qac0 = CoxWL * (Vfbeff - Vfb);
                    dQac0_dVg = CoxWL * dVfbeff_dVg;
                    dQac0_dVb = CoxWL * dVfbeff_dVb;

                    T0 = 0.5 * bsim3.pParam.BSIM3k1ox;
                    T3 = Vgs_eff - Vfbeff - VbseffCV - Vgsteff;
                    if (bsim3.pParam.BSIM3k1ox == 0.0)
                    {
                        T1 = 0.0;
                        T2 = 0.0;
                    }
                    else if (T3 < 0.0)
                    {
                        T1 = T0 + T3 / bsim3.pParam.BSIM3k1ox;
                        T2 = CoxWL;
                    }
                    else
                    {
                        T1 = Math.Sqrt(T0 * T0 + T3);
                        T2 = CoxWL * T0 / T1;
                    }

                    Qsub0 = CoxWL * bsim3.pParam.BSIM3k1ox * (T1 - T0);

                    dQsub0_dVg = T2 * (dVgs_eff_dVg - dVfbeff_dVg - dVgsteff_dVg);
                    dQsub0_dVd = -T2 * dVgsteff_dVd;
                    dQsub0_dVb = -T2 * (dVfbeff_dVb + dVbseffCV_dVb + dVgsteff_dVb);

                    AbulkCV = Abulk0 * bsim3.pParam.BSIM3abulkCVfactor;
                    dAbulkCV_dVb = bsim3.pParam.BSIM3abulkCVfactor * dAbulk0_dVb;
                    VdsatCV = Vgsteff / AbulkCV;

                    V4 = VdsatCV - Vds - Transistor.DELTA_4;
                    T0 = Math.Sqrt(V4 * V4 + 4.0 * Transistor.DELTA_4 * VdsatCV);
                    VdseffCV = VdsatCV - 0.5 * (V4 + T0);
                    T1 = 0.5 * (1.0 + V4 / T0);
                    T2 = Transistor.DELTA_4 / T0;
                    T3 = (1.0 - T1 - T2) / AbulkCV;
                    dVdseffCV_dVg = T3;
                    dVdseffCV_dVd = T1;
                    dVdseffCV_dVb = -T3 * VdsatCV * dAbulkCV_dVb;
                    /* Added to eliminate non - zero VdseffCV at Vds = 0.0 */
                    if (Vds == 0.0)
                    {
                        VdseffCV = 0.0;
                        dVdseffCV_dVg = 0.0;
                        dVdseffCV_dVb = 0.0;
                    }

                    T0 = AbulkCV * VdseffCV;
                    T1 = 12.0 * (Vgsteff - 0.5 * T0 + 1e-20);
                    T2 = VdseffCV / T1;
                    T3 = T0 * T2;

                    T4 = (1.0 - 12.0 * T2 * T2 * AbulkCV);
                    T5 = (6.0 * T0 * (4.0 * Vgsteff - T0) / (T1 * T1) - 0.5);
                    T6 = 12.0 * T2 * T2 * Vgsteff;

                    qinoi = -CoxWL * (Vgsteff - 0.5 * T0 + AbulkCV * T3);
                    qgate = CoxWL * (Vgsteff - 0.5 * VdseffCV + T3);
                    Cgg1 = CoxWL * (T4 + T5 * dVdseffCV_dVg);
                    Cgd1 = CoxWL * T5 * dVdseffCV_dVd + Cgg1 * dVgsteff_dVd;
                    Cgb1 = CoxWL * (T5 * dVdseffCV_dVb + T6 * dAbulkCV_dVb) + Cgg1 * dVgsteff_dVb;
                    Cgg1 *= dVgsteff_dVg;

                    T7 = 1.0 - AbulkCV;
                    qbulk = CoxWL * T7 * (0.5 * VdseffCV - T3);
                    T4 = -T7 * (T4 - 1.0);
                    T5 = -T7 * T5;
                    T6 = -(T7 * T6 + (0.5 * VdseffCV - T3));
                    Cbg1 = CoxWL * (T4 + T5 * dVdseffCV_dVg);
                    Cbd1 = CoxWL * T5 * dVdseffCV_dVd + Cbg1 * dVgsteff_dVd;
                    Cbb1 = CoxWL * (T5 * dVdseffCV_dVb + T6 * dAbulkCV_dVb) + Cbg1 * dVgsteff_dVb;
                    Cbg1 *= dVgsteff_dVg;

                    if (model.BSIM3xpart > 0.5)
                    {
                        /* 0 / 100 Charge petition model */
                        T1 = T1 + T1;
                        qsrc = -CoxWL * (0.5 * Vgsteff + 0.25 * T0 - T0 * T0 / T1);
                        T7 = (4.0 * Vgsteff - T0) / (T1 * T1);
                        T4 = -(0.5 + 24.0 * T0 * T0 / (T1 * T1));
                        T5 = -(0.25 * AbulkCV - 12.0 * AbulkCV * T0 * T7);
                        T6 = -(0.25 * VdseffCV - 12.0 * T0 * VdseffCV * T7);
                        Csg = CoxWL * (T4 + T5 * dVdseffCV_dVg);
                        Csd = CoxWL * T5 * dVdseffCV_dVd + Csg * dVgsteff_dVd;
                        Csb = CoxWL * (T5 * dVdseffCV_dVb + T6 * dAbulkCV_dVb) + Csg * dVgsteff_dVb;
                        Csg *= dVgsteff_dVg;
                    }
                    else if (model.BSIM3xpart < 0.5)
                    {
                        /* 40 / 60 Charge petition model */
                        T1 = T1 / 12.0;
                        T2 = 0.5 * CoxWL / (T1 * T1);
                        T3 = Vgsteff * (2.0 * T0 * T0 / 3.0 + Vgsteff * (Vgsteff - 4.0 * T0 / 3.0)) - 2.0 * T0 * T0 * T0 / 15.0;
                        qsrc = -T2 * T3;
                        T7 = 4.0 / 3.0 * Vgsteff * (Vgsteff - T0) + 0.4 * T0 * T0;
                        T4 = -2.0 * qsrc / T1 - T2 * (Vgsteff * (3.0 * Vgsteff - 8.0 * T0 / 3.0) + 2.0 * T0 * T0 / 3.0);
                        T5 = (qsrc / T1 + T2 * T7) * AbulkCV;
                        T6 = (qsrc / T1 * VdseffCV + T2 * T7 * VdseffCV);
                        Csg = (T4 + T5 * dVdseffCV_dVg);
                        Csd = T5 * dVdseffCV_dVd + Csg * dVgsteff_dVd;
                        Csb = (T5 * dVdseffCV_dVb + T6 * dAbulkCV_dVb) + Csg * dVgsteff_dVb;
                        Csg *= dVgsteff_dVg;
                    }
                    else
                    {
                        /* 50 / 50 Charge petition model */
                        qsrc = -0.5 * (qgate + qbulk);
                        Csg = -0.5 * (Cgg1 + Cbg1);
                        Csb = -0.5 * (Cgb1 + Cbb1);
                        Csd = -0.5 * (Cgd1 + Cbd1);
                    }

                    qgate += Qac0 + Qsub0;
                    qbulk -= (Qac0 + Qsub0);
                    qdrn = -(qgate + qbulk + qsrc);

                    Cgg = dQac0_dVg + dQsub0_dVg + Cgg1;
                    Cgd = dQsub0_dVd + Cgd1;
                    Cgb = dQac0_dVb + dQsub0_dVb + Cgb1;

                    Cbg = Cbg1 - dQac0_dVg - dQsub0_dVg;
                    Cbd = Cbd1 - dQsub0_dVd;
                    Cbb = Cbb1 - dQac0_dVb - dQsub0_dVb;

                    Cgb *= dVbseff_dVb;
                    Cbb *= dVbseff_dVb;
                    Csb *= dVbseff_dVb;

                    bsim3.BSIM3cggb = Cgg;
                    bsim3.BSIM3cgsb = -(Cgg + Cgd + Cgb);
                    bsim3.BSIM3cgdb = Cgd;
                    bsim3.BSIM3cdgb = -(Cgg + Cbg + Csg);
                    bsim3.BSIM3cdsb = (Cgg + Cgd + Cgb + Cbg + Cbd + Cbb + Csg + Csd + Csb);
                    bsim3.BSIM3cddb = -(Cgd + Cbd + Csd);
                    bsim3.BSIM3cbgb = Cbg;
                    bsim3.BSIM3cbsb = -(Cbg + Cbd + Cbb);
                    bsim3.BSIM3cbdb = Cbd;
                    bsim3.BSIM3qinv = qinoi;
                }

                /* New Charge - Thickness capMod (CTM) begins */
                else if (model.BSIM3capMod.Value == 3)
                {
                    V3 = bsim3.pParam.BSIM3vfbzb - Vgs_eff + VbseffCV - Transistor.DELTA_3;
                    if (bsim3.pParam.BSIM3vfbzb <= 0.0)
                    {
                        T0 = Math.Sqrt(V3 * V3 - 4.0 * Transistor.DELTA_3 * bsim3.pParam.BSIM3vfbzb);
                        T2 = -Transistor.DELTA_3 / T0;
                    }
                    else
                    {
                        T0 = Math.Sqrt(V3 * V3 + 4.0 * Transistor.DELTA_3 * bsim3.pParam.BSIM3vfbzb);
                        T2 = Transistor.DELTA_3 / T0;
                    }

                    T1 = 0.5 * (1.0 + V3 / T0);
                    Vfbeff = bsim3.pParam.BSIM3vfbzb - 0.5 * (V3 + T0);
                    dVfbeff_dVg = T1 * dVgs_eff_dVg;
                    dVfbeff_dVb = -T1 * dVbseffCV_dVb;

                    Cox = model.BSIM3cox;
                    Tox = 1.0e8 * model.BSIM3tox;
                    T0 = (Vgs_eff - VbseffCV - bsim3.pParam.BSIM3vfbzb) / Tox;
                    dT0_dVg = dVgs_eff_dVg / Tox;
                    dT0_dVb = -dVbseffCV_dVb / Tox;

                    tmp = T0 * bsim3.pParam.BSIM3acde;
                    if ((-Transistor.EXP_THRESHOLD < tmp) && (tmp < Transistor.EXP_THRESHOLD))
                    {
                        Tcen = bsim3.pParam.BSIM3ldeb * Math.Exp(tmp);
                        dTcen_dVg = bsim3.pParam.BSIM3acde * Tcen;
                        dTcen_dVb = dTcen_dVg * dT0_dVb;
                        dTcen_dVg *= dT0_dVg;
                    }
                    else if (tmp <= -Transistor.EXP_THRESHOLD)
                    {
                        Tcen = bsim3.pParam.BSIM3ldeb * Transistor.MIN_EXP;
                        dTcen_dVg = dTcen_dVb = 0.0;
                    }
                    else
                    {
                        Tcen = bsim3.pParam.BSIM3ldeb * Transistor.MAX_EXP;
                        dTcen_dVg = dTcen_dVb = 0.0;
                    }

                    LINK = 1.0e-3 * model.BSIM3tox;
                    V3 = bsim3.pParam.BSIM3ldeb - Tcen - LINK;
                    V4 = Math.Sqrt(V3 * V3 + 4.0 * LINK * bsim3.pParam.BSIM3ldeb);
                    Tcen = bsim3.pParam.BSIM3ldeb - 0.5 * (V3 + V4);
                    T1 = 0.5 * (1.0 + V3 / V4);
                    dTcen_dVg *= T1;
                    dTcen_dVb *= T1;

                    Ccen = Transistor.EPSSI / Tcen;
                    T2 = Cox / (Cox + Ccen);
                    Coxeff = T2 * Ccen;
                    T3 = -Ccen / Tcen;
                    dCoxeff_dVg = T2 * T2 * T3;
                    dCoxeff_dVb = dCoxeff_dVg * dTcen_dVb;
                    dCoxeff_dVg *= dTcen_dVg;
                    CoxWLcen = CoxWL * Coxeff / Cox;

                    Qac0 = CoxWLcen * (Vfbeff - bsim3.pParam.BSIM3vfbzb);
                    QovCox = Qac0 / Coxeff;
                    dQac0_dVg = CoxWLcen * dVfbeff_dVg + QovCox * dCoxeff_dVg;
                    dQac0_dVb = CoxWLcen * dVfbeff_dVb + QovCox * dCoxeff_dVb;

                    T0 = 0.5 * bsim3.pParam.BSIM3k1ox;
                    T3 = Vgs_eff - Vfbeff - VbseffCV - Vgsteff;
                    if (bsim3.pParam.BSIM3k1ox == 0.0)
                    {
                        T1 = 0.0;
                        T2 = 0.0;
                    }
                    else if (T3 < 0.0)
                    {
                        T1 = T0 + T3 / bsim3.pParam.BSIM3k1ox;
                        T2 = CoxWLcen;
                    }
                    else
                    {
                        T1 = Math.Sqrt(T0 * T0 + T3);
                        T2 = CoxWLcen * T0 / T1;
                    }

                    Qsub0 = CoxWLcen * bsim3.pParam.BSIM3k1ox * (T1 - T0);
                    QovCox = Qsub0 / Coxeff;
                    dQsub0_dVg = T2 * (dVgs_eff_dVg - dVfbeff_dVg - dVgsteff_dVg) + QovCox * dCoxeff_dVg;
                    dQsub0_dVd = -T2 * dVgsteff_dVd;
                    dQsub0_dVb = -T2 * (dVfbeff_dVb + dVbseffCV_dVb + dVgsteff_dVb) + QovCox * dCoxeff_dVb;

                    /* Gate - bias dependent delta Phis begins */
                    if (bsim3.pParam.BSIM3k1ox <= 0.0)
                    {
                        Denomi = 0.25 * bsim3.pParam.BSIM3moin * Vtm;
                        T0 = 0.5 * bsim3.pParam.BSIM3sqrtPhi;
                    }
                    else
                    {
                        Denomi = bsim3.pParam.BSIM3moin * Vtm * bsim3.pParam.BSIM3k1ox * bsim3.pParam.BSIM3k1ox;
                        T0 = bsim3.pParam.BSIM3k1ox * bsim3.pParam.BSIM3sqrtPhi;
                    }
                    T1 = 2.0 * T0 + Vgsteff;

                    DeltaPhi = Vtm * Math.Log(1.0 + T1 * Vgsteff / Denomi);
                    dDeltaPhi_dVg = 2.0 * Vtm * (T1 - T0) / (Denomi + T1 * Vgsteff);
                    dDeltaPhi_dVd = dDeltaPhi_dVg * dVgsteff_dVd;
                    dDeltaPhi_dVb = dDeltaPhi_dVg * dVgsteff_dVb;
                    /* End of delta Phis */

                    T3 = 4.0 * (Vth - bsim3.pParam.BSIM3vfbzb - bsim3.pParam.BSIM3phi);
                    Tox += Tox;
                    if (T3 >= 0.0)
                    {
                        T0 = (Vgsteff + T3) / Tox;
                        dT0_dVd = (dVgsteff_dVd + 4.0 * dVth_dVd) / Tox;
                        dT0_dVb = (dVgsteff_dVb + 4.0 * dVth_dVb) / Tox;
                    }
                    else
                    {
                        T0 = (Vgsteff + 1.0e-20) / Tox;
                        dT0_dVd = dVgsteff_dVd / Tox;
                        dT0_dVb = dVgsteff_dVb / Tox;
                    }
                    tmp = Math.Exp(0.7 * Math.Log(T0));
                    T1 = 1.0 + tmp;
                    T2 = 0.7 * tmp / (T0 * Tox);
                    Tcen = 1.9e-9 / T1;
                    dTcen_dVg = -1.9e-9 * T2 / T1 / T1;
                    dTcen_dVd = Tox * dTcen_dVg;
                    dTcen_dVb = dTcen_dVd * dT0_dVb;
                    dTcen_dVd *= dT0_dVd;
                    dTcen_dVg *= dVgsteff_dVg;

                    Ccen = Transistor.EPSSI / Tcen;
                    T0 = Cox / (Cox + Ccen);
                    Coxeff = T0 * Ccen;
                    T1 = -Ccen / Tcen;
                    dCoxeff_dVg = T0 * T0 * T1;
                    dCoxeff_dVd = dCoxeff_dVg * dTcen_dVd;
                    dCoxeff_dVb = dCoxeff_dVg * dTcen_dVb;
                    dCoxeff_dVg *= dTcen_dVg;
                    CoxWLcen = CoxWL * Coxeff / Cox;

                    AbulkCV = Abulk0 * bsim3.pParam.BSIM3abulkCVfactor;
                    dAbulkCV_dVb = bsim3.pParam.BSIM3abulkCVfactor * dAbulk0_dVb;
                    VdsatCV = (Vgsteff - DeltaPhi) / AbulkCV;
                    V4 = VdsatCV - Vds - Transistor.DELTA_4;
                    T0 = Math.Sqrt(V4 * V4 + 4.0 * Transistor.DELTA_4 * VdsatCV);
                    VdseffCV = VdsatCV - 0.5 * (V4 + T0);
                    T1 = 0.5 * (1.0 + V4 / T0);
                    T2 = Transistor.DELTA_4 / T0;
                    T3 = (1.0 - T1 - T2) / AbulkCV;
                    T4 = T3 * (1.0 - dDeltaPhi_dVg);
                    dVdseffCV_dVg = T4;
                    dVdseffCV_dVd = T1;
                    dVdseffCV_dVb = -T3 * VdsatCV * dAbulkCV_dVb;
                    /* Added to eliminate non - zero VdseffCV at Vds = 0.0 */
                    if (Vds == 0.0)
                    {
                        VdseffCV = 0.0;
                        dVdseffCV_dVg = 0.0;
                        dVdseffCV_dVb = 0.0;
                    }

                    T0 = AbulkCV * VdseffCV;
                    T1 = Vgsteff - DeltaPhi;
                    T2 = 12.0 * (T1 - 0.5 * T0 + 1.0e-20);
                    T3 = T0 / T2;
                    T4 = 1.0 - 12.0 * T3 * T3;
                    T5 = AbulkCV * (6.0 * T0 * (4.0 * T1 - T0) / (T2 * T2) - 0.5);
                    T6 = T5 * VdseffCV / AbulkCV;

                    qgate = qinoi = CoxWLcen * (T1 - T0 * (0.5 - T3));
                    QovCox = qgate / Coxeff;
                    Cgg1 = CoxWLcen * (T4 * (1.0 - dDeltaPhi_dVg) + T5 * dVdseffCV_dVg);
                    Cgd1 = CoxWLcen * T5 * dVdseffCV_dVd + Cgg1 * dVgsteff_dVd + QovCox * dCoxeff_dVd;
                    Cgb1 = CoxWLcen * (T5 * dVdseffCV_dVb + T6 * dAbulkCV_dVb) + Cgg1 * dVgsteff_dVb + QovCox * dCoxeff_dVb;
                    Cgg1 = Cgg1 * dVgsteff_dVg + QovCox * dCoxeff_dVg;

                    T7 = 1.0 - AbulkCV;
                    T8 = T2 * T2;
                    T9 = 12.0 * T7 * T0 * T0 / (T8 * AbulkCV);
                    T10 = T9 * (1.0 - dDeltaPhi_dVg);
                    T11 = -T7 * T5 / AbulkCV;
                    T12 = -(T9 * T1 / AbulkCV + VdseffCV * (0.5 - T0 / T2));

                    qbulk = CoxWLcen * T7 * (0.5 * VdseffCV - T0 * VdseffCV / T2);
                    QovCox = qbulk / Coxeff;
                    Cbg1 = CoxWLcen * (T10 + T11 * dVdseffCV_dVg);
                    Cbd1 = CoxWLcen * T11 * dVdseffCV_dVd + Cbg1 * dVgsteff_dVd + QovCox * dCoxeff_dVd;
                    Cbb1 = CoxWLcen * (T11 * dVdseffCV_dVb + T12 * dAbulkCV_dVb) + Cbg1 * dVgsteff_dVb + QovCox * dCoxeff_dVb;
                    Cbg1 = Cbg1 * dVgsteff_dVg + QovCox * dCoxeff_dVg;

                    if (model.BSIM3xpart > 0.5)
                    {
                        /* 0 / 100 partition */
                        qsrc = -CoxWLcen * (T1 / 2.0 + T0 / 4.0 - 0.5 * T0 * T0 / T2);
                        QovCox = qsrc / Coxeff;
                        T2 += T2;
                        T3 = T2 * T2;
                        T7 = -(0.25 - 12.0 * T0 * (4.0 * T1 - T0) / T3);
                        T4 = -(0.5 + 24.0 * T0 * T0 / T3) * (1.0 - dDeltaPhi_dVg);
                        T5 = T7 * AbulkCV;
                        T6 = T7 * VdseffCV;

                        Csg = CoxWLcen * (T4 + T5 * dVdseffCV_dVg);
                        Csd = CoxWLcen * T5 * dVdseffCV_dVd + Csg * dVgsteff_dVd + QovCox * dCoxeff_dVd;
                        Csb = CoxWLcen * (T5 * dVdseffCV_dVb + T6 * dAbulkCV_dVb) + Csg * dVgsteff_dVb + QovCox * dCoxeff_dVb;
                        Csg = Csg * dVgsteff_dVg + QovCox * dCoxeff_dVg;
                    }
                    else if (model.BSIM3xpart < 0.5)
                    {
                        /* 40 / 60 partition */
                        T2 = T2 / 12.0;
                        T3 = 0.5 * CoxWLcen / (T2 * T2);
                        T4 = T1 * (2.0 * T0 * T0 / 3.0 + T1 * (T1 - 4.0 * T0 / 3.0)) - 2.0 * T0 * T0 * T0 / 15.0;
                        qsrc = -T3 * T4;
                        QovCox = qsrc / Coxeff;
                        T8 = 4.0 / 3.0 * T1 * (T1 - T0) + 0.4 * T0 * T0;
                        T5 = -2.0 * qsrc / T2 - T3 * (T1 * (3.0 * T1 - 8.0 * T0 / 3.0) + 2.0 * T0 * T0 / 3.0);
                        T6 = AbulkCV * (qsrc / T2 + T3 * T8);
                        T7 = T6 * VdseffCV / AbulkCV;

                        Csg = T5 * (1.0 - dDeltaPhi_dVg) + T6 * dVdseffCV_dVg;
                        Csd = Csg * dVgsteff_dVd + T6 * dVdseffCV_dVd + QovCox * dCoxeff_dVd;
                        Csb = Csg * dVgsteff_dVb + T6 * dVdseffCV_dVb + T7 * dAbulkCV_dVb + QovCox * dCoxeff_dVb;
                        Csg = Csg * dVgsteff_dVg + QovCox * dCoxeff_dVg;
                    }
                    else
                    {
                        /* 50 / 50 partition */
                        qsrc = -0.5 * qgate;
                        Csg = -0.5 * Cgg1;
                        Csd = -0.5 * Cgd1;
                        Csb = -0.5 * Cgb1;
                    }

                    qgate += Qac0 + Qsub0 - qbulk;
                    qbulk -= (Qac0 + Qsub0);
                    qdrn = -(qgate + qbulk + qsrc);

                    Cbg = Cbg1 - dQac0_dVg - dQsub0_dVg;
                    Cbd = Cbd1 - dQsub0_dVd;
                    Cbb = Cbb1 - dQac0_dVb - dQsub0_dVb;

                    Cgg = Cgg1 - Cbg;
                    Cgd = Cgd1 - Cbd;
                    Cgb = Cgb1 - Cbb;

                    Cgb *= dVbseff_dVb;
                    Cbb *= dVbseff_dVb;
                    Csb *= dVbseff_dVb;

                    bsim3.BSIM3cggb = Cgg;
                    bsim3.BSIM3cgsb = -(Cgg + Cgd + Cgb);
                    bsim3.BSIM3cgdb = Cgd;
                    bsim3.BSIM3cdgb = -(Cgg + Cbg + Csg);
                    bsim3.BSIM3cdsb = (Cgg + Cgd + Cgb + Cbg + Cbd + Cbb + Csg + Csd + Csb);
                    bsim3.BSIM3cddb = -(Cgd + Cbd + Csd);
                    bsim3.BSIM3cbgb = Cbg;
                    bsim3.BSIM3cbsb = -(Cbg + Cbd + Cbb);
                    bsim3.BSIM3cbdb = Cbd;
                    bsim3.BSIM3qinv = -qinoi;
                } /* End of CTM */
            }

            finished:
            /* Returning Values to Calling Routine */
            /* 
            * COMPUTE EQUIVALENT DRAIN CURRENT SOURCE
            */

            bsim3.BSIM3qgate = qgate;
            bsim3.BSIM3qbulk = qbulk;
            bsim3.BSIM3qdrn = qdrn;
            bsim3.BSIM3cd = cdrain;

            if (ChargeComputationNeeded)
            {
                /* charge storage elements
                * bulk - drain and bulk - source depletion capacitances
                * czbd : zero bias drain junction capacitance
                * czbs : zero bias source junction capacitance
                * czbdsw: zero bias drain junction sidewall capacitance
                along field oxide
                * czbssw: zero bias source junction sidewall capacitance
                along field oxide
                * czbdswg: zero bias drain junction sidewall capacitance
                along gate side
                * czbsswg: zero bias source junction sidewall capacitance
                along gate side
                */

                czbd = model.BSIM3unitAreaTempJctCap * bsim3.BSIM3drainArea; /* bug fix */
                czbs = model.BSIM3unitAreaTempJctCap * bsim3.BSIM3sourceArea;
                if (bsim3.BSIM3drainPerimeter < bsim3.pParam.BSIM3weff)
                {
                    czbdswg = model.BSIM3unitLengthGateSidewallTempJctCap * bsim3.BSIM3drainPerimeter;
                    czbdsw = 0.0;
                }
                else
                {
                    czbdsw = model.BSIM3unitLengthSidewallTempJctCap * (bsim3.BSIM3drainPerimeter - bsim3.pParam.BSIM3weff);
                    czbdswg = model.BSIM3unitLengthGateSidewallTempJctCap * bsim3.pParam.BSIM3weff;
                }
                if (bsim3.BSIM3sourcePerimeter < bsim3.pParam.BSIM3weff)
                {
                    czbssw = 0.0;
                    czbsswg = model.BSIM3unitLengthGateSidewallTempJctCap * bsim3.BSIM3sourcePerimeter;
                }
                else
                {
                    czbssw = model.BSIM3unitLengthSidewallTempJctCap * (bsim3.BSIM3sourcePerimeter - bsim3.pParam.BSIM3weff);
                    czbsswg = model.BSIM3unitLengthGateSidewallTempJctCap * bsim3.pParam.BSIM3weff;
                }

                MJ = model.BSIM3bulkJctBotGradingCoeff;
                MJSW = model.BSIM3bulkJctSideGradingCoeff;
                MJSWG = model.BSIM3bulkJctGateSideGradingCoeff;

                /* Source Bulk Junction */
                if (vbs == 0.0)
                {
                    state.States[0][bsim3.BSIM3states + BSIM3v24.BSIM3qbs] = 0.0;
                    bsim3.BSIM3capbs = czbs + czbssw + czbsswg;
                }
                else if (vbs < 0.0)
                {
                    if (czbs > 0.0)
                    {
                        arg = 1.0 - vbs / model.BSIM3PhiB;
                        if (MJ == 0.5)
                            sarg = 1.0 / Math.Sqrt(arg);
                        else
                            sarg = Math.Exp(-MJ * Math.Log(arg));
                        state.States[0][bsim3.BSIM3states + BSIM3v24.BSIM3qbs] = model.BSIM3PhiB * czbs * (1.0 - arg * sarg) / (1.0 - MJ);
                        bsim3.BSIM3capbs = czbs * sarg;
                    }
                    else
                    {
                        state.States[0][bsim3.BSIM3states + BSIM3v24.BSIM3qbs] = 0.0;
                        bsim3.BSIM3capbs = 0.0;
                    }
                    if (czbssw > 0.0)
                    {
                        arg = 1.0 - vbs / model.BSIM3PhiBSW;
                        if (MJSW == 0.5)
                            sarg = 1.0 / Math.Sqrt(arg);
                        else
                            sarg = Math.Exp(-MJSW * Math.Log(arg));
                        state.States[0][bsim3.BSIM3states + BSIM3v24.BSIM3qbs] += model.BSIM3PhiBSW * czbssw * (1.0 - arg * sarg) / (1.0 - MJSW);
                        bsim3.BSIM3capbs += czbssw * sarg;
                    }
                    if (czbsswg > 0.0)
                    {
                        arg = 1.0 - vbs / model.BSIM3PhiBSWG;
                        if (MJSWG == 0.5)
                            sarg = 1.0 / Math.Sqrt(arg);
                        else
                            sarg = Math.Exp(-MJSWG * Math.Log(arg));
                        state.States[0][bsim3.BSIM3states + BSIM3v24.BSIM3qbs] += model.BSIM3PhiBSWG * czbsswg * (1.0 - arg * sarg) / (1.0 - MJSWG);
                        bsim3.BSIM3capbs += czbsswg * sarg;
                    }

                }
                else
                {
                    T0 = czbs + czbssw + czbsswg;
                    T1 = vbs * (czbs * MJ / model.BSIM3PhiB + czbssw * MJSW / model.BSIM3PhiBSW + czbsswg * MJSWG / model.BSIM3PhiBSWG);
                    state.States[0][bsim3.BSIM3states + BSIM3v24.BSIM3qbs] = vbs * (T0 + 0.5 * T1);
                    bsim3.BSIM3capbs = T0 + T1;
                }

                /* Drain Bulk Junction */
                if (vbd == 0.0)
                {
                    state.States[0][bsim3.BSIM3states + BSIM3v24.BSIM3qbd] = 0.0;
                    bsim3.BSIM3capbd = czbd + czbdsw + czbdswg;
                }
                else if (vbd < 0.0)
                {
                    if (czbd > 0.0)
                    {
                        arg = 1.0 - vbd / model.BSIM3PhiB;
                        if (MJ == 0.5)
                            sarg = 1.0 / Math.Sqrt(arg);
                        else
                            sarg = Math.Exp(-MJ * Math.Log(arg));
                        state.States[0][bsim3.BSIM3states + BSIM3v24.BSIM3qbd] = model.BSIM3PhiB * czbd * (1.0 - arg * sarg) / (1.0 - MJ);
                        bsim3.BSIM3capbd = czbd * sarg;
                    }
                    else
                    {
                        state.States[0][bsim3.BSIM3states + BSIM3v24.BSIM3qbd] = 0.0;
                        bsim3.BSIM3capbd = 0.0;
                    }
                    if (czbdsw > 0.0)
                    {
                        arg = 1.0 - vbd / model.BSIM3PhiBSW;
                        if (MJSW == 0.5)
                            sarg = 1.0 / Math.Sqrt(arg);
                        else
                            sarg = Math.Exp(-MJSW * Math.Log(arg));
                        state.States[0][bsim3.BSIM3states + BSIM3v24.BSIM3qbd] += model.BSIM3PhiBSW * czbdsw * (1.0 - arg * sarg) / (1.0 - MJSW);
                        bsim3.BSIM3capbd += czbdsw * sarg;
                    }
                    if (czbdswg > 0.0)
                    {
                        arg = 1.0 - vbd / model.BSIM3PhiBSWG;
                        if (MJSWG == 0.5)
                            sarg = 1.0 / Math.Sqrt(arg);
                        else
                            sarg = Math.Exp(-MJSWG * Math.Log(arg));
                        state.States[0][bsim3.BSIM3states + BSIM3v24.BSIM3qbd] += model.BSIM3PhiBSWG * czbdswg * (1.0 - arg * sarg) / (1.0 - MJSWG);
                        bsim3.BSIM3capbd += czbdswg * sarg;
                    }
                }
                else
                {
                    T0 = czbd + czbdsw + czbdswg;
                    T1 = vbd * (czbd * MJ / model.BSIM3PhiB + czbdsw * MJSW / model.BSIM3PhiBSW + czbdswg * MJSWG / model.BSIM3PhiBSWG);
                    state.States[0][bsim3.BSIM3states + BSIM3v24.BSIM3qbd] = vbd * (T0 + 0.5 * T1);
                    bsim3.BSIM3capbd = T0 + T1;
                }
            }

            /* 
            * check convergence
            */
            if (!bsim3.BSIM3off || state.Init != CircuitState.InitFlags.InitFix)
            {
                if (Check == 1)
                    state.IsCon = false;
            }
            state.States[0][bsim3.BSIM3states + BSIM3v24.BSIM3vbs] = vbs;
            state.States[0][bsim3.BSIM3states + BSIM3v24.BSIM3vbd] = vbd;
            state.States[0][bsim3.BSIM3states + BSIM3v24.BSIM3vgs] = vgs;
            state.States[0][bsim3.BSIM3states + BSIM3v24.BSIM3vds] = vds;
            state.States[0][bsim3.BSIM3states + BSIM3v24.BSIM3qdef] = qdef;

            /* bulk and channel charge plus overlaps */

            if (!ChargeComputationNeeded)
                goto line850;

            /* NQS begins */
            if (bsim3.BSIM3nqsMod != 0)
            {
                qcheq = -(qbulk + qgate);

                bsim3.BSIM3cqgb = -(bsim3.BSIM3cggb + bsim3.BSIM3cbgb);
                bsim3.BSIM3cqdb = -(bsim3.BSIM3cgdb + bsim3.BSIM3cbdb);
                bsim3.BSIM3cqsb = -(bsim3.BSIM3cgsb + bsim3.BSIM3cbsb);
                bsim3.BSIM3cqbb = -(bsim3.BSIM3cqgb + bsim3.BSIM3cqdb + bsim3.BSIM3cqsb);

                gtau_drift = Math.Abs(bsim3.pParam.BSIM3tconst * qcheq) * BSIM3v24.ScalingFactor;
                T0 = bsim3.pParam.BSIM3leffCV * bsim3.pParam.BSIM3leffCV;
                gtau_diff = 16.0 * bsim3.pParam.BSIM3u0temp * model.BSIM3vtm / T0 * BSIM3v24.ScalingFactor;
                bsim3.BSIM3gtau = gtau_drift + gtau_diff;
            }

            if (model.BSIM3capMod.Value == 0)
            /* code merge - JX */
            {
                cgdo = bsim3.pParam.BSIM3cgdo;
                qgdo = bsim3.pParam.BSIM3cgdo * vgd;
                cgso = bsim3.pParam.BSIM3cgso;
                qgso = bsim3.pParam.BSIM3cgso * vgs;
            }
            else if (model.BSIM3capMod.Value == 1)
            {
                if (vgd < 0.0)
                {
                    T1 = Math.Sqrt(1.0 - 4.0 * vgd / bsim3.pParam.BSIM3ckappa);
                    cgdo = bsim3.pParam.BSIM3cgdo + bsim3.pParam.BSIM3weffCV * bsim3.pParam.BSIM3cgdl / T1;
                    qgdo = bsim3.pParam.BSIM3cgdo * vgd - bsim3.pParam.BSIM3weffCV * 0.5 * bsim3.pParam.BSIM3cgdl * bsim3.pParam.BSIM3ckappa * (T1 - 1.0);
                }
                else
                {
                    cgdo = bsim3.pParam.BSIM3cgdo + bsim3.pParam.BSIM3weffCV * bsim3.pParam.BSIM3cgdl;
                    qgdo = (bsim3.pParam.BSIM3weffCV * bsim3.pParam.BSIM3cgdl + bsim3.pParam.BSIM3cgdo) * vgd;
                }

                if (vgs < 0.0)
                {
                    T1 = Math.Sqrt(1.0 - 4.0 * vgs / bsim3.pParam.BSIM3ckappa);
                    cgso = bsim3.pParam.BSIM3cgso + bsim3.pParam.BSIM3weffCV * bsim3.pParam.BSIM3cgsl / T1;
                    qgso = bsim3.pParam.BSIM3cgso * vgs - bsim3.pParam.BSIM3weffCV * 0.5 * bsim3.pParam.BSIM3cgsl * bsim3.pParam.BSIM3ckappa * (T1 - 1.0);
                }
                else
                {
                    cgso = bsim3.pParam.BSIM3cgso + bsim3.pParam.BSIM3weffCV * bsim3.pParam.BSIM3cgsl;
                    qgso = (bsim3.pParam.BSIM3weffCV * bsim3.pParam.BSIM3cgsl + bsim3.pParam.BSIM3cgso) * vgs;
                }
            }
            else
            {
                T0 = vgd + Transistor.DELTA_1;
                T1 = Math.Sqrt(T0 * T0 + 4.0 * Transistor.DELTA_1);
                T2 = 0.5 * (T0 - T1);

                T3 = bsim3.pParam.BSIM3weffCV * bsim3.pParam.BSIM3cgdl;
                T4 = Math.Sqrt(1.0 - 4.0 * T2 / bsim3.pParam.BSIM3ckappa);
                cgdo = bsim3.pParam.BSIM3cgdo + T3 - T3 * (1.0 - 1.0 / T4) * (0.5 - 0.5 * T0 / T1);
                qgdo = (bsim3.pParam.BSIM3cgdo + T3) * vgd - T3 * (T2 + 0.5 * bsim3.pParam.BSIM3ckappa * (T4 - 1.0));

                T0 = vgs + Transistor.DELTA_1;
                T1 = Math.Sqrt(T0 * T0 + 4.0 * Transistor.DELTA_1);
                T2 = 0.5 * (T0 - T1);
                T3 = bsim3.pParam.BSIM3weffCV * bsim3.pParam.BSIM3cgsl;
                T4 = Math.Sqrt(1.0 - 4.0 * T2 / bsim3.pParam.BSIM3ckappa);
                cgso = bsim3.pParam.BSIM3cgso + T3 - T3 * (1.0 - 1.0 / T4) * (0.5 - 0.5 * T0 / T1);
                qgso = (bsim3.pParam.BSIM3cgso + T3) * vgs - T3 * (T2 + 0.5 * bsim3.pParam.BSIM3ckappa * (T4 - 1.0));
            }

            bsim3.BSIM3cgdo = cgdo;
            bsim3.BSIM3cgso = cgso;

            ag0 = method.Slope;
            if (bsim3.BSIM3mode > 0)
            {
                if (bsim3.BSIM3nqsMod.Value == 0)
                {
                    gcggb = (bsim3.BSIM3cggb + cgdo + cgso + bsim3.pParam.BSIM3cgbo) * ag0;
                    gcgdb = (bsim3.BSIM3cgdb - cgdo) * ag0;
                    gcgsb = (bsim3.BSIM3cgsb - cgso) * ag0;

                    gcdgb = (bsim3.BSIM3cdgb - cgdo) * ag0;
                    gcddb = (bsim3.BSIM3cddb + bsim3.BSIM3capbd + cgdo) * ag0;
                    gcdsb = bsim3.BSIM3cdsb * ag0;

                    gcsgb = -(bsim3.BSIM3cggb + bsim3.BSIM3cbgb + bsim3.BSIM3cdgb + cgso) * ag0;
                    gcsdb = -(bsim3.BSIM3cgdb + bsim3.BSIM3cbdb + bsim3.BSIM3cddb) * ag0;
                    gcssb = (bsim3.BSIM3capbs + cgso - (bsim3.BSIM3cgsb + bsim3.BSIM3cbsb + bsim3.BSIM3cdsb)) * ag0;

                    gcbgb = (bsim3.BSIM3cbgb - bsim3.pParam.BSIM3cgbo) * ag0;
                    gcbdb = (bsim3.BSIM3cbdb - bsim3.BSIM3capbd) * ag0;
                    gcbsb = (bsim3.BSIM3cbsb - bsim3.BSIM3capbs) * ag0;

                    qgd = qgdo;
                    qgs = qgso;
                    qgb = bsim3.pParam.BSIM3cgbo * vgb;
                    qgate += qgd + qgs + qgb;
                    qbulk -= qgb;
                    qdrn -= qgd;
                    qsrc = -(qgate + qbulk + qdrn);

                    ggtg = ggtd = ggtb = ggts = 0.0;
                    sxpart = 0.6;
                    dxpart = 0.4;
                    ddxpart_dVd = ddxpart_dVg = ddxpart_dVb = ddxpart_dVs = 0.0;
                    dsxpart_dVd = dsxpart_dVg = dsxpart_dVb = dsxpart_dVs = 0.0;
                }
                else
                {
                    if (qcheq > 0.0)
                        T0 = bsim3.pParam.BSIM3tconst * qdef * BSIM3v24.ScalingFactor;
                    else
                        T0 = -bsim3.pParam.BSIM3tconst * qdef * BSIM3v24.ScalingFactor;
                    ggtg = bsim3.BSIM3gtg = T0 * bsim3.BSIM3cqgb;
                    ggtd = bsim3.BSIM3gtd = T0 * bsim3.BSIM3cqdb;
                    ggts = bsim3.BSIM3gts = T0 * bsim3.BSIM3cqsb;
                    ggtb = bsim3.BSIM3gtb = T0 * bsim3.BSIM3cqbb;
                    gqdef = BSIM3v24.ScalingFactor * ag0;

                    gcqgb = bsim3.BSIM3cqgb * ag0;
                    gcqdb = bsim3.BSIM3cqdb * ag0;
                    gcqsb = bsim3.BSIM3cqsb * ag0;
                    gcqbb = bsim3.BSIM3cqbb * ag0;

                    gcggb = (cgdo + cgso + bsim3.pParam.BSIM3cgbo) * ag0;
                    gcgdb = -cgdo * ag0;
                    gcgsb = -cgso * ag0;

                    gcdgb = -cgdo * ag0;
                    gcddb = (bsim3.BSIM3capbd + cgdo) * ag0;
                    gcdsb = 0.0;

                    gcsgb = -cgso * ag0;
                    gcsdb = 0.0;
                    gcssb = (bsim3.BSIM3capbs + cgso) * ag0;

                    gcbgb = -bsim3.pParam.BSIM3cgbo * ag0;
                    gcbdb = -bsim3.BSIM3capbd * ag0;
                    gcbsb = -bsim3.BSIM3capbs * ag0;

                    CoxWL = model.BSIM3cox * bsim3.pParam.BSIM3weffCV * bsim3.pParam.BSIM3leffCV;
                    if (Math.Abs(qcheq) <= 1.0e-5 * CoxWL)
                    {
                        if (model.BSIM3xpart < 0.5)
                        {
                            dxpart = 0.4;
                        }
                        else if (model.BSIM3xpart > 0.5)
                        {
                            dxpart = 0.0;
                        }
                        else
                        {
                            dxpart = 0.5;
                        }
                        ddxpart_dVd = ddxpart_dVg = ddxpart_dVb = ddxpart_dVs = 0.0;
                    }
                    else
                    {
                        dxpart = qdrn / qcheq;
                        Cdd = bsim3.BSIM3cddb;
                        Csd = -(bsim3.BSIM3cgdb + bsim3.BSIM3cddb + bsim3.BSIM3cbdb);
                        ddxpart_dVd = (Cdd - dxpart * (Cdd + Csd)) / qcheq;
                        Cdg = bsim3.BSIM3cdgb;
                        Csg = -(bsim3.BSIM3cggb + bsim3.BSIM3cdgb + bsim3.BSIM3cbgb);
                        ddxpart_dVg = (Cdg - dxpart * (Cdg + Csg)) / qcheq;

                        Cds = bsim3.BSIM3cdsb;
                        Css = -(bsim3.BSIM3cgsb + bsim3.BSIM3cdsb + bsim3.BSIM3cbsb);
                        ddxpart_dVs = (Cds - dxpart * (Cds + Css)) / qcheq;

                        ddxpart_dVb = -(ddxpart_dVd + ddxpart_dVg + ddxpart_dVs);
                    }
                    sxpart = 1.0 - dxpart;
                    dsxpart_dVd = -ddxpart_dVd;
                    dsxpart_dVg = -ddxpart_dVg;
                    dsxpart_dVs = -ddxpart_dVs;
                    dsxpart_dVb = -(dsxpart_dVd + dsxpart_dVg + dsxpart_dVs);

                    qgd = qgdo;
                    qgs = qgso;
                    qgb = bsim3.pParam.BSIM3cgbo * vgb;
                    qgate = qgd + qgs + qgb;
                    qbulk = -qgb;
                    qdrn = -qgd;
                    qsrc = -(qgate + qbulk + qdrn);
                }
            }
            else
            {
                if (bsim3.BSIM3nqsMod.Value == 0)
                {
                    gcggb = (bsim3.BSIM3cggb + cgdo + cgso + bsim3.pParam.BSIM3cgbo) * ag0;
                    gcgdb = (bsim3.BSIM3cgsb - cgdo) * ag0;
                    gcgsb = (bsim3.BSIM3cgdb - cgso) * ag0;

                    gcdgb = -(bsim3.BSIM3cggb + bsim3.BSIM3cbgb + bsim3.BSIM3cdgb + cgdo) * ag0;
                    gcddb = (bsim3.BSIM3capbd + cgdo - (bsim3.BSIM3cgsb + bsim3.BSIM3cbsb + bsim3.BSIM3cdsb)) * ag0;
                    gcdsb = -(bsim3.BSIM3cgdb + bsim3.BSIM3cbdb + bsim3.BSIM3cddb) * ag0;

                    gcsgb = (bsim3.BSIM3cdgb - cgso) * ag0;
                    gcsdb = bsim3.BSIM3cdsb * ag0;
                    gcssb = (bsim3.BSIM3cddb + bsim3.BSIM3capbs + cgso) * ag0;

                    gcbgb = (bsim3.BSIM3cbgb - bsim3.pParam.BSIM3cgbo) * ag0;
                    gcbdb = (bsim3.BSIM3cbsb - bsim3.BSIM3capbd) * ag0;
                    gcbsb = (bsim3.BSIM3cbdb - bsim3.BSIM3capbs) * ag0;

                    qgd = qgdo;
                    qgs = qgso;
                    qgb = bsim3.pParam.BSIM3cgbo * vgb;
                    qgate += qgd + qgs + qgb;
                    qbulk -= qgb;
                    qsrc = qdrn - qgs;
                    qdrn = -(qgate + qbulk + qsrc);

                    ggtg = ggtd = ggtb = ggts = 0.0;
                    sxpart = 0.4;
                    dxpart = 0.6;
                    ddxpart_dVd = ddxpart_dVg = ddxpart_dVb = ddxpart_dVs = 0.0;
                    dsxpart_dVd = dsxpart_dVg = dsxpart_dVb = dsxpart_dVs = 0.0;
                }
                else
                {
                    if (qcheq > 0.0)
                        T0 = bsim3.pParam.BSIM3tconst * qdef * BSIM3v24.ScalingFactor;
                    else
                        T0 = -bsim3.pParam.BSIM3tconst * qdef * BSIM3v24.ScalingFactor;
                    ggtg = bsim3.BSIM3gtg = T0 * bsim3.BSIM3cqgb;
                    ggts = bsim3.BSIM3gtd = T0 * bsim3.BSIM3cqdb;
                    ggtd = bsim3.BSIM3gts = T0 * bsim3.BSIM3cqsb;
                    ggtb = bsim3.BSIM3gtb = T0 * bsim3.BSIM3cqbb;
                    gqdef = BSIM3v24.ScalingFactor * ag0;

                    gcqgb = bsim3.BSIM3cqgb * ag0;
                    gcqdb = bsim3.BSIM3cqsb * ag0;
                    gcqsb = bsim3.BSIM3cqdb * ag0;
                    gcqbb = bsim3.BSIM3cqbb * ag0;

                    gcggb = (cgdo + cgso + bsim3.pParam.BSIM3cgbo) * ag0;
                    gcgdb = -cgdo * ag0;
                    gcgsb = -cgso * ag0;

                    gcdgb = -cgdo * ag0;
                    gcddb = (bsim3.BSIM3capbd + cgdo) * ag0;
                    gcdsb = 0.0;

                    gcsgb = -cgso * ag0;
                    gcsdb = 0.0;
                    gcssb = (bsim3.BSIM3capbs + cgso) * ag0;

                    gcbgb = -bsim3.pParam.BSIM3cgbo * ag0;
                    gcbdb = -bsim3.BSIM3capbd * ag0;
                    gcbsb = -bsim3.BSIM3capbs * ag0;

                    CoxWL = model.BSIM3cox * bsim3.pParam.BSIM3weffCV * bsim3.pParam.BSIM3leffCV;
                    if (Math.Abs(qcheq) <= 1.0e-5 * CoxWL)
                    {
                        if (model.BSIM3xpart < 0.5)
                        {
                            sxpart = 0.4;
                        }
                        else if (model.BSIM3xpart > 0.5)
                        {
                            sxpart = 0.0;
                        }
                        else
                        {
                            sxpart = 0.5;
                        }
                        dsxpart_dVd = dsxpart_dVg = dsxpart_dVb = dsxpart_dVs = 0.0;
                    }
                    else
                    {
                        sxpart = qdrn / qcheq;
                        Css = bsim3.BSIM3cddb;
                        Cds = -(bsim3.BSIM3cgdb + bsim3.BSIM3cddb + bsim3.BSIM3cbdb);
                        dsxpart_dVs = (Css - sxpart * (Css + Cds)) / qcheq;
                        Csg = bsim3.BSIM3cdgb;
                        Cdg = -(bsim3.BSIM3cggb + bsim3.BSIM3cdgb + bsim3.BSIM3cbgb);
                        dsxpart_dVg = (Csg - sxpart * (Csg + Cdg)) / qcheq;

                        Csd = bsim3.BSIM3cdsb;
                        Cdd = -(bsim3.BSIM3cgsb + bsim3.BSIM3cdsb + bsim3.BSIM3cbsb);
                        dsxpart_dVd = (Csd - sxpart * (Csd + Cdd)) / qcheq;

                        dsxpart_dVb = -(dsxpart_dVd + dsxpart_dVg + dsxpart_dVs);
                    }
                    dxpart = 1.0 - sxpart;
                    ddxpart_dVd = -dsxpart_dVd;
                    ddxpart_dVg = -dsxpart_dVg;
                    ddxpart_dVs = -dsxpart_dVs;
                    ddxpart_dVb = -(ddxpart_dVd + ddxpart_dVg + ddxpart_dVs);

                    qgd = qgdo;
                    qgs = qgso;
                    qgb = bsim3.pParam.BSIM3cgbo * vgb;
                    qgate = qgd + qgs + qgb;
                    qbulk = -qgb;
                    qsrc = -qgs;
                    qdrn = -(qgate + qbulk + qsrc);
                }
            }

            cqdef = cqcheq = 0.0;

            state.States[0][bsim3.BSIM3states + BSIM3v24.BSIM3qg] = qgate;
            state.States[0][bsim3.BSIM3states + BSIM3v24.BSIM3qd] = qdrn - state.States[0][bsim3.BSIM3states + BSIM3v24.BSIM3qbd];
            state.States[0][bsim3.BSIM3states + BSIM3v24.BSIM3qb] = qbulk + state.States[0][bsim3.BSIM3states + BSIM3v24.BSIM3qbd] + state.States[0][bsim3.BSIM3states +
                BSIM3v24.BSIM3qbs];

            if (bsim3.BSIM3nqsMod != 0)
            {
                state.States[0][bsim3.BSIM3states + BSIM3v24.BSIM3qcdump] = qdef * BSIM3v24.ScalingFactor;
                state.States[0][bsim3.BSIM3states + BSIM3v24.BSIM3qcheq] = qcheq;
            }

            /* store small signal parameters */
            if (state.UseSmallSignal)
                goto line1000;
            if (!ChargeComputationNeeded)
                goto line850;

            if (method != null && method.SavedTime == 0.0)
            {
                state.States[1][bsim3.BSIM3states + BSIM3v24.BSIM3qb] = state.States[0][bsim3.BSIM3states + BSIM3v24.BSIM3qb];
                state.States[1][bsim3.BSIM3states + BSIM3v24.BSIM3qg] = state.States[0][bsim3.BSIM3states + BSIM3v24.BSIM3qg];
                state.States[1][bsim3.BSIM3states + BSIM3v24.BSIM3qd] = state.States[0][bsim3.BSIM3states + BSIM3v24.BSIM3qd];
                if (bsim3.BSIM3nqsMod != 0)
                {
                    state.States[1][bsim3.BSIM3states + BSIM3v24.BSIM3qcheq] = state.States[0][bsim3.BSIM3states + BSIM3v24.BSIM3qcheq];
                    state.States[1][bsim3.BSIM3states + BSIM3v24.BSIM3qcdump] = state.States[0][bsim3.BSIM3states + BSIM3v24.BSIM3qcdump];
                }
            }

            if (method != null)
            {
                method.Integrate(state, bsim3.BSIM3states + BSIM3v24.BSIM3qb, 0.0);
                method.Integrate(state, bsim3.BSIM3states + BSIM3v24.BSIM3qg, 0.0);
                method.Integrate(state, bsim3.BSIM3states + BSIM3v24.BSIM3qd, 0.0);
                if (bsim3.BSIM3nqsMod != 0)
                {
                    method.Integrate(state, bsim3.BSIM3states + BSIM3v24.BSIM3qcdump, 0.0);
                    method.Integrate(state, bsim3.BSIM3states + BSIM3v24.BSIM3qcheq, 0.0);
                }
            }

            goto line860;

            line850:
            /* initialize to zero charge conductance and current */
            ceqqg = ceqqb = ceqqd = 0.0;
            cqcheq = cqdef = 0.0;

            gcdgb = gcddb = gcdsb = 0.0;
            gcsgb = gcsdb = gcssb = 0.0;
            gcggb = gcgdb = gcgsb = 0.0;
            gcbgb = gcbdb = gcbsb = 0.0;

            gqdef = gcqgb = gcqdb = gcqsb = gcqbb = 0.0;
            ggtg = ggtd = ggtb = ggts = 0.0;
            sxpart = (1.0 - (dxpart = (bsim3.BSIM3mode > 0) ? 0.4 : 0.6));
            ddxpart_dVd = ddxpart_dVg = ddxpart_dVb = ddxpart_dVs = 0.0;
            dsxpart_dVd = dsxpart_dVg = dsxpart_dVb = dsxpart_dVs = 0.0;

            if (bsim3.BSIM3nqsMod != 0)
                bsim3.BSIM3gtau = 16.0 * bsim3.pParam.BSIM3u0temp * model.BSIM3vtm / bsim3.pParam.BSIM3leffCV / bsim3.pParam.BSIM3leffCV * BSIM3v24.ScalingFactor;
            else
                bsim3.BSIM3gtau = 0.0;

            goto line900;

            line860:
            /* evaluate equivalent charge current */

            cqgate = state.States[0][bsim3.BSIM3states + BSIM3v24.BSIM3cqg];
            cqbulk = state.States[0][bsim3.BSIM3states + BSIM3v24.BSIM3cqb];
            cqdrn = state.States[0][bsim3.BSIM3states + BSIM3v24.BSIM3cqd];

            ceqqg = cqgate - gcggb * vgb + gcgdb * vbd + gcgsb * vbs;
            ceqqb = cqbulk - gcbgb * vgb + gcbdb * vbd + gcbsb * vbs;
            ceqqd = cqdrn - gcdgb * vgb + gcddb * vbd + gcdsb * vbs;

            if (bsim3.BSIM3nqsMod != 0)
            {
                T0 = ggtg * vgb - ggtd * vbd - ggts * vbs;
                ceqqg += T0;
                T1 = qdef * bsim3.BSIM3gtau;
                ceqqd -= dxpart * T0 + T1 * (ddxpart_dVg * vgb - ddxpart_dVd * vbd - ddxpart_dVs * vbs);
                cqdef = state.States[0][bsim3.BSIM3states + BSIM3v24.BSIM3cqcdump] - gqdef * qdef;
                cqcheq = state.States[0][bsim3.BSIM3states + BSIM3v24.BSIM3cqcheq] - (gcqgb * vgb - gcqdb * vbd - gcqsb * vbs) + T0;
            }

            if (method != null && method.SavedTime == 0.0)
            {
                state.States[1][bsim3.BSIM3states + BSIM3v24.BSIM3cqb] = state.States[0][bsim3.BSIM3states + BSIM3v24.BSIM3cqb];
                state.States[1][bsim3.BSIM3states + BSIM3v24.BSIM3cqg] = state.States[0][bsim3.BSIM3states + BSIM3v24.BSIM3cqg];
                state.States[1][bsim3.BSIM3states + BSIM3v24.BSIM3cqd] = state.States[0][bsim3.BSIM3states + BSIM3v24.BSIM3cqd];

                if (bsim3.BSIM3nqsMod != 0)
                {
                    state.States[1][bsim3.BSIM3states + BSIM3v24.BSIM3cqcheq] = state.States[0][bsim3.BSIM3states + BSIM3v24.BSIM3cqcheq];
                    state.States[1][bsim3.BSIM3states + BSIM3v24.BSIM3cqcdump] = state.States[0][bsim3.BSIM3states + BSIM3v24.BSIM3cqcdump];
                }
            }

            /* 
            * load current vector
            */
            line900:

            if (bsim3.BSIM3mode >= 0)
            {
                Gm = bsim3.BSIM3gm;
                Gmbs = bsim3.BSIM3gmbs;
                FwdSum = Gm + Gmbs;
                RevSum = 0.0;
                cdreq = model.BSIM3type * (cdrain - bsim3.BSIM3gds * vds - Gm * vgs - Gmbs * vbs);

                ceqbd = -model.BSIM3type * (bsim3.BSIM3csub - bsim3.BSIM3gbds * vds - bsim3.BSIM3gbgs * vgs - bsim3.BSIM3gbbs * vbs);
                ceqbs = 0.0;

                gbbdp = -bsim3.BSIM3gbds;
                gbbsp = (bsim3.BSIM3gbds + bsim3.BSIM3gbgs + bsim3.BSIM3gbbs);

                gbdpg = bsim3.BSIM3gbgs;
                gbdpdp = bsim3.BSIM3gbds;
                gbdpb = bsim3.BSIM3gbbs;
                gbdpsp = -(gbdpg + gbdpdp + gbdpb);

                gbspg = 0.0;
                gbspdp = 0.0;
                gbspb = 0.0;
                gbspsp = 0.0;
            }
            else
            {
                Gm = -bsim3.BSIM3gm;
                Gmbs = -bsim3.BSIM3gmbs;
                FwdSum = 0.0;
                RevSum = -(Gm + Gmbs);
                cdreq = -model.BSIM3type * (cdrain + bsim3.BSIM3gds * vds + Gm * vgd + Gmbs * vbd);

                ceqbs = -model.BSIM3type * (bsim3.BSIM3csub + bsim3.BSIM3gbds * vds - bsim3.BSIM3gbgs * vgd - bsim3.BSIM3gbbs * vbd);
                ceqbd = 0.0;

                gbbsp = -bsim3.BSIM3gbds;
                gbbdp = (bsim3.BSIM3gbds + bsim3.BSIM3gbgs + bsim3.BSIM3gbbs);

                gbdpg = 0.0;
                gbdpsp = 0.0;
                gbdpb = 0.0;
                gbdpdp = 0.0;

                gbspg = bsim3.BSIM3gbgs;
                gbspsp = bsim3.BSIM3gbds;
                gbspb = bsim3.BSIM3gbbs;
                gbspdp = -(gbspg + gbspsp + gbspb);
            }

            if (model.BSIM3type > 0)
            {
                ceqbs += (bsim3.BSIM3cbs - bsim3.BSIM3gbs * vbs);
                ceqbd += (bsim3.BSIM3cbd - bsim3.BSIM3gbd * vbd);
                /* 
                ceqqg = ceqqg;
                ceqqb = ceqqb;
                ceqqd = ceqqd;
                cqdef = cqdef;
                cqcheq = cqcheq;
                */
            }
            else
            {
                ceqbs -= (bsim3.BSIM3cbs - bsim3.BSIM3gbs * vbs);
                ceqbd -= (bsim3.BSIM3cbd - bsim3.BSIM3gbd * vbd);
                ceqqg = -ceqqg;
                ceqqb = -ceqqb;
                ceqqd = -ceqqd;
                cqdef = -cqdef;
                cqcheq = -cqcheq;
            }
            // rstate.Rhs[bsim3.BSIM3gNode] -= ceqqg;
            // rstate.Rhs[bsim3.BSIM3bNode] -= (ceqbs + ceqbd + ceqqb);
            // rstate.Rhs[bsim3.BSIM3dNodePrime] += (ceqbd - cdreq - ceqqd);
            // rstate.Rhs[bsim3.BSIM3sNodePrime] += (cdreq + ceqbs + ceqqg + ceqqb + ceqqd);
            if (bsim3.BSIM3nqsMod != 0)
                // rstate.Rhs[bsim3.BSIM3qNode] += (cqcheq - cqdef);

            /* 
            * load y matrix
            */

            T1 = qdef * bsim3.BSIM3gtau;
            // rstate.Matrix[bsim3.BSIM3dNode, bsim3.BSIM3dNode] += bsim3.BSIM3drainConductance;
            // rstate.Matrix[bsim3.BSIM3gNode, bsim3.BSIM3gNode] += gcggb - ggtg;
            // rstate.Matrix[bsim3.BSIM3sNode, bsim3.BSIM3sNode] += bsim3.BSIM3sourceConductance;
            // rstate.Matrix[bsim3.BSIM3bNode, bsim3.BSIM3bNode] += bsim3.BSIM3gbd + bsim3.BSIM3gbs - gcbgb - gcbdb - gcbsb - bsim3.BSIM3gbbs;
            // rstate.Matrix[bsim3.BSIM3dNodePrime, bsim3.BSIM3dNodePrime] += bsim3.BSIM3drainConductance + bsim3.BSIM3gds + bsim3.BSIM3gbd + RevSum + gcddb + dxpart * ggtd + T1 * ddxpart_dVd + gbdpdp;
            // rstate.Matrix[bsim3.BSIM3sNodePrime, bsim3.BSIM3sNodePrime] += bsim3.BSIM3sourceConductance + bsim3.BSIM3gds + bsim3.BSIM3gbs + FwdSum + gcssb + sxpart * ggts + T1 * dsxpart_dVs + gbspsp;
            // rstate.Matrix[bsim3.BSIM3dNode, bsim3.BSIM3dNodePrime] -= bsim3.BSIM3drainConductance;
            // rstate.Matrix[bsim3.BSIM3gNode, bsim3.BSIM3bNode] -= gcggb + gcgdb + gcgsb + ggtb;
            // rstate.Matrix[bsim3.BSIM3gNode, bsim3.BSIM3dNodePrime] += gcgdb - ggtd;
            // rstate.Matrix[bsim3.BSIM3gNode, bsim3.BSIM3sNodePrime] += gcgsb - ggts;
            // rstate.Matrix[bsim3.BSIM3sNode, bsim3.BSIM3sNodePrime] -= bsim3.BSIM3sourceConductance;
            // rstate.Matrix[bsim3.BSIM3bNode, bsim3.BSIM3gNode] += gcbgb - bsim3.BSIM3gbgs;
            // rstate.Matrix[bsim3.BSIM3bNode, bsim3.BSIM3dNodePrime] += gcbdb - bsim3.BSIM3gbd + gbbdp;
            // rstate.Matrix[bsim3.BSIM3bNode, bsim3.BSIM3sNodePrime] += gcbsb - bsim3.BSIM3gbs + gbbsp;
            // rstate.Matrix[bsim3.BSIM3dNodePrime, bsim3.BSIM3dNode] -= bsim3.BSIM3drainConductance;
            // rstate.Matrix[bsim3.BSIM3dNodePrime, bsim3.BSIM3gNode] += Gm + gcdgb + dxpart * ggtg + T1 * ddxpart_dVg + gbdpg;
            // rstate.Matrix[bsim3.BSIM3dNodePrime, bsim3.BSIM3bNode] -= bsim3.BSIM3gbd - Gmbs + gcdgb + gcddb + gcdsb - dxpart * ggtb - T1 * ddxpart_dVb - gbdpb;
            // rstate.Matrix[bsim3.BSIM3dNodePrime, bsim3.BSIM3sNodePrime] -= bsim3.BSIM3gds + FwdSum - gcdsb - dxpart * ggts - T1 * ddxpart_dVs - gbdpsp;
            // rstate.Matrix[bsim3.BSIM3sNodePrime, bsim3.BSIM3gNode] += gcsgb - Gm + sxpart * ggtg + T1 * dsxpart_dVg + gbspg;
            // rstate.Matrix[bsim3.BSIM3sNodePrime, bsim3.BSIM3sNode] -= bsim3.BSIM3sourceConductance;
            // rstate.Matrix[bsim3.BSIM3sNodePrime, bsim3.BSIM3bNode] -= bsim3.BSIM3gbs + Gmbs + gcsgb + gcsdb + gcssb - sxpart * ggtb - T1 * dsxpart_dVb - gbspb;
            // rstate.Matrix[bsim3.BSIM3sNodePrime, bsim3.BSIM3dNodePrime] -= bsim3.BSIM3gds + RevSum - gcsdb - sxpart * ggtd - T1 * dsxpart_dVd - gbspdp;

            if (bsim3.BSIM3nqsMod != 0)
            {
                // rstate.Matrix[bsim3.BSIM3qNode, bsim3.BSIM3qNode] += (gqdef + bsim3.BSIM3gtau);

                // rstate.Matrix[bsim3.BSIM3dNodePrime, bsim3.BSIM3qNode] += (dxpart * bsim3.BSIM3gtau);
                // rstate.Matrix[bsim3.BSIM3sNodePrime, bsim3.BSIM3qNode] += (sxpart * bsim3.BSIM3gtau);
                // rstate.Matrix[bsim3.BSIM3gNode, bsim3.BSIM3qNode] -= bsim3.BSIM3gtau;

                // rstate.Matrix[bsim3.BSIM3qNode, bsim3.BSIM3gNode] += (ggtg - gcqgb);
                // rstate.Matrix[bsim3.BSIM3qNode, bsim3.BSIM3dNodePrime] += (ggtd - gcqdb);
                // rstate.Matrix[bsim3.BSIM3qNode, bsim3.BSIM3sNodePrime] += (ggts - gcqsb);
                // rstate.Matrix[bsim3.BSIM3qNode, bsim3.BSIM3bNode] += (ggtb - gcqbb);
            }

            line1000:;
        }
    }
}
