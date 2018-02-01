using SpiceSharp.Attributes;
using SpiceSharp.Components.MosfetBehaviors.Level1;

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
        [PropertyName("dnode"), PropertyInfo("Number of the drain node ")]
        public int DrainNode { get; protected set; }
        [PropertyName("gnode"), PropertyInfo("Number of the gate node ")]
        public int GateNode { get; protected set; }
        [PropertyName("snode"), PropertyInfo("Number of the source node ")]
        public int SourceNode { get; protected set; }
        [PropertyName("bnode"), PropertyInfo("Number of the node ")]
        public int BulkNode { get; protected set; }

        /// <summary>
        /// Constants
        /// </summary>
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
        /// <param name="circuit">The circuit</param>
        public override void Setup(Circuit circuit)
        {
            // Allocate nodes
            var nodes = BindNodes(circuit);
            DrainNode = nodes[0].Index;
            GateNode = nodes[1].Index;
            SourceNode = nodes[2].Index;
            BulkNode = nodes[3].Index;
        }
    }
}
