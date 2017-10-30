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
            Behaviors.Behaviors.RegisterBehavior(typeof(MOS1), typeof(ComponentBehaviors.MOS1TemperatureBehavior));
            Behaviors.Behaviors.RegisterBehavior(typeof(MOS1), typeof(ComponentBehaviors.MOS1LoadBehavior));
            Behaviors.Behaviors.RegisterBehavior(typeof(MOS1), typeof(ComponentBehaviors.MOS1AcBehavior));
            Behaviors.Behaviors.RegisterBehavior(typeof(MOS1), typeof(ComponentBehaviors.MOS1NoiseBehavior));
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
            double value = MOS1cd * ckt.State.Solution[MOS1dNode];
            value += (MOS1cbd + MOS1cbs - ckt.State.States[0][MOS1states + MOS1cqgb]) * ckt.State.Solution[MOS1bNode];
            if (ckt.Method != null && !(ckt.State.Domain == CircuitState.DomainTypes.Time && ckt.State.UseDC))
            {
                value += (ckt.State.States[0][MOS1states + MOS1cqgb] + ckt.State.States[0][MOS1states + MOS1cqgd] +
                    ckt.State.States[0][MOS1states + MOS1cqgs]) * ckt.State.Solution[MOS1gNode];
            }
            temp = -MOS1cd;
            temp -= MOS1cbd + MOS1cbs;
            if (ckt.Method != null && !(ckt.State.Domain == CircuitState.DomainTypes.Time && ckt.State.UseDC))
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

            // Add series drain node if necessary
            if (model.MOS1drainResistance != 0 || (model.MOS1sheetResistance != 0 && MOS1drainSquares != 0))
                MOS1dNodePrime = CreateNode(ckt, Name.Grow("#drain")).Index;
            else
                MOS1dNodePrime = MOS1dNode;

            // Add series source node if necessary
            if (model.MOS1sourceResistance != 0 || (model.MOS1sheetResistance != 0 && MOS1sourceSquares != 0))
                MOS1sNodePrime = CreateNode(ckt, Name.Grow("#source")).Index;
            else
                MOS1sNodePrime = MOS1sNode;

            // Allocate states
            MOS1states = ckt.State.GetState(17);
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
