using SpiceSharp.Attributes;
using SpiceSharp.Components.VoltagesourceBehaviors;

namespace SpiceSharp.Components
{
    /// <summary>
    /// An independent voltage source
    /// </summary>
    [Pin(0, "V+"), Pin(1, "V-"), VoltageDriver(0, 1), IndependentSource]
    public class VoltageSource : Component
    {
        /// <summary>
        /// Nodes
        /// </summary>
        [ParameterName("pos_node")]
        public int PosNode { get; private set; }
        [ParameterName("neg_node")]
        public int NegNode { get; private set; }

        /// <summary>
        /// Constants
        /// </summary>
        [ParameterName("pincount"), ParameterInfo("Number of pins")]
		public const int VoltageSourcePinCount = 2;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name</param>
        public VoltageSource(Identifier name) : base(name, VoltageSourcePinCount)
        {
            // Register parameters
            ParameterSets.Add(new BaseParameters());
            ParameterSets.Add(new FrequencyParameters());

            // Register factories
            Behaviors.Add(typeof(LoadBehavior), () => new LoadBehavior(Name));
            Behaviors.Add(typeof(FrequencyBehavior), () => new FrequencyBehavior(Name));
            Behaviors.Add(typeof(AcceptBehavior), () => new AcceptBehavior(Name));
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the voltage source</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="dc">The DC value</param>
        public VoltageSource(Identifier name, Identifier pos, Identifier neg, double dc)
            : base(name, VoltageSourcePinCount)
        {
            // Register parameters
            ParameterSets.Add(new BaseParameters(dc));
            ParameterSets.Add(new FrequencyParameters());

            // Register factories
            Behaviors.Add(typeof(LoadBehavior), () => new LoadBehavior(Name));
            Behaviors.Add(typeof(FrequencyBehavior), () => new FrequencyBehavior(Name));
            Behaviors.Add(typeof(AcceptBehavior), () => new AcceptBehavior(Name));

            // Connect the device
            Connect(pos, neg);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the voltage source</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="waveform">The waveform</param>
        public VoltageSource(Identifier name, Identifier pos, Identifier neg, Waveform waveform) 
            : base(name, VoltageSourcePinCount)
        {
            // Register parameters
            ParameterSets.Add(new BaseParameters(waveform));
            ParameterSets.Add(new FrequencyParameters());

            // Register factories
            Behaviors.Add(typeof(LoadBehavior), () => new LoadBehavior(Name));
            Behaviors.Add(typeof(FrequencyBehavior), () => new FrequencyBehavior(Name));
            Behaviors.Add(typeof(AcceptBehavior), () => new AcceptBehavior(Name));

            // Connect the device
            Connect(pos, neg);
        }

        /// <summary>
        /// Setup the voltage source
        /// </summary>
        /// <param name="circuit">The circuit</param>
        public override void Setup(Circuit circuit)
        {
            // Bind the nodes
            var nodes = BindNodes(circuit);
            PosNode = nodes[0].Index;
            NegNode = nodes[1].Index;
        }
    }
}
