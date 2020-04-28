using System;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A mutual inductance between two inductors.
    /// </summary>
    /// <seealso cref="Component"/>
    /// <seealso cref="IParameterized{P}"/>
    /// <seealso cref="MutualInductanceParameters"/>
    public partial class MutualInductance : Entity,
        IParameterized<MutualInductanceParameters>
    {
        /// <inheritdoc/>
        public MutualInductanceParameters Parameters { get; } = new MutualInductanceParameters();

        /// <summary>
        /// Gets or sets the name of the first/primary inductor.
        /// </summary>
        /// <value>
        /// The name of the first/primary inductor.
        /// </value>
        [ParameterName("inductor1"), ParameterName("primary"), ParameterInfo("First coupled inductor")]
        public string InductorName1 { get; set; }

        /// <summary>
        /// Gets or sets the name of the second/secondary inductor.
        /// </summary>
        /// <value>
        /// The name of the second/secondary inductor.
        /// </value>
        [ParameterName("inductor2"), ParameterName("secondary"), ParameterInfo("Second coupled inductor")]
        public string InductorName2 { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MutualInductance"/> class.
        /// </summary>
        /// <param name="name">The name of the mutual inductance specification.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        public MutualInductance(string name) 
            : base(name)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MutualInductance"/> class.
        /// </summary>
        /// <param name="name">The name of the mutual inductance specification.</param>
        /// <param name="inductorName1">The name of the first/primary inductor.</param>
        /// <param name="inductorName2">The name of the second/secondary inductor.</param>
        /// <param name="coupling">The coupling coefficient.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        public MutualInductance(string name, string inductorName1, string inductorName2, double coupling)
            : this(name)
        {
            Parameters.Coupling = coupling;
            InductorName1 = inductorName1;
            InductorName2 = inductorName2;
        }

        /// <inheritdoc/>
        public override void CreateBehaviors(ISimulation simulation)
        {
            var behaviors = new BehaviorContainer(Name);
            CalculateDefaults();
            var context = new BindingContext(this, simulation, LinkParameters);
            behaviors
                .AddIfNo<IFrequencyBehavior>(simulation, () => new FrequencyBehavior(Name, context))
                .AddIfNo<ITimeBehavior>(simulation, () => new TimeBehavior(Name, context))
                .AddIfNo<ITemperatureBehavior>(simulation, () => new TemperatureBehavior(Name, context));
            simulation.EntityBehaviors.Add(behaviors);
        }
    }
}
