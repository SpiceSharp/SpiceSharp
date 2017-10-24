using System;
using SpiceSharp.Circuits;
using SpiceSharp.Behaviours;
using SpiceSharp.Components.Transistors;

namespace SpiceSharp.Components.ComponentBehaviours
{
    /// <summary>
    /// General behaviour for a <see cref="MOS1"/>
    /// </summary>
    public class MOS1LoadBehaviour : CircuitObjectBehaviourLoad
    {
        /// <summary>
        /// Setup the behaviour
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        public override void Setup(ICircuitObject component, Circuit ckt)
        {
            base.Setup(component, ckt);
            var mos1 = ComponentTyped<MOS1>();

            mos1.MOS1vdsat = 0.0;
            mos1.MOS1von = 0.0;
        }

        /// <summary>
        /// Execute behaviour
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Execute(Circuit ckt)
        {
            var mos1 = ComponentTyped<MOS1>();
            var model = mos1.Model as MOS1Model;
            var state = ckt.State;
            var rstate = state.Real;
            var method = ckt.Method;
            double vt, EffectiveLength, DrainSatCur, SourceSatCur, GateSourceOverlapCap, GateDrainOverlapCap, GateBulkOverlapCap, Beta,
                OxideCap, vgs, vds, vbs, vbd, vgb, vgd, vgdo, delvbs, delvbd, delvgs, delvds, delvgd, cdhat, cbhat, von, evbs, evbd,
                vdsat, cdrain, sargsw, vgs1, vgd1, vgb1, capgs = 0, capgd = 0, capgb = 0, gcgs, ceqgs, gcgd, ceqgd, gcgb, ceqgb, ceqbs,
                ceqbd, cdreq;
            int Check, xnrm, xrev;

            vt = Circuit.CONSTKoverQ * mos1.MOS1temp;
            Check = 1;

            /* DETAILPROF */

            /* first, we compute a few useful values - these could be
			 * pre - computed, but for historical reasons are still done
			 * here.  They may be moved at the expense of instance size
			 */

            EffectiveLength = mos1.MOS1l - 2 * model.MOS1latDiff;
            if ((mos1.MOS1tSatCurDens == 0) || (mos1.MOS1drainArea.Value == 0) || (mos1.MOS1sourceArea.Value == 0))
            {
                DrainSatCur = mos1.MOS1tSatCur;
                SourceSatCur = mos1.MOS1tSatCur;
            }
            else
            {
                DrainSatCur = mos1.MOS1tSatCurDens * mos1.MOS1drainArea;
                SourceSatCur = mos1.MOS1tSatCurDens * mos1.MOS1sourceArea;
            }
            GateSourceOverlapCap = model.MOS1gateSourceOverlapCapFactor * mos1.MOS1w;
            GateDrainOverlapCap = model.MOS1gateDrainOverlapCapFactor * mos1.MOS1w;
            GateBulkOverlapCap = model.MOS1gateBulkOverlapCapFactor * EffectiveLength;
            Beta = mos1.MOS1tTransconductance * mos1.MOS1w / EffectiveLength;
            OxideCap = model.MOS1oxideCapFactor * EffectiveLength * mos1.MOS1w;
            /* 
			 * ok - now to do the start - up operations
			 * 
			 * we must get values for vbs, vds, and vgs from somewhere
			 * so we either predict them or recover them from last iteration
			 * These are the two most common cases - either a prediction
			 * step or the general iteration step and they
			 * share some code, so we put them first - others later on
			 */
            if ((state.Init == CircuitState.InitFlags.InitFloat || state.UseSmallSignal || (method != null && method.SavedTime == 0.0)) ||
                ((state.Init == CircuitState.InitFlags.InitFix) && (!mos1.MOS1off)))
            {
                /* PREDICTOR */

                /* general iteration */
                vbs = model.MOS1type * rstate.OldSolution[mos1.MOS1bNode] - rstate.OldSolution[mos1.MOS1sNodePrime];
                vgs = model.MOS1type * rstate.OldSolution[mos1.MOS1gNode] - rstate.OldSolution[mos1.MOS1sNodePrime];
                vds = model.MOS1type * rstate.OldSolution[mos1.MOS1dNodePrime] - rstate.OldSolution[mos1.MOS1sNodePrime];

                /* now some common crunching for some more useful quantities */
                vbd = vbs - vds;
                vgd = vgs - vds;
                vgdo = state.States[0][mos1.MOS1states + MOS1.MOS1vgs] - state.States[0][mos1.MOS1states + MOS1.MOS1vds];
                delvbs = vbs - state.States[0][mos1.MOS1states + MOS1.MOS1vbs];
                delvbd = vbd - state.States[0][mos1.MOS1states + MOS1.MOS1vbd];
                delvgs = vgs - state.States[0][mos1.MOS1states + MOS1.MOS1vgs];
                delvds = vds - state.States[0][mos1.MOS1states + MOS1.MOS1vds];
                delvgd = vgd - vgdo;

                /* these are needed for convergence testing */
                if (mos1.MOS1mode >= 0)
                {
                    cdhat = mos1.MOS1cd - mos1.MOS1gbd * delvbd + mos1.MOS1gmbs * delvbs + mos1.MOS1gm * delvgs + mos1.MOS1gds * delvds;
                }
                else
                {
                    cdhat = mos1.MOS1cd - (mos1.MOS1gbd - mos1.MOS1gmbs) * delvbd - mos1.MOS1gm * delvgd + mos1.MOS1gds * delvds;
                }
                cbhat = mos1.MOS1cbs + mos1.MOS1cbd + mos1.MOS1gbd * delvbd + mos1.MOS1gbs * delvbs;
                von = model.MOS1type * mos1.MOS1von;

                /* 
				 * limiting
				 * we want to keep device voltages from changing
				 * so fast that the exponentials churn out overflows
				 * and similar rudeness
				 */

                if (state.States[0][mos1.MOS1states + MOS1.MOS1vds] >= 0)
                {
                    vgs = Transistor.DEVfetlim(vgs, state.States[0][mos1.MOS1states + MOS1.MOS1vgs], von);
                    vds = vgs - vgd;
                    vds = Transistor.DEVlimvds(vds, state.States[0][mos1.MOS1states + MOS1.MOS1vds]);
                    vgd = vgs - vds;
                }
                else
                {
                    vgd = Transistor.DEVfetlim(vgd, vgdo, von);
                    vds = vgs - vgd;
                    vds = -Transistor.DEVlimvds(-vds, -(state.States[0][mos1.MOS1states + MOS1.MOS1vds]));
                    vgs = vgd + vds;
                }
                if (vds >= 0)
                {
                    vbs = Transistor.DEVpnjlim(vbs, state.States[0][mos1.MOS1states + MOS1.MOS1vbs], vt, mos1.MOS1sourceVcrit, ref Check);
                    vbd = vbs - vds;
                }
                else
                {
                    vbd = Transistor.DEVpnjlim(vbd, state.States[0][mos1.MOS1states + MOS1.MOS1vbd], vt, mos1.MOS1drainVcrit, ref Check);
                    vbs = vbd + vds;
                }
            }
            else
            {
                /* ok - not one of the simple cases, so we have to
				 * look at all of the possibilities for why we were
				 * called.  We still just initialize the three voltages
				 */

                if ((state.Init == CircuitState.InitFlags.InitJct) && !mos1.MOS1off)
                {
                    vds = model.MOS1type * mos1.MOS1icVDS;
                    vgs = model.MOS1type * mos1.MOS1icVGS;
                    vbs = model.MOS1type * mos1.MOS1icVBS;
                    if ((vds == 0) && (vgs == 0) && (vbs == 0) && ((method != null || state.UseDC ||
                        state.Domain == CircuitState.DomainTypes.None) || (!state.UseIC)))
                    {
                        vbs = -1;
                        vgs = model.MOS1type * mos1.MOS1tVto;
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
                mos1.MOS1gbs = SourceSatCur / vt;
                mos1.MOS1cbs = mos1.MOS1gbs * vbs;
                mos1.MOS1gbs += state.Gmin;
            }
            else
            {
                evbs = Math.Exp(Math.Min(Transistor.MAX_EXP_ARG, vbs / vt));
                mos1.MOS1gbs = SourceSatCur * evbs / vt + state.Gmin;
                mos1.MOS1cbs = SourceSatCur * (evbs - 1);
            }
            if (vbd <= 0)
            {
                mos1.MOS1gbd = DrainSatCur / vt;
                mos1.MOS1cbd = mos1.MOS1gbd * vbd;
                mos1.MOS1gbd += state.Gmin;
            }
            else
            {
                evbd = Math.Exp(Math.Min(Transistor.MAX_EXP_ARG, vbd / vt));
                mos1.MOS1gbd = DrainSatCur * evbd / vt + state.Gmin;
                mos1.MOS1cbd = DrainSatCur * (evbd - 1);
            }

            /* now to determine whether the user was able to correctly
			 * identify the source and drain of his device
			 */
            if (vds >= 0)
            {
                /* normal mode */
                mos1.MOS1mode = 1;
            }
            else
            {
                /* inverse mode */
                mos1.MOS1mode = -1;
            }

            /* DETAILPROF */
            {
                /* 
				 * this block of code evaluates the drain current and its 
				 * derivatives using the shichman - hodges model and the 
				 * charges associated with the gate, channel and bulk for 
				 * mosfets
				 */

                /* the following 4 variables are local to this code block until 
				 * it is obvious that they can be made global 
				 */
                double arg;
                double betap;
                double sarg;
                double vgst;

                if ((mos1.MOS1mode == 1 ? vbs : vbd) <= 0)
                {
                    sarg = Math.Sqrt(mos1.MOS1tPhi - (mos1.MOS1mode == 1 ? vbs : vbd));
                }
                else
                {
                    sarg = Math.Sqrt(mos1.MOS1tPhi);
                    sarg = sarg - (mos1.MOS1mode == 1 ? vbs : vbd) / (sarg + sarg);
                    sarg = Math.Max(0, sarg);
                }
                von = (mos1.MOS1tVbi * model.MOS1type) + model.MOS1gamma * sarg;
                vgst = (mos1.MOS1mode == 1 ? vgs : vgd) - von;
                vdsat = Math.Max(vgst, 0);
                if (sarg <= 0)
                {
                    arg = 0;
                }
                else
                {
                    arg = model.MOS1gamma / (sarg + sarg);
                }
                if (vgst <= 0)
                {
                    /* 
					 * cutoff region
					 */
                    cdrain = 0;
                    mos1.MOS1gm = 0;
                    mos1.MOS1gds = 0;
                    mos1.MOS1gmbs = 0;
                }
                else
                {
                    /* 
					 * saturation region
					 */
                    betap = Beta * (1 + model.MOS1lambda * (vds * mos1.MOS1mode));
                    if (vgst <= (vds * mos1.MOS1mode))
                    {
                        cdrain = betap * vgst * vgst * .5;
                        mos1.MOS1gm = betap * vgst;
                        mos1.MOS1gds = model.MOS1lambda * Beta * vgst * vgst * .5;
                        mos1.MOS1gmbs = mos1.MOS1gm * arg;
                    }
                    else
                    {
                        /* 
						* linear region
						*/
                        cdrain = betap * (vds * mos1.MOS1mode) * (vgst - .5 * (vds * mos1.MOS1mode));
                        mos1.MOS1gm = betap * (vds * mos1.MOS1mode);
                        mos1.MOS1gds = betap * (vgst - (vds * mos1.MOS1mode)) + model.MOS1lambda * Beta * (vds * mos1.MOS1mode) * (vgst - .5 * (vds * mos1.MOS1mode));
                        mos1.MOS1gmbs = mos1.MOS1gm * arg;
                    }
                }
                /* 
				 * finished
				 */
            }

            /* now deal with n vs p polarity */
            mos1.MOS1von = model.MOS1type * von;
            mos1.MOS1vdsat = model.MOS1type * vdsat;
            /* line 490 */
            /* 
			 * COMPUTE EQUIVALENT DRAIN CURRENT SOURCE
			 */
            mos1.MOS1cd = mos1.MOS1mode * cdrain - mos1.MOS1cbd;

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
                    double arg, sarg;

                    /* can't bypass the diode capacitance calculations */
                    /* CAPZEROBYPASS */
                    if (vbs < mos1.MOS1tDepCap)
                    {
                        arg = 1 - vbs / mos1.MOS1tBulkPot;
                        /* 
						 * the following block looks somewhat long and messy, 
						 * but since most users use the default grading
						 * coefficients of .5, and sqrt is MUCH faster than an
						 * Math.Exp(Math.Log()) we use this special case code to buy time.
						 * (as much as 10% of total job time!)
						 */
                        if (model.MOS1bulkJctBotGradingCoeff.Value == model.MOS1bulkJctSideGradingCoeff)
                        {
                            if (model.MOS1bulkJctBotGradingCoeff.Value == .5)
                            {
                                sarg = sargsw = 1 / Math.Sqrt(arg);
                            }
                            else
                            {
                                sarg = sargsw = Math.Exp(-model.MOS1bulkJctBotGradingCoeff * Math.Log(arg));
                            }
                        }
                        else
                        {
                            if (model.MOS1bulkJctBotGradingCoeff.Value == .5)
                            {
                                sarg = 1 / Math.Sqrt(arg);
                            }
                            else
                            {
                                /* NOSQRT */
                                sarg = Math.Exp(-model.MOS1bulkJctBotGradingCoeff * Math.Log(arg));
                            }
                            if (model.MOS1bulkJctSideGradingCoeff.Value == .5)
                            {
                                sargsw = 1 / Math.Sqrt(arg);
                            }
                            else
                            {
                                /* NOSQRT */
                                sargsw = Math.Exp(-model.MOS1bulkJctSideGradingCoeff * Math.Log(arg));
                            }
                        }
                        /* NOSQRT */
                        state.States[0][mos1.MOS1states + MOS1.MOS1qbs] = mos1.MOS1tBulkPot * (mos1.MOS1Cbs * (1 - arg * sarg) / (1 - model.MOS1bulkJctBotGradingCoeff) +
                            mos1.MOS1Cbssw * (1 - arg * sargsw) / (1 - model.MOS1bulkJctSideGradingCoeff));
                        mos1.MOS1capbs = mos1.MOS1Cbs * sarg + mos1.MOS1Cbssw * sargsw;
                    }
                    else
                    {
                        state.States[0][mos1.MOS1states + MOS1.MOS1qbs] = mos1.MOS1f4s + vbs * (mos1.MOS1f2s + vbs * (mos1.MOS1f3s / 2));
                        mos1.MOS1capbs = mos1.MOS1f2s + mos1.MOS1f3s * vbs;
                    }
                    /* CAPZEROBYPASS */
                }
                /* CAPBYPASS */
                /* can't bypass the diode capacitance calculations */
                {
                    double arg, sarg;

                    /* CAPZEROBYPASS */
                    if (vbd < mos1.MOS1tDepCap)
                    {
                        arg = 1 - vbd / mos1.MOS1tBulkPot;
                        /* 
						* the following block looks somewhat long and messy, 
						* but since most users use the default grading
						* coefficients of .5, and sqrt is MUCH faster than an
						* Math.Exp(Math.Log()) we use this special case code to buy time.
						* (as much as 10% of total job time!)
						*/
                        if (model.MOS1bulkJctBotGradingCoeff.Value == .5 && model.MOS1bulkJctSideGradingCoeff.Value == .5)
                        {
                            sarg = sargsw = 1 / Math.Sqrt(arg);
                        }
                        else
                        {
                            if (model.MOS1bulkJctBotGradingCoeff.Value == .5)
                            {
                                sarg = 1 / Math.Sqrt(arg);
                            }
                            else
                            {
                                /* NOSQRT */
                                sarg = Math.Exp(-model.MOS1bulkJctBotGradingCoeff * Math.Log(arg));
                            }
                            if (model.MOS1bulkJctSideGradingCoeff.Value == .5)
                            {
                                sargsw = 1 / Math.Sqrt(arg);
                            }
                            else
                            {
                                /* NOSQRT */
                                sargsw = Math.Exp(-model.MOS1bulkJctSideGradingCoeff * Math.Log(arg));
                            }
                        }
                        /* NOSQRT */
                        state.States[0][mos1.MOS1states + MOS1.MOS1qbd] = mos1.MOS1tBulkPot * (mos1.MOS1Cbd * (1 - arg * sarg) / (1 - model.MOS1bulkJctBotGradingCoeff) +
                            mos1.MOS1Cbdsw * (1 - arg * sargsw) / (1 - model.MOS1bulkJctSideGradingCoeff));
                        mos1.MOS1capbd = mos1.MOS1Cbd * sarg + mos1.MOS1Cbdsw * sargsw;
                    }
                    else
                    {
                        state.States[0][mos1.MOS1states + MOS1.MOS1qbd] = mos1.MOS1f4d + vbd * (mos1.MOS1f2d + vbd * mos1.MOS1f3d / 2);
                        mos1.MOS1capbd = mos1.MOS1f2d + vbd * mos1.MOS1f3d;
                    }
                    /* CAPZEROBYPASS */
                }
                /* 
				
				*/

                /* DETAILPROF */
                if ((method != null) || ((method != null && method.SavedTime == 0.0) && !state.UseIC))
                {
                    /* (above only excludes tranop, since we're only at this
					 * point if tran or tranop)
					 */

                    /* 
					 * calculate equivalent conductances and currents for
					 * depletion capacitors
					 */

                    /* integrate the capacitors and save results */

                    var result = method.Integrate(state, mos1.MOS1states + MOS1.MOS1qbd, mos1.MOS1capbd);
                    mos1.MOS1gbd += result.Geq;
                    mos1.MOS1cbd += state.States[0][mos1.MOS1states + MOS1.MOS1cqbd];
                    mos1.MOS1cd -= state.States[0][mos1.MOS1states + MOS1.MOS1cqbd];
                    result = method.Integrate(state, mos1.MOS1states + MOS1.MOS1qbs, mos1.MOS1capbs);
                    mos1.MOS1gbs += result.Geq;
                    mos1.MOS1cbs += state.States[0][mos1.MOS1states + MOS1.MOS1cqbs];
                }
            }

            /* 
			 * check convergence
			 */
            if (!mos1.MOS1off || (!(state.Init == CircuitState.InitFlags.InitFix || state.UseSmallSignal)))
            {
                if (Check == 1)
                    state.IsCon = false;
            }

            /* DETAILPROF */

            /* save things away for next time */
            state.States[0][mos1.MOS1states + MOS1.MOS1vbs] = vbs;
            state.States[0][mos1.MOS1states + MOS1.MOS1vbd] = vbd;
            state.States[0][mos1.MOS1states + MOS1.MOS1vgs] = vgs;
            state.States[0][mos1.MOS1states + MOS1.MOS1vds] = vds;

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
                if (mos1.MOS1mode > 0)
                {
                    Transistor.DEVqmeyer(vgs, vgd, vgb, von, vdsat,
                        out icapgs, out icapgd, out icapgb,
                        mos1.MOS1tPhi, OxideCap);
                }
                else
                {
                    Transistor.DEVqmeyer(vgd, vgs, vgb, von, vdsat,
                        out icapgd, out icapgs, out icapgb,
                        mos1.MOS1tPhi, OxideCap);
                }
                state.States[0][mos1.MOS1states + MOS1.MOS1capgs] = icapgs;
                state.States[0][mos1.MOS1states + MOS1.MOS1capgd] = icapgd;
                state.States[0][mos1.MOS1states + MOS1.MOS1capgb] = icapgb;
                vgs1 = state.States[1][mos1.MOS1states + MOS1.MOS1vgs];
                vgd1 = vgs1 - state.States[1][mos1.MOS1states + MOS1.MOS1vds];
                vgb1 = vgs1 - state.States[1][mos1.MOS1states + MOS1.MOS1vbs];
                if ((state.Domain == CircuitState.DomainTypes.Time && state.UseDC) || state.UseSmallSignal)
                {
                    capgs = 2 * state.States[0][mos1.MOS1states + MOS1.MOS1capgs] + GateSourceOverlapCap;
                    capgd = 2 * state.States[0][mos1.MOS1states + MOS1.MOS1capgd] + GateDrainOverlapCap;
                    capgb = 2 * state.States[0][mos1.MOS1states + MOS1.MOS1capgb] + GateBulkOverlapCap;
                }
                else
                {
                    capgs = (state.States[0][mos1.MOS1states + MOS1.MOS1capgs] + state.States[1][mos1.MOS1states + MOS1.MOS1capgs] + GateSourceOverlapCap);
                    capgd = (state.States[0][mos1.MOS1states + MOS1.MOS1capgd] + state.States[1][mos1.MOS1states + MOS1.MOS1capgd] + GateDrainOverlapCap);
                    capgb = (state.States[0][mos1.MOS1states + MOS1.MOS1capgb] + state.States[1][mos1.MOS1states + MOS1.MOS1capgb] + GateBulkOverlapCap);
                }

                /* PREDICTOR */
                if (method != null)
                {
                    state.States[0][mos1.MOS1states + MOS1.MOS1qgs] = (vgs - vgs1) * capgs + state.States[1][mos1.MOS1states + MOS1.MOS1qgs];
                    state.States[0][mos1.MOS1states + MOS1.MOS1qgd] = (vgd - vgd1) * capgd + state.States[1][mos1.MOS1states + MOS1.MOS1qgd];
                    state.States[0][mos1.MOS1states + MOS1.MOS1qgb] = (vgb - vgb1) * capgb + state.States[1][mos1.MOS1states + MOS1.MOS1qgb];
                }
                else
                {
                    /* TRANOP only */
                    state.States[0][mos1.MOS1states + MOS1.MOS1qgs] = vgs * capgs;
                    state.States[0][mos1.MOS1states + MOS1.MOS1qgd] = vgd * capgd;
                    state.States[0][mos1.MOS1states + MOS1.MOS1qgb] = vgb * capgb;
                }
                /* PREDICTOR */
            }

