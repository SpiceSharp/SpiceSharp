using SpiceSharp.Attributes;
using SpiceSharp.Components.ResistorBehaviors;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A resistor
    /// </summary>
    [Pin(0, "R+"), Pin(1, "R-")]
    public class Resistor : Component
    {
        /// <summary>
        /// Set the model for the resistor
        /// </summary>
        /// <param name="model"></param>
        public void SetModel(ResistorModel model) => Model = model;

        /// <summary>
        /// Nodes
        /// </summary>
        public int PosourceNode { get; private set; }
        public int NegateNode { get; private set; }
        
        /// <summary>
        /// Constants
        /// </summary>
        public const int ResistorPinCount = 2;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the resistor</param>
        public Resistor(Identifier name) 
            : base(name, ResistorPinCount)
        {
            // Register parameters
            Parameters.Add(new BaseParameters());

            // Register factories
            AddFactory(typeof(LoadBehavior), () => new LoadBehavior(Name));
            AddFactory(typeof(FrequencyBehavior), () => new FrequencyBehavior(Name));
            AddFactory(typeof(NoiseBehavior), () => new NoiseBehavior(Name));
            AddFactory(typeof(TemperatureBehavior), () => new TemperatureBehavior(Name));
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the resistor</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="res">The resistance</param>
        public Resistor(Identifier name, Identifier pos, Identifier neg, double res) 
            : base(name, ResistorPinCount)
        {
            // Register parameters
            Parameters.Add(new BaseParameters(res));

            // Register factories
            AddFactory(typeof(LoadBehavior), () => new LoadBehavior(Name));
            AddFactory(typeof(FrequencyBehavior), () => new FrequencyBehavior(Name));
            AddFactory(typeof(NoiseBehavior), () => new NoiseBehavior(Name));
            AddFactory(typeof(TemperatureBehavior), () => new TemperatureBehavior(Name));

            // Connect
            Connect(pos, neg);
        }

        /// <summary>
        /// Setup the resistor
        /// </summary>
        /// <param name="circuit"></param>
        public override void Setup(Circuit circuit)
        {
            var nodes = BindNodes(circuit);
            PosourceNode = nodes[0].Index;
            NegateNode = nodes[1].Index;
        }
    }
}
