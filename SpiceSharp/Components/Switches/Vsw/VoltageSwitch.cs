using SpiceSharp.Circuits;
using SpiceSharp.Behaviors.VSW;
using SpiceSharp.Components.VSW;
using SpiceSharp.Attributes;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A voltage-controlled switch
    /// </summary>
    [PinsAttribute("S+", "S-", "SC+", "SC-"), ConnectedAttribute(0, 1)]
    public class VoltageSwitch : Component
    {
        /// <summary>
        /// Set the model for the voltage-controlled switch
        /// </summary>
        public void SetModel(VoltageSwitchModel model) => Model = model;

        /// <summary>
        /// Nodes
        /// </summary>
        [PropertyName("pos_node"), PropertyInfo("Positive node of the switch")]
        public int VSWposNode { get; internal set; }
        [PropertyName("neg_node"), PropertyInfo("Negative node of the switch")]
        public int VSWnegNode { get; internal set; }
        [PropertyName("cont_p_node"), PropertyInfo("Positive controlling node of the switch")]
        public int VSWcontPosNode { get; internal set; }
        [PropertyName("cont_n_node"), PropertyInfo("Negative controlling node of the switch")]
        public int VSWcontNegNode { get; internal set; }

        /// <summary>
        /// Constants
        /// </summary>
        public const int SWpinCount = 4;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the voltage-controlled switch</param>
        public VoltageSwitch(Identifier name) 
            : base(name, SWpinCount)
        {
            // Register parameters
            Parameters.Add(new BaseParameters());

            // Add factories
            AddFactory(typeof(LoadBehavior), () => new LoadBehavior(Name));
            AddFactory(typeof(FrequencyBehavior), () => new FrequencyBehavior(Name));
            AddFactory(typeof(AcceptBehavior), () => new AcceptBehavior(Name));
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the voltage-controlled switch</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="cont_pos">The positive controlling node</param>
        /// <param name="cont_neg">The negative controlling node</param>
        public VoltageSwitch(Identifier name, Identifier pos, Identifier neg, Identifier cont_pos, Identifier cont_neg) 
            : base(name, SWpinCount)
        {
            // Register parameters
            Parameters.Add(new BaseParameters());

            // Add factories
            AddFactory(typeof(LoadBehavior), () => new LoadBehavior(Name));
            AddFactory(typeof(FrequencyBehavior), () => new FrequencyBehavior(Name));
            AddFactory(typeof(AcceptBehavior), () => new AcceptBehavior(Name));

            Connect(pos, neg, cont_pos, cont_neg);
        }

        /// <summary>
        /// Setup the voltage-controlled switch
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Setup(Circuit ckt)
        {
            var nodes = BindNodes(ckt);
            VSWposNode = nodes[0].Index;
            VSWnegNode = nodes[1].Index;
            VSWcontPosNode = nodes[2].Index;
            VSWcontNegNode = nodes[3].Index;
        }
    }
}
