using SpiceSharp.Circuits;
using SpiceSharp.Parameters;
using SpiceSharp.Components.ComponentBehaviors;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A capacitor
    /// </summary>
    [SpicePins("C+", "C-"), ConnectedPins()]
    public class Capacitor : CircuitComponent
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
        public Capacitor(CircuitIdentifier name) : base(name, CAPpinCount)
        {
            RegisterBehavior(new CapacitorLoadBehavior());
            RegisterBehavior(new CapacitorAcBehavior());
            RegisterBehavior(new CapacitorTemperatureBehavior());
            RegisterBehavior(new CapacitorAcceptBehavior());
            RegisterBehavior(new CapacitorTruncateBehavior());
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the capacitor</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="cap">The capacitance</param>
        public Capacitor(CircuitIdentifier name, CircuitIdentifier pos, CircuitIdentifier neg, double cap) : base(name, CAPpinCount)
        {
            Connect(pos, neg);

            var loadbehavior = new CapacitorLoadBehavior();
            Set("capacitance", cap);
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
