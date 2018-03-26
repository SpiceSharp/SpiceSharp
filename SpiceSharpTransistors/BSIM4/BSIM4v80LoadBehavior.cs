using System;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.Transistors;
using SpiceSharp.Circuits;
using static SpiceSharp.Components.Transistors.BSIM4v80Helpers;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// General behaviour for a <see cref="BSIM4v80"/>
    /// </summary>
    public class BSIM4v80LoadBehavior : LoadBehavior
    {
        /// <summary>
        /// Execute behaviour
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Load(Circuit ckt)
        {
            var bsim4 = ComponentTyped<BSIM4v80>();
            var model = bsim4.Model as BSIM4v80Model;
            var state = ckt.State;
            var rstate = state;
            var method = ckt.Method;
            int Check, Check1, Check2;
            double vds, vgs, vbs, vges, vgms, vdbs, vsbs, vses, vdes, qdef, vgdo, vgedo, vgmdo, vbd, vdbd, vgd, vged, vgmd, delvbd,
                delvdbd, delvgd, delvged, delvgmd, delvds, delvgs, delvges, delvgms, delvbs, delvdbs, delvsbs, delvses, vdedo, delvdes,
                delvded, delvbd_jct, delvbs_jct, Idtot, cdhat, Ibtot, cbhat, Igstot, cgshat, Igdtot, cgdhat, Igbtot, cgbhat, Isestot, cseshat,
                Idedtot, cdedhat, von, vsbdo, vsbd, vgb, vgmb, vbs_jct, vbd_jct, Nvtms, SourceSatCurrent, evbs, T1, T2, T0, devbs_dvb, T3,
                Nvtmd, DrainSatCurrent, evbd, devbd_dvb, Nvtmrssws, Nvtmrsswgs, Nvtmrss, Nvtmrsswd, Nvtmrsswgd, Nvtmrsd, T9, dT1_dVb, dT0_dVb,
                dT2_dVb, dT3_dVb, dT4_dVb, dT5_dVb, dT6_dVb, Vds, Vgs, Vbs, Vdb, epsrox, toxe, epssub, Vbseff, dVbseff_dVb, Phis, dPhis_dVb,
                sqrtPhis, dsqrtPhis_dVb, Xdep, dXdep_dVb, Leff, Vtm, Vtm0, V0, T4, lt1, dlt1_dVb, ltw, dltw_dVb, Theta0, dTheta0_dVb, Delt_vth,
                dDelt_vth_dVb, T5, TempRatio, Vth_NarrowW, dDIBL_Sft_dVd, DIBL_Sft, Lpe_Vb, Vth, dVth_dVb, dVth_dVd, tmp1, tmp2, tmp3, tmp4, n,
                dn_dVb, dn_dVd, dT2_dVd, dT3_dVd, dT4_dVd, dDITS_Sft_dVd, dDITS_Sft_dVb, DITS_Sft2, dDITS_Sft2_dVd, Vgs_eff, dVgs_eff_dVg,
                Vgst, T10, dT10_dVg, dT10_dVd, dT10_dVb, ExpVgst, dT9_dVg, dT9_dVd, dT9_dVb, Vgsteff, T11, dVgsteff_dVg, dVgsteff_dVd,
                dVgsteff_dVb, Weff, dWeff_dVg, dWeff_dVb, Rds, dT0_dVg, dRds_dVg, dRds_dVb, T6 = 0, T7, Abulk0, dAbulk0_dVb, T8, dAbulk_dVg,
                Abulk, dAbulk_dVb, T14, T12, dDenomi_dVg, T13, dDenomi_dVd, dDenomi_dVb, dT1_dVg, VgsteffVth, dT11_dVg, Denomi, ueff,
                dueff_dVg, dueff_dVd, dueff_dVb, WVCox, WVCoxRds, Esat, EsatL, dEsatL_dVg, dEsatL_dVd, dEsatL_dVb, a1, dLambda_dVg, Lambda,
                Vgst2Vtm, Vdsat, dT0_dVd, dVdsat_dVg, dVdsat_dVd, dVdsat_dVb, dT1_dVd, dT2_dVg, dT3_dVg, Vdseff, dVdseff_dVg, dVdseff_dVd,
                dVdseff_dVb, diffVds, dT5_dVg, dT5_dVd, dT6_dVg, dT6_dVd, dT8_dVg, dT8_dVd, dT8_dVb, Vasat, dVasat_dVg, dVasat_dVb, dVasat_dVd,
                Tcen, dTcen_dVg, Coxeff, dCoxeff_dVg, CoxeffWovL, beta, dbeta_dVg, dbeta_dVd, dbeta_dVb, fgche1, dfgche1_dVg, dfgche1_dVd,
                dfgche1_dVb, fgche2, dfgche2_dVg, dfgche2_dVd, dfgche2_dVb, gche, dgche_dVg, dgche_dVd, dgche_dVb, Idl, dIdl_dVg, dIdl_dVd,
                dIdl_dVb, FP, dFP_dVg, PvagTerm, dPvagTerm_dVg, dPvagTerm_dVb, dPvagTerm_dVd, Cclm, dCclm_dVg, dCclm_dVb, dCclm_dVd, VACLM,
                dVACLM_dVg, dVACLM_dVb, dVACLM_dVd, VADIBL, dVADIBL_dVg, dVADIBL_dVb, dVADIBL_dVd, Va, dVa_dVg, dVa_dVb, dVa_dVd, VADITS,
                dVADITS_dVg, dVADITS_dVd, VASCBE, dVASCBE_dVg, dVASCBE_dVd, dVASCBE_dVb, Idsa, dIdsa_dVg, dIdsa_dVd, dIdsa_dVb, tmp, Isub, Gbg,
                Gbd, Gbb, Ids, Gm, Gds, Gmb, cdrain, vs, dvs_dVg, dvs_dVd, dvs_dVb, Fsevl, dFsevl_dVg, dFsevl_dVd, dFsevl_dVb, vgs_eff,
                dvgs_eff_dvg, dT0_dvg, dT1_dvb, dT3_dvg, dT3_dvb, Rs, dRs_dvg, dRs_dvb, dgstot_dvd, dgstot_dvg, dgstot_dvb, dgstot_dvs,
                vgd_eff, dvgd_eff_dvg, Rd, dRd_dvg, dRd_dvb, dgdtot_dvs, dgdtot_dvg, dgdtot_dvb, dgdtot_dvd, Igidl, Ggidld, Ggidlg, Ggidlb,
                Igisl, Ggisls, Ggislg, Ggislb, Vfb = 0, V3, Vfbeff, dVfbeff_dVg, dVfbeff_dVb, Voxacc = 0, dVoxacc_dVg = 0, dVoxacc_dVb = 0, Voxdepinv = 0,
                dVoxdepinv_dVg = 0, dVoxdepinv_dVd = 0, dVoxdepinv_dVb = 0, VxNVt = 0, Vaux = 0, dVaux_dVg = 0, dVaux_dVd = 0, dVaux_dVb = 0, ExpVxNVt, Igc, dIgc_dVg,
                dIgc_dVd, dIgc_dVb, Pigcd, dPigcd_dVg, dPigcd_dVd, dPigcd_dVb, dT7_dVg, dT7_dVd, dT7_dVb, Igcs, dIgcs_dVg, dIgcs_dVd,
                dIgcs_dVb, Igcd, dIgcd_dVg, dIgcd_dVd, dIgcd_dVb, Igs, dIgs_dVg, dIgs_dVs, Igd, dIgd_dVg, dIgd_dVd, Igbacc, dIgbacc_dVg,
                dIgbacc_dVb, Igbinv, dIgbinv_dVg, dIgbinv_dVd, dIgbinv_dVb, qgate = 0, VbseffCV, dVbseffCV_dVb, dVgst_dVb, dVgst_dVg, CoxWL, Arg1,
                qbulk = 0, qdrn = 0, One_Third_CoxWL, Two_Third_CoxWL, AbulkCV, dAbulkCV_dVb, Alphaz, dAlphaz_dVg, dAlphaz_dVb, noff,
                dnoff_dVd, dnoff_dVb, voffcv, VgstNVt, Qac0, dQac0_dVg, dQac0_dVb, Qsub0, dQsub0_dVg, dQsub0_dVd, dQsub0_dVb,
                VdsatCV, VdseffCV, dVdseffCV_dVg, dVdseffCV_dVd, dVdseffCV_dVb, Cgg1, Cgd1, Cgb1, Cbg1, Cbd1, Cbb1, qsrc, Csg, Csd, Csb, Cgg,
                Cgd, Cgb, Cbg, Cbd, Cbb, Cox, Tox, dTcen_dVb, LINK, V4, Ccen, dCoxeff_dVb, CoxWLcen, QovCox, DeltaPhi, dDeltaPhi_dVg, VgDP,
                dVgDP_dVg, dTcen_dVd, dCoxeff_dVd, qcheq, czbd, czbs, czbdsw, czbdswg, czbssw, czbsswg, MJS, MJSWS, MJSWGS, MJD, MJSWD, MJSWGD,
                arg, sarg, vgdx, vgsx, cgdo, qgdo, cgso, qgso, ag0, gcgmgmb = 0, gcgmdb = 0, gcgmsb = 0, gcgmbb = 0, gcdgmb, gcsgmb, gcbgmb, gcggb,
                gcgdb, gcgsb, gcgbb, gcdgb, gcsgb, gcbgb, qgmb, qgmid = 0, qgb, gcddb, gcdsb, gcsdb, gcssb, gcdbb, gcsbb, gcbdb, gcbsb, gcdbdb,
                gcsbsb, gcbbb, ggtg, sxpart, dxpart, ddxpart_dVd, dsxpart_dVd, ggtd, ggts, ggtb, gqdef = 0, gcqgb = 0, gcqdb = 0, gcqsb = 0, gcqbb = 0, Cdd, Cdg,
                ddxpart_dVg, Cds, Css, ddxpart_dVs, ddxpart_dVb, dsxpart_dVg, dsxpart_dVs, dsxpart_dVb, ceqqg, ceqqjd = 0, cqcheq = 0, cqgate, cqbody,
                cqdrn, ceqqd, ceqqb, ceqqgmid, ceqqjs = 0, cqdef = 0, Gmbs, FwdSum, RevSum, ceqdrn, ceqbd, ceqbs, gbbdp, gbbsp, gbdpg, gbdpdp, gbdpb,
                gbdpsp, gbspg, gbspdp, gbspb, gbspsp, gIstotg, gIstotd, gIstots, gIstotb, Istoteq, gIdtotg, gIdtotd, gIdtots, gIdtotb, Idtoteq,
                gIbtotg, gIbtotd, gIbtots, gIbtotb, Ibtoteq, gIgtotg, gIgtotd, gIgtots, gIgtotb, Igtoteq, gcrgd, gcrgg, gcrgs, gcrgb, ceqgcrg,
                gcrg, ceqgstot, gstot, gstotd, gstotg, gstots, gstotb, ceqgdtot, gdtot, gdtotd, gdtotg, gdtots, gdtotb, ceqjs, ceqjd, gjbd,
                gjbs, gdpr, gspr, geltd, ggidld = 0, ggidlg = 0, ggidlb = 0, ggislg = 0, ggisls = 0, ggislb = 0;
            bool ChargeComputationNeeded = (method != null || state.UseSmallSignal) || (state.Domain == State.DomainTypes.Time && state.UseDC && state.UseIC);

            Check = Check1 = Check2 = 1;

            if (state.UseSmallSignal)
            {
                vds = state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4vds];
                vgs = state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4vgs];
                vbs = state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4vbs];
                vges = state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4vges];
                vgms = state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4vgms];
                vdbs = state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4vdbs];
                vsbs = state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4vsbs];
                vses = state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4vses];
                vdes = state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4vdes];

                qdef = state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4qdef];
            }
            else if (state.Init == State.InitFlags.InitTransient)
            {
                vds = state.States[1][bsim4.BSIM4states + BSIM4v80.BSIM4vds];
                vgs = state.States[1][bsim4.BSIM4states + BSIM4v80.BSIM4vgs];
                vbs = state.States[1][bsim4.BSIM4states + BSIM4v80.BSIM4vbs];
                vges = state.States[1][bsim4.BSIM4states + BSIM4v80.BSIM4vges];
                vgms = state.States[1][bsim4.BSIM4states + BSIM4v80.BSIM4vgms];
                vdbs = state.States[1][bsim4.BSIM4states + BSIM4v80.BSIM4vdbs];
                vsbs = state.States[1][bsim4.BSIM4states + BSIM4v80.BSIM4vsbs];
                vses = state.States[1][bsim4.BSIM4states + BSIM4v80.BSIM4vses];
                vdes = state.States[1][bsim4.BSIM4states + BSIM4v80.BSIM4vdes];

                qdef = state.States[1][bsim4.BSIM4states + BSIM4v80.BSIM4qdef];
            }
            else if ((state.Init == State.InitFlags.InitJct) && !bsim4.BSIM4off)
            {
                vds = model.BSIM4type * bsim4.BSIM4icVDS;
                vgs = vges = vgms = model.BSIM4type * bsim4.BSIM4icVGS;
                vbs = vdbs = vsbs = model.BSIM4type * bsim4.BSIM4icVBS;
                if (vds > 0.0)
                {
                    vdes = vds + 0.01;
                    vses = -0.01;
                }
                else if (vds < 0.0)
                {
                    vdes = vds - 0.01;
                    vses = 0.01;
                }
                else
                    vdes = vses = 0.0;

                qdef = 0.0;

                if ((vds == 0.0) && (vgs == 0.0) && (vbs == 0.0) && ((method != null || state.UseDC ||
                    state.Domain == State.DomainTypes.None) || (!state.UseIC)))
                {
                    vds = 0.1;
                    vdes = 0.11;
                    vses = -0.01;
                    vgs = vges = vgms = model.BSIM4type * bsim4.BSIM4vth0 + 0.1;
                    vbs = vdbs = vsbs = 0.0;
                }
            }
            else if ((state.Init == State.InitFlags.InitJct || state.Init == State.InitFlags.InitFix) && (bsim4.BSIM4off))
            {
                vds = vgs = vbs = vges = vgms = 0.0;
                vdbs = vsbs = vdes = vses = qdef = 0.0;
            }
            else
            {
                /* PREDICTOR */
                vds = model.BSIM4type * rstate.OldSolution[bsim4.BSIM4dNodePrime] - rstate.OldSolution[bsim4.BSIM4sNodePrime];
                vgs = model.BSIM4type * rstate.OldSolution[bsim4.BSIM4gNodePrime] - rstate.OldSolution[bsim4.BSIM4sNodePrime];
                vbs = model.BSIM4type * rstate.OldSolution[bsim4.BSIM4bNodePrime] - rstate.OldSolution[bsim4.BSIM4sNodePrime];
                vges = model.BSIM4type * rstate.OldSolution[bsim4.BSIM4gNodeExt] - rstate.OldSolution[bsim4.BSIM4sNodePrime];
                vgms = model.BSIM4type * rstate.OldSolution[bsim4.BSIM4gNodeMid] - rstate.OldSolution[bsim4.BSIM4sNodePrime];
                vdbs = model.BSIM4type * rstate.OldSolution[bsim4.BSIM4dbNode] - rstate.OldSolution[bsim4.BSIM4sNodePrime];
                vsbs = model.BSIM4type * rstate.OldSolution[bsim4.BSIM4sbNode] - rstate.OldSolution[bsim4.BSIM4sNodePrime];
                vses = model.BSIM4type * rstate.OldSolution[bsim4.BSIM4sNode] - rstate.OldSolution[bsim4.BSIM4sNodePrime];
                vdes = model.BSIM4type * rstate.OldSolution[bsim4.BSIM4dNode] - rstate.OldSolution[bsim4.BSIM4sNodePrime];
                qdef = model.BSIM4type * rstate.OldSolution[bsim4.BSIM4qNode];
                /* PREDICTOR */

                vgdo = state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4vgs] - state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4vds];
                vgedo = state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4vges] - state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4vds];
                vgmdo = state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4vgms] - state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4vds];

                vbd = vbs - vds;
                vdbd = vdbs - vds;
                vgd = vgs - vds;
                vged = vges - vds;
                vgmd = vgms - vds;

                delvbd = vbd - state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4vbd];
                delvdbd = vdbd - state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4vdbd];
                delvgd = vgd - vgdo;
                delvged = vged - vgedo;
                delvgmd = vgmd - vgmdo;

                delvds = vds - state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4vds];
                delvgs = vgs - state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4vgs];
                delvges = vges - state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4vges];
                delvgms = vgms - state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4vgms];
                delvbs = vbs - state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4vbs];
                delvdbs = vdbs - state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4vdbs];
                delvsbs = vsbs - state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4vsbs];

                delvses = vses - (state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4vses]);
                vdedo = state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4vdes] - state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4vds];
                delvdes = vdes - state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4vdes];
                delvded = vdes - vds - vdedo;

                delvbd_jct = (bsim4.BSIM4rbodyMod == 0) ? delvbd : delvdbd;
                delvbs_jct = (bsim4.BSIM4rbodyMod == 0) ? delvbs : delvsbs;
                if (bsim4.BSIM4mode >= 0)
                {
                    Idtot = bsim4.BSIM4cd + bsim4.BSIM4csub - bsim4.BSIM4cbd + bsim4.BSIM4Igidl;
                    cdhat = Idtot - bsim4.BSIM4gbd * delvbd_jct + (bsim4.BSIM4gmbs + bsim4.BSIM4gbbs + bsim4.BSIM4ggidlb) * delvbs + (bsim4.BSIM4gm + bsim4.BSIM4gbgs + bsim4.BSIM4ggidlg) *
                        delvgs + (bsim4.BSIM4gds + bsim4.BSIM4gbds + bsim4.BSIM4ggidld) * delvds;
                    Ibtot = bsim4.BSIM4cbs + bsim4.BSIM4cbd - bsim4.BSIM4Igidl - bsim4.BSIM4Igisl - bsim4.BSIM4csub;
                    cbhat = Ibtot + bsim4.BSIM4gbd * delvbd_jct + bsim4.BSIM4gbs * delvbs_jct - (bsim4.BSIM4gbbs + bsim4.BSIM4ggidlb) * delvbs - (bsim4.BSIM4gbgs + bsim4.BSIM4ggidlg) *
                        delvgs - (bsim4.BSIM4gbds + bsim4.BSIM4ggidld - bsim4.BSIM4ggisls) * delvds - bsim4.BSIM4ggislg * delvgd - bsim4.BSIM4ggislb * delvbd;

                    Igstot = bsim4.BSIM4Igs + bsim4.BSIM4Igcs;
                    cgshat = Igstot + (bsim4.BSIM4gIgsg + bsim4.BSIM4gIgcsg) * delvgs + bsim4.BSIM4gIgcsd * delvds + bsim4.BSIM4gIgcsb * delvbs;

                    Igdtot = bsim4.BSIM4Igd + bsim4.BSIM4Igcd;
                    cgdhat = Igdtot + bsim4.BSIM4gIgdg * delvgd + bsim4.BSIM4gIgcdg * delvgs + bsim4.BSIM4gIgcdd * delvds + bsim4.BSIM4gIgcdb * delvbs;

                    Igbtot = bsim4.BSIM4Igb;
                    cgbhat = bsim4.BSIM4Igb + bsim4.BSIM4gIgbg * delvgs + bsim4.BSIM4gIgbd * delvds + bsim4.BSIM4gIgbb * delvbs;
                }
                else
                {
                    Idtot = bsim4.BSIM4cd + bsim4.BSIM4cbd - bsim4.BSIM4Igidl; /* bugfix */
                    cdhat = Idtot + bsim4.BSIM4gbd * delvbd_jct + bsim4.BSIM4gmbs * delvbd + bsim4.BSIM4gm * delvgd - (bsim4.BSIM4gds + bsim4.BSIM4ggidls) * delvds -
                        bsim4.BSIM4ggidlg * delvgs - bsim4.BSIM4ggidlb * delvbs;
                    Ibtot = bsim4.BSIM4cbs + bsim4.BSIM4cbd - bsim4.BSIM4Igidl - bsim4.BSIM4Igisl - bsim4.BSIM4csub;
                    cbhat = Ibtot + bsim4.BSIM4gbs * delvbs_jct + bsim4.BSIM4gbd * delvbd_jct - (bsim4.BSIM4gbbs + bsim4.BSIM4ggislb) * delvbd - (bsim4.BSIM4gbgs + bsim4.BSIM4ggislg) *
                        delvgd + (bsim4.BSIM4gbds + bsim4.BSIM4ggisld - bsim4.BSIM4ggidls) * delvds - bsim4.BSIM4ggidlg * delvgs - bsim4.BSIM4ggidlb * delvbs;

                    Igstot = bsim4.BSIM4Igs + bsim4.BSIM4Igcd;
                    cgshat = Igstot + bsim4.BSIM4gIgsg * delvgs + bsim4.BSIM4gIgcdg * delvgd - bsim4.BSIM4gIgcdd * delvds + bsim4.BSIM4gIgcdb * delvbd;

                    Igdtot = bsim4.BSIM4Igd + bsim4.BSIM4Igcs;
                    cgdhat = Igdtot + (bsim4.BSIM4gIgdg + bsim4.BSIM4gIgcsg) * delvgd - bsim4.BSIM4gIgcsd * delvds + bsim4.BSIM4gIgcsb * delvbd;

                    Igbtot = bsim4.BSIM4Igb;
                    cgbhat = bsim4.BSIM4Igb + bsim4.BSIM4gIgbg * delvgd - bsim4.BSIM4gIgbd * delvds + bsim4.BSIM4gIgbb * delvbd;
                }

                Isestot = bsim4.BSIM4gstot * (state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4vses]);
                cseshat = Isestot + bsim4.BSIM4gstot * delvses + bsim4.BSIM4gstotd * delvds + bsim4.BSIM4gstotg * delvgs + bsim4.BSIM4gstotb * delvbs;

                Idedtot = bsim4.BSIM4gdtot * vdedo;
                cdedhat = Idedtot + bsim4.BSIM4gdtot * delvded + bsim4.BSIM4gdtotd * delvds + bsim4.BSIM4gdtotg * delvgs + bsim4.BSIM4gdtotb * delvbs;

                /* NOBYPASS */

                von = bsim4.BSIM4von;
                if (state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4vds] >= 0.0)
                {
                    vgs = Transistor.DEVfetlim(vgs, state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4vgs], von);
                    vds = vgs - vgd;
                    vds = Transistor.DEVlimvds(vds, state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4vds]);
                    vgd = vgs - vds;
                    if (bsim4.BSIM4rgateMod.Value == 3)
                    {
                        vges = Transistor.DEVfetlim(vges, state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4vges], von);
                        vgms = Transistor.DEVfetlim(vgms, state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4vgms], von);
                        vged = vges - vds;
                        vgmd = vgms - vds;
                    }
                    else if ((bsim4.BSIM4rgateMod.Value == 1) || (bsim4.BSIM4rgateMod.Value == 2))
                    {
                        vges = Transistor.DEVfetlim(vges, state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4vges], von);
                        vged = vges - vds;
                    }

                    if (model.BSIM4rdsMod != 0)
                    {
                        vdes = Transistor.DEVlimvds(vdes, state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4vdes]);
                        vses = -Transistor.DEVlimvds(-vses, -(state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4vses]));
                    }

                }
                else
                {
                    vgd = Transistor.DEVfetlim(vgd, vgdo, von);
                    vds = vgs - vgd;
                    vds = -Transistor.DEVlimvds(-vds, -(state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4vds]));
                    vgs = vgd + vds;

                    if (bsim4.BSIM4rgateMod.Value == 3)
                    {
                        vged = Transistor.DEVfetlim(vged, vgedo, von);
                        vges = vged + vds;
                        vgmd = Transistor.DEVfetlim(vgmd, vgmdo, von);
                        vgms = vgmd + vds;
                    }
                    if ((bsim4.BSIM4rgateMod.Value == 1) || (bsim4.BSIM4rgateMod.Value == 2))
                    {
                        vged = Transistor.DEVfetlim(vged, vgedo, von);
                        vges = vged + vds;
                    }

                    if (model.BSIM4rdsMod != 0)
                    {
                        vdes = -Transistor.DEVlimvds(-vdes, -(state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4vdes]));
                        vses = Transistor.DEVlimvds(vses, state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4vses]);
                    }
                }

                if (vds >= 0.0)
                {
                    vbs = Transistor.DEVpnjlim(vbs, state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4vbs], Circuit.CONSTvt0, model.BSIM4vcrit, ref Check);
                    vbd = vbs - vds;
                    if (bsim4.BSIM4rbodyMod != 0)
                    {
                        vdbs = Transistor.DEVpnjlim(vdbs, state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4vdbs], Circuit.CONSTvt0, model.BSIM4vcrit, ref Check1);
                        vdbd = vdbs - vds;
                        vsbs = Transistor.DEVpnjlim(vsbs, state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4vsbs], Circuit.CONSTvt0, model.BSIM4vcrit, ref Check2);
                        if ((Check1 == 0) && (Check2 == 0))
                            Check = 0;
                        else
                            Check = 1;
                    }
                }
                else
                {
                    vbd = Transistor.DEVpnjlim(vbd, state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4vbd], Circuit.CONSTvt0, model.BSIM4vcrit, ref Check);
                    vbs = vbd + vds;
                    if (bsim4.BSIM4rbodyMod != 0)
                    {
                        vdbd = Transistor.DEVpnjlim(vdbd, state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4vdbd], Circuit.CONSTvt0, model.BSIM4vcrit, ref Check1);
                        vdbs = vdbd + vds;
                        vsbdo = state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4vsbs] - state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4vds];
                        vsbd = vsbs - vds;
                        vsbd = Transistor.DEVpnjlim(vsbd, vsbdo, Circuit.CONSTvt0, model.BSIM4vcrit, ref Check2);
                        vsbs = vsbd + vds;
                        if ((Check1 == 0) && (Check2 == 0))
                            Check = 0;
                        else
                            Check = 1;
                    }
                }
            }

            /* Calculate DC currents and their derivatives */
            vbd = vbs - vds;
            vgd = vgs - vds;
            vgb = vgs - vbs;
            vged = vges - vds;
            vgmd = vgms - vds;
            vgmb = vgms - vbs;
            vdbd = vdbs - vds;

            vbs_jct = (bsim4.BSIM4rbodyMod == 0) ? vbs : vsbs;
            vbd_jct = (bsim4.BSIM4rbodyMod == 0) ? vbd : vdbd;

            /* Source / drain junction diode DC model begins */
            Nvtms = model.BSIM4vtm * model.BSIM4SjctEmissionCoeff;
            /* if ((bsim4.BSIM4Aseff <= 0.0) && (bsim4.BSIM4Pseff <= 0.0))
            {
                SourceSatCurrent = 1.0e-14;
            } v4.7 */
            if ((bsim4.BSIM4Aseff <= 0.0) && (bsim4.BSIM4Pseff <= 0.0))
            {
                SourceSatCurrent = 0.0;
            }
            else
            {
                SourceSatCurrent = bsim4.BSIM4Aseff * model.BSIM4SjctTempSatCurDensity + bsim4.BSIM4Pseff * model.BSIM4SjctSidewallTempSatCurDensity +
                    bsim4.pParam.BSIM4weffCJ * bsim4.BSIM4nf * model.BSIM4SjctGateSidewallTempSatCurDensity;
            }

            if (SourceSatCurrent <= 0.0)
            {
                bsim4.BSIM4gbs = state.Gmin;
                bsim4.BSIM4cbs = bsim4.BSIM4gbs * vbs_jct;
            }
            else
            {
                switch (model.BSIM4dioMod.Value)
                {
                    case 0:
                        evbs = Math.Exp(vbs_jct / Nvtms);
                        T1 = model.BSIM4xjbvs * Math.Exp(-(model.BSIM4bvs + vbs_jct) / Nvtms);
                        /* WDLiu: Magic T1 in this form; different from bsim4.BSIM4v80 beta. */
                        bsim4.BSIM4gbs = SourceSatCurrent * (evbs + T1) / Nvtms + state.Gmin;
                        bsim4.BSIM4cbs = SourceSatCurrent * (evbs + bsim4.BSIM4XExpBVS - T1 - 1.0) + state.Gmin * vbs_jct;
                        break;
                    case 1:
                        T2 = vbs_jct / Nvtms;
                        if (T2 < -Transistor.EXP_THRESHOLD)
                        {
                            bsim4.BSIM4gbs = state.Gmin;
                            bsim4.BSIM4cbs = SourceSatCurrent * (Transistor.MIN_EXP - 1.0) + state.Gmin * vbs_jct;
                        }
                        else if (vbs_jct <= bsim4.BSIM4vjsmFwd)
                        {
                            evbs = Math.Exp(T2);
                            bsim4.BSIM4gbs = SourceSatCurrent * evbs / Nvtms + state.Gmin;
                            bsim4.BSIM4cbs = SourceSatCurrent * (evbs - 1.0) + state.Gmin * vbs_jct;
                        }
                        else
                        {
                            T0 = bsim4.BSIM4IVjsmFwd / Nvtms;
                            bsim4.BSIM4gbs = T0 + state.Gmin;
                            bsim4.BSIM4cbs = bsim4.BSIM4IVjsmFwd - SourceSatCurrent + T0 * (vbs_jct - bsim4.BSIM4vjsmFwd) + state.Gmin * vbs_jct;
                        }
                        break;
                    case 2:
                        if (vbs_jct < bsim4.BSIM4vjsmRev)
                        {
                            T0 = vbs_jct / Nvtms;
                            if (T0 < -Transistor.EXP_THRESHOLD)
                            {
                                evbs = Transistor.MIN_EXP;
                                devbs_dvb = 0.0;
                            }
                            else
                            {
                                evbs = Math.Exp(T0);
                                devbs_dvb = evbs / Nvtms;
                            }

                            T1 = evbs - 1.0;
                            T2 = bsim4.BSIM4IVjsmRev + bsim4.BSIM4SslpRev * (vbs_jct - bsim4.BSIM4vjsmRev);
                            bsim4.BSIM4gbs = devbs_dvb * T2 + T1 * bsim4.BSIM4SslpRev + state.Gmin;
                            bsim4.BSIM4cbs = T1 * T2 + state.Gmin * vbs_jct;
                        }
                        else if (vbs_jct <= bsim4.BSIM4vjsmFwd)
                        {
                            T0 = vbs_jct / Nvtms;
                            if (T0 < -Transistor.EXP_THRESHOLD)
                            {
                                evbs = Transistor.MIN_EXP;
                                devbs_dvb = 0.0;
                            }
                            else
                            {
                                evbs = Math.Exp(T0);
                                devbs_dvb = evbs / Nvtms;
                            }

                            T1 = (model.BSIM4bvs + vbs_jct) / Nvtms;
                            if (T1 > Transistor.EXP_THRESHOLD)
                            {
                                T2 = Transistor.MIN_EXP;
                                T3 = 0.0;
                            }
                            else
                            {
                                T2 = Math.Exp(-T1);
                                T3 = -T2 / Nvtms;
                            }
                            bsim4.BSIM4gbs = SourceSatCurrent * (devbs_dvb - model.BSIM4xjbvs * T3) + state.Gmin;
                            bsim4.BSIM4cbs = SourceSatCurrent * (evbs + bsim4.BSIM4XExpBVS - 1.0 - model.BSIM4xjbvs * T2) + state.Gmin * vbs_jct;
                        }
                        else
                        {
                            bsim4.BSIM4gbs = bsim4.BSIM4SslpFwd + state.Gmin;
                            bsim4.BSIM4cbs = bsim4.BSIM4IVjsmFwd + bsim4.BSIM4SslpFwd * (vbs_jct - bsim4.BSIM4vjsmFwd) + state.Gmin * vbs_jct;
                        }
                        break;
                    default: break;
                }
            }

            Nvtmd = model.BSIM4vtm * model.BSIM4DjctEmissionCoeff;
            /* if ((bsim4.BSIM4Adeff <= 0.0) && (bsim4.BSIM4Pdeff <= 0.0))
            {
                DrainSatCurrent = 1.0e-14;
            } v4.7 */
            if ((bsim4.BSIM4Adeff <= 0.0) && (bsim4.BSIM4Pdeff <= 0.0))
            {
                DrainSatCurrent = 0.0;
            }
            else
            {
                DrainSatCurrent = bsim4.BSIM4Adeff * model.BSIM4DjctTempSatCurDensity + bsim4.BSIM4Pdeff * model.BSIM4DjctSidewallTempSatCurDensity +
                    bsim4.pParam.BSIM4weffCJ * bsim4.BSIM4nf * model.BSIM4DjctGateSidewallTempSatCurDensity;
            }

            if (DrainSatCurrent <= 0.0)
            {
                bsim4.BSIM4gbd = state.Gmin;
                bsim4.BSIM4cbd = bsim4.BSIM4gbd * vbd_jct;
            }
            else
            {
                switch (model.BSIM4dioMod.Value)
                {
                    case 0:
                        evbd = Math.Exp(vbd_jct / Nvtmd);
                        T1 = model.BSIM4xjbvd * Math.Exp(-(model.BSIM4bvd + vbd_jct) / Nvtmd);
                        /* WDLiu: Magic T1 in this form; different from bsim4.BSIM4v80 beta. */
                        bsim4.BSIM4gbd = DrainSatCurrent * (evbd + T1) / Nvtmd + state.Gmin;
                        bsim4.BSIM4cbd = DrainSatCurrent * (evbd + bsim4.BSIM4XExpBVD - T1 - 1.0) + state.Gmin * vbd_jct;
                        break;
                    case 1:
                        T2 = vbd_jct / Nvtmd;
                        if (T2 < -Transistor.EXP_THRESHOLD)
                        {
                            bsim4.BSIM4gbd = state.Gmin;
                            bsim4.BSIM4cbd = DrainSatCurrent * (Transistor.MIN_EXP - 1.0) + state.Gmin * vbd_jct;
                        }
                        else if (vbd_jct <= bsim4.BSIM4vjdmFwd)
                        {
                            evbd = Math.Exp(T2);
                            bsim4.BSIM4gbd = DrainSatCurrent * evbd / Nvtmd + state.Gmin;
                            bsim4.BSIM4cbd = DrainSatCurrent * (evbd - 1.0) + state.Gmin * vbd_jct;
                        }
                        else
                        {
                            T0 = bsim4.BSIM4IVjdmFwd / Nvtmd;
                            bsim4.BSIM4gbd = T0 + state.Gmin;
                            bsim4.BSIM4cbd = bsim4.BSIM4IVjdmFwd - DrainSatCurrent + T0 * (vbd_jct - bsim4.BSIM4vjdmFwd) + state.Gmin * vbd_jct;
                        }
                        break;
                    case 2:
                        if (vbd_jct < bsim4.BSIM4vjdmRev)
                        {
                            T0 = vbd_jct / Nvtmd;
                            if (T0 < -Transistor.EXP_THRESHOLD)
                            {
                                evbd = Transistor.MIN_EXP;
                                devbd_dvb = 0.0;
                            }
                            else
                            {
                                evbd = Math.Exp(T0);
                                devbd_dvb = evbd / Nvtmd;
                            }

                            T1 = evbd - 1.0;
                            T2 = bsim4.BSIM4IVjdmRev + bsim4.BSIM4DslpRev * (vbd_jct - bsim4.BSIM4vjdmRev);
                            bsim4.BSIM4gbd = devbd_dvb * T2 + T1 * bsim4.BSIM4DslpRev + state.Gmin;
                            bsim4.BSIM4cbd = T1 * T2 + state.Gmin * vbd_jct;
                        }
                        else if (vbd_jct <= bsim4.BSIM4vjdmFwd)
                        {
                            T0 = vbd_jct / Nvtmd;
                            if (T0 < -Transistor.EXP_THRESHOLD)
                            {
                                evbd = Transistor.MIN_EXP;
                                devbd_dvb = 0.0;
                            }
                            else
                            {
                                evbd = Math.Exp(T0);
                                devbd_dvb = evbd / Nvtmd;
                            }

                            T1 = (model.BSIM4bvd + vbd_jct) / Nvtmd;
                            if (T1 > Transistor.EXP_THRESHOLD)
                            {
                                T2 = Transistor.MIN_EXP;
                                T3 = 0.0;
                            }
                            else
                            {
                                T2 = Math.Exp(-T1);
                                T3 = -T2 / Nvtmd;
                            }
                            bsim4.BSIM4gbd = DrainSatCurrent * (devbd_dvb - model.BSIM4xjbvd * T3) + state.Gmin;
                            bsim4.BSIM4cbd = DrainSatCurrent * (evbd + bsim4.BSIM4XExpBVD - 1.0 - model.BSIM4xjbvd * T2) + state.Gmin * vbd_jct;
                        }
                        else
                        {
                            bsim4.BSIM4gbd = bsim4.BSIM4DslpFwd + state.Gmin;
                            bsim4.BSIM4cbd = bsim4.BSIM4IVjdmFwd + bsim4.BSIM4DslpFwd * (vbd_jct - bsim4.BSIM4vjdmFwd) + state.Gmin * vbd_jct;
                        }
                        break;
                    default: break;
                }
            }

            /* trap - assisted tunneling and recombination current for reverse bias */
            Nvtmrssws = model.BSIM4vtm0 * model.BSIM4njtsswstemp;
            Nvtmrsswgs = model.BSIM4vtm0 * model.BSIM4njtsswgstemp;
            Nvtmrss = model.BSIM4vtm0 * model.BSIM4njtsstemp;
            Nvtmrsswd = model.BSIM4vtm0 * model.BSIM4njtsswdtemp;
            Nvtmrsswgd = model.BSIM4vtm0 * model.BSIM4njtsswgdtemp;
            Nvtmrsd = model.BSIM4vtm0 * model.BSIM4njtsdtemp;

            if ((model.BSIM4vtss - vbs_jct) < (model.BSIM4vtss * 1e-3))
            {
                T9 = 1.0e3;
                T0 = -vbs_jct / Nvtmrss * T9;
                Dexp(T0, out T1, out T10);
                dT1_dVb = T10 / Nvtmrss * T9;
            }
            else
            {
                T9 = 1.0 / (model.BSIM4vtss - vbs_jct);
                T0 = -vbs_jct / Nvtmrss * model.BSIM4vtss * T9;
                dT0_dVb = model.BSIM4vtss / Nvtmrss * (T9 + vbs_jct * T9 * T9);
                Dexp(T0, out T1, out T10);
                dT1_dVb = T10 * dT0_dVb;
            }

            if ((model.BSIM4vtsd - vbd_jct) < (model.BSIM4vtsd * 1e-3))
            {
                T9 = 1.0e3;
                T0 = -vbd_jct / Nvtmrsd * T9;
                Dexp(T0, out T2, out T10);
                dT2_dVb = T10 / Nvtmrsd * T9;
            }
            else
            {
                T9 = 1.0 / (model.BSIM4vtsd - vbd_jct);
                T0 = -vbd_jct / Nvtmrsd * model.BSIM4vtsd * T9;
                dT0_dVb = model.BSIM4vtsd / Nvtmrsd * (T9 + vbd_jct * T9 * T9);
                Dexp(T0, out T2, out T10);
                dT2_dVb = T10 * dT0_dVb;
            }

            if ((model.BSIM4vtssws - vbs_jct) < (model.BSIM4vtssws * 1e-3))
            {
                T9 = 1.0e3;
                T0 = -vbs_jct / Nvtmrssws * T9;
                Dexp(T0, out T3, out T10);
                dT3_dVb = T10 / Nvtmrssws * T9;
            }
            else
            {
                T9 = 1.0 / (model.BSIM4vtssws - vbs_jct);
                T0 = -vbs_jct / Nvtmrssws * model.BSIM4vtssws * T9;
                dT0_dVb = model.BSIM4vtssws / Nvtmrssws * (T9 + vbs_jct * T9 * T9);
                Dexp(T0, out T3, out T10);
                dT3_dVb = T10 * dT0_dVb;
            }

            if ((model.BSIM4vtsswd - vbd_jct) < (model.BSIM4vtsswd * 1e-3))
            {
                T9 = 1.0e3;
                T0 = -vbd_jct / Nvtmrsswd * T9;
                Dexp(T0, out T4, out T10);
                dT4_dVb = T10 / Nvtmrsswd * T9;
            }
            else
            {
                T9 = 1.0 / (model.BSIM4vtsswd - vbd_jct);
                T0 = -vbd_jct / Nvtmrsswd * model.BSIM4vtsswd * T9;
                dT0_dVb = model.BSIM4vtsswd / Nvtmrsswd * (T9 + vbd_jct * T9 * T9);
                Dexp(T0, out T4, out T10);
                dT4_dVb = T10 * dT0_dVb;
            }

            if ((model.BSIM4vtsswgs - vbs_jct) < (model.BSIM4vtsswgs * 1e-3))
            {
                T9 = 1.0e3;
                T0 = -vbs_jct / Nvtmrsswgs * T9;
                Dexp(T0, out T5, out T10);
                dT5_dVb = T10 / Nvtmrsswgs * T9;
            }
            else
            {
                T9 = 1.0 / (model.BSIM4vtsswgs - vbs_jct);
                T0 = -vbs_jct / Nvtmrsswgs * model.BSIM4vtsswgs * T9;
                dT0_dVb = model.BSIM4vtsswgs / Nvtmrsswgs * (T9 + vbs_jct * T9 * T9);
                Dexp(T0, out T5, out T10);
                dT5_dVb = T10 * dT0_dVb;
            }

            if ((model.BSIM4vtsswgd - vbd_jct) < (model.BSIM4vtsswgd * 1e-3))
            {
                T9 = 1.0e3;
                T0 = -vbd_jct / Nvtmrsswgd * T9;
                Dexp(T0, out T6, out T10);
                dT6_dVb = T10 / Nvtmrsswgd * T9;
            }
            else
            {
                T9 = 1.0 / (model.BSIM4vtsswgd - vbd_jct);
                T0 = -vbd_jct / Nvtmrsswgd * model.BSIM4vtsswgd * T9;
                dT0_dVb = model.BSIM4vtsswgd / Nvtmrsswgd * (T9 + vbd_jct * T9 * T9);
                Dexp(T0, out T6, out T10);
                dT6_dVb = T10 * dT0_dVb;
            }

            bsim4.BSIM4gbs += bsim4.BSIM4SjctTempRevSatCur * dT1_dVb + bsim4.BSIM4SswTempRevSatCur * dT3_dVb + bsim4.BSIM4SswgTempRevSatCur * dT5_dVb;
            bsim4.BSIM4cbs -= bsim4.BSIM4SjctTempRevSatCur * (T1 - 1.0) + bsim4.BSIM4SswTempRevSatCur * (T3 - 1.0) + bsim4.BSIM4SswgTempRevSatCur * (T5 - 1.0);
            bsim4.BSIM4gbd += bsim4.BSIM4DjctTempRevSatCur * dT2_dVb + bsim4.BSIM4DswTempRevSatCur * dT4_dVb + bsim4.BSIM4DswgTempRevSatCur * dT6_dVb;
            bsim4.BSIM4cbd -= bsim4.BSIM4DjctTempRevSatCur * (T2 - 1.0) + bsim4.BSIM4DswTempRevSatCur * (T4 - 1.0) + bsim4.BSIM4DswgTempRevSatCur * (T6 - 1.0);

            /* End of diode DC model */

            if (vds >= 0.0)
            {
                bsim4.BSIM4mode = 1;
                Vds = vds;
                Vgs = vgs;
                Vbs = vbs;
                Vdb = vds - vbs; /* WDLiu: for GIDL */

            }
            else
            {
                bsim4.BSIM4mode = -1;
                Vds = -vds;
                Vgs = vgd;
                Vbs = vbd;
                Vdb = -vbs;
            }

            /* dunga */
            if (model.BSIM4mtrlMod != 0)
            {
                epsrox = 3.9;
                toxe = model.BSIM4eot;
                epssub = Transistor.EPS0 * model.BSIM4epsrsub;
            }
            else
            {
                epsrox = model.BSIM4epsrox;
                toxe = model.BSIM4toxe;
                epssub = Transistor.EPSSI;
            }

            T0 = Vbs - bsim4.BSIM4vbsc - 0.001;
            T1 = Math.Sqrt(T0 * T0 - 0.004 * bsim4.BSIM4vbsc);
            if (T0 >= 0.0)
            {
                Vbseff = bsim4.BSIM4vbsc + 0.5 * (T0 + T1);
                dVbseff_dVb = 0.5 * (1.0 + T0 / T1);
            }
            else
            {
                T2 = -0.002 / (T1 - T0);
                Vbseff = bsim4.BSIM4vbsc * (1.0 + T2);
                dVbseff_dVb = T2 * bsim4.BSIM4vbsc / T1;
            }

            /* JX: Correction to forward body bias */
            T9 = 0.95 * bsim4.pParam.BSIM4phi;
            T0 = T9 - Vbseff - 0.001;
            T1 = Math.Sqrt(T0 * T0 + 0.004 * T9);
            Vbseff = T9 - 0.5 * (T0 + T1);
            dVbseff_dVb *= 0.5 * (1.0 + T0 / T1);
            Phis = bsim4.pParam.BSIM4phi - Vbseff;
            dPhis_dVb = -1.0;
            sqrtPhis = Math.Sqrt(Phis);
            dsqrtPhis_dVb = -0.5 / sqrtPhis;

            Xdep = bsim4.pParam.BSIM4Xdep0 * sqrtPhis / bsim4.pParam.BSIM4sqrtPhi;
            dXdep_dVb = (bsim4.pParam.BSIM4Xdep0 / bsim4.pParam.BSIM4sqrtPhi) * dsqrtPhis_dVb;

            Leff = bsim4.pParam.BSIM4leff;
            Vtm = model.BSIM4vtm;
            Vtm0 = model.BSIM4vtm0;

            /* Vth Calculation */
            T3 = Math.Sqrt(Xdep);
            V0 = bsim4.pParam.BSIM4vbi - bsim4.pParam.BSIM4phi;

            T0 = bsim4.pParam.BSIM4dvt2 * Vbseff;
            if (T0 >= -0.5)
            {
                T1 = 1.0 + T0;
                T2 = bsim4.pParam.BSIM4dvt2;
            }
            else
            {
                T4 = 1.0 / (3.0 + 8.0 * T0);
                T1 = (1.0 + 3.0 * T0) * T4;
                T2 = bsim4.pParam.BSIM4dvt2 * T4 * T4;
            }
            lt1 = model.BSIM4factor1 * T3 * T1;
            dlt1_dVb = model.BSIM4factor1 * (0.5 / T3 * T1 * dXdep_dVb + T3 * T2);

            T0 = bsim4.pParam.BSIM4dvt2w * Vbseff;
            if (T0 >= -0.5)
            {
                T1 = 1.0 + T0;
                T2 = bsim4.pParam.BSIM4dvt2w;
            }
            else
            {
                T4 = 1.0 / (3.0 + 8.0 * T0);
                T1 = (1.0 + 3.0 * T0) * T4;
                T2 = bsim4.pParam.BSIM4dvt2w * T4 * T4;
            }
            ltw = model.BSIM4factor1 * T3 * T1;
            dltw_dVb = model.BSIM4factor1 * (0.5 / T3 * T1 * dXdep_dVb + T3 * T2);

            T0 = bsim4.pParam.BSIM4dvt1 * Leff / lt1;
            if (T0 < Transistor.EXP_THRESHOLD)
            {
                T1 = Math.Exp(T0);
                T2 = T1 - 1.0;
                T3 = T2 * T2;
                T4 = T3 + 2.0 * T1 * Transistor.MIN_EXP;
                Theta0 = T1 / T4;
                dT1_dVb = -T0 * T1 * dlt1_dVb / lt1;
                dTheta0_dVb = dT1_dVb * (T4 - 2.0 * T1 * (T2 + Transistor.MIN_EXP)) / T4 / T4;
            }
            else
            {
                Theta0 = 1.0 / (Transistor.MAX_EXP - 2.0); /* 3.0 * Transistor.MIN_EXP omitted */
                dTheta0_dVb = 0.0;
            }
            bsim4.BSIM4thetavth = bsim4.pParam.BSIM4dvt0 * Theta0;
            Delt_vth = bsim4.BSIM4thetavth * V0;
            dDelt_vth_dVb = bsim4.pParam.BSIM4dvt0 * dTheta0_dVb * V0;

            T0 = bsim4.pParam.BSIM4dvt1w * bsim4.pParam.BSIM4weff * Leff / ltw;
            if (T0 < Transistor.EXP_THRESHOLD)
            {
                T1 = Math.Exp(T0);
                T2 = T1 - 1.0;
                T3 = T2 * T2;
                T4 = T3 + 2.0 * T1 * Transistor.MIN_EXP;
                T5 = T1 / T4;
                dT1_dVb = -T0 * T1 * dltw_dVb / ltw;
                dT5_dVb = dT1_dVb * (T4 - 2.0 * T1 * (T2 + Transistor.MIN_EXP)) / T4 / T4;
            }
            else
            {
                T5 = 1.0 / (Transistor.MAX_EXP - 2.0); /* 3.0 * Transistor.MIN_EXP omitted */
                dT5_dVb = 0.0;
            }
            T0 = bsim4.pParam.BSIM4dvt0w * T5;
            T2 = T0 * V0;
            dT2_dVb = bsim4.pParam.BSIM4dvt0w * dT5_dVb * V0;

            TempRatio = ckt.State.Temperature / model.BSIM4tnom - 1.0;
            T0 = Math.Sqrt(1.0 + bsim4.pParam.BSIM4lpe0 / Leff);
            T1 = bsim4.pParam.BSIM4k1ox * (T0 - 1.0) * bsim4.pParam.BSIM4sqrtPhi + (bsim4.pParam.BSIM4kt1 + bsim4.pParam.BSIM4kt1l / Leff + bsim4.pParam.BSIM4kt2 *
                Vbseff) * TempRatio;
            Vth_NarrowW = toxe * bsim4.pParam.BSIM4phi / (bsim4.pParam.BSIM4weff + bsim4.pParam.BSIM4w0);

            T3 = bsim4.BSIM4eta0 + bsim4.pParam.BSIM4etab * Vbseff;
            if (T3 < 1.0e-4)
            {
                T9 = 1.0 / (3.0 - 2.0e4 * T3);
                T3 = (2.0e-4 - T3) * T9;
                T4 = T9 * T9;
            }
            else
            {
                T4 = 1.0;
            }
            dDIBL_Sft_dVd = T3 * bsim4.pParam.BSIM4theta0vb0;
            DIBL_Sft = dDIBL_Sft_dVd * Vds;

            Lpe_Vb = Math.Sqrt(1.0 + bsim4.pParam.BSIM4lpeb / Leff);

            Vth = model.BSIM4type * bsim4.BSIM4vth0 + (bsim4.pParam.BSIM4k1ox * sqrtPhis - bsim4.pParam.BSIM4k1 * bsim4.pParam.BSIM4sqrtPhi) * Lpe_Vb - bsim4.BSIM4k2ox *
                Vbseff - Delt_vth - T2 + (bsim4.pParam.BSIM4k3 + bsim4.pParam.BSIM4k3b * Vbseff) * Vth_NarrowW + T1 - DIBL_Sft;

            dVth_dVb = Lpe_Vb * bsim4.pParam.BSIM4k1ox * dsqrtPhis_dVb - bsim4.BSIM4k2ox - dDelt_vth_dVb - dT2_dVb + bsim4.pParam.BSIM4k3b * Vth_NarrowW -
                bsim4.pParam.BSIM4etab * Vds * bsim4.pParam.BSIM4theta0vb0 * T4 + bsim4.pParam.BSIM4kt2 * TempRatio;
            dVth_dVd = -dDIBL_Sft_dVd;

            /* Calculate n */
            tmp1 = epssub / Xdep;
            bsim4.BSIM4nstar = model.BSIM4vtm / Transistor.Charge_q * (model.BSIM4coxe + tmp1 + bsim4.pParam.BSIM4cit);
            tmp2 = bsim4.pParam.BSIM4nfactor * tmp1;
            tmp3 = bsim4.pParam.BSIM4cdsc + bsim4.pParam.BSIM4cdscb * Vbseff + bsim4.pParam.BSIM4cdscd * Vds;
            tmp4 = (tmp2 + tmp3 * Theta0 + bsim4.pParam.BSIM4cit) / model.BSIM4coxe;
            if (tmp4 >= -0.5)
            {
                n = 1.0 + tmp4;
                dn_dVb = (-tmp2 / Xdep * dXdep_dVb + tmp3 * dTheta0_dVb + bsim4.pParam.BSIM4cdscb * Theta0) / model.BSIM4coxe;
                dn_dVd = bsim4.pParam.BSIM4cdscd * Theta0 / model.BSIM4coxe;
            }
            else
            {
                T0 = 1.0 / (3.0 + 8.0 * tmp4);
                n = (1.0 + 3.0 * tmp4) * T0;
                T0 *= T0;
                dn_dVb = (-tmp2 / Xdep * dXdep_dVb + tmp3 * dTheta0_dVb + bsim4.pParam.BSIM4cdscb * Theta0) / model.BSIM4coxe * T0;
                dn_dVd = bsim4.pParam.BSIM4cdscd * Theta0 / model.BSIM4coxe * T0;
            }

            /* Vth correction for Pocket implant */
            if (bsim4.pParam.BSIM4dvtp0 > 0.0)
            {
                T0 = -bsim4.pParam.BSIM4dvtp1 * Vds;
                if (T0 < -Transistor.EXP_THRESHOLD)
                {
                    T2 = Transistor.MIN_EXP;
                    dT2_dVd = 0.0;
                }
                else
                {
                    T2 = Math.Exp(T0);
                    dT2_dVd = -bsim4.pParam.BSIM4dvtp1 * T2;
                }

                T3 = Leff + bsim4.pParam.BSIM4dvtp0 * (1.0 + T2);
                dT3_dVd = bsim4.pParam.BSIM4dvtp0 * dT2_dVd;
                if (model.BSIM4tempMod < 2)
                {
                    T4 = Vtm * Math.Log(Leff / T3);
                    dT4_dVd = -Vtm * dT3_dVd / T3;
                }
                else
                {
                    T4 = model.BSIM4vtm0 * Math.Log(Leff / T3);
                    dT4_dVd = -model.BSIM4vtm0 * dT3_dVd / T3;
                }
                dDITS_Sft_dVd = dn_dVd * T4 + n * dT4_dVd;
                dDITS_Sft_dVb = T4 * dn_dVb;

                Vth -= n * T4;
                dVth_dVd -= dDITS_Sft_dVd;
                dVth_dVb -= dDITS_Sft_dVb;
            }

            /* v4.7 DITS_SFT2 */
            if ((bsim4.pParam.BSIM4dvtp4 == 0.0) || (bsim4.pParam.BSIM4dvtp2factor == 0.0))
            {
                T0 = 0.0;
                DITS_Sft2 = 0.0;
            }
            else
            {

                // T0 = Math.Exp(2.0 * bsim4.pParam.BSIM4dvtp4 * Vds); /* beta code */
                T1 = 2.0 * bsim4.pParam.BSIM4dvtp4 * Vds;
                Dexp(T1, out T0, out T10);
                DITS_Sft2 = bsim4.pParam.BSIM4dvtp2factor * (T0 - 1) / (T0 + 1);

                // dDITS_Sft2_dVd = bsim4.pParam.BSIM4dvtp2factor * bsim4.pParam.BSIM4dvtp4 * 4.0 * T0 / ((T0 + 1) * (T0 + 1)); /* beta code */
                dDITS_Sft2_dVd = bsim4.pParam.BSIM4dvtp2factor * bsim4.pParam.BSIM4dvtp4 * 4.0 * T10 / ((T0 + 1) * (T0 + 1));
                Vth -= DITS_Sft2;
                dVth_dVd -= dDITS_Sft2_dVd;
            }

            bsim4.BSIM4von = Vth;

            /* Poly Gate Si Depletion Effect */
            T0 = bsim4.BSIM4vfb + bsim4.pParam.BSIM4phi;
            if (model.BSIM4mtrlMod.Value == 0)
                T1 = Transistor.EPSSI;
            else
                T1 = model.BSIM4epsrgate * Transistor.EPS0;

            BSIM4polyDepletion(T0, bsim4.pParam.BSIM4ngate, T1, model.BSIM4coxe, vgs, out vgs_eff, out dvgs_eff_dvg);
            BSIM4polyDepletion(T0, bsim4.pParam.BSIM4ngate, T1, model.BSIM4coxe, vgd, out vgd_eff, out dvgd_eff_dvg);

            if (bsim4.BSIM4mode > 0)
            {
                Vgs_eff = vgs_eff;
                dVgs_eff_dVg = dvgs_eff_dvg;
            }
            else
            {
                Vgs_eff = vgd_eff;
                dVgs_eff_dVg = dvgd_eff_dvg;
            }
            bsim4.BSIM4vgs_eff = vgs_eff;
            bsim4.BSIM4vgd_eff = vgd_eff;
            bsim4.BSIM4dvgs_eff_dvg = dvgs_eff_dvg;
            bsim4.BSIM4dvgd_eff_dvg = dvgd_eff_dvg;

            Vgst = Vgs_eff - Vth;

            /* Calculate Vgsteff */
            T0 = n * Vtm;
            T1 = bsim4.pParam.BSIM4mstar * Vgst;
            T2 = T1 / T0;
            if (T2 > Transistor.EXP_THRESHOLD)
            {
                T10 = T1;
                dT10_dVg = bsim4.pParam.BSIM4mstar * dVgs_eff_dVg;
                dT10_dVd = -dVth_dVd * bsim4.pParam.BSIM4mstar;
                dT10_dVb = -dVth_dVb * bsim4.pParam.BSIM4mstar;
            }
            else if (T2 < -Transistor.EXP_THRESHOLD)
            {
                T10 = Vtm * Math.Log(1.0 + Transistor.MIN_EXP);
                dT10_dVg = 0.0;
                dT10_dVd = T10 * dn_dVd;
                dT10_dVb = T10 * dn_dVb;
                T10 *= n;
            }
            else
            {
                ExpVgst = Math.Exp(T2);
                T3 = Vtm * Math.Log(1.0 + ExpVgst);
                T10 = n * T3;
                dT10_dVg = bsim4.pParam.BSIM4mstar * ExpVgst / (1.0 + ExpVgst);
                dT10_dVb = T3 * dn_dVb - dT10_dVg * (dVth_dVb + Vgst * dn_dVb / n);
                dT10_dVd = T3 * dn_dVd - dT10_dVg * (dVth_dVd + Vgst * dn_dVd / n);
                dT10_dVg *= dVgs_eff_dVg;
            }

            T1 = bsim4.pParam.BSIM4voffcbn - (1.0 - bsim4.pParam.BSIM4mstar) * Vgst;
            T2 = T1 / T0;
            if (T2 < -Transistor.EXP_THRESHOLD)
            {
                T3 = model.BSIM4coxe * Transistor.MIN_EXP / bsim4.pParam.BSIM4cdep0;
                T9 = bsim4.pParam.BSIM4mstar + T3 * n;
                dT9_dVg = 0.0;
                dT9_dVd = dn_dVd * T3;
                dT9_dVb = dn_dVb * T3;
            }
            else if (T2 > Transistor.EXP_THRESHOLD)
            {
                T3 = model.BSIM4coxe * Transistor.MAX_EXP / bsim4.pParam.BSIM4cdep0;
                T9 = bsim4.pParam.BSIM4mstar + T3 * n;
                dT9_dVg = 0.0;
                dT9_dVd = dn_dVd * T3;
                dT9_dVb = dn_dVb * T3;
            }
            else
            {
                ExpVgst = Math.Exp(T2);
                T3 = model.BSIM4coxe / bsim4.pParam.BSIM4cdep0;
                T4 = T3 * ExpVgst;
                T5 = T1 * T4 / T0;
                T9 = bsim4.pParam.BSIM4mstar + n * T4;
                dT9_dVg = T3 * (bsim4.pParam.BSIM4mstar - 1.0) * ExpVgst / Vtm;
                dT9_dVb = T4 * dn_dVb - dT9_dVg * dVth_dVb - T5 * dn_dVb;
                dT9_dVd = T4 * dn_dVd - dT9_dVg * dVth_dVd - T5 * dn_dVd;
                dT9_dVg *= dVgs_eff_dVg;
            }
            bsim4.BSIM4Vgsteff = Vgsteff = T10 / T9;
            T11 = T9 * T9;
            dVgsteff_dVg = (T9 * dT10_dVg - T10 * dT9_dVg) / T11;
            dVgsteff_dVd = (T9 * dT10_dVd - T10 * dT9_dVd) / T11;
            dVgsteff_dVb = (T9 * dT10_dVb - T10 * dT9_dVb) / T11;

            /* Calculate Effective Channel Geometry */
            T9 = sqrtPhis - bsim4.pParam.BSIM4sqrtPhi;
            Weff = bsim4.pParam.BSIM4weff - 2.0 * (bsim4.pParam.BSIM4dwg * Vgsteff + bsim4.pParam.BSIM4dwb * T9);
            dWeff_dVg = -2.0 * bsim4.pParam.BSIM4dwg;
            dWeff_dVb = -2.0 * bsim4.pParam.BSIM4dwb * dsqrtPhis_dVb;

            if (Weff < 2.0e-8)
            /* to avoid the discontinuity problem due to Weff */
            {
                T0 = 1.0 / (6.0e-8 - 2.0 * Weff);
                Weff = 2.0e-8 * (4.0e-8 - Weff) * T0;
                T0 *= T0 * 4.0e-16;
                dWeff_dVg *= T0;
                dWeff_dVb *= T0;
            }

            if (model.BSIM4rdsMod.Value == 1)
                Rds = dRds_dVg = dRds_dVb = 0.0;
            else
            {
                T0 = 1.0 + bsim4.pParam.BSIM4prwg * Vgsteff;
                dT0_dVg = -bsim4.pParam.BSIM4prwg / T0 / T0;
                T1 = bsim4.pParam.BSIM4prwb * T9;
                dT1_dVb = bsim4.pParam.BSIM4prwb * dsqrtPhis_dVb;

                T2 = 1.0 / T0 + T1;
                T3 = T2 + Math.Sqrt(T2 * T2 + 0.01); /* 0.01 = 4.0 * 0.05 * 0.05 */
                dT3_dVg = 1.0 + T2 / (T3 - T2);
                dT3_dVb = dT3_dVg * dT1_dVb;
                dT3_dVg *= dT0_dVg;

                T4 = bsim4.pParam.BSIM4rds0 * 0.5;
                Rds = bsim4.pParam.BSIM4rdswmin + T3 * T4;
                dRds_dVg = T4 * dT3_dVg;
                dRds_dVb = T4 * dT3_dVb;

                if (Rds > 0.0)
                    bsim4.BSIM4grdsw = 1.0 / Rds * bsim4.BSIM4nf; /* 4.6.2 */
                else
                    bsim4.BSIM4grdsw = 0.0;
            }

            /* Calculate Abulk */
            T9 = 0.5 * bsim4.pParam.BSIM4k1ox * Lpe_Vb / sqrtPhis;
            T1 = T9 + bsim4.BSIM4k2ox - bsim4.pParam.BSIM4k3b * Vth_NarrowW;
            dT1_dVb = -T9 / sqrtPhis * dsqrtPhis_dVb;

            T9 = Math.Sqrt(bsim4.pParam.BSIM4xj * Xdep);
            tmp1 = Leff + 2.0 * T9;
            T5 = Leff / tmp1;
            tmp2 = bsim4.pParam.BSIM4a0 * T5;
            tmp3 = bsim4.pParam.BSIM4weff + bsim4.pParam.BSIM4b1;
            tmp4 = bsim4.pParam.BSIM4b0 / tmp3;
            T2 = tmp2 + tmp4;
            dT2_dVb = -T9 / tmp1 / Xdep * dXdep_dVb;
            T6 = T5 * T5;
            T7 = T5 * T6;

            Abulk0 = 1.0 + T1 * T2;
            dAbulk0_dVb = T1 * tmp2 * dT2_dVb + T2 * dT1_dVb;

            T8 = bsim4.pParam.BSIM4ags * bsim4.pParam.BSIM4a0 * T7;
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
            {
                T9 = 1.0 / (3.0 - 20.0 * Abulk);
                Abulk = (0.2 - Abulk) * T9;
                T10 = T9 * T9;
                dAbulk_dVb *= T10;
                dAbulk_dVg *= T10;
            }
            bsim4.BSIM4Abulk = Abulk;

            T2 = bsim4.pParam.BSIM4keta * Vbseff;
            if (T2 >= -0.9)
            {
                T0 = 1.0 / (1.0 + T2);
                dT0_dVb = -bsim4.pParam.BSIM4keta * T0 * T0;
            }
            else
            {
                T1 = 1.0 / (0.8 + T2);
                T0 = (17.0 + 20.0 * T2) * T1;
                dT0_dVb = -bsim4.pParam.BSIM4keta * T1 * T1;
            }
            dAbulk_dVg *= T0;
            dAbulk_dVb = dAbulk_dVb * T0 + Abulk * dT0_dVb;
            dAbulk0_dVb = dAbulk0_dVb * T0 + Abulk0 * dT0_dVb;
            Abulk *= T0;
            Abulk0 *= T0;

            /* Mobility calculation */
            if (model.BSIM4mtrlMod != 0 && model.BSIM4mtrlCompatMod.Value == 0)
                T14 = 2.0 * model.BSIM4type * (model.BSIM4phig - model.BSIM4easub - 0.5 * model.BSIM4Eg0 + 0.45);
            else
                T14 = 0.0;

            if (model.BSIM4mobMod.Value == 0)
            {
                T0 = Vgsteff + Vth + Vth - T14;
                T2 = bsim4.pParam.BSIM4ua + bsim4.pParam.BSIM4uc * Vbseff;
                T3 = T0 / toxe;
                T12 = Math.Sqrt(Vth * Vth + 0.0001);
                T9 = 1.0 / (Vgsteff + 2 * T12);
                T10 = T9 * toxe;
                T8 = bsim4.pParam.BSIM4ud * T10 * T10 * Vth;
                T6 = T8 * Vth;
                T5 = T3 * (T2 + bsim4.pParam.BSIM4ub * T3) + T6;
                T7 = -2.0 * T6 * T9;
                T11 = T7 * Vth / T12;
                dDenomi_dVg = (T2 + 2.0 * bsim4.pParam.BSIM4ub * T3) / toxe;
                T13 = 2.0 * (dDenomi_dVg + T11 + T8);
                dDenomi_dVd = T13 * dVth_dVd;
                dDenomi_dVb = T13 * dVth_dVb + bsim4.pParam.BSIM4uc * T3;
                dDenomi_dVg += T7;
            }
            else if (model.BSIM4mobMod.Value == 1)
            {
                T0 = Vgsteff + Vth + Vth - T14;
                T2 = 1.0 + bsim4.pParam.BSIM4uc * Vbseff;
                T3 = T0 / toxe;
                T4 = T3 * (bsim4.pParam.BSIM4ua + bsim4.pParam.BSIM4ub * T3);
                T12 = Math.Sqrt(Vth * Vth + 0.0001);
                T9 = 1.0 / (Vgsteff + 2 * T12);
                T10 = T9 * toxe;
                T8 = bsim4.pParam.BSIM4ud * T10 * T10 * Vth;
                T6 = T8 * Vth;
                T5 = T4 * T2 + T6;
                T7 = -2.0 * T6 * T9;
                T11 = T7 * Vth / T12;
                dDenomi_dVg = (bsim4.pParam.BSIM4ua + 2.0 * bsim4.pParam.BSIM4ub * T3) * T2 / toxe;
                T13 = 2.0 * (dDenomi_dVg + T11 + T8);
                dDenomi_dVd = T13 * dVth_dVd;
                dDenomi_dVb = T13 * dVth_dVb + bsim4.pParam.BSIM4uc * T4;
                dDenomi_dVg += T7;
            }
            else if (model.BSIM4mobMod.Value == 2)
            {
                T0 = (Vgsteff + bsim4.BSIM4vtfbphi1) / toxe;
                T1 = Math.Exp(bsim4.pParam.BSIM4eu * Math.Log(T0));
                dT1_dVg = T1 * bsim4.pParam.BSIM4eu / T0 / toxe;
                T2 = bsim4.pParam.BSIM4ua + bsim4.pParam.BSIM4uc * Vbseff;

                T12 = Math.Sqrt(Vth * Vth + 0.0001);
                T9 = 1.0 / (Vgsteff + 2 * T12);
                T10 = T9 * toxe;
                T8 = bsim4.pParam.BSIM4ud * T10 * T10 * Vth;
                T6 = T8 * Vth;
                T5 = T1 * T2 + T6;
                T7 = -2.0 * T6 * T9;
                T11 = T7 * Vth / T12;
                dDenomi_dVg = T2 * dT1_dVg + T7;
                T13 = 2.0 * (T11 + T8);
                dDenomi_dVd = T13 * dVth_dVd;
                dDenomi_dVb = T13 * dVth_dVb + T1 * bsim4.pParam.BSIM4uc;
            }
            else if (model.BSIM4mobMod.Value == 4)
            /* Synopsys 08 / 30 / 2013 add */
            {
                T0 = Vgsteff + bsim4.BSIM4vtfbphi1 - T14;
                T2 = bsim4.pParam.BSIM4ua + bsim4.pParam.BSIM4uc * Vbseff;
                T3 = T0 / toxe;
                T12 = Math.Sqrt(bsim4.BSIM4vtfbphi1 * bsim4.BSIM4vtfbphi1 + 0.0001);
                T9 = 1.0 / (Vgsteff + 2 * T12);
                T10 = T9 * toxe;
                T8 = bsim4.pParam.BSIM4ud * T10 * T10 * bsim4.BSIM4vtfbphi1;
                T6 = T8 * bsim4.BSIM4vtfbphi1;
                T5 = T3 * (T2 + bsim4.pParam.BSIM4ub * T3) + T6;
                T7 = -2.0 * T6 * T9;
                dDenomi_dVg = (T2 + 2.0 * bsim4.pParam.BSIM4ub * T3) / toxe;
                dDenomi_dVd = 0.0;
                dDenomi_dVb = bsim4.pParam.BSIM4uc * T3;
                dDenomi_dVg += T7;
            }
            else if (model.BSIM4mobMod.Value == 5)
            /* Synopsys 08 / 30 / 2013 add */
            {
                T0 = Vgsteff + bsim4.BSIM4vtfbphi1 - T14;
                T2 = 1.0 + bsim4.pParam.BSIM4uc * Vbseff;
                T3 = T0 / toxe;
                T4 = T3 * (bsim4.pParam.BSIM4ua + bsim4.pParam.BSIM4ub * T3);
                T12 = Math.Sqrt(bsim4.BSIM4vtfbphi1 * bsim4.BSIM4vtfbphi1 + 0.0001);
                T9 = 1.0 / (Vgsteff + 2 * T12);
                T10 = T9 * toxe;
                T8 = bsim4.pParam.BSIM4ud * T10 * T10 * bsim4.BSIM4vtfbphi1;
                T6 = T8 * bsim4.BSIM4vtfbphi1;
                T5 = T4 * T2 + T6;
                T7 = -2.0 * T6 * T9;
                dDenomi_dVg = (bsim4.pParam.BSIM4ua + 2.0 * bsim4.pParam.BSIM4ub * T3) * T2 / toxe;
                dDenomi_dVd = 0.0;
                dDenomi_dVb = bsim4.pParam.BSIM4uc * T4;
                dDenomi_dVg += T7;
            }
            else if (model.BSIM4mobMod.Value == 6)
            /* Synopsys 08 / 30 / 2013 modify */
            {
                T0 = (Vgsteff + bsim4.BSIM4vtfbphi1) / toxe;
                T1 = Math.Exp(bsim4.pParam.BSIM4eu * Math.Log(T0));
                dT1_dVg = T1 * bsim4.pParam.BSIM4eu / T0 / toxe;
                T2 = bsim4.pParam.BSIM4ua + bsim4.pParam.BSIM4uc * Vbseff;

                T12 = Math.Sqrt(bsim4.BSIM4vtfbphi1 * bsim4.BSIM4vtfbphi1 + 0.0001);
                T9 = 1.0 / (Vgsteff + 2 * T12);
                T10 = T9 * toxe;
                T8 = bsim4.pParam.BSIM4ud * T10 * T10 * bsim4.BSIM4vtfbphi1;
                T6 = T8 * bsim4.BSIM4vtfbphi1;
                T5 = T1 * T2 + T6;
                T7 = -2.0 * T6 * T9;
                dDenomi_dVg = T2 * dT1_dVg + T7;
                dDenomi_dVd = 0;
                dDenomi_dVb = T1 * bsim4.pParam.BSIM4uc;
            }

            /* high K mobility */
            else
            {
                /* univsersal mobility */
                T0 = (Vgsteff + bsim4.BSIM4vtfbphi1) * 1.0e-8 / toxe / 6.0;
                T1 = Math.Exp(bsim4.pParam.BSIM4eu * Math.Log(T0));
                dT1_dVg = T1 * bsim4.pParam.BSIM4eu * 1.0e-8 / T0 / toxe / 6.0;
                T2 = bsim4.pParam.BSIM4ua + bsim4.pParam.BSIM4uc * Vbseff;

                /* Coulombic */
                VgsteffVth = bsim4.pParam.BSIM4VgsteffVth;

                T10 = Math.Exp(bsim4.pParam.BSIM4ucs * Math.Log(0.5 + 0.5 * Vgsteff / VgsteffVth));
                T11 = bsim4.pParam.BSIM4ud / T10;
                dT11_dVg = -0.5 * bsim4.pParam.BSIM4ucs * T11 / (0.5 + 0.5 * Vgsteff / VgsteffVth) / VgsteffVth;

                dDenomi_dVg = T2 * dT1_dVg + dT11_dVg;
                dDenomi_dVd = 0.0;
                dDenomi_dVb = T1 * bsim4.pParam.BSIM4uc;

                T5 = T1 * T2 + T11;
            }

            if (T5 >= -0.8)
            {
                Denomi = 1.0 + T5;
            }
            else
            {
                T9 = 1.0 / (7.0 + 10.0 * T5);
                Denomi = (0.6 + T5) * T9;
                T9 *= T9;
                dDenomi_dVg *= T9;
                dDenomi_dVd *= T9;
                dDenomi_dVb *= T9;
            }

            bsim4.BSIM4ueff = ueff = bsim4.BSIM4u0temp / Denomi;
            T9 = -ueff / Denomi;
            dueff_dVg = T9 * dDenomi_dVg;
            dueff_dVd = T9 * dDenomi_dVd;
            dueff_dVb = T9 * dDenomi_dVb;

            /* Saturation Drain Voltage  Vdsat */
            WVCox = Weff * bsim4.BSIM4vsattemp * model.BSIM4coxe;
            WVCoxRds = WVCox * Rds;

            Esat = 2.0 * bsim4.BSIM4vsattemp / ueff;
            bsim4.BSIM4EsatL = EsatL = Esat * Leff;
            T0 = -EsatL / ueff;
            dEsatL_dVg = T0 * dueff_dVg;
            dEsatL_dVd = T0 * dueff_dVd;
            dEsatL_dVb = T0 * dueff_dVb;

            /* Sqrt() */
            a1 = bsim4.pParam.BSIM4a1;
            if (a1 == 0.0)
            {
                Lambda = bsim4.pParam.BSIM4a2;
                dLambda_dVg = 0.0;
            }
            else if (a1 > 0.0)
            {
                T0 = 1.0 - bsim4.pParam.BSIM4a2;
                T1 = T0 - bsim4.pParam.BSIM4a1 * Vgsteff - 0.0001;
                T2 = Math.Sqrt(T1 * T1 + 0.0004 * T0);
                Lambda = bsim4.pParam.BSIM4a2 + T0 - 0.5 * (T1 + T2);
                dLambda_dVg = 0.5 * bsim4.pParam.BSIM4a1 * (1.0 + T1 / T2);
            }
            else
            {
                T1 = bsim4.pParam.BSIM4a2 + bsim4.pParam.BSIM4a1 * Vgsteff - 0.0001;
                T2 = Math.Sqrt(T1 * T1 + 0.0004 * bsim4.pParam.BSIM4a2);
                Lambda = 0.5 * (T1 + T2);
                dLambda_dVg = 0.5 * bsim4.pParam.BSIM4a1 * (1.0 + T1 / T2);
            }

            Vgst2Vtm = Vgsteff + 2.0 * Vtm;
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
            bsim4.BSIM4vdsat = Vdsat;

            /* Calculate Vdseff */
            T1 = Vdsat - Vds - bsim4.pParam.BSIM4delta;
            dT1_dVg = dVdsat_dVg;
            dT1_dVd = dVdsat_dVd - 1.0;
            dT1_dVb = dVdsat_dVb;

            T2 = Math.Sqrt(T1 * T1 + 4.0 * bsim4.pParam.BSIM4delta * Vdsat);
            T0 = T1 / T2;
            T9 = 2.0 * bsim4.pParam.BSIM4delta;
            T3 = T9 / T2;
            dT2_dVg = T0 * dT1_dVg + T3 * dVdsat_dVg;
            dT2_dVd = T0 * dT1_dVd + T3 * dVdsat_dVd;
            dT2_dVb = T0 * dT1_dVb + T3 * dVdsat_dVb;

            if (T1 >= 0.0)
            {
                Vdseff = Vdsat - 0.5 * (T1 + T2);
                dVdseff_dVg = dVdsat_dVg - 0.5 * (dT1_dVg + dT2_dVg);
                dVdseff_dVd = dVdsat_dVd - 0.5 * (dT1_dVd + dT2_dVd);
                dVdseff_dVb = dVdsat_dVb - 0.5 * (dT1_dVb + dT2_dVb);
            }
            else
            {
                T4 = T9 / (T2 - T1);
                T5 = 1.0 - T4;
                T6 = Vdsat * T4 / (T2 - T1);
                Vdseff = Vdsat * T5;
                dVdseff_dVg = dVdsat_dVg * T5 + T6 * (dT2_dVg - dT1_dVg);
                dVdseff_dVd = dVdsat_dVd * T5 + T6 * (dT2_dVd - dT1_dVd);
                dVdseff_dVb = dVdsat_dVb * T5 + T6 * (dT2_dVb - dT1_dVb);
            }

            if (Vds == 0.0)
            {
                Vdseff = 0.0;
                dVdseff_dVg = 0.0;
                dVdseff_dVb = 0.0;
            }

            if (Vdseff > Vds)
                Vdseff = Vds;
            diffVds = Vds - Vdseff;
            bsim4.BSIM4Vdseff = Vdseff;

            /* Velocity Overshoot */
            if ((model.BSIM4lambda.Given) && (model.BSIM4lambda > 0.0))
            {
                T1 = Leff * ueff;
                T2 = bsim4.pParam.BSIM4lambda / T1;
                T3 = -T2 / T1 * Leff;
                dT2_dVd = T3 * dueff_dVd;
                dT2_dVg = T3 * dueff_dVg;
                dT2_dVb = T3 * dueff_dVb;
                T5 = 1.0 / (Esat * bsim4.pParam.BSIM4litl);
                T4 = -T5 / EsatL;
                dT5_dVg = dEsatL_dVg * T4;
                dT5_dVd = dEsatL_dVd * T4;
                dT5_dVb = dEsatL_dVb * T4;
                T6 = 1.0 + diffVds * T5;
                dT6_dVg = dT5_dVg * diffVds - dVdseff_dVg * T5;
                dT6_dVd = dT5_dVd * diffVds + (1.0 - dVdseff_dVd) * T5;
                dT6_dVb = dT5_dVb * diffVds - dVdseff_dVb * T5;
                T7 = 2.0 / (T6 * T6 + 1.0);
                T8 = 1.0 - T7;
                T9 = T6 * T7 * T7;
                dT8_dVg = T9 * dT6_dVg;
                dT8_dVd = T9 * dT6_dVd;
                dT8_dVb = T9 * dT6_dVb;
                T10 = 1.0 + T2 * T8;
                dT10_dVg = dT2_dVg * T8 + T2 * dT8_dVg;
                dT10_dVd = dT2_dVd * T8 + T2 * dT8_dVd;
                dT10_dVb = dT2_dVb * T8 + T2 * dT8_dVb;
                if (T10 == 1.0)
                    dT10_dVg = dT10_dVd = dT10_dVb = 0.0;

                dEsatL_dVg *= T10;
                dEsatL_dVg += EsatL * dT10_dVg;
                dEsatL_dVd *= T10;
                dEsatL_dVd += EsatL * dT10_dVd;
                dEsatL_dVb *= T10;
                dEsatL_dVb += EsatL * dT10_dVb;
                EsatL *= T10;
                Esat = EsatL / Leff; /* bugfix by Wenwei Yang (4.6.4) */
                bsim4.BSIM4EsatL = EsatL;
            }

            /* Calculate Vasat */
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

            /* Calculate Idl first */

            tmp1 = bsim4.BSIM4vtfbphi2;
            tmp2 = 2.0e8 * bsim4.BSIM4toxp;
            dT0_dVg = 1.0 / tmp2;
            T0 = (Vgsteff + tmp1) * dT0_dVg;

            tmp3 = Math.Exp(model.BSIM4bdos * 0.7 * Math.Log(T0));
            T1 = 1.0 + tmp3;
            T2 = model.BSIM4bdos * 0.7 * tmp3 / T0;
            Tcen = model.BSIM4ados * 1.9e-9 / T1;
            dTcen_dVg = -Tcen * T2 * dT0_dVg / T1;

            Coxeff = epssub * bsim4.BSIM4coxp / (epssub + bsim4.BSIM4coxp * Tcen);
            bsim4.BSIM4Coxeff = Coxeff;
            dCoxeff_dVg = -Coxeff * Coxeff * dTcen_dVg / epssub;

            CoxeffWovL = Coxeff * Weff / Leff;
            beta = ueff * CoxeffWovL;
            T3 = ueff / Leff;
            dbeta_dVg = CoxeffWovL * dueff_dVg + T3 * (Weff * dCoxeff_dVg + Coxeff * dWeff_dVg);
            dbeta_dVd = CoxeffWovL * dueff_dVd;
            dbeta_dVb = CoxeffWovL * dueff_dVb + T3 * Coxeff * dWeff_dVb;

            bsim4.BSIM4AbovVgst2Vtm = Abulk / Vgst2Vtm;
            T0 = 1.0 - 0.5 * Vdseff * bsim4.BSIM4AbovVgst2Vtm;
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
            Idl = gche / T0;
            T1 = (1.0 - Idl * Rds) / T0;
            T2 = Idl * Idl;
            dIdl_dVg = T1 * dgche_dVg - T2 * dRds_dVg;
            dIdl_dVd = T1 * dgche_dVd;
            dIdl_dVb = T1 * dgche_dVb - T2 * dRds_dVb;

            /* Calculate degradation factor due to pocket implant */

            if (bsim4.pParam.BSIM4fprout <= 0.0)
            {
                FP = 1.0;
                dFP_dVg = 0.0;
            }
            else
            {
                T9 = bsim4.pParam.BSIM4fprout * Math.Sqrt(Leff) / Vgst2Vtm;
                FP = 1.0 / (1.0 + T9);
                dFP_dVg = FP * FP * T9 / Vgst2Vtm;
            }

            /* Calculate VACLM */
            T8 = bsim4.pParam.BSIM4pvag / EsatL;
            T9 = T8 * Vgsteff;
            if (T9 > -0.9)
            {
                PvagTerm = 1.0 + T9;
                dPvagTerm_dVg = T8 * (1.0 - Vgsteff * dEsatL_dVg / EsatL);
                dPvagTerm_dVb = -T9 * dEsatL_dVb / EsatL;
                dPvagTerm_dVd = -T9 * dEsatL_dVd / EsatL;
            }
            else
            {
                T4 = 1.0 / (17.0 + 20.0 * T9);
                PvagTerm = (0.8 + T9) * T4;
                T4 *= T4;
                dPvagTerm_dVg = T8 * (1.0 - Vgsteff * dEsatL_dVg / EsatL) * T4;
                T9 *= T4 / EsatL;
                dPvagTerm_dVb = -T9 * dEsatL_dVb;
                dPvagTerm_dVd = -T9 * dEsatL_dVd;
            }

            if ((bsim4.pParam.BSIM4pclm > Transistor.MIN_EXP) && (diffVds > 1.0e-10))
            {
                T0 = 1.0 + Rds * Idl;
                dT0_dVg = dRds_dVg * Idl + Rds * dIdl_dVg;
                dT0_dVd = Rds * dIdl_dVd;
                dT0_dVb = dRds_dVb * Idl + Rds * dIdl_dVb;

                T2 = Vdsat / Esat;
                T1 = Leff + T2;
                dT1_dVg = (dVdsat_dVg - T2 * dEsatL_dVg / Leff) / Esat;
                dT1_dVd = (dVdsat_dVd - T2 * dEsatL_dVd / Leff) / Esat;
                dT1_dVb = (dVdsat_dVb - T2 * dEsatL_dVb / Leff) / Esat;

                Cclm = FP * PvagTerm * T0 * T1 / (bsim4.pParam.BSIM4pclm * bsim4.pParam.BSIM4litl);
                dCclm_dVg = Cclm * (dFP_dVg / FP + dPvagTerm_dVg / PvagTerm + dT0_dVg / T0 + dT1_dVg / T1);
                dCclm_dVb = Cclm * (dPvagTerm_dVb / PvagTerm + dT0_dVb / T0 + dT1_dVb / T1);
                dCclm_dVd = Cclm * (dPvagTerm_dVd / PvagTerm + dT0_dVd / T0 + dT1_dVd / T1);
                VACLM = Cclm * diffVds;

                dVACLM_dVg = dCclm_dVg * diffVds - dVdseff_dVg * Cclm;
                dVACLM_dVb = dCclm_dVb * diffVds - dVdseff_dVb * Cclm;
                dVACLM_dVd = dCclm_dVd * diffVds + (1.0 - dVdseff_dVd) * Cclm;
            }
            else
            {
                VACLM = Cclm = Transistor.MAX_EXP;
                dVACLM_dVd = dVACLM_dVg = dVACLM_dVb = 0.0;
                dCclm_dVd = dCclm_dVg = dCclm_dVb = 0.0;
            }

            /* Calculate VADIBL */
            if (bsim4.pParam.BSIM4thetaRout > Transistor.MIN_EXP)
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
                T2 = bsim4.pParam.BSIM4thetaRout;
                VADIBL = (Vgst2Vtm - T0 / T1) / T2;
                dVADIBL_dVg = (1.0 - dT0_dVg / T1 + T0 * dT1_dVg / T9) / T2;
                dVADIBL_dVb = (-dT0_dVb / T1 + T0 * dT1_dVb / T9) / T2;
                dVADIBL_dVd = (-dT0_dVd / T1 + T0 * dT1_dVd / T9) / T2;

                T7 = bsim4.pParam.BSIM4pdiblb * Vbseff;
                if (T7 >= -0.9)
                {
                    T3 = 1.0 / (1.0 + T7);
                    VADIBL *= T3;
                    dVADIBL_dVg *= T3;
                    dVADIBL_dVb = (dVADIBL_dVb - VADIBL * bsim4.pParam.BSIM4pdiblb) * T3;
                    dVADIBL_dVd *= T3;
                }
                else
                {
                    T4 = 1.0 / (0.8 + T7);
                    T3 = (17.0 + 20.0 * T7) * T4;
                    dVADIBL_dVg *= T3;
                    dVADIBL_dVb = dVADIBL_dVb * T3 - VADIBL * bsim4.pParam.BSIM4pdiblb * T4 * T4;
                    dVADIBL_dVd *= T3;
                    VADIBL *= T3;
                }

                dVADIBL_dVg = dVADIBL_dVg * PvagTerm + VADIBL * dPvagTerm_dVg;
                dVADIBL_dVb = dVADIBL_dVb * PvagTerm + VADIBL * dPvagTerm_dVb;
                dVADIBL_dVd = dVADIBL_dVd * PvagTerm + VADIBL * dPvagTerm_dVd;
                VADIBL *= PvagTerm;
            }
            else
            {
                VADIBL = Transistor.MAX_EXP;
                dVADIBL_dVd = dVADIBL_dVg = dVADIBL_dVb = 0.0;
            }

            /* Calculate Va */
            Va = Vasat + VACLM;
            dVa_dVg = dVasat_dVg + dVACLM_dVg;
            dVa_dVb = dVasat_dVb + dVACLM_dVb;
            dVa_dVd = dVasat_dVd + dVACLM_dVd;

            /* Calculate VADITS */
            T0 = bsim4.pParam.BSIM4pditsd * Vds;
            if (T0 > Transistor.EXP_THRESHOLD)
            {
                T1 = Transistor.MAX_EXP;
                dT1_dVd = 0;
            }
            else
            {
                T1 = Math.Exp(T0);
                dT1_dVd = T1 * bsim4.pParam.BSIM4pditsd;
            }

            if (bsim4.pParam.BSIM4pdits > Transistor.MIN_EXP)
            {
                T2 = 1.0 + model.BSIM4pditsl * Leff;
                VADITS = (1.0 + T2 * T1) / bsim4.pParam.BSIM4pdits;
                dVADITS_dVg = VADITS * dFP_dVg;
                dVADITS_dVd = FP * T2 * dT1_dVd / bsim4.pParam.BSIM4pdits;
                VADITS *= FP;
            }
            else
            {
                VADITS = Transistor.MAX_EXP;
                dVADITS_dVg = dVADITS_dVd = 0;
            }

            /* Calculate VASCBE */
            if ((bsim4.pParam.BSIM4pscbe2 > 0.0) && (bsim4.pParam.BSIM4pscbe1 >= 0.0))
            /* 4.6.2 */
            {
                if (diffVds > bsim4.pParam.BSIM4pscbe1 * bsim4.pParam.BSIM4litl / Transistor.EXP_THRESHOLD)
                {
                    T0 = bsim4.pParam.BSIM4pscbe1 * bsim4.pParam.BSIM4litl / diffVds;
                    VASCBE = Leff * Math.Exp(T0) / bsim4.pParam.BSIM4pscbe2;
                    T1 = T0 * VASCBE / diffVds;
                    dVASCBE_dVg = T1 * dVdseff_dVg;
                    dVASCBE_dVd = -T1 * (1.0 - dVdseff_dVd);
                    dVASCBE_dVb = T1 * dVdseff_dVb;
                }
                else
                {
                    VASCBE = Transistor.MAX_EXP * Leff / bsim4.pParam.BSIM4pscbe2;
                    dVASCBE_dVg = dVASCBE_dVd = dVASCBE_dVb = 0.0;
                }
            }
            else
            {
                VASCBE = Transistor.MAX_EXP;
                dVASCBE_dVg = dVASCBE_dVd = dVASCBE_dVb = 0.0;
            }

            /* Add DIBL to Ids */
            T9 = diffVds / VADIBL;
            T0 = 1.0 + T9;
            Idsa = Idl * T0;
            dIdsa_dVg = T0 * dIdl_dVg - Idl * (dVdseff_dVg + T9 * dVADIBL_dVg) / VADIBL;
            dIdsa_dVd = T0 * dIdl_dVd + Idl * (1.0 - dVdseff_dVd - T9 * dVADIBL_dVd) / VADIBL;
            dIdsa_dVb = T0 * dIdl_dVb - Idl * (dVdseff_dVb + T9 * dVADIBL_dVb) / VADIBL;

            /* Add DITS to Ids */
            T9 = diffVds / VADITS;
            T0 = 1.0 + T9;
            dIdsa_dVg = T0 * dIdsa_dVg - Idsa * (dVdseff_dVg + T9 * dVADITS_dVg) / VADITS;
            dIdsa_dVd = T0 * dIdsa_dVd + Idsa * (1.0 - dVdseff_dVd - T9 * dVADITS_dVd) / VADITS;
            dIdsa_dVb = T0 * dIdsa_dVb - Idsa * dVdseff_dVb / VADITS;
            Idsa *= T0;

            /* Add CLM to Ids */
            T0 = Math.Log(Va / Vasat);
            dT0_dVg = dVa_dVg / Va - dVasat_dVg / Vasat;
            dT0_dVb = dVa_dVb / Va - dVasat_dVb / Vasat;
            dT0_dVd = dVa_dVd / Va - dVasat_dVd / Vasat;
            T1 = T0 / Cclm;
            T9 = 1.0 + T1;
            dT9_dVg = (dT0_dVg - T1 * dCclm_dVg) / Cclm;
            dT9_dVb = (dT0_dVb - T1 * dCclm_dVb) / Cclm;
            dT9_dVd = (dT0_dVd - T1 * dCclm_dVd) / Cclm;

            dIdsa_dVg = dIdsa_dVg * T9 + Idsa * dT9_dVg;
            dIdsa_dVb = dIdsa_dVb * T9 + Idsa * dT9_dVb;
            dIdsa_dVd = dIdsa_dVd * T9 + Idsa * dT9_dVd;
            Idsa *= T9;

            /* Substrate current begins */
            tmp = bsim4.pParam.BSIM4alpha0 + bsim4.pParam.BSIM4alpha1 * Leff;
            if ((tmp <= 0.0) || (bsim4.pParam.BSIM4beta0 <= 0.0))
            {
                Isub = Gbd = Gbb = Gbg = 0.0;
            }
            else
            {
                T2 = tmp / Leff;
                if (diffVds > bsim4.pParam.BSIM4beta0 / Transistor.EXP_THRESHOLD)
                {
                    T0 = -bsim4.pParam.BSIM4beta0 / diffVds;
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
                T4 = Idsa * Vdseff;
                Isub = T1 * T4;
                Gbg = T1 * (dIdsa_dVg * Vdseff + Idsa * dVdseff_dVg) + T4 * dT1_dVg;
                Gbd = T1 * (dIdsa_dVd * Vdseff + Idsa * dVdseff_dVd) + T4 * dT1_dVd;
                Gbb = T1 * (dIdsa_dVb * Vdseff + Idsa * dVdseff_dVb) + T4 * dT1_dVb;

                Gbd += Gbg * dVgsteff_dVd;
                Gbb += Gbg * dVgsteff_dVb;
                Gbg *= dVgsteff_dVg;
                Gbb *= dVbseff_dVb;
            }
            bsim4.BSIM4csub = Isub;
            bsim4.BSIM4gbbs = Gbb;
            bsim4.BSIM4gbgs = Gbg;
            bsim4.BSIM4gbds = Gbd;

            /* Add SCBE to Ids */
            T9 = diffVds / VASCBE;
            T0 = 1.0 + T9;
            Ids = Idsa * T0;

            Gm = T0 * dIdsa_dVg - Idsa * (dVdseff_dVg + T9 * dVASCBE_dVg) / VASCBE;
            Gds = T0 * dIdsa_dVd + Idsa * (1.0 - dVdseff_dVd - T9 * dVASCBE_dVd) / VASCBE;
            Gmb = T0 * dIdsa_dVb - Idsa * (dVdseff_dVb + T9 * dVASCBE_dVb) / VASCBE;

            tmp1 = Gds + Gm * dVgsteff_dVd;
            tmp2 = Gmb + Gm * dVgsteff_dVb;
            tmp3 = Gm;

            Gm = (Ids * dVdseff_dVg + Vdseff * tmp3) * dVgsteff_dVg;
            Gds = Ids * (dVdseff_dVd + dVdseff_dVg * dVgsteff_dVd) + Vdseff * tmp1;
            Gmb = (Ids * (dVdseff_dVb + dVdseff_dVg * dVgsteff_dVb) + Vdseff * tmp2) * dVbseff_dVb;

            cdrain = Ids * Vdseff;

            /* Source End Velocity Limit */
            if ((model.BSIM4vtl.Given) && (model.BSIM4vtl > 0.0))
            {
                T12 = 1.0 / Leff / CoxeffWovL;
                T11 = T12 / Vgsteff;
                T10 = -T11 / Vgsteff;
                vs = cdrain * T11; /* vs */
                dvs_dVg = Gm * T11 + cdrain * T10 * dVgsteff_dVg;
                dvs_dVd = Gds * T11 + cdrain * T10 * dVgsteff_dVd;
                dvs_dVb = Gmb * T11 + cdrain * T10 * dVgsteff_dVb;
                T0 = 2 * Transistor.MM;
                T1 = vs / (bsim4.pParam.BSIM4vtl * bsim4.pParam.BSIM4tfactor);
                if (T1 > 0.0)
                {
                    T2 = 1.0 + Math.Exp(T0 * Math.Log(T1));
                    T3 = (T2 - 1.0) * T0 / vs;
                    Fsevl = 1.0 / Math.Exp(Math.Log(T2) / T0);
                    dT2_dVg = T3 * dvs_dVg;
                    dT2_dVd = T3 * dvs_dVd;
                    dT2_dVb = T3 * dvs_dVb;
                    T4 = -1.0 / T0 * Fsevl / T2;
                    dFsevl_dVg = T4 * dT2_dVg;
                    dFsevl_dVd = T4 * dT2_dVd;
                    dFsevl_dVb = T4 * dT2_dVb;
                }
                else
                {
                    Fsevl = 1.0;
                    dFsevl_dVg = 0.0;
                    dFsevl_dVd = 0.0;
                    dFsevl_dVb = 0.0;
                }
                Gm *= Fsevl;
                Gm += cdrain * dFsevl_dVg;
                Gmb *= Fsevl;
                Gmb += cdrain * dFsevl_dVb;
                Gds *= Fsevl;
                Gds += cdrain * dFsevl_dVd;

                cdrain *= Fsevl;
            }

            bsim4.BSIM4gds = Gds;
            bsim4.BSIM4gm = Gm;
            bsim4.BSIM4gmbs = Gmb;
            bsim4.BSIM4IdovVds = Ids;
            if (bsim4.BSIM4IdovVds <= 1.0e-9)
                bsim4.BSIM4IdovVds = 1.0e-9;

            /* Calculate Rg */
            if ((bsim4.BSIM4rgateMod > 1) || (bsim4.BSIM4trnqsMod != 0) || (bsim4.BSIM4acnqsMod != 0))
            {
                T9 = bsim4.pParam.BSIM4xrcrg2 * model.BSIM4vtm;
                T0 = T9 * beta;
                dT0_dVd = (dbeta_dVd + dbeta_dVg * dVgsteff_dVd) * T9;
                dT0_dVb = (dbeta_dVb + dbeta_dVg * dVgsteff_dVb) * T9;
                dT0_dVg = dbeta_dVg * T9;

                bsim4.BSIM4gcrg = bsim4.pParam.BSIM4xrcrg1 * (T0 + Ids);
                bsim4.BSIM4gcrgd = bsim4.pParam.BSIM4xrcrg1 * (dT0_dVd + tmp1);
                bsim4.BSIM4gcrgb = bsim4.pParam.BSIM4xrcrg1 * (dT0_dVb + tmp2) * dVbseff_dVb;
                bsim4.BSIM4gcrgg = bsim4.pParam.BSIM4xrcrg1 * (dT0_dVg + tmp3) * dVgsteff_dVg;

                if (bsim4.BSIM4nf != 1.0)
                {
                    bsim4.BSIM4gcrg *= bsim4.BSIM4nf;
                    bsim4.BSIM4gcrgg *= bsim4.BSIM4nf;
                    bsim4.BSIM4gcrgd *= bsim4.BSIM4nf;
                    bsim4.BSIM4gcrgb *= bsim4.BSIM4nf;
                }

                if (bsim4.BSIM4rgateMod.Value == 2)
                {
                    T10 = bsim4.BSIM4grgeltd * bsim4.BSIM4grgeltd;
                    T11 = bsim4.BSIM4grgeltd + bsim4.BSIM4gcrg;
                    bsim4.BSIM4gcrg = bsim4.BSIM4grgeltd * bsim4.BSIM4gcrg / T11;
                    T12 = T10 / T11 / T11;
                    bsim4.BSIM4gcrgg *= T12;
                    bsim4.BSIM4gcrgd *= T12;
                    bsim4.BSIM4gcrgb *= T12;
                }
                bsim4.BSIM4gcrgs = -(bsim4.BSIM4gcrgg + bsim4.BSIM4gcrgd + bsim4.BSIM4gcrgb);
            }

            /* Calculate bias - dependent external S / D resistance */
            if (model.BSIM4rdsMod != 0)
            {
                /* Rs(V) */
                T0 = vgs - bsim4.pParam.BSIM4vfbsd;
                T1 = Math.Sqrt(T0 * T0 + 1.0e-4);
                vgs_eff = 0.5 * (T0 + T1);
                dvgs_eff_dvg = vgs_eff / T1;

                T0 = 1.0 + bsim4.pParam.BSIM4prwg * vgs_eff;
                dT0_dvg = -bsim4.pParam.BSIM4prwg / T0 / T0 * dvgs_eff_dvg;
                T1 = -bsim4.pParam.BSIM4prwb * vbs;
                dT1_dvb = -bsim4.pParam.BSIM4prwb;

                T2 = 1.0 / T0 + T1;
                T3 = T2 + Math.Sqrt(T2 * T2 + 0.01);
                dT3_dvg = T3 / (T3 - T2);
                dT3_dvb = dT3_dvg * dT1_dvb;
                dT3_dvg *= dT0_dvg;

                T4 = bsim4.pParam.BSIM4rs0 * 0.5;
                Rs = bsim4.pParam.BSIM4rswmin + T3 * T4;
                dRs_dvg = T4 * dT3_dvg;
                dRs_dvb = T4 * dT3_dvb;

                T0 = 1.0 + bsim4.BSIM4sourceConductance * Rs;
                bsim4.BSIM4gstot = bsim4.BSIM4sourceConductance / T0;
                T0 = -bsim4.BSIM4gstot * bsim4.BSIM4gstot;
                dgstot_dvd = 0.0; /* place holder */
                dgstot_dvg = T0 * dRs_dvg;
                dgstot_dvb = T0 * dRs_dvb;
                dgstot_dvs = -(dgstot_dvg + dgstot_dvb + dgstot_dvd);

                /* Rd(V) */
                T0 = vgd - bsim4.pParam.BSIM4vfbsd;
                T1 = Math.Sqrt(T0 * T0 + 1.0e-4);
                vgd_eff = 0.5 * (T0 + T1);
                dvgd_eff_dvg = vgd_eff / T1;

                T0 = 1.0 + bsim4.pParam.BSIM4prwg * vgd_eff;
                dT0_dvg = -bsim4.pParam.BSIM4prwg / T0 / T0 * dvgd_eff_dvg;
                T1 = -bsim4.pParam.BSIM4prwb * vbd;
                dT1_dvb = -bsim4.pParam.BSIM4prwb;

                T2 = 1.0 / T0 + T1;
                T3 = T2 + Math.Sqrt(T2 * T2 + 0.01);
                dT3_dvg = T3 / (T3 - T2);
                dT3_dvb = dT3_dvg * dT1_dvb;
                dT3_dvg *= dT0_dvg;

                T4 = bsim4.pParam.BSIM4rd0 * 0.5;
                Rd = bsim4.pParam.BSIM4rdwmin + T3 * T4;
                dRd_dvg = T4 * dT3_dvg;
                dRd_dvb = T4 * dT3_dvb;

                T0 = 1.0 + bsim4.BSIM4drainConductance * Rd;
                bsim4.BSIM4gdtot = bsim4.BSIM4drainConductance / T0;
                T0 = -bsim4.BSIM4gdtot * bsim4.BSIM4gdtot;
                dgdtot_dvs = 0.0;
                dgdtot_dvg = T0 * dRd_dvg;
                dgdtot_dvb = T0 * dRd_dvb;
                dgdtot_dvd = -(dgdtot_dvg + dgdtot_dvb + dgdtot_dvs);

                bsim4.BSIM4gstotd = vses * dgstot_dvd;
                bsim4.BSIM4gstotg = vses * dgstot_dvg;
                bsim4.BSIM4gstots = vses * dgstot_dvs;
                bsim4.BSIM4gstotb = vses * dgstot_dvb;

                T2 = vdes - vds;
                bsim4.BSIM4gdtotd = T2 * dgdtot_dvd;
                bsim4.BSIM4gdtotg = T2 * dgdtot_dvg;
                bsim4.BSIM4gdtots = T2 * dgdtot_dvs;
                bsim4.BSIM4gdtotb = T2 * dgdtot_dvb;
            }
            else /* WDLiu: for bypass */
            {
                bsim4.BSIM4gstot = bsim4.BSIM4gstotd = bsim4.BSIM4gstotg = 0.0;
                bsim4.BSIM4gstots = bsim4.BSIM4gstotb = 0.0;
                bsim4.BSIM4gdtot = bsim4.BSIM4gdtotd = bsim4.BSIM4gdtotg = 0.0;
                bsim4.BSIM4gdtots = bsim4.BSIM4gdtotb = 0.0;
            }

            /* GIDL / GISL Models */

            if (model.BSIM4mtrlMod.Value == 0)
                T0 = 3.0 * toxe;
            else
                T0 = model.BSIM4epsrsub * toxe / epsrox;

            /* Calculate GIDL current */

            vgs_eff = bsim4.BSIM4vgs_eff;
            dvgs_eff_dvg = bsim4.BSIM4dvgs_eff_dvg;
            vgd_eff = bsim4.BSIM4vgd_eff;
            dvgd_eff_dvg = bsim4.BSIM4dvgd_eff_dvg;

            if (model.BSIM4gidlMod.Value == 0)
            {
                if (model.BSIM4mtrlMod.Value == 0)
                    T1 = (vds - vgs_eff - bsim4.pParam.BSIM4egidl) / T0;
                else
                    T1 = (vds - vgs_eff - bsim4.pParam.BSIM4egidl + bsim4.pParam.BSIM4vfbsd) / T0;

                if ((bsim4.pParam.BSIM4agidl <= 0.0) || (bsim4.pParam.BSIM4bgidl <= 0.0) || (T1 <= 0.0) || (bsim4.pParam.BSIM4cgidl <= 0.0) || (vbd > 0.0))
                    Igidl = Ggidld = Ggidlg = Ggidlb = 0.0;
                else
                {
                    dT1_dVd = 1.0 / T0;
                    dT1_dVg = -dvgs_eff_dvg * dT1_dVd;
                    T2 = bsim4.pParam.BSIM4bgidl / T1;
                    if (T2 < 100.0)
                    {
                        Igidl = bsim4.pParam.BSIM4agidl * bsim4.pParam.BSIM4weffCJ * T1 * Math.Exp(-T2);
                        T3 = Igidl * (1.0 + T2) / T1;
                        Ggidld = T3 * dT1_dVd;
                        Ggidlg = T3 * dT1_dVg;
                    }
                    else
                    {
                        Igidl = bsim4.pParam.BSIM4agidl * bsim4.pParam.BSIM4weffCJ * 3.720075976e-44;
                        Ggidld = Igidl * dT1_dVd;
                        Ggidlg = Igidl * dT1_dVg;
                        Igidl *= T1;
                    }

                    T4 = vbd * vbd;
                    T5 = -vbd * T4;
                    T6 = bsim4.pParam.BSIM4cgidl + T5;
                    T7 = T5 / T6;
                    T8 = 3.0 * bsim4.pParam.BSIM4cgidl * T4 / T6 / T6;
                    Ggidld = Ggidld * T7 + Igidl * T8;
                    Ggidlg = Ggidlg * T7;
                    Ggidlb = -Igidl * T8;
                    Igidl *= T7;
                }
                bsim4.BSIM4Igidl = Igidl;
                bsim4.BSIM4ggidld = Ggidld;
                bsim4.BSIM4ggidlg = Ggidlg;
                bsim4.BSIM4ggidlb = Ggidlb;
                /* Calculate GISL current */

                if (model.BSIM4mtrlMod.Value == 0)
                    T1 = (-vds - vgd_eff - bsim4.pParam.BSIM4egisl) / T0;
                else
                    T1 = (-vds - vgd_eff - bsim4.pParam.BSIM4egisl + bsim4.pParam.BSIM4vfbsd) / T0;

                if ((bsim4.pParam.BSIM4agisl <= 0.0) || (bsim4.pParam.BSIM4bgisl <= 0.0) || (T1 <= 0.0) || (bsim4.pParam.BSIM4cgisl <= 0.0) || (vbs > 0.0))
                    Igisl = Ggisls = Ggislg = Ggislb = 0.0;
                else
                {
                    dT1_dVd = 1.0 / T0;
                    dT1_dVg = -dvgd_eff_dvg * dT1_dVd;
                    T2 = bsim4.pParam.BSIM4bgisl / T1;
                    if (T2 < 100.0)
                    {
                        Igisl = bsim4.pParam.BSIM4agisl * bsim4.pParam.BSIM4weffCJ * T1 * Math.Exp(-T2);
                        T3 = Igisl * (1.0 + T2) / T1;
                        Ggisls = T3 * dT1_dVd;
                        Ggislg = T3 * dT1_dVg;
                    }
                    else
                    {
                        Igisl = bsim4.pParam.BSIM4agisl * bsim4.pParam.BSIM4weffCJ * 3.720075976e-44;
                        Ggisls = Igisl * dT1_dVd;
                        Ggislg = Igisl * dT1_dVg;
                        Igisl *= T1;
                    }

                    T4 = vbs * vbs;
                    T5 = -vbs * T4;
                    T6 = bsim4.pParam.BSIM4cgisl + T5;
                    T7 = T5 / T6;
                    T8 = 3.0 * bsim4.pParam.BSIM4cgisl * T4 / T6 / T6;
                    Ggisls = Ggisls * T7 + Igisl * T8;
                    Ggislg = Ggislg * T7;
                    Ggislb = -Igisl * T8;
                    Igisl *= T7;
                }
                bsim4.BSIM4Igisl = Igisl;
                bsim4.BSIM4ggisls = Ggisls;
                bsim4.BSIM4ggislg = Ggislg;
                bsim4.BSIM4ggislb = Ggislb;
            }
            else
            {
                /* v4.7 New Gidl / GISL model */

                /* GISL */
                if (model.BSIM4mtrlMod.Value == 0)
                    T1 = (-vds - bsim4.pParam.BSIM4rgisl * vgd_eff - bsim4.pParam.BSIM4egisl) / T0;
                else
                    T1 = (-vds - bsim4.pParam.BSIM4rgisl * vgd_eff - bsim4.pParam.BSIM4egisl + bsim4.pParam.BSIM4vfbsd) / T0;

                if ((bsim4.pParam.BSIM4agisl <= 0.0) || (bsim4.pParam.BSIM4bgisl <= 0.0) || (T1 <= 0.0) || (bsim4.pParam.BSIM4cgisl < 0.0))
                    Igisl = Ggisls = Ggislg = Ggislb = 0.0;
                else
                {
                    dT1_dVd = 1 / T0;
                    dT1_dVg = -bsim4.pParam.BSIM4rgisl * dT1_dVd * dvgd_eff_dvg;
                    T2 = bsim4.pParam.BSIM4bgisl / T1;
                    if (T2 < Transistor.EXPL_THRESHOLD)
                    {
                        Igisl = bsim4.pParam.BSIM4weffCJ * bsim4.pParam.BSIM4agisl * T1 * Math.Exp(-T2);
                        T3 = Igisl / T1 * (T2 + 1);
                        Ggisls = T3 * dT1_dVd;
                        Ggislg = T3 * dT1_dVg;
                    }
                    else
                    {
                        T3 = bsim4.pParam.BSIM4weffCJ * bsim4.pParam.BSIM4agisl * Transistor.MIN_EXPL;
                        Igisl = T3 * T1;
                        Ggisls = T3 * dT1_dVd;
                        Ggislg = T3 * dT1_dVg;

                    }
                    T4 = vbs - bsim4.pParam.BSIM4fgisl;

                    if (T4 == 0)
                        T5 = Transistor.EXPL_THRESHOLD;
                    else
                        T5 = bsim4.pParam.BSIM4kgisl / T4;
                    if (T5 < Transistor.EXPL_THRESHOLD)
                    {
                        T6 = Math.Exp(T5);
                        Ggislb = -Igisl * T6 * T5 / T4;
                    }
                    else
                    {
                        T6 = Transistor.MAX_EXPL;
                        Ggislb = 0.0;
                    }
                    Ggisls *= T6;
                    Ggislg *= T6;
                    Igisl *= T6;

                }
                bsim4.BSIM4Igisl = Igisl;
                bsim4.BSIM4ggisls = Ggisls;
                bsim4.BSIM4ggislg = Ggislg;
                bsim4.BSIM4ggislb = Ggislb;
                /* End of GISL */

                /* GIDL */
                if (model.BSIM4mtrlMod.Value == 0)
                    T1 = (vds - bsim4.pParam.BSIM4rgidl * vgs_eff - bsim4.pParam.BSIM4egidl) / T0;
                else
                    T1 = (vds - bsim4.pParam.BSIM4rgidl * vgs_eff - bsim4.pParam.BSIM4egidl + bsim4.pParam.BSIM4vfbsd) / T0;

                if ((bsim4.pParam.BSIM4agidl <= 0.0) || (bsim4.pParam.BSIM4bgidl <= 0.0) || (T1 <= 0.0) || (bsim4.pParam.BSIM4cgidl < 0.0))
                    Igidl = Ggidld = Ggidlg = Ggidlb = 0.0;
                else
                {
                    dT1_dVd = 1 / T0;
                    dT1_dVg = -bsim4.pParam.BSIM4rgidl * dT1_dVd * dvgs_eff_dvg;
                    T2 = bsim4.pParam.BSIM4bgidl / T1;
                    if (T2 < Transistor.EXPL_THRESHOLD)
                    {
                        Igidl = bsim4.pParam.BSIM4weffCJ * bsim4.pParam.BSIM4agidl * T1 * Math.Exp(-T2);
                        T3 = Igidl / T1 * (T2 + 1);
                        Ggidld = T3 * dT1_dVd;
                        Ggidlg = T3 * dT1_dVg;

                    }
                    else
                    {
                        T3 = bsim4.pParam.BSIM4weffCJ * bsim4.pParam.BSIM4agidl * Transistor.MIN_EXPL;
                        Igidl = T3 * T1;
                        Ggidld = T3 * dT1_dVd;
                        Ggidlg = T3 * dT1_dVg;
                    }
                    T4 = vbd - bsim4.pParam.BSIM4fgidl;
                    if (T4 == 0)
                        T5 = Transistor.EXPL_THRESHOLD;
                    else
                        T5 = bsim4.pParam.BSIM4kgidl / T4;
                    if (T5 < Transistor.EXPL_THRESHOLD)
                    {
                        T6 = Math.Exp(T5);
                        Ggidlb = -Igidl * T6 * T5 / T4;
                    }
                    else
                    {
                        T6 = Transistor.MAX_EXPL;
                        Ggidlb = 0.0;
                    }
                    Ggidld *= T6;
                    Ggidlg *= T6;
                    Igidl *= T6;
                }
                bsim4.BSIM4Igidl = Igidl;
                bsim4.BSIM4ggidld = Ggidld;
                bsim4.BSIM4ggidlg = Ggidlg;
                bsim4.BSIM4ggidlb = Ggidlb;
                /* End of New GIDL */
            }
            /* End of Gidl */

            /* Calculate gate tunneling current */
            if ((model.BSIM4igcMod != 0) || (model.BSIM4igbMod != 0))
            {
                Vfb = bsim4.BSIM4vfbzb;
                V3 = Vfb - Vgs_eff + Vbseff - Transistor.DELTA_3;
                if (Vfb <= 0.0)
                    T0 = Math.Sqrt(V3 * V3 - 4.0 * Transistor.DELTA_3 * Vfb);
                else
                    T0 = Math.Sqrt(V3 * V3 + 4.0 * Transistor.DELTA_3 * Vfb);
                T1 = 0.5 * (1.0 + V3 / T0);
                Vfbeff = Vfb - 0.5 * (V3 + T0);
                dVfbeff_dVg = T1 * dVgs_eff_dVg;
                dVfbeff_dVb = -T1; /* WDLiu: - No surprise? No. - Good! */

                Voxacc = Vfb - Vfbeff;
                dVoxacc_dVg = -dVfbeff_dVg;
                dVoxacc_dVb = -dVfbeff_dVb;
                if (Voxacc < 0.0)
                    /* WDLiu: Avoiding numerical instability. */
                    Voxacc = dVoxacc_dVg = dVoxacc_dVb = 0.0;

                T0 = 0.5 * bsim4.pParam.BSIM4k1ox;
                T3 = Vgs_eff - Vfbeff - Vbseff - Vgsteff;
                if (bsim4.pParam.BSIM4k1ox == 0.0)
                    Voxdepinv = dVoxdepinv_dVg = dVoxdepinv_dVd = dVoxdepinv_dVb = 0.0;
                else if (T3 < 0.0)
                {
                    Voxdepinv = -T3;
                    dVoxdepinv_dVg = -dVgs_eff_dVg + dVfbeff_dVg + dVgsteff_dVg;
                    dVoxdepinv_dVd = dVgsteff_dVd;
                    dVoxdepinv_dVb = dVfbeff_dVb + 1.0 + dVgsteff_dVb;
                }
                else
                {
                    T1 = Math.Sqrt(T0 * T0 + T3);
                    T2 = T0 / T1;
                    Voxdepinv = bsim4.pParam.BSIM4k1ox * (T1 - T0);
                    dVoxdepinv_dVg = T2 * (dVgs_eff_dVg - dVfbeff_dVg - dVgsteff_dVg);
                    dVoxdepinv_dVd = -T2 * dVgsteff_dVd;
                    dVoxdepinv_dVb = -T2 * (dVfbeff_dVb + 1.0 + dVgsteff_dVb);
                }

                Voxdepinv += Vgsteff;
                dVoxdepinv_dVg += dVgsteff_dVg;
                dVoxdepinv_dVd += dVgsteff_dVd;
                dVoxdepinv_dVb += dVgsteff_dVb;
            }

            if (model.BSIM4tempMod < 2)
                tmp = Vtm;
            else /* model.BSIM4tempMod.Value = 2, 3 */ tmp = Vtm0;
            if (model.BSIM4igcMod != 0)
            {
                T0 = tmp * bsim4.pParam.BSIM4nigc;
                if (model.BSIM4igcMod.Value == 1)
                {
                    VxNVt = (Vgs_eff - model.BSIM4type * bsim4.BSIM4vth0) / T0;
                    if (VxNVt > Transistor.EXP_THRESHOLD)
                    {
                        Vaux = Vgs_eff - model.BSIM4type * bsim4.BSIM4vth0;
                        dVaux_dVg = dVgs_eff_dVg;
                        dVaux_dVd = 0.0;
                        dVaux_dVb = 0.0;
                    }
                }
                else if (model.BSIM4igcMod.Value == 2)
                {
                    VxNVt = (Vgs_eff - bsim4.BSIM4von) / T0;
                    if (VxNVt > Transistor.EXP_THRESHOLD)
                    {
                        Vaux = Vgs_eff - bsim4.BSIM4von;
                        dVaux_dVg = dVgs_eff_dVg;
                        dVaux_dVd = -dVth_dVd;
                        dVaux_dVb = -dVth_dVb;
                    }
                }
                if (VxNVt < -Transistor.EXP_THRESHOLD)
                {
                    Vaux = T0 * Math.Log(1.0 + Transistor.MIN_EXP);
                    dVaux_dVg = dVaux_dVd = dVaux_dVb = 0.0;
                }
                else if ((VxNVt >= -Transistor.EXP_THRESHOLD) && (VxNVt <= Transistor.EXP_THRESHOLD))
                {
                    ExpVxNVt = Math.Exp(VxNVt);
                    Vaux = T0 * Math.Log(1.0 + ExpVxNVt);
                    dVaux_dVg = ExpVxNVt / (1.0 + ExpVxNVt);
                    if (model.BSIM4igcMod.Value == 1)
                    {
                        dVaux_dVd = 0.0;
                        dVaux_dVb = 0.0;
                    }
                    else if (model.BSIM4igcMod.Value == 2)
                    {
                        dVaux_dVd = -dVaux_dVg * dVth_dVd; /* Synopsys 08 / 30 / 2013 modify */
                        dVaux_dVb = -dVaux_dVg * dVth_dVb; /* Synopsys 08 / 30 / 2013 modify */
                    }
                    dVaux_dVg *= dVgs_eff_dVg;
                }

                T2 = Vgs_eff * Vaux;
                dT2_dVg = dVgs_eff_dVg * Vaux + Vgs_eff * dVaux_dVg;
                dT2_dVd = Vgs_eff * dVaux_dVd;
                dT2_dVb = Vgs_eff * dVaux_dVb;

                T11 = bsim4.pParam.BSIM4Aechvb;
                T12 = bsim4.pParam.BSIM4Bechvb;
                T3 = bsim4.pParam.BSIM4aigc * bsim4.pParam.BSIM4cigc - bsim4.pParam.BSIM4bigc;
                T4 = bsim4.pParam.BSIM4bigc * bsim4.pParam.BSIM4cigc;
                T5 = T12 * (bsim4.pParam.BSIM4aigc + T3 * Voxdepinv - T4 * Voxdepinv * Voxdepinv);

                if (T5 > Transistor.EXP_THRESHOLD)
                {
                    T6 = Transistor.MAX_EXP;
                    dT6_dVg = dT6_dVd = dT6_dVb = 0.0;
                }
                else if (T5 < -Transistor.EXP_THRESHOLD)
                {
                    T6 = Transistor.MIN_EXP;
                    dT6_dVg = dT6_dVd = dT6_dVb = 0.0;
                }
                else
                {
                    T6 = Math.Exp(T5);
                    dT6_dVg = T6 * T12 * (T3 - 2.0 * T4 * Voxdepinv);
                    dT6_dVd = dT6_dVg * dVoxdepinv_dVd;
                    dT6_dVb = dT6_dVg * dVoxdepinv_dVb;
                    dT6_dVg *= dVoxdepinv_dVg;
                }

                Igc = T11 * T2 * T6;
                dIgc_dVg = T11 * (T2 * dT6_dVg + T6 * dT2_dVg);
                dIgc_dVd = T11 * (T2 * dT6_dVd + T6 * dT2_dVd);
                dIgc_dVb = T11 * (T2 * dT6_dVb + T6 * dT2_dVb);

                if (model.BSIM4pigcd.Given)
                {
                    Pigcd = bsim4.pParam.BSIM4pigcd;
                    dPigcd_dVg = dPigcd_dVd = dPigcd_dVb = 0.0;
                }
                else
                {
                    /* T11 = bsim4.pParam.BSIM4Bechvb * toxe; v4.7 */
                    T11 = -bsim4.pParam.BSIM4Bechvb;
                    T12 = Vgsteff + 1.0e-20;
                    T13 = T11 / T12 / T12;
                    T14 = -T13 / T12;
                    Pigcd = T13 * (1.0 - 0.5 * Vdseff / T12);
                    dPigcd_dVg = T14 * (2.0 + 0.5 * (dVdseff_dVg - 3.0 * Vdseff / T12));
                    dPigcd_dVd = 0.5 * T14 * dVdseff_dVd;
                    dPigcd_dVb = 0.5 * T14 * dVdseff_dVb;
                }

                T7 = -Pigcd * Vdseff; /* bugfix */
                dT7_dVg = -Vdseff * dPigcd_dVg - Pigcd * dVdseff_dVg;
                dT7_dVd = -Vdseff * dPigcd_dVd - Pigcd * dVdseff_dVd + dT7_dVg * dVgsteff_dVd;
                dT7_dVb = -Vdseff * dPigcd_dVb - Pigcd * dVdseff_dVb + dT7_dVg * dVgsteff_dVb;
                dT7_dVg *= dVgsteff_dVg;
                /* dT7_dVb *= dVbseff_dVb;  /* Synopsys, 2013 / 08 / 30 */
                T8 = T7 * T7 + 2.0e-4;
                dT8_dVg = 2.0 * T7;
                dT8_dVd = dT8_dVg * dT7_dVd;
                dT8_dVb = dT8_dVg * dT7_dVb;
                dT8_dVg *= dT7_dVg;

                if (T7 > Transistor.EXP_THRESHOLD)
                {
                    T9 = Transistor.MAX_EXP;
                    dT9_dVg = dT9_dVd = dT9_dVb = 0.0;
                }
                else if (T7 < -Transistor.EXP_THRESHOLD)
                {
                    T9 = Transistor.MIN_EXP;
                    dT9_dVg = dT9_dVd = dT9_dVb = 0.0;
                }
                else
                {
                    T9 = Math.Exp(T7);
                    dT9_dVg = T9 * dT7_dVg;
                    dT9_dVd = T9 * dT7_dVd;
                    dT9_dVb = T9 * dT7_dVb;
                }

                T0 = T8 * T8;
                T1 = T9 - 1.0 + 1.0e-4;
                T10 = (T1 - T7) / T8;
                dT10_dVg = (dT9_dVg - dT7_dVg - T10 * dT8_dVg) / T8;
                dT10_dVd = (dT9_dVd - dT7_dVd - T10 * dT8_dVd) / T8;
                dT10_dVb = (dT9_dVb - dT7_dVb - T10 * dT8_dVb) / T8;

                Igcs = Igc * T10;
                dIgcs_dVg = dIgc_dVg * T10 + Igc * dT10_dVg;
                dIgcs_dVd = dIgc_dVd * T10 + Igc * dT10_dVd;
                dIgcs_dVb = dIgc_dVb * T10 + Igc * dT10_dVb;

                T1 = T9 - 1.0 - 1.0e-4;
                T10 = (T7 * T9 - T1) / T8;
                dT10_dVg = (dT7_dVg * T9 + (T7 - 1.0) * dT9_dVg - T10 * dT8_dVg) / T8;
                dT10_dVd = (dT7_dVd * T9 + (T7 - 1.0) * dT9_dVd - T10 * dT8_dVd) / T8;
                dT10_dVb = (dT7_dVb * T9 + (T7 - 1.0) * dT9_dVb - T10 * dT8_dVb) / T8;
                Igcd = Igc * T10;
                dIgcd_dVg = dIgc_dVg * T10 + Igc * dT10_dVg;
                dIgcd_dVd = dIgc_dVd * T10 + Igc * dT10_dVd;
                dIgcd_dVb = dIgc_dVb * T10 + Igc * dT10_dVb;

                bsim4.BSIM4Igcs = Igcs;
                bsim4.BSIM4gIgcsg = dIgcs_dVg;
                bsim4.BSIM4gIgcsd = dIgcs_dVd;
                bsim4.BSIM4gIgcsb = dIgcs_dVb * dVbseff_dVb;
                bsim4.BSIM4Igcd = Igcd;
                bsim4.BSIM4gIgcdg = dIgcd_dVg;
                bsim4.BSIM4gIgcdd = dIgcd_dVd;
                bsim4.BSIM4gIgcdb = dIgcd_dVb * dVbseff_dVb;

                T0 = vgs - (bsim4.pParam.BSIM4vfbsd + bsim4.pParam.BSIM4vfbsdoff);
                vgs_eff = Math.Sqrt(T0 * T0 + 1.0e-4);
                dvgs_eff_dvg = T0 / vgs_eff;

                T2 = vgs * vgs_eff;
                dT2_dVg = vgs * dvgs_eff_dvg + vgs_eff;
                T11 = bsim4.pParam.BSIM4AechvbEdgeS;
                T12 = bsim4.pParam.BSIM4BechvbEdge;
                T3 = bsim4.pParam.BSIM4aigs * bsim4.pParam.BSIM4cigs - bsim4.pParam.BSIM4bigs;
                T4 = bsim4.pParam.BSIM4bigs * bsim4.pParam.BSIM4cigs;
                T5 = T12 * (bsim4.pParam.BSIM4aigs + T3 * vgs_eff - T4 * vgs_eff * vgs_eff);
                if (T5 > Transistor.EXP_THRESHOLD)
                {
                    T6 = Transistor.MAX_EXP;
                    dT6_dVg = 0.0;
                }
                else if (T5 < -Transistor.EXP_THRESHOLD)
                {
                    T6 = Transistor.MIN_EXP;
                    dT6_dVg = 0.0;
                }
                else
                {
                    T6 = Math.Exp(T5);
                    dT6_dVg = T6 * T12 * (T3 - 2.0 * T4 * vgs_eff) * dvgs_eff_dvg;
                }
                Igs = T11 * T2 * T6;
                dIgs_dVg = T11 * (T2 * dT6_dVg + T6 * dT2_dVg);
                dIgs_dVs = -dIgs_dVg;

                T0 = vgd - (bsim4.pParam.BSIM4vfbsd + bsim4.pParam.BSIM4vfbsdoff);
                vgd_eff = Math.Sqrt(T0 * T0 + 1.0e-4);
                dvgd_eff_dvg = T0 / vgd_eff;

                T2 = vgd * vgd_eff;
                dT2_dVg = vgd * dvgd_eff_dvg + vgd_eff;
                T11 = bsim4.pParam.BSIM4AechvbEdgeD;
                T3 = bsim4.pParam.BSIM4aigd * bsim4.pParam.BSIM4cigd - bsim4.pParam.BSIM4bigd;
                T4 = bsim4.pParam.BSIM4bigd * bsim4.pParam.BSIM4cigd;
                T5 = T12 * (bsim4.pParam.BSIM4aigd + T3 * vgd_eff - T4 * vgd_eff * vgd_eff);
                if (T5 > Transistor.EXP_THRESHOLD)
                {
                    T6 = Transistor.MAX_EXP;
                    dT6_dVg = 0.0;
                }
                else if (T5 < -Transistor.EXP_THRESHOLD)
                {
                    T6 = Transistor.MIN_EXP;
                    dT6_dVg = 0.0;
                }
                else
                {
                    T6 = Math.Exp(T5);
                    dT6_dVg = T6 * T12 * (T3 - 2.0 * T4 * vgd_eff) * dvgd_eff_dvg;
                }
                Igd = T11 * T2 * T6;
                dIgd_dVg = T11 * (T2 * dT6_dVg + T6 * dT2_dVg);
                dIgd_dVd = -dIgd_dVg;

                bsim4.BSIM4Igs = Igs;
                bsim4.BSIM4gIgsg = dIgs_dVg;
                bsim4.BSIM4gIgss = dIgs_dVs;
                bsim4.BSIM4Igd = Igd;
                bsim4.BSIM4gIgdg = dIgd_dVg;
                bsim4.BSIM4gIgdd = dIgd_dVd;
            }
            else
            {
                bsim4.BSIM4Igcs = bsim4.BSIM4gIgcsg = bsim4.BSIM4gIgcsd = bsim4.BSIM4gIgcsb = 0.0;
                bsim4.BSIM4Igcd = bsim4.BSIM4gIgcdg = bsim4.BSIM4gIgcdd = bsim4.BSIM4gIgcdb = 0.0;
                bsim4.BSIM4Igs = bsim4.BSIM4gIgsg = bsim4.BSIM4gIgss = 0.0;
                bsim4.BSIM4Igd = bsim4.BSIM4gIgdg = bsim4.BSIM4gIgdd = 0.0;
            }

            if (model.BSIM4igbMod != 0)
            {
                T0 = tmp * bsim4.pParam.BSIM4nigbacc;
                T1 = -Vgs_eff + Vbseff + Vfb;
                VxNVt = T1 / T0;
                if (VxNVt > Transistor.EXP_THRESHOLD)
                {
                    Vaux = T1;
                    dVaux_dVg = -dVgs_eff_dVg;
                    dVaux_dVb = 1.0;
                }
                else if (VxNVt < -Transistor.EXP_THRESHOLD)
                {
                    Vaux = T0 * Math.Log(1.0 + Transistor.MIN_EXP);
                    dVaux_dVg = dVaux_dVb = 0.0;
                }
                else
                {
                    ExpVxNVt = Math.Exp(VxNVt);
                    Vaux = T0 * Math.Log(1.0 + ExpVxNVt);
                    dVaux_dVb = ExpVxNVt / (1.0 + ExpVxNVt);
                    dVaux_dVg = -dVaux_dVb * dVgs_eff_dVg;
                }

                T2 = (Vgs_eff - Vbseff) * Vaux;
                dT2_dVg = dVgs_eff_dVg * Vaux + (Vgs_eff - Vbseff) * dVaux_dVg;
                dT2_dVb = -Vaux + (Vgs_eff - Vbseff) * dVaux_dVb;

                T11 = 4.97232e-7 * bsim4.pParam.BSIM4weff * bsim4.pParam.BSIM4leff * bsim4.pParam.BSIM4ToxRatio;
                T12 = -7.45669e11 * toxe;
                T3 = bsim4.pParam.BSIM4aigbacc * bsim4.pParam.BSIM4cigbacc - bsim4.pParam.BSIM4bigbacc;
                T4 = bsim4.pParam.BSIM4bigbacc * bsim4.pParam.BSIM4cigbacc;
                T5 = T12 * (bsim4.pParam.BSIM4aigbacc + T3 * Voxacc - T4 * Voxacc * Voxacc);

                if (T5 > Transistor.EXP_THRESHOLD)
                {
                    T6 = Transistor.MAX_EXP;
                    dT6_dVg = dT6_dVb = 0.0;
                }
                else if (T5 < -Transistor.EXP_THRESHOLD)
                {
                    T6 = Transistor.MIN_EXP;
                    dT6_dVg = dT6_dVb = 0.0;
                }
                else
                {
                    T6 = Math.Exp(T5);
                    dT6_dVg = T6 * T12 * (T3 - 2.0 * T4 * Voxacc);
                    dT6_dVb = dT6_dVg * dVoxacc_dVb;
                    dT6_dVg *= dVoxacc_dVg;
                }

                Igbacc = T11 * T2 * T6;
                dIgbacc_dVg = T11 * (T2 * dT6_dVg + T6 * dT2_dVg);
                dIgbacc_dVb = T11 * (T2 * dT6_dVb + T6 * dT2_dVb);

                T0 = tmp * bsim4.pParam.BSIM4nigbinv;
                T1 = Voxdepinv - bsim4.pParam.BSIM4eigbinv;
                VxNVt = T1 / T0;
                if (VxNVt > Transistor.EXP_THRESHOLD)
                {
                    Vaux = T1;
                    dVaux_dVg = dVoxdepinv_dVg;
                    dVaux_dVd = dVoxdepinv_dVd;
                    dVaux_dVb = dVoxdepinv_dVb;
                }
                else if (VxNVt < -Transistor.EXP_THRESHOLD)
                {
                    Vaux = T0 * Math.Log(1.0 + Transistor.MIN_EXP);
                    dVaux_dVg = dVaux_dVd = dVaux_dVb = 0.0;
                }
                else
                {
                    ExpVxNVt = Math.Exp(VxNVt);
                    Vaux = T0 * Math.Log(1.0 + ExpVxNVt);
                    dVaux_dVg = ExpVxNVt / (1.0 + ExpVxNVt);
                    dVaux_dVd = dVaux_dVg * dVoxdepinv_dVd;
                    dVaux_dVb = dVaux_dVg * dVoxdepinv_dVb;
                    dVaux_dVg *= dVoxdepinv_dVg;
                }

                T2 = (Vgs_eff - Vbseff) * Vaux;
                dT2_dVg = dVgs_eff_dVg * Vaux + (Vgs_eff - Vbseff) * dVaux_dVg;
                dT2_dVd = (Vgs_eff - Vbseff) * dVaux_dVd;
                dT2_dVb = -Vaux + (Vgs_eff - Vbseff) * dVaux_dVb;

                T11 *= 0.75610;
                T12 *= 1.31724;
                T3 = bsim4.pParam.BSIM4aigbinv * bsim4.pParam.BSIM4cigbinv - bsim4.pParam.BSIM4bigbinv;
                T4 = bsim4.pParam.BSIM4bigbinv * bsim4.pParam.BSIM4cigbinv;
                T5 = T12 * (bsim4.pParam.BSIM4aigbinv + T3 * Voxdepinv - T4 * Voxdepinv * Voxdepinv);

                if (T5 > Transistor.EXP_THRESHOLD)
                {
                    T6 = Transistor.MAX_EXP;
                    dT6_dVg = dT6_dVd = dT6_dVb = 0.0;
                }
                else if (T5 < -Transistor.EXP_THRESHOLD)
                {
                    T6 = Transistor.MIN_EXP;
                    dT6_dVg = dT6_dVd = dT6_dVb = 0.0;
                }
                else
                {
                    T6 = Math.Exp(T5);
                    dT6_dVg = T6 * T12 * (T3 - 2.0 * T4 * Voxdepinv);
                    dT6_dVd = dT6_dVg * dVoxdepinv_dVd;
                    dT6_dVb = dT6_dVg * dVoxdepinv_dVb;
                    dT6_dVg *= dVoxdepinv_dVg;
                }

                Igbinv = T11 * T2 * T6;
                dIgbinv_dVg = T11 * (T2 * dT6_dVg + T6 * dT2_dVg);
                dIgbinv_dVd = T11 * (T2 * dT6_dVd + T6 * dT2_dVd);
                dIgbinv_dVb = T11 * (T2 * dT6_dVb + T6 * dT2_dVb);

                bsim4.BSIM4Igb = Igbinv + Igbacc;
                bsim4.BSIM4gIgbg = dIgbinv_dVg + dIgbacc_dVg;
                bsim4.BSIM4gIgbd = dIgbinv_dVd;
                bsim4.BSIM4gIgbb = (dIgbinv_dVb + dIgbacc_dVb) * dVbseff_dVb;
            }
            else
            {
                bsim4.BSIM4Igb = bsim4.BSIM4gIgbg = bsim4.BSIM4gIgbd = bsim4.BSIM4gIgbs = bsim4.BSIM4gIgbb = 0.0;
            } /* End of Gate current */

            if (bsim4.BSIM4nf != 1.0)
            {
                cdrain *= bsim4.BSIM4nf;
                bsim4.BSIM4gds *= bsim4.BSIM4nf;
                bsim4.BSIM4gm *= bsim4.BSIM4nf;
                bsim4.BSIM4gmbs *= bsim4.BSIM4nf;
                bsim4.BSIM4IdovVds *= bsim4.BSIM4nf;

                bsim4.BSIM4gbbs *= bsim4.BSIM4nf;
                bsim4.BSIM4gbgs *= bsim4.BSIM4nf;
                bsim4.BSIM4gbds *= bsim4.BSIM4nf;
                bsim4.BSIM4csub *= bsim4.BSIM4nf;

                bsim4.BSIM4Igidl *= bsim4.BSIM4nf;
                bsim4.BSIM4ggidld *= bsim4.BSIM4nf;
                bsim4.BSIM4ggidlg *= bsim4.BSIM4nf;
                bsim4.BSIM4ggidlb *= bsim4.BSIM4nf;

                bsim4.BSIM4Igisl *= bsim4.BSIM4nf;
                bsim4.BSIM4ggisls *= bsim4.BSIM4nf;
                bsim4.BSIM4ggislg *= bsim4.BSIM4nf;
                bsim4.BSIM4ggislb *= bsim4.BSIM4nf;

                bsim4.BSIM4Igcs *= bsim4.BSIM4nf;
                bsim4.BSIM4gIgcsg *= bsim4.BSIM4nf;
                bsim4.BSIM4gIgcsd *= bsim4.BSIM4nf;
                bsim4.BSIM4gIgcsb *= bsim4.BSIM4nf;
                bsim4.BSIM4Igcd *= bsim4.BSIM4nf;
                bsim4.BSIM4gIgcdg *= bsim4.BSIM4nf;
                bsim4.BSIM4gIgcdd *= bsim4.BSIM4nf;
                bsim4.BSIM4gIgcdb *= bsim4.BSIM4nf;

                bsim4.BSIM4Igs *= bsim4.BSIM4nf;
                bsim4.BSIM4gIgsg *= bsim4.BSIM4nf;
                bsim4.BSIM4gIgss *= bsim4.BSIM4nf;
                bsim4.BSIM4Igd *= bsim4.BSIM4nf;
                bsim4.BSIM4gIgdg *= bsim4.BSIM4nf;
                bsim4.BSIM4gIgdd *= bsim4.BSIM4nf;

                bsim4.BSIM4Igb *= bsim4.BSIM4nf;
                bsim4.BSIM4gIgbg *= bsim4.BSIM4nf;
                bsim4.BSIM4gIgbd *= bsim4.BSIM4nf;
                bsim4.BSIM4gIgbb *= bsim4.BSIM4nf;
            }

            bsim4.BSIM4ggidls = -(bsim4.BSIM4ggidld + bsim4.BSIM4ggidlg + bsim4.BSIM4ggidlb);
            bsim4.BSIM4ggisld = -(bsim4.BSIM4ggisls + bsim4.BSIM4ggislg + bsim4.BSIM4ggislb);
            bsim4.BSIM4gIgbs = -(bsim4.BSIM4gIgbg + bsim4.BSIM4gIgbd + bsim4.BSIM4gIgbb);
            bsim4.BSIM4gIgcss = -(bsim4.BSIM4gIgcsg + bsim4.BSIM4gIgcsd + bsim4.BSIM4gIgcsb);
            bsim4.BSIM4gIgcds = -(bsim4.BSIM4gIgcdg + bsim4.BSIM4gIgcdd + bsim4.BSIM4gIgcdb);
            bsim4.BSIM4cd = cdrain;

            /* Calculations for noise analysis */

            if (model.BSIM4tnoiMod.Value == 0)
            {
                Abulk = Abulk0 * bsim4.pParam.BSIM4abulkCVfactor;
                Vdsat = Vgsteff / Abulk;
                T0 = Vdsat - Vds - Transistor.DELTA_4;
                T1 = Math.Sqrt(T0 * T0 + 4.0 * Transistor.DELTA_4 * Vdsat);
                if (T0 >= 0.0)
                    Vdseff = Vdsat - 0.5 * (T0 + T1);
                else
                {
                    T3 = (Transistor.DELTA_4 + Transistor.DELTA_4) / (T1 - T0);
                    T4 = 1.0 - T3;
                    T5 = Vdsat * T3 / (T1 - T0);
                    Vdseff = Vdsat * T4;
                }
                if (Vds == 0.0)
                    Vdseff = 0.0;

                T0 = Abulk * Vdseff;
                T1 = 12.0 * (Vgsteff - 0.5 * T0 + 1.0e-20);
                T2 = Vdseff / T1;
                T3 = T0 * T2;
                bsim4.BSIM4qinv = Coxeff * bsim4.pParam.BSIM4weffCV * bsim4.BSIM4nf * bsim4.pParam.BSIM4leffCV * (Vgsteff - 0.5 * T0 + Abulk * T3);
            }
            else if (model.BSIM4tnoiMod.Value == 2)
            {
                bsim4.BSIM4noiGd0 = bsim4.BSIM4nf * beta * Vgsteff / (1.0 + gche * Rds);
            }

            /* 
            * bsim4.BSIM4v80 C - V begins
            */

            if ((model.BSIM4xpart < 0) || (!ChargeComputationNeeded))
            {
                qgate = qdrn = qsrc = qbulk = 0.0;
                bsim4.BSIM4cggb = bsim4.BSIM4cgsb = bsim4.BSIM4cgdb = 0.0;
                bsim4.BSIM4cdgb = bsim4.BSIM4cdsb = bsim4.BSIM4cddb = 0.0;
                bsim4.BSIM4cbgb = bsim4.BSIM4cbsb = bsim4.BSIM4cbdb = 0.0;
                bsim4.BSIM4csgb = bsim4.BSIM4cssb = bsim4.BSIM4csdb = 0.0;
                bsim4.BSIM4cgbb = bsim4.BSIM4csbb = bsim4.BSIM4cdbb = bsim4.BSIM4cbbb = 0.0;
                bsim4.BSIM4cqdb = bsim4.BSIM4cqsb = bsim4.BSIM4cqgb = bsim4.BSIM4cqbb = 0.0;
                bsim4.BSIM4gtau = 0.0;
                goto finished;
            }
            else if (model.BSIM4capMod.Value == 0)
            {
                if (Vbseff < 0.0)
                {
                    VbseffCV = Vbs; /* 4.6.2 */
                    dVbseffCV_dVb = 1.0;
                }
                else
                {
                    VbseffCV = bsim4.pParam.BSIM4phi - Phis;
                    dVbseffCV_dVb = -dPhis_dVb * dVbseff_dVb; /* 4.6.2 */
                }

                Vfb = bsim4.pParam.BSIM4vfbcv;
                Vth = Vfb + bsim4.pParam.BSIM4phi + bsim4.pParam.BSIM4k1ox * sqrtPhis;
                Vgst = Vgs_eff - Vth;
                dVth_dVb = bsim4.pParam.BSIM4k1ox * dsqrtPhis_dVb * dVbseff_dVb; /* 4.6.2 */
                dVgst_dVb = -dVth_dVb;
                dVgst_dVg = dVgs_eff_dVg;

                CoxWL = model.BSIM4coxe * bsim4.pParam.BSIM4weffCV * bsim4.pParam.BSIM4leffCV * bsim4.BSIM4nf;
                Arg1 = Vgs_eff - VbseffCV - Vfb;

                if (Arg1 <= 0.0)
                {
                    qgate = CoxWL * Arg1;
                    qbulk = -qgate;
                    qdrn = 0.0;

                    bsim4.BSIM4cggb = CoxWL * dVgs_eff_dVg;
                    bsim4.BSIM4cgdb = 0.0;
                    bsim4.BSIM4cgsb = CoxWL * (dVbseffCV_dVb - dVgs_eff_dVg);

                    bsim4.BSIM4cdgb = 0.0;
                    bsim4.BSIM4cddb = 0.0;
                    bsim4.BSIM4cdsb = 0.0;

                    bsim4.BSIM4cbgb = -CoxWL * dVgs_eff_dVg;
                    bsim4.BSIM4cbdb = 0.0;
                    bsim4.BSIM4cbsb = -bsim4.BSIM4cgsb;
                } /* Arg1 <= 0.0, end of accumulation */
                else if (Vgst <= 0.0)
                {
                    T1 = 0.5 * bsim4.pParam.BSIM4k1ox;
                    T2 = Math.Sqrt(T1 * T1 + Arg1);
                    qgate = CoxWL * bsim4.pParam.BSIM4k1ox * (T2 - T1);
                    qbulk = -qgate;
                    qdrn = 0.0;

                    T0 = CoxWL * T1 / T2;
                    bsim4.BSIM4cggb = T0 * dVgs_eff_dVg;
                    bsim4.BSIM4cgdb = 0.0;
                    bsim4.BSIM4cgsb = T0 * (dVbseffCV_dVb - dVgs_eff_dVg);

                    bsim4.BSIM4cdgb = 0.0;
                    bsim4.BSIM4cddb = 0.0;
                    bsim4.BSIM4cdsb = 0.0;

                    bsim4.BSIM4cbgb = -bsim4.BSIM4cggb;
                    bsim4.BSIM4cbdb = 0.0;
                    bsim4.BSIM4cbsb = -bsim4.BSIM4cgsb;
                } /* Vgst <= 0.0, end of depletion */
                else
                {
                    One_Third_CoxWL = CoxWL / 3.0;
                    Two_Third_CoxWL = 2.0 * One_Third_CoxWL;

                    AbulkCV = Abulk0 * bsim4.pParam.BSIM4abulkCVfactor;
                    dAbulkCV_dVb = bsim4.pParam.BSIM4abulkCVfactor * dAbulk0_dVb * dVbseff_dVb;

                    dVdsat_dVg = 1.0 / AbulkCV; /* 4.6.2 */
                    Vdsat = Vgst * dVdsat_dVg;
                    dVdsat_dVb = -(Vdsat * dAbulkCV_dVb + dVth_dVb) * dVdsat_dVg;

                    if (model.BSIM4xpart > 0.5)
                    {
                        /* 0 / 100 Charge partition model */
                        if (Vdsat <= Vds)
                        {
                            /* saturation region */
                            T1 = Vdsat / 3.0;
                            qgate = CoxWL * (Vgs_eff - Vfb - bsim4.pParam.BSIM4phi - T1);
                            T2 = -Two_Third_CoxWL * Vgst;
                            qbulk = -(qgate + T2);
                            qdrn = 0.0;

                            bsim4.BSIM4cggb = One_Third_CoxWL * (3.0 - dVdsat_dVg) * dVgs_eff_dVg;
                            T2 = -One_Third_CoxWL * dVdsat_dVb;
                            bsim4.BSIM4cgsb = -(bsim4.BSIM4cggb + T2);
                            bsim4.BSIM4cgdb = 0.0;

                            bsim4.BSIM4cdgb = 0.0;
                            bsim4.BSIM4cddb = 0.0;
                            bsim4.BSIM4cdsb = 0.0;

                            bsim4.BSIM4cbgb = -(bsim4.BSIM4cggb - Two_Third_CoxWL * dVgs_eff_dVg);
                            T3 = -(T2 + Two_Third_CoxWL * dVth_dVb);
                            bsim4.BSIM4cbsb = -(bsim4.BSIM4cbgb + T3);
                            bsim4.BSIM4cbdb = 0.0;
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
                            qgate = CoxWL * (Vgs_eff - Vfb - bsim4.pParam.BSIM4phi - 0.5 * (Vds - T3));
                            T10 = T4 * T8;
                            qdrn = T4 * T7;
                            qbulk = -(qgate + qdrn + T10);

                            T5 = T3 / T1;
                            bsim4.BSIM4cggb = CoxWL * (1.0 - T5 * dVdsat_dVg) * dVgs_eff_dVg;
                            T11 = -CoxWL * T5 * dVdsat_dVb;
                            bsim4.BSIM4cgdb = CoxWL * (T2 - 0.5 + 0.5 * T5);
                            bsim4.BSIM4cgsb = -(bsim4.BSIM4cggb + T11 + bsim4.BSIM4cgdb);
                            T6 = 1.0 / Vdsat;
                            dAlphaz_dVg = T6 * (1.0 - Alphaz * dVdsat_dVg);
                            dAlphaz_dVb = -T6 * (dVth_dVb + Alphaz * dVdsat_dVb);
                            T7 = T9 * T7;
                            T8 = T9 * T8;
                            T9 = 2.0 * T4 * (1.0 - 3.0 * T5);
                            bsim4.BSIM4cdgb = (T7 * dAlphaz_dVg - T9 * dVdsat_dVg) * dVgs_eff_dVg;
                            T12 = T7 * dAlphaz_dVb - T9 * dVdsat_dVb;
                            bsim4.BSIM4cddb = T4 * (3.0 - 6.0 * T2 - 3.0 * T5);
                            bsim4.BSIM4cdsb = -(bsim4.BSIM4cdgb + T12 + bsim4.BSIM4cddb);

                            T9 = 2.0 * T4 * (1.0 + T5);
                            T10 = (T8 * dAlphaz_dVg - T9 * dVdsat_dVg) * dVgs_eff_dVg;
                            T11 = T8 * dAlphaz_dVb - T9 * dVdsat_dVb;
                            T12 = T4 * (2.0 * T2 + T5 - 1.0);
                            T0 = -(T10 + T11 + T12);

                            bsim4.BSIM4cbgb = -(bsim4.BSIM4cggb + bsim4.BSIM4cdgb + T10);
                            bsim4.BSIM4cbdb = -(bsim4.BSIM4cgdb + bsim4.BSIM4cddb + T12);
                            bsim4.BSIM4cbsb = -(bsim4.BSIM4cgsb + bsim4.BSIM4cdsb + T0);
                        }
                    }
                    else if (model.BSIM4xpart < 0.5)
                    {
                        /* 40 / 60 Charge partition model */
                        if (Vds >= Vdsat)
                        {
                            /* saturation region */
                            T1 = Vdsat / 3.0;
                            qgate = CoxWL * (Vgs_eff - Vfb - bsim4.pParam.BSIM4phi - T1);
                            T2 = -Two_Third_CoxWL * Vgst;
                            qbulk = -(qgate + T2);
                            qdrn = 0.4 * T2;

                            bsim4.BSIM4cggb = One_Third_CoxWL * (3.0 - dVdsat_dVg) * dVgs_eff_dVg;
                            T2 = -One_Third_CoxWL * dVdsat_dVb;
                            bsim4.BSIM4cgsb = -(bsim4.BSIM4cggb + T2);
                            bsim4.BSIM4cgdb = 0.0;

                            T3 = 0.4 * Two_Third_CoxWL;
                            bsim4.BSIM4cdgb = -T3 * dVgs_eff_dVg;
                            bsim4.BSIM4cddb = 0.0;
                            T4 = T3 * dVth_dVb;
                            bsim4.BSIM4cdsb = -(T4 + bsim4.BSIM4cdgb);

                            bsim4.BSIM4cbgb = -(bsim4.BSIM4cggb - Two_Third_CoxWL * dVgs_eff_dVg);
                            T3 = -(T2 + Two_Third_CoxWL * dVth_dVb);
                            bsim4.BSIM4cbsb = -(bsim4.BSIM4cbgb + T3);
                            bsim4.BSIM4cbdb = 0.0;
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
                            qgate = CoxWL * (Vgs_eff - Vfb - bsim4.pParam.BSIM4phi - 0.5 * (Vds - T3));

                            T5 = T3 / T1;
                            bsim4.BSIM4cggb = CoxWL * (1.0 - T5 * dVdsat_dVg) * dVgs_eff_dVg;
                            tmp = -CoxWL * T5 * dVdsat_dVb;
                            bsim4.BSIM4cgdb = CoxWL * (T2 - 0.5 + 0.5 * T5);
                            bsim4.BSIM4cgsb = -(bsim4.BSIM4cggb + bsim4.BSIM4cgdb + tmp);

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

                            bsim4.BSIM4cdgb = (T7 * dAlphaz_dVg - tmp1 * dVdsat_dVg) * dVgs_eff_dVg;
                            T10 = T7 * dAlphaz_dVb - tmp1 * dVdsat_dVb;
                            bsim4.BSIM4cddb = T4 * (2.0 - (1.0 / (3.0 * T1 * T1) + 2.0 * tmp) * T6 + T8 * (6.0 * Vdsat - 2.4 * Vds));
                            bsim4.BSIM4cdsb = -(bsim4.BSIM4cdgb + T10 + bsim4.BSIM4cddb);

                            T7 = 2.0 * (T1 + T3);
                            qbulk = -(qgate - T4 * T7);
                            T7 *= T9;
                            T0 = 4.0 * T4 * (1.0 - T5);
                            T12 = (-T7 * dAlphaz_dVg - T0 * dVdsat_dVg) * dVgs_eff_dVg - bsim4.BSIM4cdgb; /* 4.6.2 */
                            T11 = -T7 * dAlphaz_dVb - T10 - T0 * dVdsat_dVb;
                            T10 = -4.0 * T4 * (T2 - 0.5 + 0.5 * T5) - bsim4.BSIM4cddb;
                            tmp = -(T10 + T11 + T12);

                            bsim4.BSIM4cbgb = -(bsim4.BSIM4cggb + bsim4.BSIM4cdgb + T12);
                            bsim4.BSIM4cbdb = -(bsim4.BSIM4cgdb + bsim4.BSIM4cddb + T10);
                            bsim4.BSIM4cbsb = -(bsim4.BSIM4cgsb + bsim4.BSIM4cdsb + tmp);
                        }
                    }
                    else
                    {
                        /* 50 / 50 partitioning */
                        if (Vds >= Vdsat)
                        {
                            /* saturation region */
                            T1 = Vdsat / 3.0;
                            qgate = CoxWL * (Vgs_eff - Vfb - bsim4.pParam.BSIM4phi - T1);
                            T2 = -Two_Third_CoxWL * Vgst;
                            qbulk = -(qgate + T2);
                            qdrn = 0.5 * T2;

                            bsim4.BSIM4cggb = One_Third_CoxWL * (3.0 - dVdsat_dVg) * dVgs_eff_dVg;
                            T2 = -One_Third_CoxWL * dVdsat_dVb;
                            bsim4.BSIM4cgsb = -(bsim4.BSIM4cggb + T2);
                            bsim4.BSIM4cgdb = 0.0;

                            bsim4.BSIM4cdgb = -One_Third_CoxWL * dVgs_eff_dVg;
                            bsim4.BSIM4cddb = 0.0;
                            T4 = One_Third_CoxWL * dVth_dVb;
                            bsim4.BSIM4cdsb = -(T4 + bsim4.BSIM4cdgb);

                            bsim4.BSIM4cbgb = -(bsim4.BSIM4cggb - Two_Third_CoxWL * dVgs_eff_dVg);
                            T3 = -(T2 + Two_Third_CoxWL * dVth_dVb);
                            bsim4.BSIM4cbsb = -(bsim4.BSIM4cbgb + T3);
                            bsim4.BSIM4cbdb = 0.0;
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
                            qgate = CoxWL * (Vgs_eff - Vfb - bsim4.pParam.BSIM4phi - 0.5 * (Vds - T3));

                            T5 = T3 / T1;
                            bsim4.BSIM4cggb = CoxWL * (1.0 - T5 * dVdsat_dVg) * dVgs_eff_dVg;
                            tmp = -CoxWL * T5 * dVdsat_dVb;
                            bsim4.BSIM4cgdb = CoxWL * (T2 - 0.5 + 0.5 * T5);
                            bsim4.BSIM4cgsb = -(bsim4.BSIM4cggb + bsim4.BSIM4cgdb + tmp);

                            T6 = 1.0 / Vdsat;
                            dAlphaz_dVg = T6 * (1.0 - Alphaz * dVdsat_dVg);
                            dAlphaz_dVb = -T6 * (dVth_dVb + Alphaz * dVdsat_dVb);

                            T7 = T1 + T3;
                            qdrn = -T4 * T7;
                            qbulk = -(qgate + qdrn + qdrn);
                            T7 *= T9;
                            T0 = T4 * (2.0 * T5 - 2.0);

                            bsim4.BSIM4cdgb = (T0 * dVdsat_dVg - T7 * dAlphaz_dVg) * dVgs_eff_dVg;
                            T12 = T0 * dVdsat_dVb - T7 * dAlphaz_dVb;
                            bsim4.BSIM4cddb = T4 * (1.0 - 2.0 * T2 - T5);
                            bsim4.BSIM4cdsb = -(bsim4.BSIM4cdgb + T12 + bsim4.BSIM4cddb);

                            bsim4.BSIM4cbgb = -(bsim4.BSIM4cggb + 2.0 * bsim4.BSIM4cdgb);
                            bsim4.BSIM4cbdb = -(bsim4.BSIM4cgdb + 2.0 * bsim4.BSIM4cddb);
                            bsim4.BSIM4cbsb = -(bsim4.BSIM4cgsb + 2.0 * bsim4.BSIM4cdsb);
                        } /* end of linear region */
                    } /* end of 50 / 50 partition */
                } /* end of inversion */
            } /* end of capMod = 0 */
            else
            {
                if (Vbseff < 0.0)
                {
                    VbseffCV = Vbseff;
                    dVbseffCV_dVb = 1.0;
                }
                else
                {
                    VbseffCV = bsim4.pParam.BSIM4phi - Phis;
                    dVbseffCV_dVb = -dPhis_dVb;
                }

                CoxWL = model.BSIM4coxe * bsim4.pParam.BSIM4weffCV * bsim4.pParam.BSIM4leffCV * bsim4.BSIM4nf;

                if (model.BSIM4cvchargeMod.Value == 0)
                {
                    /* Seperate VgsteffCV with noff and voffcv */
                    noff = n * bsim4.pParam.BSIM4noff;
                    dnoff_dVd = bsim4.pParam.BSIM4noff * dn_dVd;
                    dnoff_dVb = bsim4.pParam.BSIM4noff * dn_dVb;
                    T0 = Vtm * noff;
                    voffcv = bsim4.pParam.BSIM4voffcv;
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
                    }
                    /* End of VgsteffCV for cvchargeMod = 0 */
                }
                else
                {
                    T0 = n * Vtm;
                    T1 = bsim4.pParam.BSIM4mstarcv * Vgst;
                    T2 = T1 / T0;
                    if (T2 > Transistor.EXP_THRESHOLD)
                    {
                        T10 = T1;
                        dT10_dVg = bsim4.pParam.BSIM4mstarcv * dVgs_eff_dVg;
                        dT10_dVd = -dVth_dVd * bsim4.pParam.BSIM4mstarcv;
                        dT10_dVb = -dVth_dVb * bsim4.pParam.BSIM4mstarcv;
                    }
                    else if (T2 < -Transistor.EXP_THRESHOLD)
                    {
                        T10 = Vtm * Math.Log(1.0 + Transistor.MIN_EXP);
                        dT10_dVg = 0.0;
                        dT10_dVd = T10 * dn_dVd;
                        dT10_dVb = T10 * dn_dVb;
                        T10 *= n;
                    }
                    else
                    {
                        ExpVgst = Math.Exp(T2);
                        T3 = Vtm * Math.Log(1.0 + ExpVgst);
                        T10 = n * T3;
                        dT10_dVg = bsim4.pParam.BSIM4mstarcv * ExpVgst / (1.0 + ExpVgst);
                        dT10_dVb = T3 * dn_dVb - dT10_dVg * (dVth_dVb + Vgst * dn_dVb / n);
                        dT10_dVd = T3 * dn_dVd - dT10_dVg * (dVth_dVd + Vgst * dn_dVd / n);
                        dT10_dVg *= dVgs_eff_dVg;
                    }

                    T1 = bsim4.pParam.BSIM4voffcbncv - (1.0 - bsim4.pParam.BSIM4mstarcv) * Vgst;
                    T2 = T1 / T0;
                    if (T2 < -Transistor.EXP_THRESHOLD)
                    {
                        T3 = model.BSIM4coxe * Transistor.MIN_EXP / bsim4.pParam.BSIM4cdep0;
                        T9 = bsim4.pParam.BSIM4mstarcv + T3 * n;
                        dT9_dVg = 0.0;
                        dT9_dVd = dn_dVd * T3;
                        dT9_dVb = dn_dVb * T3;
                    }
                    else if (T2 > Transistor.EXP_THRESHOLD)
                    {
                        T3 = model.BSIM4coxe * Transistor.MAX_EXP / bsim4.pParam.BSIM4cdep0;
                        T9 = bsim4.pParam.BSIM4mstarcv + T3 * n;
                        dT9_dVg = 0.0;
                        dT9_dVd = dn_dVd * T3;
                        dT9_dVb = dn_dVb * T3;
                    }
                    else
                    {
                        ExpVgst = Math.Exp(T2);
                        T3 = model.BSIM4coxe / bsim4.pParam.BSIM4cdep0;
                        T4 = T3 * ExpVgst;
                        T5 = T1 * T4 / T0;
                        T9 = bsim4.pParam.BSIM4mstarcv + n * T4;
                        dT9_dVg = T3 * (bsim4.pParam.BSIM4mstarcv - 1.0) * ExpVgst / Vtm;
                        dT9_dVb = T4 * dn_dVb - dT9_dVg * dVth_dVb - T5 * dn_dVb;
                        dT9_dVd = T4 * dn_dVd - dT9_dVg * dVth_dVd - T5 * dn_dVd;
                        dT9_dVg *= dVgs_eff_dVg;
                    }

                    Vgsteff = T10 / T9;
                    T11 = T9 * T9;
                    dVgsteff_dVg = (T9 * dT10_dVg - T10 * dT9_dVg) / T11;
                    dVgsteff_dVd = (T9 * dT10_dVd - T10 * dT9_dVd) / T11;
                    dVgsteff_dVb = (T9 * dT10_dVb - T10 * dT9_dVb) / T11;
                    /* End of VgsteffCV for cvchargeMod = 1 */
                }

                if (model.BSIM4capMod.Value == 1)
                {
                    Vfb = bsim4.BSIM4vfbzb;
                    V3 = Vfb - Vgs_eff + VbseffCV - Transistor.DELTA_3;
                    if (Vfb <= 0.0)
                        T0 = Math.Sqrt(V3 * V3 - 4.0 * Transistor.DELTA_3 * Vfb);
                    else
                        T0 = Math.Sqrt(V3 * V3 + 4.0 * Transistor.DELTA_3 * Vfb);

                    T1 = 0.5 * (1.0 + V3 / T0);
                    Vfbeff = Vfb - 0.5 * (V3 + T0);
                    dVfbeff_dVg = T1 * dVgs_eff_dVg;
                    dVfbeff_dVb = -T1 * dVbseffCV_dVb;
                    Qac0 = CoxWL * (Vfbeff - Vfb);
                    dQac0_dVg = CoxWL * dVfbeff_dVg;
                    dQac0_dVb = CoxWL * dVfbeff_dVb;

                    T0 = 0.5 * bsim4.pParam.BSIM4k1ox;
                    T3 = Vgs_eff - Vfbeff - VbseffCV - Vgsteff;
                    if (bsim4.pParam.BSIM4k1ox == 0.0)
                    {
                        T1 = 0.0;
                        T2 = 0.0;
                    }
                    else if (T3 < 0.0)
                    {
                        T1 = T0 + T3 / bsim4.pParam.BSIM4k1ox;
                        T2 = CoxWL;
                    }
                    else
                    {
                        T1 = Math.Sqrt(T0 * T0 + T3);
                        T2 = CoxWL * T0 / T1;
                    }

                    Qsub0 = CoxWL * bsim4.pParam.BSIM4k1ox * (T1 - T0);

                    dQsub0_dVg = T2 * (dVgs_eff_dVg - dVfbeff_dVg - dVgsteff_dVg);
                    dQsub0_dVd = -T2 * dVgsteff_dVd;
                    dQsub0_dVb = -T2 * (dVfbeff_dVb + dVbseffCV_dVb + dVgsteff_dVb);

                    AbulkCV = Abulk0 * bsim4.pParam.BSIM4abulkCVfactor;
                    dAbulkCV_dVb = bsim4.pParam.BSIM4abulkCVfactor * dAbulk0_dVb;
                    VdsatCV = Vgsteff / AbulkCV;

                    T0 = VdsatCV - Vds - Transistor.DELTA_4;
                    dT0_dVg = 1.0 / AbulkCV;
                    dT0_dVb = -VdsatCV * dAbulkCV_dVb / AbulkCV;
                    T1 = Math.Sqrt(T0 * T0 + 4.0 * Transistor.DELTA_4 * VdsatCV);
                    dT1_dVg = (T0 + Transistor.DELTA_4 + Transistor.DELTA_4) / T1;
                    dT1_dVd = -T0 / T1;
                    dT1_dVb = dT1_dVg * dT0_dVb;
                    dT1_dVg *= dT0_dVg;
                    if (T0 >= 0.0)
                    {
                        VdseffCV = VdsatCV - 0.5 * (T0 + T1);
                        dVdseffCV_dVg = 0.5 * (dT0_dVg - dT1_dVg);
                        dVdseffCV_dVd = 0.5 * (1.0 - dT1_dVd);
                        dVdseffCV_dVb = 0.5 * (dT0_dVb - dT1_dVb);
                    }
                    else
                    {
                        T3 = (Transistor.DELTA_4 + Transistor.DELTA_4) / (T1 - T0);
                        T4 = 1.0 - T3;
                        T5 = VdsatCV * T3 / (T1 - T0);
                        VdseffCV = VdsatCV * T4;
                        dVdseffCV_dVg = dT0_dVg * T4 + T5 * (dT1_dVg - dT0_dVg);
                        dVdseffCV_dVd = T5 * (dT1_dVd + 1.0);
                        dVdseffCV_dVb = dT0_dVb * (T4 - T5) + T5 * dT1_dVb;
                    }

                    if (Vds == 0.0)
                    {
                        VdseffCV = 0.0;
                        dVdseffCV_dVg = 0.0;
                        dVdseffCV_dVb = 0.0;
                    }

                    T0 = AbulkCV * VdseffCV;
                    T1 = 12.0 * (Vgsteff - 0.5 * T0 + 1.0e-20);
                    T2 = VdseffCV / T1;
                    T3 = T0 * T2;

                    T4 = (1.0 - 12.0 * T2 * T2 * AbulkCV);
                    T5 = (6.0 * T0 * (4.0 * Vgsteff - T0) / (T1 * T1) - 0.5);
                    T6 = 12.0 * T2 * T2 * Vgsteff;

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

                    if (model.BSIM4xpart > 0.5)
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
                    else if (model.BSIM4xpart < 0.5)
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

                    bsim4.BSIM4cggb = Cgg;
                    bsim4.BSIM4cgsb = -(Cgg + Cgd + Cgb);
                    bsim4.BSIM4cgdb = Cgd;
                    bsim4.BSIM4cdgb = -(Cgg + Cbg + Csg);
                    bsim4.BSIM4cdsb = (Cgg + Cgd + Cgb + Cbg + Cbd + Cbb + Csg + Csd + Csb);
                    bsim4.BSIM4cddb = -(Cgd + Cbd + Csd);
                    bsim4.BSIM4cbgb = Cbg;
                    bsim4.BSIM4cbsb = -(Cbg + Cbd + Cbb);
                    bsim4.BSIM4cbdb = Cbd;
                }

                /* Charge - Thickness capMod (CTM) begins */
                else if (model.BSIM4capMod.Value == 2)
                {
                    V3 = bsim4.BSIM4vfbzb - Vgs_eff + VbseffCV - Transistor.DELTA_3;
                    if (bsim4.BSIM4vfbzb <= 0.0)
                        T0 = Math.Sqrt(V3 * V3 - 4.0 * Transistor.DELTA_3 * bsim4.BSIM4vfbzb);
                    else
                        T0 = Math.Sqrt(V3 * V3 + 4.0 * Transistor.DELTA_3 * bsim4.BSIM4vfbzb);

                    T1 = 0.5 * (1.0 + V3 / T0);
                    Vfbeff = bsim4.BSIM4vfbzb - 0.5 * (V3 + T0);
                    dVfbeff_dVg = T1 * dVgs_eff_dVg;
                    dVfbeff_dVb = -T1 * dVbseffCV_dVb;

                    Cox = bsim4.BSIM4coxp;
                    Tox = 1.0e8 * bsim4.BSIM4toxp;
                    T0 = (Vgs_eff - VbseffCV - bsim4.BSIM4vfbzb) / Tox;
                    dT0_dVg = dVgs_eff_dVg / Tox;
                    dT0_dVb = -dVbseffCV_dVb / Tox;

                    tmp = T0 * bsim4.pParam.BSIM4acde;
                    if ((-Transistor.EXP_THRESHOLD < tmp) && (tmp < Transistor.EXP_THRESHOLD))
                    {
                        Tcen = bsim4.pParam.BSIM4ldeb * Math.Exp(tmp);
                        dTcen_dVg = bsim4.pParam.BSIM4acde * Tcen;
                        dTcen_dVb = dTcen_dVg * dT0_dVb;
                        dTcen_dVg *= dT0_dVg;
                    }
                    else if (tmp <= -Transistor.EXP_THRESHOLD)
                    {
                        Tcen = bsim4.pParam.BSIM4ldeb * Transistor.MIN_EXP;
                        dTcen_dVg = dTcen_dVb = 0.0;
                    }
                    else
                    {
                        Tcen = bsim4.pParam.BSIM4ldeb * Transistor.MAX_EXP;
                        dTcen_dVg = dTcen_dVb = 0.0;
                    }

                    LINK = 1.0e-3 * bsim4.BSIM4toxp;
                    V3 = bsim4.pParam.BSIM4ldeb - Tcen - LINK;
                    V4 = Math.Sqrt(V3 * V3 + 4.0 * LINK * bsim4.pParam.BSIM4ldeb);
                    Tcen = bsim4.pParam.BSIM4ldeb - 0.5 * (V3 + V4);
                    T1 = 0.5 * (1.0 + V3 / V4);
                    dTcen_dVg *= T1;
                    dTcen_dVb *= T1;

                    Ccen = epssub / Tcen;
                    T2 = Cox / (Cox + Ccen);
                    Coxeff = T2 * Ccen;
                    T3 = -Ccen / Tcen;
                    dCoxeff_dVg = T2 * T2 * T3;
                    dCoxeff_dVb = dCoxeff_dVg * dTcen_dVb;
                    dCoxeff_dVg *= dTcen_dVg;
                    CoxWLcen = CoxWL * Coxeff / model.BSIM4coxe;

                    Qac0 = CoxWLcen * (Vfbeff - bsim4.BSIM4vfbzb);
                    QovCox = Qac0 / Coxeff;
                    dQac0_dVg = CoxWLcen * dVfbeff_dVg + QovCox * dCoxeff_dVg;
                    dQac0_dVb = CoxWLcen * dVfbeff_dVb + QovCox * dCoxeff_dVb;

                    T0 = 0.5 * bsim4.pParam.BSIM4k1ox;
                    T3 = Vgs_eff - Vfbeff - VbseffCV - Vgsteff;
                    if (bsim4.pParam.BSIM4k1ox == 0.0)
                    {
                        T1 = 0.0;
                        T2 = 0.0;
                    }
                    else if (T3 < 0.0)
                    {
                        T1 = T0 + T3 / bsim4.pParam.BSIM4k1ox;
                        T2 = CoxWLcen;
                    }
                    else
                    {
                        T1 = Math.Sqrt(T0 * T0 + T3);
                        T2 = CoxWLcen * T0 / T1;
                    }

                    Qsub0 = CoxWLcen * bsim4.pParam.BSIM4k1ox * (T1 - T0);
                    QovCox = Qsub0 / Coxeff;
                    dQsub0_dVg = T2 * (dVgs_eff_dVg - dVfbeff_dVg - dVgsteff_dVg) + QovCox * dCoxeff_dVg;
                    dQsub0_dVd = -T2 * dVgsteff_dVd;
                    dQsub0_dVb = -T2 * (dVfbeff_dVb + dVbseffCV_dVb + dVgsteff_dVb) + QovCox * dCoxeff_dVb;

                    /* Gate - bias dependent delta Phis begins */
                    if (bsim4.pParam.BSIM4k1ox <= 0.0)
                    {
                        Denomi = 0.25 * bsim4.pParam.BSIM4moin * Vtm;
                        T0 = 0.5 * bsim4.pParam.BSIM4sqrtPhi;
                    }
                    else
                    {
                        Denomi = bsim4.pParam.BSIM4moin * Vtm * bsim4.pParam.BSIM4k1ox * bsim4.pParam.BSIM4k1ox;
                        T0 = bsim4.pParam.BSIM4k1ox * bsim4.pParam.BSIM4sqrtPhi;
                    }
                    T1 = 2.0 * T0 + Vgsteff;

                    DeltaPhi = Vtm * Math.Log(1.0 + T1 * Vgsteff / Denomi);
                    dDeltaPhi_dVg = 2.0 * Vtm * (T1 - T0) / (Denomi + T1 * Vgsteff);
                    /* End of delta Phis */

                    /* VgDP = Vgsteff - DeltaPhi */
                    T0 = Vgsteff - DeltaPhi - 0.001;
                    dT0_dVg = 1.0 - dDeltaPhi_dVg;
                    T1 = Math.Sqrt(T0 * T0 + Vgsteff * 0.004);
                    VgDP = 0.5 * (T0 + T1);
                    dVgDP_dVg = 0.5 * (dT0_dVg + (T0 * dT0_dVg + 0.002) / T1);

                    Tox += Tox; /* WDLiu: Tcen reevaluated below due to different Vgsteff */
                    T0 = (Vgsteff + bsim4.BSIM4vtfbphi2) / Tox;
                    tmp = Math.Exp(model.BSIM4bdos * 0.7 * Math.Log(T0));
                    T1 = 1.0 + tmp;
                    T2 = model.BSIM4bdos * 0.7 * tmp / (T0 * Tox);
                    Tcen = model.BSIM4ados * 1.9e-9 / T1;
                    dTcen_dVg = -Tcen * T2 / T1;
                    dTcen_dVd = dTcen_dVg * dVgsteff_dVd;
                    dTcen_dVb = dTcen_dVg * dVgsteff_dVb;
                    dTcen_dVg *= dVgsteff_dVg;

                    Ccen = epssub / Tcen;
                    T0 = Cox / (Cox + Ccen);
                    Coxeff = T0 * Ccen;
                    T1 = -Ccen / Tcen;
                    dCoxeff_dVg = T0 * T0 * T1;
                    dCoxeff_dVd = dCoxeff_dVg * dTcen_dVd;
                    dCoxeff_dVb = dCoxeff_dVg * dTcen_dVb;
                    dCoxeff_dVg *= dTcen_dVg;
                    CoxWLcen = CoxWL * Coxeff / model.BSIM4coxe;

                    AbulkCV = Abulk0 * bsim4.pParam.BSIM4abulkCVfactor;
                    dAbulkCV_dVb = bsim4.pParam.BSIM4abulkCVfactor * dAbulk0_dVb;
                    VdsatCV = VgDP / AbulkCV;

                    T0 = VdsatCV - Vds - Transistor.DELTA_4;
                    dT0_dVg = dVgDP_dVg / AbulkCV;
                    dT0_dVb = -VdsatCV * dAbulkCV_dVb / AbulkCV;
                    T1 = Math.Sqrt(T0 * T0 + 4.0 * Transistor.DELTA_4 * VdsatCV);
                    dT1_dVg = (T0 + Transistor.DELTA_4 + Transistor.DELTA_4) / T1;
                    dT1_dVd = -T0 / T1;
                    dT1_dVb = dT1_dVg * dT0_dVb;
                    dT1_dVg *= dT0_dVg;
                    if (T0 >= 0.0)
                    {
                        VdseffCV = VdsatCV - 0.5 * (T0 + T1);
                        dVdseffCV_dVg = 0.5 * (dT0_dVg - dT1_dVg);
                        dVdseffCV_dVd = 0.5 * (1.0 - dT1_dVd);
                        dVdseffCV_dVb = 0.5 * (dT0_dVb - dT1_dVb);
                    }
                    else
                    {
                        T3 = (Transistor.DELTA_4 + Transistor.DELTA_4) / (T1 - T0);
                        T4 = 1.0 - T3;
                        T5 = VdsatCV * T3 / (T1 - T0);
                        VdseffCV = VdsatCV * T4;
                        dVdseffCV_dVg = dT0_dVg * T4 + T5 * (dT1_dVg - dT0_dVg);
                        dVdseffCV_dVd = T5 * (dT1_dVd + 1.0);
                        dVdseffCV_dVb = dT0_dVb * (T4 - T5) + T5 * dT1_dVb;
                    }

                    if (Vds == 0.0)
                    {
                        VdseffCV = 0.0;
                        dVdseffCV_dVg = 0.0;
                        dVdseffCV_dVb = 0.0;
                    }

                    T0 = AbulkCV * VdseffCV;
                    T1 = VgDP;
                    T2 = 12.0 * (T1 - 0.5 * T0 + 1.0e-20);
                    T3 = T0 / T2;
                    T4 = 1.0 - 12.0 * T3 * T3;
                    T5 = AbulkCV * (6.0 * T0 * (4.0 * T1 - T0) / (T2 * T2) - 0.5);
                    T6 = T5 * VdseffCV / AbulkCV;

                    qgate = CoxWLcen * (T1 - T0 * (0.5 - T3));
                    QovCox = qgate / Coxeff;
                    Cgg1 = CoxWLcen * (T4 * dVgDP_dVg + T5 * dVdseffCV_dVg);
                    Cgd1 = CoxWLcen * T5 * dVdseffCV_dVd + Cgg1 * dVgsteff_dVd + QovCox * dCoxeff_dVd;
                    Cgb1 = CoxWLcen * (T5 * dVdseffCV_dVb + T6 * dAbulkCV_dVb) + Cgg1 * dVgsteff_dVb + QovCox * dCoxeff_dVb;
                    Cgg1 = Cgg1 * dVgsteff_dVg + QovCox * dCoxeff_dVg;

                    T7 = 1.0 - AbulkCV;
                    T8 = T2 * T2;
                    T9 = 12.0 * T7 * T0 * T0 / (T8 * AbulkCV);
                    T10 = T9 * dVgDP_dVg;
                    T11 = -T7 * T5 / AbulkCV;
                    T12 = -(T9 * T1 / AbulkCV + VdseffCV * (0.5 - T0 / T2));

                    qbulk = CoxWLcen * T7 * (0.5 * VdseffCV - T0 * VdseffCV / T2);
                    QovCox = qbulk / Coxeff;
                    Cbg1 = CoxWLcen * (T10 + T11 * dVdseffCV_dVg);
                    Cbd1 = CoxWLcen * T11 * dVdseffCV_dVd + Cbg1 * dVgsteff_dVd + QovCox * dCoxeff_dVd;
                    Cbb1 = CoxWLcen * (T11 * dVdseffCV_dVb + T12 * dAbulkCV_dVb) + Cbg1 * dVgsteff_dVb + QovCox * dCoxeff_dVb;
                    Cbg1 = Cbg1 * dVgsteff_dVg + QovCox * dCoxeff_dVg;

                    if (model.BSIM4xpart > 0.5)
                    {
                        /* 0 / 100 partition */
                        qsrc = -CoxWLcen * (T1 / 2.0 + T0 / 4.0 - 0.5 * T0 * T0 / T2);
                        QovCox = qsrc / Coxeff;
                        T2 += T2;
                        T3 = T2 * T2;
                        T7 = -(0.25 - 12.0 * T0 * (4.0 * T1 - T0) / T3);
                        T4 = -(0.5 + 24.0 * T0 * T0 / T3) * dVgDP_dVg;
                        T5 = T7 * AbulkCV;
                        T6 = T7 * VdseffCV;

                        Csg = CoxWLcen * (T4 + T5 * dVdseffCV_dVg);
                        Csd = CoxWLcen * T5 * dVdseffCV_dVd + Csg * dVgsteff_dVd + QovCox * dCoxeff_dVd;
                        Csb = CoxWLcen * (T5 * dVdseffCV_dVb + T6 * dAbulkCV_dVb) + Csg * dVgsteff_dVb + QovCox * dCoxeff_dVb;
                        Csg = Csg * dVgsteff_dVg + QovCox * dCoxeff_dVg;
                    }
                    else if (model.BSIM4xpart < 0.5)
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

                        Csg = T5 * dVgDP_dVg + T6 * dVdseffCV_dVg;
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

                    bsim4.BSIM4cggb = Cgg;
                    bsim4.BSIM4cgsb = -(Cgg + Cgd + Cgb);
                    bsim4.BSIM4cgdb = Cgd;
                    bsim4.BSIM4cdgb = -(Cgg + Cbg + Csg);
                    bsim4.BSIM4cdsb = (Cgg + Cgd + Cgb + Cbg + Cbd + Cbb + Csg + Csd + Csb);
                    bsim4.BSIM4cddb = -(Cgd + Cbd + Csd);
                    bsim4.BSIM4cbgb = Cbg;
                    bsim4.BSIM4cbsb = -(Cbg + Cbd + Cbb);
                    bsim4.BSIM4cbdb = Cbd;
                } /* End of CTM */
            }

            bsim4.BSIM4csgb = -bsim4.BSIM4cggb - bsim4.BSIM4cdgb - bsim4.BSIM4cbgb;
            bsim4.BSIM4csdb = -bsim4.BSIM4cgdb - bsim4.BSIM4cddb - bsim4.BSIM4cbdb;
            bsim4.BSIM4cssb = -bsim4.BSIM4cgsb - bsim4.BSIM4cdsb - bsim4.BSIM4cbsb;
            bsim4.BSIM4cgbb = -bsim4.BSIM4cgdb - bsim4.BSIM4cggb - bsim4.BSIM4cgsb;
            bsim4.BSIM4cdbb = -bsim4.BSIM4cddb - bsim4.BSIM4cdgb - bsim4.BSIM4cdsb;
            bsim4.BSIM4cbbb = -bsim4.BSIM4cbgb - bsim4.BSIM4cbdb - bsim4.BSIM4cbsb;
            bsim4.BSIM4csbb = -bsim4.BSIM4cgbb - bsim4.BSIM4cdbb - bsim4.BSIM4cbbb;
            bsim4.BSIM4qgate = qgate;
            bsim4.BSIM4qbulk = qbulk;
            bsim4.BSIM4qdrn = qdrn;
            bsim4.BSIM4qsrc = -(qgate + qbulk + qdrn);

            /* NQS begins */
            if ((bsim4.BSIM4trnqsMod != 0) || (bsim4.BSIM4acnqsMod != 0))
            {
                bsim4.BSIM4qchqs = qcheq = -(qbulk + qgate);
                bsim4.BSIM4cqgb = -(bsim4.BSIM4cggb + bsim4.BSIM4cbgb);
                bsim4.BSIM4cqdb = -(bsim4.BSIM4cgdb + bsim4.BSIM4cbdb);
                bsim4.BSIM4cqsb = -(bsim4.BSIM4cgsb + bsim4.BSIM4cbsb);
                bsim4.BSIM4cqbb = -(bsim4.BSIM4cqgb + bsim4.BSIM4cqdb + bsim4.BSIM4cqsb);

                CoxWL = model.BSIM4coxe * bsim4.pParam.BSIM4weffCV * bsim4.BSIM4nf * bsim4.pParam.BSIM4leffCV;
                T1 = bsim4.BSIM4gcrg / CoxWL; /* 1 / tau */
                bsim4.BSIM4gtau = T1 * BSIM4v80.ScalingFactor;

                if (bsim4.BSIM4acnqsMod != 0)
                    bsim4.BSIM4taunet = 1.0 / T1;

                state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4qcheq] = qcheq;
                if (state.Init == State.InitFlags.InitTransient)
                    state.States[1][bsim4.BSIM4states + BSIM4v80.BSIM4qcheq] = state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4qcheq];
                if (bsim4.BSIM4trnqsMod != 0 && method != null)
                    method.Integrate(state, bsim4.BSIM4states + BSIM4v80.BSIM4qcheq, 0.0);
            }

            finished:

            /* Calculate junction C - V */
            if (ChargeComputationNeeded)
            {
                czbd = model.BSIM4DunitAreaTempJctCap * bsim4.BSIM4Adeff; /* bug fix */
                czbs = model.BSIM4SunitAreaTempJctCap * bsim4.BSIM4Aseff;
                czbdsw = model.BSIM4DunitLengthSidewallTempJctCap * bsim4.BSIM4Pdeff;
                czbdswg = model.BSIM4DunitLengthGateSidewallTempJctCap * bsim4.pParam.BSIM4weffCJ * bsim4.BSIM4nf;
                czbssw = model.BSIM4SunitLengthSidewallTempJctCap * bsim4.BSIM4Pseff;
                czbsswg = model.BSIM4SunitLengthGateSidewallTempJctCap * bsim4.pParam.BSIM4weffCJ * bsim4.BSIM4nf;

                MJS = model.BSIM4SbulkJctBotGradingCoeff;
                MJSWS = model.BSIM4SbulkJctSideGradingCoeff;
                MJSWGS = model.BSIM4SbulkJctGateSideGradingCoeff;

                MJD = model.BSIM4DbulkJctBotGradingCoeff;
                MJSWD = model.BSIM4DbulkJctSideGradingCoeff;
                MJSWGD = model.BSIM4DbulkJctGateSideGradingCoeff;

                /* Source Bulk Junction */
                if (vbs_jct == 0.0)
                {
                    state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4qbs] = 0.0;
                    bsim4.BSIM4capbs = czbs + czbssw + czbsswg;
                }
                else if (vbs_jct < 0.0)
                {
                    if (czbs > 0.0)
                    {
                        arg = 1.0 - vbs_jct / model.BSIM4PhiBS;
                        if (MJS == 0.5)
                            sarg = 1.0 / Math.Sqrt(arg);
                        else
                            sarg = Math.Exp(-MJS * Math.Log(arg));
                        state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4qbs] = model.BSIM4PhiBS * czbs * (1.0 - arg * sarg) / (1.0 - MJS);
                        bsim4.BSIM4capbs = czbs * sarg;
                    }
                    else
                    {
                        state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4qbs] = 0.0;
                        bsim4.BSIM4capbs = 0.0;
                    }
                    if (czbssw > 0.0)
                    {
                        arg = 1.0 - vbs_jct / model.BSIM4PhiBSWS;
                        if (MJSWS == 0.5)
                            sarg = 1.0 / Math.Sqrt(arg);
                        else
                            sarg = Math.Exp(-MJSWS * Math.Log(arg));
                        state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4qbs] += model.BSIM4PhiBSWS * czbssw * (1.0 - arg * sarg) / (1.0 - MJSWS);
                        bsim4.BSIM4capbs += czbssw * sarg;
                    }
                    if (czbsswg > 0.0)
                    {
                        arg = 1.0 - vbs_jct / model.BSIM4PhiBSWGS;
                        if (MJSWGS == 0.5)
                            sarg = 1.0 / Math.Sqrt(arg);
                        else
                            sarg = Math.Exp(-MJSWGS * Math.Log(arg));
                        state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4qbs] += model.BSIM4PhiBSWGS * czbsswg * (1.0 - arg * sarg) / (1.0 - MJSWGS);
                        bsim4.BSIM4capbs += czbsswg * sarg;
                    }

                }
                else
                {
                    T0 = czbs + czbssw + czbsswg;
                    T1 = vbs_jct * (czbs * MJS / model.BSIM4PhiBS + czbssw * MJSWS / model.BSIM4PhiBSWS + czbsswg * MJSWGS /
                        model.BSIM4PhiBSWGS);
                    state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4qbs] = vbs_jct * (T0 + 0.5 * T1);
                    bsim4.BSIM4capbs = T0 + T1;
                }

                /* Drain Bulk Junction */
                if (vbd_jct == 0.0)
                {
                    state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4qbd] = 0.0;
                    bsim4.BSIM4capbd = czbd + czbdsw + czbdswg;
                }
                else if (vbd_jct < 0.0)
                {
                    if (czbd > 0.0)
                    {
                        arg = 1.0 - vbd_jct / model.BSIM4PhiBD;
                        if (MJD == 0.5)
                            sarg = 1.0 / Math.Sqrt(arg);
                        else
                            sarg = Math.Exp(-MJD * Math.Log(arg));
                        state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4qbd] = model.BSIM4PhiBD * czbd * (1.0 - arg * sarg) / (1.0 - MJD);
                        bsim4.BSIM4capbd = czbd * sarg;
                    }
                    else
                    {
                        state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4qbd] = 0.0;
                        bsim4.BSIM4capbd = 0.0;
                    }
                    if (czbdsw > 0.0)
                    {
                        arg = 1.0 - vbd_jct / model.BSIM4PhiBSWD;
                        if (MJSWD == 0.5)
                            sarg = 1.0 / Math.Sqrt(arg);
                        else
                            sarg = Math.Exp(-MJSWD * Math.Log(arg));
                        state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4qbd] += model.BSIM4PhiBSWD * czbdsw * (1.0 - arg * sarg) / (1.0 - MJSWD);
                        bsim4.BSIM4capbd += czbdsw * sarg;
                    }
                    if (czbdswg > 0.0)
                    {
                        arg = 1.0 - vbd_jct / model.BSIM4PhiBSWGD;
                        if (MJSWGD == 0.5)
                            sarg = 1.0 / Math.Sqrt(arg);
                        else
                            sarg = Math.Exp(-MJSWGD * Math.Log(arg));
                        state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4qbd] += model.BSIM4PhiBSWGD * czbdswg * (1.0 - arg * sarg) / (1.0 - MJSWGD);
                        bsim4.BSIM4capbd += czbdswg * sarg;
                    }
                }
                else
                {
                    T0 = czbd + czbdsw + czbdswg;
                    T1 = vbd_jct * (czbd * MJD / model.BSIM4PhiBD + czbdsw * MJSWD / model.BSIM4PhiBSWD + czbdswg * MJSWGD / model.BSIM4PhiBSWGD);
                    state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4qbd] = vbd_jct * (T0 + 0.5 * T1);
                    bsim4.BSIM4capbd = T0 + T1;
                }
            }

            /* 
            * check convergence
            */

            if (!bsim4.BSIM4off || ((state.Init != State.InitFlags.InitFix)))
            {
                if (Check == 1)
                    state.IsCon = false;
            }
            state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4vds] = vds;
            state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4vgs] = vgs;
            state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4vbs] = vbs;
            state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4vbd] = vbd;
            state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4vges] = vges;
            state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4vgms] = vgms;
            state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4vdbs] = vdbs;
            state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4vdbd] = vdbd;
            state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4vsbs] = vsbs;
            state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4vses] = vses;
            state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4vdes] = vdes;
            state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4qdef] = qdef;

            if (!ChargeComputationNeeded)
                goto line850;

            if (bsim4.BSIM4rgateMod.Value == 3)
            {
                vgdx = vgmd;
                vgsx = vgms;
            }
            else /* For rgateMod == 0, 1 and 2 */
            {
                vgdx = vgd;
                vgsx = vgs;
            }
            if (model.BSIM4capMod.Value == 0)
            {
                cgdo = bsim4.pParam.BSIM4cgdo;
                qgdo = bsim4.pParam.BSIM4cgdo * vgdx;
                cgso = bsim4.pParam.BSIM4cgso;
                qgso = bsim4.pParam.BSIM4cgso * vgsx;
            }
            else /* For both capMod == 1 and 2 */
            {
                T0 = vgdx + Transistor.DELTA_1;
                T1 = Math.Sqrt(T0 * T0 + 4.0 * Transistor.DELTA_1);
                T2 = 0.5 * (T0 - T1);

                T3 = bsim4.pParam.BSIM4weffCV * bsim4.pParam.BSIM4cgdl;
                T4 = Math.Sqrt(1.0 - 4.0 * T2 / bsim4.pParam.BSIM4ckappad);
                cgdo = bsim4.pParam.BSIM4cgdo + T3 - T3 * (1.0 - 1.0 / T4) * (0.5 - 0.5 * T0 / T1);
                qgdo = (bsim4.pParam.BSIM4cgdo + T3) * vgdx - T3 * (T2 + 0.5 * bsim4.pParam.BSIM4ckappad * (T4 - 1.0));

                T0 = vgsx + Transistor.DELTA_1;
                T1 = Math.Sqrt(T0 * T0 + 4.0 * Transistor.DELTA_1);
                T2 = 0.5 * (T0 - T1);
                T3 = bsim4.pParam.BSIM4weffCV * bsim4.pParam.BSIM4cgsl;
                T4 = Math.Sqrt(1.0 - 4.0 * T2 / bsim4.pParam.BSIM4ckappas);
                cgso = bsim4.pParam.BSIM4cgso + T3 - T3 * (1.0 - 1.0 / T4) * (0.5 - 0.5 * T0 / T1);
                qgso = (bsim4.pParam.BSIM4cgso + T3) * vgsx - T3 * (T2 + 0.5 * bsim4.pParam.BSIM4ckappas * (T4 - 1.0));
            }

            if (bsim4.BSIM4nf != 1.0)
            {
                cgdo *= bsim4.BSIM4nf;
                cgso *= bsim4.BSIM4nf;
                qgdo *= bsim4.BSIM4nf;
                qgso *= bsim4.BSIM4nf;
            }
            bsim4.BSIM4cgdo = cgdo;
            bsim4.BSIM4qgdo = qgdo;
            bsim4.BSIM4cgso = cgso;
            bsim4.BSIM4qgso = qgso;

            ag0 = method.Slope;
            if (bsim4.BSIM4mode > 0)
            {
                if (bsim4.BSIM4trnqsMod.Value == 0)
                {
                    qdrn -= qgdo;
                    if (bsim4.BSIM4rgateMod.Value == 3)
                    {
                        gcgmgmb = (cgdo + cgso + bsim4.pParam.BSIM4cgbo) * ag0;
                        gcgmdb = -cgdo * ag0;
                        gcgmsb = -cgso * ag0;
                        gcgmbb = -bsim4.pParam.BSIM4cgbo * ag0;

                        gcdgmb = gcgmdb;
                        gcsgmb = gcgmsb;
                        gcbgmb = gcgmbb;

                        gcggb = bsim4.BSIM4cggb * ag0;
                        gcgdb = bsim4.BSIM4cgdb * ag0;
                        gcgsb = bsim4.BSIM4cgsb * ag0;
                        gcgbb = -(gcggb + gcgdb + gcgsb);

                        gcdgb = bsim4.BSIM4cdgb * ag0;
                        gcsgb = -(bsim4.BSIM4cggb + bsim4.BSIM4cbgb + bsim4.BSIM4cdgb) * ag0;
                        gcbgb = bsim4.BSIM4cbgb * ag0;

                        qgmb = bsim4.pParam.BSIM4cgbo * vgmb;
                        qgmid = qgdo + qgso + qgmb;
                        qbulk -= qgmb;
                        qsrc = -(qgate + qgmid + qbulk + qdrn);
                    }
                    else
                    {
                        gcggb = (bsim4.BSIM4cggb + cgdo + cgso + bsim4.pParam.BSIM4cgbo) * ag0;
                        gcgdb = (bsim4.BSIM4cgdb - cgdo) * ag0;
                        gcgsb = (bsim4.BSIM4cgsb - cgso) * ag0;
                        gcgbb = -(gcggb + gcgdb + gcgsb);

                        gcdgb = (bsim4.BSIM4cdgb - cgdo) * ag0;
                        gcsgb = -(bsim4.BSIM4cggb + bsim4.BSIM4cbgb + bsim4.BSIM4cdgb + cgso) * ag0;
                        gcbgb = (bsim4.BSIM4cbgb - bsim4.pParam.BSIM4cgbo) * ag0;

                        gcdgmb = gcsgmb = gcbgmb = 0.0;

                        qgb = bsim4.pParam.BSIM4cgbo * vgb;
                        qgate += qgdo + qgso + qgb;
                        qbulk -= qgb;
                        qsrc = -(qgate + qbulk + qdrn);
                    }
                    gcddb = (bsim4.BSIM4cddb + bsim4.BSIM4capbd + cgdo) * ag0;
                    gcdsb = bsim4.BSIM4cdsb * ag0;

                    gcsdb = -(bsim4.BSIM4cgdb + bsim4.BSIM4cbdb + bsim4.BSIM4cddb) * ag0;
                    gcssb = (bsim4.BSIM4capbs + cgso - (bsim4.BSIM4cgsb + bsim4.BSIM4cbsb + bsim4.BSIM4cdsb)) * ag0;

                    if (bsim4.BSIM4rbodyMod == 0)
                    {
                        gcdbb = -(gcdgb + gcddb + gcdsb + gcdgmb);
                        gcsbb = -(gcsgb + gcsdb + gcssb + gcsgmb);
                        gcbdb = (bsim4.BSIM4cbdb - bsim4.BSIM4capbd) * ag0;
                        gcbsb = (bsim4.BSIM4cbsb - bsim4.BSIM4capbs) * ag0;
                        gcdbdb = 0.0; gcsbsb = 0.0;
                    }
                    else
                    {
                        gcdbb = -(bsim4.BSIM4cddb + bsim4.BSIM4cdgb + bsim4.BSIM4cdsb) * ag0;
                        gcsbb = -(gcsgb + gcsdb + gcssb + gcsgmb) + bsim4.BSIM4capbs * ag0;
                        gcbdb = bsim4.BSIM4cbdb * ag0;
                        gcbsb = bsim4.BSIM4cbsb * ag0;

                        gcdbdb = -bsim4.BSIM4capbd * ag0;
                        gcsbsb = -bsim4.BSIM4capbs * ag0;
                    }
                    gcbbb = -(gcbdb + gcbgb + gcbsb + gcbgmb);

                    ggtg = ggtd = ggtb = ggts = 0.0;
                    sxpart = 0.6;
                    dxpart = 0.4;
                    ddxpart_dVd = ddxpart_dVg = ddxpart_dVb = ddxpart_dVs = 0.0;
                    dsxpart_dVd = dsxpart_dVg = dsxpart_dVb = dsxpart_dVs = 0.0;
                }
                else
                {
                    qcheq = bsim4.BSIM4qchqs;
                    CoxWL = model.BSIM4coxe * bsim4.pParam.BSIM4weffCV * bsim4.BSIM4nf * bsim4.pParam.BSIM4leffCV;
                    T0 = qdef * BSIM4v80.ScalingFactor / CoxWL;

                    ggtg = bsim4.BSIM4gtg = T0 * bsim4.BSIM4gcrgg;
                    ggtd = bsim4.BSIM4gtd = T0 * bsim4.BSIM4gcrgd;
                    ggts = bsim4.BSIM4gts = T0 * bsim4.BSIM4gcrgs;
                    ggtb = bsim4.BSIM4gtb = T0 * bsim4.BSIM4gcrgb;
                    gqdef = BSIM4v80.ScalingFactor * ag0;

                    gcqgb = bsim4.BSIM4cqgb * ag0;
                    gcqdb = bsim4.BSIM4cqdb * ag0;
                    gcqsb = bsim4.BSIM4cqsb * ag0;
                    gcqbb = bsim4.BSIM4cqbb * ag0;

                    if (Math.Abs(qcheq) <= 1.0e-5 * CoxWL)
                    {
                        if (model.BSIM4xpart < 0.5)
                        {
                            dxpart = 0.4;
                        }
                        else if (model.BSIM4xpart > 0.5)
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
                        Cdd = bsim4.BSIM4cddb;
                        Csd = -(bsim4.BSIM4cgdb + bsim4.BSIM4cddb + bsim4.BSIM4cbdb);
                        ddxpart_dVd = (Cdd - dxpart * (Cdd + Csd)) / qcheq;
                        Cdg = bsim4.BSIM4cdgb;
                        Csg = -(bsim4.BSIM4cggb + bsim4.BSIM4cdgb + bsim4.BSIM4cbgb);
                        ddxpart_dVg = (Cdg - dxpart * (Cdg + Csg)) / qcheq;

                        Cds = bsim4.BSIM4cdsb;
                        Css = -(bsim4.BSIM4cgsb + bsim4.BSIM4cdsb + bsim4.BSIM4cbsb);
                        ddxpart_dVs = (Cds - dxpart * (Cds + Css)) / qcheq;

                        ddxpart_dVb = -(ddxpart_dVd + ddxpart_dVg + ddxpart_dVs);
                    }
                    sxpart = 1.0 - dxpart;
                    dsxpart_dVd = -ddxpart_dVd;
                    dsxpart_dVg = -ddxpart_dVg;
                    dsxpart_dVs = -ddxpart_dVs;
                    dsxpart_dVb = -(dsxpart_dVd + dsxpart_dVg + dsxpart_dVs);

                    if (bsim4.BSIM4rgateMod.Value == 3)
                    {
                        gcgmgmb = (cgdo + cgso + bsim4.pParam.BSIM4cgbo) * ag0;
                        gcgmdb = -cgdo * ag0;
                        gcgmsb = -cgso * ag0;
                        gcgmbb = -bsim4.pParam.BSIM4cgbo * ag0;

                        gcdgmb = gcgmdb;
                        gcsgmb = gcgmsb;
                        gcbgmb = gcgmbb;

                        gcdgb = gcsgb = gcbgb = 0.0;
                        gcggb = gcgdb = gcgsb = gcgbb = 0.0;

                        qgmb = bsim4.pParam.BSIM4cgbo * vgmb;
                        qgmid = qgdo + qgso + qgmb;
                        qgate = 0.0;
                        qbulk = -qgmb;
                        qdrn = -qgdo;
                        qsrc = -(qgmid + qbulk + qdrn);
                    }
                    else
                    {
                        gcggb = (cgdo + cgso + bsim4.pParam.BSIM4cgbo) * ag0;
                        gcgdb = -cgdo * ag0;
                        gcgsb = -cgso * ag0;
                        gcgbb = -bsim4.pParam.BSIM4cgbo * ag0;

                        gcdgb = gcgdb;
                        gcsgb = gcgsb;
                        gcbgb = gcgbb;
                        gcdgmb = gcsgmb = gcbgmb = 0.0;

                        qgb = bsim4.pParam.BSIM4cgbo * vgb;
                        qgate = qgdo + qgso + qgb;
                        qbulk = -qgb;
                        qdrn = -qgdo;
                        qsrc = -(qgate + qbulk + qdrn);
                    }

                    gcddb = (bsim4.BSIM4capbd + cgdo) * ag0;
                    gcdsb = gcsdb = 0.0;
                    gcssb = (bsim4.BSIM4capbs + cgso) * ag0;

                    if (bsim4.BSIM4rbodyMod == 0)
                    {
                        gcdbb = -(gcdgb + gcddb + gcdgmb);
                        gcsbb = -(gcsgb + gcssb + gcsgmb);
                        gcbdb = -bsim4.BSIM4capbd * ag0;
                        gcbsb = -bsim4.BSIM4capbs * ag0;
                        gcdbdb = 0.0; gcsbsb = 0.0;
                    }
                    else
                    {
                        gcdbb = gcsbb = gcbdb = gcbsb = 0.0;
                        gcdbdb = -bsim4.BSIM4capbd * ag0;
                        gcsbsb = -bsim4.BSIM4capbs * ag0;
                    }
                    gcbbb = -(gcbdb + gcbgb + gcbsb + gcbgmb);
                }
            }
            else
            {
                if (bsim4.BSIM4trnqsMod.Value == 0)
                {
                    qsrc = qdrn - qgso;
                    if (bsim4.BSIM4rgateMod.Value == 3)
                    {
                        gcgmgmb = (cgdo + cgso + bsim4.pParam.BSIM4cgbo) * ag0;
                        gcgmdb = -cgdo * ag0;
                        gcgmsb = -cgso * ag0;
                        gcgmbb = -bsim4.pParam.BSIM4cgbo * ag0;

                        gcdgmb = gcgmdb;
                        gcsgmb = gcgmsb;
                        gcbgmb = gcgmbb;

                        gcggb = bsim4.BSIM4cggb * ag0;
                        gcgdb = bsim4.BSIM4cgsb * ag0;
                        gcgsb = bsim4.BSIM4cgdb * ag0;
                        gcgbb = -(gcggb + gcgdb + gcgsb);

                        gcdgb = -(bsim4.BSIM4cggb + bsim4.BSIM4cbgb + bsim4.BSIM4cdgb) * ag0;
                        gcsgb = bsim4.BSIM4cdgb * ag0;
                        gcbgb = bsim4.BSIM4cbgb * ag0;

                        qgmb = bsim4.pParam.BSIM4cgbo * vgmb;
                        qgmid = qgdo + qgso + qgmb;
                        qbulk -= qgmb;
                        qdrn = -(qgate + qgmid + qbulk + qsrc);
                    }
                    else
                    {
                        gcggb = (bsim4.BSIM4cggb + cgdo + cgso + bsim4.pParam.BSIM4cgbo) * ag0;
                        gcgdb = (bsim4.BSIM4cgsb - cgdo) * ag0;
                        gcgsb = (bsim4.BSIM4cgdb - cgso) * ag0;
                        gcgbb = -(gcggb + gcgdb + gcgsb);

                        gcdgb = -(bsim4.BSIM4cggb + bsim4.BSIM4cbgb + bsim4.BSIM4cdgb + cgdo) * ag0;
                        gcsgb = (bsim4.BSIM4cdgb - cgso) * ag0;
                        gcbgb = (bsim4.BSIM4cbgb - bsim4.pParam.BSIM4cgbo) * ag0;

                        gcdgmb = gcsgmb = gcbgmb = 0.0;

                        qgb = bsim4.pParam.BSIM4cgbo * vgb;
                        qgate += qgdo + qgso + qgb;
                        qbulk -= qgb;
                        qdrn = -(qgate + qbulk + qsrc);
                    }
                    gcddb = (bsim4.BSIM4capbd + cgdo - (bsim4.BSIM4cgsb + bsim4.BSIM4cbsb + bsim4.BSIM4cdsb)) * ag0;
                    gcdsb = -(bsim4.BSIM4cgdb + bsim4.BSIM4cbdb + bsim4.BSIM4cddb) * ag0;

                    gcsdb = bsim4.BSIM4cdsb * ag0;
                    gcssb = (bsim4.BSIM4cddb + bsim4.BSIM4capbs + cgso) * ag0;

                    if (bsim4.BSIM4rbodyMod == 0)
                    {
                        gcdbb = -(gcdgb + gcddb + gcdsb + gcdgmb);
                        gcsbb = -(gcsgb + gcsdb + gcssb + gcsgmb);
                        gcbdb = (bsim4.BSIM4cbsb - bsim4.BSIM4capbd) * ag0;
                        gcbsb = (bsim4.BSIM4cbdb - bsim4.BSIM4capbs) * ag0;
                        gcdbdb = 0.0; gcsbsb = 0.0;
                    }
                    else
                    {
                        gcdbb = -(gcdgb + gcddb + gcdsb + gcdgmb) + bsim4.BSIM4capbd * ag0;
                        gcsbb = -(bsim4.BSIM4cddb + bsim4.BSIM4cdgb + bsim4.BSIM4cdsb) * ag0;
                        gcbdb = bsim4.BSIM4cbsb * ag0;
                        gcbsb = bsim4.BSIM4cbdb * ag0;
                        gcdbdb = -bsim4.BSIM4capbd * ag0;
                        gcsbsb = -bsim4.BSIM4capbs * ag0;
                    }
                    gcbbb = -(gcbgb + gcbdb + gcbsb + gcbgmb);

                    ggtg = ggtd = ggtb = ggts = 0.0;
                    sxpart = 0.4;
                    dxpart = 0.6;
                    ddxpart_dVd = ddxpart_dVg = ddxpart_dVb = ddxpart_dVs = 0.0;
                    dsxpart_dVd = dsxpart_dVg = dsxpart_dVb = dsxpart_dVs = 0.0;
                }
                else
                {
                    qcheq = bsim4.BSIM4qchqs;
                    CoxWL = model.BSIM4coxe * bsim4.pParam.BSIM4weffCV * bsim4.BSIM4nf * bsim4.pParam.BSIM4leffCV;
                    T0 = qdef * BSIM4v80.ScalingFactor / CoxWL;
                    ggtg = bsim4.BSIM4gtg = T0 * bsim4.BSIM4gcrgg;
                    ggts = bsim4.BSIM4gts = T0 * bsim4.BSIM4gcrgd;
                    ggtd = bsim4.BSIM4gtd = T0 * bsim4.BSIM4gcrgs;
                    ggtb = bsim4.BSIM4gtb = T0 * bsim4.BSIM4gcrgb;
                    gqdef = BSIM4v80.ScalingFactor * ag0;

                    gcqgb = bsim4.BSIM4cqgb * ag0;
                    gcqdb = bsim4.BSIM4cqsb * ag0;
                    gcqsb = bsim4.BSIM4cqdb * ag0;
                    gcqbb = bsim4.BSIM4cqbb * ag0;

                    if (Math.Abs(qcheq) <= 1.0e-5 * CoxWL)
                    {
                        if (model.BSIM4xpart < 0.5)
                        {
                            sxpart = 0.4;
                        }
                        else if (model.BSIM4xpart > 0.5)
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
                        Css = bsim4.BSIM4cddb;
                        Cds = -(bsim4.BSIM4cgdb + bsim4.BSIM4cddb + bsim4.BSIM4cbdb);
                        dsxpart_dVs = (Css - sxpart * (Css + Cds)) / qcheq;
                        Csg = bsim4.BSIM4cdgb;
                        Cdg = -(bsim4.BSIM4cggb + bsim4.BSIM4cdgb + bsim4.BSIM4cbgb);
                        dsxpart_dVg = (Csg - sxpart * (Csg + Cdg)) / qcheq;

                        Csd = bsim4.BSIM4cdsb;
                        Cdd = -(bsim4.BSIM4cgsb + bsim4.BSIM4cdsb + bsim4.BSIM4cbsb);
                        dsxpart_dVd = (Csd - sxpart * (Csd + Cdd)) / qcheq;

                        dsxpart_dVb = -(dsxpart_dVd + dsxpart_dVg + dsxpart_dVs);
                    }
                    dxpart = 1.0 - sxpart;
                    ddxpart_dVd = -dsxpart_dVd;
                    ddxpart_dVg = -dsxpart_dVg;
                    ddxpart_dVs = -dsxpart_dVs;
                    ddxpart_dVb = -(ddxpart_dVd + ddxpart_dVg + ddxpart_dVs);

                    if (bsim4.BSIM4rgateMod.Value == 3)
                    {
                        gcgmgmb = (cgdo + cgso + bsim4.pParam.BSIM4cgbo) * ag0;
                        gcgmdb = -cgdo * ag0;
                        gcgmsb = -cgso * ag0;
                        gcgmbb = -bsim4.pParam.BSIM4cgbo * ag0;

                        gcdgmb = gcgmdb;
                        gcsgmb = gcgmsb;
                        gcbgmb = gcgmbb;

                        gcdgb = gcsgb = gcbgb = 0.0;
                        gcggb = gcgdb = gcgsb = gcgbb = 0.0;

                        qgmb = bsim4.pParam.BSIM4cgbo * vgmb;
                        qgmid = qgdo + qgso + qgmb;
                        qgate = 0.0;
                        qbulk = -qgmb;
                        qdrn = -qgdo;
                        qsrc = -qgso;
                    }
                    else
                    {
                        gcggb = (cgdo + cgso + bsim4.pParam.BSIM4cgbo) * ag0;
                        gcgdb = -cgdo * ag0;
                        gcgsb = -cgso * ag0;
                        gcgbb = -bsim4.pParam.BSIM4cgbo * ag0;

                        gcdgb = gcgdb;
                        gcsgb = gcgsb;
                        gcbgb = gcgbb;
                        gcdgmb = gcsgmb = gcbgmb = 0.0;

                        qgb = bsim4.pParam.BSIM4cgbo * vgb;
                        qgate = qgdo + qgso + qgb;
                        qbulk = -qgb;
                        qdrn = -qgdo;
                        qsrc = -qgso;
                    }

                    gcddb = (bsim4.BSIM4capbd + cgdo) * ag0;
                    gcdsb = gcsdb = 0.0;
                    gcssb = (bsim4.BSIM4capbs + cgso) * ag0;
                    if (bsim4.BSIM4rbodyMod == 0)
                    {
                        gcdbb = -(gcdgb + gcddb + gcdgmb);
                        gcsbb = -(gcsgb + gcssb + gcsgmb);
                        gcbdb = -bsim4.BSIM4capbd * ag0;
                        gcbsb = -bsim4.BSIM4capbs * ag0;
                        gcdbdb = 0.0; gcsbsb = 0.0;
                    }
                    else
                    {
                        gcdbb = gcsbb = gcbdb = gcbsb = 0.0;
                        gcdbdb = -bsim4.BSIM4capbd * ag0;
                        gcsbsb = -bsim4.BSIM4capbs * ag0;
                    }
                    gcbbb = -(gcbdb + gcbgb + gcbsb + gcbgmb);
                }
            }

            if (bsim4.BSIM4trnqsMod != 0)
            {
                state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4qcdump] = qdef * BSIM4v80.ScalingFactor;
                if (state.Init == State.InitFlags.InitTransient)
                    state.States[1][bsim4.BSIM4states + BSIM4v80.BSIM4qcdump] = state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4qcdump];
                if (method != null)
                    method.Integrate(state, bsim4.BSIM4states + BSIM4v80.BSIM4qcdump, 0.0);
            }

            state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4qg] = qgate;
            state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4qd] = qdrn - state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4qbd];
            state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4qs] = qsrc - state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4qbs];
            if (bsim4.BSIM4rgateMod.Value == 3)
                state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4qgmid] = qgmid;

            if (bsim4.BSIM4rbodyMod == 0)
            {
                state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4qb] = qbulk + state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4qbd] + state.States[0][bsim4.BSIM4states +
                    BSIM4v80.BSIM4qbs];
            }
            else
                state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4qb] = qbulk;

            /* Store small signal parameters */
            if (state.UseSmallSignal)
                goto line1000;

            if (!ChargeComputationNeeded)
                goto line850;

            if (state.Init == State.InitFlags.InitTransient)
            {
                state.States[1][bsim4.BSIM4states + BSIM4v80.BSIM4qb] = state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4qb];
                state.States[1][bsim4.BSIM4states + BSIM4v80.BSIM4qg] = state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4qg];
                state.States[1][bsim4.BSIM4states + BSIM4v80.BSIM4qd] = state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4qd];
                if (bsim4.BSIM4rgateMod.Value == 3)
                    state.States[1][bsim4.BSIM4states + BSIM4v80.BSIM4qgmid] = state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4qgmid];
                if (bsim4.BSIM4rbodyMod != 0)
                {
                    state.States[1][bsim4.BSIM4states + BSIM4v80.BSIM4qbs] = state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4qbs];
                    state.States[1][bsim4.BSIM4states + BSIM4v80.BSIM4qbd] = state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4qbd];
                }
            }

            if (method != null)
            {
                method.Integrate(state, bsim4.BSIM4states + BSIM4v80.BSIM4qb, 0.0);
                method.Integrate(state, bsim4.BSIM4states + BSIM4v80.BSIM4qg, 0.0);
                method.Integrate(state, bsim4.BSIM4states + BSIM4v80.BSIM4qd, 0.0);
                if (bsim4.BSIM4rgateMod == 3)
                    method.Integrate(state, bsim4.BSIM4states + BSIM4v80.BSIM4qgmid, 0.0);
                if (bsim4.BSIM4rbodyMod != 0)
                {
                    method.Integrate(state, bsim4.BSIM4states + BSIM4v80.BSIM4qbs, 0.0);
                    method.Integrate(state, bsim4.BSIM4states + BSIM4v80.BSIM4qbd, 0.0);
                }
            }

            goto line860;

            line850:
            /* Zero gcap and ceqcap if (!ChargeComputationNeeded)
            */
            ceqqg = ceqqb = ceqqd = 0.0;
            ceqqjd = ceqqjs = 0.0;
            cqcheq = cqdef = 0.0;

            gcdgb = gcddb = gcdsb = gcdbb = 0.0;
            gcsgb = gcsdb = gcssb = gcsbb = 0.0;
            gcggb = gcgdb = gcgsb = gcgbb = 0.0;
            gcbdb = gcbgb = gcbsb = gcbbb = 0.0;

            gcgmgmb = gcgmdb = gcgmsb = gcgmbb = 0.0;
            gcdgmb = gcsgmb = gcbgmb = ceqqgmid = 0.0;
            gcdbdb = gcsbsb = 0.0;

            gqdef = gcqgb = gcqdb = gcqsb = gcqbb = 0.0;
            ggtg = ggtd = ggtb = ggts = 0.0;
            sxpart = (1.0 - (dxpart = (bsim4.BSIM4mode > 0) ? 0.4 : 0.6));
            ddxpart_dVd = ddxpart_dVg = ddxpart_dVb = ddxpart_dVs = 0.0;
            dsxpart_dVd = dsxpart_dVg = dsxpart_dVb = dsxpart_dVs = 0.0;

            if (bsim4.BSIM4trnqsMod != 0)
            {
                CoxWL = model.BSIM4coxe * bsim4.pParam.BSIM4weffCV * bsim4.BSIM4nf * bsim4.pParam.BSIM4leffCV;
                T1 = bsim4.BSIM4gcrg / CoxWL;
                bsim4.BSIM4gtau = T1 * BSIM4v80.ScalingFactor;
            }
            else
                bsim4.BSIM4gtau = 0.0;

            goto line900;

            line860:
            /* Calculate equivalent charge current */

            cqgate = state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4cqg];
            cqbody = state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4cqb];
            cqdrn = state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4cqd];

            ceqqg = cqgate - gcggb * vgb + gcgdb * vbd + gcgsb * vbs;
            ceqqd = cqdrn - gcdgb * vgb - gcdgmb * vgmb + (gcddb + gcdbdb) * vbd - gcdbdb * vbd_jct + gcdsb * vbs;
            ceqqb = cqbody - gcbgb * vgb - gcbgmb * vgmb + gcbdb * vbd + gcbsb * vbs;

            if (bsim4.BSIM4rgateMod.Value == 3)
                ceqqgmid = state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4cqgmid] + gcgmdb * vbd + gcgmsb * vbs - gcgmgmb * vgmb;
            else
                ceqqgmid = 0.0;

            if (bsim4.BSIM4rbodyMod != 0)
            {
                ceqqjs = state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4cqbs] + gcsbsb * vbs_jct;
                ceqqjd = state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4cqbd] + gcdbdb * vbd_jct;
            }

            if (bsim4.BSIM4trnqsMod != 0)
            {
                T0 = ggtg * vgb - ggtd * vbd - ggts * vbs;
                ceqqg += T0;
                T1 = qdef * bsim4.BSIM4gtau;
                ceqqd -= dxpart * T0 + T1 * (ddxpart_dVg * vgb - ddxpart_dVd * vbd - ddxpart_dVs * vbs);
                cqdef = state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4cqcdump] - gqdef * qdef;
                cqcheq = state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4cqcheq] - (gcqgb * vgb - gcqdb * vbd - gcqsb * vbs) + T0;
            }

            if (state.Init == State.InitFlags.InitTransient)
            {
                state.States[1][bsim4.BSIM4states + BSIM4v80.BSIM4cqb] = state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4cqb];
                state.States[1][bsim4.BSIM4states + BSIM4v80.BSIM4cqg] = state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4cqg];
                state.States[1][bsim4.BSIM4states + BSIM4v80.BSIM4cqd] = state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4cqd];

                if (bsim4.BSIM4rgateMod.Value == 3)
                    state.States[1][bsim4.BSIM4states + BSIM4v80.BSIM4cqgmid] = state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4cqgmid];

                if (bsim4.BSIM4rbodyMod != 0)
                {
                    state.States[1][bsim4.BSIM4states + BSIM4v80.BSIM4cqbs] = state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4cqbs];
                    state.States[1][bsim4.BSIM4states + BSIM4v80.BSIM4cqbd] = state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4cqbd];
                }
            }

            /* 
            * Load current vector
            */

            line900:
            if (bsim4.BSIM4mode >= 0)
            {
                Gm = bsim4.BSIM4gm;
                Gmbs = bsim4.BSIM4gmbs;
                FwdSum = Gm + Gmbs;
                RevSum = 0.0;

                ceqdrn = model.BSIM4type * (cdrain - bsim4.BSIM4gds * vds - Gm * vgs - Gmbs * vbs);
                ceqbd = model.BSIM4type * (bsim4.BSIM4csub + bsim4.BSIM4Igidl - (bsim4.BSIM4gbds + bsim4.BSIM4ggidld) * vds - (bsim4.BSIM4gbgs + bsim4.BSIM4ggidlg) * vgs -
                    (bsim4.BSIM4gbbs + bsim4.BSIM4ggidlb) * vbs);
                ceqbs = model.BSIM4type * (bsim4.BSIM4Igisl + bsim4.BSIM4ggisls * vds - bsim4.BSIM4ggislg * vgd - bsim4.BSIM4ggislb * vbd);

                gbbdp = -(bsim4.BSIM4gbds);
                gbbsp = bsim4.BSIM4gbds + bsim4.BSIM4gbgs + bsim4.BSIM4gbbs;

                gbdpg = bsim4.BSIM4gbgs;
                gbdpdp = bsim4.BSIM4gbds;
                gbdpb = bsim4.BSIM4gbbs;
                gbdpsp = -(gbdpg + gbdpdp + gbdpb);

                gbspg = 0.0;
                gbspdp = 0.0;
                gbspb = 0.0;
                gbspsp = 0.0;

                if (model.BSIM4igcMod != 0)
                {
                    gIstotg = bsim4.BSIM4gIgsg + bsim4.BSIM4gIgcsg;
                    gIstotd = bsim4.BSIM4gIgcsd;
                    gIstots = bsim4.BSIM4gIgss + bsim4.BSIM4gIgcss;
                    gIstotb = bsim4.BSIM4gIgcsb;
                    Istoteq = model.BSIM4type * (bsim4.BSIM4Igs + bsim4.BSIM4Igcs - gIstotg * vgs - bsim4.BSIM4gIgcsd * vds - bsim4.BSIM4gIgcsb * vbs);

                    gIdtotg = bsim4.BSIM4gIgdg + bsim4.BSIM4gIgcdg;
                    gIdtotd = bsim4.BSIM4gIgdd + bsim4.BSIM4gIgcdd;
                    gIdtots = bsim4.BSIM4gIgcds;
                    gIdtotb = bsim4.BSIM4gIgcdb;
                    Idtoteq = model.BSIM4type * (bsim4.BSIM4Igd + bsim4.BSIM4Igcd - bsim4.BSIM4gIgdg * vgd - bsim4.BSIM4gIgcdg * vgs - bsim4.BSIM4gIgcdd * vds - bsim4.BSIM4gIgcdb *
                        vbs);
                }
                else
                {
                    gIstotg = gIstotd = gIstots = gIstotb = Istoteq = 0.0;
                    gIdtotg = gIdtotd = gIdtots = gIdtotb = Idtoteq = 0.0;
                }

                if (model.BSIM4igbMod != 0)
                {
                    gIbtotg = bsim4.BSIM4gIgbg;
                    gIbtotd = bsim4.BSIM4gIgbd;
                    gIbtots = bsim4.BSIM4gIgbs;
                    gIbtotb = bsim4.BSIM4gIgbb;
                    Ibtoteq = model.BSIM4type * (bsim4.BSIM4Igb - bsim4.BSIM4gIgbg * vgs - bsim4.BSIM4gIgbd * vds - bsim4.BSIM4gIgbb * vbs);
                }
                else
                    gIbtotg = gIbtotd = gIbtots = gIbtotb = Ibtoteq = 0.0;

                if ((model.BSIM4igcMod != 0) || (model.BSIM4igbMod != 0))
                {
                    gIgtotg = gIstotg + gIdtotg + gIbtotg;
                    gIgtotd = gIstotd + gIdtotd + gIbtotd;
                    gIgtots = gIstots + gIdtots + gIbtots;
                    gIgtotb = gIstotb + gIdtotb + gIbtotb;
                    Igtoteq = Istoteq + Idtoteq + Ibtoteq;
                }
                else
                    gIgtotg = gIgtotd = gIgtots = gIgtotb = Igtoteq = 0.0;

                if (bsim4.BSIM4rgateMod.Value == 2)
                    T0 = vges - vgs;
                else if (bsim4.BSIM4rgateMod.Value == 3)
                    T0 = vgms - vgs;
                if (bsim4.BSIM4rgateMod > 1)
                {
                    gcrgd = bsim4.BSIM4gcrgd * T0;
                    gcrgg = bsim4.BSIM4gcrgg * T0;
                    gcrgs = bsim4.BSIM4gcrgs * T0;
                    gcrgb = bsim4.BSIM4gcrgb * T0;
                    ceqgcrg = -(gcrgd * vds + gcrgg * vgs + gcrgb * vbs);
                    gcrgg -= bsim4.BSIM4gcrg;
                    gcrg = bsim4.BSIM4gcrg;
                }
                else
                    ceqgcrg = gcrg = gcrgd = gcrgg = gcrgs = gcrgb = 0.0;
            }
            else
            {
                Gm = -bsim4.BSIM4gm;
                Gmbs = -bsim4.BSIM4gmbs;
                FwdSum = 0.0;
                RevSum = -(Gm + Gmbs);

                ceqdrn = -model.BSIM4type * (cdrain + bsim4.BSIM4gds * vds + Gm * vgd + Gmbs * vbd);

                ceqbs = model.BSIM4type * (bsim4.BSIM4csub + bsim4.BSIM4Igisl + (bsim4.BSIM4gbds + bsim4.BSIM4ggisls) * vds - (bsim4.BSIM4gbgs + bsim4.BSIM4ggislg) * vgd -
                    (bsim4.BSIM4gbbs + bsim4.BSIM4ggislb) * vbd);
                ceqbd = model.BSIM4type * (bsim4.BSIM4Igidl - bsim4.BSIM4ggidld * vds - bsim4.BSIM4ggidlg * vgs - bsim4.BSIM4ggidlb * vbs);

                gbbsp = -(bsim4.BSIM4gbds);
                gbbdp = bsim4.BSIM4gbds + bsim4.BSIM4gbgs + bsim4.BSIM4gbbs;

                gbdpg = 0.0;
                gbdpsp = 0.0;
                gbdpb = 0.0;
                gbdpdp = 0.0;

                gbspg = bsim4.BSIM4gbgs;
                gbspsp = bsim4.BSIM4gbds;
                gbspb = bsim4.BSIM4gbbs;
                gbspdp = -(gbspg + gbspsp + gbspb);

                if (model.BSIM4igcMod != 0)
                {
                    gIstotg = bsim4.BSIM4gIgsg + bsim4.BSIM4gIgcdg;
                    gIstotd = bsim4.BSIM4gIgcds;
                    gIstots = bsim4.BSIM4gIgss + bsim4.BSIM4gIgcdd;
                    gIstotb = bsim4.BSIM4gIgcdb;
                    Istoteq = model.BSIM4type * (bsim4.BSIM4Igs + bsim4.BSIM4Igcd - bsim4.BSIM4gIgsg * vgs - bsim4.BSIM4gIgcdg * vgd + bsim4.BSIM4gIgcdd * vds - bsim4.BSIM4gIgcdb *
                        vbd);

                    gIdtotg = bsim4.BSIM4gIgdg + bsim4.BSIM4gIgcsg;
                    gIdtotd = bsim4.BSIM4gIgdd + bsim4.BSIM4gIgcss;
                    gIdtots = bsim4.BSIM4gIgcsd;
                    gIdtotb = bsim4.BSIM4gIgcsb;
                    Idtoteq = model.BSIM4type * (bsim4.BSIM4Igd + bsim4.BSIM4Igcs - (bsim4.BSIM4gIgdg + bsim4.BSIM4gIgcsg) * vgd + bsim4.BSIM4gIgcsd * vds - bsim4.BSIM4gIgcsb * vbd);
                }
                else
                {
                    gIstotg = gIstotd = gIstots = gIstotb = Istoteq = 0.0;
                    gIdtotg = gIdtotd = gIdtots = gIdtotb = Idtoteq = 0.0;
                }

                if (model.BSIM4igbMod != 0)
                {
                    gIbtotg = bsim4.BSIM4gIgbg;
                    gIbtotd = bsim4.BSIM4gIgbs;
                    gIbtots = bsim4.BSIM4gIgbd;
                    gIbtotb = bsim4.BSIM4gIgbb;
                    Ibtoteq = model.BSIM4type * (bsim4.BSIM4Igb - bsim4.BSIM4gIgbg * vgd + bsim4.BSIM4gIgbd * vds - bsim4.BSIM4gIgbb * vbd);
                }
                else
                    gIbtotg = gIbtotd = gIbtots = gIbtotb = Ibtoteq = 0.0;

                if ((model.BSIM4igcMod != 0) || (model.BSIM4igbMod != 0))
                {
                    gIgtotg = gIstotg + gIdtotg + gIbtotg;
                    gIgtotd = gIstotd + gIdtotd + gIbtotd;
                    gIgtots = gIstots + gIdtots + gIbtots;
                    gIgtotb = gIstotb + gIdtotb + gIbtotb;
                    Igtoteq = Istoteq + Idtoteq + Ibtoteq;
                }
                else
                    gIgtotg = gIgtotd = gIgtots = gIgtotb = Igtoteq = 0.0;

                if (bsim4.BSIM4rgateMod.Value == 2)
                    T0 = vges - vgs;
                else if (bsim4.BSIM4rgateMod.Value == 3)
                    T0 = vgms - vgs;
                if (bsim4.BSIM4rgateMod > 1)
                {
                    gcrgd = bsim4.BSIM4gcrgs * T0;
                    gcrgg = bsim4.BSIM4gcrgg * T0;
                    gcrgs = bsim4.BSIM4gcrgd * T0;
                    gcrgb = bsim4.BSIM4gcrgb * T0;
                    ceqgcrg = -(gcrgg * vgd - gcrgs * vds + gcrgb * vbd);
                    gcrgg -= bsim4.BSIM4gcrg;
                    gcrg = bsim4.BSIM4gcrg;
                }
                else
                    ceqgcrg = gcrg = gcrgd = gcrgg = gcrgs = gcrgb = 0.0;
            }

            if (model.BSIM4rdsMod.Value == 1)
            {
                ceqgstot = model.BSIM4type * (bsim4.BSIM4gstotd * vds + bsim4.BSIM4gstotg * vgs + bsim4.BSIM4gstotb * vbs);
                /* WDLiu: ceqgstot flowing away from sNodePrime */
                gstot = bsim4.BSIM4gstot;
                gstotd = bsim4.BSIM4gstotd;
                gstotg = bsim4.BSIM4gstotg;
                gstots = bsim4.BSIM4gstots - gstot;
                gstotb = bsim4.BSIM4gstotb;

                ceqgdtot = -model.BSIM4type * (bsim4.BSIM4gdtotd * vds + bsim4.BSIM4gdtotg * vgs + bsim4.BSIM4gdtotb * vbs);
                /* WDLiu: ceqgdtot defined as flowing into dNodePrime */
                gdtot = bsim4.BSIM4gdtot;
                gdtotd = bsim4.BSIM4gdtotd - gdtot;
                gdtotg = bsim4.BSIM4gdtotg;
                gdtots = bsim4.BSIM4gdtots;
                gdtotb = bsim4.BSIM4gdtotb;
            }
            else
            {
                gstot = gstotd = gstotg = gstots = gstotb = ceqgstot = 0.0;
                gdtot = gdtotd = gdtotg = gdtots = gdtotb = ceqgdtot = 0.0;
            }

            if (model.BSIM4type > 0)
            {
                ceqjs = (bsim4.BSIM4cbs - bsim4.BSIM4gbs * vbs_jct);
                ceqjd = (bsim4.BSIM4cbd - bsim4.BSIM4gbd * vbd_jct);
            }
            else
            {
                ceqjs = -(bsim4.BSIM4cbs - bsim4.BSIM4gbs * vbs_jct);
                ceqjd = -(bsim4.BSIM4cbd - bsim4.BSIM4gbd * vbd_jct);
                ceqqg = -ceqqg;
                ceqqd = -ceqqd;
                ceqqb = -ceqqb;
                ceqgcrg = -ceqgcrg;

                if (bsim4.BSIM4trnqsMod != 0)
                {
                    cqdef = -cqdef;
                    cqcheq = -cqcheq;
                }

                if (bsim4.BSIM4rbodyMod != 0)
                {
                    ceqqjs = -ceqqjs;
                    ceqqjd = -ceqqjd;
                }

                if (bsim4.BSIM4rgateMod.Value == 3)
                    ceqqgmid = -ceqqgmid;
            }

            /* 
            * Loading RHS
            */

            // rstate.Rhs[bsim4.BSIM4dNodePrime] += (ceqjd - ceqbd + ceqgdtot - ceqdrn - ceqqd + Idtoteq);
            // rstate.Rhs[bsim4.BSIM4gNodePrime] -= ceqqg - ceqgcrg + Igtoteq;

            if (bsim4.BSIM4rgateMod.Value == 2) { }
            // rstate.Rhs[bsim4.BSIM4gNodeExt] -= ceqgcrg;
            else if (bsim4.BSIM4rgateMod.Value == 3)
                // rstate.Rhs[bsim4.BSIM4gNodeMid] -= ceqqgmid + ceqgcrg;

                if (bsim4.BSIM4rbodyMod == 0)
                {
                    // rstate.Rhs[bsim4.BSIM4bNodePrime] += (ceqbd + ceqbs - ceqjd - ceqjs - ceqqb + Ibtoteq);
                    // rstate.Rhs[bsim4.BSIM4sNodePrime] += (ceqdrn - ceqbs + ceqjs + ceqqg + ceqqb + ceqqd + ceqqgmid - ceqgstot + Istoteq);
                }
                else
                {
                    // rstate.Rhs[bsim4.BSIM4dbNode] -= (ceqjd + ceqqjd);
                    // rstate.Rhs[bsim4.BSIM4bNodePrime] += (ceqbd + ceqbs - ceqqb + Ibtoteq);
                    // rstate.Rhs[bsim4.BSIM4sbNode] -= (ceqjs + ceqqjs);
                    // rstate.Rhs[bsim4.BSIM4sNodePrime] += (ceqdrn - ceqbs + ceqjs + ceqqd + ceqqg + ceqqb + ceqqjd + ceqqjs + ceqqgmid - ceqgstot + Istoteq);
                }

            if (model.BSIM4rdsMod != 0)
            {
                // rstate.Rhs[bsim4.BSIM4dNode] -= ceqgdtot;
                // rstate.Rhs[bsim4.BSIM4sNode] += ceqgstot;
            }

            if (bsim4.BSIM4trnqsMod != 0)
                // rstate.Rhs[bsim4.BSIM4qNode] += (cqcheq - cqdef);

            /* 
            * Loading matrix
            */

            if (bsim4.BSIM4rbodyMod == 0)
            {
                gjbd = bsim4.BSIM4gbd;
                gjbs = bsim4.BSIM4gbs;
            }
            else
                gjbd = gjbs = 0.0;

            if (model.BSIM4rdsMod == 0)
            {
                gdpr = bsim4.BSIM4drainConductance;
                gspr = bsim4.BSIM4sourceConductance;
            }
            else
                gdpr = gspr = 0.0;

            geltd = bsim4.BSIM4grgeltd;

            T1 = qdef * bsim4.BSIM4gtau;

            if (bsim4.BSIM4rgateMod.Value == 1)
            {
                // rstate.Matrix[bsim4.BSIM4gNodeExt, bsim4.BSIM4gNodeExt] += geltd;
                // rstate.Matrix[bsim4.BSIM4gNodePrime, bsim4.BSIM4gNodeExt] -= geltd;
                // rstate.Matrix[bsim4.BSIM4gNodeExt, bsim4.BSIM4gNodePrime] -= geltd;
                // rstate.Matrix[bsim4.BSIM4gNodePrime, bsim4.BSIM4gNodePrime] += gcggb + geltd - ggtg + gIgtotg;
                // rstate.Matrix[bsim4.BSIM4gNodePrime, bsim4.BSIM4dNodePrime] += gcgdb - ggtd + gIgtotd;
                // rstate.Matrix[bsim4.BSIM4gNodePrime, bsim4.BSIM4sNodePrime] += gcgsb - ggts + gIgtots;
                // rstate.Matrix[bsim4.BSIM4gNodePrime, bsim4.BSIM4bNodePrime] += gcgbb - ggtb + gIgtotb;
            } /* WDLiu: gcrg already subtracted from all gcrgg below */
            else if (bsim4.BSIM4rgateMod.Value == 2)
            {
                // rstate.Matrix[bsim4.BSIM4gNodeExt, bsim4.BSIM4gNodeExt] += gcrg;
                // rstate.Matrix[bsim4.BSIM4gNodeExt, bsim4.BSIM4gNodePrime] += gcrgg;
                // rstate.Matrix[bsim4.BSIM4gNodeExt, bsim4.BSIM4dNodePrime] += gcrgd;
                // rstate.Matrix[bsim4.BSIM4gNodeExt, bsim4.BSIM4sNodePrime] += gcrgs;
                // rstate.Matrix[bsim4.BSIM4gNodeExt, bsim4.BSIM4bNodePrime] += gcrgb;

                // rstate.Matrix[bsim4.BSIM4gNodePrime, bsim4.BSIM4gNodeExt] -= gcrg;
                // rstate.Matrix[bsim4.BSIM4gNodePrime, bsim4.BSIM4gNodePrime] += gcggb - gcrgg - ggtg + gIgtotg;
                // rstate.Matrix[bsim4.BSIM4gNodePrime, bsim4.BSIM4dNodePrime] += gcgdb - gcrgd - ggtd + gIgtotd;
                // rstate.Matrix[bsim4.BSIM4gNodePrime, bsim4.BSIM4sNodePrime] += gcgsb - gcrgs - ggts + gIgtots;
                // rstate.Matrix[bsim4.BSIM4gNodePrime, bsim4.BSIM4bNodePrime] += gcgbb - gcrgb - ggtb + gIgtotb;
            }
            else if (bsim4.BSIM4rgateMod.Value == 3)
            {
                // rstate.Matrix[bsim4.BSIM4gNodeExt, bsim4.BSIM4gNodeExt] += geltd;
                // rstate.Matrix[bsim4.BSIM4gNodeExt, bsim4.BSIM4gNodeMid] -= geltd;
                // rstate.Matrix[bsim4.BSIM4gNodeMid, bsim4.BSIM4gNodeExt] -= geltd;
                // rstate.Matrix[bsim4.BSIM4gNodeMid, bsim4.BSIM4gNodeMid] += geltd + gcrg + gcgmgmb;

                // rstate.Matrix[bsim4.BSIM4gNodeMid, bsim4.BSIM4dNodePrime] += gcrgd + gcgmdb;
                // rstate.Matrix[bsim4.BSIM4gNodeMid, bsim4.BSIM4gNodePrime] += gcrgg;
                // rstate.Matrix[bsim4.BSIM4gNodeMid, bsim4.BSIM4sNodePrime] += gcrgs + gcgmsb;
                // rstate.Matrix[bsim4.BSIM4gNodeMid, bsim4.BSIM4bNodePrime] += gcrgb + gcgmbb;

                // rstate.Matrix[bsim4.BSIM4dNodePrime, bsim4.BSIM4gNodeMid] += gcdgmb;
                // rstate.Matrix[bsim4.BSIM4gNodePrime, bsim4.BSIM4gNodeMid] -= gcrg;
                // rstate.Matrix[bsim4.BSIM4sNodePrime, bsim4.BSIM4gNodeMid] += gcsgmb;
                // rstate.Matrix[bsim4.BSIM4bNodePrime, bsim4.BSIM4gNodeMid] += gcbgmb;

                // rstate.Matrix[bsim4.BSIM4gNodePrime, bsim4.BSIM4gNodePrime] += gcggb - gcrgg - ggtg + gIgtotg;
                // rstate.Matrix[bsim4.BSIM4gNodePrime, bsim4.BSIM4dNodePrime] += gcgdb - gcrgd - ggtd + gIgtotd;
                // rstate.Matrix[bsim4.BSIM4gNodePrime, bsim4.BSIM4sNodePrime] += gcgsb - gcrgs - ggts + gIgtots;
                // rstate.Matrix[bsim4.BSIM4gNodePrime, bsim4.BSIM4bNodePrime] += gcgbb - gcrgb - ggtb + gIgtotb;
            }
            else
            {
                // rstate.Matrix[bsim4.BSIM4gNodePrime, bsim4.BSIM4gNodePrime] += gcggb - ggtg + gIgtotg;
                // rstate.Matrix[bsim4.BSIM4gNodePrime, bsim4.BSIM4dNodePrime] += gcgdb - ggtd + gIgtotd;
                // rstate.Matrix[bsim4.BSIM4gNodePrime, bsim4.BSIM4sNodePrime] += gcgsb - ggts + gIgtots;
                // rstate.Matrix[bsim4.BSIM4gNodePrime, bsim4.BSIM4bNodePrime] += gcgbb - ggtb + gIgtotb;
            }

            if (model.BSIM4rdsMod != 0)
            {
                // rstate.Matrix[bsim4.BSIM4dNode, bsim4.BSIM4gNodePrime] += gdtotg;
                // rstate.Matrix[bsim4.BSIM4dNode, bsim4.BSIM4sNodePrime] += gdtots;
                // rstate.Matrix[bsim4.BSIM4dNode, bsim4.BSIM4bNodePrime] += gdtotb;
                // rstate.Matrix[bsim4.BSIM4sNode, bsim4.BSIM4dNodePrime] += gstotd;
                // rstate.Matrix[bsim4.BSIM4sNode, bsim4.BSIM4gNodePrime] += gstotg;
                // rstate.Matrix[bsim4.BSIM4sNode, bsim4.BSIM4bNodePrime] += gstotb;
            }

            // rstate.Matrix[bsim4.BSIM4dNodePrime, bsim4.BSIM4dNodePrime] += gdpr + bsim4.BSIM4gds + bsim4.BSIM4gbd + T1 * ddxpart_dVd - gdtotd + RevSum + gcddb + gbdpdp + dxpart * ggtd - gIdtotd;
            // rstate.Matrix[bsim4.BSIM4dNodePrime, bsim4.BSIM4dNode] -= gdpr + gdtot;
            // rstate.Matrix[bsim4.BSIM4dNodePrime, bsim4.BSIM4gNodePrime] += Gm + gcdgb - gdtotg + gbdpg - gIdtotg + dxpart * ggtg + T1 * ddxpart_dVg;
            // rstate.Matrix[bsim4.BSIM4dNodePrime, bsim4.BSIM4sNodePrime] -= bsim4.BSIM4gds + gdtots - dxpart * ggts + gIdtots - T1 * ddxpart_dVs + FwdSum - gcdsb - gbdpsp;
            // rstate.Matrix[bsim4.BSIM4dNodePrime, bsim4.BSIM4bNodePrime] -= gjbd + gdtotb - Gmbs - gcdbb - gbdpb + gIdtotb - T1 * ddxpart_dVb - dxpart * ggtb;

            // rstate.Matrix[bsim4.BSIM4dNode, bsim4.BSIM4dNodePrime] -= gdpr - gdtotd;
            // rstate.Matrix[bsim4.BSIM4dNode, bsim4.BSIM4dNode] += gdpr + gdtot;

            // rstate.Matrix[bsim4.BSIM4sNodePrime, bsim4.BSIM4dNodePrime] -= bsim4.BSIM4gds + gstotd + RevSum - gcsdb - gbspdp - T1 * dsxpart_dVd - sxpart * ggtd + gIstotd;
            // rstate.Matrix[bsim4.BSIM4sNodePrime, bsim4.BSIM4gNodePrime] += gcsgb - Gm - gstotg + gbspg + sxpart * ggtg + T1 * dsxpart_dVg - gIstotg;
            // rstate.Matrix[bsim4.BSIM4sNodePrime, bsim4.BSIM4sNodePrime] += gspr + bsim4.BSIM4gds + bsim4.BSIM4gbs + T1 * dsxpart_dVs - gstots + FwdSum + gcssb + gbspsp + sxpart * ggts - gIstots;
            // rstate.Matrix[bsim4.BSIM4sNodePrime, bsim4.BSIM4sNode] -= gspr + gstot;
            // rstate.Matrix[bsim4.BSIM4sNodePrime, bsim4.BSIM4bNodePrime] -= gjbs + gstotb + Gmbs - gcsbb - gbspb - sxpart * ggtb - T1 * dsxpart_dVb + gIstotb;

            // rstate.Matrix[bsim4.BSIM4sNode, bsim4.BSIM4sNodePrime] -= gspr - gstots;
            // rstate.Matrix[bsim4.BSIM4sNode, bsim4.BSIM4sNode] += gspr + gstot;

            // rstate.Matrix[bsim4.BSIM4bNodePrime, bsim4.BSIM4dNodePrime] += gcbdb - gjbd + gbbdp - gIbtotd;
            // rstate.Matrix[bsim4.BSIM4bNodePrime, bsim4.BSIM4gNodePrime] += gcbgb - bsim4.BSIM4gbgs - gIbtotg;
            // rstate.Matrix[bsim4.BSIM4bNodePrime, bsim4.BSIM4sNodePrime] += gcbsb - gjbs + gbbsp - gIbtots;
            // rstate.Matrix[bsim4.BSIM4bNodePrime, bsim4.BSIM4bNodePrime] += gjbd + gjbs + gcbbb - bsim4.BSIM4gbbs - gIbtotb;

            ggidld = bsim4.BSIM4ggidld;
            ggidlg = bsim4.BSIM4ggidlg;
            ggidlb = bsim4.BSIM4ggidlb;
            ggislg = bsim4.BSIM4ggislg;
            ggisls = bsim4.BSIM4ggisls;
            ggislb = bsim4.BSIM4ggislb;

            /* stamp gidl */
            // rstate.Matrix[bsim4.BSIM4dNodePrime, bsim4.BSIM4dNodePrime] += ggidld;
            // rstate.Matrix[bsim4.BSIM4dNodePrime, bsim4.BSIM4gNodePrime] += ggidlg;
            // rstate.Matrix[bsim4.BSIM4dNodePrime, bsim4.BSIM4sNodePrime] -= (ggidlg + ggidld + ggidlb);
            // rstate.Matrix[bsim4.BSIM4dNodePrime, bsim4.BSIM4bNodePrime] += ggidlb;
            // rstate.Matrix[bsim4.BSIM4bNodePrime, bsim4.BSIM4dNodePrime] -= ggidld;
            // rstate.Matrix[bsim4.BSIM4bNodePrime, bsim4.BSIM4gNodePrime] -= ggidlg;
            // rstate.Matrix[bsim4.BSIM4bNodePrime, bsim4.BSIM4sNodePrime] += (ggidlg + ggidld + ggidlb);
            // rstate.Matrix[bsim4.BSIM4bNodePrime, bsim4.BSIM4bNodePrime] -= ggidlb;
            /* stamp gisl */
            // rstate.Matrix[bsim4.BSIM4sNodePrime, bsim4.BSIM4dNodePrime] -= (ggisls + ggislg + ggislb);
            // rstate.Matrix[bsim4.BSIM4sNodePrime, bsim4.BSIM4gNodePrime] += ggislg;
            // rstate.Matrix[bsim4.BSIM4sNodePrime, bsim4.BSIM4sNodePrime] += ggisls;
            // rstate.Matrix[bsim4.BSIM4sNodePrime, bsim4.BSIM4bNodePrime] += ggislb;
            // rstate.Matrix[bsim4.BSIM4bNodePrime, bsim4.BSIM4dNodePrime] += (ggislg + ggisls + ggislb);
            // rstate.Matrix[bsim4.BSIM4bNodePrime, bsim4.BSIM4gNodePrime] -= ggislg;
            // rstate.Matrix[bsim4.BSIM4bNodePrime, bsim4.BSIM4sNodePrime] -= ggisls;
            // rstate.Matrix[bsim4.BSIM4bNodePrime, bsim4.BSIM4bNodePrime] -= ggislb;

            if (bsim4.BSIM4rbodyMod != 0)
            {
                // rstate.Matrix[bsim4.BSIM4dNodePrime, bsim4.BSIM4dbNode] += gcdbdb - bsim4.BSIM4gbd;
                // rstate.Matrix[bsim4.BSIM4sNodePrime, bsim4.BSIM4sbNode] -= bsim4.BSIM4gbs - gcsbsb;

                // rstate.Matrix[bsim4.BSIM4dbNode, bsim4.BSIM4dNodePrime] += gcdbdb - bsim4.BSIM4gbd;
                // rstate.Matrix[bsim4.BSIM4dbNode, bsim4.BSIM4dbNode] += bsim4.BSIM4gbd - gcdbdb + bsim4.BSIM4grbpd + bsim4.BSIM4grbdb;
                // rstate.Matrix[bsim4.BSIM4dbNode, bsim4.BSIM4bNodePrime] -= bsim4.BSIM4grbpd;
                // rstate.Matrix[bsim4.BSIM4dbNode, bsim4.BSIM4bNode] -= bsim4.BSIM4grbdb;

                // rstate.Matrix[bsim4.BSIM4bNodePrime, bsim4.BSIM4dbNode] -= bsim4.BSIM4grbpd;
                // rstate.Matrix[bsim4.BSIM4bNodePrime, bsim4.BSIM4bNode] -= bsim4.BSIM4grbpb;
                // rstate.Matrix[bsim4.BSIM4bNodePrime, bsim4.BSIM4sbNode] -= bsim4.BSIM4grbps;
                // rstate.Matrix[bsim4.BSIM4bNodePrime, bsim4.BSIM4bNodePrime] += bsim4.BSIM4grbpd + bsim4.BSIM4grbps + bsim4.BSIM4grbpb;
                /* WDLiu: (gcbbb - bsim4.BSIM4gbbs) already added to BPbpPtr */

                // rstate.Matrix[bsim4.BSIM4sbNode, bsim4.BSIM4sNodePrime] += gcsbsb - bsim4.BSIM4gbs;
                // rstate.Matrix[bsim4.BSIM4sbNode, bsim4.BSIM4bNodePrime] -= bsim4.BSIM4grbps;
                // rstate.Matrix[bsim4.BSIM4sbNode, bsim4.BSIM4bNode] -= bsim4.BSIM4grbsb;
                // rstate.Matrix[bsim4.BSIM4sbNode, bsim4.BSIM4sbNode] += bsim4.BSIM4gbs - gcsbsb + bsim4.BSIM4grbps + bsim4.BSIM4grbsb;

                // rstate.Matrix[bsim4.BSIM4bNode, bsim4.BSIM4dbNode] -= bsim4.BSIM4grbdb;
                // rstate.Matrix[bsim4.BSIM4bNode, bsim4.BSIM4bNodePrime] -= bsim4.BSIM4grbpb;
                // rstate.Matrix[bsim4.BSIM4bNode, bsim4.BSIM4sbNode] -= bsim4.BSIM4grbsb;
                // rstate.Matrix[bsim4.BSIM4bNode, bsim4.BSIM4bNode] += bsim4.BSIM4grbsb + bsim4.BSIM4grbdb + bsim4.BSIM4grbpb;
            }

            if (bsim4.BSIM4trnqsMod != 0)
            {
                // rstate.Matrix[bsim4.BSIM4qNode, bsim4.BSIM4qNode] += gqdef + bsim4.BSIM4gtau;
                // rstate.Matrix[bsim4.BSIM4qNode, bsim4.BSIM4gNodePrime] += ggtg - gcqgb;
                // rstate.Matrix[bsim4.BSIM4qNode, bsim4.BSIM4dNodePrime] += ggtd - gcqdb;
                // rstate.Matrix[bsim4.BSIM4qNode, bsim4.BSIM4sNodePrime] += ggts - gcqsb;
                // rstate.Matrix[bsim4.BSIM4qNode, bsim4.BSIM4bNodePrime] += ggtb - gcqbb;

                // rstate.Matrix[bsim4.BSIM4dNodePrime, bsim4.BSIM4qNode] += dxpart * bsim4.BSIM4gtau;
                // rstate.Matrix[bsim4.BSIM4sNodePrime, bsim4.BSIM4qNode] += sxpart * bsim4.BSIM4gtau;
                // rstate.Matrix[bsim4.BSIM4gNodePrime, bsim4.BSIM4qNode] -= bsim4.BSIM4gtau;
            }

            line1000:;
        }
    }
}
