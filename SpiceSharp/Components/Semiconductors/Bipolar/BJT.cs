using SpiceSharp.Circuits;
using SpiceSharp.Attributes;
using SpiceSharp.Components.Bipolar;
using SpiceSharp.Behaviors.Bipolar;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A bipolar junction transistor (BJT)
    /// </summary>
    [PinsAttribute("Collector", "Base", "Emitter", "Substrate")]
    public class BJT : Component
    {
        /// <summary>
        /// Set the model for the BJT
        /// </summary>
        public void SetModel(BJTModel model) => Model = model;

        /// <summary>
        /// Nodes
        /// </summary>
        [NameAttribute("colnode"), InfoAttribute("Number of collector node")]
        public int BJTcolNode { get; private set; }
        [NameAttribute("basenode"), InfoAttribute("Number of base node")]
        public int BJTbaseNode { get; private set; }
        [NameAttribute("emitnode"), InfoAttribute("Number of emitter node")]
        public int BJTemitNode { get; private set; }
        [NameAttribute("substnode"), InfoAttribute("Number of substrate node")]
        public int BJTsubstNode { get; private set; }

        /// <summary>
        /// Constants
        /// </summary>
        public const int BJTpinCount = 4;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the device</param>
        public BJT(Identifier name) 
            : base(name, BJTpinCount)
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
