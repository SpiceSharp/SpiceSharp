using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
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
        /// Create one or more behaviors for the simulation.
        /// </summary>
        /// <param name="simulation">The simulation for which behaviors need to be created.</param>
        /// <param name="entities">The other entities.</param>
        /// <param name="behaviors">A container where all behaviors are to be stored.</param>
        protected override void CreateBehaviors(ISimulation simulation, IEntityCollection entities, BehaviorContainer behaviors)
        {
            var context = new ComponentBindingContext(simulation, behaviors, ApplyConnections(simulation.Variables), Model);
            var eb = simulation.EntityBehaviors;
            if (eb.Tracks<IFrequencyBehavior>())
                behaviors.Add(new FrequencyBehavior(Name, context));
            if (eb.Tracks<ITimeBehavior>())
                behaviors.Add(new TransientBehavior(Name, context));
            if (eb.Tracks<IBiasingBehavior>())
            {
                if (!behaviors.ContainsKey(typeof(IBiasingBehavior)))
                    behaviors.Add(new BiasingBehavior(Name, context));
            }
            else if (eb.Tracks<ITemperatureBehavior>() && !behaviors.ContainsKey(typeof(ITemperatureBehavior)))
                behaviors.Add(new TemperatureBehavior(Name, context));
        }
    }
}
