using SpiceSharp.Components.CurrentsourceBehaviors;
using SpiceSharp.Attributes;

namespace SpiceSharp.Components
{
    /// <summary>
    /// An independent current source
    /// </summary>
    [Pin(0, "I+"), Pin(1, "I-"), IndependentSource, Connected]
    public class CurrentSource : Component
    {
        /// <summary>
        /// Nodes
        /// </summary>
        public int PosourceNode { get; private set; }
        public int NegateNode { get; private set; }

        /// <summary>
        /// Constants
        /// </summary>
        public const int CurrentSourcePinCount = 2;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the current source</param>
        public CurrentSource(Identifier name) 
            : base(name, CurrentSourcePinCount)
        {
            // Add parameters
            Parameters.Add(new BaseParameters());
            Parameters.Add(new FrequencyParameters());

            // Add factories
            AddFactory(typeof(LoadBehavior), () => new LoadBehavior(Name));
            AddFactory(typeof(FrequencyBehavior), () => new FrequencyBehavior(Name));
            AddFactory(typeof(AcceptBehavior), () => new AcceptBehavior(Name));
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the current source</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="dc">The DC value</param>
        public CurrentSource(Identifier name, Identifier pos, Identifier neg, double dc)
            : base(name, CurrentSourcePinCount)
        {
            // Add parameters
            Parameters.Add(new BaseParameters(dc));
            Parameters.Add(new FrequencyParameters());

            // Add factories
            AddFactory(typeof(LoadBehavior), () => new LoadBehavior(Name));
            AddFactory(typeof(FrequencyBehavior), () => new FrequencyBehavior(Name));
            AddFactory(typeof(AcceptBehavior), () => new AcceptBehavior(Name));

            // Connect
            Connect(pos, neg);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the current source</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="waveform">The Waveform-object</param>
        public CurrentSource(Identifier name, Identifier pos, Identifier neg, Waveform waveform)
            : base(name, CurrentSourcePinCount)
        {
            // Add parameters
            Parameters.Add(new BaseParameters(waveform));
            Parameters.Add(new FrequencyParameters());

            // Add factories
            AddFactory(typeof(LoadBehavior), () => new LoadBehavior(Name));
            AddFactory(typeof(FrequencyBehavior), () => new FrequencyBehavior(Name));
            AddFactory(typeof(AcceptBehavior), () => new AcceptBehavior(Name));

            // Connect
            Connect(pos, neg);
        }

        /// <summary>
        /// Setup the current source
        /// </summary>
        /// <param name="circuit">The circuit</param>
        public override void Setup(Circuit circuit)
        {
            var nodes = BindNodes(circuit);
            PosourceNode = nodes[0].Index;
            NegateNode = nodes[1].Index;
        }
    }
}
