using System;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Circuits;
using SpiceSharp.Components.CurrentControlledCurrentSourceBehaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A current-controlled current source.
    /// </summary>
    [Pin(0, "F+"), Pin(1, "F-"), Connected(0, 0)]
    public class CurrentControlledCurrentSource : Component
    {
        static CurrentControlledCurrentSource()
        {
            RegisterBehaviorFactory(typeof(CurrentControlledCurrentSource), new BehaviorFactoryDictionary
            {
                {typeof(BiasingBehavior), e => new BiasingBehavior(e.Name)},
                {typeof(FrequencyBehavior), e => new FrequencyBehavior(e.Name)}
            });
        }

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
        /// Creates a new instance of the <see cref="CurrentControlledCurrentSource"/> class.
        /// </summary>
        /// <param name="name">The name of the current controlled current source</param>
        public CurrentControlledCurrentSource(string name) 
            : base(name, CurrentControlledCurrentSourcePinCount)
        {
            ParameterSets.Add(new BaseParameters());
        }

        /// <summary>
        /// Creates a new instance of the <see cref="CurrentControlledCurrentSource"/> class.
        /// </summary>
        /// <param name="name">The name of the current controlled current source</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="voltageSource">The name of the voltage source</param>
        /// <param name="gain">The current gain</param>
        public CurrentControlledCurrentSource(string name, string pos, string neg, string voltageSource, double gain)
            : this(name)
        {
            ParameterSets.Get<BaseParameters>().Coefficient.Value = gain;
            Connect(pos, neg);
            ControllingName = voltageSource;
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

            // Add behaviors and parameters of the controlling voltage source
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
            var clone = (CurrentControlledCurrentSource) base.Clone(data);
            if (clone.ControllingName != null && data is ComponentInstanceData cid)
                clone.ControllingName = cid.GenerateIdentifier(clone.ControllingName);
            return clone;
        }
    }
}
