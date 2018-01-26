using System;
using SpiceSharp.Circuits;
using SpiceSharp.Attributes;
using SpiceSharp.Sparse;
using SpiceSharp.Simulations;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.MosfetBehaviors.Level3
{
    /// <summary>
    /// General behavior for a <see cref="MOS3"/>
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
        public double Vdsat { get; protected set; }
        [PropertyName("id"), PropertyName("cd"), PropertyInfo("Drain current")]
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
        /// Nodes
        /// </summary>
        int dNode, gNode, sNode, bNode;
        [PropertyName("dnodeprime"), PropertyInfo("Number of protected drain node")]
        public int DrainNodePrime { get; protected set; }
        [PropertyName("snodeprime"), PropertyInfo("Number of protected source node")]
        public int SourceNodePrime { get; protected set; }
        protected MatrixElement DdPtr { get; private set; }
        protected MatrixElement GgPtr { get; private set; }
        protected MatrixElement SsPtr { get; private set; }
        protected MatrixElement BbPtr { get; private set; }
        protected MatrixElement DPdpPtr { get; private set; }
        protected MatrixElement SPspPtr { get; private set; }
        protected MatrixElement DdpPtr { get; private set; }
        protected MatrixElement GbPtr { get; private set; }
        protected MatrixElement GdpPtr { get; private set; }
        protected MatrixElement GspPtr { get; private set; }
        protected MatrixElement SspPtr { get; private set; }
        protected MatrixElement BdpPtr { get; private set; }
        protected MatrixElement BspPtr { get; private set; }
        protected MatrixElement DPspPtr { get; private set; }
        protected MatrixElement DPdPtr { get; private set; }
        protected MatrixElement BgPtr { get; private set; }
        protected MatrixElement DPgPtr { get; private set; }
        protected MatrixElement SPgPtr { get; private set; }
        protected MatrixElement SPsPtr { get; private set; }
        protected MatrixElement DPbPtr { get; private set; }
        protected MatrixElement SPbPtr { get; private set; }
        protected MatrixElement SPdpPtr { get; private set; }

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
            bp = provider.GetParameterSet<BaseParameters>(0);
            mbp = provider.GetParameterSet<ModelBaseParameters>(1);

            // Get behaviors
            temp = provider.GetBehavior<TemperatureBehavior>(0);
            modeltemp = provider.GetBehavior<ModelTemperatureBehavior>(1);

            // Initialize some variable
            Vdsat = 0;
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
                throw new Diagnostics.CircuitException($"Pin count mismatch: 4 pins expected, {pins.Length} given");
            dNode = pins[0];
            gNode = pins[1];
            sNode = pins[2];
            bNode = pins[3];
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
            if (mbp.DrainResistance != 0 || (mbp.SheetResistance != 0 && bp.DrainSquares != 0))
                DrainNodePrime = nodes.Create(Name.Grow("#drain")).Index;
            else
                DrainNodePrime = dNode;

            // Add a series source node if necessary
            if (mbp.SourceResistance != 0 || (mbp.SheetResistance != 0 && bp.SourceSquares != 0))
                SourceNodePrime = nodes.Create(Name.Grow("#source")).Index;
            else
                SourceNodePrime = sNode;

            // Get matrix elements
            DdPtr = matrix.GetElement(dNode, dNode);
            GgPtr = matrix.GetElement(gNode, gNode);
            SsPtr = matrix.GetElement(sNode, sNode);
            BbPtr = matrix.GetElement(bNode, bNode);
            DPdpPtr = matrix.GetElement(DrainNodePrime, DrainNodePrime);
            SPspPtr = matrix.GetElement(SourceNodePrime, SourceNodePrime);
            DdpPtr = matrix.GetElement(dNode, DrainNodePrime);
            GbPtr = matrix.GetElement(gNode, bNode);
            GdpPtr = matrix.GetElement(gNode, DrainNodePrime);
            GspPtr = matrix.GetElement(gNode, SourceNodePrime);
            SspPtr = matrix.GetElement(sNode, SourceNodePrime);
            BdpPtr = matrix.GetElement(bNode, DrainNodePrime);
            BspPtr = matrix.GetElement(bNode, SourceNodePrime);
            DPspPtr = matrix.GetElement(DrainNodePrime, SourceNodePrime);
            DPdPtr = matrix.GetElement(DrainNodePrime, dNode);
            BgPtr = matrix.GetElement(bNode, gNode);
            DPgPtr = matrix.GetElement(DrainNodePrime, gNode);
            SPgPtr = matrix.GetElement(SourceNodePrime, gNode);
            SPsPtr = matrix.GetElement(SourceNodePrime, sNode);
            DPbPtr = matrix.GetElement(DrainNodePrime, bNode);
            SPbPtr = matrix.GetElement(SourceNodePrime, bNode);
            SPdpPtr = matrix.GetElement(SourceNodePrime, DrainNodePrime);
        }

        /// <summary>
        /// Unsetup
        /// </summary>
        public override void Unsetup()
        {
            // Remove references
            DdPtr = null;
            GgPtr = null;
            SsPtr = null;
            BbPtr = null;
            DPdpPtr = null;
            SPspPtr = null;
            DdpPtr = null;
            GbPtr = null;
            GdpPtr = null;
            GspPtr = null;
            SspPtr = null;
            BdpPtr = null;
            BspPtr = null;
            DPspPtr = null;
            DPdPtr = null;
            BgPtr = null;
            DPgPtr = null;
            SPgPtr = null;
            SPsPtr = null;
            DPbPtr = null;
            SPbPtr = null;
            SPdpPtr = null;
        }

        /// <summary>
        /// Execute behavior
        /// </summary>
        /// <param name="sim">Base simulation</param>
        public override void Load(BaseSimulation sim)
        {
            if (sim == null)
                throw new ArgumentNullException(nameof(sim));

            var state = sim.State;
            double vt, EffectiveLength, DrainSatCur, SourceSatCur, Beta,
                OxideCap, vgs, vds, vbs, vbd, vgb, vgd, vgdo, von, evbs, evbd, vdsat,
                cdrain, ceqbs, ceqbd, cdreq;
            int Check, xnrm, xrev;

            vt = Circuit.KOverQ * bp.Temperature;
            Check = 1;


            /* DETAILPROF */

            /* first, we compute a few useful values - these could be
			* pre - computed, but for historical reasons are still done
			* here.  They may be moved at the expense of instance size
			*/

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

            if ((state.Init == State.InitFlags.InitFloat || state.UseSmallSignal || (state.Init == State.InitFlags.InitTransient)) ||
                ((state.Init == State.InitFlags.InitFix) && (!bp.Off)))
            {
                // General iteration
                vbs = mbp.Type * (state.Solution[bNode] - state.Solution[SourceNodePrime]);
                vgs = mbp.Type * (state.Solution[gNode] - state.Solution[SourceNodePrime]);
                vds = mbp.Type * (state.Solution[DrainNodePrime] - state.Solution[SourceNodePrime]);

                /* now some common crunching for some more useful quantities */
                /* DETAILPROF */

                vbd = vbs - vds;
                vgd = vgs - vds;
                vgdo = Vgs - Vds;
                von = mbp.Type * Von;

                /* 
				* limiting
				* we want to keep device voltages from changing
				* so fast that the exponentials churn out overflows
				* and similar rudeness
				*/

                // NOTE: Spice 3f5 does not write out Vgs during DC analysis, so DEVfetlim may give different results in Spice 3f5
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
                /* NODELIMITING */

            }
            else
            {
                /* DETAILPROF */
                /* ok - not one of the simple cases, so we have to
				* look at all of the possibilities for why we were
				* called.  We still just initialize the three voltages
				*/

                if ((state.Init == State.InitFlags.InitJct) && !bp.Off)
                {
                    vds = mbp.Type * bp.InitialVDS;
                    vgs = mbp.Type * bp.InitialVGS;
                    vbs = mbp.Type * bp.InitialVBS;
                    if ((vds == 0) && (vgs == 0) && (vbs == 0) && ((state.UseDC ||
                        state.Domain == State.DomainTypes.None) || (!state.UseIC)))
                    {
                        vbs = -1;
                        vgs = mbp.Type * temp.TVto;
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
            vgb = vgs - vbs;

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
                evbs = Math.Exp(Math.Min(Transistor.MAX_EXP_ARG, vbs / vt));
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
                evbd = Math.Exp(Math.Min(Transistor.MAX_EXP_ARG, vbd / vt));
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
                    phibs = temp.TPhi - (Mode == 1 ? vbs : vbd);
                    sqphbs = Math.Sqrt(phibs);
                    dsqdvb = -0.5 / sqphbs;
                }
                else
                {
                    sqphis = Math.Sqrt(temp.TPhi);
                    sqphs3 = temp.TPhi * sqphis;
                    sqphbs = sqphis / (1.0 + (Mode == 1 ? vbs : vbd) / (temp.TPhi + temp.TPhi));
                    phibs = sqphbs * sqphbs;
                    dsqdvb = -phibs / (sqphs3 + sqphs3);
                }
                /* 
				 * .....short channel effect factor
				 */
                if ((mbp.JunctionDepth != 0.0) && (modeltemp.CoeffDepLayWidth != 0.0))
                {
                    wps = modeltemp.CoeffDepLayWidth * sqphbs;
                    oneoverxj = 1.0 / mbp.JunctionDepth;
                    xjonxl = mbp.JunctionDepth * oneoverxl;
                    djonxj = mbp.LatDiff * oneoverxj;
                    wponxj = wps * oneoverxj;
                    wconxj = coeff0 + coeff1 * wponxj + coeff2 * wponxj * wponxj;
                    arga = wconxj + djonxj;
                    argc = wponxj / (1.0 + wponxj);
                    argb = Math.Sqrt(1.0 - argc * argc);
                    fshort = 1.0 - xjonxl * (arga * argb - djonxj);
                    dwpdvb = modeltemp.CoeffDepLayWidth * dsqdvb;
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
                vbix = temp.TVbi * mbp.Type - eta * (Mode * vds);
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
                        Gm = 0.0;
                        Gds = 0.0;
                        Gmbs = 0.0;
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
                us = temp.TSurfMob * 1e-4 /*(m**2/cm**2)*/ * fgate;
                dfgdvg = -mbp.Theta * fgate * fgate;
                dfgdvd = -dfgdvg * dvtdvd;
                dfgdvb = -dfgdvg * dvtdvb;
                /* 
				 * .....saturation voltage
				 */
                vdsat = (vgsx - vth) * onfbdy;
                if (mbp.MaxDriftVel <= 0.0)
                {
                    dvsdvg = onfbdy;
                    dvsdvd = -dvsdvg * dvtdvd;
                    dvsdvb = -dvsdvg * dvtdvb - vdsat * dfbdvb * onfbdy;
                }
                else
                {
                    vdsc = EffectiveLength * mbp.MaxDriftVel / us;
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
                Gm = vdsx;
                Gds = vgsx - vth - (1.0 + fbody + dvtdvd) * vdsx;
                Gmbs = dcodvb * vdsx;
                /* 
				 * .....drain current without velocity saturation effect
				 */
                cd1 = Beta * cdnorm;
                Beta = Beta * fgate;
                cdrain = Beta * cdnorm;
                Gm = Beta * Gm + dfgdvg * cd1;
                Gds = Beta * Gds + dfgdvd * cd1;
                Gmbs = Beta * Gmbs;
                /* 
				 * .....velocity saturation factor
				 */
                if (mbp.MaxDriftVel != 0.0)
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
                    Gm = fdrain * Gm + dfddvg * cdrain;
                    Gds = fdrain * Gds + dfddvd * cdrain;
                    Gmbs = fdrain * Gmbs + dfddvb * cdrain;
                    cdrain = fdrain * cdrain;
                    Beta = Beta * fdrain;
                }
                /* 
				 * .....channel length modulation
				 */
                if ((Mode * vds) <= vdsat) goto line700;
                if (mbp.MaxDriftVel <= 0.0) goto line510;
                if (modeltemp.Alpha == 0.0)
                    goto line700;
                cdsat = cdrain;
                gdsat = cdsat * (1.0 - fdrain) * onvdsc;
                gdsat = Math.Max(1.0e-12, gdsat);
                gdoncd = gdsat / cdsat;
                gdonfd = gdsat / (1.0 - fdrain);
                gdonfg = gdsat * onfg;
                dgdvg = gdoncd * Gm - gdonfd * dfddvg + gdonfg * dfgdvg;
                dgdvd = gdoncd * Gds - gdonfd * dfddvd + gdonfg * dfgdvd;
                dgdvb = gdoncd * Gmbs - gdonfd * dfddvb + gdonfg * dfgdvb;

                emax = mbp.Kappa * cdsat * oneoverxl / gdsat;
                emoncd = emax / cdsat;
                emongd = emax / gdsat;
                demdvg = emoncd * Gm - emongd * dgdvg;
                demdvd = emoncd * Gds - emongd * dgdvd;
                demdvb = emoncd * Gmbs - emongd * dgdvb;

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
                Gm = Gm * xlfact + diddl * ddldvg;
                gds0 = Gds * xlfact + diddl * ddldvd;
                Gmbs = Gmbs * xlfact + diddl * ddldvb;
                Gm = Gm + gds0 * dvsdvg;
                Gmbs = Gmbs + gds0 * dvsdvb;
                Gds = gds0 * dvsdvd + diddl * dldvd;
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
                    gms = Gm * wfact;
                    gmw = cdrain * ondvt;
                    Gm = gmw;
                    if ((Mode * vds) > vdsat)
                    {
                        Gm = Gm + gds0 * dvsdvg * wfact;
                    }
                    Gds = Gds * wfact + (gms - gmw) * dvodvd;
                    Gmbs = Gmbs * wfact + (gms - gmw) * dvodvb - gmw * ((Mode == 1 ? vgs : vgd) - von) * onxn * dxndvb;
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
                Gm = 0.0;
                Gds = Beta * (vgsx - vth);
                Gmbs = 0.0;
                if ((mbp.FastSurfaceStateDensity != 0.0) && ((Mode == 1 ? vgs : vgd) < von))
                {
                    Gds *= Math.Exp(((Mode == 1 ? vgs : vgd) - von) / (vt * xn));
                }
                innerline1000:;
                /* 
				 * .....done
				 */
            }

            /* DETAILPROF */

            /* now deal with n vs p polarity */

            Von = mbp.Type * von;
            Vdsat = mbp.Type * vdsat;
            /* line 490 */
            /* 
			* COMPUTE EQUIVALENT DRAIN CURRENT SOURCE
			*/
            Cd = Mode * cdrain - Cbd;



            /* 
			 * check convergence
			 */
            if (!bp.Off || (!(state.Init == State.InitFlags.InitFix || state.UseSmallSignal)))
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
            /* DETAILPROF */

            /* 
			 * meyer's capacitor model
			 */


            /* DETAILPROF */
            /* 
			 * load current vector
			 */
            ceqbs = mbp.Type * (Cbs - (Gbs - state.Gmin) * vbs);
            ceqbd = mbp.Type * (Cbd - (Gbd - state.Gmin) * vbd);
            if (Mode >= 0)
            {
                xnrm = 1;
                xrev = 0;
                cdreq = mbp.Type * (cdrain - Gds * vds - Gm * vgs - Gmbs * vbs);
            }
            else
            {
                xnrm = 0;
                xrev = 1;
                cdreq = -(mbp.Type) * (cdrain - Gds * (-vds) - Gm * vgd - Gmbs * vbd);
            }

            state.Rhs[bNode] -= (ceqbs + ceqbd);
            state.Rhs[DrainNodePrime] += (ceqbd - cdreq);
            state.Rhs[SourceNodePrime] += cdreq + ceqbs;

            /* 
			 * load y matrix
			 */
            DdPtr.Add(temp.DrainConductance);
            SsPtr.Add(temp.SourceConductance);
            BbPtr.Add(Gbd + Gbs);
            DPdpPtr.Add(temp.DrainConductance + Gds + Gbd + xrev * (Gm + Gmbs));
            SPspPtr.Add(temp.SourceConductance + Gds + Gbs + xnrm * (Gm + Gmbs));
            DdpPtr.Add(-temp.DrainConductance);
            SspPtr.Add(-temp.SourceConductance);
            BdpPtr.Sub(Gbd);
            BspPtr.Sub(Gbs);
            DPdPtr.Add(-temp.DrainConductance);
            DPgPtr.Add((xnrm - xrev) * Gm);
            DPbPtr.Add(-Gbd + (xnrm - xrev) * Gmbs);
            DPspPtr.Add(-Gds - xnrm * (Gm + Gmbs));
            SPgPtr.Add(-(xnrm - xrev) * Gm);
            SPsPtr.Add(-temp.SourceConductance);
            SPbPtr.Add(-Gbs - (xnrm - xrev) * Gmbs);
            SPdpPtr.Add(-Gds - xrev * (Gm + Gmbs));
        }

        /// <summary>
        /// Check convergence
        /// </summary>
        /// <param name="sim">Base simulation</param>
        /// <returns></returns>
        public override bool IsConvergent(BaseSimulation sim)
        {
			if (sim == null)
				throw new ArgumentNullException(nameof(sim));

            var state = sim.State;
            var config = sim.BaseConfiguration;

            double vbs, vgs, vds, vbd, vgd, vgdo, delvbs, delvbd, delvgs, delvds, delvgd, cdhat, cbhat;

            vbs = mbp.Type * (state.Solution[bNode] - state.Solution[SourceNodePrime]);
            vgs = mbp.Type * (state.Solution[gNode] - state.Solution[SourceNodePrime]);
            vds = mbp.Type * (state.Solution[DrainNodePrime] - state.Solution[SourceNodePrime]);
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
