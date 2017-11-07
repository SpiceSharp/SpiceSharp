using SpiceSharp.Circuits;
using SpiceSharp.Parameters;
using SpiceSharp.Diagnostics;
using SpiceSharp.Sparse;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A current-controlled current source
    /// </summary>
    [SpicePins("F+", "F-"), ConnectedPins()]
    public class CurrentControlledCurrentsource : CircuitComponent<CurrentControlledCurrentsource>
    {
        /// <summary>
        /// Register default behaviours
        /// </summary>
        static CurrentControlledCurrentsource()
        {
            Behaviors.Behaviors.RegisterBehavior(typeof(CurrentControlledCurrentsource), typeof(ComponentBehaviors.CurrentControlledCurrentsourceLoadBehavior));
            Behaviors.Behaviors.RegisterBehavior(typeof(CurrentControlledCurrentsource), typeof(ComponentBehaviors.CurrentControlledCurrentsourceAcBehavior));
        }

        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("gain"), SpiceInfo("Gain of the source")]
        public Parameter CCCScoeff { get; } = new Parameter();
        [SpiceName("control"), SpiceInfo("Name of the controlling source")]
        public CircuitIdentifier CCCScontName { get; set; }
        [SpiceName("i"), SpiceInfo("CCCS output current")]
        public double GetCurrent(Circuit ckt) => ckt.State.Solution[CCCScontBranch] * CCCScoeff;
        [SpiceName("v"), SpiceInfo("CCCS voltage at output")]
        public double GetVoltage(Circuit ckt) => ckt.State.Solution[CCCSposNode] - ckt.State.Solution[CCCSnegNode];
        [SpiceName("p"), SpiceInfo("CCCS power")]
        public double GetPower(Circuit ckt) => ckt.State.Solution[CCCScontBranch] * CCCScoeff *
            (ckt.State.Solution[CCCSposNode] - ckt.State.Solution[CCCSnegNode]);

        /// <summary>
        /// Nodes
        /// </summary>
        [SpiceName("pos_node"), SpiceInfo("Positive node of the source")]
        public int CCCSposNode { get; private set; }
        [SpiceName("neg_node"), SpiceInfo("Negative node of the source")]
        public int CCCSnegNode { get; private set; }
        public int CCCScontBranch { get; private set; }

        /// <summary>
        /// Matrix elements
        /// </summary>
        internal MatrixElement CCCSposContBrptr { get; private set; }
        internal MatrixElement CCCSnegContBrptr { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the current controlled current source</param>
        public CurrentControlledCurrentsource(CircuitIdentifier name) : base(name)
        {
            // Make sure the current controlled current source happens after voltage sources
            Priority = -1;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the current controlled current source</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="vsource">The name of the voltage source</param>
        /// <param name="gain">The current gain</param>
        public CurrentControlledCurrentsource(CircuitIdentifier name, CircuitIdentifier pos, CircuitIdentifier neg, CircuitIdentifier vsource, double gain) : base(name)
        {
            Priority = -1;
            Connect(pos, neg);
            CCCScoeff.Set(gain);
            CCCScontName = vsource;
        }

        /// <summary>
        /// Setup the current controlled current source
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Setup(Circuit ckt)
        {
            var nodes = BindNodes(ckt);
            CCCSposNode = nodes[0].Index;
            CCCSnegNode = nodes[1].Index;

            // Find the voltage source for which the current is being measured
            if (ckt.Objects[CCCScontName] is Voltagesource vsrc)
                CCCScontBranch = vsrc.VSRCbranch;
            else
                throw new CircuitException($"{Name}: Could not find voltage source '{CCCScontName}'");

            // Get matrix elements
            var matrix = ckt.State.Matrix;
            CCCSposContBrptr = matrix.GetElement(CCCSposNode, CCCScontBranch);
            CCCSnegContBrptr = matrix.GetElement(CCCSnegNode, CCCScontBranch);
        }

        /// <summary>
        /// Unsetup the source
        /// </summary>
        /// <param name="ckt"></param>
        public override void Unsetup(Circuit ckt)
        {
            // Remove references
            CCCSposContBrptr = null;
            CCCSnegContBrptr = null;
        }
    }
}
