using System;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.CurrentSwitchBehaviors;
using SpiceSharp.Simulations;
using FrequencyBehavior = SpiceSharp.Components.CurrentSwitchBehaviors.FrequencyBehavior;
using LoadBehavior = SpiceSharp.Components.CurrentSwitchBehaviors.LoadBehavior;

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
        [ParameterName("pos_node"), ParameterInfo("Positive node of the switch")]
        public int PosNode { get; internal set; }
        [ParameterName("neg_node"), ParameterInfo("Negative node of the switch")]
        public int NegNode { get; internal set; }
        [ParameterName("control"), ParameterInfo("Name of the controlling source")]
        public Identifier ControllingName { get; set; }

        /// <summary>
        /// Constants
        /// </summary>
        [ParameterName("pincount"), ParameterInfo("Number of pins")]
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
        /// <param name="simulation">Simulation</param>
        public override void Setup(Simulation simulation)
        {
            var nodes = BindNodes(simulation);
            PosNode = nodes[0].Index;
            NegNode = nodes[1].Index;
        }

        /// <summary>
        /// Build data provider
        /// </summary>
        /// <returns></returns>
        protected override SetupDataProvider BuildSetupDataProvider(ParameterPool parameters, BehaviorPool behaviors)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));
            if (behaviors == null)
                throw new ArgumentNullException(nameof(behaviors));

            var provider = base.BuildSetupDataProvider(parameters, behaviors);

            // Add controlling voltage source data
            provider.Add("control", behaviors.GetEntityBehaviors(ControllingName));
            provider.Add("control", parameters.GetEntityParameters(ControllingName));
            return provider;
        }
    }
}
