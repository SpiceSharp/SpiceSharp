using System;
using SpiceSharp.Circuits;
using SpiceSharp.Attributes;
using SpiceSharp.Sparse;
using SpiceSharp.Simulations;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.MosfetBehaviors.Level2
{
    /// <summary>
    /// General behavior of a <see cref="Mosfet2"/>
    /// </summary>
    public class LoadBehavior : Behaviors.LoadBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary behaviors and parameters
        /// </summary>
        BaseParameters bp;
        ModelBaseParameters mbp;
        TemperatureBehavior temp;
        ModelTemperatureBehavior modeltemp;

        /// <summary>
        /// Some signs used in the model
        /// </summary>
        static double[] sig1 = { 1.0, -1.0, 1.0, -1.0 };
        static double[] sig2 = { 1.0, 1.0, -1.0, -1.0 };

        /// <summary>
        /// Shared parameters
        /// </summary>
        [PropertyName("von"), PropertyInfo(" ")]
        public double Von { get; protected set; }
        [PropertyName("vdsat"), PropertyInfo("Saturation drain voltage")]
        public double SaturationVoltageDS { get; protected set; }
        [PropertyName("id"), PropertyName("cd"), PropertyInfo("Drain current")]
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
        public double VoltageGS { get; protected set; }
        public double VoltageDS { get; protected set; }
        public double VoltageBS { get; protected set; }
        public double VoltageBD { get; protected set; }

        /// <summary>
        /// Nodes
        /// </summary>
        int drainNode, gateNode, sourceNode, bulkNode;
        [PropertyName("dnodeprime"), PropertyInfo("Number of protected drain node")]
        public int DrainNodePrime { get; private set; }
        [PropertyName("snodeprime"), PropertyInfo("Number of protected source node")]
        public int SourceNodePrime { get; private set; }
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
            bp = provider.GetParameterSet<BaseParameters>(0);
            mbp = provider.GetParameterSet<ModelBaseParameters>(1);

            // Get behaviors
            temp = provider.GetBehavior<TemperatureBehavior>(0);
            modeltemp = provider.GetBehavior<ModelTemperatureBehavior>(1);

            // Reset
            SaturationVoltageDS = 0.0;
            Von = 0.0;
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
        public override void GetMatrixPointers(Nodes nodes, Matrix<double> matrix)
        {
            if (nodes == null)
                throw new ArgumentNullException(nameof(nodes));
            if (matrix == null)
                throw new ArgumentNullException(nameof(matrix));

            // Add a series drain node if necessary
            if (mbp.DrainResistance > 0 || (bp.DrainSquares != 0 && mbp.SheetResistance > 0))
                DrainNodePrime = nodes.Create(Name.Grow("#drain")).Index;
            else
                DrainNodePrime = drainNode;

            // Add a series source node if necessary
            if (mbp.SourceResistance > 0 || (bp.SourceSquares != 0 && mbp.SheetResistance > 0))
                SourceNodePrime = nodes.Create(Name.Grow("#source")).Index;
            else
                SourceNodePrime = sourceNode;

            // Get matrix elements
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
        /// Unsetup the behavior
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
            var rstate = state;
            double vt, EffectiveLength, DrainSatCur, SourceSatCur, Beta,
                OxideCap, vgs, vds, vbs, vbd, vgd, vgdo, von, evbs, evbd,
                vdsat, cdrain = 0.0, ceqbs,
                ceqbd, cdreq;
            int Check, xnrm, xrev;

            vt = Circuit.KOverQ * bp.Temperature;
            Check = 1;

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
            OxideCap = modeltemp.OxideCapFactor * EffectiveLength * bp.Width;

            if ((state.Init == RealState.InitializationStates.InitFloat || state.UseSmallSignal || (state.Init == RealState.InitializationStates.InitTransient)) ||
                ((state.Init == RealState.InitializationStates.InitFix) && (!bp.Off)))
            {
                // general iteration
                vbs = mbp.MosfetType * (rstate.Solution[bulkNode] - rstate.Solution[SourceNodePrime]);
                vgs = mbp.MosfetType * (rstate.Solution[gateNode] - rstate.Solution[SourceNodePrime]);
                vds = mbp.MosfetType * (rstate.Solution[DrainNodePrime] - rstate.Solution[SourceNodePrime]);

                // now some common crunching for some more useful quantities
                vbd = vbs - vds;
                vgd = vgs - vds;
                vgdo = VoltageGS - VoltageDS;

                von = mbp.MosfetType * Von;

                /* 
				* limiting
				* We want to keep device voltages from changing
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
				* look at other possibilities 
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

            /* now all the preliminaries are over - we can start doing the
			* real work 
			*/
            vbd = vbs - vds;
            vgd = vgs - vds;

            /* bulk - source and bulk - drain doides
			* here we just evaluate the ideal diode current and the
			* correspoinding derivative (conductance).
			*/

            if (vbs <= 0)
            {
                CondBS = SourceSatCur / vt;
                BSCurrent = CondBS * vbs;
                CondBS += state.Gmin;
            }
            else
            {
                evbs = Math.Exp(vbs / vt);
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
                evbd = Math.Exp(vbd / vt);
                CondBD = DrainSatCur * evbd / vt + state.Gmin;
                BDCurrent = DrainSatCur * (evbd - 1);
            }
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
            {
                /* moseq2(vds, vbs, vgs, gm, gds, gmbs, qg, qc, qb, 
				* cggb, cgdb, cgsb, cbgb, cbdb, cbsb)
				*/
                /* note:  cgdb, cgsb, cbdb, cbsb never used */

                /* 
				* this routine evaluates the drain current, its derivatives and
				* the charges associated with the gate, channel and bulk
				* for mosfets
				* 
				*/

                double arg;
                double sarg;
                double[] a4 = new double[4], b4 = new double[4], x4 = new double[8], poly4 = new double[8];
                double beta1, dsrgdb, d2sdb2;
                double sphi = 0.0; /* square root of phi */
                double sphi3 = 0.0; /* square root of phi cubed */
                double barg, d2bdb2, factor, dbrgdb, eta, vbin, argd = 0.0, args = 0.0, argss, argsd, argxs = 0.0, argxd = 0.0, daddb2, dasdb2, dbargd, dbargs, dbxwd, dbxws,
                    dgddb2, dgddvb, dgdvds, gamasd, xwd, xws, ddxwd, gammad, vth, cfs, cdonco, xn = 0.0, argg = 0.0, vgst, sarg3, sbiarg, dgdvbs, body, gdbdv,
                    dodvbs, dodvds = 0.0, dxndvd = 0.0, dxndvb = 0.0, udenom, dudvgs, dudvds, dudvbs, gammd2, argv, vgsx, ufact, ueff, dsdvgs, dsdvbs, a1, a3, a, b1,
                    b3, b, c1, c, d1, fi, p0, p2, p3, p4, p, r3, r, ro, s2, s, v1, v2, xv, y3, delta4, xvalid = 0.0, bsarg = 0.0, dbsrdb = 0.0, bodys = 0.0, gdbdvs = 0.0, sargv,
                    xlfact, dldsat, xdv, xlv, vqchan, dqdsat, vl, dfundg, dfunds, dfundb, xls, dldvgs = 0.0, dldvds = 0.0, dldvbs = 0.0, dfact, clfact, xleff, deltal,
                    xwb, vdson, cdson, didvds, gdson, gmw, gbson, expg, xld;
                double xlamda = mbp.Lambda;
                /* 'local' variables - these switch d & s around appropriately
				 * so that we don't have to worry about vds < 0
				 */
                double lvbs = Mode  > 0 ? vbs : vbd;
                double lvds = Mode * vds;
                double lvgs = Mode > 0 ? vgs : vgd;
                double phiMinVbs = temp.TempPhi - lvbs;
                double tmp; /* a temporary variable, not used for more than */
                            /* about 10 lines at a time */
                int iknt, jknt, i, j;

                /* 
				* compute some useful quantities
				*/

                if (lvbs <= 0.0)
                {
                    sarg = Math.Sqrt(phiMinVbs);
                    dsrgdb = -0.5 / sarg;
                    d2sdb2 = 0.5 * dsrgdb / phiMinVbs;
                }
                else
                {
                    sphi = Math.Sqrt(temp.TempPhi);
                    sphi3 = temp.TempPhi * sphi;
                    sarg = sphi / (1.0 + 0.5 * lvbs / temp.TempPhi);
                    tmp = sarg / sphi3;
                    dsrgdb = -0.5 * sarg * tmp;
                    d2sdb2 = -dsrgdb * tmp;
                }
                if ((lvds - lvbs) >= 0)
                {
                    barg = Math.Sqrt(phiMinVbs + lvds);
                    dbrgdb = -0.5 / barg;
                    d2bdb2 = 0.5 * dbrgdb / (phiMinVbs + lvds);
                }
                else
                {
                    barg = sphi / (1.0 + 0.5 * (lvbs - lvds) / temp.TempPhi);
                    tmp = barg / sphi3;
                    dbrgdb = -0.5 * barg * tmp;
                    d2bdb2 = -dbrgdb * tmp;
                }
                /* 
				* calculate threshold voltage (von)
				* narrow - channel effect
				*/

                /* XXX constant per device */
                factor = 0.125 * mbp.NarrowFactor * 2.0 * Math.PI * Transistor.EpsilonSilicon / OxideCap * EffectiveLength;
                /* XXX constant per device */
                eta = 1.0 + factor;
                vbin = temp.TempVoltageBI * mbp.MosfetType + factor * phiMinVbs;
                if ((mbp.Gamma > 0.0) || (mbp.SubstrateDoping > 0.0))
                {
                    xwd = modeltemp.XD * barg;
                    xws = modeltemp.XD * sarg;

                    /* 
					* short - channel effect with vds .ne. 0.0
					*/

                    argss = 0.0;
                    argsd = 0.0;
                    dbargs = 0.0;
                    dbargd = 0.0;
                    dgdvds = 0.0;
                    dgddb2 = 0.0;
                    if (mbp.JunctionDepth > 0)
                    {
                        tmp = 2.0 / mbp.JunctionDepth;
                        argxs = 1.0 + xws * tmp;
                        argxd = 1.0 + xwd * tmp;
                        args = Math.Sqrt(argxs);
                        argd = Math.Sqrt(argxd);
                        tmp = .5 * mbp.JunctionDepth / EffectiveLength;
                        argss = tmp * (args - 1.0);
                        argsd = tmp * (argd - 1.0);
                    }
                    gamasd = mbp.Gamma * (1.0 - argss - argsd);
                    dbxwd = modeltemp.XD * dbrgdb;
                    dbxws = modeltemp.XD * dsrgdb;
                    if (mbp.JunctionDepth > 0)
                    {
                        tmp = 0.5 / EffectiveLength;
                        dbargs = tmp * dbxws / args;
                        dbargd = tmp * dbxwd / argd;
                        dasdb2 = -modeltemp.XD * (d2sdb2 + dsrgdb * dsrgdb * modeltemp.XD / (mbp.JunctionDepth * argxs)) / (EffectiveLength *
                            args);
                        daddb2 = -modeltemp.XD * (d2bdb2 + dbrgdb * dbrgdb * modeltemp.XD / (mbp.JunctionDepth * argxd)) / (EffectiveLength *
                            argd);
                        dgddb2 = -0.5 * mbp.Gamma * (dasdb2 + daddb2);
                    }
                    dgddvb = -mbp.Gamma * (dbargs + dbargd);
                    if (mbp.JunctionDepth > 0)
                    {
                        ddxwd = -dbxwd;
                        dgdvds = -mbp.Gamma * 0.5 * ddxwd / (EffectiveLength * argd);
                    }
                }
                else
                {
                    gamasd = mbp.Gamma;
                    gammad = mbp.Gamma;
                    dgddvb = 0.0;
                    dgdvds = 0.0;
                    dgddb2 = 0.0;
                }
                von = vbin + gamasd * sarg;
                vth = von;
                vdsat = 0.0;
                if (mbp.FastSurfaceStateDensity != 0.0 && OxideCap != 0.0)
                {
                    /* XXX constant per model */
                    cfs = Circuit.Charge * mbp.FastSurfaceStateDensity * 1e4 /* (cm *  * 2 / m *  * 2) */ ;
                    cdonco = -(gamasd * dsrgdb + dgddvb * sarg) + factor;
                    xn = 1.0 + cfs / OxideCap * bp.Width * EffectiveLength + cdonco;
                    tmp = vt * xn;
                    von = von + tmp;
                    argg = 1.0 / tmp;
                    vgst = lvgs - von;
                }
                else
                {
                    vgst = lvgs - von;
                    if (lvgs <= von)
                    {
                        /* 
						* cutoff region
						*/
                        CondDS = 0.0;
                        goto line1050;
                    }
                }

                /* 
				* compute some more useful quantities
				*/

                sarg3 = sarg * sarg * sarg;
                /* XXX constant per model */
                sbiarg = Math.Sqrt(temp.TempBulkPotential);
                gammad = gamasd;
                dgdvbs = dgddvb;
                body = barg * barg * barg - sarg3;
                gdbdv = 2.0 * gammad * (barg * barg * dbrgdb - sarg * sarg * dsrgdb);
                dodvbs = -factor + dgdvbs * sarg + gammad * dsrgdb;
                if (mbp.FastSurfaceStateDensity.Value == 0.0)
                    goto line400;
                if (OxideCap == 0.0)
                    goto line410;
                dxndvb = 2.0 * dgdvbs * dsrgdb + gammad * d2sdb2 + dgddb2 * sarg;
                dodvbs = dodvbs + vt * dxndvb;
                dxndvd = dgdvds * dsrgdb;
                dodvds = dgdvds * sarg + vt * dxndvd;
                /* 
				* evaluate effective mobility and its derivatives
				*/
                line400:
                if (OxideCap <= 0.0) goto line410;
                udenom = vgst;
                tmp = mbp.CriticalField * 100 /* cm / m */  * Transistor.EpsilonSilicon / modeltemp.OxideCapFactor;
                if (udenom <= tmp) goto line410;
                ufact = Math.Exp(mbp.CriticalFieldExp * Math.Log(tmp / udenom));
                ueff = mbp.SurfaceMobility * 1e-4 /* (m *  * 2 / cm *  * 2) */  * ufact;
                dudvgs = -ufact * mbp.CriticalFieldExp / udenom;
                dudvds = 0.0;
                dudvbs = mbp.CriticalFieldExp * ufact * dodvbs / vgst;
                goto line500;
                line410:
                ufact = 1.0;
                ueff = mbp.SurfaceMobility * 1e-4 /* (m *  * 2 / cm *  * 2) */ ;
                dudvgs = 0.0;
                dudvds = 0.0;
                dudvbs = 0.0;
                /* 
				* evaluate saturation voltage and its derivatives according to
				* grove - frohman equation
				*/
                line500:
                vgsx = lvgs;
                gammad = gamasd / eta;
                dgdvbs = dgddvb;
                if (mbp.FastSurfaceStateDensity != 0 && OxideCap != 0)
                {
                    vgsx = Math.Max(lvgs, von);
                }
                if (gammad > 0)
                {
                    gammd2 = gammad * gammad;
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
                        dsdvbs = (gammad * (1.0 - arg) + 2.0 * argv / (gammad * arg)) / eta * dgdvbs + 1.0 / arg + factor * dsdvgs;
                    }
                }
                else
                {
                    vdsat = (vgsx - vbin) / eta;
                    vdsat = Math.Max(vdsat, 0.0);
                    dsdvgs = 1.0;
                    dsdvbs = 0.0;
                }
                if (mbp.MaxDriftVelocity > 0)
                {
                    /* 
					 * evaluate saturation voltage and its derivatives 
					 * according to baum's theory of scattering velocity 
					 * saturation
					 */
                    gammd2 = gammad * gammad;
                    v1 = (vgsx - vbin) / eta + phiMinVbs;
                    v2 = phiMinVbs;
                    xv = mbp.MaxDriftVelocity * EffectiveLength / ueff;
                    a1 = gammad / 0.75;
                    b1 = -2.0 * (v1 + xv);
                    c1 = -2.0 * gammad * xv;
                    d1 = 2.0 * v1 * (v2 + xv) - v2 * v2 - 4.0 / 3.0 * gammad * sarg3;
                    a = -b1;
                    b = a1 * c1 - 4.0 * d1;
                    c = -d1 * (a1 * a1 - 4.0 * b1) - c1 * c1;
                    r = -a * a / 3.0 + b;
                    s = 2.0 * a * a * a / 27.0 - a * b / 3.0 + c;
                    r3 = r * r * r;
                    s2 = s * s;
                    p = s2 / 4.0 + r3 / 27.0;
                    p0 = Math.Abs(p);
                    p2 = Math.Sqrt(p0);
                    if (p < 0)
                    {
                        ro = Math.Sqrt(s2 / 4.0 + p0);
                        ro = Math.Log(ro) / 3.0;
                        ro = Math.Exp(ro);
                        fi = Math.Atan(-2.0 * p2 / s);
                        y3 = 2.0 * ro * Math.Cos(fi / 3.0) - a / 3.0;
                    }
                    else
                    {
                        p3 = (-s / 2.0 + p2);
                        p3 = Math.Exp(Math.Log(Math.Abs(p3)) / 3.0);
                        p4 = (-s / 2.0 - p2);
                        p4 = Math.Exp(Math.Log(Math.Abs(p4)) / 3.0);
                        y3 = p3 + p4 - a / 3.0;
                    }
                    iknt = 0;
                    a3 = Math.Sqrt(a1 * a1 / 4.0 - b1 + y3);
                    b3 = Math.Sqrt(y3 * y3 / 4.0 - d1);
                    for (i = 1; i <= 4; i++)
                    {
                        a4[i - 1] = a1 / 2.0 + sig1[i - 1] * a3;
                        b4[i - 1] = y3 / 2.0 + sig2[i - 1] * b3;
                        delta4 = a4[i - 1] * a4[i - 1] / 4.0 - b4[i - 1];
                        if (delta4 < 0)
                            continue;
                        iknt = iknt + 1;
                        tmp = Math.Sqrt(delta4);
                        x4[iknt - 1] = -a4[i - 1] / 2.0 + tmp;
                        iknt = iknt + 1;
                        x4[iknt - 1] = -a4[i - 1] / 2.0 - tmp;
                    }
                    jknt = 0;
                    for (j = 1; j <= iknt; j++)
                    {
                        if (x4[j - 1] <= 0) continue;
                        /* XXX implement this sanely */
                        poly4[j - 1] = x4[j - 1] * x4[j - 1] * x4[j - 1] * x4[j - 1] + a1 * x4[j - 1] * x4[j - 1] * x4[j - 1];
                        poly4[j - 1] = poly4[j - 1] + b1 * x4[j - 1] * x4[j - 1] + c1 * x4[j - 1] + d1;
                        if (Math.Abs(poly4[j - 1]) > 1.0e-6)
                            continue;
                        jknt = jknt + 1;
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
                /* 
				 * evaluate effective channel length and its derivatives
				 */
                if (lvds != 0.0)
                {
                    gammad = gamasd;
                    if ((lvbs - vdsat) <= 0)
                    {
                        bsarg = Math.Sqrt(vdsat + phiMinVbs);
                        dbsrdb = -0.5 / bsarg;
                    }
                    else
                    {
                        bsarg = sphi / (1.0 + 0.5 * (lvbs - vdsat) / temp.TempPhi);
                        dbsrdb = -0.5 * bsarg * bsarg / sphi3;
                    }
                    bodys = bsarg * bsarg * bsarg - sarg3;
                    gdbdvs = 2.0 * gammad * (bsarg * bsarg * dbsrdb - sarg * sarg * dsrgdb);
                    if (mbp.MaxDriftVelocity <= 0)
                    {
                        if (mbp.SubstrateDoping.Value == 0.0)
                            goto line610;
                        if (xlamda > 0.0)
                            goto line610;
                        argv = (lvds - vdsat) / 4.0;
                        sargv = Math.Sqrt(1.0 + argv * argv);
                        arg = Math.Sqrt(argv + sargv);
                        xlfact = modeltemp.XD / (EffectiveLength * lvds);
                        xlamda = xlfact * arg;
                        dldsat = lvds * xlamda / (8.0 * sargv);
                    }
                    else
                    {
                        argv = (vgsx - vbin) / eta - vdsat;
                        xdv = modeltemp.XD / Math.Sqrt(mbp.ChannelCharge);
                        xlv = mbp.MaxDriftVelocity * xdv / (2.0 * ueff);
                        vqchan = argv - gammad * bsarg;
                        dqdsat = -1.0 + gammad * dbsrdb;
                        vl = mbp.MaxDriftVelocity * EffectiveLength;
                        dfunds = vl * dqdsat - ueff * vqchan;
                        dfundg = (vl - ueff * vdsat) / eta;
                        dfundb = -vl * (1.0 + dqdsat - factor / eta) + ueff * (gdbdvs - dgdvbs * bodys / 1.5) / eta;
                        dsdvgs = -dfundg / dfunds;
                        dsdvbs = -dfundb / dfunds;
                        if (mbp.SubstrateDoping.Value == 0.0)
                            goto line610;
                        if (xlamda > 0.0)
                            goto line610;
                        argv = lvds - vdsat;
                        argv = Math.Max(argv, 0.0);
                        xls = Math.Sqrt(xlv * xlv + argv);
                        dldsat = xdv / (2.0 * xls);
                        xlfact = xdv / (EffectiveLength * lvds);
                        xlamda = xlfact * (xls - xlv);
                        dldsat = dldsat / EffectiveLength;
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

                /* 
				* limit channel shortening at punch - through
				*/
                xwb = modeltemp.XD * sbiarg;
                xld = EffectiveLength - xwb;
                clfact = 1.0 - xlamda * lvds;
                dldvds = -xlamda - dldvds;
                xleff = EffectiveLength * clfact;
                deltal = xlamda * lvds * EffectiveLength;
                if (mbp.SubstrateDoping.Value == 0.0)
                    xwb = 0.25e-6;
                if (xleff < xwb)
                {
                    xleff = xwb / (1.0 + (deltal - xld) / xwb);
                    clfact = xleff / EffectiveLength;
                    dfact = xleff * xleff / (xwb * xwb);
                    dldvgs = dfact * dldvgs;
                    dldvds = dfact * dldvds;
                    dldvbs = dfact * dldvbs;
                }
                /* 
				 * evaluate effective beta (effective kp)
				 */
                beta1 = Beta * ufact / clfact;
                /* 
				 * test for mode of operation and branch appropriately
				 */
                gammad = gamasd;
                dgdvbs = dgddvb;
                if (lvds <= 1.0e-10)
                {
                    if (lvgs <= von)
                    {
                        if ((mbp.FastSurfaceStateDensity.Value == 0.0) || (OxideCap == 0.0))
                        {
                            CondDS = 0.0;
                            goto line1050;
                        }

                        CondDS = beta1 * (von - vbin - gammad * sarg) * Math.Exp(argg * (lvgs - von));
                        goto line1050;
                    }

                    CondDS = beta1 * (lvgs - vbin - gammad * sarg);
                    goto line1050;
                }

                if (lvgs > von)
                    goto line900;
                /* 
				* subthreshold region
				*/
                if (vdsat <= 0)
                {
                    CondDS = 0.0;
                    if (lvgs > vth)
                        goto doneval;
                    goto line1050;
                }
                vdson = Math.Min(vdsat, lvds);
                if (lvds > vdsat)
                {
                    barg = bsarg;
                    dbrgdb = dbsrdb;
                    body = bodys;
                    gdbdv = gdbdvs;
                }
                cdson = beta1 * ((von - vbin - eta * vdson * 0.5) * vdson - gammad * body / 1.5);
                didvds = beta1 * (von - vbin - eta * vdson - gammad * barg);
                gdson = -cdson * dldvds / clfact - beta1 * dgdvds * body / 1.5;
                if (lvds < vdsat)
                    gdson = gdson + didvds;
                gbson = -cdson * dldvbs / clfact + beta1 * (dodvbs * vdson + factor * vdson - dgdvbs * body / 1.5 - gdbdv);
                if (lvds > vdsat)
                    gbson = gbson + didvds * dsdvbs;
                expg = Math.Exp(argg * (lvgs - von));
                cdrain = cdson * expg;
                gmw = cdrain * argg;
                Transconductance = gmw;
                if (lvds > vdsat)
                    Transconductance = gmw + didvds * dsdvgs * expg;
                tmp = gmw * (lvgs - von) / xn;
                CondDS = gdson * expg - Transconductance * dodvds - tmp * dxndvd;
                TransconductanceBS = gbson * expg - Transconductance * dodvbs - tmp * dxndvb;
                goto doneval;

                line900:
                if (lvds <= vdsat)
                {
                    /* 
					* linear region
					*/
                    cdrain = beta1 * ((lvgs - vbin - eta * lvds / 2.0) * lvds - gammad * body / 1.5);
                    arg = cdrain * (dudvgs / ufact - dldvgs / clfact);
                    Transconductance = arg + beta1 * lvds;
                    arg = cdrain * (dudvds / ufact - dldvds / clfact);
                    CondDS = arg + beta1 * (lvgs - vbin - eta * lvds - gammad * barg - dgdvds * body / 1.5);
                    arg = cdrain * (dudvbs / ufact - dldvbs / clfact);
                    TransconductanceBS = arg - beta1 * (gdbdv + dgdvbs * body / 1.5 - factor * lvds);
                }
                else
                {
                    /* 
					* saturation region
					*/
                    cdrain = beta1 * ((lvgs - vbin - eta * vdsat / 2.0) * vdsat - gammad * bodys / 1.5);
                    arg = cdrain * (dudvgs / ufact - dldvgs / clfact);
                    Transconductance = arg + beta1 * vdsat + beta1 * (lvgs - vbin - eta * vdsat - gammad * bsarg) * dsdvgs;
                    CondDS = -cdrain * dldvds / clfact - beta1 * dgdvds * bodys / 1.5;
                    arg = cdrain * (dudvbs / ufact - dldvbs / clfact);
                    TransconductanceBS = arg - beta1 * (gdbdvs + dgdvbs * bodys / 1.5 - factor * vdsat) + beta1 * (lvgs - vbin - eta * vdsat - gammad *
                        bsarg) * dsdvbs;
                }
                /* 
				* compute charges for "on" region
				*/
                goto doneval;
                /* 
				* finish special cases
				*/
                line1050:
                cdrain = 0.0;
                Transconductance = 0.0;
                TransconductanceBS = 0.0;
                /* 
				* finished
				*/

            }
            doneval:
            Von = mbp.MosfetType * von;
            SaturationVoltageDS = mbp.MosfetType * vdsat;
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
            rstate.Rhs[bulkNode] -= (ceqbs + ceqbd);
            rstate.Rhs[DrainNodePrime] += (ceqbd - cdreq);
            rstate.Rhs[SourceNodePrime] += cdreq + ceqbs;

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
        /// Check convergence
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

            /* these are needed for convergence testing */

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
