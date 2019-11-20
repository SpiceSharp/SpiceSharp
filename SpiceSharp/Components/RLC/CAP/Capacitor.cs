using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.Components.CapacitorBehaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A capacitor
    /// </summary>
    [Pin(0, "C+"), Pin(1, "C-"), Connected]
    public class Capacitor : Component
    {
        /// <summary>
        /// Constants
        /// </summary>
        [ParameterName("pincount"), ParameterInfo("Number of pins")]
		public const int CapacitorPinCount = 2;

        /// <summary>
        /// Initializes a new instance of the <see cref="Capacitor"/> class.
        /// </summary>
        /// <param name="name"></param>
        public Capacitor(string name) : base(name, CapacitorPinCount)
        {
            Parameters.Add(new BaseParameters());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Capacitor"/> class.
        /// </summary>
        /// <param name="name">The name of the capacitor</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="cap">The capacitance</param>
        public Capacitor(string name, string pos, string neg, double cap) 
            : base(name, CapacitorPinCount)
        {
            Parameters.Add(new BaseParameters(cap));
            Connect(pos, neg);
        }

        /// <summary>
        /// Create one or more behaviors for the simulation.
        /// </summary>
        /// <param name="simulation">The simulation for which behaviors need to be created.</param>
        /// <param name="entities">The other entities.</param>
        /// <param name="behaviors">A container where all behaviors are to be stored.</param>
        protected override void CreateBehaviors(ISimulation simulation, IEntityCollection entities, IBehaviorContainer behaviors)
        {
            var context = new ComponentBindingContext(simulation, behaviors, MapNodes(simulation.Variables), Model);
            var eb = simulation.EntityBehaviors;
            if (eb.Tracks<ITimeBehavior>())
                behaviors.Add(new TransientBehavior(Name, context));
            if (eb.Tracks<IFrequencyBehavior>())
                behaviors.Add(new FrequencyBehavior(Name, context));
            if (eb.Tracks<ITemperatureBehavior>() && !behaviors.ContainsKey(typeof(ITemperatureBehavior)))
                behaviors.Add(new TemperatureBehavior(Name, context));
        }
    }
}