            if (((method != null && method.SavedTime == 0.0)) || (!(method != null)))
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
                    state.States[0][mos1.MOS1states + MOS1.MOS1cqgs] = 0;
                if (capgd == 0)
                    state.States[0][mos1.MOS1states + MOS1.MOS1cqgd] = 0;
                if (capgb == 0)
                    state.States[0][mos1.MOS1states + MOS1.MOS1cqgb] = 0;
                /* 
				 * calculate equivalent conductances and currents for
				 * meyer"s capacitors
				 */
                method.Integrate(state, out gcgs, out ceqgs, mos1.MOS1states + MOS1.MOS1qgs, capgs);
                method.Integrate(state, out gcgd, out ceqgd, mos1.MOS1states + MOS1.MOS1qgd, capgd);
                method.Integrate(state, out gcgb, out ceqgb, mos1.MOS1states + MOS1.MOS1qgb, capgb);
                ceqgs = ceqgs - gcgs * vgs + method.Slope * state.States[0][mos1.MOS1states + MOS1.MOS1qgs];
                ceqgd = ceqgd - gcgd * vgd + method.Slope * state.States[0][mos1.MOS1states + MOS1.MOS1qgd];
                ceqgb = ceqgb - gcgb * vgb + method.Slope * state.States[0][mos1.MOS1states + MOS1.MOS1qgb];
            }

