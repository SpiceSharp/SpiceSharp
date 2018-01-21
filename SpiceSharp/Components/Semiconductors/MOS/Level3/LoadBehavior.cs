using System;
using SpiceSharp.Circuits;
using SpiceSharp.Components.Transistors;
using SpiceSharp.Components.Mosfet.Level3;
using SpiceSharp.Attributes;
using SpiceSharp.Sparse;

namespace SpiceSharp.Behaviors.Mosfet.Level3
{
    /// <summary>
    /// General behavior for a <see cref="Components.MOS3"/>
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
        [SpiceName("von"), SpiceInfo("Turn-on voltage")]
        public double MOS3von { get; protected set; }
        [SpiceName("vdsat"), SpiceInfo("Saturation drain voltage")]
        public double MOS3vdsat { get; protected set; }
        [SpiceName("id"), SpiceName("cd"), SpiceInfo("Drain current")]
        public double MOS3cd { get; protected set; }
        [SpiceName("ibs"), SpiceInfo("B-S junction current")]
        public double MOS3cbs { get; protected set; }
        [SpiceName("ibd"), SpiceInfo("B-D junction current")]
        public double MOS3cbd { get; protected set; }
        [SpiceName("gmb"), SpiceName("gmbs"), SpiceInfo("Bulk-Source transconductance")]
        public double MOS3gmbs { get; protected set; }
        [SpiceName("gm"), SpiceInfo("Transconductance")]
        public double MOS3gm { get; protected set; }
        [SpiceName("gds"), SpiceInfo("Drain-Source conductance")]
        public double MOS3gds { get; protected set; }
        [SpiceName("gbd"), SpiceInfo("Bulk-Drain conductance")]
        public double MOS3gbd { get; protected set; }
        [SpiceName("gbs"), SpiceInfo("Bulk-Source conductance")]
        public double MOS3gbs { get; protected set; }

        /// <summary>
        /// Extra variables
        /// </summary>
        public double MOS3mode { get; protected set; }
        public double MOS3vbd { get; protected set; }
        public double MOS3vbs { get; protected set; }
        public double MOS3vgs { get; protected set; }
        public double MOS3vds { get; protected set; }

        /// <summary>
        /// Nodes
        /// </summary>
        int MOS3dNode, MOS3gNode, MOS3sNode, MOS3bNode;
        [SpiceName("dnodeprime"), SpiceInfo("Number of protected drain node")]
        public int MOS3dNodePrime { get; protected set; }
        [SpiceName("snodeprime"), SpiceInfo("Number of protected source node")]
        public int MOS3sNodePrime { get; protected set; }
        protected MatrixElement MOS3DdPtr { get; private set; }
        protected MatrixElement MOS3GgPtr { get; private set; }
        protected MatrixElement MOS3SsPtr { get; private set; }
        protected MatrixElement MOS3BbPtr { get; private set; }
        protected MatrixElement MOS3DPdpPtr { get; private set; }
        protected MatrixElement MOS3SPspPtr { get; private set; }
        protected MatrixElement MOS3DdpPtr { get; private set; }
        protected MatrixElement MOS3GbPtr { get; private set; }
        protected MatrixElement MOS3GdpPtr { get; private set; }
        protected MatrixElement MOS3GspPtr { get; private set; }
        protected MatrixElement MOS3SspPtr { get; private set; }
        protected MatrixElement MOS3BdpPtr { get; private set; }
        protected MatrixElement MOS3BspPtr { get; private set; }
        protected MatrixElement MOS3DPspPtr { get; private set; }
        protected MatrixElement MOS3DPdPtr { get; private set; }
        protected MatrixElement MOS3BgPtr { get; private set; }
        protected MatrixElement MOS3DPgPtr { get; private set; }
        protected MatrixElement MOS3SPgPtr { get; private set; }
        protected MatrixElement MOS3SPsPtr { get; private set; }
        protected MatrixElement MOS3DPbPtr { get; private set; }
        protected MatrixElement MOS3SPbPtr { get; private set; }
        protected MatrixElement MOS3SPdpPtr { get; private set; }

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
            // Get parameters
            bp = provider.GetParameters<BaseParameters>();
            mbp = provider.GetParameters<ModelBaseParameters>(1);

            // Get behaviors
            temp = provider.GetBehavior<TemperatureBehavior>();
            modeltemp = provider.GetBehavior<ModelTemperatureBehavior>(1);

