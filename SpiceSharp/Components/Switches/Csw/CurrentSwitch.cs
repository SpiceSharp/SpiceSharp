using SpiceSharp.Circuits;
using SpiceSharp.Parameters;
using SpiceSharp.Behaviors.CSW;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A current-controlled switch
    /// </summary>
    [SpicePins("W+", "W-")]
    public class CurrentSwitch : Component
    {
        /// <summary>
        /// Set the model for the current-controlled switch
        /// </summary>
        public void SetModel(CurrentSwitchModel model) => Model = model;

        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("control"), SpiceInfo("Name of the controlling source")]
        public Identifier CSWcontName { get; set; }
        
        /// <summary>
        /// Nodes
        /// </summary>
        [SpiceName("pos_node"), SpiceInfo("Positive node of the switch")]
        public int CSWposNode { get; internal set; }
        [SpiceName("neg_node"), SpiceInfo("Negative node of the switch")]
        public int CSWnegNode { get; internal set; }

        /// <summary>
        /// Get the controlling voltage source
        /// </summary>
        public Voltagesource CSWcontSource { get; protected set; }

        /// <summary>
        /// Constants
        /// </summary>
        public const int CSWpinCount = 2;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the current-controlled switch</param>
        public CurrentSwitch(Identifier name) : base(name, CSWpinCount)
        {
            // Make sure the current switch is processed after voltage sources
            Priority = -1;
            RegisterBehavior(new LoadBehavior());
            RegisterBehavior(new AcBehavior());
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the current-controlled switch</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="vsource">The controlling voltage source</param>
        public CurrentSwitch(Identifier name, Identifier pos, Identifier neg, Identifier vsource) : base(name, 2)
        {
            Connect(pos, neg);
            CSWcontName = vsource;
            Priority = -1;
        }

        /// <summary>
        /// Setup the current-controlled switch
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Setup(Circuit ckt)
        {
            var nodes = BindNodes(ckt);
            CSWposNode = nodes[0].Index;
            CSWnegNode = nodes[1].Index;

            // Find the voltage source
            if (ckt.Objects[CSWcontName] is Voltagesource vsrc)
                CSWcontSource = vsrc;
        }
    }
}
