using System.Numerics;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// AC behaviour for a <see cref="BSIM4v80"/>
    /// </summary>
    public class BSIM4v80AcBehavior : AcBehavior
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
            var cstate = state;
            double capbd, capbs, cgso, cgdo, cgbo, Csd, Csg, Css, T0 = 0, T1, T2, T3, gmr, gmbsr, gdsr, gmi, gmbsi, gdsi, Cddr, Cdgr,
                Cdsr, Cdbr, Cddi, Cdgi, Cdsi, Cdbi, Csdr, Csgr, Cssr, Csbr, Csdi, Csgi, Cssi, Csbi, Cgdr, Cggr, Cgsr, Cgbr, Cgdi, Cggi, Cgsi,
                Cgbi, Gmr, Gmbsr, FwdSumr, RevSumr, Gmi, Gmbsi, FwdSumi, RevSumi, gbbdp, gbbsp, gbdpg, gbdpdp, gbdpb, gbdpsp, gbspdp, gbspg,
                gbspb, gbspsp, gIstotg, gIstotd, gIstots, gIstotb, gIdtotg, gIdtotd, gIdtots, gIdtotb, gIbtotg, gIbtotd, gIbtots, gIbtotb,
                gIgtotg, gIgtotd, gIgtots, gIgtotb, gcrgd, gcrgg, gcrgs, gcrgb, gcrg, xcgmgmb = 0, xcgmdb = 0, xcgmsb = 0, xcgmbb = 0, xcdgmb, xcsgmb, xcbgmb,
                xcggbr, xcgdbr, xcgsbr, xcgbbr, xcdgbr, xcsgbr, xcbgb, xcddbr, xcdsbr, xcsdbr, xcssbr, xcdbbr, xcsbbr, xcbdb, xcbsb, xcdbdb,
                xcsbsb = 0, xcbbb, xcdgbi, xcsgbi, xcddbi, xcdsbi, xcsdbi, xcssbi, xcdbbi, xcsbbi, xcggbi, xcgdbi, xcgsbi, xcgbbi, gstot, gstotd,
                gstotg, gstots, gstotb, gdtot, gdtotd, gdtotg, gdtots, gdtotb, gdpr, gspr, gjbd, gjbs, geltd, ggidld = 0, ggidlg = 0, ggidlb = 0, ggislg = 0,
                ggisls = 0, ggislb = 0;
            double omega = cstate.Laplace.Imaginary;

            capbd = bsim4.BSIM4capbd;
            capbs = bsim4.BSIM4capbs;
            cgso = bsim4.BSIM4cgso;
            cgdo = bsim4.BSIM4cgdo;
            cgbo = bsim4.pParam.BSIM4cgbo;

            Csd = -(bsim4.BSIM4cddb + bsim4.BSIM4cgdb + bsim4.BSIM4cbdb);
            Csg = -(bsim4.BSIM4cdgb + bsim4.BSIM4cggb + bsim4.BSIM4cbgb);
            Css = -(bsim4.BSIM4cdsb + bsim4.BSIM4cgsb + bsim4.BSIM4cbsb);

            if (bsim4.BSIM4acnqsMod != 0)
            {
                T0 = omega * bsim4.BSIM4taunet;
                T1 = T0 * T0;
                T2 = 1.0 / (1.0 + T1);
                T3 = T0 * T2;

                gmr = bsim4.BSIM4gm * T2;
                gmbsr = bsim4.BSIM4gmbs * T2;
                gdsr = bsim4.BSIM4gds * T2;

                gmi = -bsim4.BSIM4gm * T3;
                gmbsi = -bsim4.BSIM4gmbs * T3;
                gdsi = -bsim4.BSIM4gds * T3;

                Cddr = bsim4.BSIM4cddb * T2;
                Cdgr = bsim4.BSIM4cdgb * T2;
                Cdsr = bsim4.BSIM4cdsb * T2;
                Cdbr = -(Cddr + Cdgr + Cdsr);

                /* WDLiu: Cxyi mulitplied by jomega below, and actually to be of conductance */
                Cddi = bsim4.BSIM4cddb * T3 * omega;
                Cdgi = bsim4.BSIM4cdgb * T3 * omega;
                Cdsi = bsim4.BSIM4cdsb * T3 * omega;
                Cdbi = -(Cddi + Cdgi + Cdsi);

                Csdr = Csd * T2;
                Csgr = Csg * T2;
                Cssr = Css * T2;
                Csbr = -(Csdr + Csgr + Cssr);

                Csdi = Csd * T3 * omega;
                Csgi = Csg * T3 * omega;
                Cssi = Css * T3 * omega;
                Csbi = -(Csdi + Csgi + Cssi);

                Cgdr = -(Cddr + Csdr + bsim4.BSIM4cbdb);
                Cggr = -(Cdgr + Csgr + bsim4.BSIM4cbgb);
                Cgsr = -(Cdsr + Cssr + bsim4.BSIM4cbsb);
                Cgbr = -(Cgdr + Cggr + Cgsr);

                Cgdi = -(Cddi + Csdi);
                Cggi = -(Cdgi + Csgi);
                Cgsi = -(Cdsi + Cssi);
                Cgbi = -(Cgdi + Cggi + Cgsi);
            }
            else /* QS */
            {
                gmr = bsim4.BSIM4gm;
                gmbsr = bsim4.BSIM4gmbs;
                gdsr = bsim4.BSIM4gds;
                gmi = gmbsi = gdsi = 0.0;

                Cddr = bsim4.BSIM4cddb;
                Cdgr = bsim4.BSIM4cdgb;
                Cdsr = bsim4.BSIM4cdsb;
                Cdbr = -(Cddr + Cdgr + Cdsr);
                Cddi = Cdgi = Cdsi = Cdbi = 0.0;

                Csdr = Csd;
                Csgr = Csg;
                Cssr = Css;
                Csbr = -(Csdr + Csgr + Cssr);
                Csdi = Csgi = Cssi = Csbi = 0.0;

                Cgdr = bsim4.BSIM4cgdb;
                Cggr = bsim4.BSIM4cggb;
                Cgsr = bsim4.BSIM4cgsb;
                Cgbr = -(Cgdr + Cggr + Cgsr);
                Cgdi = Cggi = Cgsi = Cgbi = 0.0;
            }

            if (bsim4.BSIM4mode >= 0)
            {
                Gmr = gmr;
                Gmbsr = gmbsr;
                FwdSumr = Gmr + Gmbsr;
                RevSumr = 0.0;
                Gmi = gmi;
                Gmbsi = gmbsi;
                FwdSumi = Gmi + Gmbsi;
                RevSumi = 0.0;

                gbbdp = -(bsim4.BSIM4gbds);
                gbbsp = bsim4.BSIM4gbds + bsim4.BSIM4gbgs + bsim4.BSIM4gbbs;
                gbdpg = bsim4.BSIM4gbgs;
                gbdpdp = bsim4.BSIM4gbds;
                gbdpb = bsim4.BSIM4gbbs;
                gbdpsp = -(gbdpg + gbdpdp + gbdpb);

                gbspdp = 0.0;
                gbspg = 0.0;
                gbspb = 0.0;
                gbspsp = 0.0;

                if (model.BSIM4igcMod != 0)
                {
                    gIstotg = bsim4.BSIM4gIgsg + bsim4.BSIM4gIgcsg;
                    gIstotd = bsim4.BSIM4gIgcsd;
                    gIstots = bsim4.BSIM4gIgss + bsim4.BSIM4gIgcss;
                    gIstotb = bsim4.BSIM4gIgcsb;

                    gIdtotg = bsim4.BSIM4gIgdg + bsim4.BSIM4gIgcdg;
                    gIdtotd = bsim4.BSIM4gIgdd + bsim4.BSIM4gIgcdd;
                    gIdtots = bsim4.BSIM4gIgcds;
                    gIdtotb = bsim4.BSIM4gIgcdb;
                }
                else
                {
                    gIstotg = gIstotd = gIstots = gIstotb = 0.0;
                    gIdtotg = gIdtotd = gIdtots = gIdtotb = 0.0;
                }

                if (model.BSIM4igbMod != 0)
                {
                    gIbtotg = bsim4.BSIM4gIgbg;
                    gIbtotd = bsim4.BSIM4gIgbd;
                    gIbtots = bsim4.BSIM4gIgbs;
                    gIbtotb = bsim4.BSIM4gIgbb;
                }
                else
                    gIbtotg = gIbtotd = gIbtots = gIbtotb = 0.0;

                if ((model.BSIM4igcMod != 0) || (model.BSIM4igbMod != 0))
                {
                    gIgtotg = gIstotg + gIdtotg + gIbtotg;
                    gIgtotd = gIstotd + gIdtotd + gIbtotd;
                    gIgtots = gIstots + gIdtots + gIbtots;
                    gIgtotb = gIstotb + gIdtotb + gIbtotb;
                }
                else
                    gIgtotg = gIgtotd = gIgtots = gIgtotb = 0.0;

                if (bsim4.BSIM4rgateMod.Value == 2)
                    T0 = state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4vges] - state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4vgs];
                else if (bsim4.BSIM4rgateMod.Value == 3)
                    T0 = state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4vgms] - state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4vgs];
                if (bsim4.BSIM4rgateMod > 1)
                {
                    gcrgd = bsim4.BSIM4gcrgd * T0;
                    gcrgg = bsim4.BSIM4gcrgg * T0;
                    gcrgs = bsim4.BSIM4gcrgs * T0;
                    gcrgb = bsim4.BSIM4gcrgb * T0;
                    gcrgg -= bsim4.BSIM4gcrg;
                    gcrg = bsim4.BSIM4gcrg;
                }
                else
                    gcrg = gcrgd = gcrgg = gcrgs = gcrgb = 0.0;

                if (bsim4.BSIM4rgateMod.Value == 3)
                {
                    xcgmgmb = (cgdo + cgso + bsim4.pParam.BSIM4cgbo) * omega;
                    xcgmdb = -cgdo * omega;
                    xcgmsb = -cgso * omega;
                    xcgmbb = -bsim4.pParam.BSIM4cgbo * omega;

                    xcdgmb = xcgmdb;
                    xcsgmb = xcgmsb;
                    xcbgmb = xcgmbb;

                    xcggbr = Cggr * omega;
                    xcgdbr = Cgdr * omega;
                    xcgsbr = Cgsr * omega;
                    xcgbbr = -(xcggbr + xcgdbr + xcgsbr);

                    xcdgbr = Cdgr * omega;
                    xcsgbr = Csgr * omega;
                    xcbgb = bsim4.BSIM4cbgb * omega;
                }
                else
                {
                    xcggbr = (Cggr + cgdo + cgso + bsim4.pParam.BSIM4cgbo) * omega;
                    xcgdbr = (Cgdr - cgdo) * omega;
                    xcgsbr = (Cgsr - cgso) * omega;
                    xcgbbr = -(xcggbr + xcgdbr + xcgsbr);

                    xcdgbr = (Cdgr - cgdo) * omega;
                    xcsgbr = (Csgr - cgso) * omega;
                    xcbgb = (bsim4.BSIM4cbgb - bsim4.pParam.BSIM4cgbo) * omega;

                    xcdgmb = xcsgmb = xcbgmb = 0.0;
                }
                xcddbr = (Cddr + bsim4.BSIM4capbd + cgdo) * omega;
                xcdsbr = Cdsr * omega;
                xcsdbr = Csdr * omega;
                xcssbr = (bsim4.BSIM4capbs + cgso + Cssr) * omega;

                if (bsim4.BSIM4rbodyMod == 0)
                {
                    xcdbbr = -(xcdgbr + xcddbr + xcdsbr + xcdgmb);
                    xcsbbr = -(xcsgbr + xcsdbr + xcssbr + xcsgmb);

                    xcbdb = (bsim4.BSIM4cbdb - bsim4.BSIM4capbd) * omega;
                    xcbsb = (bsim4.BSIM4cbsb - bsim4.BSIM4capbs) * omega;
                    xcdbdb = 0.0;
                }
                else
                {
                    xcdbbr = Cdbr * omega;
                    xcsbbr = -(xcsgbr + xcsdbr + xcssbr + xcsgmb) + bsim4.BSIM4capbs * omega;

                    xcbdb = bsim4.BSIM4cbdb * omega;
                    xcbsb = bsim4.BSIM4cbsb * omega;

                    xcdbdb = -bsim4.BSIM4capbd * omega;
                    xcsbsb = -bsim4.BSIM4capbs * omega;
                }
                xcbbb = -(xcbdb + xcbgb + xcbsb + xcbgmb);

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
            else /* Reverse mode */
            {
                Gmr = -gmr;
                Gmbsr = -gmbsr;
                FwdSumr = 0.0;
                RevSumr = -(Gmr + Gmbsr);
                Gmi = -gmi;
                Gmbsi = -gmbsi;
                FwdSumi = 0.0;
                RevSumi = -(Gmi + Gmbsi);

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

                    gIdtotg = bsim4.BSIM4gIgdg + bsim4.BSIM4gIgcsg;
                    gIdtotd = bsim4.BSIM4gIgdd + bsim4.BSIM4gIgcss;
                    gIdtots = bsim4.BSIM4gIgcsd;
                    gIdtotb = bsim4.BSIM4gIgcsb;
                }
                else
                {
                    gIstotg = gIstotd = gIstots = gIstotb = 0.0;
                    gIdtotg = gIdtotd = gIdtots = gIdtotb = 0.0;
                }

                if (model.BSIM4igbMod != 0)
                {
                    gIbtotg = bsim4.BSIM4gIgbg;
                    gIbtotd = bsim4.BSIM4gIgbs;
                    gIbtots = bsim4.BSIM4gIgbd;
                    gIbtotb = bsim4.BSIM4gIgbb;
                }
                else
                    gIbtotg = gIbtotd = gIbtots = gIbtotb = 0.0;

                if ((model.BSIM4igcMod != 0) || (model.BSIM4igbMod != 0))
                {
                    gIgtotg = gIstotg + gIdtotg + gIbtotg;
                    gIgtotd = gIstotd + gIdtotd + gIbtotd;
                    gIgtots = gIstots + gIdtots + gIbtots;
                    gIgtotb = gIstotb + gIdtotb + gIbtotb;
                }
                else
                    gIgtotg = gIgtotd = gIgtots = gIgtotb = 0.0;

                if (bsim4.BSIM4rgateMod.Value == 2)
                    T0 = state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4vges] - state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4vgs];
                else if (bsim4.BSIM4rgateMod.Value == 3)
                    T0 = state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4vgms] - state.States[0][bsim4.BSIM4states + BSIM4v80.BSIM4vgs];
                if (bsim4.BSIM4rgateMod > 1)
                {
                    gcrgd = bsim4.BSIM4gcrgs * T0;
                    gcrgg = bsim4.BSIM4gcrgg * T0;
                    gcrgs = bsim4.BSIM4gcrgd * T0;
                    gcrgb = bsim4.BSIM4gcrgb * T0;
                    gcrgg -= bsim4.BSIM4gcrg;
                    gcrg = bsim4.BSIM4gcrg;
                }
                else
                    gcrg = gcrgd = gcrgg = gcrgs = gcrgb = 0.0;

                if (bsim4.BSIM4rgateMod.Value == 3)
                {
                    xcgmgmb = (cgdo + cgso + bsim4.pParam.BSIM4cgbo) * omega;
                    xcgmdb = -cgdo * omega;
                    xcgmsb = -cgso * omega;
                    xcgmbb = -bsim4.pParam.BSIM4cgbo * omega;

                    xcdgmb = xcgmdb;
                    xcsgmb = xcgmsb;
                    xcbgmb = xcgmbb;

                    xcggbr = Cggr * omega;
                    xcgdbr = Cgsr * omega;
                    xcgsbr = Cgdr * omega;
                    xcgbbr = -(xcggbr + xcgdbr + xcgsbr);

                    xcdgbr = Csgr * omega;
                    xcsgbr = Cdgr * omega;
                    xcbgb = bsim4.BSIM4cbgb * omega;
                }
                else
                {
                    xcggbr = (Cggr + cgdo + cgso + bsim4.pParam.BSIM4cgbo) * omega;
                    xcgdbr = (Cgsr - cgdo) * omega;
                    xcgsbr = (Cgdr - cgso) * omega;
                    xcgbbr = -(xcggbr + xcgdbr + xcgsbr);

                    xcdgbr = (Csgr - cgdo) * omega;
                    xcsgbr = (Cdgr - cgso) * omega;
                    xcbgb = (bsim4.BSIM4cbgb - bsim4.pParam.BSIM4cgbo) * omega;

                    xcdgmb = xcsgmb = xcbgmb = 0.0;
                }
                xcddbr = (bsim4.BSIM4capbd + cgdo + Cssr) * omega;
                xcdsbr = Csdr * omega;
                xcsdbr = Cdsr * omega;
                xcssbr = (Cddr + bsim4.BSIM4capbs + cgso) * omega;

                if (bsim4.BSIM4rbodyMod == 0)
                {
                    xcdbbr = -(xcdgbr + xcddbr + xcdsbr + xcdgmb);
                    xcsbbr = -(xcsgbr + xcsdbr + xcssbr + xcsgmb);

                    xcbdb = (bsim4.BSIM4cbsb - bsim4.BSIM4capbd) * omega;
                    xcbsb = (bsim4.BSIM4cbdb - bsim4.BSIM4capbs) * omega;
                    xcdbdb = 0.0;
                }
                else
                {
                    xcdbbr = -(xcdgbr + xcddbr + xcdsbr + xcdgmb) + bsim4.BSIM4capbd * omega;
                    xcsbbr = Cdbr * omega;

                    xcbdb = bsim4.BSIM4cbsb * omega;
                    xcbsb = bsim4.BSIM4cbdb * omega;
                    xcdbdb = -bsim4.BSIM4capbd * omega;
                    xcsbsb = -bsim4.BSIM4capbs * omega;
                }
                xcbbb = -(xcbgb + xcbdb + xcbsb + xcbgmb);

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

            if (model.BSIM4rdsMod.Value == 1)
            {
                gstot = bsim4.BSIM4gstot;
                gstotd = bsim4.BSIM4gstotd;
                gstotg = bsim4.BSIM4gstotg;
                gstots = bsim4.BSIM4gstots - gstot;
                gstotb = bsim4.BSIM4gstotb;

                gdtot = bsim4.BSIM4gdtot;
                gdtotd = bsim4.BSIM4gdtotd - gdtot;
                gdtotg = bsim4.BSIM4gdtotg;
                gdtots = bsim4.BSIM4gdtots;
                gdtotb = bsim4.BSIM4gdtotb;
            }
            else
            {
                gstot = gstotd = gstotg = gstots = gstotb = 0.0;
                gdtot = gdtotd = gdtotg = gdtots = gdtotb = 0.0;
            }

            /* 
            * Loading AC matrix
            */

            if (model.BSIM4rdsMod == 0)
            {
                gdpr = bsim4.BSIM4drainConductance;
                gspr = bsim4.BSIM4sourceConductance;
            }
            else
                gdpr = gspr = 0.0;

            if (bsim4.BSIM4rbodyMod == 0)
            {
                gjbd = bsim4.BSIM4gbd;
                gjbs = bsim4.BSIM4gbs;
            }
            else
                gjbd = gjbs = 0.0;

            geltd = bsim4.BSIM4grgeltd;

            if (bsim4.BSIM4rgateMod.Value == 1)
            {
                // cstate.Matrix[bsim4.BSIM4gNodeExt, bsim4.BSIM4gNodeExt] += geltd;
                // cstate.Matrix[bsim4.BSIM4gNodePrime, bsim4.BSIM4gNodeExt] -= geltd;
                // cstate.Matrix[bsim4.BSIM4gNodeExt, bsim4.BSIM4gNodePrime] -= geltd;

                // cstate.Matrix[bsim4.BSIM4gNodePrime, bsim4.BSIM4gNodePrime] += new Complex(geltd + xcggbi + gIgtotg, xcggbr);

                // cstate.Matrix[bsim4.BSIM4gNodePrime, bsim4.BSIM4dNodePrime] += new Complex(xcgdbi + gIgtotd, xcgdbr);

                // cstate.Matrix[bsim4.BSIM4gNodePrime, bsim4.BSIM4sNodePrime] += new Complex(xcgsbi + gIgtots, xcgsbr);

                // cstate.Matrix[bsim4.BSIM4gNodePrime, bsim4.BSIM4bNodePrime] += new Complex(xcgbbi + gIgtotb, xcgbbr);

            } /* WDLiu: gcrg already subtracted from all gcrgg below */
            else if (bsim4.BSIM4rgateMod.Value == 2)
            {
                // cstate.Matrix[bsim4.BSIM4gNodeExt, bsim4.BSIM4gNodeExt] += gcrg;
                // cstate.Matrix[bsim4.BSIM4gNodeExt, bsim4.BSIM4gNodePrime] += gcrgg;
                // cstate.Matrix[bsim4.BSIM4gNodeExt, bsim4.BSIM4dNodePrime] += gcrgd;
                // cstate.Matrix[bsim4.BSIM4gNodeExt, bsim4.BSIM4sNodePrime] += gcrgs;
                // cstate.Matrix[bsim4.BSIM4gNodeExt, bsim4.BSIM4bNodePrime] += gcrgb;

                // cstate.Matrix[bsim4.BSIM4gNodePrime, bsim4.BSIM4gNodeExt] -= gcrg;
                // cstate.Matrix[bsim4.BSIM4gNodePrime, bsim4.BSIM4gNodePrime] -= new Complex(gcrgg - xcggbi - gIgtotg, -xcggbr);

                // cstate.Matrix[bsim4.BSIM4gNodePrime, bsim4.BSIM4dNodePrime] -= new Complex(gcrgd - xcgdbi - gIgtotd, -xcgdbr);

                // cstate.Matrix[bsim4.BSIM4gNodePrime, bsim4.BSIM4sNodePrime] -= new Complex(gcrgs - xcgsbi - gIgtots, -xcgsbr);

                // cstate.Matrix[bsim4.BSIM4gNodePrime, bsim4.BSIM4bNodePrime] -= new Complex(gcrgb - xcgbbi - gIgtotb, -xcgbbr);

            }
            else if (bsim4.BSIM4rgateMod.Value == 3)
            {
                // cstate.Matrix[bsim4.BSIM4gNodeExt, bsim4.BSIM4gNodeExt] += geltd;
                // cstate.Matrix[bsim4.BSIM4gNodeExt, bsim4.BSIM4gNodeMid] -= geltd;
                // cstate.Matrix[bsim4.BSIM4gNodeMid, bsim4.BSIM4gNodeExt] -= geltd;
                // cstate.Matrix[bsim4.BSIM4gNodeMid, bsim4.BSIM4gNodeMid] += new Complex(geltd + gcrg, xcgmgmb);

                // cstate.Matrix[bsim4.BSIM4gNodeMid, bsim4.BSIM4dNodePrime] += new Complex(gcrgd, xcgmdb);

                // cstate.Matrix[bsim4.BSIM4gNodeMid, bsim4.BSIM4gNodePrime] += gcrgg;
                // cstate.Matrix[bsim4.BSIM4gNodeMid, bsim4.BSIM4sNodePrime] += new Complex(gcrgs, xcgmsb);

                // cstate.Matrix[bsim4.BSIM4gNodeMid, bsim4.BSIM4bNodePrime] += new Complex(gcrgb, xcgmbb);

                // cstate.Matrix[bsim4.BSIM4dNodePrime, bsim4.BSIM4gNodeMid] += new Complex(0.0, xcdgmb);
                // cstate.Matrix[bsim4.BSIM4gNodePrime, bsim4.BSIM4gNodeMid] -= gcrg;
                // cstate.Matrix[bsim4.BSIM4sNodePrime, bsim4.BSIM4gNodeMid] += new Complex(0.0, xcsgmb);
                // cstate.Matrix[bsim4.BSIM4bNodePrime, bsim4.BSIM4gNodeMid] += new Complex(0.0, xcbgmb);

                // cstate.Matrix[bsim4.BSIM4gNodePrime, bsim4.BSIM4gNodePrime] -= new Complex(gcrgg - xcggbi - gIgtotg, -xcggbr);

                // cstate.Matrix[bsim4.BSIM4gNodePrime, bsim4.BSIM4dNodePrime] -= new Complex(gcrgd - xcgdbi - gIgtotd, -xcgdbr);

                // cstate.Matrix[bsim4.BSIM4gNodePrime, bsim4.BSIM4sNodePrime] -= new Complex(gcrgs - xcgsbi - gIgtots, -xcgsbr);

                // cstate.Matrix[bsim4.BSIM4gNodePrime, bsim4.BSIM4bNodePrime] -= new Complex(gcrgb - xcgbbi - gIgtotb, -xcgbbr);

            }
            else
            {
                // cstate.Matrix[bsim4.BSIM4gNodePrime, bsim4.BSIM4gNodePrime] += new Complex(xcggbi + gIgtotg, xcggbr);

                // cstate.Matrix[bsim4.BSIM4gNodePrime, bsim4.BSIM4dNodePrime] += new Complex(xcgdbi + gIgtotd, xcgdbr);

                // cstate.Matrix[bsim4.BSIM4gNodePrime, bsim4.BSIM4sNodePrime] += new Complex(xcgsbi + gIgtots, xcgsbr);

                // cstate.Matrix[bsim4.BSIM4gNodePrime, bsim4.BSIM4bNodePrime] += new Complex(xcgbbi + gIgtotb, xcgbbr);

            }

            if (model.BSIM4rdsMod != 0)
            {
                // cstate.Matrix[bsim4.BSIM4dNode, bsim4.BSIM4gNodePrime] += gdtotg;
                // cstate.Matrix[bsim4.BSIM4dNode, bsim4.BSIM4sNodePrime] += gdtots;
                // cstate.Matrix[bsim4.BSIM4dNode, bsim4.BSIM4bNodePrime] += gdtotb;
                // cstate.Matrix[bsim4.BSIM4sNode, bsim4.BSIM4dNodePrime] += gstotd;
                // cstate.Matrix[bsim4.BSIM4sNode, bsim4.BSIM4gNodePrime] += gstotg;
                // cstate.Matrix[bsim4.BSIM4sNode, bsim4.BSIM4bNodePrime] += gstotb;
            }

            // cstate.Matrix[bsim4.BSIM4dNodePrime, bsim4.BSIM4dNodePrime] += new Complex(gdpr + xcddbi + gdsr + bsim4.BSIM4gbd - gdtotd + RevSumr + gbdpdp - gIdtotd + ggidld, xcddbr + gdsi + RevSumi);

            // cstate.Matrix[bsim4.BSIM4dNodePrime, bsim4.BSIM4dNode] -= gdpr + gdtot;
            // cstate.Matrix[bsim4.BSIM4dNodePrime, bsim4.BSIM4gNodePrime] += new Complex(Gmr + xcdgbi - gdtotg + gbdpg - gIdtotg + ggidlg, xcdgbr + Gmi);

            // cstate.Matrix[bsim4.BSIM4dNodePrime, bsim4.BSIM4sNodePrime] -= new Complex(gdsr - xcdsbi + FwdSumr + gdtots - gbdpsp + gIdtots + (ggidlg + ggidld) + ggidlb, -(xcdsbr - gdsi - FwdSumi));

            // cstate.Matrix[bsim4.BSIM4dNodePrime, bsim4.BSIM4bNodePrime] -= new Complex(gjbd + gdtotb - xcdbbi - Gmbsr - gbdpb + gIdtotb - ggidlb, - (xcdbbr + Gmbsi));

            // cstate.Matrix[bsim4.BSIM4dNode, bsim4.BSIM4dNodePrime] -= gdpr - gdtotd;
            // cstate.Matrix[bsim4.BSIM4dNode, bsim4.BSIM4dNode] += gdpr + gdtot;

            // cstate.Matrix[bsim4.BSIM4sNodePrime, bsim4.BSIM4dNodePrime] -= new Complex(gdsr - xcsdbi + gstotd + RevSumr - gbspdp + gIstotd + (ggisls + ggislg) + ggislb, -(xcsdbr - gdsi - RevSumi));

            // cstate.Matrix[bsim4.BSIM4sNodePrime, bsim4.BSIM4gNodePrime] -= new Complex(Gmr - xcsgbi + gstotg - gbspg + gIstotg - ggislg, -(xcsgbr - Gmi));

            // cstate.Matrix[bsim4.BSIM4sNodePrime, bsim4.BSIM4sNodePrime] += new Complex(gspr + xcssbi + gdsr + bsim4.BSIM4gbs - gstots + FwdSumr + gbspsp - gIstots + ggisls, xcssbr + gdsi + FwdSumi);

            // cstate.Matrix[bsim4.BSIM4sNodePrime, bsim4.BSIM4sNode] -= gspr + gstot;
            // cstate.Matrix[bsim4.BSIM4sNodePrime, bsim4.BSIM4bNodePrime] -= new Complex(gjbs + gstotb - xcsbbi + Gmbsr - gbspb + gIstotb - ggislb, - (xcsbbr - Gmbsi));

            // cstate.Matrix[bsim4.BSIM4sNode, bsim4.BSIM4sNodePrime] -= gspr - gstots;
            // cstate.Matrix[bsim4.BSIM4sNode, bsim4.BSIM4sNode] += gspr + gstot;

            // cstate.Matrix[bsim4.BSIM4bNodePrime, bsim4.BSIM4dNodePrime] -= new Complex(gjbd - gbbdp + gIbtotd + ggidld - ((ggislg + ggisls) + ggislb), - xcbdb);

            // cstate.Matrix[bsim4.BSIM4bNodePrime, bsim4.BSIM4gNodePrime] -= new Complex(bsim4.BSIM4gbgs + gIbtotg + ggidlg + ggislg, -xcbgb);

            // cstate.Matrix[bsim4.BSIM4bNodePrime, bsim4.BSIM4sNodePrime] -= new Complex(gjbs - gbbsp + gIbtots - ((ggidlg + ggidld) + ggidlb) + ggisls, - xcbsb);

            // cstate.Matrix[bsim4.BSIM4bNodePrime, bsim4.BSIM4bNodePrime] += new Complex(gjbd + gjbs - bsim4.BSIM4gbbs - gIbtotb - ggidlb - ggislb, xcbbb);

            ggidld = bsim4.BSIM4ggidld;
            ggidlg = bsim4.BSIM4ggidlg;
            ggidlb = bsim4.BSIM4ggidlb;
            ggislg = bsim4.BSIM4ggislg;
            ggisls = bsim4.BSIM4ggisls;
            ggislb = bsim4.BSIM4ggislb;

            /* stamp gidl */

            /* stamp gisl */

            if (bsim4.BSIM4rbodyMod != 0)
            {
                // cstate.Matrix[bsim4.BSIM4dNodePrime, bsim4.BSIM4dbNode] -= new Complex(bsim4.BSIM4gbd, -xcdbdb);

                // cstate.Matrix[bsim4.BSIM4sNodePrime, bsim4.BSIM4sbNode] -= new Complex(bsim4.BSIM4gbs, -xcsbsb);

                // cstate.Matrix[bsim4.BSIM4dbNode, bsim4.BSIM4dNodePrime] -= new Complex(bsim4.BSIM4gbd, -xcdbdb);

                // cstate.Matrix[bsim4.BSIM4dbNode, bsim4.BSIM4dbNode] += new Complex(bsim4.BSIM4gbd + bsim4.BSIM4grbpd + bsim4.BSIM4grbdb, -xcdbdb);

                // cstate.Matrix[bsim4.BSIM4dbNode, bsim4.BSIM4bNodePrime] -= bsim4.BSIM4grbpd;
                // cstate.Matrix[bsim4.BSIM4dbNode, bsim4.BSIM4bNode] -= bsim4.BSIM4grbdb;

                // cstate.Matrix[bsim4.BSIM4bNodePrime, bsim4.BSIM4dbNode] -= bsim4.BSIM4grbpd;
                // cstate.Matrix[bsim4.BSIM4bNodePrime, bsim4.BSIM4bNode] -= bsim4.BSIM4grbpb;
                // cstate.Matrix[bsim4.BSIM4bNodePrime, bsim4.BSIM4sbNode] -= bsim4.BSIM4grbps;
                // cstate.Matrix[bsim4.BSIM4bNodePrime, bsim4.BSIM4bNodePrime] += bsim4.BSIM4grbpd + bsim4.BSIM4grbps + bsim4.BSIM4grbpb;
                /* WDLiu: (-bsim4.BSIM4gbbs) already added to BPbpPtr */

                // cstate.Matrix[bsim4.BSIM4sbNode, bsim4.BSIM4sNodePrime] -= new Complex(bsim4.BSIM4gbs, -xcsbsb);

                // cstate.Matrix[bsim4.BSIM4sbNode, bsim4.BSIM4bNodePrime] -= bsim4.BSIM4grbps;
                // cstate.Matrix[bsim4.BSIM4sbNode, bsim4.BSIM4bNode] -= bsim4.BSIM4grbsb;
                // cstate.Matrix[bsim4.BSIM4sbNode, bsim4.BSIM4sbNode] += new Complex(bsim4.BSIM4gbs + bsim4.BSIM4grbps + bsim4.BSIM4grbsb, -xcsbsb);

                // cstate.Matrix[bsim4.BSIM4bNode, bsim4.BSIM4dbNode] -= bsim4.BSIM4grbdb;
                // cstate.Matrix[bsim4.BSIM4bNode, bsim4.BSIM4bNodePrime] -= bsim4.BSIM4grbpb;
                // cstate.Matrix[bsim4.BSIM4bNode, bsim4.BSIM4sbNode] -= bsim4.BSIM4grbsb;
                // cstate.Matrix[bsim4.BSIM4bNode, bsim4.BSIM4bNode] += bsim4.BSIM4grbsb + bsim4.BSIM4grbdb + bsim4.BSIM4grbpb;
            }

            /* 
            * WDLiu: The internal charge node generated for transient NQS is not needed for
            * AC NQS. The following is not doing a real job, but we have to keep it;
            * otherwise a singular AC NQS matrix may occur if the transient NQS is on.
            * The charge node is isolated from the instance.
            */
            if (bsim4.BSIM4trnqsMod != 0)
            {
                // cstate.Matrix[bsim4.BSIM4qNode, bsim4.BSIM4qNode] += 1.0;
                // cstate.Matrix[bsim4.BSIM4qNode, bsim4.BSIM4gNodePrime] += 0.0;
                // cstate.Matrix[bsim4.BSIM4qNode, bsim4.BSIM4dNodePrime] += 0.0;
                // cstate.Matrix[bsim4.BSIM4qNode, bsim4.BSIM4sNodePrime] += 0.0;
                // cstate.Matrix[bsim4.BSIM4qNode, bsim4.BSIM4bNodePrime] += 0.0;

                // cstate.Matrix[bsim4.BSIM4dNodePrime, bsim4.BSIM4qNode] += 0.0;
                // cstate.Matrix[bsim4.BSIM4sNodePrime, bsim4.BSIM4qNode] += 0.0;
                // cstate.Matrix[bsim4.BSIM4gNodePrime, bsim4.BSIM4qNode] += 0.0;
            }
        }
    }
}
