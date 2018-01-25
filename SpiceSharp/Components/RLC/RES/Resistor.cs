using SpiceSharp.Circuits;
using SpiceSharp.Components.RES;
using SpiceSharp.Behaviors.RES;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A resistor
    /// </summary>
    [SpicePins("R+", "R-")]
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
        public int RESposNode { get; private set; }
        public int RESnegNode { get; private set; }
        
        /// <summary>
        /// Constants
        /// </summary>
        public const int RESpinCount = 2;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the resistor</param>
        public Resistor(Identifier name) 
            : base(name, RESpinCount)
        {
            // Register parameters
            Parameters.Add(new BaseParameters());

            // Register factories
            AddFactory(typeof(LoadBehavior), () => new LoadBehavior(Name));
            AddFactory(typeof(AcBehavior), () => new AcBehavior(Name));
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
            : base(name, RESpinCount)
        {
            // Register parameters
            Parameters.Add(new BaseParameters(res));

            // Register factories
            AddFactory(typeof(LoadBehavior), () => new LoadBehavior(Name));
            AddFactory(typeof(AcBehavior), () => new AcBehavior(Name));
            AddFactory(typeof(NoiseBehavior), () => new NoiseBehavior(Name));
            AddFactory(typeof(TemperatureBehavior), () => new TemperatureBehavior(Name));

            // Connect
            Connect(pos, neg);
        }

        /// <summary>
        /// Setup the resistor
        /// </summary>
        /// <param name="ckt"></param>
        public override void Setup(Circuit ckt)
        {
            var nodes = BindNodes(ckt);
            RESposNode = nodes[0].Index;
            RESnegNode = nodes[1].Index;
        }
    }
}
