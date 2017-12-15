using SpiceSharp.Circuits;
using SpiceSharp.Components.IND;

namespace SpiceSharp.Components
{
    /// <summary>
    /// An inductor
    /// </summary>
    [SpicePins("L+", "L-")]
    public class Inductor : CircuitComponent
    {
        /// <summary>
        /// Nodes
        /// </summary>
        public int INDposNode { get; internal set; }
        public int INDnegNode { get; internal set; }

        /// <summary>
        /// Constants
        /// </summary>
        public const int INDpinCount = 2;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the inductor</param>
        public Inductor(CircuitIdentifier name)
            : base(name, INDpinCount)
        {
            RegisterBehavior(new LoadBehavior());
            RegisterBehavior(new AcBehavior());
            RegisterBehavior(new AcceptBehavior());
            RegisterBehavior(new TruncateBehavior());
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the inductor</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="ind">The inductance</param>
        public Inductor(CircuitIdentifier name, CircuitIdentifier pos, CircuitIdentifier neg, double ind) 
            : base(name, INDpinCount)
        {
            Connect(pos, neg);

            // Set inductance
            var load = new LoadBehavior();
            load.INDinduct.Set(ind);
            RegisterBehavior(load);

            // Register behaviors
            RegisterBehavior(new AcBehavior());
            RegisterBehavior(new AcceptBehavior());
            RegisterBehavior(new TruncateBehavior());
        }

        /// <summary>
        /// Setup the inductor
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Setup(Circuit ckt)
        {
            var nodes = BindNodes(ckt);
            INDposNode = nodes[0].Index;
            INDnegNode = nodes[1].Index;
        }
    }
}
