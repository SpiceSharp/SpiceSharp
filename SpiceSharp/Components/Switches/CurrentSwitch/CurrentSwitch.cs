using SpiceSharp.Circuits;
using SpiceSharp.Parameters;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A current-controlled switch
    /// </summary>
    [SpicePins("W+", "W-")]
    public class CurrentSwitch : CircuitComponent<CurrentSwitch>
    {
        /// <summary>
        /// Register default behaviours
        /// </summary>
        static CurrentSwitch()
        {
            Behaviors.Behaviors.RegisterBehavior(typeof(CurrentSwitch), typeof(ComponentBehaviors.CurrentSwitchLoadBehavior));
            Behaviors.Behaviors.RegisterBehavior(typeof(CurrentSwitch), typeof(ComponentBehaviors.CurrentSwitchAcBehavior));
        }

        /// <summary>
        /// Set the model for the current-controlled switch
        /// </summary>
        public void SetModel(CurrentSwitchModel model) => Model = model;

        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("on"), SpiceInfo("Initially closed")]
        public void SetOn() { CSWzero_state = true; }
        [SpiceName("off"), SpiceInfo("Initially open")]
        public void SetOff() { CSWzero_state = false; }
        [SpiceName("control"), SpiceInfo("Name of the controlling source")]
        public CircuitIdentifier CSWcontName { get; set; }
        [SpiceName("i"), SpiceInfo("Switch current")]
        public double GetCurrent(Circuit ckt) => (ckt.State.Solution[CSWposNode] - ckt.State.Solution[CSWnegNode]) * CSWcond;
        [SpiceName("p"), SpiceInfo("Instantaneous power")]
        public double GetPower(Circuit ckt) => (ckt.State.Solution[CSWposNode] - ckt.State.Solution[CSWnegNode]) *
            (ckt.State.Solution[CSWposNode] - ckt.State.Solution[CSWnegNode]) * CSWcond;
        public double CSWcond { get; internal set; }

        /// <summary>
        /// Nodes
        /// </summary>
        [SpiceName("pos_node"), SpiceInfo("Positive node of the switch")]
        public int CSWposNode { get; internal set; }
        [SpiceName("neg_node"), SpiceInfo("Negative node of the switch")]
        public int CSWnegNode { get; internal set; }
        internal int CSWcontBranch;
        public int CSWstate { get; internal set; }

        /// <summary>
        /// Private variables
        /// </summary>
        internal bool CSWzero_state = false;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the current-controlled switch</param>
        public CurrentSwitch(CircuitIdentifier name) : base(name)
        {
            // Make sure the current switch is processed after voltage sources
            Priority = -1;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the current-controlled switch</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="vsource">The controlling voltage source</param>
        public CurrentSwitch(CircuitIdentifier name, CircuitIdentifier pos, CircuitIdentifier neg, CircuitIdentifier vsource) : base(name)
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
                CSWcontBranch = vsrc.VSRCbranch;

            CSWstate = ckt.State.GetState();
        }
    }
}
