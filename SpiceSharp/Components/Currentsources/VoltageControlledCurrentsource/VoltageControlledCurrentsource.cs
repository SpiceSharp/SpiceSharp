using SpiceSharp.Circuits;
using SpiceSharp.Parameters;
using SpiceSharp.Sparse;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A voltage-controlled current source
    /// </summary>
    [SpicePins("V+", "V-", "VC+", "VC-"), ConnectedPins(0, 1)]
    public class VoltageControlledCurrentsource : CircuitComponent<VoltageControlledCurrentsource>
    {
        /// <summary>
        /// Register default behaviours
        /// </summary>
        static VoltageControlledCurrentsource()
        {
            Behaviors.Behaviors.RegisterBehavior(typeof(VoltageControlledCurrentsource), typeof(ComponentBehaviors.VoltageControlledCurrentsourceLoadBehavior));
            Behaviors.Behaviors.RegisterBehavior(typeof(VoltageControlledCurrentsource), typeof(ComponentBehaviors.VoltageControlledCurrentsourceAcBehavior));
        }

        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("gain"), SpiceInfo("Transconductance of the source (gain)")]
        public Parameter VCCScoeff { get; } = new Parameter();
        [SpiceName("i"), SpiceInfo("Output current")]
        public double GetCurrent(Circuit ckt) => (ckt.State.Solution[VCCScontPosNode] - ckt.State.Solution[VCCScontNegNode]) * VCCScoeff;
        [SpiceName("v"), SpiceInfo("Voltage across output")]
        public double GetVoltage(Circuit ckt) => ckt.State.Solution[VCCSposNode] - ckt.State.Solution[VCCSnegNode];
        [SpiceName("p"), SpiceInfo("Power")]
        public double GetPower(Circuit ckt) => (ckt.State.Solution[VCCScontPosNode] - ckt.State.Solution[VCCScontNegNode]) * VCCScoeff *
            (ckt.State.Solution[VCCSposNode] - ckt.State.Solution[VCCSnegNode]);

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
        /// Matrix elements
        /// </summary>
        internal MatrixElement VCCSposContPosptr { get; private set; }
        internal MatrixElement VCCSposContNegptr { get; private set; }
        internal MatrixElement VCCSnegContPosptr { get; private set; }
        internal MatrixElement VCCSnegContNegptr { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the voltage-controlled current source</param>
        public VoltageControlledCurrentsource(CircuitIdentifier name) : base(name)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the voltage-controlled current source</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="cont_pos">The positive controlling node</param>
        /// <param name="cont_neg">The negative controlling node</param>
        /// <param name="coeff">The transconductance gain</param>
        public VoltageControlledCurrentsource(CircuitIdentifier name, CircuitIdentifier pos, CircuitIdentifier neg, CircuitIdentifier cont_pos, CircuitIdentifier cont_neg, double gain) 
            : base(name)
        {
            Connect(pos, neg, cont_pos, cont_neg);
            VCCScoeff.Set(gain);
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

            // Get matrix elements
            var matrix = ckt.State.Matrix;
            VCCSposContPosptr = matrix.GetElement(VCCSposNode, VCCScontPosNode);
            VCCSposContNegptr = matrix.GetElement(VCCSposNode, VCCScontNegNode);
            VCCSnegContPosptr = matrix.GetElement(VCCSnegNode, VCCScontPosNode);
            VCCSnegContNegptr = matrix.GetElement(VCCSnegNode, VCCScontNegNode);
        }

        /// <summary>
        /// Unsetup
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Unsetup(Circuit ckt)
        {
            // Remove references
            VCCSposContPosptr = null;
            VCCSposContNegptr = null;
            VCCSnegContPosptr = null;
            VCCSnegContNegptr = null;
        }
    }
}