            /* 
			 * load current vector
			 */
            ceqbs = model.MOS1type * (mos1.MOS1cbs - (mos1.MOS1gbs - state.Gmin) * vbs);
            ceqbd = model.MOS1type * (mos1.MOS1cbd - (mos1.MOS1gbd - state.Gmin) * vbd);
            if (mos1.MOS1mode >= 0)
            {
                xnrm = 1;
                xrev = 0;
                cdreq = model.MOS1type * (cdrain - mos1.MOS1gds * vds - mos1.MOS1gm * vgs - mos1.MOS1gmbs * vbs);
            }
            else
            {
                xnrm = 0;
                xrev = 1;
                cdreq = -(model.MOS1type) * (cdrain - mos1.MOS1gds * (-vds) - mos1.MOS1gm * vgd - mos1.MOS1gmbs * vbd);
            }
            rstate.Rhs[mos1.MOS1gNode] -= (model.MOS1type * (ceqgs + ceqgb + ceqgd));
            rstate.Rhs[mos1.MOS1bNode] -= (ceqbs + ceqbd - model.MOS1type * ceqgb);
            rstate.Rhs[mos1.MOS1dNodePrime] += (ceqbd - cdreq + model.MOS1type * ceqgd);
            rstate.Rhs[mos1.MOS1sNodePrime] += cdreq + ceqbs + model.MOS1type * ceqgs;

