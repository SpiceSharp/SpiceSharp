using System;
using SpiceSharp.Circuits;
using SpiceSharp.Components.Transistors;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// General behaviour for a <see cref="MOS3"/>
    /// </summary>
    public class MOS3LoadBehavior : CircuitObjectBehaviorLoad
    {
        /// <summary>
        /// Setup the behaviour
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        public override void Setup(ICircuitObject component, Circuit ckt)
        {
            base.Setup(component, ckt);
            var mos3 = ComponentTyped<MOS3>();
            mos3.MOS3vdsat = 0;
            mos3.MOS3von = 0;
            mos3.MOS3mode = 1;
        }

        /// <summary>
        /// Execute behaviour
        /// </summary>
        /// <param name="ckt"></param>
        public override void Load(Circuit ckt)
        {
            var mos3 = ComponentTyped<MOS3>();
            var model = mos3.Model as MOS3Model;
            var state = ckt.State;
            var rstate = state;
            var method = ckt.Method;
            double vt, EffectiveLength, DrainSatCur, SourceSatCur, GateSourceOverlapCap, GateDrainOverlapCap, GateBulkOverlapCap, Beta,
                OxideCap, vgs, vds, vbs, vbd, vgb, vgd, vgdo, delvbs, delvbd, delvgs, delvds, delvgd, cdhat, cbhat, von, evbs, evbd, vdsat,
                cdrain, sargsw, vgs1, vgd1, vgb1, capgs = 0.0, capgd = 0.0, capgb = 0.0, gcgs, ceqgs, gcgd, ceqgd, gcgb, ceqgb, ceqbs, ceqbd, cdreq;
            int Check, xnrm, xrev;

            vt = Circuit.CONSTKoverQ * mos3.MOS3temp;
            Check = 1;

            /* DETAILPROF */

            /* first, we compute a few useful values - these could be
			* pre - computed, but for historical reasons are still done
			* here.  They may be moved at the expense of instance size
			*/

            EffectiveLength = mos3.MOS3l - 2 * model.MOS3latDiff;
            if ((mos3.MOS3tSatCurDens == 0) || (mos3.MOS3drainArea.Value == 0) || (mos3.MOS3sourceArea.Value == 0))
            {
                DrainSatCur = mos3.MOS3tSatCur;
                SourceSatCur = mos3.MOS3tSatCur;
            }
            else
            {
                DrainSatCur = mos3.MOS3tSatCurDens * mos3.MOS3drainArea;
                SourceSatCur = mos3.MOS3tSatCurDens * mos3.MOS3sourceArea;
            }
            GateSourceOverlapCap = model.MOS3gateSourceOverlapCapFactor * mos3.MOS3w;
            GateDrainOverlapCap = model.MOS3gateDrainOverlapCapFactor * mos3.MOS3w;
            GateBulkOverlapCap = model.MOS3gateBulkOverlapCapFactor * EffectiveLength;
            Beta = mos3.MOS3tTransconductance * mos3.MOS3w / EffectiveLength;
            OxideCap = model.MOS3oxideCapFactor * EffectiveLength * mos3.MOS3w;

            /* DETAILPROF */

            /* 
			* ok - now to do the start - up operations
			* 
			* we must get values for vbs, vds, and vgs from somewhere
			* so we either predict them or recover them from last iteration
			* These are the two most common cases - either a prediction
			* step or the general iteration step and they
			* share some code, so we put them first - others later on
			*/

            if ((state.Init == CircuitState.InitFlags.InitFloat || state.UseSmallSignal || (state.Init == CircuitState.InitFlags.InitTransient)) ||
                ((state.Init == CircuitState.InitFlags.InitFix) && (!mos3.MOS3off)))
            {
                // General iteration
                vbs = model.MOS3type * (rstate.Solution[mos3.MOS3bNode] - rstate.Solution[mos3.MOS3sNodePrime]);
                vgs = model.MOS3type * (rstate.Solution[mos3.MOS3gNode] - rstate.Solution[mos3.MOS3sNodePrime]);
                vds = model.MOS3type * (rstate.Solution[mos3.MOS3dNodePrime] - rstate.Solution[mos3.MOS3sNodePrime]);

                /* now some common crunching for some more useful quantities */
                /* DETAILPROF */

                vbd = vbs - vds;
                vgd = vgs - vds;
                vgdo = state.States[0][mos3.MOS3states + MOS3.MOS3vgs] - state.States[0][mos3.MOS3states + MOS3.MOS3vds];
                delvbs = vbs - state.States[0][mos3.MOS3states + MOS3.MOS3vbs];
                delvbd = vbd - state.States[0][mos3.MOS3states + MOS3.MOS3vbd];
                delvgs = vgs - state.States[0][mos3.MOS3states + MOS3.MOS3vgs];
                delvds = vds - state.States[0][mos3.MOS3states + MOS3.MOS3vds];
                delvgd = vgd - vgdo;

                /* these are needed for convergence testing */

                if (mos3.MOS3mode >= 0)
                {
                    cdhat = mos3.MOS3cd - mos3.MOS3gbd * delvbd + mos3.MOS3gmbs * delvbs + mos3.MOS3gm * delvgs + mos3.MOS3gds * delvds;
                }
                else
                {
                    cdhat = mos3.MOS3cd - (mos3.MOS3gbd - mos3.MOS3gmbs) * delvbd - mos3.MOS3gm * delvgd + mos3.MOS3gds * delvds;
                }
                cbhat = mos3.MOS3cbs + mos3.MOS3cbd + mos3.MOS3gbd * delvbd + mos3.MOS3gbs * delvbs;

                /* DETAILPROF */
                /* NOBYPASS */

                /* DETAILPROF */
                /* ok - bypass is out, do it the hard way */

                von = model.MOS3type * mos3.MOS3von;

                /* 
				* limiting
				* we want to keep device voltages from changing
				* so fast that the exponentials churn out overflows
				* and similar rudeness
				*/

                if (state.States[0][mos3.MOS3states + MOS3.MOS3vds] >= 0)
                {
                    vgs = Transistor.DEVfetlim(vgs, state.States[0][mos3.MOS3states + MOS3.MOS3vgs], von);
                    vds = vgs - vgd;
                    vds = Transistor.DEVlimvds(vds, state.States[0][mos3.MOS3states + MOS3.MOS3vds]);
                    vgd = vgs - vds;
                }
                else
                {
                    vgd = Transistor.DEVfetlim(vgd, vgdo, von);
                    vds = vgs - vgd;
                    vds = -Transistor.DEVlimvds(-vds, -(state.States[0][mos3.MOS3states + MOS3.MOS3vds]));
                    vgs = vgd + vds;
                }
                if (vds >= 0)
                {
                    vbs = Transistor.DEVpnjlim(vbs, state.States[0][mos3.MOS3states + MOS3.MOS3vbs], vt, mos3.MOS3sourceVcrit, ref Check);
                    vbd = vbs - vds;
                }
                else
                {
                    vbd = Transistor.DEVpnjlim(vbd, state.States[0][mos3.MOS3states + MOS3.MOS3vbd], vt, mos3.MOS3drainVcrit, ref Check);
                    vbs = vbd + vds;
                }
                /* NODELIMITING */

            }
            else
            {
                /* DETAILPROF */
                /* ok - not one of the simple cases, so we have to
				* look at all of the possibilities for why we were
				* called.  We still just initialize the three voltages
				*/

                if ((state.Init == CircuitState.InitFlags.InitJct) && !mos3.MOS3off)
                {
                    vds = model.MOS3type * mos3.MOS3icVDS;
                    vgs = model.MOS3type * mos3.MOS3icVGS;
                    vbs = model.MOS3type * mos3.MOS3icVBS;
                    if ((vds == 0) && (vgs == 0) && (vbs == 0) && ((method != null || state.UseDC ||
                        state.Domain == CircuitState.DomainTypes.None) || (!state.UseIC)))
                    {
                        vbs = -1;
                        vgs = model.MOS3type * mos3.MOS3tVto;
                        vds = 0;
                    }
                }
                else
                {
                    vbs = vgs = vds = 0;
                }
            }

            /* DETAILPROF */
            /* 
			* now all the preliminaries are over - we can start doing the
			* real work
			*/
            vbd = vbs - vds;
            vgd = vgs - vds;
            vgb = vgs - vbs;

            /* 
			* bulk - source and bulk - drain diodes
			* here we just evaluate the ideal diode current and the
			* corresponding derivative (conductance).
			*/
            if (vbs <= 0)
            {
                mos3.MOS3gbs = SourceSatCur / vt;
                mos3.MOS3cbs = mos3.MOS3gbs * vbs;
                mos3.MOS3gbs += state.Gmin;
            }
            else
            {
                evbs = Math.Exp(Math.Min(Transistor.MAX_EXP_ARG, vbs / vt));
                mos3.MOS3gbs = SourceSatCur * evbs / vt + state.Gmin;
                mos3.MOS3cbs = SourceSatCur * (evbs - 1);
            }
            if (vbd <= 0)
            {
                mos3.MOS3gbd = DrainSatCur / vt;
                mos3.MOS3cbd = mos3.MOS3gbd * vbd;
                mos3.MOS3gbd += state.Gmin;
            }
            else
            {
                evbd = Math.Exp(Math.Min(Transistor.MAX_EXP_ARG, vbd / vt));
                mos3.MOS3gbd = DrainSatCur * evbd / vt + state.Gmin;
                mos3.MOS3cbd = DrainSatCur * (evbd - 1);
            }

            /* now to determine whether the user was able to correctly
			* identify the source and drain of his device
			*/
            if (vds >= 0)
            {
                /* normal mode */
                mos3.MOS3mode = 1;
            }
            else
            {
                /* inverse mode */
                mos3.MOS3mode = -1;
            }

            /* DETAILPROF */
            {
                /* 
				* subroutine moseq3(vds, vbs, vgs, gm, gds, gmbs, 
				* qg, qc, qb, cggb, cgdb, cgsb, cbgb, cbdb, cbsb)
				*/

                /* 
				* this routine evaluates the drain current, its derivatives and
				* the charges associated with the gate, channel and bulk
				* for mosfets based on semi - empirical equations
				*/

                /* 
				common / mosarg / vto, beta, gamma, phi, phib, cox, xnsub, xnfs, xd, xj, xld, 
				1   xlamda, uo, uexp, vbp, utra, vmax, xneff, xl, xw, vbi, von, vdsat, qspof, 
				2   beta0, beta1, cdrain, xqco, xqc, fnarrw, fshort, lev
				common / status / omega, time, delta, delold(7), ag(7), vt, xni, egfet, 
				1   xmu, sfactr, mode, modedc, icalc, initf, method, iord, maxord, noncon, 
				2   iterno, itemno, nosolv, modac, ipiv, ivmflg, ipostp, iscrch, iofile
				common / knstnt / twopi, xlog2, xlog10, root2, rad, boltz, charge, ctok, 
				1   gmin, reltol, abstol, vntol, trtol, chgtol, eps0, epssil, epsox, 
				2   pivtol, pivrel
				*/

                /* equivalence (xlamda, alpha), (vbp, theta), (uexp, eta), (utra, xkappa) */

                double coeff0 = 0.0631353e0;
                double coeff1 = 0.8013292e0;
                double coeff2 = -0.01110777e0;
                double oneoverxl; /* 1 / effective length */
                double eta; /* eta from model after length factor */
                double phibs; /* phi - vbs */
                double sqphbs; /* square root of phibs */
                double dsqdvb; /*  */
                double sqphis; /* square root of phi */
                double sqphs3; /* square root of phi cubed */
                double wps;
                double oneoverxj; /* 1 / junction depth */
                double xjonxl; /* junction depth / effective length */
                double djonxj, wponxj, arga, argb, argc, dwpdvb, dadvb, dbdvb, gammas, fbodys, fbody, onfbdy, qbonco, vbix, wconxj, dfsdvb,
                    dfbdvb, dqbdvb, vth, dvtdvb, csonco, cdonco, dxndvb = 0.0, dvodvb = 0.0, dvodvd = 0.0, vgsx, dvtdvd, onfg, fgate, us, dfgdvg, dfgdvd,
                    dfgdvb, dvsdvg, dvsdvb, dvsdvd, xn = 0.0, vdsc, onvdsc = 0.0, dvsdga, vdsx, dcodvb, cdnorm, cdo, cd1, fdrain = 0.0, fd2, dfddvg = 0.0, dfddvb = 0.0,
                    dfddvd = 0.0, gdsat, cdsat, gdoncd, gdonfd, gdonfg, dgdvg, dgdvd, dgdvb, emax, emongd, demdvg, demdvd, demdvb, delxl, dldvd,
                    dldem, ddldvg, ddldvd, ddldvb, dlonxl, xlfact, diddl, gds0 = 0.0, emoncd, ondvt, onxn, wfact, gms, gmw, fshort;

                /* 
				* bypasses the computation of charges
				*/

                /* 
				* reference cdrain equations to source and
				* charge equations to bulk
				*/
                vdsat = 0.0;
                oneoverxl = 1.0 / EffectiveLength;
                eta = model.MOS3eta * 8.15e-22 / (model.MOS3oxideCapFactor * EffectiveLength * EffectiveLength * EffectiveLength);
                /* 
				* .....square root term
				*/
                if ((mos3.MOS3mode == 1 ? vbs : vbd) <= 0.0)
                {
                    phibs = mos3.MOS3tPhi - (mos3.MOS3mode == 1 ? vbs : vbd);
                    sqphbs = Math.Sqrt(phibs);
                    dsqdvb = -0.5 / sqphbs;
                }
                else
                {
                    sqphis = Math.Sqrt(mos3.MOS3tPhi);
                    sqphs3 = mos3.MOS3tPhi * sqphis;
                    sqphbs = sqphis / (1.0 + (mos3.MOS3mode == 1 ? vbs : vbd) / (mos3.MOS3tPhi + mos3.MOS3tPhi));
                    phibs = sqphbs * sqphbs;
                    dsqdvb = -phibs / (sqphs3 + sqphs3);
                }
                /* 
				 * .....short channel effect factor
				 */
                if ((model.MOS3junctionDepth != 0.0) && (model.MOS3coeffDepLayWidth != 0.0))
                {
                    wps = model.MOS3coeffDepLayWidth * sqphbs;
                    oneoverxj = 1.0 / model.MOS3junctionDepth;
                    xjonxl = model.MOS3junctionDepth * oneoverxl;
                    djonxj = model.MOS3latDiff * oneoverxj;
                    wponxj = wps * oneoverxj;
                    wconxj = coeff0 + coeff1 * wponxj + coeff2 * wponxj * wponxj;
                    arga = wconxj + djonxj;
                    argc = wponxj / (1.0 + wponxj);
                    argb = Math.Sqrt(1.0 - argc * argc);
                    fshort = 1.0 - xjonxl * (arga * argb - djonxj);
                    dwpdvb = model.MOS3coeffDepLayWidth * dsqdvb;
                    dadvb = (coeff1 + coeff2 * (wponxj + wponxj)) * dwpdvb * oneoverxj;
                    dbdvb = -argc * argc * (1.0 - argc) * dwpdvb / (argb * wps);
                    dfsdvb = -xjonxl * (dadvb * argb + arga * dbdvb);
                }
                else
                {
                    fshort = 1.0;
                    dfsdvb = 0.0;
                }
                /* 
				 * .....body effect
				 */
                gammas = model.MOS3gamma * fshort;
                fbodys = 0.5 * gammas / (sqphbs + sqphbs);
                fbody = fbodys + model.MOS3narrowFactor / mos3.MOS3w;
                onfbdy = 1.0 / (1.0 + fbody);
                dfbdvb = -fbodys * dsqdvb / sqphbs + fbodys * dfsdvb / fshort;
                qbonco = gammas * sqphbs + model.MOS3narrowFactor * phibs / mos3.MOS3w;
                dqbdvb = gammas * dsqdvb + model.MOS3gamma * dfsdvb * sqphbs - model.MOS3narrowFactor / mos3.MOS3w;
                /* 
				 * .....static feedback effect
				 */
                vbix = mos3.MOS3tVbi * model.MOS3type - eta * (mos3.MOS3mode * vds);
                /* 
				 * .....threshold voltage
				 */
                vth = vbix + qbonco;
                dvtdvd = -eta;
                dvtdvb = dqbdvb;
                /* 
				 * .....joint weak inversion and strong inversion
				 */
                von = vth;
                if (model.MOS3fastSurfaceStateDensity != 0.0)
                {
                    csonco = Circuit.CHARGE * model.MOS3fastSurfaceStateDensity * 1e4 /* (cm *  * 2 / m *  * 2) */  * EffectiveLength * mos3.MOS3w /
                        OxideCap;
                    cdonco = qbonco / (phibs + phibs);
                    xn = 1.0 + csonco + cdonco;
                    von = vth + vt * xn;
                    dxndvb = dqbdvb / (phibs + phibs) - qbonco * dsqdvb / (phibs * sqphbs);
                    dvodvd = dvtdvd;
                    dvodvb = dvtdvb + vt * dxndvb;
                }
                else
                {
                    /* 
					 * .....cutoff region
					 */
                    if ((mos3.MOS3mode == 1 ? vgs : vgd) <= von)
                    {
                        cdrain = 0.0;
                        mos3.MOS3gm = 0.0;
                        mos3.MOS3gds = 0.0;
                        mos3.MOS3gmbs = 0.0;
                        goto innerline1000;
                    }
                }
                /* 
				 * .....device is on
				 */
                vgsx = Math.Max((mos3.MOS3mode == 1 ? vgs : vgd), von);
                /* 
				 * .....mobility modulation by gate voltage
				 */
                onfg = 1.0 + model.MOS3theta * (vgsx - vth);
                fgate = 1.0 / onfg;
                us = mos3.MOS3tSurfMob * 1e-4 /*(m**2/cm**2)*/ * fgate;
                dfgdvg = -model.MOS3theta * fgate * fgate;
                dfgdvd = -dfgdvg * dvtdvd;
                dfgdvb = -dfgdvg * dvtdvb;
                /* 
				 * .....saturation voltage
				 */
                vdsat = (vgsx - vth) * onfbdy;
                if (model.MOS3maxDriftVel <= 0.0)
                {
                    dvsdvg = onfbdy;
                    dvsdvd = -dvsdvg * dvtdvd;
                    dvsdvb = -dvsdvg * dvtdvb - vdsat * dfbdvb * onfbdy;
                }
                else
                {
                    vdsc = EffectiveLength * model.MOS3maxDriftVel / us;
                    onvdsc = 1.0 / vdsc;
                    arga = (vgsx - vth) * onfbdy;
                    argb = Math.Sqrt(arga * arga + vdsc * vdsc);
                    vdsat = arga + vdsc - argb;
                    dvsdga = (1.0 - arga / argb) * onfbdy;
                    dvsdvg = dvsdga - (1.0 - vdsc / argb) * vdsc * dfgdvg * onfg;
                    dvsdvd = -dvsdvg * dvtdvd;
                    dvsdvb = -dvsdvg * dvtdvb - arga * dvsdga * dfbdvb;
                }
                /* 
				 * .....current factors in linear region
				 */
                vdsx = Math.Min((mos3.MOS3mode * vds), vdsat);
                if (vdsx == 0.0)
                    goto line900;
                cdo = vgsx - vth - 0.5 * (1.0 + fbody) * vdsx;
                dcodvb = -dvtdvb - 0.5 * dfbdvb * vdsx;
                /* 
				 * .....normalized drain current
				 */
                cdnorm = cdo * vdsx;
                mos3.MOS3gm = vdsx;
                mos3.MOS3gds = vgsx - vth - (1.0 + fbody + dvtdvd) * vdsx;
                mos3.MOS3gmbs = dcodvb * vdsx;
                /* 
				 * .....drain current without velocity saturation effect
				 */
                cd1 = Beta * cdnorm;
                Beta = Beta * fgate;
                cdrain = Beta * cdnorm;
                mos3.MOS3gm = Beta * mos3.MOS3gm + dfgdvg * cd1;
                mos3.MOS3gds = Beta * mos3.MOS3gds + dfgdvd * cd1;
                mos3.MOS3gmbs = Beta * mos3.MOS3gmbs;
                /* 
				 * .....velocity saturation factor
				 */
                if (model.MOS3maxDriftVel != 0.0)
                {
                    fdrain = 1.0 / (1.0 + vdsx * onvdsc);
                    fd2 = fdrain * fdrain;
                    arga = fd2 * vdsx * onvdsc * onfg;
                    dfddvg = -dfgdvg * arga;
                    dfddvd = -dfgdvd * arga - fd2 * onvdsc;
                    dfddvb = -dfgdvb * arga;
                    /* 
					 * .....drain current
					 */
                    mos3.MOS3gm = fdrain * mos3.MOS3gm + dfddvg * cdrain;
                    mos3.MOS3gds = fdrain * mos3.MOS3gds + dfddvd * cdrain;
                    mos3.MOS3gmbs = fdrain * mos3.MOS3gmbs + dfddvb * cdrain;
                    cdrain = fdrain * cdrain;
                    Beta = Beta * fdrain;
                }
                /* 
				 * .....channel length modulation
				 */
                if ((mos3.MOS3mode * vds) <= vdsat) goto line700;
                if (model.MOS3maxDriftVel <= 0.0) goto line510;
                if (model.MOS3alpha == 0.0)
                    goto line700;
                cdsat = cdrain;
                gdsat = cdsat * (1.0 - fdrain) * onvdsc;
                gdsat = Math.Max(1.0e-12, gdsat);
                gdoncd = gdsat / cdsat;
                gdonfd = gdsat / (1.0 - fdrain);
                gdonfg = gdsat * onfg;
                dgdvg = gdoncd * mos3.MOS3gm - gdonfd * dfddvg + gdonfg * dfgdvg;
                dgdvd = gdoncd * mos3.MOS3gds - gdonfd * dfddvd + gdonfg * dfgdvd;
                dgdvb = gdoncd * mos3.MOS3gmbs - gdonfd * dfddvb + gdonfg * dfgdvb;

                emax = model.MOS3kappa * cdsat * oneoverxl / gdsat;
                emoncd = emax / cdsat;
                emongd = emax / gdsat;
                demdvg = emoncd * mos3.MOS3gm - emongd * dgdvg;
                demdvd = emoncd * mos3.MOS3gds - emongd * dgdvd;
                demdvb = emoncd * mos3.MOS3gmbs - emongd * dgdvb;

                arga = 0.5 * emax * model.MOS3alpha;
                argc = model.MOS3kappa * model.MOS3alpha;
                argb = Math.Sqrt(arga * arga + argc * ((mos3.MOS3mode * vds) - vdsat));
                delxl = argb - arga;
                dldvd = argc / (argb + argb);
                dldem = 0.5 * (arga / argb - 1.0) * model.MOS3alpha;
                ddldvg = dldem * demdvg;
                ddldvd = dldem * demdvd - dldvd;
                ddldvb = dldem * demdvb;
                goto line520;
                line510:
                delxl = Math.Sqrt(model.MOS3kappa * ((mos3.MOS3mode * vds) - vdsat) * model.MOS3alpha);
                dldvd = 0.5 * delxl / ((mos3.MOS3mode * vds) - vdsat);
                ddldvg = 0.0;
                ddldvd = -dldvd;
                ddldvb = 0.0;
                /* 
				 * .....punch through approximation
				 */
                line520:
                if (delxl > (0.5 * EffectiveLength))
                {
                    delxl = EffectiveLength - (EffectiveLength * EffectiveLength / (4.0 * delxl));
                    arga = 4.0 * (EffectiveLength - delxl) * (EffectiveLength - delxl) / (EffectiveLength * EffectiveLength);
                    ddldvg = ddldvg * arga;
                    ddldvd = ddldvd * arga;
                    ddldvb = ddldvb * arga;
                    dldvd = dldvd * arga;
                }
                /* 
				 * .....saturation region
				 */
                dlonxl = delxl * oneoverxl;
                xlfact = 1.0 / (1.0 - dlonxl);
                cdrain = cdrain * xlfact;
                diddl = cdrain / (EffectiveLength - delxl);
                mos3.MOS3gm = mos3.MOS3gm * xlfact + diddl * ddldvg;
                gds0 = mos3.MOS3gds * xlfact + diddl * ddldvd;
                mos3.MOS3gmbs = mos3.MOS3gmbs * xlfact + diddl * ddldvb;
                mos3.MOS3gm = mos3.MOS3gm + gds0 * dvsdvg;
                mos3.MOS3gmbs = mos3.MOS3gmbs + gds0 * dvsdvb;
                mos3.MOS3gds = gds0 * dvsdvd + diddl * dldvd;
                /* 
				 * .....finish strong inversion case
				 */
                line700:
                if ((mos3.MOS3mode == 1 ? vgs : vgd) < von)
                {
                    /* 
					 * .....weak inversion
					 */
                    onxn = 1.0 / xn;
                    ondvt = onxn / vt;
                    wfact = Math.Exp(((mos3.MOS3mode == 1 ? vgs : vgd) - von) * ondvt);
                    cdrain = cdrain * wfact;
                    gms = mos3.MOS3gm * wfact;
                    gmw = cdrain * ondvt;
                    mos3.MOS3gm = gmw;
                    if ((mos3.MOS3mode * vds) > vdsat)
                    {
                        mos3.MOS3gm = mos3.MOS3gm + gds0 * dvsdvg * wfact;
                    }
                    mos3.MOS3gds = mos3.MOS3gds * wfact + (gms - gmw) * dvodvd;
                    mos3.MOS3gmbs = mos3.MOS3gmbs * wfact + (gms - gmw) * dvodvb - gmw * ((mos3.MOS3mode == 1 ? vgs : vgd) - von) * onxn * dxndvb;
                }
                /* 
				 * .....charge computation
				 */
                goto innerline1000;
                /* 
				 * .....special case of vds = 0.0d0 */
                line900: Beta = Beta * fgate;
                cdrain = 0.0;
                mos3.MOS3gm = 0.0;
                mos3.MOS3gds = Beta * (vgsx - vth);
                mos3.MOS3gmbs = 0.0;
                if ((model.MOS3fastSurfaceStateDensity != 0.0) && ((mos3.MOS3mode == 1 ? vgs : vgd) < von))
                {
                    mos3.MOS3gds *= Math.Exp(((mos3.MOS3mode == 1 ? vgs : vgd) - von) / (vt * xn));
                }
                innerline1000:;
                /* 
				 * .....done
				 */
            }

            /* DETAILPROF */

            /* now deal with n vs p polarity */

            mos3.MOS3von = model.MOS3type * von;
            mos3.MOS3vdsat = model.MOS3type * vdsat;
            /* line 490 */
            /* 
			* COMPUTE EQUIVALENT DRAIN CURRENT SOURCE
			*/
            mos3.MOS3cd = mos3.MOS3mode * cdrain - mos3.MOS3cbd;

            if (state.Domain == CircuitState.DomainTypes.Time || state.UseSmallSignal)
            {
                /* 
				* now we do the hard part of the bulk - drain and bulk - source
				* diode - we evaluate the non - linear capacitance and
				* charge
				* 
				* the basic equations are not hard, but the implementation
				* is somewhat long in an attempt to avoid log / exponential
				* evaluations
				*/
                /* 
				* charge storage elements
				* 
				* .. bulk - drain and bulk - source depletion capacitances
				*/
                /* CAPBYPASS */
                {
                    /* can't bypass the diode capacitance calculations */
                    /* CAPZEROBYPASS */
                    if (vbs < mos3.MOS3tDepCap)
                    {
                        double arg = 1 - vbs / mos3.MOS3tBulkPot, sarg;
                        /* 
						* the following block looks somewhat long and messy, 
						* but since most users use the default grading
						* coefficients of .5, and sqrt is MUCH faster than an
						* Math.Exp(Math.Log()) we use this special case code to buy time.
						* (as much as 10% of total job time!)
						*/
                        if (model.MOS3bulkJctBotGradingCoeff.Value == model.MOS3bulkJctSideGradingCoeff)
                        {
                            if (model.MOS3bulkJctBotGradingCoeff.Value == .5)
                            {
                                sarg = sargsw = 1 / Math.Sqrt(arg);
                            }
                            else
                            {
                                sarg = sargsw = Math.Exp(-model.MOS3bulkJctBotGradingCoeff * Math.Log(arg));
                            }
                        }
                        else
                        {
                            if (model.MOS3bulkJctBotGradingCoeff.Value == .5)
                            {
                                sarg = 1 / Math.Sqrt(arg);
                            }
                            else
                            {
                                /* NOSQRT */
                                sarg = Math.Exp(-model.MOS3bulkJctBotGradingCoeff * Math.Log(arg));
                            }
                            if (model.MOS3bulkJctSideGradingCoeff.Value == .5)
                            {
                                sargsw = 1 / Math.Sqrt(arg);
                            }
                            else
                            {
                                /* NOSQRT */
                                sargsw = Math.Exp(-model.MOS3bulkJctSideGradingCoeff * Math.Log(arg));
                            }
                        }
                        /* NOSQRT */
                        state.States[0][mos3.MOS3states + MOS3.MOS3qbs] = mos3.MOS3tBulkPot * (mos3.MOS3Cbs * (1 - arg * sarg) / (1 - model.MOS3bulkJctBotGradingCoeff) +
                            mos3.MOS3Cbssw * (1 - arg * sargsw) / (1 - model.MOS3bulkJctSideGradingCoeff));
                        mos3.MOS3capbs = mos3.MOS3Cbs * sarg + mos3.MOS3Cbssw * sargsw;
                    }
                    else
                    {
                        state.States[0][mos3.MOS3states + MOS3.MOS3qbs] = mos3.MOS3f4s + vbs * (mos3.MOS3f2s + vbs * (mos3.MOS3f3s / 2));
                        mos3.MOS3capbs = mos3.MOS3f2s + mos3.MOS3f3s * vbs;
                    }
                    /* CAPZEROBYPASS */
                }
                /* CAPBYPASS */
                /* can't bypass the diode capacitance calculations */
                {
                    /* CAPZEROBYPASS */
                    if (vbd < mos3.MOS3tDepCap)
                    {
                        double arg = 1 - vbd / mos3.MOS3tBulkPot, sarg;
                        /* 
						* the following block looks somewhat long and messy, 
						* but since most users use the default grading
						* coefficients of .5, and sqrt is MUCH faster than an
						* Math.Exp(Math.Log()) we use this special case code to buy time.
						* (as much as 10% of total job time!)
						*/
                        if (model.MOS3bulkJctBotGradingCoeff.Value == .5 && model.MOS3bulkJctSideGradingCoeff.Value == .5)
                        {
                            sarg = sargsw = 1 / Math.Sqrt(arg);
                        }
                        else
                        {
                            if (model.MOS3bulkJctBotGradingCoeff.Value == .5)
                            {
                                sarg = 1 / Math.Sqrt(arg);
                            }
                            else
                            {
                                /* NOSQRT */
                                sarg = Math.Exp(-model.MOS3bulkJctBotGradingCoeff * Math.Log(arg));
                            }
                            if (model.MOS3bulkJctSideGradingCoeff.Value == .5)
                            {
                                sargsw = 1 / Math.Sqrt(arg);
                            }
                            else
                            {
                                /* NOSQRT */
                                sargsw = Math.Exp(-model.MOS3bulkJctSideGradingCoeff * Math.Log(arg));
                            }
                        }
                        /* NOSQRT */
                        state.States[0][mos3.MOS3states + MOS3.MOS3qbd] = mos3.MOS3tBulkPot * (mos3.MOS3Cbd * (1 - arg * sarg) / (1 - model.MOS3bulkJctBotGradingCoeff) +
                            mos3.MOS3Cbdsw * (1 - arg * sargsw) / (1 - model.MOS3bulkJctSideGradingCoeff));
                        mos3.MOS3capbd = mos3.MOS3Cbd * sarg + mos3.MOS3Cbdsw * sargsw;
                    }
                    else
                    {
                        state.States[0][mos3.MOS3states + MOS3.MOS3qbd] = mos3.MOS3f4d + vbd * (mos3.MOS3f2d + vbd * mos3.MOS3f3d / 2);
                        mos3.MOS3capbd = mos3.MOS3f2d + vbd * mos3.MOS3f3d;
                    }
                    /* CAPZEROBYPASS */
                }
                /* DETAILPROF */

                if (method != null)
                {
                    /* (above only excludes tranop, since we're only at this
					* point if tran or tranop)
					*/

                    /* 
					* calculate equivalent conductances and currents for
					* depletion capacitors
					*/

                    /* integrate the capacitors and save results */
                    var result = method.Integrate(state, mos3.MOS3states + MOS3.MOS3qbd, mos3.MOS3capbd);
                    mos3.MOS3gbd += result.Geq;
                    mos3.MOS3cbd += state.States[0][mos3.MOS3states + MOS3.MOS3cqbd];
                    mos3.MOS3cd -= state.States[0][mos3.MOS3states + MOS3.MOS3cqbd];
                    result = method.Integrate(state, mos3.MOS3states + MOS3.MOS3qbs, mos3.MOS3capbs);
                    mos3.MOS3gbs += result.Geq;
                    mos3.MOS3cbs += state.States[0][mos3.MOS3states + MOS3.MOS3cqbs];
                }
            }
            /* DETAILPROF */

            /* 
			 * check convergence
			 */
            if (!mos3.MOS3off || (!(state.Init == CircuitState.InitFlags.InitFix || state.UseSmallSignal)))
            {
                if (Check == 1)
                    state.IsCon = false;
            }

            /* DETAILPROF */

            /* save things away for next time */

            state.States[0][mos3.MOS3states + MOS3.MOS3vbs] = vbs;
            state.States[0][mos3.MOS3states + MOS3.MOS3vbd] = vbd;
            state.States[0][mos3.MOS3states + MOS3.MOS3vgs] = vgs;
            state.States[0][mos3.MOS3states + MOS3.MOS3vds] = vds;

            /* DETAILPROF */

            /* 
			 * meyer's capacitor model
			 */
            if (state.Domain == CircuitState.DomainTypes.Time || state.UseSmallSignal)
            {
                /* 
				 * calculate meyer's capacitors
				 */
                /* 
				 * new cmeyer - this just evaluates at the current time, 
				 * expects you to remember values from previous time
				 * returns 1 / 2 of non - constant portion of capacitance
				 * you must add in the other half from previous time
				 * and the constant part
				 */
                double icapgs, icapgd, icapgb;
                if (mos3.MOS3mode > 0)
                {
                    Transistor.DEVqmeyer(vgs, vgd, vgb, von, vdsat,
                        out icapgs, out icapgd, out icapgb, mos3.MOS3tPhi, OxideCap);
                }
                else
                {
                    Transistor.DEVqmeyer(vgd, vgs, vgb, von, vdsat,
                        out icapgd, out icapgs, out icapgb, mos3.MOS3tPhi, OxideCap);
                }
                state.States[0][mos3.MOS3states + MOS3.MOS3capgs] = icapgs;
                state.States[0][mos3.MOS3states + MOS3.MOS3capgd] = icapgd;
                state.States[0][mos3.MOS3states + MOS3.MOS3capgb] = icapgb;
                vgs1 = state.States[1][mos3.MOS3states + MOS3.MOS3vgs];
                vgd1 = vgs1 - state.States[1][mos3.MOS3states + MOS3.MOS3vds];
                vgb1 = vgs1 - state.States[1][mos3.MOS3states + MOS3.MOS3vbs];
                if (state.Domain == CircuitState.DomainTypes.Time && state.UseDC)
                {
                    capgs = 2 * state.States[0][mos3.MOS3states + MOS3.MOS3capgs] + GateSourceOverlapCap;
                    capgd = 2 * state.States[0][mos3.MOS3states + MOS3.MOS3capgd] + GateDrainOverlapCap;
                    capgb = 2 * state.States[0][mos3.MOS3states + MOS3.MOS3capgb] + GateBulkOverlapCap;
                }
                else
                {
                    capgs = (state.States[0][mos3.MOS3states + MOS3.MOS3capgs] + state.States[1][mos3.MOS3states + MOS3.MOS3capgs] + GateSourceOverlapCap);
                    capgd = (state.States[0][mos3.MOS3states + MOS3.MOS3capgd] + state.States[1][mos3.MOS3states + MOS3.MOS3capgd] + GateDrainOverlapCap);
                    capgb = (state.States[0][mos3.MOS3states + MOS3.MOS3capgb] + state.States[1][mos3.MOS3states + MOS3.MOS3capgb] + GateBulkOverlapCap);
                }

                /* DETAILPROF */
                /* 
				 * store small - signal parameters (for meyer's model)
				 * all parameters already stored, so done...
				 */

                /* PREDICTOR */
                if (method != null)
                {
                    state.States[0][mos3.MOS3states + MOS3.MOS3qgs] = (vgs - vgs1) * capgs + state.States[1][mos3.MOS3states + MOS3.MOS3qgs];
                    state.States[0][mos3.MOS3states + MOS3.MOS3qgd] = (vgd - vgd1) * capgd + state.States[1][mos3.MOS3states + MOS3.MOS3qgd];
                    state.States[0][mos3.MOS3states + MOS3.MOS3qgb] = (vgb - vgb1) * capgb + state.States[1][mos3.MOS3states + MOS3.MOS3qgb];
                }
                else
                {
                    /* TRANOP only */
                    state.States[0][mos3.MOS3states + MOS3.MOS3qgs] = vgs * capgs;
                    state.States[0][mos3.MOS3states + MOS3.MOS3qgd] = vgd * capgd;
                    state.States[0][mos3.MOS3states + MOS3.MOS3qgb] = vgb * capgb;
                }
                /* PREDICTOR */
            }

            /* DETAILPROF */

            if (method == null || method.SavedTime == 0.0)
            {
                /* 
				 * initialize to zero charge conductances 
				 * and current
				 */
                gcgs = 0;
                ceqgs = 0;
                gcgd = 0;
                ceqgd = 0;
                gcgb = 0;
                ceqgb = 0;
            }
            else
            {
                if (capgs == 0)
                    state.States[0][mos3.MOS3states + MOS3.MOS3cqgs] = 0;
                if (capgd == 0)
                    state.States[0][mos3.MOS3states + MOS3.MOS3cqgd] = 0;
                if (capgb == 0)
                    state.States[0][mos3.MOS3states + MOS3.MOS3cqgb] = 0;
                /* 
				 * calculate equivalent conductances and currents for
				 * meyer"s capacitors
				 */
                method.Integrate(state, out gcgs, out ceqgs, mos3.MOS3states + MOS3.MOS3qgs, capgs);
                method.Integrate(state, out gcgd, out ceqgd, mos3.MOS3states + MOS3.MOS3qgd, capgd);
                method.Integrate(state, out gcgb, out ceqgb, mos3.MOS3states + MOS3.MOS3qgb, capgb);
                ceqgs = ceqgs - gcgs * vgs + method.Slope * state.States[0][mos3.MOS3states + MOS3.MOS3qgs];
                ceqgd = ceqgd - gcgd * vgd + method.Slope * state.States[0][mos3.MOS3states + MOS3.MOS3qgd];
                ceqgb = ceqgb - gcgb * vgb + method.Slope * state.States[0][mos3.MOS3states + MOS3.MOS3qgb];
            }
            /* 
			 * store charge storage info for meyer's cap in lx table
			 */

            /* DETAILPROF */
            /* 
			 * load current vector
			 */
            ceqbs = model.MOS3type * (mos3.MOS3cbs - (mos3.MOS3gbs - state.Gmin) * vbs);
            ceqbd = model.MOS3type * (mos3.MOS3cbd - (mos3.MOS3gbd - state.Gmin) * vbd);
            if (mos3.MOS3mode >= 0)
            {
                xnrm = 1;
                xrev = 0;
                cdreq = model.MOS3type * (cdrain - mos3.MOS3gds * vds - mos3.MOS3gm * vgs - mos3.MOS3gmbs * vbs);
            }
            else
            {
                xnrm = 0;
                xrev = 1;
                cdreq = -(model.MOS3type) * (cdrain - mos3.MOS3gds * (-vds) - mos3.MOS3gm * vgd - mos3.MOS3gmbs * vbd);
            }

            rstate.Rhs[mos3.MOS3gNode] -= (model.MOS3type * (ceqgs + ceqgb + ceqgd));
            rstate.Rhs[mos3.MOS3bNode] -= (ceqbs + ceqbd - model.MOS3type * ceqgb);
            rstate.Rhs[mos3.MOS3dNodePrime] += (ceqbd - cdreq + model.MOS3type * ceqgd);
            rstate.Rhs[mos3.MOS3sNodePrime] += cdreq + ceqbs + model.MOS3type * ceqgs;

            /* 
			 * load y matrix
			 */
            mos3.MOS3DdPtr.Add(mos3.MOS3drainConductance);
            mos3.MOS3GgPtr.Add(gcgd + gcgs + gcgb);
            mos3.MOS3SsPtr.Add(mos3.MOS3sourceConductance);
            mos3.MOS3BbPtr.Add(mos3.MOS3gbd + mos3.MOS3gbs + gcgb);
            mos3.MOS3DPdpPtr.Add(mos3.MOS3drainConductance + mos3.MOS3gds + mos3.MOS3gbd + xrev * (mos3.MOS3gm + mos3.MOS3gmbs) + gcgd);
            mos3.MOS3SPspPtr.Add(mos3.MOS3sourceConductance + mos3.MOS3gds + mos3.MOS3gbs + xnrm * (mos3.MOS3gm + mos3.MOS3gmbs) + gcgs);
            mos3.MOS3DdpPtr.Add(-mos3.MOS3drainConductance);
            mos3.MOS3GbPtr.Sub(gcgb);
            mos3.MOS3GdpPtr.Sub(gcgd);
            mos3.MOS3GspPtr.Sub(gcgs);
            mos3.MOS3SspPtr.Add(-mos3.MOS3sourceConductance);
            mos3.MOS3BgPtr.Sub(gcgb);
            mos3.MOS3BdpPtr.Sub(mos3.MOS3gbd);
            mos3.MOS3BspPtr.Sub(mos3.MOS3gbs);
            mos3.MOS3DPdPtr.Add(-mos3.MOS3drainConductance);
            mos3.MOS3DPgPtr.Add((xnrm - xrev) * mos3.MOS3gm - gcgd);
            mos3.MOS3DPbPtr.Add(-mos3.MOS3gbd + (xnrm - xrev) * mos3.MOS3gmbs);
            mos3.MOS3DPspPtr.Add(-mos3.MOS3gds - xnrm * (mos3.MOS3gm + mos3.MOS3gmbs));
            mos3.MOS3SPgPtr.Add(-(xnrm - xrev) * mos3.MOS3gm - gcgs);
            mos3.MOS3SPsPtr.Add(-mos3.MOS3sourceConductance);
            mos3.MOS3SPbPtr.Add(-mos3.MOS3gbs - (xnrm - xrev) * mos3.MOS3gmbs);
            mos3.MOS3SPdpPtr.Add(-mos3.MOS3gds - xrev * (mos3.MOS3gm + mos3.MOS3gmbs));
        }

        /// <summary>
        /// Check convergence
        /// </summary>
        /// <param name="ckt">Circuit</param>
        /// <returns></returns>
        public override bool IsConvergent(Circuit ckt)
        {
            var mos3 = ComponentTyped<MOS3>();
            var model = mos3.Model as MOS3Model;
            var config = ckt.Simulation.CurrentConfig;
            var state = ckt.State;

            double vbs, vgs, vds, vbd, vgd, vgdo, delvbs, delvbd, delvgs, delvds, delvgd, cdhat, cbhat;

            vbs = model.MOS3type * (state.Solution[mos3.MOS3bNode] - state.Solution[mos3.MOS3sNodePrime]);
            vgs = model.MOS3type * (state.Solution[mos3.MOS3gNode] - state.Solution[mos3.MOS3sNodePrime]);
            vds = model.MOS3type * (state.Solution[mos3.MOS3dNodePrime] - state.Solution[mos3.MOS3sNodePrime]);
            vbd = vbs - vds;
            vgd = vgs - vds;
            vgdo = state.States[0][mos3.MOS3states + MOS3.MOS3vgs] - state.States[0][mos3.MOS3states + MOS3.MOS3vds];
            delvbs = vbs - state.States[0][mos3.MOS3states + MOS3.MOS3vbs];
            delvbd = vbd - state.States[0][mos3.MOS3states + MOS3.MOS3vbd];
            delvgs = vgs - state.States[0][mos3.MOS3states + MOS3.MOS3vgs];
            delvds = vds - state.States[0][mos3.MOS3states + MOS3.MOS3vds];
            delvgd = vgd - vgdo;

            /* these are needed for convergence testing */

            if (mos3.MOS3mode >= 0)
            {
                cdhat = mos3.MOS3cd - mos3.MOS3gbd * delvbd + mos3.MOS3gmbs * delvbs +
                    mos3.MOS3gm * delvgs + mos3.MOS3gds * delvds;
            }
            else
            {
                cdhat = mos3.MOS3cd - (mos3.MOS3gbd - mos3.MOS3gmbs) * delvbd -
                    mos3.MOS3gm * delvgd + mos3.MOS3gds * delvds;
            }
            cbhat = mos3.MOS3cbs + mos3.MOS3cbd + mos3.MOS3gbd * delvbd + mos3.MOS3gbs * delvbs;

            /*
             *  check convergence
             */
            double tol = config.RelTol * Math.Max(Math.Abs(cdhat), Math.Abs(mos3.MOS3cd)) + config.AbsTol;
            if (Math.Abs(cdhat - mos3.MOS3cd) >= tol)
            {
                state.IsCon = false;
                return false;
            }
            else
            {
                tol = config.RelTol * Math.Max(Math.Abs(cbhat), Math.Abs(mos3.MOS3cbs + mos3.MOS3cbd)) + config.AbsTol;
                if (Math.Abs(cbhat - (mos3.MOS3cbs + mos3.MOS3cbd)) > tol)
                {
                    state.IsCon = false;
                    return false;
                }
            }
            return true;
        }
    }
}
