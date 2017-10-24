using SpiceSharp.Circuits;
using SpiceSharp.Parameters;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A voltage-controlled switch
    /// </summary>
    [SpicePins("S+", "S-", "SC+", "SC-"), ConnectedPins(0, 1)]
    public class VoltageSwitch : CircuitComponent<VoltageSwitch>
    {
        /// <summary>
        /// Register default behaviours
        /// </summary>
        static VoltageSwitch()
        {
            Behaviours.Behaviours.RegisterBehaviour(typeof(VoltageSwitch), typeof(ComponentBehaviours.VoltageSwitchLoadBehaviour));
            Behaviours.Behaviours.RegisterBehaviour(typeof(VoltageSwitch), typeof(ComponentBehaviours.VoltageSwitchAcBehaviour));
        }

        /// <summary>
        /// Set the model for the voltage-controlled switch
        /// </summary>
        public void SetModel(VoltageSwitchModel model) => Model = model;

        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("on"), SpiceInfo("Switch initially closed")]
        public void SetOn() { VSWzero_state = true; }
        [SpiceName("off"), SpiceInfo("Switch initially open")]
        public void SetOff() { VSWzero_state = false; }

        /// <summary>
        /// Nodes
        /// </summary>
        [SpiceName("pos_node"), SpiceInfo("Positive node of the switch")]
        public int VSWposNode { get; internal set; }
        [SpiceName("neg_node"), SpiceInfo("Negative node of the switch")]
        public int VSWnegNode { get; internal set; }
        [SpiceName("cont_p_node"), SpiceInfo("Positive controlling node of the switch")]
        public int VSWcontPosNode { get; internal set; }
        [SpiceName("cont_n_node"), SpiceInfo("Negative controlling node of the switch")]
        public int VSWcontNegNode { get; internal set; }
        internal bool VSWzero_state = false;
        public int VSWstate { get; internal set; }
        public double VSWcond { get; internal set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the voltage-controlled switch</param>
        public VoltageSwitch(CircuitIdentifier name) : base(name) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the voltage-controlled switch</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="cont_pos">The positive controlling node</param>
        /// <param name="cont_neg">The negative controlling node</param>
        public VoltageSwitch(CircuitIdentifier name, CircuitIdentifier pos, CircuitIdentifier neg, CircuitIdentifier cont_pos, CircuitIdentifier cont_neg) : base(name)
        {
            Connect(pos, neg, cont_pos, cont_neg);
        }

        /// <summary>
        /// Setup the voltage-controlled switch
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Setup(Circuit ckt)
        {
            var nodes = BindNodes(ckt);
            VSWposNode = nodes[0].Index;
            VSWnegNode = nodes[1].Index;
            VSWcontPosNode = nodes[2].Index;
            VSWcontNegNode = nodes[3].Index;

            VSWstate = ckt.State.GetState();
        }
    }
}
