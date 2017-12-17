using System;
using SpiceSharp.Circuits;
using SpiceSharp.Parameters;
using SpiceSharp.Sparse;
using SpiceSharp.Diagnostics;
using SpiceSharp.Components.Transistors;

namespace SpiceSharp.Behaviors.MOS1
{
    /// <summary>
    /// General behavior for a <see cref="MOS1"/>
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
        public bool MOS1off { get; set; }
        [SpiceName("icvbs"), SpiceInfo("Initial B-S voltage")]
        public Parameter MOS1icVBS { get; } = new Parameter();
        [SpiceName("icvds"), SpiceInfo("Initial D-S voltage")]
        public Parameter MOS1icVDS { get; } = new Parameter();
        [SpiceName("icvgs"), SpiceInfo("Initial G-S voltage")]
        public Parameter MOS1icVGS { get; } = new Parameter();
        [SpiceName("dnode"), SpiceInfo("Number of the drain node ")]
        public int MOS1dNode { get; protected set; }
        [SpiceName("gnode"), SpiceInfo("Number of the gate node ")]
        public int MOS1gNode { get; protected set; }
        [SpiceName("snode"), SpiceInfo("Number of the source node ")]
        public int MOS1sNode { get; protected set; }
        [SpiceName("bnode"), SpiceInfo("Number of the node ")]
        public int MOS1bNode { get; protected set; }
        [SpiceName("dnodeprime"), SpiceInfo("Number of int. drain node")]
        public int MOS1dNodePrime { get; protected set; }
        [SpiceName("snodeprime"), SpiceInfo("Number of int. source node ")]
        public int MOS1sNodePrime { get; protected set; }

        [SpiceName("von"), SpiceInfo(" ")]
        public double MOS1von { get; protected set; }
        [SpiceName("vdsat"), SpiceInfo("Saturation drain voltage")]
        public double MOS1vdsat { get; protected set; }

        [SpiceName("id"), SpiceInfo("Drain current")]
        public double MOS1cd { get; protected set; }
        [SpiceName("ibs"), SpiceInfo("B-S junction current")]
        public double MOS1cbs { get; protected set; }
        [SpiceName("ibd"), SpiceInfo("B-D junction current")]
        public double MOS1cbd { get; protected set; }
        [SpiceName("gmb"), SpiceName("gmbs"), SpiceInfo("Bulk-Source transconductance")]
        public double MOS1gmbs { get; protected set; }
        [SpiceName("gm"), SpiceInfo("Transconductance")]
        public double MOS1gm { get; protected set; }
        [SpiceName("gds"), SpiceInfo("Drain-Source conductance")]
        public double MOS1gds { get; protected set; }
        [SpiceName("gbd"), SpiceInfo("Bulk-Drain conductance")]
        public double MOS1gbd { get; protected set; }
        [SpiceName("gbs"), SpiceInfo("Bulk-Source conductance")]
        public double MOS1gbs { get; protected set; }
        [SpiceName("cbd"), SpiceInfo("Bulk-Drain capacitance")]
        public double MOS1capbd { get; protected set; }
        [SpiceName("cbs"), SpiceInfo("Bulk-Source capacitance")]
        public double MOS1capbs { get; protected set; }


        /// <summary>
        /// Methods
        /// </summary>
        [SpiceName("ic"), SpiceInfo("Vector of D-S, G-S, B-S voltages")]
        public void SetIC(double[] value)
        {
            switch (value.Length)
            {
                case 3: MOS1icVBS.Set(value[2]); goto case 2;
                case 2: MOS1icVGS.Set(value[1]); goto case 1;
                case 1: MOS1icVDS.Set(value[0]); break;
                default:
                    throw new CircuitException("Bad parameter");
            }
        }

