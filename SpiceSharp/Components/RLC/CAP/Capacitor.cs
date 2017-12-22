using SpiceSharp.Circuits;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors.CAP;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A capacitor
    /// </summary>
    [SpicePins("C+", "C-"), ConnectedPins()]
    public class Capacitor : Component
    {
        /// <summary>
        /// Set the model for the capacitor
        /// </summary>
        /// <param name="model"></param>
        public void SetModel(CapacitorModel model) => Model = model;

        /// <summary>
        /// Nodes
        /// </summary>
        [SpiceName("pos"), SpiceInfo("Positive terminal of the capacitor")]
        public int CAPposNode { get; private set; }
        [SpiceName("neg"), SpiceInfo("Negative terminal of the capacitor")]
        public int CAPnegNode { get; private set; }

        /// <summary>
        /// Constants
        /// </summary>
        public const int CAPpinCount = 2;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name"></param>
        public Capacitor(Identifier name) : base(name, CAPpinCount)
        {
            RegisterBehavior(new TransientBehavior());
            RegisterBehavior(new AcBehavior());
            RegisterBehavior(new TemperatureBehavior());
            RegisterBehavior(new AcceptBehavior());
            RegisterBehavior(new TruncateBehavior());
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the capacitor</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="cap">The capacitance</param>
        public Capacitor(Identifier name, Identifier pos, Identifier neg, double cap) 
            : base(name, CAPpinCount)
        {
            // Register behaviors
            RegisterBehavior(new TransientBehavior(cap));
            RegisterBehavior(new AcBehavior());
            RegisterBehavior(new TemperatureBehavior());
            RegisterBehavior(new AcceptBehavior());
            RegisterBehavior(new TruncateBehavior());

            // Connect
            Connect(pos, neg);
        }
        
        /// <summary>
        /// Setup the capacitor
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Setup(Circuit ckt)
        {
            var nodes = BindNodes(ckt);
            CAPposNode = nodes[0].Index;
            CAPnegNode = nodes[1].Index;
        }
    }
}
