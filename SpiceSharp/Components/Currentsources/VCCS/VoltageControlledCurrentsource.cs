using SpiceSharp.Circuits;
using SpiceSharp.Parameters;
using SpiceSharp.Behaviors.VCCS;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A voltage-controlled current source
    /// </summary>
    [SpicePins("V+", "V-", "VC+", "VC-"), ConnectedPins(0, 1)]
    public class VoltageControlledCurrentsource : Component
    {
        /// <summary>
        /// Nodes
        /// </summary>
        [SpiceName("pos_node"), SpiceInfo("Positive node of the source")]
        public int VCCSposNode { get; private set; }
        [SpiceName("neg_node"), SpiceInfo("Negative node of the source")]
        public int VCCSnegNode { get; private set; }
        [SpiceName("cont_p_node"), SpiceInfo("Positive node of the controlling source voltage")]
        public int VCCScontPosNode { get; private set; }
        [SpiceName("cont_n_node"), SpiceInfo("Negative node of the controlling source voltage")]
        public int VCCScontNegNode { get; private set; }

        /// <summary>
        /// Private constants
        /// </summary>
        public const int VCCSpinCount = 4;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the voltage-controlled current source</param>
        public VoltageControlledCurrentsource(CircuitIdentifier name) : base(name, VCCSpinCount)
        {
            RegisterBehavior(new LoadBehavior());
            RegisterBehavior(new AcBehavior());
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the voltage-controlled current source</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="cont_pos">The positive controlling node</param>
        /// <param name="cont_neg">The negative controlling node</param>
        /// <param name="gain">The transconductance gain</param>
        public VoltageControlledCurrentsource(CircuitIdentifier name, CircuitIdentifier pos, CircuitIdentifier neg, CircuitIdentifier cont_pos, CircuitIdentifier cont_neg, double gain) 
            : this(name)
        {
            // Register behaviors
            RegisterBehavior(new LoadBehavior(gain));
            RegisterBehavior(new AcBehavior());

            // Connect
            Connect(pos, neg, cont_pos, cont_neg);
        }

        /// <summary>
        /// Setup the voltage-controlled current source
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Setup(Circuit ckt)
        {
            var nodes = BindNodes(ckt);
            VCCSposNode = nodes[0].Index;
            VCCSnegNode = nodes[1].Index;
            VCCScontPosNode = nodes[2].Index;
            VCCScontNegNode = nodes[3].Index;
        }
    }
}
