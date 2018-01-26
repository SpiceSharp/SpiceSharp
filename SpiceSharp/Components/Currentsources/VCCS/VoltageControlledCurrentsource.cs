using SpiceSharp.Attributes;
using SpiceSharp.Components.VoltageControlledCurrentsourceBehaviors;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A voltage-controlled current source
    /// </summary>
    [PinsAttribute("V+", "V-", "VC+", "VC-"), ConnectedAttribute(0, 1)]
    public class VoltageControlledCurrentSource : Component
    {
        /// <summary>
        /// Nodes
        /// </summary>
        [PropertyName("pos_node"), PropertyInfo("Positive node of the source")]
        public int PosNode { get; private set; }
        [PropertyName("neg_node"), PropertyInfo("Negative node of the source")]
        public int NegNode { get; private set; }
        [PropertyName("cont_p_node"), PropertyInfo("Positive node of the controlling source voltage")]
        public int ContPosNode { get; private set; }
        [PropertyName("cont_n_node"), PropertyInfo("Negative node of the controlling source voltage")]
        public int ContNegNode { get; private set; }

        /// <summary>
        /// Private constants
        /// </summary>
        public const int VoltageControlledCurrentSourcePinCount = 4;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the voltage-controlled current source</param>
        public VoltageControlledCurrentSource(Identifier name)
            : base(name, VoltageControlledCurrentSourcePinCount)
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
        /// <param name="name">The name of the voltage-controlled current source</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="cont_pos">The positive controlling node</param>
        /// <param name="cont_neg">The negative controlling node</param>
        /// <param name="gain">The transconductance gain</param>
        public VoltageControlledCurrentSource(Identifier name, Identifier pos, Identifier neg, Identifier cont_pos, Identifier cont_neg, double gain)
            : base(name, VoltageControlledCurrentSourcePinCount)
        {
            // Add parameters
            Parameters.Add(new BaseParameters(gain));

            // Add factories
            AddFactory(typeof(LoadBehavior), () => new LoadBehavior(Name));
            AddFactory(typeof(FrequencyBehavior), () => new FrequencyBehavior(Name));

            // Connect
            Connect(pos, neg, cont_pos, cont_neg);
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
            ContPosNode = nodes[2].Index;
            ContNegNode = nodes[3].Index;
        }
    }
}
