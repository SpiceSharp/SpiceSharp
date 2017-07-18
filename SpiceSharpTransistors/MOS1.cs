using System;
using SpiceSharp.Circuits;
using SpiceSharp.Diagnostics;
using SpiceSharp.Parameters;
using SpiceSharp.Components.Transistors;

namespace SpiceSharp.Components
{
    public class MOS1 : CircuitComponent
    {
        /// <summary>
        /// Gets or sets the device model
        /// </summary>
        public MOS1Model Model { get; set; }

        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("temp"), SpiceInfo("Instance temperature")]
        public Parameter<double> MOS1temp { get; } = new Parameter<double>();
        [SpiceName("w"), SpiceInfo("Width")]
        public Parameter<double> MOS1w { get; } = new Parameter<double>();
        [SpiceName("l"), SpiceInfo("Length")]
        public Parameter<double> MOS1l { get; } = new Parameter<double>();
        [SpiceName("as"), SpiceInfo("Source area")]
        public Parameter<double> MOS1sourceArea { get; } = new Parameter<double>();
        [SpiceName("ad"), SpiceInfo("Drain area")]
        public Parameter<double> MOS1drainArea { get; } = new Parameter<double>();
        [SpiceName("ps"), SpiceInfo("Source perimeter")]
        public Parameter<double> MOS1sourcePerimiter { get; } = new Parameter<double>();
        [SpiceName("pd"), SpiceInfo("Drain perimeter")]
        public Parameter<double> MOS1drainPerimiter { get; } = new Parameter<double>();
        [SpiceName("nrs"), SpiceInfo("Source squares")]
        public Parameter<double> MOS1sourceSquares { get; } = new Parameter<double>();
        [SpiceName("nrd"), SpiceInfo("Drain squares")]
        public Parameter<double> MOS1drainSquares { get; } = new Parameter<double>();
        [SpiceName("off"), SpiceInfo("Device initially off")]
        public bool MOS1off { get; set; }
        [SpiceName("icvbs"), SpiceInfo("Initial B-S voltage")]
        public Parameter<double> MOS1icVBS { get; } = new Parameter<double>();
        [SpiceName("icvds"), SpiceInfo("Initial D-S voltage")]
        public Parameter<double> MOS1icVDS { get; } = new Parameter<double>();
        [SpiceName("icvgs"), SpiceInfo("Initial G-S voltage")]
        public Parameter<double> MOS1icVGS { get; } = new Parameter<double>();
        [SpiceName("dnode"), SpiceInfo("Number of the drain node ")]
        public int MOS1dNode { get; private set; }
        [SpiceName("gnode"), SpiceInfo("Number of the gate node ")]
        public int MOS1gNode { get; private set; }
        [SpiceName("snode"), SpiceInfo("Number of the source node ")]
        public int MOS1sNode { get; private set; }
        [SpiceName("bnode"), SpiceInfo("Number of the node ")]
        public int MOS1bNode { get; private set; }
        [SpiceName("dnodeprime"), SpiceInfo("Number of int. drain node")]
        public int MOS1dNodePrime { get; private set; }
        [SpiceName("snodeprime"), SpiceInfo("Number of int. source node ")]
        public int MOS1sNodePrime { get; private set; }
        [SpiceName("sourceconductance"), SpiceInfo("Conductance of source")]
        public double MOS1sourceConductance { get; private set; }
        [SpiceName("drainconductance"), SpiceInfo("Conductance of drain")]
        public double MOS1drainConductance { get; private set; }
        [SpiceName("von"), SpiceInfo(" ")]
        public double MOS1von { get; private set; }
        [SpiceName("vdsat"), SpiceInfo("Saturation drain voltage")]
        public double MOS1vdsat { get; private set; }
        [SpiceName("sourcevcrit"), SpiceInfo("Critical source voltage")]
        public double MOS1sourceVcrit { get; private set; }
        [SpiceName("drainvcrit"), SpiceInfo("Critical drain voltage")]
        public double MOS1drainVcrit { get; private set; }
        [SpiceName("id"), SpiceInfo("Drain current")]
        public double MOS1cd { get; private set; }
        [SpiceName("ibs"), SpiceInfo("B-S junction current")]
        public double MOS1cbs { get; private set; }
        [SpiceName("ibd"), SpiceInfo("B-D junction current")]
        public double MOS1cbd { get; private set; }
        [SpiceName("gmb"), SpiceName("gmbs"), SpiceInfo("Bulk-Source transconductance")]
        public double MOS1gmbs { get; private set; }
        [SpiceName("gm"), SpiceInfo("Transconductance")]
        public double MOS1gm { get; private set; }
        [SpiceName("gds"), SpiceInfo("Drain-Source conductance")]
        public double MOS1gds { get; private set; }
        [SpiceName("gbd"), SpiceInfo("Bulk-Drain conductance")]
        public double MOS1gbd { get; private set; }
        [SpiceName("gbs"), SpiceInfo("Bulk-Source conductance")]
        public double MOS1gbs { get; private set; }
        [SpiceName("cbd"), SpiceInfo("Bulk-Drain capacitance")]
        public double MOS1capbd { get; private set; }
        [SpiceName("cbs"), SpiceInfo("Bulk-Source capacitance")]
        public double MOS1capbs { get; private set; }
        [SpiceName("cbd0"), SpiceInfo("Zero-Bias B-D junction capacitance")]
        public double MOS1Cbd { get; private set; }
        [SpiceName("cbdsw0"), SpiceInfo(" ")]
        public double MOS1Cbdsw { get; private set; }
        [SpiceName("cbs0"), SpiceInfo("Zero-Bias B-S junction capacitance")]
        public double MOS1Cbs { get; private set; }
        [SpiceName("cbssw0"), SpiceInfo(" ")]
        public double MOS1Cbssw { get; private set; }

