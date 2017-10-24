using System;
using System.Numerics;
using System.IO;
using SpiceSharp.Circuits;
using SpiceSharp.Diagnostics;
using SpiceSharp.Parameters;
using SpiceSharp.Components.Transistors;

namespace SpiceSharp.Components
{
    /// <summary>
    /// The BSIM3v30 device
    /// </summary>
    [SpicePins("Drain", "Gate", "Source", "Bulk"), ConnectedPins(0, 2, 3)]
    public class BSIM3v30 : CircuitComponent<BSIM3v30>
    {
        /// <summary>
        /// Register default behaviours
        /// </summary>
        static BSIM3v30()
        {
            Behaviours.Behaviours.RegisterBehaviour(typeof(BSIM3v30), typeof(ComponentBehaviours.BSIM3v30TemperatureBehaviour));
            Behaviours.Behaviours.RegisterBehaviour(typeof(BSIM3v30), typeof(ComponentBehaviours.BSIM3v30LoadBehaviour));
            Behaviours.Behaviours.RegisterBehaviour(typeof(BSIM3v30), typeof(ComponentBehaviours.BSIM3v30AcBehaviour));
        }

        /// <summary>
        /// Gets or sets the device model
        /// </summary>
        public void SetModel(BSIM3v30Model model) => Model = model;

        /// <summary>
        /// Size dependent parameters
        /// </summary>
        internal BSIM3SizeDependParam pParam = null;

        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("w"), SpiceInfo("Width")]
        public Parameter BSIM3w { get; } = new Parameter(5.0e-6);
        [SpiceName("l"), SpiceInfo("Length")]
        public Parameter BSIM3l { get; } = new Parameter(5.0e-6);
        [SpiceName("as"), SpiceInfo("Source area")]
        public Parameter BSIM3sourceArea { get; } = new Parameter();
        [SpiceName("ad"), SpiceInfo("Drain area")]
        public Parameter BSIM3drainArea { get; } = new Parameter();
        [SpiceName("ps"), SpiceInfo("Source perimeter")]
        public Parameter BSIM3sourcePerimeter { get; } = new Parameter();
        [SpiceName("pd"), SpiceInfo("Drain perimeter")]
        public Parameter BSIM3drainPerimeter { get; } = new Parameter();
        [SpiceName("nrs"), SpiceInfo("Number of squares in source")]
        public Parameter BSIM3sourceSquares { get; } = new Parameter(1.0);
        [SpiceName("nrd"), SpiceInfo("Number of squares in drain")]
        public Parameter BSIM3drainSquares { get; } = new Parameter(1.0);
        [SpiceName("off"), SpiceInfo("Device is initially off")]
        public bool BSIM3off { get; set; }
        [SpiceName("nqsmod"), SpiceInfo("Non-quasi-static model selector")]
        public Parameter BSIM3nqsMod { get; } = new Parameter();
        [SpiceName("acnqsmod"), SpiceInfo("AC NQS model selector")]
        public Parameter BSIM3acnqsMod { get; } = new Parameter();
        [SpiceName("id"), SpiceInfo("Ids")]
        public double BSIM3cd { get; internal set; }
        [SpiceName("gm"), SpiceInfo("Gm")]
        public double BSIM3gm { get; internal set; }
        [SpiceName("gds"), SpiceInfo("Gds")]
        public double BSIM3gds { get; internal set; }
        [SpiceName("gmbs"), SpiceInfo("Gmb")]
        public double BSIM3gmbs { get; internal set; }
        [SpiceName("vth"), SpiceInfo("Vth")]
        public double BSIM3von { get; internal set; }
        [SpiceName("vdsat"), SpiceInfo("Vdsat")]
        public double BSIM3vdsat { get; internal set; }

        /// <summary>
        /// Methods
        /// </summary>
        [SpiceName("ic"), SpiceInfo("Vector of DS,GS,BS initial voltages")]
        public void SetIC(double[] value)
        {
            switch (value.Length)
            {
                case 3: BSIM3icVBS.Set(value[2]); goto case 2;
                case 2: BSIM3icVGS.Set(value[1]); goto case 1;
                case 1: BSIM3icVDS.Set(value[0]); break;
                default:
                    throw new CircuitException("Bad parameter");
            }
        }
        [SpiceName("vbs"), SpiceInfo("Vbs")]
        public double GetVBS(Circuit ckt) => ckt.State.States[0][BSIM3states + BSIM3vbs];
        [SpiceName("vgs"), SpiceInfo("Vgs")]
        public double GetVGS(Circuit ckt) => ckt.State.States[0][BSIM3states + BSIM3vgs];
        [SpiceName("vds"), SpiceInfo("Vds")]
        public double GetVDS(Circuit ckt) => ckt.State.States[0][BSIM3states + BSIM3vds];

