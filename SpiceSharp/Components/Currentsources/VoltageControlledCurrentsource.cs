using SpiceSharp.Parameters;

namespace SpiceSharp.Components
{
    /// <summary>
    /// This class describes a voltage-controlled current source
    /// </summary>
    [SpicePins("V+", "V-", "VC+", "VC-"), ConnectedPins(0, 1)]
    public class VoltageControlledCurrentsource : CircuitComponent<VoltageControlledCurrentsource>
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("gain"), SpiceInfo("Transconductance of the source (gain)")]
        public Parameter VCCScoeff { get; } = new Parameter();
        [SpiceName("i"), SpiceInfo("Output current")]
        public double GetCurrent(Circuit ckt) => (ckt.State.Real.Solution[VCCScontPosNode] - ckt.State.Real.Solution[VCCScontNegNode]) * VCCScoeff;
        [SpiceName("v"), SpiceInfo("Voltage across output")]
        public double GetVoltage(Circuit ckt) => ckt.State.Real.Solution[VCCSposNode] - ckt.State.Real.Solution[VCCSnegNode];
        [SpiceName("p"), SpiceInfo("Power")]
        public double GetPower(Circuit ckt) => (ckt.State.Real.Solution[VCCScontPosNode] - ckt.State.Real.Solution[VCCScontNegNode]) * VCCScoeff *
            (ckt.State.Real.Solution[VCCSposNode] - ckt.State.Real.Solution[VCCSnegNode]);

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
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the voltage-controlled current source</param>
        public VoltageControlledCurrentsource(string name) : base(name)
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
        public VoltageControlledCurrentsource(string name, string pos, string neg, string cont_pos, string cont_neg, double gain) 
            : base(name)
        {
            Connect(pos, neg, cont_pos, cont_neg);
            VCCScoeff.Set(gain);
        }

        /// <summary>
        /// Setup the voltage-controlled current source
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Setup(Circuit ckt)
        {
            var nodes = BindNodes(ckt);
            VCCSposNode = nodes[0].Index;
            VCCSnegNode = nodes[1].Index;
            VCCScontPosNode = nodes[2].Index;
            VCCScontNegNode = nodes[3].Index;
        }

        /// <summary>
        /// Do temperature-dependent calculations
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Temperature(Circuit ckt)
        {
        }

        /// <summary>
        /// Load the voltage-controlled current source
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Load(Circuit ckt)
        {
            var rstate = ckt.State.Real;
            rstate.Matrix[VCCSposNode, VCCScontPosNode] += VCCScoeff;
            rstate.Matrix[VCCSposNode, VCCScontNegNode] -= VCCScoeff;
            rstate.Matrix[VCCSnegNode, VCCScontPosNode] -= VCCScoeff;
            rstate.Matrix[VCCSnegNode, VCCScontNegNode] += VCCScoeff;
        }

        /// <summary>
        /// Load the voltage-controlled current source for AC analysis
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void AcLoad(Circuit ckt)
        {
            var cstate = ckt.State.Complex;
            cstate.Matrix[VCCSposNode, VCCScontPosNode] += VCCScoeff.Value;
            cstate.Matrix[VCCSposNode, VCCScontNegNode] -= VCCScoeff.Value;
            cstate.Matrix[VCCSnegNode, VCCScontPosNode] -= VCCScoeff.Value;
            cstate.Matrix[VCCSnegNode, VCCScontNegNode] += VCCScoeff.Value;
        }
    }
}
