using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.Components.MosfetBehaviors.Level3;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A MOS3 Mosfet
    /// Level 3, a semi-empirical model(see reference for level 3).
    /// </summary>
    [Pin(0, "Drain"), Pin(1, "Gate"), Pin(2, "Source"), Pin(3, "Bulk"), Connected(0, 2), Connected(0, 3)]
    public class Mosfet3 : Component
    {
        /// <summary>
        /// Constants
        /// </summary>
        [ParameterName("pincount"), ParameterInfo("Number of pins")]
		public const int Mosfet3PinCount = 4;

        /// <summary>
        /// Initializes a new instance of the <see cref="Mosfet3"/> class.
        /// </summary>
        /// <param name="name">The name of the device</param>
        public Mosfet3(string name) : base(name, Mosfet3PinCount)
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
            if (simulation is IBehavioral<INoiseBehavior>)
                behaviors.Add(new NoiseBehavior(Name, context));
            else if (simulation is IBehavioral<IFrequencyBehavior>)
                behaviors.Add(new FrequencyBehavior(Name, context));
            if (simulation is IBehavioral<ITimeBehavior>)
                behaviors.Add(new TransientBehavior(Name, context));

            if (simulation is IBehavioral<IBiasingBehavior>)
            {
                if (!behaviors.ContainsKey(typeof(IBiasingBehavior)))
                    behaviors.Add(new BiasingBehavior(Name, context));
            }
            else if (simulation is IBehavioral<ITemperatureBehavior> && !behaviors.ContainsKey(typeof(ITemperatureBehavior)))
                behaviors.Add(new TemperatureBehavior(Name, context));
        }
    }
}
