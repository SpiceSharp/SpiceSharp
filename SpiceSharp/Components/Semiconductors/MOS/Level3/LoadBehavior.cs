using System;
using SpiceSharp.Circuits;
using SpiceSharp.Attributes;
using SpiceSharp.Algebra;
using SpiceSharp.Simulations;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.MosfetBehaviors.Level3
{
    /// <summary>
    /// General behavior for a <see cref="Mosfet3"/>
    /// </summary>
    public class LoadBehavior : Behaviors.LoadBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        BaseParameters bp;
        ModelBaseParameters mbp;
        TemperatureBehavior temp;
        ModelTemperatureBehavior modeltemp;

        /// <summary>
        /// Shared variables
        /// </summary>
        [PropertyName("von"), PropertyInfo("Turn-on voltage")]
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
        public double VoltageBD { get; protected set; }
        public double VoltageBS { get; protected set; }
        public double VoltageGS { get; protected set; }
        public double VoltageDS { get; protected set; }

        /// <summary>
        /// Nodes
        /// </summary>
        int drainNode, gateNode, sourceNode, bulkNode;
        [PropertyName("dnodeprime"), PropertyInfo("Number of protected drain node")]
        public int DrainNodePrime { get; protected set; }
        [PropertyName("snodeprime"), PropertyInfo("Number of protected source node")]
        public int SourceNodePrime { get; protected set; }
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
        /// <param name="provider"></param>
        public override void Setup(SetupDataProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            // Get parameters
            bp = provider.GetParameterSet<BaseParameters>("entity");
            mbp = provider.GetParameterSet<ModelBaseParameters>("model");

            // Get behaviors
            temp = provider.GetBehavior<TemperatureBehavior>("entity");
            modeltemp = provider.GetBehavior<ModelTemperatureBehavior>("model");

            // Initialize some variable
            SaturationVoltageDS = 0;
            Von = 0;
            Mode = 1;
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
        /// <param name="solver">Matrix</param>
        public override void GetEquationPointers(Nodes nodes, Solver<double> solver)
        {
            if (nodes == null)
                throw new ArgumentNullException(nameof(nodes));
            if (solver == null)
                throw new ArgumentNullException(nameof(solver));

            // Add a series drain node if necessary
            if (mbp.DrainResistance != 0 || (mbp.SheetResistance != 0 && bp.DrainSquares != 0))
                DrainNodePrime = nodes.Create(Name.Grow("#drain")).Index;
            else
                DrainNodePrime = drainNode;

            // Add a series source node if necessary
            if (mbp.SourceResistance != 0 || (mbp.SheetResistance != 0 && bp.SourceSquares != 0))
                SourceNodePrime = nodes.Create(Name.Grow("#source")).Index;
            else
                SourceNodePrime = sourceNode;

            // Get matrix elements
            DrainDrainPtr = solver.GetMatrixElement(drainNode, drainNode);
            GateGatePtr = solver.GetMatrixElement(gateNode, gateNode);
            SourceSourcePtr = solver.GetMatrixElement(sourceNode, sourceNode);
            BulkBulkPtr = solver.GetMatrixElement(bulkNode, bulkNode);
            DrainPrimeDrainPrimePtr = solver.GetMatrixElement(DrainNodePrime, DrainNodePrime);
            SourcePrimeSourcePrimePtr = solver.GetMatrixElement(SourceNodePrime, SourceNodePrime);
            DrainDrainPrimePtr = solver.GetMatrixElement(drainNode, DrainNodePrime);
            GateBulkPtr = solver.GetMatrixElement(gateNode, bulkNode);
            GateDrainPrimePtr = solver.GetMatrixElement(gateNode, DrainNodePrime);
            GateSourcePrimePtr = solver.GetMatrixElement(gateNode, SourceNodePrime);
            SourceSourcePrimePtr = solver.GetMatrixElement(sourceNode, SourceNodePrime);
            BulkDrainPrimePtr = solver.GetMatrixElement(bulkNode, DrainNodePrime);
            BulkSourcePrimePtr = solver.GetMatrixElement(bulkNode, SourceNodePrime);
            DrainPrimeSourcePrimePtr = solver.GetMatrixElement(DrainNodePrime, SourceNodePrime);
            DrainPrimeDrainPtr = solver.GetMatrixElement(DrainNodePrime, drainNode);
            BulkGatePtr = solver.GetMatrixElement(bulkNode, gateNode);
            DrainPrimeGatePtr = solver.GetMatrixElement(DrainNodePrime, gateNode);
            SourcePrimeGatePtr = solver.GetMatrixElement(SourceNodePrime, gateNode);
            SourcePrimeSourcePtr = solver.GetMatrixElement(SourceNodePrime, sourceNode);
            DrainPrimeBulkPtr = solver.GetMatrixElement(DrainNodePrime, bulkNode);
            SourcePrimeBulkPtr = solver.GetMatrixElement(SourceNodePrime, bulkNode);
            SourcePrimeDrainPrimePtr = solver.GetMatrixElement(SourceNodePrime, DrainNodePrime);

            // Get rhs elements
            BulkPtr = solver.GetRhsElement(bulkNode);
            DrainPrimePtr = solver.GetRhsElement(DrainNodePrime);
            SourcePrimePtr = solver.GetRhsElement(SourceNodePrime);
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
                OxideCap, vgs, vds, vbs, vbd, vgd, vgdo, von, evbs, evbd, vdsat,
                cdrain, ceqbs, ceqbd, cdreq;
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
            OxideCap = modeltemp.OxideCapFactor * EffectiveLength * bp.Width;

            /* DETAILPROF */

            /* 
			* ok - now to do the start - up operations
			* 
			* we must get values for vbs, vds, and vgs from somewhere
			* so we either predict them or recover them from last iteration
			* These are the two most common cases - either a prediction
			* step or the general iteration step and they
			* share some code, so we put them first - others later on
			*/

            if ((state.Init == RealState.InitializationStates.InitFloat || (state.Init == RealState.InitializationStates.InitTransient)) ||
                ((state.Init == RealState.InitializationStates.InitFix) && (!bp.Off)))
            {
                // General iteration
                vbs = mbp.MosfetType * (state.Solution[bulkNode] - state.Solution[SourceNodePrime]);
                vgs = mbp.MosfetType * (state.Solution[gateNode] - state.Solution[SourceNodePrime]);
                vds = mbp.MosfetType * (state.Solution[DrainNodePrime] - state.Solution[SourceNodePrime]);

                /* now some common crunching for some more useful quantities */
                /* DETAILPROF */

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

                // NOTE: Spice 3f5 does not write out Vgs during DC analysis, so DEVfetlim may give different results in Spice 3f5
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
                /* NODELIMITING */

            }
            else
            {
                /* DETAILPROF */
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
				* subroutine moseq3(vds, vbs, vgs, gm, gds, gmbs, 
				* qg, qc, qb, cggb, cgdb, cgsb, cbgb, cbdb, cbsb)
				*/

                /* 
				* this routine evaluates the drain current, its derivatives and
				* the charges associated with the gate, channel and bulk
				* for mosfets based on semi - empirical equations
				*/

                /* 
				common / mosarg / vto, beta, gamma, phi, phib, cox, xnsub, xnfs, xd, xj, xld, 
				1   xlamda, uo, uexp, vbp, utra, vmax, xneff, xl, xw, vbi, von, vdsat, qspof, 
				2   beta0, beta1, cdrain, xqco, xqc, fnarrw, fshort, lev
				common / status / omega, time, delta, delold(7), ag(7), vt, xni, egfet, 
				1   xmu, sfactr, mode, modedc, icalc, initf, method, iord, maxord, noncon, 
				2   iterno, itemno, nosolv, modac, ipiv, ivmflg, ipostp, iscrch, iofile
				common / knstnt / twopi, xlog2, xlog10, root2, rad, boltz, charge, ctok, 
				1   gmin, reltol, abstol, vntol, trtol, chgtol, eps0, epssil, epsox, 
				2   pivtol, pivrel
				*/

                /* equivalence (xlamda, alpha), (vbp, theta), (uexp, eta), (utra, xkappa) */

                double coeff0 = 0.0631353e0;
                double coeff1 = 0.8013292e0;
                double coeff2 = -0.01110777e0;
                double oneoverxl; /* 1 / effective length */
                double eta; /* eta from model after length factor */
                double phibs; /* phi - vbs */
                double sqphbs; /* square root of phibs */
                double dsqdvb; /*  */
                double sqphis; /* square root of phi */
                double sqphs3; /* square root of phi cubed */
                double wps;
                double oneoverxj; /* 1 / junction depth */
                double xjonxl; /* junction depth / effective length */
                double djonxj, wponxj, arga, argb, argc, dwpdvb, dadvb, dbdvb, gammas, fbodys, fbody, onfbdy, qbonco, vbix, wconxj, dfsdvb,
                    dfbdvb, dqbdvb, vth, dvtdvb, csonco, cdonco, dxndvb = 0.0, dvodvb = 0.0, dvodvd = 0.0, vgsx, dvtdvd, onfg, fgate, us, dfgdvg, dfgdvd,
                    dfgdvb, dvsdvg, dvsdvb, dvsdvd, xn = 0.0, vdsc, onvdsc = 0.0, dvsdga, vdsx, dcodvb, cdnorm, cdo, cd1, fdrain = 0.0, fd2, dfddvg = 0.0, dfddvb = 0.0,
                    dfddvd = 0.0, gdsat, cdsat, gdoncd, gdonfd, gdonfg, dgdvg, dgdvd, dgdvb, emax, emongd, demdvg, demdvd, demdvb, delxl, dldvd,
                    dldem, ddldvg, ddldvd, ddldvb, dlonxl, xlfact, diddl, gds0 = 0.0, emoncd, ondvt, onxn, wfact, gms, gmw, fshort;

                /* 
				* bypasses the computation of charges
				*/

                /* 
				* reference cdrain equations to source and
				* charge equations to bulk
				*/
                vdsat = 0.0;
                oneoverxl = 1.0 / EffectiveLength;
                eta = mbp.Eta * 8.15e-22 / (modeltemp.OxideCapFactor * EffectiveLength * EffectiveLength * EffectiveLength);
                /* 
				* .....square root term
				*/
                if ((Mode == 1 ? vbs : vbd) <= 0.0)
                {
                    phibs = temp.TempPhi - (Mode == 1 ? vbs : vbd);
                    sqphbs = Math.Sqrt(phibs);
                    dsqdvb = -0.5 / sqphbs;
                }
                else
                {
                    sqphis = Math.Sqrt(temp.TempPhi);
                    sqphs3 = temp.TempPhi * sqphis;
                    sqphbs = sqphis / (1.0 + (Mode == 1 ? vbs : vbd) / (temp.TempPhi + temp.TempPhi));
                    phibs = sqphbs * sqphbs;
                    dsqdvb = -phibs / (sqphs3 + sqphs3);
                }
                /* 
				 * .....short channel effect factor
				 */
                if ((mbp.JunctionDepth != 0.0) && (modeltemp.CoefficientDepletionLayerWidth != 0.0))
                {
                    wps = modeltemp.CoefficientDepletionLayerWidth * sqphbs;
                    oneoverxj = 1.0 / mbp.JunctionDepth;
                    xjonxl = mbp.JunctionDepth * oneoverxl;
                    djonxj = mbp.LateralDiffusion * oneoverxj;
                    wponxj = wps * oneoverxj;
                    wconxj = coeff0 + coeff1 * wponxj + coeff2 * wponxj * wponxj;
                    arga = wconxj + djonxj;
                    argc = wponxj / (1.0 + wponxj);
                    argb = Math.Sqrt(1.0 - argc * argc);
                    fshort = 1.0 - xjonxl * (arga * argb - djonxj);
                    dwpdvb = modeltemp.CoefficientDepletionLayerWidth * dsqdvb;
                    dadvb = (coeff1 + coeff2 * (wponxj + wponxj)) * dwpdvb * oneoverxj;
                    dbdvb = -argc * argc * (1.0 - argc) * dwpdvb / (argb * wps);
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
                gammas = mbp.Gamma * fshort;
                fbodys = 0.5 * gammas / (sqphbs + sqphbs);
                fbody = fbodys + mbp.NarrowFactor / bp.Width;
                onfbdy = 1.0 / (1.0 + fbody);
                dfbdvb = -fbodys * dsqdvb / sqphbs + fbodys * dfsdvb / fshort;
                qbonco = gammas * sqphbs + mbp.NarrowFactor * phibs / bp.Width;
                dqbdvb = gammas * dsqdvb + mbp.Gamma * dfsdvb * sqphbs - mbp.NarrowFactor / bp.Width;
                /* 
				 * .....static feedback effect
				 */
                vbix = temp.TempVoltageBI * mbp.MosfetType - eta * (Mode * vds);
                /* 
				 * .....threshold voltage
				 */
                vth = vbix + qbonco;
                dvtdvd = -eta;
                dvtdvb = dqbdvb;
                /* 
				 * .....joint weak inversion and strong inversion
				 */
                von = vth;
                if (mbp.FastSurfaceStateDensity != 0.0)
                {
                    csonco = Circuit.Charge * mbp.FastSurfaceStateDensity * 1e4 /* (cm *  * 2 / m *  * 2) */  * EffectiveLength * bp.Width /
                        OxideCap;
                    cdonco = qbonco / (phibs + phibs);
                    xn = 1.0 + csonco + cdonco;
                    von = vth + vt * xn;
                    dxndvb = dqbdvb / (phibs + phibs) - qbonco * dsqdvb / (phibs * sqphbs);
                    dvodvd = dvtdvd;
                    dvodvb = dvtdvb + vt * dxndvb;
                }
                else
                {
                    /* 
					 * .....cutoff region
					 */
                    if ((Mode == 1 ? vgs : vgd) <= von)
                    {
                        cdrain = 0.0;
                        Transconductance = 0.0;
                        CondDS = 0.0;
                        TransconductanceBS = 0.0;
                        goto innerline1000;
                    }
                }
                /* 
				 * .....device is on
				 */
                vgsx = Math.Max((Mode == 1 ? vgs : vgd), von);
                /* 
				 * .....mobility modulation by gate voltage
				 */
                onfg = 1.0 + mbp.Theta * (vgsx - vth);
                fgate = 1.0 / onfg;
                us = temp.TempSurfaceMobility * 1e-4 /*(m**2/cm**2)*/ * fgate;
                dfgdvg = -mbp.Theta * fgate * fgate;
                dfgdvd = -dfgdvg * dvtdvd;
                dfgdvb = -dfgdvg * dvtdvb;
                /* 
				 * .....saturation voltage
				 */
                vdsat = (vgsx - vth) * onfbdy;
                if (mbp.MaxDriftVelocity <= 0.0)
                {
                    dvsdvg = onfbdy;
                    dvsdvd = -dvsdvg * dvtdvd;
                    dvsdvb = -dvsdvg * dvtdvb - vdsat * dfbdvb * onfbdy;
                }
                else
                {
                    vdsc = EffectiveLength * mbp.MaxDriftVelocity / us;
                    onvdsc = 1.0 / vdsc;
                    arga = (vgsx - vth) * onfbdy;
                    argb = Math.Sqrt(arga * arga + vdsc * vdsc);
                    vdsat = arga + vdsc - argb;
                    dvsdga = (1.0 - arga / argb) * onfbdy;
                    dvsdvg = dvsdga - (1.0 - vdsc / argb) * vdsc * dfgdvg * onfg;
                    dvsdvd = -dvsdvg * dvtdvd;
                    dvsdvb = -dvsdvg * dvtdvb - arga * dvsdga * dfbdvb;
                }
                /* 
				 * .....current factors in linear region
				 */
                vdsx = Math.Min((Mode * vds), vdsat);
                if (vdsx == 0.0)
                    goto line900;
                cdo = vgsx - vth - 0.5 * (1.0 + fbody) * vdsx;
                dcodvb = -dvtdvb - 0.5 * dfbdvb * vdsx;
                /* 
				 * .....normalized drain current
				 */
                cdnorm = cdo * vdsx;
                Transconductance = vdsx;
                CondDS = vgsx - vth - (1.0 + fbody + dvtdvd) * vdsx;
                TransconductanceBS = dcodvb * vdsx;
                /* 
				 * .....drain current without velocity saturation effect
				 */
                cd1 = Beta * cdnorm;
                Beta = Beta * fgate;
                cdrain = Beta * cdnorm;
                Transconductance = Beta * Transconductance + dfgdvg * cd1;
                CondDS = Beta * CondDS + dfgdvd * cd1;
                TransconductanceBS = Beta * TransconductanceBS;
                /* 
				 * .....velocity saturation factor
				 */
                if (mbp.MaxDriftVelocity != 0.0)
                {
                    fdrain = 1.0 / (1.0 + vdsx * onvdsc);
                    fd2 = fdrain * fdrain;
                    arga = fd2 * vdsx * onvdsc * onfg;
                    dfddvg = -dfgdvg * arga;
                    dfddvd = -dfgdvd * arga - fd2 * onvdsc;
                    dfddvb = -dfgdvb * arga;
                    /* 
					 * .....drain current
					 */
                    Transconductance = fdrain * Transconductance + dfddvg * cdrain;
                    CondDS = fdrain * CondDS + dfddvd * cdrain;
                    TransconductanceBS = fdrain * TransconductanceBS + dfddvb * cdrain;
                    cdrain = fdrain * cdrain;
                    Beta = Beta * fdrain;
                }
                /* 
				 * .....channel length modulation
				 */
                if ((Mode * vds) <= vdsat) goto line700;
                if (mbp.MaxDriftVelocity <= 0.0) goto line510;
                if (modeltemp.Alpha == 0.0)
                    goto line700;
                cdsat = cdrain;
                gdsat = cdsat * (1.0 - fdrain) * onvdsc;
                gdsat = Math.Max(1.0e-12, gdsat);
                gdoncd = gdsat / cdsat;
                gdonfd = gdsat / (1.0 - fdrain);
                gdonfg = gdsat * onfg;
                dgdvg = gdoncd * Transconductance - gdonfd * dfddvg + gdonfg * dfgdvg;
                dgdvd = gdoncd * CondDS - gdonfd * dfddvd + gdonfg * dfgdvd;
                dgdvb = gdoncd * TransconductanceBS - gdonfd * dfddvb + gdonfg * dfgdvb;

                emax = mbp.Kappa * cdsat * oneoverxl / gdsat;
                emoncd = emax / cdsat;
                emongd = emax / gdsat;
                demdvg = emoncd * Transconductance - emongd * dgdvg;
                demdvd = emoncd * CondDS - emongd * dgdvd;
                demdvb = emoncd * TransconductanceBS - emongd * dgdvb;

                arga = 0.5 * emax * modeltemp.Alpha;
                argc = mbp.Kappa * modeltemp.Alpha;
                argb = Math.Sqrt(arga * arga + argc * ((Mode * vds) - vdsat));
                delxl = argb - arga;
                dldvd = argc / (argb + argb);
                dldem = 0.5 * (arga / argb - 1.0) * modeltemp.Alpha;
                ddldvg = dldem * demdvg;
                ddldvd = dldem * demdvd - dldvd;
                ddldvb = dldem * demdvb;
                goto line520;
                line510:
                delxl = Math.Sqrt(mbp.Kappa * ((Mode * vds) - vdsat) * modeltemp.Alpha);
                dldvd = 0.5 * delxl / ((Mode * vds) - vdsat);
                ddldvg = 0.0;
                ddldvd = -dldvd;
                ddldvb = 0.0;
                /* 
				 * .....punch through approximation
				 */
                line520:
                if (delxl > (0.5 * EffectiveLength))
                {
                    delxl = EffectiveLength - (EffectiveLength * EffectiveLength / (4.0 * delxl));
                    arga = 4.0 * (EffectiveLength - delxl) * (EffectiveLength - delxl) / (EffectiveLength * EffectiveLength);
                    ddldvg = ddldvg * arga;
                    ddldvd = ddldvd * arga;
                    ddldvb = ddldvb * arga;
                    dldvd = dldvd * arga;
                }
                /* 
				 * .....saturation region
				 */
                dlonxl = delxl * oneoverxl;
                xlfact = 1.0 / (1.0 - dlonxl);
                cdrain = cdrain * xlfact;
                diddl = cdrain / (EffectiveLength - delxl);
                Transconductance = Transconductance * xlfact + diddl * ddldvg;
                gds0 = CondDS * xlfact + diddl * ddldvd;
                TransconductanceBS = TransconductanceBS * xlfact + diddl * ddldvb;
                Transconductance = Transconductance + gds0 * dvsdvg;
                TransconductanceBS = TransconductanceBS + gds0 * dvsdvb;
                CondDS = gds0 * dvsdvd + diddl * dldvd;
                /* 
				 * .....finish strong inversion case
				 */
                line700:
                if ((Mode == 1 ? vgs : vgd) < von)
                {
                    /* 
					 * .....weak inversion
					 */
                    onxn = 1.0 / xn;
                    ondvt = onxn / vt;
                    wfact = Math.Exp(((Mode == 1 ? vgs : vgd) - von) * ondvt);
                    cdrain = cdrain * wfact;
                    gms = Transconductance * wfact;
                    gmw = cdrain * ondvt;
                    Transconductance = gmw;
                    if ((Mode * vds) > vdsat)
                    {
                        Transconductance = Transconductance + gds0 * dvsdvg * wfact;
                    }
                    CondDS = CondDS * wfact + (gms - gmw) * dvodvd;
                    TransconductanceBS = TransconductanceBS * wfact + (gms - gmw) * dvodvb - gmw * ((Mode == 1 ? vgs : vgd) - von) * onxn * dxndvb;
                }
                /* 
				 * .....charge computation
				 */
                goto innerline1000;
                /* 
				 * .....special case of vds = 0.0d0 */
                line900:
                Beta = Beta * fgate;
                cdrain = 0.0;
                Transconductance = 0.0;
                CondDS = Beta * (vgsx - vth);
                TransconductanceBS = 0.0;
                if ((mbp.FastSurfaceStateDensity != 0.0) && ((Mode == 1 ? vgs : vgd) < von))
                {
                    CondDS *= Math.Exp(((Mode == 1 ? vgs : vgd) - von) / (vt * xn));
                }
                innerline1000:;
                /* 
				 * .....done
				 */
            }

            /* DETAILPROF */

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
            if (!bp.Off || (!(state.Init == RealState.InitializationStates.InitFix)))
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
            /* DETAILPROF */

            /* 
			 * meyer's capacitor model
			 */


            /* DETAILPROF */
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
            BulkPtr.Value -= (ceqbs + ceqbd);
            DrainPrimePtr.Value += (ceqbd - cdreq);
            SourcePrimePtr.Value += cdreq + ceqbs;

            // Load Y-matrix
            DrainDrainPtr.Value += temp.DrainConductance;
            SourceSourcePtr.Value += temp.SourceConductance;
            BulkBulkPtr.Value += CondBD + CondBS;
            DrainPrimeDrainPrimePtr.Value += temp.DrainConductance + CondDS + CondBD + xrev * (Transconductance + TransconductanceBS);
            SourcePrimeSourcePrimePtr.Value += temp.SourceConductance + CondDS + CondBS + xnrm * (Transconductance + TransconductanceBS);
            DrainDrainPrimePtr.Value += -temp.DrainConductance;
            SourceSourcePrimePtr.Value += -temp.SourceConductance;
            BulkDrainPrimePtr.Value -= CondBD;
            BulkSourcePrimePtr.Value -= CondBS;
            DrainPrimeDrainPtr.Value += -temp.DrainConductance;
            DrainPrimeGatePtr.Value += (xnrm - xrev) * Transconductance;
            DrainPrimeBulkPtr.Value += -CondBD + (xnrm - xrev) * TransconductanceBS;
            DrainPrimeSourcePrimePtr.Value += -CondDS - xnrm * (Transconductance + TransconductanceBS);
            SourcePrimeGatePtr.Value += -(xnrm - xrev) * Transconductance;
            SourcePrimeSourcePtr.Value += -temp.SourceConductance;
            SourcePrimeBulkPtr.Value += -CondBS - (xnrm - xrev) * TransconductanceBS;
            SourcePrimeDrainPrimePtr.Value += -CondDS - xrev * (Transconductance + TransconductanceBS);
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
