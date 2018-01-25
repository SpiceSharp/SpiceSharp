using SpiceSharp.Attributes;
using SpiceSharp.Circuits;
using SpiceSharp.Behaviors.VSRC;
using SpiceSharp.Components.VSRC;

namespace SpiceSharp.Components
{
    /// <summary>
    /// An independent voltage source
    /// </summary>
    [PinsAttribute("V+", "V-"), VoltageDriverAttribute(0, 1), IndependentSourceAttribute]
    public class Voltagesource : Component
    {
        /// <summary>
        /// Nodes
        /// </summary>
        [PropertyName("pos_node")]
        public int VSRCposNode { get; private set; }
        [PropertyName("neg_node")]
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
            // Register parameters
            Parameters.Add(new BaseParameters());
            Parameters.Add(new AcParameters());

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
        public Voltagesource(Identifier name, Identifier pos, Identifier neg, double dc)
            : base(name, VSRCpinCount)
        {
            // Register parameters
            Parameters.Add(new BaseParameters(dc));
            Parameters.Add(new AcParameters());

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
        /// <param name="w">The waveform</param>
        public Voltagesource(Identifier name, Identifier pos, Identifier neg, Waveform w) 
            : base(name, VSRCpinCount)
        {
            // Register parameters
            Parameters.Add(new BaseParameters(w));
            Parameters.Add(new AcParameters());

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