        [SpiceName("vbd"), SpiceInfo("Bulk-Drain voltage")]
        public double GetVBD(Circuit ckt) => ckt.State.States[0][MOS1states + MOS1vbd];
        [SpiceName("vbs"), SpiceInfo("Bulk-Source voltage")]
        public double GetVBS(Circuit ckt) => ckt.State.States[0][MOS1states + MOS1vbs];
        [SpiceName("vgs"), SpiceInfo("Gate-Source voltage")]
        public double GetVGS(Circuit ckt) => ckt.State.States[0][MOS1states + MOS1vgs];
        [SpiceName("vds"), SpiceInfo("Drain-Source voltage")]
        public double GetVDS(Circuit ckt) => ckt.State.States[0][MOS1states + MOS1vds];
        [SpiceName("cgs"), SpiceInfo("Gate-Source capacitance")]
        public double GetCAPGS(Circuit ckt) => ckt.State.States[0][MOS1states + MOS1capgs];
        [SpiceName("qgs"), SpiceInfo("Gate-Source charge storage")]
        public double GetQGS(Circuit ckt) => ckt.State.States[0][MOS1states + MOS1qgs];
        [SpiceName("cqgs"), SpiceInfo("Capacitance due to gate-source charge storage")]
        public double GetCQGS(Circuit ckt) => ckt.State.States[0][MOS1states + MOS1cqgs];
        [SpiceName("cgd"), SpiceInfo("Gate-Drain capacitance")]
        public double GetCAPGD(Circuit ckt) => ckt.State.States[0][MOS1states + MOS1capgd];
        [SpiceName("qgd"), SpiceInfo("Gate-Drain charge storage")]
        public double GetQGD(Circuit ckt) => ckt.State.States[0][MOS1states + MOS1qgd];
        [SpiceName("cqgd"), SpiceInfo("Capacitance due to gate-drain charge storage")]
        public double GetCQGD(Circuit ckt) => ckt.State.States[0][MOS1states + MOS1cqgd];
        [SpiceName("cgb"), SpiceInfo("Gate-Bulk capacitance")]
        public double GetCAPGB(Circuit ckt) => ckt.State.States[0][MOS1states + MOS1capgb];
        [SpiceName("qgb"), SpiceInfo("Gate-Bulk charge storage")]
        public double GetQGB(Circuit ckt) => ckt.State.States[0][MOS1states + MOS1qgb];
        [SpiceName("cqgb"), SpiceInfo("Capacitance due to gate-bulk charge storage")]
        public double GetCQGB(Circuit ckt) => ckt.State.States[0][MOS1states + MOS1cqgb];
        [SpiceName("qbd"), SpiceInfo("Bulk-Drain charge storage")]
        public double GetQBD(Circuit ckt) => ckt.State.States[0][MOS1states + MOS1qbd];
        [SpiceName("cqbd"), SpiceInfo("Capacitance due to bulk-drain charge storage")]
        public double GetCQBD(Circuit ckt) => ckt.State.States[0][MOS1states + MOS1cqbd];
        [SpiceName("qbs"), SpiceInfo("Bulk-Source charge storage")]
        public double GetQBS(Circuit ckt) => ckt.State.States[0][MOS1states + MOS1qbs];
        [SpiceName("cqbs"), SpiceInfo("Capacitance due to bulk-source charge storage")]
        public double GetCQBS(Circuit ckt) => ckt.State.States[0][MOS1states + MOS1cqbs];
        [SpiceName("ib"), SpiceInfo("Bulk current")]
        public double GetCB(Circuit ckt) => MOS1cbd + MOS1cbs - ckt.State.States[0][MOS1states + MOS1cqgb];
        [SpiceName("ig"), SpiceInfo("Gate current ")]
        public double GetCG(Circuit ckt) => ckt.State.UseDC ? 0.0 : ckt.State.States[0][MOS1states + MOS1cqgb] + ckt.State.States[0][MOS1states + MOS1cqgd] +
            ckt.State.States[0][MOS1states + MOS1cqgs];
        [SpiceName("is"), SpiceInfo("Source current")]
        public double GetCS(Circuit ckt)
        {
            double value = -MOS1cd;
            value -= MOS1cbd + MOS1cbs - ckt.State.States[0][MOS1states + MOS1cqgb];
            if (ckt.Method != null && !(ckt.State.Domain == State.DomainTypes.Time && ckt.State.UseDC))
            {
                value -= ckt.State.States[0][MOS1states + MOS1cqgb] + ckt.State.States[0][MOS1states + MOS1cqgd] +
                    ckt.State.States[0][MOS1states + MOS1cqgs];
            }
            return value;
        }
        [SpiceName("p"), SpiceInfo("Instaneous power")]
        public double GetPOWER(Circuit ckt)
        {
            double temp;
            double value = MOS1cd * ckt.State.Solution[MOS1dNode];
            value += (MOS1cbd + MOS1cbs - ckt.State.States[0][MOS1states + MOS1cqgb]) * ckt.State.Solution[MOS1bNode];
            if (ckt.Method != null && !(ckt.State.Domain == State.DomainTypes.Time && ckt.State.UseDC))
            {
                value += (ckt.State.States[0][MOS1states + MOS1cqgb] + ckt.State.States[0][MOS1states + MOS1cqgd] +
                    ckt.State.States[0][MOS1states + MOS1cqgs]) * ckt.State.Solution[MOS1gNode];
            }
            temp = -MOS1cd;
            temp -= MOS1cbd + MOS1cbs;
            if (ckt.Method != null && !(ckt.State.Domain == State.DomainTypes.Time && ckt.State.UseDC))
            {
                temp -= ckt.State.States[0][MOS1states + MOS1cqgb] + ckt.State.States[0][MOS1states + MOS1cqgd] +
                    ckt.State.States[0][MOS1states + MOS1cqgs];
            }
            value += temp * ckt.State.Solution[MOS1sNode];
            return value;
        }

        /// <summary>
        /// Extra variables
        /// </summary>
        public int MOS1states { get; protected set; }
        public double MOS1mode { get; internal set; }

        /// <summary>
        /// Matrix elements
        /// </summary>
        protected MatrixElement MOS1DdPtr { get; private set; }
        protected MatrixElement MOS1GgPtr { get; private set; }
        protected MatrixElement MOS1SsPtr { get; private set; }
        protected MatrixElement MOS1BbPtr { get; private set; }
        protected MatrixElement MOS1DPdpPtr { get; private set; }
        protected MatrixElement MOS1SPspPtr { get; private set; }
        protected MatrixElement MOS1DdpPtr { get; private set; }
        protected MatrixElement MOS1GbPtr { get; private set; }
        protected MatrixElement MOS1GdpPtr { get; private set; }
        protected MatrixElement MOS1GspPtr { get; private set; }
        protected MatrixElement MOS1SspPtr { get; private set; }
        protected MatrixElement MOS1BdpPtr { get; private set; }
        protected MatrixElement MOS1BspPtr { get; private set; }
        protected MatrixElement MOS1DPspPtr { get; private set; }
        protected MatrixElement MOS1DPdPtr { get; private set; }
        protected MatrixElement MOS1BgPtr { get; private set; }
        protected MatrixElement MOS1DPgPtr { get; private set; }
        protected MatrixElement MOS1SPgPtr { get; private set; }
        protected MatrixElement MOS1SPsPtr { get; private set; }
        protected MatrixElement MOS1DPbPtr { get; private set; }
        protected MatrixElement MOS1SPbPtr { get; private set; }
        protected MatrixElement MOS1SPdpPtr { get; private set; }

