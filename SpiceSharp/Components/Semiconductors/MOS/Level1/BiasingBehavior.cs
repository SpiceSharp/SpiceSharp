using System;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.Semiconductors;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;

namespace SpiceSharp.Components.MosfetBehaviors.Level1
{
    /// <summary>
    /// Biasing behavior for a <see cref="Mosfet1" />.
    /// </summary>
    public class BiasingBehavior : TemperatureBehavior, IBiasingBehavior, IConvergenceBehavior
    {
        private readonly IIntegrationMethod _method;
        private readonly ITimeSimulationState _time;
        private readonly IIterationSimulationState _iteration;
        private readonly int _drainNode, _gateNode, _sourceNode, _bulkNode, _drainNodePrime, _sourceNodePrime;
        private readonly ElementSet<double> _elements;

        /// <summary>
        /// Gets the internal drain node.
        /// </summary>
        /// <value>
        /// The internal drain node.
        /// </value>
        protected IVariable<double> DrainPrime { get; }

        /// <summary>
        /// Gets the internal source node.
        /// </summary>
        /// <value>
        /// The internal source node.
        /// </value>
        protected IVariable<double> SourcePrime { get; }

        /// <summary>
        /// The maximum exponent argument
        /// </summary>
        protected const double MaximumExponentArgument = 709.0;

        /// <summary>
        /// Gets the base configuration.
        /// </summary>
        protected BiasingParameters BiasingParameters { get; }

        /// <summary>
        /// Gets or sets the drain current.
        /// </summary>
        [ParameterName("id"), ParameterName("cd"), ParameterInfo("Drain current")]
        public double DrainCurrent { get; private set; }

        /// <summary>
        /// Gets or sets the bulk-source current.
        /// </summary>
        [ParameterName("ibs"), ParameterInfo("B-S junction current")]
        public double BsCurrent { get; private set; }

        /// <summary>
        /// Gets or sets the bulk-drain current.
        /// </summary>
        [ParameterName("ibd"), ParameterInfo("B-D junction current")]
        public double BdCurrent { get; private set; }

        /// <summary>
        /// Gets or sets the small-signal transconductance.
        /// </summary>
        [ParameterName("gm"), ParameterInfo("Transconductance")]
        public double Transconductance { get; private set; }

        /// <summary>
        /// Gets or sets the small-signal bulk transconductance.
        /// </summary>
        [ParameterName("gmb"), ParameterName("gmbs"), ParameterInfo("Bulk-Source transconductance")]
        public double TransconductanceBs { get; private set; }

        /// <summary>
        /// Gets or sets the small-signal output conductance.
        /// </summary>
        [ParameterName("gds"), ParameterInfo("Drain-Source conductance")]
        public double CondDs { get; private set; }

        /// <summary>
        /// Gets or sets the small-signal bulk-source conductance.
        /// </summary>
        [ParameterName("gbs"), ParameterInfo("Bulk-Source conductance")]
        public double CondBs { get; private set; }

        /// <summary>
        /// Gets or sets the small-signal bulk-drain conductance.
        /// </summary>
        [ParameterName("gbd"), ParameterInfo("Bulk-Drain conductance")]
        public double CondBd { get; private set; }

        /// <summary>
        /// Gets or sets the turn-on voltage.
        /// </summary>
        [ParameterName("von"), ParameterInfo("Turn-on voltage")]
        public double Von { get; private set; }

        /// <summary>
        /// Gets or sets the saturation voltage.
        /// </summary>
        [ParameterName("vdsat"), ParameterInfo("Saturation DrainNode voltage")]
        public double SaturationVoltageDs { get; private set; }

        /// <summary>
        /// Gets the current mode of operation. +1.0 if vds is positive, -1 if it is negative.
        /// </summary>
        public double Mode { get; private set; }

        /// <summary>
        /// Gets the gate-source voltage.
        /// </summary>
        [ParameterName("vgs"), ParameterInfo("Gate-Source voltage")]
        public virtual double VoltageGs { get; protected set; }

