using System;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.SwitchBehaviors;

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
                {typeof(BiasingBehavior), name => new BiasingBehavior(name, new CurrentControlled())},
                {typeof(AcceptBehavior), name => new AcceptBehavior(name, new CurrentControlled())},
                {typeof(FrequencyBehavior), name => new FrequencyBehavior(name, new CurrentControlled())}
            });
        }

        /// <summary>
        /// Set the model for the current-controlled switch
        /// </summary>
        public void SetModel(CurrentSwitchModel model) => Model = model;
        
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
            // Make sure the current switch is processed after voltage sources
            Priority = -1;

            // Add parameters
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
            // Make sure the current switch is processed after voltage sources
            Priority = -1;

            // Add parameters
            ParameterSets.Add(new BaseParameters());

            // Connect the device
            Connect(pos, neg);
            ControllingName = controllingSource;
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
            provider.Add("control", behaviors[ControllingName]);
            provider.Add("control", parameters[ControllingName]);
            return provider;
        }
    }
}
