using SpiceSharp.Parameters;
using SpiceSharp.Circuits;
using SpiceSharp.Behaviors.VSRC;

namespace SpiceSharp.Components
{
    /// <summary>
    /// An independent voltage source
    /// </summary>
    [SpicePins("V+", "V-"), VoltageDriver(0, 1), IndependentSource]
    public class Voltagesource : Component
    {
        /// <summary>
        /// Nodes
        /// </summary>
        [SpiceName("pos_node")]
        public int VSRCposNode { get; private set; }
        [SpiceName("neg_node")]
        public int VSRCnegNode { get; private set; }

        /// <summary>
        /// Constants
        /// </summary>
        public const int VSRCpinCount = 2;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name</param>
        public Voltagesource(Identifier name) : base(name, VSRCpinCount)
        {
            RegisterBehavior(new LoadBehavior());
            RegisterBehavior(new AcBehavior());
            RegisterBehavior(new AcceptBehavior());
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the voltage source</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="dc">The DC value</param>
        public Voltagesource(Identifier name, Identifier pos, Identifier neg, double dc)
            : base(name, VSRCpinCount)
        {
            Connect(pos, neg);

            // Set waveform
            var load = new LoadBehavior();
            load.VSRCdcValue.Set(dc);
            RegisterBehavior(load);

            // Register behaviors
            RegisterBehavior(new AcBehavior());
            RegisterBehavior(new AcceptBehavior());
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the voltage source</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="w">The waveform</param>
        public Voltagesource(Identifier name, Identifier pos, Identifier neg, Waveform w) 
            : base(name, VSRCpinCount)
        {
            Connect(pos, neg);

            // Set waveform
            var load = new LoadBehavior();
            load.VSRCwaveform = w;
            RegisterBehavior(load);

            // Register behaviors
            RegisterBehavior(new AcBehavior());
            RegisterBehavior(new AcceptBehavior());
        }

        /// <summary>
        /// Setup the voltage source
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Setup(Circuit ckt)
        {
            // Bind the nodes
            var nodes = BindNodes(ckt);
            VSRCposNode = nodes[0].Index;
            VSRCnegNode = nodes[1].Index;
        }
    }
}
