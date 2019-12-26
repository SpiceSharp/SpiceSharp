﻿using System;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.Semiconductors;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;

namespace SpiceSharp.Components.MosfetBehaviors.Level3
{
    /// <summary>
    /// General behavior for a <see cref="Mosfet3"/>
    /// </summary>
    public class BiasingBehavior : TemperatureBehavior, IBiasingBehavior, IConvergenceBehavior
    {
        /// <summary>
        /// The permittivity of silicon
        /// </summary>
        protected const double EpsilonSilicon = 11.7 * 8.854214871e-12;

        /// <summary>
        /// The maximum exponent argument.
        /// </summary>
        protected const double MaximumExponentArgument = 709.0;

       /// <summary>
        /// Gets the base configuration.
        /// </summary>
        protected BiasingConfiguration BaseConfiguration { get; private set; }

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
        /// Gets or sets the bulk-drain conductance.
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
        /// Gets the (internal) drain node.
        /// </summary>
        protected Variable DrainPrime { get; private set; }

        /// <summary>
        /// Gets the (internal) source node.
        /// </summary>
        protected Variable SourcePrime { get; private set; }

        /// <summary>
        /// Gets the matrix elements.
        /// </summary>
        /// <value>
        /// The matrix elements.
        /// </value>
        protected ElementSet<double> Elements { get; private set; }

        private IIntegrationMethod _method;
        private int _drainNode, _gateNode, _sourceNode, _bulkNode, _drainNodePrime, _sourceNodePrime;

