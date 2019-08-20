using System;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Circuits;
using SpiceSharp.Components.CurrentControlledVoltageSourceBehaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A current-controlled voltage source
    /// </summary>
    [Pin(0, "H+"), Pin(1, "H-"), VoltageDriver(0, 1)]
    public class CurrentControlledVoltageSource : Component
    {
        static CurrentControlledVoltageSource()
        {
            RegisterBehaviorFactory(typeof(CurrentControlledVoltageSource), new BehaviorFactoryDictionary
            {
                {typeof(BiasingBehavior), e => new BiasingBehavior(e.Name)},
                {typeof(FrequencyBehavior), e => new FrequencyBehavior(e.Name)}
            });
        }

        /// <summary>
        /// Controlling source name
        /// </summary>
        [ParameterName("control"), ParameterInfo("Controlling voltage source")]
        public string ControllingName { get; set; }

        /// <summary>
        /// Constants
        /// </summary>
        [ParameterName("pincount"), ParameterInfo("Number of pins")]
		public const int CurrentControlledVoltageSourcePinCount = 2;
        
        /// <summary>
        /// Creates a new instance of the <see cref="CurrentControlledVoltageSource"/> class.
        /// </summary>
        /// <param name="name">The name of the current-controlled current source</param>
        public CurrentControlledVoltageSource(string name) 
            : base(name, CurrentControlledVoltageSourcePinCount)
        {
            ParameterSets.Add(new BaseParameters());
        }

        /// <summary>
        /// Creates a new instance of the <see cref="CurrentControlledVoltageSource"/> class.
        /// </summary>
        /// <param name="name">The name of the current-controlled current source</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="controllingSource">The controlling voltage source name</param>
        /// <param name="gain">The transresistance (gain)</param>
        public CurrentControlledVoltageSource(string name, string pos, string neg, string controllingSource, double gain) 
            : base(name, CurrentControlledVoltageSourcePinCount)
        {
            ParameterSets.Add(new BaseParameters(gain));
            Connect(pos, neg);
            ControllingName = controllingSource;
        }

        /// <summary>
        /// Creates behaviors of the specified type.
        /// </summary>
        /// <param name="types"></param>
        /// <param name="simulation">The simulation requesting the behaviors.</param>
        /// <param name="entities">The entities being processed.</param>
        public override void CreateBehaviors(Type[] types, Simulation simulation, EntityCollection entities)
        {
            if (ControllingName != null)
                entities[ControllingName].CreateBehaviors(types, simulation, entities);
            base.CreateBehaviors(types, simulation, entities);
        }

        /// <summary>
        /// Build the binding context.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <returns></returns>
        protected override ComponentBindingContext BuildBindingContext(Simulation simulation)
        {
            var context = base.BuildBindingContext(simulation);

            // Add the controlling source
            context.Add("control", simulation.EntityParameters[ControllingName]);
            context.Add("control", simulation.EntityBehaviors[ControllingName]);

            return context;
        }

        /// <summary>
        /// Clone the current controlled current source
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
