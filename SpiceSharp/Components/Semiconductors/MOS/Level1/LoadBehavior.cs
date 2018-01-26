using System;
using SpiceSharp.Circuits;
using SpiceSharp.Attributes;
using SpiceSharp.Sparse;
using SpiceSharp.Simulations;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.MosfetBehaviors.Level1
{
    /// <summary>
    /// General behavior for a <see cref="MOS1"/>
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
        int dNode, gNode, sNode, bNode;
        public int DrainNodePrime { get; protected set; }
        public int SourceNodePrime { get; protected set; }

        [PropertyName("von"), PropertyInfo(" ")]
        public double Von { get; protected set; } = 0.0;
        [PropertyName("vdsat"), PropertyInfo("Saturation drain voltage")]
        public double Vdsat { get; protected set; } = 0.0;
        [PropertyName("id"), PropertyInfo("Drain current")]
        public double Cd { get; protected set; }
        [PropertyName("ibs"), PropertyInfo("B-S junction current")]
        public double Cbs { get; protected set; }
        [PropertyName("ibd"), PropertyInfo("B-D junction current")]
        public double Cbd { get; protected set; }
        [PropertyName("gmb"), PropertyName("gmbs"), PropertyInfo("Bulk-Source transconductance")]
        public double Gmbs { get; protected set; }
        [PropertyName("gm"), PropertyInfo("Transconductance")]
        public double Gm { get; protected set; }
        [PropertyName("gds"), PropertyInfo("Drain-Source conductance")]
        public double Gds { get; protected set; }
        [PropertyName("gbd"), PropertyInfo("Bulk-Drain conductance")]
        public double Gbd { get; protected set; }
        [PropertyName("gbs"), PropertyInfo("Bulk-Source conductance")]
        public double Gbs { get; protected set; }

        /// <summary>
        /// Extra variables
        /// </summary>
        public double Mode { get; protected set; }

        public double Vbd { get; protected set; }
        public double Vbs { get; protected set; }
        public double Vgs { get; protected set; }
        public double Vds { get; protected set; }

        /// <summary>
        /// Matrix elements
        /// </summary>
        protected MatrixElement DdPtr { get; private set; }
        protected MatrixElement GgPtr { get; private set; }
        protected MatrixElement SsPtr { get; private set; }
        protected MatrixElement BbPtr { get; private set; }
        protected MatrixElement DPdpPtr { get; private set; }
        protected MatrixElement SPspPtr { get; private set; }
        protected MatrixElement DdpPtr { get; private set; }
        protected MatrixElement GbPtr { get; private set; }
        protected MatrixElement GdpPtr { get; private set; }
        protected MatrixElement GspPtr { get; private set; }
        protected MatrixElement SspPtr { get; private set; }
        protected MatrixElement BdpPtr { get; private set; }
        protected MatrixElement BspPtr { get; private set; }
        protected MatrixElement DPspPtr { get; private set; }
        protected MatrixElement DPdPtr { get; private set; }
        protected MatrixElement BgPtr { get; private set; }
        protected MatrixElement DPgPtr { get; private set; }
        protected MatrixElement SPgPtr { get; private set; }
        protected MatrixElement SPsPtr { get; private set; }
        protected MatrixElement DPbPtr { get; private set; }
        protected MatrixElement SPbPtr { get; private set; }
        protected MatrixElement SPdpPtr { get; private set; }

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
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            // Get parameters
            bp = provider.GetParameterSet<BaseParameters>(0);
            mbp = provider.GetParameterSet<ModelBaseParameters>(1);

            // Get behaviors
            temp = provider.GetBehavior<TemperatureBehavior>(0);
            modeltemp = provider.GetBehavior<ModelTemperatureBehavior>(1);
        }

        /// <summary>
        /// Connect
        /// </summary>
        /// <param name="pins">Pins</param>
        public void Connect(params int[] pins)
        {
            if (pins == null)
                throw new ArgumentNullException(nameof(pins));
            if (pins.Length != 4)
                throw new Diagnostics.CircuitException($"Pin count mismatch: 4 pins expected, {pins.Length} given");
            dNode = pins[0];
            gNode = pins[1];
            sNode = pins[2];
            bNode = pins[3];
        }

        /// <summary>
        /// Get matrix pointers
        /// </summary>
        /// <param name="nodes">Nodes</param>
        /// <param name="matrix">Matrix</param>
        public override void GetMatrixPointers(Nodes nodes, Matrix matrix)
        {
            if (nodes == null)
                throw new ArgumentNullException(nameof(nodes));
            if (matrix == null)
                throw new ArgumentNullException(nameof(matrix));

            // Add series drain node if necessary
            if (mbp.DrainResistance > 0 || (mbp.SheetResistance > 0 && bp.DrainSquares > 0))
                DrainNodePrime = nodes.Create(Name.Grow("#drain")).Index;
            else
                DrainNodePrime = dNode;

            // Add series source node if necessary
            if (mbp.SourceResistance > 0 || (mbp.SheetResistance > 0 && bp.SourceSquares > 0))
                SourceNodePrime = nodes.Create(Name.Grow("#source")).Index;
            else
                SourceNodePrime = sNode;

            // Get matrix pointers
            DdPtr = matrix.GetElement(dNode, dNode);
            GgPtr = matrix.GetElement(gNode, gNode);
            SsPtr = matrix.GetElement(sNode, sNode);
            BbPtr = matrix.GetElement(bNode, bNode);
            DPdpPtr = matrix.GetElement(DrainNodePrime, DrainNodePrime);
            SPspPtr = matrix.GetElement(SourceNodePrime, SourceNodePrime);
            DdpPtr = matrix.GetElement(dNode, DrainNodePrime);
            GbPtr = matrix.GetElement(gNode, bNode);
            GdpPtr = matrix.GetElement(gNode, DrainNodePrime);
            GspPtr = matrix.GetElement(gNode, SourceNodePrime);
            SspPtr = matrix.GetElement(sNode, SourceNodePrime);
            BdpPtr = matrix.GetElement(bNode, DrainNodePrime);
            BspPtr = matrix.GetElement(bNode, SourceNodePrime);
            DPspPtr = matrix.GetElement(DrainNodePrime, SourceNodePrime);
            DPdPtr = matrix.GetElement(DrainNodePrime, dNode);
            BgPtr = matrix.GetElement(bNode, gNode);
            DPgPtr = matrix.GetElement(DrainNodePrime, gNode);
            SPgPtr = matrix.GetElement(SourceNodePrime, gNode);
            SPsPtr = matrix.GetElement(SourceNodePrime, sNode);
            DPbPtr = matrix.GetElement(DrainNodePrime, bNode);
            SPbPtr = matrix.GetElement(SourceNodePrime, bNode);
            SPdpPtr = matrix.GetElement(SourceNodePrime, DrainNodePrime);
        }

        /// <summary>
        /// Unsetup
        /// </summary>
        public override void Unsetup()
        {
            // Remove references
            DdPtr = null;
            GgPtr = null;
            SsPtr = null;
            BbPtr = null;
            DPdpPtr = null;
            SPspPtr = null;
            DdpPtr = null;
            GbPtr = null;
            GdpPtr = null;
            GspPtr = null;
            SspPtr = null;
            BdpPtr = null;
            BspPtr = null;
            DPspPtr = null;
            DPdPtr = null;
            BgPtr = null;
            DPgPtr = null;
            SPgPtr = null;
            SPsPtr = null;
            DPbPtr = null;
            SPbPtr = null;
            SPdpPtr = null;
        }

        /// <summary>
        /// Execute behavior
        /// </summary>
        /// <param name="simulation">Base simulation</param>
        public override void Load(BaseSimulation simulation)
        {
            if (simulation == null)
                throw new ArgumentNullException(nameof(simulation));

            var state = simulation.State;
            double vt, EffectiveLength, DrainSatCur, SourceSatCur, Beta,
                vgs, vds, vbs, vbd, vgb, vgd, vgdo, von, evbs, evbd, ceqbs, ceqbd,
                vdsat, cdrain, cdreq;
            int Check, xnrm, xrev;

            vt = Circuit.KOverQ * bp.Temperature;
            Check = 1;

            /* DETAILPROF */

            /* first, we compute a few useful values - these could be
			 * pre - computed, but for historical reasons are still done
			 * here.  They may be moved at the expense of instance size
			 */
            EffectiveLength = bp.Length - 2 * mbp.LatDiff;
            if ((temp.TSatCurDens == 0) || (bp.DrainArea.Value == 0) || (bp.SourceArea.Value == 0))
            {
                DrainSatCur = temp.TSatCur;
                SourceSatCur = temp.TSatCur;
            }
            else
            {
                DrainSatCur = temp.TSatCurDens * bp.DrainArea;
                SourceSatCur = temp.TSatCurDens * bp.SourceArea;
            }

            Beta = temp.TTransconductance * bp.Width / EffectiveLength;
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
                ((state.Init == State.InitFlags.InitFix) && (!bp.Off)))
            {
                // general iteration
                vbs = mbp.Type * (state.Solution[bNode] - state.Solution[SourceNodePrime]);
                vgs = mbp.Type * (state.Solution[gNode] - state.Solution[SourceNodePrime]);
                vds = mbp.Type * (state.Solution[DrainNodePrime] - state.Solution[SourceNodePrime]);

                /* now some common crunching for some more useful quantities */
                vbd = vbs - vds;
                vgd = vgs - vds;
                vgdo = Vgs - Vds;
                von = mbp.Type * Von;

                /* 
				 * limiting
				 * we want to keep device voltages from changing
				 * so fast that the exponentials churn out overflows
				 * and similar rudeness
				 */

                if (Vds >= 0)
                {
                    vgs = Transistor.DEVfetlim(vgs, Vgs, von);
                    vds = vgs - vgd;
                    vds = Transistor.DEVlimvds(vds, Vds);
                    vgd = vgs - vds;
                }
                else
                {
                    vgd = Transistor.DEVfetlim(vgd, vgdo, von);
                    vds = vgs - vgd;
                    vds = -Transistor.DEVlimvds(-vds, -Vds);
                    vgs = vgd + vds;
                }
                if (vds >= 0)
                {
                    vbs = Transistor.DEVpnjlim(vbs, Vbs, vt, temp.SourceVcrit, ref Check);
                    vbd = vbs - vds;
                }
                else
                {
                    vbd = Transistor.DEVpnjlim(vbd, Vbd, vt, temp.DrainVcrit, ref Check);
                    vbs = vbd + vds;
                }
            }
            else
            {
                /* ok - not one of the simple cases, so we have to
				 * look at all of the possibilities for why we were
				 * called.  We still just initialize the three voltages
				 */

                if ((state.Init == State.InitFlags.InitJct) && !bp.Off)
                {
                    vds = mbp.Type * bp.InitialVDS;
                    vgs = mbp.Type * bp.InitialVGS;
                    vbs = mbp.Type * bp.InitialVBS;
                    if ((vds == 0) && (vgs == 0) && (vbs == 0) && ((state.UseDC ||
                        state.Domain == State.DomainTypes.None) || (!state.UseIC)))
                    {
                        vbs = -1;
                        vgs = mbp.Type * temp.TVto;
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
                Gbs = SourceSatCur / vt;
                Cbs = Gbs * vbs;
                Gbs += state.Gmin;
            }
            else
            {
                evbs = Math.Exp(Math.Min(Transistor.MAX_EXP_ARG, vbs / vt));
                Gbs = SourceSatCur * evbs / vt + state.Gmin;
                Cbs = SourceSatCur * (evbs - 1);
            }
            if (vbd <= 0)
            {
                Gbd = DrainSatCur / vt;
                Cbd = Gbd * vbd;
                Gbd += state.Gmin;
            }
            else
            {
                evbd = Math.Exp(Math.Min(Transistor.MAX_EXP_ARG, vbd / vt));
                Gbd = DrainSatCur * evbd / vt + state.Gmin;
                Cbd = DrainSatCur * (evbd - 1);
            }

            /* now to determine whether the user was able to correctly
			 * identify the source and drain of his device
			 */
            if (vds >= 0)
            {
                /* normal mode */
                Mode = 1;
            }
            else
            {
                /* inverse mode */
                Mode = -1;
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

                if ((Mode > 0 ? vbs : vbd) <= 0)
                {
                    sarg = Math.Sqrt(temp.TPhi - (Mode > 0 ? vbs : vbd));
                }
                else
                {
                    sarg = Math.Sqrt(temp.TPhi);
                    sarg = sarg - (Mode > 0 ? vbs : vbd) / (sarg + sarg);
                    sarg = Math.Max(0, sarg);
                }
                von = (temp.TVbi * mbp.Type) + mbp.Gamma * sarg;
                vgst = (Mode > 0 ? vgs : vgd) - von;
                vdsat = Math.Max(vgst, 0);
                if (sarg <= 0)
                {
                    arg = 0;
                }
                else
                {
                    arg = mbp.Gamma / (sarg + sarg);
                }
                if (vgst <= 0)
                {
                    /* 
					 * cutoff region
					 */
                    cdrain = 0;
                    Gm = 0;
                    Gds = 0;
                    Gmbs = 0;
                }
                else
                {
                    /* 
					 * saturation region
					 */
                    betap = Beta * (1 + mbp.Lambda * (vds * Mode));
                    if (vgst <= (vds * Mode))
                    {
                        cdrain = betap * vgst * vgst * .5;
                        Gm = betap * vgst;
                        Gds = mbp.Lambda * Beta * vgst * vgst * .5;
                        Gmbs = Gm * arg;
                    }
                    else
                    {
                        /* 
						* linear region
						*/
                        cdrain = betap * (vds * Mode) * (vgst - .5 * (vds * Mode));
                        Gm = betap * (vds * Mode);
                        Gds = betap * (vgst - (vds * Mode)) + mbp.Lambda * Beta * (vds * Mode) * (vgst - .5 * (vds * Mode));
                        Gmbs = Gm * arg;
                    }
                }
                /* 
				 * finished
				 */
            }

            /* now deal with n vs p polarity */
            Von = mbp.Type * von;
            Vdsat = mbp.Type * vdsat;
            /* line 490 */
            /* 
			 * COMPUTE EQUIVALENT DRAIN CURRENT SOURCE
			 */
            Cd = Mode * cdrain - Cbd;

            /* 
			 * check convergence
			 */
            if (!bp.Off || (!(state.Init == State.InitFlags.InitFix || state.UseSmallSignal)))
            {
                if (Check == 1)
                    state.IsCon = false;
            }

            /* DETAILPROF */

            /* save things away for next time */
            Vbs = vbs;
            Vbd = vbd;
            Vgs = vgs;
            Vds = vds;

            /* 
			 * load current vector
			 */
            ceqbs = mbp.Type * (Cbs - (Gbs - state.Gmin) * vbs);
            ceqbd = mbp.Type * (Cbd - (Gbd - state.Gmin) * vbd);
            if (Mode >= 0)
            {
                xnrm = 1;
                xrev = 0;
                cdreq = mbp.Type * (cdrain - Gds * vds - Gm * vgs - Gmbs * vbs);
            }
            else
            {
                xnrm = 0;
                xrev = 1;
                cdreq = -(mbp.Type) * (cdrain - Gds * (-vds) - Gm * vgd - Gmbs * vbd);
            }
            state.Rhs[bNode] -= (ceqbs + ceqbd);
            state.Rhs[DrainNodePrime] += (ceqbd - cdreq);
            state.Rhs[SourceNodePrime] += cdreq + ceqbs;

            /* 
			 * load y matrix
			 */
            DdPtr.Add(temp.DrainConductance);
            SsPtr.Add(temp.SourceConductance);
            BbPtr.Add(Gbd + Gbs);
            DPdpPtr.Add(temp.DrainConductance + Gds + Gbd + xrev * (Gm + Gmbs));
            SPspPtr.Add(temp.SourceConductance + Gds + Gbs + xnrm * (Gm + Gmbs));
            DdpPtr.Add(-temp.DrainConductance);
            SspPtr.Add(-temp.SourceConductance);
            BdpPtr.Sub(Gbd);
            BspPtr.Sub(Gbs);
            DPdPtr.Add(-temp.DrainConductance);
            DPgPtr.Add((xnrm - xrev) * Gm);
            DPbPtr.Add(-Gbd + (xnrm - xrev) * Gmbs);
            DPspPtr.Add(-Gds - xnrm * (Gm + Gmbs));
            SPgPtr.Add(-(xnrm - xrev) * Gm);
            SPsPtr.Add(-temp.SourceConductance);
            SPbPtr.Add(-Gbs - (xnrm - xrev) * Gmbs);
            SPdpPtr.Add(-Gds - xrev * (Gm + Gmbs));
        }

        /// <summary>
        /// Test convergence
        /// </summary>
        /// <param name="simulation">Base simulation</param>
        /// <returns></returns>
        public override bool IsConvergent(BaseSimulation simulation)
        {
			if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));

            var state = simulation.State;
            var config = simulation.BaseConfiguration;

            double vbs, vgs, vds, vbd, vgd, vgdo, delvbs, delvbd, delvgs, delvds, delvgd, cdhat, cbhat;

            vbs = mbp.Type * (state.Solution[bNode] - state.Solution[SourceNodePrime]);
            vgs = mbp.Type * (state.Solution[gNode] - state.Solution[SourceNodePrime]);
            vds = mbp.Type * (state.Solution[DrainNodePrime] - state.Solution[SourceNodePrime]);
            vbd = vbs - vds;
            vgd = vgs - vds;
            vgdo = Vgs - Vds;
            delvbs = vbs - Vbs;
            delvbd = vbd - Vbd;
            delvgs = vgs - Vgs;
            delvds = vds - Vds;
            delvgd = vgd - vgdo;

            // these are needed for convergence testing
            // NOTE: Cd does not include contributions for transient simulations... Should check for a way to include them!
            if (Mode >= 0)
            {
                cdhat = Cd - Gbd * delvbd + Gmbs * delvbs +
                    Gm * delvgs + Gds * delvds;
            }
            else
            {
                cdhat = Cd - (Gbd - Gmbs) * delvbd -
                    Gm * delvgd + Gds * delvds;
            }
            cbhat = Cbs + Cbd + Gbd * delvbd + Gbs * delvbs;

            /*
             *  check convergence
             */
            // NOTE: relative and absolute tolerances need to be gotten from the configuration, temporarely set to constants here
            double tol = config.RelTol * Math.Max(Math.Abs(cdhat), Math.Abs(Cd)) + config.AbsTol;
            if (Math.Abs(cdhat - Cd) >= tol)
            {
                state.IsCon = false;
                return false;
            }

            tol = config.RelTol * Math.Max(Math.Abs(cbhat), Math.Abs(Cbs + Cbd)) + config.AbsTol;
            if (Math.Abs(cbhat - (Cbs + Cbd)) > tol)
            {
                state.IsCon = false;
                return false;
            }
            return true;
        }
    }
}
