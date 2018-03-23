using System;
using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.MosfetBehaviors.Level2
{
    /// <summary>
    /// General behavior of a <see cref="Mosfet2"/>
    /// </summary>
    public class LoadBehavior : BaseLoadBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary behaviors and parameters
        /// </summary>
        private BaseParameters _bp;
        private ModelBaseParameters _mbp;
        private TemperatureBehavior _temp;
        private ModelTemperatureBehavior _modeltemp;

        /// <summary>
        /// Some signs used in the model
        /// </summary>
        private static readonly double[] Sig1 = { 1.0, -1.0, 1.0, -1.0 };
        private static readonly double[] Sig2 = { 1.0, 1.0, -1.0, -1.0 };

        /// <summary>
        /// Shared parameters
        /// </summary>
        [ParameterName("von"), ParameterInfo(" ")]
        public double Von { get; protected set; }
        [ParameterName("vdsat"), ParameterInfo("Saturation drain voltage")]
        public double SaturationVoltageDs { get; protected set; }
        [ParameterName("id"), ParameterName("cd"), ParameterInfo("Drain current")]
        public double DrainCurrent { get; protected set; }
        [ParameterName("ibs"), ParameterInfo("B-S junction current")]
        public double BsCurrent { get; protected set; }
        [ParameterName("ibd"), ParameterInfo("B-D junction current")]
        public double BdCurrent { get; protected set; }
        [ParameterName("gmb"), ParameterName("gmbs"), ParameterInfo("Bulk-Source transconductance")]
        public double TransconductanceBs { get; protected set; }
        [ParameterName("gm"), ParameterInfo("Transconductance")]
        public double Transconductance { get; protected set; }
        [ParameterName("gds"), ParameterInfo("Drain-Source conductance")]
        public double CondDs { get; protected set; }
        [ParameterName("gbd"), ParameterInfo("Bulk-Drain conductance")]
        public double CondBd { get; protected set; }
        [ParameterName("gbs"), ParameterInfo("Bulk-Source conductance")]
        public double CondBs { get; protected set; }
        
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
        private int _drainNode, _gateNode, _sourceNode, _bulkNode;
        [ParameterName("dnodeprime"), ParameterInfo("Number of protected drain node")]
        public int DrainNodePrime { get; private set; }
        [ParameterName("snodeprime"), ParameterInfo("Number of protected source node")]
        public int SourceNodePrime { get; private set; }
        protected MatrixElement<double> DrainDrainPtr { get; private set; }
        protected MatrixElement<double> GateGatePtr { get; private set; }
        protected MatrixElement<double> SourceSourcePtr { get; private set; }
        protected MatrixElement<double> BulkBulkPtr { get; private set; }
        protected MatrixElement<double> DrainPrimeDrainPrimePtr { get; private set; }
        protected MatrixElement<double> SourcePrimeSourcePrimePtr { get; private set; }
        protected MatrixElement<double> DrainDrainPrimePtr { get; private set; }
        protected MatrixElement<double> GateBulkPtr { get; private set; }
        protected MatrixElement<double> GateDrainPrimePtr { get; private set; }
        protected MatrixElement<double> GateSourcePrimePtr { get; private set; }
        protected MatrixElement<double> SourceSourcePrimePtr { get; private set; }
        protected MatrixElement<double> BulkDrainPrimePtr { get; private set; }
        protected MatrixElement<double> BulkSourcePrimePtr { get; private set; }
        protected MatrixElement<double> DrainPrimeSourcePrimePtr { get; private set; }
        protected MatrixElement<double> DrainPrimeDrainPtr { get; private set; }
        protected MatrixElement<double> BulkGatePtr { get; private set; }
        protected MatrixElement<double> DrainPrimeGatePtr { get; private set; }
        protected MatrixElement<double> SourcePrimeGatePtr { get; private set; }
        protected MatrixElement<double> SourcePrimeSourcePtr { get; private set; }
        protected MatrixElement<double> DrainPrimeBulkPtr { get; private set; }
        protected MatrixElement<double> SourcePrimeBulkPtr { get; private set; }
        protected MatrixElement<double> SourcePrimeDrainPrimePtr { get; private set; }
        protected VectorElement<double> BulkPtr { get; private set; }
        protected VectorElement<double> DrainPrimePtr { get; private set; }
        protected VectorElement<double> SourcePrimePtr { get; private set; }

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
            _bp = provider.GetParameterSet<BaseParameters>("entity");
            _mbp = provider.GetParameterSet<ModelBaseParameters>("model");

            // Get behaviors
            _temp = provider.GetBehavior<TemperatureBehavior>("entity");
            _modeltemp = provider.GetBehavior<ModelTemperatureBehavior>("model");

            // Reset
            SaturationVoltageDs = 0.0;
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
                throw new CircuitException("Pin count mismatch: 4 pins expected, {0} given".FormatString(pins.Length));
            _drainNode = pins[0];
            _gateNode = pins[1];
            _sourceNode = pins[2];
            _bulkNode = pins[3];
        }

        /// <summary>
        /// Gets matrix pointers
        /// </summary>
        /// <param name="nodes">Nodes</param>
        /// <param name="solver">Solver</param>
        public override void GetEquationPointers(UnknownCollection nodes, Solver<double> solver)
        {
            if (nodes == null)
                throw new ArgumentNullException(nameof(nodes));
            if (solver == null)
                throw new ArgumentNullException(nameof(solver));

            // Add a series drain node if necessary
            if (_mbp.DrainResistance > 0 || _bp.DrainSquares > 0 && _mbp.SheetResistance > 0)
                DrainNodePrime = nodes.Create(new SubIdentifier(Name, "drain")).Index;
            else
                DrainNodePrime = _drainNode;

            // Add a series source node if necessary
            if (_mbp.SourceResistance > 0 || _bp.SourceSquares > 0 && _mbp.SheetResistance > 0)
                SourceNodePrime = nodes.Create(new SubIdentifier(Name, "source")).Index;
            else
                SourceNodePrime = _sourceNode;

            // Get matrix elements
            DrainDrainPtr = solver.GetMatrixElement(_drainNode, _drainNode);
            GateGatePtr = solver.GetMatrixElement(_gateNode, _gateNode);
            SourceSourcePtr = solver.GetMatrixElement(_sourceNode, _sourceNode);
            BulkBulkPtr = solver.GetMatrixElement(_bulkNode, _bulkNode);
            DrainPrimeDrainPrimePtr = solver.GetMatrixElement(DrainNodePrime, DrainNodePrime);
            SourcePrimeSourcePrimePtr = solver.GetMatrixElement(SourceNodePrime, SourceNodePrime);
            DrainDrainPrimePtr = solver.GetMatrixElement(_drainNode, DrainNodePrime);
            GateBulkPtr = solver.GetMatrixElement(_gateNode, _bulkNode);
            GateDrainPrimePtr = solver.GetMatrixElement(_gateNode, DrainNodePrime);
            GateSourcePrimePtr = solver.GetMatrixElement(_gateNode, SourceNodePrime);
            SourceSourcePrimePtr = solver.GetMatrixElement(_sourceNode, SourceNodePrime);
            BulkDrainPrimePtr = solver.GetMatrixElement(_bulkNode, DrainNodePrime);
            BulkSourcePrimePtr = solver.GetMatrixElement(_bulkNode, SourceNodePrime);
            DrainPrimeSourcePrimePtr = solver.GetMatrixElement(DrainNodePrime, SourceNodePrime);
            DrainPrimeDrainPtr = solver.GetMatrixElement(DrainNodePrime, _drainNode);
            BulkGatePtr = solver.GetMatrixElement(_bulkNode, _gateNode);
            DrainPrimeGatePtr = solver.GetMatrixElement(DrainNodePrime, _gateNode);
            SourcePrimeGatePtr = solver.GetMatrixElement(SourceNodePrime, _gateNode);
            SourcePrimeSourcePtr = solver.GetMatrixElement(SourceNodePrime, _sourceNode);
            DrainPrimeBulkPtr = solver.GetMatrixElement(DrainNodePrime, _bulkNode);
            SourcePrimeBulkPtr = solver.GetMatrixElement(SourceNodePrime, _bulkNode);
            SourcePrimeDrainPrimePtr = solver.GetMatrixElement(SourceNodePrime, DrainNodePrime);

            // Get rhs elements
            BulkPtr = solver.GetRhsElement(_bulkNode);
            SourcePrimePtr = solver.GetRhsElement(SourceNodePrime);
            DrainPrimePtr = solver.GetRhsElement(DrainNodePrime);
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
            double drainSatCur, sourceSatCur,
                vgs, vds, vbs, vbd, vgd;
            double von;
            double vdsat, cdrain = 0.0,
                cdreq;
            int xnrm, xrev;

            var vt = Circuit.KOverQ * _bp.Temperature;
            var check = 1;

            var effectiveLength = _bp.Length - 2 * _mbp.LateralDiffusion;
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

            var beta = _temp.TempTransconductance * _bp.Width / effectiveLength;
            var oxideCap = _modeltemp.OxideCapFactor * effectiveLength * _bp.Width;

            if (state.Init == RealState.InitializationStates.InitFloat || state.Init == RealState.InitializationStates.InitTransient ||
                state.Init == RealState.InitializationStates.InitFix && !_bp.Off)
            {
                // general iteration
                vbs = _mbp.MosfetType * (rstate.Solution[_bulkNode] - rstate.Solution[SourceNodePrime]);
                vgs = _mbp.MosfetType * (rstate.Solution[_gateNode] - rstate.Solution[SourceNodePrime]);
                vds = _mbp.MosfetType * (rstate.Solution[DrainNodePrime] - rstate.Solution[SourceNodePrime]);

                // now some common crunching for some more useful quantities
                vbd = vbs - vds;
                vgd = vgs - vds;
                var vgdo = VoltageGs - VoltageDs;

                von = _mbp.MosfetType * Von;

                /* 
				* limiting
				* We want to keep device voltages from changing
				* so fast that the exponentials churn out overflows 
				* and similar rudeness
				*/
                if (VoltageDs >= 0)
                {
                    vgs = Transistor.LimitFet(vgs, VoltageGs, von);
                    vds = vgs - vgd;
                    vds = Transistor.LimitVoltageDs(vds, VoltageDs);
                }
                else
                {
                    vgd = Transistor.LimitFet(vgd, vgdo, von);
                    vds = vgs - vgd;
                    vds = -Transistor.LimitVoltageDs(-vds, -VoltageDs);
                    vgs = vgd + vds;
                }
                if (vds >= 0)
                {
                    vbs = Transistor.LimitJunction(vbs, VoltageBs, vt, _temp.SourceVCritical, out check);
                }
                else
                {
                    vbd = Transistor.LimitJunction(vbd, VoltageBd, vt, _temp.DrainVCritical, out check);
                    vbs = vbd + vds;
                }
            }
            else
            {
                /* ok - not one of the simple cases, so we have to 
				 * look at other possibilities 
				 */

                if (state.Init == RealState.InitializationStates.InitJunction && !_bp.Off)
                {
                    vds = _mbp.MosfetType * _bp.InitialVoltageDs;
                    vgs = _mbp.MosfetType * _bp.InitialVoltageGs;
                    vbs = _mbp.MosfetType * _bp.InitialVoltageBs;

                    // TODO: At some point, check what this is supposed to do
                    if (vds.Equals(0.0) && vgs.Equals(0.0) && vbs.Equals(0.0) && (state.UseDc || state.Domain == RealState.DomainType.None || !state.UseIc))
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
                CondBs = sourceSatCur / vt;
                BsCurrent = CondBs * vbs;
                CondBs += state.Gmin;
            }
            else
            {
                var evbs = Math.Exp(vbs / vt);
                CondBs = sourceSatCur * evbs / vt + state.Gmin;
                BsCurrent = sourceSatCur * (evbs - 1);
            }
            if (vbd <= 0)
            {
                CondBd = drainSatCur / vt;
                BdCurrent = CondBd * vbd;
                CondBd += state.Gmin;
            }
            else
            {
                var evbd = Math.Exp(vbd / vt);
                CondBd = drainSatCur * evbd / vt + state.Gmin;
                BdCurrent = drainSatCur * (evbd - 1);
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
                double dsrgdb, d2Sdb2;
                double sphi = 0.0; /* square root of phi */
                double sphi3 = 0.0; /* square root of phi cubed */
                double barg, d2Bdb2,
                    dbrgdb,
                    argd = 0.0, args = 0.0;
                double argxs = 0.0, argxd = 0.0;
                double dgddb2, dgddvb, dgdvds, gamasd;
                double xn = 0.0, argg = 0.0, vgst,
                    dodvds = 0.0, dxndvd = 0.0, dxndvb = 0.0,
                    dudvgs, dudvds, dudvbs;
                double argv,
                    ufact, ueff, dsdvgs, dsdvbs;
                double xvalid = 0.0, bsarg = 0.0;
                double bodys = 0.0, gdbdvs = 0.0;
                double dldvgs = 0.0, dldvds = 0.0, dldvbs = 0.0;
                double xlamda = _mbp.Lambda;
                /* 'local' variables - these switch d & s around appropriately
				 * so that we don't have to worry about vds < 0
				 */
                double lvbs = Mode  > 0 ? vbs : vbd;
                double lvds = Mode * vds;
                double lvgs = Mode > 0 ? vgs : vgd;
                double phiMinVbs = _temp.TempPhi - lvbs;
                double tmp; /* a temporary variable, not used for more than */
                            /* about 10 lines at a time */

                /* 
				* compute some useful quantities
				*/

                if (lvbs <= 0.0)
                {
                    sarg = Math.Sqrt(phiMinVbs);
                    dsrgdb = -0.5 / sarg;
                    d2Sdb2 = 0.5 * dsrgdb / phiMinVbs;
                }
                else
                {
                    sphi = Math.Sqrt(_temp.TempPhi);
                    sphi3 = _temp.TempPhi * sphi;
                    sarg = sphi / (1.0 + 0.5 * lvbs / _temp.TempPhi);
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
                    barg = sphi / (1.0 + 0.5 * (lvbs - lvds) / _temp.TempPhi);
                    tmp = barg / sphi3;
                    dbrgdb = -0.5 * barg * tmp;
                    d2Bdb2 = -dbrgdb * tmp;
                }
                /* 
				* calculate threshold voltage (von)
				* narrow - channel effect
				*/

                /* XXX constant per device */
                var factor = 0.125 * _mbp.NarrowFactor * 2.0 * Math.PI * Transistor.EpsilonSilicon / oxideCap * effectiveLength;
                /* XXX constant per device */
                var eta = 1.0 + factor;
                var vbin = _temp.TempVoltageBi * _mbp.MosfetType + factor * phiMinVbs;
                if (_mbp.Gamma > 0.0 || _mbp.SubstrateDoping > 0.0)
                {
                    var xwd = _modeltemp.Xd * barg;
                    var xws = _modeltemp.Xd * sarg;

                    /* 
					* short - channel effect with vds .ne. 0.0
					*/

                    var argss = 0.0;
                    var argsd = 0.0;
                    var dbargs = 0.0;
                    var dbargd = 0.0;
                    dgdvds = 0.0;
                    dgddb2 = 0.0;
                    if (_mbp.JunctionDepth > 0)
                    {
                        tmp = 2.0 / _mbp.JunctionDepth;
                        argxs = 1.0 + xws * tmp;
                        argxd = 1.0 + xwd * tmp;
                        args = Math.Sqrt(argxs);
                        argd = Math.Sqrt(argxd);
                        tmp = .5 * _mbp.JunctionDepth / effectiveLength;
                        argss = tmp * (args - 1.0);
                        argsd = tmp * (argd - 1.0);
                    }
                    gamasd = _mbp.Gamma * (1.0 - argss - argsd);
                    var dbxwd = _modeltemp.Xd * dbrgdb;
                    var dbxws = _modeltemp.Xd * dsrgdb;
                    if (_mbp.JunctionDepth > 0)
                    {
                        tmp = 0.5 / effectiveLength;
                        dbargs = tmp * dbxws / args;
                        dbargd = tmp * dbxwd / argd;
                        var dasdb2 = -_modeltemp.Xd * (d2Sdb2 + dsrgdb * dsrgdb * _modeltemp.Xd / (_mbp.JunctionDepth * argxs)) / (effectiveLength *
                                                                                                                                      args);
                        var daddb2 = -_modeltemp.Xd * (d2Bdb2 + dbrgdb * dbrgdb * _modeltemp.Xd / (_mbp.JunctionDepth * argxd)) / (effectiveLength *
                                                                                                                                      argd);
                        dgddb2 = -0.5 * _mbp.Gamma * (dasdb2 + daddb2);
                    }
                    dgddvb = -_mbp.Gamma * (dbargs + dbargd);
                    if (_mbp.JunctionDepth > 0)
                    {
                        var ddxwd = -dbxwd;
                        dgdvds = -_mbp.Gamma * 0.5 * ddxwd / (effectiveLength * argd);
                    }
                }
                else
                {
                    gamasd = _mbp.Gamma;
                    dgddvb = 0.0;
                    dgdvds = 0.0;
                    dgddb2 = 0.0;
                }
                von = vbin + gamasd * sarg;
                var vth = von;
                vdsat = 0.0;
                if (!_mbp.FastSurfaceStateDensity.Value.Equals(0.0) && !oxideCap.Equals(0.0))
                {
                    /* XXX constant per model */
                    var cfs = Circuit.Charge * _mbp.FastSurfaceStateDensity * 1e4;
                    var cdonco = -(gamasd * dsrgdb + dgddvb * sarg) + factor;
                    xn = 1.0 + cfs / oxideCap * _bp.Width * effectiveLength + cdonco;
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
                        CondDs = 0.0;
                        goto line1050;
                    }
                }

                /* 
				* compute some more useful quantities
				*/

                var sarg3 = sarg * sarg * sarg;
                /* XXX constant per model */
                var sbiarg = Math.Sqrt(_temp.TempBulkPotential);
                var gammad = gamasd;
                var dgdvbs = dgddvb;
                var body = barg * barg * barg - sarg3;
                var gdbdv = 2.0 * gammad * (barg * barg * dbrgdb - sarg * sarg * dsrgdb);
                var dodvbs = -factor + dgdvbs * sarg + gammad * dsrgdb;
                if (_mbp.FastSurfaceStateDensity.Value.Equals(0.0))
                    goto line400;
                if (oxideCap.Equals(0.0))
                    goto line410;
                dxndvb = 2.0 * dgdvbs * dsrgdb + gammad * d2Sdb2 + dgddb2 * sarg;
                dodvbs = dodvbs + vt * dxndvb;
                dxndvd = dgdvds * dsrgdb;
                dodvds = dgdvds * sarg + vt * dxndvd;
                /* 
				* evaluate effective mobility and its derivatives
				*/
                line400:
                if (oxideCap <= 0.0) goto line410;
                var udenom = vgst;
                tmp = _mbp.CriticalField * 100 /* cm / m */  * Transistor.EpsilonSilicon / _modeltemp.OxideCapFactor;
                if (udenom <= tmp) goto line410;
                ufact = Math.Exp(_mbp.CriticalFieldExp * Math.Log(tmp / udenom));
                ueff = _mbp.SurfaceMobility * 1e-4 /* (m *  * 2 / cm *  * 2) */  * ufact;
                dudvgs = -ufact * _mbp.CriticalFieldExp / udenom;
                dudvds = 0.0;
                dudvbs = _mbp.CriticalFieldExp * ufact * dodvbs / vgst;
                goto line500;
                line410:
                ufact = 1.0;
                ueff = _mbp.SurfaceMobility * 1e-4 /* (m *  * 2 / cm *  * 2) */ ;
                dudvgs = 0.0;
                dudvds = 0.0;
                dudvbs = 0.0;
                /* 
				 * evaluate saturation voltage and its derivatives according to
				 * grove - frohman equation
				 */
                line500:
                var vgsx = lvgs;
                gammad = gamasd / eta;
                dgdvbs = dgddvb;
                if (!_mbp.FastSurfaceStateDensity.Value.Equals(0.0) && !oxideCap.Equals(0.0))
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
                if (_mbp.MaxDriftVelocity > 0)
                {
                    /* 
					 * evaluate saturation voltage and its derivatives 
					 * according to baum's theory of scattering velocity 
					 * saturation
					 */
                    var v1 = (vgsx - vbin) / eta + phiMinVbs;
                    var v2 = phiMinVbs;
                    var xv = _mbp.MaxDriftVelocity * effectiveLength / ueff;
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
                    for (int i = 1; i <= 4; i++)
                    {
                        a4[i - 1] = a1 / 2.0 + Sig1[i - 1] * a3;
                        b4[i - 1] = y3 / 2.0 + Sig2[i - 1] * b3;
                        var delta4 = a4[i - 1] * a4[i - 1] / 4.0 - b4[i - 1];
                        if (delta4 < 0)
                            continue;
                        iknt = iknt + 1;
                        tmp = Math.Sqrt(delta4);
                        x4[iknt - 1] = -a4[i - 1] / 2.0 + tmp;
                        iknt = iknt + 1;
                        x4[iknt - 1] = -a4[i - 1] / 2.0 - tmp;
                    }
                    var jknt = 0;
                    for (int j = 1; j <= iknt; j++)
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
                        bsarg = sphi / (1.0 + 0.5 * (lvbs - vdsat) / _temp.TempPhi);
                        dbsrdb = -0.5 * bsarg * bsarg / sphi3;
                    }
                    bodys = bsarg * bsarg * bsarg - sarg3;
                    gdbdvs = 2.0 * gammad * (bsarg * bsarg * dbsrdb - sarg * sarg * dsrgdb);
                    double xlfact;
                    double dldsat;
                    if (_mbp.MaxDriftVelocity <= 0)
                    {
                        if (_mbp.SubstrateDoping.Value.Equals(0.0))
                            goto line610;
                        if (xlamda > 0.0)
                            goto line610;
                        argv = (lvds - vdsat) / 4.0;
                        var sargv = Math.Sqrt(1.0 + argv * argv);
                        arg = Math.Sqrt(argv + sargv);
                        xlfact = _modeltemp.Xd / (effectiveLength * lvds);
                        xlamda = xlfact * arg;
                        dldsat = lvds * xlamda / (8.0 * sargv);
                    }
                    else
                    {
                        argv = (vgsx - vbin) / eta - vdsat;
                        var xdv = _modeltemp.Xd / Math.Sqrt(_mbp.ChannelCharge);
                        var xlv = _mbp.MaxDriftVelocity * xdv / (2.0 * ueff);
                        var vqchan = argv - gammad * bsarg;
                        var dqdsat = -1.0 + gammad * dbsrdb;
                        var vl = _mbp.MaxDriftVelocity * effectiveLength;
                        var dfunds = vl * dqdsat - ueff * vqchan;
                        var dfundg = (vl - ueff * vdsat) / eta;
                        var dfundb = -vl * (1.0 + dqdsat - factor / eta) + ueff * (gdbdvs - dgdvbs * bodys / 1.5) / eta;
                        dsdvgs = -dfundg / dfunds;
                        dsdvbs = -dfundb / dfunds;
                        if (_mbp.SubstrateDoping.Value.Equals(0.0))
                            goto line610;
                        if (xlamda > 0.0)
                            goto line610;
                        argv = lvds - vdsat;
                        argv = Math.Max(argv, 0.0);
                        var xls = Math.Sqrt(xlv * xlv + argv);
                        dldsat = xdv / (2.0 * xls);
                        xlfact = xdv / (effectiveLength * lvds);
                        xlamda = xlfact * (xls - xlv);
                        dldsat = dldsat / effectiveLength;
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
                var xwb = _modeltemp.Xd * sbiarg;
                var xld = effectiveLength - xwb;
                var clfact = 1.0 - xlamda * lvds;
                dldvds = -xlamda - dldvds;
                var xleff = effectiveLength * clfact;
                var deltal = xlamda * lvds * effectiveLength;
                if (_mbp.SubstrateDoping.Value.Equals(0.0))
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
                        if (_mbp.FastSurfaceStateDensity.Value.Equals(0.0) || oxideCap.Equals(0.0))
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
                /* 
				* subthreshold region
				*/
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
                    gdson = gdson + didvds;
                var gbson = -cdson * dldvbs / clfact + beta1 * (dodvbs * vdson + factor * vdson - dgdvbs * body / 1.5 - gdbdv);
                if (lvds > vdsat)
                    gbson = gbson + didvds * dsdvbs;
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
                    /* 
					* linear region
					*/
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
                    /* 
					* saturation region
					*/
                    cdrain = beta1 * ((lvgs - vbin - eta * vdsat / 2.0) * vdsat - gammad * bodys / 1.5);
                    arg = cdrain * (dudvgs / ufact - dldvgs / clfact);
                    Transconductance = arg + beta1 * vdsat + beta1 * (lvgs - vbin - eta * vdsat - gammad * bsarg) * dsdvgs;
                    CondDs = -cdrain * dldvds / clfact - beta1 * dgdvds * bodys / 1.5;
                    arg = cdrain * (dudvbs / ufact - dldvbs / clfact);
                    TransconductanceBs = arg - beta1 * (gdbdvs + dgdvbs * bodys / 1.5 - factor * vdsat) + beta1 * (lvgs - vbin - eta * vdsat - gammad *
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
                TransconductanceBs = 0.0;
                /* 
				* finished
				*/

            }
            doneval:
            Von = _mbp.MosfetType * von;
            SaturationVoltageDs = _mbp.MosfetType * vdsat;
            /* 
			* COMPUTE EQUIVALENT DRAIN CURRENT SOURCE
			*/
            DrainCurrent = Mode * cdrain - BdCurrent;

            /* 
			 * check convergence
			 */
            if (!_bp.Off || state.Init != RealState.InitializationStates.InitFix)
            {
                if (check == 1)
                    state.IsConvergent = false;
            }
            VoltageBs = vbs;
            VoltageBd = vbd;
            VoltageGs = vgs;
            VoltageDs = vds;

            /* 
			* load current vector
			*/
            var ceqbs = _mbp.MosfetType * (BsCurrent - (CondBs - state.Gmin) * vbs);
            var ceqbd = _mbp.MosfetType * (BdCurrent - (CondBd - state.Gmin) * vbd);
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
            BulkPtr.Value -= ceqbs + ceqbd;
            DrainPrimePtr.Value += ceqbd - cdreq;
            SourcePrimePtr.Value += cdreq + ceqbs;

            // load Y-matrix
            DrainDrainPtr.Value += _temp.DrainConductance;
            SourceSourcePtr.Value += _temp.SourceConductance;
            BulkBulkPtr.Value += CondBd + CondBs;
            DrainPrimeDrainPrimePtr.Value += _temp.DrainConductance + CondDs + CondBd + xrev * (Transconductance + TransconductanceBs);
            SourcePrimeSourcePrimePtr.Value += _temp.SourceConductance + CondDs + CondBs + xnrm * (Transconductance + TransconductanceBs);
            DrainDrainPrimePtr.Value += -_temp.DrainConductance;
            SourceSourcePrimePtr.Value += -_temp.SourceConductance;
            BulkDrainPrimePtr.Value -= CondBd;
            BulkSourcePrimePtr.Value -= CondBs;
            DrainPrimeDrainPtr.Value += -_temp.DrainConductance;
            DrainPrimeGatePtr.Value += (xnrm - xrev) * Transconductance;
            DrainPrimeBulkPtr.Value += -CondBd + (xnrm - xrev) * TransconductanceBs;
            DrainPrimeSourcePrimePtr.Value += -CondDs - xnrm * (Transconductance + TransconductanceBs);
            SourcePrimeGatePtr.Value += -(xnrm - xrev) * Transconductance;
            SourcePrimeSourcePtr.Value += -_temp.SourceConductance;
            SourcePrimeBulkPtr.Value += -CondBs - (xnrm - xrev) * TransconductanceBs;
            SourcePrimeDrainPrimePtr.Value += -CondDs - xrev * (Transconductance + TransconductanceBs);
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

            double cdhat;

            var vbs = _mbp.MosfetType * (state.Solution[_bulkNode] - state.Solution[SourceNodePrime]);
            var vgs = _mbp.MosfetType * (state.Solution[_gateNode] - state.Solution[SourceNodePrime]);
            var vds = _mbp.MosfetType * (state.Solution[DrainNodePrime] - state.Solution[SourceNodePrime]);
            var vbd = vbs - vds;
            var vgd = vgs - vds;
            var vgdo = VoltageGs - VoltageDs;
            var delvbs = vbs - VoltageBs;
            var delvbd = vbd - VoltageBd;
            var delvgs = vgs - VoltageGs;
            var delvds = vds - VoltageDs;
            var delvgd = vgd - vgdo;

            /* these are needed for convergence testing */

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
            double tol = config.RelativeTolerance * Math.Max(Math.Abs(cdhat), Math.Abs(DrainCurrent)) + config.AbsoluteTolerance;
            if (Math.Abs(cdhat - DrainCurrent) >= tol)
            {
                state.IsConvergent = false;
                return false;
            }

            tol = config.RelativeTolerance * Math.Max(Math.Abs(cbhat), Math.Abs(BsCurrent + BdCurrent)) + config.AbsoluteTolerance;
            if (Math.Abs(cbhat - (BsCurrent + BdCurrent)) > tol)
            {
                state.IsConvergent = false;
                return false;
            }
            return true;
        }
    }
}