        /// <summary>
        /// Gets the drain-source voltage.
        /// </summary>
        [ParameterName("vds"), ParameterInfo("Drain-Source voltage")]
        public virtual double VoltageDs { get; protected set; }

        /// <summary>
        /// Gets the bulk-source voltage.
        /// </summary>
        [ParameterName("vbs"), ParameterInfo("Bulk-Source voltage")]
        public virtual double VoltageBs { get; protected set; }

        /// <summary>
        /// Gets the bulk-drain voltage.
        /// </summary>
        [ParameterName("vbd"), ParameterInfo("Bulk-Drain voltage")]
        public virtual double VoltageBd { get; protected set; }

        /// <summary>
        /// Gets the state of the biasing.
        /// </summary>
        /// <value>
        /// The state of the biasing.
        /// </value>
        protected IBiasingSimulationState BiasingState { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BiasingBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public BiasingBehavior(string name, ComponentBindingContext context) : base(name, context) 
        {
            context.Nodes.CheckNodes(4);
            BiasingState = context.GetState<IBiasingSimulationState>();
            BiasingParameters = context.GetSimulationParameterSet<BiasingParameters>();
            _iteration = context.GetState<IIterationSimulationState>();
            context.TryGetState(out _time);
            context.TryGetState(out _method);
            SaturationVoltageDs = 0;
            Von = 0;
            Mode = 1;

            DrainPrime = BiasingState.MapNode(context.Nodes[0]);
            _drainNode = BiasingState.Map[DrainPrime];
            _gateNode = BiasingState.Map[BiasingState.MapNode(context.Nodes[1])];
            SourcePrime = BiasingState.MapNode(context.Nodes[2]);
            _sourceNode = BiasingState.Map[SourcePrime];
            _bulkNode = BiasingState.Map[BiasingState.MapNode(context.Nodes[3])];

            // Add series drain node if necessary
            if (!ModelParameters.DrainResistance.Equals(0.0) || !ModelParameters.SheetResistance.Equals(0.0) && Parameters.DrainSquares > 0)
                DrainPrime = BiasingState.Create(Name.Combine("drain"), Units.Volt);
            _drainNodePrime = BiasingState.Map[DrainPrime];

            // Add series source node if necessary
            if (!ModelParameters.SourceResistance.Equals(0.0) || !ModelParameters.SheetResistance.Equals(0.0) && Parameters.SourceSquares > 0)
                SourcePrime = BiasingState.Create(Name.Combine("source"), Units.Volt);
            _sourceNodePrime = BiasingState.Map[SourcePrime];

            // Get matrix pointers
            _elements = new ElementSet<double>(BiasingState.Solver, new[] {
                new MatrixLocation(_drainNode, _drainNode),
                new MatrixLocation(_sourceNode, _sourceNode),
                new MatrixLocation(_bulkNode, _bulkNode),
                new MatrixLocation(_drainNodePrime, _drainNodePrime),
                new MatrixLocation(_sourceNodePrime, _sourceNodePrime),
                new MatrixLocation(_drainNode, _drainNodePrime),
                new MatrixLocation(_sourceNode, _sourceNodePrime),
                new MatrixLocation(_bulkNode, _drainNodePrime),
                new MatrixLocation(_bulkNode, _sourceNodePrime),
                new MatrixLocation(_drainNodePrime, _drainNode),
                new MatrixLocation(_drainNodePrime, _gateNode),
                new MatrixLocation(_drainNodePrime, _bulkNode),
                new MatrixLocation(_drainNodePrime, _sourceNodePrime),
                new MatrixLocation(_sourceNodePrime, _gateNode),
                new MatrixLocation(_sourceNodePrime, _sourceNode),
                new MatrixLocation(_sourceNodePrime, _bulkNode),
                new MatrixLocation(_sourceNodePrime, _drainNodePrime)
            }, new[] { _bulkNode, _drainNodePrime, _sourceNodePrime });
        }

        /// <summary>
        /// Loads the Y-matrix and Rhs-vector.
        /// </summary>
        protected virtual void Load()
        {
            // Get the current voltages
            Initialize(out double vgs, out var vds, out var vbs, out var check);
            var vbd = vbs - vds;
            var vgd = vgs - vds;

            /*
			 * bulk-source and bulk-drain diodes
			 * here we just evaluate the ideal diode current and the
			 * corresponding derivative (conductance).
			 */
            if (vbs <= 0)
            {
                CondBs = SourceSatCurrent / Vt;
                BsCurrent = CondBs * vbs;
                CondBs += BiasingParameters.Gmin;
            }
            else
            {
                var evbs = Math.Exp(Math.Min(MaximumExponentArgument, vbs / Vt));
                CondBs = SourceSatCurrent * evbs / Vt + BiasingParameters.Gmin;
                BsCurrent = SourceSatCurrent * (evbs - 1);
            }
            if (vbd <= 0)
            {
                CondBd = DrainSatCurrent / Vt;
                BdCurrent = CondBd * vbd;
                CondBd += BiasingParameters.Gmin;
            }
            else
            {
                var evbd = Math.Exp(Math.Min(MaximumExponentArgument, vbd / Vt));
                CondBd = DrainSatCurrent * evbd / Vt + BiasingParameters.Gmin;
                BdCurrent = DrainSatCurrent * (evbd - 1);
            }

            /*
             * Now to determine whether the user was able to correctly
			 * identify the source and drain of his device
			 */
            if (vds >= 0)
            {
                // normal mode
                Mode = 1;
            }
            else
            {
                // inverse mode
                Mode = -1;
            }

            // Update
            VoltageBs = vbs;
            VoltageBd = vbd;
            VoltageGs = vgs;
            VoltageDs = vds;

            // Evaluate the currents and derivatives
            var cdrain = Mode > 0 ? Evaluate(vgs, vds, vbs) : Evaluate(vgd, -vds, vbd);
            DrainCurrent = Mode * cdrain - BdCurrent;

            // Check convergence
            if (!Parameters.Off || _iteration.Mode != IterationModes.Fix)
            {
                if (check)
                    _iteration.IsConvergent = false;
            }

            // Load current vector
            double xnrm, xrev, cdreq;
            var ceqbs = ModelParameters.MosfetType * (BsCurrent - (CondBs - BiasingParameters.Gmin) * vbs);
            var ceqbd = ModelParameters.MosfetType * (BdCurrent - (CondBd - BiasingParameters.Gmin) * vbd);
            if (Mode >= 0)
            {
                xnrm = 1;
                xrev = 0;
                cdreq = ModelParameters.MosfetType * (cdrain - CondDs * vds - Transconductance * vgs - TransconductanceBs * vbs);
            }
            else
            {
                xnrm = 0;
                xrev = 1;
                cdreq = -ModelParameters.MosfetType * (cdrain - CondDs * -vds - Transconductance * vgd - TransconductanceBs * vbd);
            }

            _elements.Add(
                // Y-matrix
                DrainConductance,
                SourceConductance,
                CondBd + CondBs,
                DrainConductance + CondDs + CondBd + xrev * (Transconductance + TransconductanceBs),
                SourceConductance + CondDs + CondBs + xnrm * (Transconductance + TransconductanceBs),
                -DrainConductance,
                -SourceConductance,
                -CondBd,
                -CondBs,
                -DrainConductance,
                (xnrm - xrev) * Transconductance,
                -CondBd + (xnrm - xrev) * TransconductanceBs,
                -CondDs - xnrm * (Transconductance + TransconductanceBs),
                -(xnrm - xrev) * Transconductance,
                -SourceConductance,
                -CondBs - (xnrm - xrev) * TransconductanceBs,
                -CondDs - xrev * (Transconductance + TransconductanceBs),
                // RHS vector
                -(ceqbs + ceqbd), 
                ceqbd - cdreq, 
                cdreq + ceqbs);
        }

        /// <summary>
        /// Loads the Y-matrix and Rhs-vector.
        /// </summary>
        void IBiasingBehavior.Load() => Load();

        /// <summary>
        /// Initializes the voltages to be used for calculating the current iteration.
        /// </summary>
        /// <param name="vgs">The VGS.</param>
        /// <param name="vds">The VDS.</param>
        /// <param name="vbs">The VBS.</param>
        /// <param name="check">If set to <c>true</c>, the current voltage was limited and another iteration should be calculated.</param>
        protected void Initialize(out double vgs, out double vds, out double vbs, out bool check)
        {
            var state = BiasingState;
            check = true;

            if (_iteration.Mode == IterationModes.Float || (_time != null && !_time.UseDc && _method != null && _method.BaseTime.Equals(0.0)) ||
                _iteration.Mode == IterationModes.Fix && !Parameters.Off)
            {
                // General iteration
                vbs = ModelParameters.MosfetType * (state.Solution[_bulkNode] - state.Solution[_sourceNodePrime]);
                vgs = ModelParameters.MosfetType * (state.Solution[_gateNode] - state.Solution[_sourceNodePrime]);
                vds = ModelParameters.MosfetType * (state.Solution[_drainNodePrime] - state.Solution[_sourceNodePrime]);

                // now some common crunching for some more useful quantities
                var vbd = vbs - vds;
                var vgd = vgs - vds;
                var vgdo = VoltageGs - VoltageDs;
                var von = ModelParameters.MosfetType * Von;

                /*
				 * limiting
				 * we want to keep device voltages from changing
				 * so fast that the exponentials churn out overflows
				 * and similar rudeness
				 */
                // NOTE: Spice 3f5 does not write out Vgs during DC analysis, so DEVfetlim may give different results in Spice 3f5
                if (VoltageDs >= 0)
                {
                    vgs = Transistor.LimitFet(vgs, VoltageGs, von);
                    vds = vgs - vgd;
                    vds = Transistor.LimitVds(vds, VoltageDs);
                }
                else
                {
                    vgd = Transistor.LimitFet(vgd, vgdo, von);
                    vds = vgs - vgd;
                    vds = -Transistor.LimitVds(-vds, -VoltageDs);
                    vgs = vgd + vds;
                }

                check = false;
                if (vds >= 0)
                    vbs = Semiconductor.LimitJunction(vbs, VoltageBs, Vt, SourceVCritical, ref check);
                else
                {
                    vbd = Semiconductor.LimitJunction(vbd, VoltageBd, Vt, DrainVCritical, ref check);
                    vbs = vbd + vds;
                }
            }
            else
            {
                /* ok - not one of the simple cases, so we have to
				 * look at all of the possibilities for why we were
				 * called.  We still just initialize the three voltages
				 */
                if (_iteration.Mode == IterationModes.Junction && !Parameters.Off)
                {
                    vds = ModelParameters.MosfetType * Parameters.InitialVoltageDs;
                    vgs = ModelParameters.MosfetType * Parameters.InitialVoltageGs;
                    vbs = ModelParameters.MosfetType * Parameters.InitialVoltageBs;

                    // TODO: At some point, check what this is supposed to do
                    if (vds.Equals(0) && vgs.Equals(0) && vbs.Equals(0) && (_time == null || (_time.UseDc || !_time.UseIc)))
                    {
                        vbs = -1;
                        vgs = ModelParameters.MosfetType * TempVt0;
                        vds = 0;
                    }
                }
                else
                {
                    vbs = vgs = vds = 0;
                }
            }
        }

        /// <summary>
        /// Execute behavior
        /// </summary>
        protected double Evaluate(double vgs, double vds, double vbs)
        {
            double von;
            double vdsat, cDrainNode;
            var effectiveLength = Parameters.Length - 2 * ModelParameters.LateralDiffusion;
            var beta = TempTransconductance * Parameters.Width / effectiveLength;
            
            {
                /*
				 * this block of code evaluates the DrainNode current and its
				 * derivatives using the shichman - hodges model and the
				 * charges associated with the gate, channel and bulk for
				 * mosfets
				 */

                /* the following 4 variables are local to this code block until
				 * it is obvious that they can be made global
				 */
                double arg;
                double sarg;

                if (vbs <= 0)
                {
                    sarg = Math.Sqrt(TempPhi - vbs);
                }
                else
                {
                    sarg = Math.Sqrt(TempPhi);
                    sarg = sarg - vbs / (sarg + sarg);
                    sarg = Math.Max(0, sarg);
                }
                von = TempVoltageBi * ModelParameters.MosfetType + ModelParameters.Gamma * sarg;
                var vgst = vgs - von;
                vdsat = Math.Max(vgst, 0);
                if (sarg <= 0)
                {
                    arg = 0;
                }
                else
                {
                    arg = ModelParameters.Gamma / (sarg + sarg);
                }
                if (vgst <= 0)
                {
                    /*
					 * cutoff region
					 */
                    cDrainNode = 0;
                    Transconductance = 0;
                    CondDs = 0;
                    TransconductanceBs = 0;
                }
                else
                {
                    /*
					 * saturation region
					 */
                    var betap = beta * (1 + ModelParameters.Lambda * vds);
                    if (vgst <= vds)
                    {
                        cDrainNode = betap * vgst * vgst * .5;
                        Transconductance = betap * vgst;
                        CondDs = ModelParameters.Lambda * beta * vgst * vgst * .5;
                        TransconductanceBs = Transconductance * arg;
                    }
                    else
                    {
                        /*
						* linear region
						*/
                        cDrainNode = betap * vds * (vgst - .5 * vds);
                        Transconductance = betap * vds;
                        CondDs = betap * (vgst - vds) + ModelParameters.Lambda * beta * vds * (vgst - .5 * vds);
                        TransconductanceBs = Transconductance * arg;
                    }
                }
                /*
				 * finished
				 */
            }

            /* now deal with n vs p polarity */
            Von = ModelParameters.MosfetType * von;
            SaturationVoltageDs = ModelParameters.MosfetType * vdsat;
            return cDrainNode;
        }

        /// <summary>
        /// Tests convergence at the device-level.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the device determines the solution converges; otherwise, <c>false</c>.
        /// </returns>
        bool IConvergenceBehavior.IsConvergent()
        {
            double cdhat;

            var vbs = ModelParameters.MosfetType * (BiasingState.Solution[_bulkNode] - BiasingState.Solution[_sourceNodePrime]);
            var vgs = ModelParameters.MosfetType * (BiasingState.Solution[_gateNode] - BiasingState.Solution[_sourceNodePrime]);
            var vds = ModelParameters.MosfetType * (BiasingState.Solution[_drainNodePrime] - BiasingState.Solution[_sourceNodePrime]);
            var vbd = vbs - vds;
            var vgd = vgs - vds;
            var vgdo = VoltageGs - VoltageDs;
            var delvbs = vbs - VoltageBs;
            var delvbd = vbd - VoltageBd;
            var delvgs = vgs - VoltageGs;
            var delvds = vds - VoltageDs;
            var delvgd = vgd - vgdo;

            // these are needed for convergence testing
            // NOTE: Cd does not include contributions for transient simulations... Should check for a way to include them!
            if (Mode >= 0)
            {
                cdhat = DrainCurrent - CondBd * delvbd + TransconductanceBs * delvbs +
                    Transconductance * delvgs + CondDs * delvds;
            }
            else
            {
                cdhat = DrainCurrent - (CondBd - TransconductanceBs) * delvbd -
                    Transconductance * delvgd + CondDs * delvds;
            }
            var cbhat = BsCurrent + BdCurrent + CondBd * delvbd + CondBs * delvbs;

            /*
             *  check convergence
             */
            var tol = BiasingParameters.RelativeTolerance * Math.Max(Math.Abs(cdhat), Math.Abs(DrainCurrent)) + BiasingParameters.AbsoluteTolerance;
            if (Math.Abs(cdhat - DrainCurrent) >= tol)
            {
                _iteration.IsConvergent = false;
                return false;
            }

            tol = BiasingParameters.RelativeTolerance * Math.Max(Math.Abs(cbhat), Math.Abs(BsCurrent + BdCurrent)) + BiasingParameters.AbsoluteTolerance;
            if (Math.Abs(cbhat - (BsCurrent + BdCurrent)) > tol)
            {
                _iteration.IsConvergent = false;
                return false;
            }
            return true;
        }
    }
}
