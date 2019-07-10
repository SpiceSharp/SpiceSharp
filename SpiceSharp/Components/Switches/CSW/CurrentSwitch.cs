using System;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Circuits;
using SpiceSharp.Components.SwitchBehaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A current-controlled switch
    /// </summary>
    [Pin(0, "W+"), Pin(1, "W-")]
    public class CurrentSwitch : Component
    {
        static CurrentSwitch()
        {
            RegisterBehaviorFactory(typeof(CurrentSwitch), new BehaviorFactoryDictionary
            {
                // Add factories
                {typeof(BiasingBehavior), e => new BiasingBehavior(e.Name, new CurrentControlled())},
                {typeof(AcceptBehavior), e => new AcceptBehavior(e.Name, new CurrentControlled())},
                {typeof(FrequencyBehavior), e => new FrequencyBehavior(e.Name, new CurrentControlled())}
            });
        }

        /// <summary>
        /// Controlling source name
        /// </summary>
        [ParameterName("control"), ParameterInfo("Name of the controlling source")]
        public string ControllingName { get; set; }

        /// <summary>
        /// Constants
        /// </summary>
        [ParameterName("pincount"), ParameterInfo("Number of pins")]
		public const int CurrentSwitchPinCount = 2;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the current-controlled switch</param>
        public CurrentSwitch(string name) : base(name, CurrentSwitchPinCount)
        {
            ParameterSets.Add(new BaseParameters());
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the current-controlled switch</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="controllingSource">The controlling voltage source</param>
        public CurrentSwitch(string name, string pos, string neg, string controllingSource)
            : base(name, CurrentSwitchPinCount)
        {
            ParameterSets.Add(new BaseParameters());
            Connect(pos, neg);
            ControllingName = controllingSource;
        }

        /// <summary>
        /// Create the behaviors for the <see cref="CurrentSwitch" />.
        /// </summary>
        /// <param name="types">The behavior types.</param>
        /// <param name="simulation">The simulation.</param>
        /// <param name="entities">The entities.</param>
        public override void CreateBehaviors(Type[] types, Simulation simulation, EntityCollection entities)
        {
            if (ControllingName != null)
                entities[ControllingName].CreateBehaviors(types, simulation, entities);
            base.CreateBehaviors(types, simulation, entities);
        }

        /// <summary>
        /// Build data provider
        /// </summary>
        /// <returns></returns>
        protected override SetupDataProvider BuildSetupDataProvider(ParameterPool parameters, BehaviorPool behaviors)
        {
            parameters.ThrowIfNull(nameof(parameters));
            behaviors.ThrowIfNull(nameof(behaviors));

            var provider = base.BuildSetupDataProvider(parameters, behaviors);

            // Add controlling voltage source data
            provider.Add("control", behaviors[ControllingName]);
            provider.Add("control", parameters[ControllingName]);
            return provider;
        }

        /// <summary>
        /// Clone the current controlled switch
        /// </summary>
        /// <param name="data">Instance data.</param>
        /// <returns></returns>
        public override Entity Clone(InstanceData data)
        {
            var clone = (CurrentControlledCurrentSource)base.Clone(data);
            if (clone.ControllingName != null && data is ComponentInstanceData cid)
                clone.ControllingName = cid.GenerateIdentifier(clone.ControllingName);
            return clone;
        }
    }
}
