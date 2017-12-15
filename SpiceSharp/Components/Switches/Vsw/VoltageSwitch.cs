using SpiceSharp.Circuits;
using SpiceSharp.Behaviors.VSW;
using SpiceSharp.Parameters;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A voltage-controlled switch
    /// </summary>
    [SpicePins("S+", "S-", "SC+", "SC-"), ConnectedPins(0, 1)]
    public class VoltageSwitch : CircuitComponent
    {
        /// <summary>
        /// Set the model for the voltage-controlled switch
        /// </summary>
        public void SetModel(VoltageSwitchModel model) => Model = model;

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

        /// <summary>
        /// Constants
        /// </summary>
        public const int SWpinCount = 4;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the voltage-controlled switch</param>
        public VoltageSwitch(CircuitIdentifier name) : base(name, SWpinCount)
        {
            RegisterBehavior(new LoadBehavior());
            RegisterBehavior(new AcBehavior());
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the voltage-controlled switch</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="cont_pos">The positive controlling node</param>
        /// <param name="cont_neg">The negative controlling node</param>
        public VoltageSwitch(CircuitIdentifier name, CircuitIdentifier pos, CircuitIdentifier neg, CircuitIdentifier cont_pos, CircuitIdentifier cont_neg) 
            : this(name)
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
        }
    }
}
