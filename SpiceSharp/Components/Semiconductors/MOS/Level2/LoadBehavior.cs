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
        public double Von { get; internal set; }
        [PropertyName("vdsat"), PropertyInfo("Saturation drain voltage")]
        public double Vdsat { get; internal set; }
        [PropertyName("id"), PropertyName("cd"), PropertyInfo("Drain current")]
        public double Cd { get; internal set; }
        [PropertyName("ibs"), PropertyInfo("B-S junction current")]
        public double Cbs { get; internal set; }
        [PropertyName("ibd"), PropertyInfo("B-D junction current")]
        public double Cbd { get; internal set; }
        [PropertyName("gmb"), PropertyName("gmbs"), PropertyInfo("Bulk-Source transconductance")]
        public double Gmbs { get; internal set; }
        [PropertyName("gm"), PropertyInfo("Transconductance")]
        public double Gm { get; internal set; }
        [PropertyName("gds"), PropertyInfo("Drain-Source conductance")]
        public double Gds { get; internal set; }
        [PropertyName("gbd"), PropertyInfo("Bulk-Drain conductance")]
        public double Gbd { get; internal set; }
        [PropertyName("gbs"), PropertyInfo("Bulk-Source conductance")]
        public double Gbs { get; internal set; }
        
        /// <summary>
        /// Extra variables
        /// </summary>
        public double Mode { get; protected set; }
        public double Vgs { get; protected set; }
        public double Vds { get; protected set; }
        public double Vbs { get; protected set; }
        public double Vbd { get; protected set; }

        /// <summary>
        /// Nodes
        /// </summary>
        int drainNode, gateNode, sourceNode, bulkNode;
        [PropertyName("dnodeprime"), PropertyInfo("Number of internal drain node")]
        public int DrainNodePrime { get; private set; }
        [PropertyName("snodeprime"), PropertyInfo("Number of internal source node")]
        public int SourceNodePrime { get; private set; }
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
            modeltemp = provider.GetBehavior<ModelTemperatureBehavior>(1);

            // Reset
            Vdsat = 0.0;
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
        public override void GetMatrixPointers(Nodes nodes, Matrix matrix)
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

            var state = simulation.State;
            var rstate = state;
            double vt, EffectiveLength, DrainSatCur, SourceSatCur, Beta,
                OxideCap, vgs, vds, vbs, vbd, vgd, vgdo, von, evbs, evbd,
                vdsat, cdrain = 0.0, ceqbs,
                ceqbd, cdreq;
            int Check, xnrm, xrev;

            vt = Circuit.KOverQ * bp.Temperature;
            Check = 1;

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
            OxideCap = modeltemp.OxideCapFactor * EffectiveLength * bp.Width;

            if ((state.Init == State.InitializationState.InitFloat || state.UseSmallSignal || (state.Init == State.InitializationState.InitTransient)) ||
                ((state.Init == State.InitializationState.InitFix) && (!bp.Off)))
            {
                // general iteration
                vbs = mbp.MosfetType * (rstate.Solution[bulkNode] - rstate.Solution[SourceNodePrime]);
                vgs = mbp.MosfetType * (rstate.Solution[gateNode] - rstate.Solution[SourceNodePrime]);
                vds = mbp.MosfetType * (rstate.Solution[DrainNodePrime] - rstate.Solution[SourceNodePrime]);

                // now some common crunching for some more useful quantities
                vbd = vbs - vds;
                vgd = vgs - vds;
                vgdo = Vgs - Vds;

                von = mbp.MosfetType * Von;

                /* 
				* limiting
				* We want to keep device voltages from changing
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
				* look at other possibilities 
				*/

                if ((state.Init == State.InitializationState.InitJct) && !bp.Off)
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
                Gbs = SourceSatCur / vt;
                Cbs = Gbs * vbs;
                Gbs += state.Gmin;
            }
            else
            {
                evbs = Math.Exp(vbs / vt);
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
                evbd = Math.Exp(vbd / vt);
                Gbd = DrainSatCur * evbd / vt + state.Gmin;
                Cbd = DrainSatCur * (evbd - 1);
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
                double phiMinVbs = temp.TPhi - lvbs;
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
                    sphi = Math.Sqrt(temp.TPhi);
                    sphi3 = temp.TPhi * sphi;
                    sarg = sphi / (1.0 + 0.5 * lvbs / temp.TPhi);
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
                    barg = sphi / (1.0 + 0.5 * (lvbs - lvds) / temp.TPhi);
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
                vbin = temp.TVbi * mbp.MosfetType + factor * phiMinVbs;
                if ((mbp.Gamma > 0.0) || (mbp.SubstrateDoping > 0.0))
                {
                    xwd = modeltemp.Xd * barg;
                    xws = modeltemp.Xd * sarg;

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
                    dbxwd = modeltemp.Xd * dbrgdb;
                    dbxws = modeltemp.Xd * dsrgdb;
                    if (mbp.JunctionDepth > 0)
                    {
                        tmp = 0.5 / EffectiveLength;
                        dbargs = tmp * dbxws / args;
                        dbargd = tmp * dbxwd / argd;
                        dasdb2 = -modeltemp.Xd * (d2sdb2 + dsrgdb * dsrgdb * modeltemp.Xd / (mbp.JunctionDepth * argxs)) / (EffectiveLength *
                            args);
                        daddb2 = -modeltemp.Xd * (d2bdb2 + dbrgdb * dbrgdb * modeltemp.Xd / (mbp.JunctionDepth * argxd)) / (EffectiveLength *
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
                        Gds = 0.0;
                        goto line1050;
                    }
                }

                /* 
				* compute some more useful quantities
				*/

                sarg3 = sarg * sarg * sarg;
                /* XXX constant per model */
                sbiarg = Math.Sqrt(temp.TBulkPot);
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
                tmp = mbp.CritField * 100 /* cm / m */  * Transistor.EpsilonSilicon / modeltemp.OxideCapFactor;
                if (udenom <= tmp) goto line410;
                ufact = Math.Exp(mbp.CritFieldExp * Math.Log(tmp / udenom));
                ueff = mbp.SurfaceMobility * 1e-4 /* (m *  * 2 / cm *  * 2) */  * ufact;
                dudvgs = -ufact * mbp.CritFieldExp / udenom;
                dudvds = 0.0;
                dudvbs = mbp.CritFieldExp * ufact * dodvbs / vgst;
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
                if (mbp.MaxDriftVel > 0)
                {
                    /* 
					 * evaluate saturation voltage and its derivatives 
					 * according to baum's theory of scattering velocity 
					 * saturation
					 */
                    gammd2 = gammad * gammad;
                    v1 = (vgsx - vbin) / eta + phiMinVbs;
                    v2 = phiMinVbs;
                    xv = mbp.MaxDriftVel * EffectiveLength / ueff;
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
                        bsarg = sphi / (1.0 + 0.5 * (lvbs - vdsat) / temp.TPhi);
                        dbsrdb = -0.5 * bsarg * bsarg / sphi3;
                    }
                    bodys = bsarg * bsarg * bsarg - sarg3;
                    gdbdvs = 2.0 * gammad * (bsarg * bsarg * dbsrdb - sarg * sarg * dsrgdb);
                    if (mbp.MaxDriftVel <= 0)
                    {
                        if (mbp.SubstrateDoping.Value == 0.0)
                            goto line610;
                        if (xlamda > 0.0)
                            goto line610;
                        argv = (lvds - vdsat) / 4.0;
                        sargv = Math.Sqrt(1.0 + argv * argv);
                        arg = Math.Sqrt(argv + sargv);
                        xlfact = modeltemp.Xd / (EffectiveLength * lvds);
                        xlamda = xlfact * arg;
                        dldsat = lvds * xlamda / (8.0 * sargv);
                    }
                    else
                    {
                        argv = (vgsx - vbin) / eta - vdsat;
                        xdv = modeltemp.Xd / Math.Sqrt(mbp.ChannelCharge);
                        xlv = mbp.MaxDriftVel * xdv / (2.0 * ueff);
                        vqchan = argv - gammad * bsarg;
                        dqdsat = -1.0 + gammad * dbsrdb;
                        vl = mbp.MaxDriftVel * EffectiveLength;
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
                xwb = modeltemp.Xd * sbiarg;
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
                            Gds = 0.0;
                            goto line1050;
                        }

                        Gds = beta1 * (von - vbin - gammad * sarg) * Math.Exp(argg * (lvgs - von));
                        goto line1050;
                    }

                    Gds = beta1 * (lvgs - vbin - gammad * sarg);
                    goto line1050;
                }

                if (lvgs > von)
                    goto line900;
                /* 
				* subthreshold region
				*/
                if (vdsat <= 0)
                {
                    Gds = 0.0;
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
                Gm = gmw;
                if (lvds > vdsat)
                    Gm = gmw + didvds * dsdvgs * expg;
                tmp = gmw * (lvgs - von) / xn;
                Gds = gdson * expg - Gm * dodvds - tmp * dxndvd;
                Gmbs = gbson * expg - Gm * dodvbs - tmp * dxndvb;
                goto doneval;

                line900:
                if (lvds <= vdsat)
                {
                    /* 
					* linear region
					*/
                    cdrain = beta1 * ((lvgs - vbin - eta * lvds / 2.0) * lvds - gammad * body / 1.5);
                    arg = cdrain * (dudvgs / ufact - dldvgs / clfact);
                    Gm = arg + beta1 * lvds;
                    arg = cdrain * (dudvds / ufact - dldvds / clfact);
                    Gds = arg + beta1 * (lvgs - vbin - eta * lvds - gammad * barg - dgdvds * body / 1.5);
                    arg = cdrain * (dudvbs / ufact - dldvbs / clfact);
                    Gmbs = arg - beta1 * (gdbdv + dgdvbs * body / 1.5 - factor * lvds);
                }
                else
                {
                    /* 
					* saturation region
					*/
                    cdrain = beta1 * ((lvgs - vbin - eta * vdsat / 2.0) * vdsat - gammad * bodys / 1.5);
                    arg = cdrain * (dudvgs / ufact - dldvgs / clfact);
                    Gm = arg + beta1 * vdsat + beta1 * (lvgs - vbin - eta * vdsat - gammad * bsarg) * dsdvgs;
                    Gds = -cdrain * dldvds / clfact - beta1 * dgdvds * bodys / 1.5;
                    arg = cdrain * (dudvbs / ufact - dldvbs / clfact);
                    Gmbs = arg - beta1 * (gdbdvs + dgdvbs * bodys / 1.5 - factor * vdsat) + beta1 * (lvgs - vbin - eta * vdsat - gammad *
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
                Gm = 0.0;
                Gmbs = 0.0;
                /* 
				* finished
				*/

            }
            doneval:
            Von = mbp.MosfetType * von;
            Vdsat = mbp.MosfetType * vdsat;
            /* 
			* COMPUTE EQUIVALENT DRAIN CURRENT SOURCE
			*/
            Cd = Mode * cdrain - Cbd;

            /* 
			 * check convergence
			 */
            if (!bp.Off || (!(state.Init == State.InitializationState.InitFix || state.UseSmallSignal)))
            {
                if (Check == 1)
                    state.IsCon = false;
            }
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
            rstate.Rhs[bulkNode] -= (ceqbs + ceqbd);
            rstate.Rhs[DrainNodePrime] += (ceqbd - cdreq);
            rstate.Rhs[SourceNodePrime] += cdreq + ceqbs;

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
        /// Check convergence
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

            /* these are needed for convergence testing */

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
