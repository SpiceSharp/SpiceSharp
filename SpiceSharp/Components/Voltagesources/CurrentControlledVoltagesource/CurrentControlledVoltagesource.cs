using SpiceSharp.Circuits;
using SpiceSharp.Parameters;
using SpiceSharp.Diagnostics;
using SpiceSharp.Sparse;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A current-controlled voltage source
    /// </summary>
    [SpicePins("H+", "H-"), VoltageDriver(0, 1)]
    public class CurrentControlledVoltagesource : CircuitComponent<CurrentControlledVoltagesource>
    {
        /// <summary>
        /// Register default behaviors
        /// </summary>
        static CurrentControlledVoltagesource()
        {
            Behaviors.Behaviors.RegisterBehavior(typeof(CurrentControlledVoltagesource), typeof(ComponentBehaviors.CurrentControlledVoltagesourceLoadBehavior));
            Behaviors.Behaviors.RegisterBehavior(typeof(CurrentControlledVoltagesource), typeof(ComponentBehaviors.CurrentControlledVoltagesourceAcBehavior));
        }

        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("gain"), SpiceInfo("Transresistance (gain)")]
        public Parameter CCVScoeff { get; } = new Parameter();
        [SpiceName("control"), SpiceInfo("Controlling voltage source")]
        public CircuitIdentifier CCVScontName { get; set; }
        [SpiceName("i"), SpiceInfo("Output current")]
        public double GetCurrent(Circuit ckt) => ckt.State.Solution[CCVSbranch];
        [SpiceName("v"), SpiceInfo("Output voltage")]
        public double GetVoltage(Circuit ckt) => ckt.State.Solution[CCVSposNode] - ckt.State.Solution[CCVSnegNode];
        [SpiceName("p"), SpiceInfo("Power")]
        public double GetPower(Circuit ckt) => ckt.State.Solution[CCVSbranch] * (ckt.State.Solution[CCVSposNode] - ckt.State.Solution[CCVSnegNode]);

        /// <summary>
        /// Nodes
        /// </summary>
        [SpiceName("pos_node"), SpiceInfo("Positive node of the source")]
        public int CCVSposNode { get; internal set; }
        [SpiceName("neg_node"), SpiceInfo("Negative node of the source")]
        public int CCVSnegNode { get; internal set; }
        public int CCVSbranch { get; internal set; }
        public int CCVScontBranch { get; internal set; }

        /// <summary>
        /// Matrix elements
        /// </summary>
        internal MatrixElement CCVSposIbrptr { get; private set; }
        internal MatrixElement CCVSnegIbrptr { get; private set; }
        internal MatrixElement CCVSibrPosptr { get; private set; }
        internal MatrixElement CCVSibrNegptr { get; private set; }
        internal MatrixElement CCVSibrContBrptr { get; private set; }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the current-controlled current source</param>
        public CurrentControlledVoltagesource(CircuitIdentifier name) : base(name) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the current-controlled current source</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="vsource">The controlling voltage source name</param>
        /// <param name="gain">The transresistance (gain)</param>
        public CurrentControlledVoltagesource(CircuitIdentifier name, CircuitIdentifier pos, CircuitIdentifier neg, CircuitIdentifier vsource, double gain) : base(name)
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
            CCVSbranch = CreateNode(ckt, Name.Grow("#branch"), CircuitNode.NodeType.Current).Index;

            // Find the voltage source
            if (ckt.Objects[CCVScontName] is Voltagesource vsrc)
                CCVScontBranch = vsrc.VSRCbranch;
            else
                throw new CircuitException($"{Name}: Could not find voltage source '{CCVScontName}'");

            // Get matrix elements
            var matrix = ckt.State.Matrix;
            CCVSposIbrptr = matrix.GetElement(CCVSposNode, CCVSbranch);
            CCVSnegIbrptr = matrix.GetElement(CCVSnegNode, CCVSbranch);
            CCVSibrPosptr = matrix.GetElement(CCVSbranch, CCVSposNode);
            CCVSibrNegptr = matrix.GetElement(CCVSbranch, CCVSnegNode);
            CCVSibrContBrptr = matrix.GetElement(CCVSbranch, CCVScontBranch);
        }

        /// <summary>
        /// Unsetup
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Unsetup(Circuit ckt)
        {
            // Remove references
            CCVSposIbrptr = null;
            CCVSnegIbrptr = null;
            CCVSibrPosptr = null;
            CCVSibrNegptr = null;
            CCVSibrContBrptr = null;
        }
    }
}
