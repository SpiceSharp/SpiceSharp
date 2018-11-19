using System;
using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Simulations.Behaviors;
using Semiconductor = SpiceSharp.Components.Semiconductors.Semiconductor;

namespace SpiceSharp.Components.MosfetBehaviors.Common
{
    /// <summary>
    /// A common DC biasing behavior for mosfets.
    /// </summary>
    /// <seealso cref="SpiceSharp.Behaviors.BaseLoadBehavior" />
    public abstract class BiasingBehavior : ExportingBehavior, IBiasingBehavior, IConnectedBehavior
    {
        /// <summary>
        /// The permittivity of silicon
        /// </summary>
        protected const double EpsilonSilicon = 11.7 * 8.854214871e-12;
        protected const double MaximumExponentArgument = 709.0;

        // Necessary behaviors and parameters
        private BaseConfiguration _baseConfig;
        private ModelBaseParameters _mbp;
        private BaseParameters _bp;
        private TemperatureBehavior _temp;

        /// <summary>
        /// Gets or sets the drain current.
        /// </summary>
        /// <value>
        /// The drain current.
        /// </value>
        [ParameterName("id"), ParameterName("cd"), ParameterInfo("Drain current")]
        public double DrainCurrent { get; protected set; }

        /// <summary>
        /// Gets or sets the bulk-source current.
        /// </summary>
        /// <value>
        /// The bulk-source current.
        /// </value>
        [ParameterName("ibs"), ParameterInfo("B-S junction current")]
        public double BsCurrent { get; protected set; }

        /// <summary>
        /// Gets or sets the bulk-drain current.
        /// </summary>
        /// <value>
        /// The bulk-drain current.
        /// </value>
        [ParameterName("ibd"), ParameterInfo("B-D junction current")]
        public double BdCurrent { get; protected set; }

        /// <summary>
        /// Gets or sets the transconductance.
        /// </summary>
        /// <value>
        /// The transconductance.
        /// </value>
        [ParameterName("gm"), ParameterInfo("Transconductance")]
        public double Transconductance { get; protected set; }

        /// <summary>
        /// Gets or sets the bulk transconductance.
        /// </summary>
        /// <value>
        /// The bulk transconductance.
        /// </value>
        [ParameterName("gmb"), ParameterName("gmbs"), ParameterInfo("Bulk-Source transconductance")]
        public double TransconductanceBs { get; protected set; }

        /// <summary>
        /// Gets or sets the output conductance.
        /// </summary>
        /// <value>
        /// The output conductance.
        /// </value>
        [ParameterName("gds"), ParameterInfo("Drain-Source conductance")]
        public double CondDs { get; protected set; }

        /// <summary>
        /// Gets or sets the bulk-source conductance.
        /// </summary>
        /// <value>
        /// The bulk-source conductance.
        /// </value>
        [ParameterName("gbs"), ParameterInfo("Bulk-Source conductance")]
        public double CondBs { get; protected set; }

        /// <summary>
        /// Gets or sets the bulk-drain conductance.
        /// </summary>
        /// <value>
        /// The bulk-drain conductance.
        /// </value>
        [ParameterName("gbd"), ParameterInfo("Bulk-Drain conductance")]
        public double CondBd { get; protected set; }

        /// <summary>
        /// Gets or sets the turn-on voltage.
        /// </summary>
        /// <value>
        /// The turn-on voltage.
        /// </value>
        [ParameterName("von"), ParameterInfo("Turn-on voltage")]
        public double Von { get; protected set; }

        /// <summary>
        /// Gets or sets the saturation voltage.
        /// </summary>
        /// <value>
        /// The saturation voltage.
        /// </value>
        [ParameterName("vdsat"), ParameterInfo("Saturation drain voltage")]
        public double SaturationVoltageDs { get; protected set; }

        /// <summary>
        /// Extra variables
        /// </summary>
        public double Mode { get; protected set; }
        public double VoltageGs { get; protected set; }
        public double VoltageDs { get; protected set; }
        public double VoltageBs { get; protected set; }
        public double VoltageBd { get; protected set; }

        /// <summary>
        /// Nodes
        /// </summary>
        public int DrainNode { get; private set; }
        public int GateNode { get; private set; }
        public int SourceNode { get; private set; }
        public int BulkNode { get; private set; }
        public int DrainNodePrime { get; private set; }
        public int SourceNodePrime { get; private set; }