            /* 
			 * load y matrix
			 */
            rstate.Matrix[mos1.MOS1dNode, mos1.MOS1dNode] += (mos1.MOS1drainConductance);
            rstate.Matrix[mos1.MOS1gNode, mos1.MOS1gNode] += ((gcgd + gcgs + gcgb));
            rstate.Matrix[mos1.MOS1sNode, mos1.MOS1sNode] += (mos1.MOS1sourceConductance);
            rstate.Matrix[mos1.MOS1bNode, mos1.MOS1bNode] += (mos1.MOS1gbd + mos1.MOS1gbs + gcgb);
            rstate.Matrix[mos1.MOS1dNodePrime, mos1.MOS1dNodePrime] += (mos1.MOS1drainConductance + mos1.MOS1gds + mos1.MOS1gbd + xrev * (mos1.MOS1gm + mos1.MOS1gmbs) + gcgd);
            rstate.Matrix[mos1.MOS1sNodePrime, mos1.MOS1sNodePrime] += (mos1.MOS1sourceConductance + mos1.MOS1gds + mos1.MOS1gbs + xnrm * (mos1.MOS1gm + mos1.MOS1gmbs) +
                gcgs);
            rstate.Matrix[mos1.MOS1dNode, mos1.MOS1dNodePrime] += (-mos1.MOS1drainConductance);
            rstate.Matrix[mos1.MOS1gNode, mos1.MOS1bNode] -= gcgb;
            rstate.Matrix[mos1.MOS1gNode, mos1.MOS1dNodePrime] -= gcgd;
            rstate.Matrix[mos1.MOS1gNode, mos1.MOS1sNodePrime] -= gcgs;
            rstate.Matrix[mos1.MOS1sNode, mos1.MOS1sNodePrime] += (-mos1.MOS1sourceConductance);
            rstate.Matrix[mos1.MOS1bNode, mos1.MOS1gNode] -= gcgb;
            rstate.Matrix[mos1.MOS1bNode, mos1.MOS1dNodePrime] -= mos1.MOS1gbd;
            rstate.Matrix[mos1.MOS1bNode, mos1.MOS1sNodePrime] -= mos1.MOS1gbs;
            rstate.Matrix[mos1.MOS1dNodePrime, mos1.MOS1dNode] += (-mos1.MOS1drainConductance);
            rstate.Matrix[mos1.MOS1dNodePrime, mos1.MOS1gNode] += ((xnrm - xrev) * mos1.MOS1gm - gcgd);
            rstate.Matrix[mos1.MOS1dNodePrime, mos1.MOS1bNode] += (-mos1.MOS1gbd + (xnrm - xrev) * mos1.MOS1gmbs);
            rstate.Matrix[mos1.MOS1dNodePrime, mos1.MOS1sNodePrime] += (-mos1.MOS1gds - xnrm * (mos1.MOS1gm + mos1.MOS1gmbs));
            rstate.Matrix[mos1.MOS1sNodePrime, mos1.MOS1gNode] += (-(xnrm - xrev) * mos1.MOS1gm - gcgs);
            rstate.Matrix[mos1.MOS1sNodePrime, mos1.MOS1sNode] += (-mos1.MOS1sourceConductance);
            rstate.Matrix[mos1.MOS1sNodePrime, mos1.MOS1bNode] += (-mos1.MOS1gbs - (xnrm - xrev) * mos1.MOS1gmbs);
            rstate.Matrix[mos1.MOS1sNodePrime, mos1.MOS1dNodePrime] += (-mos1.MOS1gds - xrev * (mos1.MOS1gm + mos1.MOS1gmbs));
        }
    }
}