        /// <summary>
        /// Constants
        /// </summary>
        public const int MOS1vbd = 0;
        public const int MOS1vbs = 1;
        public const int MOS1vgs = 2;
        public const int MOS1vds = 3;
        public const int MOS1capgs = 4;
        public const int MOS1qgs = 5;
        public const int MOS1cqgs = 6;
        public const int MOS1capgd = 7;
        public const int MOS1qgd = 8;
        public const int MOS1cqgd = 9;
        public const int MOS1capgb = 10;
        public const int MOS1qgb = 11;
        public const int MOS1cqgb = 12;
        public const int MOS1qbd = 13;
        public const int MOS1cqbd = 14;
        public const int MOS1qbs = 15;
        public const int MOS1cqbs = 16;

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        public override void Setup(Entity component, Circuit ckt)
        {
            var mos1 = component as Components.MOS1;

            // Get behaviors
            temp = GetBehavior<TemperatureBehavior>(component);
            modeltemp = GetBehavior<ModelTemperatureBehavior>(mos1.Model);

            // Nodes
            MOS1dNode = mos1.MOS1dNode;
            MOS1gNode = mos1.MOS1gNode;
            MOS1sNode = mos1.MOS1sNode;
            MOS1bNode = mos1.MOS1bNode;

            // Add series drain node if necessary
            if (modeltemp.MOS1drainResistance != 0 || (modeltemp.MOS1sheetResistance != 0 && temp.MOS1drainSquares != 0))
                MOS1dNodePrime = CreateNode(ckt, mos1.Name.Grow("#drain")).Index;
            else
                MOS1dNodePrime = MOS1dNode;

            // Add series source node if necessary
            if (modeltemp.MOS1sourceResistance != 0 || (modeltemp.MOS1sheetResistance != 0 && temp.MOS1sourceSquares != 0))
                MOS1sNodePrime = CreateNode(ckt, mos1.Name.Grow("#source")).Index;
            else
                MOS1sNodePrime = MOS1sNode;

            // Get matrix elements
            var matrix = ckt.State.Matrix;
            MOS1DdPtr = matrix.GetElement(MOS1dNode, MOS1dNode);
            MOS1GgPtr = matrix.GetElement(MOS1gNode, MOS1gNode);
            MOS1SsPtr = matrix.GetElement(MOS1sNode, MOS1sNode);
            MOS1BbPtr = matrix.GetElement(MOS1bNode, MOS1bNode);
            MOS1DPdpPtr = matrix.GetElement(MOS1dNodePrime, MOS1dNodePrime);
            MOS1SPspPtr = matrix.GetElement(MOS1sNodePrime, MOS1sNodePrime);
            MOS1DdpPtr = matrix.GetElement(MOS1dNode, MOS1dNodePrime);
            MOS1GbPtr = matrix.GetElement(MOS1gNode, MOS1bNode);
            MOS1GdpPtr = matrix.GetElement(MOS1gNode, MOS1dNodePrime);
            MOS1GspPtr = matrix.GetElement(MOS1gNode, MOS1sNodePrime);
            MOS1SspPtr = matrix.GetElement(MOS1sNode, MOS1sNodePrime);
            MOS1BdpPtr = matrix.GetElement(MOS1bNode, MOS1dNodePrime);
            MOS1BspPtr = matrix.GetElement(MOS1bNode, MOS1sNodePrime);
            MOS1DPspPtr = matrix.GetElement(MOS1dNodePrime, MOS1sNodePrime);
            MOS1DPdPtr = matrix.GetElement(MOS1dNodePrime, MOS1dNode);
            MOS1BgPtr = matrix.GetElement(MOS1bNode, MOS1gNode);
            MOS1DPgPtr = matrix.GetElement(MOS1dNodePrime, MOS1gNode);
            MOS1SPgPtr = matrix.GetElement(MOS1sNodePrime, MOS1gNode);
            MOS1SPsPtr = matrix.GetElement(MOS1sNodePrime, MOS1sNode);
            MOS1DPbPtr = matrix.GetElement(MOS1dNodePrime, MOS1bNode);
            MOS1SPbPtr = matrix.GetElement(MOS1sNodePrime, MOS1bNode);
            MOS1SPdpPtr = matrix.GetElement(MOS1sNodePrime, MOS1dNodePrime);

            // Allocate states
            MOS1states = ckt.State.GetState(17);

            // Initialize
            MOS1vdsat = 0.0;
            MOS1von = 0.0;
        }