            // Initialize some variable
            MOS3vdsat = 0;
            MOS3von = 0;
            MOS3mode = 1;
        }

        /// <summary>
        /// Connect
        /// </summary>
        /// <param name="pins">Pins</param>
        public void Connect(params int[] pins)
        {
            MOS3dNode = pins[0];
            MOS3gNode = pins[1];
            MOS3sNode = pins[2];
            MOS3bNode = pins[3];
        }

        /// <summary>
        /// Get matrix pointers
        /// </summary>
        /// <param name="nodes">Nodes</param>
        /// <param name="matrix">Matrix</param>
        public override void GetMatrixPointers(Nodes nodes, Matrix matrix)
        {
            // Add a series drain node if necessary
            if (mbp.MOS3drainResistance != 0 || (mbp.MOS3sheetResistance != 0 && bp.MOS3drainSquares != 0))
                MOS3dNodePrime = nodes.Create(Name.Grow("#drain")).Index;
            else
                MOS3dNodePrime = MOS3dNode;

            // Add a series source node if necessary
            if (mbp.MOS3sourceResistance != 0 || (mbp.MOS3sheetResistance != 0 && bp.MOS3sourceSquares != 0))
                MOS3sNodePrime = nodes.Create(Name.Grow("#source")).Index;
            else
                MOS3sNodePrime = MOS3sNode;

            // Get matrix elements
            MOS3DdPtr = matrix.GetElement(MOS3dNode, MOS3dNode);
            MOS3GgPtr = matrix.GetElement(MOS3gNode, MOS3gNode);
            MOS3SsPtr = matrix.GetElement(MOS3sNode, MOS3sNode);
            MOS3BbPtr = matrix.GetElement(MOS3bNode, MOS3bNode);
            MOS3DPdpPtr = matrix.GetElement(MOS3dNodePrime, MOS3dNodePrime);
            MOS3SPspPtr = matrix.GetElement(MOS3sNodePrime, MOS3sNodePrime);
            MOS3DdpPtr = matrix.GetElement(MOS3dNode, MOS3dNodePrime);
            MOS3GbPtr = matrix.GetElement(MOS3gNode, MOS3bNode);
            MOS3GdpPtr = matrix.GetElement(MOS3gNode, MOS3dNodePrime);
            MOS3GspPtr = matrix.GetElement(MOS3gNode, MOS3sNodePrime);
            MOS3SspPtr = matrix.GetElement(MOS3sNode, MOS3sNodePrime);
            MOS3BdpPtr = matrix.GetElement(MOS3bNode, MOS3dNodePrime);
            MOS3BspPtr = matrix.GetElement(MOS3bNode, MOS3sNodePrime);
            MOS3DPspPtr = matrix.GetElement(MOS3dNodePrime, MOS3sNodePrime);
            MOS3DPdPtr = matrix.GetElement(MOS3dNodePrime, MOS3dNode);
            MOS3BgPtr = matrix.GetElement(MOS3bNode, MOS3gNode);
            MOS3DPgPtr = matrix.GetElement(MOS3dNodePrime, MOS3gNode);
            MOS3SPgPtr = matrix.GetElement(MOS3sNodePrime, MOS3gNode);
            MOS3SPsPtr = matrix.GetElement(MOS3sNodePrime, MOS3sNode);
            MOS3DPbPtr = matrix.GetElement(MOS3dNodePrime, MOS3bNode);
            MOS3SPbPtr = matrix.GetElement(MOS3sNodePrime, MOS3bNode);
            MOS3SPdpPtr = matrix.GetElement(MOS3sNodePrime, MOS3dNodePrime);
        }

        /// <summary>
        /// Unsetup
        /// </summary>
        public override void Unsetup()
        {
            // Remove references
            MOS3DdPtr = null;
            MOS3GgPtr = null;
            MOS3SsPtr = null;
            MOS3BbPtr = null;
            MOS3DPdpPtr = null;
            MOS3SPspPtr = null;
            MOS3DdpPtr = null;
            MOS3GbPtr = null;
            MOS3GdpPtr = null;
            MOS3GspPtr = null;
            MOS3SspPtr = null;
            MOS3BdpPtr = null;
            MOS3BspPtr = null;
            MOS3DPspPtr = null;
            MOS3DPdPtr = null;
            MOS3BgPtr = null;
            MOS3DPgPtr = null;
            MOS3SPgPtr = null;
            MOS3SPsPtr = null;
            MOS3DPbPtr = null;
            MOS3SPbPtr = null;
            MOS3SPdpPtr = null;
        }

        /// <summary>
        /// Execute behavior
        /// </summary>
        /// <param name="ckt"></param>
        public override void Load(Circuit ckt)
        {
            var state = ckt.State;
            var rstate = state;
            var method = ckt.Method;
            double vt, EffectiveLength, DrainSatCur, SourceSatCur, GateSourceOverlapCap, GateDrainOverlapCap, GateBulkOverlapCap, Beta,
                OxideCap, vgs, vds, vbs, vbd, vgb, vgd, vgdo, delvbs, delvbd, delvgs, delvds, delvgd, cdhat, cbhat, von, evbs, evbd, vdsat,
                cdrain, sargsw, vgs1, vgd1, vgb1, capgs = 0.0, capgd = 0.0, capgb = 0.0, gcgs, ceqgs, gcgd, ceqgd, gcgb, ceqgb, ceqbs, ceqbd, cdreq;
            int Check, xnrm, xrev;

            vt = Circuit.CONSTKoverQ * bp.MOS3temp;
            Check = 1;

            /* DETAILPROF */

            /* first, we compute a few useful values - these could be
			* pre - computed, but for historical reasons are still done
			* here.  They may be moved at the expense of instance size
			*/

            EffectiveLength = bp.MOS3l - 2 * mbp.MOS3latDiff;
            if ((temp.MOS3tSatCurDens == 0) || (bp.MOS3drainArea.Value == 0) || (bp.MOS3sourceArea.Value == 0))
            {
                DrainSatCur = temp.MOS3tSatCur;
                SourceSatCur = temp.MOS3tSatCur;
            }
            else
            {
                DrainSatCur = temp.MOS3tSatCurDens * bp.MOS3drainArea;
                SourceSatCur = temp.MOS3tSatCurDens * bp.MOS3sourceArea;
            }

            Beta = temp.MOS3tTransconductance * bp.MOS3w / EffectiveLength;
            OxideCap = modeltemp.MOS3oxideCapFactor * EffectiveLength * bp.MOS3w;

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
                ((state.Init == State.InitFlags.InitFix) && (!bp.MOS3off)))
            {
                // General iteration
                vbs = mbp.MOS3type * (rstate.Solution[MOS3bNode] - rstate.Solution[MOS3sNodePrime]);
                vgs = mbp.MOS3type * (rstate.Solution[MOS3gNode] - rstate.Solution[MOS3sNodePrime]);
                vds = mbp.MOS3type * (rstate.Solution[MOS3dNodePrime] - rstate.Solution[MOS3sNodePrime]);

                /* now some common crunching for some more useful quantities */
                /* DETAILPROF */

                vbd = vbs - vds;
                vgd = vgs - vds;
                vgdo = MOS3vgs - MOS3vds;
                von = mbp.MOS3type * MOS3von;

                /* 
				* limiting
				* we want to keep device voltages from changing
				* so fast that the exponentials churn out overflows
				* and similar rudeness
				*/

                if (MOS3vds >= 0)
                {
                    vgs = Transistor.DEVfetlim(vgs, MOS3vgs, von);
                    vds = vgs - vgd;
                    vds = Transistor.DEVlimvds(vds, MOS3vds);
                    vgd = vgs - vds;
                }
                else
                {
                    vgd = Transistor.DEVfetlim(vgd, vgdo, von);
                    vds = vgs - vgd;
                    vds = -Transistor.DEVlimvds(-vds, -MOS3vds);
                    vgs = vgd + vds;
                }
                if (vds >= 0)
                {
                    vbs = Transistor.DEVpnjlim(vbs, MOS3vbs, vt, temp.MOS3sourceVcrit, ref Check);
                    vbd = vbs - vds;
                }
                else
                {
                    vbd = Transistor.DEVpnjlim(vbd, MOS3vbd, vt, temp.MOS3drainVcrit, ref Check);
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

                if ((state.Init == State.InitFlags.InitJct) && !bp.MOS3off)
                {
                    vds = mbp.MOS3type * bp.MOS3icVDS;
                    vgs = mbp.MOS3type * bp.MOS3icVGS;
                    vbs = mbp.MOS3type * bp.MOS3icVBS;
                    if ((vds == 0) && (vgs == 0) && (vbs == 0) && ((method != null || state.UseDC ||
                        state.Domain == State.DomainTypes.None) || (!state.UseIC)))
                    {
                        vbs = -1;
                        vgs = mbp.MOS3type * temp.MOS3tVto;
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
                MOS3gbs = SourceSatCur / vt;
                MOS3cbs = MOS3gbs * vbs;
                MOS3gbs += state.Gmin;
            }
            else
            {
                evbs = Math.Exp(Math.Min(Transistor.MAX_EXP_ARG, vbs / vt));
                MOS3gbs = SourceSatCur * evbs / vt + state.Gmin;
                MOS3cbs = SourceSatCur * (evbs - 1);
            }
            if (vbd <= 0)
            {
                MOS3gbd = DrainSatCur / vt;
                MOS3cbd = MOS3gbd * vbd;
                MOS3gbd += state.Gmin;
            }
            else
            {
                evbd = Math.Exp(Math.Min(Transistor.MAX_EXP_ARG, vbd / vt));
                MOS3gbd = DrainSatCur * evbd / vt + state.Gmin;
                MOS3cbd = DrainSatCur * (evbd - 1);
            }

            /* now to determine whether the user was able to correctly
			* identify the source and drain of his device
			*/
            if (vds >= 0)
            {
                /* normal mode */
                MOS3mode = 1;
            }
            else
            {
                /* inverse mode */
                MOS3mode = -1;
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
                eta = mbp.MOS3eta * 8.15e-22 / (modeltemp.MOS3oxideCapFactor * EffectiveLength * EffectiveLength * EffectiveLength);
                /* 
				* .....square root term
				*/
                if ((MOS3mode == 1 ? vbs : vbd) <= 0.0)
                {
                    phibs = temp.MOS3tPhi - (MOS3mode == 1 ? vbs : vbd);
                    sqphbs = Math.Sqrt(phibs);
                    dsqdvb = -0.5 / sqphbs;
                }
                else
                {
                    sqphis = Math.Sqrt(temp.MOS3tPhi);
                    sqphs3 = temp.MOS3tPhi * sqphis;
                    sqphbs = sqphis / (1.0 + (MOS3mode == 1 ? vbs : vbd) / (temp.MOS3tPhi + temp.MOS3tPhi));
                    phibs = sqphbs * sqphbs;
                    dsqdvb = -phibs / (sqphs3 + sqphs3);
                }
                /* 
				 * .....short channel effect factor
				 */
                if ((mbp.MOS3junctionDepth != 0.0) && (modeltemp.MOS3coeffDepLayWidth != 0.0))
                {
                    wps = modeltemp.MOS3coeffDepLayWidth * sqphbs;
                    oneoverxj = 1.0 / mbp.MOS3junctionDepth;
                    xjonxl = mbp.MOS3junctionDepth * oneoverxl;
                    djonxj = mbp.MOS3latDiff * oneoverxj;
                    wponxj = wps * oneoverxj;
                    wconxj = coeff0 + coeff1 * wponxj + coeff2 * wponxj * wponxj;
                    arga = wconxj + djonxj;
                    argc = wponxj / (1.0 + wponxj);
                    argb = Math.Sqrt(1.0 - argc * argc);
                    fshort = 1.0 - xjonxl * (arga * argb - djonxj);
                    dwpdvb = modeltemp.MOS3coeffDepLayWidth * dsqdvb;
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
                gammas = mbp.MOS3gamma * fshort;
                fbodys = 0.5 * gammas / (sqphbs + sqphbs);
                fbody = fbodys + mbp.MOS3narrowFactor / bp.MOS3w;
                onfbdy = 1.0 / (1.0 + fbody);
                dfbdvb = -fbodys * dsqdvb / sqphbs + fbodys * dfsdvb / fshort;
                qbonco = gammas * sqphbs + mbp.MOS3narrowFactor * phibs / bp.MOS3w;
                dqbdvb = gammas * dsqdvb + mbp.MOS3gamma * dfsdvb * sqphbs - mbp.MOS3narrowFactor / bp.MOS3w;
                /* 
				 * .....static feedback effect
				 */
                vbix = temp.MOS3tVbi * mbp.MOS3type - eta * (MOS3mode * vds);
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
                if (mbp.MOS3fastSurfaceStateDensity != 0.0)
                {
                    csonco = Circuit.CHARGE * mbp.MOS3fastSurfaceStateDensity * 1e4 /* (cm *  * 2 / m *  * 2) */  * EffectiveLength * bp.MOS3w /
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
                    if ((MOS3mode == 1 ? vgs : vgd) <= von)
                    {
                        cdrain = 0.0;
                        MOS3gm = 0.0;
                        MOS3gds = 0.0;
                        MOS3gmbs = 0.0;
                        goto innerline1000;
                    }
                }
                /* 
				 * .....device is on
				 */
                vgsx = Math.Max((MOS3mode == 1 ? vgs : vgd), von);
                /* 
				 * .....mobility modulation by gate voltage
				 */
                onfg = 1.0 + mbp.MOS3theta * (vgsx - vth);
                fgate = 1.0 / onfg;
                us = temp.MOS3tSurfMob * 1e-4 /*(m**2/cm**2)*/ * fgate;
                dfgdvg = -mbp.MOS3theta * fgate * fgate;
                dfgdvd = -dfgdvg * dvtdvd;
                dfgdvb = -dfgdvg * dvtdvb;
                /* 
				 * .....saturation voltage
				 */
                vdsat = (vgsx - vth) * onfbdy;
                if (mbp.MOS3maxDriftVel <= 0.0)
                {
                    dvsdvg = onfbdy;
                    dvsdvd = -dvsdvg * dvtdvd;
                    dvsdvb = -dvsdvg * dvtdvb - vdsat * dfbdvb * onfbdy;
                }
                else
                {
                    vdsc = EffectiveLength * mbp.MOS3maxDriftVel / us;
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
                vdsx = Math.Min((MOS3mode * vds), vdsat);
                if (vdsx == 0.0)
                    goto line900;
                cdo = vgsx - vth - 0.5 * (1.0 + fbody) * vdsx;
                dcodvb = -dvtdvb - 0.5 * dfbdvb * vdsx;
                /* 
				 * .....normalized drain current
				 */
                cdnorm = cdo * vdsx;
                MOS3gm = vdsx;
                MOS3gds = vgsx - vth - (1.0 + fbody + dvtdvd) * vdsx;
                MOS3gmbs = dcodvb * vdsx;
                /* 
				 * .....drain current without velocity saturation effect
				 */
                cd1 = Beta * cdnorm;
                Beta = Beta * fgate;
                cdrain = Beta * cdnorm;
                MOS3gm = Beta * MOS3gm + dfgdvg * cd1;
                MOS3gds = Beta * MOS3gds + dfgdvd * cd1;
                MOS3gmbs = Beta * MOS3gmbs;
                /* 
				 * .....velocity saturation factor
				 */
                if (mbp.MOS3maxDriftVel != 0.0)
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
                    MOS3gm = fdrain * MOS3gm + dfddvg * cdrain;
                    MOS3gds = fdrain * MOS3gds + dfddvd * cdrain;
                    MOS3gmbs = fdrain * MOS3gmbs + dfddvb * cdrain;
                    cdrain = fdrain * cdrain;
                    Beta = Beta * fdrain;
                }
                /* 
				 * .....channel length modulation
				 */
                if ((MOS3mode * vds) <= vdsat) goto line700;
                if (mbp.MOS3maxDriftVel <= 0.0) goto line510;
                if (modeltemp.MOS3alpha == 0.0)
                    goto line700;
                cdsat = cdrain;
                gdsat = cdsat * (1.0 - fdrain) * onvdsc;
                gdsat = Math.Max(1.0e-12, gdsat);
                gdoncd = gdsat / cdsat;
                gdonfd = gdsat / (1.0 - fdrain);
                gdonfg = gdsat * onfg;
                dgdvg = gdoncd * MOS3gm - gdonfd * dfddvg + gdonfg * dfgdvg;
                dgdvd = gdoncd * MOS3gds - gdonfd * dfddvd + gdonfg * dfgdvd;
                dgdvb = gdoncd * MOS3gmbs - gdonfd * dfddvb + gdonfg * dfgdvb;

                emax = mbp.MOS3kappa * cdsat * oneoverxl / gdsat;
                emoncd = emax / cdsat;
                emongd = emax / gdsat;
                demdvg = emoncd * MOS3gm - emongd * dgdvg;
                demdvd = emoncd * MOS3gds - emongd * dgdvd;
                demdvb = emoncd * MOS3gmbs - emongd * dgdvb;

                arga = 0.5 * emax * modeltemp.MOS3alpha;
                argc = mbp.MOS3kappa * modeltemp.MOS3alpha;
                argb = Math.Sqrt(arga * arga + argc * ((MOS3mode * vds) - vdsat));
                delxl = argb - arga;
                dldvd = argc / (argb + argb);
                dldem = 0.5 * (arga / argb - 1.0) * modeltemp.MOS3alpha;
                ddldvg = dldem * demdvg;
                ddldvd = dldem * demdvd - dldvd;
                ddldvb = dldem * demdvb;
                goto line520;
                line510:
                delxl = Math.Sqrt(mbp.MOS3kappa * ((MOS3mode * vds) - vdsat) * modeltemp.MOS3alpha);
                dldvd = 0.5 * delxl / ((MOS3mode * vds) - vdsat);
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
                MOS3gm = MOS3gm * xlfact + diddl * ddldvg;
                gds0 = MOS3gds * xlfact + diddl * ddldvd;
                MOS3gmbs = MOS3gmbs * xlfact + diddl * ddldvb;
                MOS3gm = MOS3gm + gds0 * dvsdvg;
                MOS3gmbs = MOS3gmbs + gds0 * dvsdvb;
                MOS3gds = gds0 * dvsdvd + diddl * dldvd;
                /* 
				 * .....finish strong inversion case
				 */
                line700:
                if ((MOS3mode == 1 ? vgs : vgd) < von)
                {
                    /* 
					 * .....weak inversion
					 */
                    onxn = 1.0 / xn;
                    ondvt = onxn / vt;
                    wfact = Math.Exp(((MOS3mode == 1 ? vgs : vgd) - von) * ondvt);
                    cdrain = cdrain * wfact;
                    gms = MOS3gm * wfact;
                    gmw = cdrain * ondvt;
                    MOS3gm = gmw;
                    if ((MOS3mode * vds) > vdsat)
                    {
                        MOS3gm = MOS3gm + gds0 * dvsdvg * wfact;
                    }
                    MOS3gds = MOS3gds * wfact + (gms - gmw) * dvodvd;
                    MOS3gmbs = MOS3gmbs * wfact + (gms - gmw) * dvodvb - gmw * ((MOS3mode == 1 ? vgs : vgd) - von) * onxn * dxndvb;
                }
                /* 
				 * .....charge computation
				 */
                goto innerline1000;
                /* 
				 * .....special case of vds = 0.0d0 */
                line900: Beta = Beta * fgate;
                cdrain = 0.0;
                MOS3gm = 0.0;
                MOS3gds = Beta * (vgsx - vth);
                MOS3gmbs = 0.0;
                if ((mbp.MOS3fastSurfaceStateDensity != 0.0) && ((MOS3mode == 1 ? vgs : vgd) < von))
                {
                    MOS3gds *= Math.Exp(((MOS3mode == 1 ? vgs : vgd) - von) / (vt * xn));
                }
                innerline1000:;
                /* 
				 * .....done
				 */
            }

            /* DETAILPROF */

            /* now deal with n vs p polarity */

            MOS3von = mbp.MOS3type * von;
            MOS3vdsat = mbp.MOS3type * vdsat;
            /* line 490 */
            /* 
			* COMPUTE EQUIVALENT DRAIN CURRENT SOURCE
			*/
            MOS3cd = MOS3mode * cdrain - MOS3cbd;



            /* 
			 * check convergence
			 */
            if (!bp.MOS3off || (!(state.Init == State.InitFlags.InitFix || state.UseSmallSignal)))
            {
                if (Check == 1)
                    state.IsCon = false;
            }

            /* DETAILPROF */

            /* save things away for next time */
            MOS3vbs = vbs;
            MOS3vbd = vbd;
            MOS3vgs = vgs;
            MOS3vds = vds;
            /* DETAILPROF */

            /* 
			 * meyer's capacitor model
			 */


            /* DETAILPROF */
            /* 
			 * load current vector
			 */
            ceqbs = mbp.MOS3type * (MOS3cbs - (MOS3gbs - state.Gmin) * vbs);
            ceqbd = mbp.MOS3type * (MOS3cbd - (MOS3gbd - state.Gmin) * vbd);
            if (MOS3mode >= 0)
            {
                xnrm = 1;
                xrev = 0;
                cdreq = mbp.MOS3type * (cdrain - MOS3gds * vds - MOS3gm * vgs - MOS3gmbs * vbs);
            }
            else
            {
                xnrm = 0;
                xrev = 1;
                cdreq = -(mbp.MOS3type) * (cdrain - MOS3gds * (-vds) - MOS3gm * vgd - MOS3gmbs * vbd);
            }

            rstate.Rhs[MOS3bNode] -= (ceqbs + ceqbd);
            rstate.Rhs[MOS3dNodePrime] += (ceqbd - cdreq);
            rstate.Rhs[MOS3sNodePrime] += cdreq + ceqbs;

            /* 
			 * load y matrix
			 */
            MOS3DdPtr.Add(temp.MOS3drainConductance);
            MOS3SsPtr.Add(temp.MOS3sourceConductance);
            MOS3BbPtr.Add(MOS3gbd + MOS3gbs);
            MOS3DPdpPtr.Add(temp.MOS3drainConductance + MOS3gds + MOS3gbd + xrev * (MOS3gm + MOS3gmbs));
            MOS3SPspPtr.Add(temp.MOS3sourceConductance + MOS3gds + MOS3gbs + xnrm * (MOS3gm + MOS3gmbs));
            MOS3DdpPtr.Add(-temp.MOS3drainConductance);
            MOS3SspPtr.Add(-temp.MOS3sourceConductance);
            MOS3BdpPtr.Sub(MOS3gbd);
            MOS3BspPtr.Sub(MOS3gbs);
            MOS3DPdPtr.Add(-temp.MOS3drainConductance);
            MOS3DPgPtr.Add((xnrm - xrev) * MOS3gm);
            MOS3DPbPtr.Add(-MOS3gbd + (xnrm - xrev) * MOS3gmbs);
            MOS3DPspPtr.Add(-MOS3gds - xnrm * (MOS3gm + MOS3gmbs));
            MOS3SPgPtr.Add(-(xnrm - xrev) * MOS3gm);
            MOS3SPsPtr.Add(-temp.MOS3sourceConductance);
            MOS3SPbPtr.Add(-MOS3gbs - (xnrm - xrev) * MOS3gmbs);
            MOS3SPdpPtr.Add(-MOS3gds - xrev * (MOS3gm + MOS3gmbs));
        }

        /// <summary>
        /// Check convergence
        /// </summary>
        /// <param name="ckt">Circuit</param>
        /// <returns></returns>
        public override bool IsConvergent(Circuit ckt)
        {
            var state = ckt.State;

            double vbs, vgs, vds, vbd, vgd, vgdo, delvbs, delvbd, delvgs, delvds, delvgd, cdhat, cbhat;

            vbs = mbp.MOS3type * (state.Solution[MOS3bNode] - state.Solution[MOS3sNodePrime]);
            vgs = mbp.MOS3type * (state.Solution[MOS3gNode] - state.Solution[MOS3sNodePrime]);
            vds = mbp.MOS3type * (state.Solution[MOS3dNodePrime] - state.Solution[MOS3sNodePrime]);
            vbd = vbs - vds;
            vgd = vgs - vds;
            vgdo = MOS3vgs - MOS3vds;
            delvbs = vbs - MOS3vbs;
            delvbd = vbd - MOS3vbd;
            delvgs = vgs - MOS3vgs;
            delvds = vds - MOS3vds;
            delvgd = vgd - vgdo;

            /* these are needed for convergence testing */

            if (MOS3mode >= 0)
            {
                cdhat = MOS3cd - MOS3gbd * delvbd + MOS3gmbs * delvbs +
                    MOS3gm * delvgs + MOS3gds * delvds;
            }
            else
            {
                cdhat = MOS3cd - (MOS3gbd - MOS3gmbs) * delvbd -
                    MOS3gm * delvgd + MOS3gds * delvds;
            }
            cbhat = MOS3cbs + MOS3cbd + MOS3gbd * delvbd + MOS3gbs * delvbs;

            /*
             *  check convergence
             */
            double tol = 1e-3 * Math.Max(Math.Abs(cdhat), Math.Abs(MOS3cd)) + 1e-12;
            if (Math.Abs(cdhat - MOS3cd) >= tol)
            {
                state.IsCon = false;
                return false;
            }

            tol = 1e-3 * Math.Max(Math.Abs(cbhat), Math.Abs(MOS3cbs + MOS3cbd)) + 1e-12;
            if (Math.Abs(cbhat - (MOS3cbs + MOS3cbd)) > tol)
            {
                state.IsCon = false;
                return false;
            }

            return true;
        }
    }
}
