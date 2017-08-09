using System;
using SpiceSharp.Circuits;
using SpiceSharp.Diagnostics;
using SpiceSharp.Parameters;
using System.Numerics;
using SpiceSharp.Components.Transistors;

namespace SpiceSharp.Components
{
    public class MOS2 : CircuitComponent
    {
        /// <summary>
        /// Gets or sets the device model
        /// </summary>
        public MOS2Model Model { get; set; }

        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("temp"), SpiceInfo("Instance operating temperature")]
        public ParameterMethod<double> MOS2temp { get; } = new ParameterMethod<double>(300.15, (double celsius) => celsius + Circuit.CONSTCtoK, (double kelvin) => kelvin - Circuit.CONSTCtoK);
        [SpiceName("w"), SpiceInfo("Width")]
        public Parameter<double> MOS2w { get; } = new Parameter<double>();
        [SpiceName("l"), SpiceInfo("Length")]
        public Parameter<double> MOS2l { get; } = new Parameter<double>();
        [SpiceName("as"), SpiceInfo("Source area")]
        public Parameter<double> MOS2sourceArea { get; } = new Parameter<double>();
        [SpiceName("ad"), SpiceInfo("Drain area")]
        public Parameter<double> MOS2drainArea { get; } = new Parameter<double>();
        [SpiceName("ps"), SpiceInfo("Source perimeter")]
        public Parameter<double> MOS2sourcePerimiter { get; } = new Parameter<double>();
        [SpiceName("pd"), SpiceInfo("Drain perimeter")]
        public Parameter<double> MOS2drainPerimiter { get; } = new Parameter<double>();
        [SpiceName("nrs"), SpiceInfo("Source squares")]
        public Parameter<double> MOS2sourceSquares { get; } = new Parameter<double>(1);
        [SpiceName("nrd"), SpiceInfo("Drain squares")]
        public Parameter<double> MOS2drainSquares { get; } = new Parameter<double>(1);
        [SpiceName("off"), SpiceInfo("Device initially off")]
        public bool MOS2off { get; set; }
        [SpiceName("icvbs"), SpiceInfo("Initial B-S voltage")]
        public Parameter<double> MOS2icVBS { get; } = new Parameter<double>();
        [SpiceName("icvds"), SpiceInfo("Initial D-S voltage")]
        public Parameter<double> MOS2icVDS { get; } = new Parameter<double>();
        [SpiceName("icvgs"), SpiceInfo("Initial G-S voltage")]
        public Parameter<double> MOS2icVGS { get; } = new Parameter<double>();
        [SpiceName("dnode"), SpiceInfo("Number of drain node")]
        public int MOS2dNode { get; private set; }
        [SpiceName("gnode"), SpiceInfo("Number of gate node")]
        public int MOS2gNode { get; private set; }
        [SpiceName("snode"), SpiceInfo("Number of source node")]
        public int MOS2sNode { get; private set; }
        [SpiceName("bnode"), SpiceInfo("Number of bulk node")]
        public int MOS2bNode { get; private set; }
        [SpiceName("dnodeprime"), SpiceInfo("Number of internal drain node")]
        public int MOS2dNodePrime { get; private set; }
        [SpiceName("snodeprime"), SpiceInfo("Number of internal source node")]
        public int MOS2sNodePrime { get; private set; }
        [SpiceName("sourceconductance"), SpiceInfo("Source conductance")]
        public double MOS2sourceConductance { get; private set; }
        [SpiceName("drainconductance"), SpiceInfo("Drain conductance")]
        public double MOS2drainConductance { get; private set; }
        [SpiceName("von"), SpiceInfo(" ")]
        public double MOS2von { get; private set; }
        [SpiceName("vdsat"), SpiceInfo("Saturation drain voltage")]
        public double MOS2vdsat { get; private set; }
        [SpiceName("sourcevcrit"), SpiceInfo("Critical source voltage")]
        public double MOS2sourceVcrit { get; private set; }
        [SpiceName("drainvcrit"), SpiceInfo("Critical drain voltage")]
        public double MOS2drainVcrit { get; private set; }
        [SpiceName("id"), SpiceName("cd"), SpiceInfo("Drain current")]
        public double MOS2cd { get; private set; }
        [SpiceName("ibs"), SpiceInfo("B-S junction current")]
        public double MOS2cbs { get; private set; }
        [SpiceName("ibd"), SpiceInfo("B-D junction current")]
        public double MOS2cbd { get; private set; }
        [SpiceName("gmb"), SpiceName("gmbs"), SpiceInfo("Bulk-Source transconductance")]
        public double MOS2gmbs { get; private set; }
        [SpiceName("gm"), SpiceInfo("Transconductance")]
        public double MOS2gm { get; private set; }
        [SpiceName("gds"), SpiceInfo("Drain-Source conductance")]
        public double MOS2gds { get; private set; }
        [SpiceName("gbd"), SpiceInfo("Bulk-Drain conductance")]
        public double MOS2gbd { get; private set; }
        [SpiceName("gbs"), SpiceInfo("Bulk-Source conductance")]
        public double MOS2gbs { get; private set; }
        [SpiceName("cbd"), SpiceInfo("Bulk-Drain capacitance")]
        public double MOS2capbd { get; private set; }
        [SpiceName("cbs"), SpiceInfo("Bulk-Source capacitance")]
        public double MOS2capbs { get; private set; }
        [SpiceName("cbd0"), SpiceInfo("Zero-Bias B-D junction capacitance")]
        public double MOS2Cbd { get; private set; }
        [SpiceName("cbdsw0"), SpiceInfo(" ")]
        public double MOS2Cbdsw { get; private set; }
        [SpiceName("cbs0"), SpiceInfo("Zero-Bias B-S junction capacitance")]
        public double MOS2Cbs { get; private set; }
        [SpiceName("cbssw0"), SpiceInfo(" ")]
        public double MOS2Cbssw { get; private set; }