        /// <summary>
        /// Unsetup
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Unsetup()
        {
            // Remove references
            MOS1DdPtr = null;
            MOS1GgPtr = null;
            MOS1SsPtr = null;
            MOS1BbPtr = null;
            MOS1DPdpPtr = null;
            MOS1SPspPtr = null;
            MOS1DdpPtr = null;
            MOS1GbPtr = null;
            MOS1GdpPtr = null;
            MOS1GspPtr = null;
            MOS1SspPtr = null;
            MOS1BdpPtr = null;
            MOS1BspPtr = null;
            MOS1DPspPtr = null;
            MOS1DPdPtr = null;
            MOS1BgPtr = null;
            MOS1DPgPtr = null;
            MOS1SPgPtr = null;
            MOS1SPsPtr = null;
            MOS1DPbPtr = null;
            MOS1SPbPtr = null;
            MOS1SPdpPtr = null;
        }

        /// <summary>
        /// Execute behavior
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Load(Circuit ckt)
        {
            var state = ckt.State;
            var rstate = state;
            var method = ckt.Method;
            double vt, EffectiveLength, DrainSatCur, SourceSatCur, GateSourceOverlapCap, GateDrainOverlapCap, GateBulkOverlapCap, Beta,
                OxideCap, vgs, vds, vbs, vbd, vgb, vgd, vgdo, von, evbs, evbd,
                vdsat, cdrain, sargsw, vgs1, vgd1, vgb1, capgs = 0, capgd = 0, capgb = 0, gcgs, ceqgs, gcgd, ceqgd, gcgb, ceqgb, ceqbs,
                ceqbd, cdreq;
            int Check, xnrm, xrev;

            vt = Circuit.CONSTKoverQ * temp.MOS1temp;
            Check = 1;

            /* DETAILPROF */

            /* first, we compute a few useful values - these could be
			 * pre - computed, but for historical reasons are still done
			 * here.  They may be moved at the expense of instance size
			 */

            EffectiveLength = temp.MOS1l - 2 * modeltemp.MOS1latDiff;
            if ((temp.MOS1tSatCurDens == 0) || (temp.MOS1drainArea.Value == 0) || (temp.MOS1sourceArea.Value == 0))
            {
                DrainSatCur = temp.MOS1tSatCur;
                SourceSatCur = temp.MOS1tSatCur;
            }
            else
            {
                DrainSatCur = temp.MOS1tSatCurDens * temp.MOS1drainArea;
                SourceSatCur = temp.MOS1tSatCurDens * temp.MOS1sourceArea;
            }
            GateSourceOverlapCap = modeltemp.MOS1gateSourceOverlapCapFactor * temp.MOS1w;
            GateDrainOverlapCap = modeltemp.MOS1gateDrainOverlapCapFactor * temp.MOS1w;
            GateBulkOverlapCap = modeltemp.MOS1gateBulkOverlapCapFactor * EffectiveLength;
            Beta = temp.MOS1tTransconductance * temp.MOS1w / EffectiveLength;
            OxideCap = modeltemp.MOS1oxideCapFactor * EffectiveLength * temp.MOS1w;
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
                ((state.Init == State.InitFlags.InitFix) && (!MOS1off)))
            {
                // general iteration
                vbs = modeltemp.MOS1type * (rstate.Solution[MOS1bNode] - rstate.Solution[MOS1sNodePrime]);
                vgs = modeltemp.MOS1type * (rstate.Solution[MOS1gNode] - rstate.Solution[MOS1sNodePrime]);
                vds = modeltemp.MOS1type * (rstate.Solution[MOS1dNodePrime] - rstate.Solution[MOS1sNodePrime]);

                /* now some common crunching for some more useful quantities */
                vbd = vbs - vds;
                vgd = vgs - vds;
                vgdo = state.States[0][MOS1states + MOS1vgs] - state.States[0][MOS1states + MOS1vds];
                von = modeltemp.MOS1type * MOS1von;

                /* 
				 * limiting
				 * we want to keep device voltages from changing
				 * so fast that the exponentials churn out overflows
				 * and similar rudeness
				 */

                if (state.States[0][MOS1states + MOS1vds] >= 0)
                {
                    vgs = Transistor.DEVfetlim(vgs, state.States[0][MOS1states + MOS1vgs], von);
                    vds = vgs - vgd;
                    vds = Transistor.DEVlimvds(vds, state.States[0][MOS1states + MOS1vds]);
                    vgd = vgs - vds;
                }
                else
                {
                    vgd = Transistor.DEVfetlim(vgd, vgdo, von);
                    vds = vgs - vgd;
                    vds = -Transistor.DEVlimvds(-vds, -(state.States[0][MOS1states + MOS1vds]));
                    vgs = vgd + vds;
                }
                if (vds >= 0)
                {
                    vbs = Transistor.DEVpnjlim(vbs, state.States[0][MOS1states + MOS1vbs], vt, temp.MOS1sourceVcrit, ref Check);
                    vbd = vbs - vds;
                }
                else
                {
                    vbd = Transistor.DEVpnjlim(vbd, state.States[0][MOS1states + MOS1vbd], vt, temp.MOS1drainVcrit, ref Check);
                    vbs = vbd + vds;
                }
            }
            else
            {
                /* ok - not one of the simple cases, so we have to
				 * look at all of the possibilities for why we were
				 * called.  We still just initialize the three voltages
				 */

                if ((state.Init == State.InitFlags.InitJct) && !MOS1off)
                {
                    vds = modeltemp.MOS1type * MOS1icVDS;
                    vgs = modeltemp.MOS1type * MOS1icVGS;
                    vbs = modeltemp.MOS1type * MOS1icVBS;
                    if ((vds == 0) && (vgs == 0) && (vbs == 0) && ((method != null || state.UseDC ||
                        state.Domain == State.DomainTypes.None) || (!state.UseIC)))
                    {
                        vbs = -1;
                        vgs = modeltemp.MOS1type * temp.MOS1tVto;
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
                MOS1gbs = SourceSatCur / vt;
                MOS1cbs = MOS1gbs * vbs;
                MOS1gbs += state.Gmin;
            }
            else
            {
                evbs = Math.Exp(Math.Min(Transistor.MAX_EXP_ARG, vbs / vt));
                MOS1gbs = SourceSatCur * evbs / vt + state.Gmin;
                MOS1cbs = SourceSatCur * (evbs - 1);
            }
            if (vbd <= 0)
            {
                MOS1gbd = DrainSatCur / vt;
                MOS1cbd = MOS1gbd * vbd;
                MOS1gbd += state.Gmin;
            }
            else
            {
                evbd = Math.Exp(Math.Min(Transistor.MAX_EXP_ARG, vbd / vt));
                MOS1gbd = DrainSatCur * evbd / vt + state.Gmin;
                MOS1cbd = DrainSatCur * (evbd - 1);
            }

            /* now to determine whether the user was able to correctly
			 * identify the source and drain of his device
			 */
            if (vds >= 0)
            {
                /* normal mode */
                MOS1mode = 1;
            }
            else
            {
                /* inverse mode */
                MOS1mode = -1;
            }

            /* DETAILPROF */
            {
                /* 
				 * this block of code evaluates the drain current and its 
				 * derivatives using the shichman - hodges model and the 
				 * charges associated with the gate, channel and bulk for 
				 * mosfets
				 */

                /* the following 4 variables are local to this code block until 
				 * it is obvious that they can be made global 
				 */
                double arg;
                double betap;
                double sarg;
                double vgst;

                if ((MOS1mode == 1 ? vbs : vbd) <= 0)
                {
                    sarg = Math.Sqrt(temp.MOS1tPhi - (MOS1mode == 1 ? vbs : vbd));
                }
                else
                {
                    sarg = Math.Sqrt(temp.MOS1tPhi);
                    sarg = sarg - (MOS1mode == 1 ? vbs : vbd) / (sarg + sarg);
                    sarg = Math.Max(0, sarg);
                }
                von = (temp.MOS1tVbi * modeltemp.MOS1type) + modeltemp.MOS1gamma * sarg;
                vgst = (MOS1mode == 1 ? vgs : vgd) - von;
                vdsat = Math.Max(vgst, 0);
                if (sarg <= 0)
                {
                    arg = 0;
                }
                else
                {
                    arg = modeltemp.MOS1gamma / (sarg + sarg);
                }
                if (vgst <= 0)
                {
                    /* 
					 * cutoff region
					 */
                    cdrain = 0;
                    MOS1gm = 0;
                    MOS1gds = 0;
                    MOS1gmbs = 0;
                }
                else
                {
                    /* 
					 * saturation region
					 */
                    betap = Beta * (1 + modeltemp.MOS1lambda * (vds * MOS1mode));
                    if (vgst <= (vds * MOS1mode))
                    {
                        cdrain = betap * vgst * vgst * .5;
                        MOS1gm = betap * vgst;
                        MOS1gds = modeltemp.MOS1lambda * Beta * vgst * vgst * .5;
                        MOS1gmbs = MOS1gm * arg;
                    }
                    else
                    {
                        /* 
						* linear region
						*/
                        cdrain = betap * (vds * MOS1mode) * (vgst - .5 * (vds * MOS1mode));
                        MOS1gm = betap * (vds * MOS1mode);
                        MOS1gds = betap * (vgst - (vds * MOS1mode)) + modeltemp.MOS1lambda * Beta * (vds * MOS1mode) * (vgst - .5 * (vds * MOS1mode));
                        MOS1gmbs = MOS1gm * arg;
                    }
                }
                /* 
				 * finished
				 */
            }

            /* now deal with n vs p polarity */
            MOS1von = modeltemp.MOS1type * von;
            MOS1vdsat = modeltemp.MOS1type * vdsat;
            /* line 490 */
            /* 
			 * COMPUTE EQUIVALENT DRAIN CURRENT SOURCE
			 */
            MOS1cd = MOS1mode * cdrain - MOS1cbd;

            if (state.Domain == State.DomainTypes.Time || state.UseSmallSignal)
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
                    double arg, sarg;

                    /* can't bypass the diode capacitance calculations */
                    /* CAPZEROBYPASS */
                    if (vbs < temp.MOS1tDepCap)
                    {
                        arg = 1 - vbs / temp.MOS1tBulkPot;
                        /* 
						 * the following block looks somewhat long and messy, 
						 * but since most users use the default grading
						 * coefficients of .5, and sqrt is MUCH faster than an
						 * Math.Exp(Math.Log()) we use this special case code to buy time.
						 * (as much as 10% of total job time!)
						 */
                        if (modeltemp.MOS1bulkJctBotGradingCoeff.Value == modeltemp.MOS1bulkJctSideGradingCoeff)
                        {
                            if (modeltemp.MOS1bulkJctBotGradingCoeff.Value == .5)
                            {
                                sarg = sargsw = 1 / Math.Sqrt(arg);
                            }
                            else
                            {
                                sarg = sargsw = Math.Exp(-modeltemp.MOS1bulkJctBotGradingCoeff * Math.Log(arg));
                            }
                        }
                        else
                        {
                            if (modeltemp.MOS1bulkJctBotGradingCoeff.Value == .5)
                            {
                                sarg = 1 / Math.Sqrt(arg);
                            }
                            else
                            {
                                /* NOSQRT */
                                sarg = Math.Exp(-modeltemp.MOS1bulkJctBotGradingCoeff * Math.Log(arg));
                            }
                            if (modeltemp.MOS1bulkJctSideGradingCoeff.Value == .5)
                            {
                                sargsw = 1 / Math.Sqrt(arg);
                            }
                            else
                            {
                                /* NOSQRT */
                                sargsw = Math.Exp(-modeltemp.MOS1bulkJctSideGradingCoeff * Math.Log(arg));
                            }
                        }
                        /* NOSQRT */
                        state.States[0][MOS1states + MOS1qbs] = temp.MOS1tBulkPot * (temp.MOS1Cbs * (1 - arg * sarg) / (1 - modeltemp.MOS1bulkJctBotGradingCoeff) +
                            temp.MOS1Cbssw * (1 - arg * sargsw) / (1 - modeltemp.MOS1bulkJctSideGradingCoeff));
                        MOS1capbs = temp.MOS1Cbs * sarg + temp.MOS1Cbssw * sargsw;
                    }
                    else
                    {
                        state.States[0][MOS1states + MOS1qbs] = temp.MOS1f4s + vbs * (temp.MOS1f2s + vbs * (temp.MOS1f3s / 2));
                        MOS1capbs = temp.MOS1f2s + temp.MOS1f3s * vbs;
                    }
                    /* CAPZEROBYPASS */
                }
                /* CAPBYPASS */
                /* can't bypass the diode capacitance calculations */
                {
                    double arg, sarg;

                    /* CAPZEROBYPASS */
                    if (vbd < temp.MOS1tDepCap)
                    {
                        arg = 1 - vbd / temp.MOS1tBulkPot;
                        /* 
						* the following block looks somewhat long and messy, 
						* but since most users use the default grading
						* coefficients of .5, and sqrt is MUCH faster than an
						* Math.Exp(Math.Log()) we use this special case code to buy time.
						* (as much as 10% of total job time!)
						*/
                        if (modeltemp.MOS1bulkJctBotGradingCoeff.Value == .5 && modeltemp.MOS1bulkJctSideGradingCoeff.Value == .5)
                        {
                            sarg = sargsw = 1 / Math.Sqrt(arg);
                        }
                        else
                        {
                            if (modeltemp.MOS1bulkJctBotGradingCoeff.Value == .5)
                            {
                                sarg = 1 / Math.Sqrt(arg);
                            }
                            else
                            {
                                /* NOSQRT */
                                sarg = Math.Exp(-modeltemp.MOS1bulkJctBotGradingCoeff * Math.Log(arg));
                            }
                            if (modeltemp.MOS1bulkJctSideGradingCoeff.Value == .5)
                            {
                                sargsw = 1 / Math.Sqrt(arg);
                            }
                            else
                            {
                                /* NOSQRT */
                                sargsw = Math.Exp(-modeltemp.MOS1bulkJctSideGradingCoeff * Math.Log(arg));
                            }
                        }
                        /* NOSQRT */
                        state.States[0][MOS1states + MOS1qbd] = temp.MOS1tBulkPot * (temp.MOS1Cbd * (1 - arg * sarg) / (1 - modeltemp.MOS1bulkJctBotGradingCoeff) +
                            temp.MOS1Cbdsw * (1 - arg * sargsw) / (1 - modeltemp.MOS1bulkJctSideGradingCoeff));
                        MOS1capbd = temp.MOS1Cbd * sarg + temp.MOS1Cbdsw * sargsw;
                    }
                    else
                    {
                        state.States[0][MOS1states + MOS1qbd] = temp.MOS1f4d + vbd * (temp.MOS1f2d + vbd * temp.MOS1f3d / 2);
                        MOS1capbd = temp.MOS1f2d + vbd * temp.MOS1f3d;
                    }
                    /* CAPZEROBYPASS */
                }
                /* 
				
				*/

                /* DETAILPROF */
                if ((method != null) || ((state.Init == State.InitFlags.InitTransient) && !state.UseIC))
                {
                    /* (above only excludes tranop, since we're only at this
					 * point if tran or tranop)
					 */

                    /* 
					 * calculate equivalent conductances and currents for
					 * depletion capacitors
					 */

                    /* integrate the capacitors and save results */

                    var result = method.Integrate(state, MOS1states + MOS1qbd, MOS1capbd);
                    MOS1gbd += result.Geq;
                    MOS1cbd += state.States[0][MOS1states + MOS1cqbd];
                    MOS1cd -= state.States[0][MOS1states + MOS1cqbd];
                    result = method.Integrate(state, MOS1states + MOS1qbs, MOS1capbs);
                    MOS1gbs += result.Geq;
                    MOS1cbs += state.States[0][MOS1states + MOS1cqbs];
                }
            }

