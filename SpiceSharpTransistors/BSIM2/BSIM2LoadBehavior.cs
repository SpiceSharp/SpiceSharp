using System;
using SpiceSharp.Behaviors;
using SpiceSharp.Circuits;
using SpiceSharp.Components.Transistors;
using static SpiceSharp.Components.Transistors.BSIM2Helpers;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// General behaviour for a <see cref="BSIM2"/>
    /// </summary>
    public class BSIM2LoadBehavior : CircuitObjectBehaviorLoad
    {
        /// <summary>
        /// Setup the behaviour
        /// </summary>
        /// <param name="component"></param>
        /// <param name="ckt"></param>
        public override void Setup(CircuitObject component, Circuit ckt)
        {
            base.Setup(component, ckt);
            var bsim2 = ComponentTyped<BSIM2>();
            bsim2.B2vdsat = 0;
            bsim2.B2von = 0;
        }

        /// <summary>
        /// Execute the behaviour
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Load(Circuit ckt)
        {
            var bsim2 = ComponentTyped<BSIM2>();
            var model = bsim2.Model as BSIM2Model;
            var state = ckt.State;
            var rstate = state;
            var method = ckt.Method;
            double EffectiveLength, DrainArea, SourceArea, DrainPerimeter, SourcePerimeter, DrainSatCurrent, SourceSatCurrent, GateSourceOverlapCap, GateDrainOverlapCap,
                        GateBulkOverlapCap, von, vdsat, vt0;
            int Check;
            double vbs, vgs, vds, vbd, vgd, vgdo, delvbs, delvbd, delvgs, delvds, delvgd, cdhat, cbhat, vcrit, vgb, gbs, cbs, evbs, gbd, cbd,
                        evbd, cd, czbd, czbs, czbdsw, czbssw, PhiB, PhiBSW, MJ, MJSW, arg, argsw, sarg, sargsw, capbs = 0.0, capbd = 0.0;
            double ceqqg, gcdgb, gcsgb, gcggb, gcbgb, cqgate, cqbulk, cqdrn, ceqqb, ceqqd, ceqbs, ceqbd, xnrm, xrev, cdreq;
            double gm, gds, gmbs, qgate, qbulk, qdrn = 0.0, cggb, cgdb, cgsb, cbgb, cbdb, cbsb, cdgb = 0.0,
                cddb = 0.0, cdsb = 0.0, cdrain, qsrc = 0.0, csgb = 0.0, cssb = 0.0, csdb = 0.0;
            double gcgdb, gcgsb, gcbdb, gcbsb, gcddb, gcdsb, gcsdb, gcssb;

            EffectiveLength = bsim2.B2l - model.B2deltaL * 1.0e-6; /* m */
            DrainArea = bsim2.B2drainArea;
            SourceArea = bsim2.B2sourceArea;
            DrainPerimeter = bsim2.B2drainPerimeter;
            SourcePerimeter = bsim2.B2sourcePerimeter;
            if ((DrainSatCurrent = DrainArea * model.B2jctSatCurDensity) < 1e-15)
            {
                DrainSatCurrent = 1.0e-15;
            }
            if ((SourceSatCurrent = SourceArea * model.B2jctSatCurDensity) < 1.0e-15)
            {
                SourceSatCurrent = 1.0e-15;
            }
            GateSourceOverlapCap = model.B2gateSourceOverlapCap * bsim2.B2w;
            GateDrainOverlapCap = model.B2gateDrainOverlapCap * bsim2.B2w;
            GateBulkOverlapCap = model.B2gateBulkOverlapCap * EffectiveLength;
            von = model.B2type * bsim2.B2von;
            vdsat = model.B2type * bsim2.B2vdsat;
            vt0 = model.B2type * bsim2.pParam.B2vt0;

            Check = 1;
            if (state.UseSmallSignal)
            {
                vbs = state.States[0][bsim2.B2states + BSIM2.B2vbs];
                vgs = state.States[0][bsim2.B2states + BSIM2.B2vgs];
                vds = state.States[0][bsim2.B2states + BSIM2.B2vds];
            }
            else if (state.Init == CircuitState.InitFlags.InitTransient)
            {
                vbs = state.States[1][bsim2.B2states + BSIM2.B2vbs];
                vgs = state.States[1][bsim2.B2states + BSIM2.B2vgs];
                vds = state.States[1][bsim2.B2states + BSIM2.B2vds];
            }
            else if (state.Init == CircuitState.InitFlags.InitJct && !bsim2.B2off)
            {
                vds = model.B2type * bsim2.B2icVDS;
                vgs = model.B2type * bsim2.B2icVGS;
                vbs = model.B2type * bsim2.B2icVBS;
                if ((vds == 0) && (vgs == 0) && (vbs == 0) &&
                            (method != null || state.UseDC || state.Domain == CircuitState.DomainTypes.None || !state.UseIC))
                {
                    vbs = -1;
                    vgs = vt0;
                    vds = 0;
                }
            }
            else if ((state.Init == CircuitState.InitFlags.InitJct || state.Init == CircuitState.InitFlags.InitFix) && bsim2.B2off)
            {
                vbs = vgs = vds = 0;
            }
            else
            {
                /* PREDICTOR */
                vbs = model.B2type * (rstate.OldSolution[bsim2.B2bNode] - rstate.OldSolution[bsim2.B2sNodePrime]);
                vgs = model.B2type * (rstate.OldSolution[bsim2.B2gNode] - rstate.OldSolution[bsim2.B2sNodePrime]);
                vds = model.B2type * (rstate.OldSolution[bsim2.B2dNodePrime] - rstate.OldSolution[bsim2.B2sNodePrime]);
                /* PREDICTOR */
                vbd = vbs - vds;
                vgd = vgs - vds;
                vgdo = state.States[0][bsim2.B2states + BSIM2.B2vgs] - state.States[0][bsim2.B2states + BSIM2.B2vds];
                delvbs = vbs - state.States[0][bsim2.B2states + BSIM2.B2vbs];
                delvbd = vbd - state.States[0][bsim2.B2states + BSIM2.B2vbd];
                delvgs = vgs - state.States[0][bsim2.B2states + BSIM2.B2vgs];
                delvds = vds - state.States[0][bsim2.B2states + BSIM2.B2vds];
                delvgd = vgd - vgdo;

                if (bsim2.B2mode >= 0)
                {
                    cdhat = state.States[0][bsim2.B2states + BSIM2.B2cd] - state.States[0][bsim2.B2states + BSIM2.B2gbd] * delvbd + state.States[0][bsim2.B2states + BSIM2.B2gmbs] *
                         delvbs + state.States[0][bsim2.B2states + BSIM2.B2gm] * delvgs + state.States[0][bsim2.B2states + BSIM2.B2gds] * delvds;
                }
                else
                {
                    cdhat = state.States[0][bsim2.B2states + BSIM2.B2cd] - (state.States[0][bsim2.B2states + BSIM2.B2gbd] - state.States[0][bsim2.B2states + BSIM2.B2gmbs]) * delvbd -
                         state.States[0][bsim2.B2states + BSIM2.B2gm] * delvgd + state.States[0][bsim2.B2states + BSIM2.B2gds] * delvds;
                }
                cbhat = state.States[0][bsim2.B2states + BSIM2.B2cbs] + state.States[0][bsim2.B2states + BSIM2.B2cbd] + state.States[0][bsim2.B2states + BSIM2.B2gbd] * delvbd +
                     state.States[0][bsim2.B2states + BSIM2.B2gbs] * delvbs;

                /* NOBYPASS */

                von = model.B2type * bsim2.B2von;
                if (state.States[0][bsim2.B2states + BSIM2.B2vds] >= 0)
                {
                    vgs = Transistor.DEVfetlim(vgs, state.States[0][bsim2.B2states + BSIM2.B2vgs], von);
                    vds = vgs - vgd;
                    vds = Transistor.DEVlimvds(vds, state.States[0][bsim2.B2states + BSIM2.B2vds]);
                    vgd = vgs - vds;
                }
                else
                {
                    vgd = Transistor.DEVfetlim(vgd, vgdo, von);
                    vds = vgs - vgd;
                    vds = -Transistor.DEVlimvds(-vds, -(state.States[0][bsim2.B2states + BSIM2.B2vds]));
                    vgs = vgd + vds;
                }
                if (vds >= 0)
                {
                    vcrit = Circuit.CONSTvt0 * Math.Log(Circuit.CONSTvt0 / (Circuit.CONSTroot2 * SourceSatCurrent));
                    vbs = Transistor.DEVpnjlim(vbs, state.States[0][bsim2.B2states + BSIM2.B2vbs], Circuit.CONSTvt0, vcrit, ref Check); /* bsim2.B2 test */
                    vbd = vbs - vds;
                }
                else
                {
                    vcrit = Circuit.CONSTvt0 * Math.Log(Circuit.CONSTvt0 / (Circuit.CONSTroot2 * DrainSatCurrent));
                    vbd = Transistor.DEVpnjlim(vbd, state.States[0][bsim2.B2states + BSIM2.B2vbd], Circuit.CONSTvt0, vcrit, ref Check); /* bsim2.B2 test */
                    vbs = vbd + vds;
                }
            }

            /* determine DC current and derivatives */
            vbd = vbs - vds;
            vgd = vgs - vds;
            vgb = vgs - vbs;

            if (vbs <= 0.0)
            {
                gbs = SourceSatCurrent / Circuit.CONSTvt0 + state.Gmin;
                cbs = gbs * vbs;
            }
            else
            {
                evbs = Math.Exp(vbs / Circuit.CONSTvt0);
                gbs = SourceSatCurrent * evbs / Circuit.CONSTvt0 + state.Gmin;
                cbs = SourceSatCurrent * (evbs - 1) + state.Gmin * vbs;
            }
            if (vbd <= 0.0)
            {
                gbd = DrainSatCurrent / Circuit.CONSTvt0 + state.Gmin;
                cbd = gbd * vbd;
            }
            else
            {
                evbd = Math.Exp(vbd / Circuit.CONSTvt0);
                gbd = DrainSatCurrent * evbd / Circuit.CONSTvt0 + state.Gmin;
                cbd = DrainSatCurrent * (evbd - 1) + state.Gmin * vbd;
            }
            /* line 400 */
            if (vds >= 0)
            {
                /* normal mode */
                bsim2.B2mode = 1;
            }
            else
            {
                /* inverse mode */
                bsim2.B2mode = -1;
            }
            /* call bsim2.B2evaluate to calculate drain current and its 
            * derivatives and charge and capacitances related to gate
            * drain, and bulk
            */
            if (vds >= 0)
            {
                bsim2.B2evaluate(vds, vbs, vgs, out gm, out gds, out gmbs, out qgate,
                out qbulk, out qdrn, out cggb, out cgdb, out cgsb, out cbgb, out cbdb, out cbsb, out cdgb,
                out cddb, out cdsb, out cdrain, out von, out vdsat, ckt);
            }
            else
            {
                bsim2.B2evaluate(-vds, vbd, vgd, out gm, out gds, out gmbs, out qgate,
                out qbulk, out qsrc, out cggb, out cgsb, out cgdb, out cbgb, out cbsb, out cbdb, out csgb,
                out cssb, out csdb, out cdrain, out von, out vdsat, ckt);
            }

            bsim2.B2von = model.B2type * von;
            bsim2.B2vdsat = model.B2type * vdsat;

            /* 
            * COMPUTE EQUIVALENT DRAIN CURRENT SOURCE
            */
            cd = bsim2.B2mode * cdrain - cbd;
            if (method != null || state.UseSmallSignal || (state.Domain == CircuitState.DomainTypes.Time && state.UseDC && state.UseIC))
            {
                /* 
                * charge storage elements
                * 
                * bulk - drain and bulk - source depletion capacitances
                * czbd : zero bias drain junction capacitance
                * czbs : zero bias source junction capacitance
                * czbdsw:zero bias drain junction sidewall capacitance
                * czbssw:zero bias source junction sidewall capacitance
                */

                czbd = model.B2unitAreaJctCap * DrainArea;
                czbs = model.B2unitAreaJctCap * SourceArea;
                czbdsw = model.B2unitLengthSidewallJctCap * DrainPerimeter;
                czbssw = model.B2unitLengthSidewallJctCap * SourcePerimeter;
                PhiB = model.B2bulkJctPotential;
                PhiBSW = model.B2sidewallJctPotential;
                MJ = model.B2bulkJctBotGradingCoeff;
                MJSW = model.B2bulkJctSideGradingCoeff;

                /* Source Bulk Junction */
                if (vbs < 0)
                {
                    arg = 1 - vbs / PhiB;
                    argsw = 1 - vbs / PhiBSW;
                    sarg = Math.Exp(-MJ * Math.Log(arg));
                    sargsw = Math.Exp(-MJSW * Math.Log(argsw));
                    state.States[0][bsim2.B2states + BSIM2.B2qbs] = PhiB * czbs * (1 - arg * sarg) / (1 - MJ) + PhiBSW * czbssw * (1 - argsw * sargsw) / (1 -
                         MJSW);
                    capbs = czbs * sarg + czbssw * sargsw;
                }
                else
                {
                    state.States[0][bsim2.B2states + BSIM2.B2qbs] = vbs * (czbs + czbssw) + vbs * vbs * (czbs * MJ * 0.5 / PhiB + czbssw * MJSW * 0.5 / PhiBSW);
                    capbs = czbs + czbssw + vbs * (czbs * MJ / PhiB + czbssw * MJSW / PhiBSW);
                }

                /* Drain Bulk Junction */
                if (vbd < 0)
                {
                    arg = 1 - vbd / PhiB;
                    argsw = 1 - vbd / PhiBSW;
                    sarg = Math.Exp(-MJ * Math.Log(arg));
                    sargsw = Math.Exp(-MJSW * Math.Log(argsw));
                    state.States[0][bsim2.B2states + BSIM2.B2qbd] = PhiB * czbd * (1 - arg * sarg) / (1 - MJ) + PhiBSW * czbdsw * (1 - argsw * sargsw) / (1 -
                         MJSW);
                    capbd = czbd * sarg + czbdsw * sargsw;
                }
                else
                {
                    state.States[0][bsim2.B2states + BSIM2.B2qbd] = vbd * (czbd + czbdsw) + vbd * vbd * (czbd * MJ * 0.5 / PhiB + czbdsw * MJSW * 0.5 / PhiBSW);
                    capbd = czbd + czbdsw + vbd * (czbd * MJ / PhiB + czbdsw * MJSW / PhiBSW);
                }

            }

            /* 
            * check convergence
            */
            if (!bsim2.B2off || state.Init != CircuitState.InitFlags.InitFix)
            {
                if (Check == 1)
                    state.IsCon = false;
            }
            state.States[0][bsim2.B2states + BSIM2.B2vbs] = vbs;
            state.States[0][bsim2.B2states + BSIM2.B2vbd] = vbd;
            state.States[0][bsim2.B2states + BSIM2.B2vgs] = vgs;
            state.States[0][bsim2.B2states + BSIM2.B2vds] = vds;
            state.States[0][bsim2.B2states + BSIM2.B2cd] = cd;
            state.States[0][bsim2.B2states + BSIM2.B2cbs] = cbs;
            state.States[0][bsim2.B2states + BSIM2.B2cbd] = cbd;
            state.States[0][bsim2.B2states + BSIM2.B2gm] = gm;
            state.States[0][bsim2.B2states + BSIM2.B2gds] = gds;
            state.States[0][bsim2.B2states + BSIM2.B2gmbs] = gmbs;
            state.States[0][bsim2.B2states + BSIM2.B2gbd] = gbd;
            state.States[0][bsim2.B2states + BSIM2.B2gbs] = gbs;

            state.States[0][bsim2.B2states + BSIM2.B2cggb] = cggb;
            state.States[0][bsim2.B2states + BSIM2.B2cgdb] = cgdb;
            state.States[0][bsim2.B2states + BSIM2.B2cgsb] = cgsb;

            state.States[0][bsim2.B2states + BSIM2.B2cbgb] = cbgb;
            state.States[0][bsim2.B2states + BSIM2.B2cbdb] = cbdb;
            state.States[0][bsim2.B2states + BSIM2.B2cbsb] = cbsb;

            state.States[0][bsim2.B2states + BSIM2.B2cdgb] = cdgb;
            state.States[0][bsim2.B2states + BSIM2.B2cddb] = cddb;
            state.States[0][bsim2.B2states + BSIM2.B2cdsb] = cdsb;

            state.States[0][bsim2.B2states + BSIM2.B2capbs] = capbs;
            state.States[0][bsim2.B2states + BSIM2.B2capbd] = capbd;

            /* bulk and channel charge plus overlaps */

            if (method == null && ((!(state.Domain == CircuitState.DomainTypes.Time && state.UseDC)) || !state.UseIC) && !state.UseSmallSignal)
                goto line850;

            if (bsim2.B2mode > 0)
            {
                double[] args = new double[8];
                args[0] = GateDrainOverlapCap;
                args[1] = GateSourceOverlapCap;
                args[2] = GateBulkOverlapCap;
                args[3] = capbd;
                args[4] = capbs;
                args[5] = cggb;
                args[6] = cgdb;
                args[7] = cgsb;

                B2mosCap(ckt, vgd, vgs, vgb, args, cbgb, cbdb, cbsb, cdgb, cddb, cdsb,
                    out gcggb, out gcgdb, out gcgsb, out gcbgb, out gcbdb, out gcbsb, out gcdgb,
                    out gcddb, out gcdsb, out gcsgb, out gcsdb, out gcssb, ref qgate, ref qbulk,
                    ref qdrn, out qsrc);
            }
            else
            {
                double[] args = new double[8];
                args[0] = GateSourceOverlapCap;
                args[1] = GateDrainOverlapCap;
                args[2] = GateBulkOverlapCap;
                args[3] = capbs;
                args[4] = capbd;
                args[5] = cggb;
                args[6] = cgsb;
                args[7] = cgdb;

                B2mosCap(ckt, vgs, vgd, vgb, args, cbgb, cbsb, cbdb, csgb, cssb, csdb,
                    out gcggb, out gcgsb, out gcgdb, out gcbgb, out gcbsb, out gcbdb, out gcsgb,
                    out gcssb, out gcsdb, out gcdgb, out gcdsb, out gcddb, ref qgate, ref qbulk,
                    ref qsrc, out qdrn);
            }

            state.States[0][bsim2.B2states + BSIM2.B2qg] = qgate;
            state.States[0][bsim2.B2states + BSIM2.B2qd] = qdrn - state.States[0][bsim2.B2states + BSIM2.B2qbd];
            state.States[0][bsim2.B2states + BSIM2.B2qb] = qbulk + state.States[0][bsim2.B2states + BSIM2.B2qbd] + state.States[0][bsim2.B2states + BSIM2.B2qbs];

            /* store small signal parameters */
            if (method == null && (state.Domain == CircuitState.DomainTypes.Time && state.UseDC) && state.UseIC)
                goto line850;

            if (state.UseSmallSignal)
            {
                state.States[0][bsim2.B2states + BSIM2.B2cggb] = cggb;
                state.States[0][bsim2.B2states + BSIM2.B2cgdb] = cgdb;
                state.States[0][bsim2.B2states + BSIM2.B2cgsb] = cgsb;
                state.States[0][bsim2.B2states + BSIM2.B2cbgb] = cbgb;
                state.States[0][bsim2.B2states + BSIM2.B2cbdb] = cbdb;
                state.States[0][bsim2.B2states + BSIM2.B2cbsb] = cbsb;
                state.States[0][bsim2.B2states + BSIM2.B2cdgb] = cdgb;
                state.States[0][bsim2.B2states + BSIM2.B2cddb] = cddb;
                state.States[0][bsim2.B2states + BSIM2.B2cdsb] = cdsb;
                state.States[0][bsim2.B2states + BSIM2.B2capbd] = capbd;
                state.States[0][bsim2.B2states + BSIM2.B2capbs] = capbs;

                goto line1000;
            }

            if (state.Init == CircuitState.InitFlags.InitTransient)
            {
                state.States[1][bsim2.B2states + BSIM2.B2qb] = state.States[0][bsim2.B2states + BSIM2.B2qb];
                state.States[1][bsim2.B2states + BSIM2.B2qg] = state.States[0][bsim2.B2states + BSIM2.B2qg];
                state.States[1][bsim2.B2states + BSIM2.B2qd] = state.States[0][bsim2.B2states + BSIM2.B2qd];
            }

            if (method != null)
            {
                method.Integrate(state, bsim2.B2states + BSIM2.B2qb, 0.0);
                method.Integrate(state, bsim2.B2states + BSIM2.B2qg, 0.0);
                method.Integrate(state, bsim2.B2states + BSIM2.B2qd, 0.0);
            }

            goto line860;

            line850:
            /* initialize to zero charge conductance and current */
            ceqqg = ceqqb = ceqqd = 0.0;
            gcdgb = gcddb = gcdsb = 0.0;
            gcsgb = gcsdb = gcssb = 0.0;
            gcggb = gcgdb = gcgsb = 0.0;
            gcbgb = gcbdb = gcbsb = 0.0;
            goto line900;

            line860:
            /* evaluate equivalent charge current */
            cqgate = state.States[0][bsim2.B2states + BSIM2.B2iqg];
            cqbulk = state.States[0][bsim2.B2states + BSIM2.B2iqb];
            cqdrn = state.States[0][bsim2.B2states + BSIM2.B2iqd];
            ceqqg = cqgate - gcggb * vgb + gcgdb * vbd + gcgsb * vbs;
            ceqqb = cqbulk - gcbgb * vgb + gcbdb * vbd + gcbsb * vbs;
            ceqqd = cqdrn - gcdgb * vgb + gcddb * vbd + gcdsb * vbs;

            if (state.Init == CircuitState.InitFlags.InitTransient)
            {
                state.States[1][bsim2.B2states + BSIM2.B2iqb] = state.States[0][bsim2.B2states + BSIM2.B2iqb];
                state.States[1][bsim2.B2states + BSIM2.B2iqg] = state.States[0][bsim2.B2states + BSIM2.B2iqg];
                state.States[1][bsim2.B2states + BSIM2.B2iqd] = state.States[0][bsim2.B2states + BSIM2.B2iqd];
            }

            /* 
            * load current vector
            */
            line900:

            ceqbs = model.B2type * (cbs - (gbs - state.Gmin) * vbs);
            ceqbd = model.B2type * (cbd - (gbd - state.Gmin) * vbd);

            ceqqg = model.B2type * ceqqg;
            ceqqb = model.B2type * ceqqb;
            ceqqd = model.B2type * ceqqd;
            if (bsim2.B2mode >= 0)
            {
                xnrm = 1;
                xrev = 0;
                cdreq = model.B2type * (cdrain - gds * vds - gm * vgs - gmbs * vbs);
            }
            else
            {
                xnrm = 0;
                xrev = 1;
                cdreq = -(model.B2type) * (cdrain + gds * vds - gm * vgd - gmbs * vbd);
            }

            // rstate.Rhs[bsim2.B2gNode] -= ceqqg;
            // rstate.Rhs[bsim2.B2bNode] -= (ceqbs + ceqbd + ceqqb);
            // rstate.Rhs[bsim2.B2dNodePrime] += (ceqbd - cdreq - ceqqd);
            // rstate.Rhs[bsim2.B2sNodePrime] += (cdreq + ceqbs + ceqqg + ceqqb + ceqqd);

            /* 
            * load y matrix
            */

            // rstate.Matrix[bsim2.B2dNode, bsim2.B2dNode] += (bsim2.B2drainConductance);
            // rstate.Matrix[bsim2.B2gNode, bsim2.B2gNode] += (gcggb);
            // rstate.Matrix[bsim2.B2sNode, bsim2.B2sNode] += (bsim2.B2sourceConductance);
            // rstate.Matrix[bsim2.B2bNode, bsim2.B2bNode] += (gbd + gbs - gcbgb - gcbdb - gcbsb);
            // rstate.Matrix[bsim2.B2dNodePrime, bsim2.B2dNodePrime] += (bsim2.B2drainConductance + gds + gbd + xrev * (gm + gmbs) + gcddb);
            // rstate.Matrix[bsim2.B2sNodePrime, bsim2.B2sNodePrime] += (bsim2.B2sourceConductance + gds + gbs + xnrm * (gm + gmbs) + gcssb);
            // rstate.Matrix[bsim2.B2dNode, bsim2.B2dNodePrime] += (-bsim2.B2drainConductance);
            // rstate.Matrix[bsim2.B2gNode, bsim2.B2bNode] += (-gcggb - gcgdb - gcgsb);
            // rstate.Matrix[bsim2.B2gNode, bsim2.B2dNodePrime] += (gcgdb);
            // rstate.Matrix[bsim2.B2gNode, bsim2.B2sNodePrime] += (gcgsb);
            // rstate.Matrix[bsim2.B2sNode, bsim2.B2sNodePrime] += (-bsim2.B2sourceConductance);
            // rstate.Matrix[bsim2.B2bNode, bsim2.B2gNode] += (gcbgb);
            // rstate.Matrix[bsim2.B2bNode, bsim2.B2dNodePrime] += (-gbd + gcbdb);
            // rstate.Matrix[bsim2.B2bNode, bsim2.B2sNodePrime] += (-gbs + gcbsb);
            // rstate.Matrix[bsim2.B2dNodePrime, bsim2.B2dNode] += (-bsim2.B2drainConductance);
            // rstate.Matrix[bsim2.B2dNodePrime, bsim2.B2gNode] += ((xnrm - xrev) * gm + gcdgb);
            // rstate.Matrix[bsim2.B2dNodePrime, bsim2.B2bNode] += (-gbd + (xnrm - xrev) * gmbs - gcdgb - gcddb - gcdsb);
            // rstate.Matrix[bsim2.B2dNodePrime, bsim2.B2sNodePrime] += (-gds - xnrm * (gm + gmbs) + gcdsb);
            // rstate.Matrix[bsim2.B2sNodePrime, bsim2.B2gNode] += (-(xnrm - xrev) * gm + gcsgb);
            // rstate.Matrix[bsim2.B2sNodePrime, bsim2.B2sNode] += (-bsim2.B2sourceConductance);
            // rstate.Matrix[bsim2.B2sNodePrime, bsim2.B2bNode] += (-gbs - (xnrm - xrev) * gmbs - gcsgb - gcsdb - gcssb);
            // rstate.Matrix[bsim2.B2sNodePrime, bsim2.B2dNodePrime] += (-gds - xrev * (gm + gmbs) + gcsdb);

            line1000:;
        }
    }
}
