using SpiceSharp.Attributes;
using SpiceSharp.Components.CurrentsourceBehaviors;
using SpiceSharp.Simulations;

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
        public int PosNode { get; private set; }
        public int NegNode { get; private set; }

        /// <summary>
        /// Constants
        /// </summary>
        [ParameterName("pincount"), ParameterInfo("Number of pins")]
		public const int CurrentSourcePinCount = 2;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the current source</param>
        public CurrentSource(Identifier name) 
            : base(name, CurrentSourcePinCount)
        {
            // Add parameters
            ParameterSets.Add(new BaseParameters());
            ParameterSets.Add(new FrequencyParameters());

            // Add factories
            Behaviors.Add(typeof(LoadBehavior), () => new LoadBehavior(Name));
            Behaviors.Add(typeof(FrequencyBehavior), () => new FrequencyBehavior(Name));
            Behaviors.Add(typeof(AcceptBehavior), () => new AcceptBehavior(Name));
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
            ParameterSets.Add(new BaseParameters(dc));
            ParameterSets.Add(new FrequencyParameters());

            // Add factories
            Behaviors.Add(typeof(LoadBehavior), () => new LoadBehavior(Name));
            Behaviors.Add(typeof(FrequencyBehavior), () => new FrequencyBehavior(Name));
            Behaviors.Add(typeof(AcceptBehavior), () => new AcceptBehavior(Name));

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
            ParameterSets.Add(new BaseParameters(waveform));
            ParameterSets.Add(new FrequencyParameters());

            // Add factories
            Behaviors.Add(typeof(LoadBehavior), () => new LoadBehavior(Name));
            Behaviors.Add(typeof(FrequencyBehavior), () => new FrequencyBehavior(Name));
            Behaviors.Add(typeof(AcceptBehavior), () => new AcceptBehavior(Name));

            // Connect
            Connect(pos, neg);
        }

        /// <summary>
        /// Setup the current source
        /// </summary>
        /// <param name="simulation">Simulation</param>
        public override void Setup(Simulation simulation)
        {
            var nodes = BindNodes(simulation);
            PosNode = nodes[0].Index;
            NegNode = nodes[1].Index;
        }
    }
}