        /// <summary>
        /// Extra variables
        /// </summary>
        public int MOS1states { get; private set; }
        public double MOS1name { get; private set; }
        public double MOS1tTransconductance { get; private set; }
        public double MOS1tSurfMob { get; private set; }
        public double MOS1tPhi { get; private set; }
        public double MOS1tVbi { get; private set; }
        public double MOS1tVto { get; private set; }
        public double MOS1tSatCur { get; private set; }
        public double MOS1tSatCurDens { get; private set; }
        public double MOS1tCbd { get; private set; }
        public double MOS1tCbs { get; private set; }
        public double MOS1tCj { get; private set; }
        public double MOS1tCjsw { get; private set; }
        public double MOS1tBulkPot { get; private set; }
        public double MOS1tDepCap { get; private set; }
        public double MOS1f2d { get; private set; }
        public double MOS1f3d { get; private set; }
        public double MOS1f4d { get; private set; }
        public double MOS1f2s { get; private set; }
        public double MOS1f3s { get; private set; }
        public double MOS1f4s { get; private set; }
        public double MOS1senPertFlag { get; private set; }
        public double MOS1mode { get; private set; }
        public double MOS1cgs { get; private set; }
        public double MOS1cgd { get; private set; }
        public double MOS1cgb { get; private set; }

        /// <summary>
        /// Constants
        /// </summary>
        private const int MOS1vbd = 0;
        private const int MOS1vbs = 1;
        private const int MOS1vgs = 2;
        private const int MOS1vds = 3;
        private const int MOS1capgs = 4;
        private const int MOS1qgs = 5;
        private const int MOS1cqgs = 6;
        private const int MOS1capgd = 7;
        private const int MOS1qgd = 8;
        private const int MOS1cqgd = 9;
        private const int MOS1capgb = 10;
        private const int MOS1qgb = 11;
        private const int MOS1cqgb = 12;
        private const int MOS1qbd = 13;
        private const int MOS1cqbd = 14;
        private const int MOS1qbs = 15;
        private const int MOS1cqbs = 16;
        private const double MAX_EXP_ARG = 709.0;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the device</param>
        public MOS1(string name) : base(name, 4)
        {
        }

        /// <summary>
        /// Get the model
        /// </summary>
        /// <returns></returns>
        public override CircuitModel GetModel() => Model;

        /// <summary>
        /// Setup the device
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Setup(Circuit ckt)
        {
            // Allocate nodes
            var nodes = BindNodes(ckt);
            MOS1dNode = nodes[0].Index;
            MOS1gNode = nodes[1].Index;
            MOS1sNode = nodes[2].Index;
            MOS1bNode = nodes[3].Index;

            // Allocate states
            MOS1states = ckt.State.GetState(17);

            /* allocate a chunk of the state vector */

            if ((Model.MOS1drainResistance != 0
            || (Model.MOS1sheetResistance != 0
            && MOS1drainSquares != 0))
            && MOS1dNodePrime == 0)
            {
                MOS1dNodePrime = CreateNode(ckt).Index;
            }
            else
            {
                MOS1dNodePrime = MOS1dNode;
            }

            if ((Model.MOS1sourceResistance != 0 ||
            (Model.MOS1sheetResistance != 0 &&
            MOS1sourceSquares != 0)) &&
            MOS1sNodePrime == 0)
            {
                MOS1sNodePrime = CreateNode(ckt).Index;
            }
            else
            {
                MOS1sNodePrime = MOS1sNode;
            }
            /* macro to make elements with built in test for out of memory */
        }

