using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.JFETBehaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A junction field-effect transistor.
    /// </summary>
    /// <seealso cref="Component" />
    [Pin(0, "drain"), Pin(1, "gate"), Pin(2, "source")]
    public class JFET : Component
    {
        /// <summary>
        /// The number of pins on a JFET.
        /// </summary>
        public const int JFETPincount = 3;

        /// <summary>
        /// Initializes a new instance of the <see cref="JFET"/> class.
        /// </summary>
        /// <param name="name">The string of the entity.</param>
        public JFET(string name)
            : base(name, JFETPincount)
        {
            Parameters.Add(new BaseParameters());
        }

        /// <summary>
        /// Creates the behaviors for the specified simulation and registers them with the simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public override void CreateBehaviors(ISimulation simulation)
        {
            var behaviors = new BehaviorContainer(Name,
                LinkParameters ? Parameters : (IParameterSetDictionary)Parameters.Clone());
            behaviors.Parameters.CalculateDefaults();
            var context = new ComponentBindingContext(simulation, behaviors, MapNodes(simulation.Variables), Model);
            behaviors
                .AddIfNo<IFrequencyBehavior>(simulation, () => new FrequencyBehavior(Name, context))
                .AddIfNo<ITimeBehavior>(simulation, () => new TransientBehavior(Name, context))
                .AddIfNo<IBiasingBehavior>(simulation, () => new BiasingBehavior(Name, context))
                .AddIfNo<ITemperatureBehavior>(simulation, () => new TemperatureBehavior(Name, context));
            simulation.EntityBehaviors.Add(behaviors);
        }
    }
}
