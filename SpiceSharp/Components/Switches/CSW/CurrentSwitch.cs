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
        /// Creates a new instance of the <see cref="CurrentSwitch"/> class.
        /// </summary>
        /// <param name="name">The name of the current-controlled switch</param>
        public CurrentSwitch(string name) : base(name, CurrentSwitchPinCount)
        {
            ParameterSets.Add(new BaseParameters());
        }

        /// <summary>
        /// Creates a new instance of the <see cref="CurrentSwitch"/> class.
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
        /// Creates behaviors for the specified simulation that describe this <see cref="Entity"/>.
        /// </summary>
        /// <param name="simulation">The simulation requesting the behaviors.</param>
        /// <param name="entities">The entities being processed, used by the entity to find linked entities.</param>
        /// <remarks>
        /// The order typically indicates hierarchy. The entity will create the behaviors in reverse order, allowing
        /// the most specific child class to be used that is necessary. For example, the <see cref="OP" /> simulation needs
        /// <see cref="ITemperatureBehavior" /> and an <see cref="IBiasingBehavior" />. The entity will first look for behaviors
        /// of type <see cref="IBiasingBehavior" />, and then for the behaviors of type <see cref="ITemperatureBehavior" />. However,
        /// if the behavior that was created for <see cref="IBiasingBehavior" /> also implements <see cref="ITemperatureBehavior" />,
        /// then then entity will not create a new instance of the behavior.
        /// </remarks>
        public override void CreateBehaviors(ISimulation simulation, IEntityCollection entities)
        {
            if (ControllingName != null)
                entities[ControllingName].CreateBehaviors(simulation, entities);
            base.CreateBehaviors(simulation, entities);
        }

        /// <summary>
        /// Build the binding context.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <returns></returns>
        protected override ComponentBindingContext BuildBindingContext(ISimulation simulation)
        {
            var context = base.BuildBindingContext(simulation);

            // Add controlling voltage source data
            context.Add("control", simulation.EntityParameters[ControllingName]);
            context.Add("control", simulation.EntityBehaviors[ControllingName]);
            return context;
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
