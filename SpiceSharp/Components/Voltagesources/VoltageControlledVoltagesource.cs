using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpiceSharp.Parameters;
using SpiceSharp.Circuits;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A class that describes a voltage-controlled current-source
    /// </summary>
    public class VoltageControlledVoltagesource : CircuitComponent
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("gain"), SpiceInfo("Voltage gain")]
        public Parameter<double> VCVScoeff { get; } = new Parameter<double>();
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
        public int VCVSposNode { get; private set; }
        [SpiceName("neg_node"), SpiceInfo("Negative node of the source")]
        public int VCVSnegNode { get; private set; }
        [SpiceName("cont_p_node"), SpiceInfo("Positive controlling node of the source")]
        public int VCVScontPosNode { get; private set; }
        [SpiceName("cont_n_node"), SpiceInfo("Negative controlling node of the source")]
        public int VCVScontNegNode { get; private set; }
        public int VCVSbranch { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the voltage-controlled voltage source</param>
        public VoltageControlledVoltagesource(string name) : base(name, 4) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the voltage-controlled voltage source</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="cont_pos">The positive controlling node</param>
        /// <param name="cont_neg">The negative controlling node</param>
        /// <param name="coeff">The voltage gain</param>
        public VoltageControlledVoltagesource(string name, string pos, string neg, string cont_pos, string cont_neg, double coeff) : base(name, 4)
        {
            Connect(pos, neg, cont_pos, cont_neg);
            VCVScoeff.Set(coeff);
        }

        /// <summary>
        /// No model
        /// </summary>
        /// <returns>null</returns>
        public override CircuitModel GetModel() => null;

        /// <summary>
        /// Setup the voltage-controlled voltage source
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Setup(Circuit ckt)
        {
            var nodes = BindNodes(ckt, CircuitNode.NodeType.Current);
            VCVSposNode = nodes[0].Index;
            VCVSnegNode = nodes[1].Index;
            VCVScontPosNode = nodes[2].Index;
            VCVScontNegNode = nodes[3].Index;
            VCVSbranch = nodes[4].Index;
        }

        /// <summary>
        /// Temperature-dependent calculations
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Temperature(Circuit ckt)
        {
        }

        /// <summary>
        /// Load the voltage-controlled voltage source
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Load(Circuit ckt)
        {
            var rstate = ckt.State.Real;
            rstate.Matrix[VCVSposNode, VCVSbranch] += 1.0;
            rstate.Matrix[VCVSbranch, VCVSposNode] += 1.0;
            rstate.Matrix[VCVSnegNode, VCVSbranch] -= 1.0;
            rstate.Matrix[VCVSbranch, VCVSnegNode] -= 1.0;
            rstate.Matrix[VCVSbranch, VCVScontPosNode] -= VCVScoeff;
            rstate.Matrix[VCVSbranch, VCVScontNegNode] += VCVScoeff;
        }

        /// <summary>
        /// Load the voltage-controlled voltage source for AC analysis
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void AcLoad(Circuit ckt)
        {
            var cstate = ckt.State.Complex;
            cstate.Matrix[VCVSposNode, VCVSbranch] += 1.0;
            cstate.Matrix[VCVSbranch, VCVSposNode] += 1.0;
            cstate.Matrix[VCVSnegNode, VCVSbranch] -= 1.0;
            cstate.Matrix[VCVSbranch, VCVSnegNode] -= 1.0;
            cstate.Matrix[VCVSbranch, VCVScontPosNode] -= VCVScoeff.Value;
            cstate.Matrix[VCVSbranch, VCVScontNegNode] += VCVScoeff.Value;
        }
    }
}
