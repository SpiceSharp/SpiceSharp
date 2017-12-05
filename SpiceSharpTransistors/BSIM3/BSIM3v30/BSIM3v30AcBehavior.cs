using System;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// AC behaviour for a <see cref="BSIM3v30"/>
    /// </summary>
    public class BSIM3v30AcBehavior : CircuitObjectBehaviorAcLoad
    {
        /// <summary>
        /// Execute behaviour
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Load(Circuit ckt)
        {
            var bsim3 = ComponentTyped<BSIM3v30>();
            var model = bsim3.Model as BSIM3v30Model;
            var state = ckt.State;
            var cstate = state;
            double omega = cstate.Laplace.Imaginary;
            double Csd, Csg, Css, T0, T1, T2, T3, gmr, gmbsr, gds, gmi, gmbsi, gdsi, Cddr, Cdgr, Cdsr, Cddi, Cdgi, Cdsi, Cdbi, Csdr, Csgr,
                Cssr, Csdi, Csgi, Cssi, Csbi, Cgdr, Cggr, Cgsr, Cgdi, Cggi, Cgsi, Cgbi, Gm, Gmbs, FwdSum, RevSum, Gmi, Gmbsi, FwdSumi, RevSumi,
                gbbdp, gbbsp, gbdpg, gbdpb, gbdpdp, gbdpsp, gbspdp, gbspg, gbspb, gbspsp, cggb, cgsb, cgdb, cbgb, cbsb, cbdb, cdgb, cdsb, cddb,
                xgtg, sxpart, dxpart, ddxpart_dVd, dsxpart_dVd, xgtd, xgts, xgtb, xcqgb = 0.0, xcqdb = 0.0, xcqsb = 0.0, xcqbb = 0.0, CoxWL, qcheq, Cdd, Cdg,
                ddxpart_dVg, Cds, ddxpart_dVs, ddxpart_dVb, dsxpart_dVg, dsxpart_dVs, dsxpart_dVb, xcdgbi, xcsgbi, xcddbi, xcdsbi, xcsdbi,
                xcssbi, xcdbbi, xcsbbi, xcggbi, xcgdbi, xcgsbi, xcgbbi, gdpr, gspr, gbd, gbs, capbd, capbs, GSoverlapCap, GDoverlapCap,
                GBoverlapCap, xcdgb, xcddb, xcdsb, xcsgb, xcsdb, xcssb, xcggb, xcgdb, xcgsb, xcbgb, xcbdb, xcbsb;

            Csd = -(bsim3.BSIM3cddb + bsim3.BSIM3cgdb + bsim3.BSIM3cbdb);
            Csg = -(bsim3.BSIM3cdgb + bsim3.BSIM3cggb + bsim3.BSIM3cbgb);
            Css = -(bsim3.BSIM3cdsb + bsim3.BSIM3cgsb + bsim3.BSIM3cbsb);

            if (bsim3.BSIM3acnqsMod != 0.0)
            {
                T0 = omega * bsim3.BSIM3taunet;
                T1 = T0 * T0;
                T2 = 1.0 / (1.0 + T1);
                T3 = T0 * T2;

                gmr = bsim3.BSIM3gm * T2;
                gmbsr = bsim3.BSIM3gmbs * T2;
                gds = bsim3.BSIM3gds * T2;

                gmi = -bsim3.BSIM3gm * T3;
                gmbsi = -bsim3.BSIM3gmbs * T3;
                gdsi = -bsim3.BSIM3gds * T3;

                Cddr = bsim3.BSIM3cddb * T2;
                Cdgr = bsim3.BSIM3cdgb * T2;
                Cdsr = bsim3.BSIM3cdsb * T2;

                Cddi = bsim3.BSIM3cddb * T3 * omega;
                Cdgi = bsim3.BSIM3cdgb * T3 * omega;
                Cdsi = bsim3.BSIM3cdsb * T3 * omega;
                Cdbi = -(Cddi + Cdgi + Cdsi);

                Csdr = Csd * T2;
                Csgr = Csg * T2;
                Cssr = Css * T2;

                Csdi = Csd * T3 * omega;
                Csgi = Csg * T3 * omega;
                Cssi = Css * T3 * omega;
                Csbi = -(Csdi + Csgi + Cssi);

                Cgdr = -(Cddr + Csdr + bsim3.BSIM3cbdb);
                Cggr = -(Cdgr + Csgr + bsim3.BSIM3cbgb);
                Cgsr = -(Cdsr + Cssr + bsim3.BSIM3cbsb);

                Cgdi = -(Cddi + Csdi);
                Cggi = -(Cdgi + Csgi);
                Cgsi = -(Cdsi + Cssi);
                Cgbi = -(Cgdi + Cggi + Cgsi);
            }
            else /* QS */
            {
                gmr = bsim3.BSIM3gm;
                gmbsr = bsim3.BSIM3gmbs;
                gds = bsim3.BSIM3gds;
                gmi = gmbsi = gdsi = 0.0;

                Cddr = bsim3.BSIM3cddb;
                Cdgr = bsim3.BSIM3cdgb;
                Cdsr = bsim3.BSIM3cdsb;
                Cddi = Cdgi = Cdsi = Cdbi = 0.0;

                Csdr = Csd;
                Csgr = Csg;
                Cssr = Css;
                Csdi = Csgi = Cssi = Csbi = 0.0;

                Cgdr = bsim3.BSIM3cgdb;
                Cggr = bsim3.BSIM3cggb;
                Cgsr = bsim3.BSIM3cgsb;
                Cgdi = Cggi = Cgsi = Cgbi = 0.0;
            }
            if (bsim3.BSIM3mode >= 0)
            {
                Gm = gmr;
                Gmbs = gmbsr;
                FwdSum = Gm + Gmbs;
                RevSum = 0.0;
                Gmi = gmi;
                Gmbsi = gmbsi;
                FwdSumi = Gmi + Gmbsi;
                RevSumi = 0.0;

                gbbdp = -bsim3.BSIM3gbds;
                gbbsp = bsim3.BSIM3gbds + bsim3.BSIM3gbgs + bsim3.BSIM3gbbs;

                gbdpg = bsim3.BSIM3gbgs;
                gbdpb = bsim3.BSIM3gbbs;
                gbdpdp = bsim3.BSIM3gbds;
                gbdpsp = -(gbdpg + gbdpb + gbdpdp);

                gbspdp = 0.0;
                gbspg = 0.0;
                gbspb = 0.0;
                gbspsp = 0.0;

                if (bsim3.BSIM3nqsMod.Value == 0 || bsim3.BSIM3acnqsMod.Value == 1)
                {
                    cggb = Cggr;
                    cgsb = Cgsr;
                    cgdb = Cgdr;

                    cbgb = bsim3.BSIM3cbgb;
                    cbsb = bsim3.BSIM3cbsb;
                    cbdb = bsim3.BSIM3cbdb;

                    cdgb = Cdgr;
                    cdsb = Cdsr;
                    cddb = Cddr;

                    xgtg = xgtd = xgts = xgtb = 0.0;
                    sxpart = 0.6;
                    dxpart = 0.4;
                    ddxpart_dVd = ddxpart_dVg = ddxpart_dVb = ddxpart_dVs = 0.0;
                    dsxpart_dVd = dsxpart_dVg = dsxpart_dVb = dsxpart_dVs = 0.0;
                }
                else
                {
                    cggb = cgdb = cgsb = 0.0;
                    cbgb = cbdb = cbsb = 0.0;
                    cdgb = cddb = cdsb = 0.0;

                    xgtg = bsim3.BSIM3gtg;
                    xgtd = bsim3.BSIM3gtd;
                    xgts = bsim3.BSIM3gts;
                    xgtb = bsim3.BSIM3gtb;

                    xcqgb = bsim3.BSIM3cqgb * omega;
                    xcqdb = bsim3.BSIM3cqdb * omega;
                    xcqsb = bsim3.BSIM3cqsb * omega;
                    xcqbb = bsim3.BSIM3cqbb * omega;

                    CoxWL = model.BSIM3cox * bsim3.pParam.BSIM3weffCV * bsim3.pParam.BSIM3leffCV;
                    qcheq = -(bsim3.BSIM3qgate + bsim3.BSIM3qbulk);
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
                        dxpart = bsim3.BSIM3qdrn / qcheq;
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
                }
                xcdgbi = Cdgi;
                xcsgbi = Csgi;
                xcddbi = Cddi;
                xcdsbi = Cdsi;
                xcsdbi = Csdi;
                xcssbi = Cssi;
                xcdbbi = Cdbi;
                xcsbbi = Csbi;
                xcggbi = Cggi;
                xcgdbi = Cgdi;
                xcgsbi = Cgsi;
                xcgbbi = Cgbi;
            }
            else
            {
                Gm = -gmr;
                Gmbs = -gmbsr;
                FwdSum = 0.0;
                RevSum = -(Gm + Gmbs);
                Gmi = -gmi;
                Gmbsi = -gmbsi;
                FwdSumi = 0.0;
                RevSumi = -(Gmi + Gmbsi);

                gbbsp = -bsim3.BSIM3gbds;
                gbbdp = bsim3.BSIM3gbds + bsim3.BSIM3gbgs + bsim3.BSIM3gbbs;

                gbdpg = 0.0;
                gbdpsp = 0.0;
                gbdpb = 0.0;
                gbdpdp = 0.0;

                gbspg = bsim3.BSIM3gbgs;
                gbspsp = bsim3.BSIM3gbds;
                gbspb = bsim3.BSIM3gbbs;
                gbspdp = -(gbspg + gbspsp + gbspb);

                if (bsim3.BSIM3nqsMod.Value == 0 || bsim3.BSIM3acnqsMod.Value == 1)
                {
                    cggb = Cggr;
                    cgsb = Cgdr;
                    cgdb = Cgsr;

                    cbgb = bsim3.BSIM3cbgb;
                    cbsb = bsim3.BSIM3cbdb;
                    cbdb = bsim3.BSIM3cbsb;

                    cdgb = -(Cdgr + cggb + cbgb);
                    cdsb = -(Cddr + cgsb + cbsb);
                    cddb = -(Cdsr + cgdb + cbdb);

                    xgtg = xgtd = xgts = xgtb = 0.0;
                    sxpart = 0.4;
                    dxpart = 0.6;
                    ddxpart_dVd = ddxpart_dVg = ddxpart_dVb = ddxpart_dVs = 0.0;
                    dsxpart_dVd = dsxpart_dVg = dsxpart_dVb = dsxpart_dVs = 0.0;
                }
                else
                {
                    cggb = cgdb = cgsb = 0.0;
                    cbgb = cbdb = cbsb = 0.0;
                    cdgb = cddb = cdsb = 0.0;

                    xgtg = bsim3.BSIM3gtg;
                    xgtd = bsim3.BSIM3gts;
                    xgts = bsim3.BSIM3gtd;
                    xgtb = bsim3.BSIM3gtb;

                    xcqgb = bsim3.BSIM3cqgb * omega;
                    xcqdb = bsim3.BSIM3cqsb * omega;
                    xcqsb = bsim3.BSIM3cqdb * omega;
                    xcqbb = bsim3.BSIM3cqbb * omega;

                    CoxWL = model.BSIM3cox * bsim3.pParam.BSIM3weffCV * bsim3.pParam.BSIM3leffCV;
                    qcheq = -(bsim3.BSIM3qgate + bsim3.BSIM3qbulk);
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
                        sxpart = bsim3.BSIM3qdrn / qcheq;
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
                }
                xcdgbi = Csgi;
                xcsgbi = Cdgi;
                xcddbi = Cssi;
                xcdsbi = Csdi;
                xcsdbi = Cdsi;
                xcssbi = Cddi;
                xcdbbi = Csbi;
                xcsbbi = Cdbi;
                xcggbi = Cggi;
                xcgdbi = Cgsi;
                xcgsbi = Cgdi;
                xcgbbi = Cgbi;
            }

            T1 = state.States[0][bsim3.BSIM3states + BSIM3v30.BSIM3qdef] * bsim3.BSIM3gtau;
            gdpr = bsim3.BSIM3drainConductance;
            gspr = bsim3.BSIM3sourceConductance;
            gbd = bsim3.BSIM3gbd;
            gbs = bsim3.BSIM3gbs;
            capbd = bsim3.BSIM3capbd;
            capbs = bsim3.BSIM3capbs;

            GSoverlapCap = bsim3.BSIM3cgso;
            GDoverlapCap = bsim3.BSIM3cgdo;
            GBoverlapCap = bsim3.pParam.BSIM3cgbo;

            xcdgb = (cdgb - GDoverlapCap) * omega;
            xcddb = (cddb + capbd + GDoverlapCap) * omega;
            xcdsb = cdsb * omega;
            xcsgb = -(cggb + cbgb + cdgb + GSoverlapCap) * omega;
            xcsdb = -(cgdb + cbdb + cddb) * omega;
            xcssb = (capbs + GSoverlapCap - (cgsb + cbsb + cdsb)) * omega;
            xcggb = (cggb + GDoverlapCap + GSoverlapCap + GBoverlapCap) * omega;
            xcgdb = (cgdb - GDoverlapCap) * omega;
            xcgsb = (cgsb - GSoverlapCap) * omega;
            xcbgb = (cbgb - GBoverlapCap) * omega;
            xcbdb = (cbdb - capbd) * omega;
            xcbsb = (cbsb - capbs) * omega;

            // cstate.Matrix[bsim3.BSIM3gNode, bsim3.BSIM3gNode] -= new Complex(xgtg - xcggbi, -xcggb);
            // cstate.Matrix[bsim3.BSIM3bNode, bsim3.BSIM3bNode] += new Complex(gbd + gbs - bsim3.BSIM3gbbs, -(xcbgb + xcbdb + xcbsb));
            // cstate.Matrix[bsim3.BSIM3dNodePrime, bsim3.BSIM3dNodePrime] += new Complex(gdpr + gds + gbd + RevSum + xcddbi + dxpart * xgtd + T1 * ddxpart_dVd + gbdpdp, xcddb + gdsi + RevSumi);
            // cstate.Matrix[bsim3.BSIM3sNodePrime, bsim3.BSIM3sNodePrime] += new Complex(gspr + gds + gbs + FwdSum + xcssbi + sxpart * xgts + T1 * dsxpart_dVs + gbspsp, xcssb + gdsi + FwdSumi);
            // cstate.Matrix[bsim3.BSIM3gNode, bsim3.BSIM3bNode] -= new Complex(xgtb - xcgbbi, xcggb + xcgdb + xcgsb);
            // cstate.Matrix[bsim3.BSIM3gNode, bsim3.BSIM3dNodePrime] -= new Complex(xgtd - xcgdbi, -xcgdb);
            // cstate.Matrix[bsim3.BSIM3gNode, bsim3.BSIM3sNodePrime] -= new Complex(xgts - xcgsbi, -xcgsb);
            // cstate.Matrix[bsim3.BSIM3bNode, bsim3.BSIM3gNode] -= new Complex(bsim3.BSIM3gbgs, -xcbgb);
            // cstate.Matrix[bsim3.BSIM3bNode, bsim3.BSIM3dNodePrime] -= new Complex(gbd - gbbdp, -xcbdb);
            // cstate.Matrix[bsim3.BSIM3bNode, bsim3.BSIM3sNodePrime] -= new Complex(gbs - gbbsp, -xcbsb);
            // cstate.Matrix[bsim3.BSIM3dNodePrime, bsim3.BSIM3gNode] += new Complex(Gm + dxpart * xgtg + T1 * ddxpart_dVg + gbdpg + xcdgbi, xcdgb + Gmi);
            // cstate.Matrix[bsim3.BSIM3dNodePrime, bsim3.BSIM3bNode] -= new Complex(gbd - Gmbs - dxpart * xgtb - T1 * ddxpart_dVb - gbdpb - xcdbbi, xcdgb + xcddb + xcdsb + Gmbsi);
            // cstate.Matrix[bsim3.BSIM3dNodePrime, bsim3.BSIM3sNodePrime] -= new Complex(gds + FwdSum - dxpart * xgts - T1 * ddxpart_dVs - gbdpsp - xcdsbi, -(xcdsb - gdsi - FwdSumi));
            // cstate.Matrix[bsim3.BSIM3sNodePrime, bsim3.BSIM3gNode] -= new Complex(Gm - sxpart * xgtg - T1 * dsxpart_dVg - gbspg - xcsgbi, -(xcsgb - Gmi));
            // cstate.Matrix[bsim3.BSIM3sNodePrime, bsim3.BSIM3bNode] -= new Complex(gbs + Gmbs - sxpart * xgtb - T1 * dsxpart_dVb - gbspb - xcsbbi, xcsgb + xcsdb + xcssb - Gmbsi);
            // cstate.Matrix[bsim3.BSIM3sNodePrime, bsim3.BSIM3dNodePrime] -= new Complex(gds + RevSum - sxpart * xgtd - T1 * dsxpart_dVd - gbspdp - xcsdbi, -(xcsdb - gdsi - RevSumi));

            // cstate.Matrix[bsim3.BSIM3dNode, bsim3.BSIM3dNode] += gdpr;
            // cstate.Matrix[bsim3.BSIM3sNode, bsim3.BSIM3sNode] += gspr;

            // cstate.Matrix[bsim3.BSIM3dNode, bsim3.BSIM3dNodePrime] -= gdpr;
            // cstate.Matrix[bsim3.BSIM3sNode, bsim3.BSIM3sNodePrime] -= gspr;

            // cstate.Matrix[bsim3.BSIM3dNodePrime, bsim3.BSIM3dNode] -= gdpr;

            // cstate.Matrix[bsim3.BSIM3sNodePrime, bsim3.BSIM3sNode] -= gspr;

            if (bsim3.BSIM3nqsMod != 0.0)
            {
                if (bsim3.BSIM3acnqsMod != 0.0)
                {
                    // cstate.Matrix[bsim3.BSIM3qNode, bsim3.BSIM3qNode] += 1.0;
                    // cstate.Matrix[bsim3.BSIM3qNode, bsim3.BSIM3gNode] += 0.0;
                    // cstate.Matrix[bsim3.BSIM3qNode, bsim3.BSIM3dNodePrime] += 0.0;
                    // cstate.Matrix[bsim3.BSIM3qNode, bsim3.BSIM3sNodePrime] += 0.0;
                    // cstate.Matrix[bsim3.BSIM3qNode, bsim3.BSIM3bNode] += 0.0;

                    // cstate.Matrix[bsim3.BSIM3dNodePrime, bsim3.BSIM3qNode] += 0.0;
                    // cstate.Matrix[bsim3.BSIM3sNodePrime, bsim3.BSIM3qNode] += 0.0;
                    // cstate.Matrix[bsim3.BSIM3gNode, bsim3.BSIM3qNode] += 0.0;

                }
                else
                {
                    // cstate.Matrix[bsim3.BSIM3qNode, bsim3.BSIM3qNode] += new Complex(bsim3.BSIM3gtau, omega * BSIM3v30.ScalingFactor);
                    // cstate.Matrix[bsim3.BSIM3qNode, bsim3.BSIM3gNode] += new Complex(xgtg, -xcqgb);
                    // cstate.Matrix[bsim3.BSIM3qNode, bsim3.BSIM3dNodePrime] += new Complex(xgtd, -xcqdb);
                    // cstate.Matrix[bsim3.BSIM3qNode, bsim3.BSIM3sNodePrime] += new Complex(xgts, -xcqsb);
                    // cstate.Matrix[bsim3.BSIM3qNode, bsim3.BSIM3bNode] += new Complex(xgtb, -xcqbb);

                    // cstate.Matrix[bsim3.BSIM3dNodePrime, bsim3.BSIM3qNode] += dxpart * bsim3.BSIM3gtau;
                    // cstate.Matrix[bsim3.BSIM3sNodePrime, bsim3.BSIM3qNode] += sxpart * bsim3.BSIM3gtau;
                    // cstate.Matrix[bsim3.BSIM3gNode, bsim3.BSIM3qNode] -= bsim3.BSIM3gtau;

                }
            }
        }
    }
}
