using SpiceSharp.Parameters;
using SpiceSharp.Circuits;
using SpiceSharp.Sparse;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A voltage-controlled current-source
    /// </summary>
    [SpicePins("V+", "V-", "VC+", "VC-"), VoltageDriver(0, 1), ConnectedPins(0, 1)]
    public class VoltageControlledVoltagesource : CircuitComponent
    {
        /// <summary>
        /// Register our default behaviors
        /// </summary>
        static VoltageControlledVoltagesource()
        {
            Behaviors.Behaviors.RegisterBehavior(typeof(VoltageControlledVoltagesource), typeof(ComponentBehaviors.VoltageControlledVoltagesourceLoadBehavior));
            Behaviors.Behaviors.RegisterBehavior(typeof(VoltageControlledVoltagesource), typeof(ComponentBehaviors.VoltageControlledVoltagesourceAcBehavior));
        }

        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("gain"), SpiceInfo("Voltage gain")]
        public Parameter VCVScoeff { get; } = new Parameter();
        [SpiceName("i"), SpiceInfo("Output current")]
        public double GetCurrent(Circuit ckt) => ckt.State.Solution[VCVSbranch];
        [SpiceName("v"), SpiceInfo("Output current")]
        public double GetVoltage(Circuit ckt) => ckt.State.Solution[VCVSposNode] - ckt.State.Solution[VCVSnegNode];
        [SpiceName("p"), SpiceInfo("Power")]
        public double GetPower(Circuit ckt) => ckt.State.Solution[VCVSbranch] * (ckt.State.Solution[VCVSposNode] - ckt.State.Solution[VCVSnegNode]);

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
        /// Matrix elements
        /// </summary>
        internal MatrixElement VCVSposIbrptr { get; private set; }
        internal MatrixElement VCVSnegIbrptr { get; private set; }
        internal MatrixElement VCVSibrPosptr { get; private set; }
        internal MatrixElement VCVSibrNegptr { get; private set; }
        internal MatrixElement VCVSibrContPosptr { get; private set; }
        internal MatrixElement VCVSibrContNegptr { get; private set; }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the voltage-controlled voltage source</param>
        public VoltageControlledVoltagesource(CircuitIdentifier name) : base(name, 4) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the voltage-controlled voltage source</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="cont_pos">The positive controlling node</param>
        /// <param name="cont_neg">The negative controlling node</param>
        /// <param name="gain">The voltage gain</param>
        public VoltageControlledVoltagesource(CircuitIdentifier name, CircuitIdentifier pos, CircuitIdentifier neg, CircuitIdentifier cont_pos, CircuitIdentifier cont_neg, double gain) : base(name, 4)
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

            // Get matrix elements
            var matrix = ckt.State.Matrix;
            VCVSposIbrptr = matrix.GetElement(VCVSposNode, VCVSbranch);
            VCVSnegIbrptr = matrix.GetElement(VCVSnegNode, VCVSbranch);
            VCVSibrPosptr = matrix.GetElement(VCVSbranch, VCVSposNode);
            VCVSibrNegptr = matrix.GetElement(VCVSbranch, VCVSnegNode);
            VCVSibrContPosptr = matrix.GetElement(VCVSbranch, VCVScontPosNode);
            VCVSibrContNegptr = matrix.GetElement(VCVSbranch, VCVScontNegNode);
        }

        /// <summary>
        /// Unsetup
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Unsetup(Circuit ckt)
        {
            // Remove references
            VCVSposIbrptr = null;
            VCVSnegIbrptr = null;
            VCVSibrPosptr = null;
            VCVSibrNegptr = null;
            VCVSibrContPosptr = null;
            VCVSibrContNegptr = null;
        }
    }
}