            /* 
			 * check convergence
			 */
            if (!MOS1off || (!(state.Init == State.InitFlags.InitFix || state.UseSmallSignal)))
            {
                if (Check == 1)
                    state.IsCon = false;
            }

            /* DETAILPROF */

            /* save things away for next time */
            state.States[0][MOS1states + MOS1vbs] = vbs;
            state.States[0][MOS1states + MOS1vbd] = vbd;
            state.States[0][MOS1states + MOS1vgs] = vgs;
            state.States[0][MOS1states + MOS1vds] = vds;

            /* 
			 * meyer's capacitor model
			 */
            if (state.Domain == State.DomainTypes.Time || state.UseSmallSignal)
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
                if (MOS1mode > 0)
                {
                    Transistor.DEVqmeyer(vgs, vgd, vgb, von, vdsat,
                        out icapgs, out icapgd, out icapgb,
                        temp.MOS1tPhi, OxideCap);
                }
                else
                {
                    Transistor.DEVqmeyer(vgd, vgs, vgb, von, vdsat,
                        out icapgd, out icapgs, out icapgb,
                        temp.MOS1tPhi, OxideCap);
                }
                state.States[0][MOS1states + MOS1capgs] = icapgs;
                state.States[0][MOS1states + MOS1capgd] = icapgd;
                state.States[0][MOS1states + MOS1capgb] = icapgb;
                vgs1 = state.States[1][MOS1states + MOS1vgs];
                vgd1 = vgs1 - state.States[1][MOS1states + MOS1vds];
                vgb1 = vgs1 - state.States[1][MOS1states + MOS1vbs];
                if ((state.Domain == State.DomainTypes.Time && state.UseDC) || state.UseSmallSignal)
                {
                    capgs = 2 * state.States[0][MOS1states + MOS1capgs] + GateSourceOverlapCap;
                    capgd = 2 * state.States[0][MOS1states + MOS1capgd] + GateDrainOverlapCap;
                    capgb = 2 * state.States[0][MOS1states + MOS1capgb] + GateBulkOverlapCap;
                }
                else
                {
                    capgs = (state.States[0][MOS1states + MOS1capgs] + state.States[1][MOS1states + MOS1capgs] + GateSourceOverlapCap);
                    capgd = (state.States[0][MOS1states + MOS1capgd] + state.States[1][MOS1states + MOS1capgd] + GateDrainOverlapCap);
                    capgb = (state.States[0][MOS1states + MOS1capgb] + state.States[1][MOS1states + MOS1capgb] + GateBulkOverlapCap);
                }

