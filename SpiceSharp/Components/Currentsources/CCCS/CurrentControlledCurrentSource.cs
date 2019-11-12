using System.Collections.Generic;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.Components.CurrentControlledCurrentSourceBehaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Components.CommonBehaviors;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A current-controlled current source.
    /// </summary>
    [Pin(0, "F+"), Pin(1, "F-"), Connected(0, 0)]
    public class CurrentControlledCurrentSource : Component
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [ParameterName("control"), ParameterInfo("Name of the controlling source")]
        public string ControllingSource { get; set; }

        /// <summary>
        /// Constants
        /// </summary>
		[ParameterName("pincount"), ParameterInfo("Number of pins")]
		public const int CurrentControlledCurrentSourcePinCount = 2;

        /// <summary>
        /// Initializes a new instance of the <see cref="CurrentControlledCurrentSource"/> class.
        /// </summary>
        /// <param name="name">The name of the current controlled current source</param>
        public CurrentControlledCurrentSource(string name) 
            : base(name, CurrentControlledCurrentSourcePinCount)
        {
            Parameters.Add(new BaseParameters());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CurrentControlledCurrentSource"/> class.
        /// </summary>
        /// <param name="name">The name of the current controlled current source</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="voltageSource">The name of the voltage source</param>
        /// <param name="gain">The current gain</param>
        public CurrentControlledCurrentSource(string name, string pos, string neg, string voltageSource, double gain)
            : this(name)
        {
            Parameters.GetValue<BaseParameters>().Coefficient.Value = gain;
            Connect(pos, neg);
            ControllingSource = voltageSource;
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
            if (ControllingSource != null)
                entities[ControllingSource].CreateBehaviors(simulation, entities);
            base.CreateBehaviors(simulation, entities);
        }

        /// <summary>
        /// Create one or more behaviors for the simulation.
        /// </summary>
        /// <param name="simulation">The simulation for which behaviors need to be created.</param>
        /// <param name="entities">The other entities.</param>
        /// <param name="behaviors">A container where all behaviors are to be stored.</param>
        protected override void CreateBehaviors(ISimulation simulation, IEntityCollection entities, BehaviorContainer behaviors)
        {
            var context = new ControlledBindingContext(simulation, behaviors, ApplyConnections(simulation.Variables), Model, ControllingSource);
            switch (simulation)
            {
                case FrequencySimulation _:
                    behaviors.Add(new FrequencyBehavior(Name, context));
                    break;
                case BiasingSimulation _:
                    behaviors.Add(new BiasingBehavior(Name, context));
                    break;
            }
        }
    }
}
