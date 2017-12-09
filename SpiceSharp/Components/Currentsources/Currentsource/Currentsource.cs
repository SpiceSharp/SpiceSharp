using System.Numerics;
using SpiceSharp.Circuits;
using SpiceSharp.Components.ComponentBehaviors;

namespace SpiceSharp.Components
{
    /// <summary>
    /// An independent current source
    /// </summary>
    [SpicePins("I+", "I-"), IndependentSource, ConnectedPins()]
    public class Currentsource : CircuitComponent
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
        public Currentsource(CircuitIdentifier name) : base(name, ISRCpinCount)
        {
            RegisterBehavior(new CurrentsourceLoadBehavior());
            RegisterBehavior(new CurrentsourceAcBehavior());
            RegisterBehavior(new CurrentsourceAcceptBehavior());
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the current source</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="dc">The DC value</param>
        public Currentsource(CircuitIdentifier name, CircuitIdentifier pos, CircuitIdentifier neg, double dc) : base(name, ISRCpinCount)
        {
            Connect(pos, neg);

            var loadbehavior = new CurrentsourceLoadBehavior();
            loadbehavior.ISRCdcValue.Set(dc);
            RegisterBehavior(loadbehavior);
            RegisterBehavior(new CurrentsourceAcBehavior());
            RegisterBehavior(new CurrentsourceAcceptBehavior());
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the current source</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="w">The Waveform-object</param>
        public Currentsource(CircuitIdentifier name, CircuitIdentifier pos, CircuitIdentifier neg, Waveform w) : base(name, ISRCpinCount)
        {
            Connect(pos, neg);

            var loadbehavior = new CurrentsourceLoadBehavior();
            loadbehavior.ISRCwaveform = w;
            RegisterBehavior(new CurrentsourceAcBehavior());
            RegisterBehavior(new CurrentsourceAcceptBehavior());
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
