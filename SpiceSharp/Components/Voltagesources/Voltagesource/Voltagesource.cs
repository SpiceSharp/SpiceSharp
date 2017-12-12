using System.Numerics;
using SpiceSharp.Parameters;
using SpiceSharp.Circuits;
using SpiceSharp.Diagnostics;
using SpiceSharp.Components.ComponentBehaviors;
using SpiceSharp.Sparse;

namespace SpiceSharp.Components
{
    /// <summary>
    /// An independent voltage source
    /// </summary>
    [SpicePins("V+", "V-"), VoltageDriver(0, 1), IndependentSource]
    public class Voltagesource : CircuitComponent
    {
        /// <summary>
        /// Nodes
        /// </summary>
        [SpiceName("pos_node")]
        public int VSRCposNode { get; private set; }
        [SpiceName("neg_node")]
        public int VSRCnegNode { get; private set; }

        /// <summary>
        /// Constants
        /// </summary>
        public const int VSRCpinCount = 2;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name</param>
        public Voltagesource(CircuitIdentifier name) : base(name, VSRCpinCount)
        {
            RegisterBehavior(new VoltagesourceLoadBehavior());
            RegisterBehavior(new VoltageSourceLoadAcBehavior());
            RegisterBehavior(new VoltagesourceAcceptBehavior());
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the voltage source</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="dc">The DC value</param>
        public Voltagesource(CircuitIdentifier name, CircuitIdentifier pos, CircuitIdentifier neg, double dc)
            : this(name)
        {
            Connect(pos, neg);
            Set("dc", dc);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the voltage source</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="w">The waveform</param>
        public Voltagesource(CircuitIdentifier name, CircuitIdentifier pos, CircuitIdentifier neg, Waveform w) 
            : base(name, VSRCpinCount)
        {
            Connect(pos, neg);
            var load = new VoltagesourceLoadBehavior();
            load.VSRCwaveform = w;
            RegisterBehavior(load);
            RegisterBehavior(new VoltageSourceLoadAcBehavior());
            RegisterBehavior(new VoltagesourceAcceptBehavior());
        }

        /// <summary>
        /// Setup the voltage source
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Setup(Circuit ckt)
        {
            // Bind the nodes
            var nodes = BindNodes(ckt);
            VSRCposNode = nodes[0].Index;
            VSRCnegNode = nodes[1].Index;
        }
    }
}
