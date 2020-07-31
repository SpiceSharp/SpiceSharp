using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.JFETs;
using SpiceSharp.Diagnostics;
using SpiceSharp.ParameterSets;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A junction field-effect transistor.
    /// </summary>
    /// <seealso cref="Component" />
    /// <seealso cref="IParameterized{P}"/>
    /// <seealso cref="JFETs.Parameters"/>
    [Pin(0, "drain"), Pin(1, "gate"), Pin(2, "source")]
    public class JFET : Component,
        IParameterized<Parameters>
    {
        /// <inheritdoc/>
        public Parameters Parameters { get; } = new Parameters();

        /// <summary>
        /// The pin count for JFETs.
        /// </summary>
        public const int PinCount = 3;

        /// <summary>
        /// Initializes a new instance of the <see cref="JFET"/> class.
        /// </summary>
        /// <param name="name">The string of the entity.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        public JFET(string name)
            : base(name, PinCount)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JFET"/> class.
        /// </summary>
        /// <param name="name">The name of the component.</param>
        /// <param name="drain">The drain node.</param>
        /// <param name="gate">The gate node.</param>
        /// <param name="source">The source node.</param>
        /// <param name="model">The model name.</param>
        public JFET(string name, string drain, string gate, string source, string model)
            : base(name, PinCount)
        {
            Model = model;
            Connect(drain, gate, source);
        }

        /// <inheritdoc/>
        public override void CreateBehaviors(ISimulation simulation)
        {
            var behaviors = new BehaviorContainer(Name);
            var context = new ComponentBindingContext(this, simulation, behaviors, LinkParameters);
            if (context.ModelBehaviors == null)
                throw new NoModelException(Name, typeof(JFETModel));
            behaviors.Build(simulation, context)
                .AddIfNo<IFrequencyBehavior>(context => new FrequencyBehavior(Name, context))
                .AddIfNo<ITimeBehavior>(context => new Time(Name, context))
                .AddIfNo<IBiasingBehavior>(context => new Biasing(Name, context))
                .AddIfNo<ITemperatureBehavior>(context => new Temperature(Name, context));
            simulation.EntityBehaviors.Add(behaviors);
        }
    }
}
