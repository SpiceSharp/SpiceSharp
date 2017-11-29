using System;
using SpiceSharp.Circuits;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.Transistors;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// General behaviour of a <see cref="MOS2"/>
    /// </summary>
    public class MOS2LoadBehavior : CircuitObjectBehaviorLoad
    {
        private static double[] sig1 = new double[] { 1.0, -1.0, 1.0, -1.0 };
        private static double[] sig2 = new double[] { 1.0, 1.0, -1.0, -1.0 };

        /// <summary>
        /// Setup the behaviour
        /// </summary>
        /// <param name="component"></param>
        /// <param name="ckt"></param>
        public override void Setup(ICircuitObject component, Circuit ckt)
        {
            base.Setup(component, ckt);
            var mos2 = ComponentTyped<MOS2>();
            mos2.MOS2vdsat = 0.0;
            mos2.MOS2von = 0.0;
        }

        /// <summary>
        /// Execute behaviour
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Execute(Circuit ckt)
        {
            var mos2 = ComponentTyped<MOS2>();
            var model = mos2.Model as MOS2Model;
            var state = ckt.State;
            var rstate = state;
            var method = ckt.Method;
            double vt, EffectiveLength, DrainSatCur, SourceSatCur, GateSourceOverlapCap, GateDrainOverlapCap, GateBulkOverlapCap, Beta,
                OxideCap, vgs, vds, vbs, vbd, vgb, vgd, vgdo, delvbs, delvbd, delvgs, delvds, delvgd, cdhat, cbhat, von, evbs, evbd,
                vdsat, cdrain = 0.0, sargsw, vgs1, vgd1, vgb1, capgs = 0.0, capgd = 0.0, capgb = 0.0, gcgs, ceqgs, gcgd, ceqgd, gcgb, ceqgb, ceqbs,
                ceqbd, cdreq;
            int Check, xnrm, xrev;

            vt = Circuit.CONSTKoverQ * mos2.MOS2temp;
            Check = 1;

            EffectiveLength = mos2.MOS2l - 2 * model.MOS2latDiff;
            if ((mos2.MOS2tSatCurDens == 0) || (mos2.MOS2drainArea.Value == 0) || (mos2.MOS2sourceArea.Value == 0))
            {
                DrainSatCur = mos2.MOS2tSatCur;
                SourceSatCur = mos2.MOS2tSatCur;
            }
            else
            {
                DrainSatCur = mos2.MOS2tSatCurDens * mos2.MOS2drainArea;
                SourceSatCur = mos2.MOS2tSatCurDens * mos2.MOS2sourceArea;
            }
            GateSourceOverlapCap = model.MOS2gateSourceOverlapCapFactor * mos2.MOS2w;
            GateDrainOverlapCap = model.MOS2gateDrainOverlapCapFactor * mos2.MOS2w;
            GateBulkOverlapCap = model.MOS2gateBulkOverlapCapFactor * EffectiveLength;
            Beta = mos2.MOS2tTransconductance * mos2.MOS2w / EffectiveLength;
            OxideCap = model.MOS2oxideCapFactor * EffectiveLength * mos2.MOS2w;

            if ((state.Init == CircuitState.InitFlags.InitFloat || state.UseSmallSignal || (state.Init == CircuitState.InitFlags.InitTransient)) ||
                ((state.Init == CircuitState.InitFlags.InitFix) && (!mos2.MOS2off)))
            {
                // general iteration
                vbs = model.MOS2type * (rstate.Solution[mos2.MOS2bNode] - rstate.Solution[mos2.MOS2sNodePrime]);
                vgs = model.MOS2type * (rstate.Solution[mos2.MOS2gNode] - rstate.Solution[mos2.MOS2sNodePrime]);
                vds = model.MOS2type * (rstate.Solution[mos2.MOS2dNodePrime] - rstate.Solution[mos2.MOS2sNodePrime]);

                /* now some common crunching for some more useful quantities */

                vbd = vbs - vds;
                vgd = vgs - vds;
                vgdo = state.States[0][mos2.MOS2states + MOS2.MOS2vgs] - state.States[0][mos2.MOS2states + MOS2.MOS2vds];
                delvbs = vbs - state.States[0][mos2.MOS2states + MOS2.MOS2vbs];
                delvbd = vbd - state.States[0][mos2.MOS2states + MOS2.MOS2vbd];
                delvgs = vgs - state.States[0][mos2.MOS2states + MOS2.MOS2vgs];
                delvds = vds - state.States[0][mos2.MOS2states + MOS2.MOS2vds];
                delvgd = vgd - vgdo;

                /* these are needed for convergence testing */
                if (mos2.MOS2mode >= 0)
                {
                    cdhat = mos2.MOS2cd - mos2.MOS2gbd * delvbd + mos2.MOS2gmbs * delvbs + mos2.MOS2gm * delvgs + mos2.MOS2gds * delvds;
                }
                else
                {
                    cdhat = mos2.MOS2cd + (mos2.MOS2gmbs - mos2.MOS2gbd) * delvbd - mos2.MOS2gm * delvgd + mos2.MOS2gds * delvds;
                }
                cbhat = mos2.MOS2cbs + mos2.MOS2cbd + mos2.MOS2gbd * delvbd + mos2.MOS2gbs * delvbs;

                /* now lets see if we can bypass (ugh) */
                /* the following massive if should all be one
				* single compound if statement, but most compilers
				* can't handle it in one piece, so it is broken up
				* into several stages here
				*/
                /* NOBYPASS */
                /* ok - bypass is out, do it the hard way */

                von = model.MOS2type * mos2.MOS2von;
                /* 
				* limiting
				* We want to keep device voltages from changing
				* so fast that the exponentials churn out overflows 
				* and similar rudeness
				*/
                if (state.States[0][mos2.MOS2states + MOS2.MOS2vds] >= 0)
                {
                    vgs = Transistor.DEVfetlim(vgs, state.States[0][mos2.MOS2states + MOS2.MOS2vgs], von);
                    vds = vgs - vgd;
                    vds = Transistor.DEVlimvds(vds, state.States[0][mos2.MOS2states + MOS2.MOS2vds]);
                    vgd = vgs - vds;
                }
                else
                {
                    vgd = Transistor.DEVfetlim(vgd, vgdo, von);
                    vds = vgs - vgd;
                    vds = -Transistor.DEVlimvds(-vds, -(state.States[0][mos2.MOS2states + MOS2.MOS2vds]));
                    vgs = vgd + vds;
                }
                if (vds >= 0)
                {
                    vbs = Transistor.DEVpnjlim(vbs, state.States[0][mos2.MOS2states + MOS2.MOS2vbs], vt, mos2.MOS2sourceVcrit, ref Check);
                    vbd = vbs - vds;
                }
                else
                {
                    vbd = Transistor.DEVpnjlim(vbd, state.States[0][mos2.MOS2states + MOS2.MOS2vbd], vt, mos2.MOS2drainVcrit, ref Check);
                    vbs = vbd + vds;
                }
            }
            else
            {
                /* ok - not one of the simple cases, so we have to 
				* look at other possibilities 
				*/

                if ((state.Init == CircuitState.InitFlags.InitJct) && !mos2.MOS2off)
                {
                    vds = model.MOS2type * mos2.MOS2icVDS;
                    vgs = model.MOS2type * mos2.MOS2icVGS;
                    vbs = model.MOS2type * mos2.MOS2icVBS;
                    if ((vds == 0) && (vgs == 0) && (vbs == 0) && ((method != null || state.UseDC ||
                        state.Domain == CircuitState.DomainTypes.None) || (!state.UseIC)))
                    {
                        vbs = -1;
                        vgs = model.MOS2type * mos2.MOS2tVto;
                        vds = 0;
                    }
                }
                else
                {
                    vbs = vgs = vds = 0;
                }
            }

            /* now all the preliminaries are over - we can start doing the
			* real work 
			*/

            vbd = vbs - vds;
            vgd = vgs - vds;
            vgb = vgs - vbs;

            /* bulk - source and bulk - drain doides
			* here we just evaluate the ideal diode current and the
			* correspoinding derivative (conductance).
			*/

            if (vbs <= 0)
            {
                mos2.MOS2gbs = SourceSatCur / vt;
                mos2.MOS2cbs = mos2.MOS2gbs * vbs;
                mos2.MOS2gbs += state.Gmin;
            }
            else
            {
                evbs = Math.Exp(vbs / vt);
                mos2.MOS2gbs = SourceSatCur * evbs / vt + state.Gmin;
                mos2.MOS2cbs = SourceSatCur * (evbs - 1);
            }
            if (vbd <= 0)
            {
                mos2.MOS2gbd = DrainSatCur / vt;
                mos2.MOS2cbd = mos2.MOS2gbd * vbd;
                mos2.MOS2gbd += state.Gmin;
            }
            else
            {
                evbd = Math.Exp(vbd / vt);
                mos2.MOS2gbd = DrainSatCur * evbd / vt + state.Gmin;
                mos2.MOS2cbd = DrainSatCur * (evbd - 1);
            }
            if (vds >= 0)
            {
                /* normal mode */
                mos2.MOS2mode = 1;
            }
            else
            {
                /* inverse mode */
                mos2.MOS2mode = -1;
            }
            {
                /* moseq2(vds, vbs, vgs, gm, gds, gmbs, qg, qc, qb, 
				* cggb, cgdb, cgsb, cbgb, cbdb, cbsb)
				*/
                /* note:  cgdb, cgsb, cbdb, cbsb never used */

                /* 
				* this routine evaluates the drain current, its derivatives and
				* the charges associated with the gate, channel and bulk
				* for mosfets
				* 
				*/

                double arg;
                double sarg;
                double[] a4 = new double[4], b4 = new double[4], x4 = new double[8], poly4 = new double[8];
                double beta1, dsrgdb, d2sdb2;
                double sphi = 0.0; /* square root of phi */
                double sphi3 = 0.0; /* square root of phi cubed */
                double barg, d2bdb2, factor, dbrgdb, eta, vbin, argd = 0.0, args = 0.0, argss, argsd, argxs = 0.0, argxd = 0.0, daddb2, dasdb2, dbargd, dbargs, dbxwd, dbxws,
                    dgddb2, dgddvb, dgdvds, gamasd, xwd, xws, ddxwd, gammad, vth, cfs, cdonco, xn = 0.0, argg = 0.0, vgst, sarg3, sbiarg, dgdvbs, body, gdbdv,
                    dodvbs, dodvds = 0.0, dxndvd = 0.0, dxndvb = 0.0, udenom, dudvgs, dudvds, dudvbs, gammd2, argv, vgsx, ufact, ueff, dsdvgs, dsdvbs, a1, a3, a, b1,
                    b3, b, c1, c, d1, fi, p0, p2, p3, p4, p, r3, r, ro, s2, s, v1, v2, xv, y3, delta4, xvalid = 0.0, bsarg = 0.0, dbsrdb = 0.0, bodys = 0.0, gdbdvs = 0.0, sargv,
                    xlfact, dldsat, xdv, xlv, vqchan, dqdsat, vl, dfundg, dfunds, dfundb, xls, dldvgs = 0.0, dldvds = 0.0, dldvbs = 0.0, dfact, clfact, xleff, deltal,
                    xwb, vdson, cdson, didvds, gdson, gmw, gbson, expg, xld;
                double xlamda = model.MOS2lambda;
                /* 'local' variables - these switch d & s around appropriately
				 * so that we don't have to worry about vds < 0
				 */
                double lvbs = mos2.MOS2mode == 1 ? vbs : vbd;
                double lvds = mos2.MOS2mode * vds;
                double lvgs = mos2.MOS2mode == 1 ? vgs : vgd;
                double phiMinVbs = mos2.MOS2tPhi - lvbs;
                double tmp; /* a temporary variable, not used for more than */
                            /* about 10 lines at a time */
                int iknt, jknt, i, j;

                /* 
				* compute some useful quantities
				*/

                if (lvbs <= 0.0)
                {
                    sarg = Math.Sqrt(phiMinVbs);
                    dsrgdb = -0.5 / sarg;
                    d2sdb2 = 0.5 * dsrgdb / phiMinVbs;
                }
                else
                {
                    sphi = Math.Sqrt(mos2.MOS2tPhi);
                    sphi3 = mos2.MOS2tPhi * sphi;
                    sarg = sphi / (1.0 + 0.5 * lvbs / mos2.MOS2tPhi);
                    tmp = sarg / sphi3;
                    dsrgdb = -0.5 * sarg * tmp;
                    d2sdb2 = -dsrgdb * tmp;
                }
                if ((lvds - lvbs) >= 0)
                {
                    barg = Math.Sqrt(phiMinVbs + lvds);
                    dbrgdb = -0.5 / barg;
                    d2bdb2 = 0.5 * dbrgdb / (phiMinVbs + lvds);
                }
                else
                {
                    barg = sphi / (1.0 + 0.5 * (lvbs - lvds) / mos2.MOS2tPhi);
                    tmp = barg / sphi3;
                    dbrgdb = -0.5 * barg * tmp;
                    d2bdb2 = -dbrgdb * tmp;
                }
                /* 
				* calculate threshold voltage (von)
				* narrow - channel effect
				*/

                /* XXX constant per device */
                factor = 0.125 * model.MOS2narrowFactor * 2.0 * Circuit.CONSTPI * Transistor.EPSSIL / OxideCap * EffectiveLength;
                /* XXX constant per device */
                eta = 1.0 + factor;
                vbin = mos2.MOS2tVbi * model.MOS2type + factor * phiMinVbs;
                if ((model.MOS2gamma > 0.0) || (model.MOS2substrateDoping > 0.0))
                {
                    xwd = model.MOS2xd * barg;
                    xws = model.MOS2xd * sarg;

                    /* 
					* short - channel effect with vds .ne. 0.0
					*/

                    argss = 0.0;
                    argsd = 0.0;
                    dbargs = 0.0;
                    dbargd = 0.0;
                    dgdvds = 0.0;
                    dgddb2 = 0.0;
                    if (model.MOS2junctionDepth > 0)
                    {
                        tmp = 2.0 / model.MOS2junctionDepth;
                        argxs = 1.0 + xws * tmp;
                        argxd = 1.0 + xwd * tmp;
                        args = Math.Sqrt(argxs);
                        argd = Math.Sqrt(argxd);
                        tmp = .5 * model.MOS2junctionDepth / EffectiveLength;
                        argss = tmp * (args - 1.0);
                        argsd = tmp * (argd - 1.0);
                    }
                    gamasd = model.MOS2gamma * (1.0 - argss - argsd);
                    dbxwd = model.MOS2xd * dbrgdb;
                    dbxws = model.MOS2xd * dsrgdb;
                    if (model.MOS2junctionDepth > 0)
                    {
                        tmp = 0.5 / EffectiveLength;
                        dbargs = tmp * dbxws / args;
                        dbargd = tmp * dbxwd / argd;
                        dasdb2 = -model.MOS2xd * (d2sdb2 + dsrgdb * dsrgdb * model.MOS2xd / (model.MOS2junctionDepth * argxs)) / (EffectiveLength *
                            args);
                        daddb2 = -model.MOS2xd * (d2bdb2 + dbrgdb * dbrgdb * model.MOS2xd / (model.MOS2junctionDepth * argxd)) / (EffectiveLength *
                            argd);
                        dgddb2 = -0.5 * model.MOS2gamma * (dasdb2 + daddb2);
                    }
                    dgddvb = -model.MOS2gamma * (dbargs + dbargd);
                    if (model.MOS2junctionDepth > 0)
                    {
                        ddxwd = -dbxwd;
                        dgdvds = -model.MOS2gamma * 0.5 * ddxwd / (EffectiveLength * argd);
                    }
                }
                else
                {
                    gamasd = model.MOS2gamma;
                    gammad = model.MOS2gamma;
                    dgddvb = 0.0;
                    dgdvds = 0.0;
                    dgddb2 = 0.0;
                }
                von = vbin + gamasd * sarg;
                vth = von;
                vdsat = 0.0;
                if (model.MOS2fastSurfaceStateDensity != 0.0 && OxideCap != 0.0)
                {
                    /* XXX constant per model */
                    cfs = Circuit.CHARGE * model.MOS2fastSurfaceStateDensity * 1e4 /* (cm *  * 2 / m *  * 2) */ ;
                    cdonco = -(gamasd * dsrgdb + dgddvb * sarg) + factor;
                    xn = 1.0 + cfs / OxideCap * mos2.MOS2w * EffectiveLength + cdonco;
                    tmp = vt * xn;
                    von = von + tmp;
                    argg = 1.0 / tmp;
                    vgst = lvgs - von;
                }
                else
                {
                    vgst = lvgs - von;
                    if (lvgs <= von)
                    {
                        /* 
						* cutoff region
						*/
                        mos2.MOS2gds = 0.0;
                        goto line1050;
                    }
                }

                /* 
				* compute some more useful quantities
				*/

                sarg3 = sarg * sarg * sarg;
                /* XXX constant per model */
                sbiarg = Math.Sqrt(mos2.MOS2tBulkPot);
                gammad = gamasd;
                dgdvbs = dgddvb;
                body = barg * barg * barg - sarg3;
                gdbdv = 2.0 * gammad * (barg * barg * dbrgdb - sarg * sarg * dsrgdb);
                dodvbs = -factor + dgdvbs * sarg + gammad * dsrgdb;
                if (model.MOS2fastSurfaceStateDensity.Value == 0.0)
                    goto line400;
                if (OxideCap == 0.0)
                    goto line410;
                dxndvb = 2.0 * dgdvbs * dsrgdb + gammad * d2sdb2 + dgddb2 * sarg;
                dodvbs = dodvbs + vt * dxndvb;
                dxndvd = dgdvds * dsrgdb;
                dodvds = dgdvds * sarg + vt * dxndvd;
                /* 
				* evaluate effective mobility and its derivatives
				*/
                line400:
                if (OxideCap <= 0.0) goto line410;
                udenom = vgst;
                tmp = model.MOS2critField * 100 /* cm / m */  * Transistor.EPSSIL / model.MOS2oxideCapFactor;
                if (udenom <= tmp) goto line410;
                ufact = Math.Exp(model.MOS2critFieldExp * Math.Log(tmp / udenom));
                ueff = model.MOS2surfaceMobility * 1e-4 /* (m *  * 2 / cm *  * 2) */  * ufact;
                dudvgs = -ufact * model.MOS2critFieldExp / udenom;
                dudvds = 0.0;
                dudvbs = model.MOS2critFieldExp * ufact * dodvbs / vgst;
                goto line500;
                line410:
                ufact = 1.0;
                ueff = model.MOS2surfaceMobility * 1e-4 /* (m *  * 2 / cm *  * 2) */ ;
                dudvgs = 0.0;
                dudvds = 0.0;
                dudvbs = 0.0;
                /* 
				* evaluate saturation voltage and its derivatives according to
				* grove - frohman equation
				*/
                line500:
                vgsx = lvgs;
                gammad = gamasd / eta;
                dgdvbs = dgddvb;
                if (model.MOS2fastSurfaceStateDensity != 0 && OxideCap != 0)
                {
                    vgsx = Math.Max(lvgs, von);
                }
                if (gammad > 0)
                {
                    gammd2 = gammad * gammad;
                    argv = (vgsx - vbin) / eta + phiMinVbs;
                    if (argv <= 0.0)
                    {
                        vdsat = 0.0;
                        dsdvgs = 0.0;
                        dsdvbs = 0.0;
                    }
                    else
                    {
                        arg = Math.Sqrt(1.0 + 4.0 * argv / gammd2);
                        vdsat = (vgsx - vbin) / eta + gammd2 * (1.0 - arg) / 2.0;
                        vdsat = Math.Max(vdsat, 0.0);
                        dsdvgs = (1.0 - 1.0 / arg) / eta;
                        dsdvbs = (gammad * (1.0 - arg) + 2.0 * argv / (gammad * arg)) / eta * dgdvbs + 1.0 / arg + factor * dsdvgs;
                    }
                }
                else
                {
                    vdsat = (vgsx - vbin) / eta;
                    vdsat = Math.Max(vdsat, 0.0);
                    dsdvgs = 1.0;
                    dsdvbs = 0.0;
                }
                if (model.MOS2maxDriftVel > 0)
                {
                    /* 
					* evaluate saturation voltage and its derivatives 
					* according to baum's theory of scattering velocity 
					* saturation
					*/
                    gammd2 = gammad * gammad;
                    v1 = (vgsx - vbin) / eta + phiMinVbs;
                    v2 = phiMinVbs;
                    xv = model.MOS2maxDriftVel * EffectiveLength / ueff;
                    a1 = gammad / 0.75;
                    b1 = -2.0 * (v1 + xv);
                    c1 = -2.0 * gammad * xv;
                    d1 = 2.0 * v1 * (v2 + xv) - v2 * v2 - 4.0 / 3.0 * gammad * sarg3;
                    a = -b1;
                    b = a1 * c1 - 4.0 * d1;
                    c = -d1 * (a1 * a1 - 4.0 * b1) - c1 * c1;
                    r = -a * a / 3.0 + b;
                    s = 2.0 * a * a * a / 27.0 - a * b / 3.0 + c;
                    r3 = r * r * r;
                    s2 = s * s;
                    p = s2 / 4.0 + r3 / 27.0;
                    p0 = Math.Abs(p);
                    p2 = Math.Sqrt(p0);
                    if (p < 0)
                    {
                        ro = Math.Sqrt(s2 / 4.0 + p0);
                        ro = Math.Log(ro) / 3.0;
                        ro = Math.Exp(ro);
                        fi = Math.Atan(-2.0 * p2 / s);
                        y3 = 2.0 * ro * Math.Cos(fi / 3.0) - a / 3.0;
                    }
                    else
                    {
                        p3 = (-s / 2.0 + p2);
                        p3 = Math.Exp(Math.Log(Math.Abs(p3)) / 3.0);
                        p4 = (-s / 2.0 - p2);
                        p4 = Math.Exp(Math.Log(Math.Abs(p4)) / 3.0);
                        y3 = p3 + p4 - a / 3.0;
                    }
                    iknt = 0;
                    a3 = Math.Sqrt(a1 * a1 / 4.0 - b1 + y3);
                    b3 = Math.Sqrt(y3 * y3 / 4.0 - d1);
                    for (i = 1; i <= 4; i++)
                    {
                        a4[i - 1] = a1 / 2.0 + sig1[i - 1] * a3;
                        b4[i - 1] = y3 / 2.0 + sig2[i - 1] * b3;
                        delta4 = a4[i - 1] * a4[i - 1] / 4.0 - b4[i - 1];
                        if (delta4 < 0)
                            continue;
                        iknt = iknt + 1;
                        tmp = Math.Sqrt(delta4);
                        x4[iknt - 1] = -a4[i - 1] / 2.0 + tmp;
                        iknt = iknt + 1;
                        x4[iknt - 1] = -a4[i - 1] / 2.0 - tmp;
                    }
                    jknt = 0;
                    for (j = 1; j <= iknt; j++)
                    {
                        if (x4[j - 1] <= 0) continue;
                        /* XXX implement this sanely */
                        poly4[j - 1] = x4[j - 1] * x4[j - 1] * x4[j - 1] * x4[j - 1] + a1 * x4[j - 1] * x4[j - 1] * x4[j - 1];
                        poly4[j - 1] = poly4[j - 1] + b1 * x4[j - 1] * x4[j - 1] + c1 * x4[j - 1] + d1;
                        if (Math.Abs(poly4[j - 1]) > 1.0e-6)
                            continue;
                        jknt = jknt + 1;
                        if (jknt <= 1)
                        {
                            xvalid = x4[j - 1];
                        }
                        if (x4[j - 1] > xvalid)
                            continue;
                        xvalid = x4[j - 1];
                    }
                    if (jknt > 0)
                    {
                        vdsat = xvalid * xvalid - phiMinVbs;
                    }
                }
                /* 
				* evaluate effective channel length and its derivatives
				*/
                if (lvds != 0.0)
                {
                    gammad = gamasd;
                    if ((lvbs - vdsat) <= 0)
                    {
                        bsarg = Math.Sqrt(vdsat + phiMinVbs);
                        dbsrdb = -0.5 / bsarg;
                    }
                    else
                    {
                        bsarg = sphi / (1.0 + 0.5 * (lvbs - vdsat) / mos2.MOS2tPhi);
                        dbsrdb = -0.5 * bsarg * bsarg / sphi3;
                    }
                    bodys = bsarg * bsarg * bsarg - sarg3;
                    gdbdvs = 2.0 * gammad * (bsarg * bsarg * dbsrdb - sarg * sarg * dsrgdb);
                    if (model.MOS2maxDriftVel <= 0)
                    {
                        if (model.MOS2substrateDoping.Value == 0.0)
                            goto line610;
                        if (xlamda > 0.0)
                            goto line610;
                        argv = (lvds - vdsat) / 4.0;
                        sargv = Math.Sqrt(1.0 + argv * argv);
                        arg = Math.Sqrt(argv + sargv);
                        xlfact = model.MOS2xd / (EffectiveLength * lvds);
                        xlamda = xlfact * arg;
                        dldsat = lvds * xlamda / (8.0 * sargv);
                    }
                    else
                    {
                        argv = (vgsx - vbin) / eta - vdsat;
                        xdv = model.MOS2xd / Math.Sqrt(model.MOS2channelCharge);
                        xlv = model.MOS2maxDriftVel * xdv / (2.0 * ueff);
                        vqchan = argv - gammad * bsarg;
                        dqdsat = -1.0 + gammad * dbsrdb;
                        vl = model.MOS2maxDriftVel * EffectiveLength;
                        dfunds = vl * dqdsat - ueff * vqchan;
                        dfundg = (vl - ueff * vdsat) / eta;
                        dfundb = -vl * (1.0 + dqdsat - factor / eta) + ueff * (gdbdvs - dgdvbs * bodys / 1.5) / eta;
                        dsdvgs = -dfundg / dfunds;
                        dsdvbs = -dfundb / dfunds;
                        if (model.MOS2substrateDoping.Value == 0.0)
                            goto line610;
                        if (xlamda > 0.0)
                            goto line610;
                        argv = lvds - vdsat;
                        argv = Math.Max(argv, 0.0);
                        xls = Math.Sqrt(xlv * xlv + argv);
                        dldsat = xdv / (2.0 * xls);
                        xlfact = xdv / (EffectiveLength * lvds);
                        xlamda = xlfact * (xls - xlv);
                        dldsat = dldsat / EffectiveLength;
                    }
                    dldvgs = dldsat * dsdvgs;
                    dldvds = -xlamda + dldsat;
                    dldvbs = dldsat * dsdvbs;
                }

                // Edited to work
                goto line610_finish;
                line610:
                dldvgs = 0.0;
                dldvds = 0.0;
                dldvbs = 0.0;
                line610_finish:

                /* 
				* limit channel shortening at punch - through
				*/
                xwb = model.MOS2xd * sbiarg;
                xld = EffectiveLength - xwb;
                clfact = 1.0 - xlamda * lvds;
                dldvds = -xlamda - dldvds;
                xleff = EffectiveLength * clfact;
                deltal = xlamda * lvds * EffectiveLength;
                if (model.MOS2substrateDoping.Value == 0.0)
                    xwb = 0.25e-6;
                if (xleff < xwb)
                {
                    xleff = xwb / (1.0 + (deltal - xld) / xwb);
                    clfact = xleff / EffectiveLength;
                    dfact = xleff * xleff / (xwb * xwb);
                    dldvgs = dfact * dldvgs;
                    dldvds = dfact * dldvds;
                    dldvbs = dfact * dldvbs;
                }
                /* 
				* evaluate effective beta (effective kp)
				*/
                beta1 = Beta * ufact / clfact;
                /* 
				* test for mode of operation and branch appropriately
				*/
                gammad = gamasd;
                dgdvbs = dgddvb;
                if (lvds <= 1.0e-10)
                {
                    if (lvgs <= von)
                    {
                        if ((model.MOS2fastSurfaceStateDensity.Value == 0.0) || (OxideCap == 0.0))
                        {
                            mos2.MOS2gds = 0.0;
                            goto line1050;
                        }

                        mos2.MOS2gds = beta1 * (von - vbin - gammad * sarg) * Math.Exp(argg * (lvgs - von));
                        goto line1050;
                    }

                    mos2.MOS2gds = beta1 * (lvgs - vbin - gammad * sarg);
                    goto line1050;
                }

                if (lvgs > von)
                    goto line900;
                /* 
				* subthreshold region
				*/
                if (vdsat <= 0)
                {
                    mos2.MOS2gds = 0.0;
                    if (lvgs > vth)
                        goto doneval;
                    goto line1050;
                }
                vdson = Math.Min(vdsat, lvds);
                if (lvds > vdsat)
                {
                    barg = bsarg;
                    dbrgdb = dbsrdb;
                    body = bodys;
                    gdbdv = gdbdvs;
                }
                cdson = beta1 * ((von - vbin - eta * vdson * 0.5) * vdson - gammad * body / 1.5);
                didvds = beta1 * (von - vbin - eta * vdson - gammad * barg);
                gdson = -cdson * dldvds / clfact - beta1 * dgdvds * body / 1.5;
                if (lvds < vdsat)
                    gdson = gdson + didvds;
                gbson = -cdson * dldvbs / clfact + beta1 * (dodvbs * vdson + factor * vdson - dgdvbs * body / 1.5 - gdbdv);
                if (lvds > vdsat)
                    gbson = gbson + didvds * dsdvbs;
                expg = Math.Exp(argg * (lvgs - von));
                cdrain = cdson * expg;
                gmw = cdrain * argg;
                mos2.MOS2gm = gmw;
                if (lvds > vdsat)
                    mos2.MOS2gm = gmw + didvds * dsdvgs * expg;
                tmp = gmw * (lvgs - von) / xn;
                mos2.MOS2gds = gdson * expg - mos2.MOS2gm * dodvds - tmp * dxndvd;
                mos2.MOS2gmbs = gbson * expg - mos2.MOS2gm * dodvbs - tmp * dxndvb;
                goto doneval;

                line900:
                if (lvds <= vdsat)
                {
                    /* 
					* linear region
					*/
                    cdrain = beta1 * ((lvgs - vbin - eta * lvds / 2.0) * lvds - gammad * body / 1.5);
                    arg = cdrain * (dudvgs / ufact - dldvgs / clfact);
                    mos2.MOS2gm = arg + beta1 * lvds;
                    arg = cdrain * (dudvds / ufact - dldvds / clfact);
                    mos2.MOS2gds = arg + beta1 * (lvgs - vbin - eta * lvds - gammad * barg - dgdvds * body / 1.5);
                    arg = cdrain * (dudvbs / ufact - dldvbs / clfact);
                    mos2.MOS2gmbs = arg - beta1 * (gdbdv + dgdvbs * body / 1.5 - factor * lvds);
                }
                else
                {
                    /* 
					* saturation region
					*/
                    cdrain = beta1 * ((lvgs - vbin - eta * vdsat / 2.0) * vdsat - gammad * bodys / 1.5);
                    arg = cdrain * (dudvgs / ufact - dldvgs / clfact);
                    mos2.MOS2gm = arg + beta1 * vdsat + beta1 * (lvgs - vbin - eta * vdsat - gammad * bsarg) * dsdvgs;
                    mos2.MOS2gds = -cdrain * dldvds / clfact - beta1 * dgdvds * bodys / 1.5;
                    arg = cdrain * (dudvbs / ufact - dldvbs / clfact);
                    mos2.MOS2gmbs = arg - beta1 * (gdbdvs + dgdvbs * bodys / 1.5 - factor * vdsat) + beta1 * (lvgs - vbin - eta * vdsat - gammad *
                        bsarg) * dsdvbs;
                }
                /* 
				* compute charges for "on" region
				*/
                goto doneval;
                /* 
				* finish special cases
				*/
                line1050:
                cdrain = 0.0;
                mos2.MOS2gm = 0.0;
                mos2.MOS2gmbs = 0.0;
                /* 
				* finished
				*/

            }
            doneval:
            mos2.MOS2von = model.MOS2type * von;
            mos2.MOS2vdsat = model.MOS2type * vdsat;
            /* 
			* COMPUTE EQUIVALENT DRAIN CURRENT SOURCE
			*/
            mos2.MOS2cd = mos2.MOS2mode * cdrain - mos2.MOS2cbd;

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
                    if (vbs < mos2.MOS2tDepCap)
                    {
                        double arg = 1 - vbs / mos2.MOS2tBulkPot, sarg;
                        /* 
						* the following block looks somewhat long and messy, 
						* but since most users use the default grading
						* coefficients of .5, and sqrt is MUCH faster than an
						* Math.Exp(Math.Log()) we use this special case code to buy time.
						* (as much as 10% of total job time!)
						*/
                        if (model.MOS2bulkJctBotGradingCoeff.Value == model.MOS2bulkJctSideGradingCoeff)
                        {
                            if (model.MOS2bulkJctBotGradingCoeff.Value == .5)
                            {
                                sarg = sargsw = 1 / Math.Sqrt(arg);
                            }
                            else
                            {
                                sarg = sargsw = Math.Exp(-model.MOS2bulkJctBotGradingCoeff * Math.Log(arg));
                            }
                        }
                        else
                        {
                            if (model.MOS2bulkJctBotGradingCoeff.Value == .5)
                            {
                                sarg = 1 / Math.Sqrt(arg);
                            }
                            else
                            {
                                /* NOSQRT */
                                sarg = Math.Exp(-model.MOS2bulkJctBotGradingCoeff * Math.Log(arg));
                            }
                            if (model.MOS2bulkJctSideGradingCoeff.Value == .5)
                            {
                                sargsw = 1 / Math.Sqrt(arg);
                            }
                            else
                            {
                                /* NOSQRT */
                                sargsw = Math.Exp(-model.MOS2bulkJctSideGradingCoeff * Math.Log(arg));
                            }
                        }
                        /* NOSQRT */
                        state.States[0][mos2.MOS2states + MOS2.MOS2qbs] = mos2.MOS2tBulkPot * (mos2.MOS2Cbs * (1 - arg * sarg) / (1 - model.MOS2bulkJctBotGradingCoeff) +
                            mos2.MOS2Cbssw * (1 - arg * sargsw) / (1 - model.MOS2bulkJctSideGradingCoeff));
                        mos2.MOS2capbs = mos2.MOS2Cbs * sarg + mos2.MOS2Cbssw * sargsw;
                    }
                    else
                    {
                        state.States[0][mos2.MOS2states + MOS2.MOS2qbs] = mos2.MOS2f4s + vbs * (mos2.MOS2f2s + vbs * (mos2.MOS2f3s / 2));
                        mos2.MOS2capbs = mos2.MOS2f2s + mos2.MOS2f3s * vbs;
                    }
                    /* CAPZEROBYPASS */
                }
                /* CAPBYPASS */
                /* can't bypass the diode capacitance calculations */
                {
                    /* CAPZEROBYPASS */
                    if (vbd < mos2.MOS2tDepCap)
                    {
                        double arg = 1 - vbd / mos2.MOS2tBulkPot, sarg;
                        /* 
						* the following block looks somewhat long and messy, 
						* but since most users use the default grading
						* coefficients of .5, and sqrt is MUCH faster than an
						* Math.Exp(Math.Log()) we use this special case code to buy time.
						* (as much as 10% of total job time!)
						*/
                        if (model.MOS2bulkJctBotGradingCoeff.Value == .5 && model.MOS2bulkJctSideGradingCoeff.Value == .5)
                        {
                            sarg = sargsw = 1 / Math.Sqrt(arg);
                        }
                        else
                        {
                            if (model.MOS2bulkJctBotGradingCoeff.Value == .5)
                            {
                                sarg = 1 / Math.Sqrt(arg);
                            }
                            else
                            {
                                /* NOSQRT */
                                sarg = Math.Exp(-model.MOS2bulkJctBotGradingCoeff * Math.Log(arg));
                            }
                            if (model.MOS2bulkJctSideGradingCoeff.Value == .5)
                            {
                                sargsw = 1 / Math.Sqrt(arg);
                            }
                            else
                            {
                                /* NOSQRT */
                                sargsw = Math.Exp(-model.MOS2bulkJctSideGradingCoeff * Math.Log(arg));
                            }
                        }
                        /* NOSQRT */
                        state.States[0][mos2.MOS2states + MOS2.MOS2qbd] = mos2.MOS2tBulkPot * (mos2.MOS2Cbd * (1 - arg * sarg) / (1 - model.MOS2bulkJctBotGradingCoeff) +
                            mos2.MOS2Cbdsw * (1 - arg * sargsw) / (1 - model.MOS2bulkJctSideGradingCoeff));
                        mos2.MOS2capbd = mos2.MOS2Cbd * sarg + mos2.MOS2Cbdsw * sargsw;
                    }
                    else
                    {
                        state.States[0][mos2.MOS2states + MOS2.MOS2qbd] = mos2.MOS2f4d + vbd * (mos2.MOS2f2d + vbd * mos2.MOS2f3d / 2);
                        mos2.MOS2capbd = mos2.MOS2f2d + vbd * mos2.MOS2f3d;
                    }
                    /* CAPZEROBYPASS */
                }

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
                    var result = method.Integrate(state, mos2.MOS2states + MOS2.MOS2qbd, mos2.MOS2capbd);
                    mos2.MOS2gbd += result.Geq;
                    mos2.MOS2cbd += state.States[0][mos2.MOS2states + MOS2.MOS2cqbd];
                    mos2.MOS2cd -= state.States[0][mos2.MOS2states + MOS2.MOS2cqbd];
                    result = method.Integrate(state, mos2.MOS2states + MOS2.MOS2qbs, mos2.MOS2capbs);
                    mos2.MOS2gbs += result.Geq;
                    mos2.MOS2cbs += state.States[0][mos2.MOS2states + MOS2.MOS2cqbs];
                }
            }

            /* 
			 * check convergence
			 */
            if (!mos2.MOS2off || (!(state.Init == CircuitState.InitFlags.InitFix || state.UseSmallSignal)))
            {
                if (Check == 1)
                    state.IsCon = false;
            }
            state.States[0][mos2.MOS2states + MOS2.MOS2vbs] = vbs;
            state.States[0][mos2.MOS2states + MOS2.MOS2vbd] = vbd;
            state.States[0][mos2.MOS2states + MOS2.MOS2vgs] = vgs;
            state.States[0][mos2.MOS2states + MOS2.MOS2vds] = vds;

            /* 
			* meyer's capacitor model
			*/
            if (state.Domain == CircuitState.DomainTypes.Time || state.UseSmallSignal)
            {
                /* 
				 * calculate meyer's capacitors
				 */
                double icapgs, icapgd, icapgb;
                if (mos2.MOS2mode > 0)
                {
                    Transistor.DEVqmeyer(vgs, vgd, vgb, von, vdsat,
                        out icapgs, out icapgd, out icapgb, mos2.MOS2tPhi, OxideCap);
                }
                else
                {
                    Transistor.DEVqmeyer(vgd, vgs, vgb, von, vdsat,
                        out icapgd, out icapgs, out icapgb, mos2.MOS2tPhi, OxideCap);
                }
                state.States[0][mos2.MOS2states + MOS2.MOS2capgs] = icapgs;
                state.States[0][mos2.MOS2states + MOS2.MOS2capgd] = icapgd;
                state.States[0][mos2.MOS2states + MOS2.MOS2capgb] = icapgb;

                vgs1 = state.States[1][mos2.MOS2states + MOS2.MOS2vgs];
                vgd1 = vgs1 - state.States[1][mos2.MOS2states + MOS2.MOS2vds];
                vgb1 = vgs1 - state.States[1][mos2.MOS2states + MOS2.MOS2vbs];
                if (state.Domain == CircuitState.DomainTypes.Time && state.UseDC)
                {
                    capgs = 2 * state.States[0][mos2.MOS2states + MOS2.MOS2capgs] + GateSourceOverlapCap;
                    capgd = 2 * state.States[0][mos2.MOS2states + MOS2.MOS2capgd] + GateDrainOverlapCap;
                    capgb = 2 * state.States[0][mos2.MOS2states + MOS2.MOS2capgb] + GateBulkOverlapCap;
                }
                else
                {
                    capgs = state.States[0][mos2.MOS2states + MOS2.MOS2capgs] + state.States[1][mos2.MOS2states + MOS2.MOS2capgs] + GateSourceOverlapCap;
                    capgd = state.States[0][mos2.MOS2states + MOS2.MOS2capgd] + state.States[1][mos2.MOS2states + MOS2.MOS2capgd] + GateDrainOverlapCap;
                    capgb = state.States[0][mos2.MOS2states + MOS2.MOS2capgb] + state.States[1][mos2.MOS2states + MOS2.MOS2capgb] + GateBulkOverlapCap;
                }

                /* 
				* store small - signal parameters (for meyer's model)
				* all parameters already stored, so done...
				*/

                /* PREDICTOR */
                if (method != null)
                {
                    state.States[0][mos2.MOS2states + MOS2.MOS2qgs] = (vgs - vgs1) * capgs + state.States[1][mos2.MOS2states + MOS2.MOS2qgs];
                    state.States[0][mos2.MOS2states + MOS2.MOS2qgd] = (vgd - vgd1) * capgd + state.States[1][mos2.MOS2states + MOS2.MOS2qgd];
                    state.States[0][mos2.MOS2states + MOS2.MOS2qgb] = (vgb - vgb1) * capgb + state.States[1][mos2.MOS2states + MOS2.MOS2qgb];
                }
                else
                {
                    /* TRANOP */
                    state.States[0][mos2.MOS2states + MOS2.MOS2qgs] = capgs * vgs;
                    state.States[0][mos2.MOS2states + MOS2.MOS2qgd] = capgd * vgd;
                    state.States[0][mos2.MOS2states + MOS2.MOS2qgb] = capgb * vgb;
                }
                /* PREDICTOR */
            }
            /* NOBYPASS */
            if ((state.Init == CircuitState.InitFlags.InitTransient) || method == null)
            {
                /* initialize to zero charge conductances and current */

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
                    state.States[0][mos2.MOS2states + MOS2.MOS2cqgs] = 0;
                if (capgd == 0)
                    state.States[0][mos2.MOS2states + MOS2.MOS2cqgd] = 0;
                if (capgb == 0)
                    state.States[0][mos2.MOS2states + MOS2.MOS2cqgb] = 0;
                /* 
				 * calculate equivalent conductances and currents for
				 * meyer"s capacitors
				 */
                method.Integrate(state, out gcgs, out ceqgs, mos2.MOS2states + MOS2.MOS2qgs, capgs);
                method.Integrate(state, out gcgd, out ceqgd, mos2.MOS2states + MOS2.MOS2qgd, capgd);
                method.Integrate(state, out gcgb, out ceqgb, mos2.MOS2states + MOS2.MOS2qgb, capgb);
                ceqgs = ceqgs - gcgs * vgs + method.Slope * state.States[0][mos2.MOS2states + MOS2.MOS2qgs];
                ceqgd = ceqgd - gcgd * vgd + method.Slope * state.States[0][mos2.MOS2states + MOS2.MOS2qgd];
                ceqgb = ceqgb - gcgb * vgb + method.Slope * state.States[0][mos2.MOS2states + MOS2.MOS2qgb];
            }
            /* 
			* store charge storage info for meyer's cap in lx table
			*/

            /* 
			* load current vector
			*/
            ceqbs = model.MOS2type * (mos2.MOS2cbs - (mos2.MOS2gbs - state.Gmin) * vbs);
            ceqbd = model.MOS2type * (mos2.MOS2cbd - (mos2.MOS2gbd - state.Gmin) * vbd);
            if (mos2.MOS2mode >= 0)
            {
                xnrm = 1;
                xrev = 0;
                cdreq = model.MOS2type * (cdrain - mos2.MOS2gds * vds - mos2.MOS2gm * vgs - mos2.MOS2gmbs * vbs);
            }
            else
            {
                xnrm = 0;
                xrev = 1;
                cdreq = -(model.MOS2type) * (cdrain - mos2.MOS2gds * (-vds) - mos2.MOS2gm * vgd - mos2.MOS2gmbs * vbd);
            }
            rstate.Rhs[mos2.MOS2gNode] -= (model.MOS2type * (ceqgs + ceqgb + ceqgd));
            rstate.Rhs[mos2.MOS2bNode] -= (ceqbs + ceqbd - model.MOS2type * ceqgb);
            rstate.Rhs[mos2.MOS2dNodePrime] += (ceqbd - cdreq + model.MOS2type * ceqgd);
            rstate.Rhs[mos2.MOS2sNodePrime] += cdreq + ceqbs + model.MOS2type * ceqgs;

            /* 
			 * load y matrix
			 */
            mos2.MOS2DdPtr.Add(mos2.MOS2drainConductance);
            mos2.MOS2GgPtr.Add(gcgd + gcgs + gcgb);
            mos2.MOS2SsPtr.Add(mos2.MOS2sourceConductance);
            mos2.MOS2BbPtr.Add(mos2.MOS2gbd + mos2.MOS2gbs + gcgb);
            mos2.MOS2DPdpPtr.Add(mos2.MOS2drainConductance + mos2.MOS2gds + mos2.MOS2gbd + xrev * (mos2.MOS2gm + mos2.MOS2gmbs) + gcgd);
            mos2.MOS2SPspPtr.Add(mos2.MOS2sourceConductance + mos2.MOS2gds + mos2.MOS2gbs + xnrm * (mos2.MOS2gm + mos2.MOS2gmbs) + gcgs);
            mos2.MOS2DdpPtr.Add(-mos2.MOS2drainConductance);
            mos2.MOS2GbPtr.Sub(gcgb);
            mos2.MOS2GdpPtr.Sub(gcgd);
            mos2.MOS2GspPtr.Sub(gcgs);
            mos2.MOS2SspPtr.Add(-mos2.MOS2sourceConductance);
            mos2.MOS2BgPtr.Sub(gcgb);
            mos2.MOS2BdpPtr.Sub(mos2.MOS2gbd);
            mos2.MOS2BspPtr.Sub(mos2.MOS2gbs);
            mos2.MOS2DPdPtr.Add(-mos2.MOS2drainConductance);
            mos2.MOS2DPgPtr.Add((xnrm - xrev) * mos2.MOS2gm - gcgd);
            mos2.MOS2DPbPtr.Add(-mos2.MOS2gbd + (xnrm - xrev) * mos2.MOS2gmbs);
            mos2.MOS2DPspPtr.Add(-mos2.MOS2gds - xnrm * (mos2.MOS2gm + mos2.MOS2gmbs));
            mos2.MOS2SPgPtr.Add(-(xnrm - xrev) * mos2.MOS2gm - gcgs);
            mos2.MOS2SPsPtr.Add(-mos2.MOS2sourceConductance);
            mos2.MOS2SPbPtr.Add(-mos2.MOS2gbs - (xnrm - xrev) * mos2.MOS2gmbs);
            mos2.MOS2SPdpPtr.Add(-mos2.MOS2gds - xrev * (mos2.MOS2gm + mos2.MOS2gmbs));
        }

        /// <summary>
        /// Check convergence
        /// </summary>
        /// <param name="ckt">Circuit</param>
        /// <returns></returns>
        public override bool IsConvergent(Circuit ckt)
        {
            var mos2 = ComponentTyped<MOS2>();
            var model = mos2.Model as MOS2Model;
            var config = ckt.Simulation.CurrentConfig;
            var state = ckt.State;

            double vbs, vgs, vds, vbd, vgd, vgdo, delvbs, delvbd, delvgs, delvds, delvgd, cdhat, cbhat;

            vbs = model.MOS2type * (state.Solution[mos2.MOS2bNode] - state.Solution[mos2.MOS2sNodePrime]);
            vgs = model.MOS2type * (state.Solution[mos2.MOS2gNode] - state.Solution[mos2.MOS2sNodePrime]);
            vds = model.MOS2type * (state.Solution[mos2.MOS2dNodePrime] - state.Solution[mos2.MOS2sNodePrime]);
            vbd = vbs - vds;
            vgd = vgs - vds;
            vgdo = state.States[0][mos2.MOS2states + MOS2.MOS2vgs] - state.States[0][mos2.MOS2states + MOS2.MOS2vds];
            delvbs = vbs - state.States[0][mos2.MOS2states + MOS2.MOS2vbs];
            delvbd = vbd - state.States[0][mos2.MOS2states + MOS2.MOS2vbd];
            delvgs = vgs - state.States[0][mos2.MOS2states + MOS2.MOS2vgs];
            delvds = vds - state.States[0][mos2.MOS2states + MOS2.MOS2vds];
            delvgd = vgd - vgdo;

            /* these are needed for convergence testing */

            if (mos2.MOS2mode >= 0)
                cdhat = mos2.MOS2cd - mos2.MOS2gbd * delvbd + mos2.MOS2gmbs * delvbs + mos2.MOS2gm * delvgs + mos2.MOS2gds * delvds;
            else
                cdhat = mos2.MOS2cd - (mos2.MOS2gbd - mos2.MOS2gmbs) * delvbd - mos2.MOS2gm * delvgd + mos2.MOS2gds * delvds;
            cbhat = mos2.MOS2cbs + mos2.MOS2cbd + mos2.MOS2gbd * delvbd + mos2.MOS2gbs * delvbs;

            /*
             *  check convergence
             */
            double tol = config.RelTol * Math.Max(Math.Abs(cdhat), Math.Abs(mos2.MOS2cd)) + config.AbsTol;
            if (Math.Abs(cdhat - mos2.MOS2cd) >= tol)
            {
                state.IsCon = false;
                return false;
            }
            else
            {
                tol = config.RelTol * Math.Max(Math.Abs(cbhat), Math.Abs(mos2.MOS2cbs + mos2.MOS2cbd)) + config.AbsTol;
                if (Math.Abs(cbhat - (mos2.MOS2cbs + mos2.MOS2cbd)) > tol)
                {
                    state.IsCon = false;
                    return false;
                }
            }
            return true;
        }
    }
}
