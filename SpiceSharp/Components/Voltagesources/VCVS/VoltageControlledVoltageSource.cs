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
        [ParameterName("pos_node"), PropertyInfo("Positive node of the source")]
        public int PosNode { get; internal set; }
        [ParameterName("neg_node"), PropertyInfo("Negative node of the source")]
        public int NegNode { get; internal set; }
        [ParameterName("cont_p_node"), PropertyInfo("Positive controlling node of the source")]
        public int ControlPosNode { get; internal set; }
        [ParameterName("cont_n_node"), PropertyInfo("Negative controlling node of the source")]
        public int ControlNegNode { get; internal set; }

        /// <summary>
        /// Constants
        /// </summary>
        [ParameterName("pincount"), PropertyInfo("Number of pins")]
		public const int VoltageControlledVoltageSourcePinCount = 4;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the voltage-controlled voltage source</param>
        public VoltageControlledVoltageSource(Identifier name) 
            : base(name, VoltageControlledVoltageSourcePinCount)
        {
            // Add parameters
            ParameterSets.Add(new BaseParameters());

            // Add factories
            Behaviors.Add(typeof(LoadBehavior), () => new LoadBehavior(Name));
            Behaviors.Add(typeof(FrequencyBehavior), () => new FrequencyBehavior(Name));
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the voltage-controlled voltage source</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="controlPos">The positive controlling node</param>
        /// <param name="controlNeg">The negative controlling node</param>
        /// <param name="gain">The voltage gain</param>
        public VoltageControlledVoltageSource(Identifier name, Identifier pos, Identifier neg, Identifier controlPos, Identifier controlNeg, double gain) 
            : base(name, VoltageControlledVoltageSourcePinCount)
        {
            // Add parameters
            ParameterSets.Add(new BaseParameters(gain));

            // Add factories
            Behaviors.Add(typeof(LoadBehavior), () => new LoadBehavior(Name));
            Behaviors.Add(typeof(FrequencyBehavior), () => new FrequencyBehavior(Name));

            Connect(pos, neg, controlPos, controlNeg);
        }

        /// <summary>
        /// Setup the voltage-controlled voltage source
        /// </summary>
        /// <param name="circuit">The circuit</param>
        public override void Setup(Circuit circuit)
        {
            var nodes = BindNodes(circuit);
            PosNode = nodes[0].Index;
            NegNode = nodes[1].Index;
            ControlPosNode = nodes[2].Index;
            ControlNegNode = nodes[3].Index;
        }
    }
}
