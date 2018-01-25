using SpiceSharp.Circuits;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors.Mosfet.Level1;
using SpiceSharp.Components.Mosfet.Level1;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A MOS1 Mosfet.
    /// Level 1, Shichman-Hodges.
    /// </summary>
    [PinsAttribute("Drain", "Gate", "Source", "Bulk"), ConnectedAttribute(0, 2, 3)]
    public class MOS1 : Component
    {
        /// <summary>
        /// Set the model for the MOS1 Mosfet
        /// </summary>
        public void SetModel(MOS1Model model) => Model = model;

        /// <summary>
        /// Nodes
        /// </summary>
        [NameAttribute("dnode"), InfoAttribute("Number of the drain node ")]
        public int MOS1dNode { get; protected set; }
        [NameAttribute("gnode"), InfoAttribute("Number of the gate node ")]
        public int MOS1gNode { get; protected set; }
        [NameAttribute("snode"), InfoAttribute("Number of the source node ")]
        public int MOS1sNode { get; protected set; }
        [NameAttribute("bnode"), InfoAttribute("Number of the node ")]
        public int MOS1bNode { get; protected set; }

        /// <summary>
        /// Constants
        /// </summary>
        public const int MOS1pinCount = 4;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the device</param>
        public MOS1(Identifier name) : base(name, MOS1pinCount)
        {
            // Add parameters
            Parameters.Add(new BaseParameters());

            // Add factories
            AddFactory(typeof(TemperatureBehavior), () => new TemperatureBehavior(Name));
            AddFactory(typeof(LoadBehavior), () => new LoadBehavior(Name));
            AddFactory(typeof(TransientBehavior), () => new TransientBehavior(Name));
            AddFactory(typeof(AcBehavior), () => new AcBehavior(Name));
            AddFactory(typeof(NoiseBehavior), () => new NoiseBehavior(Name));
        }

        /// <summary>
        /// Setup the device
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Setup(Circuit ckt)
        {
            // Allocate nodes
            var nodes = BindNodes(ckt);
            MOS1dNode = nodes[0].Index;
            MOS1gNode = nodes[1].Index;
            MOS1sNode = nodes[2].Index;
            MOS1bNode = nodes[3].Index;
        }
    }
}