        /// <summary>
        /// Matrix elements
        /// </summary>
        public MosfetPointers Pointers { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BiasingBehavior"/> class.
        /// </summary>
        /// <param name="name">The identifier of the behavior.</param>
        /// <remarks>
        /// The identifier of the behavior should be the same as that of the entity creating it.
        /// </remarks>
        protected BiasingBehavior(string name) : base(name)
        {
            Pointers = new MosfetPointers();
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
                throw new CircuitException("Pin count mismatch: 4 pins expected, {0} given".FormatString(pins.Length));

            DrainNode = pins[0];
            GateNode = pins[1];
            SourceNode = pins[2];
            BulkNode = pins[3];
        }

        /// <summary>
        /// Setup the behavior.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="provider">The data provider.</param>
        public override void Setup(Simulation simulation, SetupDataProvider provider)
        {
            if (simulation == null)
                throw new ArgumentNullException(nameof(simulation));
            if (provider == null)
                throw new ArgumentNullException(nameof(simulation));

            // Get configurations
            _baseConfig = simulation.Configurations.Get<BaseConfiguration>();

            // Get parameters
            _bp = provider.GetParameterSet<BaseParameters>();
            _mbp = provider.GetParameterSet<ModelBaseParameters>("model");

            // Get behaviors
            _temp = provider.GetBehavior<TemperatureBehavior>();
        }

        /// <summary>
        /// Gets matrix pointers
        /// </summary>
        /// <param name="variables">Nodes</param>
        /// <param name="solver">Solver</param>
        public void GetEquationPointers(VariableSet variables, Solver<double> solver)
        {
            if (variables == null)
                throw new ArgumentNullException(nameof(variables));
            if (solver == null)
                throw new ArgumentNullException(nameof(solver));

            // Add series drain node if necessary
            if (_mbp.DrainResistance > 0 || _mbp.SheetResistance > 0 && _bp.DrainSquares > 0)
                DrainNodePrime = variables.Create(Name.Combine("drain")).Index;
            else
                DrainNodePrime = DrainNode;

            // Add series source node if necessary
            if (_mbp.SourceResistance > 0 || _mbp.SheetResistance > 0 && _bp.SourceSquares > 0)
                SourceNodePrime = variables.Create(Name.Combine("source")).Index;
            else
                SourceNodePrime = SourceNode;

            // Get the pointers
            Pointers.GetPointers(solver, DrainNode, GateNode, SourceNode, BulkNode, DrainNodePrime, SourceNodePrime);
        }
        
        /// <summary>
        /// Loads the Y-matrix and Rhs-vector.
        /// </summary>
        /// <param name="simulation">The base simulation.</param>
        public void Load(BaseSimulation simulation)
        {
            if (simulation == null)
                throw new ArgumentNullException(nameof(simulation));
            var state = simulation.RealState;

            double vgs, vds, vbs, vbd, vgd;
            double drainSatCur, sourceSatCur;
            bool check = true;

            if (_temp.TempSaturationCurrentDensity.Equals(0) || _bp.DrainArea.Value <= 0 || _bp.SourceArea.Value <= 0)
            {
                drainSatCur = _temp.TempSaturationCurrent;
                sourceSatCur = _temp.TempSaturationCurrent;
            }
            else
            {
                drainSatCur = _temp.TempSaturationCurrentDensity * _bp.DrainArea;
                sourceSatCur = _temp.TempSaturationCurrentDensity * _bp.SourceArea;
            }

            var vt = Circuit.KOverQ * _bp.Temperature;

            if (state.Init == InitializationModes.Float || (simulation is TimeSimulation tsim && tsim.Method.BaseTime.Equals(0.0)) ||
                state.Init == InitializationModes.Fix && !_bp.Off)
            {
                // General iteration
                vbs = _mbp.MosfetType * (state.Solution[BulkNode] - state.Solution[SourceNodePrime]);
                vgs = _mbp.MosfetType * (state.Solution[GateNode] - state.Solution[SourceNodePrime]);
                vds = _mbp.MosfetType * (state.Solution[DrainNodePrime] - state.Solution[SourceNodePrime]);

                // now some common crunching for some more useful quantities
                vbd = vbs - vds;
                vgd = vgs - vds;
                var vgdo = VoltageGs - VoltageDs;
                var von = _mbp.MosfetType * Von;

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
                    vbs = Semiconductor.LimitJunction(vbs, VoltageBs, vt, _temp.SourceVCritical, ref check);
                else
                {
                    vbd = Semiconductor.LimitJunction(vbd, VoltageBd, vt, _temp.DrainVCritical, ref check);
                    vbs = vbd + vds;
                }
            }
            else
            {
                /* ok - not one of the simple cases, so we have to
				 * look at all of the possibilities for why we were
				 * called.  We still just initialize the three voltages
				 */
                if (state.Init == InitializationModes.Junction && !_bp.Off)
                {
                    vds = _mbp.MosfetType * _bp.InitialVoltageDs;
                    vgs = _mbp.MosfetType * _bp.InitialVoltageGs;
                    vbs = _mbp.MosfetType * _bp.InitialVoltageBs;

                    // TODO: At some point, check what this is supposed to do
                    if (vds.Equals(0) && vgs.Equals(0) && vbs.Equals(0) && (state.UseDc || !state.UseIc))
                    {
                        vbs = -1;
                        vgs = _mbp.MosfetType * _temp.TempVt0;
                        vds = 0;
                    }
                }
                else
                {
                    vbs = vgs = vds = 0;
                }
            }

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
                CondBs = sourceSatCur / vt;
                BsCurrent = CondBs * vbs;
                CondBs += _baseConfig.Gmin;
            }
            else
            {
                var evbs = Math.Exp(Math.Min(MaximumExponentArgument, vbs / vt));
                CondBs = sourceSatCur * evbs / vt + _baseConfig.Gmin;
                BsCurrent = sourceSatCur * (evbs - 1);
            }
            if (vbd <= 0)
            {
                CondBd = drainSatCur / vt;
                BdCurrent = CondBd * vbd;
                CondBd += _baseConfig.Gmin;
            }
            else
            {
                var evbd = Math.Exp(Math.Min(MaximumExponentArgument, vbd / vt));
                CondBd = drainSatCur * evbd / vt + _baseConfig.Gmin;
                BdCurrent = drainSatCur * (evbd - 1);
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
            if (!_bp.Off || state.Init != InitializationModes.Fix)
            {
                if (check)
                    state.IsConvergent = false;
            }

            // Load current vector
            double xnrm, xrev, cdreq;
            var ceqbs = _mbp.MosfetType * (BsCurrent - (CondBs - _baseConfig.Gmin) * vbs);
            var ceqbd = _mbp.MosfetType * (BdCurrent - (CondBd - _baseConfig.Gmin) * vbd);
            if (Mode >= 0)
            {
                xnrm = 1;
                xrev = 0;
                cdreq = _mbp.MosfetType * (cdrain - CondDs * vds - Transconductance * vgs - TransconductanceBs * vbs);
            }
            else
            {
                xnrm = 0;
                xrev = 1;
                cdreq = -_mbp.MosfetType * (cdrain - CondDs * -vds - Transconductance * vgd - TransconductanceBs * vbd);
            }
            Pointers.BulkPtr.Value -= ceqbs + ceqbd;
            Pointers.DrainPrimePtr.Value += ceqbd - cdreq;
            Pointers.SourcePrimePtr.Value += cdreq + ceqbs;

            // Load Y-matrix
            Pointers.DrainDrainPtr.Value += _temp.DrainConductance;
            Pointers.SourceSourcePtr.Value += _temp.SourceConductance;
            Pointers.BulkBulkPtr.Value += CondBd + CondBs;
            Pointers.DrainPrimeDrainPrimePtr.Value += _temp.DrainConductance + CondDs + CondBd + xrev * (Transconductance + TransconductanceBs);
            Pointers.SourcePrimeSourcePrimePtr.Value += _temp.SourceConductance + CondDs + CondBs + xnrm * (Transconductance + TransconductanceBs);
            Pointers.DrainDrainPrimePtr.Value += -_temp.DrainConductance;
            Pointers.SourceSourcePrimePtr.Value += -_temp.SourceConductance;
            Pointers.BulkDrainPrimePtr.Value -= CondBd;
            Pointers.BulkSourcePrimePtr.Value -= CondBs;
            Pointers.DrainPrimeDrainPtr.Value += -_temp.DrainConductance;
            Pointers.DrainPrimeGatePtr.Value += (xnrm - xrev) * Transconductance;
            Pointers.DrainPrimeBulkPtr.Value += -CondBd + (xnrm - xrev) * TransconductanceBs;
            Pointers.DrainPrimeSourcePrimePtr.Value += -CondDs - xnrm * (Transconductance + TransconductanceBs);
            Pointers.SourcePrimeGatePtr.Value += -(xnrm - xrev) * Transconductance;
            Pointers.SourcePrimeSourcePtr.Value += -_temp.SourceConductance;
            Pointers.SourcePrimeBulkPtr.Value += -CondBs - (xnrm - xrev) * TransconductanceBs;
            Pointers.SourcePrimeDrainPrimePtr.Value += -CondDs - xrev * (Transconductance + TransconductanceBs);
        }

        /// <summary>
        /// This routine evaluates the drain current and its derivatives. It also returns the calculated
        /// drain current.
        /// </summary>
        protected abstract double Evaluate(double vgs, double vds, double vbs);

        /// <summary>
        /// Tests convergence at the device-level.
        /// </summary>
        /// <param name="simulation">The base simulation.</param>
        /// <returns>
        /// <c>true</c> if the device determines the solution converges; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">simulation</exception>
        public bool IsConvergent(BaseSimulation simulation)
        {
			if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));

            var state = simulation.RealState;
            double cdhat;

            var vbs = _mbp.MosfetType * (state.Solution[BulkNode] - state.Solution[SourceNodePrime]);
            var vgs = _mbp.MosfetType * (state.Solution[GateNode] - state.Solution[SourceNodePrime]);
            var vds = _mbp.MosfetType * (state.Solution[DrainNodePrime] - state.Solution[SourceNodePrime]);
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
            var tol = _baseConfig.RelativeTolerance * Math.Max(Math.Abs(cdhat), Math.Abs(DrainCurrent)) + _baseConfig.AbsoluteTolerance;
            if (Math.Abs(cdhat - DrainCurrent) >= tol)
            {
                state.IsConvergent = false;
                return false;
            }

            tol = _baseConfig.RelativeTolerance * Math.Max(Math.Abs(cbhat), Math.Abs(BsCurrent + BdCurrent)) + _baseConfig.AbsoluteTolerance;
            if (Math.Abs(cbhat - (BsCurrent + BdCurrent)) > tol)
            {
                state.IsConvergent = false;
                return false;
            }
            return true;
        }
    }
}
