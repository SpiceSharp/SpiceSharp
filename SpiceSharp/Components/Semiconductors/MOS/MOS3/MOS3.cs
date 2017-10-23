using System;
using SpiceSharp.Circuits;
using SpiceSharp.Diagnostics;
using SpiceSharp.Parameters;
using SpiceSharp.Components.Transistors;
using System.Numerics;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A MOS3 Mosfet
    /// Level 2, a semi-empirical model(see reference for level 2).
    /// </summary>
    [SpicePins("Drain", "Gate", "Source", "Bulk"), ConnectedPins(0, 2, 3)]
    public class MOS3 : CircuitComponent<MOS3>
    {
        /// <summary>
        /// Register default behaviour
        /// </summary>
        static MOS3()
        {
            Behaviours.Behaviours.RegisterBehaviour(typeof(MOS3), typeof(ComponentBehaviours.MOS3LoadBehaviour));
            Behaviours.Behaviours.RegisterBehaviour(typeof(MOS3), typeof(ComponentBehaviours.MOS3AcBehaviour));
            Behaviours.Behaviours.RegisterBehaviour(typeof(MOS3), typeof(ComponentBehaviours.MOS3NoiseBehaviour));
        }

        /// <summary>
        /// Set the model for the MOS3 model
        /// </summary>
        public void SetModel(MOS3Model model) => Model = model;

        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("w"), SpiceInfo("Width")]
        public Parameter MOS3w { get; } = new Parameter(1e-4);
        [SpiceName("l"), SpiceInfo("Length")]
        public Parameter MOS3l { get; } = new Parameter(1e-4);
        [SpiceName("as"), SpiceInfo("Source area")]
        public Parameter MOS3sourceArea { get; } = new Parameter();
        [SpiceName("ad"), SpiceInfo("Drain area")]
        public Parameter MOS3drainArea { get; } = new Parameter();
        [SpiceName("ps"), SpiceInfo("Source perimeter")]
        public Parameter MOS3sourcePerimiter { get; } = new Parameter();
        [SpiceName("pd"), SpiceInfo("Drain perimeter")]
        public Parameter MOS3drainPerimiter { get; } = new Parameter();
        [SpiceName("nrs"), SpiceInfo("Source squares")]
        public Parameter MOS3sourceSquares { get; } = new Parameter(1);
        [SpiceName("nrd"), SpiceInfo("Drain squares")]
        public Parameter MOS3drainSquares { get; } = new Parameter(1);
        [SpiceName("off"), SpiceInfo("Device initially off")]
        public bool MOS3off { get; set; }
        [SpiceName("icvbs"), SpiceInfo("Initial B-S voltage")]
        public Parameter MOS3icVBS { get; } = new Parameter();
        [SpiceName("icvds"), SpiceInfo("Initial D-S voltage")]
        public Parameter MOS3icVDS { get; } = new Parameter();
        [SpiceName("icvgs"), SpiceInfo("Initial G-S voltage")]
        public Parameter MOS3icVGS { get; } = new Parameter();
        [SpiceName("temp"), SpiceInfo("Instance operating temperature")]
        public double MOS3_TEMP
        {
            get => MOS3temp - Circuit.CONSTCtoK;
            set => MOS3temp.Set(value + Circuit.CONSTCtoK);
        }
        public Parameter MOS3temp { get; } = new Parameter();
        [SpiceName("dnode"), SpiceInfo("Number of drain node")]
        public int MOS3dNode { get; internal set; }
        [SpiceName("gnode"), SpiceInfo("Number of gate node")]
        public int MOS3gNode { get; internal set; }
        [SpiceName("snode"), SpiceInfo("Number of source node")]
        public int MOS3sNode { get; internal set; }
        [SpiceName("bnode"), SpiceInfo("Number of bulk node")]
        public int MOS3bNode { get; internal set; }
        [SpiceName("dnodeprime"), SpiceInfo("Number of internal drain node")]
        public int MOS3dNodePrime { get; internal set; }
        [SpiceName("snodeprime"), SpiceInfo("Number of internal source node")]
        public int MOS3sNodePrime { get; internal set; }
        [SpiceName("sourceconductance"), SpiceInfo("Source conductance")]
        public double MOS3sourceConductance { get; internal set; }
        [SpiceName("drainconductance"), SpiceInfo("Drain conductance")]
        public double MOS3drainConductance { get; internal set; }
        [SpiceName("von"), SpiceInfo("Turn-on voltage")]
        public double MOS3von { get; internal set; }
        [SpiceName("vdsat"), SpiceInfo("Saturation drain voltage")]
        public double MOS3vdsat { get; internal set; }
        [SpiceName("sourcevcrit"), SpiceInfo("Critical source voltage")]
        public double MOS3sourceVcrit { get; internal set; }
        [SpiceName("drainvcrit"), SpiceInfo("Critical drain voltage")]
        public double MOS3drainVcrit { get; internal set; }
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
        [SpiceName("cbd0"), SpiceInfo("Zero-Bias B-D junction capacitance")]
        public double MOS3Cbd { get; internal set; }
        [SpiceName("cbdsw0"), SpiceInfo("Zero-Bias B-D sidewall capacitance")]
        public double MOS3Cbdsw { get; internal set; }
        [SpiceName("cbs0"), SpiceInfo("Zero-Bias B-S junction capacitance")]
        public double MOS3Cbs { get; internal set; }
        [SpiceName("cbssw0"), SpiceInfo("Zero-Bias B-S sidewall capacitance")]
        public double MOS3Cbssw { get; internal set; }

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
        [SpiceName("rs"), SpiceInfo("Source resistance")]
        public double GetSOURCERESIST(Circuit ckt)
        {
            if (MOS3sNodePrime != MOS3sNode)
                return 1.0 / MOS3sourceConductance;
            else
                return 0.0;
        }
        [SpiceName("rd"), SpiceInfo("Drain resistance")]
        public double GetDRAINRESIST(Circuit ckt)
        {
            if (MOS3dNodePrime != MOS3dNode)
                return 1.0 / MOS3drainConductance;
            else
                return 0.0;
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

            double value = MOS3cd * ckt.State.Real.Solution[MOS3dNode];
            value += (MOS3cbd + MOS3cbs - ckt.State.States[0][MOS3states + MOS3cqgb]) * ckt.State.Real.Solution[MOS3bNode];
            if (ckt.State.Domain == CircuitState.DomainTypes.Time && !ckt.State.UseDC)
            {
                value += (ckt.State.States[0][MOS3states + MOS3cqgb] + ckt.State.States[0][MOS3states + MOS3cqgd] +
                    ckt.State.States[0][MOS3states + MOS3cqgs]) * ckt.State.Real.Solution[MOS3gNode];
            }
            temp = -MOS3cd;
            temp -= MOS3cbd + MOS3cbs;
            if (ckt.State.Domain == CircuitState.DomainTypes.Time && !ckt.State.UseDC)
            {
                temp -= ckt.State.States[0][MOS3states + MOS3cqgb] + ckt.State.States[0][MOS3states + MOS3cqgd] +
                    ckt.State.States[0][MOS3states + MOS3cqgs];
            }
            value += temp * ckt.State.Real.Solution[MOS3sNode];
            return value;
        }

        /// <summary>
        /// Extra variables
        /// </summary>
        public double MOS3mode { get; internal set; }
        public double MOS3tTransconductance { get; internal set; }
        public double MOS3tSurfMob { get; internal set; }
        public double MOS3tPhi { get; internal set; }
        public double MOS3tVbi { get; internal set; }
        public double MOS3tVto { get; internal set; }
        public double MOS3tSatCur { get; internal set; }
        public double MOS3tSatCurDens { get; internal set; }
        public double MOS3tCbd { get; internal set; }
        public double MOS3tCbs { get; internal set; }
        public double MOS3tCj { get; internal set; }
        public double MOS3tCjsw { get; internal set; }
        public double MOS3tBulkPot { get; internal set; }
        public double MOS3tDepCap { get; internal set; }
        public double MOS3f2d { get; internal set; }
        public double MOS3f3d { get; internal set; }
        public double MOS3f4d { get; internal set; }
        public double MOS3f2s { get; internal set; }
        public double MOS3f3s { get; internal set; }
        public double MOS3f4s { get; internal set; }
        public double MOS3cgs { get; internal set; }
        public double MOS3cgd { get; internal set; }
        public double MOS3cgb { get; internal set; }
        public int MOS3states { get; internal set; }

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
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the device</param>
        public MOS3(CircuitIdentifier name) : base(name)
        {
        }

        /// <summary>
        /// Setup the device
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Setup(Circuit ckt)
        {
            var model = Model as MOS3Model;

            // Allocate nodes
            var nodes = BindNodes(ckt);
            MOS3dNode = nodes[0].Index;
            MOS3gNode = nodes[1].Index;
            MOS3sNode = nodes[2].Index;
            MOS3bNode = nodes[3].Index;

            // Allocate states
            MOS3states = ckt.State.GetState(17);

            /* allocate a chunk of the state vector */
            MOS3vdsat = 0;
            MOS3von = 0;
            MOS3mode = 1;

            if (model.MOS3drainResistance != 0 || (model.MOS3sheetResistance != 0 && MOS3drainSquares != 0))
                MOS3dNodePrime = CreateNode(ckt, Name.Grow("#drain")).Index;
            else
                MOS3dNodePrime = MOS3dNode;

            if (model.MOS3sourceResistance != 0 || (model.MOS3sheetResistance != 0 && MOS3sourceSquares != 0))
                MOS3sNodePrime = CreateNode(ckt, Name.Grow("#source")).Index;
            else
                MOS3sNodePrime = MOS3sNode;


        }

        /// <summary>
        /// Do temperature-dependent calculations
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Temperature(Circuit ckt)
        {
            var model = Model as MOS3Model;
            double vt, ratio, fact2, kt, egfet, arg, pbfact, ratio4, phio, pbo, gmaold, capfact, gmanew, czbd, czbdsw, sarg, sargsw, czbs,
                czbssw;

            /* perform the parameter defaulting */

            if (!MOS3temp.Given)
            {
                MOS3temp.Value = ckt.State.Temperature;
            }
            vt = MOS3temp * Circuit.CONSTKoverQ;
            ratio = MOS3temp / model.MOS3tnom;
            fact2 = MOS3temp / Circuit.CONSTRefTemp;
            kt = MOS3temp * Circuit.CONSTBoltz;
            egfet = 1.16 - (7.02e-4 * MOS3temp * MOS3temp) / (MOS3temp + 1108);
            arg = -egfet / (kt + kt) + 1.1150877 / (Circuit.CONSTBoltz * (Circuit.CONSTRefTemp + Circuit.CONSTRefTemp));
            pbfact = -2 * vt * (1.5 * Math.Log(fact2) + Circuit.CHARGE * arg);
            
            if (model.MOS3drainResistance.Given)
            {
                if (model.MOS3drainResistance != 0)
                {
                    MOS3drainConductance = 1 / model.MOS3drainResistance;
                }
                else
                {
                    MOS3drainConductance = 0;
                }
            }
            else if (model.MOS3sheetResistance.Given)
            {
                if (model.MOS3sheetResistance != 0)
                {
                    MOS3drainConductance = 1 / (model.MOS3sheetResistance * MOS3drainSquares);
                }
                else
                {
                    MOS3drainConductance = 0;
                }
            }
            else
            {
                MOS3drainConductance = 0;
            }
            if (model.MOS3sourceResistance.Given)
            {
                if (model.MOS3sourceResistance != 0)
                {
                    MOS3sourceConductance = 1 / model.MOS3sourceResistance;
                }
                else
                {
                    MOS3sourceConductance = 0;
                }
            }
            else if (model.MOS3sheetResistance.Given)
            {
                if (model.MOS3sheetResistance != 0)
                {
                    MOS3sourceConductance = 1 / (model.MOS3sheetResistance * MOS3sourceSquares);
                }
                else
                {
                    MOS3sourceConductance = 0;
                }
            }
            else
            {
                MOS3sourceConductance = 0;
            }

            if (MOS3l - 2 * model.MOS3latDiff <= 0)
                throw new CircuitException($"{Name}: effective channel length less than zero");
            ratio4 = ratio * Math.Sqrt(ratio);
            MOS3tTransconductance = model.MOS3transconductance / ratio4;
            MOS3tSurfMob = model.MOS3surfaceMobility / ratio4;
            phio = (model.MOS3phi - model.pbfact1) / model.fact1;
            MOS3tPhi = fact2 * phio + pbfact;
            MOS3tVbi = model.MOS3vt0 - model.MOS3type * (model.MOS3gamma * Math.Sqrt(model.MOS3phi)) + .5 * (model.egfet1 - egfet) +
                model.MOS3type * .5 * (MOS3tPhi - model.MOS3phi);
            MOS3tVto = MOS3tVbi + model.MOS3type * model.MOS3gamma * Math.Sqrt(MOS3tPhi);
            MOS3tSatCur = model.MOS3jctSatCur * Math.Exp(-egfet / vt + model.egfet1 / model.vtnom);
            MOS3tSatCurDens = model.MOS3jctSatCurDensity * Math.Exp(-egfet / vt + model.egfet1 / model.vtnom);
            pbo = (model.MOS3bulkJctPotential - model.pbfact1) / model.fact1;
            gmaold = (model.MOS3bulkJctPotential - pbo) / pbo;
            capfact = 1 / (1 + model.MOS3bulkJctBotGradingCoeff * (4e-4 * (model.MOS3tnom - Circuit.CONSTRefTemp) - gmaold));
            MOS3tCbd = model.MOS3capBD * capfact;
            MOS3tCbs = model.MOS3capBS * capfact;
            MOS3tCj = model.MOS3bulkCapFactor * capfact;
            capfact = 1 / (1 + model.MOS3bulkJctSideGradingCoeff * (4e-4 * (model.MOS3tnom - Circuit.CONSTRefTemp) - gmaold));
            MOS3tCjsw = model.MOS3sideWallCapFactor * capfact;
            MOS3tBulkPot = fact2 * pbo + pbfact;
            gmanew = (MOS3tBulkPot - pbo) / pbo;
            capfact = (1 + model.MOS3bulkJctBotGradingCoeff * (4e-4 * (MOS3temp - Circuit.CONSTRefTemp) - gmanew));
            MOS3tCbd *= capfact;
            MOS3tCbs *= capfact;
            MOS3tCj *= capfact;
            capfact = (1 + model.MOS3bulkJctSideGradingCoeff * (4e-4 * (MOS3temp - Circuit.CONSTRefTemp) - gmanew));
            MOS3tCjsw *= capfact;
            MOS3tDepCap = model.MOS3fwdCapDepCoeff * MOS3tBulkPot;

            if ((model.MOS3jctSatCurDensity.Value == 0) || (MOS3drainArea.Value == 0) || (MOS3sourceArea.Value == 0))
            {
                MOS3sourceVcrit = MOS3drainVcrit = vt * Math.Log(vt / (Circuit.CONSTroot2 * model.MOS3jctSatCur));
            }
            else
            {
                MOS3drainVcrit = vt * Math.Log(vt / (Circuit.CONSTroot2 * model.MOS3jctSatCurDensity * MOS3drainArea));
                MOS3sourceVcrit = vt * Math.Log(vt / (Circuit.CONSTroot2 * model.MOS3jctSatCurDensity * MOS3sourceArea));
            }
            if (model.MOS3capBD.Given)
            {
                czbd = MOS3tCbd;
            }
            else
            {
                if (model.MOS3bulkCapFactor.Given)
                {
                    czbd = MOS3tCj * MOS3drainArea;
                }
                else
                {
                    czbd = 0;
                }
            }
            if (model.MOS3sideWallCapFactor.Given)
            {
                czbdsw = MOS3tCjsw * MOS3drainPerimiter;
            }
            else
            {
                czbdsw = 0;
            }
            arg = 1 - model.MOS3fwdCapDepCoeff;
            sarg = Math.Exp((-model.MOS3bulkJctBotGradingCoeff) * Math.Log(arg));
            sargsw = Math.Exp((-model.MOS3bulkJctSideGradingCoeff) * Math.Log(arg));
            MOS3Cbd = czbd;
            MOS3Cbdsw = czbdsw;
            MOS3f2d = czbd * (1 - model.MOS3fwdCapDepCoeff * (1 + model.MOS3bulkJctBotGradingCoeff)) * sarg / arg + czbdsw * (1 -
                model.MOS3fwdCapDepCoeff * (1 + model.MOS3bulkJctSideGradingCoeff)) * sargsw / arg;
            MOS3f3d = czbd * model.MOS3bulkJctBotGradingCoeff * sarg / arg / model.MOS3bulkJctPotential + czbdsw *
                model.MOS3bulkJctSideGradingCoeff * sargsw / arg / model.MOS3bulkJctPotential;
            MOS3f4d = czbd * model.MOS3bulkJctPotential * (1 - arg * sarg) / (1 - model.MOS3bulkJctBotGradingCoeff) + czbdsw *
                model.MOS3bulkJctPotential * (1 - arg * sargsw) / (1 - model.MOS3bulkJctSideGradingCoeff) - MOS3f3d / 2 * (MOS3tDepCap *
                MOS3tDepCap) - MOS3tDepCap * MOS3f2d;
            if (model.MOS3capBS.Given)
            {
                czbs = MOS3tCbs;
            }
            else
            {
                if (model.MOS3bulkCapFactor.Given)
                {
                    czbs = MOS3tCj * MOS3sourceArea;
                }
                else
                {
                    czbs = 0;
                }
            }
            if (model.MOS3sideWallCapFactor.Given)
            {
                czbssw = MOS3tCjsw * MOS3sourcePerimiter;
            }
            else
            {
                czbssw = 0;
            }
            arg = 1 - model.MOS3fwdCapDepCoeff;
            sarg = Math.Exp((-model.MOS3bulkJctBotGradingCoeff) * Math.Log(arg));
            sargsw = Math.Exp((-model.MOS3bulkJctSideGradingCoeff) * Math.Log(arg));
            MOS3Cbs = czbs;
            MOS3Cbssw = czbssw;
            MOS3f2s = czbs * (1 - model.MOS3fwdCapDepCoeff * (1 + model.MOS3bulkJctBotGradingCoeff)) * sarg / arg + czbssw * (1 -
                model.MOS3fwdCapDepCoeff * (1 + model.MOS3bulkJctSideGradingCoeff)) * sargsw / arg;
            MOS3f3s = czbs * model.MOS3bulkJctBotGradingCoeff * sarg / arg / model.MOS3bulkJctPotential + czbssw *
                model.MOS3bulkJctSideGradingCoeff * sargsw / arg / model.MOS3bulkJctPotential;
            MOS3f4s = czbs * model.MOS3bulkJctPotential * (1 - arg * sarg) / (1 - model.MOS3bulkJctBotGradingCoeff) + czbssw *
                model.MOS3bulkJctPotential * (1 - arg * sargsw) / (1 - model.MOS3bulkJctSideGradingCoeff) - MOS3f3s / 2 * (MOS3tBulkPot *
                MOS3tBulkPot) - MOS3tBulkPot * MOS3f2s;
        }

        /// <summary>
        /// Truncate the timestep
        /// </summary>
        /// <param name="ckt">The circuit</param>
        /// <param name="timeStep">The timestep</param>
        public override void Truncate(Circuit ckt, ref double timeStep)
        {
            var method = ckt.Method;
            method.Terr(MOS3states + MOS3qgs, ckt, ref timeStep);
            method.Terr(MOS3states + MOS3qgd, ckt, ref timeStep);
            method.Terr(MOS3states + MOS3qgb, ckt, ref timeStep);
        }
    }
}
