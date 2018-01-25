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
        [PropertyName("pos_node"), PropertyInfo("Positive node of the source")]
        public int CCVSposNode { get; internal set; }
        [PropertyName("neg_node"), PropertyInfo("Negative node of the source")]
        public int CCVSnegNode { get; internal set; }
        [PropertyName("control"), PropertyInfo("Controlling voltage source")]
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
            AddFactory(typeof(FrequencyBehavior), () => new FrequencyBehavior(Name));
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
            AddFactory(typeof(FrequencyBehavior), () => new FrequencyBehavior(Name));

            Connect(pos, neg);
            CCVScontName = vsource;
        }

        /// <summary>
        /// Setup the current-controlled voltage source
        /// </summary>
        /// <param name="circuit">The circuit</param>
        public override void Setup(Circuit circuit)
        {
            var nodes = BindNodes(circuit);
            CCVSposNode = nodes[0].Index;
            CCVSnegNode = nodes[1].Index;

            // Find the voltage source
            if (circuit.Objects[CCVScontName] is Voltagesource vsrc)
                CCVScontSource = vsrc;
            else
                throw new CircuitException($"{Name}: Could not find voltage source '{CCVScontName}'");
        }
    }
}
