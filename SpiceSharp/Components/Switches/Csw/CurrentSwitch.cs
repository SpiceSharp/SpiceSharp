using SpiceSharp.Attributes;
using SpiceSharp.Components.CurrentSwitchBehaviors;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A current-controlled switch
    /// </summary>
    [Pin(0, "W+"), Pin(1, "W-")]
    public class CurrentSwitch : Component
    {
        /// <summary>
        /// Set the model for the current-controlled switch
        /// </summary>
        public void SetModel(CurrentSwitchModel model) => Model = model;
        
        /// <summary>
        /// Nodes
        /// </summary>
        [PropertyName("pos_node"), PropertyInfo("Positive node of the switch")]
        public int PosNode { get; internal set; }
        [PropertyName("neg_node"), PropertyInfo("Negative node of the switch")]
        public int NegNode { get; internal set; }
        [PropertyName("control"), PropertyInfo("Name of the controlling source")]
        public Identifier ControllingName { get; set; }

        /// <summary>
        /// Get the controlling voltage source
        /// </summary>
        public VoltageSource ControllingSource { get; protected set; }

        /// <summary>
        /// Constants
        /// </summary>
        public const int CurrentSwitchPinCount = 2;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the current-controlled switch</param>
        public CurrentSwitch(Identifier name) : base(name, CurrentSwitchPinCount)
        {
            // Make sure the current switch is processed after voltage sources
            Priority = -1;

            // Add parameters
            ParameterSets.Add(new BaseParameters());

            // Add factories
            Behaviors.Add(typeof(LoadBehavior), () => new LoadBehavior(Name));
            Behaviors.Add(typeof(FrequencyBehavior), () => new FrequencyBehavior(Name));
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the current-controlled switch</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="controllingSource">The controlling voltage source</param>
        public CurrentSwitch(Identifier name, Identifier pos, Identifier neg, Identifier controllingSource)
            : base(name, CurrentSwitchPinCount)
        {
            // Make sure the current switch is processed after voltage sources
            Priority = -1;

            // Add parameters
            ParameterSets.Add(new BaseParameters());

            // Add factories
            Behaviors.Add(typeof(LoadBehavior), () => new LoadBehavior(Name));
            Behaviors.Add(typeof(FrequencyBehavior), () => new FrequencyBehavior(Name));

            Connect(pos, neg);
            ControllingName = controllingSource;
        }

        /// <summary>
        /// Setup the current-controlled switch
        /// </summary>
        /// <param name="circuit">The circuit</param>
        public override void Setup(Circuit circuit)
        {
            var nodes = BindNodes(circuit);
            PosNode = nodes[0].Index;
            NegNode = nodes[1].Index;

            // Find the voltage source
            if (circuit.Objects[ControllingName] is VoltageSource vsrc)
                ControllingSource = vsrc;
        }

        /// <summary>
        /// Build data provider
        /// </summary>
        /// <param name="pool">Behaviors</param>
        /// <returns></returns>
        protected override Behaviors.SetupDataProvider BuildSetupDataProvider(Behaviors.BehaviorPool pool)
        {
            var provider = base.BuildSetupDataProvider(pool);

            // Add controlling voltage source data
            provider.Add(pool.GetEntityBehaviors(ControllingName));
            provider.Add(ControllingSource.ParameterSets);
            return provider;
        }
    }
}