        /// <summary>
        /// Initializes a new instance of the <see cref="BiasingBehavior"/> class.
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="context"></param>
        public BiasingBehavior(string name, ComponentBindingContext context) : base(name, context)
        {
            context.Nodes.CheckNodes(4);

            BaseConfiguration = context.Configurations.GetValue<BiasingConfiguration>();
            context.TryGetState(out _method);
            SaturationVoltageDs = 0;
            Von = 0;
            Mode = 1;

            _drainNode = BiasingState.Map[context.Nodes[0]];
            _gateNode = BiasingState.Map[context.Nodes[1]];
            _sourceNode = BiasingState.Map[context.Nodes[2]];
            _bulkNode = BiasingState.Map[context.Nodes[3]];
            var variables = context.Variables;

            // Add series drain node if necessary
            if (ModelParameters.DrainResistance > 0 || ModelParameters.SheetResistance > 0 && BaseParameters.DrainSquares > 0)
                DrainPrime = variables.Create(Name.Combine("drain"), VariableType.Voltage);
            else
                DrainPrime = context.Nodes[0];
            _drainNodePrime = BiasingState.Map[DrainPrime];

            // Add series source node if necessary
            if (ModelParameters.SourceResistance > 0 || ModelParameters.SheetResistance > 0 && BaseParameters.SourceSquares > 0)
                SourcePrime = variables.Create(Name.Combine("source"), VariableType.Voltage);
            else
                SourcePrime = context.Nodes[2];
            _sourceNodePrime = BiasingState.Map[SourcePrime];

            // Get matrix pointers
            Elements = new ElementSet<double>(BiasingState.Solver, new[] {
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
            var state = BiasingState.ThrowIfNotBound(this);

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
            if (!BaseParameters.Off || state.Init != InitializationModes.Fix)
            {
                if (check)
                    state.IsConvergent = false;
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

            Elements.Add(
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
        /// Initializes the voltages to be used for the current iteration.
        /// </summary>
        /// <param name="vgs">The VGS.</param>
        /// <param name="vds">The VDS.</param>
        /// <param name="vbs">The VBS.</param>
        /// <param name="check">if set to <c>true</c> [check].</param>
        protected void Initialize(out double vgs, out double vds, out double vbs, out bool check)
        {
            var state = BiasingState;
            check = true;

            if (state.Init == InitializationModes.Float || (_method != null && _method.BaseTime.Equals(0.0)) ||
                state.Init == InitializationModes.Fix && !BaseParameters.Off)
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
                if (state.Init == InitializationModes.Junction && !BaseParameters.Off)
                {
                    vds = ModelParameters.MosfetType * BaseParameters.InitialVoltageDs;
                    vgs = ModelParameters.MosfetType * BaseParameters.InitialVoltageGs;
                    vbs = ModelParameters.MosfetType * BaseParameters.InitialVoltageBs;

                    // TODO: At some point, check what this is supposed to do
                    if (vds.Equals(0) && vgs.Equals(0) && vbs.Equals(0) && (state.UseDc || !state.UseIc))
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
            double vdsat, cdrain;
            var Vt = Constants.KOverQ * BaseParameters.Temperature;
            var effectiveLength = BaseParameters.Length - 2 * ModelParameters.LateralDiffusion;
            var beta = TempTransconductance * BaseParameters.Width / effectiveLength;
            var oxideCap = ModelParameters.OxideCapFactor * effectiveLength * BaseParameters.Width;
            double von;

            {
                var coeff0 = 0.0631353e0;
                var coeff1 = 0.8013292e0;
                var coeff2 = -0.01110777e0;
                double phibs; /* phi - vbs */
                double sqphbs; /* square root of phibs */
                double dsqdvb; /*  */
                double arga, argb, argc;
                double dfsdvb;
                double dxndvb = 0.0, dvodvb = 0.0, dvodvd = 0.0,
                    dvsdvg, dvsdvb, dvsdvd, xn = 0.0;
                var onvdsc = 0.0;
                var fdrain = 0.0;
                double dfddvg = 0.0, dfddvb = 0.0,
                    dfddvd = 0.0,
                    delxl, dldvd,
                    ddldvg, ddldvd, ddldvb,
                    gds0 = 0.0;
                double fshort;

                /*
				 * reference cdrain equations to source and
				 * charge equations to bulk
				 */
                vdsat = 0.0;
                var oneoverxl = 1.0 / effectiveLength;
                var eta = ModelParameters.Eta * 8.15e-22 / (ModelParameters.OxideCapFactor * effectiveLength * effectiveLength * effectiveLength);
                /*
				* .....square root term
				*/
                if (vbs <= 0.0)
                {
                    phibs = TempPhi - vbs;
                    sqphbs = Math.Sqrt(phibs);
                    dsqdvb = -0.5 / sqphbs;
                }
                else
                {
                    var sqphis = Math.Sqrt(TempPhi); /* square root of phi */
                    var sqphs3 = TempPhi * sqphis; /* square root of phi cubed */
                    sqphbs = sqphis / (1.0 + vbs / (TempPhi + TempPhi));
                    phibs = sqphbs * sqphbs;
                    dsqdvb = -phibs / (sqphs3 + sqphs3);
                }
                /*
				 * .....short channel effect factor
				 */
                if (ModelParameters.JunctionDepth > 0 && ModelTemperature.CoefficientDepletionLayerWidth > 0.0)
                {
                    var wps = ModelTemperature.CoefficientDepletionLayerWidth * sqphbs;
                    var oneoverxj = 1.0 / ModelParameters.JunctionDepth; /* 1 / junction depth */
                    var xjonxl = ModelParameters.JunctionDepth * oneoverxl; /* junction depth / effective length */
                    var djonxj = ModelParameters.LateralDiffusion * oneoverxj;
                    var wponxj = wps * oneoverxj;
                    var wconxj = coeff0 + coeff1 * wponxj + coeff2 * wponxj * wponxj;
                    arga = wconxj + djonxj;
                    argc = wponxj / (1.0 + wponxj);
                    argb = Math.Sqrt(1.0 - argc * argc);
                    fshort = 1.0 - xjonxl * (arga * argb - djonxj);
                    var dwpdvb = ModelTemperature.CoefficientDepletionLayerWidth * dsqdvb;
                    var dadvb = (coeff1 + coeff2 * (wponxj + wponxj)) * dwpdvb * oneoverxj;
                    var dbdvb = -argc * argc * (1.0 - argc) * dwpdvb / (argb * wps);
                    dfsdvb = -xjonxl * (dadvb * argb + arga * dbdvb);
                }
                else
                {
                    fshort = 1.0;
                    dfsdvb = 0.0;
                }
                /*
				 * .....body effect
				 */
                var gammas = ModelParameters.Gamma * fshort;
                var fbodys = 0.5 * gammas / (sqphbs + sqphbs);
                var fbody = fbodys + ModelParameters.NarrowFactor / BaseParameters.Width;
                var onfbdy = 1.0 / (1.0 + fbody);
                var dfbdvb = -fbodys * dsqdvb / sqphbs + fbodys * dfsdvb / fshort;
                var qbonco = gammas * sqphbs + ModelParameters.NarrowFactor * phibs / BaseParameters.Width;
                var dqbdvb = gammas * dsqdvb + ModelParameters.Gamma * dfsdvb * sqphbs - ModelParameters.NarrowFactor / BaseParameters.Width;
                /*
				 * .....static feedback effect
				 */
                var vbix = TempVoltageBi * ModelParameters.MosfetType - eta * vds;
                /*
				 * .....threshold voltage
				 */
                var vth = vbix + qbonco;
                var dvtdvd = -eta;
                var dvtdvb = dqbdvb;
                /*
				 * .....joint weak inversion and strong inversion
				 */
                von = vth;
                if (ModelParameters.FastSurfaceStateDensity > 0.0)
                {
                    var csonco = Constants.Charge * ModelParameters.FastSurfaceStateDensity * 1e4 /* (cm *  * 2 / m *  * 2) */  * effectiveLength * BaseParameters.Width /
                                    oxideCap;
                    var cdonco = qbonco / (phibs + phibs);
                    xn = 1.0 + csonco + cdonco;
                    von = vth + Vt * xn;
                    dxndvb = dqbdvb / (phibs + phibs) - qbonco * dsqdvb / (phibs * sqphbs);
                    dvodvd = dvtdvd;
                    dvodvb = dvtdvb + Vt * dxndvb;
                }
                else
                {
                    /*
					 * .....cutoff region
					 */
                    if (vgs <= von)
                    {
                        cdrain = 0.0;
                        Transconductance = 0.0;
                        CondDs = 0.0;
                        TransconductanceBs = 0.0;
                        goto innerline1000;
                    }
                }
                /*
				 * .....device is on
				 */
                var vgsx = Math.Max(vgs, von);
                /*
				 * .....mobility modulation by gate voltage
				 */
                var onfg = 1.0 + ModelParameters.Theta * (vgsx - vth);
                var fgate = 1.0 / onfg;
                var us = TempSurfaceMobility * 1e-4 /*(m**2/cm**2)*/ * fgate;
                var dfgdvg = -ModelParameters.Theta * fgate * fgate;
                var dfgdvd = -dfgdvg * dvtdvd;
                var dfgdvb = -dfgdvg * dvtdvb;
                /*
				 * .....saturation voltage
				 */
                vdsat = (vgsx - vth) * onfbdy;
                if (ModelParameters.MaxDriftVelocity <= 0.0)
                {
                    dvsdvg = onfbdy;
                    dvsdvd = -dvsdvg * dvtdvd;
                    dvsdvb = -dvsdvg * dvtdvb - vdsat * dfbdvb * onfbdy;
                }
                else
                {
                    var vdsc = effectiveLength * ModelParameters.MaxDriftVelocity / us;
                    onvdsc = 1.0 / vdsc;
                    arga = (vgsx - vth) * onfbdy;
                    argb = Math.Sqrt(arga * arga + vdsc * vdsc);
                    vdsat = arga + vdsc - argb;
                    var dvsdga = (1.0 - arga / argb) * onfbdy;
                    dvsdvg = dvsdga - (1.0 - vdsc / argb) * vdsc * dfgdvg * onfg;
                    dvsdvd = -dvsdvg * dvtdvd;
                    dvsdvb = -dvsdvg * dvtdvb - arga * dvsdga * dfbdvb;
                }
                /*
				 * .....current factors in linear region
				 */
                var vdsx = Math.Min(vds, vdsat);
                if (vdsx.Equals(0.0))
                    goto line900;
                var cdo = vgsx - vth - 0.5 * (1.0 + fbody) * vdsx;
                var dcodvb = -dvtdvb - 0.5 * dfbdvb * vdsx;
                /*
				 * .....normalized drain current
				 */
                var cdnorm = cdo * vdsx;
                Transconductance = vdsx;
                CondDs = vgsx - vth - (1.0 + fbody + dvtdvd) * vdsx;
                TransconductanceBs = dcodvb * vdsx;
                /*
				 * .....drain current without velocity saturation effect
				 */
                var cd1 = beta * cdnorm;
                beta = beta * fgate;
                cdrain = beta * cdnorm;
                Transconductance = beta * Transconductance + dfgdvg * cd1;
                CondDs = beta * CondDs + dfgdvd * cd1;
                TransconductanceBs = beta * TransconductanceBs;
                /*
				 * .....velocity saturation factor
				 */
                if (ModelParameters.MaxDriftVelocity > 0.0)
                {
                    fdrain = 1.0 / (1.0 + vdsx * onvdsc);
                    var fd2 = fdrain * fdrain;
                    arga = fd2 * vdsx * onvdsc * onfg;
                    dfddvg = -dfgdvg * arga;
                    dfddvd = -dfgdvd * arga - fd2 * onvdsc;
                    dfddvb = -dfgdvb * arga;
                    /*
					 * .....drain current
					 */
                    Transconductance = fdrain * Transconductance + dfddvg * cdrain;
                    CondDs = fdrain * CondDs + dfddvd * cdrain;
                    TransconductanceBs = fdrain * TransconductanceBs + dfddvb * cdrain;
                    cdrain = fdrain * cdrain;
                }
                /*
				 * .....channel length modulation
				 */
                if (vds <= vdsat) goto line700;
                if (ModelParameters.MaxDriftVelocity <= 0.0) goto line510;
                if (ModelTemperature.Alpha.Equals(0.0))
                    goto line700;
                var cdsat = cdrain;
                var gdsat = cdsat * (1.0 - fdrain) * onvdsc;
                gdsat = Math.Max(1.0e-12, gdsat);
                var gdoncd = gdsat / cdsat;
                var gdonfd = gdsat / (1.0 - fdrain);
                var gdonfg = gdsat * onfg;
                var dgdvg = gdoncd * Transconductance - gdonfd * dfddvg + gdonfg * dfgdvg;
                var dgdvd = gdoncd * CondDs - gdonfd * dfddvd + gdonfg * dfgdvd;
                var dgdvb = gdoncd * TransconductanceBs - gdonfd * dfddvb + gdonfg * dfgdvb;

                var emax = ModelParameters.Kappa * cdsat * oneoverxl / gdsat;
                var emoncd = emax / cdsat;
                var emongd = emax / gdsat;
                var demdvg = emoncd * Transconductance - emongd * dgdvg;
                var demdvd = emoncd * CondDs - emongd * dgdvd;
                var demdvb = emoncd * TransconductanceBs - emongd * dgdvb;

                arga = 0.5 * emax * ModelTemperature.Alpha;
                argc = ModelParameters.Kappa * ModelTemperature.Alpha;
                argb = Math.Sqrt(arga * arga + argc * (vds - vdsat));
                delxl = argb - arga;
                dldvd = argc / (argb + argb);
                var dldem = 0.5 * (arga / argb - 1.0) * ModelTemperature.Alpha;
                ddldvg = dldem * demdvg;
                ddldvd = dldem * demdvd - dldvd;
                ddldvb = dldem * demdvb;
                goto line520;
                line510:
                delxl = Math.Sqrt(ModelParameters.Kappa * (vds - vdsat) * ModelTemperature.Alpha);
                dldvd = 0.5 * delxl / (vds - vdsat);
                ddldvg = 0.0;
                ddldvd = -dldvd;
                ddldvb = 0.0;
                /*
				 * .....punch through approximation
				 */
                line520:
                if (delxl > 0.5 * effectiveLength)
                {
                    delxl = effectiveLength - effectiveLength * effectiveLength / (4.0 * delxl);
                    arga = 4.0 * (effectiveLength - delxl) * (effectiveLength - delxl) / (effectiveLength * effectiveLength);
                    ddldvg = ddldvg * arga;
                    ddldvd = ddldvd * arga;
                    ddldvb = ddldvb * arga;
                    dldvd = dldvd * arga;
                }
                /*
				 * .....saturation region
				 */
                var dlonxl = delxl * oneoverxl;
                var xlfact = 1.0 / (1.0 - dlonxl);
                cdrain = cdrain * xlfact;
                var diddl = cdrain / (effectiveLength - delxl);
                Transconductance = Transconductance * xlfact + diddl * ddldvg;
                gds0 = CondDs * xlfact + diddl * ddldvd;
                TransconductanceBs = TransconductanceBs * xlfact + diddl * ddldvb;
                Transconductance = Transconductance + gds0 * dvsdvg;
                TransconductanceBs = TransconductanceBs + gds0 * dvsdvb;
                CondDs = gds0 * dvsdvd + diddl * dldvd;
                /*
				 * .....finish strong inversion case
				 */
                line700:
                if (vgs < von)
                {
                    /*
					 * .....weak inversion
					 */
                    var onxn = 1.0 / xn;
                    var ondvt = onxn / Vt;
                    var wfact = Math.Exp((vgs - von) * ondvt);
                    cdrain = cdrain * wfact;
                    var gms = Transconductance * wfact;
                    var gmw = cdrain * ondvt;
                    Transconductance = gmw;
                    if (vds > vdsat)
                    {
                        Transconductance = Transconductance + gds0 * dvsdvg * wfact;
                    }
                    CondDs = CondDs * wfact + (gms - gmw) * dvodvd;
                    TransconductanceBs = TransconductanceBs * wfact + (gms - gmw) * dvodvb - gmw * (vgs - von) * onxn * dxndvb;
                }
                /*
				 * .....charge computation
				 */
                goto innerline1000;
                /*
				 * .....special case of vds = 0.0d0 */
                line900:
                beta = beta * fgate;
                cdrain = 0.0;
                Transconductance = 0.0;
                CondDs = beta * (vgsx - vth);
                TransconductanceBs = 0.0;
                if (ModelParameters.FastSurfaceStateDensity > 0.0 && vgs < von)
                {
                    CondDs *= Math.Exp((vgs - von) / (Vt * xn));
                }
                innerline1000:;
                /*
				 * .....done
				 */
            }

            Von = ModelParameters.MosfetType * von;
            SaturationVoltageDs = ModelParameters.MosfetType * vdsat;
            return cdrain;
        }

        /// <summary>
        /// Tests convergence at the device-level.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the device determines the solution converges; otherwise, <c>false</c>.
        /// </returns>
        bool IConvergenceBehavior.IsConvergent()
        {
            var state = BiasingState.ThrowIfNotBound(this);
            double cdhat;

            var vbs = ModelParameters.MosfetType * (state.Solution[_bulkNode] - state.Solution[_sourceNodePrime]);
            var vgs = ModelParameters.MosfetType * (state.Solution[_gateNode] - state.Solution[_sourceNodePrime]);
            var vds = ModelParameters.MosfetType * (state.Solution[_drainNodePrime] - state.Solution[_sourceNodePrime]);
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
                state.IsConvergent = false;
                return false;
            }

            tol = BaseConfiguration.RelativeTolerance * Math.Max(Math.Abs(cbhat), Math.Abs(BsCurrent + BdCurrent)) + BaseConfiguration.AbsoluteTolerance;
            if (Math.Abs(cbhat - (BsCurrent + BdCurrent)) > tol)
            {
                state.IsConvergent = false;
                return false;
            }
            return true;
        }
    }
}
