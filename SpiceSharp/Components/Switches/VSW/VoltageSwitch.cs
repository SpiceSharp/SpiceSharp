using SpiceSharp.Attributes;
using SpiceSharp.Components.VoltageSwitchBehaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A voltage-controlled switch
    /// </summary>
    [Pin(0, "S+"), Pin(1, "S-"), Pin(2, "SC+"), Pin(3, "SC-"), Connected(0, 1)]
    public class VoltageSwitch : Component
    {
        /// <summary>
        /// Set the model for the voltage-controlled switch
        /// </summary>
        public void SetModel(VoltageSwitchModel model) => Model = model;

        /// <summary>
        /// Nodes
        /// </summary>
        [ParameterName("pos_node"), ParameterInfo("Positive node of the switch")]
        public int PosNode { get; internal set; }
        [ParameterName("neg_node"), ParameterInfo("Negative node of the switch")]
        public int NegNode { get; internal set; }
        [ParameterName("cont_p_node"), ParameterInfo("Positive controlling node of the switch")]
        public int ControlPosNode { get; internal set; }
        [ParameterName("cont_n_node"), ParameterInfo("Negative controlling node of the switch")]
        public int ControlNegNode { get; internal set; }

        /// <summary>
        /// Constants
        /// </summary>
        [ParameterName("pincount"), ParameterInfo("Number of pins")]
		public const int VoltageSwitchPinCount = 4;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the voltage-controlled switch</param>
        public VoltageSwitch(Identifier name) 
            : base(name, VoltageSwitchPinCount)
        {
            // Register parameters
            ParameterSets.Add(new BaseParameters());

            // Add factories
            Behaviors.Add(typeof(LoadBehavior), () => new LoadBehavior(Name));
            Behaviors.Add(typeof(FrequencyBehavior), () => new FrequencyBehavior(Name));
            Behaviors.Add(typeof(AcceptBehavior), () => new AcceptBehavior(Name));
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the voltage-controlled switch</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="controlPos">The positive controlling node</param>
        /// <param name="controlNeg">The negative controlling node</param>
        public VoltageSwitch(Identifier name, Identifier pos, Identifier neg, Identifier controlPos, Identifier controlNeg) 
            : base(name, VoltageSwitchPinCount)
        {
            // Register parameters
            ParameterSets.Add(new BaseParameters());

            // Add factories
            Behaviors.Add(typeof(LoadBehavior), () => new LoadBehavior(Name));
            Behaviors.Add(typeof(FrequencyBehavior), () => new FrequencyBehavior(Name));
            Behaviors.Add(typeof(AcceptBehavior), () => new AcceptBehavior(Name));

            Connect(pos, neg, controlPos, controlNeg);
        }

        /// <summary>
        /// Setup the voltage-controlled switch
        /// </summary>
        /// <param name="simulation">Simulation</param>
        public override void Setup(Simulation simulation)
        {
            var nodes = BindNodes(simulation);
            PosNode = nodes[0].Index;
            NegNode = nodes[1].Index;
            ControlPosNode = nodes[2].Index;
            ControlNegNode = nodes[3].Index;
        }
    }
}
