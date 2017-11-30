using SpiceSharp.Circuits;
using SpiceSharp.Diagnostics;
using SpiceSharp.Parameters;

namespace SpiceSharp.Components
{
    [SpicePins("Drain", "Gate", "Source", "Bulk"), ConnectedPins(0, 2, 3)]
    public class BSIM1 : CircuitComponent<BSIM1>
    {
        /// <summary>
        /// Register default behaviours
        /// </summary>
        static BSIM1()
        {
            Behaviors.Behaviors.RegisterBehavior(typeof(BSIM1), typeof(ComponentBehaviors.BSIM1TemperatureBehavior));
            Behaviors.Behaviors.RegisterBehavior(typeof(BSIM1), typeof(ComponentBehaviors.BSIM1LoadBehavior));
            Behaviors.Behaviors.RegisterBehavior(typeof(BSIM1), typeof(ComponentBehaviors.BSIM1AcBehavior));
            Behaviors.Behaviors.RegisterBehavior(typeof(BSIM1), typeof(ComponentBehaviors.BSIM1TruncateBehavior));
        }

        /// <summary>
        /// Gets or sets the device model
        /// </summary>
        public void SetModel(BSIM1Model model) => Model = (ICircuitObject)model;

        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("w"), SpiceInfo("Width")]
        public Parameter B1w { get; } = new Parameter(5e-6);
        [SpiceName("l"), SpiceInfo("Length")]
        public Parameter B1l { get; } = new Parameter(5e-6);
        [SpiceName("as"), SpiceInfo("Source area")]
        public Parameter B1sourceArea { get; } = new Parameter();
        [SpiceName("ad"), SpiceInfo("Drain area")]
        public Parameter B1drainArea { get; } = new Parameter();
        [SpiceName("ps"), SpiceInfo("Source perimeter")]
        public Parameter B1sourcePerimeter { get; } = new Parameter();
        [SpiceName("pd"), SpiceInfo("Drain perimeter")]
        public Parameter B1drainPerimeter { get; } = new Parameter();
        [SpiceName("nrs"), SpiceInfo("Number of squares in source")]
        public Parameter B1sourceSquares { get; } = new Parameter(1);
        [SpiceName("nrd"), SpiceInfo("Number of squares in drain")]
        public Parameter B1drainSquares { get; } = new Parameter(1);
        [SpiceName("off"), SpiceInfo("Device is initially off")]
        public bool B1off { get; set; }
        [SpiceName("vbs"), SpiceInfo("Initial B-S voltage")]
        public Parameter B1icVBS { get; } = new Parameter();
        [SpiceName("vds"), SpiceInfo("Initial D-S voltage")]
        public Parameter B1icVDS { get; } = new Parameter();
        [SpiceName("vgs"), SpiceInfo("Initial G-S voltage")]
        public Parameter B1icVGS { get; } = new Parameter();

        /// <summary>
        /// Methods
        /// </summary>
        [SpiceName("ic"), SpiceInfo("Vector of DS,GS,BS initial voltages")]
        public void SetIC(double[] value)
        {
            switch (value.Length)
            {
                case 3: B1icVBS.Set(value[2]); goto case 2;
                case 2: B1icVGS.Set(value[1]); goto case 1;
                case 1: B1icVDS.Set(value[0]); break;
                default:
                    throw new CircuitException("Bad parameter");
            }
        }

        /// <summary>
        /// Extra variables
        /// </summary>
        public double B1vdsat { get; internal set; }
        public double B1von { get; internal set; }
        public double B1GDoverlapCap { get; internal set; }
        public double B1GSoverlapCap { get; internal set; }
        public double B1GBoverlapCap { get; internal set; }
        public double B1drainConductance { get; internal set; }
        public double B1sourceConductance { get; internal set; }
        public double B1vfb { get; internal set; }
        public double B1phi { get; internal set; }
        public double B1K1 { get; internal set; }
        public double B1K2 { get; internal set; }
        public double B1eta { get; internal set; }
        public double B1etaB { get; internal set; }
        public double B1etaD { get; internal set; }
        public double B1betaZero { get; internal set; }
        public double B1betaZeroB { get; internal set; }
        public double B1ugs { get; internal set; }
        public double B1ugsB { get; internal set; }
        public double B1uds { get; internal set; }
        public double B1udsB { get; internal set; }
        public double B1udsD { get; internal set; }
        public double B1betaVdd { get; internal set; }
        public double B1betaVddB { get; internal set; }
        public double B1betaVddD { get; internal set; }
        public double B1subthSlope { get; internal set; }
        public double B1subthSlopeB { get; internal set; }
        public double B1subthSlopeD { get; internal set; }
        public double B1vt0 { get; internal set; }
        public double B1mode { get; internal set; }
        public int B1dNode { get; internal set; }
        public int B1gNode { get; internal set; }
        public int B1sNode { get; internal set; }
        public int B1bNode { get; internal set; }
        public int B1dNodePrime { get; internal set; }
        public int B1sNodePrime { get; internal set; }
        public int B1states { get; internal set; }

        /// <summary>
        /// Constants
        /// </summary>
        public const int B1vbd = 0;
        public const int B1vbs = 1;
        public const int B1vgs = 2;
        public const int B1vds = 3;
        public const int B1cd = 4;
        public const int B1id = 4;
        public const int B1cbs = 5;
        public const int B1ibs = 5;
        public const int B1cbd = 6;
        public const int B1ibd = 6;
        public const int B1gm = 7;
        public const int B1gds = 8;
        public const int B1gmbs = 9;
        public const int B1gbd = 10;
        public const int B1gbs = 11;
        public const int B1qb = 12;
        public const int B1cqb = 13;
        public const int B1iqb = 13;
        public const int B1qg = 14;
        public const int B1cqg = 15;
        public const int B1iqg = 15;
        public const int B1qd = 16;
        public const int B1cqd = 17;
        public const int B1iqd = 17;
        public const int B1cggb = 18;
        public const int B1cgdb = 19;
        public const int B1cgsb = 20;
        public const int B1cbgb = 21;
        public const int B1cbdb = 22;
        public const int B1cbsb = 23;
        public const int B1capbd = 24;
        public const int B1iqbd = 25;
        public const int B1cqbd = 25;
        public const int B1capbs = 26;
        public const int B1iqbs = 27;
        public const int B1cqbs = 27;
        public const int B1cdgb = 28;
        public const int B1cddb = 29;
        public const int B1cdsb = 30;
        public const int B1vono = 31;
        public const int B1vdsato = 32;
        public const int B1qbs = 33;
        public const int B1qbd = 34;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the device</param>
        public BSIM1(CircuitIdentifier name) : base(name)
        {
        }

        /// <summary>
        /// Setup the device
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Setup(Circuit ckt)
        {
            var model = Model as BSIM1Model;

            // Allocate nodes
            var nodes = BindNodes(ckt);
            B1dNode = nodes[0].Index;
            B1gNode = nodes[1].Index;
            B1sNode = nodes[2].Index;
            B1bNode = nodes[3].Index;

            // process drain series resistance
            if (model.B1sheetResistance.Value != 0 && B1drainSquares.Value != 0.0)
                B1dNodePrime = CreateNode(ckt, Name.Grow("#drain")).Index;
            else
                B1dNodePrime = B1dNode;

            // process source series resistance
            if (model.B1sheetResistance.Value != 0 && B1sourceSquares.Value != 0.0)
                B1sNodePrime = CreateNode(ckt, Name.Grow("#source")).Index;
            else
                B1sNodePrime = B1sNode;

            // Allocate states
            B1states = ckt.State.GetState(35);
        }
    }
}