        /// <summary>
        /// Methods
        /// </summary>
        [SpiceName("ic"), SpiceInfo("Vector of D-S, G-S, B-S voltages")]
        public void SetIC(double[] value)
        {
            switch (value.Length)
            {
                case 3: MOS2icVBS.Set(value[2]); goto case 2;
                case 2: MOS2icVGS.Set(value[1]); goto case 1;
                case 1: MOS2icVDS.Set(value[0]); break;
                default:
                    throw new CircuitException("Bad parameter");
            }
        }
        [SpiceName("rs"), SpiceInfo("Source resistance")]
        public double GetSOURCERESIST(Circuit ckt)
        {
            if (MOS2sNodePrime != MOS2sNode)
                return 1.0 / MOS2sourceConductance;
            else
                return 0.0;
        }
        [SpiceName("rd"), SpiceInfo("Drain resistance")]
        public double GetDRAINRESIST(Circuit ckt)
        {
            if (MOS2dNodePrime != MOS2dNode)
                return 1.0 / MOS2drainConductance;
            else
                return 0.0;
        }
        [SpiceName("vbd"), SpiceInfo("Bulk-Drain voltage")]
        public double GetVBD(Circuit ckt) => ckt.State.States[0][MOS2states + MOS2vbd];
        [SpiceName("vbs"), SpiceInfo("Bulk-Source voltage")]
        public double GetVBS(Circuit ckt) => ckt.State.States[0][MOS2states + MOS2vbs];
        [SpiceName("vgs"), SpiceInfo("Gate-Source voltage")]
        public double GetVGS(Circuit ckt) => ckt.State.States[0][MOS2states + MOS2vgs];
        [SpiceName("vds"), SpiceInfo("Drain-Source voltage")]
        public double GetVDS(Circuit ckt) => ckt.State.States[0][MOS2states + MOS2vds];
        [SpiceName("cgs"), SpiceInfo("Gate-Source capacitance")]
        public double GetCAPGS(Circuit ckt) => ckt.State.States[0][MOS2states + MOS2capgs];
        [SpiceName("qgs"), SpiceInfo("Gate-Source charge storage")]
        public double GetQGS(Circuit ckt) => ckt.State.States[0][MOS2states + MOS2qgs];
        [SpiceName("cqgs"), SpiceInfo("Capacitance due to gate-source charge storage")]
        public double GetCQGS(Circuit ckt) => ckt.State.States[0][MOS2states + MOS2cqgs];
        [SpiceName("cgd"), SpiceInfo("Gate-Drain capacitance")]
        public double GetCAPGD(Circuit ckt) => ckt.State.States[0][MOS2states + MOS2capgd];
        [SpiceName("qgd"), SpiceInfo("Gate-Drain charge storage")]
        public double GetQGD(Circuit ckt) => ckt.State.States[0][MOS2states + MOS2qgd];
        [SpiceName("cqgd"), SpiceInfo("Capacitance due to gate-drain charge storage")]
        public double GetCQGD(Circuit ckt) => ckt.State.States[0][MOS2states + MOS2cqgd];
        [SpiceName("cgb"), SpiceInfo("Gate-Bulk capacitance")]
        public double GetCAPGB(Circuit ckt) => ckt.State.States[0][MOS2states + MOS2capgb];
        [SpiceName("qgb"), SpiceInfo("Gate-Bulk charge storage")]
        public double GetQGB(Circuit ckt) => ckt.State.States[0][MOS2states + MOS2qgb];
        [SpiceName("cqgb"), SpiceInfo("Capacitance due to gate-bulk charge storage")]
        public double GetCQGB(Circuit ckt) => ckt.State.States[0][MOS2states + MOS2cqgb];
        [SpiceName("qbd"), SpiceInfo("Bulk-Drain charge storage")]
        public double GetQBD(Circuit ckt) => ckt.State.States[0][MOS2states + MOS2qbd];
        [SpiceName("cqbd"), SpiceInfo("Capacitance due to bulk-drain charge storage")]
        public double GetCQBD(Circuit ckt) => ckt.State.States[0][MOS2states + MOS2cqbd];
        [SpiceName("qbs"), SpiceInfo("Bulk-Source charge storage")]
        public double GetQBS(Circuit ckt) => ckt.State.States[0][MOS2states + MOS2qbs];
        [SpiceName("cqbs"), SpiceInfo("Capacitance due to bulk-source charge storage")]
        public double GetCQBS(Circuit ckt) => ckt.State.States[0][MOS2states + MOS2cqbs];
        [SpiceName("ib"), SpiceInfo("Bulk current ")]
        public double GetCB(Circuit ckt) => MOS2cbd + MOS2cbs - ckt.State.States[0][MOS2states + MOS2cqgb];
        [SpiceName("ig"), SpiceInfo("Gate current ")]
        public double GetCG(Circuit ckt)
        {
            if (ckt.Method == null)
                return 0;
            else
                return ckt.State.States[0][MOS2states + MOS2cqgb] + ckt.State.States[0][MOS2states + MOS2cqgd] + ckt.State.States[0][MOS2states + MOS2cqgs];
        }
        [SpiceName("is"), SpiceInfo("Source current ")]
        public double GetCS(Circuit ckt)
        {
            double value = -MOS2cd;
            value -= MOS2cbd + MOS2cbs - ckt.State.States[0][MOS2states + MOS2cqgb];
            if (ckt.Method != null)
                value -= ckt.State.States[0][MOS2states + MOS2cqgb] + ckt.State.States[0][MOS2states + MOS2cqgd] + ckt.State.States[0][MOS2states + MOS2cqgs];
            return value;
        }
        [SpiceName("p"), SpiceInfo("Instantaneous power ")]
        public double GetPOWER(Circuit ckt)
        {
            double temp;
            double value = MOS2cd * ckt.State.Real.Solution[MOS2dNode];
            value += (MOS2cbd + MOS2cbs - ckt.State.States[0][MOS2states + MOS2cqgb]) * ckt.State.Real.Solution[MOS2bNode];
            if (ckt.Method != null)
            {
                value += (ckt.State.States[0][MOS2states + MOS2cqgb] +
                ckt.State.States[0][MOS2states + MOS2cqgd] +
                ckt.State.States[0][MOS2states + MOS2cqgs]) *
                ckt.State.Real.Solution[MOS2gNode];
            }
            temp = -MOS2cd;
            temp -= MOS2cbd + MOS2cbs;
            if (ckt.Method != null)
            {
                temp -= ckt.State.States[0][MOS2states + MOS2cqgb] +
                ckt.State.States[0][MOS2states + MOS2cqgd] +
                ckt.State.States[0][MOS2states + MOS2cqgs];
            }
            value += temp * ckt.State.Real.Solution[MOS2sNode];
            return value;
        }

        /// <summary>
        /// Extra variables
        /// </summary>
        public double MOS2mode { get; private set; }
        public double MOS2tTransconductance { get; private set; }
        public double MOS2tSurfMob { get; private set; }
        public double MOS2tPhi { get; private set; }
        public double MOS2tVbi { get; private set; }
        public double MOS2tVto { get; private set; }
        public double MOS2tSatCur { get; private set; }
        public double MOS2tSatCurDens { get; private set; }
        public double MOS2tCbd { get; private set; }
        public double MOS2tCbs { get; private set; }
        public double MOS2tCj { get; private set; }
        public double MOS2tCjsw { get; private set; }
        public double MOS2tBulkPot { get; private set; }
        public double MOS2tDepCap { get; private set; }
        public double MOS2f2d { get; private set; }
        public double MOS2f3d { get; private set; }
        public double MOS2f4d { get; private set; }
        public double MOS2f2s { get; private set; }
        public double MOS2f3s { get; private set; }
        public double MOS2f4s { get; private set; }
        public double MOS2senPertFlag { get; private set; }
        public double MOS2cgs { get; private set; }
        public double MOS2cgd { get; private set; }
        public double MOS2cgb { get; private set; }
        public int MOS2states { get; private set; }

        /// <summary>
        /// Constants
        /// </summary>
        private const int MOS2vbd = 0;
        private const int MOS2vbs = 1;
        private const int MOS2vgs = 2;
        private const int MOS2vds = 3;
        private const int MOS2capgs = 4;
        private const int MOS2qgs = 5;
        private const int MOS2cqgs = 6;
        private const int MOS2capgd = 7;
        private const int MOS2qgd = 8;
        private const int MOS2cqgd = 9;
        private const int MOS2capgb = 10;
        private const int MOS2qgb = 11;
        private const int MOS2cqgb = 12;
        private const int MOS2qbd = 13;
        private const int MOS2cqbd = 14;
        private const int MOS2qbs = 15;
        private const int MOS2cqbs = 16;
        private static double[] sig1 = new double[] { 1.0, -1.0, 1.0, -1.0 };
        private static double[] sig2 = new double[] { 1.0, 1.0, -1.0, -1.0 };

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the device</param>
        public MOS2(string name) : base(name, "Drain", "Gate", "Source", "Bulk")
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
            MOS2dNode = nodes[0].Index;
            MOS2gNode = nodes[1].Index;
            MOS2sNode = nodes[2].Index;
            MOS2bNode = nodes[3].Index;

            // Allocate states
            MOS2states = ckt.State.GetState(26);

            /* allocate a chunk of the state vector */
            MOS2vdsat = 0;

            if ((Model.MOS2drainResistance != 0 || (MOS2drainSquares != 0 && Model.MOS2sheetResistance != 0)) && MOS2dNodePrime == 0)
                MOS2dNodePrime = CreateNode(ckt).Index;
            else
                MOS2dNodePrime = MOS2dNode;

