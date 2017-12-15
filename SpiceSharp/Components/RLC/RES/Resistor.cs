using SpiceSharp.Circuits;
using SpiceSharp.Behaviors.RES;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A resistor
    /// </summary>
    [SpicePins("R+", "R-")]
    public class Resistor : CircuitComponent
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
        public Resistor(CircuitIdentifier name) 
            : base(name, RESpinCount)
        {
            RegisterBehavior(new LoadBehavior());
            RegisterBehavior(new AcBehavior());
            RegisterBehavior(new NoiseBehavior());
            RegisterBehavior(new TemperatureBehavior());
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the resistor</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="res">The resistance</param>
        public Resistor(CircuitIdentifier name, CircuitIdentifier pos, CircuitIdentifier neg, double res) 
            : base(name, RESpinCount)
        {
            // Register behaviors
            RegisterBehavior(new LoadBehavior(res));
            RegisterBehavior(new AcBehavior());
            RegisterBehavior(new NoiseBehavior());
            RegisterBehavior(new TemperatureBehavior());

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
