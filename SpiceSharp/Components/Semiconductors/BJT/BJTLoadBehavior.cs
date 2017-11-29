using System;
using SpiceSharp.Circuits;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.Semiconductors;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// General behaviour for <see cref="BJT"/>
    /// </summary>
    public class BJTLoadBehavior : CircuitObjectBehaviorLoad
    {
        /// <summary>
        /// Execute behaviour for DC and Transient analysis
        /// </summary>
        /// <param name="ckt"></param>
        public override void Execute(Circuit ckt)
        {
            BJT bjt = ComponentTyped<BJT>();
            BJTModel model = bjt.Model as BJTModel;
            var state = ckt.State;
            var rstate = state;
            var method = ckt.Method;
            double vt;
            double gccs, ceqcs, geqbx, ceqbx, geqcb, csat, rbpr, rbpi, gcpr, gepr, oik, c2, vte, oikr, c4, vtc, td, xjrb, vbe, vbc, vbx, vcs;
            bool icheck;
            double vce, delvbe, delvbc, cchat, cbhat;
            bool ichk1;
            double vtn, evbe, cbe, gbe, gben, evben, cben, evbc, cbc, gbc, gbcn, evbcn, cbcn, q1, dqbdve, dqbdvc, q2, arg, sqarg, qb, cc, cex,
                gex, arg1, arg2, denom, arg3, cb, gx, gpi, gmu, go, gm, tf, tr, czbe, pe, xme, cdis, ctot, czbc, czbx, pc, xmc, fcpe, czcs, ps,
                xms, xtf, ovtf, xjtf, argtf, temp, sarg, capbe, f1, f2, f3, czbef2, fcpc, capbc, czbcf2, capbx = 0.0, czbxf2, capcs = 0.0;
            double ceqbe, ceqbc, ceq;

            vt = bjt.BJTtemp * Circuit.CONSTKoverQ;

            gccs = 0;
            ceqcs = 0;
            geqbx = 0;
            ceqbx = 0;
            geqcb = 0;

            /* 
			 * dc model paramters
			 */
            csat = bjt.BJTtSatCur * bjt.BJTarea;
            rbpr = model.BJTminBaseResist / bjt.BJTarea;
            rbpi = model.BJTbaseResist / bjt.BJTarea - rbpr;
            gcpr = model.BJTcollectorConduct * bjt.BJTarea;
            gepr = model.BJTemitterConduct * bjt.BJTarea;
            oik = model.BJTinvRollOffF / bjt.BJTarea;
            c2 = bjt.BJTtBEleakCur * bjt.BJTarea;
            vte = model.BJTleakBEemissionCoeff * vt;
            oikr = model.BJTinvRollOffR / bjt.BJTarea;
            c4 = bjt.BJTtBCleakCur * bjt.BJTarea;
            vtc = model.BJTleakBCemissionCoeff * vt;
            td = model.BJTexcessPhaseFactor;
            xjrb = model.BJTbaseCurrentHalfResist * bjt.BJTarea;

            /* 
			* initialization
			*/
            icheck = true;
            if (state.UseSmallSignal)
            {
                vbe = state.States[0][bjt.BJTstate + BJT.BJTvbe];
                vbc = state.States[0][bjt.BJTstate + BJT.BJTvbc];
                vbx = model.BJTtype * (rstate.Solution[bjt.BJTbaseNode] - rstate.Solution[bjt.BJTcolPrimeNode]);
                vcs = model.BJTtype * (rstate.Solution[bjt.BJTsubstNode] - rstate.Solution[bjt.BJTcolPrimeNode]);
            }
            else if (state.Init == CircuitState.InitFlags.InitTransient)
            {
                vbe = state.States[1][bjt.BJTstate + BJT.BJTvbe];
                vbc = state.States[1][bjt.BJTstate + BJT.BJTvbc];
                vbx = model.BJTtype * (rstate.Solution[bjt.BJTbaseNode] - rstate.Solution[bjt.BJTcolPrimeNode]);
                vcs = model.BJTtype * (rstate.Solution[bjt.BJTsubstNode] - rstate.Solution[bjt.BJTcolPrimeNode]);
                if (state.UseIC)
                {
                    vbx = model.BJTtype * (bjt.BJTicVBE - bjt.BJTicVCE);
                    vcs = 0;
                }
                icheck = false; // EDIT: Spice does not check the first timepoint for convergence, but we do...
            }
            else if (state.Init == CircuitState.InitFlags.InitJct && state.Domain == CircuitState.DomainTypes.Time && state.UseDC && state.UseIC)
            {
                vbe = model.BJTtype * bjt.BJTicVBE;
                vce = model.BJTtype * bjt.BJTicVCE;
                vbc = vbe - vce;
                vbx = vbc;
                vcs = 0;
            }
            else if (state.Init == CircuitState.InitFlags.InitJct && !bjt.BJToff)
            {
                vbe = bjt.BJTtVcrit;
                vbc = 0;
                /* ERROR:  need to initialize VCS, VBX here */
                vcs = vbx = 0;
            }
            else if (state.Init == CircuitState.InitFlags.InitJct || (state.Init == CircuitState.InitFlags.InitFix && bjt.BJToff))
            {
                vbe = 0;
                vbc = 0;
                /* ERROR:  need to initialize VCS, VBX here */
                vcs = vbx = 0;
            }
            else
            {
                /* 
                 * compute new nonlinear branch voltages
                 */
                vbe = model.BJTtype * (rstate.Solution[bjt.BJTbasePrimeNode] - rstate.Solution[bjt.BJTemitPrimeNode]);
                vbc = model.BJTtype * (rstate.Solution[bjt.BJTbasePrimeNode] - rstate.Solution[bjt.BJTcolPrimeNode]);

                /* PREDICTOR */
                delvbe = vbe - state.States[0][bjt.BJTstate + BJT.BJTvbe];
                delvbc = vbc - state.States[0][bjt.BJTstate + BJT.BJTvbc];
                vbx = model.BJTtype * (rstate.Solution[bjt.BJTbaseNode] - rstate.Solution[bjt.BJTcolPrimeNode]);
                vcs = model.BJTtype * (rstate.Solution[bjt.BJTsubstNode] - rstate.Solution[bjt.BJTcolPrimeNode]);
                cchat = state.States[0][bjt.BJTstate + BJT.BJTcc] + (state.States[0][bjt.BJTstate + BJT.BJTgm] + state.States[0][bjt.BJTstate + BJT.BJTgo]) * delvbe -
                     (state.States[0][bjt.BJTstate + BJT.BJTgo] + state.States[0][bjt.BJTstate + BJT.BJTgmu]) * delvbc;
                cbhat = state.States[0][bjt.BJTstate + BJT.BJTcb] + state.States[0][bjt.BJTstate + BJT.BJTgpi] * delvbe + state.States[0][bjt.BJTstate + BJT.BJTgmu] *
                     delvbc;
                /* NOBYPASS */
                /* 
				 * limit nonlinear branch voltages
				 */
                ichk1 = true;
                vbe = Semiconductor.DEVpnjlim(vbe, state.States[0][bjt.BJTstate + BJT.BJTvbe], vt, bjt.BJTtVcrit, ref icheck);
                vbc = Semiconductor.DEVpnjlim(vbc, state.States[0][bjt.BJTstate + BJT.BJTvbc], vt, bjt.BJTtVcrit, ref ichk1);
                if (ichk1 == true)
                    icheck = true;
            }

            /* 
			 * determine dc current and derivitives
			 */
            vtn = vt * model.BJTemissionCoeffF;
            if (vbe > -5 * vtn)
            {
                evbe = Math.Exp(vbe / vtn);
                cbe = csat * (evbe - 1) + state.Gmin * vbe;
                gbe = csat * evbe / vtn + state.Gmin;
                if (c2 == 0)
                {
                    cben = 0;
                    gben = 0;
                }
                else
                {
                    evben = Math.Exp(vbe / vte);
                    cben = c2 * (evben - 1);
                    gben = c2 * evben / vte;
                }
            }
            else
            {
                gbe = -csat / vbe + state.Gmin;
                cbe = gbe * vbe;
                gben = -c2 / vbe;
                cben = gben * vbe;
            }
            vtn = vt * model.BJTemissionCoeffR;
            if (vbc > -5 * vtn)
            {
                evbc = Math.Exp(vbc / vtn);
                cbc = csat * (evbc - 1) + state.Gmin * vbc;
                gbc = csat * evbc / vtn + state.Gmin;
                if (c4 == 0)
                {
                    cbcn = 0;
                    gbcn = 0;
                }
                else
                {
                    evbcn = Math.Exp(vbc / vtc);
                    cbcn = c4 * (evbcn - 1);
                    gbcn = c4 * evbcn / vtc;
                }
            }
            else
            {
                gbc = -csat / vbc + state.Gmin;
                cbc = gbc * vbc;
                gbcn = -c4 / vbc;
                cbcn = gbcn * vbc;
            }
            /* 
			 * determine base charge terms
			 */
            q1 = 1 / (1 - model.BJTinvEarlyVoltF * vbc - model.BJTinvEarlyVoltR * vbe);
            if (oik == 0 && oikr == 0)
            {
                qb = q1;
                dqbdve = q1 * qb * model.BJTinvEarlyVoltR;
                dqbdvc = q1 * qb * model.BJTinvEarlyVoltF;
            }
            else
            {
                q2 = oik * cbe + oikr * cbc;
                arg = Math.Max(0, 1 + 4 * q2);
                sqarg = 1;
                if (arg != 0)
                    sqarg = Math.Sqrt(arg);
                qb = q1 * (1 + sqarg) / 2;
                dqbdve = q1 * (qb * model.BJTinvEarlyVoltR + oik * gbe / sqarg);
                dqbdvc = q1 * (qb * model.BJTinvEarlyVoltF + oikr * gbc / sqarg);
            }
            /* 
			 * weil's approx. for excess phase applied with backward - 
			 * euler integration
			 */
            cc = 0;
            cex = cbe;
            gex = gbe;
            if (method != null && td != 0)
            {
                arg1 = method.Delta / td;
                arg2 = 3 * arg1;
                arg1 = arg2 * arg1;
                denom = 1 + arg1 + arg2;
                arg3 = arg1 / denom;
                if (method.SavedTime == 0.0)
                {
                    state.States[1][bjt.BJTstate + BJT.BJTcexbc] = cbe / qb;
                    state.States[2][bjt.BJTstate + BJT.BJTcexbc] = state.States[1][bjt.BJTstate + BJT.BJTcexbc];
                }
                cc = (state.States[1][bjt.BJTstate + BJT.BJTcexbc] * (1 + method.Delta / method.DeltaOld[1] + arg2) - state.States[2][bjt.BJTstate +
                     BJT.BJTcexbc] * method.Delta / method.DeltaOld[1]) / denom;
                cex = cbe * arg3;
                gex = gbe * arg3;
                state.States[0][bjt.BJTstate + BJT.BJTcexbc] = cc + cex / qb;
            }

            /* 
			 * determine dc incremental conductances
			 */
            cc = cc + (cex - cbc) / qb - cbc / bjt.BJTtBetaR - cbcn;
            cb = cbe / bjt.BJTtBetaF + cben + cbc / bjt.BJTtBetaR + cbcn;
            gx = rbpr + rbpi / qb;
            if (xjrb != 0)
            {
                arg1 = Math.Max(cb / xjrb, 1e-9);
                arg2 = (-1 + Math.Sqrt(1 + 14.59025 * arg1)) / 2.4317 / Math.Sqrt(arg1);
                arg1 = Math.Tan(arg2);
                gx = rbpr + 3 * rbpi * (arg1 - arg2) / arg2 / arg1 / arg1;
            }
            if (gx != 0)
                gx = 1 / gx;
            gpi = gbe / bjt.BJTtBetaF + gben;
            gmu = gbc / bjt.BJTtBetaR + gbcn;
            go = (gbc + (cex - cbc) * dqbdvc / qb) / qb;
            gm = (gex - (cex - cbc) * dqbdve / qb) / qb - go;

            if (state.Domain == CircuitState.DomainTypes.Time || state.UseIC || state.UseSmallSignal)
            {
                /* 
				 * charge storage elements
				 */
                tf = model.BJTtransitTimeF;
                tr = model.BJTtransitTimeR;
                czbe = bjt.BJTtBEcap * bjt.BJTarea;
                pe = bjt.BJTtBEpot;
                xme = model.BJTjunctionExpBE;
                cdis = model.BJTbaseFractionBCcap;
                ctot = bjt.BJTtBCcap * bjt.BJTarea;
                czbc = ctot * cdis;
                czbx = ctot - czbc;
                pc = bjt.BJTtBCpot;
                xmc = model.BJTjunctionExpBC;
                fcpe = bjt.BJTtDepCap;
                czcs = model.BJTcapCS * bjt.BJTarea;
                ps = model.BJTpotentialSubstrate;
                xms = model.BJTexponentialSubstrate;
                xtf = model.BJTtransitTimeBiasCoeffF;
                ovtf = model.BJTtransitTimeVBCFactor;
                xjtf = model.BJTtransitTimeHighCurrentF * bjt.BJTarea;
                if (tf != 0 && vbe > 0)
                {
                    argtf = 0;
                    arg2 = 0;
                    arg3 = 0;
                    if (xtf != 0)
                    {
                        argtf = xtf;
                        if (ovtf != 0)
                        {
                            argtf = argtf * Math.Exp(vbc * ovtf);
                        }
                        arg2 = argtf;
                        if (xjtf != 0)
                        {
                            temp = cbe / (cbe + xjtf);
                            argtf = argtf * temp * temp;
                            arg2 = argtf * (3 - temp - temp);
                        }
                        arg3 = cbe * argtf * ovtf;
                    }
                    cbe = cbe * (1 + argtf) / qb;
                    gbe = (gbe * (1 + arg2) - cbe * dqbdve) / qb;
                    geqcb = tf * (arg3 - cbe * dqbdvc) / qb;
                }
                if (vbe < fcpe)
                {
                    arg = 1 - vbe / pe;
                    sarg = Math.Exp(-xme * Math.Log(arg));
                    state.States[0][bjt.BJTstate + BJT.BJTqbe] = tf * cbe + pe * czbe * (1 - arg * sarg) / (1 - xme);
                    capbe = tf * gbe + czbe * sarg;
                }
                else
                {
                    f1 = bjt.BJTtf1;
                    f2 = model.BJTf2;
                    f3 = model.BJTf3;
                    czbef2 = czbe / f2;
                    state.States[0][bjt.BJTstate + BJT.BJTqbe] = tf * cbe + czbe * f1 + czbef2 * (f3 * (vbe - fcpe) + (xme / (pe + pe)) * (vbe * vbe -
                         fcpe * fcpe));
                    capbe = tf * gbe + czbef2 * (f3 + xme * vbe / pe);
                }
                fcpc = bjt.BJTtf4;
                f1 = bjt.BJTtf5;
                f2 = model.BJTf6;
                f3 = model.BJTf7;
                if (vbc < fcpc)
                {
                    arg = 1 - vbc / pc;
                    sarg = Math.Exp(-xmc * Math.Log(arg));
                    state.States[0][bjt.BJTstate + BJT.BJTqbc] = tr * cbc + pc * czbc * (1 - arg * sarg) / (1 - xmc);
                    capbc = tr * gbc + czbc * sarg;
                }
                else
                {
                    czbcf2 = czbc / f2;
                    state.States[0][bjt.BJTstate + BJT.BJTqbc] = tr * cbc + czbc * f1 + czbcf2 * (f3 * (vbc - fcpc) + (xmc / (pc + pc)) * (vbc * vbc -
                         fcpc * fcpc));
                    capbc = tr * gbc + czbcf2 * (f3 + xmc * vbc / pc);
                }
                if (vbx < fcpc)
                {
                    arg = 1 - vbx / pc;
                    sarg = Math.Exp(-xmc * Math.Log(arg));
                    state.States[0][bjt.BJTstate + BJT.BJTqbx] = pc * czbx * (1 - arg * sarg) / (1 - xmc);
                    capbx = czbx * sarg;
                }
                else
                {
                    czbxf2 = czbx / f2;
                    state.States[0][bjt.BJTstate + BJT.BJTqbx] = czbx * f1 + czbxf2 * (f3 * (vbx - fcpc) + (xmc / (pc + pc)) * (vbx * vbx - fcpc * fcpc));
                    capbx = czbxf2 * (f3 + xmc * vbx / pc);
                }
                if (vcs < 0)
                {
                    arg = 1 - vcs / ps;
                    sarg = Math.Exp(-xms * Math.Log(arg));
                    state.States[0][bjt.BJTstate + BJT.BJTqcs] = ps * czcs * (1 - arg * sarg) / (1 - xms);
                    capcs = czcs * sarg;
                }
                else
                {
                    state.States[0][bjt.BJTstate + BJT.BJTqcs] = vcs * czcs * (1 + xms * vcs / (2 * ps));
                    capcs = czcs * (1 + xms * vcs / ps);
                }
                bjt.BJTcapbe = capbe;
                bjt.BJTcapbc = capbc;
                bjt.BJTcapcs = capcs;
                bjt.BJTcapbx = capbx;

                /* 
				 * store small - signal parameters
				 */
                if (!(state.Domain == CircuitState.DomainTypes.Time && state.UseDC && state.UseIC))
                {
                    if (state.UseSmallSignal)
                    {
                        state.States[0][bjt.BJTstate + BJT.BJTcqbe] = capbe;
                        state.States[0][bjt.BJTstate + BJT.BJTcqbc] = capbc;
                        state.States[0][bjt.BJTstate + BJT.BJTcqcs] = capcs;
                        state.States[0][bjt.BJTstate + BJT.BJTcqbx] = capbx;
                        state.States[0][bjt.BJTstate + BJT.BJTcexbc] = geqcb;

                        /* SENSDEBUG */
                        return; /* go to 1000 */
                    }
                    /* 
					 * transient analysis
					 */

                    if (state.Init == CircuitState.InitFlags.InitTransient)
                    {
                        state.States[1][bjt.BJTstate + BJT.BJTqbe] = state.States[0][bjt.BJTstate + BJT.BJTqbe];
                        state.States[1][bjt.BJTstate + BJT.BJTqbc] = state.States[0][bjt.BJTstate + BJT.BJTqbc];
                        state.States[1][bjt.BJTstate + BJT.BJTqbx] = state.States[0][bjt.BJTstate + BJT.BJTqbx];
                        state.States[1][bjt.BJTstate + BJT.BJTqcs] = state.States[0][bjt.BJTstate + BJT.BJTqcs];
                    }

                    if (method != null)
                    {
                        var result = method.Integrate(state, bjt.BJTstate + BJT.BJTqbe, capbe);
                        geqcb = geqcb * method.Slope;
                        gpi = gpi + result.Geq;
                        result = method.Integrate(state, bjt.BJTstate + BJT.BJTqbc, capbc);
                        gmu = gmu + result.Geq;
                    }
                    cb = cb + state.States[0][bjt.BJTstate + BJT.BJTcqbe];
                    cb = cb + state.States[0][bjt.BJTstate + BJT.BJTcqbc];
                    cc = cc - state.States[0][bjt.BJTstate + BJT.BJTcqbc];

                    if (state.Init == CircuitState.InitFlags.InitTransient)
                    {
                        state.States[1][bjt.BJTstate + BJT.BJTcqbe] = state.States[0][bjt.BJTstate + BJT.BJTcqbe];
                        state.States[1][bjt.BJTstate + BJT.BJTcqbc] = state.States[0][bjt.BJTstate + BJT.BJTcqbc];
                    }
                }
            }

            /* 
			 * check convergence
			 */
            if (state.Init != CircuitState.InitFlags.InitFix || !bjt.BJToff)
            {
                if (icheck)
                    state.IsCon = false;
            }

            /* 
			 * charge storage for c - s and b - x junctions
			 */
            if (method != null)
            {
                method.Integrate(state, out gccs, out ceq, bjt.BJTstate + BJT.BJTqcs, capcs);
                method.Integrate(state, out geqbx, out ceq, bjt.BJTstate + BJT.BJTqbx, capbx);
                if (method.SavedTime == 0.0)
                {
                    state.States[1][bjt.BJTstate + BJT.BJTcqbx] = state.States[0][bjt.BJTstate + BJT.BJTcqbx];
                    state.States[1][bjt.BJTstate + BJT.BJTcqcs] = state.States[0][bjt.BJTstate + BJT.BJTcqcs];
                }
            }

            state.States[0][bjt.BJTstate + BJT.BJTvbe] = vbe;
            state.States[0][bjt.BJTstate + BJT.BJTvbc] = vbc;
            state.States[0][bjt.BJTstate + BJT.BJTcc] = cc;
            state.States[0][bjt.BJTstate + BJT.BJTcb] = cb;
            state.States[0][bjt.BJTstate + BJT.BJTgpi] = gpi;
            state.States[0][bjt.BJTstate + BJT.BJTgmu] = gmu;
            state.States[0][bjt.BJTstate + BJT.BJTgm] = gm;
            state.States[0][bjt.BJTstate + BJT.BJTgo] = go;
            state.States[0][bjt.BJTstate + BJT.BJTgx] = gx;
            state.States[0][bjt.BJTstate + BJT.BJTgeqcb] = geqcb;
            state.States[0][bjt.BJTstate + BJT.BJTgccs] = gccs;
            state.States[0][bjt.BJTstate + BJT.BJTgeqbx] = geqbx;

            /* Do not load the Jacobian and the rhs if
			   perturbation is being carried out */

            /* 
			 * load current excitation vector
			 */
            ceqcs = model.BJTtype * (state.States[0][bjt.BJTstate + BJT.BJTcqcs] - vcs * gccs);
            ceqbx = model.BJTtype * (state.States[0][bjt.BJTstate + BJT.BJTcqbx] - vbx * geqbx);
            ceqbe = model.BJTtype * (cc + cb - vbe * (gm + go + gpi) + vbc * (go - geqcb));
            ceqbc = model.BJTtype * (-cc + vbe * (gm + go) - vbc * (gmu + go));

            rstate.Rhs[bjt.BJTbaseNode] += (-ceqbx);
            rstate.Rhs[bjt.BJTcolPrimeNode] += (ceqcs + ceqbx + ceqbc);
            rstate.Rhs[bjt.BJTbasePrimeNode] += (-ceqbe - ceqbc);
            rstate.Rhs[bjt.BJTemitPrimeNode] += (ceqbe);
            rstate.Rhs[bjt.BJTsubstNode] += (-ceqcs);

            /* 
			 * load y matrix
			 */
            bjt.BJTcolColPtr.Add(gcpr);
            bjt.BJTbaseBasePtr.Add(gx + geqbx);
            bjt.BJTemitEmitPtr.Add(gepr);
            bjt.BJTcolPrimeColPrimePtr.Add(gmu + go + gcpr + gccs + geqbx);
            bjt.BJTbasePrimeBasePrimePtr.Add(gx + gpi + gmu + geqcb);
            bjt.BJTemitPrimeEmitPrimePtr.Add(gpi + gepr + gm + go);
            bjt.BJTcolColPrimePtr.Add(-gcpr);
            bjt.BJTbaseBasePrimePtr.Add(-gx);
            bjt.BJTemitEmitPrimePtr.Add(-gepr);
            bjt.BJTcolPrimeColPtr.Add(-gcpr);
            bjt.BJTcolPrimeBasePrimePtr.Add(-gmu + gm);
            bjt.BJTcolPrimeEmitPrimePtr.Add(-gm - go);
            bjt.BJTbasePrimeBasePtr.Add(-gx);
            bjt.BJTbasePrimeColPrimePtr.Add(-gmu - geqcb);
            bjt.BJTbasePrimeEmitPrimePtr.Add(-gpi);
            bjt.BJTemitPrimeEmitPtr.Add(-gepr);
            bjt.BJTemitPrimeColPrimePtr.Add(-go + geqcb);
            bjt.BJTemitPrimeBasePrimePtr.Add(-gpi - gm - geqcb);
            bjt.BJTsubstSubstPtr.Add(gccs);
            bjt.BJTcolPrimeSubstPtr.Add(-gccs);
            bjt.BJTsubstColPrimePtr.Add(-gccs);
            bjt.BJTbaseColPrimePtr.Add(-geqbx);
            bjt.BJTcolPrimeBasePtr.Add(-geqbx);
        }
    }
}
