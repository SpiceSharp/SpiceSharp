using SpiceSharp.Circuits;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors.Bipolar;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A bipolar junction transistor (BJT)
    /// </summary>
    [SpicePins("Collector", "Base", "Emitter", "Substrate")]
    public class BJT : Component
    {
        /// <summary>
        /// Set the model for the BJT
        /// </summary>
        public void SetModel(BJTModel model) => Model = model;

        /// <summary>
        /// Nodes
        /// </summary>
        [SpiceName("colnode"), SpiceInfo("Number of collector node")]
        public int BJTcolNode { get; private set; }
        [SpiceName("basenode"), SpiceInfo("Number of base node")]
        public int BJTbaseNode { get; private set; }
        [SpiceName("emitnode"), SpiceInfo("Number of emitter node")]
        public int BJTemitNode { get; private set; }
        [SpiceName("substnode"), SpiceInfo("Number of substrate node")]
        public int BJTsubstNode { get; private set; }
        [SpiceName("colprimenode"), SpiceInfo("Internal collector node")]
        public int BJTcolPrimeNode { get; private set; }
        [SpiceName("baseprimenode"), SpiceInfo("Internal base node")]
        public int BJTbasePrimeNode { get; private set; }
        [SpiceName("emitprimenode"), SpiceInfo("Internal emitter node")]
        public int BJTemitPrimeNode { get; private set; }

        /// <summary>
        /// Constants
        /// </summary>
        public const int BJTpinCount = 4;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the device</param>
        public BJT(Identifier name) : base(name, BJTpinCount)
        {
            RegisterBehavior(new LoadBehavior());
            RegisterBehavior(new TemperatureBehavior());
            RegisterBehavior(new AcBehavior());
            RegisterBehavior(new TruncateBehavior());
            RegisterBehavior(new NoiseBehavior());
        }

        /// <summary>
        /// Setup the device
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Setup(Circuit ckt)
        {
            BJTModel model = Model as BJTModel;

            // Allocate nodes
            var nodes = BindNodes(ckt);
            BJTcolNode = nodes[0].Index;
            BJTbaseNode = nodes[1].Index;
            BJTemitNode = nodes[2].Index;
            BJTsubstNode = nodes[3].Index;
        }
    }
}
