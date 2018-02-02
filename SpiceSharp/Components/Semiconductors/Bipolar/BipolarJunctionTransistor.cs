using SpiceSharp.Attributes;
using SpiceSharp.Components.BipolarBehaviors;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A bipolar junction transistor (BJT)
    /// </summary>
    [Pin(0, "Collector"), Pin(1, "Base"), Pin(2, "Emitter"), Pin(3, "Substrate")]
    public class BipolarJunctionTransistor : Component
    {
        /// <summary>
        /// Set the model for the BJT
        /// </summary>
        public void SetModel(BipolarJunctionTransistorModel model) => Model = model;

        /// <summary>
        /// Nodes
        /// </summary>
        [PropertyName("colnode"), PropertyInfo("Number of collector node")]
        public int CollectorNode { get; private set; }
        [PropertyName("basenode"), PropertyInfo("Number of base node")]
        public int BaseNode { get; private set; }
        [PropertyName("emitnode"), PropertyInfo("Number of emitter node")]
        public int EmitterNode { get; private set; }
        [PropertyName("substnode"), PropertyInfo("Number of substrate node")]
        public int SubstrateNode { get; private set; }

        /// <summary>
        /// Constants
        /// </summary>
        [PropertyName("pincount"), PropertyInfo("Number of pins")]
		public const int BipolarJunctionTransistorPinCount = 4;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the device</param>
        public BipolarJunctionTransistor(Identifier name) 
            : base(name, BipolarJunctionTransistorPinCount)
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
            var nodes = BindNodes(circuit);
            CollectorNode = nodes[0].Index;
            BaseNode = nodes[1].Index;
            EmitterNode = nodes[2].Index;
            SubstrateNode = nodes[3].Index;
        }
    }
}
