using SpiceSharp.Parameters;
using SpiceSharp.Circuits;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A voltage-controlled current-source
    /// </summary>
    [SpicePins("V+", "V-", "VC+", "VC-"), VoltageDriver(0, 1), ConnectedPins(0, 1)]
    public class VoltageControlledVoltagesource : CircuitComponent<VoltageControlledVoltagesource>
    {
        /// <summary>
        /// Register our default behaviours
        /// </summary>
        static VoltageControlledVoltagesource()
        {
            Behaviours.Behaviours.RegisterBehaviour(typeof(VoltageControlledVoltagesource), typeof(ComponentBehaviours.VoltageControlledVoltagesourceLoadBehaviour));
            Behaviours.Behaviours.RegisterBehaviour(typeof(VoltageControlledVoltagesource), typeof(ComponentBehaviours.VoltageControlledVoltagesourceAcBehaviour));
        }

        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("gain"), SpiceInfo("Voltage gain")]
        public Parameter VCVScoeff { get; } = new Parameter();
        [SpiceName("i"), SpiceInfo("Output current")]
        public double GetCurrent(Circuit ckt) => ckt.State.Real.Solution[VCVSbranch];
        [SpiceName("v"), SpiceInfo("Output current")]
        public double GetVoltage(Circuit ckt) => ckt.State.Real.Solution[VCVSposNode] - ckt.State.Real.Solution[VCVSnegNode];
        [SpiceName("p"), SpiceInfo("Power")]
        public double GetPower(Circuit ckt) => ckt.State.Real.Solution[VCVSbranch] * (ckt.State.Real.Solution[VCVSposNode] - ckt.State.Real.Solution[VCVSnegNode]);

        /// <summary>
        /// Nodes
        /// </summary>
        [SpiceName("pos_node"), SpiceInfo("Positive node of the source")]
        public int VCVSposNode { get; internal set; }
        [SpiceName("neg_node"), SpiceInfo("Negative node of the source")]
        public int VCVSnegNode { get; internal set; }
        [SpiceName("cont_p_node"), SpiceInfo("Positive controlling node of the source")]
        public int VCVScontPosNode { get; internal set; }
        [SpiceName("cont_n_node"), SpiceInfo("Negative controlling node of the source")]
        public int VCVScontNegNode { get; internal set; }
        public int VCVSbranch { get; internal set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the voltage-controlled voltage source</param>
        public VoltageControlledVoltagesource(CircuitIdentifier name) : base(name) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the voltage-controlled voltage source</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="cont_pos">The positive controlling node</param>
        /// <param name="cont_neg">The negative controlling node</param>
        /// <param name="gain">The voltage gain</param>
        public VoltageControlledVoltagesource(CircuitIdentifier name, CircuitIdentifier pos, CircuitIdentifier neg, CircuitIdentifier cont_pos, CircuitIdentifier cont_neg, double gain) : base(name)
        {
            Connect(pos, neg, cont_pos, cont_neg);
            VCVScoeff.Set(gain);
        }

        /// <summary>
        /// Setup the voltage-controlled voltage source
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Setup(Circuit ckt)
        {
            var nodes = BindNodes(ckt);
            VCVSposNode = nodes[0].Index;
            VCVSnegNode = nodes[1].Index;
            VCVScontPosNode = nodes[2].Index;
            VCVScontNegNode = nodes[3].Index;
            VCVSbranch = CreateNode(ckt, Name.Grow("#branch"), CircuitNode.NodeType.Current).Index;
        }

        /// <summary>
        /// Temperature-dependent calculations
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Temperature(Circuit ckt)
        {
        }




    }
}
