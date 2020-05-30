using SpiceSharp.ParameterSets;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.Diodes;
using SpiceSharp.Diagnostics;
using SpiceSharp.Simulations;
using System;
using SpiceSharp.Attributes;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A diode.
    /// </summary>
    /// <seealso cref="Component"/>
    /// <seealso cref="IParameterized{P}"/>
    /// <seealso cref="Diodes.Parameters"/>
    [Pin(0, "D+"), Pin(1, "D-")]
    public class Diode : Component,
        IParameterized<Parameters>
    {
        /// <inheritdoc/>
        public Parameters Parameters { get; } = new Parameters();

        /// <summary>
        /// The pin count for diodes.
        /// </summary>
        [ParameterName("pincount"), ParameterInfo("Number of pins")]
		public const int PinCount = 2;

        /// <summary>
        /// Initializes a new instance of the <see cref="Diode"/> class.
        /// </summary>
        /// <param name="name">The name of the device.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        public Diode(string name) 
            : base(name, PinCount)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Diode"/> class.
        /// </summary>
        /// <param name="name">The name of the device.</param>
        /// <param name="anode">The anode.</param>
        /// <param name="cathode">The cathode.</param>
        /// <param name="model">The model.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        public Diode(string name, string anode, string cathode, string model)
            : this(name)
        {
            Connect(anode, cathode);
            Model = model;
        }

        /// <inheritdoc/>
        public override void CreateBehaviors(ISimulation simulation)
        {
            var behaviors = new BehaviorContainer(Name);
            var context = new ComponentBindingContext(this, simulation, behaviors, LinkParameters);
            if (context.ModelBehaviors == null || !context.ModelBehaviors.ContainsKey(typeof(ModelTemperature)))
                throw new NoModelException(Name, typeof(DiodeModel));
            behaviors
                .AddIfNo<INoiseBehavior>(simulation, () => new Diodes.Noise(Name, context))
                .AddIfNo<IFrequencyBehavior>(simulation, () => new Frequency(Name, context))
                .AddIfNo<ITimeBehavior>(simulation, () => new Time(Name, context))
                .AddIfNo<IBiasingBehavior>(simulation, () => new Biasing(Name, context))
                .AddIfNo<ITemperatureBehavior>(simulation, () => new Temperature(Name, context));
            simulation.EntityBehaviors.Add(behaviors);
        }
    }
}
