using System;
using SpiceSharp.Circuits;
using SpiceSharp.Attributes;
using SpiceSharp.Sparse;
using SpiceSharp.Simulations;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.MosfetBehaviors.Level1
{
    /// <summary>
    /// General behavior for a <see cref="Mosfet1"/>
    /// </summary>
    public class LoadBehavior : Behaviors.LoadBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        BaseParameters bp;
        TemperatureBehavior temp;
        ModelBaseParameters mbp;

        /// <summary>
        /// Nodes
        /// </summary>
        int drainNode, gateNode, sourceNode, bulkNode;
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
        protected MatrixElement DrainDrainPtr { get; private set; }
        protected MatrixElement GateGatePtr { get; private set; }
        protected MatrixElement SourceSourcePtr { get; private set; }
        protected MatrixElement BulkBulkPtr { get; private set; }
        protected MatrixElement DrainPrimeDrainPrimePtr { get; private set; }
        protected MatrixElement SourcePrimeSourcePrimePtr { get; private set; }
        protected MatrixElement DrainDrainPrimePtr { get; private set; }
        protected MatrixElement GateBulkPtr { get; private set; }
        protected MatrixElement GateDrainPrimePtr { get; private set; }
        protected MatrixElement GateSourcePrimePtr { get; private set; }
        protected MatrixElement SourceSourcePrimePtr { get; private set; }
        protected MatrixElement BulkDrainPrimePtr { get; private set; }
        protected MatrixElement BulkSourcePrimePtr { get; private set; }
        protected MatrixElement DrainPrimeSourcePrimePtr { get; private set; }
        protected MatrixElement DrainPrimeDrainPtr { get; private set; }
        protected MatrixElement BulkGatePtr { get; private set; }
        protected MatrixElement DrainPrimeGatePtr { get; private set; }
        protected MatrixElement SourcePrimeGatePtr { get; private set; }
        protected MatrixElement SourcePrimeSourcePtr { get; private set; }
        protected MatrixElement DrainPrimeBulkPtr { get; private set; }
        protected MatrixElement SourcePrimeBulkPtr { get; private set; }
        protected MatrixElement SourcePrimeDrainPrimePtr { get; private set; }

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
                throw new Diagnostics.CircuitException("Pin count mismatch: 4 pins expected, {0} given".FormatString(pins.Length));
            drainNode = pins[0];
            gateNode = pins[1];
            sourceNode = pins[2];
            bulkNode = pins[3];
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
                DrainNodePrime = drainNode;

            // Add series source node if necessary
            if (mbp.SourceResistance > 0 || (mbp.SheetResistance > 0 && bp.SourceSquares > 0))
                SourceNodePrime = nodes.Create(Name.Grow("#source")).Index;
            else
                SourceNodePrime = sourceNode;

            // Get matrix pointers
            DrainDrainPtr = matrix.GetElement(drainNode, drainNode);
            GateGatePtr = matrix.GetElement(gateNode, gateNode);
            SourceSourcePtr = matrix.GetElement(sourceNode, sourceNode);
            BulkBulkPtr = matrix.GetElement(bulkNode, bulkNode);
            DrainPrimeDrainPrimePtr = matrix.GetElement(DrainNodePrime, DrainNodePrime);
            SourcePrimeSourcePrimePtr = matrix.GetElement(SourceNodePrime, SourceNodePrime);
            DrainDrainPrimePtr = matrix.GetElement(drainNode, DrainNodePrime);
            GateBulkPtr = matrix.GetElement(gateNode, bulkNode);
            GateDrainPrimePtr = matrix.GetElement(gateNode, DrainNodePrime);
            GateSourcePrimePtr = matrix.GetElement(gateNode, SourceNodePrime);
            SourceSourcePrimePtr = matrix.GetElement(sourceNode, SourceNodePrime);
            BulkDrainPrimePtr = matrix.GetElement(bulkNode, DrainNodePrime);
            BulkSourcePrimePtr = matrix.GetElement(bulkNode, SourceNodePrime);
            DrainPrimeSourcePrimePtr = matrix.GetElement(DrainNodePrime, SourceNodePrime);
            DrainPrimeDrainPtr = matrix.GetElement(DrainNodePrime, drainNode);
            BulkGatePtr = matrix.GetElement(bulkNode, gateNode);
            DrainPrimeGatePtr = matrix.GetElement(DrainNodePrime, gateNode);
            SourcePrimeGatePtr = matrix.GetElement(SourceNodePrime, gateNode);
            SourcePrimeSourcePtr = matrix.GetElement(SourceNodePrime, sourceNode);
            DrainPrimeBulkPtr = matrix.GetElement(DrainNodePrime, bulkNode);
            SourcePrimeBulkPtr = matrix.GetElement(SourceNodePrime, bulkNode);
            SourcePrimeDrainPrimePtr = matrix.GetElement(SourceNodePrime, DrainNodePrime);
        }

        /// <summary>
        /// Unsetup
        /// </summary>
        public override void Unsetup()
        {
            // Remove references
            DrainDrainPtr = null;
            GateGatePtr = null;
            SourceSourcePtr = null;
            BulkBulkPtr = null;
            DrainPrimeDrainPrimePtr = null;
            SourcePrimeSourcePrimePtr = null;
            DrainDrainPrimePtr = null;
            GateBulkPtr = null;
            GateDrainPrimePtr = null;
            GateSourcePrimePtr = null;
            SourceSourcePrimePtr = null;
            BulkDrainPrimePtr = null;
            BulkSourcePrimePtr = null;
            DrainPrimeSourcePrimePtr = null;
            DrainPrimeDrainPtr = null;
            BulkGatePtr = null;
            DrainPrimeGatePtr = null;
            SourcePrimeGatePtr = null;
            SourcePrimeSourcePtr = null;
            DrainPrimeBulkPtr = null;
            SourcePrimeBulkPtr = null;
            SourcePrimeDrainPrimePtr = null;
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
                vgs, vds, vbs, vbd, vgd, vgdo, von, evbs, evbd, ceqbs, ceqbd,
                vdsat, cdrain, cdreq;
            int Check, xnrm, xrev;

            vt = Circuit.KOverQ * bp.Temperature;
            Check = 1;

            /* DETAILPROF */

            /* first, we compute a few useful values - these could be
			 * pre - computed, but for historical reasons are still done
			 * here.  They may be moved at the expense of instance size
			 */
            EffectiveLength = bp.Length - 2 * mbp.LateralDiffusion;
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
            if ((state.Init == State.InitializationStates.InitFloat || state.UseSmallSignal || (state.Init == State.InitializationStates.InitTransient)) ||
                ((state.Init == State.InitializationStates.InitFix) && (!bp.Off)))
            {
                // general iteration
                vbs = mbp.MosfetType * (state.Solution[bulkNode] - state.Solution[SourceNodePrime]);
                vgs = mbp.MosfetType * (state.Solution[gateNode] - state.Solution[SourceNodePrime]);
                vds = mbp.MosfetType * (state.Solution[DrainNodePrime] - state.Solution[SourceNodePrime]);

                /* now some common crunching for some more useful quantities */
                vbd = vbs - vds;
                vgd = vgs - vds;
                vgdo = Vgs - Vds;
                von = mbp.MosfetType * Von;

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

                if ((state.Init == State.InitializationStates.InitJct) && !bp.Off)
                {
                    vds = mbp.MosfetType * bp.InitialVds;
                    vgs = mbp.MosfetType * bp.InitialVgs;
                    vbs = mbp.MosfetType * bp.InitialVbs;
                    if ((vds == 0) && (vgs == 0) && (vbs == 0) && ((state.UseDC ||
                        state.Domain == State.DomainType.None) || (!state.UseIC)))
                    {
                        vbs = -1;
                        vgs = mbp.MosfetType * temp.TVto;
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
                evbs = Math.Exp(Math.Min(Transistor.MaximumExponentArgument, vbs / vt));
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
                evbd = Math.Exp(Math.Min(Transistor.MaximumExponentArgument, vbd / vt));
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
                von = (temp.TVbi * mbp.MosfetType) + mbp.Gamma * sarg;
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
            Von = mbp.MosfetType * von;
            Vdsat = mbp.MosfetType * vdsat;
            /* line 490 */
            /* 
			 * COMPUTE EQUIVALENT DRAIN CURRENT SOURCE
			 */
            Cd = Mode * cdrain - Cbd;

            /* 
			 * check convergence
			 */
            if (!bp.Off || (!(state.Init == State.InitializationStates.InitFix || state.UseSmallSignal)))
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
            ceqbs = mbp.MosfetType * (Cbs - (Gbs - state.Gmin) * vbs);
            ceqbd = mbp.MosfetType * (Cbd - (Gbd - state.Gmin) * vbd);
            if (Mode >= 0)
            {
                xnrm = 1;
                xrev = 0;
                cdreq = mbp.MosfetType * (cdrain - Gds * vds - Gm * vgs - Gmbs * vbs);
            }
            else
            {
                xnrm = 0;
                xrev = 1;
                cdreq = -(mbp.MosfetType) * (cdrain - Gds * (-vds) - Gm * vgd - Gmbs * vbd);
            }
            state.Rhs[bulkNode] -= (ceqbs + ceqbd);
            state.Rhs[DrainNodePrime] += (ceqbd - cdreq);
            state.Rhs[SourceNodePrime] += cdreq + ceqbs;

            /* 
			 * load y matrix
			 */
            DrainDrainPtr.Add(temp.DrainConductance);
            SourceSourcePtr.Add(temp.SourceConductance);
            BulkBulkPtr.Add(Gbd + Gbs);
            DrainPrimeDrainPrimePtr.Add(temp.DrainConductance + Gds + Gbd + xrev * (Gm + Gmbs));
            SourcePrimeSourcePrimePtr.Add(temp.SourceConductance + Gds + Gbs + xnrm * (Gm + Gmbs));
            DrainDrainPrimePtr.Add(-temp.DrainConductance);
            SourceSourcePrimePtr.Add(-temp.SourceConductance);
            BulkDrainPrimePtr.Sub(Gbd);
            BulkSourcePrimePtr.Sub(Gbs);
            DrainPrimeDrainPtr.Add(-temp.DrainConductance);
            DrainPrimeGatePtr.Add((xnrm - xrev) * Gm);
            DrainPrimeBulkPtr.Add(-Gbd + (xnrm - xrev) * Gmbs);
            DrainPrimeSourcePrimePtr.Add(-Gds - xnrm * (Gm + Gmbs));
            SourcePrimeGatePtr.Add(-(xnrm - xrev) * Gm);
            SourcePrimeSourcePtr.Add(-temp.SourceConductance);
            SourcePrimeBulkPtr.Add(-Gbs - (xnrm - xrev) * Gmbs);
            SourcePrimeDrainPrimePtr.Add(-Gds - xrev * (Gm + Gmbs));
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

            vbs = mbp.MosfetType * (state.Solution[bulkNode] - state.Solution[SourceNodePrime]);
            vgs = mbp.MosfetType * (state.Solution[gateNode] - state.Solution[SourceNodePrime]);
            vds = mbp.MosfetType * (state.Solution[DrainNodePrime] - state.Solution[SourceNodePrime]);
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
            double tol = config.RelTolerance * Math.Max(Math.Abs(cdhat), Math.Abs(Cd)) + config.AbsTolerance;
            if (Math.Abs(cdhat - Cd) >= tol)
            {
                state.IsCon = false;
                return false;
            }

            tol = config.RelTolerance * Math.Max(Math.Abs(cbhat), Math.Abs(Cbs + Cbd)) + config.AbsTolerance;
            if (Math.Abs(cbhat - (Cbs + Cbd)) > tol)
            {
                state.IsCon = false;
                return false;
            }
            return true;
        }
    }
}
