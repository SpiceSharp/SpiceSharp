using SpiceSharp.Attributes;
using SpiceSharp.Circuits;
using SpiceSharp.Behaviors.VCVS;
using SpiceSharp.Components.VCVS;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A voltage-controlled current-source
    /// </summary>
    [PinsAttribute("V+", "V-", "VC+", "VC-"), VoltageDriverAttribute(0, 1), ConnectedAttribute(0, 1)]
    public class VoltageControlledVoltagesource : Component
    {
        /// <summary>
        /// Nodes
        /// </summary>
        [PropertyName("pos_node"), PropertyInfo("Positive node of the source")]
        public int VCVSposNode { get; internal set; }
        [PropertyName("neg_node"), PropertyInfo("Negative node of the source")]
        public int VCVSnegNode { get; internal set; }
        [PropertyName("cont_p_node"), PropertyInfo("Positive controlling node of the source")]
        public int VCVScontPosNode { get; internal set; }
        [PropertyName("cont_n_node"), PropertyInfo("Negative controlling node of the source")]
        public int VCVScontNegNode { get; internal set; }

        /// <summary>
        /// Constants
        /// </summary>
        public const int VCVSpinCount = 4;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the voltage-controlled voltage source</param>
        public VoltageControlledVoltagesource(Identifier name) 
            : base(name, VCVSpinCount)
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
        public VoltageControlledVoltagesource(Identifier name, Identifier pos, Identifier neg, Identifier cont_pos, Identifier cont_neg, double gain) 
            : base(name, VCVSpinCount)
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
            var nodes = BindNodes(circuit);
            VCVSposNode = nodes[0].Index;
            VCVSnegNode = nodes[1].Index;
            VCVScontPosNode = nodes[2].Index;
            VCVScontNegNode = nodes[3].Index;
        }
    }
}
