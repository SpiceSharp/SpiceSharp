using System;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.CurrentControlledCurrentSourceBehaviors;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A current-controlled current source
    /// </summary>
    [Pin(0, "F+"), Pin(1, "F-"), Connected(0, 0)]
    public class CurrentControlledCurrentSource : Component
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [ParameterName("control"), ParameterInfo("Name of the controlling source")]
        public string ControllingName { get; set; }

        /// <summary>
        /// Constants
        /// </summary>
		[ParameterName("pincount"), ParameterInfo("Number of pins")]
		public const int CurrentControlledCurrentSourcePinCount = 2;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the current controlled current source</param>
        public CurrentControlledCurrentSource(string name) 
            : base(name, CurrentControlledCurrentSourcePinCount)
        {
            // Make sure the current controlled current source happens after voltage sources
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
        /// <param name="name">The name of the current controlled current source</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="voltageSource">The name of the voltage source</param>
        /// <param name="gain">The current gain</param>
        public CurrentControlledCurrentSource(string name, string pos, string neg, string voltageSource, double gain)
            : base(name, CurrentControlledCurrentSourcePinCount)
        {
            // Register behaviors
            Priority = -1;

            // Add parameters
            ParameterSets.Add(new BaseParameters(gain));

            // Add factories
            Behaviors.Add(typeof(LoadBehavior), () => new LoadBehavior(Name));
            Behaviors.Add(typeof(FrequencyBehavior), () => new FrequencyBehavior(Name));

            // Connect
            Connect(pos, neg);
            ControllingName = voltageSource;
        }

        /// <summary>
        /// Build the data provider
        /// </summary>
        /// <returns></returns>
        protected override SetupDataProvider BuildSetupDataProvider(ParameterPool parameters, BehaviorPool behaviors)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));
            if (behaviors == null)
                throw new ArgumentNullException(nameof(behaviors));
            var provider = base.BuildSetupDataProvider(parameters, behaviors);

            // Add behaviors and parameters of the controlling voltage source
            provider.Add("control", behaviors[ControllingName]);
            provider.Add("control", parameters[ControllingName]);
            return provider;
        }
    }
}
