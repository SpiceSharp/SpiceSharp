﻿using System;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;
using SpiceSharp.Components.MutualInductances;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A mutual inductance between two inductors.
    /// </summary>
    /// <seealso cref="Component"/>
    /// <seealso cref="IParameterized{P}"/>
    /// <seealso cref="MutualInductances.Parameters"/>
    public class MutualInductance : Entity,
        IParameterized<Parameters>
    {
        /// <inheritdoc/>
        public Parameters Parameters { get; } = new Parameters();

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
            var context = new MutualInductances.BindingContext(this, simulation, LinkParameters);
            behaviors
                .AddIfNo<IFrequencyBehavior>(simulation, () => new Frequency(Name, context))
                .AddIfNo<ITimeBehavior>(simulation, () => new Time(Name, context))
                .AddIfNo<ITemperatureBehavior>(simulation, () => new Temperature(Name, context));
            simulation.EntityBehaviors.Add(behaviors);
        }
    }
}
