using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.Components.ResistorBehaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A resistor
    /// </summary>
    [Pin(0, "R+"), Pin(1, "R-")]
    public class Resistor : Component
    {
        /// <summary>
        /// Constants
        /// </summary>
        [ParameterName("pincount"), ParameterInfo("Number of pins")]
		public const int ResistorPinCount = 2;

        /// <summary>
        /// Initializes a new instance of the <see cref="Resistor"/> class.
        /// </summary>
        /// <param name="name">The name of the entity.</param>
        public Resistor(string name) 
            : base(name, ResistorPinCount)
        {
            Parameters.Add(new BaseParameters());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Resistor"/> class.
        /// </summary>
        /// <param name="name">The name of the resistor</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="res">The resistance</param>
        public Resistor(string name, string pos, string neg, double res) 
            : base(name, ResistorPinCount)
        {
            Parameters.Add(new BaseParameters(res));
            Connect(pos, neg);
        }

        /// <summary>
        /// Creates the behaviors for the specified simulation and registers them with the simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="behaviors">An <see cref="IBehaviorContainer" /> where the behaviors can be stored.</param>
        public override void CreateBehaviors(ISimulation simulation, IBehaviorContainer behaviors)
        {
            base.CreateBehaviors(simulation, behaviors);

            var context = new ComponentBindingContext(simulation, behaviors, MapNodes(simulation.Variables), Model);
            var eb = simulation.EntityBehaviors;
            if (eb.Tracks<INoiseBehavior>())
                behaviors.Add(new NoiseBehavior(Name, context));
            else if (eb.Tracks<IFrequencyBehavior>())
                behaviors.Add(new FrequencyBehavior(Name, context));
            else if (eb.Tracks<IBiasingBehavior>())
                behaviors.Add(new BiasingBehavior(Name, context));
            else if (eb.Tracks<ITemperatureBehavior>())
                behaviors.Add(new TemperatureBehavior(Name, context));
        }
    }
}
