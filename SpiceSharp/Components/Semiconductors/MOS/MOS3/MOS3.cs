using SpiceSharp.Circuits;
using SpiceSharp.Parameters;
using SpiceSharp.Behaviors.MOS3;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A MOS3 Mosfet
    /// Level 3, a semi-empirical model(see reference for level 3).
    /// </summary>
    [SpicePins("Drain", "Gate", "Source", "Bulk"), ConnectedPins(0, 2, 3)]
    public class MOS3 : CircuitComponent
    {
        /// <summary>
        /// Set the model for the MOS3 model
        /// </summary>
        public void SetModel(MOS3Model model) => Model = model;

        /// <summary>
        /// Nodes
        /// </summary>
        [SpiceName("dnode"), SpiceInfo("Number of drain node")]
        public int MOS3dNode { get; internal set; }
        [SpiceName("gnode"), SpiceInfo("Number of gate node")]
        public int MOS3gNode { get; internal set; }
        [SpiceName("snode"), SpiceInfo("Number of source node")]
        public int MOS3sNode { get; internal set; }
        [SpiceName("bnode"), SpiceInfo("Number of bulk node")]
        public int MOS3bNode { get; internal set; }

        /// <summary>
        /// Constants
        /// </summary>
        public const int MOS3pinCount = 4;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the device</param>
        public MOS3(CircuitIdentifier name) : base(name, MOS3pinCount)
        {
            RegisterBehavior(new TemperatureBehavior());
            RegisterBehavior(new LoadBehavior());
            RegisterBehavior(new AcBehavior());
            RegisterBehavior(new NoiseBehavior());
            RegisterBehavior(new TruncateBehavior());
        }

        /// <summary>
        /// Setup the device
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Setup(Circuit ckt)
        {
            // Allocate nodes
            var nodes = BindNodes(ckt);
            MOS3dNode = nodes[0].Index;
            MOS3gNode = nodes[1].Index;
            MOS3sNode = nodes[2].Index;
            MOS3bNode = nodes[3].Index;
        }
    }
}
