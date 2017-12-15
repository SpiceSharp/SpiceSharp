using SpiceSharp.Circuits;
using SpiceSharp.Behaviors.ISRC;

namespace SpiceSharp.Components
{
    /// <summary>
    /// An independent current source
    /// </summary>
    [SpicePins("I+", "I-"), IndependentSource, ConnectedPins()]
    public class Currentsource : Component
    {
        /// <summary>
        /// Nodes
        /// </summary>
        public int ISRCposNode { get; private set; }
        public int ISRCnegNode { get; private set; }

        /// <summary>
        /// Constants
        /// </summary>
        private const int ISRCpinCount = 2;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the current source</param>
        public Currentsource(Identifier name) : base(name, ISRCpinCount)
        {
            RegisterBehavior(new LoadBehavior());
            RegisterBehavior(new AcBehavior());
            RegisterBehavior(new AcceptBehavior());
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the current source</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="dc">The DC value</param>
        public Currentsource(Identifier name, Identifier pos, Identifier neg, double dc)
            : this(name)
        {
            // Register behaviors
            RegisterBehavior(new LoadBehavior(dc));
            RegisterBehavior(new AcBehavior());
            RegisterBehavior(new AcceptBehavior());

            // Connect
            Connect(pos, neg);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the current source</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="w">The Waveform-object</param>
        public Currentsource(Identifier name, Identifier pos, Identifier neg, Waveform w) : base(name, ISRCpinCount)
        {
            // Register behaviors
            RegisterBehavior(new LoadBehavior(w));
            RegisterBehavior(new AcBehavior());
            RegisterBehavior(new AcceptBehavior());

            // Connect
            Connect(pos, neg);
        }

        /// <summary>
        /// Setup the current source
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Setup(Circuit ckt)
        {
            var nodes = BindNodes(ckt);
            ISRCposNode = nodes[0].Index;
            ISRCnegNode = nodes[1].Index;
        }
    }
}
