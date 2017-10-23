using System;
using SpiceSharp.Circuits;
using SpiceSharp.Diagnostics;
using SpiceSharp.Parameters;
using SpiceSharp.Components.Transistors;
using System.Numerics;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A MOS1 Mosfet.
    /// Level 1, Shichman-Hodges.
    /// </summary>
    [SpicePins("Drain", "Gate", "Source", "Bulk"), ConnectedPins(0, 2, 3)]
    public class MOS1 : CircuitComponent<MOS1>
    {
        /// <summary>
        /// Register default behaviours
        /// </summary>
        static MOS1()
        {
            Behaviours.Behaviours.RegisterBehaviour(typeof(MOS1), typeof(ComponentBehaviours.MOS1LoadBehaviour));
            Behaviours.Behaviours.RegisterBehaviour(typeof(MOS1), typeof(ComponentBehaviours.MOS1AcBehaviour));
            Behaviours.Behaviours.RegisterBehaviour(typeof(MOS1), typeof(ComponentBehaviours.MOS1NoiseBehaviour));
        }

        /// <summary>
        /// Set the model for the MOS1 Mosfet
        /// </summary>
        public void SetModel(MOS1Model model) => Model = model;

        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("temp"), SpiceInfo("Instance temperature")]
        public double MOS1_TEMP
        {
            get => MOS1temp - Circuit.CONSTCtoK;
            set => MOS1temp.Set(value + Circuit.CONSTCtoK);
        }
        public Parameter MOS1temp { get; } = new Parameter();
        [SpiceName("w"), SpiceInfo("Width")]
        public Parameter MOS1w { get; } = new Parameter(1e-4);
        [SpiceName("l"), SpiceInfo("Length")]
        public Parameter MOS1l { get; } = new Parameter(1e-4);
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
        public int MOS1dNode { get; internal set; }
        [SpiceName("gnode"), SpiceInfo("Number of the gate node ")]
        public int MOS1gNode { get; internal set; }
        [SpiceName("snode"), SpiceInfo("Number of the source node ")]
        public int MOS1sNode { get; internal set; }
        [SpiceName("bnode"), SpiceInfo("Number of the node ")]
        public int MOS1bNode { get; internal set; }
        [SpiceName("dnodeprime"), SpiceInfo("Number of int. drain node")]
        public int MOS1dNodePrime { get; internal set; }
        [SpiceName("snodeprime"), SpiceInfo("Number of int. source node ")]
        public int MOS1sNodePrime { get; internal set; }
        [SpiceName("sourceconductance"), SpiceInfo("Conductance of source")]
        public double MOS1sourceConductance { get; internal set; }
        [SpiceName("drainconductance"), SpiceInfo("Conductance of drain")]
        public double MOS1drainConductance { get; internal set; }
        [SpiceName("von"), SpiceInfo(" ")]
        public double MOS1von { get; internal set; }
        [SpiceName("vdsat"), SpiceInfo("Saturation drain voltage")]
        public double MOS1vdsat { get; internal set; }
        [SpiceName("sourcevcrit"), SpiceInfo("Critical source voltage")]
        public double MOS1sourceVcrit { get; internal set; }
        [SpiceName("drainvcrit"), SpiceInfo("Critical drain voltage")]
        public double MOS1drainVcrit { get; internal set; }
        [SpiceName("id"), SpiceInfo("Drain current")]
        public double MOS1cd { get; internal set; }
        [SpiceName("ibs"), SpiceInfo("B-S junction current")]
        public double MOS1cbs { get; internal set; }
        [SpiceName("ibd"), SpiceInfo("B-D junction current")]
        public double MOS1cbd { get; internal set; }
        [SpiceName("gmb"), SpiceName("gmbs"), SpiceInfo("Bulk-Source transconductance")]
        public double MOS1gmbs { get; internal set; }
        [SpiceName("gm"), SpiceInfo("Transconductance")]
        public double MOS1gm { get; internal set; }
        [SpiceName("gds"), SpiceInfo("Drain-Source conductance")]
        public double MOS1gds { get; internal set; }
        [SpiceName("gbd"), SpiceInfo("Bulk-Drain conductance")]
        public double MOS1gbd { get; internal set; }
        [SpiceName("gbs"), SpiceInfo("Bulk-Source conductance")]
        public double MOS1gbs { get; internal set; }
        [SpiceName("cbd"), SpiceInfo("Bulk-Drain capacitance")]
        public double MOS1capbd { get; internal set; }
        [SpiceName("cbs"), SpiceInfo("Bulk-Source capacitance")]
        public double MOS1capbs { get; internal set; }
        [SpiceName("cbd0"), SpiceInfo("Zero-Bias B-D junction capacitance")]
        public double MOS1Cbd { get; internal set; }
        [SpiceName("cbdsw0"), SpiceInfo(" ")]
        public double MOS1Cbdsw { get; internal set; }
        [SpiceName("cbs0"), SpiceInfo("Zero-Bias B-S junction capacitance")]
        public double MOS1Cbs { get; internal set; }
        [SpiceName("cbssw0"), SpiceInfo(" ")]
        public double MOS1Cbssw { get; internal set; }

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
            if (ckt.Method != null && !(ckt.State.Domain == CircuitState.DomainTypes.Time && ckt.State.UseDC))
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
            double value = MOS1cd * ckt.State.Real.Solution[MOS1dNode];
            value += (MOS1cbd + MOS1cbs - ckt.State.States[0][MOS1states + MOS1cqgb]) * ckt.State.Real.Solution[MOS1bNode];
            if (ckt.Method != null && !(ckt.State.Domain == CircuitState.DomainTypes.Time && ckt.State.UseDC))
            {
                value += (ckt.State.States[0][MOS1states + MOS1cqgb] + ckt.State.States[0][MOS1states + MOS1cqgd] +
                    ckt.State.States[0][MOS1states + MOS1cqgs]) * ckt.State.Real.Solution[MOS1gNode];
            }
            temp = -MOS1cd;
            temp -= MOS1cbd + MOS1cbs;
            if (ckt.Method != null && !(ckt.State.Domain == CircuitState.DomainTypes.Time && ckt.State.UseDC))
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
        public double MOS1tTransconductance { get; internal set; }
        public double MOS1tSurfMob { get; internal set; }
        public double MOS1tPhi { get; internal set; }
        public double MOS1tVbi { get; internal set; }
        public double MOS1tVto { get; internal set; }
        public double MOS1tSatCur { get; internal set; }
        public double MOS1tSatCurDens { get; internal set; }
        public double MOS1tCbd { get; internal set; }
        public double MOS1tCbs { get; internal set; }
        public double MOS1tCj { get; internal set; }
        public double MOS1tCjsw { get; internal set; }
        public double MOS1tBulkPot { get; internal set; }
        public double MOS1tDepCap { get; internal set; }
        public double MOS1f2d { get; internal set; }
        public double MOS1f3d { get; internal set; }
        public double MOS1f4d { get; internal set; }
        public double MOS1f2s { get; internal set; }
        public double MOS1f3s { get; internal set; }
        public double MOS1f4s { get; internal set; }
        public double MOS1mode { get; internal set; }
        public double MOS1cgs { get; internal set; }
        public double MOS1cgd { get; internal set; }
        public double MOS1cgb { get; internal set; }
        public int MOS1states { get; internal set; }

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
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the device</param>
        public MOS1(CircuitIdentifier name) : base(name)
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
            MOS1vdsat = 0.0;
            MOS1von = 0.0;

            if (model.MOS1drainResistance != 0 || (model.MOS1sheetResistance != 0 && MOS1drainSquares != 0))
                MOS1dNodePrime = CreateNode(ckt, Name.Grow("#drain")).Index;
            else
                MOS1dNodePrime = MOS1dNode;

            if (model.MOS1sourceResistance != 0 || (model.MOS1sheetResistance != 0 && MOS1sourceSquares != 0))
                MOS1sNodePrime = CreateNode(ckt, Name.Grow("#source")).Index;
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
            double vt, ratio, fact2, kt, egfet, arg, pbfact, ratio4, phio, pbo, gmaold, capfact, gmanew, czbd, czbdsw, sarg, sargsw, czbs,
                czbssw;

            /* perform the parameter defaulting */
            if (!MOS1temp.Given)
            {
                MOS1temp.Value = ckt.State.Temperature;
            }
            vt = MOS1temp * Circuit.CONSTKoverQ;
            ratio = MOS1temp / model.MOS1tnom;
            fact2 = MOS1temp / Circuit.CONSTRefTemp;
            kt = MOS1temp * Circuit.CONSTBoltz;
            egfet = 1.16 - (7.02e-4 * MOS1temp * MOS1temp) / (MOS1temp + 1108);
            arg = -egfet / (kt + kt) + 1.1150877 / (Circuit.CONSTBoltz * (Circuit.CONSTRefTemp + Circuit.CONSTRefTemp));
            pbfact = -2 * vt * (1.5 * Math.Log(fact2) + Circuit.CHARGE * arg);

            if (MOS1l - 2 * model.MOS1latDiff <= 0)
                CircuitWarning.Warning(this, $"{Name}: effective channel length less than zero");
            ratio4 = ratio * Math.Sqrt(ratio);
            MOS1tTransconductance = model.MOS1transconductance / ratio4;
            MOS1tSurfMob = model.MOS1surfaceMobility / ratio4;
            phio = (model.MOS1phi - model.pbfact1) / model.fact1;
            MOS1tPhi = fact2 * phio + pbfact;
            MOS1tVbi = model.MOS1vt0 - model.MOS1type * (model.MOS1gamma * Math.Sqrt(model.MOS1phi)) + .5 * (model.egfet1 - egfet) +
                model.MOS1type * .5 * (MOS1tPhi - model.MOS1phi);
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
            if ((MOS1tSatCurDens == 0) || (MOS1drainArea.Value == 0) || (MOS1sourceArea.Value == 0))
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
            MOS1f2d = czbd * (1 - model.MOS1fwdCapDepCoeff * (1 + model.MOS1bulkJctBotGradingCoeff)) * sarg / arg + czbdsw * (1 -
                model.MOS1fwdCapDepCoeff * (1 + model.MOS1bulkJctSideGradingCoeff)) * sargsw / arg;
            MOS1f3d = czbd * model.MOS1bulkJctBotGradingCoeff * sarg / arg / MOS1tBulkPot + czbdsw * model.MOS1bulkJctSideGradingCoeff *
                sargsw / arg / MOS1tBulkPot;
            MOS1f4d = czbd * MOS1tBulkPot * (1 - arg * sarg) / (1 - model.MOS1bulkJctBotGradingCoeff) + czbdsw * MOS1tBulkPot * (1 - arg *
                sargsw) / (1 - model.MOS1bulkJctSideGradingCoeff) - MOS1f3d / 2 * (MOS1tDepCap * MOS1tDepCap) - MOS1tDepCap * MOS1f2d;
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
            MOS1f2s = czbs * (1 - model.MOS1fwdCapDepCoeff * (1 + model.MOS1bulkJctBotGradingCoeff)) * sarg / arg + czbssw * (1 -
                model.MOS1fwdCapDepCoeff * (1 + model.MOS1bulkJctSideGradingCoeff)) * sargsw / arg;
            MOS1f3s = czbs * model.MOS1bulkJctBotGradingCoeff * sarg / arg / MOS1tBulkPot + czbssw * model.MOS1bulkJctSideGradingCoeff *
                sargsw / arg / MOS1tBulkPot;
            MOS1f4s = czbs * MOS1tBulkPot * (1 - arg * sarg) / (1 - model.MOS1bulkJctBotGradingCoeff) + czbssw * MOS1tBulkPot * (1 - arg *
                sargsw) / (1 - model.MOS1bulkJctSideGradingCoeff) - MOS1f3s / 2 * (MOS1tDepCap * MOS1tDepCap) - MOS1tDepCap * MOS1f2s;

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
                    MOS1drainConductance = 1 / (model.MOS1sheetResistance * MOS1drainSquares);
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
                    MOS1sourceConductance = 1 / (model.MOS1sheetResistance * MOS1sourceSquares);
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
        /// Truncate
        /// </summary>
        /// <param name="ckt">Circuit</param>
        /// <param name="timeStep">Timestep</param>
        public override void Truncate(Circuit ckt, ref double timeStep)
        {
            var method = ckt.Method;
            method.Terr(MOS1states + MOS1qgs, ckt, ref timeStep);
            method.Terr(MOS1states + MOS1qgd, ckt, ref timeStep);
            method.Terr(MOS1states + MOS1qgb, ckt, ref timeStep);
        }
    }
}
