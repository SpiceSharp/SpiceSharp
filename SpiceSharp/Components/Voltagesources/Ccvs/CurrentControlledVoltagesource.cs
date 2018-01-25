using SpiceSharp.Circuits;
using SpiceSharp.Attributes;
using SpiceSharp.Diagnostics;
using SpiceSharp.Behaviors.CCVS;
using SpiceSharp.Components.CCVS;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A current-controlled voltage source
    /// </summary>
    [PinsAttribute("H+", "H-"), VoltageDriverAttribute(0, 1)]
    public class CurrentControlledVoltagesource : Component
    {
        /// <summary>
        /// Nodes
        /// </summary>
        [PropertyNameAttribute("pos_node"), PropertyInfoAttribute("Positive node of the source")]
        public int CCVSposNode { get; internal set; }
        [PropertyNameAttribute("neg_node"), PropertyInfoAttribute("Negative node of the source")]
        public int CCVSnegNode { get; internal set; }
        [PropertyNameAttribute("control"), PropertyInfoAttribute("Controlling voltage source")]
        public Identifier CCVScontName { get; set; }

        /// <summary>
        /// Get the controlling voltage source
        /// </summary>
        public Voltagesource CCVScontSource { get; protected set; }

        /// <summary>
        /// Constants
        /// </summary>
        public const int CCVSpinCount = 2;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the current-controlled current source</param>
        public CurrentControlledVoltagesource(Identifier name) 
            : base(name, CCVSpinCount)
        {
            // Add parameters
            Parameters.Add(new BaseParameters());

            // Add factories
            AddFactory(typeof(LoadBehavior), () => new LoadBehavior(Name));
            AddFactory(typeof(AcBehavior), () => new AcBehavior(Name));
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the current-controlled current source</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="vsource">The controlling voltage source name</param>
        /// <param name="gain">The transresistance (gain)</param>
        public CurrentControlledVoltagesource(Identifier name, Identifier pos, Identifier neg, Identifier vsource, double gain) 
            : base(name, CCVSpinCount)
        {
            // Add parameters
            Parameters.Add(new BaseParameters(gain));

            // Add factories
            AddFactory(typeof(LoadBehavior), () => new LoadBehavior(Name));
            AddFactory(typeof(AcBehavior), () => new AcBehavior(Name));

            Connect(pos, neg);
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

            // Find the voltage source
            if (ckt.Objects[CCVScontName] is Voltagesource vsrc)
                CCVScontSource = vsrc;
            else
                throw new CircuitException($"{Name}: Could not find voltage source '{CCVScontName}'");
        }
    }
}
