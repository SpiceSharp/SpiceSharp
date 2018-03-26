using System;
using SpiceSharp.Behaviors;
using SpiceSharp.Circuits;
using SpiceSharp.Components.Transistors;
using static SpiceSharp.Components.Transistors.BSIM1Helpers;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// General behaviour for a <see cref="BSIM1"/>
    /// </summary>
    public class BSIM1LoadBehavior : LoadBehavior
    {
        /// <summary>
        /// Setup the behaviour
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        public override void Setup(Entity component, Circuit ckt)
        {
            base.Setup(component, ckt);
            var bsim1 = ComponentTyped<BSIM1>();
            bsim1.B1vdsat = 0;
            bsim1.B1von = 0;
        }

        /// <summary>
        /// Execute the behaviour
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Load(Circuit ckt)
        {
            var bsim1 = ComponentTyped<BSIM1>();
            var model = bsim1.Model as BSIM1Model;
            var state = ckt.State;
            var rstate = state;
            var method = ckt.Method;
            double EffectiveLength, DrainArea, SourceArea, DrainPerimeter, SourcePerimeter, DrainSatCurrent, SourceSatCurrent,
                GateSourceOverlapCap, GateDrainOverlapCap, GateBulkOverlapCap, von, vdsat, vt0;
            int Check;
            double vbs, vgs, vds, vbd, vgd, vgdo, delvbs, delvbd, delvgs, delvds, delvgd, cdhat, cbhat, vcrit, vgb, gbs, cbs,
                evbs, gbd, cbd, evbd, cd, czbd, czbs, czbdsw, czbssw, PhiB, PhiBSW, MJ, MJSW, arg, argsw, sarg, sargsw, capbs = 0.0,
                capbd = 0.0, ceqqg, gcdgb, gcsgb, gcggb, gcbgb, cqgate, cqbulk, cqdrn, ceqqb, ceqqd, ceqbs, ceqbd, xnrm, xrev, cdreq;
            double gm, gds, gmbs, qgate, qbulk, qdrn = 0.0, cggb = 0.0, cgdb = 0.0, cgsb = 0.0, cbgb = 0.0, cbdb = 0.0, cbsb = 0.0, cdgb = 0.0,
                cddb = 0.0, cdsb = 0.0, cdrain, qsrc = 0.0, csgb = 0.0, cssb = 0.0, csdb = 0.0, gcgdb, gcgsb, gcbdb, gcbsb, gcddb, gcdsb, gcsdb, gcssb;

            EffectiveLength = bsim1.B1l - model.B1deltaL * 1.0e-6; /* m */
            DrainArea = bsim1.B1drainArea;
            SourceArea = bsim1.B1sourceArea;
            DrainPerimeter = bsim1.B1drainPerimeter;
            SourcePerimeter = bsim1.B1sourcePerimeter;
            if ((DrainSatCurrent = DrainArea * model.B1jctSatCurDensity) < 1e-15)
            {
                DrainSatCurrent = 1.0e-15;
            }
            if ((SourceSatCurrent = SourceArea * model.B1jctSatCurDensity) < 1.0e-15)
            {
                SourceSatCurrent = 1.0e-15;
            }
            GateSourceOverlapCap = model.B1gateSourceOverlapCap * bsim1.B1w;
            GateDrainOverlapCap = model.B1gateDrainOverlapCap * bsim1.B1w;
            GateBulkOverlapCap = model.B1gateBulkOverlapCap * EffectiveLength;
            von = model.B1type * bsim1.B1von;
            vdsat = model.B1type * bsim1.B1vdsat;
            vt0 = model.B1type * bsim1.B1vt0;

            Check = 1;
            if (state.UseSmallSignal)
            {
                vbs = state.States[0][bsim1.B1states + BSIM1.B1vbs];
                vgs = state.States[0][bsim1.B1states + BSIM1.B1vgs];
                vds = state.States[0][bsim1.B1states + BSIM1.B1vds];
            }
            else if (state.Init == State.InitFlags.InitTransient)
            {
                vbs = state.States[1][bsim1.B1states + BSIM1.B1vbs];
                vgs = state.States[1][bsim1.B1states + BSIM1.B1vgs];
                vds = state.States[1][bsim1.B1states + BSIM1.B1vds];
            }
            else if (state.Init == State.InitFlags.InitJct && !bsim1.B1off)
            {
                vds = model.B1type * bsim1.B1icVDS;
                vgs = model.B1type * bsim1.B1icVGS;
                vbs = model.B1type * bsim1.B1icVBS;
                if ((vds == 0) && (vgs == 0) && (vbs == 0) &&
                    (method != null || state.UseDC || state.Domain == State.DomainTypes.None || !state.UseIC))
                {
                    vbs = -1;
                    vgs = vt0;
                    vds = 0;
                }
            }
            else if ((state.Init == State.InitFlags.InitJct || state.Init == State.InitFlags.InitFix) && (bsim1.B1off))
            {
                vbs = vgs = vds = 0;
            }
            else
            {
                /* PREDICTOR */
                vbs = model.B1type * (rstate.OldSolution[bsim1.B1bNode] - rstate.OldSolution[bsim1.B1sNodePrime]);
                vgs = model.B1type * (rstate.OldSolution[bsim1.B1gNode] - rstate.OldSolution[bsim1.B1sNodePrime]);
                vds = model.B1type * (rstate.OldSolution[bsim1.B1dNodePrime] - rstate.OldSolution[bsim1.B1sNodePrime]);
                /* PREDICTOR */
                vbd = vbs - vds;
                vgd = vgs - vds;
                vgdo = state.States[0][bsim1.B1states + BSIM1.B1vgs] - state.States[0][bsim1.B1states + BSIM1.B1vds];
                delvbs = vbs - state.States[0][bsim1.B1states + BSIM1.B1vbs];
                delvbd = vbd - state.States[0][bsim1.B1states + BSIM1.B1vbd];
                delvgs = vgs - state.States[0][bsim1.B1states + BSIM1.B1vgs];
                delvds = vds - state.States[0][bsim1.B1states + BSIM1.B1vds];
                delvgd = vgd - vgdo;

                if (bsim1.B1mode >= 0)
                {
                    cdhat = state.States[0][bsim1.B1states + BSIM1.B1cd] - state.States[0][bsim1.B1states + BSIM1.B1gbd] * delvbd + state.States[0][bsim1.B1states + BSIM1.B1gmbs] *
                         delvbs + state.States[0][bsim1.B1states + BSIM1.B1gm] * delvgs + state.States[0][bsim1.B1states + BSIM1.B1gds] * delvds;
                }
                else
                {
                    cdhat = state.States[0][bsim1.B1states + BSIM1.B1cd] - (state.States[0][bsim1.B1states + BSIM1.B1gbd] - state.States[0][bsim1.B1states + BSIM1.B1gmbs]) * delvbd -
                         state.States[0][bsim1.B1states + BSIM1.B1gm] * delvgd + state.States[0][bsim1.B1states + BSIM1.B1gds] * delvds;
                }
                cbhat = state.States[0][bsim1.B1states + BSIM1.B1cbs] + state.States[0][bsim1.B1states + BSIM1.B1cbd] + state.States[0][bsim1.B1states + BSIM1.B1gbd] * delvbd +
                     state.States[0][bsim1.B1states + BSIM1.B1gbs] * delvbs;

                /* NOBYPASS */

                von = model.B1type * bsim1.B1von;
                if (state.States[0][bsim1.B1states + BSIM1.B1vds] >= 0)
                {
                    vgs = Transistor.DEVfetlim(vgs, state.States[0][bsim1.B1states + BSIM1.B1vgs], von);
                    vds = vgs - vgd;
                    vds = Transistor.DEVlimvds(vds, state.States[0][bsim1.B1states + BSIM1.B1vds]);
                    vgd = vgs - vds;
                }
                else
                {
                    vgd = Transistor.DEVfetlim(vgd, vgdo, von);
                    vds = vgs - vgd;
                    vds = -Transistor.DEVlimvds(-vds, -(state.States[0][bsim1.B1states + BSIM1.B1vds]));
                    vgs = vgd + vds;
                }
                if (vds >= 0)
                {
                    vcrit = Circuit.CONSTvt0 * Math.Log(Circuit.CONSTvt0 / (Circuit.CONSTroot2 * SourceSatCurrent));
                    vbs = Transistor.DEVpnjlim(vbs, state.States[0][bsim1.B1states + BSIM1.B1vbs], Circuit.CONSTvt0, vcrit, ref Check); /* bsim1.B1 test */
                    vbd = vbs - vds;
                }
                else
                {
                    vcrit = Circuit.CONSTvt0 * Math.Log(Circuit.CONSTvt0 / (Circuit.CONSTroot2 * DrainSatCurrent));
                    vbd = Transistor.DEVpnjlim(vbd, state.States[0][bsim1.B1states + BSIM1.B1vbd], Circuit.CONSTvt0, vcrit, ref Check); /* bsim1.B1 test */
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
                bsim1.B1mode = 1;
            }
            else
            {
                /* inverse mode */
                bsim1.B1mode = -1;
            }
            /* call bsim1.B1evaluate to calculate drain current and its 
			* derivatives and charge and capacitances related to gate
			* drain, and bulk
			*/
            if (vds >= 0)
            {
                bsim1.B1evaluate(vds, vbs, vgs, out gm, out gds, out gmbs, out qgate,
                out qbulk, out qdrn, out cggb, out cgdb, out cgsb, out cbgb, out cbdb, out cbsb, out cdgb,
                out cddb, out cdsb, out cdrain, out von, out vdsat, ckt);
            }
            else
            {
                bsim1.B1evaluate(-vds, vbd, vgd, out gm, out gds, out gmbs, out qgate,
                out qbulk, out qsrc, out cggb, out cgsb, out cgdb, out cbgb, out cbsb, out cbdb, out csgb,
                out cssb, out csdb, out cdrain, out von, out vdsat, ckt);
            }

            bsim1.B1von = model.B1type * von;
            bsim1.B1vdsat = model.B1type * vdsat;

            /* 
			* COMPUTE EQUIVALENT DRAIN CURRENT SOURCE
			*/
            cd = bsim1.B1mode * cdrain - cbd;
            if ((method != null || state.UseSmallSignal) || ((state.Domain == State.DomainTypes.Time && state.UseDC) && state.UseIC))
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

                czbd = model.B1unitAreaJctCap * DrainArea;
                czbs = model.B1unitAreaJctCap * SourceArea;
                czbdsw = model.B1unitLengthSidewallJctCap * DrainPerimeter;
                czbssw = model.B1unitLengthSidewallJctCap * SourcePerimeter;
                PhiB = model.B1bulkJctPotential;
                PhiBSW = model.B1sidewallJctPotential;
                MJ = model.B1bulkJctBotGradingCoeff;
                MJSW = model.B1bulkJctSideGradingCoeff;

                /* Source Bulk Junction */
                if (vbs < 0)
                {
                    arg = 1 - vbs / PhiB;
                    argsw = 1 - vbs / PhiBSW;
                    sarg = Math.Exp(-MJ * Math.Log(arg));
                    sargsw = Math.Exp(-MJSW * Math.Log(argsw));
                    state.States[0][bsim1.B1states + BSIM1.B1qbs] = PhiB * czbs * (1 - arg * sarg) / (1 - MJ) + PhiBSW * czbssw * (1 - argsw * sargsw) / (1 -
                         MJSW);
                    capbs = czbs * sarg + czbssw * sargsw;
                }
                else
                {
                    state.States[0][bsim1.B1states + BSIM1.B1qbs] = vbs * (czbs + czbssw) + vbs * vbs * (czbs * MJ * 0.5 / PhiB + czbssw * MJSW * 0.5 / PhiBSW);
                    capbs = czbs + czbssw + vbs * (czbs * MJ / PhiB + czbssw * MJSW / PhiBSW);
                }

                /* Drain Bulk Junction */
                if (vbd < 0)
                {
                    arg = 1 - vbd / PhiB;
                    argsw = 1 - vbd / PhiBSW;
                    sarg = Math.Exp(-MJ * Math.Log(arg));
                    sargsw = Math.Exp(-MJSW * Math.Log(argsw));
                    state.States[0][bsim1.B1states + BSIM1.B1qbd] = PhiB * czbd * (1 - arg * sarg) / (1 - MJ) + PhiBSW * czbdsw * (1 - argsw * sargsw) / (1 -
                         MJSW);
                    capbd = czbd * sarg + czbdsw * sargsw;
                }
                else
                {
                    state.States[0][bsim1.B1states + BSIM1.B1qbd] = vbd * (czbd + czbdsw) + vbd * vbd * (czbd * MJ * 0.5 / PhiB + czbdsw * MJSW * 0.5 / PhiBSW);
                    capbd = czbd + czbdsw + vbd * (czbd * MJ / PhiB + czbdsw * MJSW / PhiBSW);
                }

            }

            /* 
			* check convergence
			*/
            if (!bsim1.B1off || state.Init != State.InitFlags.InitFix)
            {
                if (Check == 1)
                    state.IsCon = false;
            }
            state.States[0][bsim1.B1states + BSIM1.B1vbs] = vbs;
            state.States[0][bsim1.B1states + BSIM1.B1vbd] = vbd;
            state.States[0][bsim1.B1states + BSIM1.B1vgs] = vgs;
            state.States[0][bsim1.B1states + BSIM1.B1vds] = vds;
            state.States[0][bsim1.B1states + BSIM1.B1cd] = cd;
            state.States[0][bsim1.B1states + BSIM1.B1cbs] = cbs;
            state.States[0][bsim1.B1states + BSIM1.B1cbd] = cbd;
            state.States[0][bsim1.B1states + BSIM1.B1gm] = gm;
            state.States[0][bsim1.B1states + BSIM1.B1gds] = gds;
            state.States[0][bsim1.B1states + BSIM1.B1gmbs] = gmbs;
            state.States[0][bsim1.B1states + BSIM1.B1gbd] = gbd;
            state.States[0][bsim1.B1states + BSIM1.B1gbs] = gbs;

            state.States[0][bsim1.B1states + BSIM1.B1cggb] = cggb;
            state.States[0][bsim1.B1states + BSIM1.B1cgdb] = cgdb;
            state.States[0][bsim1.B1states + BSIM1.B1cgsb] = cgsb;

            state.States[0][bsim1.B1states + BSIM1.B1cbgb] = cbgb;
            state.States[0][bsim1.B1states + BSIM1.B1cbdb] = cbdb;
            state.States[0][bsim1.B1states + BSIM1.B1cbsb] = cbsb;

            state.States[0][bsim1.B1states + BSIM1.B1cdgb] = cdgb;
            state.States[0][bsim1.B1states + BSIM1.B1cddb] = cddb;
            state.States[0][bsim1.B1states + BSIM1.B1cdsb] = cdsb;

            state.States[0][bsim1.B1states + BSIM1.B1capbs] = capbs;
            state.States[0][bsim1.B1states + BSIM1.B1capbd] = capbd;

            /* bulk and channel charge plus overlaps */
            if (method == null && ((!(state.Domain == State.DomainTypes.Time && state.UseDC)) || (!state.UseIC)) && (!state.UseSmallSignal))
                goto line850;

            if (bsim1.B1mode > 0)
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

                B1mosCap(ckt, vgd, vgs, vgb, args, cbgb, cbdb, cbsb, cdgb, cddb, cdsb,
                    out gcggb, out gcgdb, out gcgsb, out gcbgb, out gcbdb, out gcbsb,
                    out gcdgb, out gcddb, out gcdsb, out gcsgb, out gcsdb, out gcssb,
                    ref qgate, ref qbulk, ref qdrn, out qsrc);
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

                B1mosCap(ckt, vgs, vgd, vgb, args, cbgb, cbsb, cbdb, csgb, cssb, csdb,
                out gcggb, out gcgsb, out gcgdb,
                out gcbgb, out gcbsb, out gcbdb,
                out gcsgb, out gcssb, out gcsdb, out gcdgb, out gcdsb, out gcddb,
                ref qgate, ref qbulk, ref qsrc, out qdrn);
            }

            state.States[0][bsim1.B1states + BSIM1.B1qg] = qgate;
            state.States[0][bsim1.B1states + BSIM1.B1qd] = qdrn - state.States[0][bsim1.B1states + BSIM1.B1qbd];
            state.States[0][bsim1.B1states + BSIM1.B1qb] = qbulk + state.States[0][bsim1.B1states + BSIM1.B1qbd] + state.States[0][bsim1.B1states + BSIM1.B1qbs];

            /* store small signal parameters */
            if (method == null && (state.Domain == State.DomainTypes.Time && state.UseDC) && state.UseIC)
                goto line850;
            if (state.UseSmallSignal)
            {
                state.States[0][bsim1.B1states + BSIM1.B1cggb] = cggb;
                state.States[0][bsim1.B1states + BSIM1.B1cgdb] = cgdb;
                state.States[0][bsim1.B1states + BSIM1.B1cgsb] = cgsb;
                state.States[0][bsim1.B1states + BSIM1.B1cbgb] = cbgb;
                state.States[0][bsim1.B1states + BSIM1.B1cbdb] = cbdb;
                state.States[0][bsim1.B1states + BSIM1.B1cbsb] = cbsb;
                state.States[0][bsim1.B1states + BSIM1.B1cdgb] = cdgb;
                state.States[0][bsim1.B1states + BSIM1.B1cddb] = cddb;
                state.States[0][bsim1.B1states + BSIM1.B1cdsb] = cdsb;
                state.States[0][bsim1.B1states + BSIM1.B1capbd] = capbd;
                state.States[0][bsim1.B1states + BSIM1.B1capbs] = capbs;

                goto line1000;
            }

            if (state.Init == State.InitFlags.InitTransient)
            {
                state.States[1][bsim1.B1states + BSIM1.B1qb] = state.States[0][bsim1.B1states + BSIM1.B1qb];
                state.States[1][bsim1.B1states + BSIM1.B1qg] = state.States[0][bsim1.B1states + BSIM1.B1qg];
                state.States[1][bsim1.B1states + BSIM1.B1qd] = state.States[0][bsim1.B1states + BSIM1.B1qd];
            }

            if (method != null)
            {
                method.Integrate(state, bsim1.B1states + BSIM1.B1qb, 0.0);
                method.Integrate(state, bsim1.B1states + BSIM1.B1qg, 0.0);
                method.Integrate(state, bsim1.B1states + BSIM1.B1qd, 0.0);
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
            cqgate = state.States[0][bsim1.B1states + BSIM1.B1iqg];
            cqbulk = state.States[0][bsim1.B1states + BSIM1.B1iqb];
            cqdrn = state.States[0][bsim1.B1states + BSIM1.B1iqd];
            ceqqg = cqgate - gcggb * vgb + gcgdb * vbd + gcgsb * vbs;
            ceqqb = cqbulk - gcbgb * vgb + gcbdb * vbd + gcbsb * vbs;
            ceqqd = cqdrn - gcdgb * vgb + gcddb * vbd + gcdsb * vbs;

            if (state.Init == State.InitFlags.InitTransient)
            {
                state.States[1][bsim1.B1states + BSIM1.B1iqb] = state.States[0][bsim1.B1states + BSIM1.B1iqb];
                state.States[1][bsim1.B1states + BSIM1.B1iqg] = state.States[0][bsim1.B1states + BSIM1.B1iqg];
                state.States[1][bsim1.B1states + BSIM1.B1iqd] = state.States[0][bsim1.B1states + BSIM1.B1iqd];
            }

            /* 
			* load current vector
			*/
            line900:

            ceqbs = model.B1type * (cbs - (gbs - state.Gmin) * vbs);
            ceqbd = model.B1type * (cbd - (gbd - state.Gmin) * vbd);

            ceqqg = model.B1type * ceqqg;
            ceqqb = model.B1type * ceqqb;
            ceqqd = model.B1type * ceqqd;
            if (bsim1.B1mode >= 0)
            {
                xnrm = 1;
                xrev = 0;
                cdreq = model.B1type * (cdrain - gds * vds - gm * vgs - gmbs * vbs);
            }
            else
            {
                xnrm = 0;
                xrev = 1;
                cdreq = -(model.B1type) * (cdrain + gds * vds - gm * vgd - gmbs * vbd);
            }

            // rstate.Rhs[bsim1.B1gNode] -= ceqqg;
            // rstate.Rhs[bsim1.B1bNode] -= (ceqbs + ceqbd + ceqqb);
            // rstate.Rhs[bsim1.B1dNodePrime] += (ceqbd - cdreq - ceqqd);
            // rstate.Rhs[bsim1.B1sNodePrime] += (cdreq + ceqbs + ceqqg + ceqqb + ceqqd);

            /* 
			* load y matrix
			*/

            // rstate.Matrix[bsim1.B1dNode, bsim1.B1dNode] += (bsim1.B1drainConductance);
            // rstate.Matrix[bsim1.B1gNode, bsim1.B1gNode] += (gcggb);
            // rstate.Matrix[bsim1.B1sNode, bsim1.B1sNode] += (bsim1.B1sourceConductance);
            // rstate.Matrix[bsim1.B1bNode, bsim1.B1bNode] += (gbd + gbs - gcbgb - gcbdb - gcbsb);
            // rstate.Matrix[bsim1.B1dNodePrime, bsim1.B1dNodePrime] += (bsim1.B1drainConductance + gds + gbd + xrev * (gm + gmbs) + gcddb);
            // rstate.Matrix[bsim1.B1sNodePrime, bsim1.B1sNodePrime] += (bsim1.B1sourceConductance + gds + gbs + xnrm * (gm + gmbs) + gcssb);
            // rstate.Matrix[bsim1.B1dNode, bsim1.B1dNodePrime] += (-bsim1.B1drainConductance);
            // rstate.Matrix[bsim1.B1gNode, bsim1.B1bNode] += (-gcggb - gcgdb - gcgsb);
            // rstate.Matrix[bsim1.B1gNode, bsim1.B1dNodePrime] += (gcgdb);
            // rstate.Matrix[bsim1.B1gNode, bsim1.B1sNodePrime] += (gcgsb);
            // rstate.Matrix[bsim1.B1sNode, bsim1.B1sNodePrime] += (-bsim1.B1sourceConductance);
            // rstate.Matrix[bsim1.B1bNode, bsim1.B1gNode] += (gcbgb);
            // rstate.Matrix[bsim1.B1bNode, bsim1.B1dNodePrime] += (-gbd + gcbdb);
            // rstate.Matrix[bsim1.B1bNode, bsim1.B1sNodePrime] += (-gbs + gcbsb);
            // rstate.Matrix[bsim1.B1dNodePrime, bsim1.B1dNode] += (-bsim1.B1drainConductance);
            // rstate.Matrix[bsim1.B1dNodePrime, bsim1.B1gNode] += ((xnrm - xrev) * gm + gcdgb);
            // rstate.Matrix[bsim1.B1dNodePrime, bsim1.B1bNode] += (-gbd + (xnrm - xrev) * gmbs - gcdgb - gcddb - gcdsb);
            // rstate.Matrix[bsim1.B1dNodePrime, bsim1.B1sNodePrime] += (-gds - xnrm * (gm + gmbs) + gcdsb);
            // rstate.Matrix[bsim1.B1sNodePrime, bsim1.B1gNode] += (-(xnrm - xrev) * gm + gcsgb);
            // rstate.Matrix[bsim1.B1sNodePrime, bsim1.B1sNode] += (-bsim1.B1sourceConductance);
            // rstate.Matrix[bsim1.B1sNodePrime, bsim1.B1bNode] += (-gbs - (xnrm - xrev) * gmbs - gcsgb - gcsdb - gcssb);
            // rstate.Matrix[bsim1.B1sNodePrime, bsim1.B1dNodePrime] += (-gds - xrev * (gm + gmbs) + gcsdb);

            line1000:;
        }
    }
}
