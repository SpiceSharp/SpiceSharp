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
        public int MOS3dNode { get; private set; }
        [SpiceName("gnode"), SpiceInfo("Number of gate node")]
        public int MOS3gNode { get; private set; }
        [SpiceName("snode"), SpiceInfo("Number of source node")]
        public int MOS3sNode { get; private set; }
        [SpiceName("bnode"), SpiceInfo("Number of bulk node")]
        public int MOS3bNode { get; private set; }
        [SpiceName("dnodeprime"), SpiceInfo("Number of internal drain node")]
        public int MOS3dNodePrime { get; private set; }
        [SpiceName("snodeprime"), SpiceInfo("Number of internal source node")]
        public int MOS3sNodePrime { get; private set; }
        [SpiceName("sourceconductance"), SpiceInfo("Source conductance")]
        public double MOS3sourceConductance { get; private set; }
        [SpiceName("drainconductance"), SpiceInfo("Drain conductance")]
        public double MOS3drainConductance { get; private set; }
        [SpiceName("von"), SpiceInfo("Turn-on voltage")]
        public double MOS3von { get; private set; }
        [SpiceName("vdsat"), SpiceInfo("Saturation drain voltage")]
        public double MOS3vdsat { get; private set; }
        [SpiceName("sourcevcrit"), SpiceInfo("Critical source voltage")]
        public double MOS3sourceVcrit { get; private set; }
        [SpiceName("drainvcrit"), SpiceInfo("Critical drain voltage")]
        public double MOS3drainVcrit { get; private set; }
        [SpiceName("id"), SpiceName("cd"), SpiceInfo("Drain current")]
        public double MOS3cd { get; private set; }
        [SpiceName("ibs"), SpiceInfo("B-S junction current")]
        public double MOS3cbs { get; private set; }
        [SpiceName("ibd"), SpiceInfo("B-D junction current")]
        public double MOS3cbd { get; private set; }
        [SpiceName("gmb"), SpiceName("gmbs"), SpiceInfo("Bulk-Source transconductance")]
        public double MOS3gmbs { get; private set; }
        [SpiceName("gm"), SpiceInfo("Transconductance")]
        public double MOS3gm { get; private set; }
        [SpiceName("gds"), SpiceInfo("Drain-Source conductance")]
        public double MOS3gds { get; private set; }
        [SpiceName("gbd"), SpiceInfo("Bulk-Drain conductance")]
        public double MOS3gbd { get; private set; }
        [SpiceName("gbs"), SpiceInfo("Bulk-Source conductance")]
        public double MOS3gbs { get; private set; }
        [SpiceName("cbd"), SpiceInfo("Bulk-Drain capacitance")]
        public double MOS3capbd { get; private set; }
        [SpiceName("cbs"), SpiceInfo("Bulk-Source capacitance")]
        public double MOS3capbs { get; private set; }
        [SpiceName("cbd0"), SpiceInfo("Zero-Bias B-D junction capacitance")]
        public double MOS3Cbd { get; private set; }
        [SpiceName("cbdsw0"), SpiceInfo("Zero-Bias B-D sidewall capacitance")]
        public double MOS3Cbdsw { get; private set; }
        [SpiceName("cbs0"), SpiceInfo("Zero-Bias B-S junction capacitance")]
        public double MOS3Cbs { get; private set; }
        [SpiceName("cbssw0"), SpiceInfo("Zero-Bias B-S sidewall capacitance")]
        public double MOS3Cbssw { get; private set; }

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
        public double MOS3mode { get; private set; }
        public double MOS3tTransconductance { get; private set; }
        public double MOS3tSurfMob { get; private set; }
        public double MOS3tPhi { get; private set; }
        public double MOS3tVbi { get; private set; }
        public double MOS3tVto { get; private set; }
        public double MOS3tSatCur { get; private set; }
        public double MOS3tSatCurDens { get; private set; }
        public double MOS3tCbd { get; private set; }
        public double MOS3tCbs { get; private set; }
        public double MOS3tCj { get; private set; }
        public double MOS3tCjsw { get; private set; }
        public double MOS3tBulkPot { get; private set; }
        public double MOS3tDepCap { get; private set; }
        public double MOS3f2d { get; private set; }
        public double MOS3f3d { get; private set; }
        public double MOS3f4d { get; private set; }
        public double MOS3f2s { get; private set; }
        public double MOS3f3s { get; private set; }
        public double MOS3f4s { get; private set; }
        public double MOS3cgs { get; private set; }
        public double MOS3cgd { get; private set; }
        public double MOS3cgb { get; private set; }
        public int MOS3states { get; private set; }

        /// <summary>
        /// Noise generators
        /// </summary>
        public ComponentNoise MOS3noise { get; } = new ComponentNoise(
            new Noise.NoiseThermal("rd", 0, 4),
            new Noise.NoiseThermal("rs", 2, 5),
            new Noise.NoiseThermal("id", 4, 5),
            new Noise.NoiseGain("1overf", 4, 5)
            );

        /// <summary>
        /// Constants
        /// </summary>
        private const int MOS3vbd = 0;
        private const int MOS3vbs = 1;
        private const int MOS3vgs = 2;
        private const int MOS3vds = 3;
        private const int MOS3capgs = 4;
        private const int MOS3qgs = 5;
        private const int MOS3cqgs = 6;
        private const int MOS3capgd = 7;
        private const int MOS3qgd = 8;
        private const int MOS3cqgd = 9;
        private const int MOS3capgb = 10;
        private const int MOS3qgb = 11;
        private const int MOS3cqgb = 12;
        private const int MOS3qbd = 13;
        private const int MOS3cqbd = 14;
        private const int MOS3qbs = 15;
        private const int MOS3cqbs = 16;

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

            MOS3noise.Setup(ckt,
                MOS3dNode,
                MOS3gNode,
                MOS3sNode,
                MOS3bNode,
                MOS3dNodePrime,
                MOS3sNodePrime);
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
        /// Load the device
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Load(Circuit ckt)
        {
            var model = Model as MOS3Model;
            var state = ckt.State;
            var rstate = state.Real;
            var method = ckt.Method;
            double vt, EffectiveLength, DrainSatCur, SourceSatCur, GateSourceOverlapCap, GateDrainOverlapCap, GateBulkOverlapCap, Beta,
                OxideCap, vgs, vds, vbs, vbd, vgb, vgd, vgdo, delvbs, delvbd, delvgs, delvds, delvgd, cdhat, cbhat, von, evbs, evbd, vdsat, 
                cdrain, sargsw, vgs1, vgd1, vgb1, capgs = 0.0, capgd = 0.0, capgb = 0.0, gcgs, ceqgs, gcgd, ceqgd, gcgb, ceqgb, ceqbs, ceqbd, cdreq;
            int Check, xnrm, xrev;

            vt = Circuit.CONSTKoverQ * MOS3temp;
            Check = 1;

            /* DETAILPROF */

            /* first, we compute a few useful values - these could be
			* pre - computed, but for historical reasons are still done
			* here.  They may be moved at the expense of instance size
			*/

            EffectiveLength = MOS3l - 2 * model.MOS3latDiff;
            if ((MOS3tSatCurDens == 0) || (MOS3drainArea.Value == 0) || (MOS3sourceArea.Value == 0))
            {
                DrainSatCur = MOS3tSatCur;
                SourceSatCur = MOS3tSatCur;
            }
            else
            {
                DrainSatCur = MOS3tSatCurDens * MOS3drainArea;
                SourceSatCur = MOS3tSatCurDens * MOS3sourceArea;
            }
            GateSourceOverlapCap = model.MOS3gateSourceOverlapCapFactor * MOS3w;
            GateDrainOverlapCap = model.MOS3gateDrainOverlapCapFactor * MOS3w;
            GateBulkOverlapCap = model.MOS3gateBulkOverlapCapFactor * EffectiveLength;
            Beta = MOS3tTransconductance * MOS3w / EffectiveLength;
            OxideCap = model.MOS3oxideCapFactor * EffectiveLength * MOS3w;
            
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

            if ((state.Init == CircuitState.InitFlags.InitFloat || state.UseSmallSignal || (method != null && method.SavedTime == 0.0)) ||
                ((state.Init == CircuitState.InitFlags.InitFix) && (!MOS3off)))
            {
                /* PREDICTOR */

                /* general iteration */

                vbs = model.MOS3type * (rstate.OldSolution[MOS3bNode] - rstate.OldSolution[MOS3sNodePrime]);
                vgs = model.MOS3type * (rstate.OldSolution[MOS3gNode] - rstate.OldSolution[MOS3sNodePrime]);
                vds = model.MOS3type * (rstate.OldSolution[MOS3dNodePrime] - rstate.OldSolution[MOS3sNodePrime]);
                /* PREDICTOR */

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

                von = model.MOS3type * MOS3von;

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
                    vbs = Transistor.DEVpnjlim(vbs, state.States[0][MOS3states + MOS3vbs], vt, MOS3sourceVcrit, ref Check);
                    vbd = vbs - vds;
                }
                else
                {
                    vbd = Transistor.DEVpnjlim(vbd, state.States[0][MOS3states + MOS3vbd], vt, MOS3drainVcrit, ref Check);
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
                    vds = model.MOS3type * MOS3icVDS;
                    vgs = model.MOS3type * MOS3icVGS;
                    vbs = model.MOS3type * MOS3icVBS;
                    if ((vds == 0) && (vgs == 0) && (vbs == 0) && ((method != null || state.UseDC ||
                        state.Domain == CircuitState.DomainTypes.None) || (!state.UseIC)))
                    {
                        vbs = -1;
                        vgs = model.MOS3type * MOS3tVto;
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
                eta = model.MOS3eta * 8.15e-22 / (model.MOS3oxideCapFactor * EffectiveLength * EffectiveLength * EffectiveLength);
                /* 
				* .....square root term
				*/
                if ((MOS3mode == 1 ? vbs : vbd) <= 0.0)
                {
                    phibs = MOS3tPhi - (MOS3mode == 1 ? vbs : vbd);
                    sqphbs = Math.Sqrt(phibs);
                    dsqdvb = -0.5 / sqphbs;
                }
                else
                {
                    sqphis = Math.Sqrt(MOS3tPhi);
                    sqphs3 = MOS3tPhi * sqphis;
                    sqphbs = sqphis / (1.0 + (MOS3mode == 1 ? vbs : vbd) / (MOS3tPhi + MOS3tPhi));
                    phibs = sqphbs * sqphbs;
                    dsqdvb = -phibs / (sqphs3 + sqphs3);
                }
                /* 
				 * .....short channel effect factor
				 */
                if ((model.MOS3junctionDepth != 0.0) && (model.MOS3coeffDepLayWidth != 0.0))
                {
                    wps = model.MOS3coeffDepLayWidth * sqphbs;
                    oneoverxj = 1.0 / model.MOS3junctionDepth;
                    xjonxl = model.MOS3junctionDepth * oneoverxl;
                    djonxj = model.MOS3latDiff * oneoverxj;
                    wponxj = wps * oneoverxj;
                    wconxj = coeff0 + coeff1 * wponxj + coeff2 * wponxj * wponxj;
                    arga = wconxj + djonxj;
                    argc = wponxj / (1.0 + wponxj);
                    argb = Math.Sqrt(1.0 - argc * argc);
                    fshort = 1.0 - xjonxl * (arga * argb - djonxj);
                    dwpdvb = model.MOS3coeffDepLayWidth * dsqdvb;
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
                gammas = model.MOS3gamma * fshort;
                fbodys = 0.5 * gammas / (sqphbs + sqphbs);
                fbody = fbodys + model.MOS3narrowFactor / MOS3w;
                onfbdy = 1.0 / (1.0 + fbody);
                dfbdvb = -fbodys * dsqdvb / sqphbs + fbodys * dfsdvb / fshort;
                qbonco = gammas * sqphbs + model.MOS3narrowFactor * phibs / MOS3w;
                dqbdvb = gammas * dsqdvb + model.MOS3gamma * dfsdvb * sqphbs - model.MOS3narrowFactor / MOS3w;
                /* 
				 * .....static feedback effect
				 */
                vbix = MOS3tVbi * model.MOS3type - eta * (MOS3mode * vds);
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
                if (model.MOS3fastSurfaceStateDensity != 0.0)
                {
                    csonco = Circuit.CHARGE * model.MOS3fastSurfaceStateDensity * 1e4 /* (cm *  * 2 / m *  * 2) */  * EffectiveLength * MOS3w /
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
                onfg = 1.0 + model.MOS3theta * (vgsx - vth);
                fgate = 1.0 / onfg;
                us = MOS3tSurfMob * 1e-4 /*(m**2/cm**2)*/ * fgate;
                dfgdvg = -model.MOS3theta * fgate * fgate;
                dfgdvd = -dfgdvg * dvtdvd;
                dfgdvb = -dfgdvg * dvtdvb;
                /* 
				 * .....saturation voltage
				 */
                vdsat = (vgsx - vth) * onfbdy;
                if (model.MOS3maxDriftVel <= 0.0)
                {
                    dvsdvg = onfbdy;
                    dvsdvd = -dvsdvg * dvtdvd;
                    dvsdvb = -dvsdvg * dvtdvb - vdsat * dfbdvb * onfbdy;
                }
                else
                {
                    vdsc = EffectiveLength * model.MOS3maxDriftVel / us;
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
                if (model.MOS3maxDriftVel != 0.0)
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
                if (model.MOS3maxDriftVel <= 0.0) goto line510;
                if (model.MOS3alpha == 0.0)
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

                emax = model.MOS3kappa * cdsat * oneoverxl / gdsat;
                emoncd = emax / cdsat;
                emongd = emax / gdsat;
                demdvg = emoncd * MOS3gm - emongd * dgdvg;
                demdvd = emoncd * MOS3gds - emongd * dgdvd;
                demdvb = emoncd * MOS3gmbs - emongd * dgdvb;

                arga = 0.5 * emax * model.MOS3alpha;
                argc = model.MOS3kappa * model.MOS3alpha;
                argb = Math.Sqrt(arga * arga + argc * ((MOS3mode * vds) - vdsat));
                delxl = argb - arga;
                dldvd = argc / (argb + argb);
                dldem = 0.5 * (arga / argb - 1.0) * model.MOS3alpha;
                ddldvg = dldem * demdvg;
                ddldvd = dldem * demdvd - dldvd;
                ddldvb = dldem * demdvb;
                goto line520;
                line510:
                delxl = Math.Sqrt(model.MOS3kappa * ((MOS3mode * vds) - vdsat) * model.MOS3alpha);
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
                if ((model.MOS3fastSurfaceStateDensity != 0.0) && ((MOS3mode == 1 ? vgs : vgd) < von))
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

            MOS3von = model.MOS3type * von;
            MOS3vdsat = model.MOS3type * vdsat;
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
                    if (vbs < MOS3tDepCap)
                    {
                        double arg = 1 - vbs / MOS3tBulkPot, sarg;
                        /* 
						* the following block looks somewhat long and messy, 
						* but since most users use the default grading
						* coefficients of .5, and sqrt is MUCH faster than an
						* Math.Exp(Math.Log()) we use this special case code to buy time.
						* (as much as 10% of total job time!)
						*/
                        if (model.MOS3bulkJctBotGradingCoeff.Value == model.MOS3bulkJctSideGradingCoeff)
                        {
                            if (model.MOS3bulkJctBotGradingCoeff.Value == .5)
                            {
                                sarg = sargsw = 1 / Math.Sqrt(arg);
                            }
                            else
                            {
                                sarg = sargsw = Math.Exp(-model.MOS3bulkJctBotGradingCoeff * Math.Log(arg));
                            }
                        }
                        else
                        {
                            if (model.MOS3bulkJctBotGradingCoeff.Value == .5)
                            {
                                sarg = 1 / Math.Sqrt(arg);
                            }
                            else
                            {
                                /* NOSQRT */
                                sarg = Math.Exp(-model.MOS3bulkJctBotGradingCoeff * Math.Log(arg));
                            }
                            if (model.MOS3bulkJctSideGradingCoeff.Value == .5)
                            {
                                sargsw = 1 / Math.Sqrt(arg);
                            }
                            else
                            {
                                /* NOSQRT */
                                sargsw = Math.Exp(-model.MOS3bulkJctSideGradingCoeff * Math.Log(arg));
                            }
                        }
                        /* NOSQRT */
                        state.States[0][MOS3states + MOS3qbs] = MOS3tBulkPot * (MOS3Cbs * (1 - arg * sarg) / (1 - model.MOS3bulkJctBotGradingCoeff) +
                            MOS3Cbssw * (1 - arg * sargsw) / (1 - model.MOS3bulkJctSideGradingCoeff));
                        MOS3capbs = MOS3Cbs * sarg + MOS3Cbssw * sargsw;
                    }
                    else
                    {
                        state.States[0][MOS3states + MOS3qbs] = MOS3f4s + vbs * (MOS3f2s + vbs * (MOS3f3s / 2));
                        MOS3capbs = MOS3f2s + MOS3f3s * vbs;
                    }
                    /* CAPZEROBYPASS */
                }
                /* CAPBYPASS */
                /* can't bypass the diode capacitance calculations */
                {
                    /* CAPZEROBYPASS */
                    if (vbd < MOS3tDepCap)
                    {
                        double arg = 1 - vbd / MOS3tBulkPot, sarg;
                        /* 
						* the following block looks somewhat long and messy, 
						* but since most users use the default grading
						* coefficients of .5, and sqrt is MUCH faster than an
						* Math.Exp(Math.Log()) we use this special case code to buy time.
						* (as much as 10% of total job time!)
						*/
                        if (model.MOS3bulkJctBotGradingCoeff.Value == .5 && model.MOS3bulkJctSideGradingCoeff.Value == .5)
                        {
                            sarg = sargsw = 1 / Math.Sqrt(arg);
                        }
                        else
                        {
                            if (model.MOS3bulkJctBotGradingCoeff.Value == .5)
                            {
                                sarg = 1 / Math.Sqrt(arg);
                            }
                            else
                            {
                                /* NOSQRT */
                                sarg = Math.Exp(-model.MOS3bulkJctBotGradingCoeff * Math.Log(arg));
                            }
                            if (model.MOS3bulkJctSideGradingCoeff.Value == .5)
                            {
                                sargsw = 1 / Math.Sqrt(arg);
                            }
                            else
                            {
                                /* NOSQRT */
                                sargsw = Math.Exp(-model.MOS3bulkJctSideGradingCoeff * Math.Log(arg));
                            }
                        }
                        /* NOSQRT */
                        state.States[0][MOS3states + MOS3qbd] = MOS3tBulkPot * (MOS3Cbd * (1 - arg * sarg) / (1 - model.MOS3bulkJctBotGradingCoeff) +
                            MOS3Cbdsw * (1 - arg * sargsw) / (1 - model.MOS3bulkJctSideGradingCoeff));
                        MOS3capbd = MOS3Cbd * sarg + MOS3Cbdsw * sargsw;
                    }
                    else
                    {
                        state.States[0][MOS3states + MOS3qbd] = MOS3f4d + vbd * (MOS3f2d + vbd * MOS3f3d / 2);
                        MOS3capbd = MOS3f2d + vbd * MOS3f3d;
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
                        out icapgs, out icapgd, out icapgb, MOS3tPhi, OxideCap);
                }
                else
                {
                    Transistor.DEVqmeyer(vgd, vgs, vgb, von, vdsat,
                        out icapgd, out icapgs, out icapgb, MOS3tPhi, OxideCap);
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

            if (method == null || method.SavedTime == 0.0)
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
            ceqbs = model.MOS3type * (MOS3cbs - (MOS3gbs - state.Gmin) * vbs);
            ceqbd = model.MOS3type * (MOS3cbd - (MOS3gbd - state.Gmin) * vbd);
            if (MOS3mode >= 0)
            {
                xnrm = 1;
                xrev = 0;
                cdreq = model.MOS3type * (cdrain - MOS3gds * vds - MOS3gm * vgs - MOS3gmbs * vbs);
            }
            else
            {
                xnrm = 0;
                xrev = 1;
                cdreq = -(model.MOS3type) * (cdrain - MOS3gds * (-vds) - MOS3gm * vgd - MOS3gmbs * vbd);
            }

            rstate.Rhs[MOS3gNode] -= (model.MOS3type * (ceqgs + ceqgb + ceqgd));
            rstate.Rhs[MOS3bNode] -= (ceqbs + ceqbd - model.MOS3type * ceqgb);
            rstate.Rhs[MOS3dNodePrime] += (ceqbd - cdreq + model.MOS3type * ceqgd);
            rstate.Rhs[MOS3sNodePrime] += cdreq + ceqbs + model.MOS3type * ceqgs;
            
            /* 
			 * load y matrix
			 */
            rstate.Matrix[MOS3dNode, MOS3dNode] += (MOS3drainConductance);
            rstate.Matrix[MOS3gNode, MOS3gNode] += ((gcgd + gcgs + gcgb));
            rstate.Matrix[MOS3sNode, MOS3sNode] += (MOS3sourceConductance);
            rstate.Matrix[MOS3bNode, MOS3bNode] += (MOS3gbd + MOS3gbs + gcgb);
            rstate.Matrix[MOS3dNodePrime, MOS3dNodePrime] += (MOS3drainConductance + MOS3gds + MOS3gbd + xrev * (MOS3gm + MOS3gmbs) + gcgd);
            rstate.Matrix[MOS3sNodePrime, MOS3sNodePrime] += (MOS3sourceConductance + MOS3gds + MOS3gbs + xnrm * (MOS3gm + MOS3gmbs) +
                gcgs);
            rstate.Matrix[MOS3dNode, MOS3dNodePrime] += (-MOS3drainConductance);
            rstate.Matrix[MOS3gNode, MOS3bNode] -= gcgb;
            rstate.Matrix[MOS3gNode, MOS3dNodePrime] -= gcgd;
            rstate.Matrix[MOS3gNode, MOS3sNodePrime] -= gcgs;
            rstate.Matrix[MOS3sNode, MOS3sNodePrime] += (-MOS3sourceConductance);
            rstate.Matrix[MOS3bNode, MOS3gNode] -= gcgb;
            rstate.Matrix[MOS3bNode, MOS3dNodePrime] -= MOS3gbd;
            rstate.Matrix[MOS3bNode, MOS3sNodePrime] -= MOS3gbs;
            rstate.Matrix[MOS3dNodePrime, MOS3dNode] += (-MOS3drainConductance);
            rstate.Matrix[MOS3dNodePrime, MOS3gNode] += ((xnrm - xrev) * MOS3gm - gcgd);
            rstate.Matrix[MOS3dNodePrime, MOS3bNode] += (-MOS3gbd + (xnrm - xrev) * MOS3gmbs);
            rstate.Matrix[MOS3dNodePrime, MOS3sNodePrime] += (-MOS3gds - xnrm * (MOS3gm + MOS3gmbs));
            rstate.Matrix[MOS3sNodePrime, MOS3gNode] += (-(xnrm - xrev) * MOS3gm - gcgs);
            rstate.Matrix[MOS3sNodePrime, MOS3sNode] += (-MOS3sourceConductance);
            rstate.Matrix[MOS3sNodePrime, MOS3bNode] += (-MOS3gbs - (xnrm - xrev) * MOS3gmbs);
            rstate.Matrix[MOS3sNodePrime, MOS3dNodePrime] += (-MOS3gds - xrev * (MOS3gm + MOS3gmbs));
        }

        /// <summary>
        /// Load the device for AC simulation
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void AcLoad(Circuit ckt)
        {
            var model = Model as MOS3Model;
            var state = ckt.State;
            var cstate = state.Complex;
            int xnrm, xrev;
            double EffectiveLength, GateSourceOverlapCap, GateDrainOverlapCap, GateBulkOverlapCap, capgs, capgd, capgb, xgs, xgd, xgb, xbd,
                xbs;

            if (MOS3mode < 0)
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
			* charge oriented model parameters
			*/
            EffectiveLength = MOS3l - 2 * model.MOS3latDiff;
            GateSourceOverlapCap = model.MOS3gateSourceOverlapCapFactor * MOS3w;
            GateDrainOverlapCap = model.MOS3gateDrainOverlapCapFactor * MOS3w;
            GateBulkOverlapCap = model.MOS3gateBulkOverlapCapFactor * EffectiveLength;
            /* 
			* meyer"s model parameters
			*/
            capgs = (state.States[0][MOS3states + MOS3capgs] + state.States[0][MOS3states + MOS3capgs] + GateSourceOverlapCap);
            capgd = (state.States[0][MOS3states + MOS3capgd] + state.States[0][MOS3states + MOS3capgd] + GateDrainOverlapCap);
            capgb = (state.States[0][MOS3states + MOS3capgb] + state.States[0][MOS3states + MOS3capgb] + GateBulkOverlapCap);
            xgs = capgs * cstate.Laplace.Imaginary;
            xgd = capgd * cstate.Laplace.Imaginary;
            xgb = capgb * cstate.Laplace.Imaginary;
            xbd = MOS3capbd * cstate.Laplace.Imaginary;
            xbs = MOS3capbs * cstate.Laplace.Imaginary;

            /* 
			* load matrix
			*/

            cstate.Matrix[MOS3gNode, MOS3gNode] += new Complex(0.0, xgd + xgs + xgb);
            cstate.Matrix[MOS3bNode, MOS3bNode] += new Complex(MOS3gbd + MOS3gbs, xgb + xbd + xbs);
            cstate.Matrix[MOS3dNodePrime, MOS3dNodePrime] += new Complex(MOS3drainConductance + MOS3gds + MOS3gbd + xrev * (MOS3gm +
                MOS3gmbs), xgd + xbd);
            cstate.Matrix[MOS3sNodePrime, MOS3sNodePrime] += new Complex(MOS3sourceConductance + MOS3gds + MOS3gbs + xnrm * (MOS3gm +
                MOS3gmbs), xgs + xbs);
            cstate.Matrix[MOS3gNode, MOS3bNode] -= new Complex(0.0, xgb);
            cstate.Matrix[MOS3gNode, MOS3dNodePrime] -= new Complex(0.0, xgd);
            cstate.Matrix[MOS3gNode, MOS3sNodePrime] -= new Complex(0.0, xgs);
            cstate.Matrix[MOS3bNode, MOS3gNode] -= new Complex(0.0, xgb);
            cstate.Matrix[MOS3bNode, MOS3dNodePrime] -= new Complex(MOS3gbd, xbd);
            cstate.Matrix[MOS3bNode, MOS3sNodePrime] -= new Complex(MOS3gbs, xbs);
            cstate.Matrix[MOS3dNodePrime, MOS3gNode] += new Complex((xnrm - xrev) * MOS3gm, -xgd);
            cstate.Matrix[MOS3dNodePrime, MOS3bNode] += new Complex(-MOS3gbd + (xnrm - xrev) * MOS3gmbs, -xbd);
            cstate.Matrix[MOS3sNodePrime, MOS3gNode] -= new Complex((xnrm - xrev) * MOS3gm, xgs);
            cstate.Matrix[MOS3sNodePrime, MOS3bNode] -= new Complex(MOS3gbs + (xnrm - xrev) * MOS3gmbs, xbs);
            cstate.Matrix[MOS3dNode, MOS3dNode] += MOS3drainConductance;
            cstate.Matrix[MOS3sNode, MOS3sNode] += MOS3sourceConductance;

            cstate.Matrix[MOS3dNode, MOS3dNodePrime] -= MOS3drainConductance;
            cstate.Matrix[MOS3sNode, MOS3sNodePrime] -= MOS3sourceConductance;

            cstate.Matrix[MOS3dNodePrime, MOS3dNode] -= MOS3drainConductance;

            cstate.Matrix[MOS3dNodePrime, MOS3sNodePrime] -= MOS3gds + xnrm * (MOS3gm + MOS3gmbs);

            cstate.Matrix[MOS3sNodePrime, MOS3sNode] -= MOS3sourceConductance;

            cstate.Matrix[MOS3sNodePrime, MOS3dNodePrime] -= MOS3gds + xrev * (MOS3gm + MOS3gmbs);
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

        /// <summary>
        /// Noise calculations
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Noise(Circuit ckt)
        {
            var model = Model as MOS3Model;
            var state = ckt.State;
            var noise = state.Noise;

            double Kf = model.MOS3fNcoef * Math.Exp(model.MOS3fNexp * Math.Log(Math.Max(Math.Abs(MOS3cd), 1e-38))) / (MOS3w * (MOS3l - 2 * model.MOS3latDiff) * model.MOS3oxideCapFactor * model.MOS3oxideCapFactor);

            MOS3noise.Evaluate(ckt,
                MOS3drainConductance,
                MOS3sourceConductance,
                2.0 / 3.0 * Math.Abs(MOS3gm),
                Kf / noise.Freq);
        }
    }
}
