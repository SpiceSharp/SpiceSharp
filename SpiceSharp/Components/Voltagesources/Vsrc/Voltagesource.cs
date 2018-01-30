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
        [PropertyName("pos_node")]
        public int PosNode { get; private set; }
        [PropertyName("neg_node")]
        public int NegNode { get; private set; }

        /// <summary>
        /// Constants
        /// </summary>
        public const int VoltageSourcePinCount = 2;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name</param>
        public VoltageSource(Identifier name) : base(name, VoltageSourcePinCount)
        {
            // Register parameters
            Parameters.Add(new BaseParameters());
            Parameters.Add(new FrequencyParameters());

            // Register factories
            AddFactory(typeof(LoadBehavior), () => new LoadBehavior(Name));
            AddFactory(typeof(FrequencyBehavior), () => new FrequencyBehavior(Name));
            AddFactory(typeof(AcceptBehavior), () => new AcceptBehavior(Name));
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
            Parameters.Add(new BaseParameters(dc));
            Parameters.Add(new FrequencyParameters());

            // Register factories
            AddFactory(typeof(LoadBehavior), () => new LoadBehavior(Name));
            AddFactory(typeof(FrequencyBehavior), () => new FrequencyBehavior(Name));
            AddFactory(typeof(AcceptBehavior), () => new AcceptBehavior(Name));

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
            Parameters.Add(new BaseParameters(waveform));
            Parameters.Add(new FrequencyParameters());

            // Register factories
            AddFactory(typeof(LoadBehavior), () => new LoadBehavior(Name));
            AddFactory(typeof(FrequencyBehavior), () => new FrequencyBehavior(Name));
            AddFactory(typeof(AcceptBehavior), () => new AcceptBehavior(Name));

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
