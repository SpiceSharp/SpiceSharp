using SpiceSharp.Attributes;
using SpiceSharp.Components.MosfetBehaviors.Level1;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A Mosfet.
    /// Level 1, Shichman-Hodges.
    /// </summary>
    [Pin(0, "Drain"), Pin(1, "Gate"), Pin(2, "Source"), Pin(3, "Bulk"), Connected(0, 2), Connected(0, 3)]
    public class Mosfet1 : Component
    {
        /// <summary>
        /// Set the model for the MOS1 Mosfet
        /// </summary>
        public void SetModel(Mosfet1Model model) => Model = model;

        /// <summary>
        /// Nodes
        /// </summary>
        [ParameterName("dnode"), ParameterInfo("Number of the drain node ")]
        public int DrainNode { get; protected set; }
        [ParameterName("gnode"), ParameterInfo("Number of the gate node ")]
        public int GateNode { get; protected set; }
        [ParameterName("snode"), ParameterInfo("Number of the source node ")]
        public int SourceNode { get; protected set; }
        [ParameterName("bnode"), ParameterInfo("Number of the node ")]
        public int BulkNode { get; protected set; }

        /// <summary>
        /// Constants
        /// </summary>
        [ParameterName("pincount"), ParameterInfo("Number of pins")]
		public const int Mosfet1PinCount = 4;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the device</param>
        public Mosfet1(Identifier name) : base(name, Mosfet1PinCount)
        {
            // Add parameters
            ParameterSets.Add(new BaseParameters());

            // Add factories
            Behaviors.Add(typeof(TemperatureBehavior), () => new TemperatureBehavior(Name));
            Behaviors.Add(typeof(LoadBehavior), () => new LoadBehavior(Name));
            Behaviors.Add(typeof(TransientBehavior), () => new TransientBehavior(Name));
            Behaviors.Add(typeof(FrequencyBehavior), () => new FrequencyBehavior(Name));
            Behaviors.Add(typeof(NoiseBehavior), () => new NoiseBehavior(Name));
        }

        /// <summary>
        /// Setup the device
        /// </summary>
        /// <param name="simulation">Simulation</param>
        public override void Setup(Simulation simulation)
        {
            // Allocate nodes
            var nodes = BindNodes(simulation);
            DrainNode = nodes[0].Index;
            GateNode = nodes[1].Index;
            SourceNode = nodes[2].Index;
            BulkNode = nodes[3].Index;
        }
    }
}
