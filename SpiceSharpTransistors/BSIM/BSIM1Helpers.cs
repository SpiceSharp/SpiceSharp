using System;
using SpiceSharp.Circuits;

namespace SpiceSharp.Components.Transistors
{
    /// <summary>
    /// Helper methods for the BSIM1 model
    /// </summary>
    internal static class BSIM1Helpers
    {
        /// <summary>
        /// This routine evaluates the drain current, its derivatives and the
        /// charges associated with the gate,bulk and drain terminal
        /// using the B1(Berkeley Short-Channel IGFET Model) Equations.
        /// </summary>
        /// <param name="bsim1"></param>
        /// <param name="vds"></param>
        /// <param name="vbs"></param>
        /// <param name="vgs"></param>
        /// <param name="gmPointer"></param>
        /// <param name="gdsPointer"></param>
        /// <param name="gmbsPointer"></param>
        /// <param name="qgPointer"></param>
        /// <param name="qbPointer"></param>
        /// <param name="qdPointer"></param>
        /// <param name="cggbPointer"></param>
        /// <param name="cgdbPointer"></param>
        /// <param name="cgsbPointer"></param>
        /// <param name="cbgbPointer"></param>
        /// <param name="cbdbPointer"></param>
        /// <param name="cbsbPointer"></param>
        /// <param name="cdgbPointer"></param>
        /// <param name="cddbPointer"></param>
        /// <param name="cdsbPointer"></param>
        /// <param name="cdrainPointer"></param>
        /// <param name="vonPointer"></param>
        /// <param name="vdsatPointer"></param>
        /// <param name="ckt"></param>
        public static void B1evaluate(this BSIM1 bsim1, double vds, double vbs, double vgs, out double gmPointer, out double gdsPointer, out double gmbsPointer,
                out double qgPointer, out double qbPointer, out double qdPointer, out double cggbPointer, out double cgdbPointer, out double cgsbPointer,
                out double cbgbPointer, out double cbdbPointer, out double cbsbPointer, out double cdgbPointer, out double cddbPointer,
                out double cdsbPointer, out double cdrainPointer, out double vonPointer, out double vdsatPointer, Circuit ckt)
        {
            var model = bsim1.Model as BSIM1Model;
            double gm, gds, gmbs, qg = 0.0, qb = 0.0, qd = 0.0, cggb = 0, cgdb = 0, cgsb = 0, cbgb = 0, cbdb = 0, cbsb = 0, cdgb = 0, cddb = 0, cdsb = 0, Vfb, Phi, K1, K2, Vdd, Ugs, Uds, dUgsdVbs,
                Leff, dUdsdVbs, dUdsdVds, Eta, dEtadVds, dEtadVbs, Vpb, SqrtVpb, Von, Vth, dVthdVbs, dVthdVds, Vgs_Vth, DrainCurrent,
                G, A, Arg, dGdVbs, dAdVbs, Beta, Beta_Vds_0, BetaVdd, dBetaVdd_dVds, Beta0, dBeta0dVds, dBeta0dVbs, VddSquare, C1, C2,
                dBetaVdd_dVbs, dBeta_Vds_0_dVbs, dC1dVbs, dC2dVbs, dBetadVgs, dBetadVds, dBetadVbs, VdsSat = 0, Argl1, Argl2, Vc, Term1, K,
                Args1, dVcdVgs, dVcdVds, dVcdVbs, dKdVc, dKdVgs, dKdVds, dKdVbs, Args2, Args3, Warg1, Vcut, N, N0, NB, ND, Warg2, Wds,
                Wgs, Ilimit, Iexp, Temp1, Vth0, Arg1, Arg2, Arg3, Arg5, Ent, Vcom, Vgb, Vgb_Vfb, VdsPinchoff, EntSquare, Vgs_VthSquare,
                Argl3, Argl4, Argl5, Argl6, Argl7, Argl8, Argl9, dEntdVds, dEntdVbs, cgbb, cdbb, cbbb, WLCox, Vtsquare, Temp3;
            bool ChargeComputationNeeded;
            double co4v15;

            if (ckt.Method != null)
            {
                ChargeComputationNeeded = true;
            }
            else if (ckt.State.UseSmallSignal)
            {
                ChargeComputationNeeded = true;
            }
            else if (ckt.State.Domain == CircuitState.DomainTypes.Time && ckt.State.UseDC && ckt.State.UseIC)
            {
                ChargeComputationNeeded = true;
            }
            else
            {
                ChargeComputationNeeded = false;
            }

            Vfb = bsim1.B1vfb;
            Phi = bsim1.B1phi;
            K1 = bsim1.B1K1;
            K2 = bsim1.B1K2;
            Vdd = model.B1vdd;
            if ((Ugs = bsim1.B1ugs + bsim1.B1ugsB * vbs) <= 0)
            {
                Ugs = 0;
                dUgsdVbs = 0.0;
            }
            else
            {
                dUgsdVbs = bsim1.B1ugsB;
            }
            if ((Uds = bsim1.B1uds + bsim1.B1udsB * vbs +
                    bsim1.B1udsD * (vds - Vdd)) <= 0)
            {
                Uds = 0.0;
                dUdsdVbs = dUdsdVds = 0.0;
            }
            else
            {
                Leff = bsim1.B1l * 1.0e6 - model.B1deltaL; /* Leff in um */
                Uds = Uds / Leff;
                dUdsdVbs = bsim1.B1udsB / Leff;
                dUdsdVds = bsim1.B1udsD / Leff;
            }
            Eta = bsim1.B1eta + bsim1.B1etaB * vbs + bsim1.B1etaD *
                (vds - Vdd);
            if (Eta <= 0)
            {
                Eta = 0;
                dEtadVds = dEtadVbs = 0.0;
            }
            else if (Eta > 1)
            {
                Eta = 1;
                dEtadVds = dEtadVbs = 0;
            }
            else
            {
                dEtadVds = bsim1.B1etaD;
                dEtadVbs = bsim1.B1etaB;
            }
            if (vbs < 0)
            {
                Vpb = Phi - vbs;
            }
            else
            {
                Vpb = Phi;
            }
            SqrtVpb = Math.Sqrt(Vpb);
            Von = Vfb + Phi + K1 * SqrtVpb - K2 * Vpb - Eta * vds;
            Vth = Von;
            dVthdVds = -Eta - dEtadVds * vds;
            dVthdVbs = K2 - 0.5 * K1 / SqrtVpb - dEtadVbs * vds;
            Vgs_Vth = vgs - Vth;

            G = 1.0 - 1.0 / (1.744 + 0.8364 * Vpb);
            A = 1.0 + 0.5 * G * K1 / SqrtVpb;
            A = Math.Max(A, 1.0);   /* Modified */
            Arg = Math.Max((1 + Ugs * Vgs_Vth), 1.0);
            dGdVbs = -0.8364 * (1 - G) * (1 - G);
            dAdVbs = 0.25 * K1 / SqrtVpb * (2 * dGdVbs + G / Vpb);

            if (Vgs_Vth < 0)
            {
                /* cutoff */
                DrainCurrent = 0;
                gm = 0;
                gds = 0;
                gmbs = 0;
                goto SubthresholdComputation;
            }

            /* Quadratic Interpolation for Beta0 (Beta at vgs  =  0, vds=Vds) */

            Beta_Vds_0 = (bsim1.B1betaZero + bsim1.B1betaZeroB * vbs);
            BetaVdd = (bsim1.B1betaVdd + bsim1.B1betaVddB * vbs);
            dBetaVdd_dVds = Math.Max(bsim1.B1betaVddD, 0.0); /* Modified */
            if (vds > Vdd)
            {
                Beta0 = BetaVdd + dBetaVdd_dVds * (vds - Vdd);
                dBeta0dVds = dBetaVdd_dVds;
                dBeta0dVbs = bsim1.B1betaVddB;
            }
            else
            {
                VddSquare = Vdd * Vdd;
                C1 = (-BetaVdd + Beta_Vds_0 + dBetaVdd_dVds * Vdd) / VddSquare;
                C2 = 2 * (BetaVdd - Beta_Vds_0) / Vdd - dBetaVdd_dVds;
                dBeta_Vds_0_dVbs = bsim1.B1betaZeroB;
                dBetaVdd_dVbs = bsim1.B1betaVddB;
                dC1dVbs = (dBeta_Vds_0_dVbs - dBetaVdd_dVbs) / VddSquare;
                dC2dVbs = dC1dVbs * (-2) * Vdd;
                Beta0 = (C1 * vds + C2) * vds + Beta_Vds_0;
                dBeta0dVds = 2 * C1 * vds + C2;
                dBeta0dVbs = dC1dVbs * vds * vds + dC2dVbs * vds + dBeta_Vds_0_dVbs;
            }

            /*Beta  =  Beta0 / ( 1 + Ugs * Vgs_Vth );*/

            Beta = Beta0 / Arg;
            dBetadVgs = -Beta * Ugs / Arg;
            dBetadVds = dBeta0dVds / Arg - dBetadVgs * dVthdVds;
            dBetadVbs = dBeta0dVbs / Arg + Beta * Ugs * dVthdVbs / Arg -
                    Beta * Vgs_Vth * dUgsdVbs / Arg;

            /*VdsSat  = Math.Max( Vgs_Vth / ( A + Uds * Vgs_Vth ),  0.0);*/

            if ((Vc = Uds * Vgs_Vth / A) < 0.0) Vc = 0.0;
            Term1 = Math.Sqrt(1 + 2 * Vc);
            K = 0.5 * (1 + Vc + Term1);
            VdsSat = Math.Max(Vgs_Vth / (A * Math.Sqrt(K)), 0.0);

            if (vds < VdsSat)
            {
                /* Triode Region */
                /*Argl1  =  1 + Uds * vds;*/
                Argl1 = Math.Max((1 + Uds * vds), 1.0);
                Argl2 = Vgs_Vth - 0.5 * A * vds;
                DrainCurrent = Beta * Argl2 * vds / Argl1;
                gm = (dBetadVgs * Argl2 * vds + Beta * vds) / Argl1;
                gds = (dBetadVds * Argl2 * vds + Beta *
                    (Vgs_Vth - vds * dVthdVds - A * vds) -
                    DrainCurrent * (vds * dUdsdVds + Uds)) / Argl1;
                gmbs = (dBetadVbs * Argl2 * vds + Beta * vds *
                    (-dVthdVbs - 0.5 * vds * dAdVbs) -
                    DrainCurrent * vds * dUdsdVbs) / Argl1;
            }
            else
            {
                /* Pinchoff (Saturation) Region */
                Args1 = 1.0 + 1.0 / Term1;
                dVcdVgs = Uds / A;
                dVcdVds = Vgs_Vth * dUdsdVds / A - dVcdVgs * dVthdVds;
                dVcdVbs = (Vgs_Vth * dUdsdVbs - Uds *
                            (dVthdVbs + Vgs_Vth * dAdVbs / A)) / A;
                dKdVc = 0.5 * Args1;
                dKdVgs = dKdVc * dVcdVgs;
                dKdVds = dKdVc * dVcdVds;
                dKdVbs = dKdVc * dVcdVbs;
                Args2 = Vgs_Vth / A / K;
                Args3 = Args2 * Vgs_Vth;
                DrainCurrent = 0.5 * Beta * Args3;
                gm = 0.5 * Args3 * dBetadVgs + Beta * Args2 -
                            DrainCurrent * dKdVgs / K;
                gds = 0.5 * Args3 * dBetadVds - Beta * Args2 * dVthdVds -
                    DrainCurrent * dKdVds / K;
                gmbs = 0.5 * dBetadVbs * Args3 - Beta * Args2 * dVthdVbs -
                    DrainCurrent * (dAdVbs / A + dKdVbs / K);
            }

            SubthresholdComputation:

            N0 = bsim1.B1subthSlope;
            Vcut = -40.0 * N0 * Circuit.CONSTvt0;

            /* The following 'if' statement has been modified so that subthreshold  *
             * current computation is always executed unless N0 >= 200. This should *
             * get rid of the Ids kink seen on Ids-Vgs plots at low Vds.            *
             *                                                Peter M. Lee          *
             *                                                6/8/90                *
             *  Old 'if' statement:                                                 *
             *  if( (N0 >=  200) || (Vgs_Vth < Vcut ) || (Vgs_Vth > (-0.5*Vcut)))   */

            if (N0 >= 200)
            {
                goto ChargeComputation;
            }

            NB = bsim1.B1subthSlopeB;
            ND = bsim1.B1subthSlopeD;
            N = N0 + NB * vbs + ND * vds; /* subthreshold slope */
            if (N < 0.5) N = 0.5;
            Warg1 = Math.Exp(-vds / Circuit.CONSTvt0);
            Wds = 1 - Warg1;
            Wgs = Math.Exp(Vgs_Vth / (N * Circuit.CONSTvt0));
            Vtsquare = Circuit.CONSTvt0 * Circuit.CONSTvt0;
            Warg2 = 6.04965 * Vtsquare * bsim1.B1betaZero;
            Ilimit = 4.5 * Vtsquare * bsim1.B1betaZero;
            Iexp = Warg2 * Wgs * Wds;
            DrainCurrent = DrainCurrent + Ilimit * Iexp / (Ilimit + Iexp);
            Temp1 = Ilimit / (Ilimit + Iexp);
            Temp1 = Temp1 * Temp1;
            Temp3 = Ilimit / (Ilimit + Wgs * Warg2);
            Temp3 = Temp3 * Temp3 * Warg2 * Wgs;
            /*    if ( Temp3 > Ilimit ) Temp3=Ilimit;*/
            gm = gm + Temp1 * Iexp / (N * Circuit.CONSTvt0);
            /* gds term has been modified to prevent blow up at Vds=0 */
            gds = gds + Temp3 * (-Wds / N / Circuit.CONSTvt0 * (dVthdVds +
                Vgs_Vth * ND / N) + Warg1 / Circuit.CONSTvt0);
            gmbs = gmbs - Temp1 * Iexp * (dVthdVbs + Vgs_Vth * NB / N) /
                (N * Circuit.CONSTvt0);

            ChargeComputation:

            /* Some Limiting of DC Parameters */
            if (DrainCurrent < 0.0) DrainCurrent = 0.0;
            if (gm < 0.0) gm = 0.0;
            if (gds < 0.0) gds = 0.0;
            if (gmbs < 0.0) gmbs = 0.0;

            WLCox = model.B1Cox *
                (bsim1.B1l - model.B1deltaL * 1.0e-6) *
                (bsim1.B1w - model.B1deltaW * 1.0e-6) * 1.0e4;   /* F */

            if (!ChargeComputationNeeded)
            {
                qg = 0;
                qd = 0;
                qb = 0;
                cggb = 0;
                cgsb = 0;
                cgdb = 0;
                cdgb = 0;
                cdsb = 0;
                cddb = 0;
                cbgb = 0;
                cbsb = 0;
                cbdb = 0;
                goto finished;
            }
            G = 1.0 - 1.0 / (1.744 + 0.8364 * Vpb);
            A = 1.0 + 0.5 * G * K1 / SqrtVpb;
            A = Math.Max(A, 1.0);   /* Modified */
                                    /*Arg  =  1 + Ugs * Vgs_Vth;*/
            dGdVbs = -0.8364 * (1 - G) * (1 - G);
            dAdVbs = 0.25 * K1 / SqrtVpb * (2 * dGdVbs + G / Vpb);
            Phi = Math.Max(0.1, Phi);

            if (model.B1channelChargePartitionFlag >= 1)
            {

                /*0/100 partitioning for drain/source chArges at the saturation region*/
                Vth0 = Vfb + Phi + K1 * SqrtVpb;
                Vgs_Vth = vgs - Vth0;
                Arg1 = A * vds;
                Arg2 = Vgs_Vth - 0.5 * Arg1;
                Arg3 = vds - Arg1;
                Arg5 = Arg1 * Arg1;
                dVthdVbs = -0.5 * K1 / SqrtVpb;
                dAdVbs = 0.5 * K1 * (0.5 * G / Vpb - 0.8364 * (1 - G) * (1 - G)) /
                    SqrtVpb;
                Ent = Math.Max(Arg2, 1.0e-8);
                dEntdVds = -0.5 * A;
                dEntdVbs = -dVthdVbs - 0.5 * vds * dAdVbs;
                Vcom = Vgs_Vth * Vgs_Vth / 6.0 - 1.25e-1 * Arg1 *
                            Vgs_Vth + 2.5e-2 * Arg5;
                VdsPinchoff = Math.Max(Vgs_Vth / A, 0.0);
                Vgb = vgs - vbs;
                Vgb_Vfb = Vgb - Vfb;

                if (Vgb_Vfb < 0)
                {
                    /* Accumulation Region */
                    qg = WLCox * Vgb_Vfb;
                    qb = -qg;
                    qd = 0.0;
                    cggb = WLCox;
                    cgdb = 0.0;
                    cgsb = 0.0;
                    cbgb = -WLCox;
                    cbdb = 0.0;
                    cbsb = 0.0;
                    cdgb = 0.0;
                    cddb = 0.0;
                    cdsb = 0.0;
                    goto finished;
                }
                else if (vgs < Vth0)
                {
                    /* Subthreshold Region */
                    qg = 0.5 * WLCox * K1 * K1 * (-1 +
                        Math.Sqrt(1 + 4 * Vgb_Vfb / (K1 * K1)));
                    qb = -qg;
                    qd = 0.0;
                    cggb = WLCox / Math.Sqrt(1 + 4 * Vgb_Vfb / (K1 * K1));
                    cgdb = cgsb = 0.0;
                    cbgb = -cggb;
                    cbdb = cbsb = cdgb = cddb = cdsb = 0.0;
                    goto finished;
                }
                else if (vds < VdsPinchoff)
                {    /* triode region  */
                     /*Vgs_Vth2 = Vgs_Vth*Vgs_Vth;*/
                    EntSquare = Ent * Ent;
                    Argl1 = 1.2e1 * EntSquare;
                    Argl2 = 1.0 - A;
                    Argl3 = Arg1 * vds;
                    /*Argl4 = Vcom/Ent/EntSquare;*/
                    if (Ent > 1.0e-8)
                    {
                        Argl5 = Arg1 / Ent;
                        /*Argl6 = Vcom/EntSquare;*/
                    }
                    else
                    {
                        Argl5 = 2.0;
                        Argl6 = 4.0 / 1.5e1;
                    }
                    Argl7 = Argl5 / 1.2e1;
                    Argl8 = 6.0 * Ent;
                    Argl9 = 0.125 * Argl5 * Argl5;
                    qg = WLCox * (vgs - Vfb - Phi - 0.5 * vds + vds * Argl7);
                    qb = WLCox * (-Vth0 + Vfb + Phi + 0.5 * Arg3 - Arg3 * Argl7);
                    qd = -WLCox * (0.5 * Vgs_Vth - 0.75 * Arg1 +
                        0.125 * Arg1 * Argl5);
                    cggb = WLCox * (1.0 - Argl3 / Argl1);
                    cgdb = WLCox * (-0.5 + Arg1 / Argl8 - Argl3 * dEntdVds /
                        Argl1);
                    cgbb = WLCox * (vds * vds * dAdVbs * Ent - Argl3 * dEntdVbs) /
                        Argl1;
                    cgsb = -(cggb + cgdb + cgbb);
                    cbgb = WLCox * Argl3 * Argl2 / Argl1;
                    cbdb = WLCox * Argl2 * (0.5 - Arg1 / Argl8 + Argl3 * dEntdVds /
                        Argl1);
                    cbbb = -WLCox * (dVthdVbs + 0.5 * vds * dAdVbs + vds *
                        vds * ((1.0 - 2.0 * A) * dAdVbs * Ent - Argl2 *
                        A * dEntdVbs) / Argl1);
                    cbsb = -(cbgb + cbdb + cbbb);
                    cdgb = -WLCox * (0.5 - Argl9);
                    cddb = WLCox * (0.75 * A - 0.25 * A * Arg1 / Ent +
                        Argl9 * dEntdVds);
                    cdbb = WLCox * (0.5 * dVthdVbs + vds * dAdVbs *
                        (0.75 - 0.25 * Argl5) + Argl9 * dEntdVbs);
                    cdsb = -(cdgb + cddb + cdbb);
                    goto finished;
                }
                else if (vds >= VdsPinchoff)
                {    /* saturation region   */
                    Args1 = 1.0 / (3.0 * A);
                    qg = WLCox * (vgs - Vfb - Phi - Vgs_Vth * Args1);
                    qb = WLCox * (Vfb + Phi - Vth0 + (1.0 - A) * Vgs_Vth * Args1);
                    qd = 0.0;
                    cggb = WLCox * (1.0 - Args1);
                    cgdb = 0.0;
                    cgbb = WLCox * Args1 * (dVthdVbs + Vgs_Vth * dAdVbs / A);
                    cgsb = -(cggb + cgdb + cgbb);
                    cbgb = WLCox * (Args1 - 1.0 / 3.0);
                    cbdb = 0.0;
                    cbbb = -WLCox * ((2.0 / 3.0 + Args1) * dVthdVbs +
                        Vgs_Vth * Args1 * dAdVbs / A);      /* Modified */
                    cbsb = -(cbgb + cbdb + cbbb);
                    cdgb = 0.0;
                    cddb = 0.0;
                    cdsb = 0.0;
                    goto finished;
                }

                goto finished;

            }
            else
            {
                /* ChannelChargePartionFlag  < = 0 */

                /*40/60 partitioning for drain/source chArges at the saturation region*/
                co4v15 = 4.0 / 15.0;
                Vth0 = Vfb + Phi + K1 * SqrtVpb;
                Vgs_Vth = vgs - Vth0;
                Arg1 = A * vds;
                Arg2 = Vgs_Vth - 0.5 * Arg1;
                Arg3 = vds - Arg1;
                Arg5 = Arg1 * Arg1;
                dVthdVbs = -0.5 * K1 / SqrtVpb;
                dAdVbs = 0.5 * K1 * (0.5 * G / Vpb - 0.8364 * (1 - G) * (1 - G)) / SqrtVpb;
                Ent = Math.Max(Arg2, 1.0e-8);
                dEntdVds = -0.5 * A;
                dEntdVbs = -dVthdVbs - 0.5 * vds * dAdVbs;
                Vcom = Vgs_Vth * Vgs_Vth / 6.0 - 1.25e-1 * Arg1 * Vgs_Vth + 2.5e-2 * Arg5;
                VdsPinchoff = Math.Max(Vgs_Vth / A, 0.0);
                Vgb = vgs - vbs;
                Vgb_Vfb = Vgb - Vfb;

                if (Vgb_Vfb < 0)
                {           /* Accumulation Region */
                    qg = WLCox * Vgb_Vfb;
                    qb = -qg;
                    qd = 0.0;
                    cggb = WLCox;
                    cgdb = 0.0;
                    cgsb = 0.0;
                    cbgb = -WLCox;
                    cbdb = 0.0;
                    cbsb = 0.0;
                    cdgb = 0.0;
                    cddb = 0.0;
                    cdsb = 0.0;
                    goto finished;
                }
                else if (vgs < Vth0)
                {    /* Subthreshold Region */
                    qg = 0.5 * WLCox * K1 * K1 * (-1 + Math.Sqrt(1 + 4 * Vgb_Vfb / (K1 * K1)));
                    qb = -qg;
                    qd = 0.0;
                    cggb = WLCox / Math.Sqrt(1 + 4 * Vgb_Vfb / (K1 * K1));
                    cgdb = cgsb = 0.0;
                    cbgb = -cggb;
                    cbdb = cbsb = cdgb = cddb = cdsb = 0.0;
                    goto finished;
                }
                else if (vds < VdsPinchoff)
                {      /* triode region */

                    Vgs_VthSquare = Vgs_Vth * Vgs_Vth;
                    EntSquare = Ent * Ent;
                    Argl1 = 1.2e1 * EntSquare;
                    Argl2 = 1.0 - A;
                    Argl3 = Arg1 * vds;
                    Argl4 = Vcom / Ent / EntSquare;
                    if (Ent > 1.0e-8)
                    {
                        Argl5 = Arg1 / Ent;
                        Argl6 = Vcom / EntSquare;
                    }
                    else
                    {
                        Argl5 = 2.0;
                        Argl6 = 4.0 / 1.5e1;
                    }
                    Argl7 = Argl5 / 1.2e1;
                    Argl8 = 6.0 * Ent;
                    qg = WLCox * (vgs - Vfb - Phi - 0.5 * vds + vds * Argl7);
                    qb = WLCox * (-Vth0 + Vfb + Phi + 0.5 * Arg3 - Arg3 * Argl7);
                    qd = -WLCox * (0.5 * (Vgs_Vth - Arg1) + Arg1 * Argl6);
                    cggb = WLCox * (1.0 - Argl3 / Argl1);
                    cgdb = WLCox * (-0.5 + Arg1 / Argl8 - Argl3 * dEntdVds / Argl1);
                    cgbb = WLCox * (vds * vds * dAdVbs * Ent - Argl3 * dEntdVbs) / Argl1;
                    cgsb = -(cggb + cgdb + cgbb);
                    cbgb = WLCox * Argl3 * Argl2 / Argl1;
                    cbdb = WLCox * Argl2 * (0.5 - Arg1 / Argl8 + Argl3 * dEntdVds / Argl1);
                    cbbb = -WLCox * (dVthdVbs + 0.5 * vds * dAdVbs + vds * vds * ((1.0 - 2.0 * A)
                        * dAdVbs * Ent - Argl2 * A * dEntdVbs) / Argl1);
                    cbsb = -(cbgb + cbdb + cbbb);
                    cdgb = -WLCox * (0.5 + Arg1 * (4.0 * Vgs_Vth - 1.5 * Arg1) / Argl1 -
                        2.0 * Arg1 * Argl4);
                    cddb = WLCox * (0.5 * A + 2.0 * Arg1 * dEntdVds * Argl4 - A * (2.0 * Vgs_VthSquare
                        - 3.0 * Arg1 * Vgs_Vth + 0.9 * Arg5) / Argl1);
                    cdbb = WLCox * (0.5 * dVthdVbs + 0.5 * vds * dAdVbs + 2.0 * Arg1 * dEntdVbs
                        * Argl4 - vds * (2.0 * Vgs_VthSquare * dAdVbs - 4.0 * A * Vgs_Vth * dVthdVbs - 3.0
                        * Arg1 * Vgs_Vth * dAdVbs + 1.5 * A * Arg1 * dVthdVbs + 0.9 * Arg5 * dAdVbs)
                        / Argl1);
                    cdsb = -(cdgb + cddb + cdbb);
                    goto finished;
                }
                else if (vds >= VdsPinchoff)
                {      /* saturation region */

                    Args1 = 1.0 / (3.0 * A);
                    qg = WLCox * (vgs - Vfb - Phi - Vgs_Vth * Args1);
                    qb = WLCox * (Vfb + Phi - Vth0 + (1.0 - A) * Vgs_Vth * Args1);
                    qd = -co4v15 * WLCox * Vgs_Vth;
                    cggb = WLCox * (1.0 - Args1);
                    cgdb = 0.0;
                    cgbb = WLCox * Args1 * (dVthdVbs + Vgs_Vth * dAdVbs / A);
                    cgsb = -(cggb + cgdb + cgbb);
                    cbgb = WLCox * (Args1 - 1.0 / 3.0);
                    cbdb = 0.0;
                    cbbb = -WLCox * ((2.0 / 3.0 + Args1) * dVthdVbs + Vgs_Vth * Args1 * dAdVbs / A);
                    cbsb = -(cbgb + cbdb + cbbb);
                    cdgb = -co4v15 * WLCox;
                    cddb = 0.0;
                    cdbb = co4v15 * WLCox * dVthdVbs;
                    cdsb = -(cdgb + cddb + cdbb);
                    goto finished;
                }
            }

            finished:       /* returning Values to Calling Routine */


            gmPointer = Math.Max(gm, 0.0);
            gdsPointer = Math.Max(gds, 0.0);
            gmbsPointer = Math.Max(gmbs, 0.0);
            qgPointer = qg;
            qbPointer = qb;
            qdPointer = qd;
            cggbPointer = cggb;
            cgdbPointer = cgdb;
            cgsbPointer = cgsb;
            cbgbPointer = cbgb;
            cbdbPointer = cbdb;
            cbsbPointer = cbsb;
            cdgbPointer = cdgb;
            cddbPointer = cddb;
            cdsbPointer = cdsb;
            cdrainPointer = Math.Max(DrainCurrent, 0.0);
            vonPointer = Von;
            vdsatPointer = VdsSat;
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
        public static void B1mosCap(Circuit ckt, double vgd, double vgs, double vgb, double[] args,
            double cbgb, double cbdb, double cbsb, double cdgb, double cddb, double cdsb,
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
            gcssbPointer = (args[4] + args[1] - (args[7] + cbsb + cdsb)) * ag0;
            gcggbPointer = (args[5] + args[0] + args[1] + args[2]) * ag0;
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
