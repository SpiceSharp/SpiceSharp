using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.Components.MosfetBehaviors.Level1;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A Mosfet.
    /// Level 1, Shichman-Hodges.
    /// </summary>
    [Pin(0, "Drain"), Pin(1, "Gate"), Pin(2, "Source"), Pin(3, "Bulk"), Connected(0, 2), Connected(0, 3)]
    public class Mosfet1 : Component
    {
        /// <summary>
        /// Constants
        /// </summary>
        [ParameterName("pincount"), ParameterInfo("Number of pins")]
		public const int Mosfet1PinCount = 4;

        /// <summary>
        /// Initializes a new instance of the <see cref="Mosfet1"/> class.
        /// </summary>
        /// <param name="name">The name of the device</param>
        public Mosfet1(string name) : base(name, Mosfet1PinCount)
        {
            Parameters.Add(new BaseParameters());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Mosfet1"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="d">The drain node.</param>
        /// <param name="g">The gate node.</param>
        /// <param name="s">The source node.</param>
        /// <param name="b">The bulk node.</param>
        /// <param name="model">The mosfet model.</param>
        public Mosfet1(string name, string d, string g, string s, string b, string model)
            : this(name)
        {
            Connect(d, g, s, b);
            Model = model;
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
            var eb = simulation.EntityBehaviors;
            if (simulation.UsesBehaviors<INoiseBehavior>())
                behaviors.Add(new NoiseBehavior(Name, context));
            else if (simulation.UsesBehaviors<IFrequencyBehavior>())
                behaviors.Add(new FrequencyBehavior(Name, context));
            if (simulation.UsesBehaviors<ITimeBehavior>())
                behaviors.Add(new TransientBehavior(Name, context));

            if (simulation.UsesBehaviors<IBiasingBehavior>())
            {
                if (!behaviors.ContainsKey(typeof(IBiasingBehavior)))
                    behaviors.Add(new BiasingBehavior(Name, context));
            }
            else if (simulation.UsesBehaviors<ITemperatureBehavior>() && !behaviors.ContainsKey(typeof(ITemperatureBehavior)))
                behaviors.Add(new TemperatureBehavior(Name, context));
            simulation.EntityBehaviors.Add(behaviors);
        }
    }
}
