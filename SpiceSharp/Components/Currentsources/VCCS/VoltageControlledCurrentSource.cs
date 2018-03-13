using SpiceSharp.Attributes;
using SpiceSharp.Components.VoltageControlledCurrentsourceBehaviors;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A voltage-controlled current source
    /// </summary>
    [Pin(0, "V+"), Pin(1, "V-"), Pin(2, "VC+"), Pin(3, "VC-"), Connected(0, 1)]
    public class VoltageControlledCurrentSource : Component
    {
        /// <summary>
        /// Nodes
        /// </summary>
        [ParameterName("pos_node"), PropertyInfo("Positive node of the source")]
        public int PosNode { get; private set; }
        [ParameterName("neg_node"), PropertyInfo("Negative node of the source")]
        public int NegNode { get; private set; }
        [ParameterName("cont_p_node"), PropertyInfo("Positive node of the controlling source voltage")]
        public int ControlPosNode { get; private set; }
        [ParameterName("cont_n_node"), PropertyInfo("Negative node of the controlling source voltage")]
        public int ControlNegNode { get; private set; }

        /// <summary>
        /// Private constants
        /// </summary>
        [ParameterName("pincount"), PropertyInfo("Number of pins")]
		public const int VoltageControlledCurrentSourcePinCount = 4;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the voltage-controlled current source</param>
        public VoltageControlledCurrentSource(Identifier name)
            : base(name, VoltageControlledCurrentSourcePinCount)
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
        /// <param name="name">The name of the voltage-controlled current source</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="controlPos">The positive controlling node</param>
        /// <param name="controlNeg">The negative controlling node</param>
        /// <param name="gain">The transconductance gain</param>
        public VoltageControlledCurrentSource(Identifier name, Identifier pos, Identifier neg, Identifier controlPos, Identifier controlNeg, double gain)
            : base(name, VoltageControlledCurrentSourcePinCount)
        {
            // Add parameters
            ParameterSets.Add(new BaseParameters(gain));

            // Add factories
            Behaviors.Add(typeof(LoadBehavior), () => new LoadBehavior(Name));
            Behaviors.Add(typeof(FrequencyBehavior), () => new FrequencyBehavior(Name));

            // Connect
            Connect(pos, neg, controlPos, controlNeg);
        }

        /// <summary>
        /// Setup the voltage-controlled current source
        /// </summary>
        /// <param name="circuit">Circuit</param>
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
