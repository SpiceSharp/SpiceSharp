﻿using System;
using SpiceSharp.ParameterSets;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.Semiconductors;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;

namespace SpiceSharp.Components.Mosfets.Level2
{
    /// <summary>
    /// Biasing behavior for a <see cref="Mosfet2" />.
    /// </summary>
    /// <seealso cref="Temperature"/>
    /// <seealso cref="IBiasingBehavior"/>
    /// <seealso cref="IConvergenceBehavior"/>
    public class Biasing : Temperature, 
        IBiasingBehavior, 
        IConvergenceBehavior
    {
        private readonly IIntegrationMethod _method;
        private readonly ITimeSimulationState _time;
        private readonly IIterationSimulationState _iteration;
        private readonly int _drainNode, _gateNode, _sourceNode, _bulkNode, _drainNodePrime, _sourceNodePrime;
        private readonly ElementSet<double> _elements;

        /// <summary>
        /// Signs used in the model
        /// </summary>
        private static readonly double[] _sig1 = { 1.0, -1.0, 1.0, -1.0 };
        private static readonly double[] _sig2 = { 1.0, 1.0, -1.0, -1.0 };

        /// <summary>
        /// The permittivity of silicon.
        /// </summary>
        protected const double EpsilonSilicon = 11.7 * 8.854214871e-12;

        /// <summary>
        /// The maximum exponent argument.
        /// </summary>
        protected const double MaximumExponentArgument = 709.0;

        /// <summary>
        /// Gets the base configuration.
        /// </summary>
        protected readonly BiasingParameters BaseConfiguration;

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
        /// Gets the drain current.
        /// </summary>
        /// <value>
        /// The drain current.
        /// </value>
        [ParameterName("id"), ParameterName("cd"), ParameterInfo("Drain current")]
        public double DrainCurrent { get; private set; }

        /// <summary>
        /// Gets the bulk-source current.
        /// </summary>
        /// <value>
        /// The bulk-source current.
        /// </value>
        [ParameterName("ibs"), ParameterInfo("B-S junction current")]
        public double BsCurrent { get; private set; }

        /// <summary>
        /// Gets the bulk-drain current.
        /// </summary>
        /// <value>
        /// The bulk-drain current.
        /// </value>
        [ParameterName("ibd"), ParameterInfo("B-D junction current")]
        public double BdCurrent { get; private set; }

        /// <summary>
        /// Gets the small-signal transconductance.
        /// </summary>
        /// <value>
        /// The transconductance.
        /// </value>
        [ParameterName("gm"), ParameterInfo("Transconductance")]
        public double Transconductance { get; private set; }

        /// <summary>
        /// Gets the small-signal bulk transconductance.
        /// </summary>
        /// <value>
        /// The small-signal bulk transconductance.
        /// </value>
        [ParameterName("gmb"), ParameterName("gmbs"), ParameterInfo("Bulk-Source transconductance")]
        public double TransconductanceBs { get; private set; }

        /// <summary>
        /// Gets the small-signal drain-source conductance.
        /// </summary>
        /// <value>
        /// The small-signal drain-source conductance.
        /// </value>
        [ParameterName("gds"), ParameterInfo("Drain-Source conductance")]
        public double CondDs { get; private set; }

        /// <summary>
        /// Gets the small-signal bulk-source conductance.
        /// </summary>
        /// <value>
        /// The small-signal bulk-source conductance.
        /// </value>
        [ParameterName("gbs"), ParameterInfo("Bulk-Source conductance")]
        public double CondBs { get; private set; }

        /// <summary>
        /// Gets the small-signal bulk-drain conductance.
        /// </summary>
        /// <value>
        /// The small-signal bulk-drain conductance.
        /// </value>
        [ParameterName("gbd"), ParameterInfo("Bulk-Drain conductance")]
        public double CondBd { get; private set; }

        /// <summary>
        /// Gets the turn-on voltage.
        /// </summary>
        /// <value>
        /// The turn-on voltage.
        /// </value>
        [ParameterName("von"), ParameterInfo("Turn-on voltage")]
        public double Von { get; private set; }

        /// <summary>
        /// Gets the saturation voltage.
        /// </summary>
        /// <value>
        /// The saturation voltage.
        /// </value>
        [ParameterName("vdsat"), ParameterInfo("Saturation DrainNode voltage")]
        public double SaturationVoltageDs { get; private set; }

        /// <summary>
        /// Gets the current mode of operation. +1.0 if vds is positive, -1 if it is negative.
        /// </summary>
        /// <value>
        /// The current operation mode.
        /// </value>
        public double Mode { get; private set; }

        /// <summary>
        /// Gets the gate-source voltage.
        /// </summary>
        /// <value>
        /// The gate-source voltage.
        /// </value>
        [ParameterName("vgs"), ParameterInfo("Gate-Source voltage")]
        public virtual double VoltageGs { get; protected set; }

        /// <summary>
        /// Gets the drain-source voltage.
        /// </summary>
        /// <value>
        /// The drain-source voltage.
        /// </value>
        [ParameterName("vds"), ParameterInfo("Drain-Source voltage")]
        public virtual double VoltageDs { get; protected set; }

        /// <summary>
        /// Gets the bulk-source voltage.
        /// </summary>
        /// <value>
        /// The bulk-source voltage
        /// </value>
        [ParameterName("vbs"), ParameterInfo("Bulk-Source voltage")]
        public virtual double VoltageBs { get; protected set; }

        /// <summary>
        /// Gets the bulk-drain voltage.
        /// </summary>
        /// <value>
        /// The bulk-drain voltage.
        /// </value>
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
        /// Initializes a new instance of the <see cref="Biasing"/> class.
        /// </summary>
        /// <param name="name">The name of the behavior.</param>
        /// <param name="context">The context.</param>
        public Biasing(string name, ComponentBindingContext context) 
            : base(name, context)
        {
            context.Nodes.CheckNodes(4);
            BiasingState = context.GetState<IBiasingSimulationState>();
            BaseConfiguration = context.GetSimulationParameterSet<BiasingParameters>();
            context.TryGetState(out _time);
            context.TryGetState(out _method);
            _iteration = context.GetState<IIterationSimulationState>();
            SaturationVoltageDs = 0;
            Von = 0;
            Mode = 1;

            DrainPrime = BiasingState.GetSharedVariable(context.Nodes[0]);
            _drainNode = BiasingState.Map[DrainPrime];
            _gateNode = BiasingState.Map[BiasingState.GetSharedVariable(context.Nodes[1])];
            SourcePrime = BiasingState.GetSharedVariable(context.Nodes[2]);
            _sourceNode = BiasingState.Map[SourcePrime];
            _bulkNode = BiasingState.Map[BiasingState.GetSharedVariable(context.Nodes[3])];

            // Add series drain node if necessary
            if (!ModelParameters.DrainResistance.Equals(0.0) || !ModelParameters.SheetResistance.Equals(0.0) && Parameters.DrainSquares > 0)
                DrainPrime = BiasingState.CreatePrivateVariable(Name.Combine("drain"), Units.Volt);
            _drainNodePrime = BiasingState.Map[DrainPrime];

            // Add series source node if necessary
            if (!ModelParameters.SourceResistance.Equals(0.0) || !ModelParameters.SheetResistance.Equals(0.0) && Parameters.SourceSquares > 0)
                SourcePrime = BiasingState.CreatePrivateVariable(Name.Combine("source"), Units.Volt);
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
        /// Loads the Y-matrix and right hand side vector.
        /// </summary>
        protected virtual void Load()
        {
            // Get the current voltages
            Initialize(out var vgs, out var vds, out var vbs, out var check);
            var vbd = vbs - vds;
            var vgd = vgs - vds;

            /*
			 * bulk - source and bulk - drain diodes
			 * here we just evaluate the ideal diode current and the
			 * corresponding derivative (conductance).
			 */
            if (vbs <= 0)
            {
                CondBs = SourceSatCurrent / Vt;
                BsCurrent = CondBs * vbs;
                CondBs += BaseConfiguration.Gmin;
            }
            else
            {
                var evbs = Math.Exp(Math.Min(MaximumExponentArgument, vbs / Vt));
                CondBs = SourceSatCurrent * evbs / Vt + BaseConfiguration.Gmin;
                BsCurrent = SourceSatCurrent * (evbs - 1);
            }
            if (vbd <= 0)
            {
                CondBd = DrainSatCurrent / Vt;
                BdCurrent = CondBd * vbd;
                CondBd += BaseConfiguration.Gmin;
            }
            else
            {
                var evbd = Math.Exp(Math.Min(MaximumExponentArgument, vbd / Vt));
                CondBd = DrainSatCurrent * evbd / Vt + BaseConfiguration.Gmin;
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
            var ceqbs = ModelParameters.MosfetType * (BsCurrent - (CondBs - BaseConfiguration.Gmin) * vbs);
            var ceqbd = ModelParameters.MosfetType * (BdCurrent - (CondBd - BaseConfiguration.Gmin) * vbd);
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

        void IBiasingBehavior.Load() => Load();

        /// <summary>
        /// Initializes the voltages to be used for calculating the current iteration.
        /// </summary>
        /// <param name="vgs">The gate-source voltage.</param>
        /// <param name="vds">The drain-source voltage.</param>
        /// <param name="vbs">The bulk-source voltage.</param>
        /// <param name="check">If set to <c>true</c>, the current voltage was limited and another iteration should be calculated.</param>
        protected void Initialize(out double vgs, out double vds, out double vbs, out bool check)
        {
            var state = BiasingState;
            check = true;

            if (_iteration.Mode == IterationModes.Float || (_method != null && _method.BaseTime.Equals(0.0)) ||
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
        /// Evaluate drain-source current quantities.
        /// </summary>
        protected double Evaluate(double vgs, double vds, double vbs)
        {
            double von;
            double vdsat, cdrain = 0.0;

            var Vt = Constants.KOverQ * Parameters.Temperature;
            var effectiveLength = Parameters.Length - 2 * ModelParameters.LateralDiffusion;
            var beta = TempTransconductance * Parameters.Width / effectiveLength;
            var oxideCap = ModelParameters.OxideCapFactor * effectiveLength * Parameters.Width;

            /*
            * this routine evaluates the drain current, its derivatives and
            * the charges associated with the gate, channel and bulk
            * for mosfets
            *
            */

            double arg;
            double sarg;
            double[] a4 = new double[4], b4 = new double[4], x4 = new double[8], poly4 = new double[8];
            double dsrgdb, d2Sdb2;
            var sphi = 0.0; /* square root of phi */
            var sphi3 = 0.0; /* square root of phi cubed */
            double barg,
                d2Bdb2,
                dbrgdb,
                argd = 0.0,
                args = 0.0;
            double argxs = 0.0, argxd = 0.0;
            double dgddb2, dgddvb, dgdvds, gamasd;
            double xn = 0.0,
                argg = 0.0,
                vgst,
                dodvds = 0.0,
                dxndvd = 0.0,
                dxndvb = 0.0,
                dudvgs,
                dudvds,
                dudvbs;
            double argv,
                ufact,
                ueff,
                dsdvgs,
                dsdvbs;
            double xvalid = 0.0, bsarg = 0.0;
            double bodys = 0.0, gdbdvs = 0.0;
            double dldvgs = 0.0, dldvds = 0.0, dldvbs = 0.0;
            double xlamda = ModelParameters.Lambda;
            /* 'local' variables - these switch d & s around appropriately
               * so that we don't have to worry about vds < 0
               */
            var lvbs = vbs;
            var lvds = vds;
            var lvgs = vgs;
            var phiMinVbs = TempPhi - lvbs;
            double tmp; /* a temporary variable, not used for more than */
            /* about 10 lines at a time */

            // Compute some useful quantities
            if (lvbs <= 0.0)
            {
                sarg = Math.Sqrt(phiMinVbs);
                dsrgdb = -0.5 / sarg;
                d2Sdb2 = 0.5 * dsrgdb / phiMinVbs;
            }
            else
            {
                sphi = Math.Sqrt(TempPhi);
                sphi3 = TempPhi * sphi;
                sarg = sphi / (1.0 + 0.5 * lvbs / TempPhi);
                tmp = sarg / sphi3;
                dsrgdb = -0.5 * sarg * tmp;
                d2Sdb2 = -dsrgdb * tmp;
            }

            if (lvds - lvbs >= 0)
            {
                barg = Math.Sqrt(phiMinVbs + lvds);
                dbrgdb = -0.5 / barg;
                d2Bdb2 = 0.5 * dbrgdb / (phiMinVbs + lvds);
            }
            else
            {
                barg = sphi / (1.0 + 0.5 * (lvbs - lvds) / TempPhi);
                tmp = barg / sphi3;
                dbrgdb = -0.5 * barg * tmp;
                d2Bdb2 = -dbrgdb * tmp;
            }
            /*
            * calculate threshold voltage (von)
            * narrow - channel effect
            */

            // XXX constant per device
            var factor = 0.125 * ModelParameters.NarrowFactor * 2.0 * Math.PI * EpsilonSilicon / oxideCap * effectiveLength;
            /* XXX constant per device */
            var eta = 1.0 + factor;
            var vbin = TempVoltageBi * ModelParameters.MosfetType + factor * phiMinVbs;
            if (ModelParameters.Gamma > 0.0 || ModelParameters.SubstrateDoping > 0.0)
            {
                var xwd = ModelTemperature.Xd * barg;
                var xws = ModelTemperature.Xd * sarg;

                /*
                * short - channel effect with vds .ne. 0.0
                */

                var argss = 0.0;
                var argsd = 0.0;
                var dbargs = 0.0;
                var dbargd = 0.0;
                dgdvds = 0.0;
                dgddb2 = 0.0;
                if (ModelParameters.JunctionDepth > 0)
                {
                    tmp = 2.0 / ModelParameters.JunctionDepth;
                    argxs = 1.0 + xws * tmp;
                    argxd = 1.0 + xwd * tmp;
                    args = Math.Sqrt(argxs);
                    argd = Math.Sqrt(argxd);
                    tmp = .5 * ModelParameters.JunctionDepth / effectiveLength;
                    argss = tmp * (args - 1.0);
                    argsd = tmp * (argd - 1.0);
                }

                gamasd = ModelParameters.Gamma * (1.0 - argss - argsd);
                var dbxwd = ModelTemperature.Xd * dbrgdb;
                var dbxws = ModelTemperature.Xd * dsrgdb;
                if (ModelParameters.JunctionDepth > 0)
                {
                    tmp = 0.5 / effectiveLength;
                    dbargs = tmp * dbxws / args;
                    dbargd = tmp * dbxwd / argd;
                    var dasdb2 = -ModelTemperature.Xd *
                                 (d2Sdb2 + dsrgdb * dsrgdb * ModelTemperature.Xd / (ModelParameters.JunctionDepth * argxs)) /
                                 (effectiveLength *
                                  args);
                    var daddb2 = -ModelTemperature.Xd *
                                 (d2Bdb2 + dbrgdb * dbrgdb * ModelTemperature.Xd / (ModelParameters.JunctionDepth * argxd)) /
                                 (effectiveLength *
                                  argd);
                    dgddb2 = -0.5 * ModelParameters.Gamma * (dasdb2 + daddb2);
                }

                dgddvb = -ModelParameters.Gamma * (dbargs + dbargd);
                if (ModelParameters.JunctionDepth > 0)
                {
                    var ddxwd = -dbxwd;
                    dgdvds = -ModelParameters.Gamma * 0.5 * ddxwd / (effectiveLength * argd);
                }
            }
            else
            {
                gamasd = ModelParameters.Gamma;
                dgddvb = 0.0;
                dgdvds = 0.0;
                dgddb2 = 0.0;
            }

            von = vbin + gamasd * sarg;
            var vth = von;
            vdsat = 0.0;
            if (!ModelParameters.FastSurfaceStateDensity.Value.Equals(0.0) && !oxideCap.Equals(0.0))
            {
                // XXX constant per model
                var cfs = Constants.Charge * ModelParameters.FastSurfaceStateDensity * 1e4;
                var cdonco = -(gamasd * dsrgdb + dgddvb * sarg) + factor;
                xn = 1.0 + cfs / oxideCap * Parameters.Width * effectiveLength + cdonco;
                tmp = Vt * xn;
                von += tmp;
                argg = 1.0 / tmp;
                vgst = lvgs - von;
            }
            else
            {
                vgst = lvgs - von;
                if (lvgs <= von)
                {
                    // Cutoff region
                    CondDs = 0.0;
                    goto line1050;
                }
            }

            // Compute some more useful quantities
            var sarg3 = sarg * sarg * sarg;

            // XXX constant per model
            var sbiarg = Math.Sqrt(TempBulkPotential);
            var gammad = gamasd;
            var dgdvbs = dgddvb;
            var body = barg * barg * barg - sarg3;
            var gdbdv = 2.0 * gammad * (barg * barg * dbrgdb - sarg * sarg * dsrgdb);
            var dodvbs = -factor + dgdvbs * sarg + gammad * dsrgdb;
            if (ModelParameters.FastSurfaceStateDensity.Value.Equals(0.0))
                goto line400;
            if (oxideCap.Equals(0.0))
                goto line410;
            dxndvb = 2.0 * dgdvbs * dsrgdb + gammad * d2Sdb2 + dgddb2 * sarg;
            dodvbs += Vt * dxndvb;
            dxndvd = dgdvds * dsrgdb;
            dodvds = dgdvds * sarg + Vt * dxndvd;

            // Evaluate effective mobility and its derivatives
            line400:
            if (oxideCap <= 0.0) goto line410;
            var udenom = vgst;
            tmp = ModelParameters.CriticalField * 100 /*cm/m*/ * EpsilonSilicon / ModelParameters.OxideCapFactor;
            if (udenom <= tmp) goto line410;
            ufact = Math.Exp(ModelParameters.CriticalFieldExp * Math.Log(tmp / udenom));
            ueff = ModelParameters.SurfaceMobility * 1e-4 /* (m^2/cm^2) */ * ufact;
            dudvgs = -ufact * ModelParameters.CriticalFieldExp / udenom;
            dudvds = 0.0;
            dudvbs = ModelParameters.CriticalFieldExp * ufact * dodvbs / vgst;
            goto line500;
            line410:
            ufact = 1.0;
            ueff = ModelParameters.SurfaceMobility * 1e-4; /* (m^2/cm^2) */
            dudvgs = 0.0;
            dudvds = 0.0;
            dudvbs = 0.0;
            /*
               * Evaluate saturation voltage and its derivatives according to
               * grove - frohman equation
               */
            line500:
            var vgsx = lvgs;
            gammad = gamasd / eta;
            dgdvbs = dgddvb;
            if (!ModelParameters.FastSurfaceStateDensity.Value.Equals(0.0) && !oxideCap.Equals(0.0))
            {
                vgsx = Math.Max(lvgs, von);
            }

            if (gammad > 0)
            {
                var gammd2 = gammad * gammad;
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
                    dsdvbs = (gammad * (1.0 - arg) + 2.0 * argv / (gammad * arg)) / eta * dgdvbs + 1.0 / arg +
                             factor * dsdvgs;
                }
            }
            else
            {
                vdsat = (vgsx - vbin) / eta;
                vdsat = Math.Max(vdsat, 0.0);
                dsdvgs = 1.0;
                dsdvbs = 0.0;
            }

            if (ModelParameters.MaxDriftVelocity > 0)
            {
                /*
                   * evaluate saturation voltage and its derivatives
                   * according to baum's theory of scattering velocity
                   * saturation
                   */
                var v1 = (vgsx - vbin) / eta + phiMinVbs;
                var v2 = phiMinVbs;
                var xv = ModelParameters.MaxDriftVelocity * effectiveLength / ueff;
                var a1 = gammad / 0.75;
                var b1 = -2.0 * (v1 + xv);
                var c1 = -2.0 * gammad * xv;
                var d1 = 2.0 * v1 * (v2 + xv) - v2 * v2 - 4.0 / 3.0 * gammad * sarg3;
                var a = -b1;
                var b = a1 * c1 - 4.0 * d1;
                var c = -d1 * (a1 * a1 - 4.0 * b1) - c1 * c1;
                var r = -a * a / 3.0 + b;
                var s = 2.0 * a * a * a / 27.0 - a * b / 3.0 + c;
                var r3 = r * r * r;
                var s2 = s * s;
                var p = s2 / 4.0 + r3 / 27.0;
                var p0 = Math.Abs(p);
                var p2 = Math.Sqrt(p0);
                double y3;
                if (p < 0)
                {
                    var ro = Math.Sqrt(s2 / 4.0 + p0);
                    ro = Math.Log(ro) / 3.0;
                    ro = Math.Exp(ro);
                    var fi = Math.Atan(-2.0 * p2 / s);
                    y3 = 2.0 * ro * Math.Cos(fi / 3.0) - a / 3.0;
                }
                else
                {
                    var p3 = -s / 2.0 + p2;
                    p3 = Math.Exp(Math.Log(Math.Abs(p3)) / 3.0);
                    var p4 = -s / 2.0 - p2;
                    p4 = Math.Exp(Math.Log(Math.Abs(p4)) / 3.0);
                    y3 = p3 + p4 - a / 3.0;
                }

                var iknt = 0;
                var a3 = Math.Sqrt(a1 * a1 / 4.0 - b1 + y3);
                var b3 = Math.Sqrt(y3 * y3 / 4.0 - d1);
                for (var i = 1; i <= 4; i++)
                {
                    a4[i - 1] = a1 / 2.0 + _sig1[i - 1] * a3;
                    b4[i - 1] = y3 / 2.0 + _sig2[i - 1] * b3;
                    var delta4 = a4[i - 1] * a4[i - 1] / 4.0 - b4[i - 1];
                    if (delta4 < 0)
                        continue;
                    iknt += 1;
                    tmp = Math.Sqrt(delta4);
                    x4[iknt - 1] = -a4[i - 1] / 2.0 + tmp;
                    iknt += 1;
                    x4[iknt - 1] = -a4[i - 1] / 2.0 - tmp;
                }

                var jknt = 0;
                for (var j = 1; j <= iknt; j++)
                {
                    if (x4[j - 1] <= 0) continue;
                    // XXX implement this sanely
                    poly4[j - 1] = x4[j - 1] * x4[j - 1] * x4[j - 1] * x4[j - 1] +
                                   a1 * x4[j - 1] * x4[j - 1] * x4[j - 1];
                    poly4[j - 1] = poly4[j - 1] + b1 * x4[j - 1] * x4[j - 1] + c1 * x4[j - 1] + d1;
                    if (Math.Abs(poly4[j - 1]) > 1.0e-6)
                        continue;
                    jknt += 1;
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

            // Evaluate effective channel length and its derivatives
            if (!lvds.Equals(0.0))
            {
                gammad = gamasd;
                double dbsrdb;
                if (lvbs - vdsat <= 0)
                {
                    bsarg = Math.Sqrt(vdsat + phiMinVbs);
                    dbsrdb = -0.5 / bsarg;
                }
                else
                {
                    bsarg = sphi / (1.0 + 0.5 * (lvbs - vdsat) / TempPhi);
                    dbsrdb = -0.5 * bsarg * bsarg / sphi3;
                }

                bodys = bsarg * bsarg * bsarg - sarg3;
                gdbdvs = 2.0 * gammad * (bsarg * bsarg * dbsrdb - sarg * sarg * dsrgdb);
                double xlfact;
                double dldsat;
                if (ModelParameters.MaxDriftVelocity <= 0)
                {
                    if (ModelParameters.SubstrateDoping.Value.Equals(0.0))
                        goto line610;
                    if (xlamda > 0.0)
                        goto line610;
                    argv = (lvds - vdsat) / 4.0;
                    var sargv = Math.Sqrt(1.0 + argv * argv);
                    arg = Math.Sqrt(argv + sargv);
                    xlfact = ModelTemperature.Xd / (effectiveLength * lvds);
                    xlamda = xlfact * arg;
                    dldsat = lvds * xlamda / (8.0 * sargv);
                }
                else
                {
                    argv = (vgsx - vbin) / eta - vdsat;
                    var xdv = ModelTemperature.Xd / Math.Sqrt(ModelParameters.ChannelCharge);
                    var xlv = ModelParameters.MaxDriftVelocity * xdv / (2.0 * ueff);
                    var vqchan = argv - gammad * bsarg;
                    var dqdsat = -1.0 + gammad * dbsrdb;
                    var vl = ModelParameters.MaxDriftVelocity * effectiveLength;
                    var dfunds = vl * dqdsat - ueff * vqchan;
                    var dfundg = (vl - ueff * vdsat) / eta;
                    var dfundb = -vl * (1.0 + dqdsat - factor / eta) + ueff * (gdbdvs - dgdvbs * bodys / 1.5) / eta;
                    dsdvgs = -dfundg / dfunds;
                    dsdvbs = -dfundb / dfunds;
                    if (ModelParameters.SubstrateDoping.Value.Equals(0.0))
                        goto line610;
                    if (xlamda > 0.0)
                        goto line610;
                    argv = lvds - vdsat;
                    argv = Math.Max(argv, 0.0);
                    var xls = Math.Sqrt(xlv * xlv + argv);
                    dldsat = xdv / (2.0 * xls);
                    xlfact = xdv / (effectiveLength * lvds);
                    xlamda = xlfact * (xls - xlv);
                    dldsat /= effectiveLength;
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

            // Limit channel shortening at punch - through
            var xwb = ModelTemperature.Xd * sbiarg;
            var xld = effectiveLength - xwb;
            var clfact = 1.0 - xlamda * lvds;
            dldvds = -xlamda - dldvds;
            var xleff = effectiveLength * clfact;
            var deltal = xlamda * lvds * effectiveLength;
            if (ModelParameters.SubstrateDoping.Value.Equals(0.0))
                xwb = 0.25e-6;
            if (xleff < xwb)
            {
                xleff = xwb / (1.0 + (deltal - xld) / xwb);
                clfact = xleff / effectiveLength;
                var dfact = xleff * xleff / (xwb * xwb);
                dldvgs = dfact * dldvgs;
                dldvds = dfact * dldvds;
                dldvbs = dfact * dldvbs;
            }

            /*
               * evaluate effective beta (effective kp)
               */
            var beta1 = beta * ufact / clfact;
            /*
               * test for mode of operation and branch appropriately
               */
            gammad = gamasd;
            dgdvbs = dgddvb;
            if (lvds <= 1.0e-10)
            {
                if (lvgs <= von)
                {
                    if (ModelParameters.FastSurfaceStateDensity.Value.Equals(0.0) || oxideCap.Equals(0.0))
                    {
                        CondDs = 0.0;
                        goto line1050;
                    }

                    CondDs = beta1 * (von - vbin - gammad * sarg) * Math.Exp(argg * (lvgs - von));
                    goto line1050;
                }

                CondDs = beta1 * (lvgs - vbin - gammad * sarg);
                goto line1050;
            }

            if (lvgs > von)
                goto line900;

            // Subthreshold region
            if (vdsat <= 0)
            {
                CondDs = 0.0;
                if (lvgs > vth)
                    goto doneval;
                goto line1050;
            }

            var vdson = Math.Min(vdsat, lvds);
            if (lvds > vdsat)
            {
                barg = bsarg;
                body = bodys;
                gdbdv = gdbdvs;
            }

            var cdson = beta1 * ((von - vbin - eta * vdson * 0.5) * vdson - gammad * body / 1.5);
            var didvds = beta1 * (von - vbin - eta * vdson - gammad * barg);
            var gdson = -cdson * dldvds / clfact - beta1 * dgdvds * body / 1.5;
            if (lvds < vdsat)
                gdson += didvds;
            var gbson = -cdson * dldvbs / clfact +
                        beta1 * (dodvbs * vdson + factor * vdson - dgdvbs * body / 1.5 - gdbdv);
            if (lvds > vdsat)
                gbson += didvds * dsdvbs;
            var expg = Math.Exp(argg * (lvgs - von));
            cdrain = cdson * expg;
            var gmw = cdrain * argg;
            Transconductance = gmw;
            if (lvds > vdsat)
                Transconductance = gmw + didvds * dsdvgs * expg;
            tmp = gmw * (lvgs - von) / xn;
            CondDs = gdson * expg - Transconductance * dodvds - tmp * dxndvd;
            TransconductanceBs = gbson * expg - Transconductance * dodvbs - tmp * dxndvb;
            goto doneval;

            line900:
            if (lvds <= vdsat)
            {
                // Linear region
                cdrain = beta1 * ((lvgs - vbin - eta * lvds / 2.0) * lvds - gammad * body / 1.5);
                arg = cdrain * (dudvgs / ufact - dldvgs / clfact);
                Transconductance = arg + beta1 * lvds;
                arg = cdrain * (dudvds / ufact - dldvds / clfact);
                CondDs = arg + beta1 * (lvgs - vbin - eta * lvds - gammad * barg - dgdvds * body / 1.5);
                arg = cdrain * (dudvbs / ufact - dldvbs / clfact);
                TransconductanceBs = arg - beta1 * (gdbdv + dgdvbs * body / 1.5 - factor * lvds);
            }
            else
            {
                // Saturation region
                cdrain = beta1 * ((lvgs - vbin - eta * vdsat / 2.0) * vdsat - gammad * bodys / 1.5);
                arg = cdrain * (dudvgs / ufact - dldvgs / clfact);
                Transconductance = arg + beta1 * vdsat + beta1 * (lvgs - vbin - eta * vdsat - gammad * bsarg) * dsdvgs;
                CondDs = -cdrain * dldvds / clfact - beta1 * dgdvds * bodys / 1.5;
                arg = cdrain * (dudvbs / ufact - dldvbs / clfact);
                TransconductanceBs = arg - beta1 * (gdbdvs + dgdvbs * bodys / 1.5 - factor * vdsat) + beta1 *
                                     (lvgs - vbin - eta * vdsat - gammad *
                                      bsarg) * dsdvbs;
            }

            // Compute charges for "on" region
            goto doneval;

            // Finish special cases
            line1050:
            cdrain = 0.0;
            Transconductance = 0.0;
            TransconductanceBs = 0.0;
            // Finished

            doneval:
            Von = ModelParameters.MosfetType * von;
            SaturationVoltageDs = ModelParameters.MosfetType * vdsat;
            return cdrain;
        }

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
            var tol = BaseConfiguration.RelativeTolerance * Math.Max(Math.Abs(cdhat), Math.Abs(DrainCurrent)) + BaseConfiguration.AbsoluteTolerance;
            if (Math.Abs(cdhat - DrainCurrent) >= tol)
            {
                _iteration.IsConvergent = false;
                return false;
            }

            tol = BaseConfiguration.RelativeTolerance * Math.Max(Math.Abs(cbhat), Math.Abs(BsCurrent + BdCurrent)) + BaseConfiguration.AbsoluteTolerance;
            if (Math.Abs(cbhat - (BsCurrent + BdCurrent)) > tol)
            {
                _iteration.IsConvergent = false;
                return false;
            }
            return true;
        }
    }
}