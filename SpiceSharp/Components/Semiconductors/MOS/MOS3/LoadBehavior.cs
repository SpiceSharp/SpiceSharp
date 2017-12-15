using System;
using SpiceSharp.Circuits;
using SpiceSharp.Components.Transistors;
using SpiceSharp.Parameters;
using SpiceSharp.Sparse;
using SpiceSharp.Diagnostics;

namespace SpiceSharp.Behaviors.MOS3
{
    /// <summary>
    /// General behaviour for a <see cref="Components.MOS3"/>
    /// </summary>
    public class LoadBehavior : Behaviors.LoadBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        private TemperatureBehavior temp;
        private ModelTemperatureBehavior modeltemp;

        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("off"), SpiceInfo("Device initially off")]
        public bool MOS3off { get; set; }
        [SpiceName("icvbs"), SpiceInfo("Initial B-S voltage")]
        public Parameter MOS3icVBS { get; } = new Parameter();
        [SpiceName("icvds"), SpiceInfo("Initial D-S voltage")]
        public Parameter MOS3icVDS { get; } = new Parameter();
        [SpiceName("icvgs"), SpiceInfo("Initial G-S voltage")]
        public Parameter MOS3icVGS { get; } = new Parameter();
        [SpiceName("von"), SpiceInfo("Turn-on voltage")]
        public double MOS3von { get; internal set; }
        [SpiceName("vdsat"), SpiceInfo("Saturation drain voltage")]
        public double MOS3vdsat { get; internal set; }
        [SpiceName("id"), SpiceName("cd"), SpiceInfo("Drain current")]
        public double MOS3cd { get; internal set; }
        [SpiceName("ibs"), SpiceInfo("B-S junction current")]
        public double MOS3cbs { get; internal set; }
        [SpiceName("ibd"), SpiceInfo("B-D junction current")]
        public double MOS3cbd { get; internal set; }
        [SpiceName("gmb"), SpiceName("gmbs"), SpiceInfo("Bulk-Source transconductance")]
        public double MOS3gmbs { get; internal set; }
        [SpiceName("gm"), SpiceInfo("Transconductance")]
        public double MOS3gm { get; internal set; }
        [SpiceName("gds"), SpiceInfo("Drain-Source conductance")]
        public double MOS3gds { get; internal set; }
        [SpiceName("gbd"), SpiceInfo("Bulk-Drain conductance")]
        public double MOS3gbd { get; internal set; }
        [SpiceName("gbs"), SpiceInfo("Bulk-Source conductance")]
        public double MOS3gbs { get; internal set; }
        [SpiceName("cbd"), SpiceInfo("Bulk-Drain capacitance")]
        public double MOS3capbd { get; internal set; }
        [SpiceName("cbs"), SpiceInfo("Bulk-Source capacitance")]
        public double MOS3capbs { get; internal set; }

        /// <summary>
        /// Methods
        /// </summary>
        [SpiceName("ic"), SpiceInfo("Vector of D-S, G-S, B-S voltages")]
        public void SetIC(double[] value)
        {
            switch (value.Length)
            {
                case 3: MOS3icVBS.Set(value[2]); goto case 2;
                case 2: MOS3icVGS.Set(value[1]); goto case 1;
                case 1: MOS3icVDS.Set(value[0]); break;
                default:
                    throw new CircuitException("Bad parameter");
            }
        }

        [SpiceName("vbd"), SpiceInfo("Bulk-Drain voltage")]
        public double GetVBD(Circuit ckt) => ckt.State.States[0][MOS3states + MOS3vbd];
        [SpiceName("vbs"), SpiceInfo("Bulk-Source voltage")]
        public double GetVBS(Circuit ckt) => ckt.State.States[0][MOS3states + MOS3vbs];
        [SpiceName("vgs"), SpiceInfo("Gate-Source voltage")]
        public double GetVGS(Circuit ckt) => ckt.State.States[0][MOS3states + MOS3vgs];
        [SpiceName("vds"), SpiceInfo("Drain-Source voltage")]
        public double GetVDS(Circuit ckt) => ckt.State.States[0][MOS3states + MOS3vds];
        [SpiceName("cgs"), SpiceInfo("Gate-Source capacitance")]
        public double GetCAPGS(Circuit ckt) => ckt.State.States[0][MOS3states + MOS3capgs];
        [SpiceName("qgs"), SpiceInfo("Gate-Source charge storage")]
        public double GetQGS(Circuit ckt) => ckt.State.States[0][MOS3states + MOS3qgs];
        [SpiceName("cqgs"), SpiceInfo("Capacitance due to gate-source charge storage")]
        public double GetCQGS(Circuit ckt) => ckt.State.States[0][MOS3states + MOS3cqgs];
        [SpiceName("cgd"), SpiceInfo("Gate-Drain capacitance")]
        public double GetCAPGD(Circuit ckt) => ckt.State.States[0][MOS3states + MOS3capgd];
        [SpiceName("qgd"), SpiceInfo("Gate-Drain charge storage")]
        public double GetQGD(Circuit ckt) => ckt.State.States[0][MOS3states + MOS3qgd];
        [SpiceName("cqgd"), SpiceInfo("Capacitance due to gate-drain charge storage")]
        public double GetCQGD(Circuit ckt) => ckt.State.States[0][MOS3states + MOS3cqgd];
        [SpiceName("cgb"), SpiceInfo("Gate-Bulk capacitance")]
        public double GetCAPGB(Circuit ckt) => ckt.State.States[0][MOS3states + MOS3capgb];
        [SpiceName("qgb"), SpiceInfo("Gate-Bulk charge storage")]
        public double GetQGB(Circuit ckt) => ckt.State.States[0][MOS3states + MOS3qgb];
        [SpiceName("cqgb"), SpiceInfo("Capacitance due to gate-bulk charge storage")]
        public double GetCQGB(Circuit ckt) => ckt.State.States[0][MOS3states + MOS3cqgb];
        [SpiceName("qbd"), SpiceInfo("Bulk-Drain charge storage")]
        public double GetQBD(Circuit ckt) => ckt.State.States[0][MOS3states + MOS3qbd];
        [SpiceName("cqbd"), SpiceInfo("Capacitance due to bulk-drain charge storage")]
        public double GetCQBD(Circuit ckt) => ckt.State.States[0][MOS3states + MOS3cqbd];
        [SpiceName("qbs"), SpiceInfo("Bulk-Source charge storage")]
        public double GetQBS(Circuit ckt) => ckt.State.States[0][MOS3states + MOS3qbs];
        [SpiceName("cqbs"), SpiceInfo("Capacitance due to bulk-source charge storage")]
        public double GetCQBS(Circuit ckt) => ckt.State.States[0][MOS3states + MOS3cqbs];
        [SpiceName("ib"), SpiceInfo("Bulk current")]
        public double GetCB(Circuit ckt) => MOS3cbd + MOS3cbs - ckt.State.States[0][MOS3states + MOS3cqgb];
        [SpiceName("ig"), SpiceInfo("Gate current")]
        public double GetCG(Circuit ckt) => ckt.State.UseDC ? 0.0 : ckt.State.States[0][MOS3states + MOS3cqgb] + ckt.State.States[0][MOS3states + MOS3cqgd] +
            ckt.State.States[0][MOS3states + MOS3cqgs];
        [SpiceName("is"), SpiceInfo("Source current")]
        public double GetCS(Circuit ckt)
        {
            double value = -MOS3cd;
            value -= MOS3cbd + MOS3cbs - ckt.State.States[0][MOS3states + MOS3cqgb];
            if (ckt.State.Domain == CircuitState.DomainTypes.Time && !ckt.State.UseDC)
            {
                value -= ckt.State.States[0][MOS3states + MOS3cqgb] + ckt.State.States[0][MOS3states + MOS3cqgd] +
                    ckt.State.States[0][MOS3states + MOS3cqgs];
            }
            return value;
        }
        [SpiceName("p"), SpiceInfo("Instantaneous power")]
        public double GetPOWER(Circuit ckt)
        {
            double temp;

            double value = MOS3cd * ckt.State.Solution[MOS3dNode];
            value += (MOS3cbd + MOS3cbs - ckt.State.States[0][MOS3states + MOS3cqgb]) * ckt.State.Solution[MOS3bNode];
            if (ckt.State.Domain == CircuitState.DomainTypes.Time && !ckt.State.UseDC)
            {
                value += (ckt.State.States[0][MOS3states + MOS3cqgb] + ckt.State.States[0][MOS3states + MOS3cqgd] +
                    ckt.State.States[0][MOS3states + MOS3cqgs]) * ckt.State.Solution[MOS3gNode];
            }
            temp = -MOS3cd;
            temp -= MOS3cbd + MOS3cbs;
            if (ckt.State.Domain == CircuitState.DomainTypes.Time && !ckt.State.UseDC)
            {
                temp -= ckt.State.States[0][MOS3states + MOS3cqgb] + ckt.State.States[0][MOS3states + MOS3cqgd] +
                    ckt.State.States[0][MOS3states + MOS3cqgs];
            }
            value += temp * ckt.State.Solution[MOS3sNode];
            return value;
        }

        /// <summary>
        /// Extra variables
        /// </summary>
        public double MOS3mode { get; internal set; }
        public int MOS3states { get; internal set; }

        /// <summary>
        /// Matrix elements
        /// </summary>
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
        /// Constants
        /// </summary>
        public const int MOS3vbd = 0;
        public const int MOS3vbs = 1;
        public const int MOS3vgs = 2;
        public const int MOS3vds = 3;
        public const int MOS3capgs = 4;
        public const int MOS3qgs = 5;
        public const int MOS3cqgs = 6;
        public const int MOS3capgd = 7;
        public const int MOS3qgd = 8;
        public const int MOS3cqgd = 9;
        public const int MOS3capgb = 10;
        public const int MOS3qgb = 11;
        public const int MOS3cqgb = 12;
        public const int MOS3qbd = 13;
        public const int MOS3cqbd = 14;
        public const int MOS3qbs = 15;
        public const int MOS3cqbs = 16;

        /// <summary>
        /// Nodes
        /// </summary>
        protected int MOS3dNode, MOS3gNode, MOS3sNode, MOS3bNode;
        [SpiceName("dnodeprime"), SpiceInfo("Number of internal drain node")]
        public int MOS3dNodePrime { get; internal set; }
        [SpiceName("snodeprime"), SpiceInfo("Number of internal source node")]
        public int MOS3sNodePrime { get; internal set; }

        /// <summary>
        /// Setup the behaviour
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        public override void Setup(CircuitObject component, Circuit ckt)
        {
            var mos3 = component as Components.MOS3;

            // Get behaviors
            temp = GetBehavior<TemperatureBehavior>(component);
            modeltemp = GetBehavior<ModelTemperatureBehavior>(mos3.Model);

            // Get nodes
            MOS3dNode = mos3.MOS3dNode;
            MOS3gNode = mos3.MOS3gNode;
            MOS3sNode = mos3.MOS3sNode;
            MOS3bNode = mos3.MOS3bNode;

            // Add a series drain node if necessary
            if (modeltemp.MOS3drainResistance != 0 || (modeltemp.MOS3sheetResistance != 0 && temp.MOS3drainSquares != 0))
                MOS3dNodePrime = CreateNode(ckt, component.Name.Grow("#drain")).Index;
            else
                MOS3dNodePrime = MOS3dNode;

            // Add a series source node if necessary
            if (modeltemp.MOS3sourceResistance != 0 || (modeltemp.MOS3sheetResistance != 0 && temp.MOS3sourceSquares != 0))
                MOS3sNodePrime = CreateNode(ckt, component.Name.Grow("#source")).Index;
            else
                MOS3sNodePrime = MOS3sNode;

            // Get matrix elements
            var matrix = ckt.State.Matrix;
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

            // Allocate states
            MOS3states = ckt.State.GetState(17);

            MOS3vdsat = 0;
            MOS3von = 0;
            MOS3mode = 1;
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
        /// Execute behaviour
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

            vt = Circuit.CONSTKoverQ * temp.MOS3temp;
            Check = 1;

            /* DETAILPROF */

            /* first, we compute a few useful values - these could be
			* pre - computed, but for historical reasons are still done
			* here.  They may be moved at the expense of instance size
			*/

            EffectiveLength = temp.MOS3l - 2 * modeltemp.MOS3latDiff;
            if ((temp.MOS3tSatCurDens == 0) || (temp.MOS3drainArea.Value == 0) || (temp.MOS3sourceArea.Value == 0))
            {
                DrainSatCur = temp.MOS3tSatCur;
                SourceSatCur = temp.MOS3tSatCur;
            }
            else
            {
                DrainSatCur = temp.MOS3tSatCurDens * temp.MOS3drainArea;
                SourceSatCur = temp.MOS3tSatCurDens * temp.MOS3sourceArea;
            }
            GateSourceOverlapCap = modeltemp.MOS3gateSourceOverlapCapFactor * temp.MOS3w;
            GateDrainOverlapCap = modeltemp.MOS3gateDrainOverlapCapFactor * temp.MOS3w;
            GateBulkOverlapCap = modeltemp.MOS3gateBulkOverlapCapFactor * EffectiveLength;
            Beta = temp.MOS3tTransconductance * temp.MOS3w / EffectiveLength;
            OxideCap = modeltemp.MOS3oxideCapFactor * EffectiveLength * temp.MOS3w;

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

            if ((state.Init == CircuitState.InitFlags.InitFloat || state.UseSmallSignal || (state.Init == CircuitState.InitFlags.InitTransient)) ||
                ((state.Init == CircuitState.InitFlags.InitFix) && (!MOS3off)))
            {
                // General iteration
                vbs = modeltemp.MOS3type * (rstate.Solution[MOS3bNode] - rstate.Solution[MOS3sNodePrime]);
                vgs = modeltemp.MOS3type * (rstate.Solution[MOS3gNode] - rstate.Solution[MOS3sNodePrime]);
                vds = modeltemp.MOS3type * (rstate.Solution[MOS3dNodePrime] - rstate.Solution[MOS3sNodePrime]);

                /* now some common crunching for some more useful quantities */
                /* DETAILPROF */

                vbd = vbs - vds;
                vgd = vgs - vds;
                vgdo = state.States[0][MOS3states + MOS3vgs] - state.States[0][MOS3states + MOS3vds];
                delvbs = vbs - state.States[0][MOS3states + MOS3vbs];
                delvbd = vbd - state.States[0][MOS3states + MOS3vbd];
                delvgs = vgs - state.States[0][MOS3states + MOS3vgs];
                delvds = vds - state.States[0][MOS3states + MOS3vds];
                delvgd = vgd - vgdo;

                /* these are needed for convergence testing */

                if (MOS3mode >= 0)
                {
                    cdhat = MOS3cd - MOS3gbd * delvbd + MOS3gmbs * delvbs + MOS3gm * delvgs + MOS3gds * delvds;
                }
                else
                {
                    cdhat = MOS3cd - (MOS3gbd - MOS3gmbs) * delvbd - MOS3gm * delvgd + MOS3gds * delvds;
                }
                cbhat = MOS3cbs + MOS3cbd + MOS3gbd * delvbd + MOS3gbs * delvbs;

                /* DETAILPROF */
                /* NOBYPASS */

                /* DETAILPROF */
                /* ok - bypass is out, do it the hard way */

                von = modeltemp.MOS3type * MOS3von;

                /* 
				* limiting
				* we want to keep device voltages from changing
				* so fast that the exponentials churn out overflows
				* and similar rudeness
				*/

                if (state.States[0][MOS3states + MOS3vds] >= 0)
                {
                    vgs = Transistor.DEVfetlim(vgs, state.States[0][MOS3states + MOS3vgs], von);
                    vds = vgs - vgd;
                    vds = Transistor.DEVlimvds(vds, state.States[0][MOS3states + MOS3vds]);
                    vgd = vgs - vds;
                }
                else
                {
                    vgd = Transistor.DEVfetlim(vgd, vgdo, von);
                    vds = vgs - vgd;
                    vds = -Transistor.DEVlimvds(-vds, -(state.States[0][MOS3states + MOS3vds]));
                    vgs = vgd + vds;
                }
                if (vds >= 0)
                {
                    vbs = Transistor.DEVpnjlim(vbs, state.States[0][MOS3states + MOS3vbs], vt, temp.MOS3sourceVcrit, ref Check);
                    vbd = vbs - vds;
                }
                else
                {
                    vbd = Transistor.DEVpnjlim(vbd, state.States[0][MOS3states + MOS3vbd], vt, temp.MOS3drainVcrit, ref Check);
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

                if ((state.Init == CircuitState.InitFlags.InitJct) && !MOS3off)
                {
                    vds = modeltemp.MOS3type * MOS3icVDS;
                    vgs = modeltemp.MOS3type * MOS3icVGS;
                    vbs = modeltemp.MOS3type * MOS3icVBS;
                    if ((vds == 0) && (vgs == 0) && (vbs == 0) && ((method != null || state.UseDC ||
                        state.Domain == CircuitState.DomainTypes.None) || (!state.UseIC)))
                    {
                        vbs = -1;
                        vgs = modeltemp.MOS3type * temp.MOS3tVto;
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
                eta = modeltemp.MOS3eta * 8.15e-22 / (modeltemp.MOS3oxideCapFactor * EffectiveLength * EffectiveLength * EffectiveLength);
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
                if ((modeltemp.MOS3junctionDepth != 0.0) && (modeltemp.MOS3coeffDepLayWidth != 0.0))
                {
                    wps = modeltemp.MOS3coeffDepLayWidth * sqphbs;
                    oneoverxj = 1.0 / modeltemp.MOS3junctionDepth;
                    xjonxl = modeltemp.MOS3junctionDepth * oneoverxl;
                    djonxj = modeltemp.MOS3latDiff * oneoverxj;
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
                gammas = modeltemp.MOS3gamma * fshort;
                fbodys = 0.5 * gammas / (sqphbs + sqphbs);
                fbody = fbodys + modeltemp.MOS3narrowFactor / temp.MOS3w;
                onfbdy = 1.0 / (1.0 + fbody);
                dfbdvb = -fbodys * dsqdvb / sqphbs + fbodys * dfsdvb / fshort;
                qbonco = gammas * sqphbs + modeltemp.MOS3narrowFactor * phibs / temp.MOS3w;
                dqbdvb = gammas * dsqdvb + modeltemp.MOS3gamma * dfsdvb * sqphbs - modeltemp.MOS3narrowFactor / temp.MOS3w;
                /* 
				 * .....static feedback effect
				 */
                vbix = temp.MOS3tVbi * modeltemp.MOS3type - eta * (MOS3mode * vds);
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
                if (modeltemp.MOS3fastSurfaceStateDensity != 0.0)
                {
                    csonco = Circuit.CHARGE * modeltemp.MOS3fastSurfaceStateDensity * 1e4 /* (cm *  * 2 / m *  * 2) */  * EffectiveLength * temp.MOS3w /
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
                onfg = 1.0 + modeltemp.MOS3theta * (vgsx - vth);
                fgate = 1.0 / onfg;
                us = temp.MOS3tSurfMob * 1e-4 /*(m**2/cm**2)*/ * fgate;
                dfgdvg = -modeltemp.MOS3theta * fgate * fgate;
                dfgdvd = -dfgdvg * dvtdvd;
                dfgdvb = -dfgdvg * dvtdvb;
                /* 
				 * .....saturation voltage
				 */
                vdsat = (vgsx - vth) * onfbdy;
                if (modeltemp.MOS3maxDriftVel <= 0.0)
                {
                    dvsdvg = onfbdy;
                    dvsdvd = -dvsdvg * dvtdvd;
                    dvsdvb = -dvsdvg * dvtdvb - vdsat * dfbdvb * onfbdy;
                }
                else
                {
                    vdsc = EffectiveLength * modeltemp.MOS3maxDriftVel / us;
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
                if (modeltemp.MOS3maxDriftVel != 0.0)
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
                if (modeltemp.MOS3maxDriftVel <= 0.0) goto line510;
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

                emax = modeltemp.MOS3kappa * cdsat * oneoverxl / gdsat;
                emoncd = emax / cdsat;
                emongd = emax / gdsat;
                demdvg = emoncd * MOS3gm - emongd * dgdvg;
                demdvd = emoncd * MOS3gds - emongd * dgdvd;
                demdvb = emoncd * MOS3gmbs - emongd * dgdvb;

                arga = 0.5 * emax * modeltemp.MOS3alpha;
                argc = modeltemp.MOS3kappa * modeltemp.MOS3alpha;
                argb = Math.Sqrt(arga * arga + argc * ((MOS3mode * vds) - vdsat));
                delxl = argb - arga;
                dldvd = argc / (argb + argb);
                dldem = 0.5 * (arga / argb - 1.0) * modeltemp.MOS3alpha;
                ddldvg = dldem * demdvg;
                ddldvd = dldem * demdvd - dldvd;
                ddldvb = dldem * demdvb;
                goto line520;
                line510:
                delxl = Math.Sqrt(modeltemp.MOS3kappa * ((MOS3mode * vds) - vdsat) * modeltemp.MOS3alpha);
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
                if ((modeltemp.MOS3fastSurfaceStateDensity != 0.0) && ((MOS3mode == 1 ? vgs : vgd) < von))
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

            MOS3von = modeltemp.MOS3type * von;
            MOS3vdsat = modeltemp.MOS3type * vdsat;
            /* line 490 */
            /* 
			* COMPUTE EQUIVALENT DRAIN CURRENT SOURCE
			*/
            MOS3cd = MOS3mode * cdrain - MOS3cbd;

            if (state.Domain == CircuitState.DomainTypes.Time || state.UseSmallSignal)
            {
                /* 
				* now we do the hard part of the bulk - drain and bulk - source
				* diode - we evaluate the non - linear capacitance and
				* charge
				* 
				* the basic equations are not hard, but the implementation
				* is somewhat long in an attempt to avoid log / exponential
				* evaluations
				*/
                /* 
				* charge storage elements
				* 
				* .. bulk - drain and bulk - source depletion capacitances
				*/
                /* CAPBYPASS */
                {
                    /* can't bypass the diode capacitance calculations */
                    /* CAPZEROBYPASS */
                    if (vbs < temp.MOS3tDepCap)
                    {
                        double arg = 1 - vbs / temp.MOS3tBulkPot, sarg;
                        /* 
						* the following block looks somewhat long and messy, 
						* but since most users use the default grading
						* coefficients of .5, and sqrt is MUCH faster than an
						* Math.Exp(Math.Log()) we use this special case code to buy time.
						* (as much as 10% of total job time!)
						*/
                        if (modeltemp.MOS3bulkJctBotGradingCoeff.Value == modeltemp.MOS3bulkJctSideGradingCoeff)
                        {
                            if (modeltemp.MOS3bulkJctBotGradingCoeff.Value == .5)
                            {
                                sarg = sargsw = 1 / Math.Sqrt(arg);
                            }
                            else
                            {
                                sarg = sargsw = Math.Exp(-modeltemp.MOS3bulkJctBotGradingCoeff * Math.Log(arg));
                            }
                        }
                        else
                        {
                            if (modeltemp.MOS3bulkJctBotGradingCoeff.Value == .5)
                            {
                                sarg = 1 / Math.Sqrt(arg);
                            }
                            else
                            {
                                /* NOSQRT */
                                sarg = Math.Exp(-modeltemp.MOS3bulkJctBotGradingCoeff * Math.Log(arg));
                            }
                            if (modeltemp.MOS3bulkJctSideGradingCoeff.Value == .5)
                            {
                                sargsw = 1 / Math.Sqrt(arg);
                            }
                            else
                            {
                                /* NOSQRT */
                                sargsw = Math.Exp(-modeltemp.MOS3bulkJctSideGradingCoeff * Math.Log(arg));
                            }
                        }
                        /* NOSQRT */
                        state.States[0][MOS3states + MOS3qbs] = temp.MOS3tBulkPot * (temp.MOS3Cbs * (1 - arg * sarg) / (1 - modeltemp.MOS3bulkJctBotGradingCoeff) +
                            temp.MOS3Cbssw * (1 - arg * sargsw) / (1 - modeltemp.MOS3bulkJctSideGradingCoeff));
                        MOS3capbs = temp.MOS3Cbs * sarg + temp.MOS3Cbssw * sargsw;
                    }
                    else
                    {
                        state.States[0][MOS3states + MOS3qbs] = temp.MOS3f4s + vbs * (temp.MOS3f2s + vbs * (temp.MOS3f3s / 2));
                        MOS3capbs = temp.MOS3f2s + temp.MOS3f3s * vbs;
                    }
                    /* CAPZEROBYPASS */
                }
                /* CAPBYPASS */
                /* can't bypass the diode capacitance calculations */
                {
                    /* CAPZEROBYPASS */
                    if (vbd < temp.MOS3tDepCap)
                    {
                        double arg = 1 - vbd / temp.MOS3tBulkPot, sarg;
                        /* 
						* the following block looks somewhat long and messy, 
						* but since most users use the default grading
						* coefficients of .5, and sqrt is MUCH faster than an
						* Math.Exp(Math.Log()) we use this special case code to buy time.
						* (as much as 10% of total job time!)
						*/
                        if (modeltemp.MOS3bulkJctBotGradingCoeff.Value == .5 && modeltemp.MOS3bulkJctSideGradingCoeff.Value == .5)
                        {
                            sarg = sargsw = 1 / Math.Sqrt(arg);
                        }
                        else
                        {
                            if (modeltemp.MOS3bulkJctBotGradingCoeff.Value == .5)
                            {
                                sarg = 1 / Math.Sqrt(arg);
                            }
                            else
                            {
                                /* NOSQRT */
                                sarg = Math.Exp(-modeltemp.MOS3bulkJctBotGradingCoeff * Math.Log(arg));
                            }
                            if (modeltemp.MOS3bulkJctSideGradingCoeff.Value == .5)
                            {
                                sargsw = 1 / Math.Sqrt(arg);
                            }
                            else
                            {
                                /* NOSQRT */
                                sargsw = Math.Exp(-modeltemp.MOS3bulkJctSideGradingCoeff * Math.Log(arg));
                            }
                        }
                        /* NOSQRT */
                        state.States[0][MOS3states + MOS3qbd] = temp.MOS3tBulkPot * (temp.MOS3Cbd * (1 - arg * sarg) / (1 - modeltemp.MOS3bulkJctBotGradingCoeff) +
                            temp.MOS3Cbdsw * (1 - arg * sargsw) / (1 - modeltemp.MOS3bulkJctSideGradingCoeff));
                        MOS3capbd = temp.MOS3Cbd * sarg + temp.MOS3Cbdsw * sargsw;
                    }
                    else
                    {
                        state.States[0][MOS3states + MOS3qbd] = temp.MOS3f4d + vbd * (temp.MOS3f2d + vbd * temp.MOS3f3d / 2);
                        MOS3capbd = temp.MOS3f2d + vbd * temp.MOS3f3d;
                    }
                    /* CAPZEROBYPASS */
                }
                /* DETAILPROF */

                if (method != null)
                {
                    /* (above only excludes tranop, since we're only at this
					* point if tran or tranop)
					*/

                    /* 
					* calculate equivalent conductances and currents for
					* depletion capacitors
					*/

                    /* integrate the capacitors and save results */
                    var result = method.Integrate(state, MOS3states + MOS3qbd, MOS3capbd);
                    MOS3gbd += result.Geq;
                    MOS3cbd += state.States[0][MOS3states + MOS3cqbd];
                    MOS3cd -= state.States[0][MOS3states + MOS3cqbd];
                    result = method.Integrate(state, MOS3states + MOS3qbs, MOS3capbs);
                    MOS3gbs += result.Geq;
                    MOS3cbs += state.States[0][MOS3states + MOS3cqbs];
                }
            }
            /* DETAILPROF */

            /* 
			 * check convergence
			 */
            if (!MOS3off || (!(state.Init == CircuitState.InitFlags.InitFix || state.UseSmallSignal)))
            {
                if (Check == 1)
                    state.IsCon = false;
            }

            /* DETAILPROF */

            /* save things away for next time */

            state.States[0][MOS3states + MOS3vbs] = vbs;
            state.States[0][MOS3states + MOS3vbd] = vbd;
            state.States[0][MOS3states + MOS3vgs] = vgs;
            state.States[0][MOS3states + MOS3vds] = vds;

            /* DETAILPROF */

            /* 
			 * meyer's capacitor model
			 */
            if (state.Domain == CircuitState.DomainTypes.Time || state.UseSmallSignal)
            {
                /* 
				 * calculate meyer's capacitors
				 */
                /* 
				 * new cmeyer - this just evaluates at the current time, 
				 * expects you to remember values from previous time
				 * returns 1 / 2 of non - constant portion of capacitance
				 * you must add in the other half from previous time
				 * and the constant part
				 */
                double icapgs, icapgd, icapgb;
                if (MOS3mode > 0)
                {
                    Transistor.DEVqmeyer(vgs, vgd, vgb, von, vdsat,
                        out icapgs, out icapgd, out icapgb, temp.MOS3tPhi, OxideCap);
                }
                else
                {
                    Transistor.DEVqmeyer(vgd, vgs, vgb, von, vdsat,
                        out icapgd, out icapgs, out icapgb, temp.MOS3tPhi, OxideCap);
                }
                state.States[0][MOS3states + MOS3capgs] = icapgs;
                state.States[0][MOS3states + MOS3capgd] = icapgd;
                state.States[0][MOS3states + MOS3capgb] = icapgb;
                vgs1 = state.States[1][MOS3states + MOS3vgs];
                vgd1 = vgs1 - state.States[1][MOS3states + MOS3vds];
                vgb1 = vgs1 - state.States[1][MOS3states + MOS3vbs];
                if (state.Domain == CircuitState.DomainTypes.Time && state.UseDC)
                {
                    capgs = 2 * state.States[0][MOS3states + MOS3capgs] + GateSourceOverlapCap;
                    capgd = 2 * state.States[0][MOS3states + MOS3capgd] + GateDrainOverlapCap;
                    capgb = 2 * state.States[0][MOS3states + MOS3capgb] + GateBulkOverlapCap;
                }
                else
                {
                    capgs = (state.States[0][MOS3states + MOS3capgs] + state.States[1][MOS3states + MOS3capgs] + GateSourceOverlapCap);
                    capgd = (state.States[0][MOS3states + MOS3capgd] + state.States[1][MOS3states + MOS3capgd] + GateDrainOverlapCap);
                    capgb = (state.States[0][MOS3states + MOS3capgb] + state.States[1][MOS3states + MOS3capgb] + GateBulkOverlapCap);
                }

                /* DETAILPROF */
                /* 
				 * store small - signal parameters (for meyer's model)
				 * all parameters already stored, so done...
				 */

                /* PREDICTOR */
                if (method != null)
                {
                    state.States[0][MOS3states + MOS3qgs] = (vgs - vgs1) * capgs + state.States[1][MOS3states + MOS3qgs];
                    state.States[0][MOS3states + MOS3qgd] = (vgd - vgd1) * capgd + state.States[1][MOS3states + MOS3qgd];
                    state.States[0][MOS3states + MOS3qgb] = (vgb - vgb1) * capgb + state.States[1][MOS3states + MOS3qgb];
                }
                else
                {
                    /* TRANOP only */
                    state.States[0][MOS3states + MOS3qgs] = vgs * capgs;
                    state.States[0][MOS3states + MOS3qgd] = vgd * capgd;
                    state.States[0][MOS3states + MOS3qgb] = vgb * capgb;
                }
                /* PREDICTOR */
            }

            /* DETAILPROF */

            if (method == null || state.Init == CircuitState.InitFlags.InitTransient)
            {
                /* 
				 * initialize to zero charge conductances 
				 * and current
				 */
                gcgs = 0;
                ceqgs = 0;
                gcgd = 0;
                ceqgd = 0;
                gcgb = 0;
                ceqgb = 0;
            }
            else
            {
                if (capgs == 0)
                    state.States[0][MOS3states + MOS3cqgs] = 0;
                if (capgd == 0)
                    state.States[0][MOS3states + MOS3cqgd] = 0;
                if (capgb == 0)
                    state.States[0][MOS3states + MOS3cqgb] = 0;
                /* 
				 * calculate equivalent conductances and currents for
				 * meyer"s capacitors
				 */
                method.Integrate(state, out gcgs, out ceqgs, MOS3states + MOS3qgs, capgs);
                method.Integrate(state, out gcgd, out ceqgd, MOS3states + MOS3qgd, capgd);
                method.Integrate(state, out gcgb, out ceqgb, MOS3states + MOS3qgb, capgb);
                ceqgs = ceqgs - gcgs * vgs + method.Slope * state.States[0][MOS3states + MOS3qgs];
                ceqgd = ceqgd - gcgd * vgd + method.Slope * state.States[0][MOS3states + MOS3qgd];
                ceqgb = ceqgb - gcgb * vgb + method.Slope * state.States[0][MOS3states + MOS3qgb];
            }
            /* 
			 * store charge storage info for meyer's cap in lx table
			 */

            /* DETAILPROF */
            /* 
			 * load current vector
			 */
            ceqbs = modeltemp.MOS3type * (MOS3cbs - (MOS3gbs - state.Gmin) * vbs);
            ceqbd = modeltemp.MOS3type * (MOS3cbd - (MOS3gbd - state.Gmin) * vbd);
            if (MOS3mode >= 0)
            {
                xnrm = 1;
                xrev = 0;
                cdreq = modeltemp.MOS3type * (cdrain - MOS3gds * vds - MOS3gm * vgs - MOS3gmbs * vbs);
            }
            else
            {
                xnrm = 0;
                xrev = 1;
                cdreq = -(modeltemp.MOS3type) * (cdrain - MOS3gds * (-vds) - MOS3gm * vgd - MOS3gmbs * vbd);
            }

            rstate.Rhs[MOS3gNode] -= (modeltemp.MOS3type * (ceqgs + ceqgb + ceqgd));
            rstate.Rhs[MOS3bNode] -= (ceqbs + ceqbd - modeltemp.MOS3type * ceqgb);
            rstate.Rhs[MOS3dNodePrime] += (ceqbd - cdreq + modeltemp.MOS3type * ceqgd);
            rstate.Rhs[MOS3sNodePrime] += cdreq + ceqbs + modeltemp.MOS3type * ceqgs;

            /* 
			 * load y matrix
			 */
            MOS3DdPtr.Add(temp.MOS3drainConductance);
            MOS3GgPtr.Add(gcgd + gcgs + gcgb);
            MOS3SsPtr.Add(temp.MOS3sourceConductance);
            MOS3BbPtr.Add(MOS3gbd + MOS3gbs + gcgb);
            MOS3DPdpPtr.Add(temp.MOS3drainConductance + MOS3gds + MOS3gbd + xrev * (MOS3gm + MOS3gmbs) + gcgd);
            MOS3SPspPtr.Add(temp.MOS3sourceConductance + MOS3gds + MOS3gbs + xnrm * (MOS3gm + MOS3gmbs) + gcgs);
            MOS3DdpPtr.Add(-temp.MOS3drainConductance);
            MOS3GbPtr.Sub(gcgb);
            MOS3GdpPtr.Sub(gcgd);
            MOS3GspPtr.Sub(gcgs);
            MOS3SspPtr.Add(-temp.MOS3sourceConductance);
            MOS3BgPtr.Sub(gcgb);
            MOS3BdpPtr.Sub(MOS3gbd);
            MOS3BspPtr.Sub(MOS3gbs);
            MOS3DPdPtr.Add(-temp.MOS3drainConductance);
            MOS3DPgPtr.Add((xnrm - xrev) * MOS3gm - gcgd);
            MOS3DPbPtr.Add(-MOS3gbd + (xnrm - xrev) * MOS3gmbs);
            MOS3DPspPtr.Add(-MOS3gds - xnrm * (MOS3gm + MOS3gmbs));
            MOS3SPgPtr.Add(-(xnrm - xrev) * MOS3gm - gcgs);
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
            var config = ckt.Simulation.CurrentConfig;
            var state = ckt.State;

            double vbs, vgs, vds, vbd, vgd, vgdo, delvbs, delvbd, delvgs, delvds, delvgd, cdhat, cbhat;

            vbs = modeltemp.MOS3type * (state.Solution[MOS3bNode] - state.Solution[MOS3sNodePrime]);
            vgs = modeltemp.MOS3type * (state.Solution[MOS3gNode] - state.Solution[MOS3sNodePrime]);
            vds = modeltemp.MOS3type * (state.Solution[MOS3dNodePrime] - state.Solution[MOS3sNodePrime]);
            vbd = vbs - vds;
            vgd = vgs - vds;
            vgdo = state.States[0][MOS3states + MOS3vgs] - state.States[0][MOS3states + MOS3vds];
            delvbs = vbs - state.States[0][MOS3states + MOS3vbs];
            delvbd = vbd - state.States[0][MOS3states + MOS3vbd];
            delvgs = vgs - state.States[0][MOS3states + MOS3vgs];
            delvds = vds - state.States[0][MOS3states + MOS3vds];
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
            double tol = config.RelTol * Math.Max(Math.Abs(cdhat), Math.Abs(MOS3cd)) + config.AbsTol;
            if (Math.Abs(cdhat - MOS3cd) >= tol)
            {
                state.IsCon = false;
                return false;
            }
            else
            {
                tol = config.RelTol * Math.Max(Math.Abs(cbhat), Math.Abs(MOS3cbs + MOS3cbd)) + config.AbsTol;
                if (Math.Abs(cbhat - (MOS3cbs + MOS3cbd)) > tol)
                {
                    state.IsCon = false;
                    return false;
                }
            }
            return true;
        }
    }
}
