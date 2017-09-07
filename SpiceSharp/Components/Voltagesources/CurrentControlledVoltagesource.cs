using SpiceSharp.Circuits;
using SpiceSharp.Parameters;
using SpiceSharp.Diagnostics;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A current-controlled voltage source
    /// </summary>
    [SpicePins("H+", "H-"), VoltageDriver(0, 1)]
    public class CurrentControlledVoltagesource : CircuitComponent<CurrentControlledVoltagesource>
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("gain"), SpiceInfo("Transresistance (gain)")]
        public Parameter CCVScoeff { get; } = new Parameter();
        [SpiceName("control"), SpiceInfo("Controlling voltage source")]
        public string CCVScontName { get; set; }
        [SpiceName("i"), SpiceInfo("Output current")]
        public double GetCurrent(Circuit ckt) => ckt.State.Real.Solution[CCVSbranch];
        [SpiceName("v"), SpiceInfo("Output voltage")]
        public double GetVoltage(Circuit ckt) => ckt.State.Real.Solution[CCVSposNode] - ckt.State.Real.Solution[CCVSnegNode];
        [SpiceName("p"), SpiceInfo("Power")]
        public double GetPower(Circuit ckt) => ckt.State.Real.Solution[CCVSbranch] * (ckt.State.Real.Solution[CCVSposNode] - ckt.State.Real.Solution[CCVSnegNode]);

        /// <summary>
        /// Nodes
        /// </summary>
        [SpiceName("pos_node"), SpiceInfo("Positive node of the source")]
        public int CCVSposNode { get; private set; }
        [SpiceName("neg_node"), SpiceInfo("Negative node of the source")]
        public int CCVSnegNode { get; private set; }
        public int CCVSbranch { get; private set; }
        public int CCVScontBranch { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the current-controlled current source</param>
        public CurrentControlledVoltagesource(string name) : base(name) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the current-controlled current source</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="vsource">The controlling voltage source name</param>
        /// <param name="gain">The transresistance (gain)</param>
        public CurrentControlledVoltagesource(string name, string pos, string neg, string vsource, double gain) : base(name)
        {
            Connect(pos, neg);
            CCVScoeff.Set(gain);
            CCVScontName = vsource;
        }

        /// <summary>
        /// Setup the current-controlled voltage source
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Setup(Circuit ckt)
        {
            var nodes = BindNodes(ckt);
            CCVSposNode = nodes[0].Index;
            CCVSnegNode = nodes[1].Index;
            CCVSbranch = CreateNode(ckt, CircuitNode.NodeType.Current).Index;

            // Find the voltage source
            var vsource = ckt.Objects[CCVScontName];
            if (vsource is Voltagesource)
                CCVScontBranch = ((Voltagesource)vsource).VSRCbranch;
            else
                throw new CircuitException($"{Name}: Could not find voltage source '{CCVScontName}'");
        }

        /// <summary>
        /// Temperature-dependent calculations
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Temperature(Circuit ckt)
        {
        }

        /// <summary>
        /// Load the current-controlled voltage source
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Load(Circuit ckt)
        {
            var rstate = ckt.State.Real;
            rstate.Matrix[CCVSposNode, CCVSbranch] += 1.0;
            rstate.Matrix[CCVSbranch, CCVSposNode] += 1.0;
            rstate.Matrix[CCVSnegNode, CCVSbranch] -= 1.0;
            rstate.Matrix[CCVSbranch, CCVSnegNode] -= 1.0;
            rstate.Matrix[CCVSbranch, CCVScontBranch] -= CCVScoeff;
        }

        /// <summary>
        /// Load the current-controlled voltage source for AC analysis
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void AcLoad(Circuit ckt)
        {
            var cstate = ckt.State.Complex;
            cstate.Matrix[CCVSposNode, CCVSbranch] += 1.0;
            cstate.Matrix[CCVSbranch, CCVSposNode] += 1.0;
            cstate.Matrix[CCVSnegNode, CCVSbranch] -= 1.0;
            cstate.Matrix[CCVSbranch, CCVSnegNode] -= 1.0;
            cstate.Matrix[CCVSbranch, CCVScontBranch] -= CCVScoeff.Value;
        }
    }
}