            if (((Model.MOS2sourceResistance != 0) || ((MOS2sourceSquares != 0) && (Model.MOS2sheetResistance != 0))) && (MOS2sNodePrime == 0))
                MOS2sNodePrime = CreateNode(ckt).Index;
            else
                MOS2sNodePrime = MOS2sNode;
        }

        /// <summary>
        /// Do temperature-dependent calculations
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Temperature(Circuit ckt)
        {
            double vt, ratio, fact2, kt, egfet, arg, pbfact, ratio4, phio, pbo, gmaold, capfact, gmanew, czbd, czbdsw, sarg, sargsw, czbs, czbssw;

            /* perform the parameter defaulting */
            if (!MOS2temp.Given)
                MOS2temp.Value = ckt.State.Temperature;
            MOS2mode = 1;
            MOS2von = 0;

            vt = MOS2temp * Circuit.CONSTKoverQ;
            ratio = MOS2temp / Model.MOS2tnom;
            fact2 = MOS2temp / Circuit.CONSTRefTemp;
            kt = MOS2temp * Circuit.CONSTBoltz;
            egfet = 1.16 - (7.02e-4 * MOS2temp * MOS2temp) /
            (MOS2temp + 1108);
            arg = -egfet / (kt + kt) + 1.1150877 / (Circuit.CONSTBoltz * (Circuit.CONSTRefTemp + Circuit.CONSTRefTemp));
            pbfact = -2 * vt * (1.5 * Math.Log(fact2) + Circuit.CHARGE * arg);
            
            if (Model.MOS2drainResistance.Given)
            {
                if (Model.MOS2drainResistance != 0)
                {
                    MOS2drainConductance = 1 / Model.MOS2drainResistance;
                }
                else
                {
                    MOS2drainConductance = 0;
                }
            }
            else if (Model.MOS2sheetResistance.Given)
            {
                if (Model.MOS2sheetResistance != 0)
                {
                    MOS2drainConductance =
                    1 / (Model.MOS2sheetResistance * MOS2drainSquares);
                }
                else
                {
                    MOS2drainConductance = 0;
                }
            }
            else
            {
                MOS2drainConductance = 0;
            }
            if (Model.MOS2sourceResistance.Given)
            {
                if (Model.MOS2sourceResistance != 0)
                {
                    MOS2sourceConductance = 1 / Model.MOS2sourceResistance;
                }
                else
                {
                    MOS2sourceConductance = 0;
                }
            }
            else if (Model.MOS2sheetResistance.Given)
            {
                if (Model.MOS2sheetResistance != 0)
                {
                    MOS2sourceConductance =
                    1 / (Model.MOS2sheetResistance * MOS2sourceSquares);
                }
                else
                {
                    MOS2sourceConductance = 0;
                }
            }
            else
            {
                MOS2sourceConductance = 0;
            }
            if (MOS2l - 2 * Model.MOS2latDiff <= 0)
                CircuitWarning.Warning(this, $"{Name}: effective channel length less than zero");

            ratio4 = ratio * Math.Sqrt(ratio);
            MOS2tTransconductance = Model.MOS2transconductance / ratio4;
            MOS2tSurfMob = Model.MOS2surfaceMobility / ratio4;
            phio = (Model.MOS2phi - Model.pbfact1) / Model.fact1;
            MOS2tPhi = fact2 * phio + pbfact;
            MOS2tVbi = Model.MOS2vt0 - Model.MOS2type * (Model.MOS2gamma * Math.Sqrt(Model.MOS2phi))
                + .5 * (Model.egfet1 - egfet) + Model.MOS2type * .5 * (MOS2tPhi - Model.MOS2phi);
            MOS2tVto = MOS2tVbi + Model.MOS2type * Model.MOS2gamma * Math.Sqrt(MOS2tPhi);
            MOS2tSatCur = Model.MOS2jctSatCur * Math.Exp(-egfet / vt + Model.egfet1 / Model.vtnom);
            MOS2tSatCurDens = Model.MOS2jctSatCurDensity * Math.Exp(-egfet / vt + Model.egfet1 / Model.vtnom);
            pbo = (Model.MOS2bulkJctPotential - Model.pbfact1) / Model.fact1;
            gmaold = (Model.MOS2bulkJctPotential - pbo) / pbo;
            capfact = 1 / (1 + Model.MOS2bulkJctBotGradingCoeff * (4e-4 * (Model.MOS2tnom - Circuit.CONSTRefTemp) - gmaold));
            MOS2tCbd = Model.MOS2capBD * capfact;
            MOS2tCbs = Model.MOS2capBS * capfact;
            MOS2tCj = Model.MOS2bulkCapFactor * capfact;
            capfact = 1 / (1 + Model.MOS2bulkJctSideGradingCoeff * (4e-4 * (Model.MOS2tnom - Circuit.CONSTRefTemp) - gmaold));
            MOS2tCjsw = Model.MOS2sideWallCapFactor * capfact;
            MOS2tBulkPot = fact2 * pbo + pbfact;
            gmanew = (MOS2tBulkPot - pbo) / pbo;
            capfact = (1 + Model.MOS2bulkJctBotGradingCoeff * (4e-4 * (MOS2temp - Circuit.CONSTRefTemp) - gmanew));
            MOS2tCbd *= capfact;
            MOS2tCbs *= capfact;
            MOS2tCj *= capfact;
            capfact = (1 + Model.MOS2bulkJctSideGradingCoeff * (4e-4 * (MOS2temp - Circuit.CONSTRefTemp) - gmanew));
            MOS2tCjsw *= capfact;
            MOS2tDepCap = Model.MOS2fwdCapDepCoeff * MOS2tBulkPot;

            if ((MOS2tSatCurDens == 0) || (MOS2drainArea == 0) || (MOS2sourceArea == 0))
            {
                MOS2sourceVcrit = MOS2drainVcrit = vt * Math.Log(vt / (Circuit.CONSTroot2 * MOS2tSatCur));
            }
            else
            {
                MOS2drainVcrit = vt * Math.Log(vt / (Circuit.CONSTroot2 * MOS2tSatCurDens * MOS2drainArea));
                MOS2sourceVcrit = vt * Math.Log(vt / (Circuit.CONSTroot2 * MOS2tSatCurDens * MOS2sourceArea));
            }
            if (Model.MOS2capBD.Given)
            {
                czbd = MOS2tCbd;
            }
            else
            {
                if (Model.MOS2bulkCapFactor.Given)
                {
                    czbd = MOS2tCj * MOS2drainArea;
                }
                else
                {
                    czbd = 0;
                }
            }
            if (Model.MOS2sideWallCapFactor.Given)
            {
                czbdsw = MOS2tCjsw * MOS2drainPerimiter;
            }
            else
            {
                czbdsw = 0;
            }
            arg = 1 - Model.MOS2fwdCapDepCoeff;
            sarg = Math.Exp((-Model.MOS2bulkJctBotGradingCoeff) * Math.Log(arg));
            sargsw = Math.Exp((-Model.MOS2bulkJctSideGradingCoeff) * Math.Log(arg));
            MOS2Cbd = czbd;
            MOS2Cbdsw = czbdsw;
            MOS2f2d = czbd * (1 - Model.MOS2fwdCapDepCoeff * (1 + Model.MOS2bulkJctBotGradingCoeff)) * sarg / arg
                + czbdsw * (1 - Model.MOS2fwdCapDepCoeff * (1 + Model.MOS2bulkJctSideGradingCoeff)) * sargsw / arg;
            MOS2f3d = czbd * Model.MOS2bulkJctBotGradingCoeff * sarg / arg / MOS2tBulkPot
                + czbdsw * Model.MOS2bulkJctSideGradingCoeff * sargsw / arg / MOS2tBulkPot;
            MOS2f4d = czbd * MOS2tBulkPot * (1 - arg * sarg) / (1 - Model.MOS2bulkJctBotGradingCoeff)
                + czbdsw * MOS2tBulkPot * (1 - arg * sargsw) / (1 - Model.MOS2bulkJctSideGradingCoeff)
                - MOS2f3d / 2 * (MOS2tDepCap * MOS2tDepCap) - MOS2tDepCap * MOS2f2d;
            if (Model.MOS2capBS.Given)
            {
                czbs = MOS2tCbs;
            }
            else
            {
                if (Model.MOS2bulkCapFactor.Given)
                {
                    czbs = MOS2tCj * MOS2sourceArea;
                }
                else
                {
                    czbs = 0;
                }
            }
            if (Model.MOS2sideWallCapFactor.Given)
            {
                czbssw = MOS2tCjsw * MOS2sourcePerimiter;
            }
            else
            {
                czbssw = 0;
            }
            arg = 1 - Model.MOS2fwdCapDepCoeff;
            sarg = Math.Exp((-Model.MOS2bulkJctBotGradingCoeff) * Math.Log(arg));
            sargsw = Math.Exp((-Model.MOS2bulkJctSideGradingCoeff) * Math.Log(arg));
            MOS2Cbs = czbs;
            MOS2Cbssw = czbssw;
            MOS2f2s = czbs * (1 - Model.MOS2fwdCapDepCoeff * (1 + Model.MOS2bulkJctBotGradingCoeff)) * sarg / arg
            + czbssw * (1 - Model.MOS2fwdCapDepCoeff * (1 + Model.MOS2bulkJctSideGradingCoeff)) * sargsw / arg;
            MOS2f3s = czbs * Model.MOS2bulkJctBotGradingCoeff * sarg / arg / MOS2tBulkPot
                + czbssw * Model.MOS2bulkJctSideGradingCoeff * sargsw / arg / MOS2tBulkPot;
            MOS2f4s = czbs * MOS2tBulkPot * (1 - arg * sarg) / (1 - Model.MOS2bulkJctBotGradingCoeff)
                + czbssw * MOS2tBulkPot * (1 - arg * sargsw) / (1 - Model.MOS2bulkJctSideGradingCoeff)
                - MOS2f3s / 2 * (MOS2tDepCap * MOS2tDepCap) - MOS2tDepCap * MOS2f2s;
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
            double EffectiveLength, DrainSatCur, SourceSatCur, GateSourceOverlapCap, GateDrainOverlapCap, GateBulkOverlapCap, Beta, OxideCap, 
                vgs, vds, vbs, vbd, vgb, vgd, vgdo, delvbs, delvbd, delvgs, delvds, delvgd, cdhat, cbhat, von, evbs, evbd, sarg, vdsat, arg, 
                cdrain = 0.0, sargsw;
            double vgs1, vgd1, vgb1, capgs = 0.0, capgd = 0.0, capgb = 0.0, gcgs, ceqgs, gcgd, ceqgd, gcgb, ceqgb, ceqbs, ceqbd;
            int xnrm;
            int xrev;
            double cdreq;

            vt = Circuit.CONSTKoverQ * MOS2temp;
            Check = 1;

            EffectiveLength = MOS2l - 2 * Model.MOS2latDiff;
            if ((MOS2tSatCurDens == 0) || (MOS2drainArea == 0) || (MOS2sourceArea == 0))
            {
                DrainSatCur = MOS2tSatCur;
                SourceSatCur = MOS2tSatCur;
            }
            else
            {
                DrainSatCur = MOS2tSatCurDens * MOS2drainArea;
                SourceSatCur = MOS2tSatCurDens * MOS2sourceArea;
            }
            GateSourceOverlapCap = Model.MOS2gateSourceOverlapCapFactor * MOS2w;
            GateDrainOverlapCap = Model.MOS2gateDrainOverlapCapFactor * MOS2w;
            GateBulkOverlapCap = Model.MOS2gateBulkOverlapCapFactor * EffectiveLength;
            Beta = MOS2tTransconductance * MOS2w / EffectiveLength;
            OxideCap = Model.MOS2oxideCapFactor * EffectiveLength * MOS2w;

            if (state.Init == CircuitState.InitFlags.InitFloat || state.UseSmallSignal || (method != null && method.SavedTime == 0.0) || (state.Init == CircuitState.InitFlags.InitFix && !MOS2off))
            {
                /* PREDICTOR */

                /* general iteration */

                vbs = Model.MOS2type * (rstate.OldSolution[MOS2bNode] - rstate.OldSolution[MOS2sNodePrime]);
                vgs = Model.MOS2type * (rstate.OldSolution[MOS2gNode] - rstate.OldSolution[MOS2sNodePrime]);
                vds = Model.MOS2type * (rstate.OldSolution[MOS2dNodePrime] - rstate.OldSolution[MOS2sNodePrime]);
                /* PREDICTOR */

                /* now some common crunching for some more useful quantities */

                vbd = vbs - vds;
                vgd = vgs - vds;
                vgdo = state.States[0][MOS2states + MOS2vgs] - state.States[0][MOS2states + MOS2vds];
                delvbs = vbs - state.States[0][MOS2states + MOS2vbs];
                delvbd = vbd - state.States[0][MOS2states + MOS2vbd];
                delvgs = vgs - state.States[0][MOS2states + MOS2vgs];
                delvds = vds - state.States[0][MOS2states + MOS2vds];
                delvgd = vgd - vgdo;

                /* these are needed for convergence testing */
                if (MOS2mode >= 0)
                    cdhat = MOS2cd - MOS2gbd * delvbd + MOS2gmbs * delvbs + MOS2gm * delvgs + MOS2gds * delvds;
                else
                    cdhat = MOS2cd + (MOS2gmbs - MOS2gbd) * delvbd - MOS2gm * delvgd + MOS2gds * delvds;
                cbhat = MOS2cbs + MOS2cbd + MOS2gbd * delvbd + MOS2gbs * delvbs;

                /* now lets see if we can bypass (ugh) */
                /* the following massive if should all be one
				* single compound if statement, but most compilers
				* can't handle it in one piece, so it is broken up
				* into several stages here
				*/
                /*NOBYPASS*/
                /* ok - bypass is out, do it the hard way */

                von = Model.MOS2type * MOS2von;
                /*
				* limiting
				* We want to keep device voltages from changing
				* so fast that the exponentials churn out overflows 
				* and similar rudeness
				*/
                if (state.States[0][MOS2states + MOS2vds] >= 0)
                {
                    vgs = Transistor.DEVfetlim(vgs, state.States[0][MOS2states + MOS2vgs], von);
                    vds = vgs - vgd;
                    vds = Transistor.DEVlimvds(vds, state.States[0][MOS2states + MOS2vds]);
                    vgd = vgs - vds;
                }
                else
                {
                    vgd = Transistor.DEVfetlim(vgd, vgdo, von);
                    vds = vgs - vgd;
                    vds = -Transistor.DEVlimvds(-vds, -(state.States[0][MOS2states + MOS2vds]));
                    vgs = vgd + vds;
                }
                if (vds >= 0)
                {
                    vbs = Transistor.DEVpnjlim(vbs, state.States[0][MOS2states + MOS2vbs], vt, MOS2sourceVcrit, ref Check);
                    vbd = vbs - vds;
                }
                else
                {
                    vbd = Transistor.DEVpnjlim(vbd, state.States[0][MOS2states + MOS2vbd], vt, MOS2drainVcrit, ref Check);
                    vbs = vbd + vds;
                }
            }
            else
            {
                /* ok - not one of the simple cases, so we have to 
				* look at other possibilities 
				*/

                if (state.Init == CircuitState.InitFlags.InitJct && !MOS2off)
                {
                    vds = Model.MOS2type * MOS2icVDS;
                    vgs = Model.MOS2type * MOS2icVGS;
                    vbs = Model.MOS2type * MOS2icVBS;
                    if ((vds == 0) && (vgs == 0) && (vbs == 0) &&
                        (method != null || state.UseDC || state.Domain == CircuitState.DomainTypes.None || !state.UseIC))
                    {
                        vbs = -1;
                        vgs = Model.MOS2type * MOS2tVto;
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
            vgb = vgs - vbs;

            /* bulk-source and bulk-drain doides
			* here we just evaluate the ideal diode current and the
			* correspoinding derivative (conductance).
			*/

            if (vbs <= 0)
            {
                MOS2gbs = SourceSatCur / vt;
                MOS2cbs = MOS2gbs * vbs;
                MOS2gbs += state.Gmin;
            }
            else
            {
                evbs = Math.Exp(vbs / vt);
                MOS2gbs = SourceSatCur * evbs / vt + state.Gmin;
                MOS2cbs = SourceSatCur * (evbs - 1);
            }
            if (vbd <= 0)
            {
                MOS2gbd = DrainSatCur / vt;
                MOS2cbd = MOS2gbd * vbd;
                MOS2gbd += state.Gmin;
            }
            else
            {
                evbd = Math.Exp(vbd / vt);
                MOS2gbd = DrainSatCur * evbd / vt + state.Gmin;
                MOS2cbd = DrainSatCur * (evbd - 1);
            }
            if (vds >= 0)
            {
                /* normal mode */
                MOS2mode = 1;
            }
            else
            {
                /* inverse mode */
                MOS2mode = -1;
            }
            {
                /* moseq2(vds,vbs,vgs,gm,gds,gmbs,qg,qc,qb,
				*        cggb,cgdb,cgsb,cbgb,cbdb,cbsb)
				*/
                /* note:  cgdb, cgsb, cbdb, cbsb never used */

                /*
				*     this routine evaluates the drain current, its derivatives and
				*     the charges associated with the gate, channel and bulk
				*     for mosfets
				*
				*/

                double[] a4 = new double[4], b4 = new double[4], x4 = new double[4], poly4 = new double[8];
                double beta1, dsrgdb, d2sdb2, sphi = 0.0;    /* square root of phi */
                double sphi3 = 0.0;   /* square root of phi cubed */
                double barg, d2bdb2, factor, dbrgdb, eta, vbin, argd = 0.0, args = 0.0, argss, argsd = 0.0, argxs = 0.0, argxd = 0.0, daddb2, dasdb2, dbargd, dbargs, dbxwd, dbxws, 
                    dgddb2, dgddvb, dgdvds, gamasd, xwd, xws, ddxwd, gammad, vth, cfs, cdonco, xn = 0.0, argg = 0.0, vgst, sarg3, sbiarg, dgdvbs, body, gdbdv, 
                    dodvbs, dodvds = 0.0, dxndvd = 0.0, dxndvb = 0.0, udenom, dudvgs, dudvds, dudvbs, gammd2, argv, vgsx, ufact, ueff, dsdvgs, dsdvbs, a1, a3, a, b1, 
                    b3, b, c1, c, d1, fi, p0, p2, p3, p4, p, r3, r, ro, s2, s, v1, v2, xv, y3, delta4, xvalid = 0.0, bsarg = 0.0, dbsrdb = 0.0, bodys = 0.0, gdbdvs = 0.0, sargv, 
                    xlfact, dldsat = 0.0, xdv, xlv, vqchan, dqdsat, vl, dfundg, dfunds, dfundb, xls, dldvgs, dldvds, dldvbs, dfact, clfact, xleff, deltal, 
                    xwb, vdson, cdson, didvds, gdson, gmw, gbson, expg, xld;
                double xlamda = Model.MOS2lambda;
                /* 'local' variables - these switch d & s around appropriately
				* so that we don't have to worry about vds < 0
				*/
                double lvbs = MOS2mode == 1 ? vbs : vbd;
                double lvds = MOS2mode * vds;
                double lvgs = MOS2mode == 1 ? vgs : vgd;
                double phiMinVbs = MOS2tPhi - lvbs;
                double tmp; /* a temporary variable, not used for more than */
                            /* about 10 lines at a time */
                int iknt;
                int jknt;
                int i;
                int j;

                /*
				*  compute some useful quantities
				*/

                if (lvbs <= 0.0)
                {
                    sarg = Math.Sqrt(phiMinVbs);
                    dsrgdb = -0.5 / sarg;
                    d2sdb2 = 0.5 * dsrgdb / phiMinVbs;
                }
                else
                {
                    sphi = Math.Sqrt(MOS2tPhi);
                    sphi3 = MOS2tPhi * sphi;
                    sarg = sphi / (1.0 + 0.5 * lvbs / MOS2tPhi);
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
                    barg = sphi / (1.0 + 0.5 * (lvbs - lvds) / MOS2tPhi);
                    tmp = barg / sphi3;
                    dbrgdb = -0.5 * barg * tmp;
                    d2bdb2 = -dbrgdb * tmp;
                }
                /*
				*  calculate threshold voltage (von)
				*     narrow-channel effect
				*/

                /*XXX constant per device */
                factor = 0.125 * Model.MOS2narrowFactor * 2.0 * Circuit.CONSTPI * Transistor.EPSSIL /
                OxideCap * EffectiveLength;
                /*XXX constant per device */
                eta = 1.0 + factor;
                vbin = MOS2tVbi * Model.MOS2type + factor * phiMinVbs;
                if ((Model.MOS2gamma > 0.0) ||
                (Model.MOS2substrateDoping > 0.0))
                {
                    xwd = Model.MOS2xd * barg;
                    xws = Model.MOS2xd * sarg;

                    /*
					*     short-channel effect with vds .ne. 0.0
					*/

                    argss = 0.0;
                    argsd = 0.0;
                    dbargs = 0.0;
                    dbargd = 0.0;
                    dgdvds = 0.0;
                    dgddb2 = 0.0;
                    if (Model.MOS2junctionDepth > 0)
                    {
                        tmp = 2.0 / Model.MOS2junctionDepth;
                        argxs = 1.0 + xws * tmp;
                        argxd = 1.0 + xwd * tmp;
                        args = Math.Sqrt(argxs);
                        argd = Math.Sqrt(argxd);
                        tmp = .5 * Model.MOS2junctionDepth / EffectiveLength;
                        argss = tmp * (args - 1.0);
                        argsd = tmp * (argd - 1.0);
                    }
                    gamasd = Model.MOS2gamma * (1.0 - argss - argsd);
                    dbxwd = Model.MOS2xd * dbrgdb;
                    dbxws = Model.MOS2xd * dsrgdb;
                    if (Model.MOS2junctionDepth > 0)
                    {
                        tmp = 0.5 / EffectiveLength;
                        dbargs = tmp * dbxws / args;
                        dbargd = tmp * dbxwd / argd;
                        dasdb2 = -Model.MOS2xd * (d2sdb2 + dsrgdb * dsrgdb *
                        Model.MOS2xd / (Model.MOS2junctionDepth * argxs)) /
                        (EffectiveLength * args);
                        daddb2 = -Model.MOS2xd * (d2bdb2 + dbrgdb * dbrgdb *
                        Model.MOS2xd / (Model.MOS2junctionDepth * argxd)) /
                        (EffectiveLength * argd);
                        dgddb2 = -0.5 * Model.MOS2gamma * (dasdb2 + daddb2);
                    }
                    dgddvb = -Model.MOS2gamma * (dbargs + dbargd);
                    if (Model.MOS2junctionDepth > 0)
                    {
                        ddxwd = -dbxwd;
                        dgdvds = -Model.MOS2gamma * 0.5 * ddxwd / (EffectiveLength * argd);
                    }
                }
                else
                {
                    gamasd = Model.MOS2gamma;
                    gammad = Model.MOS2gamma;
                    dgddvb = 0.0;
                    dgdvds = 0.0;
                    dgddb2 = 0.0;
                }
                von = vbin + gamasd * sarg;
                vth = von;
                vdsat = 0.0;
                if (Model.MOS2fastSurfaceStateDensity != 0.0 && OxideCap != 0.0)
                {
                    /* XXX constant per model */
                    cfs = Circuit.CHARGE * Model.MOS2fastSurfaceStateDensity *
                    1e4 /*(cm**2/m**2)*/;
                    cdonco = -(gamasd * dsrgdb + dgddvb * sarg) + factor;
                    xn = 1.0 + cfs / OxideCap * MOS2w * EffectiveLength + cdonco;
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
						*  cutoff region
						*/
                        MOS2gds = 0.0;
                        goto line1050;
                    }
                }

                /*
				*  compute some more useful quantities
				*/

                sarg3 = sarg * sarg * sarg;
                /* XXX constant per model */
                sbiarg = Math.Sqrt(MOS2tBulkPot);
                gammad = gamasd;
                dgdvbs = dgddvb;
                body = barg * barg * barg - sarg3;
                gdbdv = 2.0 * gammad * (barg * barg * dbrgdb - sarg * sarg * dsrgdb);
                dodvbs = -factor + dgdvbs * sarg + gammad * dsrgdb;
                if (Model.MOS2fastSurfaceStateDensity == 0.0) goto line400;
                if (OxideCap == 0.0) goto line410;
                dxndvb = 2.0 * dgdvbs * dsrgdb + gammad * d2sdb2 + dgddb2 * sarg;
                dodvbs = dodvbs + vt * dxndvb;
                dxndvd = dgdvds * dsrgdb;
                dodvds = dgdvds * sarg + vt * dxndvd;
                /*
				*  evaluate effective mobility and its derivatives
				*/
                line400:
                if (OxideCap <= 0.0) goto line410;
                udenom = vgst;
                tmp = Model.MOS2critField * 100 /* cm/m */ * Transistor.EPSSIL /
                Model.MOS2oxideCapFactor;
                if (udenom <= tmp) goto line410;
                ufact = Math.Exp(Model.MOS2critFieldExp * Math.Log(tmp / udenom));
                ueff = Model.MOS2surfaceMobility * 1e-4 /*(m**2/cm**2) */ * ufact;
                dudvgs = -ufact * Model.MOS2critFieldExp / udenom;
                dudvds = 0.0;
                dudvbs = Model.MOS2critFieldExp * ufact * dodvbs / vgst;
                goto line500;
                line410:
                ufact = 1.0;
                ueff = Model.MOS2surfaceMobility * 1e-4 /*(m**2/cm**2) */ ;
                dudvgs = 0.0;
                dudvds = 0.0;
                dudvbs = 0.0;
                /*
				*     evaluate saturation voltage and its derivatives according to
				*     grove-frohman equation
				*/
                line500:
                vgsx = lvgs;
                gammad = gamasd / eta;
                dgdvbs = dgddvb;
                if (Model.MOS2fastSurfaceStateDensity != 0 && OxideCap != 0)
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
                        dsdvbs = (gammad * (1.0 - arg) + 2.0 * argv / (gammad * arg)) /
                        eta * dgdvbs + 1.0 / arg + factor * dsdvgs;
                    }
                }
                else
                {
                    vdsat = (vgsx - vbin) / eta;
                    vdsat = Math.Max(vdsat, 0.0);
                    dsdvgs = 1.0;
                    dsdvbs = 0.0;
                }
                if (Model.MOS2maxDriftVel > 0)
                {
                    /* 
					*     evaluate saturation voltage and its derivatives 
					*     according to baum's theory of scattering velocity 
					*     saturation
					*/
                    gammd2 = gammad * gammad;
                    v1 = (vgsx - vbin) / eta + phiMinVbs;
                    v2 = phiMinVbs;
                    xv = Model.MOS2maxDriftVel * EffectiveLength / ueff;
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
                        if (delta4 < 0) continue;
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
                        poly4[j - 1] = x4[j - 1] * x4[j - 1] * x4[j - 1] * x4[j - 1] + a1 * x4[j - 1] *
                        x4[j - 1] * x4[j - 1];
                        poly4[j - 1] = poly4[j - 1] + b1 * x4[j - 1] * x4[j - 1] + c1 * x4[j - 1] + d1;
                        if (Math.Abs(poly4[j - 1]) > 1.0e-6) continue;
                        jknt = jknt + 1;
                        if (jknt <= 1)
                        {
                            xvalid = x4[j - 1];
                        }
                        if (x4[j - 1] > xvalid) continue;
                        xvalid = x4[j - 1];
                    }
                    if (jknt > 0)
                    {
                        vdsat = xvalid * xvalid - phiMinVbs;
                    }
                }
                /*
				*  evaluate effective channel length and its derivatives
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
                        bsarg = sphi / (1.0 + 0.5 * (lvbs - vdsat) / MOS2tPhi);
                        dbsrdb = -0.5 * bsarg * bsarg / sphi3;
                    }
                    bodys = bsarg * bsarg * bsarg - sarg3;
                    gdbdvs = 2.0 * gammad * (bsarg * bsarg * dbsrdb - sarg * sarg * dsrgdb);
                    if (Model.MOS2maxDriftVel <= 0)
                    {
                        if (Model.MOS2substrateDoping == 0.0 || xlamda > 0.0)
                        {
                            dldvgs = 0.0;
                            dldvds = 0.0;
                            dldvbs = 0.0;
                        }
                        else
                        {
                            argv = (lvds - vdsat) / 4.0;
                            sargv = Math.Sqrt(1.0 + argv * argv);
                            arg = Math.Sqrt(argv + sargv);
                            xlfact = Model.MOS2xd / (EffectiveLength * lvds);
                            xlamda = xlfact * arg;
                            dldsat = lvds * xlamda / (8.0 * sargv);
                        }
                    }
                    else
                    {
                        argv = (vgsx - vbin) / eta - vdsat;
                        xdv = Model.MOS2xd / Math.Sqrt(Model.MOS2channelCharge);
                        xlv = Model.MOS2maxDriftVel * xdv / (2.0 * ueff);
                        vqchan = argv - gammad * bsarg;
                        dqdsat = -1.0 + gammad * dbsrdb;
                        vl = Model.MOS2maxDriftVel * EffectiveLength;
                        dfunds = vl * dqdsat - ueff * vqchan;
                        dfundg = (vl - ueff * vdsat) / eta;
                        dfundb = -vl * (1.0 + dqdsat - factor / eta) + ueff *
                        (gdbdvs - dgdvbs * bodys / 1.5) / eta;
                        dsdvgs = -dfundg / dfunds;
                        dsdvbs = -dfundb / dfunds;
                        if (Model.MOS2substrateDoping == 0.0 || xlamda > 0.0)
                        {
                            dldvgs = 0.0;
                            dldvds = 0.0;
                            dldvbs = 0.0;
                        }
                        else
                        {
                            argv = lvds - vdsat;
                            argv = Math.Max(argv, 0.0);
                            xls = Math.Sqrt(xlv * xlv + argv);
                            dldsat = xdv / (2.0 * xls);
                            xlfact = xdv / (EffectiveLength * lvds);
                            xlamda = xlfact * (xls - xlv);
                            dldsat = dldsat / EffectiveLength;
                        }
                    }
                    dldvgs = dldsat * dsdvgs;
                    dldvds = -xlamda + dldsat;
                    dldvbs = dldsat * dsdvbs;
                }
                else
                {
                    dldvgs = 0.0;
                    dldvds = 0.0;
                    dldvbs = 0.0;
                }
                /*
				*     limit channel shortening at punch-through
				*/
                xwb = Model.MOS2xd * sbiarg;
                xld = EffectiveLength - xwb;
                clfact = 1.0 - xlamda * lvds;
                dldvds = -xlamda - dldvds;
                xleff = EffectiveLength * clfact;
                deltal = xlamda * lvds * EffectiveLength;
                if (Model.MOS2substrateDoping == 0.0)
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
				*  evaluate effective beta (effective kp)
				*/
                beta1 = Beta * ufact / clfact;
                /*
				*  test for mode of operation and branch appropriately
				*/
                gammad = gamasd;
                dgdvbs = dgddvb;
                if (lvds <= 1.0e-10)
                {
                    if (lvgs <= von)
                    {
                        if ((Model.MOS2fastSurfaceStateDensity == 0.0) ||
                        (OxideCap == 0.0))
                        {
                            MOS2gds = 0.0;
                            goto line1050;
                        }

                        MOS2gds = beta1 * (von - vbin - gammad * sarg) * Math.Exp(argg *
                        (lvgs - von));
                        goto line1050;
                    }

                    MOS2gds = beta1 * (lvgs - vbin - gammad * sarg);
                    goto line1050;
                }

                if (lvgs > von) goto line900;
                /*
				*  subthreshold region
				*/
                if (vdsat <= 0)
                {
                    MOS2gds = 0.0;
                    if (lvgs > vth) goto doneval;
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
                if (lvds < vdsat) gdson = gdson + didvds;
                gbson = -cdson * dldvbs / clfact + beta1 *
                (dodvbs * vdson + factor * vdson - dgdvbs * body / 1.5 - gdbdv);
                if (lvds > vdsat) gbson = gbson + didvds * dsdvbs;
                expg = Math.Exp(argg * (lvgs - von));
                cdrain = cdson * expg;
                gmw = cdrain * argg;
                MOS2gm = gmw;
                if (lvds > vdsat) MOS2gm = gmw + didvds * dsdvgs * expg;
                tmp = gmw * (lvgs - von) / xn;
                MOS2gds = gdson * expg - MOS2gm * dodvds - tmp * dxndvd;
                MOS2gmbs = gbson * expg - MOS2gm * dodvbs - tmp * dxndvb;
                goto doneval;

                line900:
                if (lvds <= vdsat)
                {
                    /*
					*  linear region
					*/
                    cdrain = beta1 * ((lvgs - vbin - eta * lvds / 2.0) * lvds - gammad * body / 1.5);
                    arg = cdrain * (dudvgs / ufact - dldvgs / clfact);
                    MOS2gm = arg + beta1 * lvds;
                    arg = cdrain * (dudvds / ufact - dldvds / clfact);
                    MOS2gds = arg + beta1 * (lvgs - vbin - eta *
                    lvds - gammad * barg - dgdvds * body / 1.5);
                    arg = cdrain * (dudvbs / ufact - dldvbs / clfact);
                    MOS2gmbs = arg - beta1 * (gdbdv + dgdvbs * body / 1.5 - factor * lvds);
                }
                else
                {
                    /* 
					*  saturation region
					*/
                    cdrain = beta1 * ((lvgs - vbin - eta *
                    vdsat / 2.0) * vdsat - gammad * bodys / 1.5);
                    arg = cdrain * (dudvgs / ufact - dldvgs / clfact);
                    MOS2gm = arg + beta1 * vdsat + beta1 * (lvgs -
                    vbin - eta * vdsat - gammad * bsarg) * dsdvgs;
                    MOS2gds = -cdrain * dldvds / clfact - beta1 * dgdvds * bodys / 1.5;
                    arg = cdrain * (dudvbs / ufact - dldvbs / clfact);
                    MOS2gmbs = arg - beta1 * (gdbdvs + dgdvbs * bodys / 1.5 - factor *
                    vdsat) + beta1 * (lvgs - vbin - eta * vdsat - gammad * bsarg) * dsdvbs;
                }
                /*
				*     compute charges for "on" region
				*/
                goto doneval;
                /*
				*  finish special cases
				*/
                line1050:
                cdrain = 0.0;
                MOS2gm = 0.0;
                MOS2gmbs = 0.0;
                /*
				*  finished
				*/

            }
            doneval:
            MOS2von = Model.MOS2type * von;
            MOS2vdsat = Model.MOS2type * vdsat;
            /*
			*  COMPUTE EQUIVALENT DRAIN CURRENT SOURCE
			*/
            MOS2cd = MOS2mode * cdrain - MOS2cbd;

            if (state.Domain == CircuitState.DomainTypes.Time || state.UseDC)
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
                    if (vbs < MOS2tDepCap)
                    {
                        arg = 1 - vbs / MOS2tBulkPot;
                        /*
						* the following block looks somewhat long and messy,
						* but since most users use the default grading
						* coefficients of .5, and sqrt is MUCH faster than an
						* Math.Exp(Math.Log()) we use this special case code to buy time.
						* (as much as 10% of total job time!)
						*/
                        if (Model.MOS2bulkJctBotGradingCoeff ==
                        Model.MOS2bulkJctSideGradingCoeff)
                        {
                            if (Model.MOS2bulkJctBotGradingCoeff == .5)
                            {
                                sarg = sargsw = 1 / Math.Sqrt(arg);
                            }
                            else
                            {
                                sarg = sargsw = Math.Exp(-Model.MOS2bulkJctBotGradingCoeff * Math.Log(arg));
                            }
                        }
                        else
                        {
                            if (Model.MOS2bulkJctBotGradingCoeff == .5)
                            {
                                sarg = 1 / Math.Sqrt(arg);
                            }
                            else
                            {
                                /*NOSQRT*/
                                sarg = Math.Exp(-Model.MOS2bulkJctBotGradingCoeff * Math.Log(arg));
                            }
                            if (Model.MOS2bulkJctSideGradingCoeff == .5)
                            {
                                sargsw = 1 / Math.Sqrt(arg);
                            }
                            else
                            {
                                /*NOSQRT*/
                                sargsw = Math.Exp(-Model.MOS2bulkJctSideGradingCoeff * Math.Log(arg));
                            }
                        }
                        /*NOSQRT*/
                        state.States[0][MOS2states + MOS2qbs] = MOS2tBulkPot * (MOS2Cbs * (1 - arg * sarg) / (1 - Model.MOS2bulkJctBotGradingCoeff)
                            + MOS2Cbssw * (1 - arg * sargsw) / (1 - Model.MOS2bulkJctSideGradingCoeff));
                        MOS2capbs = MOS2Cbs * sarg +
                        MOS2Cbssw * sargsw;
                    }
                    else
                    {
                        state.States[0][MOS2states + MOS2qbs] = MOS2f4s + vbs * (MOS2f2s + vbs * (MOS2f3s / 2));
                        MOS2capbs = MOS2f2s + MOS2f3s * vbs;
                    }
                    /*CAPZEROBYPASS*/
                }
                /*CAPBYPASS*/
                /* can't bypass the diode capacitance calculations */
                {
                    /*CAPZEROBYPASS*/
                    if (vbd < MOS2tDepCap)
                    {
                        arg = 1 - vbd / MOS2tBulkPot;
                        /*
						* the following block looks somewhat long and messy,
						* but since most users use the default grading
						* coefficients of .5, and sqrt is MUCH faster than an
						* Math.Exp(Math.Log()) we use this special case code to buy time.
						* (as much as 10% of total job time!)
						*/
                        if (Model.MOS2bulkJctBotGradingCoeff == .5 && Model.MOS2bulkJctSideGradingCoeff == .5)
                        {
                            sarg = sargsw = 1 / Math.Sqrt(arg);
                        }
                        else
                        {
                            if (Model.MOS2bulkJctBotGradingCoeff == .5)
                            {
                                sarg = 1 / Math.Sqrt(arg);
                            }
                            else
                            {
                                /*NOSQRT*/
                                sarg = Math.Exp(-Model.MOS2bulkJctBotGradingCoeff * Math.Log(arg));
                            }
                            if (Model.MOS2bulkJctSideGradingCoeff == .5)
                            {
                                sargsw = 1 / Math.Sqrt(arg);
                            }
                            else
                            {
                                /*NOSQRT*/
                                sargsw = Math.Exp(-Model.MOS2bulkJctSideGradingCoeff *
                                Math.Log(arg));
                            }
                        }
                        /*NOSQRT*/
                        state.States[0][MOS2states + MOS2qbd] = MOS2tBulkPot * (MOS2Cbd * (1 - arg * sarg) / (1 - Model.MOS2bulkJctBotGradingCoeff)
                            + MOS2Cbdsw * (1 - arg * sargsw) / (1 - Model.MOS2bulkJctSideGradingCoeff));
                        MOS2capbd = MOS2Cbd * sarg +
                        MOS2Cbdsw * sargsw;
                    }
                    else
                    {
                        state.States[0][MOS2states + MOS2qbd] = MOS2f4d + vbd * (MOS2f2d + vbd * MOS2f3d / 2);
                        MOS2capbd = MOS2f2d + vbd * MOS2f3d;
                    }
                    /*CAPZEROBYPASS*/
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

                    /* integrate the capacitors and save results */
                    var result = method.Integrate(state, MOS2states + MOS2qbd, MOS2capbd);
                    MOS2gbd += result.Geq;
                    MOS2cbd += state.States[0][MOS2states + MOS2cqbd];
                    MOS2cd -= state.States[0][MOS2states + MOS2cqbd];
                    result = method.Integrate(state, MOS2states + MOS2qbs, MOS2capbs);
                    MOS2gbs += result.Geq;
                    MOS2cbs += state.States[0][MOS2states + MOS2cqbs];
                }
            }

            /*
			 *  check convergence
			 */
            if (!MOS2off || !(state.Init == CircuitState.InitFlags.InitFix || state.UseSmallSignal))
            {
                if (Check == 1)
                    state.IsCon = false;
            }
            state.States[0][MOS2states + MOS2vbs] = vbs;
            state.States[0][MOS2states + MOS2vbd] = vbd;
            state.States[0][MOS2states + MOS2vgs] = vgs;
            state.States[0][MOS2states + MOS2vds] = vds;

            /*
			 *     meyer's capacitor model
			 */
            if (state.Domain == CircuitState.DomainTypes.Time || state.UseSmallSignal)
            {
                /*
				 *     calculate meyer's capacitors
				 */
                double icapgs, icapgd, icapgb;
                if (MOS2mode > 0)
                    Transistor.DEVqmeyer(vgs, vgd, vgb, von, vdsat, out icapgs, out icapgd, out icapgb, MOS2tPhi, OxideCap);
                else
                    Transistor.DEVqmeyer(vgd, vgs, vgb, von, vdsat, out icapgd, out icapgs, out icapgb, MOS2tPhi, OxideCap);
                state.States[0][MOS2states + MOS2capgs] = icapgs;
                state.States[0][MOS2states + MOS2capgd] = icapgd;
                state.States[0][MOS2states + MOS2capgb] = icapgb;
                vgs1 = state.States[1][MOS2states + MOS2vgs];
                vgd1 = vgs1 - state.States[1][MOS2states + MOS2vds];
                vgb1 = vgs1 - state.States[1][MOS2states + MOS2vbs];
                if (state.Domain == CircuitState.DomainTypes.Time && state.UseDC)
                {
                    capgs = 2 * state.States[0][MOS2states + MOS2capgs] + GateSourceOverlapCap;
                    capgd = 2 * state.States[0][MOS2states + MOS2capgd] + GateDrainOverlapCap;
                    capgb = 2 * state.States[0][MOS2states + MOS2capgb] + GateBulkOverlapCap;
                }
                else
                {
                    capgs = state.States[0][MOS2states + MOS2capgs] + state.States[1][MOS2states + MOS2capgs] + GateSourceOverlapCap;
                    capgd = state.States[0][MOS2states + MOS2capgd] + state.States[1][MOS2states + MOS2capgd] + GateDrainOverlapCap;
                    capgb = state.States[0][MOS2states + MOS2capgb] + state.States[1][MOS2states + MOS2capgb] + GateBulkOverlapCap;
                }

                /*
				*     store small-signal parameters (for meyer's model)
				*  all parameters already stored, so done...
				*/

                /* PREDICTOR */
                if (method != null)
                {
                    state.States[0][MOS2states + MOS2qgs] = (vgs - vgs1) * capgs + state.States[1][MOS2states + MOS2qgs];
                    state.States[0][MOS2states + MOS2qgd] = (vgd - vgd1) * capgd + state.States[1][MOS2states + MOS2qgd];
                    state.States[0][MOS2states + MOS2qgb] = (vgb - vgb1) * capgb + state.States[1][MOS2states + MOS2qgb];
                }
                else
                {
                    /* TRANOP */
                    state.States[0][MOS2states + MOS2qgs] = capgs * vgs;
                    state.States[0][MOS2states + MOS2qgd] = capgd * vgd;
                    state.States[0][MOS2states + MOS2qgb] = capgb * vgb;
                }
                /* PREDICTOR */
            }
            /* NOBYPASS */

            if ((method != null && method.SavedTime == 0.0) || method == null)
            {
                /* initialize to zero charge conductances and current */

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
                    state.States[0][MOS2states + MOS2cqgs] = 0;
                if (capgd == 0)
                    state.States[0][MOS2states + MOS2cqgd] = 0;
                if (capgb == 0)
                    state.States[0][MOS2states + MOS2cqgb] = 0;
                /*
				 *    calculate equivalent conductances and currents for
				 *    meyer"s capacitors
				 */
                method.Integrate(state, out gcgs, out ceqgs, MOS2states + MOS2qgs, capgs);
                method.Integrate(state, out gcgd, out ceqgd, MOS2states + MOS2qgd, capgd);
                method.Integrate(state, out gcgb, out ceqgb, MOS2states + MOS2qgb, capgb);
                ceqgs = ceqgs - gcgs * vgs + method.Slope * state.States[0][MOS2states + MOS2qgs];
                ceqgd = ceqgd - gcgd * vgd + method.Slope * state.States[0][MOS2states + MOS2qgd];
                ceqgb = ceqgb - gcgb * vgb + method.Slope * state.States[0][MOS2states + MOS2qgb];
            }
            /*
			*     store charge storage info for meyer's cap in lx table
			*/

            /*
			*  load current vector
			*/
            ceqbs = Model.MOS2type * (MOS2cbs - (MOS2gbs - state.Gmin) * vbs);
            ceqbd = Model.MOS2type * (MOS2cbd - (MOS2gbd - state.Gmin) * vbd);
            if (MOS2mode >= 0)
            {
                xnrm = 1;
                xrev = 0;
                cdreq = Model.MOS2type * (cdrain - MOS2gds * vds - MOS2gm * vgs - MOS2gmbs * vbs);
            }
            else
            {
                xnrm = 0;
                xrev = 1;
                cdreq = -(Model.MOS2type) * (cdrain - MOS2gds * (-vds) - MOS2gm * vgd - MOS2gmbs * vbd);
            }

            /*
             * Load rhs vector
             */
            rstate.Rhs[MOS2gNode] -= Model.MOS2type * (ceqgs + ceqgb + ceqgd);
            rstate.Rhs[MOS2bNode] -= (ceqbs + ceqbd - Model.MOS2type * ceqgb);
            rstate.Rhs[MOS2dNodePrime] += ceqbd - cdreq + Model.MOS2type * ceqgd;
            rstate.Rhs[MOS2sNodePrime] += cdreq + ceqbs + Model.MOS2type * ceqgs;

            /*
			 *  load y matrix
			 */
            rstate.Matrix[MOS2dNode, MOS2dNode] += (MOS2drainConductance);
            rstate.Matrix[MOS2gNode, MOS2gNode] += gcgd + gcgs + gcgb;
            rstate.Matrix[MOS2sNode, MOS2sNode] += (MOS2sourceConductance);
            rstate.Matrix[MOS2bNode, MOS2bNode] += (MOS2gbd + MOS2gbs + gcgb);
            rstate.Matrix[MOS2dNodePrime, MOS2dNodePrime] += MOS2drainConductance + MOS2gds + MOS2gbd + xrev * (MOS2gm + MOS2gmbs) + gcgd;
            rstate.Matrix[MOS2sNodePrime, MOS2sNodePrime] += MOS2sourceConductance + MOS2gds + MOS2gbs + xnrm * (MOS2gm + MOS2gmbs) + gcgs;
            rstate.Matrix[MOS2dNode, MOS2dNodePrime] -= MOS2drainConductance;
            rstate.Matrix[MOS2gNode, MOS2bNode] -= gcgb;
            rstate.Matrix[MOS2gNode, MOS2dNodePrime] -= gcgd;
            rstate.Matrix[MOS2gNode, MOS2sNodePrime] -= gcgs;
            rstate.Matrix[MOS2sNode, MOS2sNodePrime] -= MOS2sourceConductance;
            rstate.Matrix[MOS2bNode, MOS2gNode] -= gcgb;
            rstate.Matrix[MOS2bNode, MOS2dNodePrime] -= MOS2gbd;
            rstate.Matrix[MOS2bNode, MOS2sNodePrime] -= MOS2gbs;
            rstate.Matrix[MOS2dNodePrime, MOS2dNode] -= MOS2drainConductance;
            rstate.Matrix[MOS2dNodePrime, MOS2gNode] += ((xnrm - xrev) * MOS2gm - gcgd);
            rstate.Matrix[MOS2dNodePrime, MOS2bNode] += (-MOS2gbd + (xnrm - xrev) * MOS2gmbs);
            rstate.Matrix[MOS2dNodePrime, MOS2sNodePrime] -= MOS2gds + xnrm * (MOS2gm + MOS2gmbs);
            rstate.Matrix[MOS2sNodePrime, MOS2gNode] -= (xnrm - xrev) * MOS2gm + gcgs;
            rstate.Matrix[MOS2sNodePrime, MOS2sNode] -= MOS2sourceConductance;
            rstate.Matrix[MOS2sNodePrime, MOS2bNode] -= MOS2gbs + (xnrm - xrev) * MOS2gmbs;
            rstate.Matrix[MOS2sNodePrime, MOS2dNodePrime] -= MOS2gds + xrev * (MOS2gm + MOS2gmbs);
        }

        /// <summary>
        /// Load the device for AC simulation
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void AcLoad(Circuit ckt)
        {
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

            if (MOS2mode < 0)
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
            EffectiveLength = MOS2l - 2 * Model.MOS2latDiff;
            GateSourceOverlapCap = Model.MOS2gateSourceOverlapCapFactor * MOS2w;
            GateDrainOverlapCap = Model.MOS2gateDrainOverlapCapFactor * MOS2w;
            GateBulkOverlapCap = Model.MOS2gateBulkOverlapCapFactor * EffectiveLength;
            capgs = (state.States[0][MOS2states + MOS2capgs] + state.States[0][MOS2states + MOS2capgs] + GateSourceOverlapCap);
            capgd = (state.States[0][MOS2states + MOS2capgd] + state.States[0][MOS2states + MOS2capgd] + GateDrainOverlapCap);
            capgb = (state.States[0][MOS2states + MOS2capgb] + state.States[0][MOS2states + MOS2capgb] + GateBulkOverlapCap);
            xgs = capgs * cstate.Laplace;
            xgd = capgd * cstate.Laplace;
            xgb = capgb * cstate.Laplace;
            xbd = MOS2capbd * cstate.Laplace;
            xbs = MOS2capbs * cstate.Laplace;

            /*
			 *    load matrix
			 */
            cstate.Matrix[MOS2gNode, MOS2gNode] += xgd + xgs + xgb;
            cstate.Matrix[MOS2bNode, MOS2bNode] += xgb + xbd + xbs;
            cstate.Matrix[MOS2dNodePrime, MOS2dNodePrime] += xgd + xbd;
            cstate.Matrix[MOS2sNodePrime, MOS2sNodePrime] += xgs + xbs;
            cstate.Matrix[MOS2gNode, MOS2bNode] -= xgb;
            cstate.Matrix[MOS2gNode, MOS2dNodePrime] -= xgd;
            cstate.Matrix[MOS2gNode, MOS2sNodePrime] -= xgs;
            cstate.Matrix[MOS2bNode, MOS2gNode] -= xgb;
            cstate.Matrix[MOS2bNode, MOS2dNodePrime] -= xbd;
            cstate.Matrix[MOS2bNode, MOS2sNodePrime] -= xbs;
            cstate.Matrix[MOS2dNodePrime, MOS2gNode] -= xgd;
            cstate.Matrix[MOS2dNodePrime, MOS2bNode] -= xbd;
            cstate.Matrix[MOS2sNodePrime, MOS2gNode] -= xgs;
            cstate.Matrix[MOS2sNodePrime, MOS2bNode] -= xbs;
            cstate.Matrix[MOS2dNode, MOS2dNode] += MOS2drainConductance;
            cstate.Matrix[MOS2sNode, MOS2sNode] += MOS2sourceConductance;
            cstate.Matrix[MOS2bNode, MOS2bNode] += MOS2gbd + MOS2gbs + xgb + xbd + xbs;
            cstate.Matrix[MOS2dNodePrime, MOS2dNodePrime] += MOS2drainConductance + MOS2gds + MOS2gbd + xrev * (MOS2gm + MOS2gmbs) + xgd + xbd;
            cstate.Matrix[MOS2sNodePrime, MOS2sNodePrime] += MOS2sourceConductance + MOS2gds + MOS2gbs + xnrm * (MOS2gm + MOS2gmbs) + xgs + xbs;
            cstate.Matrix[MOS2dNode, MOS2dNodePrime] -= MOS2drainConductance;
            cstate.Matrix[MOS2sNode, MOS2sNodePrime] -= MOS2sourceConductance;
            cstate.Matrix[MOS2bNode, MOS2dNodePrime] -= MOS2gbd + xbd;
            cstate.Matrix[MOS2bNode, MOS2sNodePrime] -= MOS2gbs + xbs;
            cstate.Matrix[MOS2dNodePrime, MOS2dNode] -= MOS2drainConductance;
            cstate.Matrix[MOS2dNodePrime, MOS2gNode] += (xnrm - xrev) * MOS2gm - xgd;
            cstate.Matrix[MOS2dNodePrime, MOS2bNode] += -MOS2gbd + (xnrm - xrev) * MOS2gmbs - xbd;
            cstate.Matrix[MOS2dNodePrime, MOS2sNodePrime] -= MOS2gds + xnrm * (MOS2gm + MOS2gmbs);
            cstate.Matrix[MOS2sNodePrime, MOS2gNode] -= (xnrm - xrev) * MOS2gm + xgs;
            cstate.Matrix[MOS2sNodePrime, MOS2sNode] -= MOS2sourceConductance;
            cstate.Matrix[MOS2sNodePrime, MOS2bNode] -= MOS2gbs + (xnrm - xrev) * MOS2gmbs + xbs;
            cstate.Matrix[MOS2sNodePrime, MOS2dNodePrime] -= MOS2gds + xrev * (MOS2gm + MOS2gmbs);
        }
    }
}
