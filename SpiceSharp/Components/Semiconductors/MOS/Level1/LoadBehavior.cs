using System;
using SpiceSharp.Circuits;
using SpiceSharp.Attributes;
using SpiceSharp.Sparse;
using SpiceSharp.Simulations;
using SpiceSharp.Components.Transistors;
using SpiceSharp.Components.Mosfet.Level1;

namespace SpiceSharp.Behaviors.Mosfet.Level1
{
    /// <summary>
    /// General behavior for a <see cref="Components.MOS1"/>
    /// </summary>
    public class LoadBehavior : Behaviors.LoadBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        BaseParameters bp;
        TemperatureBehavior temp;
        ModelBaseParameters mbp;
        ModelTemperatureBehavior modeltemp;

        /// <summary>
        /// Nodes
        /// </summary>
        int MOS1dNode, MOS1gNode, MOS1sNode, MOS1bNode;
        public int MOS1dNodePrime { get; protected set; }
        public int MOS1sNodePrime { get; protected set; }

        [SpiceName("von"), SpiceInfo(" ")]
        public double MOS1von { get; protected set; } = 0.0;
        [SpiceName("vdsat"), SpiceInfo("Saturation drain voltage")]
        public double MOS1vdsat { get; protected set; } = 0.0;
        [SpiceName("id"), SpiceInfo("Drain current")]
        public double MOS1cd { get; protected set; }
        [SpiceName("ibs"), SpiceInfo("B-S junction current")]
        public double MOS1cbs { get; protected set; }
        [SpiceName("ibd"), SpiceInfo("B-D junction current")]
        public double MOS1cbd { get; protected set; }
        [SpiceName("gmb"), SpiceName("gmbs"), SpiceInfo("Bulk-Source transconductance")]
        public double MOS1gmbs { get; protected set; }
        [SpiceName("gm"), SpiceInfo("Transconductance")]
        public double MOS1gm { get; protected set; }
        [SpiceName("gds"), SpiceInfo("Drain-Source conductance")]
        public double MOS1gds { get; protected set; }
        [SpiceName("gbd"), SpiceInfo("Bulk-Drain conductance")]
        public double MOS1gbd { get; protected set; }
        [SpiceName("gbs"), SpiceInfo("Bulk-Source conductance")]
        public double MOS1gbs { get; protected set; }

        /// <summary>
        /// Extra variables
        /// </summary>
        public double MOS1mode { get; protected set; }

        public double MOS1vbd { get; protected set; }
        public double MOS1vbs { get; protected set; }
        public double MOS1vgs { get; protected set; }
        public double MOS1vds { get; protected set; }