        /// <summary>
        /// Extra variables
        /// </summary>
        public Parameter BSIM3icVBS { get; } = new Parameter();
        public Parameter BSIM3icVDS { get; } = new Parameter();
        public Parameter BSIM3icVGS { get; } = new Parameter();
        public double BSIM3drainConductance { get; internal set; }
        public double BSIM3sourceConductance { get; internal set; }
        public double BSIM3cgso { get; internal set; }
        public double BSIM3cgdo { get; internal set; }
        public double BSIM3vjsm { get; internal set; }
        public double BSIM3IsEvjsm { get; internal set; }
        public double BSIM3vjdm { get; internal set; }
        public double BSIM3IsEvjdm { get; internal set; }
        public double BSIM3mode { get; internal set; }
        public double BSIM3csub { get; internal set; }
        public double BSIM3cbd { get; internal set; }
        public double BSIM3gbd { get; internal set; }
        public double BSIM3gbbs { get; internal set; }
        public double BSIM3gbgs { get; internal set; }
        public double BSIM3gbds { get; internal set; }
        public double BSIM3cbs { get; internal set; }
        public double BSIM3gbs { get; internal set; }
        public double BSIM3thetavth { get; internal set; }
        public double BSIM3Vgsteff { get; internal set; }
        public double BSIM3rds { get; internal set; }
        public double BSIM3Abulk { get; internal set; }
        public double BSIM3ueff { get; internal set; }
        public double BSIM3AbovVgst2Vtm { get; internal set; }
        public double BSIM3Vdseff { get; internal set; }
        public double BSIM3qinv { get; internal set; }
        public double BSIM3cggb { get; internal set; }
        public double BSIM3cgsb { get; internal set; }
        public double BSIM3cgdb { get; internal set; }
        public double BSIM3cdgb { get; internal set; }
        public double BSIM3cdsb { get; internal set; }
        public double BSIM3cddb { get; internal set; }
        public double BSIM3cbgb { get; internal set; }
        public double BSIM3cbsb { get; internal set; }
        public double BSIM3cbdb { get; internal set; }
        public double BSIM3cqdb { get; internal set; }
        public double BSIM3cqsb { get; internal set; }
        public double BSIM3cqgb { get; internal set; }
        public double BSIM3cqbb { get; internal set; }
        public double BSIM3gtau { get; internal set; }
        public double BSIM3qgate { get; internal set; }
        public double BSIM3qbulk { get; internal set; }
        public double BSIM3qdrn { get; internal set; }
        public double BSIM3capbs { get; internal set; }
        public double BSIM3capbd { get; internal set; }
        public double BSIM3taunet { get; internal set; }
        public double BSIM3gtg { get; internal set; }
        public double BSIM3gtd { get; internal set; }
        public double BSIM3gts { get; internal set; }
        public double BSIM3gtb { get; internal set; }
        public int BSIM3dNode { get; internal set; }
        public int BSIM3gNode { get; internal set; }
        public int BSIM3sNode { get; internal set; }
        public int BSIM3bNode { get; internal set; }
        public int BSIM3dNodePrime { get; internal set; }
        public int BSIM3sNodePrime { get; internal set; }
        public int BSIM3qNode { get; internal set; }
        public int BSIM3states { get; internal set; }

        /// <summary>
        /// Constants
        /// </summary>
        public const int BSIM3vbd = 0;
        public const int BSIM3vbs = 1;
        public const int BSIM3vgs = 2;
        public const int BSIM3vds = 3;
        public const int BSIM3qb = 4;
        public const int BSIM3cqb = 5;
        public const int BSIM3qg = 6;
        public const int BSIM3cqg = 7;
        public const int BSIM3qd = 8;
        public const int BSIM3cqd = 9;
        public const int BSIM3qbs = 10;
        public const int BSIM3qbd = 11;
        public const int BSIM3qcheq = 12;
        public const int BSIM3cqcheq = 13;
        public const int BSIM3qcdump = 14;
        public const int BSIM3cqcdump = 15;
        public const int BSIM3qdef = 16;

        internal const double ScalingFactor = 1e-9;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the device</param>
        public BSIM3v30(CircuitIdentifier name) : base(name)
        {
        }

        /// <summary>
        /// Setup the device
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Setup(Circuit ckt)
        {
            var model = Model as BSIM3v30Model;
			pParam = null;

            // Allocate nodes
            var nodes = BindNodes(ckt);
            BSIM3dNode = nodes[0].Index;
            BSIM3gNode = nodes[1].Index;
            BSIM3sNode = nodes[2].Index;
            BSIM3bNode = nodes[3].Index;

            // Allocate states
            BSIM3states = ckt.State.GetState(17);

            /* allocate a chunk of the state vector */

            /* perform the parameter defaulting */
            if (!BSIM3acnqsMod.Given)
                BSIM3acnqsMod.Value = model.BSIM3acnqsMod;
            else if ((BSIM3acnqsMod != 0) && (BSIM3acnqsMod != 1))
            {
                BSIM3acnqsMod.Value = model.BSIM3acnqsMod;
                CircuitWarning.Warning(this, $"Warning: acnqsMod has been set to its global value {model.BSIM3acnqsMod}.");
            }

            /* process drain series resistance */
            if ((model.BSIM3sheetResistance > 0.0) && (BSIM3drainSquares > 0.0))
                BSIM3dNodePrime = CreateNode(ckt, Name.Grow("#drain")).Index;
            else
                BSIM3dNodePrime = BSIM3dNode;

            /* process source series resistance */
            if ((model.BSIM3sheetResistance > 0.0) && (BSIM3sourceSquares > 0.0))
                BSIM3sNodePrime = CreateNode(ckt, Name.Grow("#source")).Index;
            else
                BSIM3sNodePrime = BSIM3sNode;

            /* internal charge node */
            if ((BSIM3nqsMod > 0) && (BSIM3qNode == 0))
                BSIM3qNode = CreateNode(ckt, Name.Grow("#charge")).Index;
            else
                BSIM3qNode = 0;
        }

        /// <summary>
        /// Truncate
        /// </summary>
        /// <param name="ckt">Circuit</param>
        /// <param name="timeStep">Timestep</param>
        public override void Truncate(Circuit ckt, ref double timeStep)
        {
            var method = ckt.Method;
            method.Terr(BSIM3states + BSIM3qb, ckt, ref timeStep);
            method.Terr(BSIM3states + BSIM3qg, ckt, ref timeStep);
            method.Terr(BSIM3states + BSIM3qd, ckt, ref timeStep);
        }
    }
}