        /// <summary>
        /// Do temperature-dependent calculations
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Temperature(Circuit ckt)
        {
            double vt;
            double ratio;
            double fact2;
            double kt;
            double egfet;
            double arg;
            double pbfact;
            double ratio4;
            double phio;
            double pbo;
            double gmaold;
            double capfact;
            double gmanew;
            double czbd;
            double czbdsw;
            double sarg;
            double sargsw;
            double czbs;
            double czbssw;


            /* perform the parameter defaulting */
            if (!MOS1temp.Given)
            {
                MOS1temp.Value = ckt.State.Temperature;
            }
            vt = MOS1temp * Circuit.CONSTKoverQ;
            ratio = MOS1temp / Model.MOS1tnom;
            fact2 = MOS1temp / Circuit.CONSTRefTemp;
            kt = MOS1temp * Circuit.CONSTBoltz;
            egfet = 1.16 - (7.02e-4 * MOS1temp * MOS1temp) /
            (MOS1temp + 1108);
            arg = -egfet / (kt + kt) + 1.1150877 / (Circuit.CONSTBoltz * (Circuit.CONSTRefTemp + Circuit.CONSTRefTemp));
            pbfact = -2 * vt * (1.5 * Math.Log(fact2) + Circuit.CHARGE * arg);

            if (MOS1l - 2 * Model.MOS1latDiff <= 0)
                throw new CircuitException($"{Name}: effective channel length less than zero");
            ratio4 = ratio * Math.Sqrt(ratio);
            MOS1tTransconductance = Model.MOS1transconductance / ratio4;
            MOS1tSurfMob = Model.MOS1surfaceMobility / ratio4;
            phio = (Model.MOS1phi - Model.pbfact1) / Model.fact1;
            MOS1tPhi = fact2 * phio + pbfact;
            MOS1tVbi =
            Model.MOS1vt0 - Model.MOS1type * (Model.MOS1gamma * Math.Sqrt(Model.MOS1phi)) + .5 * (Model.egfet1 - egfet) + Model.MOS1type * .5 * (MOS1tPhi - Model.MOS1phi);
            MOS1tVto = MOS1tVbi + Model.MOS1type * Model.MOS1gamma * Math.Sqrt(MOS1tPhi);
            MOS1tSatCur = Model.MOS1jctSatCur * Math.Exp(-egfet / vt + Model.egfet1 / Model.vtnom);
            MOS1tSatCurDens = Model.MOS1jctSatCurDensity * Math.Exp(-egfet / vt + Model.egfet1 / Model.vtnom);
            pbo = (Model.MOS1bulkJctPotential - Model.pbfact1) / Model.fact1;
            gmaold = (Model.MOS1bulkJctPotential - pbo) / pbo;
            capfact = 1 / (1 + Model.MOS1bulkJctBotGradingCoeff * (4e-4 * (Model.MOS1tnom - Circuit.CONSTRefTemp) - gmaold));
            MOS1tCbd = Model.MOS1capBD * capfact;
            MOS1tCbs = Model.MOS1capBS * capfact;
            MOS1tCj = Model.MOS1bulkCapFactor * capfact;
            capfact = 1 / (1 + Model.MOS1bulkJctSideGradingCoeff * (4e-4 * (Model.MOS1tnom - Circuit.CONSTRefTemp) - gmaold));
            MOS1tCjsw = Model.MOS1sideWallCapFactor * capfact;
            MOS1tBulkPot = fact2 * pbo + pbfact;
            gmanew = (MOS1tBulkPot - pbo) / pbo;
            capfact = (1 + Model.MOS1bulkJctBotGradingCoeff * (4e-4 * (MOS1temp - Circuit.CONSTRefTemp) - gmanew));
            MOS1tCbd *= capfact;
            MOS1tCbs *= capfact;
            MOS1tCj *= capfact;
            capfact = (1 + Model.MOS1bulkJctSideGradingCoeff * (4e-4 * (MOS1temp - Circuit.CONSTRefTemp) - gmanew));
            MOS1tCjsw *= capfact;
            MOS1tDepCap = Model.MOS1fwdCapDepCoeff * MOS1tBulkPot;
            if ((MOS1tSatCurDens == 0) || (MOS1drainArea == 0) || (MOS1sourceArea == 0))
            {
                MOS1sourceVcrit = MOS1drainVcrit =
                vt * Math.Log(vt / (Circuit.CONSTroot2 * MOS1tSatCur));
            }
            else
            {
                MOS1drainVcrit = vt * Math.Log(vt / (Circuit.CONSTroot2 * MOS1tSatCurDens * MOS1drainArea));
                MOS1sourceVcrit = vt * Math.Log(vt / (Circuit.CONSTroot2 * MOS1tSatCurDens * MOS1sourceArea));
            }

            if (Model.MOS1capBD.Given)
            {
                czbd = MOS1tCbd;
            }
            else
            {
                if (Model.MOS1bulkCapFactor.Given)
                {
                    czbd = MOS1tCj * MOS1drainArea;
                }
                else
                {
                    czbd = 0;
                }
            }
            if (Model.MOS1sideWallCapFactor.Given)
            {
                czbdsw = MOS1tCjsw * MOS1drainPerimiter;
            }
            else
            {
                czbdsw = 0;
            }
            arg = 1 - Model.MOS1fwdCapDepCoeff;
            sarg = Math.Exp((-Model.MOS1bulkJctBotGradingCoeff) * Math.Log(arg));
            sargsw = Math.Exp((-Model.MOS1bulkJctSideGradingCoeff) * Math.Log(arg));
            MOS1Cbd = czbd;
            MOS1Cbdsw = czbdsw;
            MOS1f2d = czbd * (1 - Model.MOS1fwdCapDepCoeff * (1 + Model.MOS1bulkJctBotGradingCoeff)) * sarg / arg
                + czbdsw * (1 - Model.MOS1fwdCapDepCoeff * (1 + Model.MOS1bulkJctSideGradingCoeff)) * sargsw / arg;
            MOS1f3d = czbd * Model.MOS1bulkJctBotGradingCoeff * sarg / arg / MOS1tBulkPot
                + czbdsw * Model.MOS1bulkJctSideGradingCoeff * sargsw / arg / MOS1tBulkPot;
            MOS1f4d = czbd * MOS1tBulkPot * (1 - arg * sarg) / (1 - Model.MOS1bulkJctBotGradingCoeff)
                + czbdsw * MOS1tBulkPot * (1 - arg * sargsw) / (1 - Model.MOS1bulkJctSideGradingCoeff)
                - MOS1f3d / 2 * (MOS1tDepCap * MOS1tDepCap) - MOS1tDepCap * MOS1f2d;
            if (Model.MOS1capBS.Given)
            {
                czbs = MOS1tCbs;
            }
            else
            {
                if (Model.MOS1bulkCapFactor.Given)
                {
                    czbs = MOS1tCj * MOS1sourceArea;
                }
                else
                {
                    czbs = 0;
                }
            }
            if (Model.MOS1sideWallCapFactor.Given)
            {
                czbssw = MOS1tCjsw * MOS1sourcePerimiter;
            }
            else
            {
                czbssw = 0;
            }
            arg = 1 - Model.MOS1fwdCapDepCoeff;
            sarg = Math.Exp((-Model.MOS1bulkJctBotGradingCoeff) * Math.Log(arg));
            sargsw = Math.Exp((-Model.MOS1bulkJctSideGradingCoeff) * Math.Log(arg));
            MOS1Cbs = czbs;
            MOS1Cbssw = czbssw;
            MOS1f2s = czbs * (1 - Model.MOS1fwdCapDepCoeff * (1 + Model.MOS1bulkJctBotGradingCoeff)) * sarg / arg
                + czbssw * (1 - Model.MOS1fwdCapDepCoeff * (1 + Model.MOS1bulkJctSideGradingCoeff)) * sargsw / arg;
            MOS1f3s = czbs * Model.MOS1bulkJctBotGradingCoeff * sarg / arg / MOS1tBulkPot
                + czbssw * Model.MOS1bulkJctSideGradingCoeff * sargsw / arg / MOS1tBulkPot;
            MOS1f4s = czbs * MOS1tBulkPot * (1 - arg * sarg) / (1 - Model.MOS1bulkJctBotGradingCoeff)
                + czbssw * MOS1tBulkPot * (1 - arg * sargsw) / (1 - Model.MOS1bulkJctSideGradingCoeff)
                - MOS1f3s / 2 * (MOS1tDepCap * MOS1tDepCap) - MOS1tDepCap * MOS1f2s;

            if (Model.MOS1drainResistance.Given)
            {
                if (Model.MOS1drainResistance != 0)
                {
                    MOS1drainConductance = 1 / Model.MOS1drainResistance;
                }
                else
                {
                    MOS1drainConductance = 0;
                }
            }
            else if (Model.MOS1sheetResistance.Given)
            {
                if (Model.MOS1sheetResistance != 0)
                {
                    MOS1drainConductance =
                    1 / (Model.MOS1sheetResistance * MOS1drainSquares);
                }
                else
                {
                    MOS1drainConductance = 0;
                }
            }
            else
            {
                MOS1drainConductance = 0;
            }
            if (Model.MOS1sourceResistance.Given)
            {
                if (Model.MOS1sourceResistance != 0)
                {
                    MOS1sourceConductance = 1 / Model.MOS1sourceResistance;
                }
                else
                {
                    MOS1sourceConductance = 0;
                }
            }
            else if (Model.MOS1sheetResistance.Given)
            {
                if (Model.MOS1sheetResistance != 0)
                {
                    MOS1sourceConductance =
                    1 / (Model.MOS1sheetResistance * MOS1sourceSquares);
                }
                else
                {
                    MOS1sourceConductance = 0;
                }
            }
            else
            {
                MOS1sourceConductance = 0;
            }
        }