        /// <summary>
        /// Matrix elements
        /// </summary>
        protected MatrixElement MOS1DdPtr { get; private set; }
        protected MatrixElement MOS1GgPtr { get; private set; }
        protected MatrixElement MOS1SsPtr { get; private set; }
        protected MatrixElement MOS1BbPtr { get; private set; }
        protected MatrixElement MOS1DPdpPtr { get; private set; }
        protected MatrixElement MOS1SPspPtr { get; private set; }
        protected MatrixElement MOS1DdpPtr { get; private set; }
        protected MatrixElement MOS1GbPtr { get; private set; }
        protected MatrixElement MOS1GdpPtr { get; private set; }
        protected MatrixElement MOS1GspPtr { get; private set; }
        protected MatrixElement MOS1SspPtr { get; private set; }
        protected MatrixElement MOS1BdpPtr { get; private set; }
        protected MatrixElement MOS1BspPtr { get; private set; }
        protected MatrixElement MOS1DPspPtr { get; private set; }
        protected MatrixElement MOS1DPdPtr { get; private set; }
        protected MatrixElement MOS1BgPtr { get; private set; }
        protected MatrixElement MOS1DPgPtr { get; private set; }
        protected MatrixElement MOS1SPgPtr { get; private set; }
        protected MatrixElement MOS1SPsPtr { get; private set; }
        protected MatrixElement MOS1DPbPtr { get; private set; }
        protected MatrixElement MOS1SPbPtr { get; private set; }
        protected MatrixElement MOS1SPdpPtr { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public LoadBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Setup behavior
        /// </summary>
        /// <param name="provider">Data provider</param>
        public override void Setup(SetupDataProvider provider)
        {
            // Get parameters
            bp = provider.GetParameters<BaseParameters>();
            mbp = provider.GetParameters<ModelBaseParameters>(1);

            // Get behaviors
            temp = provider.GetBehavior<TemperatureBehavior>();
            modeltemp = provider.GetBehavior<ModelTemperatureBehavior>(1);
        }

        /// <summary>
        /// Connect
        /// </summary>
        /// <param name="pins">Pins</param>
        public void Connect(params int[] pins)
        {
            MOS1dNode = pins[0];
            MOS1gNode = pins[1];
            MOS1sNode = pins[2];
            MOS1bNode = pins[3];
        }

        /// <summary>
        /// Get matrix pointers
        /// </summary>
        /// <param name="nodes">Nodes</param>
        /// <param name="matrix">Matrix</param>
        public override void GetMatrixPointers(Nodes nodes, Matrix matrix)
        {
            // Add series drain node if necessary
            if (mbp.MOS1drainResistance != 0 || (mbp.MOS1sheetResistance != 0 && bp.MOS1drainSquares != 0))
                MOS1dNodePrime = nodes.Create(Name.Grow("#drain")).Index;
            else
                MOS1dNodePrime = MOS1dNode;

            // Add series source node if necessary
            if (mbp.MOS1sourceResistance != 0 || (mbp.MOS1sheetResistance != 0 && bp.MOS1sourceSquares != 0))
                MOS1sNodePrime = nodes.Create(Name.Grow("#source")).Index;
            else
                MOS1sNodePrime = MOS1sNode;

            // Get matrix pointers
            MOS1DdPtr = matrix.GetElement(MOS1dNode, MOS1dNode);
            MOS1GgPtr = matrix.GetElement(MOS1gNode, MOS1gNode);
            MOS1SsPtr = matrix.GetElement(MOS1sNode, MOS1sNode);
            MOS1BbPtr = matrix.GetElement(MOS1bNode, MOS1bNode);
            MOS1DPdpPtr = matrix.GetElement(MOS1dNodePrime, MOS1dNodePrime);
            MOS1SPspPtr = matrix.GetElement(MOS1sNodePrime, MOS1sNodePrime);
            MOS1DdpPtr = matrix.GetElement(MOS1dNode, MOS1dNodePrime);
            MOS1GbPtr = matrix.GetElement(MOS1gNode, MOS1bNode);
            MOS1GdpPtr = matrix.GetElement(MOS1gNode, MOS1dNodePrime);
            MOS1GspPtr = matrix.GetElement(MOS1gNode, MOS1sNodePrime);
            MOS1SspPtr = matrix.GetElement(MOS1sNode, MOS1sNodePrime);
            MOS1BdpPtr = matrix.GetElement(MOS1bNode, MOS1dNodePrime);
            MOS1BspPtr = matrix.GetElement(MOS1bNode, MOS1sNodePrime);
            MOS1DPspPtr = matrix.GetElement(MOS1dNodePrime, MOS1sNodePrime);
            MOS1DPdPtr = matrix.GetElement(MOS1dNodePrime, MOS1dNode);
            MOS1BgPtr = matrix.GetElement(MOS1bNode, MOS1gNode);
            MOS1DPgPtr = matrix.GetElement(MOS1dNodePrime, MOS1gNode);
            MOS1SPgPtr = matrix.GetElement(MOS1sNodePrime, MOS1gNode);
            MOS1SPsPtr = matrix.GetElement(MOS1sNodePrime, MOS1sNode);
            MOS1DPbPtr = matrix.GetElement(MOS1dNodePrime, MOS1bNode);
            MOS1SPbPtr = matrix.GetElement(MOS1sNodePrime, MOS1bNode);
            MOS1SPdpPtr = matrix.GetElement(MOS1sNodePrime, MOS1dNodePrime);
        }

        /// <summary>
        /// Unsetup
        /// </summary>
        public override void Unsetup()
        {
            // Remove references
            MOS1DdPtr = null;
            MOS1GgPtr = null;
            MOS1SsPtr = null;
            MOS1BbPtr = null;
            MOS1DPdpPtr = null;
            MOS1SPspPtr = null;
            MOS1DdpPtr = null;
            MOS1GbPtr = null;
            MOS1GdpPtr = null;
            MOS1GspPtr = null;
            MOS1SspPtr = null;
            MOS1BdpPtr = null;
            MOS1BspPtr = null;
            MOS1DPspPtr = null;
            MOS1DPdPtr = null;
            MOS1BgPtr = null;
            MOS1DPgPtr = null;
            MOS1SPgPtr = null;
            MOS1SPsPtr = null;
            MOS1DPbPtr = null;
            MOS1SPbPtr = null;
            MOS1SPdpPtr = null;
        }

        /// <summary>
        /// Execute behavior
        /// </summary>
        /// <param name="sim">Base simulation</param>
        public override void Load(BaseSimulation sim)
        {
            var state = sim.State;
            double vt, EffectiveLength, DrainSatCur, SourceSatCur, Beta,
                vgs, vds, vbs, vbd, vgb, vgd, vgdo, von, evbs, evbd, ceqbs, ceqbd,
                vdsat, cdrain, cdreq;
            int Check, xnrm, xrev;

            vt = Circuit.CONSTKoverQ * bp.MOS1temp;
            Check = 1;

            /* DETAILPROF */

            /* first, we compute a few useful values - these could be
			 * pre - computed, but for historical reasons are still done
			 * here.  They may be moved at the expense of instance size
			 */
            EffectiveLength = bp.MOS1l - 2 * mbp.MOS1latDiff;
            if ((temp.MOS1tSatCurDens == 0) || (bp.MOS1drainArea.Value == 0) || (bp.MOS1sourceArea.Value == 0))
            {
                DrainSatCur = temp.MOS1tSatCur;
                SourceSatCur = temp.MOS1tSatCur;
            }
            else
            {
                DrainSatCur = temp.MOS1tSatCurDens * bp.MOS1drainArea;
                SourceSatCur = temp.MOS1tSatCurDens * bp.MOS1sourceArea;
            }

            Beta = temp.MOS1tTransconductance * bp.MOS1w / EffectiveLength;
            /* 
			 * ok - now to do the start - up operations
			 * 
			 * we must get values for vbs, vds, and vgs from somewhere
			 * so we either predict them or recover them from last iteration
			 * These are the two most common cases - either a prediction
			 * step or the general iteration step and they
			 * share some code, so we put them first - others later on
			 */
            if ((state.Init == State.InitFlags.InitFloat || state.UseSmallSignal || (state.Init == State.InitFlags.InitTransient)) ||
                ((state.Init == State.InitFlags.InitFix) && (!bp.MOS1off)))
            {
                // general iteration
                vbs = mbp.MOS1type * (state.Solution[MOS1bNode] - state.Solution[MOS1sNodePrime]);
                vgs = mbp.MOS1type * (state.Solution[MOS1gNode] - state.Solution[MOS1sNodePrime]);
                vds = mbp.MOS1type * (state.Solution[MOS1dNodePrime] - state.Solution[MOS1sNodePrime]);

                /* now some common crunching for some more useful quantities */
                vbd = vbs - vds;
                vgd = vgs - vds;
                vgdo = MOS1vgs - MOS1vds;
                von = mbp.MOS1type * MOS1von;

                /* 
				 * limiting
				 * we want to keep device voltages from changing
				 * so fast that the exponentials churn out overflows
				 * and similar rudeness
				 */

                if (MOS1vds >= 0)
                {
                    vgs = Transistor.DEVfetlim(vgs, MOS1vgs, von);
                    vds = vgs - vgd;
                    vds = Transistor.DEVlimvds(vds, MOS1vds);
                    vgd = vgs - vds;
                }
                else
                {
                    vgd = Transistor.DEVfetlim(vgd, vgdo, von);
                    vds = vgs - vgd;
                    vds = -Transistor.DEVlimvds(-vds, -MOS1vds);
                    vgs = vgd + vds;
                }
                if (vds >= 0)
                {
                    vbs = Transistor.DEVpnjlim(vbs, MOS1vbs, vt, temp.MOS1sourceVcrit, ref Check);
                    vbd = vbs - vds;
                }
                else
                {
                    vbd = Transistor.DEVpnjlim(vbd, MOS1vbd, vt, temp.MOS1drainVcrit, ref Check);
                    vbs = vbd + vds;
                }
            }
            else
            {
                /* ok - not one of the simple cases, so we have to
				 * look at all of the possibilities for why we were
				 * called.  We still just initialize the three voltages
				 */

                if ((state.Init == State.InitFlags.InitJct) && !bp.MOS1off)
                {
                    vds = mbp.MOS1type * bp.MOS1icVDS;
                    vgs = mbp.MOS1type * bp.MOS1icVGS;
                    vbs = mbp.MOS1type * bp.MOS1icVBS;
                    if ((vds == 0) && (vgs == 0) && (vbs == 0) && ((state.UseDC ||
                        state.Domain == State.DomainTypes.None) || (!state.UseIC)))
                    {
                        vbs = -1;
                        vgs = mbp.MOS1type * temp.MOS1tVto;
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
                MOS1gbs = SourceSatCur / vt;
                MOS1cbs = MOS1gbs * vbs;
                MOS1gbs += state.Gmin;
            }
            else
            {
                evbs = Math.Exp(Math.Min(Transistor.MAX_EXP_ARG, vbs / vt));
                MOS1gbs = SourceSatCur * evbs / vt + state.Gmin;
                MOS1cbs = SourceSatCur * (evbs - 1);
            }
            if (vbd <= 0)
            {
                MOS1gbd = DrainSatCur / vt;
                MOS1cbd = MOS1gbd * vbd;
                MOS1gbd += state.Gmin;
            }
            else
            {
                evbd = Math.Exp(Math.Min(Transistor.MAX_EXP_ARG, vbd / vt));
                MOS1gbd = DrainSatCur * evbd / vt + state.Gmin;
                MOS1cbd = DrainSatCur * (evbd - 1);
            }

            /* now to determine whether the user was able to correctly
			 * identify the source and drain of his device
			 */
            if (vds >= 0)
            {
                /* normal mode */
                MOS1mode = 1;
            }
            else
            {
                /* inverse mode */
                MOS1mode = -1;
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

                if ((MOS1mode > 0 ? vbs : vbd) <= 0)
                {
                    sarg = Math.Sqrt(temp.MOS1tPhi - (MOS1mode > 0 ? vbs : vbd));
                }
                else
                {
                    sarg = Math.Sqrt(temp.MOS1tPhi);
                    sarg = sarg - (MOS1mode > 0 ? vbs : vbd) / (sarg + sarg);
                    sarg = Math.Max(0, sarg);
                }
                von = (temp.MOS1tVbi * mbp.MOS1type) + mbp.MOS1gamma * sarg;
                vgst = (MOS1mode > 0 ? vgs : vgd) - von;
                vdsat = Math.Max(vgst, 0);
                if (sarg <= 0)
                {
                    arg = 0;
                }
                else
                {
                    arg = mbp.MOS1gamma / (sarg + sarg);
                }
                if (vgst <= 0)
                {
                    /* 
					 * cutoff region
					 */
                    cdrain = 0;
                    MOS1gm = 0;
                    MOS1gds = 0;
                    MOS1gmbs = 0;
                }
                else
                {
                    /* 
					 * saturation region
					 */
                    betap = Beta * (1 + mbp.MOS1lambda * (vds * MOS1mode));
                    if (vgst <= (vds * MOS1mode))
                    {
                        cdrain = betap * vgst * vgst * .5;
                        MOS1gm = betap * vgst;
                        MOS1gds = mbp.MOS1lambda * Beta * vgst * vgst * .5;
                        MOS1gmbs = MOS1gm * arg;
                    }
                    else
                    {
                        /* 
						* linear region
						*/
                        cdrain = betap * (vds * MOS1mode) * (vgst - .5 * (vds * MOS1mode));
                        MOS1gm = betap * (vds * MOS1mode);
                        MOS1gds = betap * (vgst - (vds * MOS1mode)) + mbp.MOS1lambda * Beta * (vds * MOS1mode) * (vgst - .5 * (vds * MOS1mode));
                        MOS1gmbs = MOS1gm * arg;
                    }
                }
                /* 
				 * finished
				 */
            }

            /* now deal with n vs p polarity */
            MOS1von = mbp.MOS1type * von;
            MOS1vdsat = mbp.MOS1type * vdsat;
            /* line 490 */
            /* 
			 * COMPUTE EQUIVALENT DRAIN CURRENT SOURCE
			 */
            MOS1cd = MOS1mode * cdrain - MOS1cbd;

            /* 
			 * check convergence
			 */
            if (!bp.MOS1off || (!(state.Init == State.InitFlags.InitFix || state.UseSmallSignal)))
            {
                if (Check == 1)
                    state.IsCon = false;
            }

            /* DETAILPROF */

            /* save things away for next time */
            MOS1vbs = vbs;
            MOS1vbd = vbd;
            MOS1vgs = vgs;
            MOS1vds = vds;

            /* 
			 * load current vector
			 */
            ceqbs = mbp.MOS1type * (MOS1cbs - (MOS1gbs - state.Gmin) * vbs);
            ceqbd = mbp.MOS1type * (MOS1cbd - (MOS1gbd - state.Gmin) * vbd);
            if (MOS1mode >= 0)
            {
                xnrm = 1;
                xrev = 0;
                cdreq = mbp.MOS1type * (cdrain - MOS1gds * vds - MOS1gm * vgs - MOS1gmbs * vbs);
            }
            else
            {
                xnrm = 0;
                xrev = 1;
                cdreq = -(mbp.MOS1type) * (cdrain - MOS1gds * (-vds) - MOS1gm * vgd - MOS1gmbs * vbd);
            }
            state.Rhs[MOS1bNode] -= (ceqbs + ceqbd);
            state.Rhs[MOS1dNodePrime] += (ceqbd - cdreq);
            state.Rhs[MOS1sNodePrime] += cdreq + ceqbs;

            /* 
			 * load y matrix
			 */
            MOS1DdPtr.Add(temp.MOS1drainConductance);
            MOS1SsPtr.Add(temp.MOS1sourceConductance);
            MOS1BbPtr.Add(MOS1gbd + MOS1gbs);
            MOS1DPdpPtr.Add(temp.MOS1drainConductance + MOS1gds + MOS1gbd + xrev * (MOS1gm + MOS1gmbs));
            MOS1SPspPtr.Add(temp.MOS1sourceConductance + MOS1gds + MOS1gbs + xnrm * (MOS1gm + MOS1gmbs));
            MOS1DdpPtr.Add(-temp.MOS1drainConductance);
            MOS1SspPtr.Add(-temp.MOS1sourceConductance);
            MOS1BdpPtr.Sub(MOS1gbd);
            MOS1BspPtr.Sub(MOS1gbs);
            MOS1DPdPtr.Add(-temp.MOS1drainConductance);
            MOS1DPgPtr.Add((xnrm - xrev) * MOS1gm);
            MOS1DPbPtr.Add(-MOS1gbd + (xnrm - xrev) * MOS1gmbs);
            MOS1DPspPtr.Add(-MOS1gds - xnrm * (MOS1gm + MOS1gmbs));
            MOS1SPgPtr.Add(-(xnrm - xrev) * MOS1gm);
            MOS1SPsPtr.Add(-temp.MOS1sourceConductance);
            MOS1SPbPtr.Add(-MOS1gbs - (xnrm - xrev) * MOS1gmbs);
            MOS1SPdpPtr.Add(-MOS1gds - xrev * (MOS1gm + MOS1gmbs));
        }

        /// <summary>
        /// Test convergence
        /// </summary>
        /// <param name="sim">Base simulation</param>
        /// <returns></returns>
        public override bool IsConvergent(BaseSimulation sim)
        {
            var state = sim.State;
            var config = sim.CurrentConfig;

            double vbs, vgs, vds, vbd, vgd, vgdo, delvbs, delvbd, delvgs, delvds, delvgd, cdhat, cbhat;

            vbs = mbp.MOS1type * (state.Solution[MOS1bNode] - state.Solution[MOS1sNodePrime]);
            vgs = mbp.MOS1type * (state.Solution[MOS1gNode] - state.Solution[MOS1sNodePrime]);
            vds = mbp.MOS1type * (state.Solution[MOS1dNodePrime] - state.Solution[MOS1sNodePrime]);
            vbd = vbs - vds;
            vgd = vgs - vds;
            vgdo = MOS1vgs - MOS1vds;
            delvbs = vbs - MOS1vbs;
            delvbd = vbd - MOS1vbd;
            delvgs = vgs - MOS1vgs;
            delvds = vds - MOS1vds;
            delvgd = vgd - vgdo;

            // these are needed for convergence testing
            // NOTE: MOS1cd does not include contributions for transient simulations... Should check for a way to include them!
            if (MOS1mode >= 0)
            {
                cdhat = MOS1cd - MOS1gbd * delvbd + MOS1gmbs * delvbs +
                    MOS1gm * delvgs + MOS1gds * delvds;
            }
            else
            {
                cdhat = MOS1cd - (MOS1gbd - MOS1gmbs) * delvbd -
                    MOS1gm * delvgd + MOS1gds * delvds;
            }
            cbhat = MOS1cbs + MOS1cbd + MOS1gbd * delvbd + MOS1gbs * delvbs;

            /*
             *  check convergence
             */
            // NOTE: relative and absolute tolerances need to be gotten from the configuration, temporarely set to constants here
            double tol = config.RelTol * Math.Max(Math.Abs(cdhat), Math.Abs(MOS1cd)) + config.AbsTol;
            if (Math.Abs(cdhat - MOS1cd) >= tol)
            {
                state.IsCon = false;
                return false;
            }

            tol = config.RelTol * Math.Max(Math.Abs(cbhat), Math.Abs(MOS1cbs + MOS1cbd)) + config.AbsTol;
            if (Math.Abs(cbhat - (MOS1cbs + MOS1cbd)) > tol)
            {
                state.IsCon = false;
                return false;
            }
            return true;
        }
    }
}
