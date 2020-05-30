using SpiceSharp.ParameterSets;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.Bipolars;
using SpiceSharp.Diagnostics;
using SpiceSharp.Simulations;
using SpiceSharp.Attributes;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A bipolar junction transistor (BJT).
    /// </summary>
    /// <seealso cref="Component"/>
    /// <seealso cref="IParameterized{P}"/>
    /// <seealso cref="Bipolars.Parameters"/>
    [Pin(0, "Collector"), Pin(1, "Base"), Pin(2, "Emitter"), Pin(3, "Substrate")]
    public class BipolarJunctionTransistor : Component,
        IParameterized<Parameters>
    {
        /// <inheritdoc/>
        public Parameters Parameters { get; } = new Parameters();

        /// <summary>
        /// The pin count for a bipolar junction transistor.
        /// </summary>
        [ParameterName("pincount"), ParameterInfo("Number of pins")]
		public const int PinCount = 4;

        /// <summary>
        /// Initializes a new instance of the <see cref="BipolarJunctionTransistor"/> class.
        /// </summary>
        /// <param name="name">The name of the device.</param>
        public BipolarJunctionTransistor(string name) 
            : base(name, PinCount)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BipolarJunctionTransistor"/> class.
        /// </summary>
        /// <param name="name">The name of the device.</param>
        /// <param name="c">The collector node.</param>
        /// <param name="b">The base node.</param>
        /// <param name="e">The emitter node.</param>
        /// <param name="s">The substrate node.</param>
        /// <param name="model">The model.</param>
        public BipolarJunctionTransistor(string name, string c, string b, string e, string s, string model)
            : this(name)
        {
            Connect(c, b, e, s);
            Model = model;
        }

        /// <inheritdoc/>
        public override void CreateBehaviors(ISimulation simulation)
        {
            var behaviors = new BehaviorContainer(Name);
            var context = new ComponentBindingContext(this, simulation, behaviors, LinkParameters);
            if (context.ModelBehaviors == null)
                throw new NoModelException(Name, typeof(BipolarJunctionTransistorModel));
            behaviors
                .AddIfNo<INoiseBehavior>(simulation, () => new Bipolars.Noise(Name, context))
                .AddIfNo<IFrequencyBehavior>(simulation, () => new Frequency(Name, context))
                .AddIfNo<ITimeBehavior>(simulation, () => new Time(Name, context))
                .AddIfNo<IBiasingBehavior>(simulation, () => new Biasing(Name, context))
                .AddIfNo<ITemperatureBehavior>(simulation, () => new Temperature(Name, context));
            simulation.EntityBehaviors.Add(behaviors);
        }
    }
}