                /* PREDICTOR */
                if (method != null)
                {
                    state.States[0][MOS1states + MOS1qgs] = (vgs - vgs1) * capgs + state.States[1][MOS1states + MOS1qgs];
                    state.States[0][MOS1states + MOS1qgd] = (vgd - vgd1) * capgd + state.States[1][MOS1states + MOS1qgd];
                    state.States[0][MOS1states + MOS1qgb] = (vgb - vgb1) * capgb + state.States[1][MOS1states + MOS1qgb];
                }
                else
                {
                    /* TRANOP only */
                    state.States[0][MOS1states + MOS1qgs] = vgs * capgs;
                    state.States[0][MOS1states + MOS1qgd] = vgd * capgd;
                    state.States[0][MOS1states + MOS1qgb] = vgb * capgb;
                }
                /* PREDICTOR */
            }

            if (((state.Init == State.InitFlags.InitTransient)) || (!(method != null)))
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
                    state.States[0][MOS1states + MOS1cqgs] = 0;
                if (capgd == 0)
                    state.States[0][MOS1states + MOS1cqgd] = 0;
                if (capgb == 0)
                    state.States[0][MOS1states + MOS1cqgb] = 0;
                /* 
				 * calculate equivalent conductances and currents for
				 * meyer"s capacitors
				 */
                method.Integrate(state, out gcgs, out ceqgs, MOS1states + MOS1qgs, capgs);
                method.Integrate(state, out gcgd, out ceqgd, MOS1states + MOS1qgd, capgd);
                method.Integrate(state, out gcgb, out ceqgb, MOS1states + MOS1qgb, capgb);
                ceqgs = ceqgs - gcgs * vgs + method.Slope * state.States[0][MOS1states + MOS1qgs];
                ceqgd = ceqgd - gcgd * vgd + method.Slope * state.States[0][MOS1states + MOS1qgd];
                ceqgb = ceqgb - gcgb * vgb + method.Slope * state.States[0][MOS1states + MOS1qgb];
            }

            /* 
			 * load current vector
			 */
            ceqbs = modeltemp.MOS1type * (MOS1cbs - (MOS1gbs - state.Gmin) * vbs);
            ceqbd = modeltemp.MOS1type * (MOS1cbd - (MOS1gbd - state.Gmin) * vbd);
            if (MOS1mode >= 0)
            {
                xnrm = 1;
                xrev = 0;
                cdreq = modeltemp.MOS1type * (cdrain - MOS1gds * vds - MOS1gm * vgs - MOS1gmbs * vbs);
            }
            else
            {
                xnrm = 0;
                xrev = 1;
                cdreq = -(modeltemp.MOS1type) * (cdrain - MOS1gds * (-vds) - MOS1gm * vgd - MOS1gmbs * vbd);
            }
            rstate.Rhs[MOS1gNode] -= (modeltemp.MOS1type * (ceqgs + ceqgb + ceqgd));
            rstate.Rhs[MOS1bNode] -= (ceqbs + ceqbd - modeltemp.MOS1type * ceqgb);
            rstate.Rhs[MOS1dNodePrime] += (ceqbd - cdreq + modeltemp.MOS1type * ceqgd);
            rstate.Rhs[MOS1sNodePrime] += cdreq + ceqbs + modeltemp.MOS1type * ceqgs;

            /* 
			 * load y matrix
			 */
            MOS1DdPtr.Add(temp.MOS1drainConductance);
            MOS1GgPtr.Add(gcgd + gcgs + gcgb);
            MOS1SsPtr.Add(temp.MOS1sourceConductance);
            MOS1BbPtr.Add(MOS1gbd + MOS1gbs + gcgb);
            MOS1DPdpPtr.Add(temp.MOS1drainConductance + MOS1gds + MOS1gbd + xrev * (MOS1gm + MOS1gmbs) + gcgd);
            MOS1SPspPtr.Add(temp.MOS1sourceConductance + MOS1gds + MOS1gbs + xnrm * (MOS1gm + MOS1gmbs) + gcgs);
            MOS1DdpPtr.Add(-temp.MOS1drainConductance);
            MOS1GbPtr.Sub(gcgb);
            MOS1GdpPtr.Sub(gcgd);
            MOS1GspPtr.Sub(gcgs);
            MOS1SspPtr.Add(-temp.MOS1sourceConductance);
            MOS1BgPtr.Sub(gcgb);
            MOS1BdpPtr.Sub(MOS1gbd);
            MOS1BspPtr.Sub(MOS1gbs);
            MOS1DPdPtr.Add(-temp.MOS1drainConductance);
            MOS1DPgPtr.Add((xnrm - xrev) * MOS1gm - gcgd);
            MOS1DPbPtr.Add(-MOS1gbd + (xnrm - xrev) * MOS1gmbs);
            MOS1DPspPtr.Add(-MOS1gds - xnrm * (MOS1gm + MOS1gmbs));
            MOS1SPgPtr.Add(-(xnrm - xrev) * MOS1gm - gcgs);
            MOS1SPsPtr.Add(-temp.MOS1sourceConductance);
            MOS1SPbPtr.Add(-MOS1gbs - (xnrm - xrev) * MOS1gmbs);
            MOS1SPdpPtr.Add(-MOS1gds - xrev * (MOS1gm + MOS1gmbs));
        }

        /// <summary>
        /// Test convergence
        /// </summary>
        /// <param name="ckt">Circuit</param>
        /// <returns></returns>
        public override bool IsConvergent(Circuit ckt)
        {
            var config = ckt.Simulation.CurrentConfig;
            var state = ckt.State;

            double vbs, vgs, vds, vbd, vgd, vgdo, delvbs, delvbd, delvgs, delvds, delvgd, cdhat, cbhat;

            vbs = modeltemp.MOS1type * (state.Solution[MOS1bNode] - state.Solution[MOS1sNodePrime]);
            vgs = modeltemp.MOS1type * (state.Solution[MOS1gNode] - state.Solution[MOS1sNodePrime]);
            vds = modeltemp.MOS1type * (state.Solution[MOS1dNodePrime] - state.Solution[MOS1sNodePrime]);
            vbd = vbs - vds;
            vgd = vgs - vds;
            vgdo = state.States[0][MOS1states + MOS1vgs] - state.States[0][MOS1states + MOS1vds];
            delvbs = vbs - state.States[0][MOS1states + MOS1vbs];
            delvbd = vbd - state.States[0][MOS1states + MOS1vbd];
            delvgs = vgs - state.States[0][MOS1states + MOS1vgs];
            delvds = vds - state.States[0][MOS1states + MOS1vds];
            delvgd = vgd - vgdo;

            /* these are needed for convergence testing */
            if (MOS1mode >= 0)
            {
                cdhat = MOS1cd - MOS1gbd * delvbd + MOS1gmbs * delvbs +
                    MOS1gm * delvgs + MOS1gds * delvds;
            }
            else
            {
                cdhat = MOS1cd - (MOS1gbd - MOS1gmbs) * delvbd -
                    MOS1gm * delvgd + MOS1gds * delvds;
            }
            cbhat = MOS1cbs + MOS1cbd + MOS1gbd * delvbd + MOS1gbs * delvbs;

            /*
             *  check convergence
             */
            double tol = config.RelTol * Math.Max(Math.Abs(cdhat), Math.Abs(MOS1cd)) + config.AbsTol;
            if (Math.Abs(cdhat - MOS1cd) >= tol)
            {
                state.IsCon = false;
                return false;
            }
            else
            {
                tol = config.RelTol * Math.Max(Math.Abs(cbhat), Math.Abs(MOS1cbs + MOS1cbd)) + config.AbsTol;
                if (Math.Abs(cbhat - (MOS1cbs + MOS1cbd)) > tol)
                {
                    state.IsCon = false;
                    return false;
                }
            }
            return true;
        }
    }
}