        /// <summary>
        /// Load the device
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Load(Circuit ckt)
        {
            var state = ckt.State;
            var rstate = state.Real;
            var method = ckt.Method;

            double vt;
            int Check;
            double EffectiveLength;
            double DrainSatCur;
            double SourceSatCur;
            double GateSourceOverlapCap;
            double GateDrainOverlapCap;
            double GateBulkOverlapCap;
            double Beta;
            double OxideCap;
            double vgs;
            double vds;
            double vbs;
            double vbd;
            double vgb;
            double vgd;
            double vgdo;
            double delvbs;
            double delvbd;
            double delvgs;
            double delvds;
            double delvgd;
            double cdhat = double.NaN;
            double cbhat = double.NaN;
            double von;
            double evbs;
            double evbd;
            double sarg;
            double vgst;
            double vdsat;
            double arg;
            double cdrain;
            double betap;
            double sargsw;
            double tol;
            double vgs1;
            double vgd1;
            double vgb1;
            double capgs = double.NaN;
            double capgd = double.NaN;
            double capgb = double.NaN;
            double gcgs = double.NaN;
            double ceqgs = double.NaN;
            double gcgd = double.NaN;
            double ceqgd = double.NaN;
            double gcgb = double.NaN;
            double ceqgb = double.NaN;
            double ceqbs = double.NaN;
            double ceqbd = double.NaN;
            int xnrm;
            int xrev;
            double cdreq;

            vt = Circuit.CONSTKoverQ * MOS1temp;
            Check = 1;

            /* first, we compute a few useful values - these could be
			* pre-computed, but for historical reasons are still done
			* here.  They may be moved at the expense of instance size
			*/

            EffectiveLength = MOS1l - 2 * Model.MOS1latDiff;
            if ((MOS1tSatCurDens == 0) || (MOS1drainArea == 0) || (MOS1sourceArea == 0))
            {
                DrainSatCur = MOS1tSatCur;
                SourceSatCur = MOS1tSatCur;
            }
            else
            {
                DrainSatCur = MOS1tSatCurDens * MOS1drainArea;
                SourceSatCur = MOS1tSatCurDens * MOS1sourceArea;
            }
            GateSourceOverlapCap = Model.MOS1gateSourceOverlapCapFactor * MOS1w;
            GateDrainOverlapCap = Model.MOS1gateDrainOverlapCapFactor * MOS1w;
            GateBulkOverlapCap = Model.MOS1gateBulkOverlapCapFactor * EffectiveLength;
            Beta = MOS1tTransconductance * MOS1w / EffectiveLength;
            OxideCap = Model.MOS1oxideCapFactor * EffectiveLength * MOS1w;

            /* 
			 * ok - now to do the start-up operations
			 *
			 * we must get values for vbs, vds, and vgs from somewhere
			 * so we either predict them or recover them from last iteration
			 * These are the two most common cases - either a prediction
			 * step or the general iteration step and they
			 * share some code, so we put them first - others later on
			 */
            if (state.Init == CircuitState.InitFlags.InitFloat || state.UseSmallSignal || (method != null && method.SavedTime == 0.0) || (state.Init == CircuitState.InitFlags.InitFix && !MOS1off))
            {
                // general iteration 
                vbs = Model.MOS1type * (rstate.OldSolution[MOS1bNode] - rstate.OldSolution[MOS1sNodePrime]);
                vgs = Model.MOS1type * (rstate.OldSolution[MOS1gNode] - rstate.OldSolution[MOS1sNodePrime]);
                vds = Model.MOS1type * (rstate.OldSolution[MOS1dNodePrime] - rstate.OldSolution[MOS1sNodePrime]);

                // now some common crunching for some more useful quantities

                vbd = vbs - vds;
                vgd = vgs - vds;
                vgdo = state.States[0][MOS1states + MOS1vgs] - state.States[0][MOS1states + MOS1vds];
                delvbs = vbs - state.States[0][MOS1states + MOS1vbs];
                delvbd = vbd - state.States[0][MOS1states + MOS1vbd];
                delvgs = vgs - state.States[0][MOS1states + MOS1vgs];
                delvds = vds - state.States[0][MOS1states + MOS1vds];
                delvgd = vgd - vgdo;

                // these are needed for convergence testing
                if (MOS1mode >= 0)
                    cdhat = MOS1cd - MOS1gbd * delvbd + MOS1gmbs * delvbs + MOS1gm * delvgs + MOS1gds * delvds;
                else
                    cdhat = MOS1cd - (MOS1gbd - MOS1gmbs) * delvbd - MOS1gm * delvgd + MOS1gds * delvds;
                cbhat = MOS1cbs + MOS1cbd + MOS1gbd * delvbd + MOS1gbs * delvbs;

                von = Model.MOS1type * MOS1von;

                /* 
				* limiting
				*  we want to keep device voltages from changing
				* so fast that the exponentials churn out overflows
				* and similar rudeness
				*/
                if (state.States[0][MOS1states + MOS1vds] >= 0)
                {
                    vgs = Transistor.DEVfetlim(vgs, state.States[0][MOS1states + MOS1vgs] , von);
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
                    vbs = Transistor.DEVpnjlim(vbs, state.States[0][MOS1states + MOS1vbs], vt, MOS1sourceVcrit, ref Check);
                    vbd = vbs - vds;
                }
                else
                {
                    vbd = Transistor.DEVpnjlim(vbd, state.States[0][MOS1states + MOS1vbd], vt, MOS1drainVcrit, ref Check);
                    vbs = vbd + vds;
                }
            }
            else
            {

                /* ok - not one of the simple cases, so we have to
				* look at all of the possibilities for why we were
				* called.  We still just initialize the three voltages
				*/

                if (state.Init == CircuitState.InitFlags.InitJct && !MOS1off)
                {
                    vds = Model.MOS1type * MOS1icVDS;
                    vgs = Model.MOS1type * MOS1icVGS;
                    vbs = Model.MOS1type * MOS1icVBS;
                    if ((vds == 0) && (vgs == 0) && (vbs == 0) && (method != null || state.UseDC || state.Domain == CircuitState.DomainTypes.None || !state.UseIC))
                    {
                        vbs = -1;
                        vgs = Model.MOS1type * MOS1tVto;
                        vds = 0;
                    }
                }
                else
                {
                    vbs = vgs = vds = 0;
                }
            }

            /*
			* now all the preliminaries are over - we can start doing the
			* real work
			*/
            vbd = vbs - vds;
            vgd = vgs - vds;
            vgb = vgs - vbs;

            /*
			* bulk-source and bulk-drain diodes
			*   here we just evaluate the ideal diode current and the
			*   corresponding derivative (conductance).
			*/
            if (vbs <= 0)
            {
                MOS1gbs = SourceSatCur / vt;
                MOS1cbs = MOS1gbs * vbs;
                MOS1gbs += state.Gmin;
            }
            else
            {
                evbs = Math.Exp(Math.Min(MAX_EXP_ARG, vbs / vt));
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
                evbd = Math.Exp(Math.Min(MAX_EXP_ARG, vbd / vt));
                MOS1gbd = DrainSatCur * evbd / vt + state.Gmin;
                MOS1cbd = DrainSatCur * (evbd - 1);
            }

            /* now to determine whether the user was able to correctly
			* identify the source and drain of his device
			*/
            if (vds >= 0)
            {
                // normal mode
                MOS1mode = 1;
            }
            else
            {
                // inverse mode
                MOS1mode = -1;
            }

            {
                /*
				*     this block of code evaluates the drain current and its 
				*     derivatives using the shichman-hodges model and the 
				*     charges associated with the gate, channel and bulk for 
				*     mosfets
				*
				*/

                /* the following 4 variables are local to this code block until 
				* it is obvious that they can be made global 
				*/

                if ((MOS1mode == 1 ? vbs : vbd) <= 0)
                {
                    sarg = Math.Sqrt(MOS1tPhi - (MOS1mode == 1 ? vbs : vbd));
                }
                else
                {
                    sarg = Math.Sqrt(MOS1tPhi);
                    sarg = sarg - (MOS1mode == 1 ? vbs : vbd) / (sarg + sarg);
                    sarg = Math.Max(0, sarg);
                }
                von = (MOS1tVbi * Model.MOS1type) + Model.MOS1gamma * sarg;
                vgst = (MOS1mode == 1 ? vgs : vgd) - von;
                vdsat = Math.Max(vgst, 0);
                if (sarg <= 0)
                {
                    arg = 0;
                }
                else
                {
                    arg = Model.MOS1gamma / (sarg + sarg);
                }
                if (vgst <= 0)
                {
                    // cutoff region
                    cdrain = 0;
                    MOS1gm = 0;
                    MOS1gds = 0;
                    MOS1gmbs = 0;
                }
                else
                {
                    // saturation region
                    betap = Beta * (1 + Model.MOS1lambda * (vds * MOS1mode));
                    if (vgst <= (vds * MOS1mode))
                    {
                        cdrain = betap * vgst * vgst * .5;
                        MOS1gm = betap * vgst;
                        MOS1gds = Model.MOS1lambda * Beta * vgst * vgst * .5;
                        MOS1gmbs = MOS1gm * arg;
                    }
                    else
                    {
                        // linear region
                        cdrain = betap * (vds * MOS1mode) *
                        (vgst - .5 * (vds * MOS1mode));
                        MOS1gm = betap * (vds * MOS1mode);
                        MOS1gds = betap * (vgst - (vds * MOS1mode)) +
                        Model.MOS1lambda * Beta *
                        (vds * MOS1mode) *
                        (vgst - .5 * (vds * MOS1mode));
                        MOS1gmbs = MOS1gm * arg;
                    }
                }
            }

            // now deal with n vs p polarity
            MOS1von = Model.MOS1type * von;
            MOS1vdsat = Model.MOS1type * vdsat;

            /*
			*  COMPUTE EQUIVALENT DRAIN CURRENT SOURCE
			*/
            MOS1cd = MOS1mode * cdrain - MOS1cbd;
            if (state.UseSmallSignal || method != null || (state.Domain == CircuitState.DomainTypes.Time && state.UseDC))
            {
                /* 
				* now we do the hard part of the bulk-drain and bulk-source
				* diode - we evaluate the non-linear capacitance and
				* charge
				*
				* the basic equations are not hard, but the implementation
				* is somewhat long in an attempt to avoid log/exponential
				* evaluations
				*/
                /*
				*  charge storage elements
				*
				*.. bulk-drain and bulk-source depletion capacitances
				*/
                {
                    // can't bypass the diode capacitance calculations 
                    // CAPZEROBYPASS 
                    if (vbs < MOS1tDepCap)
                    {
                        arg = 1 - vbs / MOS1tBulkPot;
                        /*
						* the following block looks somewhat long and messy,
						* but since most users use the default grading
						* coefficients of .5, and sqrt is MUCH faster than an
						* Math.Exp(Math.Log()) we use this special case code to buy time.
						* (as much as 10% of total job time!)
						*/
                        if (Model.MOS1bulkJctBotGradingCoeff ==
                        Model.MOS1bulkJctSideGradingCoeff)
                        {
                            if (Model.MOS1bulkJctBotGradingCoeff == .5)
                            {
                                sarg = sargsw = 1 / Math.Sqrt(arg);
                            }
                            else
                            {
                                sarg = sargsw =
                                Math.Exp(-Model.MOS1bulkJctBotGradingCoeff *
                                Math.Log(arg));
                            }
                        }
                        else
                        {
                            if (Model.MOS1bulkJctBotGradingCoeff == .5)
                            {
                                sarg = 1 / Math.Sqrt(arg);
                            }
                            else
                            {
                                sarg = Math.Exp(-Model.MOS1bulkJctBotGradingCoeff *
                                Math.Log(arg));
                            }
                            if (Model.MOS1bulkJctSideGradingCoeff == .5)
                            {
                                sargsw = 1 / Math.Sqrt(arg);
                            }
                            else
                            {
                                sargsw = Math.Exp(-Model.MOS1bulkJctSideGradingCoeff *
                                Math.Log(arg));
                            }
                        }
                        state.States[0][MOS1states + MOS1qbs] =
                        MOS1tBulkPot * (MOS1Cbs *
                        (1 - arg * sarg) / (1 - Model.MOS1bulkJctBotGradingCoeff)
                        + MOS1Cbssw *
                        (1 - arg * sargsw) /
                        (1 - Model.MOS1bulkJctSideGradingCoeff));
                        MOS1capbs = MOS1Cbs * sarg +
                        MOS1Cbssw * sargsw;
                    }
                    else
                    {
                        state.States[0][MOS1states + MOS1qbs] = MOS1f4s +
                        vbs * (MOS1f2s + vbs * (MOS1f3s / 2));
                        MOS1capbs = MOS1f2s + MOS1f3s * vbs;
                    }
                }
                // can't bypass the diode capacitance calculations
                {
                    if (vbd < MOS1tDepCap)
                    {
                        arg = 1 - vbd / MOS1tBulkPot;
                        /*
						* the following block looks somewhat long and messy,
						* but since most users use the default grading
						* coefficients of .5, and sqrt is MUCH faster than an
						* Math.Exp(Math.Log()) we use this special case code to buy time.
						* (as much as 10% of total job time!)
						*/
                        if (Model.MOS1bulkJctBotGradingCoeff == .5 &&
                        Model.MOS1bulkJctSideGradingCoeff == .5)
                        {
                            sarg = sargsw = 1 / Math.Sqrt(arg);
                        }
                        else
                        {
                            if (Model.MOS1bulkJctBotGradingCoeff == .5)
                            {
                                sarg = 1 / Math.Sqrt(arg);
                            }
                            else
                            {
                                sarg = Math.Exp(-Model.MOS1bulkJctBotGradingCoeff *
                                Math.Log(arg));
                            }
                            if (Model.MOS1bulkJctSideGradingCoeff == .5)
                            {
                                sargsw = 1 / Math.Sqrt(arg);
                            }
                            else
                            {
                                sargsw = Math.Exp(-Model.MOS1bulkJctSideGradingCoeff *
                                Math.Log(arg));
                            }
                        }
                        state.States[0][MOS1states + MOS1qbd] =
                        MOS1tBulkPot * (MOS1Cbd *
                        (1 - arg * sarg)
                        / (1 - Model.MOS1bulkJctBotGradingCoeff)
                        + MOS1Cbdsw *
                        (1 - arg * sargsw)
                        / (1 - Model.MOS1bulkJctSideGradingCoeff));
                        MOS1capbd = MOS1Cbd * sarg +
                        MOS1Cbdsw * sargsw;
                    }
                    else
                    {
                        state.States[0][MOS1states + MOS1qbd] = MOS1f4d +
                        vbd * (MOS1f2d + vbd * MOS1f3d / 2);
                        MOS1capbd = MOS1f2d + vbd * MOS1f3d;
                    }
                }

                if (method != null)
                {
                    /* (above only excludes tranop, since we're only at this
					* point if tran or tranop )
					*/

                    /*
					*    calculate equivalent conductances and currents for
					*    depletion capacitors
					*/

                    // integrate the capacitors and save results 
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
			*  check convergence
			*/
            if (!MOS1off || (!state.UseSmallSignal && state.Init != CircuitState.InitFlags.InitFix))
            {
                if (Check == 1)
                {
                    state.IsCon = false;
                }
                else
                {
                    tol = ckt.Simulation.Config.RelTol * Math.Max(Math.Abs(cdhat), Math.Abs(MOS1cd)) + ckt.Simulation.Config.AbsTol;
                    if (Math.Abs(cdhat - MOS1cd) >= tol)
                        state.IsCon = false;
                    else
                    {
                        tol = ckt.Simulation.Config.RelTol * Math.Max(Math.Abs(cbhat), Math.Abs(MOS1cbs + MOS1cbd)) + ckt.Simulation.Config.AbsTol;
                        if (Math.Abs(cbhat - (MOS1cbs + MOS1cbd)) > tol)
                            state.IsCon = false;
                    }
                }
            }

            // save things away for next time 

            state.States[0][MOS1states + MOS1vbs] = vbs;
            state.States[0][MOS1states + MOS1vbd] = vbd;
            state.States[0][MOS1states + MOS1vgs] = vgs;
            state.States[0][MOS1states + MOS1vds] = vds;

            /*
			*     meyer's capacitor model
			*/
            if (state.Domain == CircuitState.DomainTypes.Time || state.UseSmallSignal)
            {
                /*
				*     calculate meyer's capacitors
				*/
                /* 
				* new cmeyer - this just evaluates at the current time,
				* expects you to remember values from previous time
				* returns 1/2 of non-constant portion of capacitance
				* you must add in the other half from previous time
				* and the constant part
				*/
                if (MOS1mode > 0)
                {
                    double icapgs, icapgd, icapgb;
                    Transistor.DEVqmeyer(vgs, vgd, vgb, von, vdsat, out icapgs, out icapgd, out icapgb, MOS1tPhi, OxideCap);
                    state.States[0][MOS1states + MOS1capgs] = icapgs;
                    state.States[0][MOS1states + MOS1capgd] = icapgd;
                    state.States[0][MOS1states + MOS1capgb] = icapgb;
                }
                else
                {
                    double icapgs, icapgd, icapgb;
                    Transistor.DEVqmeyer(vgs, vgd, vgb, von, vdsat, out icapgd, out icapgs, out icapgb, MOS1tPhi, OxideCap);
                    state.States[0][MOS1states + MOS1capgs] = icapgs;
                    state.States[0][MOS1states + MOS1capgd] = icapgd;
                    state.States[0][MOS1states + MOS1capgb] = icapgb;
                }
                vgs1 = state.States[1][MOS1states + MOS1vgs];
                vgd1 = vgs1 - state.States[1][MOS1states + MOS1vds];
                vgb1 = vgs1 - state.States[1][MOS1states + MOS1vbs];
                if (state.UseDC || state.UseSmallSignal)
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

                /*
				*     store small-signal parameters (for meyer's model)
				*  all parameters already stored, so done...
				*/

                if (method != null)
                {
                    state.States[0][MOS1states + MOS1qgs] = (vgs - vgs1) * capgs + state.States[1][MOS1states + MOS1qgs];
                    state.States[0][MOS1states + MOS1qgd] = (vgd - vgd1) * capgd + state.States[1][MOS1states + MOS1qgd];
                    state.States[0][MOS1states + MOS1qgb] = (vgb - vgb1) * capgb + state.States[1][MOS1states + MOS1qgb];
                }
                else
                {
                    // TRANOP only 
                    state.States[0][MOS1states + MOS1qgs] = vgs * capgs;
                    state.States[0][MOS1states + MOS1qgd] = vgd * capgd;
                    state.States[0][MOS1states + MOS1qgb] = vgb * capgb;
                }
            }

            if (method == null)
            {
                /*
				*  initialize to zero charge conductances 
				*  and current
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
				*    calculate equivalent conductances and currents for
				*    meyer"s capacitors
				*/
                method.Integrate(state, ref gcgs, ref ceqgs, MOS1states + MOS1qgs, capgs);
                ceqgs = ceqgs - gcgs * vgs + method.Slope * state.States[0][MOS1states + MOS1qgs];
                method.Integrate(state, ref gcgd, ref ceqgd, MOS1states + MOS1qgd, capgd);
                ceqgd = ceqgd - gcgs * vgd + method.Slope * state.States[0][MOS1states + MOS1qgd];
                method.Integrate(state, ref gcgb, ref ceqgb, MOS1states + MOS1qgb, capgb);
                ceqgb = ceqgb - gcgb * vgb + method.Slope * state.States[0][MOS1states + MOS1qgb];
            }
            /*
			*     store charge storage info for meyer's cap in lx table
			*/

            /*
			*  load current vector
			*/
            ceqbs = Model.MOS1type * (MOS1cbs - (MOS1gbs - state.Gmin) * vbs);
            ceqbd = Model.MOS1type * (MOS1cbd - (MOS1gbd - state.Gmin) * vbd);
            if (MOS1mode >= 0)
            {
                xnrm = 1;
                xrev = 0;
                cdreq = Model.MOS1type * (cdrain - MOS1gds * vds -
                MOS1gm * vgs - MOS1gmbs * vbs);
            }
            else
            {
                xnrm = 0;
                xrev = 1;
                cdreq = -(Model.MOS1type) * (cdrain - MOS1gds * (-vds) -
                MOS1gm * vgd - MOS1gmbs * vbd);
            }

            // load rhs vector
            rstate.Solution[MOS1gNode] -= (Model.MOS1type * (ceqgs + ceqgb + ceqgd));
            rstate.Solution[MOS1bNode] -= (ceqbs + ceqbd - Model.MOS1type * ceqgb);
            rstate.Solution[MOS1dNodePrime] += (ceqbd - cdreq + Model.MOS1type * ceqgd);
            rstate.Solution[MOS1sNodePrime] += cdreq + ceqbs + Model.MOS1type * ceqgs;

            // load y matrix
            rstate.Matrix[MOS1dNode, MOS1dNode] += (MOS1drainConductance);
            rstate.Matrix[MOS1gNode, MOS1gNode] += ((gcgd + gcgs + gcgb));
            rstate.Matrix[MOS1sNode, MOS1sNode] += (MOS1sourceConductance);
            rstate.Matrix[MOS1bNode, MOS1bNode] += (MOS1gbd + MOS1gbs + gcgb);
            rstate.Matrix[MOS1dNodePrime, MOS1dNodePrime] += (MOS1drainConductance + MOS1gds + MOS1gbd + xrev * (MOS1gm + MOS1gmbs) + gcgd);
            rstate.Matrix[MOS1sNodePrime, MOS1sNodePrime] += (MOS1sourceConductance + MOS1gds + MOS1gbs + xnrm * (MOS1gm + MOS1gmbs) + gcgs);
            rstate.Matrix[MOS1dNode, MOS1dNodePrime] += (-MOS1drainConductance);
            rstate.Matrix[MOS1gNode, MOS1bNode] -= gcgb;
            rstate.Matrix[MOS1gNode, MOS1dNodePrime] -= gcgd;
            rstate.Matrix[MOS1gNode, MOS1sNodePrime] -= gcgs;
            rstate.Matrix[MOS1sNode, MOS1sNodePrime] += (-MOS1sourceConductance);
            rstate.Matrix[MOS1bNode, MOS1gNode] -= gcgb;
            rstate.Matrix[MOS1bNode, MOS1dNodePrime] -= MOS1gbd;
            rstate.Matrix[MOS1bNode, MOS1sNodePrime] -= MOS1gbs;
            rstate.Matrix[MOS1dNodePrime, MOS1dNode] += (-MOS1drainConductance);
            rstate.Matrix[MOS1dNodePrime, MOS1gNode] += ((xnrm - xrev) * MOS1gm - gcgd);
            rstate.Matrix[MOS1dNodePrime, MOS1bNode] += (-MOS1gbd + (xnrm - xrev) * MOS1gmbs);
            rstate.Matrix[MOS1dNodePrime, MOS1sNodePrime] += (-MOS1gds - xnrm * (MOS1gm + MOS1gmbs));
            rstate.Matrix[MOS1sNodePrime, MOS1gNode] += (-(xnrm - xrev) * MOS1gm - gcgs);
            rstate.Matrix[MOS1sNodePrime, MOS1sNode] += (-MOS1sourceConductance);
            rstate.Matrix[MOS1sNodePrime, MOS1bNode] += (-MOS1gbs - (xnrm - xrev) * MOS1gmbs);
            rstate.Matrix[MOS1sNodePrime, MOS1dNodePrime] += (-MOS1gds - xrev * (MOS1gm + MOS1gmbs));
        }
    }
}
