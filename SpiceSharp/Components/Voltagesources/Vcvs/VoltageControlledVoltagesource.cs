using SpiceSharp.Attributes;
using SpiceSharp.Components.VoltageControlledVoltagesourceBehaviors;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A voltage-controlled current-source
    /// </summary>
    [Pin(0, "V+"), Pin(1, "V-"), Pin(2, "VC+"), Pin(3, "VC-"), VoltageDriver(0, 1), Connected(0, 1)]
    public class VoltageControlledVoltageSource : Component
    {
        /// <summary>
        /// Nodes
        /// </summary>
        [PropertyName("pos_node"), PropertyInfo("Positive node of the source")]
        public int PosourceNode { get; internal set; }
        [PropertyName("neg_node"), PropertyInfo("Negative node of the source")]
        public int NegateNode { get; internal set; }
        [PropertyName("cont_p_node"), PropertyInfo("Positive controlling node of the source")]
        public int ControlPosourceNode { get; internal set; }
        [PropertyName("cont_n_node"), PropertyInfo("Negative controlling node of the source")]
        public int ControlNegateNode { get; internal set; }

        /// <summary>
        /// Constants
        /// </summary>
        public const int VoltageControlledVoltageSourcePinCount = 4;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the voltage-controlled voltage source</param>
        public VoltageControlledVoltageSource(Identifier name) 
            : base(name, VoltageControlledVoltageSourcePinCount)
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
        /// <param name="name">The name of the voltage-controlled voltage source</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="cont_pos">The positive controlling node</param>
        /// <param name="cont_neg">The negative controlling node</param>
        /// <param name="gain">The voltage gain</param>
        public VoltageControlledVoltageSource(Identifier name, Identifier pos, Identifier neg, Identifier cont_pos, Identifier cont_neg, double gain) 
            : base(name, VoltageControlledVoltageSourcePinCount)
        {
            // Add parameters
            Parameters.Add(new BaseParameters(gain));

            // Add factories
            AddFactory(typeof(LoadBehavior), () => new LoadBehavior(Name));
            AddFactory(typeof(FrequencyBehavior), () => new FrequencyBehavior(Name));

            Connect(pos, neg, cont_pos, cont_neg);
        }

        /// <summary>
        /// Setup the voltage-controlled voltage source
        /// </summary>
        /// <param name="circuit">The circuit</param>
        public override void Setup(Circuit circuit)
        {
            var nodes = BindrainNodes(circuit);
            PosourceNode = nodes[0].Index;
            NegateNode = nodes[1].Index;
            ControlPosourceNode = nodes[2].Index;
            ControlNegateNode = nodes[3].Index;
        }
    }
}
