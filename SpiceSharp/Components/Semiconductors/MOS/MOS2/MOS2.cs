using System;
using SpiceSharp.Circuits;
using SpiceSharp.Diagnostics;
using SpiceSharp.Parameters;
using SpiceSharp.Sparse;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A MOS2 Mosfet.
    /// Level 2, A. Vladimirescu and S. Liu, The Simulation of MOS Integrated Circuits Using SPICE2, ERL Memo No. M80/7, Electronics Research Laboratory University of California, Berkeley, October 1980.
    /// </summary>
    [SpicePins("Drain", "Gate", "Source", "Bulk"), ConnectedPins(0, 2, 3)]
    public class MOS2 : CircuitComponent
    {
        /// <summary>
        /// Register default behaviors
        /// </summary>
        static MOS2()
        {
            Behaviors.Behaviors.RegisterBehavior(typeof(MOS2), typeof(ComponentBehaviors.MOS2TemperatureBehavior));
            Behaviors.Behaviors.RegisterBehavior(typeof(MOS2), typeof(ComponentBehaviors.MOS2LoadBehavior));
            Behaviors.Behaviors.RegisterBehavior(typeof(MOS2), typeof(ComponentBehaviors.MOS2AcBehavior));
            Behaviors.Behaviors.RegisterBehavior(typeof(MOS2), typeof(ComponentBehaviors.MOS2NoiseBehavior));
            Behaviors.Behaviors.RegisterBehavior(typeof(MOS2), typeof(ComponentBehaviors.MOS2TruncateBehavior));
        }

        /// <summary>
        /// Set the model for the MOS2 Mosfet.
        /// </summary>
        public void SetModel(MOS2Model model) => Model = model;

        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("temp"), SpiceInfo("Instance operating temperature")]
        public double MOS2_TEMP
        {
            get => MOS2temp - Circuit.CONSTCtoK;
            set => MOS2temp.Set(value + Circuit.CONSTCtoK);
        }
        public Parameter MOS2temp { get; } = new Parameter();
        [SpiceName("w"), SpiceInfo("Width")]
        public Parameter MOS2w { get; } = new Parameter(1e-4);
        [SpiceName("l"), SpiceInfo("Length")]
        public Parameter MOS2l { get; } = new Parameter(1e-4);
        [SpiceName("as"), SpiceInfo("Source area")]
        public Parameter MOS2sourceArea { get; } = new Parameter();
        [SpiceName("ad"), SpiceInfo("Drain area")]
        public Parameter MOS2drainArea { get; } = new Parameter();
        [SpiceName("ps"), SpiceInfo("Source perimeter")]
        public Parameter MOS2sourcePerimiter { get; } = new Parameter();
        [SpiceName("pd"), SpiceInfo("Drain perimeter")]
        public Parameter MOS2drainPerimiter { get; } = new Parameter();
        [SpiceName("nrs"), SpiceInfo("Source squares")]
        public Parameter MOS2sourceSquares { get; } = new Parameter(1);
        [SpiceName("nrd"), SpiceInfo("Drain squares")]
        public Parameter MOS2drainSquares { get; } = new Parameter(1);
        [SpiceName("off"), SpiceInfo("Device initially off")]
        public bool MOS2off { get; set; }
        [SpiceName("icvbs"), SpiceInfo("Initial B-S voltage")]
        public Parameter MOS2icVBS { get; } = new Parameter();
        [SpiceName("icvds"), SpiceInfo("Initial D-S voltage")]
        public Parameter MOS2icVDS { get; } = new Parameter();
        [SpiceName("icvgs"), SpiceInfo("Initial G-S voltage")]
        public Parameter MOS2icVGS { get; } = new Parameter();
        [SpiceName("dnode"), SpiceInfo("Number of drain node")]
        public int MOS2dNode { get; internal set; }
        [SpiceName("gnode"), SpiceInfo("Number of gate node")]
        public int MOS2gNode { get; internal set; }
        [SpiceName("snode"), SpiceInfo("Number of source node")]
        public int MOS2sNode { get; internal set; }
        [SpiceName("bnode"), SpiceInfo("Number of bulk node")]
        public int MOS2bNode { get; internal set; }
        [SpiceName("dnodeprime"), SpiceInfo("Number of internal drain node")]
        public int MOS2dNodePrime { get; internal set; }
        [SpiceName("snodeprime"), SpiceInfo("Number of internal source node")]
        public int MOS2sNodePrime { get; internal set; }
        [SpiceName("sourceconductance"), SpiceInfo("Source conductance")]
        public double MOS2sourceConductance { get; internal set; }
        [SpiceName("drainconductance"), SpiceInfo("Drain conductance")]
        public double MOS2drainConductance { get; internal set; }
        [SpiceName("von"), SpiceInfo(" ")]
        public double MOS2von { get; internal set; }
        [SpiceName("vdsat"), SpiceInfo("Saturation drain voltage")]
        public double MOS2vdsat { get; internal set; }
        [SpiceName("sourcevcrit"), SpiceInfo("Critical source voltage")]
        public double MOS2sourceVcrit { get; internal set; }
        [SpiceName("drainvcrit"), SpiceInfo("Critical drain voltage")]
        public double MOS2drainVcrit { get; internal set; }
        [SpiceName("id"), SpiceName("cd"), SpiceInfo("Drain current")]
        public double MOS2cd { get; internal set; }
        [SpiceName("ibs"), SpiceInfo("B-S junction current")]
        public double MOS2cbs { get; internal set; }
        [SpiceName("ibd"), SpiceInfo("B-D junction current")]
        public double MOS2cbd { get; internal set; }
        [SpiceName("gmb"), SpiceName("gmbs"), SpiceInfo("Bulk-Source transconductance")]
        public double MOS2gmbs { get; internal set; }
        [SpiceName("gm"), SpiceInfo("Transconductance")]
        public double MOS2gm { get; internal set; }
        [SpiceName("gds"), SpiceInfo("Drain-Source conductance")]
        public double MOS2gds { get; internal set; }
        [SpiceName("gbd"), SpiceInfo("Bulk-Drain conductance")]
        public double MOS2gbd { get; internal set; }
        [SpiceName("gbs"), SpiceInfo("Bulk-Source conductance")]
        public double MOS2gbs { get; internal set; }
        [SpiceName("cbd"), SpiceInfo("Bulk-Drain capacitance")]
        public double MOS2capbd { get; internal set; }
        [SpiceName("cbs"), SpiceInfo("Bulk-Source capacitance")]
        public double MOS2capbs { get; internal set; }
        [SpiceName("cbd0"), SpiceInfo("Zero-Bias B-D junction capacitance")]
        public double MOS2Cbd { get; internal set; }
        [SpiceName("cbdsw0"), SpiceInfo(" ")]
        public double MOS2Cbdsw { get; internal set; }
        [SpiceName("cbs0"), SpiceInfo("Zero-Bias B-S junction capacitance")]
        public double MOS2Cbs { get; internal set; }
        [SpiceName("cbssw0"), SpiceInfo(" ")]
        public double MOS2Cbssw { get; internal set; }

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
        public double GetCG(Circuit ckt) => ckt.State.UseDC ? 0.0 : ckt.State.States[0][MOS2states + MOS2cqgb] + ckt.State.States[0][MOS2states + MOS2cqgd] + 
            ckt.State.States[0][MOS2states + MOS2cqgs];
        [SpiceName("is"), SpiceInfo("Source current ")]
        public double GetCS(Circuit ckt)
        {
            double value = -MOS2cd;
            value -= MOS2cbd + MOS2cbs - ckt.State.States[0][MOS2states + MOS2cqgb];
            if (ckt.State.Domain == CircuitState.DomainTypes.Time && !ckt.State.UseDC)
            {
                value -= ckt.State.States[0][MOS2states + MOS2cqgb] + ckt.State.States[0][MOS2states + MOS2cqgd] +
                    ckt.State.States[0][MOS2states + MOS2cqgs];
            }
            return value;
        }
        [SpiceName("p"), SpiceInfo("Instantaneous power ")]
        public double GetPOWER(Circuit ckt)
        {
            double temp;
            double value = MOS2cd * ckt.State.Solution[MOS2dNode];
            value += (MOS2cbd + MOS2cbs - ckt.State.States[0][MOS2states + MOS2cqgb]) * ckt.State.Solution[MOS2bNode];
            if (ckt.State.Domain == CircuitState.DomainTypes.Time && !ckt.State.UseDC)
            {
                value += (ckt.State.States[0][MOS2states + MOS2cqgb] + ckt.State.States[0][MOS2states + MOS2cqgd] +
                    ckt.State.States[0][MOS2states + MOS2cqgs]) * ckt.State.Solution[MOS2gNode];
            }
            temp = -MOS2cd;
            temp -= MOS2cbd + MOS2cbs;
            if (ckt.State.Domain == CircuitState.DomainTypes.Time && !ckt.State.UseDC)
            {
                temp -= ckt.State.States[0][MOS2states + MOS2cqgb] + ckt.State.States[0][MOS2states + MOS2cqgd] +
                    ckt.State.States[0][MOS2states + MOS2cqgs];
            }
            value += temp * ckt.State.Solution[MOS2sNode];
            return value;
        }

        /// <summary>
        /// Extra variables
        /// </summary>
        public double MOS2mode { get; internal set; }
        public double MOS2tTransconductance { get; internal set; }
        public double MOS2tSurfMob { get; internal set; }
        public double MOS2tPhi { get; internal set; }
        public double MOS2tVbi { get; internal set; }
        public double MOS2tVto { get; internal set; }
        public double MOS2tSatCur { get; internal set; }
        public double MOS2tSatCurDens { get; internal set; }
        public double MOS2tCbd { get; internal set; }
        public double MOS2tCbs { get; internal set; }
        public double MOS2tCj { get; internal set; }
        public double MOS2tCjsw { get; internal set; }
        public double MOS2tBulkPot { get; internal set; }
        public double MOS2tDepCap { get; internal set; }
        public double MOS2f2d { get; internal set; }
        public double MOS2f3d { get; internal set; }
        public double MOS2f4d { get; internal set; }
        public double MOS2f2s { get; internal set; }
        public double MOS2f3s { get; internal set; }
        public double MOS2f4s { get; internal set; }
        public double MOS2cgs { get; internal set; }
        public double MOS2cgd { get; internal set; }
        public double MOS2cgb { get; internal set; }
        public int MOS2states { get; internal set; }

        /// <summary>
        /// Matrix elements
        /// </summary>
        internal MatrixElement MOS2DdPtr { get; private set; }
        internal MatrixElement MOS2GgPtr { get; private set; }
        internal MatrixElement MOS2SsPtr { get; private set; }
        internal MatrixElement MOS2BbPtr { get; private set; }
        internal MatrixElement MOS2DPdpPtr { get; private set; }
        internal MatrixElement MOS2SPspPtr { get; private set; }
        internal MatrixElement MOS2DdpPtr { get; private set; }
        internal MatrixElement MOS2GbPtr { get; private set; }
        internal MatrixElement MOS2GdpPtr { get; private set; }
        internal MatrixElement MOS2GspPtr { get; private set; }
        internal MatrixElement MOS2SspPtr { get; private set; }
        internal MatrixElement MOS2BdpPtr { get; private set; }
        internal MatrixElement MOS2BspPtr { get; private set; }
        internal MatrixElement MOS2DPspPtr { get; private set; }
        internal MatrixElement MOS2DPdPtr { get; private set; }
        internal MatrixElement MOS2BgPtr { get; private set; }
        internal MatrixElement MOS2DPgPtr { get; private set; }
        internal MatrixElement MOS2SPgPtr { get; private set; }
        internal MatrixElement MOS2SPsPtr { get; private set; }
        internal MatrixElement MOS2DPbPtr { get; private set; }
        internal MatrixElement MOS2SPbPtr { get; private set; }
        internal MatrixElement MOS2SPdpPtr { get; private set; }

        /// <summary>
        /// Constants
        /// </summary>
        public const int MOS2vbd = 0;
        public const int MOS2vbs = 1;
        public const int MOS2vgs = 2;
        public const int MOS2vds = 3;
        public const int MOS2capgs = 4;
        public const int MOS2qgs = 5;
        public const int MOS2cqgs = 6;
        public const int MOS2capgd = 7;
        public const int MOS2qgd = 8;
        public const int MOS2cqgd = 9;
        public const int MOS2capgb = 10;
        public const int MOS2qgb = 11;
        public const int MOS2cqgb = 12;
        public const int MOS2qbd = 13;
        public const int MOS2cqbd = 14;
        public const int MOS2qbs = 15;
        public const int MOS2cqbs = 16;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the device</param>
        public MOS2(CircuitIdentifier name) : base(name, 4)
        {
        }

        /// <summary>
        /// Setup the device
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Setup(Circuit ckt)
        {
            var model = Model as MOS2Model;

            // Allocate nodes
            var nodes = BindNodes(ckt);
            MOS2dNode = nodes[0].Index;
            MOS2gNode = nodes[1].Index;
            MOS2sNode = nodes[2].Index;
            MOS2bNode = nodes[3].Index;

            // Add a series drain node if necessary
            if (model.MOS2drainResistance != 0 || (MOS2drainSquares != 0 && model.MOS2sheetResistance != 0))
                MOS2dNodePrime = CreateNode(ckt, Name.Grow("#drain")).Index;
            else
                MOS2dNodePrime = MOS2dNode;

            // Add a series source node if necessary
            if (model.MOS2sourceResistance != 0 || (MOS2sourceSquares != 0 && model.MOS2sheetResistance != 0))
                MOS2sNodePrime = CreateNode(ckt, Name.Grow("#source")).Index;
            else
                MOS2sNodePrime = MOS2sNode;

            // Get matrix elements
            var matrix = ckt.State.Matrix;
            MOS2DdPtr = matrix.GetElement(MOS2dNode, MOS2dNode);
            MOS2GgPtr = matrix.GetElement(MOS2gNode, MOS2gNode);
            MOS2SsPtr = matrix.GetElement(MOS2sNode, MOS2sNode);
            MOS2BbPtr = matrix.GetElement(MOS2bNode, MOS2bNode);
            MOS2DPdpPtr = matrix.GetElement(MOS2dNodePrime, MOS2dNodePrime);
            MOS2SPspPtr = matrix.GetElement(MOS2sNodePrime, MOS2sNodePrime);
            MOS2DdpPtr = matrix.GetElement(MOS2dNode, MOS2dNodePrime);
            MOS2GbPtr = matrix.GetElement(MOS2gNode, MOS2bNode);
            MOS2GdpPtr = matrix.GetElement(MOS2gNode, MOS2dNodePrime);
            MOS2GspPtr = matrix.GetElement(MOS2gNode, MOS2sNodePrime);
            MOS2SspPtr = matrix.GetElement(MOS2sNode, MOS2sNodePrime);
            MOS2BdpPtr = matrix.GetElement(MOS2bNode, MOS2dNodePrime);
            MOS2BspPtr = matrix.GetElement(MOS2bNode, MOS2sNodePrime);
            MOS2DPspPtr = matrix.GetElement(MOS2dNodePrime, MOS2sNodePrime);
            MOS2DPdPtr = matrix.GetElement(MOS2dNodePrime, MOS2dNode);
            MOS2BgPtr = matrix.GetElement(MOS2bNode, MOS2gNode);
            MOS2DPgPtr = matrix.GetElement(MOS2dNodePrime, MOS2gNode);
            MOS2SPgPtr = matrix.GetElement(MOS2sNodePrime, MOS2gNode);
            MOS2SPsPtr = matrix.GetElement(MOS2sNodePrime, MOS2sNode);
            MOS2DPbPtr = matrix.GetElement(MOS2dNodePrime, MOS2bNode);
            MOS2SPbPtr = matrix.GetElement(MOS2sNodePrime, MOS2bNode);
            MOS2SPdpPtr = matrix.GetElement(MOS2sNodePrime, MOS2dNodePrime);

            // Allocate states
            MOS2states = ckt.State.GetState(17);
        }

        /// <summary>
        /// Unsetup
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Unsetup(Circuit ckt)
        {
            // Remove references
            MOS2DdPtr = null;
            MOS2GgPtr = null;
            MOS2SsPtr = null;
            MOS2BbPtr = null;
            MOS2DPdpPtr = null;
            MOS2SPspPtr = null;
            MOS2DdpPtr = null;
            MOS2GbPtr = null;
            MOS2GdpPtr = null;
            MOS2GspPtr = null;
            MOS2SspPtr = null;
            MOS2BdpPtr = null;
            MOS2BspPtr = null;
            MOS2DPspPtr = null;
            MOS2DPdPtr = null;
            MOS2BgPtr = null;
            MOS2DPgPtr = null;
            MOS2SPgPtr = null;
            MOS2SPsPtr = null;
            MOS2DPbPtr = null;
            MOS2SPbPtr = null;
            MOS2SPdpPtr = null;
        }
    }
}
