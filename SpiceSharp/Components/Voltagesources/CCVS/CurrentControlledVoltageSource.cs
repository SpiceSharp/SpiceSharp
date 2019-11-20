using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.Components.CurrentControlledVoltageSourceBehaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Components.CommonBehaviors;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A current-controlled voltage source
    /// </summary>
    [Pin(0, "H+"), Pin(1, "H-"), VoltageDriver(0, 1)]
    public class CurrentControlledVoltageSource : Component
    {
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
        /// Initializes a new instance of the <see cref="CurrentControlledVoltageSource"/> class.
        /// </summary>
        /// <param name="name">The name of the current-controlled current source</param>
        public CurrentControlledVoltageSource(string name) 
            : base(name, CurrentControlledVoltageSourcePinCount)
        {
            Parameters.Add(new BaseParameters());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CurrentControlledVoltageSource"/> class.
        /// </summary>
        /// <param name="name">The name of the current-controlled current source</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="controllingSource">The controlling voltage source name</param>
        /// <param name="gain">The transresistance (gain)</param>
        public CurrentControlledVoltageSource(string name, string pos, string neg, string controllingSource, double gain) 
            : base(name, CurrentControlledVoltageSourcePinCount)
        {
            Parameters.Add(new BaseParameters(gain));
            Connect(pos, neg);
            ControllingName = controllingSource;
        }

        /// <summary>
        /// Creates behaviors for the specified simulation that describe this <see cref="Entity" />.
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
        /// Create one or more behaviors for the simulation.
        /// </summary>
        /// <param name="simulation">The simulation for which behaviors need to be created.</param>
        /// <param name="entities">The other entities.</param>
        /// <param name="behaviors">A container where all behaviors are to be stored.</param>
        protected override void CreateBehaviors(ISimulation simulation, IEntityCollection entities, IBehaviorContainer behaviors)
        {
            var context = new ControlledBindingContext(simulation, behaviors, MapNodes(simulation.Variables), Model, ControllingName);
            var eb = simulation.EntityBehaviors;
            if (eb.Tracks<IFrequencyBehavior>())
                behaviors.Add(new FrequencyBehavior(Name, context));
            else if (eb.Tracks<IBiasingBehavior>())
                behaviors.Add(new BiasingBehavior(Name, context));
        }
    }
}
