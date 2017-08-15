using System;
using SpiceSharp.Circuits;
using SpiceSharp.Diagnostics;
using SpiceSharp.Parameters;
using System.Numerics;
using SpiceSharp.Components.Transistors;

namespace SpiceSharp.Components
{
    public class MOS1 : CircuitComponent<MOS1>
    {
        /// <summary>
        /// Register our parameters
        /// </summary>
        static MOS1()
        {
            Register();
            terminals = new string[] { "Drain", "Gate", "Source", "Bulk" };
        }

        /// <summary>
        /// Gets or sets the device model
        /// </summary>
        public void SetModel(MOS1Model model) => Model = (ICircuitObject)model;

        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("temp"), SpiceInfo("Instance temperature in Kelvin")]
        public Parameter MOS1temp { get; } = new Parameter(300.15);
        [SpiceName("w"), SpiceInfo("Width")]
        public Parameter MOS1w { get; } = new Parameter();
        [SpiceName("l"), SpiceInfo("Length")]
        public Parameter MOS1l { get; } = new Parameter();
        [SpiceName("as"), SpiceInfo("Source area")]
        public Parameter MOS1sourceArea { get; } = new Parameter();
        [SpiceName("ad"), SpiceInfo("Drain area")]
        public Parameter MOS1drainArea { get; } = new Parameter();
        [SpiceName("ps"), SpiceInfo("Source perimeter")]
        public Parameter MOS1sourcePerimiter { get; } = new Parameter();
        [SpiceName("pd"), SpiceInfo("Drain perimeter")]
        public Parameter MOS1drainPerimiter { get; } = new Parameter();
        [SpiceName("nrs"), SpiceInfo("Source squares")]
        public Parameter MOS1sourceSquares { get; } = new Parameter(1);
        [SpiceName("nrd"), SpiceInfo("Drain squares")]
        public Parameter MOS1drainSquares { get; } = new Parameter(1);
        [SpiceName("off"), SpiceInfo("Device initially off")]
        public bool MOS1off { get; set; }
        [SpiceName("icvbs"), SpiceInfo("Initial B-S voltage")]
        public Parameter MOS1icVBS { get; } = new Parameter();
        [SpiceName("icvds"), SpiceInfo("Initial D-S voltage")]
        public Parameter MOS1icVDS { get; } = new Parameter();
        [SpiceName("icvgs"), SpiceInfo("Initial G-S voltage")]
        public Parameter MOS1icVGS { get; } = new Parameter();
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
        [SpiceName("rs"), SpiceInfo("Source resistance")]
        public double GetSOURCERESIST(Circuit ckt)
        {
            if (MOS1sNodePrime != MOS1sNode)
                return 1.0 / MOS1sourceConductance;
            else
                return 0.0;
        }
        [SpiceName("rd"), SpiceInfo("Drain conductance")]
        public double GetDRAINRESIST(Circuit ckt)
        {
            if (MOS1dNodePrime != MOS1dNode)
                return 1.0 / MOS1drainConductance;
            else
                return 0.0;
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
        [SpiceName("ib"), SpiceInfo("Bulk current ")]
        public double GetCB(Circuit ckt) => MOS1cbd + MOS1cbs - ckt.State.States[0][MOS1states + MOS1cqgb];
        [SpiceName("ig"), SpiceInfo("Gate current ")]
        public double GetCG(Circuit ckt)
        {
            if (ckt.Method == null)
                return 0;
            else
                return ckt.State.States[0][MOS1states + MOS1cqgb] + ckt.State.States[0][MOS1states + MOS1cqgd] + ckt.State.States[0][MOS1states + MOS1cqgs];
        }
        [SpiceName("is"), SpiceInfo("Source current")]
        public double GetCS(Circuit ckt)
        {
            double value = -MOS1cd;
            value -= MOS1cbd + MOS1cbs - ckt.State.States[0][MOS1states + MOS1cqgb];
            if (ckt.Method != null)
                value -= ckt.State.States[0][MOS1states + MOS1cqgb] + ckt.State.States[0][MOS1states + MOS1cqgd] + ckt.State.States[0][MOS1states + MOS1cqgs];
            return value;
        }
        [SpiceName("p"), SpiceInfo("Instaneous power")]
        public double GetPOWER(Circuit ckt)
        {
            double temp;

            double value = MOS1cd * ckt.State.Real.Solution[MOS1dNode];
            value += (MOS1cbd + MOS1cbs - ckt.State.States[0][MOS1states + MOS1cqgb]) * ckt.State.Real.Solution[MOS1bNode];
            if (ckt.Method != null)
            {
                value += (ckt.State.States[0][MOS1states + MOS1cqgb] + ckt.State.States[0][MOS1states + MOS1cqgd] +
                    ckt.State.States[0][MOS1states + MOS1cqgs]) * ckt.State.Real.Solution[MOS1gNode];
            }
            temp = -MOS1cd;
            temp -= MOS1cbd + MOS1cbs;
            if (ckt.Method != null)
            {
                temp -= ckt.State.States[0][MOS1states + MOS1cqgb] + ckt.State.States[0][MOS1states + MOS1cqgd] +
                    ckt.State.States[0][MOS1states + MOS1cqgs];
            }
            value += temp * ckt.State.Real.Solution[MOS1sNode];
            return value;
        }

        /// <summary>
        /// Extra variables
        /// </summary>
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
        public int MOS1states { get; private set; }

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

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the device</param>
        public MOS1(string name) : base(name)
        {
        }

        /// <summary>
        /// Setup the device
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Setup(Circuit ckt)
        {
            var model = Model as MOS1Model;

            // Allocate nodes
            var nodes = BindNodes(ckt);
            MOS1dNode = nodes[0].Index;
            MOS1gNode = nodes[1].Index;
            MOS1sNode = nodes[2].Index;
            MOS1bNode = nodes[3].Index;

            // Allocate states
            MOS1states = ckt.State.GetState(17);
            MOS1vdsat = 0;
            MOS1von = 0;

            /* allocate a chunk of the state vector */

            if ((model.MOS1drainResistance != 0 || (model.MOS1sheetResistance != 0 && MOS1drainSquares != 0)) && MOS1dNodePrime == 0)
                MOS1dNodePrime = CreateNode(ckt).Index;
            else
                MOS1dNodePrime = MOS1dNode;

            if ((model.MOS1sourceResistance != 0 || (model.MOS1sheetResistance != 0 && MOS1sourceSquares != 0)) && MOS1sNodePrime == 0)
                MOS1sNodePrime = CreateNode(ckt).Index;
            else
                MOS1sNodePrime = MOS1sNode;
        }

        /// <summary>
        /// Do temperature-dependent calculations
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Temperature(Circuit ckt)
        {
            var model = Model as MOS1Model;
            double vt, ratio, fact2, kt, egfet, arg, pbfact, ratio4, phio, pbo, gmaold, capfact, gmanew, czbd, czbdsw, sarg, sargsw, czbs, czbssw;

            /* perform the parameter defaulting */
            if (!MOS1temp.Given)
                MOS1temp.Value = ckt.State.Temperature;
            vt = MOS1temp * Circuit.CONSTKoverQ;
            ratio = MOS1temp / model.MOS1tnom;
            fact2 = MOS1temp / Circuit.CONSTRefTemp;
            kt = MOS1temp * Circuit.CONSTBoltz;
            egfet = 1.16 - (7.02e-4 * MOS1temp * MOS1temp) /
            (MOS1temp + 1108);
            arg = -egfet / (kt + kt) + 1.1150877 / (Circuit.CONSTBoltz * (Circuit.CONSTRefTemp + Circuit.CONSTRefTemp));
            pbfact = -2 * vt * (1.5 * Math.Log(fact2) + Circuit.CHARGE * arg);

            if (MOS1l - 2 * model.MOS1latDiff <= 0)
                CircuitWarning.Warning(this, $"{Name}: effective channel length less than zero");
            ratio4 = ratio * Math.Sqrt(ratio);
            MOS1tTransconductance = model.MOS1transconductance / ratio4;
            MOS1tSurfMob = model.MOS1surfaceMobility / ratio4;
            phio = (model.MOS1phi - model.pbfact1) / model.fact1;
            MOS1tPhi = fact2 * phio + pbfact;
            MOS1tVbi = model.MOS1vt0 - model.MOS1type * (model.MOS1gamma * Math.Sqrt(model.MOS1phi))
                + .5 * (model.egfet1 - egfet) + model.MOS1type * .5 * (MOS1tPhi - model.MOS1phi);
            MOS1tVto = MOS1tVbi + model.MOS1type * model.MOS1gamma * Math.Sqrt(MOS1tPhi);
            MOS1tSatCur = model.MOS1jctSatCur * Math.Exp(-egfet / vt + model.egfet1 / model.vtnom);
            MOS1tSatCurDens = model.MOS1jctSatCurDensity * Math.Exp(-egfet / vt + model.egfet1 / model.vtnom);
            pbo = (model.MOS1bulkJctPotential - model.pbfact1) / model.fact1;
            gmaold = (model.MOS1bulkJctPotential - pbo) / pbo;
            capfact = 1 / (1 + model.MOS1bulkJctBotGradingCoeff * (4e-4 * (model.MOS1tnom - Circuit.CONSTRefTemp) - gmaold));
            MOS1tCbd = model.MOS1capBD * capfact;
            MOS1tCbs = model.MOS1capBS * capfact;
            MOS1tCj = model.MOS1bulkCapFactor * capfact;
            capfact = 1 / (1 + model.MOS1bulkJctSideGradingCoeff * (4e-4 * (model.MOS1tnom - Circuit.CONSTRefTemp) - gmaold));
            MOS1tCjsw = model.MOS1sideWallCapFactor * capfact;
            MOS1tBulkPot = fact2 * pbo + pbfact;
            gmanew = (MOS1tBulkPot - pbo) / pbo;
            capfact = (1 + model.MOS1bulkJctBotGradingCoeff * (4e-4 * (MOS1temp - Circuit.CONSTRefTemp) - gmanew));
            MOS1tCbd *= capfact;
            MOS1tCbs *= capfact;
            MOS1tCj *= capfact;
            capfact = (1 + model.MOS1bulkJctSideGradingCoeff * (4e-4 * (MOS1temp - Circuit.CONSTRefTemp) - gmanew));
            MOS1tCjsw *= capfact;
            MOS1tDepCap = model.MOS1fwdCapDepCoeff * MOS1tBulkPot;
            if ((MOS1tSatCurDens == 0) || (MOS1drainArea == 0) || (MOS1sourceArea == 0))
            {
                MOS1sourceVcrit = MOS1drainVcrit = vt * Math.Log(vt / (Circuit.CONSTroot2 * MOS1tSatCur));
            }
            else
            {
                MOS1drainVcrit = vt * Math.Log(vt / (Circuit.CONSTroot2 * MOS1tSatCurDens * MOS1drainArea));
                MOS1sourceVcrit = vt * Math.Log(vt / (Circuit.CONSTroot2 * MOS1tSatCurDens * MOS1sourceArea));
            }

            if (model.MOS1capBD.Given)
            {
                czbd = MOS1tCbd;
            }
            else
            {
                if (model.MOS1bulkCapFactor.Given)
                {
                    czbd = MOS1tCj * MOS1drainArea;
                }
                else
                {
                    czbd = 0;
                }
            }
            if (model.MOS1sideWallCapFactor.Given)
            {
                czbdsw = MOS1tCjsw * MOS1drainPerimiter;
            }
            else
            {
                czbdsw = 0;
            }
            arg = 1 - model.MOS1fwdCapDepCoeff;
            sarg = Math.Exp((-model.MOS1bulkJctBotGradingCoeff) * Math.Log(arg));
            sargsw = Math.Exp((-model.MOS1bulkJctSideGradingCoeff) * Math.Log(arg));
            MOS1Cbd = czbd;
            MOS1Cbdsw = czbdsw;
            MOS1f2d = czbd * (1 - model.MOS1fwdCapDepCoeff * (1 + model.MOS1bulkJctBotGradingCoeff)) * sarg / arg
                + czbdsw * (1 - model.MOS1fwdCapDepCoeff * (1 + model.MOS1bulkJctSideGradingCoeff)) * sargsw / arg;
            MOS1f3d = czbd * model.MOS1bulkJctBotGradingCoeff * sarg / arg / MOS1tBulkPot
                + czbdsw * model.MOS1bulkJctSideGradingCoeff * sargsw / arg / MOS1tBulkPot;
            MOS1f4d = czbd * MOS1tBulkPot * (1 - arg * sarg) / (1 - model.MOS1bulkJctBotGradingCoeff)
                + czbdsw * MOS1tBulkPot * (1 - arg * sargsw) / (1 - model.MOS1bulkJctSideGradingCoeff)
                - MOS1f3d / 2 * (MOS1tDepCap * MOS1tDepCap) - MOS1tDepCap * MOS1f2d;
            if (model.MOS1capBS.Given)
            {
                czbs = MOS1tCbs;
            }
            else
            {
                if (model.MOS1bulkCapFactor.Given)
                {
                    czbs = MOS1tCj * MOS1sourceArea;
                }
                else
                {
                    czbs = 0;
                }
            }
            if (model.MOS1sideWallCapFactor.Given)
            {
                czbssw = MOS1tCjsw * MOS1sourcePerimiter;
            }
            else
            {
                czbssw = 0;
            }
            arg = 1 - model.MOS1fwdCapDepCoeff;
            sarg = Math.Exp((-model.MOS1bulkJctBotGradingCoeff) * Math.Log(arg));
            sargsw = Math.Exp((-model.MOS1bulkJctSideGradingCoeff) * Math.Log(arg));
            MOS1Cbs = czbs;
            MOS1Cbssw = czbssw;
            MOS1f2s = czbs * (1 - model.MOS1fwdCapDepCoeff * (1 + model.MOS1bulkJctBotGradingCoeff)) * sarg / arg
                + czbssw * (1 - model.MOS1fwdCapDepCoeff * (1 + model.MOS1bulkJctSideGradingCoeff)) * sargsw / arg;
            MOS1f3s = czbs * model.MOS1bulkJctBotGradingCoeff * sarg / arg / MOS1tBulkPot
                + czbssw * model.MOS1bulkJctSideGradingCoeff * sargsw / arg / MOS1tBulkPot;
            MOS1f4s = czbs * MOS1tBulkPot * (1 - arg * sarg) / (1 - model.MOS1bulkJctBotGradingCoeff)
                + czbssw * MOS1tBulkPot * (1 - arg * sargsw) / (1 - model.MOS1bulkJctSideGradingCoeff)
                - MOS1f3s / 2 * (MOS1tDepCap * MOS1tDepCap) - MOS1tDepCap * MOS1f2s;

            if (model.MOS1drainResistance.Given)
            {
                if (model.MOS1drainResistance != 0)
                {
                    MOS1drainConductance = 1 / model.MOS1drainResistance;
                }
                else
                {
                    MOS1drainConductance = 0;
                }
            }
            else if (model.MOS1sheetResistance.Given)
            {
                if (model.MOS1sheetResistance != 0)
                {
                    MOS1drainConductance =
                    1 / (model.MOS1sheetResistance * MOS1drainSquares);
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
            if (model.MOS1sourceResistance.Given)
            {
                if (model.MOS1sourceResistance != 0)
                {
                    MOS1sourceConductance = 1 / model.MOS1sourceResistance;
                }
                else
                {
                    MOS1sourceConductance = 0;
                }
            }
            else if (model.MOS1sheetResistance.Given)
            {
                if (model.MOS1sheetResistance != 0)
                {
                    MOS1sourceConductance =
                    1 / (model.MOS1sheetResistance * MOS1sourceSquares);
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
            var model = Model as MOS1Model;
            var state = ckt.State;
            var rstate = state.Real;
            var method = ckt.Method;
            double vt;
            int Check;
            double EffectiveLength, DrainSatCur, SourceSatCur, GateSourceOverlapCap, GateDrainOverlapCap, GateBulkOverlapCap, Beta, 
                OxideCap, vgs, vds, vbs, vbd, vgb, vgd, vgdo, delvbs, delvbd, delvgs, delvds, delvgd, cdhat, cbhat, von, evbs, evbd, 
                sarg, vdsat, arg, cdrain, sargsw;
            double vgs1, vgd1, vgb1, capgs = 0.0, capgd = 0.0, capgb = 0.0, gcgs, ceqgs, gcgd, ceqgd, gcgb, ceqgb, ceqbs, ceqbd;
            int xnrm;
            int xrev;
            double cdreq;

            vt = Circuit.CONSTKoverQ * MOS1temp;
            Check = 1;
            /*DETAILPROF*/

            /* first, we compute a few useful values - these could be
			* pre-computed, but for historical reasons are still done
			* here.  They may be moved at the expense of instance size
			*/

            EffectiveLength = MOS1l - 2 * model.MOS1latDiff;
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
            GateSourceOverlapCap = model.MOS1gateSourceOverlapCapFactor * MOS1w;
            GateDrainOverlapCap = model.MOS1gateDrainOverlapCapFactor * MOS1w;
            GateBulkOverlapCap = model.MOS1gateBulkOverlapCapFactor * EffectiveLength;
            Beta = MOS1tTransconductance * MOS1w / EffectiveLength;
            OxideCap = model.MOS1oxideCapFactor * EffectiveLength * MOS1w;

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
                /* PREDICTOR */

                /* general iteration */

                vbs = model.MOS1type * (rstate.OldSolution[MOS1bNode] - rstate.OldSolution[MOS1sNodePrime]);
                vgs = model.MOS1type * (rstate.OldSolution[MOS1gNode] - rstate.OldSolution[MOS1sNodePrime]);
                vds = model.MOS1type * (rstate.OldSolution[MOS1dNodePrime] - rstate.OldSolution[MOS1sNodePrime]);
                /* PREDICTOR */

                /* now some common crunching for some more useful quantities */

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
                    cdhat = MOS1cd - MOS1gbd * delvbd + MOS1gmbs * delvbs + MOS1gm * delvgs + MOS1gds * delvds;
                else
                    cdhat = MOS1cd - (MOS1gbd - MOS1gmbs) * delvbd - MOS1gm * delvgd + MOS1gds * delvds;
                cbhat = MOS1cbs + MOS1cbd + MOS1gbd * delvbd + MOS1gbs * delvbs;

                von = model.MOS1type * MOS1von;

                /* 
				* limiting
				*  we want to keep device voltages from changing
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
                    vds = model.MOS1type * MOS1icVDS;
                    vgs = model.MOS1type * MOS1icVGS;
                    vbs = model.MOS1type * MOS1icVBS;
                    if ((vds == 0) && (vgs == 0) && (vbs == 0) &&
                        (method != null || state.UseDC || state.Domain == CircuitState.DomainTypes.None || !state.UseIC))
                    {
                        vbs = -1;
                        vgs = model.MOS1type * MOS1tVto;
                        vds = 0;
                    }
                }
                else
                {
                    vbs = vgs = vds = 0;
                }
            }

            /*DETAILPROF*/

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
            /*
			
			*/

            /*DETAILPROF*/
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
                // double arg;
                double betap;
                // double sarg;
                double vgst;

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
                von = (MOS1tVbi * model.MOS1type) + model.MOS1gamma * sarg;
                vgst = (MOS1mode == 1 ? vgs : vgd) - von;
                vdsat = Math.Max(vgst, 0);
                if (sarg <= 0)
                {
                    arg = 0;
                }
                else
                {
                    arg = model.MOS1gamma / (sarg + sarg);
                }
                if (vgst <= 0)
                {
                    /*
					*     cutoff region
					*/
                    cdrain = 0;
                    MOS1gm = 0;
                    MOS1gds = 0;
                    MOS1gmbs = 0;
                }
                else
                {
                    /*
					*     saturation region
					*/
                    betap = Beta * (1 + model.MOS1lambda * (vds * MOS1mode));
                    if (vgst <= (vds * MOS1mode))
                    {
                        cdrain = betap * vgst * vgst * .5;
                        MOS1gm = betap * vgst;
                        MOS1gds = model.MOS1lambda * Beta * vgst * vgst * .5;
                        MOS1gmbs = MOS1gm * arg;
                    }
                    else
                    {
                        /*
						*     linear region
						*/
                        cdrain = betap * (vds * MOS1mode) *
                        (vgst - .5 * (vds * MOS1mode));
                        MOS1gm = betap * (vds * MOS1mode);
                        MOS1gds = betap * (vgst - (vds * MOS1mode)) +
                        model.MOS1lambda * Beta *
                        (vds * MOS1mode) *
                        (vgst - .5 * (vds * MOS1mode));
                        MOS1gmbs = MOS1gm * arg;
                    }
                }
                /*
				*     finished
				*/
            }
            /*
			
			*/

            /*DETAILPROF*/

            /* now deal with n vs p polarity */

            MOS1von = model.MOS1type * von;
            MOS1vdsat = model.MOS1type * vdsat;
            /* line 490 */
            /*
			*  COMPUTE EQUIVALENT DRAIN CURRENT SOURCE
			*/
            MOS1cd = MOS1mode * cdrain - MOS1cbd;

            if (state.Domain == CircuitState.DomainTypes.Time || state.UseSmallSignal)
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
                /*CAPBYPASS*/
                {
                    /* can't bypass the diode capacitance calculations */
                    /*CAPZEROBYPASS*/
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
                        if (model.MOS1bulkJctBotGradingCoeff == model.MOS1bulkJctSideGradingCoeff)
                        {
                            if (model.MOS1bulkJctBotGradingCoeff == .5)
                            {
                                sarg = sargsw = 1 / Math.Sqrt(arg);
                            }
                            else
                            {
                                sarg = sargsw = Math.Exp(-model.MOS1bulkJctBotGradingCoeff * Math.Log(arg));
                            }
                        }
                        else
                        {
                            if (model.MOS1bulkJctBotGradingCoeff == .5)
                            {
                                sarg = 1 / Math.Sqrt(arg);
                            }
                            else
                            {
                                /*NOSQRT*/
                                sarg = Math.Exp(-model.MOS1bulkJctBotGradingCoeff * Math.Log(arg));
                            }
                            if (model.MOS1bulkJctSideGradingCoeff == .5)
                            {
                                sargsw = 1 / Math.Sqrt(arg);
                            }
                            else
                            {
                                /*NOSQRT*/
                                sargsw = Math.Exp(-model.MOS1bulkJctSideGradingCoeff * Math.Log(arg));
                            }
                        }
                        /*NOSQRT*/
                        state.States[0][MOS1states + MOS1qbs] = MOS1tBulkPot * (MOS1Cbs *
                            (1 - arg * sarg) / (1 - model.MOS1bulkJctBotGradingCoeff) + MOS1Cbssw *
                            (1 - arg * sargsw) / (1 - model.MOS1bulkJctSideGradingCoeff));
                            MOS1capbs = MOS1Cbs * sarg + MOS1Cbssw * sargsw;
                    }
                    else
                    {
                        state.States[0][MOS1states + MOS1qbs] = MOS1f4s + vbs * (MOS1f2s + vbs * (MOS1f3s / 2));
                        MOS1capbs = MOS1f2s + MOS1f3s * vbs;
                    }
                }

                /* can't bypass the diode capacitance calculations */
                {
                    /*CAPZEROBYPASS*/
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
                        if (model.MOS1bulkJctBotGradingCoeff == .5 && model.MOS1bulkJctSideGradingCoeff == .5)
                        {
                            sarg = sargsw = 1 / Math.Sqrt(arg);
                        }
                        else
                        {
                            if (model.MOS1bulkJctBotGradingCoeff == .5)
                            {
                                sarg = 1 / Math.Sqrt(arg);
                            }
                            else
                            {
                                /*NOSQRT*/
                                sarg = Math.Exp(-model.MOS1bulkJctBotGradingCoeff * Math.Log(arg));
                            }
                            if (model.MOS1bulkJctSideGradingCoeff == .5)
                            {
                                sargsw = 1 / Math.Sqrt(arg);
                            }
                            else
                            {
                                /*NOSQRT*/
                                sargsw = Math.Exp(-model.MOS1bulkJctSideGradingCoeff * Math.Log(arg));
                            }
                        }
                        /*NOSQRT*/
                        state.States[0][MOS1states + MOS1qbd] = MOS1tBulkPot * (MOS1Cbd *
                            (1 - arg * sarg) / (1 - model.MOS1bulkJctBotGradingCoeff)
                            + MOS1Cbdsw * (1 - arg * sargsw) / (1 - model.MOS1bulkJctSideGradingCoeff));
                        MOS1capbd = MOS1Cbd * sarg + MOS1Cbdsw * sargsw;
                    }
                    else
                    {
                        state.States[0][MOS1states + MOS1qbd] = MOS1f4d +
                        vbd * (MOS1f2d + vbd * MOS1f3d / 2);
                        MOS1capbd = MOS1f2d + vbd * MOS1f3d;
                    }
                    /*CAPZEROBYPASS*/
                }
                /*
				
				*/

                /*DETAILPROF*/

                if (method != null || (method == null && method.SavedTime == 0.0 && !state.UseIC))
                {
                    /* (above only excludes tranop, since we're only at this
					 * point if tran or tranop )
					 */

                    /*
					 *    calculate equivalent conductances and currents for
					 *    depletion capacitors
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

            /*DETAILPROF*/

            /*
			*  check convergence
			*/
            if (MOS1off || !(state.Init == CircuitState.InitFlags.Init || state.UseSmallSignal))
            {
                if (Check == 1)
                    state.IsCon = false;
            }

            /*DETAILPROF*/

            /* save things away for next time */

            state.States[0][MOS1states + MOS1vbs] = vbs;
            state.States[0][MOS1states + MOS1vbd] = vbd;
            state.States[0][MOS1states + MOS1vgs] = vgs;
            state.States[0][MOS1states + MOS1vds] = vds;

            /*DETAILPROF*/
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
                double icapgs, icapgd, icapgb;
                if (MOS1mode > 0)
                    Transistor.DEVqmeyer(vgs, vgd, vgb, von, vdsat, out icapgs, out icapgd, out icapgb, MOS1tPhi, OxideCap);
                else
                    Transistor.DEVqmeyer(vgd, vgs, vgb, von, vdsat, out icapgd, out icapgs, out icapgb, MOS1tPhi, OxideCap);
                vgs1 = state.States[1][MOS1states + MOS1vgs];
                vgd1 = vgs1 - state.States[1][MOS1states + MOS1vds];
                vgb1 = vgs1 - state.States[1][MOS1states + MOS1vbs];
                if ((state.Domain == CircuitState.DomainTypes.Time && state.UseDC) || state.UseSmallSignal)
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

                /*DETAILPROF*/
                /*
				*     store small-signal parameters (for meyer's model)
				*  all parameters already stored, so done...
				*/

                /*PREDICTOR*/
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
                /*PREDICTOR*/
            }

            if ((method != null && method.SavedTime == 0.0) || method == null)
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
                method.Integrate(state, out gcgs, out ceqgs, MOS1states + MOS1qgs, capgs);
                method.Integrate(state, out gcgd, out ceqgd, MOS1states + MOS1qgd, capgd);
                method.Integrate(state, out gcgb, out ceqgb, MOS1states + MOS1qgb, capgb);
                ceqgs = ceqgs - gcgs * vgs + method.Slope * state.States[0][MOS1states + MOS1qgs];
                ceqgd = ceqgd - gcgd * vgd + method.Slope * state.States[0][MOS1states + MOS1qgd];
                ceqgb = ceqgb - gcgb * vgb + method.Slope * state.States[0][MOS1states + MOS1qgb];
            }
            /*
			*     store charge storage info for meyer's cap in lx table
			*/

            /*
			*  load current vector
			*/
            ceqbs = model.MOS1type * (MOS1cbs - (MOS1gbs - state.Gmin) * vbs);
            ceqbd = model.MOS1type * (MOS1cbd - (MOS1gbd - state.Gmin) * vbd);
            if (MOS1mode >= 0)
            {
                xnrm = 1;
                xrev = 0;
                cdreq = model.MOS1type * (cdrain - MOS1gds * vds - MOS1gm * vgs - MOS1gmbs * vbs);
            }
            else
            {
                xnrm = 0;
                xrev = 1;
                cdreq = -(model.MOS1type) * (cdrain - MOS1gds * (-vds) - MOS1gm * vgd - MOS1gmbs * vbd);
            }

            /*
             * Load rhs matrix
             */
            rstate.Rhs[MOS1gNode] -= (model.MOS1type * (ceqgs + ceqgb + ceqgd));
            rstate.Rhs[MOS1bNode] -= (ceqbs + ceqbd - model.MOS1type * ceqgb);
            rstate.Rhs[MOS1dNodePrime] += (ceqbd - cdreq + model.MOS1type * ceqgd);
            rstate.Rhs[MOS1sNodePrime] += cdreq + ceqbs + model.MOS1type * ceqgs;

            /*
			 *  load y matrix
			 */
            rstate.Matrix[MOS1dNode, MOS1dNode] += MOS1drainConductance;
            rstate.Matrix[MOS1gNode, MOS1gNode] += gcgd + gcgs + gcgb;
            rstate.Matrix[MOS1sNode, MOS1sNode] += MOS1sourceConductance;
            rstate.Matrix[MOS1bNode, MOS1bNode] += MOS1gbd + MOS1gbs + gcgb;
            rstate.Matrix[MOS1dNodePrime, MOS1dNodePrime] += MOS1drainConductance + MOS1gds + MOS1gbd + xrev * (MOS1gm + MOS1gmbs) + gcgd;
            rstate.Matrix[MOS1sNodePrime, MOS1sNodePrime] += MOS1sourceConductance + MOS1gds + MOS1gbs + xnrm * (MOS1gm + MOS1gmbs) + gcgs;
            rstate.Matrix[MOS1dNode, MOS1dNodePrime] += -MOS1drainConductance;
            rstate.Matrix[MOS1gNode, MOS1bNode] -= gcgb;
            rstate.Matrix[MOS1gNode, MOS1dNodePrime] -= gcgd;
            rstate.Matrix[MOS1gNode, MOS1sNodePrime] -= gcgs;
            rstate.Matrix[MOS1sNode, MOS1sNodePrime] += -MOS1sourceConductance;
            rstate.Matrix[MOS1bNode, MOS1gNode] -= gcgb;
            rstate.Matrix[MOS1bNode, MOS1dNodePrime] -= MOS1gbd;
            rstate.Matrix[MOS1bNode, MOS1sNodePrime] -= MOS1gbs;
            rstate.Matrix[MOS1dNodePrime, MOS1dNode] += -MOS1drainConductance;
            rstate.Matrix[MOS1dNodePrime, MOS1gNode] += (xnrm - xrev) * MOS1gm - gcgd;
            rstate.Matrix[MOS1dNodePrime, MOS1bNode] += -MOS1gbd + (xnrm - xrev) * MOS1gmbs;
            rstate.Matrix[MOS1dNodePrime, MOS1sNodePrime] += -MOS1gds - xnrm * (MOS1gm + MOS1gmbs);
            rstate.Matrix[MOS1sNodePrime, MOS1gNode] += -(xnrm - xrev) * MOS1gm - gcgs;
            rstate.Matrix[MOS1sNodePrime, MOS1sNode] += -MOS1sourceConductance;
            rstate.Matrix[MOS1sNodePrime, MOS1bNode] += -MOS1gbs - (xnrm - xrev) * MOS1gmbs;
            rstate.Matrix[MOS1sNodePrime, MOS1dNodePrime] += -MOS1gds - xrev * (MOS1gm + MOS1gmbs);
        }

        /// <summary>
        /// Load the device for AC simulation
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void AcLoad(Circuit ckt)
        {
            var model = Model as MOS1Model;
            var state = ckt.State;
            var cstate = state.Complex;
            int xnrm;
            int xrev;
            double EffectiveLength;
            double GateSourceOverlapCap;
            double GateDrainOverlapCap;
            double GateBulkOverlapCap;
            double capgs;
            double capgd;
            double capgb;
            Complex xgs;
            Complex xgd;
            Complex xgb;
            Complex xbd;
            Complex xbs;

            if (MOS1mode < 0)
            {
                xnrm = 0;
                xrev = 1;
            }
            else
            {
                xnrm = 1;
                xrev = 0;
            }

            /*
			 *     meyer's model parameters
			 */
            EffectiveLength = MOS1l - 2 * model.MOS1latDiff;
            GateSourceOverlapCap = model.MOS1gateSourceOverlapCapFactor * MOS1w;
            GateDrainOverlapCap = model.MOS1gateDrainOverlapCapFactor * MOS1w;
            GateBulkOverlapCap = model.MOS1gateBulkOverlapCapFactor * EffectiveLength;
            capgs = (state.States[0][MOS1states + MOS1capgs] + state.States[0][MOS1states + MOS1capgs] + GateSourceOverlapCap);
            capgd = (state.States[0][MOS1states + MOS1capgd] + state.States[0][MOS1states + MOS1capgd] + GateDrainOverlapCap);
            capgb = (state.States[0][MOS1states + MOS1capgb] + state.States[0][MOS1states + MOS1capgb] + GateBulkOverlapCap);
            xgs = capgs * cstate.Laplace;
            xgd = capgd * cstate.Laplace;
            xgb = capgb * cstate.Laplace;
            xbd = MOS1capbd * cstate.Laplace;
            xbs = MOS1capbs * cstate.Laplace;

            /*
			 *    load matrix
			 */
            cstate.Matrix[MOS1gNode, MOS1gNode] += xgd + xgs + xgb;
            cstate.Matrix[MOS1bNode, MOS1bNode] += xgb + xbd + xbs;
            cstate.Matrix[MOS1dNodePrime, MOS1dNodePrime] += xgd + xbd;
            cstate.Matrix[MOS1sNodePrime, MOS1sNodePrime] += xgs + xbs;
            cstate.Matrix[MOS1gNode, MOS1bNode] -= xgb;
            cstate.Matrix[MOS1gNode, MOS1dNodePrime] -= xgd;
            cstate.Matrix[MOS1gNode, MOS1sNodePrime] -= xgs;
            cstate.Matrix[MOS1bNode, MOS1gNode] -= xgb;
            cstate.Matrix[MOS1bNode, MOS1dNodePrime] -= xbd;
            cstate.Matrix[MOS1bNode, MOS1sNodePrime] -= xbs;
            cstate.Matrix[MOS1dNodePrime, MOS1gNode] -= xgd;
            cstate.Matrix[MOS1dNodePrime, MOS1bNode] -= xbd;
            cstate.Matrix[MOS1sNodePrime, MOS1gNode] -= xgs;
            cstate.Matrix[MOS1sNodePrime, MOS1bNode] -= xbs;
            cstate.Matrix[MOS1dNode, MOS1dNode] += MOS1drainConductance;
            cstate.Matrix[MOS1sNode, MOS1sNode] += MOS1sourceConductance;
            cstate.Matrix[MOS1bNode, MOS1bNode] += MOS1gbd + MOS1gbs + xgb + xbd + xbs;
            cstate.Matrix[MOS1dNodePrime, MOS1dNodePrime] += MOS1drainConductance + MOS1gds + MOS1gbd + xrev * (MOS1gm + MOS1gmbs) + xgd + xbd;
            cstate.Matrix[MOS1sNodePrime, MOS1sNodePrime] += MOS1sourceConductance + MOS1gds + MOS1gbs + xnrm * (MOS1gm + MOS1gmbs) + xgs + xbs;
            cstate.Matrix[MOS1dNode, MOS1dNodePrime] -= MOS1drainConductance;
            cstate.Matrix[MOS1sNode, MOS1sNodePrime] -= MOS1sourceConductance;
            cstate.Matrix[MOS1bNode, MOS1dNodePrime] -= MOS1gbd + xbd;
            cstate.Matrix[MOS1bNode, MOS1sNodePrime] -= MOS1gbs + xbs;
            cstate.Matrix[MOS1dNodePrime, MOS1dNode] -= MOS1drainConductance;
            cstate.Matrix[MOS1dNodePrime, MOS1gNode] += (xnrm - xrev) * MOS1gm - xgd;
            cstate.Matrix[MOS1dNodePrime, MOS1bNode] += -MOS1gbd + (xnrm - xrev) * MOS1gmbs - xbd;
            cstate.Matrix[MOS1dNodePrime, MOS1sNodePrime] -= MOS1gds + xnrm * (MOS1gm + MOS1gmbs);
            cstate.Matrix[MOS1sNodePrime, MOS1gNode] -= (xnrm - xrev) * MOS1gm + xgs;
            cstate.Matrix[MOS1sNodePrime, MOS1sNode] -= MOS1sourceConductance;
            cstate.Matrix[MOS1sNodePrime, MOS1bNode] -= MOS1gbs + (xnrm - xrev) * MOS1gmbs + xbs;
            cstate.Matrix[MOS1sNodePrime, MOS1dNodePrime] -= MOS1gds + xrev * (MOS1gm + MOS1gmbs);
        }
    }
}
