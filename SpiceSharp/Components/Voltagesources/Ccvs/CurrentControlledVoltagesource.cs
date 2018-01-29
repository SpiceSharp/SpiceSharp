using SpiceSharp.Attributes;
using SpiceSharp.Diagnostics;
using SpiceSharp.Components.CurrentControlledVoltagesourceBehaviors;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A current-controlled voltage source
    /// </summary>
    [Pin(0, "H+"), Pin(1, "H-"), VoltageDriver(0, 1)]
    public class CurrentControlledVoltageSource : Component
    {
        /// <summary>
        /// Nodes
        /// </summary>
        [PropertyName("pos_node"), PropertyInfo("Positive node of the source")]
        public int PosourceNode { get; internal set; }
        [PropertyName("neg_node"), PropertyInfo("Negative node of the source")]
        public int NegateNode { get; internal set; }
        [PropertyName("control"), PropertyInfo("Controlling voltage source")]
        public Identifier ControllingName { get; set; }

        /// <summary>
        /// Get the controlling voltage source
        /// </summary>
        public VoltageSource ControllingSource { get; protected set; }

        /// <summary>
        /// Constants
        /// </summary>
        public const int CurrentControlledVoltageSourcePinCount = 2;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the current-controlled current source</param>
        public CurrentControlledVoltageSource(Identifier name) 
            : base(name, CurrentControlledVoltageSourcePinCount)
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
        public CurrentControlledVoltageSource(Identifier name, Identifier pos, Identifier neg, Identifier vsource, double gain) 
            : base(name, CurrentControlledVoltageSourcePinCount)
        {
            // Add parameters
            Parameters.Add(new BaseParameters(gain));

            // Add factories
            AddFactory(typeof(LoadBehavior), () => new LoadBehavior(Name));
            AddFactory(typeof(FrequencyBehavior), () => new FrequencyBehavior(Name));

            Connect(pos, neg);
            ControllingName = vsource;
        }

        /// <summary>
        /// Setup the current-controlled voltage source
        /// </summary>
        /// <param name="circuit">The circuit</param>
        public override void Setup(Circuit circuit)
        {
            var nodes = BindNodes(circuit);
            PosourceNode = nodes[0].Index;
            NegateNode = nodes[1].Index;

            // Find the voltage source
            if (circuit.Objects[ControllingName] is VoltageSource vsrc)
                ControllingSource = vsrc;
            else
                throw new CircuitException("{0}: Could not find voltage source '{1}'".FormatString(Name, ControllingName));
        }
    }
}
