using System;
using System.Collections.Generic;
using SpiceSharp.Circuits;
using SpiceSharp.Diagnostics;
using SpiceSharp.Parameters;
using SpiceSharp.Components.Transistors;

namespace SpiceSharp.Components
{
    /// <summary>
    /// BSIM2 model device
    /// </summary>
    [SpicePins("Drain", "Gate", "Source", "Bulk"), ConnectedPins(0, 2, 3)]
    public class BSIM2 : CircuitComponent
    {
        /// <summary>
        /// Register default behaviours
        /// </summary>
        static BSIM2()
        {
            Behaviors.Behaviors.RegisterBehavior(typeof(BSIM2), typeof(ComponentBehaviors.BSIM2TemperatureBehavior));
            Behaviors.Behaviors.RegisterBehavior(typeof(BSIM2), typeof(ComponentBehaviors.BSIM2LoadBehavior));
            Behaviors.Behaviors.RegisterBehavior(typeof(BSIM2), typeof(ComponentBehaviors.BSIM2AcBehavior));
            Behaviors.Behaviors.RegisterBehavior(typeof(BSIM2), typeof(ComponentBehaviors.BSIM2TruncateBehavior));
        }

        /// <summary>
        /// Gets or sets the device model
        /// </summary>
        public void SetModel(BSIM2Model model) => Model = (ICircuitObject)model;

        /// <summary>
        /// Sizes
        /// </summary>
        private static Dictionary<Tuple<double, double>, BSIM2SizeDependParam> sizes = new Dictionary<Tuple<double, double>, BSIM2SizeDependParam>();

        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("w"), SpiceInfo("Width")]
        public Parameter B2w { get; } = new Parameter(5e-6);
        [SpiceName("l"), SpiceInfo("Length")]
        public Parameter B2l { get; } = new Parameter(5e-6);
        [SpiceName("as"), SpiceInfo("Source area")]
        public Parameter B2sourceArea { get; } = new Parameter();
        [SpiceName("ad"), SpiceInfo("Drain area")]
        public Parameter B2drainArea { get; } = new Parameter();
        [SpiceName("ps"), SpiceInfo("Source perimeter")]
        public Parameter B2sourcePerimeter { get; } = new Parameter();
        [SpiceName("pd"), SpiceInfo("Drain perimeter")]
        public Parameter B2drainPerimeter { get; } = new Parameter();
        [SpiceName("nrs"), SpiceInfo("Number of squares in source")]
        public Parameter B2sourceSquares { get; } = new Parameter(1);
        [SpiceName("nrd"), SpiceInfo("Number of squares in drain")]
        public Parameter B2drainSquares { get; } = new Parameter(1);
        [SpiceName("off"), SpiceInfo("Device is initially off")]
        public bool B2off { get; set; }
        [SpiceName("vbs"), SpiceInfo("Initial B-S voltage")]
        public Parameter B2icVBS { get; } = new Parameter();
        [SpiceName("vds"), SpiceInfo("Initial D-S voltage")]
        public Parameter B2icVDS { get; } = new Parameter();
        [SpiceName("vgs"), SpiceInfo("Initial G-S voltage")]
        public Parameter B2icVGS { get; } = new Parameter();

        /// <summary>
        /// Methods
        /// </summary>
        [SpiceName("ic"), SpiceInfo("Vector of DS,GS,BS initial voltages")]
        public void SetIC(double[] value)
        {
            switch (value.Length)
            {
                case 3: B2icVBS.Set(value[2]); goto case 2;
                case 2: B2icVGS.Set(value[1]); goto case 1;
                case 1: B2icVDS.Set(value[0]); break;
                default:
                    throw new CircuitException("Bad parameter");
            }
        }

        /// <summary>
        /// Extra variables
        /// </summary>
        public double B2vdsat { get; internal set; }
        public double B2von { get; internal set; }
        public double B2drainConductance { get; internal set; }
        public double B2sourceConductance { get; internal set; }
        public double B2mode { get; internal set; }
        public int B2dNode { get; internal set; }
        public int B2gNode { get; internal set; }
        public int B2sNode { get; internal set; }
        public int B2bNode { get; internal set; }
        public int B2dNodePrime { get; internal set; }
        public int B2sNodePrime { get; internal set; }
        public int B2states { get; internal set; }

        internal BSIM2SizeDependParam pParam = null;

        /// <summary>
        /// Constants
        /// </summary>
        public const int B2vbd = 0;
        public const int B2vbs = 1;
        public const int B2vgs = 2;
        public const int B2vds = 3;
        public const int B2cd = 4;
        public const int B2id = 4;
        public const int B2cbs = 5;
        public const int B2ibs = 5;
        public const int B2cbd = 6;
        public const int B2ibd = 6;
        public const int B2gm = 7;
        public const int B2gds = 8;
        public const int B2gmbs = 9;
        public const int B2gbd = 10;
        public const int B2gbs = 11;
        public const int B2qb = 12;
        public const int B2cqb = 13;
        public const int B2iqb = 13;
        public const int B2qg = 14;
        public const int B2cqg = 15;
        public const int B2iqg = 15;
        public const int B2qd = 16;
        public const int B2cqd = 17;
        public const int B2iqd = 17;
        public const int B2cggb = 18;
        public const int B2cgdb = 19;
        public const int B2cgsb = 20;
        public const int B2cbgb = 21;
        public const int B2cbdb = 22;
        public const int B2cbsb = 23;
        public const int B2capbd = 24;
        public const int B2iqbd = 25;
        public const int B2cqbd = 25;
        public const int B2capbs = 26;
        public const int B2iqbs = 27;
        public const int B2cqbs = 27;
        public const int B2cdgb = 28;
        public const int B2cddb = 29;
        public const int B2cdsb = 30;
        public const int B2vono = 31;
        public const int B2vdsato = 32;
        public const int B2qbs = 33;
        public const int B2qbd = 34;
        public const int B2pinCount = 4;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the device</param>
        public BSIM2(CircuitIdentifier name) : base(name, B2pinCount)
        {
        }

        /// <summary>
        /// Setup the device
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Setup(Circuit ckt)
        {
            var model = Model as BSIM2Model;

            // Allocate nodes
            var nodes = BindNodes(ckt);
            B2dNode = nodes[0].Index;
            B2gNode = nodes[1].Index;
            B2sNode = nodes[2].Index;
            B2bNode = nodes[3].Index;

            /* process drain series resistance */
            if (model.B2sheetResistance != 0 && B2drainSquares != 0.0)
                B2dNodePrime = CreateNode(ckt, Name.Grow("#drain")).Index;
            else
                B2dNodePrime = B2dNode;

            /* process source series resistance */
            if (model.B2sheetResistance != 0 && B2sourceSquares != 0.0)
                B2sNodePrime = CreateNode(ckt, Name.Grow("#source")).Index;
            else
                B2sNodePrime = B2sNode;

            // Allocate states
            B2states = ckt.State.GetState(35);
            pParam = null;
        }
    }
}
