using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.ResistorBehaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A resistor
    /// </summary>
    [Pin(0, "R+"), Pin(1, "R-")]
    public class Resistor : Component,
        IParameterized<BaseParameters>
    {
        /// <summary>
        /// Constants
        /// </summary>
        [ParameterName("pincount"), ParameterInfo("Number of pins")]
		public const int ResistorPinCount = 2;

        private readonly BaseParameters _bp = new BaseParameters();
        BaseParameters IParameterized<BaseParameters>.Parameters => _bp;

        /// <summary>
        /// Initializes a new instance of the <see cref="Resistor"/> class.
        /// </summary>
        /// <param name="name">The name of the entity.</param>
        public Resistor(string name) 
            : base(name, ResistorPinCount)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Resistor"/> class.
        /// </summary>
        /// <param name="name">The name of the resistor</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="res">The resistance</param>
        public Resistor(string name, string pos, string neg, double res) 
            : this(name)
        {
            _bp.Resistance.Value = res;
            Connect(pos, neg);
        }

        /// <summary>
        /// Creates the behaviors for the specified simulation and registers them with the simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public override void CreateBehaviors(ISimulation simulation)
        {
            var behaviors = new BehaviorContainer(Name);
            CalculateDefaults();
            var context = new ComponentBindingContext(this, simulation);
            behaviors
                .AddIfNo<INoiseBehavior>(simulation, () => new NoiseBehavior(Name, context))
                .AddIfNo<IFrequencyBehavior>(simulation, () => new FrequencyBehavior(Name, context))
                .AddIfNo<IBiasingBehavior>(simulation, () => new BiasingBehavior(Name, context))
                .AddIfNo<ITemperatureBehavior>(simulation, () => new TemperatureBehavior(Name, context));
            simulation.EntityBehaviors.Add(behaviors);
        }
    }
}
