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
        public double SaturationVoltageDS { get; protected set; } = 0.0;
        [PropertyName("id"), PropertyInfo("Drain current")]
        public double DrainCurrent { get; protected set; }
        [PropertyName("ibs"), PropertyInfo("B-S junction current")]
        public double BSCurrent { get; protected set; }
        [PropertyName("ibd"), PropertyInfo("B-D junction current")]
        public double BDCurrent { get; protected set; }
        [PropertyName("gmb"), PropertyName("gmbs"), PropertyInfo("Bulk-Source transconductance")]
        public double TransconductanceBS { get; protected set; }
        [PropertyName("gm"), PropertyInfo("Transconductance")]
        public double Transconductance { get; protected set; }
        [PropertyName("gds"), PropertyInfo("Drain-Source conductance")]
        public double CondDS { get; protected set; }
        [PropertyName("gbd"), PropertyInfo("Bulk-Drain conductance")]
        public double CondBD { get; protected set; }
        [PropertyName("gbs"), PropertyInfo("Bulk-Source conductance")]
        public double CondBS { get; protected set; }

        /// <summary>
        /// Extra variables
        /// </summary>
        public double Mode { get; protected set; }

        public double VoltageBD { get; protected set; }
        public double VoltageBS { get; protected set; }
        public double VoltageGS { get; protected set; }
        public double VoltageDS { get; protected set; }

        /// <summary>
        /// Matrix elements
        /// </summary>
        protected Element<double> DrainDrainPtr { get; private set; }
        protected Element<double> GateGatePtr { get; private set; }
        protected Element<double> SourceSourcePtr { get; private set; }
        protected Element<double> BulkBulkPtr { get; private set; }
        protected Element<double> DrainPrimeDrainPrimePtr { get; private set; }
        protected Element<double> SourcePrimeSourcePrimePtr { get; private set; }
        protected Element<double> DrainDrainPrimePtr { get; private set; }
        protected Element<double> GateBulkPtr { get; private set; }
        protected Element<double> GateDrainPrimePtr { get; private set; }
        protected Element<double> GateSourcePrimePtr { get; private set; }
        protected Element<double> SourceSourcePrimePtr { get; private set; }
        protected Element<double> BulkDrainPrimePtr { get; private set; }
        protected Element<double> BulkSourcePrimePtr { get; private set; }
        protected Element<double> DrainPrimeSourcePrimePtr { get; private set; }
        protected Element<double> DrainPrimeDrainPtr { get; private set; }
        protected Element<double> BulkGatePtr { get; private set; }
        protected Element<double> DrainPrimeGatePtr { get; private set; }
        protected Element<double> SourcePrimeGatePtr { get; private set; }
        protected Element<double> SourcePrimeSourcePtr { get; private set; }
        protected Element<double> DrainPrimeBulkPtr { get; private set; }
        protected Element<double> SourcePrimeBulkPtr { get; private set; }
        protected Element<double> SourcePrimeDrainPrimePtr { get; private set; }

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
            bp = provider.GetParameterSet<BaseParameters>("entity");
            mbp = provider.GetParameterSet<ModelBaseParameters>("model");

            // Get behaviors
            temp = provider.GetBehavior<TemperatureBehavior>("entity");
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
        /// Gets matrix pointers
        /// </summary>
        /// <param name="nodes">Nodes</param>
        /// <param name="matrix">Matrix</param>
        public override void GetMatrixPointers(Nodes nodes, Matrix<double> matrix)
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

            var state = simulation.RealState;
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
            if ((temp.TempSaturationCurrentDensity == 0) || (bp.DrainArea.Value == 0) || (bp.SourceArea.Value == 0))
            {
                DrainSatCur = temp.TempSaturationCurrent;
                SourceSatCur = temp.TempSaturationCurrent;
            }
            else
            {
                DrainSatCur = temp.TempSaturationCurrentDensity * bp.DrainArea;
                SourceSatCur = temp.TempSaturationCurrentDensity * bp.SourceArea;
            }

            Beta = temp.TempTransconductance * bp.Width / EffectiveLength;
            /* 
			 * ok - now to do the start - up operations
			 * 
			 * we must get values for vbs, vds, and vgs from somewhere
			 * so we either predict them or recover them from last iteration
			 * These are the two most common cases - either a prediction
			 * step or the general iteration step and they
			 * share some code, so we put them first - others later on
			 */
            if ((state.Init == RealState.InitializationStates.InitFloat || state.UseSmallSignal || (state.Init == RealState.InitializationStates.InitTransient)) ||
                ((state.Init == RealState.InitializationStates.InitFix) && (!bp.Off)))
            {
                // general iteration
                vbs = mbp.MosfetType * (state.Solution[bulkNode] - state.Solution[SourceNodePrime]);
                vgs = mbp.MosfetType * (state.Solution[gateNode] - state.Solution[SourceNodePrime]);
                vds = mbp.MosfetType * (state.Solution[DrainNodePrime] - state.Solution[SourceNodePrime]);

                /* now some common crunching for some more useful quantities */
                vbd = vbs - vds;
                vgd = vgs - vds;
                vgdo = VoltageGS - VoltageDS;
                von = mbp.MosfetType * Von;

                /* 
				 * limiting
				 * we want to keep device voltages from changing
				 * so fast that the exponentials churn out overflows
				 * and similar rudeness
				 */

                if (VoltageDS >= 0)
                {
                    vgs = Transistor.LimitFet(vgs, VoltageGS, von);
                    vds = vgs - vgd;
                    vds = Transistor.LimitVoltageDS(vds, VoltageDS);
                    vgd = vgs - vds;
                }
                else
                {
                    vgd = Transistor.LimitFet(vgd, vgdo, von);
                    vds = vgs - vgd;
                    vds = -Transistor.LimitVoltageDS(-vds, -VoltageDS);
                    vgs = vgd + vds;
                }
                if (vds >= 0)
                {
                    vbs = Transistor.LimitJunction(vbs, VoltageBS, vt, temp.SourceVCritical, ref Check);
                    vbd = vbs - vds;
                }
                else
                {
                    vbd = Transistor.LimitJunction(vbd, VoltageBD, vt, temp.DrainVCritical, ref Check);
                    vbs = vbd + vds;
                }
            }
            else
            {
                /* ok - not one of the simple cases, so we have to
				 * look at all of the possibilities for why we were
				 * called.  We still just initialize the three voltages
				 */

                if ((state.Init == RealState.InitializationStates.InitJunction) && !bp.Off)
                {
                    vds = mbp.MosfetType * bp.InitialVoltageDS;
                    vgs = mbp.MosfetType * bp.InitialVoltageGS;
                    vbs = mbp.MosfetType * bp.InitialVoltageBS;
                    if ((vds == 0) && (vgs == 0) && (vbs == 0) && ((state.UseDC ||
                        state.Domain == RealState.DomainType.None) || (!state.UseIC)))
                    {
                        vbs = -1;
                        vgs = mbp.MosfetType * temp.TempVt0;
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
                CondBS = SourceSatCur / vt;
                BSCurrent = CondBS * vbs;
                CondBS += state.Gmin;
            }
            else
            {
                evbs = Math.Exp(Math.Min(Transistor.MaximumExponentArgument, vbs / vt));
                CondBS = SourceSatCur * evbs / vt + state.Gmin;
                BSCurrent = SourceSatCur * (evbs - 1);
            }
            if (vbd <= 0)
            {
                CondBD = DrainSatCur / vt;
                BDCurrent = CondBD * vbd;
                CondBD += state.Gmin;
            }
            else
            {
                evbd = Math.Exp(Math.Min(Transistor.MaximumExponentArgument, vbd / vt));
                CondBD = DrainSatCur * evbd / vt + state.Gmin;
                BDCurrent = DrainSatCur * (evbd - 1);
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
                    sarg = Math.Sqrt(temp.TempPhi - (Mode > 0 ? vbs : vbd));
                }
                else
                {
                    sarg = Math.Sqrt(temp.TempPhi);
                    sarg = sarg - (Mode > 0 ? vbs : vbd) / (sarg + sarg);
                    sarg = Math.Max(0, sarg);
                }
                von = (temp.TempVoltageBI * mbp.MosfetType) + mbp.Gamma * sarg;
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
                    Transconductance = 0;
                    CondDS = 0;
                    TransconductanceBS = 0;
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
                        Transconductance = betap * vgst;
                        CondDS = mbp.Lambda * Beta * vgst * vgst * .5;
                        TransconductanceBS = Transconductance * arg;
                    }
                    else
                    {
                        /* 
						* linear region
						*/
                        cdrain = betap * (vds * Mode) * (vgst - .5 * (vds * Mode));
                        Transconductance = betap * (vds * Mode);
                        CondDS = betap * (vgst - (vds * Mode)) + mbp.Lambda * Beta * (vds * Mode) * (vgst - .5 * (vds * Mode));
                        TransconductanceBS = Transconductance * arg;
                    }
                }
                /* 
				 * finished
				 */
            }

            /* now deal with n vs p polarity */
            Von = mbp.MosfetType * von;
            SaturationVoltageDS = mbp.MosfetType * vdsat;
            /* line 490 */
            /* 
			 * COMPUTE EQUIVALENT DRAIN CURRENT SOURCE
			 */
            DrainCurrent = Mode * cdrain - BDCurrent;

            /* 
			 * check convergence
			 */
            if (!bp.Off || (!(state.Init == RealState.InitializationStates.InitFix || state.UseSmallSignal)))
            {
                if (Check == 1)
                    state.IsConvergent = false;
            }

            /* DETAILPROF */

            /* save things away for next time */
            VoltageBS = vbs;
            VoltageBD = vbd;
            VoltageGS = vgs;
            VoltageDS = vds;

            /* 
			 * load current vector
			 */
            ceqbs = mbp.MosfetType * (BSCurrent - (CondBS - state.Gmin) * vbs);
            ceqbd = mbp.MosfetType * (BDCurrent - (CondBD - state.Gmin) * vbd);
            if (Mode >= 0)
            {
                xnrm = 1;
                xrev = 0;
                cdreq = mbp.MosfetType * (cdrain - CondDS * vds - Transconductance * vgs - TransconductanceBS * vbs);
            }
            else
            {
                xnrm = 0;
                xrev = 1;
                cdreq = -(mbp.MosfetType) * (cdrain - CondDS * (-vds) - Transconductance * vgd - TransconductanceBS * vbd);
            }
            state.Rhs[bulkNode] -= (ceqbs + ceqbd);
            state.Rhs[DrainNodePrime] += (ceqbd - cdreq);
            state.Rhs[SourceNodePrime] += cdreq + ceqbs;

            /* 
			 * load y matrix
			 */
            DrainDrainPtr.Add(temp.DrainConductance);
            SourceSourcePtr.Add(temp.SourceConductance);
            BulkBulkPtr.Add(CondBD + CondBS);
            DrainPrimeDrainPrimePtr.Add(temp.DrainConductance + CondDS + CondBD + xrev * (Transconductance + TransconductanceBS));
            SourcePrimeSourcePrimePtr.Add(temp.SourceConductance + CondDS + CondBS + xnrm * (Transconductance + TransconductanceBS));
            DrainDrainPrimePtr.Add(-temp.DrainConductance);
            SourceSourcePrimePtr.Add(-temp.SourceConductance);
            BulkDrainPrimePtr.Sub(CondBD);
            BulkSourcePrimePtr.Sub(CondBS);
            DrainPrimeDrainPtr.Add(-temp.DrainConductance);
            DrainPrimeGatePtr.Add((xnrm - xrev) * Transconductance);
            DrainPrimeBulkPtr.Add(-CondBD + (xnrm - xrev) * TransconductanceBS);
            DrainPrimeSourcePrimePtr.Add(-CondDS - xnrm * (Transconductance + TransconductanceBS));
            SourcePrimeGatePtr.Add(-(xnrm - xrev) * Transconductance);
            SourcePrimeSourcePtr.Add(-temp.SourceConductance);
            SourcePrimeBulkPtr.Add(-CondBS - (xnrm - xrev) * TransconductanceBS);
            SourcePrimeDrainPrimePtr.Add(-CondDS - xrev * (Transconductance + TransconductanceBS));
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

            var state = simulation.RealState;
            var config = simulation.BaseConfiguration;

            double vbs, vgs, vds, vbd, vgd, vgdo, delvbs, delvbd, delvgs, delvds, delvgd, cdhat, cbhat;

            vbs = mbp.MosfetType * (state.Solution[bulkNode] - state.Solution[SourceNodePrime]);
            vgs = mbp.MosfetType * (state.Solution[gateNode] - state.Solution[SourceNodePrime]);
            vds = mbp.MosfetType * (state.Solution[DrainNodePrime] - state.Solution[SourceNodePrime]);
            vbd = vbs - vds;
            vgd = vgs - vds;
            vgdo = VoltageGS - VoltageDS;
            delvbs = vbs - VoltageBS;
            delvbd = vbd - VoltageBD;
            delvgs = vgs - VoltageGS;
            delvds = vds - VoltageDS;
            delvgd = vgd - vgdo;

            // these are needed for convergence testing
            // NOTE: Cd does not include contributions for transient simulations... Should check for a way to include them!
            if (Mode >= 0)
            {
                cdhat = DrainCurrent - CondBD * delvbd + TransconductanceBS * delvbs +
                    Transconductance * delvgs + CondDS * delvds;
            }
            else
            {
                cdhat = DrainCurrent - (CondBD - TransconductanceBS) * delvbd -
                    Transconductance * delvgd + CondDS * delvds;
            }
            cbhat = BSCurrent + BDCurrent + CondBD * delvbd + CondBS * delvbs;

            /*
             *  check convergence
             */
            // NOTE: relative and absolute tolerances need to be gotten from the configuration, temporarely set to constants here
            double tol = config.RelativeTolerance * Math.Max(Math.Abs(cdhat), Math.Abs(DrainCurrent)) + config.AbsoluteTolerance;
            if (Math.Abs(cdhat - DrainCurrent) >= tol)
            {
                state.IsConvergent = false;
                return false;
            }

            tol = config.RelativeTolerance * Math.Max(Math.Abs(cbhat), Math.Abs(BSCurrent + BDCurrent)) + config.AbsoluteTolerance;
            if (Math.Abs(cbhat - (BSCurrent + BDCurrent)) > tol)
            {
                state.IsConvergent = false;
                return false;
            }
            return true;
        }
    }
}
